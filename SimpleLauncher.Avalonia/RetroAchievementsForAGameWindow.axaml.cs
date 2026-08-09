using System.Diagnostics;
using System.Globalization;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Avalonia.Controls;
using SimpleLauncher.Avalonia.Models;
using SimpleLauncher.Avalonia.Services;
using SimpleLauncher.Avalonia.Services.RetroAchievements;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.PlaySound;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia;

/// <summary>
/// Window displaying RetroAchievements data for a specific game, including achievements, rankings, and progress.
/// </summary>
public partial class RetroAchievementsForAGameWindow : Window, ILoadingState
{
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly ILogger _logger;

    private int _gameId;
    private string _gameTitleForDisplay = "";
    private readonly SettingsManagerService _settings;
    private readonly RetroAchievementsService _raService;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetroAchievementsForAGameWindow"/> class.
    /// </summary>
    /// <param name="playSoundEffects">The sound effects service.</param>
    /// <param name="settings">The application settings manager.</param>
    /// <param name="raService">The RetroAchievements API service.</param>
    /// <param name="logger">The error logging service.</param>
    public RetroAchievementsForAGameWindow(PlaySoundEffects playSoundEffects, SettingsManagerService settings, RetroAchievementsService raService, ILogger logger)
    {
        InitializeComponent();

        _settings = settings;
        _raService = raService;
        _playSoundEffects = playSoundEffects;
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _messageBox = App.ServiceProvider.GetRequiredService<IMessageBoxLibraryService>();

        Opened += AchievementsWindow_Opened;
    }

    /// <summary>
    /// Initializes the window with the specified game ID and display title.
    /// </summary>
    /// <param name="gameId">The RetroAchievements game ID.</param>
    /// <param name="gameTitleForDisplay">The game title to display in the window.</param>
    public void Initialize(int gameId, string gameTitleForDisplay)
    {
        _gameId = gameId;
        _gameTitleForDisplay = gameTitleForDisplay;
    }

    private void AchievementsWindow_Opened(object? sender, EventArgs e)
    {
        try
        {
            GameTitleTextBlock.Text = _gameTitleForDisplay;
            // Force load the first tab's data
            // The SelectionChanged event might not fire if the first tab is already selected
            _ = LoadGameAchievementsAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to initialize RetroAchievementsForAGameWindow.");
        }
    }

    private void TabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.Source is not TabControl { SelectedItem: TabItem selectedTab })
                return;

            if (!selectedTab.IsSelected) return;

            switch (selectedTab.Tag?.ToString())
            {
                case "Achievements":
                    _playSoundEffects.PlayNotificationSound();
                    _ = LoadGameAchievementsAsync();
                    break;
                case "GameInfo":
                    _playSoundEffects.PlayNotificationSound();
                    _ = LoadGameInfoAsync();
                    break;
                case "GameRanking":
                    _playSoundEffects.PlayNotificationSound();
                    _ = LoadGameRankingAsync();
                    break;
                case "MyProfile":
                    _playSoundEffects.PlayNotificationSound();
                    _ = LoadUserProfileAsync();
                    break;
                case "Unlocks":
                    _playSoundEffects.PlayNotificationSound();
                    _ = LoadUnlocksByDateAsync();
                    break;
                case "UserProgress":
                    _playSoundEffects.PlayNotificationSound();
                    _ = LoadUserProgressAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in TabControl_SelectionChanged of RetroAchievementsForAGameWindow.");
        }
    }

    private void UpdateProgressDisplay(RaUserGameProgress progress)
    {
        try
        {
            // Parse completion percentages
            double casualCompletion = 0;
            double hardcoreCompletion = 0;

            if (!string.IsNullOrWhiteSpace(progress.UserCompletion))
            {
                var casualText = progress.UserCompletion.Replace("%", "").Trim();
                if (!double.TryParse(casualText, NumberStyles.Float, CultureInfo.InvariantCulture, out casualCompletion))
                {
                    _logger.Warning($"Failed to parse casual completion percentage: '{casualText}' (original: '{progress.UserCompletion}')");
                }
            }

            if (!string.IsNullOrWhiteSpace(progress.UserCompletionHardcore))
            {
                var hardcoreText = progress.UserCompletionHardcore.Replace("%", "").Trim();
                if (!double.TryParse(hardcoreText, NumberStyles.Float, CultureInfo.InvariantCulture, out hardcoreCompletion))
                {
                    _logger.Warning($"Failed to parse hardcore completion percentage: '{hardcoreText}' (original: '{progress.UserCompletionHardcore}')");
                }
            }

            // Update progress bars
            CasualProgressbar.Value = casualCompletion;
            HardcoreProgressbar.Value = hardcoreCompletion;

            // Update progress text
            CasualProgressText.Text = $"{casualCompletion:F1}%";
            HardcoreProgressText.Text = $"{hardcoreCompletion:F1}%";

            // Update achievement stats
            EarnedAchievementsValue.Text = $"{progress.AchievementsEarned}";
            TotalAchievementsValue.Text = $"{progress.TotalAchievements}";
            TotalPointsEarnedValue.Text = $"{progress.PointsEarned:N0}";
            TruePointsEarnedValue.Text = $"{progress.PointsEarnedHardcore:N0}";

            // Update highest award info
            string awardKindDisplay;
            switch (progress.HighestAwardKind?.ToLowerInvariant())
            {
                case "mastered":
                    awardKindDisplay = "🏆 Mastered";
                    break;
                default:
                    awardKindDisplay = string.IsNullOrWhiteSpace(progress.HighestAwardKind) ? "None" : CapitalizeFirstLetter(progress.HighestAwardKind);
                    break;
            }

            HighestAwardKindText.Text = awardKindDisplay;

            // Highest award icon (mastered shows a trophy glyph prefix instead of a bitmap)
            HighestAwardIcon.IsVisible = progress.HighestAwardKind?.Equals("mastered", StringComparison.OrdinalIgnoreCase) == true;

            if (DateTime.TryParse(progress.HighestAwardDate, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var awardDate))
            {
                HighestAwardDateText.Text = awardDate.ToLocalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            }
            else
            {
                HighestAwardDateText.Text = "N/A";
            }
        }
        catch (Exception ex)
        {
            // Fallback values if parsing fails
            CasualProgressbar.Value = 0;
            HardcoreProgressbar.Value = 0;
            CasualProgressText.Text = "0%";
            HardcoreProgressText.Text = "0%";
            EarnedAchievementsValue.Text = "0";
            TotalAchievementsValue.Text = "0";
            TotalPointsEarnedValue.Text = "0";
            TruePointsEarnedValue.Text = "0";
            HighestAwardKindText.Text = "N/A";
            HighestAwardDateText.Text = "N/A";
            HighestAwardIcon.IsVisible = false; // Ensure icon is hidden on error

            _logger.Error(ex, "Failed to parse progress data for achievements display");
        }
    }

    private static string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return char.ToUpper(input[0], CultureInfo.InvariantCulture) + input[1..];
    }

    private async void OpenUrlInBrowserAsync(string url)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Error opening URL: {url}");
            await _messageBox.UnableToOpenLinkMessageBoxAsync();
        }
    }

    private void ViewProfileOnRaButton_Click(object? sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_settings.RaUsername))
        {
            var url = $"https://retroachievements.org/user/{Uri.EscapeDataString(_settings.RaUsername)}";
            OpenUrlInBrowserAsync(url);
        }
    }

    private void ViewGameOnRaButton_Click(object? sender, RoutedEventArgs e)
    {
        var url = $"https://retroachievements.org/game/{_gameId}";
        OpenUrlInBrowserAsync(url);
    }

    private async void GameImage_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        try
        {
            if (sender is RemoteImage { Url: { } imageUrl })
            {
                _playSoundEffects.PlayNotificationSound();
                OpenRaImageViewerAsync(imageUrl);
            }
            else
            {
                // Log and potentially inform the user if the image source is not a valid URI
                _logger.Warning("Clicked image has no valid URI source to display in viewer.");
                await _messageBox.ErrorMessageBoxAsync(); // Generic error for the user
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in the method GameImage_PointerPressed.");
        }
    }

    private void OpenRaImageViewerAsync(string imageUrl)
    {
        try
        {
            var raImageViewer = App.ServiceProvider.GetRequiredService<ImageViewerWindow>();
            raImageViewer.LoadImageUrl(new Uri(imageUrl));
            raImageViewer.Show(this);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to open RetroAchievements image viewer for URI: {imageUrl}");
        }
    }

    /// <summary>
    /// Toggles the loading overlay with an optional message.
    /// </summary>
    /// <param name="isLoading">Whether to show or hide the loading overlay.</param>
    /// <param name="message">Optional message to display while loading.</param>
    public void SetLoadingState(bool isLoading, string? message = null)
    {
        LoadingOverlay.IsVisible = isLoading;
        if (isLoading)
        {
            LoadingOverlayMessage.Text = message ?? "Loading...";
        }
    }

    private static string FormatDateString(string dateString)
    {
        if (DateTime.TryParse(dateString, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var date))
        {
            return date.ToLocalTime().ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        }

        return dateString;
    }

    private static string GetPermissionDescription(int permissions)
    {
        return permissions switch
        {
            0 => "Unregistered",
            1 => "Registered",
            2 => "Junior Developer",
            3 => "Developer",
            4 => "Admin",
            _ => $"Unknown ({permissions})"
        };
    }

    private void OpenRaSettings_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var settingsWindow = App.ServiceProvider.GetRequiredService<RetroAchievementsSettingsWindow>();
            _playSoundEffects.PlayNotificationSound();
            settingsWindow.ShowDialog(this);

            // Reload current tab using Tag instead of Header
            if (TabControl.SelectedItem is TabItem selectedTab)
            {
                switch (selectedTab.Tag?.ToString())
                {
                    case "Achievements":
                        _playSoundEffects.PlayNotificationSound();
                        _ = LoadGameAchievementsAsync();
                        break;
                    case "GameInfo":
                        _playSoundEffects.PlayNotificationSound();
                        _ = LoadGameInfoAsync();
                        break;
                    case "GameRanking":
                        _playSoundEffects.PlayNotificationSound();
                        _ = LoadGameRankingAsync();
                        break;
                    case "MyProfile":
                        _playSoundEffects.PlayNotificationSound();
                        _ = LoadUserProfileAsync();
                        break;
                    case "Unlocks":
                        _playSoundEffects.PlayNotificationSound();
                        _ = LoadUnlocksByDateAsync();
                        break;
                    case "UserProgress":
                        _playSoundEffects.PlayNotificationSound();
                        _ = LoadUserProgressAsync();
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in OpenRaSettings_Click of RetroAchievementsForAGameWindow.");
        }
    }

    private async Task LoadGameAchievementsAsync()
    {
        _logger.Debug("Fetching game achievements...");

        SetLoadingState(true, "Loading...");
        await Task.Yield();

        NoAchievementsOverlay.IsVisible = false; // Hide overlay initially
        AchievementsDataGrid.ItemsSource = null; // Clear previous data

        if (string.IsNullOrWhiteSpace(_settings.RaUsername) || string.IsNullOrWhiteSpace(_settings.RaApiKey))
        {
            NoAchievementsOverlay.IsVisible = true;
            NoAchievementsMessage.Text = "RetroAchievements username or API key is not set. Configure in settings.";
            SetLoadingState(false);
            await Task.Yield();

            return;
        }

        try
        {
            // Use the injected service
            var (progress, achievements) = await _raService.GetGameInfoAndUserProgressAsync(_gameId, _settings.RaUsername, _settings.RaApiKey);

            if (progress != null && achievements is { Count: > 0 })
            {
                // Update progress summary header
                GameTitleTextBlock.Text = string.IsNullOrWhiteSpace(progress.GameTitle) ? "Unknown Game" : progress.GameTitle;
                ConsoleNameTextBlock.Text = string.IsNullOrWhiteSpace(progress.ConsoleName) ? "Unknown Console" : progress.ConsoleName;

                if (!string.IsNullOrEmpty(progress.GameIconUrl))
                {
                    GameCoverImage.Url = progress.GameIconUrl;
                }

                // Update progress bars and stats
                UpdateProgressDisplay(progress);

                AchievementsDataGrid.ItemsSource = achievements;
                NoAchievementsOverlay.IsVisible = false;
            }
            else
            {
                NoAchievementsOverlay.IsVisible = true;
                // If progress is null, it indicates an API failure (since credentials were provided)
                // If progress is not null but achievements is empty, it means no achievements for the game.
                NoAchievementsMessage.Text = progress == null
                    ? "Failed to load achievements. Please check your RetroAchievements credentials or try again later."
                    : "No achievements found for this game.";
            }
        }
        catch (RaUnauthorizedException)
        {
            NoAchievementsOverlay.IsVisible = true;
            NoAchievementsMessage.Text = "RetroAchievements credentials invalid. Please check your username and API key in settings.";
        }
        catch (Exception ex)
        {
            NoAchievementsOverlay.IsVisible = true;
            NoAchievementsMessage.Text = "An error occurred while loading achievements. Please try again.";
            _logger.Error(ex, $"Failed to load achievements for game ID: {_gameId}");
        }
        finally
        {
            SetLoadingState(false);
            await Task.Yield();
        }
    }

    private async Task LoadGameInfoAsync()
    {
        _logger.Debug("Fetching extended game info...");

        SetLoadingState(true, "Loading...");
        NoGameInfoOverlay.IsVisible = false; // Hide overlay initially
        GameInfoAchievementsSection.IsVisible = false;
        await Task.Yield();

        if (string.IsNullOrWhiteSpace(_settings.RaUsername) || string.IsNullOrWhiteSpace(_settings.RaApiKey))
        {
            NoGameInfoOverlay.IsVisible = true;
            NoGameInfoMessage.Text = "RetroAchievements username or API key is not set. Configure in settings.";
            SetLoadingState(false);
            await Task.Yield();

            return;
        }

        try
        {
            // Use the injected service
            var gameInfo = await _raService.GetGameExtendedAsync(_gameId, _settings.RaUsername, _settings.RaApiKey);
            if (gameInfo != null)
            {
                // Load game icon (for header and the new image section)
                GameInfoImageIcon.Url = string.IsNullOrEmpty(gameInfo.ImageIcon)
                    ? null
                    : $"https://retroachievements.org{gameInfo.ImageIcon}";

                // Game images
                GameInfoTitleImage.Url = string.IsNullOrEmpty(gameInfo.ImageTitle)
                    ? null
                    : $"https://retroachievements.org{gameInfo.ImageTitle}";

                GameInfoIngameImage.Url = string.IsNullOrEmpty(gameInfo.ImageIngame)
                    ? null
                    : $"https://retroachievements.org{gameInfo.ImageIngame}";

                GameInfoBoxArtImage.Url = string.IsNullOrEmpty(gameInfo.ImageBoxArt)
                    ? null
                    : $"https://retroachievements.org{gameInfo.ImageBoxArt}";

                // Basic details
                GameInfoGenre.Text = string.IsNullOrWhiteSpace(gameInfo.Genre) ? "N/A" : gameInfo.Genre;
                GameInfoDeveloper.Text = string.IsNullOrWhiteSpace(gameInfo.Developer) ? "N/A" : gameInfo.Developer;
                GameInfoPublisher.Text = string.IsNullOrWhiteSpace(gameInfo.Publisher) ? "N/A" : gameInfo.Publisher;
                GameInfoReleased.Text = string.IsNullOrWhiteSpace(gameInfo.Released) ? "N/A" : gameInfo.Released;

                // Additional details
                GameInfoConsoleName.Text = string.IsNullOrWhiteSpace(gameInfo.ConsoleName) ? "N/A" : gameInfo.ConsoleName;
                GameInfoPlayers.Text = gameInfo.NumDistinctPlayers.ToString("N0", CultureInfo.InvariantCulture);
                GameInfoAchievementCount.Text = gameInfo.NumAchievements.ToString(CultureInfo.InvariantCulture);
                GameInfoForumTopic.Text = gameInfo.ForumTopicId?.ToString(CultureInfo.InvariantCulture) ?? "N/A";
                GameInfoUpdated.Text = string.IsNullOrWhiteSpace(gameInfo.Updated) ? "N/A" : FormatDateString(gameInfo.Updated);
                GameInfoConsoleId.Text = gameInfo.ConsoleId.ToString(CultureInfo.InvariantCulture);
                GameInfoId.Text = gameInfo.Id.ToString(CultureInfo.InvariantCulture);
                GameInfoParentGame.Text = gameInfo.ParentGameId?.ToString(CultureInfo.InvariantCulture) ?? "None";
                GameInfoReleaseGranularity.Text = string.IsNullOrWhiteSpace(gameInfo.ReleasedAtGranularity) ? "N/A" : gameInfo.ReleasedAtGranularity;
                GameInfoGuideUrl.Text = string.IsNullOrWhiteSpace(gameInfo.GuideUrl) ? "N/A" : gameInfo.GuideUrl;

                // Player statistics
                DistinctPlayersValue.Text = gameInfo.NumDistinctPlayers.ToString("N0", CultureInfo.InvariantCulture);
                CasualPlayersValue.Text = gameInfo.NumDistinctPlayersCasual.ToString("N0", CultureInfo.InvariantCulture);
                HardcorePlayersValue.Text = gameInfo.NumDistinctPlayersHardcore.ToString("N0", CultureInfo.InvariantCulture);

                // Claims
                GameInfoClaims.Text = gameInfo.Claims.Count == 0
                    ? "No active development claims"
                    : string.Format(CultureInfo.InvariantCulture, "{0} active development claim(s)", gameInfo.Claims.Count);

                // Achievements list
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
                    GameInfoAchievementsDataGrid.ItemsSource = achievementsList;
                    GameInfoAchievementsSection.IsVisible = true;
                }
                else
                {
                    GameInfoAchievementsSection.IsVisible = false;
                }

                NoGameInfoOverlay.IsVisible = false;
            }
            else
            {
                NoGameInfoOverlay.IsVisible = true;
                NoGameInfoMessage.Text = "Failed to load extended game information. Please check your RetroAchievements credentials or try again later.";
            }
        }
        catch (RaUnauthorizedException)
        {
            NoGameInfoOverlay.IsVisible = true;
            NoGameInfoMessage.Text = "RetroAchievements credentials invalid. Please check your username and API key in settings.";
        }
        catch (Exception ex)
        {
            NoGameInfoOverlay.IsVisible = true;
            NoGameInfoMessage.Text = "An error occurred while loading game info. Please try again.";
            _logger.Error(ex, $"Failed to load extended game info for game ID: {_gameId}");
        }
        finally
        {
            SetLoadingState(false);
            await Task.Yield();
        }
    }

    private async Task LoadGameRankingAsync()
    {
        _logger.Debug("Fetching game rankings...");

        SetLoadingState(true, "Loading...");
        NoUserRankOverlay.IsVisible = false; // Hide overlay initially
        NoLatestMastersOverlay.IsVisible = false; // Hide overlay initially
        NoHighScoresOverlay.IsVisible = false; // Hide overlay initially
        await Task.Yield();

        // Clear previous data
        LatestMastersDataGrid.ItemsSource = null;
        HighScoresDataGrid.ItemsSource = null;

        // Reset user info
        UserRankText.Text = "N/A";
        UserScoreText.Text = "N/A";
        UserLastAwardText.Text = "N/A";

        // Check credentials first
        if (string.IsNullOrWhiteSpace(_settings.RaUsername) || string.IsNullOrWhiteSpace(_settings.RaApiKey))
        {
            LatestMastersDataGrid.ItemsSource = null;
            LatestMastersDataGrid.IsVisible = false;
            HighScoresDataGrid.ItemsSource = null;
            HighScoresDataGrid.IsVisible = false;

            NoUserRankOverlay.IsVisible = true;
            NoUserRankMessage.Text = "RetroAchievements username or API key is not set. Configure in settings.";
            NoLatestMastersOverlay.IsVisible = true;
            NoLatestMastersMessage.Text = "RetroAchievements username or API key is not set. Configure in settings.";
            NoHighScoresOverlay.IsVisible = true;
            NoHighScoresMessage.Text = "RetroAchievements username or API key is not set. Configure in settings.";

            SetLoadingState(false);
            await Task.Yield();

            return;
        }

        try
        {
            // Load Latest Masters (t=1)
            var latestMasters = await _raService.GetGameRankAndScoreAsync(_gameId, _settings.RaUsername, _settings.RaApiKey, true);
            if (latestMasters is { Count: > 0 })
            {
                for (var i = 0; i < latestMasters.Count; i++)
                {
                    latestMasters[i].Rank = i + 1; // Assign display rank
                }

                LatestMastersDataGrid.ItemsSource = latestMasters;
                LatestMastersDataGrid.IsVisible = true;
                NoLatestMastersOverlay.IsVisible = false;
            }
            else
            {
                LatestMastersDataGrid.ItemsSource = null;
                LatestMastersDataGrid.IsVisible = false;
                NoLatestMastersOverlay.IsVisible = true;
                NoLatestMastersMessage.Text = latestMasters == null
                    ? "Failed to load latest masters. Please check your RetroAchievements credentials or try again later."
                    : "No latest masters found for this game.";
            }

            // Load High Scores (t=0, default)
            var rankings = await _raService.GetGameRankAndScoreAsync(_gameId, _settings.RaUsername, _settings.RaApiKey);
            if (rankings is { Count: > 0 })
            {
                for (var i = 0; i < rankings.Count; i++)
                {
                    rankings[i].Rank = i + 1; // Assign display rank
                }

                HighScoresDataGrid.ItemsSource = rankings;
                HighScoresDataGrid.IsVisible = true;
                NoHighScoresOverlay.IsVisible = false;
            }
            else
            {
                HighScoresDataGrid.ItemsSource = null;
                HighScoresDataGrid.IsVisible = false;
                NoHighScoresOverlay.IsVisible = true;
                NoHighScoresMessage.Text = rankings == null
                    ? "Failed to load high scores. Please check your RetroAchievements credentials or try again later."
                    : "No high scores found for this game.";
            }

            // Load User Rank and Score (for the current user)
            var userGameRankAndScoreList = await _raService.GetUserGameRankAndScoreAsync(_gameId, _settings.RaUsername, _settings.RaApiKey);
            if (userGameRankAndScoreList is { Count: > 0 })
            {
                var userData = userGameRankAndScoreList.First();

                // Apply the requested logic: if UserRank is null or 0, display "Unranked"
                UserRankText.Text = userData.UserRank is null or 0 ? "Unranked" : userData.UserRank.Value.ToString(CultureInfo.InvariantCulture);

                UserScoreText.Text = userData.TotalScore.ToString("N0", CultureInfo.InvariantCulture); // Format score
                UserLastAwardText.Text = string.IsNullOrWhiteSpace(userData.LastAward) ? "N/A" : userData.LastAward;
                NoUserRankOverlay.IsVisible = false; // Ensure hidden if data is present
            }
            else // userGameRankAndScoreList is null or empty
            {
                // If the list is empty, it means the user has no rank for this game.
                UserRankText.Text = "Unranked";
                UserScoreText.Text = "0"; // Assuming 0 score if unranked
                UserLastAwardText.Text = "N/A";
                NoUserRankOverlay.IsVisible = true;
                NoUserRankMessage.Text = userGameRankAndScoreList == null
                    ? "Failed to load your rank data. Please check your RetroAchievements credentials or try again later."
                    : "No rank data available for this game.";
            }
        }
        catch (RaUnauthorizedException)
        {
            // Apply unauthorized message to all relevant overlays
            const string unauthorizedMessage = "RetroAchievements credentials invalid. Please check your username and API key in settings.";
            NoUserRankOverlay.IsVisible = true;
            NoUserRankMessage.Text = unauthorizedMessage;
            NoLatestMastersOverlay.IsVisible = true;
            NoLatestMastersMessage.Text = unauthorizedMessage;
            NoHighScoresOverlay.IsVisible = true;
            NoHighScoresMessage.Text = unauthorizedMessage;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"Failed to load game ranking tab for game ID: {_gameId}");
            // Show error state
            LatestMastersDataGrid.ItemsSource = null;
            LatestMastersDataGrid.IsVisible = false;
            HighScoresDataGrid.ItemsSource = null;
            HighScoresDataGrid.IsVisible = false;

            UserRankText.Text = "Error";
            UserScoreText.Text = "Error";
            UserLastAwardText.Text = "Error";

            NoUserRankOverlay.IsVisible = true;
            NoUserRankMessage.Text = "Error loading ranking data. Please try again.";
            NoLatestMastersOverlay.IsVisible = true;
            NoLatestMastersMessage.Text = "Error loading latest masters. Please try again.";
            NoHighScoresOverlay.IsVisible = true;
            NoHighScoresMessage.Text = "Error loading high scores. Please try again.";
        }
        finally
        {
            SetLoadingState(false);
            await Task.Yield();
        }
    }

    private async Task LoadUserProfileAsync()
    {
        _logger.Debug("Fetching user profile...");

        SetLoadingState(true, "Loading...");
        NoProfileOverlay.IsVisible = false; // Hide overlay initially
        UserProfileRecentlyPlayed.ItemsSource = null; // Clear previous data
        await Task.Yield();

        if (string.IsNullOrWhiteSpace(_settings.RaUsername) || string.IsNullOrWhiteSpace(_settings.RaApiKey))
        {
            NoProfileOverlay.IsVisible = true;
            NoProfileMainMessage.Text = "RetroAchievements username or API key is not set.";
            NoProfileSubMessage.Text = "Please configure your credentials in the RetroAchievements settings.";
            SetLoadingState(false);
            await Task.Yield();

            return;
        }

        try
        {
            // Fetch main user profile
            var userProfile = await _raService.GetUserProfileAsync(_settings.RaUsername, _settings.RaApiKey);

            // Fetch detailed recently played games separately (max 50 games)
            var recentlyPlayedGames = await _raService.GetUserRecentlyPlayedGamesAsync(_settings.RaUsername, _settings.RaApiKey, 50);

            if (userProfile != null)
            {
                // Basic profile info
                UserProfilePic.Url = string.IsNullOrEmpty(userProfile.UserPic)
                    ? null
                    : $"https://retroachievements.org{userProfile.UserPic}";

                UserProfileUser.Text = userProfile.User;
                UserProfileMotto.Text = string.IsNullOrWhiteSpace(userProfile.Motto) ? "No motto set" : userProfile.Motto;

                // Current activity
                UserProfileRichPresence.Text = string.IsNullOrWhiteSpace(userProfile.RichPresenceMsg)
                    ? "Not currently playing"
                    : userProfile.RichPresenceMsg;

                // Statistics
                var rankFormat = "#{0}";
                RankValue.Text = string.IsNullOrWhiteSpace(userProfile.Rank) ? "N/A" : string.Format(CultureInfo.InvariantCulture, rankFormat, userProfile.Rank);
                PointsValue.Text = userProfile.TotalPoints.ToString("N0", CultureInfo.InvariantCulture);
                TruePointsValue.Text = userProfile.TotalTruePoints.ToString("N0", CultureInfo.InvariantCulture);

                // Format MemberSince date
                if (DateTime.TryParse(userProfile.MemberSince, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var memberSinceDate))
                {
                    UserProfileMemberSince.Text = memberSinceDate.ToLocalTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                else
                {
                    UserProfileMemberSince.Text = string.IsNullOrWhiteSpace(userProfile.MemberSince) ? "Unknown" : userProfile.MemberSince;
                }

                // Additional details
                UserProfileId.Text = userProfile.Id.ToString(CultureInfo.InvariantCulture);
                var contributionsFormat = "{0} contributions ({1:N0} points)";
                UserProfileContributions.Text = string.Format(CultureInfo.InvariantCulture, contributionsFormat, userProfile.ContribCount, userProfile.ContribYield);
                UserProfileSoftcorePoints.Text = userProfile.TotalSoftcorePoints.ToString("N0", CultureInfo.InvariantCulture);
                UserProfilePermissions.Text = GetPermissionDescription(userProfile.Permissions);
                UserProfileStatus.Text = userProfile.Untracked == 1 ? "Untracked" : "Tracked";
                UserProfileProfileId.Text = string.IsNullOrWhiteSpace(userProfile.Ulid) ? "N/A" : userProfile.Ulid;
                UserProfileWallActive.Text = userProfile.UserWallActive ? "Yes" : "No";

                switch (recentlyPlayedGames)
                {
                    // Recently played - use the detailed list from GetUserRecentlyPlayedGamesAsync
                    case { Count: > 0 }:
                        UserProfileRecentlyPlayed.ItemsSource = recentlyPlayedGames;
                        break;
                    case null:
                        // If recentlyPlayedGames is null, it indicates an API failure for this specific call
                        _logger.Debug($"[RA Window] Failed to load recently played games for user {_settings.RaUsername}. API returned null.");
                        UserProfileRecentlyPlayed.ItemsSource = null; // Ensure it's cleared
                        break;
                    // recentlyPlayedGames is not null but empty
                    default:
                        UserProfileRecentlyPlayed.ItemsSource = null; // No recently played games
                        break;
                }

                NoProfileOverlay.IsVisible = false;
            }
            else
            {
                // If userProfile is null, something went wrong with the main profile fetch
                NoProfileOverlay.IsVisible = true;
                NoProfileMainMessage.Text = "Failed to load user profile.";
                NoProfileSubMessage.Text = "Please check your RetroAchievements credentials or try again later.";
            }
        }
        catch (RaUnauthorizedException)
        {
            NoProfileOverlay.IsVisible = true;
            NoProfileMainMessage.Text = "RetroAchievements credentials invalid. Please check your username and API key in settings.";
            NoProfileSubMessage.Text = "Please configure your credentials in the RetroAchievements settings.";
        }
        catch (Exception ex)
        {
            NoProfileOverlay.IsVisible = true;
            NoProfileMainMessage.Text = "An error occurred while loading user profile.";
            NoProfileSubMessage.Text = "Please try again or check your internet connection.";
            _logger.Error(ex, $"Failed to load user profile for {_settings.RaUsername}");
        }
        finally
        {
            SetLoadingState(false);
            await Task.Yield();
        }
    }

    private async Task LoadUnlocksByDateAsync()
    {
        _logger.Debug("Fetching earned achievements by date...");

        SetLoadingState(true, "Loading...");
        FetchUnlocksButton.IsEnabled = false; // Disable button during fetch
        NoUnlocksOverlay.IsVisible = false; // Hide overlay initially
        UnlocksDataGrid.ItemsSource = null; // Clear previous data
        TotalUnlocksInRangeText.Text = "0";
        TotalPointsEarnedInRangeText.Text = "0";
        await Task.Yield();

        if (string.IsNullOrWhiteSpace(_settings.RaUsername) || string.IsNullOrWhiteSpace(_settings.RaApiKey))
        {
            // Display specific message for missing credentials
            NoUnlocksOverlay.IsVisible = true;
            NoUnlocksMessage.Text = "RetroAchievements username or API key is not set. Configure in settings.";
            SetLoadingState(false);
            FetchUnlocksButton.IsEnabled = true; // Re-enable button
            await Task.Yield();

            return;
        }

        // Set default dates if not already set
        if (FromDatePicker.SelectedDate == null)
        {
            FromDatePicker.SelectedDate = new DateTimeOffset(DateTime.Today.AddMonths(-1)); // Default to last month
        }

        if (ToDatePicker.SelectedDate == null)
        {
            ToDatePicker.SelectedDate = new DateTimeOffset(DateTime.Today); // Default to today
        }

        var fromDate = FromDatePicker.SelectedDate?.DateTime ?? DateTime.Today.AddMonths(-1);
        var toDate = ToDatePicker.SelectedDate?.DateTime ?? DateTime.Today;

        try
        {
            var unlocks = await _raService.GetAchievementsEarnedBetweenAsync(_settings.RaUsername, _settings.RaApiKey, fromDate, toDate);

            if (unlocks is { Count: > 0 })
            {
                UnlocksDataGrid.ItemsSource = unlocks;
                TotalUnlocksInRangeText.Text = unlocks.Count.ToString("N0", CultureInfo.InvariantCulture);
                TotalPointsEarnedInRangeText.Text = unlocks.Sum(static a => a.Points).ToString("N0", CultureInfo.InvariantCulture);
                NoUnlocksOverlay.IsVisible = false; // Hide overlay if data is present
            }
            else
            {
                UnlocksDataGrid.ItemsSource = null;
                TotalUnlocksInRangeText.Text = "0";
                TotalPointsEarnedInRangeText.Text = "0";
                NoUnlocksOverlay.IsVisible = true; // Show overlay if no data
                // If unlocks is null, it indicates an API failure (since credentials were provided)
                NoUnlocksMessage.Text = unlocks == null
                    ? "Failed to load unlocks. Please check your RetroAchievements credentials or try again later."
                    : "No unlocks found for the selected date range.";
            }
        }
        catch (RaUnauthorizedException)
        {
            NoUnlocksOverlay.IsVisible = true;
            NoUnlocksMessage.Text = "RetroAchievements credentials invalid. Please check your username and API key in settings.";
        }
        catch (Exception ex)
        {
            UnlocksDataGrid.ItemsSource = null;
            TotalUnlocksInRangeText.Text = "0";
            TotalPointsEarnedInRangeText.Text = "0";
            NoUnlocksOverlay.IsVisible = true; // Show overlay on error
            NoUnlocksMessage.Text = "An error occurred while loading unlocks. Please try again.";
            _logger.Error(ex, $"Failed to load unlocks by date for user {_settings.RaUsername}");
        }
        finally
        {
            SetLoadingState(false);
            await Task.Yield();
            FetchUnlocksButton.IsEnabled = true; // Re-enable button
        }
    }

    private async void FetchUnlocksClickAsync(object? sender, RoutedEventArgs e)
    {
        try
        {
            // Validate dates before fetching
            var fromDate = FromDatePicker.SelectedDate?.DateTime ?? DateTime.Today.AddMonths(-1);
            var toDate = ToDatePicker.SelectedDate?.DateTime ?? DateTime.Today;

            if (fromDate > toDate)
            {
                await _messageBox.ErrorMessageBoxAsync();
                return; // Exit without fetching
            }

            // Proceed with loading
            await LoadUnlocksByDateAsync();
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to fetch unlocks by date");
        }
    }

    private async void ResetDatesClickAsync(object? sender, RoutedEventArgs e)
    {
        try
        {
            FromDatePicker.SelectedDate = new DateTimeOffset(DateTime.Today.AddMonths(-1));
            ToDatePicker.SelectedDate = new DateTimeOffset(DateTime.Today);
            // Optionally clear the grid and summary to reflect reset
            UnlocksDataGrid.ItemsSource = null;
            TotalUnlocksInRangeText.Text = "0";
            TotalPointsEarnedInRangeText.Text = "0";
            NoUnlocksOverlay.IsVisible = true; // Show overlay when cleared
            NoUnlocksMessage.Text = "No unlocks found for the selected date range."; // Reset message

            _logger.Debug("Resetting dates and fetching unlocks...");

            await LoadUnlocksByDateAsync(); // Automatically fetch for the new date range
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to reset date range");
        }
    }

    private async Task LoadUserProgressAsync()
    {
        _logger.Debug("Fetching user completion progress...");

        SetLoadingState(true, "Loading...");
        NoUserProgressOverlay.IsVisible = false;
        UserProgressDataGrid.ItemsSource = null; // Clear previous data
        await Task.Yield();

        if (string.IsNullOrWhiteSpace(_settings.RaUsername) || string.IsNullOrWhiteSpace(_settings.RaApiKey))
        {
            NoUserProgressOverlay.IsVisible = true;
            NoUserProgressMainMessage.Text = "RetroAchievements username or API key is not set.";
            NoUserProgressSubMessage.Text = "Please configure your credentials in the RetroAchievements settings.";
            SetLoadingState(false);
            await Task.Yield();

            return;
        }

        try
        {
            var userProgressList = await _raService.GetUserCompletionProgressAsync(_settings.RaUsername, _settings.RaApiKey);

            if (userProgressList is { Count: > 0 })
            {
                UserProgressDataGrid.ItemsSource = userProgressList;
                NoUserProgressOverlay.IsVisible = false;
            }
            else
            {
                UserProgressDataGrid.ItemsSource = null;
                NoUserProgressOverlay.IsVisible = true;
                // If userProgressList is null, it indicates an API failure (since credentials were provided)
                if (userProgressList == null)
                {
                    NoUserProgressMainMessage.Text = "Failed to load user completion progress.";
                    NoUserProgressSubMessage.Text = "Please check your RetroAchievements credentials or try again later.";
                }
                else // userProgressList is not null but empty
                {
                    NoUserProgressMainMessage.Text = "No user completion progress found.";
                    NoUserProgressSubMessage.Text = "This could be because you haven't played any games yet.";
                }
            }
        }
        catch (RaUnauthorizedException)
        {
            NoUserProgressOverlay.IsVisible = true;
            NoUserProgressMainMessage.Text = "RetroAchievements credentials invalid. Please check your username and API key in settings.";
            NoUserProgressSubMessage.Text = "Please configure your credentials in the RetroAchievements settings.";
        }
        catch (Exception ex)
        {
            NoUserProgressOverlay.IsVisible = true;
            NoUserProgressMainMessage.Text = "An error occurred while loading user completion progress.";
            NoUserProgressSubMessage.Text = "Please try again or check your internet connection.";
            _logger.Error(ex, $"Failed to load user completion progress for user {_settings.RaUsername}");
        }
        finally
        {
            SetLoadingState(false);
            await Task.Yield();
        }
    }

    private void EmergencyOverlayRelease_Click(object? sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();
        LoadingOverlay.IsVisible = false;

        _logger.Debug("[Emergency] User forced overlay dismissal in RetroAchievements Window.");
    }
}
