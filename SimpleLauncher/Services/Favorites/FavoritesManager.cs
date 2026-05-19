using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using MessagePack;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Models;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.Favorites;

[MessagePackObject]
public class FavoritesManager
{
    [IgnoreMember] private static readonly object ListLock = new();

    // Portable mode support for favorites.dat
    [IgnoreMember] private static string _datFilePath;
    [IgnoreMember] private static string _tempDatFilePath;
    [IgnoreMember] private static bool _isPortableMode;
    [IgnoreMember] private static bool _pathInitialized;

    // This collection will be serialized with MessagePack
    [Key(0)]
    public ObservableCollection<Favorite> FavoriteList { get; set; } = [];

    [Key(1)] public int Version { get; set; } = 1;

    private static string DatFilePath => GetDatFilePath();
    private static string TempDatFilePath => GetTempDatFilePath();

    /// <summary>
    /// Gets the path to favorites.dat, determining the location based on portable mode availability.
    /// </summary>
    private static string GetDatFilePath()
    {
        if (!_pathInitialized)
        {
            InitializeFavoritesPath();
        }

        return _datFilePath;
    }

    /// <summary>
    /// Gets the path to the temporary favorites file.
    /// </summary>
    private static string GetTempDatFilePath()
    {
        if (!_pathInitialized)
        {
            InitializeFavoritesPath();
        }

        return _tempDatFilePath;
    }

    /// <summary>
    /// Initializes the favorites.dat file path, preferring portable mode but falling back to LocalAppData if necessary.
    /// </summary>
    private static void InitializeFavoritesPath()
    {
        var portablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favorites.dat");
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var appDataFolder = Path.Combine(localAppData, "SimpleLauncher");
        var localAppDataPath = Path.Combine(appDataFolder, "favorites.dat");

        // Check if favorites already exist in either location
        var portableFavoritesExist = File.Exists(portablePath);
        var localAppDataFavoritesExist = File.Exists(localAppDataPath);

        switch (portableFavoritesExist)
        {
            case true when !localAppDataFavoritesExist:
                // Existing portable installation - keep using portable mode
                _datFilePath = portablePath;
                _tempDatFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favorites.dat.tmp");
                _isPortableMode = true;
                break;
            case false when localAppDataFavoritesExist:
                // Favorites were previously moved to LocalAppData - continue using it
                _datFilePath = localAppDataPath;
                _tempDatFilePath = Path.Combine(appDataFolder, "favorites.dat.tmp");
                _isPortableMode = false;
                break;
            case true when localAppDataFavoritesExist:
            {
                // Both exist - use the more recently modified one
                var portableInfo = new FileInfo(portablePath);
                var localInfo = new FileInfo(localAppDataPath);
                if (portableInfo.LastWriteTimeUtc > localInfo.LastWriteTimeUtc)
                {
                    _datFilePath = portablePath;
                    _tempDatFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favorites.dat.tmp");
                    _isPortableMode = true;
                }
                else
                {
                    _datFilePath = localAppDataPath;
                    _tempDatFilePath = Path.Combine(appDataFolder, "favorites.dat.tmp");
                    _isPortableMode = false;
                }

                break;
            }
            default:
            {
                // No existing favorites - try portable mode first
                if (IsDirectoryWritable(AppDomain.CurrentDomain.BaseDirectory))
                {
                    _datFilePath = portablePath;
                    _tempDatFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favorites.dat.tmp");
                    _isPortableMode = true;
                }
                else
                {
                    // Application directory is not writable - fallback to LocalAppData
                    try
                    {
                        if (!Directory.Exists(appDataFolder))
                        {
                            Directory.CreateDirectory(appDataFolder);
                        }
                    }
                    catch
                    {
                        // If we can't create the directory, we'll still use the path
                    }

                    _datFilePath = localAppDataPath;
                    _tempDatFilePath = Path.Combine(appDataFolder, "favorites.dat.tmp");
                    _isPortableMode = false;
                }

                break;
            }
        }

        _pathInitialized = true;
    }

    /// <summary>
    /// Checks if a directory is writable by attempting to create and delete a temporary file.
    /// </summary>
    /// <param name="directoryPath">The directory to test.</param>
    /// <returns>True if the directory is writable; otherwise, false.</returns>
    private static bool IsDirectoryWritable(string directoryPath)
    {
        try
        {
            if (!Directory.Exists(directoryPath))
            {
                return false;
            }

            var testFilePath = Path.Combine(directoryPath, $".write_test_{Guid.NewGuid()}.tmp");
            File.WriteAllText(testFilePath, "test");
            File.Delete(testFilePath);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Attempts to fallback from portable mode to LocalAppData when save fails.
    /// </summary>
    private static void TryFallbackToLocalAppData()
    {
        try
        {
            var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var appDataFolder = Path.Combine(localAppData, "SimpleLauncher");
            var newFilePath = Path.Combine(appDataFolder, "favorites.dat");

            // Ensure the directory exists
            if (!Directory.Exists(appDataFolder))
            {
                Directory.CreateDirectory(appDataFolder);
            }

            _datFilePath = newFilePath;
            _tempDatFilePath = Path.Combine(appDataFolder, "favorites.dat.tmp");
            _isPortableMode = false;
        }
        catch
        {
            // If fallback fails, we'll just keep the original path
        }
    }

    /// <summary>
    /// Gets whether the application is running in portable mode for favorites.dat.
    /// </summary>
    public static bool IsPortableMode
    {
        get
        {
            if (!_pathInitialized)
            {
                // Initialize with default configuration if not already done
                // This shouldn't happen in normal operation
                return true; // Assume portable by default
            }

            return _isPortableMode;
        }
    }

    /// <summary>
    /// Loads favorites from the DAT file. If the DAT file doesn't exist, will create a new instance.
    /// </summary>
    public static FavoritesManager LoadFavorites()
    {
        if (File.Exists(DatFilePath))
        {
            try
            {
                var bytes = File.ReadAllBytes(DatFilePath);

                return MessagePackSerializer.Deserialize<FavoritesManager>(bytes);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error loading favorites.dat";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
            }
        }

        // If no files exist, create a new instance
        var defaultManager = new FavoritesManager();
        defaultManager.SaveFavorites();
        return defaultManager; // Return default instance if error occurs
    }

    /// <summary>
    /// Saves the provided favorites to the DAT file.
    /// The favorites are ordered by FileName before saving.
    /// </summary>
    public void SaveFavorites()
    {
        // Notify user outside of any lock to prevent potential deadlock
        Application.Current.Dispatcher.Invoke(static () => UpdateStatusBar.UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("SavingFavorites") ?? "Saving favorites...", Application.Current.MainWindow as MainWindow));

        lock (ListLock)
        {
            // Order the favorites by FileName
            var orderedFavorites = FavoriteList
                .OrderBy(static fav => fav.FileName, StringComparer.OrdinalIgnoreCase)
                .ToList();

            // Clear and repopulate the existing list to maintain UI bindings
            FavoriteList.Clear();
            foreach (var fav in orderedFavorites)
            {
                FavoriteList.Add(fav);
            }
        }

        // Now serialize
        const int maxRetries = 3;
        var retryDelayMs = 500;
        Exception lastException = null;

        for (var attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                // Serialize using MessagePack
                byte[] bytes;
                lock (ListLock)
                {
                    bytes = MessagePackSerializer.Serialize(this);
                }

                // Write to temporary file first to prevent corruption on crash
                File.WriteAllBytes(TempDatFilePath, bytes);

                // Atomically replace the main file with the temp file
                File.Move(TempDatFilePath, DatFilePath, true);
                return; // Success
            }
            catch (Exception ex) when (ex is UnauthorizedAccessException or IOException)
            {
                lastException = ex;

                // If in portable mode and this is the last attempt, try falling back to LocalAppData
                if (_isPortableMode && attempt == maxRetries - 1)
                {
                    try
                    {
                        // Attempt fallback
                        TryFallbackToLocalAppData();
                        // Retry with new paths (don't count this as an attempt)
                        attempt--;
                        continue;
                    }
                    catch
                    {
                        // Fallback failed, continue with normal error handling
                    }
                }

                if (attempt < maxRetries - 1)
                {
                    // Attempt to clean up temp file before retrying
                    try
                    {
                        if (File.Exists(TempDatFilePath))
                        {
                            File.Delete(TempDatFilePath);
                        }
                    }
                    catch
                    {
                        // Ignore cleanup errors
                    }

                    Thread.Sleep(retryDelayMs);
                    retryDelayMs *= 2; // Exponential backoff
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                break; // Don't retry non-transient errors
            }
        }

        // All retries exhausted or non-transient error
        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(lastException, "Error saving favorites.dat");

        // Attempt to clean up temp file if it exists
        try
        {
            if (File.Exists(TempDatFilePath))
            {
                File.Delete(TempDatFilePath);
            }
        }
        catch (Exception cleanupEx)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(cleanupEx, "Error cleaning up temporary favorites file after failed save");
        }

        throw new InvalidOperationException("Failed to save favorites.", lastException);
    }
}