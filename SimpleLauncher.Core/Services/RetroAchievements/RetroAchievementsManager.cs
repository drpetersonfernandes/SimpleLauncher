using System.IO;
using MessagePack;
using SimpleLauncher.Core.Services.AppDataFile;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.RetroAchievements.Models;

namespace SimpleLauncher.Core.Services.RetroAchievements;

[MessagePackObject]
public class RetroAchievementsManager
{
    [Key(0)]
    public List<RaGameInfo> AllGames { get; set; } = [];

    private Dictionary<string, RaGameInfo> _hashToGameInfoLookup;
    private IDebugLogger _debugLogger;
    private static readonly DataFileLocation FileLocation = new("RetroAchievements.dat");
    private static string DatFilePath => FileLocation.FilePath;

    public static RetroAchievementsManager LoadRetroAchievement(ILogErrors logErrors, IDebugLogger debugLogger)
    {
        var manager = new RetroAchievementsManager { _debugLogger = debugLogger };
        if (File.Exists(DatFilePath))
        {
            try
            {
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
                logErrors.LogAndForget(ex, contextMessage);

                debugLogger.Log($"[RA Manager] Failed to load RetroAchievements.dat: {ex.Message}");
            }
        }

        // Populate the hash lookup dictionary after loading AllGames
        manager.PopulateHashLookup();

        // If the file doesn't exist, is empty, or fails to load, log it for debugging
        if (manager.AllGames.Count == 0)
        {
            // Notify developer
            const string contextMessage = "RetroAchievements.dat is missing or empty. Starting with an empty database.";
            logErrors.LogAndForget(null, contextMessage);

            debugLogger.Log("[RA Manager] Starting with empty RetroAchievements database");
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

        _debugLogger.Log($"[RA Manager] Populated hash lookup with {_hashToGameInfoLookup.Count} entries.");
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