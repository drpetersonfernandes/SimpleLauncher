using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SimpleLauncher.Managers;
using SimpleLauncher.Services;

namespace SimpleLauncher.UiHelpers;

public static class ContextMenu
{
    private static string _selectedEmulatorName;

    public static System.Windows.Controls.ContextMenu AddRightClickReturnContextMenu(
        string filePath,
        string fileNameWithExtension,
        string fileNameWithoutExtension,
        string selectedSystemName,
        ComboBox emulatorComboBox,
        FavoritesManager favoritesManager,
        SystemManager selectedSystemManager,
        List<MameManager> machines,
        SettingsManager settings,
        MainWindow mainWindow)
    {
        _selectedEmulatorName = emulatorComboBox.SelectedItem.ToString();

        var contextMenu = new System.Windows.Controls.ContextMenu();

        // Launch Game Context Menu
        var launchMenuItemIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/launch.png")),
            Width = 16,
            Height = 16
        };
        var launchMenuItem2 = (string)Application.Current.TryFindResource("LaunchGame") ?? "Launch Game";
        var launchMenuItem = new MenuItem
        {
            Header = launchMenuItem2,
            Icon = launchMenuItemIcon
        };
        launchMenuItem.Click += async (_, _) =>
        {
            PlayClick.PlayClickSound();
            await GameLauncher.HandleButtonClick(filePath, _selectedEmulatorName, selectedSystemName, selectedSystemManager, settings, mainWindow);
        };

        // Add To Favorites Context Menu
        var addToFavoritesIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/heart.png")),
            Width = 16,
            Height = 16
        };
        var addToFavorites2 = (string)Application.Current.TryFindResource("AddToFavorites") ?? "Add To Favorites";
        var addToFavorites = new MenuItem
        {
            Header = addToFavorites2,
            Icon = addToFavoritesIcon
        };
        addToFavorites.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.AddToFavorites(selectedSystemName, fileNameWithExtension, null, favoritesManager, mainWindow);
        };

        // Remove From Favorites Context Menu
        var removeFromFavoritesIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/brokenheart.png")),
            Width = 16,
            Height = 16
        };
        var removeFromFavorites2 = (string)Application.Current.TryFindResource("RemoveFromFavorites") ?? "Remove From Favorites";
        var removeFromFavorites = new MenuItem
        {
            Header = removeFromFavorites2,
            Icon = removeFromFavoritesIcon
        };
        removeFromFavorites.Click += (_, _) =>
        {
            PlayClick.PlayTrashSound();
            ContextMenuFunctions.RemoveFromFavorites(selectedSystemName, fileNameWithExtension, null, favoritesManager, mainWindow);
        };

        // Open Video Link Context Menu
        var openVideoLinkIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png")),
            Width = 16,
            Height = 16
        };
        var openVideoLink2 = (string)Application.Current.TryFindResource("OpenVideoLink") ?? "Open Video Link";
        var openVideoLink = new MenuItem
        {
            Header = openVideoLink2,
            Icon = openVideoLinkIcon
        };
        openVideoLink.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenVideoLink(selectedSystemName, fileNameWithoutExtension, machines, settings);
        };

        // Open Info Link Context Menu
        var openInfoLinkIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/info.png")),
            Width = 16,
            Height = 16
        };
        var openInfoLink2 = (string)Application.Current.TryFindResource("OpenInfoLink") ?? "Open Info Link";
        var openInfoLink = new MenuItem
        {
            Header = openInfoLink2,
            Icon = openInfoLinkIcon
        };
        openInfoLink.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenInfoLink(selectedSystemName, fileNameWithoutExtension, machines, settings);
        };

        // Open History Context Menu
        var openHistoryIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/romhistory.png")),
            Width = 16,
            Height = 16
        };
        var openHistoryWindow2 = (string)Application.Current.TryFindResource("OpenROMHistory") ?? "Open ROM History";
        var openHistoryWindow = new MenuItem
        {
            Header = openHistoryWindow2,
            Icon = openHistoryIcon
        };
        openHistoryWindow.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenRomHistoryWindow(selectedSystemName, fileNameWithoutExtension, selectedSystemManager, machines);
        };

        // Open Cover Context Menu
        var openCoverIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cover.png")),
            Width = 16,
            Height = 16
        };
        var openCover2 = (string)Application.Current.TryFindResource("Cover") ?? "Cover";
        var openCover = new MenuItem
        {
            Header = openCover2,
            Icon = openCoverIcon
        };
        openCover.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenCover(selectedSystemName, fileNameWithoutExtension, selectedSystemManager);
        };

        // Open Title Snapshot Context Menu
        var openTitleSnapshotIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
            Width = 16,
            Height = 16
        };
        var openTitleSnapshot2 = (string)Application.Current.TryFindResource("TitleSnapshot") ?? "Title Snapshot";
        var openTitleSnapshot = new MenuItem
        {
            Header = openTitleSnapshot2,
            Icon = openTitleSnapshotIcon
        };
        openTitleSnapshot.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenTitleSnapshot(selectedSystemName, fileNameWithoutExtension);
        };

        // Open Gameplay Snapshot Context Menu
        var openGameplaySnapshotIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
            Width = 16,
            Height = 16
        };
        var openGameplaySnapshot2 = (string)Application.Current.TryFindResource("GameplaySnapshot") ?? "Gameplay Snapshot";
        var openGameplaySnapshot = new MenuItem
        {
            Header = openGameplaySnapshot2,
            Icon = openGameplaySnapshotIcon
        };
        openGameplaySnapshot.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenGameplaySnapshot(selectedSystemName, fileNameWithoutExtension);
        };

        // Open Cart Context Menu
        var openCartIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cart.png")),
            Width = 16,
            Height = 16
        };
        var openCart2 = (string)Application.Current.TryFindResource("Cart") ?? "Cart";
        var openCart = new MenuItem
        {
            Header = openCart2,
            Icon = openCartIcon
        };
        openCart.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenCart(selectedSystemName, fileNameWithoutExtension);
        };

        // Open Video Context Menu
        var openVideoIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png")),
            Width = 16,
            Height = 16
        };
        var openVideo2 = (string)Application.Current.TryFindResource("Video") ?? "Video";
        var openVideo = new MenuItem
        {
            Header = openVideo2,
            Icon = openVideoIcon
        };
        openVideo.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.PlayVideo(selectedSystemName, fileNameWithoutExtension);
        };

        // Open Manual Context Menu
        var openManualIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/manual.png")),
            Width = 16,
            Height = 16
        };
        var openManual2 = (string)Application.Current.TryFindResource("Manual") ?? "Manual";
        var openManual = new MenuItem
        {
            Header = openManual2,
            Icon = openManualIcon
        };
        openManual.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenManual(selectedSystemName, fileNameWithoutExtension);
        };

        // Open Walkthrough Context Menu
        var openWalkthroughIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/walkthrough.png")),
            Width = 16,
            Height = 16
        };
        var openWalkthrough2 = (string)Application.Current.TryFindResource("Walkthrough") ?? "Walkthrough";
        var openWalkthrough = new MenuItem
        {
            Header = openWalkthrough2,
            Icon = openWalkthroughIcon
        };
        openWalkthrough.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenWalkthrough(selectedSystemName, fileNameWithoutExtension);
        };

        // Open Cabinet Context Menu
        var openCabinetIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cabinet.png")),
            Width = 16,
            Height = 16
        };
        var openCabinet2 = (string)Application.Current.TryFindResource("Cabinet") ?? "Cabinet";
        var openCabinet = new MenuItem
        {
            Header = openCabinet2,
            Icon = openCabinetIcon
        };
        openCabinet.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenCabinet(selectedSystemName, fileNameWithoutExtension);
        };

        // Open Flyer Context Menu
        var openFlyerIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/flyer.png")),
            Width = 16,
            Height = 16
        };
        var openFlyer2 = (string)Application.Current.TryFindResource("Flyer") ?? "Flyer";
        var openFlyer = new MenuItem
        {
            Header = openFlyer2,
            Icon = openFlyerIcon
        };
        openFlyer.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenFlyer(selectedSystemName, fileNameWithoutExtension);
        };

        // Open PCB Context Menu
        var openPcbIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/pcb.png")),
            Width = 16,
            Height = 16
        };
        var openPcb2 = (string)Application.Current.TryFindResource("PCB") ?? "PCB";
        var openPcb = new MenuItem
        {
            Header = openPcb2,
            Icon = openPcbIcon
        };
        openPcb.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenPcb(selectedSystemName, fileNameWithoutExtension);
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

        takeScreenshot.Click += async (_, _) =>
        {
            PlayClick.PlayClickSound();
            MessageBoxLibrary.TakeScreenShotMessageBox();

            _ = ContextMenuFunctions.TakeScreenshotOfSelectedWindow(fileNameWithoutExtension, selectedSystemManager, null, mainWindow);
            await GameLauncher.HandleButtonClick(filePath, _selectedEmulatorName, selectedSystemName, selectedSystemManager, settings, mainWindow);
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
            await DoYouWanToDeleteMessageBox();
        };

        contextMenu.Items.Add(launchMenuItem);
        contextMenu.Items.Add(addToFavorites);
        contextMenu.Items.Add(removeFromFavorites);
        contextMenu.Items.Add(openVideoLink);
        contextMenu.Items.Add(openInfoLink);
        contextMenu.Items.Add(openHistoryWindow);
        contextMenu.Items.Add(openCover);
        contextMenu.Items.Add(openTitleSnapshot);
        contextMenu.Items.Add(openGameplaySnapshot);
        contextMenu.Items.Add(openCart);
        contextMenu.Items.Add(openVideo);
        contextMenu.Items.Add(openManual);
        contextMenu.Items.Add(openWalkthrough);
        contextMenu.Items.Add(openCabinet);
        contextMenu.Items.Add(openFlyer);
        contextMenu.Items.Add(openPcb);
        contextMenu.Items.Add(takeScreenshot);
        contextMenu.Items.Add(deleteGame);

        // Return
        return contextMenu;

        async Task DoYouWanToDeleteMessageBox()
        {
            var result = MessageBoxLibrary.AreYouSureYouWantToDeleteTheFileMessageBox(fileNameWithExtension);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                await ContextMenuFunctions.DeleteFile(filePath, fileNameWithExtension, mainWindow);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error deleting the file.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ThereWasAnErrorDeletingTheFileMessageBox();
            }

            ContextMenuFunctions.RemoveFromFavorites(selectedSystemName, fileNameWithExtension, null, favoritesManager, mainWindow);
        }
    }

    public static Button AddRightClickReturnButton(
        string filePath,
        string fileNameWithExtension,
        string fileNameWithoutExtension,
        string selectedSystemName,
        FavoritesManager favoritesManager,
        WrapPanel gameFileGrid,
        SystemManager selectedSystemManager,
        Button button,
        List<MameManager> machines,
        SettingsManager settings,
        MainWindow mainWindow)
    {
        var contextMenu = new System.Windows.Controls.ContextMenu();

        // Launch Game Context Menu
        var launchMenuItemIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/launch.png")),
            Width = 16,
            Height = 16
        };
        var launchMenuItem2 = (string)Application.Current.TryFindResource("LaunchGame") ?? "Launch Game";
        var launchMenuItem = new MenuItem
        {
            Header = launchMenuItem2,
            Icon = launchMenuItemIcon
        };
        launchMenuItem.Click += async (_, _) =>
        {
            PlayClick.PlayClickSound();
            await GameLauncher.HandleButtonClick(filePath, _selectedEmulatorName, selectedSystemName, selectedSystemManager, settings, mainWindow);
        };

        // Add To Favorites Context Menu
        var addToFavoritesIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/heart.png")),
            Width = 16,
            Height = 16
        };
        var addToFavorites2 = (string)Application.Current.TryFindResource("AddToFavorites") ?? "Add To Favorites";
        var addToFavorites = new MenuItem
        {
            Header = addToFavorites2,
            Icon = addToFavoritesIcon
        };
        addToFavorites.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            // FIX: Pass the correct WrapPanel reference
            ContextMenuFunctions.AddToFavorites(selectedSystemName, fileNameWithExtension, gameFileGrid, favoritesManager, mainWindow);
        };

        // Remove From Favorites Context Menu
        var removeFromFavoritesIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/brokenheart.png")),
            Width = 16,
            Height = 16
        };
        var removeFromFavorites2 = (string)Application.Current.TryFindResource("RemoveFromFavorites") ?? "Remove From Favorites";
        var removeFromFavorites = new MenuItem
        {
            Header = removeFromFavorites2,
            Icon = removeFromFavoritesIcon
        };
        removeFromFavorites.Click += (_, _) =>
        {
            PlayClick.PlayTrashSound();
            // FIX: Pass the correct WrapPanel reference
            ContextMenuFunctions.RemoveFromFavorites(selectedSystemName, fileNameWithExtension, gameFileGrid, favoritesManager, mainWindow);
        };

        // Open Video Link Context Menu
        var openVideoLinkIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png")),
            Width = 16,
            Height = 16
        };
        var openVideoLink2 = (string)Application.Current.TryFindResource("OpenVideoLink") ?? "Open Video Link";
        var openVideoLink = new MenuItem
        {
            Header = openVideoLink2,
            Icon = openVideoLinkIcon
        };
        openVideoLink.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenVideoLink(selectedSystemName, fileNameWithoutExtension, machines, settings);
        };

        // Open Info Link Context Menu
        var openInfoLinkIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/info.png")),
            Width = 16,
            Height = 16
        };
        var openInfoLink2 = (string)Application.Current.TryFindResource("OpenInfoLink") ?? "Open Info Link";
        var openInfoLink = new MenuItem
        {
            Header = openInfoLink2,
            Icon = openInfoLinkIcon
        };
        openInfoLink.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenInfoLink(selectedSystemName, fileNameWithoutExtension, machines, settings);
        };

        // Open History Context Menu
        var openHistoryIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/romhistory.png")),
            Width = 16,
            Height = 16
        };
        var openHistoryWindow2 = (string)Application.Current.TryFindResource("OpenROMHistory") ?? "Open ROM History";
        var openHistoryWindow = new MenuItem
        {
            Header = openHistoryWindow2,
            Icon = openHistoryIcon
        };
        openHistoryWindow.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenRomHistoryWindow(selectedSystemName, fileNameWithoutExtension, selectedSystemManager, machines);
        };

        // Open Cover Context Menu
        var openCoverIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cover.png")),
            Width = 16,
            Height = 16
        };
        var openCover2 = (string)Application.Current.TryFindResource("Cover") ?? "Cover";
        var openCover = new MenuItem
        {
            Header = openCover2,
            Icon = openCoverIcon
        };
        openCover.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenCover(selectedSystemName, fileNameWithoutExtension, selectedSystemManager);
        };

        // Open Title Snapshot Context Menu
        var openTitleSnapshotIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
            Width = 16,
            Height = 16
        };
        var openTitleSnapshot2 = (string)Application.Current.TryFindResource("TitleSnapshot") ?? "Title Snapshot";
        var openTitleSnapshot = new MenuItem
        {
            Header = openTitleSnapshot2,
            Icon = openTitleSnapshotIcon
        };
        openTitleSnapshot.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenTitleSnapshot(selectedSystemName, fileNameWithoutExtension);
        };

        // Open Gameplay Snapshot Context Menu
        var openGameplaySnapshotIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/snapshot.png")),
            Width = 16,
            Height = 16
        };
        var openGameplaySnapshot2 = (string)Application.Current.TryFindResource("GameplaySnapshot") ?? "Gameplay Snapshot";
        var openGameplaySnapshot = new MenuItem
        {
            Header = openGameplaySnapshot2,
            Icon = openGameplaySnapshotIcon
        };
        openGameplaySnapshot.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenGameplaySnapshot(selectedSystemName, fileNameWithoutExtension);
        };

        // Open Cart Context Menu
        var openCartIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cart.png")),
            Width = 16,
            Height = 16
        };
        var openCart2 = (string)Application.Current.TryFindResource("Cart") ?? "Cart";
        var openCart = new MenuItem
        {
            Header = openCart2,
            Icon = openCartIcon
        };
        openCart.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenCart(selectedSystemName, fileNameWithoutExtension);
        };

        // Open Video Context Menu
        var openVideoIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/video.png")),
            Width = 16,
            Height = 16
        };
        var openVideo2 = (string)Application.Current.TryFindResource("Video") ?? "Video";
        var openVideo = new MenuItem
        {
            Header = openVideo2,
            Icon = openVideoIcon
        };
        openVideo.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.PlayVideo(selectedSystemName, fileNameWithoutExtension);
        };

        // Open Manual Context Menu
        var openManualIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/manual.png")),
            Width = 16,
            Height = 16
        };
        var openManual2 = (string)Application.Current.TryFindResource("Manual") ?? "Manual";
        var openManual = new MenuItem
        {
            Header = openManual2,
            Icon = openManualIcon
        };
        openManual.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenManual(selectedSystemName, fileNameWithoutExtension);
        };

        // Open Walkthrough Context Menu
        var openWalkthroughIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/walkthrough.png")),
            Width = 16,
            Height = 16
        };
        var openWalkthrough2 = (string)Application.Current.TryFindResource("Walkthrough") ?? "Walkthrough";
        var openWalkthrough = new MenuItem
        {
            Header = openWalkthrough2,
            Icon = openWalkthroughIcon
        };
        openWalkthrough.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenWalkthrough(selectedSystemName, fileNameWithoutExtension);
        };

        // Open Cabinet Context Menu
        var openCabinetIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/cabinet.png")),
            Width = 16,
            Height = 16
        };
        var openCabinet2 = (string)Application.Current.TryFindResource("Cabinet") ?? "Cabinet";
        var openCabinet = new MenuItem
        {
            Header = openCabinet2,
            Icon = openCabinetIcon
        };
        openCabinet.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenCabinet(selectedSystemName, fileNameWithoutExtension);
        };

        // Open Flyer Context Menu
        var openFlyerIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/flyer.png")),
            Width = 16,
            Height = 16
        };
        var openFlyer2 = (string)Application.Current.TryFindResource("Flyer") ?? "Flyer";
        var openFlyer = new MenuItem
        {
            Header = openFlyer2,
            Icon = openFlyerIcon
        };
        openFlyer.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenFlyer(selectedSystemName, fileNameWithoutExtension);
        };

        // Open PCB Context Menu
        var openPcbIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/pcb.png")),
            Width = 16,
            Height = 16
        };
        var openPcb2 = (string)Application.Current.TryFindResource("PCB") ?? "PCB";
        var openPcb = new MenuItem
        {
            Header = openPcb2,
            Icon = openPcbIcon
        };
        openPcb.Click += (_, _) =>
        {
            PlayClick.PlayClickSound();
            ContextMenuFunctions.OpenPcb(selectedSystemName, fileNameWithoutExtension);
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

        takeScreenshot.Click += async (_, _) =>
        {
            PlayClick.PlayClickSound();

            // Notify user
            MessageBoxLibrary.TakeScreenShotMessageBox();

            _ = ContextMenuFunctions.TakeScreenshotOfSelectedWindow(fileNameWithoutExtension, selectedSystemManager, button, mainWindow);
            await GameLauncher.HandleButtonClick(filePath, _selectedEmulatorName, selectedSystemName, selectedSystemManager, settings, mainWindow);
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

            await DoYouWantToDeleteMessageBox();
        };

        contextMenu.Items.Add(launchMenuItem);
        contextMenu.Items.Add(addToFavorites);
        contextMenu.Items.Add(removeFromFavorites);
        contextMenu.Items.Add(openVideoLink);
        contextMenu.Items.Add(openInfoLink);
        contextMenu.Items.Add(openHistoryWindow);
        contextMenu.Items.Add(openCover);
        contextMenu.Items.Add(openTitleSnapshot);
        contextMenu.Items.Add(openGameplaySnapshot);
        contextMenu.Items.Add(openCart);
        contextMenu.Items.Add(openVideo);
        contextMenu.Items.Add(openManual);
        contextMenu.Items.Add(openWalkthrough);
        contextMenu.Items.Add(openCabinet);
        contextMenu.Items.Add(openFlyer);
        contextMenu.Items.Add(openPcb);
        contextMenu.Items.Add(takeScreenshot);
        contextMenu.Items.Add(deleteGame);
        button.ContextMenu = contextMenu;
        return button;

        async Task DoYouWantToDeleteMessageBox()
        {
            var result = MessageBoxLibrary.AreYouSureYouWantToDeleteTheFileMessageBox(fileNameWithExtension);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await ContextMenuFunctions.DeleteFile(filePath, fileNameWithExtension, mainWindow);
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error deleting the file.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Notify user
                MessageBoxLibrary.ThereWasAnErrorDeletingTheFileMessageBox();
            }

            ContextMenuFunctions.RemoveFromFavorites(selectedSystemName, fileNameWithExtension, gameFileGrid, favoritesManager, mainWindow);
        }
    }
}