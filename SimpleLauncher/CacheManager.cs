using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MessagePack;

namespace SimpleLauncher
{
    public class CacheManager
    {
        private const string CacheDirectory = "cache";
        private readonly Dictionary<string, List<string>> _cachedGameFiles = new();

        public CacheManager()
        {
            EnsureCacheDirectoryExists();
        }

        private static async void EnsureCacheDirectoryExists()
        {
            try
            {
                string cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CacheDirectory);
                if (!Directory.Exists(cacheDir))
                {
                    try
                    {
                        Directory.CreateDirectory(cacheDir);
                    }
                    catch (Exception ex)
                    {
                        // Notify developer
                        string errorMessage = $"Error creating cache directory.\n\n" +
                                              $"Exception type: {ex.GetType().Name}\n" +
                                              $"Exception details: {ex.Message}";
                        await LogErrors.LogErrorAsync(ex, errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                string errorMessage = $"Error creating cache directory.\n\n" +
                                      $"Exception type: {ex.GetType().Name}\n" +
                                      $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, errorMessage);
            }
        }

        /// <summary>
        /// Loads system files, either from cache or by rescanning.
        /// </summary>
        public async Task<List<string>> LoadSystemFilesAsync(string systemName, string systemFolderPath, List<string> fileExtensions, int gameCount)
        {
            string cacheFilePath = GetCacheFilePath(systemName);

            // Check if cache exists
            if (File.Exists(cacheFilePath))
            {
                var cachedData = await LoadCacheFromDisk(cacheFilePath);

                // Compare file count (using `gameCount` from MainWindow)
                if (cachedData.FileCount == gameCount)
                {
                    _cachedGameFiles[systemName] = cachedData.FileNames;
                    return cachedData.FileNames;
                }
            }

            // If cache doesn't exist or count differs, rebuild cache
            return await RebuildCache(systemName, systemFolderPath, fileExtensions);
        }

        /// <summary>
        /// Rebuilds the cache by scanning the system folder.
        /// </summary>
        private async Task<List<string>> RebuildCache(string systemName, string systemFolderPath, List<string> fileExtensions)
        {
            var files = await Task.Run(async () =>
            {
                var fileList = new List<string>();
                try
                {
                    foreach (var extension in fileExtensions)
                    {
                        fileList.AddRange(Directory.EnumerateFiles(systemFolderPath, extension, SearchOption.TopDirectoryOnly));
                    }
                }
                catch (Exception ex)
                {
                    // Notify developer
                    string errorMessage = $"Error caching files for {systemName}.\n\n" +
                                          $"Exception type: {ex.GetType().Name}\n" +
                                          $"Exception details: {ex.Message}";
                    await LogErrors.LogErrorAsync(ex, errorMessage);
                }
                return fileList;
            });

            if (files != null)
            {
                _cachedGameFiles[systemName] = files;
                await SaveCacheToDisk(systemName, files);
            }

            return files;
        }

        /// <summary>
        /// Saves the cache to a binary file inside the Cache folder.
        /// </summary>
        private async Task SaveCacheToDisk(string systemName, List<string> fileNames)
        {
            try
            {
                string cacheFilePath = GetCacheFilePath(systemName);
                var cacheData = new GameCache
                {
                    FileCount = fileNames.Count,
                    FileNames = fileNames
                };

                byte[] binaryData = MessagePackSerializer.Serialize(cacheData);
                await File.WriteAllBytesAsync(cacheFilePath, binaryData);
            }
            catch (Exception ex)
            {
                // Notify developer
                string errorMessage = $"Error saving cache for {systemName}.\n\n" +
                                      $"Exception type: {ex.GetType().Name}\n" +
                                      $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, errorMessage);
            }
        }

        /// <summary>
        /// Loads cache from a binary file inside the Cache folder.
        /// </summary>
        private async Task<GameCache> LoadCacheFromDisk(string cacheFilePath)
        {
            try
            {
                byte[] binaryData = await File.ReadAllBytesAsync(cacheFilePath);
                return MessagePackSerializer.Deserialize<GameCache>(binaryData);
            }
            catch (Exception ex)
            {
                // Notify developer
                string errorMessage = $"Error loading cache file {cacheFilePath}.\n\n" +
                                      $"Exception type: {ex.GetType().Name}\n" +
                                      $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, errorMessage);
                
                // Return empty cache
                return new GameCache { FileCount = 0, FileNames = new List<string>() };
            }
        }

        /// <summary>
        /// Returns the cache file path inside the Cache folder for a given system.
        /// </summary>
        private string GetCacheFilePath(string systemName)
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, CacheDirectory, $"{systemName}.cache");
        }
        
        /// <summary>
        /// Returns the cached files for the given system.
        /// </summary>
        public List<string> GetCachedFiles(string systemName)
        {
            if (_cachedGameFiles.TryGetValue(systemName, out var files))
                return files;
            return new List<string>();
        }

    }

    [MessagePackObject]
    public class GameCache
    {
        [Key(0)] public int FileCount { get; set; }
        [Key(1)] public List<string> FileNames { get; set; }
    }
}
