using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;
using System.Windows.Input;

namespace SimpleLauncher;

public partial class RetroAchievementsWindow
{
    private readonly int _gameId;
    private readonly string _gameTitleForDisplay;
    private readonly SettingsManager _settings;
    private readonly RetroAchievementsService _raService;

    public RetroAchievementsWindow(int gameId, string gameTitleForDisplay)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        Owner = Application.Current.MainWindow;

        _gameId = gameId;
        _gameTitleForDisplay = gameTitleForDisplay;
        _settings = App.ServiceProvider.GetRequiredService<SettingsManager>();
        _raService = App.ServiceProvider.GetRequiredService<RetroAchievementsService>();

        Loaded += AchievementsWindow_Loaded;
    }

    private void AchievementsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            GameTitleTextBlock.Text = _gameTitleForDisplay;
            // Load the first tab (Achievements) data when the window loads
            _ = LoadGameAchievementsAsync();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Failed to initialize RetroAchievementsWindow.");
        }
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // Explicitly cast e.Source to TabControl and then access SelectedItem
        if (e.Source is TabControl { SelectedItem: TabItem selectedTab })
        {
            // Ensure the event is not fired during initialization or when no tab is selected
            if (selectedTab is not { IsSelected: true }) return;

            var header = selectedTab.Header.ToString();
            // Remove the trailing " |" for matching, if present
            header = header?.Replace(" |", "").Trim();

            // All data loading methods now manage their own loading overlay and error states.
            switch (header)
            {
                case "Achievements":
                    _ = LoadGameAchievementsAsync();
                    break;
                case "Game Info":
                    _ = LoadGameInfoAsync();
                    break;
                case "Game Ranking":
                    _ = LoadGameRankingAsync();
                    break;
                case "My Profile":
                    _ = LoadUserProfileAsync();
                    break;
                case "Unlocks":
                    _ = LoadUnlocksByDateAsync();
                    break;
                case "User Progress":
                    _ = LoadUserProgressAsync();
                    break;
            }
        }
    }

    private void UpdateProgressDisplay(RaUserGameProgress progress)
    {
        Dispatcher.Invoke(() =>
        {
            try
            {
                // Parse completion percentages
                double casualCompletion = 0;
                double hardcoreCompletion = 0;

                if (!string.IsNullOrWhiteSpace(progress.UserCompletion))
                {
                    var casualText = progress.UserCompletion.Trim('%');
                    if (!double.TryParse(casualText, NumberStyles.Float, CultureInfo.InvariantCulture, out casualCompletion))
                    {
                        _ = LogErrors.LogErrorAsync(null, $"Failed to parse casual completion percentage: {casualText}");
                    }
                }

                if (!string.IsNullOrWhiteSpace(progress.UserCompletionHardcore))
                {
                    var hardcoreText = progress.UserCompletionHardcore.Trim('%');
                    if (!double.TryParse(hardcoreText, NumberStyles.Float, CultureInfo.InvariantCulture, out hardcoreCompletion))
                    {
                        _ = LogErrors.LogErrorAsync(null, $"Failed to parse hardcore completion percentage: {hardcoreText}");
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
                HighestAwardKindText.Text = string.IsNullOrWhiteSpace(progress.HighestAwardKind) ? "None" : CapitalizeFirstLetter(progress.HighestAwardKind);

                // Set Highest Award Icon (using existing trophy.png from ContextMenu.cs)
                if (progress.HighestAwardKind?.Equals("mastered", StringComparison.OrdinalIgnoreCase) == true)
                {
                    HighestAwardIcon.Source = new BitmapImage(new Uri("pack://application:,,,/images/trophy.png"));
                    HighestAwardIcon.Visibility = Visibility.Visible;
                }
                else
                {
                    HighestAwardIcon.Visibility = Visibility.Collapsed;
                }

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
                HighestAwardIcon.Visibility = Visibility.Collapsed; // Ensure icon is hidden on error

                _ = LogErrors.LogErrorAsync(ex, "Failed to parse progress data for achievements display");
            }
        });
    }

    private static string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return char.ToUpper(input[0], CultureInfo.InvariantCulture) + input.Substring(1);
    }

    private void GameImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is Image clickedImage)
        {
            if (clickedImage.Source is BitmapImage bitmapImage && bitmapImage.UriSource != null)
            {
                OpenRaImageViewer(bitmapImage.UriSource); // Use the new RetroAchievementsImageViewerWindow
            }
            else
            {
                // Log and potentially inform the user if the image source is not a valid URI
                _ = LogErrors.LogErrorAsync(null, "Clicked image has no valid URI source to display in viewer.");
                MessageBoxLibrary.ErrorMessageBox(); // Generic error for the user
            }
        }
    }

    private void OpenRaImageViewer(Uri imageUri)
    {
        try
        {
            var raImageViewer = new RetroAchievementsImageViewerWindow(); // Instantiate the new window
            raImageViewer.LoadImage(imageUri);
            raImageViewer.Owner = this; // Set owner to this window
            raImageViewer.Show();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"Failed to open RetroAchievements image viewer for URI: {imageUri}");
            MessageBoxLibrary.ErrorMessageBox(); // Display a generic error message to the user
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

    private void OpenRaSettings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = new RetroAchievementsSettingsWindow(_settings);
        settingsWindow.Owner = this; // Set owner to this window
        settingsWindow.ShowDialog();

        // After settings are saved, try reloading the current tab's data
        // Get the currently selected tab header
        if (TabControl.SelectedItem is TabItem selectedTab)
        {
            var header = selectedTab.Header.ToString()?.Replace(" |", "").Trim();
            switch (header)
            {
                case "Achievements":
                    _ = LoadGameAchievementsAsync();
                    break;
                case "Game Info":
                    _ = LoadGameInfoAsync();
                    break;
                case "Game Ranking":
                    _ = LoadGameRankingAsync();
                    break;
                case "My Profile":
                    _ = LoadUserProfileAsync();
                    break;
                case "Unlocks":
                    _ = LoadUnlocksByDateAsync();
                    break;
                case "User Progress":
                    _ = LoadUserProgressAsync();
                    break;
            }
        }
    }

    private async Task LoadGameAchievementsAsync()
    {
        await Dispatcher.InvokeAsync(async () => // Ensure UI updates are on the UI thread
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            NoAchievementsOverlay.Visibility = Visibility.Collapsed; // Hide overlay initially
            AchievementsDataGrid.ItemsSource = null; // Clear previous data

            if (string.IsNullOrWhiteSpace(_settings.RaUsername) || string.IsNullOrWhiteSpace(_settings.RaApiKey))
            {
                NoAchievementsOverlay.Visibility = Visibility.Visible;
                NoAchievementsMessage.Text = "RetroAchievements username or API key is not set. Configure in settings.";
                LoadingOverlay.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                // Use the injected service
                var (progress, achievements) = await _raService.GetGameInfoAndUserProgress(_gameId, _settings.RaUsername, _settings.RaApiKey);

                if (progress != null && achievements is { Count: > 0 })
                {
                    // Update progress summary header
                    GameTitleTextBlock.Text = string.IsNullOrWhiteSpace(progress.GameTitle) ? "Unknown Game" : progress.GameTitle;
                    ConsoleNameTextBlock.Text = string.IsNullOrWhiteSpace(progress.ConsoleName) ? "Unknown Console" : progress.ConsoleName;

                    if (!string.IsNullOrEmpty(progress.GameIconUrl))
                    {
                        GameCoverImage.Source = new BitmapImage(new Uri(progress.GameIconUrl));
                    }

                    // Update progress bars and stats
                    UpdateProgressDisplay(progress);

                    AchievementsDataGrid.ItemsSource = achievements;
                    NoAchievementsOverlay.Visibility = Visibility.Collapsed;
                }
                else
                {
                    NoAchievementsOverlay.Visibility = Visibility.Visible;
                    NoAchievementsMessage.Text = "No achievements found for this game.";
                }
            }
            catch (Exception ex)
            {
                NoAchievementsOverlay.Visibility = Visibility.Visible;
                NoAchievementsMessage.Text = "An error occurred while loading achievements. Please try again.";
                _ = LogErrors.LogErrorAsync(ex, $"Failed to load achievements for game ID: {_gameId}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        });
    }

    private async Task LoadGameInfoAsync()
    {
        await Dispatcher.InvokeAsync(async () => // Ensure UI updates are on the UI thread
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            NoGameInfoOverlay.Visibility = Visibility.Collapsed; // Hide overlay initially

            if (string.IsNullOrWhiteSpace(_settings.RaUsername) || string.IsNullOrWhiteSpace(_settings.RaApiKey))
            {
                NoGameInfoOverlay.Visibility = Visibility.Visible;
                NoGameInfoMessage.Text = "RetroAchievements username or API key is not set. Configure in settings.";
                LoadingOverlay.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                // Use the injected service
                var gameInfo = await _raService.GetGameExtended(_gameId, _settings.RaUsername, _settings.RaApiKey);
                if (gameInfo != null)
                {
                    // Load game icon (for header and the new image section)
                    if (!string.IsNullOrEmpty(gameInfo.ImageIcon))
                    {
                        try
                        {
                            var uri = new Uri($"https://retroachievements.org{gameInfo.ImageIcon}");
                            GameInfoImageIcon.Source = new BitmapImage(uri); // For the image section
                        }
                        catch
                        {
                            GameInfoImageIcon.Source = null;
                        }
                    }
                    else
                    {
                        GameInfoImageIcon.Source = null;
                    }


                    // Game images
                    if (!string.IsNullOrEmpty(gameInfo.ImageTitle))
                    {
                        try
                        {
                            GameInfoTitleImage.Source = new BitmapImage(new Uri($"https://retroachievements.org{gameInfo.ImageTitle}"));
                        }
                        catch
                        {
                            GameInfoTitleImage.Source = null;
                        }
                    }
                    else
                    {
                        GameInfoTitleImage.Source = null;
                    }

                    if (!string.IsNullOrEmpty(gameInfo.ImageIngame))
                    {
                        try
                        {
                            GameInfoIngameImage.Source = new BitmapImage(new Uri($"https://retroachievements.org{gameInfo.ImageIngame}"));
                        }
                        catch
                        {
                            GameInfoIngameImage.Source = null;
                        }
                    }
                    else
                    {
                        GameInfoIngameImage.Source = null;
                    }

                    if (!string.IsNullOrEmpty(gameInfo.ImageBoxArt))
                    {
                        try
                        {
                            GameInfoBoxArtImage.Source = new BitmapImage(new Uri($"https://retroachievements.org{gameInfo.ImageBoxArt}"));
                        }
                        catch
                        {
                            GameInfoBoxArtImage.Source = null;
                        }
                    }
                    else
                    {
                        GameInfoBoxArtImage.Source = null;
                    }

                    // Basic details
                    GameInfoGenre.Text = string.IsNullOrWhiteSpace(gameInfo.Genre) ? "N/A" : gameInfo.Genre;
                    GameInfoDeveloper.Text = string.IsNullOrWhiteSpace(gameInfo.Developer) ? "N/A" : gameInfo.Developer;
                    GameInfoPublisher.Text = string.IsNullOrWhiteSpace(gameInfo.Publisher) ? "N/A" : gameInfo.Publisher;
                    GameInfoReleased.Text = string.IsNullOrWhiteSpace(gameInfo.Released) ? "N/A" : gameInfo.Released;

                    // Additional details
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

                    // Rich Presence
                    GameInfoRichPresence.Text = string.IsNullOrWhiteSpace(gameInfo.RichPresencePatch) ? "Not available" : gameInfo.RichPresencePatch;

                    // Claims
                    GameInfoClaims.Text = gameInfo.Claims.Count == 0
                        ? "No active development claims"
                        : $"{gameInfo.Claims.Count} active development claim(s)";

                    NoGameInfoOverlay.Visibility = Visibility.Collapsed;
                }
                else
                {
                    NoGameInfoOverlay.Visibility = Visibility.Visible;
                    NoGameInfoMessage.Text = "Extended game information not available.";
                }
            }
            catch (Exception ex)
            {
                NoGameInfoOverlay.Visibility = Visibility.Visible;
                NoGameInfoMessage.Text = "An error occurred while loading game info. Please try again.";
                await LogErrors.LogErrorAsync(ex, $"Failed to load extended game info for game ID: {_gameId}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        });
    }

    private async Task LoadGameRankingAsync()
    {
        await Dispatcher.InvokeAsync(async () => // Ensure UI updates are on the UI thread
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            NoUserRankOverlay.Visibility = Visibility.Collapsed; // Hide overlay initially
            NoHighScoresOverlay.Visibility = Visibility.Collapsed; // Hide overlay initially
            HighScoresDataGrid.ItemsSource = null; // Clear previous data
            UserRankText.Text = "N/A";
            UserScoreText.Text = "N/A";
            UserLastAwardText.Text = "N/A";

            // Check credentials first
            if (string.IsNullOrWhiteSpace(_settings.RaUsername) || string.IsNullOrWhiteSpace(_settings.RaApiKey))
            {
                HighScoresDataGrid.ItemsSource = null;
                HighScoresDataGrid.Visibility = Visibility.Collapsed; // Hide the DataGrid
                NoUserRankOverlay.Visibility = Visibility.Visible;
                NoUserRankMessage.Text = "RetroAchievements username or API key is not set. Configure in settings.";
                NoHighScoresOverlay.Visibility = Visibility.Visible;
                NoHighScoresMessage.Text = "RetroAchievements username or API key is not set. Configure in settings.";
                LoadingOverlay.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                // Load High Scores
                var rankings = await _raService.GetGameRankAndScoreAsync(_gameId, _settings.RaUsername, _settings.RaApiKey);
                if (rankings != null && rankings.Count > 0)
                {
                    for (var i = 0; i < rankings.Count; i++)
                    {
                        rankings[i].Rank = i + 1;
                    }

                    HighScoresDataGrid.ItemsSource = rankings;
                    HighScoresDataGrid.Visibility = Visibility.Visible;
                    NoHighScoresOverlay.Visibility = Visibility.Collapsed;
                }
                else
                {
                    HighScoresDataGrid.ItemsSource = null;
                    HighScoresDataGrid.Visibility = Visibility.Collapsed;
                    NoHighScoresOverlay.Visibility = Visibility.Visible;
                    NoHighScoresMessage.Text = "No high scores found for this game.";
                }

                // Load User Rank and Score
                var userRank = await _raService.GetUserGameRankAndScoreAsync(_gameId, _settings.RaUsername, _settings.RaApiKey);
                if (userRank != null && userRank.Count > 0)
                {
                    var userData = userRank.First(); // API returns a list, but typically one entry
                    UserRankText.Text = userData.UserRank?.ToString(CultureInfo.InvariantCulture) ?? "Unranked";
                    UserScoreText.Text = userData.TotalScore.ToString("N0", CultureInfo.InvariantCulture); // Format score
                    UserLastAwardText.Text = string.IsNullOrWhiteSpace(userData.LastAward) ? "N/A" : userData.LastAward;
                    NoUserRankOverlay.Visibility = Visibility.Collapsed; // Ensure hidden if data is present
                }
                else
                {
                    UserRankText.Text = "N/A";
                    UserScoreText.Text = "N/A";
                    UserLastAwardText.Text = "N/A";
                    NoUserRankOverlay.Visibility = Visibility.Visible;
                    NoUserRankMessage.Text = "No rank data available for this game.";
                }
            }
            catch (Exception ex)
            {
                _ = LogErrors.LogErrorAsync(ex, $"Failed to load game ranking tab for game ID: {_gameId}");
                // Show error state
                HighScoresDataGrid.ItemsSource = null;
                HighScoresDataGrid.Visibility = Visibility.Collapsed; // Hide the DataGrid
                UserRankText.Text = "Error";
                UserScoreText.Text = "Error";
                UserLastAwardText.Text = "Error";
                NoUserRankOverlay.Visibility = Visibility.Visible;
                NoUserRankMessage.Text = "Error loading ranking data. Please try again.";
                NoHighScoresOverlay.Visibility = Visibility.Visible;
                NoHighScoresMessage.Text = "Error loading high scores. Please try again.";
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        });
    }

    private async Task LoadUserProfileAsync()
    {
        await Dispatcher.InvokeAsync(async () => // Ensure UI updates are on the UI thread
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            NoProfileOverlay.Visibility = Visibility.Collapsed; // Hide overlay initially
            UserProfileRecentlyPlayed.ItemsSource = null; // Clear previous data

            if (string.IsNullOrWhiteSpace(_settings.RaUsername) || string.IsNullOrWhiteSpace(_settings.RaApiKey))
            {
                NoProfileOverlay.Visibility = Visibility.Visible;
                NoProfileMainMessage.Text = "RetroAchievements username or API key is not set.";
                NoProfileSubMessage.Text = "Please configure your credentials in the RetroAchievements settings.";
                LoadingOverlay.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                // Fetch main user profile
                var userProfile = await _raService.GetUserProfile(_settings.RaUsername, _settings.RaApiKey);

                // Fetch detailed recently played games separately (max 50 games)
                var recentlyPlayedGames = await _raService.GetUserRecentlyPlayedGamesAsync(_settings.RaUsername, _settings.RaApiKey, 50);

                if (userProfile != null)
                {
                    // Basic profile info
                    if (!string.IsNullOrEmpty(userProfile.UserPic))
                    {
                        UserProfilePic.Source = new BitmapImage(new Uri($"https://retroachievements.org{userProfile.UserPic}"));
                    }
                    else
                    {
                        UserProfilePic.Source = null; // Clear image if not available
                    }

                    UserProfileUser.Text = userProfile.User;
                    UserProfileMotto.Text = string.IsNullOrWhiteSpace(userProfile.Motto) ? "No motto set" : userProfile.Motto;

                    // Current activity
                    UserProfileRichPresence.Text = string.IsNullOrWhiteSpace(userProfile.RichPresenceMsg)
                        ? "Not currently playing"
                        : userProfile.RichPresenceMsg;

                    // Statistics
                    RankValue.Text = string.IsNullOrWhiteSpace(userProfile.Rank) ? "N/A" : $"#{userProfile.Rank}";
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
                    UserProfileContributions.Text = $"{userProfile.ContribCount} contributions ({userProfile.ContribYield:N0} points)";
                    UserProfileSoftcorePoints.Text = userProfile.TotalSoftcorePoints.ToString("N0", CultureInfo.InvariantCulture);
                    UserProfilePermissions.Text = GetPermissionDescription(userProfile.Permissions);
                    UserProfileStatus.Text = userProfile.Untracked == 1 ? "Untracked" : "Tracked";
                    UserProfileProfileId.Text = string.IsNullOrWhiteSpace(userProfile.Uuid) ? "N/A" : userProfile.Uuid;
                    UserProfileWallActive.Text = userProfile.UserWallActive ? "Yes" : "No";

                    // Recently played - use the detailed list from GetUserRecentlyPlayedGamesAsync
                    if (recentlyPlayedGames is { Count: > 0 })
                    {
                        // Ensure full URLs are used (handled in model)
                        UserProfileRecentlyPlayed.ItemsSource = recentlyPlayedGames;
                    }
                    else
                    {
                        UserProfileRecentlyPlayed.ItemsSource = null;
                        // If no recently played games are found, the ListBox will simply be empty.
                    }

                    NoProfileOverlay.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // If userProfile is null, something went wrong with the main profile fetch
                    NoProfileOverlay.Visibility = Visibility.Visible;
                    // Update messages for general API failure
                    NoProfileMainMessage.Text = "Could not load user profile.";
                    NoProfileSubMessage.Text = "Please check your username and API key in settings, or try again later.";
                }
            }
            catch (Exception ex)
            {
                NoProfileOverlay.Visibility = Visibility.Visible;
                // Update messages for exception
                NoProfileMainMessage.Text = "An error occurred while loading user profile.";
                NoProfileSubMessage.Text = "Please try again or check your internet connection.";
                _ = LogErrors.LogErrorAsync(ex, $"Failed to load user profile for {_settings.RaUsername}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        });
    }

    private async Task LoadUnlocksByDateAsync()
    {
        await Dispatcher.InvokeAsync(async () => // Ensure UI updates are on the UI thread
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            FetchUnlocksButton.IsEnabled = false; // Disable button during fetch
            NoUnlocksOverlay.Visibility = Visibility.Collapsed; // Hide overlay initially
            UnlocksDataGrid.ItemsSource = null; // Clear previous data
            TotalUnlocksInRangeText.Text = "0";
            TotalPointsEarnedInRangeText.Text = "0";

            if (string.IsNullOrWhiteSpace(_settings.RaUsername) || string.IsNullOrWhiteSpace(_settings.RaApiKey))
            {
                // Display specific message for missing credentials
                NoUnlocksOverlay.Visibility = Visibility.Visible;
                NoUnlocksMessage.Text = "RetroAchievements username or API key is not set. Configure in settings.";
                LoadingOverlay.Visibility = Visibility.Collapsed;
                return;
            }

            // Set default dates if not already set
            if (FromDatePicker.SelectedDate == null)
            {
                FromDatePicker.SelectedDate = DateTime.Today.AddMonths(-1); // Default to last month
            }

            if (ToDatePicker.SelectedDate == null)
            {
                ToDatePicker.SelectedDate = DateTime.Today; // Default to today
            }

            var fromDate = FromDatePicker.SelectedDate ?? DateTime.Today.AddMonths(-1);
            var toDate = ToDatePicker.SelectedDate ?? DateTime.Today;

            try
            {
                var unlocks = await _raService.GetAchievementsEarnedBetween(_settings.RaUsername, _settings.RaApiKey, fromDate, toDate);

                if (unlocks is { Count: > 0 })
                {
                    UnlocksDataGrid.ItemsSource = unlocks;
                    TotalUnlocksInRangeText.Text = unlocks.Count.ToString("N0", CultureInfo.InvariantCulture);
                    TotalPointsEarnedInRangeText.Text = unlocks.Sum(static a => a.Points).ToString("N0", CultureInfo.InvariantCulture);
                    NoUnlocksOverlay.Visibility = Visibility.Collapsed; // Hide overlay if data is present
                }
                else
                {
                    UnlocksDataGrid.ItemsSource = null;
                    TotalUnlocksInRangeText.Text = "0";
                    TotalPointsEarnedInRangeText.Text = "0";
                    NoUnlocksOverlay.Visibility = Visibility.Visible; // Show overlay if no data
                    NoUnlocksMessage.Text = "No unlocks found for the selected date range.";
                }
            }
            catch (Exception ex)
            {
                UnlocksDataGrid.ItemsSource = null;
                TotalUnlocksInRangeText.Text = "0";
                TotalPointsEarnedInRangeText.Text = "0";
                NoUnlocksOverlay.Visibility = Visibility.Visible; // Show overlay on error
                NoUnlocksMessage.Text = "An error occurred while loading unlocks. Please try again.";
                _ = LogErrors.LogErrorAsync(ex, $"Failed to load unlocks by date for user {_settings.RaUsername}");
                DebugLogger.Log($"[RA Window] Failed to load unlocks by date for user {_settings.RaUsername}: {ex.Message}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
                FetchUnlocksButton.IsEnabled = true; // Re-enable button
            }
        });
    }

    private async void FetchUnlocks_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Validate dates before fetching
            var fromDate = FromDatePicker.SelectedDate ?? DateTime.Today.AddMonths(-1);
            var toDate = ToDatePicker.SelectedDate ?? DateTime.Today;

            if (fromDate > toDate)
            {
                MessageBoxLibrary.ErrorMessageBox(); // This message box is already on UI thread.
                return; // Exit without fetching
            }

            // Proceed with loading
            try
            {
                await LoadUnlocksByDateAsync();
            }
            catch (Exception ex)
            {
                _ = LogErrors.LogErrorAsync(ex, "Failed to fetch unlocks by date");
                DebugLogger.Log($"[RA Window] Failed to fetch unlocks by date for user {_settings.RaUsername}: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Failed to fetch unlocks by date");
            DebugLogger.Log($"[RA Window] Failed to fetch unlocks by date for user {_settings.RaUsername}: {ex.Message}");
        }
    }

    private async void ResetDates_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await Dispatcher.InvokeAsync(async () => // Ensure UI updates are on the UI thread
            {
                try
                {
                    FromDatePicker.SelectedDate = DateTime.Today.AddMonths(-1);
                    ToDatePicker.SelectedDate = DateTime.Today;
                    // Optionally clear the grid and summary to reflect reset
                    UnlocksDataGrid.ItemsSource = null;
                    TotalUnlocksInRangeText.Text = "0";
                    TotalPointsEarnedInRangeText.Text = "0";
                    NoUnlocksOverlay.Visibility = Visibility.Visible; // Show overlay when cleared
                    NoUnlocksMessage.Text = "No unlocks found for the selected date range."; // Reset message
                    await LoadUnlocksByDateAsync(); // Automatically fetch for the new date range
                }
                catch (Exception ex)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(ex, "Failed to reset date range");
                    DebugLogger.Log($"[RA Window] Failed to reset date range for user {_settings.RaUsername}: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Failed to reset date range");
            DebugLogger.Log($"[RA Window] Failed to reset date range for user {_settings.RaUsername}: {ex.Message}");
        }
    }

    private async Task LoadUserProgressAsync()
    {
        await Dispatcher.InvokeAsync(async () => // Ensure UI updates are on the UI thread
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            NoUserProgressOverlay.Visibility = Visibility.Collapsed;
            UserProgressDataGrid.ItemsSource = null; // Clear previous data

            if (string.IsNullOrWhiteSpace(_settings.RaUsername) || string.IsNullOrWhiteSpace(_settings.RaApiKey))
            {
                NoUserProgressOverlay.Visibility = Visibility.Visible;
                NoUserProgressMainMessage.Text = "RetroAchievements username or API key is not set.";
                NoUserProgressSubMessage.Text = "Please configure your credentials in the RetroAchievements settings.";
                LoadingOverlay.Visibility = Visibility.Collapsed;
                return;
            }

            try
            {
                var userProgressList = await _raService.GetUserCompletionProgress(_settings.RaUsername, _settings.RaApiKey);

                if (userProgressList is { Count: > 0 })
                {
                    UserProgressDataGrid.ItemsSource = userProgressList;
                    NoUserProgressOverlay.Visibility = Visibility.Collapsed;
                }
                else
                {
                    UserProgressDataGrid.ItemsSource = null;
                    NoUserProgressOverlay.Visibility = Visibility.Visible;
                    NoUserProgressMainMessage.Text = "No user completion progress found.";
                    NoUserProgressSubMessage.Text = "This could be because you haven't played any games or the API request failed.";
                }
            }
            catch (Exception ex)
            {
                NoUserProgressOverlay.Visibility = Visibility.Visible;
                NoUserProgressMainMessage.Text = "An error occurred while loading user completion progress.";
                NoUserProgressSubMessage.Text = "Please try again or check your internet connection.";
                _ = LogErrors.LogErrorAsync(ex, $"Failed to load user completion progress for user {_settings.RaUsername}");
                DebugLogger.Log($"[RA Window] Failed to load user completion progress for user {_settings.RaUsername}: {ex.Message}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        });
    }
}