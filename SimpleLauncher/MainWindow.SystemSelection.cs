using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Automation;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.FindAndLoadImages;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services.UpdateStatusBar;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher;

public partial class MainWindow
{
    private async Task DisplaySystemSelectionScreenAsync(CancellationToken cancellationToken = default)
    {
        TopSystemSelection.Visibility = Visibility.Collapsed;

        GameFileGrid.Children.Clear();
        GameListItems.Clear();

        PreviewImage.Source = null;

        TotalFilesLabel.Content = null;
        _prevPageButton.IsEnabled = false;
        _nextPageButton.IsEnabled = false;
        _currentFilter = null;
        _activeSearchQueryOrMode = null;
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
            await PopulateSystemSelectionGridAsync(cancellationToken);
        }

        _topLetterNumberMenu.DeselectLetter();
    }

    private async Task PopulateSystemSelectionGridAsync(CancellationToken cancellationToken)
    {
        foreach (var config in _systemManagers.OrderBy(static s => s.SystemName))
        {
            cancellationToken.ThrowIfCancellationRequested();

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

            systemButton.Click += SystemButtonClickAsync;
            GameFileGrid.Children.Add(systemButton);
        }
    }

    private async void SystemButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            CancelAndRecreateToken();
            var token = _cancellationSource.Token;

            try
            {
                if (sender is Button { Tag: string systemName })
                {
                    if (_isUiUpdating)
                    {
                        return;
                    }

                    SetLoadingState(true);
                    await Task.Delay(100, token);

                    TopSystemSelection.Visibility = Visibility.Visible;
                    SystemComboBox.SelectedItem = systemName;
                }

                _playSoundEffects.PlayNotificationSound();
                UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingSystem") ?? "Loading system...", this);
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation if needed, e.g., reset UI state
                SetLoadingState(false);
                UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("SystemLoadCancelled") ?? "System load cancelled.", this);
            }
            catch (Exception ex)
            {
                _ = _logErrors.LogErrorAsync(ex, "Error in SystemButtonClickAsync.");
                MessageBoxLibrary.InvalidSystemConfigMessageBox();
                SortOrderToggleButton.Visibility = Visibility.Collapsed;

                SystemComboBox.SelectedItem = null; // This will trigger another selection changed event with null
                await DisplaySystemSelectionScreenAsync(token);

                // Clear the cached list on error
                await _allGamesLock.WaitAsync(token);
                try
                {
                    _allGamesForCurrentSystem.Clear();
                }
                finally
                {
                    _allGamesLock.Release();
                }
            }
            finally
            {
                SetLoadingState(false);
            }
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, "Error in SystemButtonClickAsync.");
        }
    }

    // Update method signature to accept SettingsManager
    private Task<string> GetSystemDisplayImagePathAsync(SystemManager config, SettingsManager settings) // ADDED PARAMETER
    {
        var appBaseDir = AppDomain.CurrentDomain.BaseDirectory;
        var systemImageFolder = Path.Combine(appBaseDir, "images", "systems");
        var systemName = config.SystemName;
        var imageExtensions = _configuration.GetSection("ImageExtensions").Get<string[]>() ?? []; // Get supported extensions

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

    private async void NavToggleButtonAspectRatioClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            CancelAndRecreateToken();

            _playSoundEffects.PlayNotificationSound();

            // Define the array of aspect ratios in the desired order
            string[] aspectRatios = ["Square", "Wider", "SuperWider", "SuperWider2", "Taller", "SuperTaller", "SuperTaller2"];

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
            // Notify user
            UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("TogglingButtonAspectRatio") ?? "Toggling button aspect ratio...", this);

            var (sl, sq) = GetLoadGameFilesParams();
            await LoadGameFilesAsync(sl, sq, _cancellationSource.Token);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string errorMessage = "Error in the method NavToggleButtonAspectRatioClickAsync.";
            _ = _logErrors.LogErrorAsync(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.ErrorMessageBox();
        }
    }
}