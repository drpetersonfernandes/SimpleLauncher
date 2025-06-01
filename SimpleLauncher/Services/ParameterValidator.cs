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
    // Known parameter placeholders that shouldn't be validated as actual paths
    private static readonly string[] KnownPlaceholders =
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
    private static readonly char[] Separator5 = [';']; // Used for ValidateEmulatorParameters splitting

    /// <summary>
    /// Checks if a string looks like a file path
    /// </summary>
    private static bool LooksLikePath(string text)
    {
        // Skip empty strings
        if (string.IsNullOrWhiteSpace(text)) return false;

        // Check if it contains any of these characters that suggest it's a path
        // Also check for %BASEFOLDER% prefix
        return text.Contains('\\') || text.Contains('/') ||
               (text.Length >= 2 && text[1] == ':') || // drive letter
               text.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
               text.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
               text.Contains(".dll") || // Catch DLL files even if they have additional text after
               text.StartsWith("%BASEFOLDER%", StringComparison.OrdinalIgnoreCase) || // Check for the placeholder
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
    /// Checks if a path contains a known placeholder (like %ROM%)
    /// </summary>
    private static bool ContainsPlaceholder(string path)
    {
        // Exclude %BASEFOLDER% from this check, as it's a valid prefix we handle
        return KnownPlaceholders.Any(placeholder =>
                   path.Contains(placeholder, StringComparison.OrdinalIgnoreCase)) &&
               !path.StartsWith("%BASEFOLDER%", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Validates a single path string by resolving it relative to the app directory
    /// (handling %BASEFOLDER%) and checking if the resulting file or directory exists.
    /// </summary>
    private static bool ValidateSinglePath(string path, string systemFolder = null)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;
        if (ContainsPlaceholder(path)) return true;

        // Expand environment variables *before* resolving relative paths
        if (path.Contains('%'))
        {
            path = Environment.ExpandEnvironmentVariables(path);
        }

        try
        {
            // Primary resolution: relative to app directory (handles %BASEFOLDER%)
            var resolvedPath = PathHelper.ResolveRelativeToAppDirectory(path);

            // If primary resolution was successful and the path exists, it's valid
            if (!string.IsNullOrEmpty(resolvedPath) && (File.Exists(resolvedPath) || Directory.Exists(resolvedPath)))
            {
                return true;
            }

            // Secondary resolution: try resolving relative to the system folder
            // Only attempt this if the original path was not absolute and didn't start with %BASEFOLDER%
            if (Path.IsPathRooted(path) || path.StartsWith("%BASEFOLDER%", StringComparison.OrdinalIgnoreCase) ||
                string.IsNullOrEmpty(systemFolder)) return false;

            var resolvedSystemRelativePath = PathHelper.CombineAndResolveRelativeToAppDirectory(systemFolder, path);
            if (!string.IsNullOrEmpty(resolvedSystemRelativePath) && (File.Exists(resolvedSystemRelativePath) || Directory.Exists(resolvedSystemRelativePath)))
            {
                return true;
            }

            // If neither resolution method found an existing path, it's invalid
            return false;
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

        // Match parameter flags followed by paths
        var flaggedPathRegex = MyRegex(); // Regex: (-\w+)\s+(?:"([^"]+)"|'([^']+)'|(\S+))
        var matches = flaggedPathRegex.Matches(parameters);

        foreach (Match match in matches)
        {
            var flag = match.Groups[1].Value;

            // Get the path value from whichever group matched
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
    public static (bool overallValid, List<string> allInvalidPaths) ValidateParameterPaths(string parameters, string systemFolder = null, bool isMameSystem = false)
    {
        var invalidPaths = new List<string>(); // Local list to collect all invalid paths
        if (string.IsNullOrWhiteSpace(parameters)) return (true, invalidPaths); // No parameters, so valid

        var allPathsValid = true; // Initial assumption

        // Get all parameter paths with their flags
        var parameterPaths = ExtractParameterPaths(parameters);

        // Validate each parameter path based on its flag
        foreach (var (flag, path) in parameterPaths)
        {
            switch (flag)
            {
                // Handle specific flag types differently
                case "-rompath":
                {
                    // For rompath, split by semicolons and validate each directory
                    var romPaths = path.Split(Separator2, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var romPath in romPaths)
                    {
                        var trimmedPath = romPath.Trim();
                        if (string.IsNullOrWhiteSpace(trimmedPath) || ContainsPlaceholder(trimmedPath)) continue;

                        // Validate using the updated ValidateSinglePath
                        if (ValidateSinglePath(trimmedPath, systemFolder)) continue;

                        invalidPaths.Add(trimmedPath);
                        allPathsValid = false;
                    }

                    break;
                }
                case "-L":
                {
                    // For library paths (-L), check for file existence using updated ValidateSinglePath
                    if (!string.IsNullOrWhiteSpace(path) && !ContainsPlaceholder(path) && !ValidateSinglePath(path, systemFolder))
                    {
                        invalidPaths.Add(path);
                        allPathsValid = false;
                    }

                    break;
                }
                default:
                {
                    // For other parameters, check using standard path validation with updated ValidateSinglePath
                    if (!string.IsNullOrWhiteSpace(path) &&
                        !ContainsPlaceholder(path) &&
                        LooksLikePath(path) && // Ensure it looks like a path before validating existence
                        !ValidateSinglePath(path, systemFolder))
                    {
                        invalidPaths.Add(path);
                        allPathsValid = false;
                    }

                    break;
                }
            }
        }

        // Process all quoted paths that might not be associated with flags
        var quotedPathsRegex = MyRegex1(); // Regex: (?:"([^"]+)"|'([^']+)')
        var quotedMatches = quotedPathsRegex.Matches(parameters);
        foreach (Match match in quotedMatches)
        {
            // Get the value from whichever group matched
            var quotedPath = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;

            // Skip if it's not a path-like string, contains a placeholder,
            // or was already validated as a parameter path (to avoid duplicates)
            if (!LooksLikePath(quotedPath) ||
                ContainsPlaceholder(quotedPath) ||
                parameterPaths.Any(p => p.Path == quotedPath)) continue;

            // Handle multi-paths separated by semicolons (like in -rompath)
            if (quotedPath.Contains(';'))
            {
                // Split by semicolons and validate each part using updated ValidateSinglePath
                var subPaths = quotedPath.Split(Separator3, StringSplitOptions.RemoveEmptyEntries);
                foreach (var subPath in subPaths)
                {
                    var trimmedSubPath = subPath.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedSubPath) || ContainsPlaceholder(trimmedSubPath)) continue;

                    if (ValidateSinglePath(trimmedSubPath, systemFolder)) continue;

                    invalidPaths.Add(trimmedSubPath);
                    allPathsValid = false;
                }
            }
            else
            {
                // Single path, validate normally using updated ValidateSinglePath
                if (ValidateSinglePath(quotedPath, systemFolder)) continue;

                invalidPaths.Add(quotedPath);
                allPathsValid = false;
            }
        }

        // Process remaining unquoted potential paths (less common)
        // Remove quoted strings first
        var remainingParams = MyRegex2().Replace(parameters, " "); // Regex: (?:"[^"]*"|'[^']*')
        // Remove flagged parameters (flag + value)
        var flagsRemoved = MyRegex3().Replace(remainingParams, " "); // Regex: -\w+\s+

        // Split by whitespace and check each token
        var words = flagsRemoved.Split(Separator4, StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words)
        {
            // Skip known parameter flags or placeholders
            if (IsKnownFlag(word) || ContainsPlaceholder(word)) continue;

            // If it looks like a path, validate it using updated ValidateSinglePath
            if (!LooksLikePath(word) || ValidateSinglePath(word, systemFolder)) continue;

            // Add the invalid path
            invalidPaths.Add(word);
            allPathsValid = false;
        }

        // For MAME systems, apply leniency
        if (!isMameSystem || invalidPaths.Count <= 0)
            return (allPathsValid, invalidPaths.Distinct().ToList()); // Return the final state and the distinct list

        {
            // Identify critical paths for MAME leniency: -rompath or -L
            var criticalPaths = invalidPaths
                .Where(path => parameterPaths.Any(p =>
                    p.Flag is "-rompath" or "-L" &&
                    (p.Path == path || (p.Path != null && p.Path.Contains(path))))) // Check if the invalid path is part of a flagged path
                .ToList();

            if (criticalPaths.Count == 0)
            {
                // Only non-critical paths are invalid, so overall valid due to leniency
                return (true, invalidPaths.Distinct().ToList()); // Return true but still provide the full list
            }
            // If critical paths exist, overallValid remains false
        }

        return (allPathsValid, invalidPaths.Distinct().ToList()); // Return the final state and the distinct list
    }

    /// <summary>
    /// Identifies potential relative paths within a parameter string that do NOT start with %BASEFOLDER%.
    /// </summary>
    /// <param name="parameters">The parameter string to analyze.</param>
    /// <returns>A list of identified relative paths without the %BASEFOLDER% prefix.</returns>
    public static List<string> GetRelativePathsInParameters(string parameters)
    {
        var relativePathsWithoutPrefix = new HashSet<string>();
        if (string.IsNullOrWhiteSpace(parameters)) return relativePathsWithoutPrefix.ToList();

        var potentialPaths = new List<string>();

        var flaggedPaths = ExtractParameterPaths(parameters);
        potentialPaths.AddRange(flaggedPaths.Select(p => p.Path));

        var quotedPathsRegex = MyRegex1();
        var quotedMatches = quotedPathsRegex.Matches(parameters);
        potentialPaths.AddRange(quotedMatches.Select(match => match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value));

        var remainingParams = MyRegex2().Replace(parameters, " ");
        var flagsRemoved = MyRegex3().Replace(remainingParams, " ");
        var words = flagsRemoved.Split(Separator4, StringSplitOptions.RemoveEmptyEntries);
        potentialPaths.AddRange(words);

        foreach (var potentialPath in potentialPaths)
        {
            var trimmedPath = potentialPath.Trim();

            if (string.IsNullOrWhiteSpace(trimmedPath) || ContainsPlaceholder(trimmedPath) || IsKnownFlag(trimmedPath))
            {
                continue;
            }

            // Use PathHelper.IsRelativePathWithoutBaseFolder to check if it's relative AND lacks the prefix
            if (LooksLikePath(trimmedPath) && PathHelper.IsRelativePathWithoutBaseFolder(trimmedPath))
            {
                relativePathsWithoutPrefix.Add(trimmedPath);
            }
            else if (trimmedPath.Contains(';'))
            {
                var subPaths = trimmedPath.Split(Separator5, StringSplitOptions.RemoveEmptyEntries);
                foreach (var subPath in subPaths)
                {
                    var trimmedSubPath = subPath.Trim();
                    if (!string.IsNullOrWhiteSpace(trimmedSubPath) && !ContainsPlaceholder(trimmedSubPath) && LooksLikePath(trimmedSubPath) && PathHelper.IsRelativePathWithoutBaseFolder(trimmedSubPath))
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
    /// handling %BASEFOLDER% and relative paths.
    /// </summary>
    /// <param name="parameters">The parameter string.</param>
    /// <param name="resolvedSystemFolder"></param>
    /// <returns>The parameter string with path tokens resolved to absolute paths.</returns>
    public static string ResolveParameterString(string parameters, string resolvedSystemFolder = null)
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
            var token = match.Value;
            var trimmedToken = token.Trim('"', '\'');

            if (ContainsPlaceholder(trimmedToken) || IsKnownFlag(trimmedToken) || !LooksLikePath(trimmedToken))
            {
                return token;
            }

            try
            {
                // Attempt primary resolution: relative to app directory (handles %BASEFOLDER%)
                var resolvedPath = PathHelper.ResolveRelativeToAppDirectory(trimmedToken);

                // If primary resolution failed, try secondary resolution: relative to system folder
                // Only attempt this if the original token was not absolute and didn't start with %BASEFOLDER%
                if ((string.IsNullOrEmpty(resolvedPath) || !(File.Exists(resolvedPath) || Directory.Exists(resolvedPath))) && // Check if primary resolution failed or didn't find the path
                    !Path.IsPathRooted(trimmedToken) && !trimmedToken.StartsWith("%BASEFOLDER%", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(resolvedSystemFolder))
                {
                    resolvedPath = PathHelper.CombineAndResolveRelativeToAppDirectory(resolvedSystemFolder, trimmedToken);
                }

                // If resolution was successful (either primary or secondary) and the path exists, return the resolved path.
                // Otherwise, return the original token.
                if (!string.IsNullOrEmpty(resolvedPath) && (File.Exists(resolvedPath) || Directory.Exists(resolvedPath)))
                {
                    return token.StartsWith('"') ? $"\"{resolvedPath}\"" :
                        token.StartsWith('\'') ? $"'{resolvedPath}'" :
                        resolvedPath;
                }
            }
            catch (Exception ex)
            {
                _ = LogErrors.LogErrorAsync(ex, $"Error resolving parameter path token '{token}'.");
            }

            return token;
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