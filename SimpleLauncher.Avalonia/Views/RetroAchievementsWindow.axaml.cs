using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Avalonia.Views;

public partial class RetroAchievementsWindow : Window
{
    private static readonly HttpClient HttpClient = new();
    private readonly AvaloniaRetroAchievementsViewModel _viewModel;

    public RetroAchievementsWindow() : this(App.ServiceProvider.GetRequiredService<AvaloniaRetroAchievementsViewModel>()) { }

    public RetroAchievementsWindow(AvaloniaRetroAchievementsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += async (_, _) => await LoadUserProfileAsync();
    }

    private async void TabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (TabControl?.SelectedItem is not TabItem selectedTab) return;

            var tag = selectedTab.Tag?.ToString();
            switch (tag)
            {
                case "MyProfile":
                    await LoadUserProfileAsync();
                    break;
                case "Unlocks":
                    await _viewModel.LoadUnlocksByDateAsync();
                    break;
                case "UserProgress":
                    await _viewModel.LoadUserProgressAsync();
                    break;
            }
        }
        catch (Exception ex)
        {
            var logErrors = App.ServiceProvider.GetService<ILogErrors>();
            logErrors?.LogAndForget(ex, "Error in TabControl_SelectionChanged");
        }
    }

    private async Task LoadUserProfileAsync()
    {
        await _viewModel.LoadUserProfileAsync();

        if (_viewModel.ProfileImageUrl != null)
        {
            try
            {
                var imageBytes = await HttpClient.GetByteArrayAsync(_viewModel.ProfileImageUrl);
                using var ms = new MemoryStream(imageBytes);
                UserProfilePic.Source = new Bitmap(ms);
            }
            catch
            {
                UserProfilePic.Source = null;
            }
        }
        else
        {
            UserProfilePic.Source = null;
        }
    }

    private void ViewProfileOnRaButton_Click(object? sender, RoutedEventArgs e)
    {
        var url = _viewModel.GetProfileUrl();
        if (!string.IsNullOrWhiteSpace(url))
        {
            OpenUrlInBrowser(url);
        }
    }

    private void OpenRaSettings_Click(object? sender, RoutedEventArgs e)
    {
        // TODO: Open RetroAchievementsSettingsWindow when ported
    }

    private static void OpenUrlInBrowser(string url)
    {
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch
        {
            // Ignore errors
        }
    }
}
