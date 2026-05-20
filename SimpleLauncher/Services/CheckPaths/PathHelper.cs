using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.CheckPaths;

internal static partial class PathHelper
{
    private const string BaseFolderPlaceholder = "%BASEFOLDER%";

    // Path length limit to prevent potential recursion issues or overflows
    private const int MaxPathLength = 4096;

    private static readonly string[] GameSpecificPlaceholders =
    [
        "%GAME%", "%ROMNAME%", "%ROMFILE%", "$game$", "$romname$", "$romfile$",
        "{game}", "{romname}", "{romfile}"
    ];

    private static readonly string[] KnownParameterFlags =
    [
        "-f", "--fullscreen", "/f", "-window", "-fullscreen", "--window", "-cart",
        "-L", "-g", "-rompath"
    ];

    private static bool IsKnownFlag(string text)
    {
        return KnownParameterFlags.Any(flag =>
            string.Equals(text, flag, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Validates that a resolved path is contained within a base folder.
    /// This prevents path traversal attacks where a malicious path could escape the intended directory.
    /// </summary>
    /// <param name="resolvedPath">The fully resolved (absolute) path to validate.</param>
    /// <param name="baseFolder">The base folder that the resolved path should be contained within.</param>
    /// <returns>True if the resolved path is within the base folder, false otherwise.</returns>
    private static bool IsPathContainedInBaseFolder(string resolvedPath, string baseFolder)
    {
        if (string.IsNullOrEmpty(resolvedPath) || string.IsNullOrEmpty(baseFolder))
        {
            return false;
        }

        // Normalize paths for comparison
        var normalizedResolved = Path.GetFullPath(resolvedPath);
        var normalizedBase = Path.GetFullPath(baseFolder);

        // Exact match is valid (e.g., resolvedPath == baseFolder)
        if (normalizedResolved.Equals(normalizedBase, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        // Ensure base folder ends with separator for proper containment check
        if (!normalizedBase.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) &&
            !normalizedBase.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
        {
            normalizedBase += Path.DirectorySeparatorChar;
        }

        // Check if the resolved path starts with the base folder path
        return normalizedResolved.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase);
    }

    internal static bool ContainsGameSpecificPlaceholder(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        return GameSpecificPlaceholders.Any(placeholder =>
            text.Contains(placeholder, StringComparison.OrdinalIgnoreCase));
    }

    public static string ResolveParameterString(
        string parameters,
        List<string> systemFolders = null,
        string resolvedEmulatorFolderPath = null,
        string resolvedRomPath = null,
        string romSystemFolder = null,
        string resolvedRomName = null)
    {
        if (string.IsNullOrWhiteSpace(parameters))
        {
            return string.Empty;
        }

        var pathTokenRegex = MyRegex();

        var resolvedParameters = pathTokenRegex.Replace(parameters, match =>
        {
            // Ensure match is valid to prevent NRE
            if (match is not { Success: true })
            {
                return string.Empty;
            }

            var originalToken = match.Value;
            var isQuotedToken = (originalToken.StartsWith('"') && originalToken.EndsWith('"')) ||
                                (originalToken.StartsWith('\'') && originalToken.EndsWith('\''));
            var tokenForLogic = isQuotedToken ? originalToken[1..^1] : originalToken;

            // Skip processing if it's a game-specific placeholder, a known flag, or doesn't contain our custom placeholders.
            if (ContainsGameSpecificPlaceholder(tokenForLogic) ||
                IsKnownFlag(tokenForLogic) ||
                (!tokenForLogic.Contains("%BASEFOLDER%", StringComparison.OrdinalIgnoreCase) &&
                 !tokenForLogic.Contains("%SYSTEMFOLDER%", StringComparison.OrdinalIgnoreCase) &&
                 !tokenForLogic.Contains("%ROMSYSTEMFOLDER%", StringComparison.OrdinalIgnoreCase) &&
                 !tokenForLogic.Contains("%EMULATORFOLDER%", StringComparison.OrdinalIgnoreCase) &&
                 !tokenForLogic.Contains("%ROM%", StringComparison.OrdinalIgnoreCase) &&
                 !tokenForLogic.Contains("%NAME%", StringComparison.OrdinalIgnoreCase)))
            {
                return originalToken;
            }

            var processedToken = tokenForLogic;

            // Replace custom placeholders
            if (processedToken.Contains("%BASEFOLDER%", StringComparison.OrdinalIgnoreCase))
            {
                processedToken = processedToken.Replace("%BASEFOLDER%", SanitizePathToken(AppDomain.CurrentDomain.BaseDirectory), StringComparison.OrdinalIgnoreCase);
            }

            if (processedToken.Contains("%SYSTEMFOLDER%", StringComparison.OrdinalIgnoreCase))
            {
                var resolvedSystemFolderPaths = string.Empty;
                if (systemFolders is { Count: > 0 })
                {
                    var resolvedFolders = systemFolders
                        .Select(ResolveRelativeToAppDirectory)
                        .Where(static resolved => !string.IsNullOrEmpty(resolved))
                        .Select(SanitizePathToken)
                        .ToList();

                    if (resolvedFolders.Count > 0)
                    {
                        resolvedSystemFolderPaths = string.Join(";", resolvedFolders);
                    }
                }

                processedToken = processedToken.Replace("%SYSTEMFOLDER%", resolvedSystemFolderPaths, StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrEmpty(romSystemFolder) && processedToken.Contains("%ROMSYSTEMFOLDER%", StringComparison.OrdinalIgnoreCase))
            {
                processedToken = processedToken.Replace("%ROMSYSTEMFOLDER%", SanitizePathToken(romSystemFolder), StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrEmpty(resolvedEmulatorFolderPath) && processedToken.Contains("%EMULATORFOLDER%", StringComparison.OrdinalIgnoreCase))
            {
                processedToken = processedToken.Replace("%EMULATORFOLDER%", SanitizePathToken(resolvedEmulatorFolderPath), StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrEmpty(resolvedRomPath) && processedToken.Contains("%ROM%", StringComparison.OrdinalIgnoreCase))
            {
                processedToken = processedToken.Replace("%ROM%", SanitizePathToken(resolvedRomPath), StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrEmpty(resolvedRomName) && processedToken.Contains("%NAME%", StringComparison.OrdinalIgnoreCase))
            {
                processedToken = processedToken.Replace("%NAME%", resolvedRomName, StringComparison.OrdinalIgnoreCase);
            }

            // Expand environment variables
            processedToken = Environment.ExpandEnvironmentVariables(processedToken);

            var finalTokenValue = processedToken;

            // Re-apply quotes if necessary
            if (isQuotedToken)
            {
                if (originalToken.StartsWith('"'))
                    return $"\"{finalTokenValue}\"";

                return $"'{finalTokenValue}'";
            }

            // Add quotes if the resolved path contains spaces and wasn't originally quoted
            // but only if the value doesn't already have internal quotes (they already protect the spaces)
            if (finalTokenValue.Contains(' ') && !isQuotedToken && !finalTokenValue.Contains('"') && !finalTokenValue.Contains('\''))
            {
                return $"\"{finalTokenValue}\"";
            }

            return finalTokenValue;
        });

        return resolvedParameters;
    }

    /// <summary>
    /// Converts a path to its absolute form, resolving relative paths against the current working directory.
    /// Handles '.', '..', and resolves symbolic links if the file/directory exists.
    /// Returns a canonical absolute path.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The canonical absolute path relative to the current working directory.</returns>
    public static string ResolveRelativeToCurrentWorkingDirectory(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        // This method is specifically for the current working directory, so no %BASEFOLDER% handling here.
        return Path.GetFullPath(path);
    }

    /// <summary>
    /// Converts a path to its absolute form, resolving relative paths against the application's base directory.
    /// If the path is already absolute, it is canonicalized.
    /// Handles the %BASEFOLDER% placeholder.
    /// Returns a canonical absolute path.
    /// </summary>
    /// <param name="path">The path to convert.</param>
    /// <returns>The canonical absolute path (relative to the application base directory if the input was relative or used %BASEFOLDER%, otherwise the canonicalized original absolute path).</returns>
    public static string ResolveRelativeToAppDirectory(string path)
    {
        // Check path length to prevent infinite recursion risks or overflows
        if (string.IsNullOrWhiteSpace(path) || path.Length > MaxPathLength)
        {
            return null;
        }

        string basePath;
        var remainingPath = path;

        // Check for the %BASEFOLDER% placeholder
        if (path.StartsWith(BaseFolderPlaceholder, StringComparison.OrdinalIgnoreCase))
        {
            basePath = AppDomain.CurrentDomain.BaseDirectory;
            // Remove the placeholder and any trailing separators
            remainingPath = path[BaseFolderPlaceholder.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }

        else if (Path.IsPathRooted(path))
        {
            // If the path is already rooted (absolute), use it directly as the base.
            // Path.GetFullPath below will canonicalize it.
            basePath = string.Empty; // Path.Combine handles this case correctly
        }

        else
        {
            // If the path is relative and doesn't use %BASEFOLDER%,
            // resolve it relative to the application's base directory.
            basePath = AppDomain.CurrentDomain.BaseDirectory;
        }

        try
        {
            // Path.Combine handles combining basePath and remainingPath correctly,
            // including cases where basePath is empty (for absolute paths).
            var combinedPath = Path.Combine(basePath, remainingPath);

            // Path.GetFullPath resolves '.\', '..\' segments and ensures it's canonical.
            return Path.GetFullPath(combinedPath);
        }
        catch (Exception ex)
        {
            // Log the error and return null to indicate resolution failure.
            // The calling code should handle the null return.

            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Error resolving path '{path}' relative to app directory.");

            return null;
        }
    }

    /// <summary>
    /// Checks if a path is relative and does NOT start with the %BASEFOLDER% placeholder.
    /// </summary>
    /// <param name="path">The path string.</param>
    /// <returns>True if the path is relative and does not start with %BASEFOLDER%, false otherwise.</returns>
    public static bool IsRelativePathWithoutBaseFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        // It's relative if it's not rooted AND doesn't start with the placeholder
        return !Path.IsPathRooted(path) && !path.StartsWith(BaseFolderPlaceholder, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Combines two path segments and resolves the result to a canonical absolute path
    /// relative to the application's base directory if the combined path is not rooted.
    /// Handles the %BASEFOLDER% placeholder in path1.
    /// </summary>
    /// <param name="path1">The first path segment (can contain %BASEFOLDER%).</param>
    /// <param name="path2">The second path segment.</param>
    /// <returns>The canonical absolute path resulting from combining path1 and path2, resolved relative to the application base directory if needed.</returns>
    private static string CombineAndResolveRelativeToAppDirectory(string path1, string path2)
    {
        // Resolve path1 first, which handles %BASEFOLDER% and relative-to-app resolution
        var resolvedPath1 = ResolveRelativeToAppDirectory(path1);

        // If path1 resolution failed, combining will also likely fail or be meaningless
        if (string.IsNullOrEmpty(resolvedPath1))
        {
            return string.Empty;
        }

        // Combine the resolved path1 with path2.
        // Path.Combine handles cases where path2 is absolute (it will ignore path1).
        var combinedPath = Path.Combine(resolvedPath1, path2);

        // Resolve the final combined path. This handles cases where path2 was absolute
        // or resolves any remaining '.' or '..' segments from the combination.
        return ResolveRelativeToAppDirectory(combinedPath);
    }

    public static string GetFileNameWithoutExtension(string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }

    public static string GetFileName(string path)
    {
        return Path.GetFileName(path);
    }

    /// <summary>
    /// Sanitizes a path string intended for use as a token in string replacements.
    /// It removes any trailing directory separator characters.
    /// E.g., "C:\MyFolder\" becomes "C:\MyFolder".
    /// This helps prevent double separators when concatenating with a path segment like "\subfolder".
    /// </summary>
    /// <param name="pathTokenValue">The path string to sanitize.</param>
    /// <returns>The sanitized path string without a trailing separator.</returns>
    public static string SanitizePathToken(string pathTokenValue)
    {
        if (string.IsNullOrEmpty(pathTokenValue))
        {
            return string.Empty;
        }

        return pathTokenValue.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    /// <summary>
    /// Converts a path to an extended-length path when needed.
    /// Preserves UNC paths by converting them to the \?\UNC\ form.
    /// </summary>
    /// <param name="path">The input path.</param>
    /// <returns>An extended-length path, or the original path when already extended or not applicable.</returns>
    public static string GetLongPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            path.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith(@"\\.\", StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }

        if (path.StartsWith(@"\\", StringComparison.Ordinal))
        {
            return @"\\?\UNC\" + path[2..];
        }

        return @"\\?\" + path;
    }

    public static string FindFileInSystemFolders(SystemManager.SystemManager systemManager, string fileName)
    {
        if (systemManager?.SystemFolders == null || string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        foreach (var folder in systemManager.SystemFolders)
        {
            var filePath = CombineAndResolveRelativeToAppDirectory(folder, fileName);
            if (string.IsNullOrEmpty(filePath)) continue;

            var longPath = GetLongPath(filePath);

            // Check both standard and long-path prefixed versions
            if (File.Exists(filePath) || File.Exists(longPath))
            {
                return filePath;
            }
        }

        return null;
    }

    /// <summary>
    /// Finds which system folder contains the given file path.
    /// Iterates through all configured system folders and returns the resolved path
    /// of the folder that actually contains the file.
    /// Falls back to the primary (first) system folder if no match is found.
    /// </summary>
    /// <param name="systemManager">The system manager containing the configured folders.</param>
    /// <param name="filePath">The absolute path to the file.</param>
    /// <returns>The resolved system folder path that contains the file, or the primary system folder as fallback.</returns>
    public static string FindContainingSystemFolder(SystemManager.SystemManager systemManager, string filePath)
    {
        if (systemManager?.SystemFolders == null || string.IsNullOrEmpty(filePath))
        {
            return systemManager?.PrimarySystemFolder;
        }

        var normalizedFilePath = Path.GetFullPath(filePath);

        foreach (var folder in systemManager.SystemFolders)
        {
            var resolvedFolder = ResolveRelativeToAppDirectory(folder);
            if (string.IsNullOrEmpty(resolvedFolder)) continue;

            var normalizedFolder = Path.GetFullPath(resolvedFolder);

            if (IsPathContainedInBaseFolder(normalizedFilePath, normalizedFolder))
            {
                return resolvedFolder;
            }
        }

        return systemManager.PrimarySystemFolder;
    }

    [GeneratedRegex("""
                    "[^"]*"|'[^']*'|\S+
                    """)]
    private static partial Regex MyRegex();

    /// <summary>
    /// Attempts to find a file by trying different Unicode normalization forms.
    /// This is necessary because Windows filesystems can store filenames with different
    /// normalization forms (NFC vs NFD), especially when files are created on different
    /// operating systems (e.g., macOS uses NFD by default, Windows uses NFC).
    /// </summary>
    /// <param name="filePath">The original file path to check.</param>
    /// <returns>The normalized path if found, otherwise null.</returns>
    public static string TryFindFileWithNormalizedPath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        try
        {
            // Get the directory and filename components
            var directoryPath = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);

            if (string.IsNullOrEmpty(directoryPath) || string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            // If directory doesn't exist, no point in searching
            if (!Directory.Exists(directoryPath))
            {
                return null;
            }

            // Try different Unicode normalization forms for the filename
            var normalizedFileNames = new HashSet<string>(StringComparer.Ordinal)
            {
                fileName, // Original
                fileName.Normalize(NormalizationForm.FormC), // NFC (Windows default)
                fileName.Normalize(NormalizationForm.FormD), // NFD (macOS default)
                fileName.Normalize(NormalizationForm.FormKC), // NFKC
                fileName.Normalize(NormalizationForm.FormKD) // NFKD
            };

            // Get all files in the directory and check for normalized matches
            foreach (var existingFile in Directory.EnumerateFiles(directoryPath))
            {
                var existingFileName = Path.GetFileName(existingFile);

                // Check if any normalized form matches the existing file (case-insensitive)
                foreach (var normalizedFileName in normalizedFileNames)
                {
                    if (existingFileName.Equals(normalizedFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        // Found a match, return the full path with the actual filename from disk
                        return existingFile;
                    }
                }
            }

            // Also check directories if the original path was a directory
            foreach (var existingDir in Directory.EnumerateDirectories(directoryPath))
            {
                var existingDirName = Path.GetFileName(existingDir);

                foreach (var normalizedFileName in normalizedFileNames)
                {
                    if (existingDirName.Equals(normalizedFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        return existingDir;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Log but don't throw - this is a best-effort attempt
            DebugLogger.Log($"[TryFindFileWithNormalizedPath] Error during normalization search: {ex.Message}");
        }

        return null;
    }
}