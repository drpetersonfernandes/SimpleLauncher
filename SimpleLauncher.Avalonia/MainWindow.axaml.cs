using System.Collections.ObjectModel;
using System.Text.Json;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Avalonia.Converters;
using SimpleLauncher.Avalonia.InjectConfigWindows;
using SimpleLauncher.Avalonia.ViewModels;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.PlaySound;
using SimpleLauncher.Core.Services.RetroAchievements;

namespace SimpleLauncher.Avalonia;

/// <summary>
/// Main application window — OpenEmu-inspired shell with sidebar, toolbar, and game content area.
/// Avalonia port of the WPF-UI MainWindow.
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly Services.SystemManager.SystemManagerService _systemManagerService;

    // Bounds persistence (separate file from the WPF app)
    private static readonly string BoundsFilePath = Path.Combine(
        Core.Services.AppDataPaths.SimpleLauncherDataFolder, "window_bounds_avalonia.json");

    public MainWindow(
        MainViewModel viewModel,
        Services.SystemArtRatioService ratioService,
        Services.SystemManager.SystemManagerService systemManagerService)
    {
        _viewModel = viewModel;
        _systemManagerService = systemManagerService;
        DataContext = _viewModel;

        // Initialize converter with ratio service
        ConsoleToCardHeightConverter.SetRatioService(ratioService);

        InitializeComponent();

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
    }

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
                _viewModel.NavigateToAllGamesCommand.Execute(null);
                break;
            case "RecentTab":
                _viewModel.NavigateToRecentlyPlayedCommand.Execute(null);
                break;
            case "FavoritesTab":
                _viewModel.NavigateToFavoritesCommand.Execute(null);
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
                        _viewModel.NavigateToAllGamesCommand.Execute(null);
                        break;
                    case "recently_added":
                        _viewModel.NavigateToRecentlyAddedCommand.Execute(null);
                        break;
                    case "recently_played":
                        _viewModel.NavigateToRecentlyPlayedCommand.Execute(null);
                        break;
                    case "favorites":
                        _viewModel.NavigateToFavoritesCommand.Execute(null);
                        break;
                    default:
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

        contextMenu.Open(placementTarget);
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
