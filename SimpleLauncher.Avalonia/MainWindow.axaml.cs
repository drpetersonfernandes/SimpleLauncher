using System.Collections.ObjectModel;
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
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Avalonia.Converters;
using SimpleLauncher.Avalonia.ViewModels;

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
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SimpleLauncher", "window_bounds_avalonia.json");

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

        // Set initial sidebar selection
        CollectionsList.SelectedIndex = 0;

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
        _viewModel.ToggleViewCommand.Execute(null);
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

    private void AddSystem_Click(object? sender, RoutedEventArgs e)
    {
        var easyModeWindow = App.ServiceProvider.GetRequiredService<EasyModeWindow>();
        easyModeWindow.ShowDialog(this);

        // If a system was added, refresh the UI
        if (easyModeWindow.DataContext is EasyModeViewModel { SystemAdded: true })
        {
            _systemManagerService.InvalidateCache();
            _viewModel.NavigateToAllGamesCommand.Execute(null);
        }
    }

    #endregion

    #region Banner / Toast

    private void BannerClose_Click(object? sender, RoutedEventArgs e)
    {
        ImportBanner.IsVisible = false;
    }

    #endregion

    #region Drag-Drop Import

    private void Content_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy;
            DragDropOverlay.IsVisible = true;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void Content_DragOver(object? sender, DragEventArgs e)
    {
        if (e.DataTransfer.Contains(DataFormat.File))
        {
            e.DragEffects = DragDropEffects.Copy;
        }
        else
        {
            e.DragEffects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void Content_DragLeave(object? sender, DragEventArgs e)
    {
        DragDropOverlay.IsVisible = false;
        e.Handled = true;
    }

    private void Content_Drop(object? sender, DragEventArgs e)
    {
        DragDropOverlay.IsVisible = false;

        var files = e.DataTransfer.TryGetFiles();
        if (files is null || files.Length == 0) return;

        var first = files[0];

        // Check if it's a directory
        if (first is not IStorageFolder)
        {
            ShowToast("Import", "Please drop a folder containing ROM files.", ToastType.Warning);
            return;
        }

        var folderPath = first.TryGetLocalPath() ?? first.Path?.LocalPath;
        if (string.IsNullOrEmpty(folderPath))
        {
            ShowToast("Import", "Please drop a folder containing ROM files.", ToastType.Warning);
            return;
        }

        ShowImportBanner($"Scanning: {Path.GetFileName(folderPath)}");

        // Start scanning in background
        _ = ScanFolderAsync(folderPath);
    }

    private async Task ScanFolderAsync(string folderPath)
    {
        try
        {
            ShowLoading($"Scanning {Path.GetFileName(folderPath)}...");
            BannerText.Text = $"Scanning: {Path.GetFileName(folderPath)}";

            // Scan ROM folders
            _viewModel.NavigateToAllGamesCommand.Execute(null);

            // Also check for storefront games
            await Task.Run(async () =>
            {
                try
                {
                    var scanner = App.ServiceProvider.GetRequiredService<Services.GameScan.StorefrontGameScanner>();
                    var storeGames = await scanner.ScanAllAsync();

                    if (storeGames.Count > 0)
                    {
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            BannerText.Text = $"Found {storeGames.Count} storefront games";
                            BannerProgress.IsIndeterminate = false;
                            BannerProgress.Maximum = storeGames.Count;
                            BannerProgress.Value = 0;

                            foreach (var (name, _, storefront) in storeGames)
                            {
                                BannerProgress.Value++;
                                BannerText.Text = $"Adding: {name} ({storefront})";
                                _viewModel.StatusText = $"Found: {name} ({storefront})";
                            }
                        });
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Storefront scan failed during folder import");
                }
            });

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                HideLoading();
                HideImportBanner();
                _viewModel.NavigateToAllGamesCommand.Execute(null);
                RefreshSidebarCounts();
                ShowToast("Scan Complete",
                    $"Folder scanned: {Path.GetFileName(folderPath)}",
                    ToastType.Success);
            });
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to scan imported folder {Folder}", folderPath);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                HideLoading();
                HideImportBanner();
                ShowToast("Import Error", ex.Message, ToastType.Error);
            });
        }
    }

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

    public void ShowImportBanner(string text, bool isIndeterminate = true)
    {
        BannerText.Text = text;
        BannerProgress.IsIndeterminate = isIndeterminate;
        ImportBanner.IsVisible = true;
    }

    public void HideImportBanner()
    {
        ImportBanner.IsVisible = false;
    }

    #endregion

    #region Loading Overlay

    public void ShowLoading(string message = "Loading…")
    {
        LoadingMessage.Text = message;
        LoadingOverlay.IsVisible = true;
        _viewModel.IsLoading = true;
    }

    public void HideLoading()
    {
        LoadingOverlay.IsVisible = false;
        _viewModel.IsLoading = false;
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
