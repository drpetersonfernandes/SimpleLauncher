using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SimpleLauncher.Core.Models;
using Settings = SimpleLauncher.Core.Services.SettingsManager.SettingsManagerService;

namespace SimpleLauncher.Services.GameListUI;

using Interfaces;

/// <summary>
/// Manages the game list UI, including grid/list view switching, pagination, and game button image cleanup.
/// </summary>
public class GameListUiService
{
    private readonly Settings _settings;
    private IGameListUiHost _host = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="GameListUiService"/> class.
    /// </summary>
    /// <param name="settings">The application settings manager.</param>
    public GameListUiService(Settings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Initializes the service with the specified host that provides UI elements and dispatcher access.
    /// </summary>
    /// <param name="host">The game list UI host.</param>
    public void Initialize(IGameListUiHost host)
    {
        _host = host;
    }

    /// <summary>
    /// Prepares the UI before loading game files by clearing existing content, scrolling to top, and setting the appropriate view mode.
    /// </summary>
    public async Task SetUiBeforeLoadGameFilesAsync()
    {
        _host.Scroller.Dispatcher.Invoke(() => _host.Scroller.ScrollToTop());
        _host.PreviewImage.Dispatcher.Invoke(() => _host.PreviewImage.Source = null);

        _host.GameFileGrid.Dispatcher.Invoke(() =>
        {
            ClearGameButtonImages(_host.GameFileGrid);
            _host.GameFileGrid.Children.Clear();
        });

        await _host.Dispatcher.InvokeAsync(() => _host.GameListItems.Clear());

        await _host.Dispatcher.InvokeAsync(() =>
        {
            if (string.Equals(_settings.ViewMode, "GridView", StringComparison.Ordinal))
            {
                _host.SetGameFileGridVisible(true);
                _host.SetListViewPreviewAreaVisible(false);
            }
            else
            {
                _host.SetGameFileGridVisible(false);
                _host.SetListViewPreviewAreaVisible(true);
            }
        });

        await _host.Dispatcher.InvokeAsync(() =>
        {
            _host.SetPaginationButtonsVisible(true);
        });
    }

    /// <summary>
    /// Displays a message indicating that no games matched the current search or filter.
    /// </summary>
    public void AddNoFilesMessage()
    {
        var noGamesMatched = Application.Current.Dispatcher.CheckAccess()
            ? (string)Application.Current.TryFindResource("nogamesmatched") ?? "Unfortunately, no games matched your search query or the selected button."
            : Application.Current.Dispatcher.Invoke(static () => (string)Application.Current.TryFindResource("nogamesmatched") ?? "Unfortunately, no games matched your search query or the selected button.");

        if (string.Equals(_settings.ViewMode, "GridView", StringComparison.Ordinal))
        {
            ClearGameButtonImages(_host.GameFileGrid);
            _host.GameFileGrid.Children.Clear();
            _host.GameFileGrid.Children.Add(new TextBlock
            {
                Text = $"\n{noGamesMatched}",
                Padding = new Thickness(10)
            });
        }
        else
        {
            _host.GameListItems.Clear();
            _host.GameListItems.Add(new GameListViewItem
            {
                FileName = noGamesMatched,
                MachineDescription = ""
            });
        }
    }

    /// <summary>
    /// Enables or disables all game buttons in the grid.
    /// </summary>
    /// <param name="isEnabled">True to enable buttons; false to disable them.</param>
    public void SetGameButtonsEnabled(bool isEnabled)
    {
        if (_host.GameFileGrid == null) return;

        foreach (var child in _host.GameFileGrid.Children)
        {
            if (child is Button button)
            {
                button.IsEnabled = isEnabled;
            }
        }
    }

    /// <summary>
    /// Recursively clears all BitmapImage sources from Image elements within the specified panel and its children.
    /// </summary>
    /// <param name="panel">The panel whose game button images should be cleared.</param>
    public static void ClearGameButtonImages(Panel panel)
    {
        foreach (var child in panel.Children)
        {
            switch (child)
            {
                case Image image:
                    if (image.Source is BitmapImage)
                    {
                        image.Source = null;
                    }

                    break;

                case Button button:
                    switch (button.Content)
                    {
                        case Panel buttonPanel:
                            ClearGameButtonImages(buttonPanel);
                            break;
                        case Border border:
                            ClearImageFromBorder(border);
                            break;
                    }

                    break;

                case Panel childPanel:
                    ClearGameButtonImages(childPanel);
                    break;

                case Border border:
                    ClearImageFromBorder(border);
                    break;
            }
        }
    }

    /// <summary>
    /// Clears the BitmapImage source of an Image element contained within a Border, if present.
    /// </summary>
    /// <param name="border">The border whose child image source should be cleared.</param>
    public static void ClearImageFromBorder(Border border)
    {
        switch (border.Child)
        {
            case Image image:
                if (image.Source is BitmapImage)
                {
                    image.Source = null;
                }

                break;
            case Panel panel:
                ClearGameButtonImages(panel);
                break;
        }
    }
}
