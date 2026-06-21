using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.Favorites;
using SimpleLauncher.Services.GameLauncher;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.GlobalSearch.Models;
using SimpleLauncher.Services.LoadImages;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services.SystemManager;
using SimpleLauncher.Services.WpfServices;
using SimpleLauncher.ViewModels;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;

#nullable enable

namespace SimpleLauncher.Pages;

internal partial class GlobalSearchPage : IDisposable, ILoadingState
{
    private readonly GlobalSearchViewModel _viewModel;
    private readonly MainWindow _mainWindow;
    private readonly GamePadController _gamePadController;
    private readonly GameLauncher _gameLauncher;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IFindCoverImageService _findCoverImage;
    private readonly List<MameManager> _machines;
    private readonly FavoritesManager _favoritesManager;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly IConfiguration _configuration;
    private readonly SettingsManager _settings;
    private readonly IContextMenuFunctions _contextMenuFunctions;
    private readonly IDebugLogger _debugLogger;
    private readonly IContextMenuService _contextMenuService;

    public GlobalSearchPage(
        List<SystemManager> systemManagers,
        List<MameManager> machines,
        Dictionary<string, string> mameLookup,
        FavoritesManager favoritesManager,
        SettingsManager settings,
        MainWindow mainWindow,
        GamePadController gamePadController,
        GameLauncher gameLauncher,
        PlaySoundEffects playSoundEffects,
        ILogErrors logErrors,
        IConfiguration configuration,
        IGetListOfFilesService getListOfFiles,
        IFindCoverImageService findCoverImage,
        IImageLoader imageLoader,
        IContextMenuFunctions contextMenuFunctions,
        IDebugLogger debugLogger,
        IContextMenuService contextMenuService)
    {
        InitializeComponent();

        _mainWindow = mainWindow ?? throw new ArgumentNullException(nameof(mainWindow));
        _gamePadController = gamePadController ?? throw new ArgumentNullException(nameof(gamePadController));
        _gameLauncher = gameLauncher ?? throw new ArgumentNullException(nameof(gameLauncher));
        _playSoundEffects = playSoundEffects ?? throw new ArgumentNullException(nameof(playSoundEffects));
        _logErrors = logErrors ?? throw new ArgumentNullException(nameof(logErrors));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _findCoverImage = findCoverImage ?? throw new ArgumentNullException(nameof(findCoverImage));
        _machines = machines ?? throw new ArgumentNullException(nameof(machines));
        _favoritesManager = favoritesManager ?? throw new ArgumentNullException(nameof(favoritesManager));
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _contextMenuFunctions = contextMenuFunctions ?? throw new ArgumentNullException(nameof(contextMenuFunctions));
        _debugLogger = debugLogger ?? throw new ArgumentNullException(nameof(debugLogger));
        _contextMenuService = contextMenuService ?? throw new ArgumentNullException(nameof(contextMenuService));
        _messageBox = App.ServiceProvider.GetRequiredService<IMessageBoxLibraryService>();

        _viewModel = new GlobalSearchViewModel(
            configuration,
            logErrors,
            settings,
            systemManagers,
            machines,
            mameLookup,
            favoritesManager,
            playSoundEffects,
            getListOfFiles,
            findCoverImage,
            imageLoader,
            _messageBox,
            App.ServiceProvider.GetRequiredService<IResourceProvider>());

        DataContext = _viewModel;

        // Populate System ComboBox
        SystemComboBox.ItemsSource = _viewModel.SystemNames;
        SystemComboBox.SelectedIndex = 0;

        Loaded += (_, _) =>
        {
            LoadingOverlay.ApplyTemplate();
            if (LoadingOverlay.Template.FindName("PART_EmergencyReturnButton", LoadingOverlay) is Button emergencyBtn)
            {
                emergencyBtn.Click += EmergencyOverlayRelease_Click;
            }
        };

        Unloaded += GlobalSearchPage_Unloaded;
    }

    private void GlobalSearchPage_Unloaded(object sender, RoutedEventArgs e)
    {
        try
        {
            _viewModel.CancelSearch();
            ResultsDataGrid.ItemsSource = null;
            _viewModel.Dispose();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error cleaning up resources on page unload.");
        }
    }

    private async void SearchButtonClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            _playSoundEffects.PlayNotificationSound();

            var searchTerm = SearchTextBox.Text;
            var selectedSystem = SystemComboBox.SelectedItem as string;
            var searchFilename = SearchFilenameCheckBox.IsChecked == true;
            var searchMameDescription = SearchMameDescriptionCheckBox.IsChecked == true;
            var searchFolderName = SearchFolderNameCheckBox.IsChecked == true;
            var searchRecursively = SearchRecursivelyCheckBox.IsChecked == true;

            SetLoadingState(true, "Searching... Please wait.");
            await Task.Yield();

            try
            {
                await _viewModel.SearchAsync(searchTerm, selectedSystem, searchFilename, searchMameDescription,
                    searchFolderName, searchRecursively);

                // Update UI after search
                ResultsDataGrid.ItemsSource = _viewModel.SearchResults;
                NoResultsMessageOverlay.Visibility = _viewModel.NoResultsVisible ? Visibility.Visible : Visibility.Collapsed;
            }
            finally
            {
                SetLoadingState(false);
            }
        }
        catch (Exception ex)
        {
            SetLoadingState(false);
            await _logErrors.LogErrorAsync(ex, "Error in SearchButtonClickAsync.");
        }
    }

    private void SearchWhenPressEnterKey(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter)
        {
            SearchButtonClickAsync(sender, e);
        }
    }

    private async void LaunchButton_ClickAsync(object sender, RoutedEventArgs e)
    {
        try
        {
            if (ResultsDataGrid.SelectedItem is SearchResult selectedResult && !string.IsNullOrEmpty(selectedResult.FilePath))
            {
                _playSoundEffects.PlayNotificationSound();
                await LaunchGameFromSearchResultAsync(selectedResult.FilePath, selectedResult.SystemName, selectedResult.EmulatorManager);
            }
            else
            {
                await _messageBox.SelectAGameToLaunchMessageBoxAsync();
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in LaunchButton_ClickAsync (GlobalSearch).");
            await _messageBox.ErrorLaunchingGameMessageBoxAsync(
                PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
        }
    }

    private async Task LaunchGameFromSearchResultAsync(string filePath, string selectedSystemName, Emulator? selectedEmulatorManager)
    {
        try
        {
            if (string.IsNullOrEmpty(filePath) || string.IsNullOrEmpty(selectedSystemName) || selectedEmulatorManager == null)
            {
                _logErrors.LogAndForget(null, "[LaunchGameFromSearchResultAsync] filePath or selectedSystemName or selectedEmulatorManager is null.");
                await _messageBox.ErrorLaunchingGameMessageBoxAsync(
                    PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
                return;
            }

            var selectedSystemManager = _viewModel.GetSystemManager(selectedSystemName);
            if (selectedSystemManager == null)
            {
                _logErrors.LogAndForget(null, "[LaunchGameFromSearchResultAsync] System manager not found.");
                await _messageBox.ErrorLaunchingGameMessageBoxAsync(
                    PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
                return;
            }

            await _gameLauncher.HandleButtonClickAsync(filePath, selectedEmulatorManager.EmulatorName, selectedSystemName,
                selectedSystemManager, _settings, WpfWindowContext.FromMainWindow(_mainWindow), _gamePadController, this);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"[LaunchGameFromSearchResultAsync] Error launching: {filePath}, System: {selectedSystemName}");
            await _messageBox.ErrorLaunchingGameMessageBoxAsync(
                PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
        }
    }

    private async void GlobalSearchPrepareForRightClickContextMenuAsync(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (ResultsDataGrid.SelectedItem is not SearchResult selectedResult || string.IsNullOrEmpty(selectedResult.FilePath))
                return;

            var systemManager = _viewModel.GetSystemManager(selectedResult.SystemName);
            if (systemManager == null)
            {
                _logErrors.LogAndForget(null, "SystemManager is null");
                await _messageBox.ErrorLaunchingGameMessageBoxAsync(
                    PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
                return;
            }

            if (string.IsNullOrEmpty(selectedResult.FilePath) || string.IsNullOrEmpty(selectedResult.SystemName) || selectedResult.EmulatorManager == null)
            {
                _logErrors.LogAndForget(null, "FilePath, SystemName, or EmulatorManager is null.");
                await _messageBox.ErrorLaunchingGameMessageBoxAsync(
                    PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
                return;
            }

            var context = new RightClickContext(
                selectedResult.FilePath,
                selectedResult.FileNameWithExtension,
                selectedResult.FileName,
                selectedResult.SystemName,
                systemManager,
                _machines,
                _favoritesManager,
                _settings,
                null,
                null,
                selectedResult.EmulatorManager,
                null,
                null,
                _mainWindow,
                _gamePadController,
                null,
                _gameLauncher,
                _playSoundEffects,
                this
            );

            var contextMenu = _contextMenuService.AddRightClickReturnContextMenu(context, _findCoverImage, _contextMenuFunctions);
            if (contextMenu != null)
            {
                ResultsDataGrid.ContextMenu = contextMenu;
                contextMenu.IsOpen = true;
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in GlobalSearch right-click context menu.");
            await _messageBox.RightClickContextMenuErrorMessageBoxAsync();
        }
    }

    private async void ResultsDataGrid_MouseDoubleClickAsync(object sender, MouseButtonEventArgs e)
    {
        try
        {
            if (ResultsDataGrid.SelectedItem is not SearchResult selectedResult || string.IsNullOrEmpty(selectedResult.FilePath)) return;

            _playSoundEffects.PlayNotificationSound();
            await LaunchGameFromSearchResultAsync(selectedResult.FilePath, selectedResult.SystemName, selectedResult.EmulatorManager);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in ResultsDataGrid_MouseDoubleClickAsync (GlobalSearch).");
            await _messageBox.CouldNotLaunchThisGameMessageBoxAsync(
                PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue("LogPath", "error_user.log")));
        }
    }

    private async void ActionsWhenUserSelectAResultItemAsync(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (ResultsDataGrid.SelectedItem is SearchResult selectedResult && !string.IsNullOrEmpty(selectedResult.FilePath))
            {
                LaunchButton.IsEnabled = true;
                await _viewModel.UpdatePreviewImageAsync(selectedResult.CoverImage);

                if (ResultsDataGrid.SelectedItem == selectedResult)
                {
                    PreviewImage.Source = _viewModel.PreviewImageSource?.ToBitmapImage();
                }
            }
            else
            {
                LaunchButton.IsEnabled = false;
                PreviewImage.Source = null;
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error loading image in ActionsWhenUserSelectAResultItemAsync (GlobalSearch).");
            PreviewImage.Source = null;
        }
    }

    public void SetLoadingState(bool isLoading, string? message = null)
    {
        Dispatcher.Invoke(() =>
        {
            LoadingOverlay.Visibility = isLoading ? Visibility.Visible : Visibility.Collapsed;
            if (isLoading)
            {
                LoadingOverlay.Content = message;
            }
        });
    }

    private void EmergencyOverlayRelease_Click(object sender, RoutedEventArgs e)
    {
        _playSoundEffects.PlayNotificationSound();
        _viewModel.CancelSearch();
        LoadingOverlay.Visibility = Visibility.Collapsed;

        _debugLogger.Log("[Emergency] User forced overlay dismissal in GlobalSearchPage.");
        _mainWindow.UpdateStatusBarService.UpdateContent("Emergency reset performed.");
    }

    public void Dispose()
    {
        _viewModel.Dispose();
    }
}