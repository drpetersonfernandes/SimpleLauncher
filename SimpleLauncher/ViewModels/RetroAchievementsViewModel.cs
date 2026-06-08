#nullable enable

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.LoadingInterface;
using SimpleLauncher.Core.Services.RetroAchievements;
using SimpleLauncher.Core.Services.RetroAchievements.Models;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.PlaySound;

namespace SimpleLauncher.ViewModels;

public partial class RetroAchievementsViewModel : ObservableObject
{
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IResourceProvider _resourceProvider;
    private readonly SettingsManager _settings;
    private readonly RetroAchievementsService _raService;
    private readonly PlaySoundEffects _playSoundEffects;

    // Profile tab
    [ObservableProperty]
    private string? _profileImageUrl;

    [ObservableProperty]
    private string _profileUser = string.Empty;

    [ObservableProperty]
    private string _profileMotto = string.Empty;

    [ObservableProperty]
    private string _profileRichPresence = string.Empty;

    [ObservableProperty]
    private string _profileRank = string.Empty;

    [ObservableProperty]
    private string _profilePoints = string.Empty;

    [ObservableProperty]
    private string _profileTruePoints = string.Empty;

    [ObservableProperty]
    private string _profileMemberSince = string.Empty;

    [ObservableProperty]
    private string _profileId = string.Empty;

    [ObservableProperty]
    private string _profileContributions = string.Empty;

    [ObservableProperty]
    private string _profileSoftcorePoints = string.Empty;

    [ObservableProperty]
    private string _profilePermissions = string.Empty;

    [ObservableProperty]
    private string _profileStatus = string.Empty;

    [ObservableProperty]
    private string _profileProfileId = string.Empty;

    [ObservableProperty]
    private string _profileWallActive = string.Empty;

    [ObservableProperty]
    private ObservableCollection<RaRecentlyPlayedGame>? _recentlyPlayedGames;

    [ObservableProperty]
    private bool _noProfileVisible;

    [ObservableProperty]
    private string _noProfileMainMessage = string.Empty;

    [ObservableProperty]
    private string _noProfileSubMessage = string.Empty;

    // Unlocks tab
    [ObservableProperty]
    private ObservableCollection<RaEarnedAchievement>? _unlocks;

    [ObservableProperty]
    private string _totalUnlocksInRange = "0";

    [ObservableProperty]
    private string _totalPointsEarnedInRange = "0";

    [ObservableProperty]
    private bool _noUnlocksVisible;

    [ObservableProperty]
    private string _noUnlocksMessage = string.Empty;

    [ObservableProperty]
    private DateTime? _fromDate;

    [ObservableProperty]
    private DateTime? _toDate;

    [ObservableProperty]
    private bool _fetchUnlocksEnabled = true;

    // User Progress tab
    [ObservableProperty]
    private ObservableCollection<RaUserCompletionGame>? _userProgress;

    [ObservableProperty]
    private bool _noUserProgressVisible;

    [ObservableProperty]
    private string _noUserProgressMainMessage = string.Empty;

    [ObservableProperty]
    private string _noUserProgressSubMessage = string.Empty;

    // Loading state
    [ObservableProperty]
    private bool _isLoading;

    public RetroAchievementsViewModel(
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

        // Set default dates
        FromDate = DateTime.Today.AddMonths(-1);
        ToDate = DateTime.Today;
    }

    public async Task LoadUserProfileAsync()
    {
        IsLoading = true;
        NoProfileVisible = false;
        RecentlyPlayedGames = null;

        if (string.IsNullOrWhiteSpace(_settings.RaUsername) || string.IsNullOrWhiteSpace(_settings.RaApiKey))
        {
            NoProfileVisible = true;
            NoProfileMainMessage = _resourceProvider.GetString("RaErrorCredentialsNotSetShort", "RetroAchievements username or API key is not set.");
            NoProfileSubMessage = _resourceProvider.GetString("RaInfoConfigureCredentials", "Please configure your credentials in the RetroAchievements settings.");
            IsLoading = false;
            return;
        }

        try
        {
            DebugLogger.Log($"[RA VM] Fetching user profile for {_settings.RaUsername}...");
            var userProfile = await _raService.GetUserProfileAsync(_settings.RaUsername, _settings.RaApiKey);

            DebugLogger.Log($"[RA VM] Fetching recently played games for {_settings.RaUsername}...");
            var recentlyPlayedGames = await _raService.GetUserRecentlyPlayedGamesAsync(_settings.RaUsername, _settings.RaApiKey, 50);

            if (userProfile != null)
            {
                ProfileImageUrl = !string.IsNullOrEmpty(userProfile.UserPic)
                    ? $"https://retroachievements.org{userProfile.UserPic}"
                    : null;

                ProfileUser = userProfile.User;
                ProfileMotto = string.IsNullOrWhiteSpace(userProfile.Motto)
                    ? _resourceProvider.GetString("RaInfoNoMotto", "No motto set")
                    : userProfile.Motto;

                ProfileRichPresence = string.IsNullOrWhiteSpace(userProfile.RichPresenceMsg)
                    ? _resourceProvider.GetString("RaInfoNotCurrentlyPlaying", "Not currently playing")
                    : userProfile.RichPresenceMsg;

                var rankFormat = _resourceProvider.GetString("RaInfoRankFormat", "#{0}");
                ProfileRank = string.IsNullOrWhiteSpace(userProfile.Rank)
                    ? _resourceProvider.GetString("RaStatusNotApplicable", "N/A")
                    : string.Format(CultureInfo.InvariantCulture, rankFormat, userProfile.Rank);

                ProfilePoints = userProfile.TotalPoints.ToString("N0", CultureInfo.InvariantCulture);
                ProfileTruePoints = userProfile.TotalTruePoints.ToString("N0", CultureInfo.InvariantCulture);

                if (DateTime.TryParse(userProfile.MemberSince, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var memberSinceDate))
                {
                    ProfileMemberSince = memberSinceDate.ToLocalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                else
                {
                    ProfileMemberSince = string.IsNullOrWhiteSpace(userProfile.MemberSince)
                        ? _resourceProvider.GetString("RaStatusUnknown", "Unknown")
                        : userProfile.MemberSince;
                }

                ProfileId = userProfile.Id.ToString(CultureInfo.InvariantCulture);
                var contributionsFormat = _resourceProvider.GetString("RaInfoContributionsFormat", "{0} contributions ({1:N0} points)");
                ProfileContributions = string.Format(CultureInfo.InvariantCulture, contributionsFormat, userProfile.ContribCount, userProfile.ContribYield);
                ProfileSoftcorePoints = userProfile.TotalSoftcorePoints.ToString("N0", CultureInfo.InvariantCulture);
                ProfilePermissions = GetPermissionDescription(userProfile.Permissions);
                ProfileStatus = userProfile.Untracked == 1
                    ? _resourceProvider.GetString("RaStatusUntracked", "Untracked")
                    : _resourceProvider.GetString("RaStatusTracked", "Tracked");
                ProfileProfileId = string.IsNullOrWhiteSpace(userProfile.Uuid)
                    ? _resourceProvider.GetString("RaStatusNotApplicable", "N/A")
                    : userProfile.Uuid;
                ProfileWallActive = userProfile.UserWallActive
                    ? _resourceProvider.GetString("RaGenericYes", "Yes")
                    : _resourceProvider.GetString("RaGenericNo", "No");

                if (recentlyPlayedGames is { Count: > 0 })
                {
                    RecentlyPlayedGames = new ObservableCollection<RaRecentlyPlayedGame>(recentlyPlayedGames);
                }
                else
                {
                    RecentlyPlayedGames = null;
                }

                NoProfileVisible = false;
            }
            else
            {
                NoProfileVisible = true;
                NoProfileMainMessage = _resourceProvider.GetString("RaErrorFailedToLoadUserProfile", "Failed to load user profile.");
                NoProfileSubMessage = _resourceProvider.GetString("RaInfoCheckCredentials", "Please check your RetroAchievements credentials or try again later.");
            }
        }
        catch (RaUnauthorizedException)
        {
            NoProfileVisible = true;
            NoProfileMainMessage = _resourceProvider.GetString("RaErrorUnauthorized", "RetroAchievements credentials invalid. Please check your username and API key in settings.");
            NoProfileSubMessage = _resourceProvider.GetString("RaInfoConfigureCredentials", "Please configure your credentials in the RetroAchievements settings.");
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

        if (string.IsNullOrWhiteSpace(_settings.RaUsername) || string.IsNullOrWhiteSpace(_settings.RaApiKey))
        {
            NoUnlocksVisible = true;
            NoUnlocksMessage = _resourceProvider.GetString("RaErrorCredentialsNotSet", "RetroAchievements username or API key is not set. Configure in settings.");
            IsLoading = false;
            FetchUnlocksEnabled = true;
            return;
        }

        var fromDate = FromDate ?? DateTime.Today.AddMonths(-1);
        var toDate = ToDate ?? DateTime.Today;

        try
        {
            DebugLogger.Log($"[RA VM] Fetching unlocks for {_settings.RaUsername} from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}...");
            var unlocks = await _raService.GetAchievementsEarnedBetweenAsync(_settings.RaUsername, _settings.RaApiKey, fromDate, toDate);

            if (unlocks is { Count: > 0 })
            {
                Unlocks = new ObservableCollection<RaEarnedAchievement>(unlocks);
                TotalUnlocksInRange = unlocks.Count.ToString("N0", CultureInfo.InvariantCulture);
                TotalPointsEarnedInRange = unlocks.Sum(a => a.Points).ToString("N0", CultureInfo.InvariantCulture);
                NoUnlocksVisible = false;
            }
            else
            {
                Unlocks = null;
                TotalUnlocksInRange = "0";
                TotalPointsEarnedInRange = "0";
                NoUnlocksVisible = true;
                NoUnlocksMessage = unlocks == null
                    ? _resourceProvider.GetString("RaErrorFailedToLoadUnlocks", "Failed to load unlocks. Please check your RetroAchievements credentials or try again later.")
                    : _resourceProvider.GetString("RaInfoNoUnlocksFound", "No unlocks found for the selected date range.");
            }
        }
        catch (RaUnauthorizedException)
        {
            NoUnlocksVisible = true;
            NoUnlocksMessage = _resourceProvider.GetString("RaErrorUnauthorized", "RetroAchievements credentials invalid. Please check your username and API key in settings.");
        }
        catch (Exception ex)
        {
            Unlocks = null;
            TotalUnlocksInRange = "0";
            TotalPointsEarnedInRange = "0";
            NoUnlocksVisible = true;
            NoUnlocksMessage = _resourceProvider.GetString("RaErrorLoadingUnlocks", "An error occurred while loading unlocks. Please try again.");
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
    private async Task ResetDatesAsync()
    {
        FromDate = DateTime.Today.AddMonths(-1);
        ToDate = DateTime.Today;
        Unlocks = null;
        TotalUnlocksInRange = "0";
        TotalPointsEarnedInRange = "0";
        NoUnlocksVisible = true;
        NoUnlocksMessage = _resourceProvider.GetString("RaInfoNoUnlocksFound", "No unlocks found for the selected date range.");

        await LoadUnlocksByDateAsync();
    }

    public async Task LoadUserProgressAsync()
    {
        IsLoading = true;
        NoUserProgressVisible = false;
        UserProgress = null;

        if (string.IsNullOrWhiteSpace(_settings.RaUsername) || string.IsNullOrWhiteSpace(_settings.RaApiKey))
        {
            NoUserProgressVisible = true;
            NoUserProgressMainMessage = _resourceProvider.GetString("RaErrorCredentialsNotSetShort", "RetroAchievements username or API key is not set.");
            NoUserProgressSubMessage = _resourceProvider.GetString("RaInfoConfigureCredentials", "Please configure your credentials in the RetroAchievements settings.");
            IsLoading = false;
            return;
        }

        try
        {
            var userProgressList = await _raService.GetUserCompletionProgressAsync(_settings.RaUsername, _settings.RaApiKey);

            if (userProgressList is { Count: > 0 })
            {
                UserProgress = new ObservableCollection<RaUserCompletionGame>(userProgressList);
                NoUserProgressVisible = false;
            }
            else
            {
                UserProgress = null;
                NoUserProgressVisible = true;
                if (userProgressList == null)
                {
                    NoUserProgressMainMessage = _resourceProvider.GetString("RaErrorFailedToLoadUserProgress", "Failed to load user completion progress.");
                    NoUserProgressSubMessage = _resourceProvider.GetString("RaInfoCheckCredentials", "Please check your RetroAchievements credentials or try again later.");
                }
                else
                {
                    NoUserProgressMainMessage = _resourceProvider.GetString("RaInfoNoUserProgressFound", "No user completion progress found.");
                    NoUserProgressSubMessage = _resourceProvider.GetString("RaInfoNoUserProgressSubMessage", "This could be because you haven't played any games yet.");
                }
            }
        }
        catch (RaUnauthorizedException)
        {
            NoUserProgressVisible = true;
            NoUserProgressMainMessage = _resourceProvider.GetString("RaErrorUnauthorized", "RetroAchievements credentials invalid. Please check your username and API key in settings.");
            NoUserProgressSubMessage = _resourceProvider.GetString("RaInfoConfigureCredentials", "Please configure your credentials in the RetroAchievements settings.");
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
}