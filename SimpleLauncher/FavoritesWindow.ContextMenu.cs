using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using SimpleLauncher.Models;
using SimpleLauncher.Services;
using SimpleLauncher.UiHelpers;
using ContextMenu = System.Windows.Controls.ContextMenu;

namespace SimpleLauncher;

public partial class FavoritesWindow
{
    private void AddRightClickContextMenuFavoritesWindow(RightClickContextFavorites context)
    {
        var menu = new ContextMenu();

        // Launch Game
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("LaunchSelectedGame") ?? "Launch Selected Game",
            Icon = CreateIcon("pack://application:,,,/images/launch.png"),
            Command = new RelayCommand(async void () =>
            {
                try
                {
                    PlaySoundEffects.PlayNotificationSound();
                    await LaunchGameFromFavorite(context.FileNameWithExtension, context.SelectedSystemName);
                }
                catch (Exception ex)
                {
                    _ = LogErrors.LogErrorAsync(ex, "Error launching game from favorites context.");
                }
            })
        });

        // Remove from Favorites
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("RemoveFromFavorites") ?? "Remove From Favorites",
            Icon = CreateIcon("pack://application:,,,/images/brokenheart.png"),
            Command = new RelayCommand(() =>
            {
                PlaySoundEffects.PlayTrashSound();
                RemoveFavoriteFromXmlAndEmptyPreviewImage(context.Favorite);
            })
        });

        // Open Video Link
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("OpenVideoLink") ?? "Open Video Link",
            Icon = CreateIcon("pack://application:,,,/images/video.png"),
            Command = new RelayCommand(() =>
            {
                PlaySoundEffects.PlayNotificationSound();
                ContextMenuFunctions.OpenVideoLink(context.SelectedSystemName, context.FileNameWithoutExtension,
                    context.Machines, context.Settings);
            })
        });

        // Open Info Link
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("OpenInfoLink") ?? "Open Info Link",
            Icon = CreateIcon("pack://application:,,,/images/info.png"),
            Command = new RelayCommand(() =>
            {
                PlaySoundEffects.PlayNotificationSound();
                ContextMenuFunctions.OpenInfoLink(context.SelectedSystemName, context.FileNameWithoutExtension,
                    context.Machines, context.Settings);
            })
        });

        // Open ROM History
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("OpenROMHistory") ?? "Open ROM History",
            Icon = CreateIcon("pack://application:,,,/images/romhistory.png"),
            Command = new RelayCommand(() =>
            {
                PlaySoundEffects.PlayNotificationSound();
                ContextMenuFunctions.OpenRomHistoryWindow(context.SelectedSystemName, context.FileNameWithoutExtension,
                    context.SelectedSystemManager, context.Machines);
            })
        });

        // Cover
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("Cover") ?? "Cover",
            Icon = CreateIcon("pack://application:,,,/images/cover.png"),
            Command = new RelayCommand(() =>
            {
                PlaySoundEffects.PlayNotificationSound();

                var systemManager = _systemManagers?.FirstOrDefault(config =>
                    config.SystemName.Equals(context.SelectedSystemName, StringComparison.OrdinalIgnoreCase));

                if (systemManager == null)
                {
                    // Notify developer
                    const string contextMessage = "systemManager is null.";
                    _ = LogErrors.LogErrorAsync(null, contextMessage);

                    // Notify user
                    MessageBoxLibrary.ErrorOpeningCoverImageMessageBox();

                    return;
                }

                ContextMenuFunctions.OpenCover(context.SelectedSystemName, context.FileNameWithoutExtension,
                    systemManager);
            })
        });

        // Title Snapshot
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("TitleSnapshot") ?? "Title Snapshot",
            Icon = CreateIcon("pack://application:,,,/images/snapshot.png"),
            Command = new RelayCommand(() =>
            {
                PlaySoundEffects.PlayNotificationSound();
                ContextMenuFunctions.OpenTitleSnapshot(context.SelectedSystemName, context.FileNameWithoutExtension);
            })
        });

        // Gameplay Snapshot
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("GameplaySnapshot") ?? "Gameplay Snapshot",
            Icon = CreateIcon("pack://application:,,,/images/snapshot.png"),
            Command = new RelayCommand(() =>
            {
                PlaySoundEffects.PlayNotificationSound();
                ContextMenuFunctions.OpenGameplaySnapshot(context.SelectedSystemName, context.FileNameWithoutExtension);
            })
        });

        // Cart
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("Cart") ?? "Cart",
            Icon = CreateIcon("pack://application:,,,/images/cart.png"),
            Command = new RelayCommand(() =>
            {
                PlaySoundEffects.PlayNotificationSound();
                ContextMenuFunctions.OpenCart(context.SelectedSystemName, context.FileNameWithoutExtension);
            })
        });

        // Video
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("Video") ?? "Video",
            Icon = CreateIcon("pack://application:,,,/images/video.png"),
            Command = new RelayCommand(() =>
            {
                PlaySoundEffects.PlayNotificationSound();
                ContextMenuFunctions.PlayVideo(context.SelectedSystemName, context.FileNameWithoutExtension);
            })
        });

        // Manual
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("Manual") ?? "Manual",
            Icon = CreateIcon("pack://application:,,,/images/manual.png"),
            Command = new RelayCommand(() =>
            {
                PlaySoundEffects.PlayNotificationSound();
                ContextMenuFunctions.OpenManual(context.SelectedSystemName, context.FileNameWithoutExtension);
            })
        });

        // Walkthrough
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("Walkthrough") ?? "Walkthrough",
            Icon = CreateIcon("pack://application:,,,/images/walkthrough.png"),
            Command = new RelayCommand(() =>
            {
                PlaySoundEffects.PlayNotificationSound();
                ContextMenuFunctions.OpenWalkthrough(context.SelectedSystemName, context.FileNameWithoutExtension);
            })
        });

        // Cabinet
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("Cabinet") ?? "Cabinet",
            Icon = CreateIcon("pack://application:,,,/images/cabinet.png"),
            Command = new RelayCommand(() =>
            {
                PlaySoundEffects.PlayNotificationSound();
                ContextMenuFunctions.OpenCabinet(context.SelectedSystemName, context.FileNameWithoutExtension);
            })
        });

        // Flyer
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("Flyer") ?? "Flyer",
            Icon = CreateIcon("pack://application:,,,/images/flyer.png"),
            Command = new RelayCommand(() =>
            {
                PlaySoundEffects.PlayNotificationSound();
                ContextMenuFunctions.OpenFlyer(context.SelectedSystemName, context.FileNameWithoutExtension);
            })
        });

        // PCB
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("PCB") ?? "PCB",
            Icon = CreateIcon("pack://application:,,,/images/pcb.png"),
            Command = new RelayCommand(() =>
            {
                PlaySoundEffects.PlayNotificationSound();
                ContextMenuFunctions.OpenPcb(context.SelectedSystemName, context.FileNameWithoutExtension);
            })
        });

        // Take Screenshot
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("TakeScreenshot") ?? "Take Screenshot",
            Icon = CreateIcon("pack://application:,,,/images/snapshot.png"),
            Command = new AsyncRelayCommand(async () =>
            {
                PlaySoundEffects.PlayNotificationSound();
                MessageBoxLibrary.TakeScreenShotMessageBox();

                var systemManager = _systemManagers?.FirstOrDefault(s => s.SystemName.Equals(context.SelectedSystemName, StringComparison.OrdinalIgnoreCase));
                if (systemManager == null)
                {
                    // Notify developer
                    const string contextMessage = "systemManager is null.";
                    _ = LogErrors.LogErrorAsync(null, contextMessage);

                    // Notify user
                    MessageBoxLibrary.CouldNotTakeScreenshotMessageBox();

                    return;
                }

                _ = ContextMenuFunctions.TakeScreenshotOfSelectedWindow(context.FileNameWithoutExtension, systemManager,
                    null, _mainWindow);

                await LaunchGameFromFavorite(context.FileNameWithExtension, context.SelectedSystemName);
            })
        });

        // Delete Game
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("DeleteGame") ?? "Delete Game",
            Icon = CreateIcon("pack://application:,,,/images/delete.png"),
            Command = new AsyncRelayCommand(async () =>
            {
                PlaySoundEffects.PlayNotificationSound();
                var result = MessageBoxLibrary.AreYouSureYouWantToDeleteTheGameMessageBox(context.FileNameWithExtension);
                if (result != MessageBoxResult.Yes)
                {
                    return;
                }

                try
                {
                    await ContextMenuFunctions.DeleteGame(context.FilePath, context.FileNameWithExtension, _mainWindow);
                    RemoveFavoriteFromXmlAndEmptyPreviewImage(context.Favorite);
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "Error deleting the file.";
                    _ = LogErrors.LogErrorAsync(ex, contextMessage);

                    // Notify user
                    MessageBoxLibrary.ThereWasAnErrorDeletingTheGameMessageBox();
                }

                RemoveFavoriteFromXmlAndEmptyPreviewImage(context.Favorite);
            })
        });

        // Delete Cover Image
        menu.Items.Add(new MenuItem
        {
            Header = Application.Current.TryFindResource("DeleteCoverImage") ?? "Delete Cover Image",
            Icon = CreateIcon("pack://application:,,,/images/delete.png"),
            Command = new AsyncRelayCommand(async () =>
            {
                PlaySoundEffects.PlayNotificationSound();
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
                    var msg = $"Error deleting cover image: {context.FileNameWithoutExtension}";
                    _ = LogErrors.LogErrorAsync(ex, msg);

                    // Notify user
                    MessageBoxLibrary.ThereWasAnErrorDeletingTheCoverImageMessageBox();
                }
            })
        });

        menu.IsOpen = true;
    }

    private static Image CreateIcon(string uri)
    {
        return new Image
        {
            Source = new BitmapImage(new Uri(uri)),
            Width = 16,
            Height = 16
        };
    }
}