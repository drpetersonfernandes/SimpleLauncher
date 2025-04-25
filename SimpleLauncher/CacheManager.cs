using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MessagePack;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public class CacheManager
{
    private const string CacheDirectory = "cache";
    private readonly Dictionary<string, List<string>> _cachedGameFiles = new();

    public CacheManager()
    {
        EnsureCacheDirectoryExists();
    }

    private static void EnsureCacheDirectoryExists()
    {
        var cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CacheDirectory);
        if (Directory.Exists(cacheDir)) return;

        try
        {
            IoOperations.CreateDirectory(cacheDir);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error creating cache directory. User was not notified.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    /// <summary>
    /// Loads system files, either from cache or by rescanning.
    /// </summary>
    public async Task<List<string>> LoadSystemFilesAsync(string systemName, string systemFolderPath, List<string> fileExtensions, int gameCount)
    {
        if (systemFolderPath == null || fileExtensions == null || gameCount == 0)
        {
            // Return an empty list
            return [];
        }

        var cacheFilePath = GetCacheFilePath(systemName);

        // Check if cache exists
        if (!File.Exists(cacheFilePath)) return await RebuildCache(systemName, systemFolderPath, fileExtensions);

        var cachedData = await LoadCacheFromDisk(cacheFilePath);

        // Compare file count (using `gameCount` from MainWindow)
        // If cache doesn't exist or count differs, rebuild cache
        if (cachedData.FileCount != gameCount)
            return await RebuildCache(systemName, systemFolderPath, fileExtensions);

        _cachedGameFiles[systemName] = cachedData.FileNames;
        return cachedData.FileNames;
    }

    /// <summary>
    /// Rebuilds the cache by scanning the system folder.
    /// </summary>
    private async Task<List<string>> RebuildCache(string systemName, string systemFolderPath, List<string> fileExtensions)
    {
        if (systemFolderPath == null || fileExtensions == null)
        {
            // Return an empty list
            return [];
        }

        var files = await Task.Run(() =>
        {
            var fileList = new List<string>();
            try
            {
                foreach (var extension in fileExtensions)
                {
                    // Ensure the extension is in search pattern format.
                    var searchPattern = extension.StartsWith('*') ? extension : "*." + extension;
                    var foundFiles = Directory.EnumerateFiles(systemFolderPath, searchPattern, SearchOption.TopDirectoryOnly);
                    fileList.AddRange(foundFiles);
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                var contextMessage = $"Error caching files for {systemName}.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
            }

            return fileList;
        });

        if (files == null)
        {
            // Return an empty list
            return [];
        }

        _cachedGameFiles[systemName] = files;
        await SaveCacheToDisk(systemName, files);

        return files;
    }

    /// <summary>
    /// Saves the cache to a binary file inside the Cache folder using MessagePack.
    /// </summary>
    private static async Task SaveCacheToDisk(string systemName, List<string> fileNames)
    {
        try
        {
            var cacheFilePath = GetCacheFilePath(systemName);
            var cacheData = new GameCache
            {
                FileCount = fileNames.Count,
                FileNames = fileNames
            };

            var binaryData = MessagePackSerializer.Serialize(cacheData);
            await File.WriteAllBytesAsync(cacheFilePath, binaryData);
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorMessage = $"Error saving cache for {systemName}.";
            _ = LogErrors.LogErrorAsync(ex, errorMessage);
        }
    }

    /// <summary>
    /// Loads cache from a binary file inside the Cache folder using MessagePack.
    /// </summary>
    private static async Task<GameCache> LoadCacheFromDisk(string cacheFilePath)
    {
        try
        {
            var binaryData = await File.ReadAllBytesAsync(cacheFilePath);
            return MessagePackSerializer.Deserialize<GameCache>(binaryData);
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorMessage = $"Error loading cache file {cacheFilePath}.";
            _ = LogErrors.LogErrorAsync(ex, errorMessage);

            // Return an empty cache
            return new GameCache { FileCount = 0, FileNames = [] };
        }
    }

    /// <summary>
    /// Returns the cache file path inside the Cache folder for a given system.
    /// </summary>
    private static string GetCacheFilePath(string systemName)
    {
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CacheDirectory, $"{systemName}.cache");
    }

    /// <summary>
    /// Returns the cached files for the given system.
    /// </summary>
    public List<string> GetCachedFiles(string systemName)
    {
        return _cachedGameFiles.TryGetValue(systemName, out var files) ? files : [];
    }
}

[MessagePackObject]
public class GameCache
{
    [Key(0)]
    public int FileCount { get; set; }

    [Key(1)]
    public List<string> FileNames { get; set; }
}