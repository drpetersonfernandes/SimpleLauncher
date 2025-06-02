using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace SimpleLauncher.Services;

/// <summary>
/// Provides methods for validating file paths and parameters used in emulator configurations.
/// </summary>
public static partial class ParameterValidator
{
    // Known game-specific parameter placeholders that shouldn't be validated as actual paths
    private static readonly string[] GameSpecificPlaceholders =
    [
        "%ROM%", "%GAME%", "%ROMNAME%", "%ROMFILE%", "$rom$", "$game$", "$romname$", "$romfile$",
        "{rom}", "{game}", "{romname}", "{romfile}"
    ];

    // Known parameter flags that shouldn't be validated as actual paths
    private static readonly string[] KnownParameterFlags =
    [
        "-f", "--fullscreen", "/f", "-window", "-fullscreen", "--window", "-cart",
        "-L", "-g", "-rompath"
    ];

    private static readonly char[] Separator = ['\\', '/'];
    private static readonly char[] Separator2 = [';']; // Used for -rompath splitting
    private static readonly char[] Separator3 = [';']; // Used for quoted path splitting
    private static readonly char[] Separator4 = [' ', '\t']; // Used for splitting remaining words
    private static readonly char[] Separator5 = [';']; // Used for ValidateEmulatorParameters splitting (and GetRelativePathsInParameters)

    /// <summary>
    /// Checks if a string looks like a file path
    /// </summary>
    private static bool LooksLikePath(string text)
    {
        // Skip empty strings
        if (string.IsNullOrWhiteSpace(text)) return false;

        // Check if it contains any of these characters that suggest it's a path
        return text.Contains('\\') || text.Contains('/') ||
               (text.Length >= 2 && text[1] == ':') || // drive letter
               text.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
               text.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
               text.Contains(".dll") || // Catch DLL files even if they have additional text after
               text.StartsWith("%BASEFOLDER%", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("%SYSTEMFOLDER%", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("%EMULATORFOLDER%", StringComparison.OrdinalIgnoreCase) ||
               IsDirectoryPath(text);
    }

    /// <summary>
    /// Checks if the text appears to be a directory path (more thorough than just checking for slashes)
    /// </summary>
    private static bool IsDirectoryPath(string text)
    {
        // Check for directory-like structures with multiple segments
        var hasDrivePrefix = text.Length >= 2 && text[1] == ':';
        var hasMultipleSegments = text.Split(Separator, StringSplitOptions.RemoveEmptyEntries).Length > 1;

        return (hasDrivePrefix && hasMultipleSegments) ||
               (text.Contains('\\') && hasMultipleSegments) ||
               (text.Contains('/') && hasMultipleSegments);
    }

    /// <summary>
    /// Checks if a string is a known parameter flag
    /// </summary>
    private static bool IsKnownFlag(string text)
    {
        return KnownParameterFlags.Any(flag =>
            string.Equals(text, flag, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if a path contains a game-specific placeholder (like %ROM%).
    /// </summary>
    private static bool ContainsGameSpecificPlaceholder(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        return GameSpecificPlaceholders.Any(placeholder =>
            text.Contains(placeholder, StringComparison.OrdinalIgnoreCase));
    }

    private static bool IsQuoted(string token)
    {
        return (token.StartsWith('"') && token.EndsWith('"')) ||
               (token.StartsWith('\'') && token.EndsWith('\''));
    }

    /// <summary>
    /// Validates a single path string by resolving it using known structural placeholders
    /// (%BASEFOLDER%, %SYSTEMFOLDER%, %EMULATORFOLDER%) and checking if the resulting file or directory exists.
    /// </summary>
    private static bool ValidateSinglePath(string pathToValidate, string configuredSystemFolder, string configuredEmulatorLocation)
    {
        if (string.IsNullOrWhiteSpace(pathToValidate)) return false;
        if (ContainsGameSpecificPlaceholder(pathToValidate)) return true; // If it contains %ROM% etc., assume valid for validation purposes.

        var pathAfterEnvExpansion = Environment.ExpandEnvironmentVariables(pathToValidate);

        try
        {
            string finalPathToTest;
            if (pathAfterEnvExpansion.StartsWith("%BASEFOLDER%", StringComparison.OrdinalIgnoreCase))
            {
                finalPathToTest = PathHelper.ResolveRelativeToAppDirectory(pathAfterEnvExpansion);
            }
            else if (pathAfterEnvExpansion.StartsWith("%SYSTEMFOLDER%", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(configuredSystemFolder)) return false;

                var resolvedSystemDir = PathHelper.ResolveRelativeToAppDirectory(configuredSystemFolder);
                if (string.IsNullOrEmpty(resolvedSystemDir) || !Directory.Exists(resolvedSystemDir)) return false;

                var relativePart = pathAfterEnvExpansion.Substring("%SYSTEMFOLDER%".Length).TrimStart('\\', '/');
                finalPathToTest = Path.GetFullPath(Path.Combine(resolvedSystemDir, relativePart));
            }
            else if (pathAfterEnvExpansion.StartsWith("%EMULATORFOLDER%", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(configuredEmulatorLocation)) return false;

                var resolvedEmulatorPath = PathHelper.ResolveRelativeToAppDirectory(configuredEmulatorLocation);
                string emulatorDir;

                if (File.Exists(resolvedEmulatorPath))
                {
                    emulatorDir = Path.GetDirectoryName(resolvedEmulatorPath);
                }
                else if (Directory.Exists(resolvedEmulatorPath))
                {
                    emulatorDir = resolvedEmulatorPath;
                }
                else
                {
                    return false;
                }

                if (string.IsNullOrEmpty(emulatorDir)) return false;

                var relativePart = pathAfterEnvExpansion.Substring("%EMULATORFOLDER%".Length).TrimStart('\\', '/');
                finalPathToTest = Path.GetFullPath(Path.Combine(emulatorDir, relativePart));
            }
            else if (Path.IsPathRooted(pathAfterEnvExpansion))
            {
                finalPathToTest = Path.GetFullPath(pathAfterEnvExpansion);
            }
            else
            {
                finalPathToTest = PathHelper.ResolveRelativeToAppDirectory(pathAfterEnvExpansion);
                if (!string.IsNullOrEmpty(finalPathToTest) && (File.Exists(finalPathToTest) || Directory.Exists(finalPathToTest)))
                {
                    return true;
                }

                if (string.IsNullOrEmpty(configuredSystemFolder)) return false;

                var resolvedSystemDir = PathHelper.ResolveRelativeToAppDirectory(configuredSystemFolder);
                if (string.IsNullOrEmpty(resolvedSystemDir) || !Directory.Exists(resolvedSystemDir)) return false;

                finalPathToTest = Path.GetFullPath(Path.Combine(resolvedSystemDir, pathAfterEnvExpansion));
                if (File.Exists(finalPathToTest) || Directory.Exists(finalPathToTest))
                {
                    return true;
                }

                return false;
            }

            return !string.IsNullOrEmpty(finalPathToTest) && (File.Exists(finalPathToTest) || Directory.Exists(finalPathToTest));
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Extracts parameter paths from a command line with thorough validation
    /// </summary>
    private static List<(string Flag, string Path)> ExtractParameterPaths(string parameters)
    {
        var result = new List<(string Flag, string Path)>();
        if (string.IsNullOrWhiteSpace(parameters)) return result;

        var flaggedPathRegex = MyRegex();
        var matches = flaggedPathRegex.Matches(parameters);

        foreach (Match match in matches)
        {
            var flag = match.Groups[1].Value;
            var path = match.Groups[2].Success ? match.Groups[2].Value :
                match.Groups[3].Success ? match.Groups[3].Value :
                match.Groups[4].Value;

            if (!string.IsNullOrWhiteSpace(path))
            {
                result.Add((flag, path));
            }
        }

        return result;
    }

    /// <summary>
    /// Validates paths in parameter strings and returns invalid paths
    /// </summary>
    public static (bool overallValid, List<string> allInvalidPaths) ValidateParameterPaths(
        string parameters,
        string configuredSystemFolder = null,
        string configuredEmulatorLocation = null,
        bool isMameSystem = false)
    {
        var invalidPaths = new List<string>();
        if (string.IsNullOrWhiteSpace(parameters)) return (true, invalidPaths);

        var allPathsValid = true;

        var parameterPaths = ExtractParameterPaths(parameters);

        foreach (var (flag, path) in parameterPaths)
        {
            switch (flag)
            {
                case "-rompath":
                {
                    var romPaths = path.Split(Separator2, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var romPath in romPaths)
                    {
                        var trimmedPath = romPath.Trim();
                        if (string.IsNullOrWhiteSpace(trimmedPath) || ContainsGameSpecificPlaceholder(trimmedPath)) continue;
                        if (ValidateSinglePath(trimmedPath, configuredSystemFolder, configuredEmulatorLocation)) continue;

                        invalidPaths.Add(trimmedPath);
                        allPathsValid = false;
                    }

                    break;
                }
                case "-L":
                {
                    if (!string.IsNullOrWhiteSpace(path) && !ContainsGameSpecificPlaceholder(path) &&
                        !ValidateSinglePath(path, configuredSystemFolder, configuredEmulatorLocation))
                    {
                        invalidPaths.Add(path);
                        allPathsValid = false;
                    }

                    break;
                }
                default:
                {
                    if (!string.IsNullOrWhiteSpace(path) &&
                        !ContainsGameSpecificPlaceholder(path) &&
                        LooksLikePath(path) &&
                        !ValidateSinglePath(path, configuredSystemFolder, configuredEmulatorLocation))
                    {
                        invalidPaths.Add(path);
                        allPathsValid = false;
                    }

                    break;
                }
            }
        }

        var quotedPathsRegex = MyRegex1();
        var quotedMatches = quotedPathsRegex.Matches(parameters);
        foreach (Match match in quotedMatches)
        {
            var quotedPath = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;

            if (!LooksLikePath(quotedPath) ||
                ContainsGameSpecificPlaceholder(quotedPath) ||
                parameterPaths.Any(p => p.Path == quotedPath)) continue;

            if (quotedPath.Contains(';'))
            {
                var subPaths = quotedPath.Split(Separator3, StringSplitOptions.RemoveEmptyEntries);
                foreach (var subPath in subPaths)
                {
                    var trimmedSubPath = subPath.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedSubPath) || ContainsGameSpecificPlaceholder(trimmedSubPath)) continue;
                    if (ValidateSinglePath(trimmedSubPath, configuredSystemFolder, configuredEmulatorLocation)) continue;

                    invalidPaths.Add(trimmedSubPath);
                    allPathsValid = false;
                }
            }
            else
            {
                if (ValidateSinglePath(quotedPath, configuredSystemFolder, configuredEmulatorLocation)) continue;

                invalidPaths.Add(quotedPath);
                allPathsValid = false;
            }
        }

        var remainingParams = MyRegex2().Replace(parameters, " ");
        var flagsRemoved = MyRegex3().Replace(remainingParams, " ");

        var words = flagsRemoved.Split(Separator4, StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words)
        {
            if (IsKnownFlag(word) || ContainsGameSpecificPlaceholder(word)) continue;
            if (!LooksLikePath(word) || ValidateSinglePath(word, configuredSystemFolder, configuredEmulatorLocation)) continue;

            invalidPaths.Add(word);
            allPathsValid = false;
        }

        if (!isMameSystem || invalidPaths.Count <= 0)
            return (allPathsValid, invalidPaths.Distinct().ToList());

        {
            var criticalPaths = invalidPaths
                .Where(path => parameterPaths.Any(p =>
                    p.Flag is "-rompath" or "-L" &&
                    (p.Path == path || (p.Path != null && p.Path.Contains(path)))))
                .ToList();

            if (criticalPaths.Count == 0)
            {
                return (true, invalidPaths.Distinct().ToList());
            }
        }
        return (allPathsValid, invalidPaths.Distinct().ToList());
    }

    private static bool IsRelativePathWithoutKnownStructuralPrefix(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        if (Path.IsPathRooted(path)) return false;
        if (path.StartsWith("%BASEFOLDER%", StringComparison.OrdinalIgnoreCase)) return false;
        if (path.StartsWith("%SYSTEMFOLDER%", StringComparison.OrdinalIgnoreCase)) return false;
        if (path.StartsWith("%EMULATORFOLDER%", StringComparison.OrdinalIgnoreCase)) return false;

        return true;
    }

    /// <summary>
    /// Identifies potential relative paths within a parameter string that do NOT start with a known structural prefix.
    /// Known structural prefixes are %BASEFOLDER%, %SYSTEMFOLDER%, %EMULATORFOLDER%.
    /// </summary>
    /// <param name="parameters">The parameter string to analyze.</param>
    /// <returns>A list of identified relative paths without a known structural prefix.</returns>
    public static List<string> GetRelativePathsInParameters(string parameters)
    {
        var relativePathsWithoutPrefix = new HashSet<string>();
        if (string.IsNullOrWhiteSpace(parameters)) return relativePathsWithoutPrefix.ToList();

        var potentialPaths = new List<string>();

        var flaggedPaths = ExtractParameterPaths(parameters);
        potentialPaths.AddRange(flaggedPaths.Select(static p => p.Path));

        var quotedPathsRegex = MyRegex1();
        var quotedMatches = quotedPathsRegex.Matches(parameters);
        potentialPaths.AddRange(quotedMatches.Select(static match => match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value));

        var remainingParams = MyRegex2().Replace(parameters, " ");
        var flagsRemoved = MyRegex3().Replace(remainingParams, " ");
        var words = flagsRemoved.Split(Separator4, StringSplitOptions.RemoveEmptyEntries);
        potentialPaths.AddRange(words);

        foreach (var potentialPath in potentialPaths)
        {
            var trimmedPath = potentialPath.Trim();

            if (string.IsNullOrWhiteSpace(trimmedPath) || ContainsGameSpecificPlaceholder(trimmedPath) || IsKnownFlag(trimmedPath))
            {
                continue;
            }

            if (LooksLikePath(trimmedPath) && IsRelativePathWithoutKnownStructuralPrefix(trimmedPath))
            {
                relativePathsWithoutPrefix.Add(trimmedPath);
            }
            else if (trimmedPath.Contains(';'))
            {
                var subPaths = trimmedPath.Split(Separator5, StringSplitOptions.RemoveEmptyEntries);
                foreach (var subPath in subPaths)
                {
                    var trimmedSubPath = subPath.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedSubPath) &&
                        !ContainsGameSpecificPlaceholder(trimmedSubPath) &&
                        LooksLikePath(trimmedSubPath) &&
                        IsRelativePathWithoutKnownStructuralPrefix(trimmedSubPath))
                    {
                        relativePathsWithoutPrefix.Add(trimmedSubPath);
                    }
                }
            }
        }

        return relativePathsWithoutPrefix.Distinct().ToList();
    }

    /// <summary>
    /// Resolves all path-like tokens within a parameter string to their absolute paths,
    /// handling %BASEFOLDER%, %SYSTEMFOLDER%, %EMULATORFOLDER% and relative paths.
    /// </summary>
    /// <param name="parameters">The parameter string.</param>
    /// <param name="resolvedSystemFolderPath">The fully resolved path to the system's configured folder. Used to replace %SYSTEMFOLDER%.</param>
    /// <param name="resolvedEmulatorFolderPath">The fully resolved path to the emulator's directory. Used to replace %EMULATORFOLDER%.</param>
    /// <returns>The parameter string with path tokens resolved to absolute paths.</returns>
    public static string ResolveParameterString(
        string parameters,
        string resolvedSystemFolderPath = null,
        string resolvedEmulatorFolderPath = null)
    {
        if (string.IsNullOrWhiteSpace(parameters))
        {
            return string.Empty;
        }

        var pathTokenRegex = new Regex("""
                                       "[^"]*"|'[^']*'|\S+
                                       """);

        var resolvedParameters = pathTokenRegex.Replace(parameters, match =>
        {
            var originalToken = match.Value;
            var tokenForLogic = originalToken.Trim('"', '\'');

            // Return flags and game placeholders as-is
            if (ContainsGameSpecificPlaceholder(tokenForLogic) || IsKnownFlag(tokenForLogic))
            {
                return originalToken;
            }

            // If it doesn't look like a path AND doesn't contain our placeholders, it's likely not a path argument.
            if (!LooksLikePath(tokenForLogic) &&
                !tokenForLogic.Contains("%BASEFOLDER%", StringComparison.OrdinalIgnoreCase) &&
                !tokenForLogic.Contains("%SYSTEMFOLDER%", StringComparison.OrdinalIgnoreCase) &&
                !tokenForLogic.Contains("%EMULATORFOLDER%", StringComparison.OrdinalIgnoreCase))
            {
                return originalToken;
            }

            var processedToken = tokenForLogic;

            // 1. Replace our specific structural placeholders
            // Order matters somewhat if placeholders could be nested, but they typically aren't.
            // %BASEFOLDER% is most fundamental.
            if (processedToken.Contains("%BASEFOLDER%", StringComparison.OrdinalIgnoreCase))
            {
                processedToken = processedToken.Replace("%BASEFOLDER%", PathHelper.SanitizePathToken(AppDomain.CurrentDomain.BaseDirectory), StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrEmpty(resolvedSystemFolderPath) && processedToken.Contains("%SYSTEMFOLDER%", StringComparison.OrdinalIgnoreCase))
            {
                processedToken = processedToken.Replace("%SYSTEMFOLDER%", PathHelper.SanitizePathToken(resolvedSystemFolderPath), StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrEmpty(resolvedEmulatorFolderPath) && processedToken.Contains("%EMULATORFOLDER%", StringComparison.OrdinalIgnoreCase))
            {
                processedToken = processedToken.Replace("%EMULATORFOLDER%", PathHelper.SanitizePathToken(resolvedEmulatorFolderPath), StringComparison.OrdinalIgnoreCase);
            }

            // 2. Expand standard environment variables (e.g., %TEMP%, %USERPROFILE%)
            processedToken = Environment.ExpandEnvironmentVariables(processedToken);

            var finalTokenValue = processedToken;

            // 3. Canonicalize if it appears to be a *single* path that might need it.
            //    Avoid Path.GetFullPath on multi-path strings (e.g., -rompath "path1;path2").
            //    A simple heuristic is to check for common path list separators.
            if (!processedToken.Contains(';')) // Add other separators like '|' if used by some emulators for lists
            {
                try
                {
                    if (Path.IsPathRooted(processedToken))
                    {
                        finalTokenValue = Path.GetFullPath(processedToken); // Canonicalize existing absolute path
                    }
                    else
                    {
                        // If still relative, it was likely a simple relative path like "myconfig.ini"
                        // or became relative after stripping a placeholder that wasn't %BASEFOLDER%.
                        // Resolve it against the application directory.
                        var tempResolved = PathHelper.ResolveRelativeToAppDirectory(processedToken);
                        if (!string.IsNullOrEmpty(tempResolved)) // ResolveRelativeToAppDirectory returns empty on error
                        {
                            finalTokenValue = tempResolved; // This is already an absolute, canonical path
                        }
                        // If tempResolved is empty, finalTokenValue remains as processedToken
                    }
                }
                catch (Exception ex)
                {
                    _ = LogErrors.LogErrorAsync(ex, $"Error during path canonicalization for token '{processedToken}'. Using as-is after placeholder/env var replacement: '{finalTokenValue}'.");
                    // finalTokenValue already holds processedToken, so it's the fallback.
                }
            }
            // else: It's likely a multi-path string; use it as-is after placeholder/env var replacement.

            // Re-apply quotes if the original token had them, or add if needed (contains spaces)
            if (originalToken.StartsWith('"') && originalToken.EndsWith('"'))
            {
                return $"\"{finalTokenValue}\"";
            }

            if (originalToken.StartsWith('\'') && originalToken.EndsWith('\''))
            {
                return $"'{finalTokenValue}'";
            }

            // If not originally quoted, but now contains spaces, quote it.
            if (finalTokenValue.Contains(' ') && !IsQuoted(originalToken))
            {
                return $"\"{finalTokenValue}\"";
            }

            return finalTokenValue;
        });

        return resolvedParameters;
    }

    [GeneratedRegex("""(-\w+)\s+(?:"([^"]+)"|'([^']+)'|(\S+))""")]
    private static partial Regex MyRegex();

    [GeneratedRegex("""(?:"([^"]+)"|'([^']+)')""")]
    private static partial Regex MyRegex1();

    [GeneratedRegex("""(?:"[^"]*"|'[^']*')""")]
    private static partial Regex MyRegex2();

    [GeneratedRegex(@"-\w+\s+")]
    private static partial Regex MyRegex3();
}