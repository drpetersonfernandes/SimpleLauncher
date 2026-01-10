using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Managers;
using SimpleLauncher.Models.RetroAchievements;

namespace SimpleLauncher.Services.RetroAchievements;

public class RetroAchievementsService(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, RetroAchievementsManager raManager, ILogErrors logErrors) : IDisposable
{
    private const string ApiBaseUrl = "https://retroachievements.org/API/";
    private const string SiteBaseUrl = "https://retroachievements.org";
    private readonly HttpClient _httpClient = httpClientFactory?.CreateClient("RetroAchievementsClient");
    private readonly IMemoryCache _cache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
    private readonly ILogErrors _logErrors = logErrors ?? throw new ArgumentNullException(nameof(logErrors));
    private CancellationTokenSource _cacheResetTokenSource = new();
    public RetroAchievementsManager RaManager { get; } = raManager ?? throw new ArgumentNullException(nameof(raManager));

    // Constructor to inject dependencies

    /// <summary>
    /// Clears all RetroAchievements related cache entries.
    /// This should be called when user credentials change.
    /// </summary>
    public void ClearCache()
    {
        // Cancel the old token, which invalidates all cache entries tied to it.
        _cacheResetTokenSource.Cancel();
        _cacheResetTokenSource.Dispose();

        // Create a new token source for future cache entries.
        _cacheResetTokenSource = new CancellationTokenSource();
        DebugLogger.Log("[RA Service] Cache cleared due to credential change.");
    }

    /// <summary>
    /// Fetches the user's progress and achievement list for a specific game ID.
    /// https://github.com/RetroAchievements/RAWeb/blob/master/public/API/API_GetGameInfoAndUserProgress.php
    /// </summary>
    /// <param name="gameId">The RetroAchievements ID of the game.</param>
    /// <param name="username">The user's RetroAchievements username.</param>
    /// <param name="apiKey">The user's RetroAchievements Web API Key.</param>
    /// <returns>A tuple containing the user's game progress and a list of achievements, or null if an error occurs.</returns>
    /// <exception cref="RaUnauthorizedException">Thrown if the API returns an Unauthorized status (401).</exception>
    public async Task<(RaUserGameProgress Progress, List<RaAchievement> Achievements)> GetGameInfoAndUserProgressAsync(int gameId, string username, string apiKey)
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
                _ = _logErrors.LogErrorAsync(null, $"[RA Service] API_GetGameInfoAndUserProgress failed with status {response.StatusCode} for gameId {gameId}: {error}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new RaUnauthorizedException("RetroAchievements API returned Unauthorized. Check username and API key.");
                }

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
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .AddExpirationToken(new CancellationChangeToken(_cacheResetTokenSource.Token));
            _cache.Set(cacheKey, result, cacheOptions);
            DebugLogger.Log($"[RA Service] Cached {cacheKey}");
            return result;
        }
        catch (RaUnauthorizedException)
        {
            throw; // Re-throw the specific exception
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, $"[RA Service] Unexpected error in GetGameInfoAndUserProgressAsync for gameId {gameId}.");
            return (null, null);
        }
    }

    /// <summary>
    /// Fetches extended information for a specific game.
    /// https://github.com/RetroAchievements/RAWeb/blob/master/public/API/API_GetGameExtended.php
    /// </summary>
    /// <exception cref="RaUnauthorizedException">Thrown if the API returns an Unauthorized status (401).</exception>
    public async Task<RaGameExtendedDetails> GetGameExtendedAsync(int gameId, string username, string apiKey)
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
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _ = _logErrors.LogErrorAsync(null, $"[RA Service] API_GetGameExtended failed with status {response.StatusCode} for gameId {gameId}: {error}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new RaUnauthorizedException("RetroAchievements API returned Unauthorized. Check username and API key.");
                }

                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RaGameExtendedDetails>(json);
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromHours(1))
                .AddExpirationToken(new CancellationChangeToken(_cacheResetTokenSource.Token));
            _cache.Set(cacheKey, result, cacheOptions);
            DebugLogger.Log($"[RA Service] Cached {cacheKey}");
            return result;
        }
        catch (RaUnauthorizedException)
        {
            throw; // Re-throw the specific exception
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, $"[RA Service] Error in GetGameExtendedAsync for gameId {gameId}.");
            return null;
        }
    }

    /// <summary>
    /// Fetches the user's rank and score for a specific game.
    /// https://retroachievements.org/API/API_GetUserGameRankAndScore.php
    /// </summary>
    /// <exception cref="RaUnauthorizedException">Thrown if the API returns an Unauthorized status (401).</exception>
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
                _ = _logErrors.LogErrorAsync(null, $"[RA Service] API_GetUserGameRankAndScore failed with status {response.StatusCode} for gameId {gameId}: {error}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new RaUnauthorizedException("RetroAchievements API returned Unauthorized. Check username and API key.");
                }

                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<List<RaUserGameRank>>(json);
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(2))
                .AddExpirationToken(new CancellationChangeToken(_cacheResetTokenSource.Token));
            _cache.Set(cacheKey, result, cacheOptions);
            DebugLogger.Log($"[RA Service] Cached {cacheKey}");
            return result;
        }
        catch (RaUnauthorizedException)
        {
            throw; // Re-throw the specific exception
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, $"[RA Service] Error in GetUserGameRankAndScoreAsync for gameId {gameId}.");
            return null;
        }
    }

    /// <summary>
    /// Fetches the top 10 ranked players for a specific game.
    /// https://github.com/RetroAchievements/RAWeb/blob/master/public/API/API_GetGameRankAndScore.php
    /// </summary>
    /// <exception cref="RaUnauthorizedException">Thrown if the API returns an Unauthorized status (401).</exception>
    public async Task<List<RaGameRankAndScore>> GetGameRankAndScoreAsync(int gameId, string username, string apiKey, bool latestMasters = false)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey)) return null;

        var typeParam = latestMasters ? "1" : "0";
        var cacheKey = $"GameRankAndScore_{gameId}_{typeParam}";
        if (_cache.TryGetValue(cacheKey, out List<RaGameRankAndScore> cachedResult))
        {
            DebugLogger.Log($"[RA Service] Cache hit for {cacheKey}");
            return cachedResult;
        }

        try
        {
            var url = $"{ApiBaseUrl}API_GetGameRankAndScore.php?u={Uri.EscapeDataString(username)}&g={gameId}&y={Uri.EscapeDataString(apiKey)}&t={typeParam}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _ = _logErrors.LogErrorAsync(null, $"[RA Service] API_GetGameRankAndScore failed with status {response.StatusCode} for gameId {gameId}: {error}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new RaUnauthorizedException("RetroAchievements API returned Unauthorized. Check username and API key.");
                }

                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            DebugLogger.Log($"[RA Service] API_GetGameRankAndScore response: {json}");

            var result = JsonSerializer.Deserialize<List<RaGameRankAndScore>>(json);
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(2))
                .AddExpirationToken(new CancellationChangeToken(_cacheResetTokenSource.Token));
            _cache.Set(cacheKey, result, cacheOptions);
            DebugLogger.Log($"[RA Service] Cached {cacheKey}");

            return result;
        }
        catch (RaUnauthorizedException)
        {
            throw; // Re-throw the specific exception
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, $"[RA Service] Error in GetGameRankAndScoreAsync for gameId {gameId}.");
            return null;
        }
    }

    /// <summary>
    /// Fetches the profile information for a specific user.
    /// https://retroachievements.org/API/API_GetUserProfile.php
    /// </summary>
    /// <exception cref="RaUnauthorizedException">Thrown if the API returns an Unauthorized status (401).</exception>
    public async Task<RaProfile> GetUserProfileAsync(string username, string apiKey)
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
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _ = _logErrors.LogErrorAsync(null, $"[RA Service] API_GetUserProfile failed with status {response.StatusCode} for user {username}: {error}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new RaUnauthorizedException("RetroAchievements API returned Unauthorized. Check username and API key.");
                }

                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<RaProfile>(json);

            if (result == null)
            {
                DebugLogger.Log("[RA Service] Failed to deserialize user profile response.");
                return null;
            }

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(30))
                .AddExpirationToken(new CancellationChangeToken(_cacheResetTokenSource.Token));
            _cache.Set(cacheKey, result, cacheOptions);
            DebugLogger.Log($"[RA Service] Cached {cacheKey}");
            return result;
        }
        catch (RaUnauthorizedException)
        {
            throw; // Re-throw the specific exception
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, $"[RA Service] Error in GetUserProfileAsync for user {username}.");
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
    /// <exception cref="RaUnauthorizedException">Thrown if the API returns an Unauthorized status (401).</exception>
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
                _ = _logErrors.LogErrorAsync(null, $"[RA Service] API_GetUserRecentlyPlayedGames failed with status {response.StatusCode} for user {username}: {error}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new RaUnauthorizedException("RetroAchievements API returned Unauthorized. Check username and API key.");
                }

                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonSerializer.Deserialize<List<RaRecentlyPlayedGame>>(json);

            if (apiResponse == null) return null;

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                .AddExpirationToken(new CancellationChangeToken(_cacheResetTokenSource.Token));
            _cache.Set(cacheKey, apiResponse, cacheOptions);
            DebugLogger.Log($"[RA Service] Cached {cacheKey}");
            return apiResponse;
        }
        catch (RaUnauthorizedException)
        {
            throw; // Re-throw the specific exception
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, $"[RA Service] Unexpected error in GetUserRecentlyPlayedGamesAsync for user {username}.");
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
    /// <exception cref="RaUnauthorizedException">Thrown if the API returns an Unauthorized status (401).</exception>
    public async Task<List<RaEarnedAchievement>> GetAchievementsEarnedBetweenAsync(string username, string apiKey, DateTime fromDate, DateTime toDate)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey))
        {
            DebugLogger.Log("[RA Service] Username or API Key is missing for GetAchievementsEarnedBetweenAsync.");
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
            DebugLogger.Log($"[RA Service] Epoch timestamps: from={epochFrom}, to={epochTo}");

            var url = $"{ApiBaseUrl}API_GetAchievementsEarnedBetween.php?u={Uri.EscapeDataString(username)}&f={epochFrom}&t={epochTo}&y={Uri.EscapeDataString(apiKey)}";
            DebugLogger.Log($"[RA Service] Request URL: {url.Replace(apiKey, "***")}"); // Log URL without exposing API key

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                DebugLogger.Log($"[RA Service] API_GetAchievementsEarnedBetween failed. Status: {response.StatusCode}, Error: {error}");
                _ = _logErrors.LogErrorAsync(null, $"[RA Service] API_GetAchievementsEarnedBetween failed with status {response.StatusCode} for user {username}: {error}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new RaUnauthorizedException("RetroAchievements API returned Unauthorized. Check username and API key.");
                }

                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            DebugLogger.Log($"[RA Service] API_GetAchievementsEarnedBetween response length: {json.Length} characters");

            var apiResponse = JsonSerializer.Deserialize<List<RaEarnedAchievement>>(json);

            if (apiResponse == null)
            {
                DebugLogger.Log("[RA Service] Failed to deserialize achievements earned between response.");
            }

            if (apiResponse == null) return null;

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                .AddExpirationToken(new CancellationChangeToken(_cacheResetTokenSource.Token));
            _cache.Set(cacheKey, apiResponse, cacheOptions);
            DebugLogger.Log($"[RA Service] Cached {cacheKey}");
            return apiResponse;
        }
        catch (RaUnauthorizedException)
        {
            throw; // Re-throw the specific exception
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, $"[RA Service] Unexpected error in GetAchievementsEarnedBetweenAsync for user {username}.");
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
    /// <exception cref="RaUnauthorizedException">Thrown if the API returns an Unauthorized status (401).</exception>
    public async Task<List<RaUserCompletionGame>> GetUserCompletionProgressAsync(string username, string apiKey, int count = 100, int offset = 0)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(apiKey))
        {
            DebugLogger.Log("[RA Service] Username or API Key is missing for GetUserCompletionProgressAsync.");
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
                _ = _logErrors.LogErrorAsync(null, $"[RA Service] API_GetUserCompletionProgress failed with status {response.StatusCode} for user {username}: {error}");

                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    throw new RaUnauthorizedException("RetroAchievements API returned Unauthorized. Check username and API key.");
                }

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

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(15))
                .AddExpirationToken(new CancellationChangeToken(_cacheResetTokenSource.Token));
            _cache.Set(cacheKey, apiResponse.Results, cacheOptions);
            DebugLogger.Log($"[RA Service] Cached {cacheKey}");
            return apiResponse.Results;
        }
        catch (RaUnauthorizedException)
        {
            throw; // Re-throw the specific exception
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, $"[RA Service] Unexpected error in GetUserCompletionProgressAsync for user {username}.");
            return null;
        }
    }

    public void Dispose()
    {
        _cacheResetTokenSource.Dispose();
        GC.SuppressFinalize(this);
    }
}