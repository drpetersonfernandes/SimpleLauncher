using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Avalonia.Views;

public partial class RetroAchievementsForAGameWindow : Window, IDisposable
{
    private readonly AvaloniaRetroAchievementsForAGameViewModel _viewModel;
    private readonly HttpClient _httpClient = new();

    public RetroAchievementsForAGameWindow(AvaloniaRetroAchievementsForAGameViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = _viewModel;

        Loaded += async (_, _) =>
        {
            await LoadGameAchievementsAsync();
            await LoadGameCoverImageAsync();
        };
    }

    public void Initialize(int gameId, string gameTitleForDisplay)
    {
        _viewModel.Initialize(gameId, gameTitleForDisplay);
    }

    private async void TabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (TabControl?.SelectedItem is not TabItem selectedTab) return;

            var tag = selectedTab.Tag?.ToString();
            switch (tag)
            {
                case "Achievements":
                    await LoadGameAchievementsAsync();
                    break;
                case "GameInfo":
                    await LoadGameInfoAsync();
                    break;
                case "GameRanking":
                    await _viewModel.LoadGameRankingAsync();
                    break;
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

    private async Task LoadGameAchievementsAsync()
    {
        await _viewModel.LoadGameAchievementsAsync();
        await LoadGameCoverImageAsync();
    }

    private async Task LoadGameInfoAsync()
    {
        await _viewModel.LoadGameInfoAsync();

        if (_viewModel.IsGameInfoLoaded)
        {
            await LoadGameInfoImagesAsync();
        }
    }

    private async Task LoadUserProfileAsync()
    {
        await _viewModel.LoadUserProfileAsync();

        if (_viewModel.ProfileImageUrl != null)
        {
            try
            {
                var imageBytes = await _httpClient.GetByteArrayAsync(_viewModel.ProfileImageUrl);
                using var ms = new MemoryStream(imageBytes);
                UserProfilePic2.Source = new Bitmap(ms);
            }
            catch
            {
                UserProfilePic2.Source = null;
            }
        }
        else
        {
            UserProfilePic2.Source = null;
        }
    }

    private async Task LoadGameCoverImageAsync()
    {
        if (!string.IsNullOrEmpty(_viewModel.GameIconUrl))
        {
            try
            {
                var imageBytes = await _httpClient.GetByteArrayAsync(_viewModel.GameIconUrl);
                using var ms = new MemoryStream(imageBytes);
                GameCoverImage.Source = new Bitmap(ms);
            }
            catch
            {
                GameCoverImage.Source = null;
            }
        }
    }

    private async Task LoadGameInfoImagesAsync()
    {
        await LoadImageToControl(GameInfoImageIcon, _viewModel.GameIconUrl);
        await LoadImageToControl(GameInfoTitleImage, _viewModel.GameTitleImageUrl);
        await LoadImageToControl(GameInfoIngameImage, _viewModel.GameIngameImageUrl);
        await LoadImageToControl(GameInfoBoxArtImage, _viewModel.GameBoxArtUrl);
    }

    private async Task LoadImageToControl(Image imageControl, string? imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl))
        {
            imageControl.Source = null;
            return;
        }

        try
        {
            var imageBytes = await _httpClient.GetByteArrayAsync(imageUrl);
            using var ms = new MemoryStream(imageBytes);
            imageControl.Source = new Bitmap(ms);
        }
        catch
        {
            imageControl.Source = null;
        }
    }

    private void GameImage_Click(object? sender, global::Avalonia.Input.PointerPressedEventArgs e)
    {
        if (sender is not Image clickedImage) return;

        var imageUrl = clickedImage == GameInfoImageIcon ? _viewModel.GameIconUrl
            : clickedImage == GameInfoTitleImage ? _viewModel.GameTitleImageUrl
            : clickedImage == GameInfoIngameImage ? _viewModel.GameIngameImageUrl
            : clickedImage == GameInfoBoxArtImage ? _viewModel.GameBoxArtUrl
            : null;

        if (string.IsNullOrEmpty(imageUrl)) return;

        try
        {
            if (App.ServiceProvider.GetService(typeof(ImageViewerWindow)) is ImageViewerWindow viewer)
            {
                viewer.LoadImageUrl(new Uri(imageUrl));
                viewer.Show(this);
            }
        }
        catch
        {
            // Ignore errors
        }
    }

    private void ViewProfileOnRaButton_Click(object? sender, RoutedEventArgs e)
    {
        OpenUrlInBrowser(_viewModel.GetProfileUrl());
    }

    private void ViewGameOnRaButton_Click(object? sender, RoutedEventArgs e)
    {
        OpenUrlInBrowser(_viewModel.GetGameUrl());
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

    public void Dispose()
    {
        _httpClient.Dispose();
        GC.SuppressFinalize(this);
    }
}
