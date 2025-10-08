using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Automation; // Added for AutomationProperties
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
        _activeSearchQueryOrMode = null; // Reset active search mode
        SearchTextBox.Text = "";

        GameFileGrid.Visibility = Visibility.Visible;
        ListViewPreviewArea.Visibility = Visibility.Collapsed;

        if (_systemManagers == null || _systemManagers.Count == 0)
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
        foreach (var config in _systemManagers.OrderBy(static s => s.SystemName))
        {
            // Pass the injected _settings instance to GetSystemDisplayImagePathAsync
            var imagePath = await GetSystemDisplayImagePathAsync(config, _settings); // UPDATED CALL
            var (loadedImage, _) = await ImageLoader.LoadImageAsync(imagePath);

            var buttonContentPanel = new StackPanel { Orientation = Orientation.Vertical };

            var systemImageSize = _settings.ThumbnailSize;
            if (systemImageSize > 101)
            {
                systemImageSize = 100;
            }

            var image = new Image
            {
                Source = loadedImage,
                Height = systemImageSize * 1.3,
                Width = systemImageSize * 1.3 * 1.6,
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
                TextTrimming = TextTrimming.CharacterEllipsis,
                FontSize = 12,
                ToolTip = config.SystemName,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 0)
            };
            buttonContentPanel.Children.Add(textBlock);

            var systemButton = new Button
            {
                Content = buttonContentPanel,
                Tag = config.SystemName,
                Width = systemImageSize * 1.3 * 1.6 + 20,
                Height = systemImageSize * 1.3 + 40 + 20, // +40 for text, +20 for padding
                Margin = new Thickness(5),
                Padding = new Thickness(5)
            };

            // Set AutomationProperties.Name for screen readers
            AutomationProperties.SetName(systemButton, config.SystemName);
            AutomationProperties.SetHelpText(systemButton, (string)Application.Current.TryFindResource("SelectSystemButtonHelpText") ?? $"Select {config.SystemName} system");

            // Apply the 3D style from MainWindow's resources
            systemButton.SetResourceReference(StyleProperty, "SystemButtonStyle");

            systemButton.Click += SystemButton_Click;
            GameFileGrid.Children.Add(systemButton);
        }
    }

    private async void SystemButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is Button button && button.Tag is string systemName)
            {
                if (_isUiUpdating)
                {
                    return;
                }

                SetUiLoadingState(true);
                await Task.Yield(); // Allow UI to update and show spinner
                SystemComboBox.SelectedItem = systemName;
            }

            PlaySoundEffects.PlayNotificationSound();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Error in SystemButton_Click.");
        }
    }

    // Update method signature to accept SettingsManager
    private static Task<string> GetSystemDisplayImagePathAsync(SystemManager config, SettingsManager settings) // ADDED PARAMETER
    {
        var appBaseDir = AppDomain.CurrentDomain.BaseDirectory;
        var systemImageFolder = Path.Combine(appBaseDir, "images", "systems");
        var systemName = config.SystemName;
        var imageExtensions = GetImageExtensions.GetExtensions(); // Get supported extensions

        // 1. Check for system-specific image files (exact match)
        foreach (var ext in imageExtensions)
        {
            var systemImagePath = Path.Combine(systemImageFolder, $"{systemName}{ext}");
            if (File.Exists(systemImagePath))
            {
                return Task.FromResult(systemImagePath);
            }
        }

        // Get settings for fuzzy matching (now from the passed parameter)
        // var settings = App.Settings; // REMOVED: settings is now a parameter
        var enableFuzzyMatching = settings.EnableFuzzyMatching;
        var similarityThreshold = settings.FuzzyMatchingThreshold;

        // 2. If fuzzy matching is enabled and the directory exists, check for similar filenames
        if (enableFuzzyMatching && Directory.Exists(systemImageFolder))
        {
            var filesInImageFolder = Directory.GetFiles(systemImageFolder)
                .Where(f => imageExtensions.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase)))
                .ToList();

            string bestMatchPath = null;
            double highestSimilarity = 0;
            var lowerSystemName = systemName.ToLowerInvariant();

            foreach (var filePath in filesInImageFolder)
            {
                var fileWithoutExt = Path.GetFileNameWithoutExtension(filePath);
                if (string.IsNullOrEmpty(fileWithoutExt)) continue; // Skip files without names

                var lowerFileName = fileWithoutExt.ToLowerInvariant();

                // Calculate similarity using Jaro-Winkler
                var similarity = FindCoverImage.CalculateJaroWinklerSimilarity(lowerSystemName, lowerFileName);

                if (!(similarity > highestSimilarity)) continue;

                highestSimilarity = similarity;
                bestMatchPath = filePath;
            }

            // If the highest similarity meets the threshold, return that path
            if (bestMatchPath != null && highestSimilarity >= similarityThreshold)
            {
                return Task.FromResult(bestMatchPath);
            }
        }

        // 3. Fallback to the global default image if no match is found
        var defaultImagePath = Path.Combine(systemImageFolder, "default.png");
        return Task.FromResult(File.Exists(defaultImagePath) ? defaultImagePath : Path.Combine(appBaseDir, "images", "default.png"));
    }

    private async void NavToggleButtonAspectRatio_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (_isLoadingGames) return;

            PlaySoundEffects.PlayNotificationSound();

            // Define the array of aspect ratios in the desired order
            string[] aspectRatios = { "Square", "Wider", "SuperWider", "Taller", "SuperTaller" };

            // Get the current index of the aspect ratio
            var currentIndex = Array.IndexOf(aspectRatios, _settings.ButtonAspectRatio);

            // Calculate the next index, wrapping around to 0 if at the end
            var nextIndex = (currentIndex + 1) % aspectRatios.Length;

            // Get the new aspect ratio
            var newAspectRatio = aspectRatios[nextIndex];

            // Update the settings
            _settings.ButtonAspectRatio = newAspectRatio;
            _settings.Save();

            UpdateButtonAspectRatioCheckMarks(newAspectRatio);

            var (sl, sq) = GetLoadGameFilesParams();
            await LoadGameFilesAsync(sl, sq);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string errorMessage = "Error in the method NavToggleButtonAspectRatio_Click.";
            _ = LogErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.ErrorMessageBox();
        }
    }
}