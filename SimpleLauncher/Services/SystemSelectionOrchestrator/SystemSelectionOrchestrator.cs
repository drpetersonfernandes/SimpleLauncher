using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using CoreMessageBoxResult = SimpleLauncher.Core.Interfaces.MessageBoxResult;
using PathHelper = SimpleLauncher.Core.Services.CheckPaths.PathHelper;
using SimpleLauncher.Services.DisplaySystemInfo;
using SimpleLauncher.Services.GameCache;
using SimpleLauncher.Services.GameFileWatcher;
using SimpleLauncher.Services.GameItemRender;
using SimpleLauncher.Services.GetListOfFiles;
using SimpleLauncher.Services.LoadImages;
using SimpleLauncher.Services.MameData;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.QuitOrReinstall;
using SimpleLauncher.Services.SystemConfiguration;
using SimpleLauncher.Services.SystemImageResolver;
using SimpleLauncher.Services.UIReset;
using SimpleLauncher.Services.UpdateStatusBar;

namespace SimpleLauncher.Services.SystemSelectionOrchestrator;

public class SystemSelectionOrchestrator : ISystemSelectionOrchestrator
{
    private ISystemSelectionHost _host;
    private readonly SettingsManager.SettingsManager _settings;
    private readonly ISystemImageResolverService _systemImageResolverService;
    private readonly IImageLoader _imageLoader;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly ILogErrors _logErrors;
    private readonly IGameCacheService _gameCacheService;
    private readonly GameFileWatcherService _gameFileWatcherService;
    private readonly IConfiguration _configuration;
    private readonly IHelpUserService _helpUserService;
    private readonly IGameItemRenderService _gameItemRenderService;
    private readonly IGetListOfFiles _getListOfFiles;
    private readonly IUpdateStatusBar _updateStatusBarService;
    private readonly ISystemConfigurationService _systemConfigurationService;
    private readonly IMameDataService _mameDataService;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly QuitSimpleLauncher _quitSimpleLauncher;

    public SystemSelectionOrchestrator(
        SettingsManager.SettingsManager settings,
        ISystemImageResolverService systemImageResolverService,
        IImageLoader imageLoader,
        PlaySoundEffects playSoundEffects,
        ILogErrors logErrors,
        IGameCacheService gameCacheService,
        GameFileWatcherService gameFileWatcherService,
        IConfiguration configuration,
        IHelpUserService helpUserService,
        IGameItemRenderService gameItemRenderService,
        IGetListOfFiles getListOfFiles,
        IUpdateStatusBar updateStatusBarService,
        ISystemConfigurationService systemConfigurationService,
        IMameDataService mameDataService,
        IMessageBoxLibraryService messageBox,
        QuitSimpleLauncher quitSimpleLauncher)
    {
        _settings = settings;
        _systemImageResolverService = systemImageResolverService;
        _imageLoader = imageLoader;
        _playSoundEffects = playSoundEffects;
        _logErrors = logErrors;
        _gameCacheService = gameCacheService;
        _gameFileWatcherService = gameFileWatcherService;
        _configuration = configuration;
        _helpUserService = helpUserService;
        _gameItemRenderService = gameItemRenderService;
        _getListOfFiles = getListOfFiles;
        _updateStatusBarService = updateStatusBarService;
        _systemConfigurationService = systemConfigurationService;
        _mameDataService = mameDataService;
        _messageBox = messageBox;
        _quitSimpleLauncher = quitSimpleLauncher;
    }

    public void Initialize(ISystemSelectionHost host)
    {
        _host = host;
    }

    public void LoadOrReloadSystemManager()
    {
        var managers = _systemConfigurationService.LoadSystemManagers();
        _host.SetSystemManagers(managers);
        var sortedSystemNames = managers.Select(static manager => manager.SystemName).OrderBy(static name => name)
            .ToList();
        _host.SystemComboBox.ItemsSource = sortedSystemNames;

        _gameItemRenderService.ReloadFactories(managers, _mameDataService.Machines.ToList());
    }

    public async Task DisplaySystemSelectionScreenAsync(CancellationToken cancellationToken = default)
    {
        _host.TopSystemSelection.Visibility = Visibility.Collapsed;
        _host.StatusBarArea.Visibility = Visibility.Collapsed;

        _host.ClearGameButtonImages(_host.GameFileGrid);
        _host.GameFileGrid.Children.Clear();
        _host.GameListItems.Clear();

        _host.PreviewImage.Source = null;

        _host.TotalFilesLabel.Content = null;
        _host.PrevPageButton2.IsEnabled = false;
        _host.NextPageButton2.IsEnabled = false;
        ((IUiResetHost)_host).CurrentFilter = null;
        ((IUiResetHost)_host).ActiveSearchQueryOrMode = null;
        _host.SearchTextBox.Text = "";

        _host.GameFileGrid.Visibility = Visibility.Visible;
        _host.ListViewPreviewArea.Visibility = Visibility.Collapsed;

        var systemManagers = _host.GetSystemManagers();
        if (systemManagers == null || systemManagers.Count == 0)
        {
            var noSystemsConfiguredMsg = (string)Application.Current.TryFindResource("NoSystemsConfiguredMessage") ?? "No systems configured. Please use the 'Edit System' menu to add systems.";
            _host.GameFileGrid.Children.Add(new TextBlock
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

        _host.ResetPaginationButtons();
    }

    private async Task PopulateSystemSelectionGridAsync(CancellationToken cancellationToken)
    {
        foreach (var config in _host.GetSystemManagers().OrderBy(static s => s.SystemName))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var imagePath = await _systemImageResolverService.ResolveDisplayImageAsync(config);
            var (imageStream, _) = await _imageLoader.LoadImageAsync(imagePath);

            var buttonContentPanel = new StackPanel { Orientation = Orientation.Vertical };

            var systemImageSize = _settings.ThumbnailSizeForSystem;

            var image = new Image
            {
                Source = imageStream.ToBitmapImage(),
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
                Height = systemImageSize * 1.3 + 40 + 20,
                Margin = new Thickness(5),
                Padding = new Thickness(5)
            };

            AutomationProperties.SetName(systemButton, config.SystemName);
            AutomationProperties.SetHelpText(systemButton, (string)Application.Current.TryFindResource("SelectSystemButtonHelpText") ?? $"Select {config.SystemName} system");

            systemButton.SetResourceReference(FrameworkElement.StyleProperty, "SystemButtonStyle");

            systemButton.Click += async (_, _) => await SystemButtonClickAsync(config.SystemName, _host.CurrentCancellationToken);

            var contextMenu = new System.Windows.Controls.ContextMenu();

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

            _host.GameFileGrid.Children.Add(systemButton);
        }
    }

    public async Task SystemButtonClickAsync(string systemName, CancellationToken cancellationToken)
    {
        try
        {
            _host.CancelAndRecreateToken();
            var token = _host.CurrentCancellationToken;

            try
            {
                if (((IUiResetHost)_host).IsUiUpdating)
                {
                    return;
                }

                _host.SetLoadingState(true);
                await Task.Delay(100, token);

                _host.TopSystemSelection.Visibility = Visibility.Visible;
                _host.StatusBarArea.Visibility = Visibility.Visible;
                _host.SystemComboBox.SelectedItem = systemName;

                _playSoundEffects.PlayNotificationSound();
                _updateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("LoadingSystem") ?? "Loading system...");
            }
            catch (OperationCanceledException)
            {
                _host.SetLoadingState(false);
                _updateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("SystemLoadCancelled") ?? "System load cancelled.");
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error in SystemButtonClickAsync.");
                await _messageBox.InvalidSystemConfigMessageBox();
                _host.SortOrderToggleButton.Visibility = Visibility.Collapsed;

                _host.SystemComboBox.SelectedItem = null;
                await DisplaySystemSelectionScreenAsync(token);

                await _gameCacheService.InvalidateAsync(token);
            }
            finally
            {
                _host.SetLoadingState(false);
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

    public async void DeleteSystemFromContextMenu(string systemName)
    {
        try
        {
            var result = await _messageBox.AreYouSureDoYouWantToDeleteThisSystemMessageBox();
            if (result != CoreMessageBoxResult.Yes) return;

            _playSoundEffects.PlayNotificationSound();

            SystemManager.SystemManager.DeleteSystemAsync(systemName);

            await Task.Delay(100, _host.CurrentCancellationToken);

            LoadOrReloadSystemManager();
            await _host.ResetUiAsync();

            await _messageBox.SystemHasBeenDeletedMessageBox(systemName);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in DeleteSystemFromContextMenu.");
        }
    }

    public void EditSystemFromContextMenu(string systemName)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();
            _updateStatusBarService.UpdateContent((string)Application.Current.TryFindResource("OpeningExpertMode") ?? "Opening Expert Mode...");

            EditSystemWindow editSystemWindow = new(_settings, _playSoundEffects, _configuration, _logErrors, _helpUserService, _imageLoader, _messageBox, _quitSimpleLauncher, systemName)
            {
                Owner = Application.Current.MainWindow
            };
            editSystemWindow.ShowDialog();

            LoadOrReloadSystemManager();
            _host.ResetUiAsync();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in EditSystemFromContextMenu.");
        }
    }

    public async Task SystemComboBoxSelectionChangedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            try
            {
                if (((IUiResetHost)_host).IsUiUpdating)
                {
                    return;
                }

                if (_host.SystemComboBox.SelectedItem == null)
                {
                    await _gameCacheService.InvalidateAsync(CancellationToken.None);
                    _host.IsPlayTimeVisible = false;

                    _gameFileWatcherService.StopWatching();

                    return;
                }

                ((IUiResetHost)_host).IsUiUpdating = true;
                _host.SetLoadingState(true, (string)Application.Current.TryFindResource("LoadingSystem") ?? "Loading system...");
                await Task.Delay(100, cancellationToken);
                try
                {
                    _host.SearchTextBox.Text = "";
                    _host.EmulatorComboBox.ItemsSource = null;
                    _host.EmulatorComboBox.SelectedIndex = -1;
                    _host.PreviewImage.Source = null;

                    await _gameCacheService.SetSearchResultsAsync([], cancellationToken);
                    ((IUiResetHost)_host).CurrentFilter = null;
                    ((IUiResetHost)_host).ActiveSearchQueryOrMode = null;

                    _host.GameFileGrid.Visibility = Visibility.Visible;
                    _host.ListViewPreviewArea.Visibility = Visibility.Collapsed;

                    var selectedSystem = _host.SystemComboBox.SelectedItem?.ToString();
                    var systemManagers = _host.GetSystemManagers();
                    var selectedManager = systemManagers.FirstOrDefault(c => c.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase));
                    if (selectedSystem == null || selectedManager == null)
                    {
                        const string errorMessage = "Selected system or its configuration is null.";
                        _logErrors.LogAndForget(null, errorMessage);

                        await _messageBox.InvalidSystemConfigMessageBox();
                        _host.SortOrderToggleButton.Visibility = Visibility.Collapsed;

                        _host.SystemComboBox.SelectedItem = null;
                        await DisplaySystemSelectionScreenAsync(cancellationToken);

                        await _gameCacheService.InvalidateAsync(cancellationToken);

                        return;
                    }

                    var formats = selectedManager.FileFormatsToSearch;
                    _host.IsPlayTimeVisible = formats == null || !formats.Any(static f =>
                        f.Equals("url", StringComparison.OrdinalIgnoreCase) ||
                        f.Equals("lnk", StringComparison.OrdinalIgnoreCase));

                    ((IUiResetHost)_host).MameSortOrder = "FileName";
                    _host.UpdateSortOrderButtonUi();
                    _host.SortOrderToggleButton.Visibility = Visibility.Visible;

                    _host.EmulatorComboBox.ItemsSource = selectedManager.Emulators.Select(static emulator => emulator.EmulatorName).ToList();
                    if (_host.EmulatorComboBox.Items.Count > 0)
                    {
                        _host.EmulatorComboBox.SelectedIndex = 0;
                    }

                    _host.SelectedSystem = selectedSystem;

                    var systemPlayTime = _settings.SystemPlayTimes.FirstOrDefault(s => s.SystemName.Equals(selectedSystem, StringComparison.OrdinalIgnoreCase));
                    _host.PlayTime = systemPlayTime != null ? systemPlayTime.FormattedPlayTime : "00:00:00";

                    var validationResult = await DisplaySystemInformation.DisplaySystemInfoAsync(selectedManager, _host.GameFileGrid, cancellationToken);

                    if (!validationResult.IsValid)
                    {
                        var errorMessages = new StringBuilder();
                        foreach (var msg in validationResult.ErrorMessages)
                        {
                            errorMessages.Append(msg);
                        }

                        await _messageBox.ListOfErrorsMessageBox(errorMessages);
                    }

                    var resolvedSystemImageFolderPath = PathHelper.ResolveRelativeToAppDirectory(selectedManager.SystemImageFolder);

                    var selectedRomFolders = selectedManager.SystemFolders.Select(PathHelper.ResolveRelativeToAppDirectory).ToList();
                    var selectedImageFolder = string.IsNullOrWhiteSpace(resolvedSystemImageFolderPath)
                        ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", selectedManager.SystemName)
                        : resolvedSystemImageFolderPath;

                    _host.SetSelectedRomFolders(selectedRomFolders);
                    _host.SetSelectedImageFolder(selectedImageFolder);

                    await PopulateAllGamesForCurrentSystem(selectedManager, selectedSystem, selectedRomFolders, cancellationToken);

                    _gameFileWatcherService.StartWatching(
                        selectedManager.SystemFolders,
                        selectedSystem,
                        selectedManager.FileFormatsToSearch);

                    _host.ResetPaginationButtons();
                }
                catch (Exception ex)
                {
                    const string errorMessage = "Error in the method SystemComboBoxSelectionChangedAsync.";
                    _logErrors.LogAndForget(ex, errorMessage);

                    await _messageBox.InvalidSystemConfigMessageBox();

                    await _gameCacheService.InvalidateAsync(cancellationToken);
                }
                finally
                {
                    _host.SetLoadingState(false);
                    ((IUiResetHost)_host).IsUiUpdating = false;
                }
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error in SystemComboBoxSelectionChangedAsync.");
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in SystemComboBoxSelectionChangedAsync.");
        }
    }

    private async Task PopulateAllGamesForCurrentSystem(SystemManager.SystemManager selectedManager, string currentSelectedSystem, List<string> selectedRomFolders, CancellationToken cancellationToken)
    {
        var uniqueFilesForSystem = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var folder in selectedRomFolders)
        {
            var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(folder);
            if (string.IsNullOrEmpty(resolvedSystemFolderPath) || !Directory.Exists(resolvedSystemFolderPath) || selectedManager.FileFormatsToSearch == null) continue;

            var filesInFolder = await _getListOfFiles.GetFilesAsync(resolvedSystemFolderPath, selectedManager.FileFormatsToSearch, selectedManager, cancellationToken);
            foreach (var file in filesInFolder)
            {
                uniqueFilesForSystem.TryAdd(Path.GetFileName(file), file);
            }
        }

        await _gameCacheService.SetAllGamesAsync(uniqueFilesForSystem.Values.ToList(), currentSelectedSystem, cancellationToken);
    }
}
