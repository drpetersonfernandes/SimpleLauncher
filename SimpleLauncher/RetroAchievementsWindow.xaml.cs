using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class RetroAchievementsWindow
{
    private readonly int _gameId;
    private readonly string _gameTitleForDisplay;
    private readonly SettingsManager _settings;
    private readonly RetroAchievementsService _raService;

    // Define a constant for the unauthorized message to avoid repetition
    private const string UnauthorizedMessage = "RetroAchievements credentials invalid. Please check your username and API key in settings.";

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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to initialize RetroAchievementsWindow.");
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
                    UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingAchievements") ?? "Loading achievements...", Owner as MainWindow);
                    _ = LoadGameAchievementsAsync();
                    break;
                case "Game Info":
                    UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingExtendedGameInfo") ?? "Loading extended game info...", Owner as MainWindow);
                    _ = LoadGameInfoAsync();
                    break;
                case "Game Ranking":
                    UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingGameRankings") ?? "Loading game rankings...", Owner as MainWindow);
                    _ = LoadGameRankingAsync();
                    break;
                case "My Profile":
                    UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingUserProfile") ?? "Loading user profile...", Owner as MainWindow);
                    _ = LoadUserProfileAsync();
                    break;
                case "Unlocks":
                    UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingUserUnlocks") ?? "Loading user unlocks...", Owner as MainWindow);
                    _ = LoadUnlocksByDateAsync();
                    break;
                case "User Progress":
                    UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingUserCompletionProgress") ?? "Loading user completion progress...", Owner as MainWindow);
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
                        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"Failed to parse casual completion percentage: {casualText}");
                    }
                }

                if (!string.IsNullOrWhiteSpace(progress.UserCompletionHardcore))
                {
                    var hardcoreText = progress.UserCompletionHardcore.Trim('%');
                    if (!double.TryParse(hardcoreText, NumberStyles.Float, CultureInfo.InvariantCulture, out hardcoreCompletion))
                    {
                        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"Failed to parse hardcore completion percentage: {hardcoreText}");
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

                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to parse progress data for achievements display");
            }
        });
    }

    private static string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return char.ToUpper(input[0], CultureInfo.InvariantCulture) + input.Substring(1);
    }

    private void OpenUrlInBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Error opening URL: {url}");
            MessageBoxLibrary.UnableToOpenLinkMessageBox();
        }
    }

    private void ViewProfileOnRaButton_Click(object sender, RoutedEventArgs e)
    {
        if (!string.IsNullOrWhiteSpace(_settings.RaUsername))
        {
            var url = $"https://retroachievements.org/user/{Uri.EscapeDataString(_settings.RaUsername)}";
            OpenUrlInBrowser(url);
        }
    }

    private void ViewGameOnRaButton_Click(object sender, RoutedEventArgs e)
    {
        var url = $"https://retroachievements.org/game/{_gameId}";
        OpenUrlInBrowser(url);
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
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, "Clicked image has no valid URI source to display in viewer.");
                MessageBoxLibrary.ErrorMessageBox(); // Generic error for the user
            }
        }
    }

    private void OpenRaImageViewer(Uri imageUri)
    {
        try
        {
            var raImageViewer = new ImageViewerWindow(); // Instantiate the new window
            raImageViewer.LoadImageUrl(imageUri);
            raImageViewer.Owner = this; // Set owner to this window
            raImageViewer.Show();
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to open RetroAchievements image viewer for URI: {imageUri}");
            DebugLogger.Log($"Failed to open RetroAchievements image viewer for URI: {imageUri}");
            MessageBoxLibrary.ErrorMessageBox();
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
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("FetchingGameAchievements") ?? "Fetching game achievements...", Owner as MainWindow);
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
                var (progress, achievements) = await _raService.GetGameInfoAndUserProgressAsync(_gameId, _settings.RaUsername, _settings.RaApiKey);

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
                    // If progress is null, it indicates an API failure (since credentials were provided)
                    // If progress is not null but achievements is empty, it means no achievements for the game.
                    if (progress == null)
                    {
                        NoAchievementsMessage.Text = "Failed to load achievements. Please check your RetroAchievements credentials or try again later.";
                    }
                    else // progress is not null, but achievements is empty
                    {
                        NoAchievementsMessage.Text = "No achievements found for this game.";
                    }
                }
            }
            catch (RaUnauthorizedException)
            {
                NoAchievementsOverlay.Visibility = Visibility.Visible;
                NoAchievementsMessage.Text = UnauthorizedMessage;
            }
            catch (Exception ex)
            {
                NoAchievementsOverlay.Visibility = Visibility.Visible;
                NoAchievementsMessage.Text = "An error occurred while loading achievements. Please try again.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to load achievements for game ID: {_gameId}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        });
    }

    private async Task LoadGameInfoAsync()
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("FetchingExtendedGameInfo") ?? "Fetching extended game info...", Owner as MainWindow);
        await Dispatcher.InvokeAsync(async () => // Ensure UI updates are on the UI thread
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            NoGameInfoOverlay.Visibility = Visibility.Collapsed; // Hide overlay initially
            GameInfoAchievementsSection.Visibility = Visibility.Collapsed;

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
                var gameInfo = await _raService.GetGameExtendedAsync(_gameId, _settings.RaUsername, _settings.RaApiKey);
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
                        : $"{gameInfo.Claims.Count} active development claim(s)";

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
                        GameInfoAchievementsSection.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        GameInfoAchievementsSection.Visibility = Visibility.Collapsed;
                    }

                    NoGameInfoOverlay.Visibility = Visibility.Collapsed;
                }
                else
                {
                    NoGameInfoOverlay.Visibility = Visibility.Visible;
                    NoGameInfoMessage.Text = "Failed to load extended game information. Please check your RetroAchievements credentials or try again later.";
                }
            }
            catch (RaUnauthorizedException)
            {
                NoGameInfoOverlay.Visibility = Visibility.Visible;
                NoGameInfoMessage.Text = UnauthorizedMessage;
            }
            catch (Exception ex)
            {
                NoGameInfoOverlay.Visibility = Visibility.Visible;
                NoGameInfoMessage.Text = "An error occurred while loading game info. Please try again.";
                await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to load extended game info for game ID: {_gameId}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        });
    }

    private async Task LoadGameRankingAsync()
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("FetchingGameRankings") ?? "Fetching game rankings...", Owner as MainWindow);
        await Dispatcher.InvokeAsync(async () => // Ensure UI updates are on the UI thread
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            NoUserRankOverlay.Visibility = Visibility.Collapsed; // Hide overlay initially
            NoLatestMastersOverlay.Visibility = Visibility.Collapsed; // Hide overlay initially
            NoHighScoresOverlay.Visibility = Visibility.Collapsed; // Hide overlay initially

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
                LatestMastersDataGrid.Visibility = Visibility.Collapsed;
                HighScoresDataGrid.ItemsSource = null;
                HighScoresDataGrid.Visibility = Visibility.Collapsed;

                NoUserRankOverlay.Visibility = Visibility.Visible;
                NoUserRankMessage.Text = "RetroAchievements username or API key is not set. Configure in settings.";
                NoLatestMastersOverlay.Visibility = Visibility.Visible;
                NoLatestMastersMessage.Text = "RetroAchievements username or API key is not set. Configure in settings.";
                NoHighScoresOverlay.Visibility = Visibility.Visible;
                NoHighScoresMessage.Text = "RetroAchievements username or API key is not set. Configure in settings.";

                LoadingOverlay.Visibility = Visibility.Collapsed;
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
                    LatestMastersDataGrid.Visibility = Visibility.Visible;
                    NoLatestMastersOverlay.Visibility = Visibility.Collapsed;
                }
                else
                {
                    LatestMastersDataGrid.ItemsSource = null;
                    LatestMastersDataGrid.Visibility = Visibility.Collapsed;
                    NoLatestMastersOverlay.Visibility = Visibility.Visible;
                    NoLatestMastersMessage.Text = latestMasters == null
                        ? "Failed to load latest masters. Please check your RetroAchievements credentials or try again later."
                        : "No latest masters found for this game.";
                }

                // Load High Scores (t=0, default)
                var rankings = await _raService.GetGameRankAndScoreAsync(_gameId, _settings.RaUsername, _settings.RaApiKey, false);
                if (rankings is { Count: > 0 })
                {
                    for (var i = 0; i < rankings.Count; i++)
                    {
                        rankings[i].Rank = i + 1; // Assign display rank
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
                    if (userData.UserRank is null or 0)
                    {
                        UserRankText.Text = "Unranked";
                    }
                    else
                    {
                        UserRankText.Text = userData.UserRank.Value.ToString(CultureInfo.InvariantCulture);
                    }

                    UserScoreText.Text = userData.TotalScore.ToString("N0", CultureInfo.InvariantCulture); // Format score
                    UserLastAwardText.Text = string.IsNullOrWhiteSpace(userData.LastAward) ? "N/A" : userData.LastAward;
                    NoUserRankOverlay.Visibility = Visibility.Collapsed; // Ensure hidden if data is present
                }
                else // userGameRankAndScoreList is null or empty
                {
                    // If the list is empty, it means the user has no rank for this game.
                    UserRankText.Text = "Unranked"; // As per request
                    UserScoreText.Text = "0"; // Assuming 0 score if unranked
                    UserLastAwardText.Text = "N/A";
                    NoUserRankOverlay.Visibility = Visibility.Visible;
                    NoUserRankMessage.Text = userGameRankAndScoreList == null
                        ? "Failed to load your rank data. Please check your RetroAchievements credentials or try again later."
                        : "No rank data available for this game.";
                }
            }
            catch (RaUnauthorizedException)
            {
                // Apply unauthorized message to all relevant overlays
                NoUserRankOverlay.Visibility = Visibility.Visible;
                NoUserRankMessage.Text = UnauthorizedMessage;
                NoLatestMastersOverlay.Visibility = Visibility.Visible;
                NoLatestMastersMessage.Text = UnauthorizedMessage;
                NoHighScoresOverlay.Visibility = Visibility.Visible;
                NoHighScoresMessage.Text = UnauthorizedMessage;
            }
            catch (Exception ex)
            {
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to load game ranking tab for game ID: {_gameId}");
                // Show error state
                LatestMastersDataGrid.ItemsSource = null;
                LatestMastersDataGrid.Visibility = Visibility.Collapsed;
                HighScoresDataGrid.ItemsSource = null;
                HighScoresDataGrid.Visibility = Visibility.Collapsed;

                UserRankText.Text = "Error"; // Set to error state on exception
                UserScoreText.Text = "Error";
                UserLastAwardText.Text = "Error";

                NoUserRankOverlay.Visibility = Visibility.Visible;
                NoUserRankMessage.Text = "Error loading ranking data. Please try again.";
                NoLatestMastersOverlay.Visibility = Visibility.Visible;
                NoLatestMastersMessage.Text = "Error loading latest masters. Please try again.";
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
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("FetchingUserProfile") ?? "Fetching user profile...", Owner as MainWindow);
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
                var userProfile = await _raService.GetUserProfileAsync(_settings.RaUsername, _settings.RaApiKey);

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
                    else if (recentlyPlayedGames == null)
                    {
                        // If recentlyPlayedGames is null, it indicates an API failure for this specific call
                        DebugLogger.Log($"[RA Window] Failed to load recently played games for user {_settings.RaUsername}. API returned null.");
                        UserProfileRecentlyPlayed.ItemsSource = null; // Ensure it's cleared
                        // Optionally, add a message to the ListBox itself or a small text below it.
                        // For now, just clear it and log.
                    }
                    else // recentlyPlayedGames is not null but empty
                    {
                        UserProfileRecentlyPlayed.ItemsSource = null; // No recently played games
                    }

                    NoProfileOverlay.Visibility = Visibility.Collapsed;
                }
                else
                {
                    // If userProfile is null, something went wrong with the main profile fetch
                    NoProfileOverlay.Visibility = Visibility.Visible;
                    // Update messages for general API failure
                    NoProfileMainMessage.Text = "Failed to load user profile.";
                    NoProfileSubMessage.Text = "Please check your RetroAchievements credentials or try again later.";
                }
            }
            catch (RaUnauthorizedException)
            {
                NoProfileOverlay.Visibility = Visibility.Visible;
                NoProfileMainMessage.Text = UnauthorizedMessage;
                NoProfileSubMessage.Text = "Please configure your credentials in the RetroAchievements settings.";
            }
            catch (Exception ex)
            {
                NoProfileOverlay.Visibility = Visibility.Visible;
                // Update messages for exception
                NoProfileMainMessage.Text = "An error occurred while loading user profile.";
                NoProfileSubMessage.Text = "Please try again or check your internet connection.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to load user profile for {_settings.RaUsername}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        });
    }

    private async Task LoadUnlocksByDateAsync()
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("FetchingEarnedAchievementsByDate") ?? "Fetching earned achievements by date...", Owner as MainWindow);
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
                var unlocks = await _raService.GetAchievementsEarnedBetweenAsync(_settings.RaUsername, _settings.RaApiKey, fromDate, toDate);

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
                    // If unlocks is null, it indicates an API failure (since credentials were provided)
                    NoUnlocksMessage.Text = unlocks == null
                        ? "Failed to load unlocks. Please check your RetroAchievements credentials or try again later."
                        : "No unlocks found for the selected date range.";
                }
            }
            catch (RaUnauthorizedException)
            {
                NoUnlocksOverlay.Visibility = Visibility.Visible;
                NoUnlocksMessage.Text = UnauthorizedMessage;
            }
            catch (Exception ex)
            {
                UnlocksDataGrid.ItemsSource = null;
                TotalUnlocksInRangeText.Text = "0";
                TotalPointsEarnedInRangeText.Text = "0";
                NoUnlocksOverlay.Visibility = Visibility.Visible; // Show overlay on error
                NoUnlocksMessage.Text = "An error occurred while loading unlocks. Please try again.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to load unlocks by date for user {_settings.RaUsername}");
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

                // Notify user
                UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("FetchingUnlocks") ?? "Fetching unlocks...", Owner as MainWindow);

                return; // Exit without fetching
            }

            // Proceed with loading
            try
            {
                await LoadUnlocksByDateAsync();
            }
            catch (RaUnauthorizedException)
            {
                // Handled by LoadUnlocksByDateAsync
            }
            catch (Exception ex)
            {
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to fetch unlocks by date");
                DebugLogger.Log($"[RA Window] Failed to fetch unlocks by date for user {_settings.RaUsername}: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to fetch unlocks by date");
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
                    // Notify user
                    UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("ResettingDatesAndFetchingUnlocks") ?? "Resetting dates and fetching unlocks...", Owner as MainWindow);
                    await LoadUnlocksByDateAsync(); // Automatically fetch for the new date range
                }
                catch (RaUnauthorizedException)
                {
                    // Handled by LoadUnlocksByDateAsync
                }
                catch (Exception ex)
                {
                    // Notify developer
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to reset date range");
                    DebugLogger.Log($"[RA Window] Failed to reset date range for user {_settings.RaUsername}: {ex.Message}");
                }
            });
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to reset date range");
            DebugLogger.Log($"[RA Window] Failed to reset date range for user {_settings.RaUsername}: {ex.Message}");
        }
    }

    private async Task LoadUserProgressAsync()
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("FetchingUserCompletionProgress") ?? "Fetching user completion progress...", Owner as MainWindow);
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
                var userProgressList = await _raService.GetUserCompletionProgressAsync(_settings.RaUsername, _settings.RaApiKey);

                if (userProgressList is { Count: > 0 })
                {
                    UserProgressDataGrid.ItemsSource = userProgressList;
                    NoUserProgressOverlay.Visibility = Visibility.Collapsed;
                }
                else
                {
                    UserProgressDataGrid.ItemsSource = null;
                    NoUserProgressOverlay.Visibility = Visibility.Visible;
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
                NoUserProgressOverlay.Visibility = Visibility.Visible;
                NoUserProgressMainMessage.Text = UnauthorizedMessage;
                NoUserProgressSubMessage.Text = "Please configure your credentials in the RetroAchievements settings.";
            }
            catch (Exception ex)
            {
                NoUserProgressOverlay.Visibility = Visibility.Visible;
                NoUserProgressMainMessage.Text = "An error occurred while loading user completion progress.";
                NoUserProgressSubMessage.Text = "Please try again or check your internet connection.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to load user completion progress for user {_settings.RaUsername}");
                DebugLogger.Log($"[RA Window] Failed to load user completion progress for user {_settings.RaUsername}: {ex.Message}");
            }
            finally
            {
                LoadingOverlay.Visibility = Visibility.Collapsed;
            }
        });
    }
}