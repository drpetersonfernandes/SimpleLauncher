using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Managers;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class AchievementsWindow
{
    private readonly string _gameIdentifier;
    private readonly string _systemName;
    private readonly SettingsManager _settings;

    public AchievementsWindow(string gameIdentifier, string systemName)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        Owner = Application.Current.MainWindow;

        _gameIdentifier = gameIdentifier;
        _systemName = systemName;
        _settings = App.ServiceProvider.GetRequiredService<SettingsManager>();

        Loaded += AchievementsWindow_Loaded;
    }

    private async void AchievementsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            GameTitleTextBlock.Text = _gameIdentifier;
            LoadingOverlay.Visibility = Visibility.Visible;

            await LoadAchievementsAsync();

            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Failed to load achievements.");
        }
    }

    private async Task LoadAchievementsAsync()
    {
        try
        {
            var (progress, achievements) = await RetroAchievementsService.GetUserGameProgressAsync(_gameIdentifier, _systemName, _settings.RaUsername, _settings.RaApiKey);

            if (achievements != null && achievements.Count > 0 && progress != null)
            {
                GameTitleTextBlock.Text = progress.GameTitle;
                ProgressTextBlock.Text = $"Progress: {progress.AchievementsEarned}/{progress.TotalAchievements} ({progress.PointsEarned}/{progress.TotalPoints} Points)";

                // Load game icon from API response
                if (!string.IsNullOrEmpty(progress.GameIconUrl))
                {
                    GameCoverImage.Source = new BitmapImage(new Uri(progress.GameIconUrl));
                }

                AchievementsDataGrid.ItemsSource = achievements;
                NoResultsOverlay.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoResultsOverlay.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            NoResultsOverlay.Visibility = Visibility.Visible;
            await LogErrors.LogErrorAsync(ex, $"Failed to load achievements for game: {_gameIdentifier}");
        }
    }
}