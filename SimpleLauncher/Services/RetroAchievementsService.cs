using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Models;

namespace SimpleLauncher.Services;

public static class RetroAchievementsService
{
    private const string ApiBaseUrl = "https://retroachievements.org/API/";
    private const string SiteBaseUrl = "https://retroachievements.org";
    private static readonly IHttpClientFactory HttpClientFactory = App.ServiceProvider.GetRequiredService<IHttpClientFactory>();

    // Cache for game lists per console to avoid repeated API calls
    private static readonly Dictionary<int, List<ApiGameInfo>> ConsoleGameListCache = new();
    private static readonly Dictionary<int, DateTime> CacheTimestamp = new();

    /// <summary>
    /// Fetches the user's progress and achievement list for a specific game.
    /// </summary>
    /// <param name="gameTitle">The title of the game (usually the filename without extension).</param>
    /// <param name="systemName">The name of the system/console.</param>
    /// <param name="username">The user's RetroAchievements username.</param>
    /// <param name="apiKey">The user's RetroAchievements Web API Key.</param>
    /// <returns>A tuple containing the user's game progress and a list of achievements, or null if an error occurs.</returns>
    public static async Task<(UserGameProgress Progress, List<Achievement> Achievements)> GetUserGameProgressAsync(string gameTitle, string systemName, string username, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey))
        {
            DebugLogger.Log("[RA Service] Username or API Key is missing.");
            return (null, null);
        }

        try
        {
            // Step 1: Find the Game ID for the given title and system.
            var gameId = await FindGameIdAsync(gameTitle, systemName, username, apiKey);
            if (gameId == null)
            {
                DebugLogger.Log($"[RA Service] Could not find a matching GameID for '{gameTitle}' on console '{systemName}'.");
                return (null, null);
            }

            DebugLogger.Log($"[RA Service] Found GameID {gameId} for '{gameTitle}'. Fetching user progress...");

            // Step 2: Fetch the game info and user progress using the found Game ID.
            var client = HttpClientFactory.CreateClient();
            var url = $"{ApiBaseUrl}API_GetGameInfoAndUserProgress.php?u={Uri.EscapeDataString(username)}&g={gameId}&y={Uri.EscapeDataString(apiKey)}";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _ = LogErrors.LogErrorAsync(null, $"[RA Service] API_GetGameInfoAndUserProgress failed with status {response.StatusCode}: {error}");
                return (null, null);
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<ApiGameProgressResponse>(json);

            if (apiResponse == null) return (null, null);

            // Step 3: Map the API response to our local models.
            var progress = new UserGameProgress
            {
                GameTitle = apiResponse.Title,
                GameIconUrl = $"{SiteBaseUrl}{apiResponse.ImageIcon}",
                ConsoleName = apiResponse.ConsoleName,
                AchievementsEarned = apiResponse.NumAwardedToUser,
                TotalAchievements = apiResponse.NumAchievements,
                PointsEarned = apiResponse.Achievements.Values.Where(static a => a.DateEarned != null).Sum(static a => a.Points),
                TotalPoints = apiResponse.Achievements.Values.Sum(static a => a.Points)
            };

            var achievements = apiResponse.Achievements.Values
                .OrderBy(static a => a.DisplayOrder)
                .Select(static a => new Achievement
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Points = a.Points,
                    BadgeUri = $"{SiteBaseUrl}/Badge/{a.BadgeName}.png",
                    IsUnlocked = a.DateEarned != null || a.DateEarnedHardcore != null,
                    DateUnlocked = a.DateEarnedHardcore ?? a.DateEarned,
                    UnlockedInHardcore = a.DateEarnedHardcore != null,
                    DisplayOrder = a.DisplayOrder
                }).ToList();

            DebugLogger.Log($"[RA Service] Successfully fetched {achievements.Count} achievements for GameID {gameId}.");
            return (progress, achievements);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"[RA Service] Unexpected error in GetUserGameProgressAsync for '{gameTitle}'.");
            return (null, null);
        }
    }

    /// <summary>
    /// Finds the RetroAchievements Game ID for a given game title and console.
    /// </summary>
    private static async Task<int?> FindGameIdAsync(string gameTitle, string systemName, string username, string apiKey)
    {
        var consoleId = MapConsoleNameToId(systemName);
        if (consoleId == null)
        {
            DebugLogger.Log($"[RA Service] No ConsoleID mapping found for system: {systemName}");
            return null;
        }

        // Use cache if valid (less than 24 hours old)
        if (CacheTimestamp.TryGetValue(consoleId.Value, out var timestamp) && (DateTime.UtcNow - timestamp).TotalHours < 24)
        {
            if (ConsoleGameListCache.TryGetValue(consoleId.Value, out var cachedList))
            {
                DebugLogger.Log($"[RA Service] Using cached game list for ConsoleID {consoleId.Value}.");
                return FindBestMatch(gameTitle, cachedList);
            }
        }

        // Fetch fresh list from API
        DebugLogger.Log($"[RA Service] Fetching fresh game list for ConsoleID {consoleId.Value}.");
        var client = HttpClientFactory.CreateClient();
        var url = $"{ApiBaseUrl}API_GetGameList.php?i={consoleId}&y={Uri.EscapeDataString(apiKey)}";

        var response = await client.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _ = LogErrors.LogErrorAsync(null, $"[RA Service] API_GetGameList failed with status {response.StatusCode}: {error}");
            return null;
        }

        var json = await response.Content.ReadAsStringAsync();
        var gameList = JsonSerializer.Deserialize<List<ApiGameInfo>>(json);

        if (gameList == null || gameList.Count == 0) return null;

        // Update cache
        ConsoleGameListCache[consoleId.Value] = gameList;
        CacheTimestamp[consoleId.Value] = DateTime.UtcNow;

        return FindBestMatch(gameTitle, gameList);
    }

    private static int? FindBestMatch(string gameTitle, List<ApiGameInfo> gameList)
    {
        if (gameList == null || gameList.Count == 0) return null;

        ApiGameInfo bestMatch = null;
        var highestSimilarity = 0.0;

        foreach (var game in gameList)
        {
            var similarity = FindCoverImage.CalculateJaroWinklerSimilarity(gameTitle, game.Title);
            if (similarity > highestSimilarity)
            {
                highestSimilarity = similarity;
                bestMatch = game;
            }
        }

        // Require a reasonably high similarity score to avoid false positives
        return highestSimilarity > 0.85 ? bestMatch?.Id : null;
    }

    private static int? MapConsoleNameToId(string systemName)
    {
        return systemName.ToLowerInvariant() switch
        {
            // This mapping needs to be maintained. Add more systems as needed.
            "sega genesis" or "mega drive" => 1,
            "nintendo 64" or "n64" => 2,
            "nintendo snes" or "snes" => 3,
            "nintendo game boy" or "game boy" => 4,
            "nintendo game boy advance" or "gba" => 5,
            "nintendo game boy color" or "gbc" => 6,
            "nintendo nes" or "nes" => 7,
            "sony playstation 1" or "playstation" or "ps1" => 12,
            "sega 32x" => 15,
            "sega master system" => 16,
            "sega dreamcast" => 17,
            "sony playstation 2" or "ps2" => 21,
            "nintendo wii" => 22,
            "nintendo ds" => 24,
            "arcade" => 27,
            "sony psp" => 41,
            _ => null
        };
    }
}