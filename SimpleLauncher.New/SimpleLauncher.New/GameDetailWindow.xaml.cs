using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Services.MameData;
using SimpleLauncher.New.ViewModels;

namespace SimpleLauncher.New;

public partial class GameDetailWindow
{
    private readonly GameCardViewModel _game;
    private readonly MainViewModel _mainViewModel;
    private readonly MameDataService? _mameData;
    private readonly Services.LocalizationService _localization;

    public GameDetailWindow(GameCardViewModel game, MainViewModel mainViewModel)
    {
        InitializeComponent();

        _game = game;
        _mainViewModel = mainViewModel;
        DataContext = game;

        try
        {
            _mameData = App.ServiceProvider.GetRequiredService<MameDataService>();
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "MameDataService unavailable in GameDetailWindow");
            _mameData = null;
        }

        try
        {
            _localization = App.ServiceProvider.GetRequiredService<Services.LocalizationService>();
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "LocalizationService unavailable in GameDetailWindow");
            _localization = null!;
        }

        UpdateFavoriteButton();
        LoadDescription();
    }

    private void LoadDescription()
    {
        var descText = DescriptionBlock;

        if (_mameData is not null &&
            (_game.SystemName.Contains("Arcade", StringComparison.OrdinalIgnoreCase) ||
             _game.SystemName.Contains("MAME", StringComparison.OrdinalIgnoreCase)))
        {
            var machineName = Path.GetFileNameWithoutExtension(_game.FilePath);
            if (_mameData.Lookup.TryGetValue(machineName, out var mameDesc))
            {
                descText.Text = mameDesc;
                return;
            }
        }

        try
        {
            if (File.Exists(_game.FilePath))
            {
                var fi = new FileInfo(_game.FilePath);
                descText.Text = _localization is not null
                    ? _localization.GetString("GameDetail.FileInfo",
                        fi.Name, fi.Length.ToString("N0"), fi.LastWriteTime.ToString("yyyy-MM-dd HH:mm"))
                    : $"File: {fi.Name}\nSize: {fi.Length:N0} bytes\nModified: {fi.LastWriteTime:yyyy-MM-dd HH:mm}";
            }
            else
            {
                descText.Text = _localization is not null
                    ? _localization.GetString("GameDetail.FileNotFound")
                    : "File not found on disk.";
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to read file details for {Path}", _game.FilePath);
            descText.Text = _localization is not null
                ? _localization.GetString("GameDetail.NoInfo")
                : "No additional information available.";
        }
    }

    private void PlayButton_Click(object sender, RoutedEventArgs e)
    {
        _mainViewModel.PlayGameCommand.Execute(_game);
        Close();
    }

    private async void FavoriteButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _mainViewModel.ToggleFavoriteCommand.ExecuteAsync(_game);
            UpdateFavoriteButton();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to favorite game");
        }
    }

    private void RemoveButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            this,
            $"Remove \"{_game.DisplayTitle}\" from your library?\nThis will not delete the file.",
            "Remove Game",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // TODO: Remove from game list
            Close();
        }
    }

    private void UpdateFavoriteButton()
    {
        FavoriteButton.Content = _game.IsFavorite ? "♥ Favorited" : "♡ Favorite";
        FavoriteButton.Style = _game.IsFavorite
            ? (Style)FindResource("PrimaryButtonStyle")
            : (Style)FindResource("SecondaryButtonStyle");
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Cleanup
    }
}
