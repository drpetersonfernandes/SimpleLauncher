using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.RetroAchievements;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.ViewModels;

#nullable enable

namespace SimpleLauncher;

/// <summary>
/// Window for browsing RetroAchievements user profile, unlocks, and completion progress.
/// </summary>
public partial class RetroAchievementsWindow : ILoadingState
{
    private readonly RetroAchievementsViewModel _viewModel;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IDebugLogger _debugLogger;
    private Button? _emergencyReturnButton;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetroAchievementsWindow"/> class.
    /// </summary>
    /// <param name="playSoundEffects">The sound effects service.</param>
    /// <param name="logErrors">The error logging service.</param>
    /// <param name="debugLogger">The debug logger.</param>
    /// <param name="settings">The application settings manager.</param>
    /// <param name="raService">The RetroAchievements API service.</param>
    public RetroAchievementsWindow(PlaySoundEffects playSoundEffects, ILogErrors logErrors, IDebugLogger debugLogger, SettingsManager settings, RetroAchievementsService raService)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        Owner = Application.Current.MainWindow;

        _playSoundEffects = playSoundEffects;
        _logErrors = logErrors;
        _debugLogger = debugLogger ?? throw new ArgumentNullException(nameof(debugLogger));
        _messageBox = App.ServiceProvider.GetRequiredService<IMessageBoxLibraryService>();

        _viewModel = new RetroAchievementsViewModel(
            logErrors,
            _messageBox,
            App.ServiceProvider.GetRequiredService<IResourceProvider>(),
            settings,
            raService,
            debugLogger);

        DataContext = _viewModel;

        Loaded += RetroAchievementsWindow_Loaded;

        Loaded += (_, _) =>
        {
            LoadingOverlay.ApplyTemplate();
            if (LoadingOverlay.Template.FindName("PART_EmergencyReturnButton", LoadingOverlay) is Button emergencyBtn)
            {
                _emergencyReturnButton = emergencyBtn;
                emergencyBtn.Click += EmergencyOverlayRelease_Click;
            }
        };

        Closing += (_, _) =>
        {
            if (_emergencyReturnButton != null)
            {
                _emergencyReturnButton.Click -= EmergencyOverlayRelease_Click;
                _emergencyReturnButton = null;
            }
        };
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.Source is not TabControl { SelectedItem: TabItem selectedTab })
                return;

            if (selectedTab is not { IsSelected: true }) return;

            var tag = selectedTab.Tag?.ToString();
            switch (tag)
            {
                case "MyProfile":
                    _playSoundEffects.PlayNotificationSound();
                    (Owner as MainWindow)?.UpdateStatusBarService.UpdateContent(
                        (string)Application.Current.TryFindResource("LoadingUserProfile") ?? "Loading user profile...");
                    _ = LoadUserProfileAsync();
                    break;
                case "Unlocks":
                    _playSoundEffects.PlayNotificationSound();
                    (Owner as MainWindow)?.UpdateStatusBarService.UpdateContent(
                        (string)Application.Current.TryFindResource("LoadingUserUnlocks") ?? "Loading user unlocks...");
                    _ = LoadUnlocksByDateAsync();
                    break;
                case "UserProgress":
                    _playSoundEffects.PlayNotificationSound();
                    (Owner as MainWindow)?.UpdateStatusBarService.UpdateContent(
                        (string)Application.Current.TryFindResource("LoadingUserCompletionProgress") ?? "Loading user completion progress...");
                    _ = LoadUserProgressAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in TabControl_SelectionChanged of RetroAchievementsWindow.");
        }
    }

    private void RetroAchievementsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _ = LoadUserProfileAsync();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in RetroAchievementsWindow_Loaded.");
        }
    }

    private async Task LoadUserProfileAsync()
    {
        (Owner as MainWindow)?.UpdateStatusBarService.UpdateContent(
            (string)Application.Current.TryFindResource("FetchingUserProfile") ?? "Fetching user profile...");
        SetLoadingState(true);

        await _viewModel.LoadUserProfileAsync();

        // Toggle overlays
        UserProfilePanel.Visibility = _viewModel.NoProfileVisible ? Visibility.Collapsed : Visibility.Visible;
        NoProfileOverlay.Visibility = _viewModel.NoProfileVisible ? Visibility.Visible : Visibility.Collapsed;

        if (_viewModel.NoProfileVisible)
        {
            NoProfileMainMessage.Text = _viewModel.NoProfileMainMessage;
            NoProfileSubMessage.Text = _viewModel.NoProfileSubMessage;
            SetLoadingState(false);
            return;
        }

        // Update profile header
        UserProfileUser.Text = _viewModel.ProfileUser;
        UserProfileMotto.Text = _viewModel.ProfileMotto;
        UserProfileRichPresence.Text = _viewModel.ProfileRichPresence;

        // Update stats
        PointsValue.Text = _viewModel.ProfilePoints;
        TruePointsValue.Text = _viewModel.ProfileTruePoints;
        RankValue.Text = _viewModel.ProfileRank;

        // Update detailed info
        UserProfileMemberSince.Text = _viewModel.ProfileMemberSince;
        UserProfileId.Text = _viewModel.ProfileId;
        UserProfileContributions.Text = _viewModel.ProfileContributions;
        UserProfileSoftcorePoints.Text = _viewModel.ProfileSoftcorePoints;
        UserProfilePermissions.Text = _viewModel.ProfilePermissions;
        UserProfileStatus.Text = _viewModel.ProfileStatus;
        UserProfileProfileId.Text = _viewModel.ProfileProfileId;
        UserProfileWallActive.Text = _viewModel.ProfileWallActive;

        // Update WPF-specific UI (profile image as BitmapImage)
        if (_viewModel.ProfileImageUrl != null)
        {
            UserProfilePic.Source = new BitmapImage(new Uri(_viewModel.ProfileImageUrl));
        }
        else
        {
            UserProfilePic.Source = null;
        }

        // Bind recently played games
        UserProfileRecentlyPlayed.ItemsSource = _viewModel.RecentlyPlayedGames;

        SetLoadingState(false);
    }

    private async Task LoadUnlocksByDateAsync()
    {
        (Owner as MainWindow)?.UpdateStatusBarService.UpdateContent(
            (string)Application.Current.TryFindResource("FetchingEarnedAchievementsByDate") ?? "Fetching earned achievements by date...");
        SetLoadingState(true);

        // Sync DatePickers with ViewModel
        FromDatePicker.SelectedDate = _viewModel.FromDate;
        ToDatePicker.SelectedDate = _viewModel.ToDate;

        await _viewModel.LoadUnlocksByDateAsync();

        // Bind unlocks data
        UnlocksDataGrid.ItemsSource = _viewModel.Unlocks;

        // Update totals
        TotalUnlocksInRangeText.Text = _viewModel.TotalUnlocksInRange;
        TotalPointsEarnedInRangeText.Text = _viewModel.TotalPointsEarnedInRange;

        // Toggle overlay
        NoUnlocksOverlay.Visibility = _viewModel.NoUnlocksVisible ? Visibility.Visible : Visibility.Collapsed;
        if (_viewModel.NoUnlocksVisible)
        {
            NoUnlocksMessage.Text = _viewModel.NoUnlocksMessage;
        }

        FetchUnlocksButton.IsEnabled = _viewModel.FetchUnlocksEnabled;

        SetLoadingState(false);
    }

    private async void FetchUnlocksClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            // Sync ViewModel with DatePickers before fetching
            _viewModel.FromDate = FromDatePicker.SelectedDate;
            _viewModel.ToDate = ToDatePicker.SelectedDate;

            await _viewModel.FetchUnlocksCommand.ExecuteAsync(null);
            UnlocksDataGrid.ItemsSource = _viewModel.Unlocks;
            TotalUnlocksInRangeText.Text = _viewModel.TotalUnlocksInRange;
            TotalPointsEarnedInRangeText.Text = _viewModel.TotalPointsEarnedInRange;
            NoUnlocksOverlay.Visibility = _viewModel.NoUnlocksVisible ? Visibility.Visible : Visibility.Collapsed;
            if (_viewModel.NoUnlocksVisible)
            {
                NoUnlocksMessage.Text = _viewModel.NoUnlocksMessage;
            }
            FetchUnlocksButton.IsEnabled = _viewModel.FetchUnlocksEnabled;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Failed to fetch unlocks by date");
        }
    }

    private async void ResetDatesClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            (Owner as MainWindow)?.UpdateStatusBarService.UpdateContent(
                (string)Application.Current.TryFindResource("ResettingDatesAndFetchingUnlocks") ?? "Resetting dates and fetching unlocks...");

            await _viewModel.ResetDatesCommand.ExecuteAsync(null);
            UnlocksDataGrid.ItemsSource = _viewModel.Unlocks;
            TotalUnlocksInRangeText.Text = _viewModel.TotalUnlocksInRange;
            TotalPointsEarnedInRangeText.Text = _viewModel.TotalPointsEarnedInRange;
            NoUnlocksOverlay.Visibility = _viewModel.NoUnlocksVisible ? Visibility.Visible : Visibility.Collapsed;
            if (_viewModel.NoUnlocksVisible)
            {
                NoUnlocksMessage.Text = _viewModel.NoUnlocksMessage;
            }
            FromDatePicker.SelectedDate = _viewModel.FromDate;
            ToDatePicker.SelectedDate = _viewModel.ToDate;
            FetchUnlocksButton.IsEnabled = _viewModel.FetchUnlocksEnabled;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Failed to reset date range");
        }
    }

    private async Task LoadUserProgressAsync()
    {
        (Owner as MainWindow)?.UpdateStatusBarService.UpdateContent(
            (string)Application.Current.TryFindResource("FetchingUserCompletionProgress") ?? "Fetching user completion progress...");
        SetLoadingState(true);

        await _viewModel.LoadUserProgressAsync();

        // Bind user progress data
        UserProgressDataGrid.ItemsSource = _viewModel.UserProgress;

        // Toggle overlay
        NoUserProgressOverlay.Visibility = _viewModel.NoUserProgressVisible ? Visibility.Visible : Visibility.Collapsed;
        if (_viewModel.NoUserProgressVisible)
        {
            NoUserProgressMainMessage.Text = _viewModel.NoUserProgressMainMessage;
            NoUserProgressSubMessage.Text = _viewModel.NoUserProgressSubMessage;
        }

        SetLoadingState(false);
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
            _logErrors.LogAndForget(ex, $"Error opening URL: {url}");
            await _messageBox.UnableToOpenLinkMessageBoxAsync();
        }
    }

    private void ViewProfileOnRaButton_Click(object sender, RoutedEventArgs e)
    {
        var url = _viewModel.GetProfileUrl();
        if (!string.IsNullOrWhiteSpace(url))
        {
            OpenUrlInBrowserAsync(url);
        }
    }

    private void OpenRaSettings_Click(object sender, RoutedEventArgs e)
    {
        var settingsWindow = App.ServiceProvider.GetRequiredService<RetroAchievementsSettingsWindow>();
        settingsWindow.Owner = this;
        _playSoundEffects.PlayNotificationSound();
        settingsWindow.ShowDialog();

        // Reload current tab
        if (TabControl.SelectedItem is TabItem selectedTab)
        {
            var tag = selectedTab.Tag?.ToString();
            switch (tag)
            {
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

    /// <summary>
    /// Toggles the loading overlay with an optional message.
    /// </summary>
    /// <param name="isLoading">Whether to show or hide the loading overlay.</param>
    /// <param name="message">Optional message to display while loading.</param>
    public void SetLoadingState(bool isLoading, string? message = null)
    {
        Dispatcher.Invoke(() =>
        {
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            if (isLoading)
            {
                LoadingOverlay.Content = message;
            }
        });
    }

    private void EmergencyOverlayRelease_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();
        LoadingOverlay.Visibility = Visibility.Collapsed;

        _debugLogger.Log("[Emergency] User forced overlay dismissal in RetroAchievements Window.");
        (Owner as MainWindow)?.UpdateStatusBarService.UpdateContent("Emergency reset performed.");
    }
}