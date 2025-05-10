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
    // Regular expression to detect potential paths in parameter strings
    // private static readonly Regex PathRegex = new(
    //     @"(?:""|')([^""']+)(?:""|')|(?:(?:^|\s)(?:-\w+\s+)?(?:[A-Za-z]:)?[\\\/](?:[^""\s\\\/;]+[\\\/])+[^""\s\\\/;]*)",
    //     RegexOptions.Compiled);

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
    private static readonly char[] Separator2 = [';'];
    private static readonly char[] Separator3 = [';'];
    private static readonly char[] Separator4 = [' ', '\t'];
    private static readonly char[] Separator5 = [';'];

    /// <summary>
    /// Checks if a path exists (either as an absolute path or relative to the application directory)
    /// </summary>
    public static bool IsValidPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) return false;

        // Directly check if the path exists (for absolute paths)
        if (Directory.Exists(path) || File.Exists(path)) return true;

        // Allow relative paths
        // Combine with the base directory to check for relative paths
        var basePath = AppDomain.CurrentDomain.BaseDirectory;
        try
        {
            // Ensure we correctly handle relative paths that go up from the base directory
            var fullPath = Path.GetFullPath(new Uri(Path.Combine(basePath, path)).LocalPath);
            return Directory.Exists(fullPath) || File.Exists(fullPath);
        }
        catch (Exception)
        {
            // If there's any exception parsing the path, consider it invalid
            return false;
        }
    }

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
    /// Checks if a path contains a known placeholder
    /// </summary>
    private static bool ContainsPlaceholder(string path)
    {
        return KnownPlaceholders.Any(placeholder =>
            path.Contains(placeholder, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Validates a single path with multiple resolution strategies
    /// </summary>
    private static bool ValidateSinglePath(string path, string baseDir, string systemFolder = null)
    {
        // Skip ROM placeholders
        if (ContainsPlaceholder(path)) return true;

        // Expand environment variables
        if (path.Contains('%'))
        {
            path = Environment.ExpandEnvironmentVariables(path);
        }

        try
        {
            // Try as an absolute path
            if (File.Exists(path) || Directory.Exists(path))
                return true;

            // Try as relative to the app directory
            var appRelativePath = Path.GetFullPath(Path.Combine(baseDir, path));
            if (File.Exists(appRelativePath) || Directory.Exists(appRelativePath))
                return true;

            // Try as relative to the system folder if provided
            if (string.IsNullOrEmpty(systemFolder)) return false;

            var systemRelativePath = Path.GetFullPath(Path.Combine(systemFolder, path));
            if (File.Exists(systemRelativePath) || Directory.Exists(systemRelativePath))
                return true;

            return false;
        }
        catch (Exception)
        {
            // If there's any exception parsing the path, consider it invalid
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
        var flaggedPathRegex = MyRegex();
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
    public static bool ValidateParameterPaths(string parameters, out List<string> invalidPaths, string systemFolder = null, bool isMameSystem = false)
    {
        invalidPaths = [];
        if (string.IsNullOrWhiteSpace(parameters)) return true;

        var allPathsValid = true;
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;

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

                        if (Directory.Exists(trimmedPath)) continue;

                        invalidPaths.Add(trimmedPath);
                        allPathsValid = false;
                    }

                    break;
                }
                case "-L":
                {
                    // For library paths (-L), check for file existence
                    if (!string.IsNullOrWhiteSpace(path) && !ContainsPlaceholder(path) && !File.Exists(path))
                    {
                        invalidPaths.Add(path);
                        allPathsValid = false;
                    }

                    break;
                }
                default:
                {
                    // For other parameters, check using standard path validation
                    if (!string.IsNullOrWhiteSpace(path) &&
                        !ContainsPlaceholder(path) &&
                        LooksLikePath(path) &&
                        !ValidateSinglePath(path, baseDir, systemFolder))
                    {
                        invalidPaths.Add(path);
                        allPathsValid = false;
                    }

                    break;
                }
            }
        }

        // Process all quoted paths that might not be associated with flags
        var quotedPathsRegex = MyRegex1();
        var quotedMatches = quotedPathsRegex.Matches(parameters);
        foreach (Match match in quotedMatches)
        {
            // Get the value from whichever group matched
            var quotedPath = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;

            // Skip if it's not a path-like string, contains a placeholder,
            // or was already validated as a parameter path
            if (!LooksLikePath(quotedPath) ||
                ContainsPlaceholder(quotedPath) ||
                parameterPaths.Any(p => p.Path == quotedPath)) continue;

            // Handle multi-paths separated by semicolons (like in -rompath)
            if (quotedPath.Contains(';'))
            {
                // Split by semicolons and validate each part
                var subPaths = quotedPath.Split(Separator3, StringSplitOptions.RemoveEmptyEntries);
                foreach (var subPath in subPaths)
                {
                    var trimmedSubPath = subPath.Trim();
                    if (string.IsNullOrWhiteSpace(trimmedSubPath) || ContainsPlaceholder(trimmedSubPath)) continue;

                    // Check if the path exists
                    var pathValid = Directory.Exists(trimmedSubPath) || File.Exists(trimmedSubPath);
                    if (pathValid) continue;

                    invalidPaths.Add(trimmedSubPath);
                    allPathsValid = false;
                }
            }
            else
            {
                // Single path, validate normally
                if (ValidateSinglePath(quotedPath, baseDir, systemFolder)) continue;

                invalidPaths.Add(quotedPath);
                allPathsValid = false;
            }
        }

        // Process remaining unquoted potential paths (less common)
        var remainingParams = MyRegex2().Replace(parameters, " ");
        var flagsRemoved = MyRegex3().Replace(remainingParams, " ");

        // Split by whitespace and check each token
        var words = flagsRemoved.Split(Separator4, StringSplitOptions.RemoveEmptyEntries);
        foreach (var word in words)
        {
            // Skip known parameter flags or placeholders
            if (IsKnownFlag(word) || ContainsPlaceholder(word)) continue;

            // If it looks like a path, validate it
            if (!LooksLikePath(word) || ValidateSinglePath(word, baseDir, systemFolder)) continue;

            invalidPaths.Add(word);
            allPathsValid = false;
        }

        // For MAME systems, we can be more lenient with some paths
        // but not for critical paths like -rompath or -L
        if (!isMameSystem || invalidPaths.Count <= 0) return allPathsValid;

        {
            // Only be lenient for certain parameters
            var criticalPaths = invalidPaths
                .Where(path => parameterPaths.Any(p =>
                    p.Flag is "-rompath" or "-L" &&
                    (p.Path == path || p.Path.Contains(path))))
                .ToList();

            if (criticalPaths.Count == 0)
            {
                // Only non-critical paths are invalid, so we can be lenient
                return true;
            }
        }

        return allPathsValid;
    }

    /// <summary>
    /// Validates emulator parameters and returns invalid paths
    /// </summary>
    public static (bool success, List<string> invalidPaths) ValidateEmulatorParameters(string parameters, string systemFolder = null, bool isMameSystem = false)
    {
        if (string.IsNullOrWhiteSpace(parameters))
        {
            return (true, null); // No parameters are valid
        }

        var pathsValid = ValidateParameterPaths(parameters, out var invalidPaths, systemFolder, isMameSystem);

        if (pathsValid || invalidPaths.Count <= 0) return (true, null); // No error

        // Extract all parameter paths for more detailed analysis
        var paramPaths = ExtractParameterPaths(parameters);

        // Add any additional invalid paths that may have been missed
        foreach (var (flag, path) in paramPaths)
        {
            switch (flag)
            {
                case "-L" when !File.Exists(path) && !invalidPaths.Contains(path):
                    invalidPaths.Add(path);
                    break;
                case "-rompath":
                {
                    // For rompath, check all semicolon-separated paths
                    if (path != null)
                    {
                        var romPaths = path.Split(Separator5, StringSplitOptions.RemoveEmptyEntries);
                        foreach (var romPath in romPaths)
                        {
                            var trimmedPath = romPath.Trim();
                            if (!Directory.Exists(trimmedPath) && !invalidPaths.Contains(trimmedPath))
                            {
                                invalidPaths.Add(trimmedPath);
                            }
                        }
                    }

                    break;
                }
            }
        }

        return (false, invalidPaths);
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