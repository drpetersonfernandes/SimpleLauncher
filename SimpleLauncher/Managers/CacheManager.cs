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
            try
            {
                Directory.CreateDirectory(cacheDir);
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Error creating cache directory.");
            }
        }
    }

    /// <summary>
    /// Loads system files, either from cache or by rescanning.
    /// Cache validation is based on the system folder's last modification time.
    /// </summary>
    /// <param name="systemName">The name of the system.</param>
    /// <param name="resolvedSystemFolderPath">The fully resolved path to the system's game folder.</param>
    /// <param name="fileExtensions">A list of file extensions.</param>
    /// <returns>A list of file paths.</returns>
    public async Task<List<string>> LoadSystemFilesAsync(string systemName, string resolvedSystemFolderPath, List<string> fileExtensions)
    {
        if (string.IsNullOrEmpty(resolvedSystemFolderPath) || !Directory.Exists(resolvedSystemFolderPath) || fileExtensions == null)
        {
            // Log if path was provided but invalid
            if (!string.IsNullOrEmpty(resolvedSystemFolderPath))
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(null, $"CacheManager.LoadSystemFilesAsync: Invalid or non-existent systemFolderPath '{resolvedSystemFolderPath}' for system '{systemName}'.");
            }

            return new List<string>(); // Return an empty list
        }

        var cacheFilePath = GetCacheFilePath(systemName);
        var currentFolderLastWriteTimeUtc = Directory.GetLastWriteTimeUtc(resolvedSystemFolderPath);

        if (File.Exists(cacheFilePath))
        {
            try
            {
                var cachedData = await LoadCacheFromDisk(cacheFilePath);

                // Validate cache based on folder modification time
                if (cachedData != null && cachedData.FolderLastWriteTimeUtc == currentFolderLastWriteTimeUtc)
                {
                    // Cache is valid, use it
                    lock (_cacheLock)
                    {
                        _cachedGameFiles[systemName] = cachedData.FileNames ?? new List<string>();
                    }

                    DebugLogger.Log($"Cache hit for system '{systemName}'. Folder timestamp matched.");
                    return cachedData.FileNames ?? new List<string>();
                }
                else
                {
                    DebugLogger.Log($"Cache miss for system '{systemName}'. Timestamp mismatch or invalid cache data. Expected: {currentFolderLastWriteTimeUtc}, Cached: {cachedData?.FolderLastWriteTimeUtc}. Rebuilding cache.");
                }
            }
            catch (Exception ex)
            {
                // Error reading cache file, treat as cache miss
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, $"Error reading cache file '{cacheFilePath}' for system '{systemName}'. Rebuilding cache.");
            }
        }
        else
        {
            DebugLogger.Log($"Cache file not found for system '{systemName}' at '{cacheFilePath}'. Rebuilding cache.");
        }

        // Cache doesn't exist, is invalid, or error reading it; rebuild it
        return await RebuildCache(systemName, resolvedSystemFolderPath, fileExtensions, currentFolderLastWriteTimeUtc);
    }

    /// <summary>
    /// Rebuilds the cache by scanning the system folder.
    /// </summary>
    /// <param name="systemName">The name of the system.</param>
    /// <param name="resolvedSystemFolderPath">The fully resolved path to the system's game folder.</param>
    /// <param name="fileExtensions">A list of file extensions (e.g., "txt", "jpg").</param>
    /// <param name="folderLastWriteTimeUtc">The last write time of the folder, used for saving with the new cache.</param>
    /// <returns>A list of file paths found.</returns>
    private async Task<List<string>> RebuildCache(string systemName, string resolvedSystemFolderPath, List<string> fileExtensions, DateTime folderLastWriteTimeUtc)
    {
        if (string.IsNullOrEmpty(resolvedSystemFolderPath) || !Directory.Exists(resolvedSystemFolderPath) || fileExtensions == null)
        {
            return new List<string>(); // Empty list
        }

        DebugLogger.Log($"Rebuilding cache for system '{systemName}' from path '{resolvedSystemFolderPath}'.");

        var files = await GetListOfFiles.GetFilesAsync(resolvedSystemFolderPath, fileExtensions);

        lock (_cacheLock)
        {
            _cachedGameFiles[systemName] = files;
        }

        await SaveCacheToDisk(systemName, files, folderLastWriteTimeUtc);
        DebugLogger.Log($"Cache rebuilt and saved for system '{systemName}'. File count: {files.Count}, Timestamp: {folderLastWriteTimeUtc}.");

        return files;
    }

    /// <summary>
    /// Saves the cache to a binary file inside the Cache folder using MessagePack.
    /// </summary>
    private static async Task SaveCacheToDisk(string systemName, List<string> fileNames, DateTime folderLastWriteTimeUtc)
    {
        try
        {
            var cacheFilePath = GetCacheFilePath(systemName);
            var cacheData = new GameCache
            {
                FileCount = fileNames.Count,
                FileNames = fileNames,
                FolderLastWriteTimeUtc = folderLastWriteTimeUtc
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
            if (!File.Exists(cacheFilePath))
            {
                DebugLogger.Log($"Cache file '{cacheFilePath}' not found during LoadCacheFromDisk.");
                return null;
            }

            var binaryData = await File.ReadAllBytesAsync(cacheFilePath);
            return MessagePackSerializer.Deserialize<GameCache>(binaryData);
        }
        catch (MessagePackSerializationException mpex)
        {
            // Deserialization issues, often due to model mismatch or corruption
            // Notify developer
            _ = LogErrors.LogErrorAsync(mpex, $"Error deserializing cache file {cacheFilePath}. It might be corrupted or from an older version.");

            DeleteCorruptedCacheFile(cacheFilePath); // Delete corrupted cache

            return null; // Return null to indicate failure
        }
        catch (Exception ex)
        {
            // Notify developer
            var errorMessage = $"Error loading cache file {cacheFilePath}.";
            _ = LogErrors.LogErrorAsync(ex, errorMessage);

            DeleteCorruptedCacheFile(cacheFilePath); // Delete corrupted cache

            return null; // Return null to indicate failure
        }
    }

    private static void DeleteCorruptedCacheFile(string cacheFilePath)
    {
        try
        {
            if (!File.Exists(cacheFilePath)) return;

            File.Delete(cacheFilePath);
            DebugLogger.Log($"Deleted corrupted cache file: {cacheFilePath}");
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, $"Failed to delete corrupted cache file: {cacheFilePath}");
        }
    }

    /// <summary>
    /// Returns the cache file path inside the Cache folder for a given system.
    /// </summary>
    private static string GetCacheFilePath(string systemName)
    {
        // Sanitize systemName to prevent path traversal or invalid characters in filename
        var sanitizedSystemName = SanitizeInputSystemName.SanitizeFolderName(systemName);
        if (!string.IsNullOrWhiteSpace(sanitizedSystemName) && sanitizedSystemName.Length <= 100) // Basic sanity check
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CacheDirectory, $"{sanitizedSystemName}.cache");

        sanitizedSystemName = Guid.NewGuid().ToString("N"); // Fallback to a GUID if sanitization fails badly

        // Notify developer
        _ = LogErrors.LogErrorAsync(null, $"Invalid system name '{systemName}' resulted in problematic sanitized name for cache file. Used GUID fallback.");

        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CacheDirectory, $"{sanitizedSystemName}.cache");
    }

    /// <summary>
    /// Returns the cached files for the given system from the in-memory dictionary.
    /// This does not load from disk; it's for accessing already loaded/validated cache.
    /// </summary>
    public List<string> GetCachedFiles(string systemName)
    {
        lock (_cacheLock)
        {
            return _cachedGameFiles.TryGetValue(systemName, out var files) ? new List<string>(files) : new List<string>();
        }
    }
}