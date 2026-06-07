using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Automation;
using System.Windows.Controls.Primitives;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.UIReset;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

namespace SimpleLauncher;

public partial class MainWindow
{
    private async Task DisplaySystemSelectionScreenAsync(CancellationToken cancellationToken = default)
    {
        TopSystemSelection.Visibility = Visibility.Collapsed;
        StatusBarArea.Visibility = Visibility.Collapsed;

        // Clear image sources first to prevent memory leaks from BitmapImage references
        ClearGameButtonImages(GameFileGrid);
        GameFileGrid.Children.Clear();
        GameListItems.Clear();

        PreviewImage.Source = null;
        // TopSystemImage.Visibility = Visibility.Collapsed;
        // TopSystemLogoImage.Source = null;

        TotalFilesLabel.Content = null;
        PrevPageButton2.IsEnabled = false;
        NextPageButton2.IsEnabled = false;
        ((IUiResetHost)this).CurrentFilter = null;
        ((IUiResetHost)this).ActiveSearchQueryOrMode = null;
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

            // Resolve system display image via service
            var imagePath = await _systemImageResolverService.ResolveDisplayImageAsync(config);
            var (loadedImage, _) = await _imageLoader.LoadImageAsync(imagePath);

            var buttonContentPanel = new StackPanel { Orientation = Orientation.Vertical };

            var systemImageSize = _settings.ThumbnailSizeForSystem;

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

            var contextMenu = new ContextMenu();

            var selectIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/play.png")),
                Width = 16,
                Height = 16
            };
            var selectMenuItem = new MenuItem
            {
                Header = (string)Application.Current.TryFindResource("SelectSystem") ?? "Select System",
                Icon = selectIcon
            };
            selectMenuItem.Click += (_, _) => systemButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, systemButton));

            var editIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/settings.png")),
                Width = 16,
                Height = 16
            };
            var editMenuItem = new MenuItem
            {
                Header = (string)Application.Current.TryFindResource("EditSystem") ?? "Edit System",
                Icon = editIcon
            };
            editMenuItem.Click += (_, _) => EditSystemFromContextMenu(config.SystemName);

            var deleteIcon = new Image
            {
                Source = new BitmapImage(new Uri("pack://application:,,,/images/delete.png")),
                Width = 16,
                Height = 16
            };
            var deleteMenuItem = new MenuItem
            {
                Header = (string)Application.Current.TryFindResource("DeleteSystem") ?? "Delete System",
                Icon = deleteIcon
            };
            deleteMenuItem.Click += (_, _) => DeleteSystemFromContextMenu(config.SystemName);

            contextMenu.Items.Add(selectMenuItem);
            contextMenu.Items.Add(editMenuItem);
            contextMenu.Items.Add(deleteMenuItem);
            systemButton.ContextMenu = contextMenu;

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
                    if (((IUiResetHost)this).IsUiUpdating)
                    {
                        return;
                    }

                    SetLoadingState(true);
                    await Task.Delay(100, token);

                    TopSystemSelection.Visibility = Visibility.Visible;
                    StatusBarArea.Visibility = Visibility.Visible;
                    SystemComboBox.SelectedItem = systemName;
                }

                _playSoundEffects.PlayNotificationSound();
                UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LoadingSystem") ?? "Loading system...", this);
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation if needed, e.g., reset UI state
                SetLoadingState(false);
                UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("SystemLoadCancelled") ?? "System load cancelled.", this);
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error in SystemButtonClickAsync.");
                MessageBoxLibrary.InvalidSystemConfigMessageBox();
                SortOrderToggleButton.Visibility = Visibility.Collapsed;

                SystemComboBox.SelectedItem = null; // This will trigger another selection changed event with null
                await DisplaySystemSelectionScreenAsync(token);

                // Clear the cached list on error
                await _gameCacheService.InvalidateAsync(token);
            }
            finally
            {
                SetLoadingState(false);
            }
        }
        catch (OperationCanceledException)
        {
            // Do nothing - cancellation is expected when the UI is reset
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in SystemButtonClickAsync.");
        }
    }

    private async void DeleteSystemFromContextMenu(string systemName)
    {
        try
        {
            var result = MessageBoxLibrary.AreYouSureDoYouWantToDeleteThisSystemMessageBox();
            if (result != MessageBoxResult.Yes) return;

            _playSoundEffects.PlayNotificationSound();

            SystemManager.DeleteSystemAsync(systemName);

            await Task.Delay(100, _cancellationSource.Token);

            LoadOrReloadSystemManager();
            ResetUiAsync();

            MessageBoxLibrary.SystemHasBeenDeletedMessageBox(systemName);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in DeleteSystemFromContextMenu.");
        }
    }

    private void EditSystemFromContextMenu(string systemName)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("OpeningExpertMode") ?? "Opening Expert Mode...", this);

            EditSystemWindow editSystemWindow = new(_settings, _playSoundEffects, _configuration, _logErrors, _helpUserService, _imageLoader, systemName)
            {
                Owner = this
            };
            editSystemWindow.ShowDialog();

            LoadOrReloadSystemManager();
            ResetUiAsync();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in EditSystemFromContextMenu.");
        }
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
            await _settings.SaveAsync();

            UpdateButtonAspectRatioCheckMarks(newAspectRatio);
            // Notify user
            UpdateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("TogglingButtonAspectRatio") ?? "Toggling button aspect ratio...", this);

            var (sl, sq) = GetLoadGameFilesParams();
            SetLoadingState(true, (string)Application.Current.TryFindResource("ReloadingGames") ?? "Reloading games...");
            await Task.Yield(); // Allow UI to render the loading overlay
            await LoadGameFilesAsync(sl, sq, _cancellationSource.Token);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string errorMessage = "Error in the method NavToggleButtonAspectRatioClickAsync.";
            _logErrors.LogAndForget(ex, errorMessage);

            // Notify user
            MessageBoxLibrary.ErrorMessageBox();
        }
    }
}