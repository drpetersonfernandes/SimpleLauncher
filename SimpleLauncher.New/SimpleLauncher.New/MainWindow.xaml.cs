using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.New.Converters;
using SimpleLauncher.New.ViewModels;

namespace SimpleLauncher.New;

/// <summary>
/// Main application window — OpenEmu-inspired shell with sidebar, toolbar, and game content area.
/// Phase 3: Virtualized game grid + list view, wired to MainViewModel.
/// </summary>
public partial class MainWindow
{
    private readonly MainViewModel _viewModel;
    private readonly Services.SystemManager.SystemManagerService _systemManagerService;

    // Bounds persistence
    private static readonly string BoundsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SimpleLauncher", "window_bounds_new.json");

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
    }

    /// <summary>
    /// Loads the game library after the window is shown so the UI thread is not
    /// blocked during window construction. Refreshes sidebar count badges when done.
    /// </summary>
    private async void Window_Loaded(object sender, RoutedEventArgs e)
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
        header.Checked += (_, _) => { panel.Visibility = Visibility.Visible; };
        header.Unchecked += (_, _) => { panel.Visibility = Visibility.Collapsed; };
    }

    #region Window Bounds Persistence

    private void Window_SourceInitialized(object? sender, EventArgs e)
    {
        RestoreWindowBounds();
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        Log.Debug("Main window closing");
        SaveBounds();
    }

    private CancellationTokenSource? _shutdownWatchdogCts;

    /// <summary>
    /// Failsafe shutdown watchdog. The window's Closed event always fires when the user
    /// closes it, even if WPF's Application.Windows bookkeeping was left in a broken state
    /// (e.g., after a startup template crash). If normal shutdown (ShutdownMode + OnExit)
    /// has not terminated the process within the grace period, force-exit so the app can
    /// never linger in the background after the user closes it.
    /// </summary>
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);

        // Cancel any previous watchdog so re-entrant OnClosed calls cannot stack timers.
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
    }

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
                data.Left = Left;
                data.Top = Top;
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
                    var bounds = new Rect(data.Left.Value, data.Top.Value, data.Width.Value, data.Height.Value);
                    var virtualScreen = new Rect(
                        SystemParameters.VirtualScreenLeft,
                        SystemParameters.VirtualScreenTop,
                        SystemParameters.VirtualScreenWidth,
                        SystemParameters.VirtualScreenHeight);

                    if (virtualScreen.IntersectsWith(bounds))
                    {
                        Left = data.Left.Value;
                        Top = data.Top.Value;
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

    private void ViewToggle_Click(object sender, RoutedEventArgs e)
    {
        _viewModel.ToggleViewCommand.Execute(null);
    }

    private void GameListView_ColumnClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not GridViewColumnHeader header || header.Column is null)
            return;

        var columnName = header.Column.Header as string ?? "";
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
        foreach (var col in ((GridView)GameListView.View).Columns)
        {
            if (col.Header is string s)
            {
                col.Header = s.Replace(" ▲", "").Replace(" ▼", "");
            }
        }

        header.Column.Header = columnName + (_sortAscending ? " ▲" : " ▼");
    }

    private void GameListView_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (GameListView.SelectedItem is GameCardViewModel game)
            _viewModel.PlayGameCommand.Execute(game);
    }

    #endregion

    #region Keyboard Shortcuts

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        switch (e.Key)
        {
            case Key.Escape:
                SearchBox.Text = "";
                _viewModel.SearchText = "";
                _viewModel.NavigateToAllGamesCommand.Execute(null);
                e.Handled = true;
                break;
            case Key.F when Keyboard.Modifiers == ModifierKeys.Control:
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

    #region Card Size Slider

    private void CardSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        Resources["LibraryCardWidth"] = e.NewValue;
        _viewModel.CardWidth = e.NewValue;
    }

    #endregion

    #region Category Tabs

    private void CategoryTab_Click(object sender, RoutedEventArgs e)
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

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var query = SearchBox.Text;
        SearchPlaceholder.Visibility = string.IsNullOrEmpty(query) ? Visibility.Visible : Visibility.Collapsed;
        _viewModel.SearchText = query;
        _viewModel.StatusText = string.IsNullOrEmpty(query) ? "Ready" : $"Search: \"{query}\"";
    }

    #endregion

    #region Sidebar System Selection

    private void SystemList_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

    private void GameCard_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: GameCardViewModel game } fe)
        {
            if (e.ChangedButton == MouseButton.Right)
            {
                ShowGameContextMenu(game, fe);
                e.Handled = true; // prevent the popup from closing on the subsequent mouse-up
            }
            else if (e.ClickCount == 2)
            {
                _viewModel.PlayGameCommand.Execute(game);
            }
            else
            {
                // Single left click: select the game
                var listBoxItem = FindParent<ListBoxItem>(fe);
                if (listBoxItem is not null)
                {
                    listBoxItem.IsSelected = true;
                }
            }
        }
    }

    private void ShowGameContextMenu(GameCardViewModel game, FrameworkElement placementTarget)
    {
        var contextMenu = new ContextMenu
        {
            // Anchor to the card at the mouse point so the menu stays open
            // (a bare ContextMenu without a PlacementTarget is dismissed by the
            //  mouse-up that follows the right-click)
            PlacementTarget = placementTarget,
            Placement = PlacementMode.MousePoint
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
        copyItem.Click += (_, _) =>
        {
            Clipboard.SetText(game.FilePath);
            ShowToast("Copied", game.FilePath);
        };
        contextMenu.Items.Add(copyItem);

        // Detach the menu from the element when it closes so a later open rebuilds it cleanly
        contextMenu.Closed += (_, _) =>
        {
            if (placementTarget.ContextMenu == contextMenu)
            {
                placementTarget.ContextMenu = null;
            }
        };

        contextMenu.IsOpen = true;
    }

    /// <summary>
    /// Opens the GameDetailWindow for the given game.
    /// </summary>
    private void OpenGameDetail(GameCardViewModel game)
    {
        // GameDetailWindow takes per-game constructor arguments (game + main VM),
        // so it is created manually — DI cannot resolve it.
        var window = new GameDetailWindow(game, _viewModel)
        {
            Owner = this
        };
        window.ShowDialog();
    }

    private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        var parent = VisualTreeHelper.GetParent(child);
        while (parent is not null && parent is not T)
        {
            parent = VisualTreeHelper.GetParent(parent);
        }

        return parent as T;
    }

    #endregion

    #region Preferences

    private void Preferences_Click(object sender, RoutedEventArgs e)
    {
        var prefsWindow = App.ServiceProvider.GetRequiredService<PreferencesWindow>();
        prefsWindow.Owner = this;
        prefsWindow.ShowDialog();
    }

    #endregion

    #region EasyMode

    private void AddSystem_Click(object sender, RoutedEventArgs e)
    {
        var easyModeWindow = App.ServiceProvider.GetRequiredService<EasyModeWindow>();
        easyModeWindow.Owner = this;
        easyModeWindow.ShowDialog();

        // If a system was added, refresh the UI
        if (easyModeWindow.DataContext is EasyModeViewModel { SystemAdded: true })
        {
            _systemManagerService.InvalidateCache();
            _viewModel.NavigateToAllGamesCommand.Execute(null);
        }
    }

    #endregion

    #region Banner / Toast

    private void BannerClose_Click(object sender, RoutedEventArgs e)
    {
        ImportBanner.Visibility = Visibility.Collapsed;
    }

    #endregion

    #region Drag-Drop Import

    private void Content_DragEnter(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
            DragDropOverlay.Visibility = Visibility.Visible;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void Content_DragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
        {
            e.Effects = DragDropEffects.Copy;
        }
        else
        {
            e.Effects = DragDropEffects.None;
        }

        e.Handled = true;
    }

    private void Content_DragLeave(object sender, DragEventArgs e)
    {
        DragDropOverlay.Visibility = Visibility.Collapsed;
        e.Handled = true;
    }

    private void Content_Drop(object sender, DragEventArgs e)
    {
        DragDropOverlay.Visibility = Visibility.Collapsed;

        if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

        if (e.Data.GetData(DataFormats.FileDrop) is not string[] paths || paths.Length == 0)
            return;

        var folderPath = paths[0];

        // Check if it's a directory
        if (!Directory.Exists(folderPath))
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
                        await Dispatcher.InvokeAsync(() =>
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

            await Dispatcher.InvokeAsync(() =>
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
            await Dispatcher.InvokeAsync(() =>
            {
                HideLoading();
                HideImportBanner();
                ShowToast("Import Error", ex.Message, ToastType.Error);
            });
        }
    }

    public void ShowToast(string title, string message, ToastType type = ToastType.Info)
    {
        var color = type switch
        {
            ToastType.Success => (Brush)FindResource("NotificationSuccessBrush"),
            ToastType.Warning => (Brush)FindResource("NotificationWarningBrush"),
            ToastType.Error => (Brush)FindResource("NotificationErrorBrush"),
            _ => (Brush)FindResource("NotificationInfoBrush")
        };

        var toast = new Border
        {
            Background = (Brush)FindResource("BgTertiaryBrush"),
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
                        Foreground = (Brush)FindResource("TextPrimaryBrush"),
                        FontWeight = FontWeights.SemiBold,
                        FontSize = 12
                    },
                    new TextBlock
                    {
                        Text = message,
                        Foreground = (Brush)FindResource("TextSecondaryBrush"),
                        FontSize = 11,
                        Margin = new Thickness(0, 2, 0, 0),
                        TextWrapping = TextWrapping.Wrap
                    }
                }
            }
        };

        ToastStack.Visibility = Visibility.Visible;
        ToastStack.Children.Add(toast);

        _ = DismissToastAsync(toast);
    }

    private async Task DismissToastAsync(Border toast)
    {
        await Task.Delay(5000);
        await Dispatcher.InvokeAsync(() =>
        {
            ToastStack.Children.Remove(toast);
            if (ToastStack.Children.Count == 0)
            {
                ToastStack.Visibility = Visibility.Collapsed;
            }
        });
    }

    public void ShowImportBanner(string text, bool isIndeterminate = true)
    {
        BannerText.Text = text;
        BannerProgress.IsIndeterminate = isIndeterminate;
        ImportBanner.Visibility = Visibility.Visible;
    }

    public void HideImportBanner()
    {
        ImportBanner.Visibility = Visibility.Collapsed;
    }

    #endregion

    #region Loading Overlay

    public void ShowLoading(string message = "Loading…")
    {
        LoadingMessage.Text = message;
        LoadingOverlay.Visibility = Visibility.Visible;
        _viewModel.IsLoading = true;
    }

    public void HideLoading()
    {
        LoadingOverlay.Visibility = Visibility.Collapsed;
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
