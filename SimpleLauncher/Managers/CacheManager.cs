using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher.Managers;

public class CacheManager
{
    private const string CacheDirectory = "cache";
    private readonly Dictionary<string, List<string>> _cachedGameFiles = new();
    private readonly Lock _cacheLock = new();

    public CacheManager()
    {
        EnsureCacheDirectoryExists();
    }

    private static void EnsureCacheDirectoryExists()
    {
        var cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CacheDirectory);
        if (Directory.Exists(cacheDir))
        {
            return;
        }
        else
        {
            IoOperations.CreateDirectory(cacheDir);
        }
    }

    /// <summary>
    /// Loads system files, either from cache or by rescanning.
    /// </summary>
    public async Task<List<string>> LoadSystemFilesAsync(string systemName, string systemFolderPath, List<string> fileExtensions, int gameCount)
    {
        if (systemFolderPath == null || fileExtensions == null || gameCount == 0)
        {
            return new List<string>();
        }

        var cacheFilePath = GetCacheFilePath(systemName);

        if (!File.Exists(cacheFilePath))
        {
            return await RebuildCache(systemName, systemFolderPath, fileExtensions);
        }

        var cachedData = await LoadCacheFromDisk(cacheFilePath);

        if (cachedData.FileCount != gameCount)
            return await RebuildCache(systemName, systemFolderPath, fileExtensions);

        lock (_cacheLock)
        {
            _cachedGameFiles[systemName] = cachedData.FileNames;
        }

        return cachedData.FileNames;
    }

    /// <summary>
    /// Rebuilds the cache by scanning the system folder.
    /// </summary>
    private async Task<List<string>> RebuildCache(string systemName, string systemFolderPath, List<string> fileExtensions)
    {
        if (systemFolderPath == null || fileExtensions == null)
        {
            return new List<string>();
        }

        var files = await Task.Run(() =>
        {
            var fileList = new List<string>();
            try
            {
                foreach (var extension in fileExtensions)
                {
                    var searchPattern = extension.StartsWith('*') ? extension : "*." + extension;
                    var foundFiles = Directory.EnumerateFiles(systemFolderPath, searchPattern, SearchOption.TopDirectoryOnly);
                    fileList.AddRange(foundFiles);
                }
            }
            catch (Exception ex)
            {
                var contextMessage = $"Error caching files for {systemName}.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
            }

            return fileList;
        });

        if (files == null)
        {
            return new List<string>();
        }

        lock (_cacheLock)
        {
            _cachedGameFiles[systemName] = files;
        }

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
        lock (_cacheLock)
        {
            return _cachedGameFiles.TryGetValue(systemName, out var files) ? files : new List<string>();
        }
    }
}