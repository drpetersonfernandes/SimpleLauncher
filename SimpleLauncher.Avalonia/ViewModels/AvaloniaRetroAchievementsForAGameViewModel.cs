using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.PlaySound;
using SimpleLauncher.Core.Services.RetroAchievements;
using SimpleLauncher.Core.Services.RetroAchievements.Models;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class AvaloniaRetroAchievementsForAGameViewModel : ObservableObject
{
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IResourceProvider _resourceProvider;
    private readonly SettingsManager _settings;
    private readonly RetroAchievementsService _raService;
    private readonly IPlaySoundEffects _playSoundEffects;
    private int _gameId;
    private string _gameTitleForDisplay = string.Empty;

    [ObservableProperty] private string _gameTitle = string.Empty;
    [ObservableProperty] private string _consoleName = string.Empty;
    [ObservableProperty] private string? _gameIconUrl;
    [ObservableProperty] private string? _gameIngameImageUrl;
    [ObservableProperty] private string? _gameTitleImageUrl;
    [ObservableProperty] private string? _gameBoxArtUrl;
    [ObservableProperty] private string? _gamePublisher;
    [ObservableProperty] private string? _gameDeveloper;
    [ObservableProperty] private string? _gameGenre;
    [ObservableProperty] private string? _gameReleased;
    [ObservableProperty] private string? _gameConsoleName;
    [ObservableProperty] private int _gamePlayers;
    [ObservableProperty] private int _gameAchievementCount;
    [ObservableProperty] private string? _gameForumTopic;
    [ObservableProperty] private string? _gameUpdated;
    [ObservableProperty] private int _gameConsoleId;
    [ObservableProperty] private int _gameInfoId;
    [ObservableProperty] private string? _gameParentGame;
    [ObservableProperty] private string? _gameReleaseGranularity;
    [ObservableProperty] private string? _gameGuideUrl;
    [ObservableProperty] private int _distinctPlayers;
    [ObservableProperty] private int _casualPlayers;
    [ObservableProperty] private int _hardcorePlayers;
    [ObservableProperty] private string _gameClaims = string.Empty;
    [ObservableProperty] private ObservableCollection<RaAchievement>? _gameInfoAchievements;
    [ObservableProperty] private bool _isGameInfoLoaded;
    [ObservableProperty] private bool _noGameInfoVisible;
    [ObservableProperty] private string _noGameInfoMessage = string.Empty;

    [ObservableProperty] private ObservableCollection<RaAchievement>? _achievements;
    [ObservableProperty] private bool _noAchievementsVisible;
    [ObservableProperty] private string _noAchievementsMessage = string.Empty;

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

    [ObservableProperty] private ObservableCollection<RaGameRankAndScore>? _highScores;
    [ObservableProperty] private ObservableCollection<RaGameRankAndScore>? _latestMasters;
    [ObservableProperty] private string _userRank = string.Empty;
    [ObservableProperty] private string _userScore = string.Empty;
    [ObservableProperty] private string _userLastAward = string.Empty;
    [ObservableProperty] private bool _noRankingVisible;
    [ObservableProperty] private string _noRankingMessage = string.Empty;
    [ObservableProperty] private bool _noLatestMastersVisible;
    [ObservableProperty] private string _noLatestMastersMessage = string.Empty;
    [ObservableProperty] private bool _noHighScoresVisible;
    [ObservableProperty] private string _noHighScoresMessage = string.Empty;
    [ObservableProperty] private bool _noUserRankVisible;
    [ObservableProperty] private string _noUserRankMessage = string.Empty;

    [ObservableProperty] private string? _profileImageUrl;
    [ObservableProperty] private string _profileUser = string.Empty;
    [ObservableProperty] private string _profileMotto = string.Empty;
    [ObservableProperty] private string _profileRichPresence = string.Empty;
    [ObservableProperty] private string _profileRank = string.Empty;
    [ObservableProperty] private string _profilePoints = string.Empty;
    [ObservableProperty] private string _profileTruePoints = string.Empty;
    [ObservableProperty] private string _profileMemberSince = string.Empty;
    [ObservableProperty] private string _profileId = string.Empty;
    [ObservableProperty] private string _profileContributions = string.Empty;
    [ObservableProperty] private string _profileSoftcorePoints = string.Empty;
    [ObservableProperty] private string _profilePermissions = string.Empty;
    [ObservableProperty] private string _profileStatus = string.Empty;
    [ObservableProperty] private string _profileProfileId = string.Empty;
    [ObservableProperty] private string _profileWallActive = string.Empty;
    [ObservableProperty] private ObservableCollection<RaRecentlyPlayedGame>? _recentlyPlayedGames;
    [ObservableProperty] private bool _noProfileVisible;
    [ObservableProperty] private string _noProfileMainMessage = string.Empty;
    [ObservableProperty] private string _noProfileSubMessage = string.Empty;

    [ObservableProperty] private ObservableCollection<RaEarnedAchievement>? _unlocks;
    [ObservableProperty] private string _totalUnlocksInRange = "0";
    [ObservableProperty] private string _totalPointsEarnedInRange = "0";
    [ObservableProperty] private bool _noUnlocksVisible;
    [ObservableProperty] private string _noUnlocksMessage = string.Empty;
    [ObservableProperty] private DateTime? _fromDate;
    [ObservableProperty] private DateTime? _toDate;
    [ObservableProperty] private bool _fetchUnlocksEnabled = true;

    [ObservableProperty] private ObservableCollection<RaUserCompletionGame>? _userProgress;
    [ObservableProperty] private bool _noUserProgressVisible;
    [ObservableProperty] private string _noUserProgressMainMessage = string.Empty;
    [ObservableProperty] private string _noUserProgressSubMessage = string.Empty;

    [ObservableProperty] private bool _isLoading;

    public AvaloniaRetroAchievementsForAGameViewModel(
        ILogErrors logErrors,
        IMessageBoxLibraryService messageBox,
        IResourceProvider resourceProvider,
        SettingsManager settings,
        RetroAchievementsService raService,
        IPlaySoundEffects playSoundEffects)
    {
        _logErrors = logErrors;
        _messageBox = messageBox;
        _resourceProvider = resourceProvider;
        _settings = settings;
        _raService = raService;
        _playSoundEffects = playSoundEffects;

        FromDate = DateTime.Today.AddMonths(-1);
        ToDate = DateTime.Today;
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
                    ? _resourceProvider.GetString("RaErrorFailedToLoadAchievements", "Failed to load achievements.")
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
        NoGameInfoVisible = false;
        GameInfoAchievements = null;

        if (!CheckCredentials())
        {
            NoGameInfoVisible = true;
            NoGameInfoMessage = _resourceProvider.GetString("RaErrorCredentialsNotSet", "RetroAchievements username or API key is not set.");
            IsLoading = false;
            return;
        }

        try
        {
            var gameInfo = await _raService.GetGameExtendedAsync(_gameId, _settings.RaUsername!, _settings.RaApiKey!);
            if (gameInfo != null)
            {
                GameIconUrl = !string.IsNullOrEmpty(gameInfo.ImageIcon) ? $"https://retroachievements.org{gameInfo.ImageIcon}" : null;
                GameTitleImageUrl = !string.IsNullOrEmpty(gameInfo.ImageTitle) ? $"https://retroachievements.org{gameInfo.ImageTitle}" : null;
                GameIngameImageUrl = !string.IsNullOrEmpty(gameInfo.ImageIngame) ? $"https://retroachievements.org{gameInfo.ImageIngame}" : null;
                GameBoxArtUrl = !string.IsNullOrEmpty(gameInfo.ImageBoxArt) ? $"https://retroachievements.org{gameInfo.ImageBoxArt}" : null;
                GamePublisher = gameInfo.Publisher;
                GameDeveloper = gameInfo.Developer;
                GameGenre = gameInfo.Genre;
                GameReleased = gameInfo.Released;
                GameConsoleName = gameInfo.ConsoleName;
                GamePlayers = gameInfo.NumDistinctPlayers;
                GameAchievementCount = gameInfo.NumAchievements;
                GameForumTopic = gameInfo.ForumTopicId?.ToString(CultureInfo.InvariantCulture);
                GameUpdated = !string.IsNullOrWhiteSpace(gameInfo.Updated) ? FormatDateString(gameInfo.Updated) : null;
                GameConsoleId = gameInfo.ConsoleId;
                GameInfoId = gameInfo.Id;
                GameParentGame = gameInfo.ParentGameId?.ToString(CultureInfo.InvariantCulture);
                GameReleaseGranularity = gameInfo.ReleasedAtGranularity;
                GameGuideUrl = gameInfo.GuideUrl;
                DistinctPlayers = gameInfo.NumDistinctPlayers;
                CasualPlayers = gameInfo.NumDistinctPlayersCasual;
                HardcorePlayers = gameInfo.NumDistinctPlayersHardcore;

                var claimsFormat = _resourceProvider.GetString("RaInfoActiveClaimsCount", "{0} active development claim(s)");
                GameClaims = gameInfo.Claims.Count == 0
                    ? _resourceProvider.GetString("RaInfoNoActiveClaims", "No active development claims")
                    : string.Format(CultureInfo.InvariantCulture, claimsFormat, gameInfo.Claims.Count);

                if (gameInfo.Achievements is { Count: > 0 })
                {
                    var achievementsList = gameInfo.Achievements.Values
                        .OrderBy(static a => a.DisplayOrder)
                        .Select(static a => new RaAchievement
                        {
                            Id = a.Id,
                            Title = a.Title,
                            Description = a.Description,
                            Points = a.Points,
                            BadgeUri = a.BadgeUri,
                            TrueRatio = a.TrueRatio,
                            Author = a.Author,
                            DateCreated = a.DateCreated
                        })
                        .ToList();
                    GameInfoAchievements = new ObservableCollection<RaAchievement>(achievementsList);
                }

                IsGameInfoLoaded = true;
                NoGameInfoVisible = false;
            }
            else
            {
                NoGameInfoVisible = true;
                NoGameInfoMessage = _resourceProvider.GetString("RaErrorFailedToLoadGameInfo", "Failed to load extended game information.");
            }
        }
        catch (RaUnauthorizedException)
        {
            NoGameInfoVisible = true;
            NoGameInfoMessage = _resourceProvider.GetString("RaErrorUnauthorized", "RetroAchievements credentials invalid.");
        }
        catch (Exception ex)
        {
            NoGameInfoVisible = true;
            NoGameInfoMessage = _resourceProvider.GetString("RaErrorLoadingGameInfo", "An error occurred while loading game info.");
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
        NoLatestMastersVisible = false;
        NoHighScoresVisible = false;
        NoUserRankVisible = false;
        HighScores = null;
        LatestMasters = null;
        UserRank = _resourceProvider.GetString("RaStatusNotApplicable", "N/A");
        UserScore = _resourceProvider.GetString("RaStatusNotApplicable", "N/A");
        UserLastAward = _resourceProvider.GetString("RaStatusNotApplicable", "N/A");

        if (!CheckCredentials())
        {
            NoRankingVisible = true;
            NoRankingMessage = _resourceProvider.GetString("RaErrorCredentialsNotSet", "RetroAchievements username or API key is not set.");
            NoLatestMastersVisible = true;
            NoLatestMastersMessage = NoRankingMessage;
            NoHighScoresVisible = true;
            NoHighScoresMessage = NoRankingMessage;
            NoUserRankVisible = true;
            NoUserRankMessage = NoRankingMessage;
            IsLoading = false;
            return;
        }

        try
        {
            var latestMasters = await _raService.GetGameRankAndScoreAsync(_gameId, _settings.RaUsername!, _settings.RaApiKey!, true);
            if (latestMasters is { Count: > 0 })
            {
                for (var i = 0; i < latestMasters.Count; i++)
                {
                    latestMasters[i].Rank = i + 1;
                }

                LatestMasters = new ObservableCollection<RaGameRankAndScore>(latestMasters);
                NoLatestMastersVisible = false;
            }
            else
            {
                NoLatestMastersVisible = true;
                NoLatestMastersMessage = latestMasters == null
                    ? _resourceProvider.GetString("RaErrorFailedToLoadLatestMasters", "Failed to load latest masters.")
                    : _resourceProvider.GetString("RaInfoNoLatestMasters", "No latest masters found for this game.");
            }

            var rankings = await _raService.GetGameRankAndScoreAsync(_gameId, _settings.RaUsername!, _settings.RaApiKey!);
            if (rankings is { Count: > 0 })
            {
                for (var i = 0; i < rankings.Count; i++)
                {
                    rankings[i].Rank = i + 1;
                }

                HighScores = new ObservableCollection<RaGameRankAndScore>(rankings);
                NoHighScoresVisible = false;
            }
            else
            {
                NoHighScoresVisible = true;
                NoHighScoresMessage = rankings == null
                    ? _resourceProvider.GetString("RaErrorFailedToLoadHighScores", "Failed to load high scores.")
                    : _resourceProvider.GetString("RaInfoNoHighScores", "No high scores found for this game.");
            }

            var userGameRankAndScoreList = await _raService.GetUserGameRankAndScoreAsync(_gameId, _settings.RaUsername!, _settings.RaApiKey!);
            if (userGameRankAndScoreList is { Count: > 0 })
            {
                var userData = userGameRankAndScoreList.First();
                UserRank = userData.UserRank is null or 0
                    ? _resourceProvider.GetString("RaStatusUnranked", "Unranked")
                    : userData.UserRank.Value.ToString(CultureInfo.InvariantCulture);
                UserScore = userData.TotalScore.ToString("N0", CultureInfo.InvariantCulture);
                UserLastAward = string.IsNullOrWhiteSpace(userData.LastAward)
                    ? _resourceProvider.GetString("RaStatusNotApplicable", "N/A")
                    : userData.LastAward;
                NoUserRankVisible = false;
            }
            else
            {
                UserRank = _resourceProvider.GetString("RaStatusUnranked", "Unranked");
                UserScore = "0";
                UserLastAward = _resourceProvider.GetString("RaStatusNotApplicable", "N/A");
                NoUserRankVisible = true;
                NoUserRankMessage = userGameRankAndScoreList == null
                    ? _resourceProvider.GetString("RaErrorFailedToLoadUserRank", "Failed to load your rank data.")
                    : _resourceProvider.GetString("RaInfoNoRankDataForGame", "No rank data available for this game.");
            }

            NoRankingVisible = false;
        }
        catch (RaUnauthorizedException)
        {
            var unauthorizedMessage = _resourceProvider.GetString("RaErrorUnauthorized", "RetroAchievements credentials invalid.");
            NoRankingVisible = true;
            NoRankingMessage = unauthorizedMessage;
            NoLatestMastersVisible = true;
            NoLatestMastersMessage = unauthorizedMessage;
            NoHighScoresVisible = true;
            NoHighScoresMessage = unauthorizedMessage;
            NoUserRankVisible = true;
            NoUserRankMessage = unauthorizedMessage;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Failed to load game ranking for game ID: {_gameId}");
            NoRankingVisible = true;
            NoRankingMessage = _resourceProvider.GetString("RaErrorLoadingRankingData", "Error loading ranking data.");
            NoLatestMastersVisible = true;
            NoLatestMastersMessage = _resourceProvider.GetString("RaErrorLoadingLatestMasters", "Error loading latest masters.");
            NoHighScoresVisible = true;
            NoHighScoresMessage = _resourceProvider.GetString("RaErrorLoadingHighScores", "Error loading high scores.");
            NoUserRankVisible = true;
            NoUserRankMessage = _resourceProvider.GetString("RaErrorLoadingUserRank", "Error loading your rank data.");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadUserProfileAsync()
    {
        IsLoading = true;
        NoProfileVisible = false;
        RecentlyPlayedGames = null;

        if (!CheckCredentials())
        {
            NoProfileVisible = true;
            NoProfileMainMessage = _resourceProvider.GetString("RaErrorCredentialsNotSetShort", "RetroAchievements username or API key is not set.");
            NoProfileSubMessage = _resourceProvider.GetString("RaInfoConfigureCredentials", "Please configure your credentials in the RetroAchievements settings.");
            IsLoading = false;
            return;
        }

        try
        {
            var userProfile = await _raService.GetUserProfileAsync(_settings.RaUsername!, _settings.RaApiKey!);
            var recentlyPlayedGames = await _raService.GetUserRecentlyPlayedGamesAsync(_settings.RaUsername!, _settings.RaApiKey!, 50);

            if (userProfile != null)
            {
                ProfileImageUrl = !string.IsNullOrEmpty(userProfile.UserPic) ? $"https://retroachievements.org{userProfile.UserPic}" : null;
                ProfileUser = userProfile.User;
                ProfileMotto = string.IsNullOrWhiteSpace(userProfile.Motto) ? _resourceProvider.GetString("RaInfoNoMotto", "No motto set") : userProfile.Motto;
                ProfileRichPresence = string.IsNullOrWhiteSpace(userProfile.RichPresenceMsg) ? _resourceProvider.GetString("RaInfoNotCurrentlyPlaying", "Not currently playing") : userProfile.RichPresenceMsg;

                var rankFormat = _resourceProvider.GetString("RaInfoRankFormat", "#{0}");
                ProfileRank = string.IsNullOrWhiteSpace(userProfile.Rank) ? _resourceProvider.GetString("RaStatusNotApplicable", "N/A") : string.Format(CultureInfo.InvariantCulture, rankFormat, userProfile.Rank);
                ProfilePoints = userProfile.TotalPoints.ToString("N0", CultureInfo.InvariantCulture);
                ProfileTruePoints = userProfile.TotalTruePoints.ToString("N0", CultureInfo.InvariantCulture);

                if (DateTime.TryParse(userProfile.MemberSince, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var memberSinceDate))
                {
                    ProfileMemberSince = memberSinceDate.ToLocalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                else
                {
                    ProfileMemberSince = string.IsNullOrWhiteSpace(userProfile.MemberSince) ? _resourceProvider.GetString("RaStatusUnknown", "Unknown") : userProfile.MemberSince;
                }

                ProfileId = userProfile.Id.ToString(CultureInfo.InvariantCulture);
                var contributionsFormat = _resourceProvider.GetString("RaInfoContributionsFormat", "{0} contributions ({1:N0} points)");
                ProfileContributions = string.Format(CultureInfo.InvariantCulture, contributionsFormat, userProfile.ContribCount, userProfile.ContribYield);
                ProfileSoftcorePoints = userProfile.TotalSoftcorePoints.ToString("N0", CultureInfo.InvariantCulture);
                ProfilePermissions = GetPermissionDescription(userProfile.Permissions);
                ProfileStatus = userProfile.Untracked == 1 ? _resourceProvider.GetString("RaStatusUntracked", "Untracked") : _resourceProvider.GetString("RaStatusTracked", "Tracked");
                ProfileProfileId = string.IsNullOrWhiteSpace(userProfile.Uuid) ? _resourceProvider.GetString("RaStatusNotApplicable", "N/A") : userProfile.Uuid;
                ProfileWallActive = userProfile.UserWallActive ? _resourceProvider.GetString("RaGenericYes", "Yes") : _resourceProvider.GetString("RaGenericNo", "No");

                if (recentlyPlayedGames is { Count: > 0 })
                {
                    RecentlyPlayedGames = new ObservableCollection<RaRecentlyPlayedGame>(recentlyPlayedGames);
                }

                NoProfileVisible = false;
            }
            else
            {
                NoProfileVisible = true;
                NoProfileMainMessage = _resourceProvider.GetString("RaErrorFailedToLoadUserProfile", "Failed to load user profile.");
                NoProfileSubMessage = _resourceProvider.GetString("RaInfoCheckCredentials", "Please check your credentials or try again later.");
            }
        }
        catch (RaUnauthorizedException)
        {
            NoProfileVisible = true;
            NoProfileMainMessage = _resourceProvider.GetString("RaErrorUnauthorized", "RetroAchievements credentials invalid.");
            NoProfileSubMessage = _resourceProvider.GetString("RaInfoConfigureCredentials", "Please configure your credentials in settings.");
        }
        catch (Exception ex)
        {
            NoProfileVisible = true;
            NoProfileMainMessage = _resourceProvider.GetString("RaErrorLoadingUserProfile", "An error occurred while loading user profile.");
            NoProfileSubMessage = _resourceProvider.GetString("RaInfoCheckConnection", "Please try again or check your internet connection.");
            _logErrors.LogAndForget(ex, $"Failed to load user profile for {_settings.RaUsername}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task LoadUnlocksByDateAsync()
    {
        IsLoading = true;
        FetchUnlocksEnabled = false;
        NoUnlocksVisible = false;
        Unlocks = null;
        TotalUnlocksInRange = "0";
        TotalPointsEarnedInRange = "0";

        if (!CheckCredentials())
        {
            NoUnlocksVisible = true;
            NoUnlocksMessage = _resourceProvider.GetString("RaErrorCredentialsNotSet", "RetroAchievements username or API key is not set.");
            IsLoading = false;
            FetchUnlocksEnabled = true;
            return;
        }

        var fromDate = FromDate ?? DateTime.Today.AddMonths(-1);
        var toDate = ToDate ?? DateTime.Today;

        try
        {
            var unlocks = await _raService.GetAchievementsEarnedBetweenAsync(_settings.RaUsername!, _settings.RaApiKey!, fromDate, toDate);

            if (unlocks is { Count: > 0 })
            {
                Unlocks = new ObservableCollection<RaEarnedAchievement>(unlocks);
                TotalUnlocksInRange = unlocks.Count.ToString("N0", CultureInfo.InvariantCulture);
                TotalPointsEarnedInRange = unlocks.Sum(static a => a.Points).ToString("N0", CultureInfo.InvariantCulture);
            }
            else
            {
                NoUnlocksVisible = true;
                NoUnlocksMessage = unlocks == null
                    ? _resourceProvider.GetString("RaErrorFailedToLoadUnlocks", "Failed to load unlocks.")
                    : _resourceProvider.GetString("RaInfoNoUnlocksFound", "No unlocks found for the selected date range.");
            }
        }
        catch (RaUnauthorizedException)
        {
            NoUnlocksVisible = true;
            NoUnlocksMessage = _resourceProvider.GetString("RaErrorUnauthorized", "RetroAchievements credentials invalid.");
        }
        catch (Exception ex)
        {
            NoUnlocksVisible = true;
            NoUnlocksMessage = _resourceProvider.GetString("RaErrorLoadingUnlocks", "An error occurred while loading unlocks.");
            _logErrors.LogAndForget(ex, $"Failed to load unlocks by date for user {_settings.RaUsername}");
        }
        finally
        {
            IsLoading = false;
            FetchUnlocksEnabled = true;
        }
    }

    [RelayCommand]
    private async Task FetchUnlocksAsync()
    {
        var fromDate = FromDate ?? DateTime.Today.AddMonths(-1);
        var toDate = ToDate ?? DateTime.Today;

        if (fromDate > toDate)
        {
            await _messageBox.ErrorMessageBox();
            return;
        }

        await LoadUnlocksByDateAsync();
    }

    [RelayCommand]
    private Task ResetDatesAsync()
    {
        FromDate = DateTime.Today.AddMonths(-1);
        ToDate = DateTime.Today;
        Unlocks = null;
        TotalUnlocksInRange = "0";
        TotalPointsEarnedInRange = "0";
        NoUnlocksVisible = true;
        NoUnlocksMessage = _resourceProvider.GetString("RaInfoNoUnlocksFound", "No unlocks found for the selected date range.");

        return LoadUnlocksByDateAsync();
    }

    public async Task LoadUserProgressAsync()
    {
        IsLoading = true;
        NoUserProgressVisible = false;
        UserProgress = null;

        if (!CheckCredentials())
        {
            NoUserProgressVisible = true;
            NoUserProgressMainMessage = _resourceProvider.GetString("RaErrorCredentialsNotSetShort", "RetroAchievements username or API key is not set.");
            NoUserProgressSubMessage = _resourceProvider.GetString("RaInfoConfigureCredentials", "Please configure your credentials in settings.");
            IsLoading = false;
            return;
        }

        try
        {
            var userProgressList = await _raService.GetUserCompletionProgressAsync(_settings.RaUsername!, _settings.RaApiKey!);

            if (userProgressList is { Count: > 0 })
            {
                UserProgress = new ObservableCollection<RaUserCompletionGame>(userProgressList);
            }
            else
            {
                NoUserProgressVisible = true;
                NoUserProgressMainMessage = userProgressList == null
                    ? _resourceProvider.GetString("RaErrorFailedToLoadUserProgress", "Failed to load user completion progress.")
                    : _resourceProvider.GetString("RaInfoNoUserProgressFound", "No user completion progress found.");
                NoUserProgressSubMessage = userProgressList == null
                    ? _resourceProvider.GetString("RaInfoCheckCredentials", "Please check your credentials or try again later.")
                    : _resourceProvider.GetString("RaInfoNoUserProgressSubMessage", "This could be because you haven't played any games yet.");
            }
        }
        catch (RaUnauthorizedException)
        {
            NoUserProgressVisible = true;
            NoUserProgressMainMessage = _resourceProvider.GetString("RaErrorUnauthorized", "RetroAchievements credentials invalid.");
            NoUserProgressSubMessage = _resourceProvider.GetString("RaInfoConfigureCredentials", "Please configure your credentials in settings.");
        }
        catch (Exception ex)
        {
            NoUserProgressVisible = true;
            NoUserProgressMainMessage = _resourceProvider.GetString("RaErrorLoadingUserProgress", "An error occurred while loading user completion progress.");
            NoUserProgressSubMessage = _resourceProvider.GetString("RaInfoCheckConnection", "Please try again or check your internet connection.");
            _logErrors.LogAndForget(ex, $"Failed to load user completion progress for user {_settings.RaUsername}");
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

    private string GetPermissionDescription(int permissions)
    {
        return permissions switch
        {
            0 => _resourceProvider.GetString("RaPermissionUnregistered", "Unregistered"),
            1 => _resourceProvider.GetString("RaPermissionRegistered", "Registered"),
            2 => _resourceProvider.GetString("RaPermissionJuniorDeveloper", "Junior Developer"),
            3 => _resourceProvider.GetString("RaPermissionDeveloper", "Developer"),
            4 => _resourceProvider.GetString("RaPermissionAdmin", "Admin"),
            _ => $"{_resourceProvider.GetString("RaStatusUnknown", "Unknown")} ({permissions})"
        };
    }

    private static string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return input;

        return char.ToUpper(input[0], CultureInfo.InvariantCulture) + input[1..];
    }

    private static string FormatDateString(string dateString)
    {
        if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var date))
            return date.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

        return dateString;
    }
}
