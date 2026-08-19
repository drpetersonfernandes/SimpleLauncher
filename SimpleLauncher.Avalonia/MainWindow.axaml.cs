using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Avalonia.Converters;
using SimpleLauncher.Avalonia.InjectConfigWindows;
using SimpleLauncher.Avalonia.Services;
using SimpleLauncher.Avalonia.Services.GameScan;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.ExternalToolLauncher;
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

    /// <summary>Favorites page section ViewModel (WPF FavoritesPage equivalent).</summary>
    public FavoritesSectionViewModel FavoritesSection { get; }

    /// <summary>Play history page section ViewModel (WPF PlayHistoryPage equivalent).</summary>
    public PlayHistorySectionViewModel PlayHistorySection { get; }

    /// <summary>Global search page section ViewModel (WPF GlobalSearchPage equivalent).</summary>
    public GlobalSearchSectionViewModel GlobalSearchSection { get; }

    // Bounds persistence (separate file from the WPF app)
    private static readonly string BoundsFilePath = Path.Combine(
        Core.Services.AppDataPaths.SimpleLauncherDataFolder, "window_bounds_avalonia.json");

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
        AvaloniaMenuCheckMarkService menuCheckMarks)
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
        FavoritesSection = favoritesSection;
        PlayHistorySection = playHistorySection;
        GlobalSearchSection = globalSearchSection;
        DataContext = _viewModel;

        // Surface ViewModel toast requests (e.g. RetroAchievements hash scan status)
        _viewModel.ToastRequested += (title, message) => ShowToast(title, message);

        // Initialize converter with ratio service
        ConsoleToCardHeightConverter.SetRatioService(ratioService);
        BooleanToFavoriteStatusConverter.SetLocalizationService(localization);

        InitializeComponent();

        // Bind the page-section ViewModels (WPF FavoritesPage / PlayHistoryPage / GlobalSearchPage equivalents)
        FavoritesSectionRoot.DataContext = FavoritesSection;
        PlayHistorySectionRoot.DataContext = PlayHistorySection;
        GlobalSearchSectionRoot.DataContext = GlobalSearchSection;

        // Wire up manufacturer group collapse/expand
        WireGroupHeader(ArcadeGroupHeader, ArcadeGroupPanel);
        WireGroupHeader(NintendoGroupHeader, NintendoGroupPanel);
        WireGroupHeader(SegaGroupHeader, SegaGroupPanel);
        WireGroupHeader(SonyGroupHeader, SonyGroupPanel);
        WireGroupHeader(NecGroupHeader, NecGroupPanel);
        WireGroupHeader(SnkGroupHeader, SnkGroupPanel);
        WireGroupHeader(OtherGroupHeader, OtherGroupPanel);
        WireGroupHeader(ConsolesHeader, ConsolesPanel);

        // Populate sidebar from system.xml
        PopulateSidebarFromSystemXml();

        // NOTE: no initial sidebar selection here — selecting "All Games" would fire
        // SystemList_SelectionChanged synchronously and trigger a full library scan on
        // the UI thread during construction. Window_Opened → InitializeAsync does the
        // single initial scan asynchronously instead.

        // Restore window position/size before the window is shown
        RestoreWindowBounds();

        // Failsafe shutdown watchdog: if normal shutdown has not terminated the process
        // within the grace period, force-exit so the app can never linger in the background.
        Closed += (_, _) =>
        {
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

        // Live library refresh: when a watched ROM folder changes on disk, reload the
        // current view on the UI thread (same debounced behavior as the WPF app).
        _fileWatcher.GameFilesChanged += (_, e) =>
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
            _viewModel.Sidebar.Populate(_systemManagerService.LoadSystems());
            _viewModel.Sidebar.RefreshCounts(_viewModel.SystemGameCounts);
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

    private static void WireGroupHeader(ToggleButton header, Panel panel)
    {
        header.IsCheckedChanged += (_, _) => { panel.IsVisible = header.IsChecked == true; };
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

    private void ViewToggle_Click(object? sender, RoutedEventArgs e)
    {
        // Set the view state explicitly per button (mirroring the single stateful
        // toggle of the WPF app) and keep both toggle buttons mutually exclusive.
        if (ReferenceEquals(sender, GridViewToggle))
        {
            _viewModel.IsGridView = true;
        }
        else if (ReferenceEquals(sender, ListViewToggle))
        {
            _viewModel.IsGridView = false;
        }

        GridViewToggle.IsChecked = _viewModel.IsGridView;
        ListViewToggle.IsChecked = !_viewModel.IsGridView;
    }

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
                _viewModel.NavigateToAllGamesCommand.Execute(null);
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

    #region Category Tabs

    private void CategoryTab_Click(object? sender, RoutedEventArgs e)
    {
        if (sender is not ToggleButton clicked) return;

        foreach (var tab in new[] { GamesTab, RecentTab, FavoritesTab })
        {
            if (tab != clicked)
            {
                tab.IsChecked = false;
            }
        }

        clicked.IsChecked = true;

        switch (clicked.Name)
        {
            case "GamesTab":
                _ = ShowSectionAsync(MainSection.None);
                _viewModel.NavigateToAllGamesCommand.Execute(null);
                break;
            case "RecentTab":
                _ = ShowSectionAsync(MainSection.PlayHistory);
                break;
            case "FavoritesTab":
                _ = ShowSectionAsync(MainSection.Favorites);
                break;
        }
    }

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
        SearchPlaceholder.IsVisible = string.IsNullOrEmpty(query);
        _viewModel.SearchText = query ?? "";
        _viewModel.StatusText = string.IsNullOrEmpty(query) ? "Ready" : $"Search: \"{query}\"";
    }

    #endregion

    #region Sidebar System Selection

    private void SystemList_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox { SelectedItem: not null } listBox) return;

        // System lists are bound to SidebarSystemItem; the collections list uses
        // ListBoxItem items with a Tag.
        switch (listBox.SelectedItem)
        {
            case SidebarSystemItem systemItem:
                _viewModel.NavigateToSystemCommand.Execute(systemItem.SystemName);
                break;
            case ListBoxItem { Tag: string tag }:
                switch (tag)
                {
                    case "all":
                        _ = ShowSectionAsync(MainSection.None);
                        _viewModel.NavigateToAllGamesCommand.Execute(null);
                        break;
                    case "recently_added":
                        _ = ShowSectionAsync(MainSection.None);
                        _viewModel.NavigateToRecentlyAddedCommand.Execute(null);
                        break;
                    case "recently_played":
                        _ = ShowSectionAsync(MainSection.PlayHistory);
                        break;
                    case "favorites":
                        _ = ShowSectionAsync(MainSection.Favorites);
                        break;
                    case "global_search":
                        _ = ShowSectionAsync(MainSection.GlobalSearch);
                        break;
                    default:
                        _ = ShowSectionAsync(MainSection.None);
                        _viewModel.NavigateToSystemCommand.Execute(tag);
                        break;
                }

                break;
        }

        listBox.SelectedIndex = -1;
    }

    #endregion

    #region Game Card Interaction

    private void GameCard_Click(object? sender, PointerPressedEventArgs e)
    {
        if (sender is Control { DataContext: GameCardViewModel game } card)
        {
            var properties = e.GetCurrentPoint(card).Properties;

            if (properties.IsRightButtonPressed)
            {
                ShowGameContextMenu(game, card);
                e.Handled = true; // prevent the popup from closing on the subsequent mouse-up
            }
            else if (e.ClickCount == 2)
            {
                _viewModel.PlayGameCommand.Execute(game);
            }
            else
            {
                // Single left click: select the game
                var listBoxItem = FindParent<ListBoxItem>(card);
                if (listBoxItem is not null)
                {
                    listBoxItem.IsSelected = true;
                }
            }
        }
    }

    private void ShowGameContextMenu(GameCardViewModel game, Control placementTarget)
    {
        var contextMenu = new ContextMenu
        {
            Placement = PlacementMode.Pointer
        };

        var playItem = new MenuItem { Header = "▶ Play" };
        playItem.Click += (_, _) => _viewModel.PlayGameCommand.Execute(game);
        contextMenu.Items.Add(playItem);

        var favItem = new MenuItem
        {
            Header = game.IsFavorite ? "♥ Remove from Favorites" : "♡ Add to Favorites"
        };
        favItem.Click += async (_, _) => await _viewModel.ToggleFavoriteCommand.ExecuteAsync(game);
        contextMenu.Items.Add(favItem);

        contextMenu.Items.Add(new Separator());

        var detailItem = new MenuItem { Header = "ℹ Show Details" };
        detailItem.Click += (_, _) => OpenGameDetail(game);
        contextMenu.Items.Add(detailItem);

        var raItem = new MenuItem { Header = "🏆 Achievements" };
        raItem.Click += async (_, _) => await OpenRetroAchievementsForGameAsync(game);
        contextMenu.Items.Add(raItem);

        var copyItem = new MenuItem { Header = "📋 Copy Path" };
        copyItem.Click += async (_, _) =>
        {
            await CopyToClipboardAsync(game.FilePath);
            ShowToast("Copied", game.FilePath);
        };
        contextMenu.Items.Add(copyItem);

        var copyNameItem = new MenuItem { Header = "📝 Copy Name" };
        copyNameItem.Click += async (_, _) =>
        {
            var fileName = Path.GetFileName(game.FilePath);
            await CopyToClipboardAsync(fileName);
            ShowToast("Copied", fileName);
        };
        contextMenu.Items.Add(copyNameItem);

        contextMenu.Items.Add(new Separator());

        var showInFolderItem = new MenuItem { Header = "📂 Show in Folder" };
        showInFolderItem.Click += async (_, _) => await ShowGameInFolderAsync(game);
        contextMenu.Items.Add(showInFolderItem);

        var editSystemItem = new MenuItem { Header = "✏ Edit System" };
        editSystemItem.Click += (_, _) => OpenEditSystemForGame(game);
        contextMenu.Items.Add(editSystemItem);

        contextMenu.Open(placementTarget);
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

    private async void FavoritesDataGrid_KeyDown(object? sender, KeyEventArgs e)
    {
        try
        {
            switch (e.Key)
            {
                case Key.Delete:
                    e.Handled = true;
                    await FavoritesSection.RemoveSelectedCommand.ExecuteAsync(null);
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

    #region Preferences

    private void Preferences_Click(object? sender, RoutedEventArgs e)
    {
        var prefsWindow = App.ServiceProvider.GetRequiredService<PreferencesWindow>();
        prefsWindow.ShowDialog(this);
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
                _systemManagerService.InvalidateCache();
                _viewModel.InvalidateAllGameFileCaches();
                _viewModel.NavigateToAllGamesCommand.Execute(null);
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

            _viewModel.IsGridView = item.Name == "GridView";
            GridViewToggle.IsChecked = _viewModel.IsGridView;
            ListViewToggle.IsChecked = !_viewModel.IsGridView;
            _settings.ViewMode = _viewModel.IsGridView ? "GridView" : "ListView";
            _ = _settings.SaveAsync();
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
            window.ShowDialog(this);
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

    private void ExpertMode_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            var factory = App.ServiceProvider.GetRequiredService<Func<string?, EditSystemWindow>>();
            var editWindow = factory(null);
            editWindow.ShowDialog(this);
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
                _systemManagerService.InvalidateCache();
                await _viewModel.InitializeAsync();
                PopulateSidebarFromSystemXml();
                _fileWatcher.StartWatchingForSystems(_systemManagerService.LoadSystems());
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
