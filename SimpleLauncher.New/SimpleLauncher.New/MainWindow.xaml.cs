using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
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

    // Bounds persistence
    private static readonly string BoundsFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SimpleLauncher", "window_bounds_new.json");

    public MainWindow()
    {
        _viewModel = App.ServiceProvider.GetRequiredService<MainViewModel>();
        DataContext = _viewModel;

        // Initialize converter with ratio service
        var ratioService = App.ServiceProvider.GetRequiredService<Services.SystemArtRatioService>();
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
    /// Reads system.xml and populates the sidebar manufacturer groups.
    /// </summary>
    private void PopulateSidebarFromSystemXml()
    {
        try
        {
            var systemManager = App.ServiceProvider.GetRequiredService<Services.SystemManager.SystemManagerService>();
            var systems = systemManager.LoadSystems();

            // Manufacturer lookup
            var manufacturerMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["Atari 2600"] = "ATARI", ["Atari 5200"] = "ATARI", ["Atari 7800"] = "ATARI",
                ["Atari Jaguar"] = "ATARI", ["Atari Jaguar CD"] = "ATARI", ["Atari Lynx"] = "ATARI",
                ["Atari ST"] = "ATARI", ["Atari 8-Bit"] = "ATARI",
                ["NES"] = "NINTENDO", ["Nintendo NES"] = "NINTENDO", ["Famicom"] = "NINTENDO",
                ["SNES"] = "NINTENDO", ["Nintendo SNES"] = "NINTENDO", ["Super Famicom"] = "NINTENDO",
                ["Nintendo 64"] = "NINTENDO", ["Nintendo 64DD"] = "NINTENDO",
                ["Nintendo GameCube"] = "NINTENDO", ["Wii"] = "NINTENDO", ["Nintendo Wii"] = "NINTENDO",
                ["Wii U"] = "NINTENDO", ["Nintendo WiiU"] = "NINTENDO",
                ["Nintendo Switch"] = "NINTENDO",
                ["Game Boy"] = "NINTENDO", ["Nintendo Game Boy"] = "NINTENDO",
                ["Game Boy Color"] = "NINTENDO", ["Nintendo Game Boy Color"] = "NINTENDO",
                ["Game Boy Advance"] = "NINTENDO", ["Nintendo Game Boy Advance"] = "NINTENDO",
                ["Nintendo DS"] = "NINTENDO", ["Nintendo 3DS"] = "NINTENDO",
                ["Virtual Boy"] = "NINTENDO",
                ["Sega Genesis"] = "SEGA", ["Sega Mega Drive"] = "SEGA",
                ["Sega Master System"] = "SEGA", ["Sega Saturn"] = "SEGA",
                ["Sega Dreamcast"] = "SEGA", ["Sega Game Gear"] = "SEGA",
                ["Sega CD"] = "SEGA", ["Sega 32X"] = "SEGA", ["Sega Genesis CD"] = "SEGA",
                ["Sega Genesis 32X"] = "SEGA", ["Sega SG-1000"] = "SEGA",
                ["PS1"] = "SONY", ["Sony PlayStation 1"] = "SONY",
                ["PS2"] = "SONY", ["Sony PlayStation 2"] = "SONY",
                ["PS3"] = "SONY", ["Sony PlayStation 3"] = "SONY",
                ["PSP"] = "SONY", ["Sony PSP"] = "SONY",
                ["PS Vita"] = "SONY",
                ["PC Engine"] = "NEC", ["NEC PC Engine"] = "NEC",
                ["NEC PC Engine CD"] = "NEC", ["TurboGrafx-16"] = "NEC",
                ["NEC PC-FX"] = "NEC", ["NEC SuperGrafx"] = "NEC",
                ["Neo Geo"] = "SNK", ["Neo Geo CD"] = "SNK",
                ["SNK Neo Geo CD"] = "SNK", ["Neo Geo Pocket"] = "SNK",
                ["SNK Neo Geo Pocket"] = "SNK", ["Neo Geo Pocket Color"] = "SNK",
                ["SNK Neo Geo Pocket Color"] = "SNK",
                ["Arcade"] = "ARCADE", ["MAME"] = "ARCADE"
            };

            var groupLists = new Dictionary<string, ListBox>
            {
                ["ARCADE"] = ArcadeSystemsList,
                ["NINTENDO"] = NintendoSystemsList,
                ["SEGA"] = SegaSystemsList,
                ["SONY"] = SonySystemsList,
                ["NEC"] = NecSystemsList,
                ["SNK"] = SnkSystemsList
            };

            foreach (var system in systems)
            {
                var manufacturer = manufacturerMap.GetValueOrDefault(system.SystemName, "OTHER");
                var list = groupLists.GetValueOrDefault(manufacturer, OtherSystemsList);
                var count = _viewModel.SystemGameCounts.GetValueOrDefault(system.SystemName, 0);

                var sp = new StackPanel { Orientation = Orientation.Horizontal };
                // System icon
                var iconPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "systems", system.SystemName + ".png");
                if (File.Exists(iconPath))
                {
                    var img = new Image
                    {
                        Source = new BitmapImage(new Uri(iconPath)),
                        Width = 20, Height = 20,
                        Margin = new Thickness(0, 0, 6, 0),
                        VerticalAlignment = VerticalAlignment.Center
                    };
                    sp.Children.Add(img);
                }

                sp.Children.Add(new TextBlock
                {
                    Text = $"  🎮  {system.SystemName}",
                    Foreground = (Brush)FindResource("TextSecondaryBrush"),
                    FontSize = 13,
                    VerticalAlignment = VerticalAlignment.Center
                });
                sp.Children.Add(new TextBlock
                {
                    Text = count > 0 ? $"  {count}" : "",
                    Foreground = (Brush)FindResource("TextMutedBrush"),
                    FontSize = 11,
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(8, 0, 0, 0)
                });

                var item = new ListBoxItem { Tag = system.SystemName, Content = sp };
                list.Items.Add(item);
            }
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
        var counts = _viewModel.SystemGameCounts;
        var allLists = new[]
        {
            ArcadeSystemsList, NintendoSystemsList, SegaSystemsList,
            SonySystemsList, NecSystemsList, SnkSystemsList, OtherSystemsList
        };

        foreach (var list in allLists)
        {
            foreach (ListBoxItem item in list.Items)
            {
                var tag = item.Tag as string ?? "";
                var c = counts.GetValueOrDefault(tag, 0);
                if (item.Content is StackPanel { Children.Count: > 1 } sp && sp.Children[1] is TextBlock tb)
                {
                    tb.Text = c > 0 ? $"  {c}" : "";
                }
            }
        }
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

        _ = Task.Delay(TimeSpan.FromSeconds(5)).ContinueWith(_ =>
        {
            try
            {
                Log.Warning("Shutdown watchdog fired after window close; forcing process exit");
                Environment.Exit(0);
            }
            catch
            {
                // Last resort — nothing else we can do
            }
        });
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
            var hdr = col.Header as string ?? "";
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
        if (sender is not ListBox { SelectedItem: ListBoxItem item } listBox) return;

        var tag = item.Tag as string ?? "";

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
        var contextMenu = new ContextMenu();

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

        contextMenu.IsOpen = true;
    }

    /// <summary>
    /// Opens the GameDetailWindow for the given game.
    /// </summary>
    private void OpenGameDetail(GameCardViewModel game)
    {
        var detailWindow = App.ServiceProvider.GetRequiredService<GameDetailWindow>();
        // We need a constructor that takes the game and main VM
        // The DI-registered one won't work — create manually
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

                            foreach (var (name, exePath, storefront) in storeGames)
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
