#nullable enable

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.RetroAchievements;
using SimpleLauncher.Core.Services.RetroAchievements.Models;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.Services.PlaySound;

namespace SimpleLauncher.ViewModels;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class RetroAchievementsForAGameViewModel : ObservableObject
{
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IResourceProvider _resourceProvider;
    private readonly SettingsManager _settings;
    private readonly RetroAchievementsService _raService;
    private readonly PlaySoundEffects _playSoundEffects;
    private int _gameId;
    private string _gameTitleForDisplay = string.Empty;

    // Game info
    [ObservableProperty] private string _gameTitle = string.Empty;

    [ObservableProperty] private string _consoleName = string.Empty;

    [ObservableProperty] private string? _gameIconUrl;

    [ObservableProperty] private string? _gameTitleImageUrl;

    [ObservableProperty] private string? _gameBoxArtUrl;

    [ObservableProperty] private string? _gamePublisher;

    [ObservableProperty] private string? _gameDeveloper;

    [ObservableProperty] private string? _gameGenre;

    [ObservableProperty] private string? _gameReleased;

    [ObservableProperty] private bool _isGameInfoLoaded;

    // Achievements
    [ObservableProperty] private ObservableCollection<RaAchievement>? _achievements;

    [ObservableProperty] private bool _noAchievementsVisible;

    [ObservableProperty] private string _noAchievementsMessage = string.Empty;

    // Progress
    [ObservableProperty] private double _casualCompletion;

    [ObservableProperty] private double _hardcoreCompletion;

    [ObservableProperty] private string _casualProgressText = "0%";

    [ObservableProperty] private string _hardcoreProgressText = "0%";

    [ObservableProperty] private string _earnedAchievements = "0";

    [ObservableProperty] private string _totalAchievements = "0";

    [ObservableProperty] private string _totalPointsEarned = "0";

    [ObservableProperty] private string _truePointsEarned = "0";

    [ObservableProperty] private string _highestAwardKind = string.Empty;

    [ObservableProperty] private string _highestAwardDate = string.Empty;

    [ObservableProperty] private bool _isMastered;

    // Rankings
    [ObservableProperty] private ObservableCollection<RaGameRankAndScore>? _highScores;

    [ObservableProperty] private ObservableCollection<RaGameRankAndScore>? _latestMasters;

    [ObservableProperty] private RaGameRankAndScore? _userRank;

    [ObservableProperty] private bool _noRankingVisible;

    [ObservableProperty] private string _noRankingMessage = string.Empty;

    // Loading state
    [ObservableProperty] private bool _isLoading;

    public RetroAchievementsForAGameViewModel(
        ILogErrors logErrors,
        IMessageBoxLibraryService messageBox,
        IResourceProvider resourceProvider,
        SettingsManager settings,
        RetroAchievementsService raService,
        PlaySoundEffects playSoundEffects)
    {
        _logErrors = logErrors;
        _messageBox = messageBox;
        _resourceProvider = resourceProvider;
        _settings = settings;
        _raService = raService;
        _playSoundEffects = playSoundEffects;
    }

    public void Initialize(int gameId, string gameTitleForDisplay)
    {
        _gameId = gameId;
        _gameTitleForDisplay = gameTitleForDisplay;
        GameTitle = gameTitleForDisplay;
    }

    public async Task LoadGameAchievementsAsync()
    {
        IsLoading = true;
        NoAchievementsVisible = false;
        Achievements = null;

        if (!CheckCredentials())
        {
            NoAchievementsVisible = true;
            NoAchievementsMessage = _resourceProvider.GetString("RaErrorCredentialsNotSet", "RetroAchievements username or API key is not set. Configure in settings.");
            IsLoading = false;
            return;
        }

        try
        {
            var (progress, achievements) = await _raService.GetGameInfoAndUserProgressAsync(_gameId, _settings.RaUsername!, _settings.RaApiKey!);

            if (progress != null && achievements is { Count: > 0 })
            {
                GameTitle = string.IsNullOrWhiteSpace(progress.GameTitle)
                    ? _resourceProvider.GetString("RaFallbackUnknownGame", "Unknown Game")
                    : progress.GameTitle;
                ConsoleName = string.IsNullOrWhiteSpace(progress.ConsoleName)
                    ? _resourceProvider.GetString("RaFallbackUnknownConsole", "Unknown Console")
                    : progress.ConsoleName;
                GameIconUrl = progress.GameIconUrl;

                UpdateProgressFromData(progress);
                Achievements = new ObservableCollection<RaAchievement>(achievements);
                NoAchievementsVisible = false;
            }
            else
            {
                NoAchievementsVisible = true;
                NoAchievementsMessage = progress == null
                    ? _resourceProvider.GetString("RaErrorFailedToLoadAchievements", "Failed to load achievements. Please check your RetroAchievements credentials or try again later.")
                    : _resourceProvider.GetString("RaInfoNoAchievementsForGame", "No achievements found for this game.");
            }
        }
        catch (RaUnauthorizedException)
        {
            NoAchievementsVisible = true;
            NoAchievementsMessage = _resourceProvider.GetString("RaErrorUnauthorized", "RetroAchievements credentials invalid.");
        }
        catch (Exception ex)
        {
            NoAchievementsVisible = true;
            NoAchievementsMessage = _resourceProvider.GetString("RaErrorLoadingAchievements", "An error occurred while loading achievements.");
            _logErrors.LogAndForget(ex, $"Failed to load achievements for game ID: {_gameId}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadGameInfoAsync()
    {
        IsLoading = true;
        IsGameInfoLoaded = false;

        if (!CheckCredentials())
        {
            IsLoading = false;
            return;
        }

        try
        {
            var gameInfo = await _raService.GetGameExtendedAsync(_gameId, _settings.RaUsername!, _settings.RaApiKey!);
            if (gameInfo != null)
            {
                GameIconUrl = !string.IsNullOrEmpty(gameInfo.ImageIcon)
                    ? $"https://retroachievements.org{gameInfo.ImageIcon}"
                    : null;
                GameTitleImageUrl = !string.IsNullOrEmpty(gameInfo.ImageTitle)
                    ? $"https://retroachievements.org{gameInfo.ImageTitle}"
                    : null;
                GameBoxArtUrl = !string.IsNullOrEmpty(gameInfo.ImageBoxArt)
                    ? $"https://retroachievements.org{gameInfo.ImageBoxArt}"
                    : null;
                GamePublisher = gameInfo.Publisher;
                GameDeveloper = gameInfo.Developer;
                GameGenre = gameInfo.Genre;
                GameReleased = gameInfo.Released;
                IsGameInfoLoaded = true;
            }
        }
        catch (RaUnauthorizedException)
        {
            NoAchievementsVisible = true;
            NoAchievementsMessage = _resourceProvider.GetString("RaErrorUnauthorized", "RetroAchievements credentials invalid.");
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Failed to load game info for game ID: {_gameId}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadGameRankingAsync()
    {
        IsLoading = true;
        NoRankingVisible = false;
        HighScores = null;
        LatestMasters = null;
        UserRank = null;

        if (!CheckCredentials())
        {
            NoRankingVisible = true;
            NoRankingMessage = _resourceProvider.GetString("RaErrorCredentialsNotSet", "RetroAchievements username or API key is not set.");
            IsLoading = false;
            return;
        }

        try
        {
            // Load Latest Masters (latestMasters=true)
            var latestMasters = await _raService.GetGameRankAndScoreAsync(_gameId, _settings.RaUsername!, _settings.RaApiKey!, true);
            if (latestMasters is { Count: > 0 })
            {
                for (var i = 0; i < latestMasters.Count; i++)
                {
                    latestMasters[i].Rank = i + 1;
                }

                LatestMasters = new ObservableCollection<RaGameRankAndScore>(latestMasters);
            }

            // Load High Scores (default)
            var rankings = await _raService.GetGameRankAndScoreAsync(_gameId, _settings.RaUsername!, _settings.RaApiKey!);
            if (rankings is { Count: > 0 })
            {
                for (var i = 0; i < rankings.Count; i++)
                {
                    rankings[i].Rank = i + 1;
                }

                HighScores = new ObservableCollection<RaGameRankAndScore>(rankings);
            }

            NoRankingVisible = HighScores == null && LatestMasters == null;
        }
        catch (RaUnauthorizedException)
        {
            NoRankingVisible = true;
            NoRankingMessage = _resourceProvider.GetString("RaErrorUnauthorized", "RetroAchievements credentials invalid.");
        }
        catch (Exception ex)
        {
            NoRankingVisible = true;
            NoRankingMessage = _resourceProvider.GetString("RaErrorLoadingRankings", "An error occurred while loading game rankings.");
            _logErrors.LogAndForget(ex, $"Failed to load game rankings for game ID: {_gameId}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public string GetProfileUrl()
    {
        return $"https://retroachievements.org/user/{Uri.EscapeDataString(_settings.RaUsername ?? string.Empty)}";
    }

    public string GetGameUrl()
    {
        return $"https://retroachievements.org/game/{_gameId}";
    }

    private bool CheckCredentials()
    {
        return !string.IsNullOrWhiteSpace(_settings.RaUsername) && !string.IsNullOrWhiteSpace(_settings.RaApiKey);
    }

    private void UpdateProgressFromData(RaUserGameProgress progress)
    {
        try
        {
            if (!string.IsNullOrWhiteSpace(progress.UserCompletion))
            {
                var casualText = progress.UserCompletion.Replace("%", "").Trim();
                if (double.TryParse(casualText, NumberStyles.Float, CultureInfo.InvariantCulture, out var casual))
                {
                    CasualCompletion = casual;
                }
            }

            if (!string.IsNullOrWhiteSpace(progress.UserCompletionHardcore))
            {
                var hardcoreText = progress.UserCompletionHardcore.Replace("%", "").Trim();
                if (double.TryParse(hardcoreText, NumberStyles.Float, CultureInfo.InvariantCulture, out var hardcore))
                {
                    HardcoreCompletion = hardcore;
                }
            }

            CasualProgressText = $"{CasualCompletion:F1}%";
            HardcoreProgressText = $"{HardcoreCompletion:F1}%";
            EarnedAchievements = $"{progress.AchievementsEarned}";
            TotalAchievements = $"{progress.TotalAchievements}";
            TotalPointsEarned = $"{progress.PointsEarned:N0}";
            TruePointsEarned = $"{progress.PointsEarnedHardcore:N0}";

            HighestAwardKind = progress.HighestAwardKind?.ToLowerInvariant() switch
            {
                "mastered" => _resourceProvider.GetString("RaAwardMastered", "Mastered"),
                _ => string.IsNullOrWhiteSpace(progress.HighestAwardKind)
                    ? _resourceProvider.GetString("RaStatusNone", "None")
                    : CapitalizeFirstLetter(progress.HighestAwardKind)
            };

            IsMastered = progress.HighestAwardKind?.Equals("mastered", StringComparison.OrdinalIgnoreCase) == true;

            if (DateTime.TryParse(progress.HighestAwardDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var awardDate))
            {
                HighestAwardDate = awardDate.ToLocalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            else
            {
                HighestAwardDate = _resourceProvider.GetString("RaStatusNotApplicable", "N/A");
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Failed to parse progress data");
        }
    }

    private static string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return char.ToUpper(input[0], CultureInfo.InvariantCulture) + input[1..];
    }

    public static string FormatDateString(string dateString)
    {
        if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var date))
        {
            return date.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        }

        return dateString;
    }
}