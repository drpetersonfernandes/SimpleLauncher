using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Linq;
using MessagePack;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher.Managers;

[MessagePackObject]
public class FavoritesManager
{
    // This collection will be serialized with MessagePack
    [Key(0)]
    public ObservableCollection<Favorite> FavoriteList { get; set; } = [];

    private static string DatFilePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favorites.dat");

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

                // Notify user
                Application.Current.Dispatcher.Invoke(static () => UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingFavorites") ?? "Loading favorites...", Application.Current.MainWindow as MainWindow));

                return MessagePackSerializer.Deserialize<FavoritesManager>(bytes);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error loading favorites.dat";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
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
        // Order the favorites by FileName
        var orderedFavorites = FavoriteList
            .OrderBy(static fav => fav.FileName, StringComparer.OrdinalIgnoreCase)
            .ToList();

        FavoriteList.Clear();
        foreach (var fav in orderedFavorites)
        {
            FavoriteList.Add(fav);
        }

        try
        {
            // Notify user
            Application.Current.Dispatcher.Invoke(static () => UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("SavingFavorites") ?? "Saving favorites...", Application.Current.MainWindow as MainWindow));

            // Serialize using MessagePack
            var bytes = MessagePackSerializer.Serialize(this);
            File.WriteAllBytes(DatFilePath, bytes);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error saving favorites.dat";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            throw; // Re-throw to notify caller
        }
    }
}