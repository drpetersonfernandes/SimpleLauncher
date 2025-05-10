using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SimpleLauncher.Managers;
using SimpleLauncher.Models;
using SimpleLauncher.Services;
using SimpleLauncher.UiHelpers;
using ContextMenu = System.Windows.Controls.ContextMenu;

namespace SimpleLauncher;

public partial class GlobalSearchWindow
{
    private void AddRightClickContextMenuGlobalSearchWindow(SearchResult selectedResult,
        string fileNameWithoutExtension,
        SystemManager systemManager, string fileNameWithExtension, string filePath)
    {
        var contextMenu = new ContextMenu();

        // "Launch Selected Game" MenuItem
        var launchIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/launch.png")),
            Width = 16,
            Height = 16
        };
        var launchSelectedGame2 =
            (string)Application.Current.TryFindResource("LaunchSelectedGame") ?? "Launch Selected Game";
        var launchMenuItem = new MenuItem
        {
            Header = launchSelectedGame2,
            Icon = launchIcon
        };
        launchMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            LaunchGameFromSearchResult(selectedResult.FilePath, selectedResult.SystemName,
                selectedResult.EmulatorConfig);
        };

        // "Add To Favorites" MenuItem
        var addToFavoritesIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/heart.png")),
            Width = 16,
            Height = 16
        };
        var addToFavorites2 = (string)Application.Current.TryFindResource("AddToFavorites") ?? "Add To Favorites";
        var addToFavoritesMenuItem = new MenuItem
        {
            Header = addToFavorites2,
            Icon = addToFavoritesIcon
        };
        addToFavoritesMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.AddToFavorites(selectedResult.SystemName, selectedResult.FileNameWithExtension,
                _favoritesManager, _fakeGameFileGrid, _mainWindow);
        };

        // "Open Video Link" MenuItem
        var videoLinkIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png")),
            Width = 16,
            Height = 16
        };
        var openVideoLink2 = (string)Application.Current.TryFindResource("OpenVideoLink") ?? "Open Video Link";
        var videoLinkMenuItem = new MenuItem
        {
            Header = openVideoLink2,
            Icon = videoLinkIcon
        };
        videoLinkMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenVideoLink(selectedResult.SystemName, fileNameWithoutExtension, _machines,
                _settings);
        };

        // "Open Info Link" MenuItem
        var infoLinkIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/info.png")),
            Width = 16,
            Height = 16
        };
        var openInfoLink2 = (string)Application.Current.TryFindResource("OpenInfoLink") ?? "Open Info Link";
        var infoLinkMenuItem = new MenuItem
        {
            Header = openInfoLink2,
            Icon = infoLinkIcon
        };
        infoLinkMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenInfoLink(selectedResult.SystemName, fileNameWithoutExtension, _machines,
                _settings);
        };

        // "Open ROM History" MenuItem
        var openHistoryIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/romhistory.png",
                UriKind.RelativeOrAbsolute)),
            Width = 16,
            Height = 16
        };
        var openRomHistory2 = (string)Application.Current.TryFindResource("OpenROMHistory") ?? "Open ROM History";
        var openHistoryMenuItem = new MenuItem
        {
            Header = openRomHistory2,
            Icon = openHistoryIcon
        };
        openHistoryMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenRomHistoryWindow(selectedResult.SystemName, fileNameWithoutExtension,
                systemManager, _machines);
        };

        // "Cover" MenuItem
        var coverIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cover.png")),
            Width = 16,
            Height = 16
        };
        var cover2 = (string)Application.Current.TryFindResource("Cover") ?? "Cover";
        var coverMenuItem = new MenuItem
        {
            Header = cover2,
            Icon = coverIcon
        };
        coverMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenCover(selectedResult.SystemName, fileNameWithoutExtension, systemManager);
        };

        // "Title Snapshot" MenuItem
        var titleSnapshotIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
            Width = 16,
            Height = 16
        };
        var titleSnapshot2 = (string)Application.Current.TryFindResource("TitleSnapshot") ?? "Title Snapshot";
        var titleSnapshotMenuItem = new MenuItem
        {
            Header = titleSnapshot2,
            Icon = titleSnapshotIcon
        };
        titleSnapshotMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenTitleSnapshot(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // "Gameplay Snapshot" MenuItem
        var gameplaySnapshotIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
            Width = 16,
            Height = 16
        };
        var gameplaySnapshot2 = (string)Application.Current.TryFindResource("GameplaySnapshot") ?? "Gameplay Snapshot";
        var gameplaySnapshotMenuItem = new MenuItem
        {
            Header = gameplaySnapshot2,
            Icon = gameplaySnapshotIcon
        };
        gameplaySnapshotMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenGameplaySnapshot(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // "Cart" MenuItem
        var cartIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cart.png")),
            Width = 16,
            Height = 16
        };
        var cart2 = (string)Application.Current.TryFindResource("Cart") ?? "Cart";
        var cartMenuItem = new MenuItem
        {
            Header = cart2,
            Icon = cartIcon
        };
        cartMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenCart(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // "Video" MenuItem
        var videoIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png")),
            Width = 16,
            Height = 16
        };
        var video2 = (string)Application.Current.TryFindResource("Video") ?? "Video";
        var videoMenuItem = new MenuItem
        {
            Header = video2,
            Icon = videoIcon
        };
        videoMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.PlayVideo(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // "Manual" MenuItem
        var manualIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/manual.png")),
            Width = 16,
            Height = 16
        };
        var manual2 = (string)Application.Current.TryFindResource("Manual") ?? "Manual";
        var manualMenuItem = new MenuItem
        {
            Header = manual2,
            Icon = manualIcon
        };
        manualMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenManual(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // "Walkthrough" MenuItem
        var walkthroughIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/walkthrough.png")),
            Width = 16,
            Height = 16
        };
        var walkthrough2 = (string)Application.Current.TryFindResource("Walkthrough") ?? "Walkthrough";
        var walkthroughMenuItem = new MenuItem
        {
            Header = walkthrough2,
            Icon = walkthroughIcon
        };
        walkthroughMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenWalkthrough(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // "Cabinet" MenuItem
        var cabinetIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cabinet.png")),
            Width = 16,
            Height = 16
        };
        var cabinet2 = (string)Application.Current.TryFindResource("Cabinet") ?? "Cabinet";
        var cabinetMenuItem = new MenuItem
        {
            Header = cabinet2,
            Icon = cabinetIcon
        };
        cabinetMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenCabinet(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // "Flyer" MenuItem
        var flyerIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/flyer.png")),
            Width = 16,
            Height = 16
        };
        var flyer2 = (string)Application.Current.TryFindResource("Flyer") ?? "Flyer";
        var flyerMenuItem = new MenuItem
        {
            Header = flyer2,
            Icon = flyerIcon
        };
        flyerMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenFlyer(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // "PCB" MenuItem
        var pcbIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/pcb.png")),
            Width = 16,
            Height = 16
        };
        var pCb2 = (string)Application.Current.TryFindResource("PCB") ?? "PCB";
        var pcbMenuItem = new MenuItem
        {
            Header = pCb2,
            Icon = pcbIcon
        };
        pcbMenuItem.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenPcb(selectedResult.SystemName, fileNameWithoutExtension);
        };

        // Take Screenshot Context Menu
        var takeScreenshotIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
            Width = 16,
            Height = 16
        };
        var takeScreenshot2 = (string)Application.Current.TryFindResource("TakeScreenshot") ?? "Take Screenshot";
        var takeScreenshot = new MenuItem
        {
            Header = takeScreenshot2,
            Icon = takeScreenshotIcon
        };
        takeScreenshot.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();

            // Notify user
            MessageBoxLibrary.TakeScreenShotMessageBox();

            _ = ContextMenuFunctions.TakeScreenshotOfSelectedWindow(fileNameWithoutExtension, systemManager,
                _fakeButton, _mainWindow);
            LaunchGameFromSearchResult(selectedResult.FilePath, selectedResult.SystemName,
                selectedResult.EmulatorConfig);
        };

        // Delete Game Context Menu
        var deleteGameIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/delete.png")),
            Width = 16,
            Height = 16
        };
        var deleteGame2 = (string)Application.Current.TryFindResource("DeleteGame") ?? "Delete Game";
        var deleteGame = new MenuItem
        {
            Header = deleteGame2,
            Icon = deleteGameIcon
        };
        deleteGame.Click += async (_, _) =>
        {
            PlayClick.PlayClickSound();

            // Notify user
            await DoYouWanToDeleteMessageBox();
            return;

            async Task DoYouWanToDeleteMessageBox()
            {
                var result = MessageBoxLibrary.AreYouSureYouWantToDeleteTheFileMessageBox(fileNameWithExtension);

                if (result != MessageBoxResult.Yes) return;

                try
                {
                    await ContextMenuFunctions.DeleteFile(filePath, fileNameWithExtension, _mainWindow);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "Error deleting the file.";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);

                    // Notify user
                    MessageBoxLibrary.ThereWasAnErrorDeletingTheFileMessageBox();
                }

                ContextMenuFunctions.RemoveFromFavorites(selectedResult.SystemName, fileNameWithExtension,
                    _favoritesManager, _fakeGameFileGrid, _mainWindow);
            }
        };

        contextMenu.Items.Add(launchMenuItem);
        contextMenu.Items.Add(addToFavoritesMenuItem);
        contextMenu.Items.Add(videoLinkMenuItem);
        contextMenu.Items.Add(infoLinkMenuItem);
        contextMenu.Items.Add(openHistoryMenuItem);
        contextMenu.Items.Add(coverMenuItem);
        contextMenu.Items.Add(titleSnapshotMenuItem);
        contextMenu.Items.Add(gameplaySnapshotMenuItem);
        contextMenu.Items.Add(cartMenuItem);
        contextMenu.Items.Add(videoMenuItem);
        contextMenu.Items.Add(manualMenuItem);
        contextMenu.Items.Add(walkthroughMenuItem);
        contextMenu.Items.Add(cabinetMenuItem);
        contextMenu.Items.Add(flyerMenuItem);
        contextMenu.Items.Add(pcbMenuItem);
        contextMenu.Items.Add(takeScreenshot);
        contextMenu.Items.Add(deleteGame);
        contextMenu.IsOpen = true;
    }
}