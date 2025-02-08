using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MessagePack;

namespace SimpleLauncher
{
    public class CacheManager
    {
        private const string CacheDirectory = "cache";
        private Dictionary<string, List<string>> _cachedGameFiles = new();
        private CancellationTokenSource _cancellationTokenSource;

        public CacheManager()
        {
            EnsureCacheDirectoryExists();
        }

        private async void EnsureCacheDirectoryExists()
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
                        string errorMessage = $"Error.\n\n" +
                                              $"Exception type: {ex.GetType().Name}\n" +
                                              $"Exception details: {ex.Message}";
                        await LogErrors.LogErrorAsync(ex, errorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"Error.\n\n" +
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
            await _cancellationTokenSource?.CancelAsync()!;
            _cancellationTokenSource = new CancellationTokenSource();
            var cancellationToken = _cancellationTokenSource.Token;

            var files = await Task.Run(() =>
            {
                var fileList = new List<string>();
                try
                {
                    foreach (var extension in fileExtensions)
                    {
                        if (cancellationToken.IsCancellationRequested) return null;
                        fileList.AddRange(Directory.EnumerateFiles(systemFolderPath, extension, SearchOption.TopDirectoryOnly));
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($@"Error caching files for {systemName}: {ex.Message}");
                }
                return fileList;
            }, cancellationToken);

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
                Console.WriteLine($@"Error saving cache for {systemName}: {ex.Message}");
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
                Console.WriteLine($@"Error loading cache file {cacheFilePath}: {ex.Message}");
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
    }

    [MessagePackObject]
    public class GameCache
    {
        [Key(0)] public int FileCount { get; set; }
        [Key(1)] public List<string> FileNames { get; set; }
    }
}
