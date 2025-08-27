using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SimpleLauncher.Models;
using SimpleLauncher.Services;
using SimpleLauncher.UiHelpers;
using ContextMenu = System.Windows.Controls.ContextMenu;

namespace SimpleLauncher;

public partial class GlobalSearchWindow
{
    private void AddRightClickContextMenuGlobalSearchWindow(RightClickContext context)
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
            PlaySoundEffects.PlayNotificationSound();
            LaunchGameFromSearchResult(context.FilePath, context.SelectedSystemName, context.Emulator);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.AddToFavorites(context.SelectedSystemName, context.FileNameWithExtension, null, _favoritesManager, _mainWindow);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenVideoLink(context.SelectedSystemName, context.FileNameWithoutExtension, _machines, _settings);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenInfoLink(context.SelectedSystemName, context.FileNameWithoutExtension, _machines, _settings);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenRomHistoryWindow(context.SelectedSystemName, context.FileNameWithoutExtension, context.SelectedSystemManager, _machines);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenCover(context.SelectedSystemName, context.FileNameWithoutExtension, context.SelectedSystemManager);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenTitleSnapshot(context.SelectedSystemName, context.FileNameWithoutExtension);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenGameplaySnapshot(context.SelectedSystemName, context.FileNameWithoutExtension);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenCart(context.SelectedSystemName, context.FileNameWithoutExtension);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.PlayVideo(context.SelectedSystemName, context.FileNameWithoutExtension);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenManual(context.SelectedSystemName, context.FileNameWithoutExtension);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenWalkthrough(context.SelectedSystemName, context.FileNameWithoutExtension);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenCabinet(context.SelectedSystemName, context.FileNameWithoutExtension);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenFlyer(context.SelectedSystemName, context.FileNameWithoutExtension);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenPcb(context.SelectedSystemName, context.FileNameWithoutExtension);
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
            PlaySoundEffects.PlayNotificationSound();

            // Notify user
            MessageBoxLibrary.TakeScreenShotMessageBox();

            _ = ContextMenuFunctions.TakeScreenshotOfSelectedWindow(context.FileNameWithoutExtension, context.SelectedSystemManager, null, _mainWindow);

            LaunchGameFromSearchResult(context.FilePath, context.SelectedSystemName, context.Emulator);
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
            PlaySoundEffects.PlayNotificationSound();

            // Notify user
            await DoYouWanToDeleteMessageBox();
            return;

            async Task DoYouWanToDeleteMessageBox()
            {
                var result = MessageBoxLibrary.AreYouSureYouWantToDeleteTheGameMessageBox(context.FileNameWithExtension);

                if (result != MessageBoxResult.Yes) return;

                try
                {
                    await ContextMenuFunctions.DeleteGame(context.FilePath, context.FileNameWithExtension, _mainWindow);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "Error deleting the file.";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);

                    // Notify user
                    MessageBoxLibrary.ThereWasAnErrorDeletingTheGameMessageBox();
                }

                ContextMenuFunctions.RemoveFromFavorites(context.SelectedSystemName, context.FileNameWithExtension, null, _favoritesManager, _mainWindow);
            }
        };

        // Delete Cover Image Context Menu
        var deleteCoverImageIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/delete.png")),
            Width = 16,
            Height = 16
        };
        var deleteCoverImage2 = (string)Application.Current.TryFindResource("DeleteCoverImage") ?? "Delete Cover Image";
        var deleteCoverImage = new MenuItem
        {
            Header = deleteCoverImage2,
            Icon = deleteCoverImageIcon
        };
        deleteCoverImage.Click += async (_, _) =>
        {
            PlaySoundEffects.PlayNotificationSound();

            // Notify user
            await DoYouWanToDeleteMessageBox();
            return;

            async Task DoYouWanToDeleteMessageBox()
            {
                var result = MessageBoxLibrary.AreYouSureYouWantToDeleteTheCoverImageMessageBox(context.FileNameWithoutExtension);

                if (result != MessageBoxResult.Yes) return;

                try
                {
                    await ContextMenuFunctions.DeleteCoverImage(
                        context.FileNameWithoutExtension,
                        context.SelectedSystemName,
                        context.SelectedSystemManager,
                        _mainWindow);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    var contextMessage = $"Error deleting the cover image of {context.FileNameWithoutExtension}.";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);

                    // Notify user
                    MessageBoxLibrary.ThereWasAnErrorDeletingTheCoverImageMessageBox();
                }
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
        contextMenu.Items.Add(deleteCoverImage);
        contextMenu.IsOpen = true;
    }
}