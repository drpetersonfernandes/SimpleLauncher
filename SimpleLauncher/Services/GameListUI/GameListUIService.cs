using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SimpleLauncher.Models;
using Settings = SimpleLauncher.Services.SettingsManager.SettingsManager;

namespace SimpleLauncher.Services.GameListUI;

public class GameListUiService
{
    private readonly Settings _settings;
    private IGameListUiHost _host;

    public GameListUiService(Settings settings)
    {
        _settings = settings;
    }

    public void Initialize(IGameListUiHost host)
    {
        _host = host;
    }

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
            if (_settings.ViewMode == "GridView")
            {
                _host.SetGameFileGridVisibility(Visibility.Visible);
                _host.SetListViewPreviewAreaVisibility(Visibility.Collapsed);
            }
            else
            {
                _host.SetGameFileGridVisibility(Visibility.Collapsed);
                _host.SetListViewPreviewAreaVisibility(Visibility.Visible);
            }
        });

        await _host.Dispatcher.InvokeAsync(() =>
        {
            _host.SetPaginationButtonsVisibility(Visibility.Visible);
        });
    }

    public void AddNoFilesMessage()
    {
        var noGamesMatched = (string)Application.Current.TryFindResource("nogamesmatched") ?? "Unfortunately, no games matched your search query or the selected button.";

        if (_settings.ViewMode == "GridView")
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
                MachineDescription = string.Empty
            });
        }
    }

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
