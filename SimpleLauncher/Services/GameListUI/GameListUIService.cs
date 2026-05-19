using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SimpleLauncher.Models;
using Settings = SimpleLauncher.Services.SettingsManager.SettingsManager;

namespace SimpleLauncher.Services.GameListUI;

public class GameListUiService
{
    private readonly Settings _settings;
    private MainWindow _mainWindow;

    public GameListUiService(Settings settings)
    {
        _settings = settings;
    }

    public void Initialize(MainWindow mainWindow)
    {
        _mainWindow = mainWindow;
    }

    public async Task SetUiBeforeLoadGameFilesAsync()
    {
        _mainWindow.Scroller.Dispatcher.Invoke(() => _mainWindow.Scroller.ScrollToTop());
        _mainWindow.PreviewImage.Dispatcher.Invoke(() => _mainWindow.PreviewImage.Source = null);

        _mainWindow.GameFileGrid.Dispatcher.Invoke(() =>
        {
            ClearGameButtonImages(_mainWindow.GameFileGrid);
            _mainWindow.GameFileGrid.Children.Clear();
        });

        await _mainWindow.Dispatcher.InvokeAsync(() => _mainWindow.GameListItems.Clear());

        await _mainWindow.Dispatcher.InvokeAsync(() =>
        {
            if (_settings.ViewMode == "GridView")
            {
                _mainWindow.GameFileGrid.Visibility = Visibility.Visible;
                _mainWindow.ListViewPreviewArea.Visibility = Visibility.Collapsed;
            }
            else
            {
                _mainWindow.GameFileGrid.Visibility = Visibility.Collapsed;
                _mainWindow.ListViewPreviewArea.Visibility = Visibility.Visible;
            }
        });

        await _mainWindow.Dispatcher.InvokeAsync(() =>
        {
            _mainWindow.SetPaginationButtonsVisibility(Visibility.Visible);
        });
    }

    public void AddNoFilesMessage()
    {
        var noGamesMatched = (string)Application.Current.TryFindResource("nogamesmatched") ?? "Unfortunately, no games matched your search query or the selected button.";

        if (_settings.ViewMode == "GridView")
        {
            ClearGameButtonImages(_mainWindow.GameFileGrid);
            _mainWindow.GameFileGrid.Children.Clear();
            _mainWindow.GameFileGrid.Children.Add(new TextBlock
            {
                Text = $"\n{noGamesMatched}",
                Padding = new Thickness(10)
            });
        }
        else
        {
            _mainWindow.GameListItems.Clear();
            _mainWindow.GameListItems.Add(new GameListViewItem
            {
                FileName = noGamesMatched,
                MachineDescription = string.Empty
            });
        }
    }

    public void SetGameButtonsEnabled(bool isEnabled)
    {
        if (_mainWindow.GameFileGrid == null) return;

        foreach (var child in _mainWindow.GameFileGrid.Children)
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
