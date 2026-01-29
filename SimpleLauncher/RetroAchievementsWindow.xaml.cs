using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Managers;
using SimpleLauncher.Services;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.RetroAchievements;
using SimpleLauncher.Services.RetroAchievements.Models;

namespace SimpleLauncher;

public partial class RetroAchievementsWindow
{
    private readonly SettingsManager _settings;
    private readonly RetroAchievementsService _raService;

    // Define a constant for the unauthorized message to avoid repetition
    private const string UnauthorizedMessage = "RetroAchievements credentials invalid. Please check your username and API key in settings.";

    public RetroAchievementsWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        Owner = Application.Current.MainWindow;

        _settings = App.ServiceProvider.GetRequiredService<SettingsManager>();
        _raService = App.ServiceProvider.GetRequiredService<RetroAchievementsService>();

        Loaded += RetroAchievementsWindow_Loaded;
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source is TabControl { SelectedItem: TabItem selectedTab })
        {
            if (selectedTab is not { IsSelected: true }) return;

            // Use Tag instead of Header for language-independent tab identification
            var tag = selectedTab.Tag?.ToString();

            switch (tag)
            {
                case "MyProfile":
                    UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingUserProfile") ?? "Loading user profile...", Owner as MainWindow);
                    _ = LoadUserProfileAsync();
                    break;
                case "Unlocks":
                    UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingUserUnlocks") ?? "Loading user unlocks...", Owner as MainWindow);
                    _ = LoadUnlocksByDateAsync();
                    break;
                case "UserProgress":
                    UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingUserCompletionProgress") ?? "Loading user completion progress...", Owner as MainWindow);
                    _ = LoadUserProgressAsync();
                    break;
            }
        }
    }

    private void RetroAchievementsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        // Force load the first tab's data
        // The SelectionChanged event might not fire if the first tab is already selected
        _ = LoadUserProfileAsync();
    }

    private static void OpenUrlInBrowser(string url)
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
        var settingsWindow = new RetroAchievementsSettingsWindow(_settings)
        {
            Owner = this
        };
        settingsWindow.ShowDialog();

        // Reload current tab using Tag instead of Header
        if (TabControl.SelectedItem is TabItem selectedTab)
        {
            var tag = selectedTab.Tag?.ToString();
            switch (tag)
            {
                case "MyProfile":
                    _ = LoadUserProfileAsync();
                    break;
                case "Unlocks":
                    _ = LoadUnlocksByDateAsync();
                    break;
                case "UserProgress":
                    _ = LoadUserProgressAsync();
                    break;
            }
        }
    }

    private async Task LoadUserProfileAsync()
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("FetchingUserProfile") ?? "Fetching user profile...", Owner as MainWindow);

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
            DebugLogger.Log($"[RA Window] Fetching user profile for {_settings.RaUsername}...");
            var userProfile = await _raService.GetUserProfileAsync(_settings.RaUsername, _settings.RaApiKey);

            if (userProfile == null)
            {
                DebugLogger.Log($"[RA Window] GetUserProfileAsync returned null for user {_settings.RaUsername}");
            }

            // Fetch detailed recently played games separately (max 50 games)
            DebugLogger.Log($"[RA Window] Fetching recently played games for {_settings.RaUsername}...");
            var recentlyPlayedGames = await _raService.GetUserRecentlyPlayedGamesAsync(_settings.RaUsername, _settings.RaApiKey, 50);

            if (recentlyPlayedGames == null)
            {
                DebugLogger.Log($"[RA Window] GetUserRecentlyPlayedGamesAsync returned null for user {_settings.RaUsername}");
            }

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

                switch (recentlyPlayedGames)
                {
                    // Recently played - use the detailed list from GetUserRecentlyPlayedGamesAsync
                    case { Count: > 0 }:
                        // Ensure full URLs are used (handled in model)
                        UserProfileRecentlyPlayed.ItemsSource = recentlyPlayedGames;
                        break;
                    case null:
                        // If recentlyPlayedGames is null, it indicates an API failure for this specific call
                        DebugLogger.Log($"[RA Window] Failed to load recently played games for user {_settings.RaUsername}. API returned null.");
                        UserProfileRecentlyPlayed.ItemsSource = null; // Ensure it's cleared
                        // Optionally, add a message to the ListBox itself or a small text below it.
                        // For now, just clear it and log.
                        break;
                    // recentlyPlayedGames is not null but empty
                    default:
                        UserProfileRecentlyPlayed.ItemsSource = null; // No recently played games
                        break;
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
    }

    private async Task LoadUnlocksByDateAsync()
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("FetchingEarnedAchievementsByDate") ?? "Fetching earned achievements by date...", Owner as MainWindow);

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
            FetchUnlocksButton.IsEnabled = true; // Re-enable button
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
            DebugLogger.Log($"[RA Window] Fetching unlocks for {_settings.RaUsername} from {fromDate:yyyy-MM-dd} to {toDate:yyyy-MM-dd}...");
            var unlocks = await _raService.GetAchievementsEarnedBetweenAsync(_settings.RaUsername, _settings.RaApiKey, fromDate, toDate);

            DebugLogger.Log($"[RA Window] GetAchievementsEarnedBetweenAsync returned {unlocks?.Count ?? 0} unlocks.");

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
    }

    private async void FetchUnlocksClickAsync(object sender, RoutedEventArgs e)
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
            await LoadUnlocksByDateAsync();
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to fetch unlocks by date");
            DebugLogger.Log($"[RA Window] Failed to fetch unlocks by date for user {_settings.RaUsername}: {ex.Message}");
        }
    }

    private async void ResetDatesClickAsync(object sender, RoutedEventArgs e)
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
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to reset date range");
            DebugLogger.Log($"[RA Window] Failed to reset date range for user {_settings.RaUsername}: {ex.Message}");
        }
    }

    private async Task LoadUserProgressAsync()
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("FetchingUserCompletionProgress") ?? "Fetching user completion progress...", Owner as MainWindow);

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
    }
}