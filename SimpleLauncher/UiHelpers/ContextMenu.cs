using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SimpleLauncher.Models;
using SimpleLauncher.Services;

namespace SimpleLauncher.UiHelpers;

public static class ContextMenu
{
    public static System.Windows.Controls.ContextMenu AddRightClickReturnContextMenu(RightClickContext context)
    {
        return CreateMenu(context);
    }

    public static Button AddRightClickReturnButton(RightClickContext context)
    {
        context.Button.ContextMenu = CreateMenu(context);
        return context.Button;
    }

    private static System.Windows.Controls.ContextMenu CreateMenu(RightClickContext context)
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
            PlaySoundEffects.PlayNotificationSound();

            string selectedEmulatorName;

            if (context.EmulatorComboBox is { SelectedItem: not null })
            {
                selectedEmulatorName = context.EmulatorComboBox.SelectedItem.ToString();
            }
            else if (context.Emulator != null) // This branch is taken if EmulatorComboBox is null (e.g., from GlobalSearch)
            {
                selectedEmulatorName = context.Emulator.EmulatorName;
            }
            else
            {
                selectedEmulatorName = null; // <-- selectedEmulatorName could be null here
            }

            if (await CheckParametersForNullOrEmpty(selectedEmulatorName)) return; // Will check Parameters for Null or Empty values. If true, will return the call and will not launch the game.

            await GameLauncher.HandleButtonClick(context.FilePath, selectedEmulatorName, context.SelectedSystemName, context.SelectedSystemManager, context.Settings, context.MainWindow);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.AddToFavorites(context.SelectedSystemName, context.FileNameWithExtension, context.GameFileGrid, context.FavoritesManager, context.MainWindow);
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
            PlaySoundEffects.PlayTrashSound();
            ContextMenuFunctions.RemoveFromFavorites(context.SelectedSystemName, context.FileNameWithExtension, context.GameFileGrid, context.FavoritesManager, context.MainWindow);

            // *** FIX: Invoke the callback if it exists ***
            context.OnFavoriteRemoved?.Invoke();
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenVideoLink(context.SelectedSystemName, context.FileNameWithoutExtension, context.Machines, context.Settings);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenInfoLink(context.SelectedSystemName, context.FileNameWithoutExtension, context.Machines, context.Settings);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenRomHistoryWindow(context.SelectedSystemName, context.FileNameWithoutExtension, context.SelectedSystemManager, context.Machines);
        };

        // View Achievements Context Menu ***
        var viewAchievementsIcon = new Image
        {
            Source = new BitmapImage(new Uri("pack://application:,,,/images/trophy.png")),
            Width = 16, Height = 16
        };
        var viewAchievementsText = (string)Application.Current.TryFindResource("ViewAchievements") ?? "View Achievements";
        var viewAchievementsItem = new MenuItem
        {
            Header = viewAchievementsText,
            Icon = viewAchievementsIcon
        };
        viewAchievementsItem.Click += (s, e) =>
        {
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenAchievementsWindow(context.FilePath, context.FileNameWithoutExtension);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenCover(context.SelectedSystemName, context.FileNameWithoutExtension, context.SelectedSystemManager);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenTitleSnapshot(context.SelectedSystemName, context.FileNameWithoutExtension);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenGameplaySnapshot(context.SelectedSystemName, context.FileNameWithoutExtension);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenCart(context.SelectedSystemName, context.FileNameWithoutExtension);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.PlayVideo(context.SelectedSystemName, context.FileNameWithoutExtension);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenManual(context.SelectedSystemName, context.FileNameWithoutExtension);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenWalkthrough(context.SelectedSystemName, context.FileNameWithoutExtension);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenCabinet(context.SelectedSystemName, context.FileNameWithoutExtension);
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
            PlaySoundEffects.PlayNotificationSound();
            ContextMenuFunctions.OpenFlyer(context.SelectedSystemName, context.FileNameWithoutExtension);
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

        takeScreenshot.Click += async (_, _) =>
        {
            PlaySoundEffects.PlayNotificationSound();

            // Notify user
            MessageBoxLibrary.TakeScreenShotMessageBox();

            string selectedEmulatorName;

            if (context.EmulatorComboBox is { SelectedItem: not null })
            {
                selectedEmulatorName = context.EmulatorComboBox.SelectedItem.ToString();
            }
            else if (context.Emulator != null)
            {
                selectedEmulatorName = context.Emulator.EmulatorName;
            }
            else
            {
                selectedEmulatorName = null;
            }

            _ = ContextMenuFunctions.TakeScreenshotOfSelectedWindow(context.FileNameWithoutExtension, context.SelectedSystemManager, null, context.MainWindow);

            await GameLauncher.HandleButtonClick(context.FilePath, selectedEmulatorName, context.SelectedSystemName, context.SelectedSystemManager, context.Settings, context.MainWindow);
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

            var result = MessageBoxLibrary.AreYouSureYouWantToDeleteTheGameMessageBox(context.FileNameWithExtension);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    ContextMenuFunctions.RemoveFromFavorites(context.SelectedSystemName, context.FileNameWithExtension, context.GameFileGrid, context.FavoritesManager, context.MainWindow);
                    await Task.Delay(500);
                    await ContextMenuFunctions.DeleteGame(context.FilePath, context.FileNameWithExtension, context.MainWindow);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "Error deleting the game.";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);

                    // Notify user
                    MessageBoxLibrary.ThereWasAnErrorDeletingTheGameMessageBox();
                }
            }
            else
            {
                return;
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

            var result = MessageBoxLibrary.AreYouSureYouWantToDeleteTheCoverImageMessageBox(context.FileNameWithoutExtension);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    await ContextMenuFunctions.DeleteCoverImage(
                        context.FileNameWithoutExtension,
                        context.SelectedSystemName,
                        context.SelectedSystemManager,
                        context.MainWindow);
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
            else
            {
                return;
            }
        };

        contextMenu.Items.Add(launchMenuItem);
        contextMenu.Items.Add(addToFavorites);
        contextMenu.Items.Add(removeFromFavorites);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(viewAchievementsItem);
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(openVideoLink);
        contextMenu.Items.Add(openInfoLink);
        contextMenu.Items.Add(openHistoryWindow);
        contextMenu.Items.Add(new Separator());
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
        contextMenu.Items.Add(new Separator());
        contextMenu.Items.Add(takeScreenshot);
        contextMenu.Items.Add(deleteGame);
        contextMenu.Items.Add(deleteCoverImage);

        return contextMenu;

        async Task<bool> CheckParametersForNullOrEmpty(string selectedEmulatorName)
        {
            if (string.IsNullOrEmpty(context.FilePath))
            {
                // Notify developer
                await LogErrors.LogErrorAsync(null, "Right click context menu was invoked, but the FilePath is null or empty.");

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(GetLogPath.Path());

                return true;
            }

            if (string.IsNullOrEmpty(selectedEmulatorName))
            {
                // Notify developer
                await LogErrors.LogErrorAsync(null, "Right click context menu was invoked, but the SelectedEmulatorName is null or empty.");

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(GetLogPath.Path());

                return true;
            }

            if (string.IsNullOrEmpty(context.SelectedSystemName))
            {
                // Notify developer
                await LogErrors.LogErrorAsync(null, "Right click context menu was invoked, but the SelectedSystemName is null or empty.");

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(GetLogPath.Path());

                return true;
            }

            if (context.SelectedSystemManager == null)
            {
                // Notify developer
                await LogErrors.LogErrorAsync(null, "Right click context menu was invoked, but the SelectedSystemManager is null.");

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(GetLogPath.Path());

                return true;
            }

            return false;
        }
    }
}
