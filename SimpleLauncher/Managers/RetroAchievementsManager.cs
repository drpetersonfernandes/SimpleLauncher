using System;
using System.Collections.Generic;
using System.IO;
using MessagePack;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher.Managers;

[MessagePackObject]
public class RetroAchievementsManager
{
    [Key(0)]
    public List<RaGameInfo> AllGames { get; set; } = [];

    private Dictionary<string, RaGameInfo> _hashToGameInfoLookup;
    private static string DatFilePath { get; } = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RetroAchievements.dat");

    public static RetroAchievementsManager LoadRetroAchievement()
    {
        var manager = new RetroAchievementsManager();

        if (File.Exists(DatFilePath))
        {
            try
            {
                // Notify user
                Application.Current.Dispatcher.Invoke(static () => UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingRetroAchievementsDatabase") ?? "Loading RetroAchievements database...", Application.Current.MainWindow as MainWindow));

                var bytes = File.ReadAllBytes(DatFilePath);
                if (bytes.Length > 0)
                {
                    // The root object in the .dat file is a List<RaGameInfo>,
                    // so we deserialize that directly and wrap it in our manager.
                    manager.AllGames = MessagePackSerializer.Deserialize<List<RaGameInfo>>(bytes);
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error loading RetroAchievements.dat. The file might be corrupted or invalid. A new empty file will be created.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                DebugLogger.Log($"[RA Manager] Failed to load RetroAchievements.dat: {ex.Message}");
            }
        }

        // Populate the hash lookup dictionary after loading AllGames
        manager.PopulateHashLookup();

        // If the file doesn't exist, is empty, or fails to load, log it for debugging
        if (manager.AllGames.Count == 0)
        {
            // Notify developer
            const string contextMessage = "RetroAchievements.dat is missing or empty. Starting with an empty database.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

            DebugLogger.Log("[RA Manager] Starting with empty RetroAchievements database");
        }

        return manager;
    }

    /// <summary>
    /// Populates the internal dictionary for fast hash-to-gameinfo lookups.
    /// </summary>
    private void PopulateHashLookup()
    {
        _hashToGameInfoLookup = new Dictionary<string, RaGameInfo>(StringComparer.OrdinalIgnoreCase);
        foreach (var game in AllGames)
        {
            foreach (var hash in game.Hashes)
            {
                // Add the hash to the dictionary. If a hash maps to multiple games,
                // we'll just take the first one encountered. This is a simplification.
                // RetroAchievements API usually handles this by returning the primary game.
                _hashToGameInfoLookup.TryAdd(hash, game);
            }
        }

        DebugLogger.Log($"[RA Manager] Populated hash lookup with {_hashToGameInfoLookup.Count} entries.");
    }

    /// <summary>
    /// Retrieves RaGameInfo by a given hash from the in-memory lookup.
    /// </summary>
    /// <param name="hash">The hash to look up.</param>
    /// <returns>The matching RaGameInfo, or null if not found.</returns>
    public RaGameInfo GetGameInfoByHash(string hash)
    {
        if (string.IsNullOrEmpty(hash))
        {
            return null;
        }

        _hashToGameInfoLookup ??= new Dictionary<string, RaGameInfo>(StringComparer.OrdinalIgnoreCase); // Ensure initialized
        _hashToGameInfoLookup.TryGetValue(hash, out var gameInfo);
        return gameInfo;
    }
}