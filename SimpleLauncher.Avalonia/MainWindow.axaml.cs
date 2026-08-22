using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Layout;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Avalonia.Converters;
using SimpleLauncher.Avalonia.InjectConfigWindows;
using SimpleLauncher.Avalonia.Models;
using SimpleLauncher.Avalonia.Services;
using SimpleLauncher.Avalonia.Services.GameScan;
using SimpleLauncher.Avalonia.Services.SystemSelectionOrchestrator;
using SimpleLauncher.Avalonia.Services.UIReset;
using SimpleLauncher.Avalonia.Services.ContextMenus;
using SimpleLauncher.Avalonia.Services.LoadingOverlay;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.ExternalToolLauncher;
using SimpleLauncher.Core.Services.GamePad;
using SimpleLauncher.Core.Services.PlaySound;
using SimpleLauncher.Core.Services.RetroAchievements;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia;

/// <summary>
/// Main application window — OpenEmu-inspired shell with sidebar, toolbar, and game content area.
/// Avalonia port of the WPF-UI MainWindow.
/// </summary>
public partial class MainWindow : Window, IPaginationHost
{
    private readonly MainViewModel _viewModel;
    private readonly Services.SystemManager.SystemManagerService _systemManagerService;
    private readonly SettingsManagerService _settings;
    private readonly LocalizationService _localization;
    private readonly ExternalToolLauncherService _externalToolLauncher;
    private readonly PlaySoundEffects _playSound;
    private readonly GameScannerService _gameScannerService;
    private readonly IPaginationService _pagination;
    private readonly AvaloniaGameFileWatcherService _fileWatcher;
    private readonly AvaloniaLanguageMenuService _languageMenu;
    private readonly AvaloniaMenuCheckMarkService _menuCheckMarks;
    private readonly GamePadController _gamePadController;
    private readonly UiResetService _uiResetService;
    private readonly AvaloniaSystemSelectionOrchestratorService _systemSelectionOrchestrator;
    private readonly AvaloniaContextMenuService _contextMenuService;
    private readonly AvaloniaLoadingOverlayService _loadingOverlay;

    /// <summary>Favorites page section ViewModel (WPF FavoritesPage equivalent).</summary>
    public FavoritesSectionViewModel FavoritesSection { get; }

    /// <summary>Play history page section ViewModel (WPF PlayHistoryPage equivalent).</summary>
    public PlayHistorySectionViewModel PlayHistorySection { get; }

    /// <summary>Global search page section ViewModel (WPF GlobalSearchPage equivalent).</summary>
    public GlobalSearchSectionViewModel GlobalSearchSection { get; }

    // Bounds persistence (separate file from the WPF app)
    private static readonly string BoundsFilePath = Path.Combine(
        Core.Services.AppDataPaths.SimpleLauncherDataFolder, "window_bounds_avalonia.json");

    // Event handler references for cleanup
    private readonly Action<string, string> _toastRequestedHandler;
    private readonly EventHandler<EventArgs<string>> _gameFilesChangedHandler;
    private readonly EventHandler<PointerWheelEventArgs> _pointerWheelChangedHandler;
    private bool _wasControllerRunningBeforeDeactivation;

    public MainWindow(
        MainViewModel viewModel,
        SystemArtRatioService ratioService,
        Services.SystemManager.SystemManagerService systemManagerService,
        SettingsManagerService settings,
        LocalizationService localization,
        ExternalToolLauncherService externalToolLauncher,
        PlaySoundEffects playSound,
        GameScannerService gameScannerService,
        FavoritesSectionViewModel favoritesSection,
        PlayHistorySectionViewModel playHistorySection,
        GlobalSearchSectionViewModel globalSearchSection,
        IPaginationService pagination,
        AvaloniaGameFileWatcherService fileWatcher,
        AvaloniaLanguageMenuService languageMenu,
        AvaloniaMenuCheckMarkService menuCheckMarks,
        GamePadController gamePadController,
        UiResetService uiResetService,
        AvaloniaSystemSelectionOrchestratorService systemSelectionOrchestrator,
        AvaloniaContextMenuService contextMenuService,
        AvaloniaLoadingOverlayService loadingOverlay)
    {
        _viewModel = viewModel;
        _systemManagerService = systemManagerService;
        _settings = settings;
        _localization = localization;
        _externalToolLauncher = externalToolLauncher;
        _playSound = playSound;
        _gameScannerService = gameScannerService;
        _pagination = pagination;
        _fileWatcher = fileWatcher;
        _languageMenu = languageMenu;
        _menuCheckMarks = menuCheckMarks;
        _gamePadController = gamePadController;
        _uiResetService = uiResetService;
        _systemSelectionOrchestrator = systemSelectionOrchestrator;
        _contextMenuService = contextMenuService;
        _loadingOverlay = loadingOverlay;
        FavoritesSection = favoritesSection;
        PlayHistorySection = playHistorySection;
        GlobalSearchSection = globalSearchSection;
        DataContext = _viewModel;

        // Surface ViewModel toast requests (e.g. RetroAchievements hash scan status)
        _toastRequestedHandler = (title, message) => ShowToast(title, message);
        _viewModel.ToastRequested += _toastRequestedHandler;

        // Initialize converter with ratio service
        ConsoleToCardHeightConverter.SetRatioService(ratioService);
        BooleanToFavoriteStatusConverter.SetLocalizationService(localization);

        InitializeComponent();

        // Wire the extracted services to this host (WPF parity)
        _uiResetService.Initialize(this);
        _systemSelectionOrchestrator.Initialize(this);

        // Localize the emergency return button (WPF DynamicResource ReturnButton parity)
        EmergencyReturnButton.Content = _localization.GetString("ReturnButton");
        ToolTip.SetTip(EmergencyReturnButton,
            _localization.GetString("ClickHereIfTheLoadingScreenIsStuckToReturnToTheMainMenu"));

        // Bind the page-section ViewModels (WPF FavoritesPage / PlayHistoryPage / GlobalSearchPage equivalents)
        FavoritesSectionRoot.DataContext = FavoritesSection;
        PlayHistorySectionRoot.DataContext = PlayHistorySection;
        GlobalSearchSectionRoot.DataContext = GlobalSearchSection;

        // Populate system data from system.xml (sidebar + top System ComboBox)
        PopulateSidebarFromSystemXml();

        // NOTE: no initial System ComboBox selection here — selecting a system would
        // fire SystemComboBox_SelectionChanged synchronously and trigger a full library
        // scan on the UI thread during construction. Window_Opened → InitializeAsync
        // does the single initial scan asynchronously instead.

        // Restore window position/size before the window is shown
        RestoreWindowBounds();

        // Pause/resume gamepad controller on window focus changes to prevent
        // mouse input leaking to other windows (mirrors the WPF app behavior).
        Activated += (_, _) =>
        {
            if (_wasControllerRunningBeforeDeactivation)
            {
                _ = _gamePadController.StartAsync();
                Log.Debug("Gamepad controller restarted on window activation.");
            }
            _wasControllerRunningBeforeDeactivation = false;
        };
        Deactivated += (_, _) =>
        {
            if (_gamePadController.IsRunning)
            {
                _wasControllerRunningBeforeDeactivation = true;
                _ = _gamePadController.StopAsync();
                Log.Debug("Gamepad controller temporarily stopped on window deactivation.");
            }
            else
            {
                _wasControllerRunningBeforeDeactivation = false;
            }
        };

        // Failsafe shutdown watchdog: if normal shutdown has not terminated the process
        // within the grace period, force-exit so the app can never linger in the background.
        Closed += (_, _) =>
        {
            // Unsubscribe event handlers to prevent memory leaks
            _viewModel.ToastRequested -= _toastRequestedHandler;
            _fileWatcher.GameFilesChanged -= _gameFilesChangedHandler;
            RemoveHandler(InputElement.PointerWheelChangedEvent, _pointerWheelChangedHandler);

            _shutdownWatchdogCts?.Cancel();
            _shutdownWatchdogCts?.Dispose();
            _shutdownWatchdogCts = new CancellationTokenSource();
            var token = _shutdownWatchdogCts.Token;

            _ = Task.Delay(TimeSpan.FromSeconds(5), token).ContinueWith(_ =>
            {
                try
                {
                    Log.Warning("Shutdown watchdog fired after window close; forcing process exit");
                    Environment.Exit(0);
                }
                catch (Exception ex)
                {
                    // Last resort — nothing else we can do
                    Log.Debug(ex, "Shutdown watchdog failed to force exit");
                }
            }, token);
        };

        // Set initial check marks from the saved settings (settings.xml)
        UpdateMenuCheckMarks();

        // Wire the pagination service to the status-bar controls and apply the saved
        // Games Per Page preference (mirrors the WPF app, which paginates the game
        // list once the total exceeds the configured page size).
        _pagination.Initialize(this);
        _viewModel.ConfigurePagination(_settings.GamesPerPage);

        // Populate the letter/number quick-filter bar (WPF FilterMenu parity) and
        // sync the MAME sort-order button tooltip from the current setting.
        PopulateLetterFilterBar();
        UpdateMameSortOrderButtonToolTip();

        // Ctrl+wheel zooms the card size over the game grid (WPF MainWindow_MouseWheelAsync parity).
        _pointerWheelChangedHandler = OnPointerWheelChangedForZoom;
        AddHandler(InputElement.PointerWheelChangedEvent, _pointerWheelChangedHandler, handledEventsToo: true);

        // Live library refresh: when a watched ROM folder changes on disk, reload the
        // current view on the UI thread (same debounced behavior as the WPF app).
        _gameFilesChangedHandler = (_, e) =>
        {
            try
            {
                Dispatcher.UIThread.Post(() =>
                {
                    // The affected system's cached file list is stale — drop it so the
                    // refresh below re-scans that system's folders from disk.
                    _viewModel.InvalidateGameFileCacheForSystem(e.Value);
                    _viewModel.RefreshCurrentView();
                    RefreshSidebarCounts();
                    ShowToast("Game Library", _localization.GetString("Toast.Refreshed", "Game list reloaded."));
                    _playSound.PlayNotificationSound();
                });
            }
            catch (Exception ex)
            {
                Log.Debug(ex, "Failed to refresh the game list after a file change for system '{System}'", e.Value);
            }
        };
        _fileWatcher.GameFilesChanged += _gameFilesChangedHandler;
    }

    #region IPaginationHost

    /// <inheritdoc />
    public void SetPrevPageButtonEnabled(bool enabled)
    {
        PrevPageButton.IsEnabled = enabled;
    }

    /// <inheritdoc />
    public void SetNextPageButtonEnabled(bool enabled)
    {
        NextPageButton.IsEnabled = enabled;
    }

    /// <inheritdoc />
    public void ScrollToTop()
    {
        if (GameGridView.Items.Count > 0)
        {
            GameGridView.ScrollIntoView(GameGridView.Items[0]!);
        }

        if (GameListView.Items.Count > 0)
        {
            GameListView.ScrollIntoView(GameListView.Items[0]!);
        }
    }

    /// <inheritdoc />
    public void UpdateTotalFilesLabel(string? text)
    {
        PaginationPanel.IsVisible = !string.IsNullOrEmpty(text);
        PaginationLabel.Text = text;
    }

    /// <inheritdoc />
    public void AddNoFilesMessage()
    {
        PaginationLabel.Text = "";
        StatusRight.Text = _localization.GetString("Empty.Title", "No Games Found");
    }

    /// <summary>
    /// Clears the status text (wired to the status-bar timeout timer of the startup
    /// initialization service).
    /// </summary>
    public void ResetStatusText()
    {
        StatusRight.Text = "";
    }

    private void PrevPage_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            _viewModel.GoToPreviousPage();
            _playSound.PlayNotificationSound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error navigating to the previous page");
        }
    }

    private void NextPage_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            _viewModel.GoToNextPage();
            _playSound.PlayNotificationSound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error navigating to the next page");
        }
    }

    #endregion

    /// <summary>
    /// Loads the game library after the window is shown so the UI thread is not
    /// blocked during window construction. Refreshes sidebar count badges when done.
    /// </summary>
    private async void Window_Opened(object? sender, EventArgs e)
    {
        try
        {
            await _viewModel.InitializeAsync();
            RefreshSidebarCounts();

            // Start watching all configured ROM folders (debounced live refresh)
            _fileWatcher.StartWatchingForSystems(_systemManagerService.LoadSystems());

            // WPF parity: show system selection screen at startup instead of
            // the All Games browser. User picks a system from the grid to begin.
            await ShowSystemSelectionScreenAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Main window load failed");
        }
    }

    /// <summary>
    /// Populates the sidebar system groups (manufacturer mapping, icons, counts)
    /// from system.xml — logic lives in SidebarViewModel.
    /// </summary>
    private void PopulateSidebarFromSystemXml()
    {
        try
        {
            var systems = _systemManagerService.LoadSystems();
            _viewModel.PopulateSidebar(systems);
            _viewModel.Sidebar.RefreshCounts(_viewModel.SystemGameCounts);
            _systemSelectionOrchestrator.LoadOrReloadSystemManager();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to populate sidebar from system.xml");
        }
    }

    /// <summary>
    /// Refreshes count badges on sidebar system entries after scanning.
    /// </summary>
    public void RefreshSidebarCounts()
    {
        _viewModel.Sidebar.RefreshCounts(_viewModel.SystemGameCounts);
    }

    #region Window Bounds Persistence

    private void Window_Closing(object? sender, WindowClosingEventArgs e)
    {
        Log.Debug("Main window closing");
        SaveBounds();
    }

    private CancellationTokenSource? _shutdownWatchdogCts;

    private void SaveBounds()
    {
        try
        {
            var dir = Path.GetDirectoryName(BoundsFilePath);
            if (dir is not null) Directory.CreateDirectory(dir);

            var data = new WindowBoundsData
            {
                State = WindowState.ToString()
            };

            if (WindowState == WindowState.Normal)
            {
                data.Left = Position.X;
                data.Top = Position.Y;
                data.Width = Width;
                data.Height = Height;
            }

            File.WriteAllText(BoundsFilePath, JsonSerializer.Serialize(data));
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to save window bounds");
        }
    }

    private void RestoreWindowBounds()
    {
        try
        {
            if (!File.Exists(BoundsFilePath)) return;

            var json = File.ReadAllText(BoundsFilePath);
            var data = JsonSerializer.Deserialize<WindowBoundsData>(json);
            switch (data)
            {
                case null:
                    return;
                case { Left: not null, Top: not null, Width: not null, Height: not null }:
                {
                    // Only restore when the saved top-left corner is still on some screen
                    // (positions may be negative on multi-monitor setups with left displays).
                    var corner = new PixelPoint((int)data.Left.Value, (int)data.Top.Value);
                    var onScreen = Screens?.All.Any(s => s.Bounds.Contains(corner)) ?? false;

                    if (onScreen)
                    {
                        Position = corner;
                        Width = data.Width.Value;
                        Height = data.Height.Value;
                    }

                    break;
                }
            }

            if (data.State is not null && Enum.TryParse<WindowState>(data.State, out var state))
            {
                WindowState = state;
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to restore window bounds");
        }
    }

    private sealed class WindowBoundsData
    {
        public double? Left { get; set; }
        public double? Top { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public string? State { get; set; }
    }

    #endregion

    #region View Toggle

    private string _lastSortColumn = "";
    private bool _sortAscending = true;

    private void ListHeader_Click(object? sender, PointerPressedEventArgs e)
    {
        if (sender is not TextBlock { Tag: string columnName } header)
            return;

        var collection = _viewModel.Games;

        // Toggle direction if same column
        if (_lastSortColumn == columnName)
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _sortAscending = true;
            _lastSortColumn = columnName;
        }

        var sorted = columnName switch
        {
            "Name" => _sortAscending
                ? collection.OrderBy(g => g.DisplayTitle)
                : collection.OrderByDescending(g => g.DisplayTitle),
            "System" => _sortAscending
                ? collection.OrderBy(g => g.SystemName)
                : collection.OrderByDescending(g => g.SystemName),
            "Times Played" => _sortAscending
                ? collection.OrderBy(g => g.PlayCount)
                : collection.OrderByDescending(g => g.PlayCount),
            "Path" => _sortAscending
                ? collection.OrderBy(g => g.FilePath)
                : collection.OrderByDescending(g => g.FilePath),
            _ => collection.OrderBy(g => g.DisplayTitle)
        };

        _viewModel.Games = new ObservableCollection<GameCardViewModel>(sorted);

        // Update header text with arrow
        foreach (var headerBlock in new[] { ListHeaderName, ListHeaderSystem, ListHeaderPlayed, ListHeaderPath })
        {
            var baseName = headerBlock.Tag as string ?? "";
            headerBlock.Text = baseName.Replace(" ▲", "").Replace(" ▼", "");
        }

        header.Text = columnName + (_sortAscending ? " ▲" : " ▼");
    }

    private void GameListView_DoubleClick(object? sender, TappedEventArgs e)
    {
        if (GameListView.SelectedItem is GameCardViewModel game)
            _viewModel.PlayGameCommand.Execute(game);
    }

    private void GameListItem_Click(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control { DataContext: GameCardViewModel game } item)
        {
            var properties = e.GetCurrentPoint(item).Properties;

            if (properties.IsRightButtonPressed)
            {
                ShowGameContextMenu(game, item);
                e.Handled = true; // prevent the popup from closing on the subsequent mouse-up
            }
            else
            {
                // Single left click: select the game (double-click is handled by the ListBox)
                var listBoxItem = FindParent<ListBoxItem>(item);
                if (listBoxItem is not null)
                {
                    listBoxItem.IsSelected = true;
                }
            }
        }
    }

    #endregion

    #region Keyboard Shortcuts

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        switch (e.Key)
        {
            case Key.Escape:
                SearchBox.Text = "";
                _viewModel.SearchText = "";
                _ = _uiResetService.ResetUiAsync();
                e.Handled = true;
                break;
            case Key.F when e.KeyModifiers.HasFlag(KeyModifiers.Control):
                SearchBox.Focus();
                SearchBox.SelectAll();
                e.Handled = true;
                break;
            case Key.Enter:
                switch (_viewModel.IsGridView)
                {
                    case true when GameGridView.SelectedItem is GameCardViewModel gridGame:
                        _viewModel.PlayGameCommand.Execute(gridGame);
                        break;
                    case false when GameListView.SelectedItem is GameCardViewModel listGame:
                        _viewModel.PlayGameCommand.Execute(listGame);
                        break;
                }

                e.Handled = true;
                break;
            case Key.F5:
                _viewModel.NavigateToAllGamesCommand.Execute(null);
                RefreshSidebarCounts();
                ShowToast("Refreshed", "Game list reloaded.");
                e.Handled = true;
                break;
        }
    }

    #endregion

    #region Emergency Return Button

    private void EmergencyReturnButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            _loadingOverlay.EmergencyRelease();
            ShowToast("Emergency Reset", _localization.GetString("Toast.EmergencyReset"));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in EmergencyReturnButton_Click");
        }
    }

    #endregion

    #region Quick Actions (Home / Random / Sort / Letter Bar)

    private async void HomeButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            await _uiResetService.ResetUiAsync();
            ScrollToTop();
            _playSound.PlayNotificationSound();
            UpdateLetterBarSelection("");
            ShowToast("Restart", _localization.GetString("Toast.Restarted", "Returned to the main game list."));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in HomeButton_Click");
        }
    }

    private async void RandomGameButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            // WPF parity: Feeling Lucky operates on the currently selected system
            if (string.IsNullOrEmpty(_viewModel.SelectedSystem))
            {
                ShowToast("Feeling Lucky", _localization.GetString("Toast.SelectSystemFirst", "Please select a system first."));
                return;
            }

            _playSound.PlayNotificationSound();

            // Reset the visible filter controls (the ViewModel resets its own state)
            UpdateLetterBarSelection("");
            SearchBox.Text = "";

            var randomGame = await _viewModel.PickRandomGameAsync();
            if (randomGame is null)
            {
                ShowToast("Feeling Lucky", _localization.GetString("Toast.NoGameFound", "No games found to pick from."));
                return;
            }

            // ListView mode auto-selects the picked game (WPF DataGrid row-0 parity);
            // GridView intentionally leaves nothing selected, like the WPF app.
            if (!_viewModel.IsGridView)
            {
                GameListView.SelectedItem = randomGame;
                GameListView.ScrollIntoView(randomGame);
            }

            ShowToast("Feeling Lucky", _localization.GetString("Toast.FeelingLucky", "Picked a random game."));
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in RandomGameButton_Click");
            var messageBox = App.ServiceProvider.GetRequiredService<IMessageBoxLibraryService>();
            await messageBox.ErrorMessageBoxAsync();
        }
    }

    /// <summary>Star button (WPF SelectedSystemFavoriteButton parity): favorites of the selected system.</summary>
    private void SelectedSystemFavoriteButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            _playSound.PlayNotificationSound();

            if (string.IsNullOrEmpty(_viewModel.SelectedSystem))
            {
                _viewModel.NavigateToFavoritesCommand.Execute(null);
                return;
            }

            _viewModel.NavigateToSelectedSystemFavoritesCommand.Execute(null);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method SelectedSystemFavoriteButton_Click");
        }
    }

    private void MameSortOrderButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            _playSound.PlayNotificationSound();
            _viewModel.ToggleMameSortOrder();
            UpdateMameSortOrderButtonToolTip();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in MameSortOrderButton_Click");
        }
    }

    private void UpdateMameSortOrderButtonToolTip()
    {
        try
        {
            var tooltip = string.Equals(_viewModel.MameSortOrder, "MachineDescription", StringComparison.Ordinal)
                ? "Sort order: Machine Description"
                : "Sort order: File Name";
            ToolTip.SetTip(MameSortOrderButton, tooltip);
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to update the MAME sort order button tooltip");
        }
    }

    private void PopulateLetterFilterBar()
    {
        try
        {
            LetterFilterBar.Children.Clear();
            AddLetterButton("All", "");
            AddLetterButton("#", "#");
            foreach (var c in Enumerable.Range('A', 26).Select(static x => (char)x))
            {
                AddLetterButton(c.ToString(), c.ToString());
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to populate the letter filter bar");
        }
    }

    private void AddLetterButton(string label, string letter)
    {
        var button = new Button
        {
            Content = label,
            Padding = new Thickness(7, 2),
            MinWidth = 28,
            Tag = letter
        };
        button.Classes.Add("toolbar-icon");
        button.Click += (_, _) => LetterFilterButton_Click(letter, button);
        LetterFilterBar.Children.Add(button);
    }

    private void LetterFilterButton_Click(string letter, Button clickedButton)
    {
        try
        {
            _playSound.PlayNotificationSound();
            _viewModel.SetLetterFilter(letter);
            UpdateLetterBarSelection(letter);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in LetterFilterButton_Click");
        }
    }

    private void UpdateLetterBarSelection(string letter)
    {
        foreach (var child in LetterFilterBar.Children)
        {
            if (child is not Button button) continue;

            var isActive = string.Equals((button.Tag as string) ?? "", letter, StringComparison.Ordinal);
            button.Classes.Set("active", isActive);
        }
    }

    private void OnPointerWheelChangedForZoom(object? sender, PointerWheelEventArgs e)
    {
        if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
        {
            if (e.Delta.Y > 0)
            {
                _viewModel.ZoomIn();
                e.Handled = true;
            }
            else if (e.Delta.Y < 0)
            {
                _viewModel.ZoomOut();
                e.Handled = true;
            }
        }
    }

    #endregion

    #region Category Tabs

    /// <summary>
    /// The page sections embedded in the content area (WPF FavoritesPage /
    /// PlayHistoryPage / GlobalSearchPage equivalents).
    /// </summary>
    private enum MainSection
    {
        None,
        Favorites,
        PlayHistory,
        GlobalSearch
    }

    /// <summary>
    /// Shows the requested section and hides the others (including the game browser).
    /// Favorites and Play History reload their data every time they are opened.
    /// </summary>
    private async Task ShowSectionAsync(MainSection section)
    {
        // System selection screen is always hidden when a content view is active
        SystemSelectionRoot.IsVisible = false;

        FavoritesSectionRoot.IsVisible = section == MainSection.Favorites;
        PlayHistorySectionRoot.IsVisible = section == MainSection.PlayHistory;
        GlobalSearchSectionRoot.IsVisible = section == MainSection.GlobalSearch;
        GameBrowserPanel.IsVisible = section == MainSection.None;

        switch (section)
        {
            case MainSection.Favorites:
                await FavoritesSection.LoadFavoritesAsync();
                break;
            case MainSection.PlayHistory:
                await PlayHistorySection.LoadHistoryAsync();
                break;
        }
    }

    #endregion

    #region Search

    private void SearchBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        var query = SearchBox.Text;
        _viewModel.SearchText = query ?? "";
        _viewModel.StatusText = string.IsNullOrEmpty(query) ? "Ready" : $"Search: \"{query}\"";
    }

    /// <summary>Search button (WPF SearchButton parity): re-applies the current filter.</summary>
    private void SearchButton_Click(object? sender, RoutedEventArgs e)
    {
        var query = SearchBox.Text;
        _viewModel.SearchText = query ?? "";
        _viewModel.StatusText = string.IsNullOrEmpty(query) ? "Ready" : $"Search: \"{query}\"";
        SearchBox.Focus();
    }

    #endregion

    #region System & Emulator Selection (Top Bar)

    /// <summary>
    /// Top System ComboBox: delegates to the system selection orchestrator
    /// which navigates to the selected system and refreshes the Emulator ComboBox.
    /// </summary>
    private void SystemComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _systemSelectionOrchestrator.HandleSystemSelectionChanged();
    }

    /// <summary>
    /// Top Emulator ComboBox: stores the chosen emulator so launches use it instead
    /// of the system's first emulator.
    /// </summary>
    private void EmulatorComboBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        _viewModel.SelectedEmulatorName = EmulatorComboBox.SelectedItem?.ToString();
    }

    #endregion

    #region Left Navigation Rail

    /// <summary>Nav rail: restart / home — returns to the All Games view.</summary>
    private void NavRestartButton_Click(object? sender, RoutedEventArgs e)
    {
        HomeButton_Click(sender, e);
    }

    /// <summary>Nav rail: opens the Favorites section.</summary>
    private async void NavFavoritesButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            await ShowSectionAsync(MainSection.Favorites);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method NavFavoritesButton_Click");
        }
    }

    /// <summary>Nav rail: opens the Global Search section.</summary>
    private async void NavGlobalSearchButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            await ShowSectionAsync(MainSection.GlobalSearch);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method NavGlobalSearchButton_Click");
        }
    }

    /// <summary>Nav rail: opens the Play History section.</summary>
    private async void NavHistoryButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            await ShowSectionAsync(MainSection.PlayHistory);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method NavHistoryButton_Click");
        }
    }

    /// <summary>Nav rail: opens the RetroAchievements window.</summary>
    private void NavRetroAchievementsButton_Click(object? sender, RoutedEventArgs e)
    {
        RetroAchievements_Click(sender, e);
    }

    /// <summary>Nav rail: opens the Edit System (expert) window.</summary>
    private void NavEditSystemButton_Click(object? sender, RoutedEventArgs e)
    {
        ExpertMode_Click(sender, e);
    }

    /// <summary>Nav rail: toggles between grid and list view (WPF ToggleViewMode parity).</summary>
    private void NavToggleViewModeButton_Click(object? sender, RoutedEventArgs e)
    {
        _viewModel.IsGridView = !_viewModel.IsGridView;
        _settings.ViewMode = _viewModel.IsGridView ? "GridView" : "ListView";
        _ = _settings.SaveAsync();
        UpdateViewModeCheckMarks();
        _playSound.PlayNotificationSound();
    }

    /// <summary>Nav rail: cycles the button aspect ratio (WPF NavToggleButtonAspectRatio parity).</summary>
    private async void NavToggleButtonAspectRatioButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            // WPF parity: ignore the toggle while a game-library load is in progress.
            if (_viewModel.IsLoading)
            {
                return;
            }

            var aspectRatios = new List<string> { "Square", "Wider", "SuperWider", "SuperWider2", "Taller", "SuperTaller", "SuperTaller2" };

            var currentIndex = Math.Max(aspectRatios.IndexOf(_settings.ButtonAspectRatio ?? "Square"), 0);
            var nextIndex = (currentIndex + 1) % aspectRatios.Count;
            var newAspectRatio = aspectRatios[nextIndex];

            _settings.ButtonAspectRatio = newAspectRatio;
            await _settings.SaveAsync();
            UpdateButtonAspectRatioCheckMarks(newAspectRatio);
            _viewModel.ReloadGames();
            _playSound.PlayNotificationSound();
            ShowToast("Button Aspect Ratio", $"Toggling button aspect ratio... {newAspectRatio}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method NavToggleButtonAspectRatioButton_Click");
        }
    }

    /// <summary>Nav rail: zooms the card size in (WPF NavZoomInButton parity).</summary>
    private void NavZoomInButton_Click(object? sender, RoutedEventArgs e)
    {
        _viewModel.ZoomIn();
        _playSound.PlayNotificationSound();
    }

    /// <summary>Nav rail: zooms the card size out (WPF NavZoomOutButton parity).</summary>
    private void NavZoomOutButton_Click(object? sender, RoutedEventArgs e)
    {
        _viewModel.ZoomOut();
        _playSound.PlayNotificationSound();
    }

    #endregion

    #region Game Card Interaction

    /// <summary>Single left click on a game card: launches the game (and keeps it selected).</summary>
    private void GameCard_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is Control { DataContext: GameCardViewModel game } card)
        {
            var listBoxItem = FindParent<ListBoxItem>(card);
            if (listBoxItem is not null)
            {
                listBoxItem.IsSelected = true;
            }

            _viewModel.PlayGameCommand.Execute(game);
        }
    }

    /// <summary>Right-button press on a game card: opens the context menu.</summary>
    private void GameCard_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control { DataContext: GameCardViewModel game } card)
        {
            var properties = e.GetCurrentPoint(card).Properties;

            if (properties.IsRightButtonPressed)
            {
                ShowGameContextMenu(game, card);
                e.Handled = true; // prevent the popup from closing on the subsequent mouse-up
            }
        }
    }

    private void ShowGameContextMenu(GameCardViewModel game, Control placementTarget)
    {
        var context = BuildRightClickContext(game.FilePath, game.SystemName, game);
        _contextMenuService.ShowContextMenu(context, placementTarget, BuildExtraCallbacks());
    }

    /// <summary>Builds the WPF-parity right-click context for a game file.</summary>
    private AvaloniaRightClickContext BuildRightClickContext(string filePath, string systemName, GameCardViewModel? card = null,
        string? fileNameWithExtensionOverride = null, Action? onFavoriteRemoved = null)
    {
        var sp = App.ServiceProvider;
        var fileNameWithExtension = fileNameWithExtensionOverride ?? Path.GetFileName(filePath) ?? filePath;
        var safeFilePath = filePath ?? "";
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(safeFilePath);
        return new AvaloniaRightClickContext(
            filePath: safeFilePath,
            fileNameWithExtension: fileNameWithExtension,
            fileNameWithoutExtension: fileNameWithoutExtension,
            selectedSystemName: systemName,
            selectedSystemManager: _systemManagerService,
            settings: sp.GetRequiredService<SettingsManagerService>(),
            favoritesManager: sp.GetRequiredService<Services.Favorites.FavoritesManager>(),
            ownerWindow: this,
            mainViewModel: _viewModel,
            sourceCard: card,
            onFavoriteRemoved: onFavoriteRemoved);
    }

    /// <summary>Avalonia-only extras appended to the WPF-parity context menu.</summary>
    private GameContextMenuCallbacks BuildExtraCallbacks()
    {
        return new GameContextMenuCallbacks
        {
            OnShowDetails = OpenGameDetail,
            OnCopyPath = g =>
            {
                _ = CopyToClipboardAsync(g.FilePath);
                ShowToast(_localization.GetString("Context.Copied"), g.FilePath);
            },
            OnCopyName = g =>
            {
                var fileName = Path.GetFileName(g.FilePath);
                _ = CopyToClipboardAsync(fileName);
                ShowToast(_localization.GetString("Context.Copied"), fileName);
            },
            OnShowInFolder = g => _ = ShowGameInFolderAsync(g),
            OnEditSystem = OpenEditSystemForGame
        };
    }

    /// <summary>
    /// Reveals the game's containing folder in the OS file manager.
    /// Windows uses explorer's /select to highlight the file; other platforms open the folder.
    /// </summary>
    private async Task ShowGameInFolderAsync(GameCardViewModel game)
    {
        var directory = Path.GetDirectoryName(game.FilePath);
        if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
        {
            ShowToast("Show in Folder", "Folder not found.");
            return;
        }

        try
        {
            if (OperatingSystem.IsWindows())
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "explorer.exe",
                    Arguments = $"/select,\"{game.FilePath}\"",
                    UseShellExecute = true
                });
            }
            else if (GetTopLevel(this)?.Launcher is { } launcher)
            {
                await launcher.LaunchDirectoryInfoAsync(new DirectoryInfo(directory));
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to show game in folder: {Path}", game.FilePath);
            ShowToast("Show in Folder", "Could not open the folder.");
        }
    }

    /// <summary>
    /// Opens the Edit System window (Expert Mode) pre-selected to the game's system.
    /// </summary>
    private void OpenEditSystemForGame(GameCardViewModel game)
    {
        try
        {
            if (string.IsNullOrEmpty(game.SystemName)) return;

            var factory = App.ServiceProvider.GetRequiredService<Func<string?, EditSystemWindow>>();
            var editWindow = factory(game.SystemName);
            editWindow.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method OpenEditSystemForGame");
        }
    }

    private async Task CopyToClipboardAsync(string text)
    {
        try
        {
            if (Clipboard is null) return;

            var dataTransfer = new DataTransfer();
            dataTransfer.Add(DataTransferItem.CreateText(text));
            await Clipboard.SetDataAsync(dataTransfer);
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to copy text to clipboard");
        }
    }

    /// <summary>
    /// Opens the GameDetailWindow for the given game.
    /// </summary>
    private void OpenGameDetail(GameCardViewModel game)
    {
        // GameDetailWindow takes per-game constructor arguments (game + main VM),
        // so it is created manually — DI cannot resolve it.
        var window = new GameDetailWindow(game, _viewModel);
        window.ShowDialog(this);
    }

    private static T? FindParent<T>(Visual child) where T : Visual
    {
        return child.FindAncestorOfType<T>();
    }

    #endregion

    #region Page Sections (Favorites / Play History / Global Search)

    private async void FavoritesDataGrid_DoubleTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            await FavoritesSection.LaunchSelectedCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method FavoritesDataGrid_DoubleTapped");
        }
    }

    /// <summary>Remove button (WPF RemoveFavoriteButton_ClickAsync parity): removes all selected favorites.</summary>
    private async void FavoritesRemoveButton_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var selected = FavoritesDataGrid.SelectedItems.Cast<FavoriteRowViewModel>().ToList();
            await FavoritesSection.RemoveFavoritesAsync(selected);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method FavoritesRemoveButton_Click");
        }
    }

    /// <summary>Right-click on a favorites row: opens the WPF-parity context menu.</summary>
    private void FavoritesDataGrid_RightClick(object? sender, PointerReleasedEventArgs e)
    {
        try
        {
            if (e.InitialPressMouseButton != MouseButton.Right) return;
            if (e.Source is not Visual favoritesVisual
                || FindParent<DataGridRow>(favoritesVisual) is not { } row
                || row.DataContext is not FavoriteRowViewModel favorite)
            {
                return;
            }

            // WPF stores favorites as a file NAME resolved against the system folders;
            // resolve it for launch/media actions while keeping the stored name for
            // favorites matching.
            var filePath = FavoritesSection.ResolveFavoritePath(favorite) ?? favorite.FilePath;
            var context = BuildRightClickContext(
                filePath, favorite.SystemName,
                fileNameWithExtensionOverride: favorite.FilePath,
                onFavoriteRemoved: () => _ = FavoritesSection.LoadFavoritesAsync());

            _contextMenuService.ShowContextMenu(context, FavoritesDataGrid);
            e.Handled = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method FavoritesDataGrid_RightClick");
        }
    }

    /// <summary>Right-click on a play history row: opens the WPF-parity context menu.</summary>
    private void PlayHistoryDataGrid_RightClick(object? sender, PointerReleasedEventArgs e)
    {
        try
        {
            if (e.InitialPressMouseButton != MouseButton.Right) return;
            if (e.Source is not Visual historyVisual
                || FindParent<DataGridRow>(historyVisual) is not { } row
                || row.DataContext is not PlayHistoryItem item)
            {
                return;
            }

            var context = BuildRightClickContext(item.FileName, item.SystemName);
            _contextMenuService.ShowContextMenu(context, PlayHistoryDataGrid);
            e.Handled = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method PlayHistoryDataGrid_RightClick");
        }
    }

    /// <summary>Right-click on a global search result row: opens the WPF-parity context menu.</summary>
    private void GlobalSearchResults_RightClick(object? sender, PointerReleasedEventArgs e)
    {
        try
        {
            if (e.InitialPressMouseButton != MouseButton.Right) return;
            if (e.Source is not Visual searchVisual
                || FindParent<DataGridRow>(searchVisual) is not { } row
                || row.DataContext is not SearchResult result)
            {
                return;
            }

            var context = BuildRightClickContext(result.FilePath, result.SystemName);
            _contextMenuService.ShowContextMenu(context, GlobalSearchResultsDataGrid);
            e.Handled = true;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method GlobalSearchResults_RightClick");
        }
    }

    private async void FavoritesDataGrid_KeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            switch (e.Key)
            {
                case Key.Delete:
                    e.Handled = true;
                    var selected = FavoritesDataGrid.SelectedItems.Cast<FavoriteRowViewModel>().ToList();
                    await FavoritesSection.RemoveFavoritesAsync(selected);
                    break;
                case Key.Enter:
                    e.Handled = true;
                    await FavoritesSection.LaunchSelectedCommand.ExecuteAsync(null);
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method FavoritesDataGrid_KeyDown");
        }
    }

    private async void PlayHistoryDataGrid_DoubleTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            await PlayHistorySection.LaunchSelectedCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method PlayHistoryDataGrid_DoubleTapped");
        }
    }

    private async void PlayHistoryDataGrid_KeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            switch (e.Key)
            {
                case Key.Delete:
                    e.Handled = true;
                    await PlayHistorySection.RemoveSelectedCommand.ExecuteAsync(null);
                    break;
                case Key.Enter:
                    e.Handled = true;
                    await PlayHistorySection.LaunchSelectedCommand.ExecuteAsync(null);
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method PlayHistoryDataGrid_KeyDown");
        }
    }

    private async void GlobalSearchTextBox_KeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            if (e.Key != Key.Enter) return;

            e.Handled = true;
            await GlobalSearchSection.SearchCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method GlobalSearchTextBox_KeyDown");
        }
    }

    private async void GlobalSearchResults_DoubleTapped(object? sender, TappedEventArgs e)
    {
        try
        {
            await GlobalSearchSection.LaunchSelectedCommand.ExecuteAsync(null);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method GlobalSearchResults_DoubleTapped");
        }
    }

    #endregion

    #region EasyMode

    private async void AddSystem_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var easyModeWindow = App.ServiceProvider.GetRequiredService<EasyModeWindow>();
            // Avalonia ShowDialog returns immediately without a nested pump — must await it
            // before reading the result (otherwise the SystemAdded refresh below never runs).
            await easyModeWindow.ShowDialog(this);

            // If a system was added, refresh the UI
            if (easyModeWindow.DataContext is EasyModeViewModel { SystemAdded: true })
            {
                _viewModel.InvalidateAllGameFileCaches();
                _viewModel.NavigateToAllGamesCommand.Execute(null);

                // Rebuild the sidebar so the new system (e.g. Atari 2600) appears in the
                // left menu immediately and can be clicked to filter its games.
                await _systemSelectionOrchestrator.ReloadAfterConfigurationChangeAsync();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in method AddSystem_Click");
        }
    }

    #endregion

    #region Emulator Settings (config injection)

    /// <summary>
    /// Opens a menu listing every emulator whose configuration can be injected,
    /// then opens the matching config window in standalone mode (no launch).
    /// Same feature as the WPF app's "Tools → Inject Emulator Config" menu.
    /// </summary>
    private void EmulatorSettings_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Control placementTarget) return;

        var menu = new ContextMenu
        {
            Placement = PlacementMode.Pointer
        };

        AddInjectMenuItem(menu, "Ares", () => OpenInjectWindow<InjectAresConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "Azahar", () => OpenInjectWindow<InjectAzaharConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "Blastem", () => OpenInjectWindow<InjectBlastemConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "Cemu", () => OpenInjectWindow<InjectCemuConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "Daphne", () => OpenInjectWindow<InjectDaphneConfigWindow>(w => w.Initialize(false)));
        AddInjectMenuItem(menu, "Dolphin", () => OpenInjectWindow<InjectDolphinConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "DuckStation", () => OpenInjectWindow<InjectDuckStationConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "Flycast", () => OpenInjectWindow<InjectFlycastConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "MAME", () => OpenInjectWindow<InjectMameConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "Mednafen", () => OpenInjectWindow<InjectMednafenConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "Mesen", () => OpenInjectWindow<InjectMesenConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "PCSX2", () => OpenInjectWindow<InjectPcsx2ConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "Raine", () => OpenInjectWindow<InjectRaineConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "Redream", () => OpenInjectWindow<InjectRedreamConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "RetroArch", () => OpenInjectWindow<InjectRetroArchConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "RPCS3", () => OpenInjectWindow<InjectRpcs3ConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "SEGA Model 2", () => OpenInjectWindow<InjectSegaModel2ConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "Stella", () => OpenInjectWindow<InjectStellaConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "Supermodel", () => OpenInjectWindow<InjectSupermodelConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "Xenia", () => OpenInjectWindow<InjectXeniaConfigWindow>(w => w.Initialize(null, false)));
        AddInjectMenuItem(menu, "Yumir", () => OpenInjectWindow<InjectYumirConfigWindow>(w => w.Initialize(null, false)));

        menu.Open(placementTarget);
    }

    private void OpenInjectWindow<T>(Action<T> initialize) where T : Window
    {
        var win = App.ServiceProvider.GetRequiredService<T>();
        initialize(win);
        win.ShowDialog(this);
    }

    private static void AddInjectMenuItem(ContextMenu menu, string header, Action openWindow)
    {
        var item = new MenuItem { Header = header };
        item.Click += (_, _) =>
        {
            try
            {
                openWindow();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open {Emulator} config window", header);
            }
        };
        menu.Items.Add(item);
    }

    #endregion

    #region RetroAchievements

    /// <summary>
    /// Opens the RetroAchievements profile window.
    /// </summary>
    private void RetroAchievements_Click(object? sender, RoutedEventArgs e)
    {
        var raWindow = App.ServiceProvider.GetRequiredService<RetroAchievementsWindow>();
        if (raWindow.IsVisible)
        {
            raWindow.Activate();
            return;
        }

        raWindow.Show(this);
    }

    /// <summary>
    /// Computes the game hash, looks it up in the local RA database and opens the
    /// per-game achievements window (same flow as the WPF context menu).
    /// </summary>
    private async Task OpenRetroAchievementsForGameAsync(GameCardViewModel game)
    {
        var sp = App.ServiceProvider;
        var logger = sp.GetRequiredService<ILogger>();
        var messageBox = sp.GetRequiredService<IMessageBoxLibraryService>();
        var raManager = sp.GetRequiredService<RetroAchievementsManager>();
        var raHasherTool = sp.GetRequiredService<IRetroAchievementsHasherTool>();
        var playSound = sp.GetRequiredService<PlaySoundEffects>();

        var filePath = game.FilePath;
        var systemName = game.SystemName;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath) ?? filePath;

        try
        {
            if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrWhiteSpace(systemName))
            {
                await messageBox.ErrorMessageBoxAsync();
                return;
            }

            var fileFormatsToLaunch = _systemManagerService.GetSystem(systemName)?.FileFormatsToLaunch
                                      ?? new List<string>();

            // Loading adapter: show a toast while hashing (MainWindow has no overlay)
            ILoadingState loadingState = new ToastLoadingState((title, message) => ShowToast(title, message));

            ShowToast("RetroAchievements", "Calculating game hash... Please wait.");

            var raHashResult = await raHasherTool.GetGameHashForRetroAchievementsAsync(
                filePath, systemName, fileFormatsToLaunch, loadingState, logger);

            if (string.Equals(raHashResult.ExtractionErrorMessage, "System selection cancelled by user.", StringComparison.Ordinal))
            {
                logger.Debug("[RA Service] User cancelled RetroAchievements hashing.");
                return;
            }

            var hash = raHashResult.Hash;

            if (string.IsNullOrEmpty(hash))
            {
                // Check if the failure was due to "system not supported"
                if (raHashResult.ExtractionErrorMessage?.Contains("not supported for RetroAchievements hashing", StringComparison.OrdinalIgnoreCase) == true)
                {
                    var result = await messageBox.GameNotSupportedByRetroAchievementsMessageBoxAsync();
                    if (result == MessageBoxResult.Yes)
                    {
                        playSound.PlayNotificationSound();
                        var retroAchievementsWindow = sp.GetRequiredService<RetroAchievementsWindow>();
                        retroAchievementsWindow.Show(this);
                    }
                }
                else if (!raHashResult.IsExtractionSuccessful)
                {
                    await messageBox.ExtractionFailedMessageBoxAsync(); // Inform user about extraction failure
                }
                else
                {
                    var result = await messageBox.GameNotSupportedByRetroAchievementsMessageBoxAsync();
                    if (result == MessageBoxResult.Yes)
                    {
                        playSound.PlayNotificationSound();
                        var retroAchievementsWindow = sp.GetRequiredService<RetroAchievementsWindow>();
                        retroAchievementsWindow.Show(this);
                    }
                }

                return;
            }

            logger.Debug($"[RA Service] Successfully obtained hash: {hash}");

            // Look up the hash in the local database
            var matchedGame = raManager.GetGameInfoByHash(hash);

            if (matchedGame != null)
            {
                logger.Debug($"[RA Service] Found match for hash: {hash} -> {matchedGame.Title} (ID: {matchedGame.Id})");

                var achievementsWindow = sp.GetRequiredService<RetroAchievementsForAGameWindow>();
                achievementsWindow.Initialize(matchedGame.Id, fileNameWithoutExtension);
                achievementsWindow.Show(this);
            }
            else
            {
                logger.Debug($"[RA Service] No match found for hash: {hash}");

                var result = await messageBox.GameNotSupportedByRetroAchievementsMessageBoxAsync();
                if (result == MessageBoxResult.Yes)
                {
                    playSound.PlayNotificationSound();
                    var retroAchievementsWindow = sp.GetRequiredService<RetroAchievementsWindow>();
                    retroAchievementsWindow.Show(this);
                }
            }
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Error opening RetroAchievements for the selected game.");
            await messageBox.CouldNotOpenAchievementsWindowMessageBoxAsync();
        }
    }

    /// <summary>
    /// ILoadingState adapter that surfaces loading messages as toasts.
    /// </summary>
    private sealed class ToastLoadingState(Action<string, string> showToast) : ILoadingState
    {
        private readonly Action<string, string> _showToast = showToast;

        public void SetLoadingState(bool isLoading, string? message = null)
        {
            if (isLoading && !string.IsNullOrEmpty(message))
            {
                _showToast("RetroAchievements", message);
            }
        }
    }

    #endregion

    #region Menu Bar

    /// <summary>
    /// Initializes check marks on all menu items from the saved settings (settings.xml).
    /// Called once after the window is constructed.
    /// </summary>
    private void UpdateMenuCheckMarks()
    {
        // Language
        UpdateLanguageCheckMarks(_settings.Language);

        // Button size
        UpdateThumbnailSizeCheckMarks(_settings.ThumbnailSize);

        // Aspect ratio
        UpdateButtonAspectRatioCheckMarks(_settings.ButtonAspectRatio);

        // Games per page
        UpdateGamesPerPageCheckMarks(_settings.GamesPerPage);

        // View mode
        UpdateViewModeCheckMarks();

        // Show games
        UpdateShowGamesCheckMarks(_settings.ShowGames);

        // Filename preferences
        UpdateFilenameCheckMarks();

        // Gamepad / fuzzy / annotation
        ToggleGamepad.IsChecked = _settings.EnableGamePadNavigation;
        ToggleFuzzyMatching.IsChecked = _settings.EnableFuzzyMatching;
        ToggleAnnotationStripping.IsChecked = _settings.EnableAnnotationStripping;

        // Overlay buttons
        RetroAchievementButton.IsChecked = _settings.OverlayRetroAchievementButton;
        VideoLinkButton.IsChecked = _settings.OverlayOpenVideoButton;
        InfoLinkButton.IsChecked = _settings.OverlayOpenInfoButton;
    }

    private void UpdateLanguageCheckMarks(string lang)
    {
        // Check exactly the language menu item whose code matches the active language
        var checkedName = _languageMenu.GetMenuItemNameForLanguageCode(lang);
        foreach (var item in LanguageMenu.Items.OfType<MenuItem>())
        {
            item.IsChecked = _languageMenu.IsLanguageMenuItem(item.Name) && string.Equals(item.Name, checkedName, StringComparison.Ordinal);
        }
    }

    private void UpdateThumbnailSizeCheckMarks(int size)
    {
        _menuCheckMarks.UpdateCheckedByTag(SizeMenu.Items.OfType<MenuItem>(), size);
    }

    private void UpdateButtonAspectRatioCheckMarks(string? ratio)
    {
        _menuCheckMarks.UpdateCheckedByName(AspectRatioMenu.Items.OfType<MenuItem>(), ratio);
    }

    private void UpdateGamesPerPageCheckMarks(int page)
    {
        _menuCheckMarks.UpdateCheckedByTag(GamesPerPageMenu.Items.OfType<MenuItem>(), page);
    }

    private void UpdateViewModeCheckMarks()
    {
        var grid = _viewModel.IsGridView;
        GridView.IsChecked = grid;
        ListView.IsChecked = !grid;
    }

    private void UpdateShowGamesCheckMarks(string? mode)
    {
        ShowAll.IsChecked = string.Equals(mode, "ShowAll", StringComparison.Ordinal);
        ShowWithCover.IsChecked = string.Equals(mode, "ShowWithCover", StringComparison.Ordinal);
        ShowWithoutCover.IsChecked = string.Equals(mode, "ShowWithoutCover", StringComparison.Ordinal);
    }

    private void UpdateFilenameCheckMarks()
    {
        var mode = _settings.FilenameDisplayMode;
        FilenameDisplayOriginal.IsChecked = string.Equals(mode, "Original", StringComparison.Ordinal);
        FilenameDisplayCleanUp.IsChecked = string.Equals(mode, "CleanUp", StringComparison.Ordinal);
        FilenameDisplayNoFilename.IsChecked = string.Equals(mode, "NoFilename", StringComparison.Ordinal);
        DisplayMachineNameToggle.IsChecked = _settings.DisplayMachineName;

        UpdateFontSizeCheckMarks(FilenameFontSizeMenu, FilenameFontSizeSmall, FilenameFontSizeNormal, FilenameFontSizeBig, _settings.FilenameFontSize);
        UpdateFontSizeCheckMarks(MachineNameFontSizeMenu, MachineNameFontSizeSmall, MachineNameFontSizeNormal, MachineNameFontSizeBig, _settings.MachineNameFontSize);
    }

    private static void UpdateFontSizeCheckMarks(MenuItem menu, MenuItem small, MenuItem normal, MenuItem big, string? value)
    {
        _ = menu; // container passed for symmetry with the other check-mark helpers
        small.IsChecked = string.Equals(value, "Small", StringComparison.Ordinal);
        normal.IsChecked = string.Equals(value, "Normal", StringComparison.Ordinal);
        big.IsChecked = string.Equals(value, "Big", StringComparison.Ordinal);
    }

    // ── Language ──

    private async void ChangeLanguage_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem item) return;

            var lang = _languageMenu.GetLanguageCodeFromMenuItemName(item.Name) ?? "en";

            _settings.Language = lang;
            await _settings.SaveAsync();
            _localization.LoadLanguage(lang);
            UpdateLanguageCheckMarks(lang);
            _playSound.PlayNotificationSound();
            ShowToast("Language", $"Language set to {lang}. Restart the app to apply it everywhere.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method ChangeLanguage_Click");
        }
    }

    // ── Button size ──

    private async void ButtonSizeClickAsync(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem { Tag: string tag } || !int.TryParse(tag, out var size)) return;

            _settings.ThumbnailSize = size;
            await _settings.SaveAsync();
            _viewModel.CardWidth = size;
            UpdateThumbnailSizeCheckMarks(size);
            _playSound.PlayNotificationSound();
            ShowToast("Button Size", $"{size} px");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method ButtonSizeClickAsync");
        }
    }

    // ── Button aspect ratio ──

    private async void ButtonAspectRatioClickAsync(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem item) return;

            var ratio = item.Name ?? "Square";
            _settings.ButtonAspectRatio = ratio;
            await _settings.SaveAsync();
            UpdateButtonAspectRatioCheckMarks(ratio);
            _viewModel.ReloadGames();
            _playSound.PlayNotificationSound();
            ShowToast("Button Aspect Ratio", ratio);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method ButtonAspectRatioClickAsync");
        }
    }

    // ── Games per page ──

    private async void GamesPerPageClickAsync(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem { Tag: string tag } || !int.TryParse(tag, out var page)) return;

            _settings.GamesPerPage = page;
            await _settings.SaveAsync();
            UpdateGamesPerPageCheckMarks(page);
            _viewModel.ConfigurePagination(page);
            _viewModel.ReloadGames();
            _playSound.PlayNotificationSound();
            ShowToast("Games Per Page", $"Preference saved: {page} games per page.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method GamesPerPageClickAsync");
        }
    }

    // ── View mode ──

    private void ChangeViewMode_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem item) return;

            _viewModel.IsGridView = string.Equals(item.Name, "GridView", StringComparison.Ordinal);
            _settings.ViewMode = _viewModel.IsGridView ? "GridView" : "ListView";
            _ = _settings.SaveAsync().ContinueWith(t =>
            {
                if (t.IsFaulted) Log.Warning(t.Exception, "Failed to save view mode preference");
            }, TaskContinuationOptions.OnlyOnFaulted);
            UpdateViewModeCheckMarks();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method ChangeViewMode_Click");
        }
    }

    // ── Show games filter ──

    private async void ShowGamesClickAsync(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem item) return;

            var mode = item.Name switch
            {
                "ShowWithCover" => "ShowWithCover",
                "ShowWithoutCover" => "ShowWithoutCover",
                _ => "ShowAll"
            };

            _settings.ShowGames = mode;
            await _settings.SaveAsync();
            UpdateShowGamesCheckMarks(mode);
            _viewModel.ReloadGames();
            _playSound.PlayNotificationSound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method ShowGamesClickAsync");
        }
    }

    // ── Filename preferences ──

    private async void FilenameDisplayMode_ClickAsync(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem item) return;

            var mode = item.Name switch
            {
                "FilenameDisplayCleanUp" => "CleanUp",
                "FilenameDisplayNoFilename" => "NoFilename",
                _ => "Original"
            };

            _settings.FilenameDisplayMode = mode;
            await _settings.SaveAsync();
            UpdateFilenameCheckMarks();
            _viewModel.ReloadGames();
            _playSound.PlayNotificationSound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method FilenameDisplayMode_ClickAsync");
        }
    }

    private async void DisplayMachineName_ClickAsync(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem item) return;

            _settings.DisplayMachineName = item.IsChecked;
            await _settings.SaveAsync();
            UpdateFilenameCheckMarks();
            _viewModel.ReloadGames();
            _playSound.PlayNotificationSound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method DisplayMachineName_ClickAsync");
        }
    }

    private async void FilenameFontSize_ClickAsync(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem item) return;

            var size = item.Name switch
            {
                "FilenameFontSizeSmall" => "Small",
                "FilenameFontSizeBig" => "Big",
                _ => "Normal"
            };

            _settings.FilenameFontSize = size;
            await _settings.SaveAsync();
            _viewModel.CaptionFontSize = size switch
            {
                "Small" => 11,
                "Big" => 16,
                _ => 13
            };
            UpdateFilenameCheckMarks();
            _playSound.PlayNotificationSound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method FilenameFontSize_ClickAsync");
        }
    }

    private async void MachineNameFontSize_ClickAsync(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem item) return;

            var size = item.Name switch
            {
                "MachineNameFontSizeSmall" => "Small",
                "MachineNameFontSizeBig" => "Big",
                _ => "Normal"
            };

            _settings.MachineNameFontSize = size;
            await _settings.SaveAsync();
            UpdateFilenameCheckMarks();
            _playSound.PlayNotificationSound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method MachineNameFontSize_ClickAsync");
        }
    }

    // ── Phase 4.1 windows ──

    private void EditLinks_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var window = App.ServiceProvider.GetRequiredService<SetLinksWindow>();
            window.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in method EditLinks_Click");
        }
    }

    private void SetGamepadDeadZone_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var window = App.ServiceProvider.GetRequiredService<SetGamepadDeadZoneWindow>();
            window.ShowDialog(this).ContinueWith(_ =>
            {
                // Apply the new dead zone values to the running controller
                _gamePadController.DeadZoneX = _settings.DeadZoneX;
                _gamePadController.DeadZoneY = _settings.DeadZoneY;
                if (_settings.EnableGamePadNavigation)
                {
                    _ = _gamePadController.StopAsync();
                    _ = _gamePadController.StartAsync();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in method SetGamepadDeadZone_Click");
        }
    }

    private void SetFuzzyMatchingThreshold_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var window = App.ServiceProvider.GetRequiredService<SetFuzzyMatchingWindow>();
            window.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in method SetFuzzyMatchingThreshold_Click");
        }
    }

    private void SoundConfiguration_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var window = App.ServiceProvider.GetRequiredService<SoundConfigurationWindow>();
            window.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in method SoundConfiguration_Click");
        }
    }

    private void DownloadImagePack_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var window = App.ServiceProvider.GetRequiredService<DownloadImagePackWindow>();
            window.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in method DownloadImagePack_Click");
        }
    }

    private void GlobalStats_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var window = App.ServiceProvider.GetRequiredService<GlobalStatsWindow>();
            window.Initialize(_systemManagerService.LoadSystems());
            window.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in method GlobalStats_Click");
        }
    }

    private void About_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var window = App.ServiceProvider.GetRequiredService<AboutWindow>();
            window.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in method About_Click");
        }
    }

    private void Support_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var window = App.ServiceProvider.GetRequiredService<SupportWindow>();
            window.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in method Support_Click");
        }
    }

    // ── Gamepad / fuzzy / overlay toggles ──

    private async void ToggleGamepad_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem item) return;

            _settings.EnableGamePadNavigation = item.IsChecked;
            await _settings.SaveAsync();

            if (item.IsChecked)
                await _gamePadController.StartAsync();
            else
                await _gamePadController.StopAsync();

            _playSound.PlayNotificationSound();
            ShowToast("Gamepad Support", item.IsChecked ? "Enabled" : "Disabled");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method ToggleGamepad_Click");
        }
    }

    private async void ToggleFuzzyMatchingClickAsync(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem item) return;

            _settings.EnableFuzzyMatching = item.IsChecked;
            await _settings.SaveAsync();
            _viewModel.ReloadGames();
            _playSound.PlayNotificationSound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method ToggleFuzzyMatchingClickAsync");
        }
    }

    private async void ToggleAnnotationStrippingClickAsync(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem item) return;

            _settings.EnableAnnotationStripping = item.IsChecked;
            await _settings.SaveAsync();
            _viewModel.ReloadGames();
            _playSound.PlayNotificationSound();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method ToggleAnnotationStrippingClickAsync");
        }
    }

    private async void ToggleOverlayButton_ClickAsync(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (sender is not MenuItem item) return;

            var isChecked = item.IsChecked;
            if (ReferenceEquals(item, RetroAchievementButton))
            {
                _settings.OverlayRetroAchievementButton = isChecked;
            }
            else if (ReferenceEquals(item, VideoLinkButton))
            {
                _settings.OverlayOpenVideoButton = isChecked;
            }
            else
            {
                _settings.OverlayOpenInfoButton = isChecked;
            }

            await _settings.SaveAsync();
            _playSound.PlayNotificationSound();
            var header = item.Header?.ToString() ?? "Overlay button";
            ShowToast("Overlay Button", $"{header} {(isChecked ? "enabled" : "disabled")}");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method ToggleOverlayButton_ClickAsync");
        }
    }

    // ── Edit System ──

    private void EasyMode_Click(object? sender, RoutedEventArgs e)
    {
        AddSystem_Click(sender, e);
    }

    private async void ExpertMode_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var selectedSystem = _viewModel.SelectedSystem;
            var systemToPreselect = string.IsNullOrWhiteSpace(selectedSystem) ? null : selectedSystem;
            var factory = App.ServiceProvider.GetRequiredService<Func<string?, EditSystemWindow>>();
            var editWindow = factory(systemToPreselect);
            await editWindow.ShowDialog(this);

            // The Expert window can add, rename, or delete systems — keep the sidebar in sync.
            await _systemSelectionOrchestrator.ReloadAfterConfigurationChangeAsync();
            RefreshSidebarCounts();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method ExpertMode_Click");
        }
    }

    private async void ScanForMicrosoftWindowsGames_ClickAsync(object? sender, RoutedEventArgs e)
    {
        try
        {
            _viewModel.IsLoading = true;
            _viewModel.StatusText = "Scanning for Windows games...";
            var result = await _gameScannerService.ScanForStoreGamesAsync();

            if (result.GamesFound > 0)
            {
                // Reload systems + games so the new/updated system shows up immediately
                // (sidebars, counts, and the file watcher pick up the new folders too).
                await _viewModel.InitializeAsync();
                await _systemSelectionOrchestrator.ReloadAfterConfigurationChangeAsync();
            }

            var action = result.SystemWasCreated ? "Created" : "Updated";
            _viewModel.StatusText = $"Found {result.GamesFound} PC games. {action} the Microsoft Windows system.";
            ShowToast("Scan Complete", result.GamesFound == 0
                ? "No PC games were found on this system."
                : $"Found {result.GamesFound} PC games. {action} the Microsoft Windows system with {result.ShortcutsCreated} new game shortcut(s).");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method ScanForMicrosoftWindowsGames_ClickAsync");
            _viewModel.StatusText = "Error scanning for Windows games";
        }
        finally
        {
            _viewModel.IsLoading = false;
        }
    }

    // ── System Selection Screen (WPF DisplaySystemSelectionScreenAsync parity) ──

    /// <summary>
    /// Shows the system selection grid and hides all other content panels.
    /// Mirrors WPF's DisplaySystemSelectionScreenAsync which populates the
    /// GameFileGrid with clickable system icon buttons.
    /// </summary>
    private async Task ShowSystemSelectionScreenAsync()
    {
        // WPF parity: the top system-selection bar is hidden while the full-screen
        // system selection grid is shown (SystemSelectionOrchestratorService line-parity).
        TopSystemSelection.IsVisible = false;
        FavoritesSectionRoot.IsVisible = false;
        PlayHistorySectionRoot.IsVisible = false;
        GlobalSearchSectionRoot.IsVisible = false;
        GameBrowserPanel.IsVisible = false;
        SystemSelectionRoot.IsVisible = true;

        await PopulateSystemSelectionGridAsync();
    }

    /// <summary>
    /// Populates the system selection grid with clickable system cards
    /// (icon + name), mirroring WPF PopulateSystemSelectionGridAsync.
    /// </summary>
    private async Task PopulateSystemSelectionGridAsync()
    {
        SystemSelectionWrapPanel.Children.Clear();
        NoSystemsConfiguredMessage.IsVisible = false;

        var systems = _systemManagerService.LoadSystems()
            .OrderBy(static s => s.SystemName, StringComparer.Ordinal)
            .ToList();

        if (systems.Count == 0)
        {
            NoSystemsConfiguredMessage.IsVisible = true;
            return;
        }

        // Rebuild sidebar data so icon paths are resolved
        _viewModel.PopulateSidebar(systems);
        _viewModel.Sidebar.RefreshCounts(_viewModel.SystemGameCounts);

        var systemImageSize = _settings.ThumbnailSizeForSystem;

        foreach (var sidebarItem in _viewModel.Sidebar.Systems)
        {
            var systemName = sidebarItem.SystemName;
            var iconPath = sidebarItem.IconPath;

            var buttonContentPanel = new StackPanel
            {
                Orientation = Orientation.Vertical,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            // System icon image — or fallback glyph if no icon resolved
            if (!string.IsNullOrEmpty(iconPath) && File.Exists(iconPath))
            {
                try
                {
                    using var stream = File.OpenRead(iconPath);
                    var bitmap = Bitmap.DecodeToWidth(stream, (int)(systemImageSize * 1.3 * 1.6));
                    var image = new Image
                    {
                        Source = bitmap,
                        Height = systemImageSize * 1.3,
                        Width = systemImageSize * 1.3 * 1.6,
                        Stretch = Stretch.Uniform,
                        Margin = new Thickness(5)
                    };
                    buttonContentPanel.Children.Add(image);
                }
                catch
                {
                    buttonContentPanel.Children.Add(new TextBlock
                    {
                        Text = "🎮",
                        FontSize = 36,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        Margin = new Thickness(0, 10, 0, 0)
                    });
                }
            }
            else
            {
                buttonContentPanel.Children.Add(new TextBlock
                {
                    Text = "🎮",
                    FontSize = 36,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Margin = new Thickness(0, 10, 0, 0)
                });
            }

            buttonContentPanel.Children.Add(new TextBlock
            {
                Text = systemName,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                FontWeight = FontWeight.Bold,
                TextTrimming = TextTrimming.CharacterEllipsis,
                FontSize = 12,
                MaxWidth = systemImageSize * 1.3 * 1.6,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 0)
            });
            ToolTip.SetTip(buttonContentPanel.Children[^1], systemName);

            var systemButton = new Button
            {
                Content = buttonContentPanel,
                Tag = systemName,
                Width = systemImageSize * 1.3 * 1.6 + 20,
                Height = systemImageSize * 1.3 + 40 + 20,
                Margin = new Thickness(5),
                Padding = new Thickness(5)
            };
            systemButton.Classes.Add("game-button-3d"); // WPF SystemButton3DTemplate parity
            systemButton.Click += SystemCard_Click;

            // Right-click context menu (WPF parity: Select / Edit / Delete)
            var contextMenu = new ContextMenu();
            var selectItem = new MenuItem { Header = "Select System" };
            selectItem.Click += (_, _) => SystemComboBox.SelectedItem = systemName;
            var editItem = new MenuItem { Header = "Edit System" };
            editItem.Click += (_, _) => EditSystemFromGrid(systemName);
            var deleteItem = new MenuItem { Header = "Delete System" };
            deleteItem.Click += (_, _) => _ = DeleteSystemFromGrid(systemName);
            contextMenu.Items.Add(selectItem);
            contextMenu.Items.Add(editItem);
            contextMenu.Items.Add(deleteItem);
            systemButton.ContextMenu = contextMenu;

            SystemSelectionWrapPanel.Children.Add(systemButton);
        }

        await Task.CompletedTask;
    }

    /// <summary>
    /// System card click — sets the System ComboBox to the clicked system,
    /// which fires the orchestrator pipeline that loads games and returns
    /// to the game browser.
    /// </summary>
    private void SystemCard_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not Button btn) return;
        var systemName = btn.Tag as string;
        if (string.IsNullOrEmpty(systemName)) return;

        _playSound.PlayNotificationSound();
        SystemComboBox.SelectedItem = systemName;
    }

    /// <summary>
    /// Opens Edit System for the specified system, then refreshes the grid.
    /// </summary>
    private async void EditSystemFromGrid(string systemName)
    {
        try
        {
            _playSound.PlayNotificationSound();
            var factory = App.ServiceProvider.GetRequiredService<Func<string?, EditSystemWindow>>();
            var editWindow = factory(systemName);
            await editWindow.ShowDialog(this);
            await _systemSelectionOrchestrator.ReloadAfterConfigurationChangeAsync();
            RefreshSidebarCounts();
            await ShowSystemSelectionScreenAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error editing system from grid");
        }
    }

    /// <summary>
    /// Deletes a system after confirmation, then refreshes the grid.
    /// </summary>
    private async Task DeleteSystemFromGrid(string systemName)
    {
        try
        {
            var messageBox = App.ServiceProvider.GetRequiredService<IMessageBoxLibraryService>();
            var result = await messageBox.AreYouSureDoYouWantToDeleteThisSystemMessageBoxAsync();
            if (result != MessageBoxResult.Yes) return;

            _playSound.PlayNotificationSound();

            var writer = App.ServiceProvider.GetRequiredService<ISystemConfigurationWriterService>();
            await writer.DeleteSystemAsync(systemName);

            await _systemSelectionOrchestrator.ReloadAfterConfigurationChangeAsync();
            RefreshSidebarCounts();
            await ShowSystemSelectionScreenAsync();

            await messageBox.SystemHasBeenDeletedMessageBoxAsync(systemName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error deleting system from grid");
        }
    }

    // ── RetroAchievements ──

    private void RetroAchievementsSettings_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var settingsWindow = App.ServiceProvider.GetRequiredService<RetroAchievementsSettingsWindow>();
            settingsWindow.ShowDialog(this);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method RetroAchievementsSettings_Click");
        }
    }

    private async void CalculateHashesForAllGamePaths_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var sp = App.ServiceProvider;
            var hasher = sp.GetRequiredService<IRetroAchievementsHasherTool>();
            var logger = sp.GetRequiredService<ILogger>();

            var games = _viewModel.GetAllGamesForHashing();
            if (games.Count == 0)
            {
                ShowToast("RetroAchievements", "No games found to hash.");
                return;
            }

            var loading = new ToastLoadingState((title, message) => ShowToast(title, message));
            var successCount = 0;

            _viewModel.IsLoading = true;
            try
            {
                foreach (var game in games)
                {
                    var system = _systemManagerService.GetSystem(game.SystemName);
                    var formats = system?.FileFormatsToLaunch ?? new List<string>();

                    try
                    {
                        var result = await hasher.GetGameHashForRetroAchievementsAsync(
                            game.FilePath, game.SystemName, formats, loading, logger);
                        if (!string.IsNullOrEmpty(result.Hash))
                        {
                            successCount++;
                        }
                    }
                    catch (Exception ex)
                    {
                        logger.Debug(ex, "Failed to hash {Path} for RetroAchievements", game.FilePath);
                    }
                }

                ShowToast("RetroAchievements", $"Hashed {successCount} of {games.Count} games.");
            }
            finally
            {
                _viewModel.IsLoading = false;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method CalculateHashesForAllGamePaths_Click");
        }
    }

    // ── Inject Emulator Config (21 emulators) ──

    private void ShowEmulatorConfig_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { Tag: string emulatorName }) return;

        try
        {
            switch (emulatorName)
            {
                case "Ares":
                    OpenInjectWindow<InjectAresConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "Azahar":
                    OpenInjectWindow<InjectAzaharConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "Blastem":
                    OpenInjectWindow<InjectBlastemConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "Cemu":
                    OpenInjectWindow<InjectCemuConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "Daphne":
                    OpenInjectWindow<InjectDaphneConfigWindow>(w => w.Initialize(false));
                    break;
                case "Dolphin":
                    OpenInjectWindow<InjectDolphinConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "DuckStation":
                    OpenInjectWindow<InjectDuckStationConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "Flycast":
                    OpenInjectWindow<InjectFlycastConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "Mame":
                    OpenInjectWindow<InjectMameConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "Mednafen":
                    OpenInjectWindow<InjectMednafenConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "Mesen":
                    OpenInjectWindow<InjectMesenConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "PCSX2":
                    OpenInjectWindow<InjectPcsx2ConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "Raine":
                    OpenInjectWindow<InjectRaineConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "Redream":
                    OpenInjectWindow<InjectRedreamConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "RetroArch":
                    OpenInjectWindow<InjectRetroArchConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "RPCS3":
                    OpenInjectWindow<InjectRpcs3ConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "SegaModel2":
                    OpenInjectWindow<InjectSegaModel2ConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "Stella":
                    OpenInjectWindow<InjectStellaConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "Supermodel":
                    OpenInjectWindow<InjectSupermodelConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "Xenia":
                    OpenInjectWindow<InjectXeniaConfigWindow>(w => w.Initialize(null, false));
                    break;
                case "Yumir":
                    OpenInjectWindow<InjectYumirConfigWindow>(w => w.Initialize(null, false));
                    break;
                default:
                    Log.Warning("Unknown emulator for config injection: {Emulator}", emulatorName);
                    break;
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to open {Emulator} config window", emulatorName);
        }
    }

    // ── Tools (external executables) ──

    private async void LaunchTool_Click(object? sender, RoutedEventArgs e)
    {
        var tool = "";
        try
        {
            if (sender is not MenuItem { Tag: string toolTag }) return;

            tool = toolTag;

            try
            {
                _viewModel.StatusText = $"Launching tool: {tool}...";
                _playSound.PlayNotificationSound();

                var romFolder = GetSelectedRomFolder();
                var imageFolder = GetSelectedImageFolder();

                switch (tool)
                {
                    case "BatchConvertIsoToXiso":
                        await _externalToolLauncher.BatchConvertIsoToXisoAsync();
                        break;
                    case "BatchConvertToCHD":
                        await _externalToolLauncher.BatchConvertToChdAsync(romFolder);
                        break;
                    case "BatchConvertToCompressedFile":
                        await _externalToolLauncher.BatchConvertToCompressedFileAsync();
                        break;
                    case "BatchConvertToRVZ":
                        await _externalToolLauncher.BatchConvertToRvzAsync();
                        break;
                    case "CreateBatchFilesForPS3Games":
                        await _externalToolLauncher.CreateBatchFilesForPs3GamesAsync();
                        break;
                    case "CreateBatchFilesForScummVMGames":
                        await _externalToolLauncher.CreateBatchFilesForScummVmGamesAsync();
                        break;
                    case "CreateBatchFilesForWindowsGames":
                        await _externalToolLauncher.CreateBatchFilesForWindowsGamesAsync();
                        break;
                    case "CreateBatchFilesForXbox360XBLAGames":
                        await _externalToolLauncher.CreateBatchFilesForXbox360XblaGamesAsync();
                        break;
                    case "FindRomCover":
                        await _externalToolLauncher.FindRomCoverLaunchAsync(imageFolder, romFolder);
                        break;
                    case "RetroGameCoverDownloader":
                        await _externalToolLauncher.RetroGameCoverDownloaderAsync(imageFolder, romFolder);
                        break;
                    case "RomValidator":
                        await _externalToolLauncher.RomValidatorAsync();
                        break;
                    default:
                        Log.Warning("Unknown tool menu item: {Tool}", tool);
                        break;
                }

                _viewModel.StatusText = "Ready";
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error launching tool {Tool}", tool);
                _viewModel.StatusText = "Error launching tool";
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unexpected error in LaunchTool_Click for tool {Tool}", tool);
        }
    }

    /// <summary>
    /// Resolves the first ROM folder of the currently selected system (null when none).
    /// </summary>
    private string? GetSelectedRomFolder()
    {
        try
        {
            var system = _systemManagerService.GetSystem(_viewModel.SelectedSystem);
            var folder = system?.SystemFolders.FirstOrDefault();
            return folder is null ? null : Core.Services.CheckPaths.PathHelper.ResolveRelativeToAppDirectory(folder);
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to resolve selected ROM folder");
            return null;
        }
    }

    /// <summary>
    /// Resolves the image folder of the currently selected system (null when none).
    /// </summary>
    private string? GetSelectedImageFolder()
    {
        try
        {
            var system = _systemManagerService.GetSystem(_viewModel.SelectedSystem);
            var folder = system?.SystemImageFolder;
            return string.IsNullOrEmpty(folder) ? null : Core.Services.CheckPaths.PathHelper.ResolveRelativeToAppDirectory(folder);
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to resolve selected image folder");
            return null;
        }
    }

    // ── Donate / AppData / Exit ──

    private void Donate_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var configuration = App.ServiceProvider.GetRequiredService<IConfiguration>();
            var url = configuration.GetValue<string>("Urls:DonationPage") ?? "https://www.purelogiccode.com/Donate/";
            Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Unable to open the donation link from the menu.");
        }
    }

    private void OpenAppDataPath_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var appDataPath = Core.Services.AppDataPaths.SimpleLauncherDataFolder;
            if (string.IsNullOrEmpty(appDataPath) || !Directory.Exists(appDataPath))
            {
                Log.Debug("AppData path does not exist: {Path}", appDataPath);
                return;
            }

            Process.Start(new ProcessStartInfo { FileName = appDataPath, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in the method OpenAppDataPath_Click");
        }
    }

    private void Exit_Click(object? sender, RoutedEventArgs e)
    {
        _playSound.PlayNotificationSound();
        Close();
    }

    #endregion

    #region Toast

    public void ShowToast(string title, string message, ToastType type = ToastType.Info)
    {
        var color = GetBrush(type switch
        {
            ToastType.Success => "NotificationSuccessBrush",
            ToastType.Warning => "NotificationWarningBrush",
            ToastType.Error => "NotificationErrorBrush",
            _ => "NotificationInfoBrush"
        }) ?? Brushes.Blue;

        var toast = new Border
        {
            Background = GetBrush("BgTertiaryBrush"),
            BorderBrush = color,
            BorderThickness = new Thickness(3, 0, 0, 0),
            CornerRadius = new CornerRadius(6),
            Padding = new Thickness(12, 8, 12, 8),
            Margin = new Thickness(0, 0, 0, 8),
            Child = new StackPanel
            {
                Children =
                {
                    new TextBlock
                    {
                        Text = title,
                        Foreground = GetBrush("TextPrimaryBrush"),
                        FontWeight = FontWeight.SemiBold,
                        FontSize = 12
                    },
                    new TextBlock
                    {
                        Text = message,
                        Foreground = GetBrush("TextSecondaryBrush"),
                        FontSize = 11,
                        Margin = new Thickness(0, 2, 0, 0),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };

        ToastStack.IsVisible = true;
        ToastStack.Children.Add(toast);

        _ = DismissToastAsync(toast);
    }

    private IBrush? GetBrush(string key)
    {
        return ResourceNodeExtensions.TryFindResource(this, key, out var resource) ? resource as IBrush : null;
    }

    private async Task DismissToastAsync(Border toast)
    {
        await Task.Delay(5000);
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ToastStack.Children.Remove(toast);
            if (ToastStack.Children.Count == 0)
            {
                ToastStack.IsVisible = false;
            }
        });
    }

    #endregion
}

public enum ToastType
{
    Info,
    Success,
    Warning,
    Error
}
