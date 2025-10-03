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

    /// <summary>
    /// Fetches the user's progress and achievement list for a specific game ID.
    /// </summary>
    /// <param name="gameId">The RetroAchievements ID of the game.</param>
    /// <param name="username">The user's RetroAchievements username.</param>
    /// <param name="apiKey">The user's RetroAchievements Web API Key.</param>
    /// <returns>A tuple containing the user's game progress and a list of achievements, or null if an error occurs.</returns>
    public static async Task<(RaUserGameProgress Progress, List<RaAchievement> Achievements)> GetUserGameProgressByGameIdAsync(int gameId, string username, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey))
        {
            DebugLogger.Log("[RA Service] Username or API Key is missing.");
            return (null, null);
        }

        try
        {
            DebugLogger.Log($"[RA Service] Fetching user progress for GameID {gameId}...");

            var client = HttpClientFactory.CreateClient();
            var url = $"{ApiBaseUrl}API_GetGameInfoAndUserProgress.php?u={Uri.EscapeDataString(username)}&g={gameId}&y={Uri.EscapeDataString(apiKey)}";

            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _ = LogErrors.LogErrorAsync(null, $"[RA Service] API_GetGameInfoAndUserProgress failed with status {response.StatusCode} for gameId {gameId}: {error}");
                return (null, null);
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<RaGameProgressResponse>(json);

            if (apiResponse == null) return (null, null);

            // Map the API response to our local models.
            var progress = new RaUserGameProgress
            {
                GameTitle = apiResponse.Title,
                GameIconUrl = $"{SiteBaseUrl}{apiResponse.ImageIcon}",
                ConsoleName = apiResponse.ConsoleName,
                AchievementsEarned = apiResponse.NumAwardedToUser,
                TotalAchievements = apiResponse.NumAchievements,
                PointsEarned = apiResponse.Achievements.Values.Where(static a => a.DateEarned != null).Sum(static a => a.Points),
                TotalPoints = apiResponse.Achievements.Values.Sum(static a => a.Points),
                UserCompletion = apiResponse.UserCompletion,
                UserCompletionHardcore = apiResponse.UserCompletionHardcore,
                HighestAwardKind = apiResponse.HighestAwardKind,
                HighestAwardDate = apiResponse.HighestAwardDate
            };

            var achievements = apiResponse.Achievements.Values
                .OrderBy(static a => a.DisplayOrder)
                .Select(static a => new RaAchievement
                {
                    Id = a.Id,
                    Title = a.Title,
                    Description = a.Description,
                    Points = a.Points,
                    BadgeUri = $"{SiteBaseUrl}/Badge/{a.BadgeName}.png",
                    IsUnlocked = a.DateEarned != null || a.DateEarnedHardcore != null,
                    DateUnlocked = a.DateEarnedHardcore ?? a.DateEarned,
                    UnlockedInHardcore = a.DateEarnedHardcore != null,
                    DisplayOrder = a.DisplayOrder,
                    NumAwarded = a.NumAwarded,
                    NumAwardedHardcore = a.NumAwardedHardcore,
                    Author = a.Author,
                    AuthorUlid = a.AuthorUlid,
                    DateModified = a.DateModified,
                    DateCreated = a.DateCreated,
                    BadgeName = a.BadgeName,
                    Type = a.Type,
                    DateEarnedHardcore = a.DateEarnedHardcore,
                    DateEarned = a.DateEarned
                }).ToList();

            DebugLogger.Log($"[RA Service] Successfully fetched {achievements.Count} achievements for GameID {gameId}.");
            return (progress, achievements);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"[RA Service] Unexpected error in GetUserGameProgressByGameIdAsync for gameId {gameId}.");
            return (null, null);
        }
    }

    /// <summary>
    /// Fetches extended information for a specific game.
    /// </summary>
    public static async Task<RaGameExtendedDetails> GetGameExtendedInfoAsync(int gameId, string username, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey)) return null;

        try
        {
            var client = HttpClientFactory.CreateClient();
            var url = $"{ApiBaseUrl}API_GetGameExtended.php?u={Uri.EscapeDataString(username)}&i={gameId}&y={Uri.EscapeDataString(apiKey)}";
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<RaGameExtendedDetails>(json);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"[RA Service] Error in GetGameExtendedInfoAsync for gameId {gameId}.");
            return null;
        }
    }

    /// <summary>
    /// Fetches the top 10 ranked players for a specific game.
    /// </summary>
    // Change the return type from ApiGameRankAndScoreResponse to List<RaGameRankAndScore>
    public static async Task<List<RaGameRankAndScore>> GetGameRankAndScoreAsync(int gameId, string username, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey)) return null;

        try
        {
            var client = HttpClientFactory.CreateClient();
            var url = $"{ApiBaseUrl}API_GetGameRankAndScore.php?u={Uri.EscapeDataString(username)}&g={gameId}&y={Uri.EscapeDataString(apiKey)}";
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _ = LogErrors.LogErrorAsync(null, $"[RA Service] API_GetGameRankAndScore failed with status {response.StatusCode} for gameId {gameId}: {error}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            // Deserialize directly into a List of RaGameRankAndScore
            return JsonSerializer.Deserialize<List<RaGameRankAndScore>>(json);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"[RA Service] Error in GetGameRankAndScoreAsync for gameId {gameId}.");
            return null;
        }
    }

    /// <summary>
    /// Fetches the profile information for a specific user.
    /// </summary>
    public static async Task<RaProfile> GetUserProfileAsync(string username, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey)) return null;

        try
        {
            var client = HttpClientFactory.CreateClient();
            var url = $"{ApiBaseUrl}API_GetUserProfile.php?u={Uri.EscapeDataString(username)}&y={Uri.EscapeDataString(apiKey)}";
            var response = await client.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<RaProfile>(json);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"[RA Service] Error in GetUserProfileAsync for user {username}.");
            return null;
        }
    }
}
