using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using MessagePack;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.AppDataFile;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.Favorites;

[MessagePackObject(AllowPrivate = true)]
public class FavoritesManager
{
    [IgnoreMember] private static readonly object ListLock = new();
    [IgnoreMember] private ILogErrors _logErrors;
    [IgnoreMember] private static readonly DataFileLocation FileLocation = new("favorites.dat");

    // This collection will be serialized with MessagePack
    [Key(0)]
    public ObservableCollection<Favorite> FavoriteList { get; set; } = [];

    [Key(1)] public int Version { get; set; } = 1;

    private static string DatFilePath => FileLocation.FilePath;
    private static string TempDatFilePath => FileLocation.TempFilePath;
    public static bool IsPortableMode => FileLocation.IsPortableMode;

    /// <summary>
    /// Loads favorites from the DAT file. If the DAT file doesn't exist, will create a new instance.
    /// </summary>
    public static FavoritesManager LoadFavorites(ILogErrors logErrors = null)
    {
        if (File.Exists(DatFilePath))
        {
            try
            {
                var bytes = File.ReadAllBytes(DatFilePath);
                var manager = MessagePackSerializer.Deserialize<FavoritesManager>(bytes);
                manager._logErrors = logErrors;
                return manager;
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error loading favorites.dat";
                logErrors?.LogAndForget(ex, contextMessage);
            }
        }

        // If no files exist, create a new instance
        var defaultManager = new FavoritesManager { _logErrors = logErrors };
        defaultManager.SaveFavoritesAsync();
        return defaultManager; // Return default instance if error occurs
    }

    /// <summary>
    /// Saves the provided favorites to the DAT file.
    /// The favorites are ordered by FileName before saving.
    /// </summary>
    public Task SaveFavoritesAsync()
    {
        // Notify user outside of any lock to prevent potential deadlock
        Application.Current.Dispatcher.Invoke(static () => (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("SavingFavorites") ?? "Saving favorites..."));

        // Snapshot the ordered list inside the lock, then clear/repopulate outside
        // to avoid ObservableCollection firing CollectionChanged while holding the lock.
        List<Favorite> orderedFavorites;
        lock (ListLock)
        {
            orderedFavorites = FavoriteList
                .OrderBy(static fav => fav.FileName, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        // Clear and repopulate outside the lock so CollectionChanged handlers
        // (which may touch the UI or even re-enter ListLock) cannot deadlock.
        FavoriteList.Clear();
        foreach (var fav in orderedFavorites)
        {
            FavoriteList.Add(fav);
        }

        // Serialize and write on a background thread so Thread.Sleep in the
        // retry loop does not block the UI thread.
        return Task.Run(() =>
        {
            const int maxRetries = 3;
            var retryDelayMs = 500;
            Exception lastException = null;
            var attempt = 0;

            while (attempt < maxRetries)
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
                    attempt++;

                    // If in portable mode, try falling back to LocalAppData and reset retries
                    if (FileLocation.IsPortableMode && attempt >= maxRetries)
                    {
                        try
                        {
                            if (FileLocation.TryFallbackToLocalAppData())
                            {
                                attempt = 0;
                                continue;
                            }
                        }
                        catch
                        {
                            // Fallback failed, continue with normal error handling
                        }
                    }

                    if (attempt < maxRetries)
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
            _logErrors?.LogAndForget(lastException, "Error saving favorites.dat");

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
                _logErrors?.LogAndForget(cleanupEx, "Error cleaning up temporary favorites file after failed save");
            }
        });
    }
}