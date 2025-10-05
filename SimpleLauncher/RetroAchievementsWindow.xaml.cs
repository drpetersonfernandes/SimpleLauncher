using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
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
            // Removed: Concurrent loading of all data.
            // TabControl_SelectionChanged will handle loading on tab selection (including initial load for the default tab).
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Failed to initialize RetroAchievementsWindow.");
        }
    }

    // New event handler for tab selection
    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source is TabControl { SelectedItem: TabItem selectedTab })
        {
            var header = selectedTab.Header.ToString();
            switch (header)
            {
                case "Achievements":
                    _ = LoadAchievementsAsync();
                    break;
                case "Game Info":
                    _ = LoadGameInfoAsync();
                    break;
                case "Rankings":
                    _ = LoadRankingsAsync();
                    break;
                case "My Profile":
                    _ = LoadUserProfileAsync();
                    break;
            }
        }
    }

    private async Task LoadAchievementsAsync()
    {
        try
        {
            LoadingOverlay.Visibility = Visibility.Visible;

            // Use the injected service
            var (progress, achievements) = await _raService.GetGameInfoAndUserProgress(_gameId, _settings.RaUsername, _settings.RaApiKey);

            if (achievements is { Count: > 0 } && progress != null)
            {
                // Update progress summary header
                GameProgressTitle.Text = string.IsNullOrWhiteSpace(progress.GameTitle) ? "Unknown Game" : progress.GameTitle;
                GameProgressConsole.Text = string.IsNullOrWhiteSpace(progress.ConsoleName) ? "" : $"({progress.ConsoleName})";

                if (!string.IsNullOrEmpty(progress.GameIconUrl))
                {
                    GameCoverImage.Source = new BitmapImage(new Uri(progress.GameIconUrl));
                }

                // Bind achievements to DataGrid
                Dispatcher.Invoke(() =>
                {
                    // Update progress bars and stats
                    UpdateProgressDisplay(progress);

                    AchievementsDataGrid.ItemsSource = achievements;
                    NoAchievementsOverlay.Visibility = Visibility.Collapsed;
                });
            }
            else
            {
                Dispatcher.Invoke(() =>
                {
                    NoAchievementsOverlay.Visibility = Visibility.Visible;
                });
            }
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() =>
            {
                NoAchievementsOverlay.Visibility = Visibility.Visible;
            });

            _ = LogErrors.LogErrorAsync(ex, $"Failed to load achievements for game ID: {_gameId}");
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
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
                var casualText = progress.UserCompletion.Trim('%');
                try
                {
                    _ = double.TryParse(casualText, out casualCompletion);
                }
                catch (Exception ex)
                {
                    _ = LogErrors.LogErrorAsync(ex, $"Failed to parse casual completion percentage: {casualText}");
                }
            }

            if (!string.IsNullOrWhiteSpace(progress.UserCompletionHardcore))
            {
                var hardcoreText = progress.UserCompletionHardcore.Trim('%');
                _ = double.TryParse(hardcoreText, out hardcoreCompletion);
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
            TotalPointsEarnedValue.Text = $"{progress.PointsEarned:N0}"; // RENAMED
            TruePointsEarnedValue.Text = $"{progress.PointsEarnedHardcore:N0}"; // ADDED

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

            if (DateTime.TryParse(progress.HighestAwardDate, out var awardDate))
            {
                HighestAwardDateText.Text = awardDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
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
            TotalPointsEarnedValue.Text = "0"; // RENAMED
            TruePointsEarnedValue.Text = "0"; // ADDED
            HighestAwardKindText.Text = "N/A";
            HighestAwardDateText.Text = "N/A";
            HighestAwardIcon.Visibility = Visibility.Collapsed; // Ensure icon is hidden on error

            _ = LogErrors.LogErrorAsync(ex, "Failed to parse progress data for achievements display");
        }
    }

    private string CapitalizeFirstLetter(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        return char.ToUpper(input[0], CultureInfo.InvariantCulture) + input.Substring(1);
    }

    private async Task LoadGameInfoAsync()
    {
        try
        {
            LoadingOverlay.Visibility = Visibility.Visible;

            // Use the injected service
            var gameInfo = await _raService.GetGameExtendedInfoAsync(_gameId, _settings.RaUsername, _settings.RaApiKey);
            if (gameInfo != null)
            {
                // Game header
                GameInfoTitle.Text = string.IsNullOrWhiteSpace(gameInfo.Title) ? "Unknown Title" : gameInfo.Title;
                GameInfoConsole.Text = string.IsNullOrWhiteSpace(gameInfo.ConsoleName) ? "Unknown Console" : gameInfo.ConsoleName;

                // Load game icon
                if (!string.IsNullOrEmpty(gameInfo.ImageIcon))
                {
                    try
                    {
                        GameInfoIcon.Source = new BitmapImage(new Uri($"https://retroachievements.org{gameInfo.ImageIcon}"));
                    }
                    catch
                    {
                        GameInfoIcon.Source = null;
                    }
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
            }
        }
        catch (Exception ex)
        {
            NoGameInfoOverlay.Visibility = Visibility.Visible;
            await LogErrors.LogErrorAsync(ex, $"Failed to load extended game info for game ID: {_gameId}");
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private string FormatDateString(string dateString)
    {
        if (DateTime.TryParse(dateString, out var date))
        {
            return date.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        }

        return dateString;
    }

    private async Task LoadRankingsAsync()
    {
        try
        {
            LoadingOverlay.Visibility = Visibility.Visible;

            // Use the injected service
            var rankings = await _raService.GetGameRankAndScoreAsync(_gameId, _settings.RaUsername, _settings.RaApiKey);

            if (rankings != null && rankings.Count > 0)
            {
                // Assign ranks based on the order (which represents the actual ranking from the API)
                for (var i = 0; i < rankings.Count; i++)
                {
                    rankings[i].Rank = i + 1;
                }

                RankingsDataGrid.ItemsSource = rankings;
                NoRankingsOverlay.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoRankingsOverlay.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            NoRankingsOverlay.Visibility = Visibility.Visible;
            await LogErrors.LogErrorAsync(ex, $"Failed to load rankings for game ID: {_gameId}");
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private async Task LoadUserProfileAsync()
    {
        try
        {
            LoadingOverlay.Visibility = Visibility.Visible;

            // Use the injected service
            var userProfile = await _raService.GetUserProfileAsync(_settings.RaUsername, _settings.RaApiKey);
            if (userProfile != null)
            {
                // Basic profile info
                if (!string.IsNullOrEmpty(userProfile.UserPic))
                {
                    UserProfilePic.Source = new BitmapImage(new Uri($"https://retroachievements.org{userProfile.UserPic}"));
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
                UserProfileMemberSince.Text = string.IsNullOrWhiteSpace(userProfile.MemberSince) ? "Unknown" : userProfile.MemberSince;

                // Additional details
                UserProfileId.Text = userProfile.Id.ToString(CultureInfo.InvariantCulture);
                UserProfileContributions.Text = $"{userProfile.ContribCount} contributions ({userProfile.ContribYield:N0} points)";
                UserProfileSoftcorePoints.Text = userProfile.TotalSoftcorePoints.ToString("N0", CultureInfo.InvariantCulture);
                UserProfilePermissions.Text = GetPermissionDescription(userProfile.Permissions);
                UserProfileStatus.Text = userProfile.Untracked == 1 ? "Untracked" : "Tracked";
                UserProfileProfileId.Text = string.IsNullOrWhiteSpace(userProfile.Uuid) ? "N/A" : userProfile.Uuid;
                UserProfileWallActive.Text = userProfile.UserWallActive ? "Yes" : "No";

                // Recently played
                UserProfileRecentlyPlayed.ItemsSource = userProfile.RecentlyPlayed;

                NoProfileOverlay.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoProfileOverlay.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            NoProfileOverlay.Visibility = Visibility.Visible;
            await LogErrors.LogErrorAsync(ex, $"Failed to load user profile for {_settings.RaUsername}");
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private string GetPermissionDescription(int permissions)
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

    private void RankingsDataGrid_OnSorting(object sender, DataGridSortingEventArgs e)
    {
        // Prevent default sorting and handle it manually if needed
        // This allows us to maintain our custom ranking while still allowing column sorting
        if (e.Column.SortMemberPath == "Rank")
        {
            // Keep default sorting for rank
            return;
        }

        // For other columns, let the default sorting work
        // The rankings list will maintain its original order for rank display
    }

    private async void RefreshRankings_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            LoadingOverlay.Visibility = Visibility.Visible;
            await LoadRankingsAsync();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Failed to refresh rankings");
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }
}