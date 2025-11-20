using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Managers;

namespace SimpleLauncher.Services;

public static class PathHelper
{
    private const string BaseFolderPlaceholder = "%BASEFOLDER%";

    // --- Parameter Resolution Logic ---

    private static readonly string[] GameSpecificPlaceholders =
    [
        "%ROM%", "%GAME%", "%ROMNAME%", "%ROMFILE%", "$rom$", "$game$", "$romname$", "$romfile$",
        "{rom}", "{game}", "{romname}", "{romfile}"
    ];

    private static readonly string[] KnownParameterFlags =
    [
        "-f", "--fullscreen", "/f", "-window", "-fullscreen", "--window", "-cart",
        "-L", "-g", "-rompath"
    ];

    private static readonly char[] Separator = ['\\', '/'];

    private static bool LooksLikePath(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        return text.Contains('\\') || text.Contains('/') ||
               (text.Length >= 2 && text[1] == ':') ||
               text.EndsWith(".exe", StringComparison.OrdinalIgnoreCase) ||
               text.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
               text.Contains(".dll") ||
               text.StartsWith("%BASEFOLDER%", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("%SYSTEMFOLDER%", StringComparison.OrdinalIgnoreCase) ||
               text.StartsWith("%EMULATORFOLDER%", StringComparison.OrdinalIgnoreCase) ||
               IsDirectoryPath(text);
    }

    private static bool IsDirectoryPath(string text)
    {
        var hasDrivePrefix = text.Length >= 2 && text[1] == ':';
        var hasMultipleSegments = text.Split(Separator, StringSplitOptions.RemoveEmptyEntries).Length > 1;

        return (hasDrivePrefix && hasMultipleSegments) ||
               (text.Contains('\\') && hasMultipleSegments) ||
               (text.Contains('/') && hasMultipleSegments);
    }

    private static bool IsKnownFlag(string text)
    {
        return KnownParameterFlags.Any(flag =>
            string.Equals(text, flag, StringComparison.OrdinalIgnoreCase));
    }

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

            if (ContainsGameSpecificPlaceholder(tokenForLogic) || IsKnownFlag(tokenForLogic) || (!LooksLikePath(tokenForLogic) &&
                                                                                                 !tokenForLogic.Contains("%BASEFOLDER%", StringComparison.OrdinalIgnoreCase) &&
                                                                                                 !tokenForLogic.Contains("%SYSTEMFOLDER%", StringComparison.OrdinalIgnoreCase) &&
                                                                                                 !tokenForLogic.Contains("%EMULATORFOLDER%", StringComparison.OrdinalIgnoreCase)))
            {
                return originalToken;
            }

            var processedToken = tokenForLogic;

            if (processedToken.Contains("%BASEFOLDER%", StringComparison.OrdinalIgnoreCase))
            {
                processedToken = processedToken.Replace("%BASEFOLDER%", SanitizePathToken(AppDomain.CurrentDomain.BaseDirectory), StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrEmpty(resolvedSystemFolderPath) && processedToken.Contains("%SYSTEMFOLDER%", StringComparison.OrdinalIgnoreCase))
            {
                processedToken = processedToken.Replace("%SYSTEMFOLDER%", SanitizePathToken(resolvedSystemFolderPath), StringComparison.OrdinalIgnoreCase);
            }

            if (!string.IsNullOrEmpty(resolvedEmulatorFolderPath) && processedToken.Contains("%EMULATORFOLDER%", StringComparison.OrdinalIgnoreCase))
            {
                processedToken = processedToken.Replace("%EMULATORFOLDER%", SanitizePathToken(resolvedEmulatorFolderPath), StringComparison.OrdinalIgnoreCase);
            }

            processedToken = Environment.ExpandEnvironmentVariables(processedToken);

            var finalTokenValue = processedToken;

            if (!processedToken.Contains(';'))
            {
                try
                {
                    if (Path.IsPathRooted(processedToken))
                    {
                        finalTokenValue = Path.GetFullPath(processedToken);
                    }
                    else
                    {
                        var tempResolved = ResolveRelativeToAppDirectory(processedToken);
                        if (!string.IsNullOrEmpty(tempResolved))
                        {
                            finalTokenValue = tempResolved;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Error during path canonicalization for token '{processedToken}'. Using as-is after placeholder/env var replacement: '{finalTokenValue}'.");
                }
            }

            if (originalToken.StartsWith('"') && originalToken.EndsWith('"'))
            {
                return $"\"{finalTokenValue}\"";
            }

            if (originalToken.StartsWith('\'') && originalToken.EndsWith('\''))
            {
                return $"'{finalTokenValue}'";
            }

            if (finalTokenValue.Contains(' ') && !IsQuoted(originalToken))
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
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        string basePath;
        var remainingPath = path;

        // Check for the %BASEFOLDER% placeholder
        if (path.StartsWith(BaseFolderPlaceholder, StringComparison.OrdinalIgnoreCase))
        {
            basePath = AppDomain.CurrentDomain.BaseDirectory;
            // Remove the placeholder and any trailing separators
            remainingPath = path.Substring(BaseFolderPlaceholder.Length).TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
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
            // Log the error but return an empty string to indicate resolution failure.
            // The calling code should handle the empty string.

            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Error resolving path '{path}' relative to app directory.");

            return string.Empty;
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
    /// Searches for a file across all configured system folders for a given system.
    /// </summary>
    /// <param name="systemManager">The system manager containing the folder list.</param>
    /// <param name="fileName">The name of the file to find.</param>
    /// <returns>The full, resolved path to the first found file, or null if not found.</returns>
    public static string FindFileInSystemFolders(SystemManager systemManager, string fileName)
    {
        if (systemManager?.SystemFolders == null || string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        foreach (var folder in systemManager.SystemFolders)
        {
            var filePath = CombineAndResolveRelativeToAppDirectory(folder, fileName);
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                return filePath;
            }
        }

        return null;
    }
}
