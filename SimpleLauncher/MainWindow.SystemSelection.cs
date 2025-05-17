using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using SimpleLauncher.Managers;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class MainWindow
{
    private async Task DisplaySystemSelectionScreenAsync()
    {
        GameFileGrid.Children.Clear();
        GameListItems.Clear();

        PreviewImage.Source = null;

        TotalFilesLabel.Content = null;
        _prevPageButton.IsEnabled = false;
        _nextPageButton.IsEnabled = false;
        _currentFilter = null;
        SearchTextBox.Text = "";

        GameFileGrid.Visibility = Visibility.Visible;
        ListViewPreviewArea.Visibility = Visibility.Collapsed;

        if (_systemConfigs == null || _systemConfigs.Count == 0)
        {
            var noSystemsConfiguredMsg = (string)Application.Current.TryFindResource("NoSystemsConfiguredMessage") ?? "No systems configured. Please use the 'Edit System' menu to add systems.";
            GameFileGrid.Children.Add(new TextBlock
            {
                Text = $"\n{noSystemsConfiguredMsg}",
                Padding = new Thickness(10),
                TextWrapping = TextWrapping.Wrap,
                HorizontalAlignment = HorizontalAlignment.Center
            });
        }
        else
        {
            await PopulateSystemSelectionGridAsync();
        }

        _topLetterNumberMenu.DeselectLetter();
    }

    private async Task PopulateSystemSelectionGridAsync()
    {
        foreach (var config in _systemConfigs.OrderBy(static s => s.SystemName))
        {
            var imagePath = await GetSystemDisplayImagePathAsync(config);
            var (loadedImage, _) = await ImageLoader.LoadImageAsync(imagePath);

            var buttonContentPanel = new StackPanel { Orientation = Orientation.Vertical };

            var image = new Image
            {
                Source = loadedImage,
                Height = _settings.ThumbnailSize * 1.3,
                Width = _settings.ThumbnailSize * 1.3 * 1.6,
                Stretch = Stretch.Uniform,
                Margin = new Thickness(5)
            };
            buttonContentPanel.Children.Add(image);

            var textBlock = new TextBlock
            {
                Text = config.SystemName,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(0, 5, 0, 0)
            };
            buttonContentPanel.Children.Add(textBlock);

            var systemButton = new Button
            {
                Content = buttonContentPanel,
                Tag = config.SystemName,
                Width = _settings.ThumbnailSize * 1.3 * 1.6 + 20,
                Height = _settings.ThumbnailSize * 1.3 + 40 + 20, // +40 for text, +20 for padding
                Margin = new Thickness(5),
                Padding = new Thickness(5)
            };
            systemButton.Click += SystemButton_Click;
            GameFileGrid.Children.Add(systemButton);
        }
    }

    private void SystemButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Tag is string systemName)
        {
            SystemComboBox.SelectedItem = systemName;
        }
    }

    private static Task<string> GetSystemDisplayImagePathAsync(SystemManager config)
    {
        var appBaseDir = AppDomain.CurrentDomain.BaseDirectory;
        var systemImageFolder = Path.Combine(appBaseDir, "images", "systems");
        var systemName = config.SystemName;

        // Check for system-specific image files (png, jpg, jpeg)
        var possibleExtensions = new[] { ".png", ".jpg", ".jpeg" };
        foreach (var ext in possibleExtensions)
        {
            var systemImagePath = Path.Combine(systemImageFolder, systemName + ext);
            if (File.Exists(systemImagePath))
            {
                return Task.FromResult(systemImagePath);
            }
        }

        // Fallback to the global default image if no system-specific image is found
        return Task.FromResult(Path.Combine(systemImageFolder, "default.png"));
    }
}