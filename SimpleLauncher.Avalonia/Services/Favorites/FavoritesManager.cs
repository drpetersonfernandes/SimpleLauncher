using System.Collections.ObjectModel;
using MessagePack;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services;
using ILogger = Serilog.ILogger;

namespace SimpleLauncher.Avalonia.Services.Favorites;

/// <summary>
/// Manages the user's favorite games list with MessagePack serialization.
/// Compatible with the existing favorites.dat format from SimpleLauncher.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
public class FavoritesManager
{
    [IgnoreMember] private static readonly Lock ListLock = new();
    [IgnoreMember] private ILogger? _logger;
    [IgnoreMember] private static readonly DataFileLocation FileLocation = new("favorites.dat");

    [Key(0)]
    public ObservableCollection<Favorite> FavoriteList { get; set; } = [];

    [Key(1)]
    public int Version { get; set; } = 1;

    private static string DatFilePath => FileLocation.FilePath;
    private static string TempDatFilePath => FileLocation.TempFilePath;
    public static bool IsPortableMode => FileLocation.IsPortableMode;

    /// <summary>
    /// Loads favorites from the DAT file, or creates a new instance if none exists.
    /// </summary>
    public static FavoritesManager LoadFavorites(ILogger? logErrors = null)
    {
        if (File.Exists(DatFilePath))
        {
            try
            {
                var bytes = File.ReadAllBytes(DatFilePath);
                var manager = MessagePackSerializer.Deserialize<FavoritesManager>(bytes);
                manager._logger = logErrors;
                return manager;
            }
            catch (Exception ex)
            {
                logErrors?.Error(ex, "Error loading favorites.dat");
            }
        }

        var newManager = new FavoritesManager { _logger = logErrors };
        // Write the initial file synchronously. This runs on the UI thread at startup —
        // awaiting the async save via GetAwaiter().GetResult() would deadlock (the async
        // continuation needs the UI thread that is blocked waiting).
        newManager.SaveFavoritesSync();
        return newManager;
    }

    /// <summary>
    /// Synchronous initial save (startup path only). Mirrors <see cref="SaveFavoritesAsync"/>
    /// with retry logic, but never awaits — safe to call on the UI thread.
    /// </summary>
    private void SaveFavoritesSync()
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                var bytes = MessagePackSerializer.Serialize(this);
                File.WriteAllBytes(TempDatFilePath, bytes);
                File.Move(TempDatFilePath, DatFilePath, true);
                return;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error saving favorites.dat (attempt {Attempt})", attempt + 1);
                if (attempt < 2) Thread.Sleep(100);
            }
        }
    }

    /// <summary>
    /// Saves favorites atomically to the DAT file with retry logic.
    /// </summary>
    public async Task SaveFavoritesAsync()
    {
        for (var attempt = 0; attempt < 3; attempt++)
        {
            try
            {
                var bytes = MessagePackSerializer.Serialize(this);
                await File.WriteAllBytesAsync(TempDatFilePath, bytes);
                File.Move(TempDatFilePath, DatFilePath, true);
                return;
            }
            catch (Exception ex)
            {
                _logger?.Error(ex, "Error saving favorites.dat (attempt {Attempt})", attempt + 1);
                if (attempt < 2) await Task.Delay(100);
            }
        }
    }

    /// <summary>
    /// Checks whether a game is in the favorites list.
    /// </summary>
    public bool IsFavorite(string filePath)
    {
        lock (ListLock)
        {
            return FavoriteList.Any(f =>
                string.Equals(f.FileName, filePath, StringComparison.OrdinalIgnoreCase));
        }
    }

    /// <summary>
    /// Adds a game to favorites. Returns true if added, false if already present.
    /// </summary>
    public async Task<bool> AddFavoriteAsync(string filePath, string systemName)
    {
        lock (ListLock)
        {
            if (FavoriteList.Any(f =>
                    string.Equals(f.FileName, filePath, StringComparison.OrdinalIgnoreCase)))
                return false;

            FavoriteList.Add(new Favorite
            {
                FileName = filePath,
                SystemName = systemName
            });
        }

        await SaveFavoritesAsync();
        return true;
    }

    /// <summary>
    /// Removes a game from favorites. Returns true if removed, false if not found.
    /// </summary>
    public async Task<bool> RemoveFavoriteAsync(string filePath)
    {
        lock (ListLock)
        {
            var toRemove = FavoriteList.FirstOrDefault(f =>
                string.Equals(f.FileName, filePath, StringComparison.OrdinalIgnoreCase));
            if (toRemove is null) return false;

            FavoriteList.Remove(toRemove);
        }

        await SaveFavoritesAsync();
        return true;
    }

    /// <summary>
    /// Toggles favorite status for a game. Returns the new state (true = favorited).
    /// </summary>
    public async Task<bool> ToggleAsync(string filePath, string systemName)
    {
        if (IsFavorite(filePath))
        {
            await RemoveFavoriteAsync(filePath);
            return false;
        }

        await AddFavoriteAsync(filePath, systemName);
        return true;
    }

    /// <summary>
    /// Gets all favorite file paths.
    /// </summary>
    public HashSet<string> GetFavoritePaths()
    {
        lock (ListLock)
        {
            return FavoriteList.Select(f => f.FileName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }
    }
}
