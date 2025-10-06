using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;

namespace SimpleLauncher.Services;

public class RetroAchievementsService
{
    private const string ApiBaseUrl = "https://retroachievements.org/API/";
    private const string SiteBaseUrl = "https://retroachievements.org";
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    public RetroAchievementsManager RaManager { get; }

    // Constructor to inject dependencies
    public RetroAchievementsService(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, RetroAchievementsManager raManager)
    {
        _httpClient = httpClientFactory.CreateClient("RetroAchievementsClient");
        _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        RaManager = raManager ?? throw new ArgumentNullException(nameof(raManager));
    }

    /// <summary>
    /// Fetches the user's progress and achievement list for a specific game ID.
    /// https://github.com/RetroAchievements/RAWeb/blob/master/public/API/API_GetGameInfoAndUserProgress.php
    /// </summary>
    /// <param name="gameId">The RetroAchievements ID of the game.</param>
    /// <param name="username">The user's RetroAchievements username.</param>
    /// <param name="apiKey">The user's RetroAchievements Web API Key.</param>
    /// <returns>A tuple containing the user's game progress and a list of achievements, or null if an error occurs.</returns>
    public async Task<(RaUserGameProgress Progress, List<RaAchievement> Achievements)> GetGameInfoAndUserProgress(int gameId, string username, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey))
        {
            DebugLogger.Log("[RA Service] Username or API Key is missing.");
            return (null, null);
        }

        var cacheKey = $"UserGameProgress_{username}_{gameId}";
        if (_cache.TryGetValue(cacheKey, out (RaUserGameProgress Progress, List<RaAchievement> Achievements) cachedResult))
        {
            DebugLogger.Log($"[RA Service] Cache hit for {cacheKey}");
            return cachedResult;
        }

        try
        {
            DebugLogger.Log($"[RA Service] Fetching user progress for GameID {gameId}...");

            var url = $"{ApiBaseUrl}API_GetGameInfoAndUserProgress.php?u={Uri.EscapeDataString(username)}&g={gameId}&y={Uri.EscapeDataString(apiKey)}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _ = LogErrors.LogErrorAsync(null, $"[RA Service] API_GetGameInfoAndUserProgress failed with status {response.StatusCode} for gameId {gameId}: {error}");
                return (null, null);
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<RaGameProgressResponse>(json);

            if (apiResponse == null) return (null, null);

            // Calculate points earned hardcore
            var pointsEarnedHardcore = apiResponse.Achievements.Values
                .Where(static a => a.DateEarnedHardcore != null)
                .Sum(static a => a.Points);

            // Map the API response to our local models.
            var progress = new RaUserGameProgress
            {
                GameTitle = apiResponse.Title,
                GameIconUrl = $"{SiteBaseUrl}{apiResponse.ImageIcon}",
                ConsoleName = apiResponse.ConsoleName,
                AchievementsEarned = apiResponse.NumAwardedToUser,
                TotalAchievements = apiResponse.NumAchievements,
                PointsEarned = apiResponse.Achievements.Values.Where(static a => a.DateEarned != null).Sum(static a => a.Points), // Total points from any earned achievement
                PointsEarnedHardcore = pointsEarnedHardcore,
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
                    DateEarned = a.DateEarned,
                    TrueRatio = a.TrueRatio
                }).ToList();

            var result = (progress, achievements);
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5)); // Cache for 5 minutes
            DebugLogger.Log($"[RA Service] Cached {cacheKey}");
            return result;
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"[RA Service] Unexpected error in GetGameInfoAndUserProgress for gameId {gameId}.");
            return (null, null);
        }
    }

    /// <summary>
    /// Fetches extended information for a specific game.
    /// https://github.com/RetroAchievements/RAWeb/blob/master/public/API/API_GetGameExtended.php
    /// </summary>
    public async Task<RaGameExtendedDetails> GetGameExtended(int gameId, string username, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey)) return null;

        var cacheKey = $"GameExtendedInfo_{gameId}";
        if (_cache.TryGetValue(cacheKey, out RaGameExtendedDetails cachedResult))
        {
            DebugLogger.Log($"[RA Service] Cache hit for {cacheKey}");
            return cachedResult;
        }

        try
        {
            var url = $"{ApiBaseUrl}API_GetGameExtended.php?u={Uri.EscapeDataString(username)}&i={gameId}&y={Uri.EscapeDataString(apiKey)}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RaGameExtendedDetails>(json);
            _cache.Set(cacheKey, result, TimeSpan.FromHours(1)); // Cache for 1 hour
            DebugLogger.Log($"[RA Service] Cached {cacheKey}");
            return result;
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"[RA Service] Error in GetGameExtended for gameId {gameId}.");
            return null;
        }
    }

    /// <summary>
    /// Fetches the user's rank and score for a specific game.
    /// https://retroachievements.org/API/API_GetUserGameRankAndScore.php
    /// </summary>
    public async Task<List<RaUserGameRank>> GetUserGameRankAndScoreAsync(int gameId, string username, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey)) return null;

        var cacheKey = $"UserGameRankAndScore_{username}_{gameId}";
        if (_cache.TryGetValue(cacheKey, out List<RaUserGameRank> cachedResult))
        {
            DebugLogger.Log($"[RA Service] Cache hit for {cacheKey}");
            return cachedResult;
        }

        try
        {
            var url = $"{ApiBaseUrl}API_GetUserGameRankAndScore.php?u={Uri.EscapeDataString(username)}&g={gameId}&y={Uri.EscapeDataString(apiKey)}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _ = LogErrors.LogErrorAsync(null, $"[RA Service] API_GetUserGameRankAndScore failed with status {response.StatusCode} for gameId {gameId}: {error}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<RaUserGameRank>>(json);
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10)); // Cache for 10 minutes
            DebugLogger.Log($"[RA Service] Cached {cacheKey}");
            return result;
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"[RA Service] Error in GetUserGameRankAndScoreAsync for gameId {gameId}.");
            return null;
        }
    }

    /// <summary>
    /// Fetches the top 10 ranked players for a specific game.
    /// https://github.com/RetroAchievements/RAWeb/blob/master/public/API/API_GetGameRankAndScore.php
    /// </summary>
    public async Task<List<RaGameRankAndScore>> GetGameRankAndScoreAsync(int gameId, string username, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey)) return null;

        var cacheKey = $"GameRankAndScore_{gameId}";
        if (_cache.TryGetValue(cacheKey, out List<RaGameRankAndScore> cachedResult))
        {
            DebugLogger.Log($"[RA Service] Cache hit for {cacheKey}");
            return cachedResult;
        }

        try
        {
            var url = $"{ApiBaseUrl}API_GetGameRankAndScore.php?u={Uri.EscapeDataString(username)}&g={gameId}&y={Uri.EscapeDataString(apiKey)}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _ = LogErrors.LogErrorAsync(null, $"[RA Service] API_GetGameRankAndScore failed with status {response.StatusCode} for gameId {gameId}: {error}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<RaGameRankAndScore>>(json);
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10)); // Cache for 10 minutes
            DebugLogger.Log($"[RA Service] Cached {cacheKey}");
            return result;
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"[RA Service] Error in GetGameRankAndScoreAsync for gameId {gameId}.");
            return null;
        }
    }

    /// <summary>
    /// Fetches the profile information for a specific user.
    /// https://retroachievements.org/API/API_GetUserProfile.php
    /// </summary>
    public async Task<RaProfile> GetUserProfile(string username, string apiKey)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey)) return null;

        var cacheKey = $"UserProfile_{username}";
        if (_cache.TryGetValue(cacheKey, out RaProfile cachedResult))
        {
            DebugLogger.Log($"[RA Service] Cache hit for {cacheKey}");
            return cachedResult;
        }

        try
        {
            var url = $"{ApiBaseUrl}API_GetUserProfile.php?u={Uri.EscapeDataString(username)}&y={Uri.EscapeDataString(apiKey)}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode) return null;

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RaProfile>(json);
            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(30)); // Cache for 30 minutes
            DebugLogger.Log($"[RA Service] Cached {cacheKey}");
            return result;
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"[RA Service] Error in GetUserProfile for user {username}.");
            return null;
        }
    }

    /// <summary>
    /// Fetches a list of a target user's recently played games.
    /// https://retroachievements.org/API/API_GetUserRecentlyPlayedGames.php
    /// </summary>
    /// <param name="username">The user's RetroAchievements username.</param>
    /// <param name="apiKey">The user's RetroAchievements Web API Key.</param>
    /// <param name="count">Number of records to return (default: 10, max: 50).</param>
    /// <param name="offset">Number of entries to skip (default: 0).</param>
    /// <returns>A list of <see cref="RaRecentlyPlayedGame"/>, or null if an error occurs.</returns>
    public async Task<List<RaRecentlyPlayedGame>> GetUserRecentlyPlayedGamesAsync(string username, string apiKey, int count = 10, int offset = 0)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey))
        {
            DebugLogger.Log("[RA Service] Username or API Key is missing for GetUserRecentlyPlayedGamesAsync.");
            return null;
        }

        var cacheKey = $"UserRecentlyPlayedGames_{username}_{count}_{offset}";
        if (_cache.TryGetValue(cacheKey, out List<RaRecentlyPlayedGame> cachedResult))
        {
            DebugLogger.Log($"[RA Service] Cache hit for {cacheKey}");
            return cachedResult;
        }

        try
        {
            DebugLogger.Log($"[RA Service] Fetching recently played games for {username} (Count: {count}, Offset: {offset})...");

            var url = $"{ApiBaseUrl}API_GetUserRecentlyPlayedGames.php?u={Uri.EscapeDataString(username)}&y={Uri.EscapeDataString(apiKey)}&c={count}&o={offset}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _ = LogErrors.LogErrorAsync(null, $"[RA Service] API_GetUserRecentlyPlayedGames failed with status {response.StatusCode} for user {username}: {error}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<List<RaRecentlyPlayedGame>>(json);

            if (apiResponse == null) return null;

            _cache.Set(cacheKey, apiResponse, TimeSpan.FromMinutes(15)); // Cache for 15 minutes
            DebugLogger.Log($"[RA Service] Cached {cacheKey}");
            return apiResponse;
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"[RA Service] Unexpected error in GetUserRecentlyPlayedGamesAsync for user {username}.");
            return null;
        }
    }

    /// <summary>
    /// Fetches a list of achievements unlocked by a user between two given dates.
    /// https://github.com/RetroAchievements/RAWeb/blob/master/public/API/API_GetAchievementsEarnedBetween.php
    /// </summary>
    /// <param name="username">The user's RetroAchievements username.</param>
    /// <param name="apiKey">The user's RetroAchievements Web API Key.</param>
    /// <param name="fromDate">The start date for the range.</param>
    /// <param name="toDate">The end date for the range (inclusive).</param>
    /// <returns>A list of <see cref="RaEarnedAchievement"/>, or null if an error occurs.</returns>
    public async Task<List<RaEarnedAchievement>> GetAchievementsEarnedBetween(string username, string apiKey, DateTime fromDate, DateTime toDate)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey))
        {
            DebugLogger.Log("[RA Service] Username or API Key is missing for GetAchievementsEarnedBetween.");
            return null;
        }

        // Convert DateTime to Unix epoch timestamp.
        // For 'toDate', add one day and subtract one second to include the entire day.
        var epochFrom = new DateTimeOffset(fromDate).ToUnixTimeSeconds();
        var epochTo = new DateTimeOffset(toDate.AddDays(1).AddSeconds(-1)).ToUnixTimeSeconds();

        var cacheKey = $"AchievementsEarnedBetween_{username}_{epochFrom}_{epochTo}";
        if (_cache.TryGetValue(cacheKey, out List<RaEarnedAchievement> cachedResult))
        {
            DebugLogger.Log($"[RA Service] Cache hit for {cacheKey}");
            return cachedResult;
        }

        try
        {
            DebugLogger.Log($"[RA Service] Fetching achievements earned by {username} between {fromDate:yyyy-MM-dd} and {toDate:yyyy-MM-dd}...");

            var url = $"{ApiBaseUrl}API_GetAchievementsEarnedBetween.php?u={Uri.EscapeDataString(username)}&f={epochFrom}&t={epochTo}&y={Uri.EscapeDataString(apiKey)}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _ = LogErrors.LogErrorAsync(null, $"[RA Service] API_GetAchievementsEarnedBetween failed with status {response.StatusCode} for user {username}: {error}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<List<RaEarnedAchievement>>(json);

            if (apiResponse == null) return null;

            _cache.Set(cacheKey, apiResponse, TimeSpan.FromMinutes(15)); // Cache for 15 minutes
            DebugLogger.Log($"[RA Service] Cached {cacheKey}");
            return apiResponse;
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"[RA Service] Unexpected error in GetAchievementsEarnedBetween for user {username}.");
            return null;
        }
    }

    /// <summary>
    /// Fetches a user's overall game completion progress.
    /// https://retroachievements.org/API/API_GetUserCompletionProgress.php
    /// </summary>
    /// <param name="username">The user's RetroAchievements username.</param>
    /// <param name="apiKey">The user's RetroAchievements Web API Key.</param>
    /// <param name="count">Number of records to return (default: 100, max: 500).</param>
    /// <param name="offset">Number of entries to skip (default: 0).</param>
    /// <returns>A list of <see cref="RaUserCompletionGame"/>, or null if an error occurs.</returns>
    public async Task<List<RaUserCompletionGame>> GetUserCompletionProgress(string username, string apiKey, int count = 100, int offset = 0)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey))
        {
            DebugLogger.Log("[RA Service] Username or API Key is missing for GetUserCompletionProgress.");
            return null;
        }

        var cacheKey = $"UserCompletionProgress_{username}_{count}_{offset}";
        if (_cache.TryGetValue(cacheKey, out List<RaUserCompletionGame> cachedResult))
        {
            DebugLogger.Log($"[RA Service] Cache hit for {cacheKey}");
            return cachedResult;
        }

        try
        {
            DebugLogger.Log($"[RA Service] Fetching user completion progress for {username} (Count: {count}, Offset: {offset})...");

            var url = $"{ApiBaseUrl}API_GetUserCompletionProgress.php?u={Uri.EscapeDataString(username)}&y={Uri.EscapeDataString(apiKey)}&c={count}&o={offset}";

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _ = LogErrors.LogErrorAsync(null, $"[RA Service] API_GetUserCompletionProgress failed with status {response.StatusCode} for user {username}: {error}");
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<RaUserCompletionProgressResponse>(json);

            if (apiResponse?.Results == null) return null;

            // The API returns ImageIcon as "/Images/..." so we need to prepend the base URL
            foreach (var game in apiResponse.Results)
            {
                if (!string.IsNullOrEmpty(game.ImageIcon) && !game.ImageIcon.StartsWith(SiteBaseUrl, StringComparison.OrdinalIgnoreCase))
                {
                    game.ImageIcon = $"{SiteBaseUrl}{game.ImageIcon}";
                }
            }

            _cache.Set(cacheKey, apiResponse.Results, TimeSpan.FromMinutes(15)); // Cache for 15 minutes
            DebugLogger.Log($"[RA Service] Cached {cacheKey}");
            return apiResponse.Results;
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"[RA Service] Unexpected error in GetUserCompletionProgress for user {username}.");
            return null;
        }
    }
}