#nullable enable

using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace SimpleLauncher.Core.Services.CheckPaths;

public static partial class PathHelper
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

    private static bool IsPathContainedInBaseFolder(string resolvedPath, string baseFolder)
    {
        if (string.IsNullOrEmpty(resolvedPath) || string.IsNullOrEmpty(baseFolder))
        {
            return false;
        }

        var normalizedResolved = Path.GetFullPath(resolvedPath);
        var normalizedBase = Path.GetFullPath(baseFolder);

        if (normalizedResolved.Equals(normalizedBase, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!normalizedBase.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal) &&
            !normalizedBase.EndsWith(Path.AltDirectorySeparatorChar.ToString(), StringComparison.Ordinal))
        {
            normalizedBase += Path.DirectorySeparatorChar;
        }

        return normalizedResolved.StartsWith(normalizedBase, StringComparison.OrdinalIgnoreCase);
    }

    public static bool ContainsGameSpecificPlaceholder(string? text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;

        return GameSpecificPlaceholders.Any(placeholder =>
            text.Contains(placeholder, StringComparison.OrdinalIgnoreCase));
    }

    public static string ResolveParameterString(
        string parameters,
        List<string>? systemFolders = null,
        string? resolvedEmulatorFolderPath = null,
        string? resolvedRomPath = null,
        string? romSystemFolder = null,
        string? resolvedRomName = null)
    {
        if (string.IsNullOrWhiteSpace(parameters))
        {
            return string.Empty;
        }

        var pathTokenRegex = MyRegex();

        var resolvedParameters = pathTokenRegex.Replace(parameters, match =>
        {
            if (match is not { Success: true })
            {
                return string.Empty;
            }

            var originalToken = match.Value;
            var isQuotedToken = (originalToken.StartsWith('"') && originalToken.EndsWith('"')) ||
                                (originalToken.StartsWith('\'') && originalToken.EndsWith('\''));
            var tokenForLogic = isQuotedToken ? originalToken[1..^1] : originalToken;

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

            processedToken = Environment.ExpandEnvironmentVariables(processedToken);

            var finalTokenValue = processedToken;

            if (isQuotedToken)
            {
                if (originalToken.StartsWith('"'))
                    return $"\"{finalTokenValue}\"";

                return $"'{finalTokenValue}'";
            }

            if (finalTokenValue.Contains(' ') && !isQuotedToken && !finalTokenValue.Contains('"') && !finalTokenValue.Contains('\''))
            {
                return $"\"{finalTokenValue}\"";
            }

            return finalTokenValue;
        });

        return resolvedParameters;
    }

    public static string ResolveRelativeToCurrentWorkingDirectory(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return string.Empty;
        }

        return Path.GetFullPath(path);
    }

    public static string? ResolveRelativeToAppDirectory(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) || path.Length > MaxPathLength)
        {
            return null;
        }

        string basePath;
        var remainingPath = path;

        if (path.StartsWith(BaseFolderPlaceholder, StringComparison.OrdinalIgnoreCase))
        {
            basePath = AppDomain.CurrentDomain.BaseDirectory;
            remainingPath = path[BaseFolderPlaceholder.Length..].TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
        else if (Path.IsPathRooted(path))
        {
            basePath = string.Empty;
        }
        else
        {
            basePath = AppDomain.CurrentDomain.BaseDirectory;
        }

        try
        {
            var combinedPath = Path.Combine(basePath, remainingPath);
            return Path.GetFullPath(combinedPath);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PathHelper] Error resolving path '{path}' relative to app directory: {ex.Message}");
            return null;
        }
    }

    public static bool IsRelativePathWithoutBaseFolder(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        return !Path.IsPathRooted(path) && !path.StartsWith(BaseFolderPlaceholder, StringComparison.OrdinalIgnoreCase);
    }

    private static string CombineAndResolveRelativeToAppDirectory(string path1, string path2)
    {
        var resolvedPath1 = ResolveRelativeToAppDirectory(path1);

        if (string.IsNullOrEmpty(resolvedPath1))
        {
            return string.Empty;
        }

        var combinedPath = Path.Combine(resolvedPath1, path2);
        return ResolveRelativeToAppDirectory(combinedPath) ?? string.Empty;
    }

    public static string GetFileNameWithoutExtension(string path)
    {
        return Path.GetFileNameWithoutExtension(path);
    }

    public static string GetFileName(string path)
    {
        return Path.GetFileName(path);
    }

    public static string SanitizePathToken(string? pathTokenValue)
    {
        if (string.IsNullOrEmpty(pathTokenValue))
        {
            return string.Empty;
        }

        return pathTokenValue.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    public static string GetLongPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path) ||
            path.StartsWith(@"\\?\", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith(@"\\.\", StringComparison.OrdinalIgnoreCase))
        {
            if (path != null) return path;
        }

        if (path != null && path.StartsWith(@"\\", StringComparison.Ordinal))
        {
            return @"\\?\UNC\" + path[2..];
        }

        return @"\\?\" + path;
    }

    public static string? FindFileInSystemFolders(List<string>? systemFolders, string? fileName)
    {
        if (systemFolders == null || systemFolders.Count == 0 || string.IsNullOrEmpty(fileName))
        {
            return null;
        }

        foreach (var folder in systemFolders)
        {
            var filePath = CombineAndResolveRelativeToAppDirectory(folder, fileName);
            if (string.IsNullOrEmpty(filePath)) continue;

            var longPath = GetLongPath(filePath);

            if (File.Exists(filePath) || File.Exists(longPath))
            {
                return filePath;
            }
        }

        return null;
    }

    public static string? FindContainingSystemFolder(List<string>? systemFolders, string? primarySystemFolder, string? filePath)
    {
        if (systemFolders == null || systemFolders.Count == 0 || string.IsNullOrEmpty(filePath))
        {
            return primarySystemFolder;
        }

        var normalizedFilePath = Path.GetFullPath(filePath);

        foreach (var folder in systemFolders)
        {
            var resolvedFolder = ResolveRelativeToAppDirectory(folder);
            if (string.IsNullOrEmpty(resolvedFolder)) continue;

            var normalizedFolder = Path.GetFullPath(resolvedFolder);

            if (IsPathContainedInBaseFolder(normalizedFilePath, normalizedFolder))
            {
                return resolvedFolder;
            }
        }

        return primarySystemFolder;
    }

    [GeneratedRegex("""
                    "[^"]*"|'[^']*'|\S+
                    """)]
    private static partial Regex MyRegex();

    public static string? TryGetExistingDirectory(string? folderPath)
    {
        if (string.IsNullOrWhiteSpace(folderPath))
        {
            return null;
        }

        try
        {
            var resolved = ResolveRelativeToAppDirectory(folderPath);
            if (!string.IsNullOrEmpty(resolved) && Directory.Exists(resolved))
            {
                return resolved;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[PathHelper] Error resolving '{folderPath}': {ex.Message}");
        }

        return null;
    }

    public static string? TryFindFileWithNormalizedPath(string? filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return null;
        }

        try
        {
            var directoryPath = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);

            if (string.IsNullOrEmpty(directoryPath) || string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            if (!Directory.Exists(directoryPath))
            {
                return null;
            }

            var normalizedFileNames = new HashSet<string>(StringComparer.Ordinal)
            {
                fileName,
                fileName.Normalize(NormalizationForm.FormC),
                fileName.Normalize(NormalizationForm.FormD),
                fileName.Normalize(NormalizationForm.FormKC),
                fileName.Normalize(NormalizationForm.FormKD)
            };

            foreach (var existingFile in Directory.EnumerateFiles(directoryPath))
            {
                var existingFileName = Path.GetFileName(existingFile);

                foreach (var normalizedFileName in normalizedFileNames)
                {
                    if (existingFileName.Equals(normalizedFileName, StringComparison.OrdinalIgnoreCase))
                    {
                        return existingFile;
                    }
                }
            }

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
            Debug.WriteLine($"[PathHelper] Error during normalization search: {ex.Message}");
        }

        return null;
    }
}
