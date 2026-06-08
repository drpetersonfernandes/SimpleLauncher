using System.Diagnostics;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.LoadingInterface;
using SimpleLauncher.Core.Services.RetroAchievements;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.ViewModels;

#nullable enable

namespace SimpleLauncher;

public partial class RetroAchievementsWindow : ILoadingState
{
    private readonly RetroAchievementsViewModel _viewModel;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;

    public RetroAchievementsWindow(PlaySoundEffects playSoundEffects, ILogErrors logErrors, SettingsManager settings, RetroAchievementsService raService)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        Owner = Application.Current.MainWindow;

        _playSoundEffects = playSoundEffects;
        _logErrors = logErrors;
        _messageBox = App.ServiceProvider.GetRequiredService<IMessageBoxLibraryService>();

        _viewModel = new RetroAchievementsViewModel(
            logErrors,
            _messageBox,
            App.ServiceProvider.GetRequiredService<IResourceProvider>(),
            settings,
            raService,
            playSoundEffects);

        DataContext = _viewModel;

        Loaded += RetroAchievementsWindow_Loaded;

        Loaded += (_, _) =>
        {
            LoadingOverlay.ApplyTemplate();
            if (LoadingOverlay.Template.FindName("PART_EmergencyReturnButton", LoadingOverlay) is Button emergencyBtn)
            {
                emergencyBtn.Click += EmergencyOverlayRelease_Click;
            }
        };
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.Source is TabControl { SelectedItem: TabItem selectedTab })
        {
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
    }

    private void RetroAchievementsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _ = LoadUserProfileAsync();
    }

    private async Task LoadUserProfileAsync()
    {
        (Owner as MainWindow)?.UpdateStatusBarService.UpdateContent(
            (string)Application.Current.TryFindResource("FetchingUserProfile") ?? "Fetching user profile...");
        SetLoadingState(true);

        await _viewModel.LoadUserProfileAsync();

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

        await _viewModel.LoadUnlocksByDateAsync();

        // Bind unlocks data
        UnlocksDataGrid.ItemsSource = _viewModel.Unlocks;

        SetLoadingState(false);
    }

    private async void FetchUnlocksClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            await _viewModel.FetchUnlocksCommand.ExecuteAsync(null);
            UnlocksDataGrid.ItemsSource = _viewModel.Unlocks;
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

        SetLoadingState(false);
    }

    private async void OpenUrlInBrowser(string url)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Error opening URL: {url}");
            await _messageBox.UnableToOpenLinkMessageBox();
        }
    }

    private void ViewProfileOnRaButton_Click(object sender, RoutedEventArgs e)
    {
        var url = _viewModel.GetProfileUrl();
        if (!string.IsNullOrWhiteSpace(url))
        {
            OpenUrlInBrowser(url);
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

    public void SetLoadingState(bool isLoading, string message = null)
    {
        Dispatcher.Invoke(() =>
        {
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            if (isLoading)
            {
                LoadingOverlay.Content = message ?? (string)Application.Current.TryFindResource("Loading") ?? "Loading...";
            }
        });
    }

    private void EmergencyOverlayRelease_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();
        LoadingOverlay.Visibility = Visibility.Collapsed;

        DebugLogger.Log("[Emergency] User forced overlay dismissal in RetroAchievements Window.");
        (Owner as MainWindow)?.UpdateStatusBarService.UpdateContent("Emergency reset performed.");
    }
}