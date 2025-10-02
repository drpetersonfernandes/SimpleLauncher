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
    private readonly int _gameId;
    private readonly string _gameTitleForDisplay;
    private readonly SettingsManager _settings;

    public AchievementsWindow(int gameId, string gameTitleForDisplay)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        Owner = Application.Current.MainWindow;

        _gameId = gameId;
        _gameTitleForDisplay = gameTitleForDisplay;
        _settings = App.ServiceProvider.GetRequiredService<SettingsManager>();

        Loaded += AchievementsWindow_Loaded;
    }

    private async void AchievementsWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            GameTitleTextBlock.Text = _gameTitleForDisplay;
            LoadingOverlay.Visibility = Visibility.Visible;

            // Fire off all data loading tasks concurrently
            var achievementsTask = LoadAchievementsAsync();
            var gameInfoTask = LoadGameInfoAsync();
            var rankingsTask = LoadRankingsAsync();
            var userProfileTask = LoadUserProfileAsync();

            // Wait for all tasks to complete
            await Task.WhenAll(achievementsTask, gameInfoTask, rankingsTask, userProfileTask);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Failed to load data for AchievementsWindow.");
        }
        finally
        {
            LoadingOverlay.Visibility = Visibility.Collapsed;
        }
    }

    private async Task LoadAchievementsAsync()
    {
        try
        {
            var (progress, achievements) = await RetroAchievementsService.GetUserGameProgressByGameIdAsync(_gameId, _settings.RaUsername, _settings.RaApiKey);

            if (achievements != null && achievements.Count > 0 && progress != null)
            {
                // This data is shared across the window, so update it here
                GameTitleTextBlock.Text = progress.GameTitle;
                ProgressTextBlock.Text = $"Progress: {progress.AchievementsEarned}/{progress.TotalAchievements} ({progress.PointsEarned}/{progress.TotalPoints} Points)";
                if (!string.IsNullOrEmpty(progress.GameIconUrl))
                {
                    GameCoverImage.Source = new BitmapImage(new Uri(progress.GameIconUrl));
                }

                AchievementsDataGrid.ItemsSource = achievements;
                NoAchievementsOverlay.Visibility = Visibility.Collapsed;
            }
            else
            {
                NoAchievementsOverlay.Visibility = Visibility.Visible;
            }
        }
        catch (Exception ex)
        {
            NoAchievementsOverlay.Visibility = Visibility.Visible;
            await LogErrors.LogErrorAsync(ex, $"Failed to load achievements for game ID: {_gameId}");
        }
    }

    private async Task LoadGameInfoAsync()
    {
        try
        {
            var gameInfo = await RetroAchievementsService.GetGameExtendedInfoAsync(_gameId, _settings.RaUsername, _settings.RaApiKey);
            if (gameInfo != null)
            {
                GameInfoTitle.Text = gameInfo.Title;
                GameInfoConsole.Text = gameInfo.ConsoleName;
                GameInfoGenre.Text = gameInfo.Genre;
                GameInfoDeveloper.Text = gameInfo.Developer;
                GameInfoPublisher.Text = gameInfo.Publisher;
                GameInfoReleased.Text = gameInfo.Released;
                GameInfoRichPresence.Text = gameInfo.RichPresencePatch;
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
    }

    private async Task LoadRankingsAsync()
    {
        try
        {
            var rankInfo = await RetroAchievementsService.GetGameRankAndScoreAsync(_gameId, _settings.RaUsername, _settings.RaApiKey);
            if (rankInfo?.Top10 != null && rankInfo.Top10.Count > 0)
            {
                // Assign ranks
                for (var i = 0; i < rankInfo.Top10.Count; i++)
                {
                    rankInfo.Top10[i].Rank = i + 1;
                }

                RankingsDataGrid.ItemsSource = rankInfo.Top10;
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
    }

    private async Task LoadUserProfileAsync()
    {
        try
        {
            var userProfile = await RetroAchievementsService.GetUserProfileAsync(_settings.RaUsername, _settings.RaApiKey);
            if (userProfile != null)
            {
                UserProfilePic.Source = new BitmapImage(new Uri($"https://retroachievements.org{userProfile.UserPic}"));
                UserProfileUser.Text = userProfile.User;
                UserProfileMotto.Text = userProfile.Motto;
                UserProfileRank.Text = $"#{userProfile.Rank}";
                UserProfilePoints.Text = $"{userProfile.TotalPoints:N0}";
                UserProfileTruePoints.Text = $"{userProfile.TotalTruePoints:N0}";
                UserProfileMemberSince.Text = userProfile.MemberSince;
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
    }
}
