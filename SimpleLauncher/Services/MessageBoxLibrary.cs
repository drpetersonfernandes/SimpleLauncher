using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SimpleLauncher.Models;

namespace SimpleLauncher.Services;

public static class MessageBoxLibrary
{
    internal static void TakeScreenShotMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        var thegamewilllaunchnow = (string)Application.Current.TryFindResource("Thegamewilllaunchnow") ?? "The game will launch now.";
        var setthegamewindowto = (string)Application.Current.TryFindResource("Setthegamewindowto") ?? "Set the game window to non-fullscreen. This is important.";
        var youshouldchangetheemulatorparameters = (string)Application.Current.TryFindResource("Youshouldchangetheemulatorparameters") ?? "You should change the emulator parameters to prevent the emulator from starting in fullscreen.";
        var aselectionwindowwillopeninSimpleLauncherallowingyou = (string)Application.Current.TryFindResource("AselectionwindowwillopeninSimpleLauncherallowingyou") ?? "A selection window will open in 'Simple Launcher', allowing you to choose the desired window to capture.";
        var assoonasyouselectawindow = (string)Application.Current.TryFindResource("assoonasyouselectawindow") ?? "As soon as you select a window, a screenshot will be taken and saved in the image folder of the selected system.";
        var takeScreenshot = (string)Application.Current.TryFindResource("TakeScreenshot") ?? "Take Screenshot";

        if (dispatcher.CheckAccess())
            ShowMsg();
        else
            dispatcher.Invoke(ShowMsg);
        return;

        void ShowMsg()
        {
            MessageBox.Show(
                $"{thegamewilllaunchnow}\n\n" +
                $"{setthegamewindowto}\n\n" +
                $"{youshouldchangetheemulatorparameters}\n\n" +
                $"{aselectionwindowwillopeninSimpleLauncherallowingyou}\n\n" +
                $"{assoonasyouselectawindow}",
                takeScreenshot, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void CouldNotSaveScreenshotMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        var failedtosavescreenshot = (string)Application.Current.TryFindResource("Failedtosavescreenshot") ?? "Failed to save screenshot.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (dispatcher.CheckAccess())
            ShowMsg();
        else
            dispatcher.Invoke(ShowMsg);
        return;

        void ShowMsg()
        {
            MessageBox.Show(
                $"{failedtosavescreenshot}\n\n" +
                $"{theerrorwasreportedtothedeveloper}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void GameIsAlreadyInFavoritesMessageBox(string fileNameWithExtension)
    {
        var dispatcher = Application.Current.Dispatcher;
        var isalreadyinfavorites = (string)Application.Current.TryFindResource("isalreadyinfavorites") ??
                                   "is already in favorites.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";

        if (dispatcher.CheckAccess())
            ShowMsg();
        else
            dispatcher.Invoke(ShowMsg);
        return;

        void ShowMsg()
        {
            MessageBox.Show(
                $"{fileNameWithExtension} {isalreadyinfavorites}",
                info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ErrorWhileAddingFavoritesMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        var anerroroccurredwhileaddingthisgame =
            (string)Application.Current.TryFindResource("Anerroroccurredwhileaddingthisgame") ??
            "An error occurred while adding this game to the favorites.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (dispatcher.CheckAccess())
            ShowMsg();
        else
            dispatcher.Invoke(ShowMsg);
        return;

        void ShowMsg()
        {
            MessageBox.Show(
                $"{anerroroccurredwhileaddingthisgame}\n\n{theerrorwasreportedtothedeveloper}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorWhileRemovingGameFromFavoriteMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        var anerroroccurredwhileremoving =
            (string)Application.Current.TryFindResource("Anerroroccurredwhileremoving") ??
            "An error occurred while removing this game from favorites.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (dispatcher.CheckAccess())
            ShowMsg();
        else
            dispatcher.Invoke(ShowMsg);
        return;

        void ShowMsg()
        {
            MessageBox.Show(
                $"{anerroroccurredwhileremoving}\n\n{theerrorwasreportedtothedeveloper}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorOpeningTheUpdateHistoryWindowMessageBox()
    {
        var erroropeningtheUpdateHistorywindow =
            (string)Application.Current.TryFindResource("ErroropeningtheUpdateHistorywindow") ??
            "Error opening the Update History window.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            ShowMessageBox();
        else
            dispatcher.Invoke((Action)ShowMessageBox);
        return;

        void ShowMessageBox()
        {
            MessageBox.Show($"{erroropeningtheUpdateHistorywindow}\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorOpeningVideoLinkMessageBox()
    {
        var therewasaproblemopeningtheVideo =
            (string)Application.Current.TryFindResource("TherewasaproblemopeningtheVideo") ??
            "There was a problem opening the Video Link.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            ShowMessageBox();
        else
            dispatcher.Invoke((Action)ShowMessageBox);
        return;

        void ShowMessageBox()
        {
            MessageBox.Show($"{therewasaproblemopeningtheVideo}\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ProblemOpeningInfoLinkMessageBox()
    {
        var therewasaproblemopeningthe = (string)Application.Current.TryFindResource("Therewasaproblemopeningthe") ??
                                         "There was a problem opening the Info Link.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            ShowMessageBox();
        else
            dispatcher.Invoke((Action)ShowMessageBox);
        return;

        void ShowMessageBox()
        {
            MessageBox.Show($"{therewasaproblemopeningthe}\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ThereIsNoCoverMessageBox()
    {
        var thereisnocoverfileassociated =
            (string)Application.Current.TryFindResource("Thereisnocoverfileassociated") ??
            "There is no cover file associated with this game.";
        var covernotfound = (string)Application.Current.TryFindResource("Covernotfound") ?? "Cover not found";

        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            ShowMessageBox();
        else
            dispatcher.Invoke((Action)ShowMessageBox);
        return;

        void ShowMessageBox()
        {
            MessageBox.Show(thereisnocoverfileassociated, covernotfound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ThereIsNoTitleSnapshotMessageBox()
    {
        var thereisnotitlesnapshot = (string)Application.Current.TryFindResource("Thereisnotitlesnapshot") ??
                                     "There is no title snapshot file associated with this game.";
        var titleSnapshotnotfound = (string)Application.Current.TryFindResource("TitleSnapshotnotfound") ??
                                    "Title Snapshot not found";

        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            ShowMessageBox();
        else
            dispatcher.Invoke((Action)ShowMessageBox);
        return;

        void ShowMessageBox()
        {
            MessageBox.Show(thereisnotitlesnapshot, titleSnapshotnotfound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ThereIsNoGameplaySnapshotMessageBox()
    {
        var thereisnogameplaysnapshot = (string)Application.Current.TryFindResource("Thereisnogameplaysnapshot") ??
                                        "There is no gameplay snapshot file associated with this game.";
        var gameplaySnapshotnotfound = (string)Application.Current.TryFindResource("GameplaySnapshotnotfound") ??
                                       "Gameplay Snapshot not found";

        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            ShowMessageBox();
        else
            dispatcher.Invoke((Action)ShowMessageBox);
        return;

        void ShowMessageBox()
        {
            MessageBox.Show(thereisnogameplaysnapshot, gameplaySnapshotnotfound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ThereIsNoCartMessageBox()
    {
        var thereisnocartfile = (string)Application.Current.TryFindResource("Thereisnocartfile") ??
                                "There is no cart file associated with this game.";
        var cartnotfound = (string)Application.Current.TryFindResource("Cartnotfound") ?? "Cart not found";
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
        {
            MessageBox.Show(thereisnocartfile, cartnotfound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            dispatcher.Invoke(() =>
                MessageBox.Show(thereisnocartfile, cartnotfound, MessageBoxButton.OK, MessageBoxImage.Information));
        }
    }

    internal static void ThereIsNoVideoFileMessageBox()
    {
        var thereisnovideofile = (string)Application.Current.TryFindResource("Thereisnovideofile") ??
                                 "There is no video file associated with this game.";
        var videonotfound = (string)Application.Current.TryFindResource("Videonotfound") ?? "Video not found";
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
        {
            MessageBox.Show(thereisnovideofile, videonotfound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            dispatcher.Invoke(() =>
                MessageBox.Show(thereisnovideofile, videonotfound, MessageBoxButton.OK, MessageBoxImage.Information));
        }
    }

    internal static void CouldNotOpenManualMessageBox()
    {
        var failedtoopenthemanual = (string)Application.Current.TryFindResource("Failedtoopenthemanual") ??
                                    "Failed to open the manual.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var message = $"{failedtoopenthemanual}\n\n{theerrorwasreportedtothedeveloper}";
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
        {
            MessageBox.Show(message, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            dispatcher.Invoke(() => MessageBox.Show(message, error, MessageBoxButton.OK, MessageBoxImage.Error));
        }
    }

    internal static void ThereIsNoManualMessageBox()
    {
        var thereisnomanual = (string)Application.Current.TryFindResource("Thereisnomanual") ??
                              "There is no manual associated with this file.";
        var manualNotFound = (string)Application.Current.TryFindResource("Manualnotfound") ?? "Manual not found";
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
        {
            MessageBox.Show(thereisnomanual, manualNotFound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            dispatcher.Invoke(() =>
                MessageBox.Show(thereisnomanual, manualNotFound, MessageBoxButton.OK, MessageBoxImage.Information));
        }
    }

    internal static void ThereIsNoWalkthroughMessageBox()
    {
        var thereisnowalkthrough = (string)Application.Current.TryFindResource("Thereisnowalkthrough") ??
                                   "There is no walkthrough file associated with this game.";
        var walkthroughnotfound = (string)Application.Current.TryFindResource("Walkthroughnotfound") ??
                                  "Walkthrough not found";
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
        {
            MessageBox.Show(thereisnowalkthrough, walkthroughnotfound, MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        else
        {
            dispatcher.Invoke(() => MessageBox.Show(thereisnowalkthrough, walkthroughnotfound, MessageBoxButton.OK,
                MessageBoxImage.Information));
        }
    }

    internal static void ThereIsNoCabinetMessageBox()
    {
        var thereisnocabinetfile = (string)Application.Current.TryFindResource("Thereisnocabinetfile") ??
                                   "There is no cabinet file associated with this game.";
        var cabinetnotfound = (string)Application.Current.TryFindResource("Cabinetnotfound") ?? "Cabinet not found";
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
        {
            MessageBox.Show(thereisnocabinetfile, cabinetnotfound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            dispatcher.Invoke(() => MessageBox.Show(thereisnocabinetfile, cabinetnotfound, MessageBoxButton.OK,
                MessageBoxImage.Information));
        }
    }

    internal static void ThereIsNoFlyerMessageBox()
    {
        var thereisnoflyer = (string)Application.Current.TryFindResource("Thereisnoflyer") ??
                             "There is no flyer file associated with this game.";
        var flyernotfound = (string)Application.Current.TryFindResource("Flyernotfound") ?? "Flyer not found";
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
        {
            MessageBox.Show(thereisnoflyer, flyernotfound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            dispatcher.Invoke(() =>
                MessageBox.Show(thereisnoflyer, flyernotfound, MessageBoxButton.OK, MessageBoxImage.Information));
        }
    }

    internal static void ThereIsNoPcbMessageBox()
    {
        var thereisnoPcBfile = (string)Application.Current.TryFindResource("ThereisnoPCBfile") ??
                               "There is no PCB file associated with this game.";
        var pCBnotfound = (string)Application.Current.TryFindResource("PCBnotfound") ?? "PCB not found";
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            ShowMsg();
        else
            dispatcher.Invoke(ShowMsg);
        return;

        void ShowMsg()
        {
            MessageBox.Show(thereisnoPcBfile, pCBnotfound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void FileSuccessfullyDeletedMessageBox(string fileNameWithExtension)
    {
        var thefile = (string)Application.Current.TryFindResource("Thefile") ?? "The file";
        var hasbeensuccessfullydeleted = (string)Application.Current.TryFindResource("hasbeensuccessfullydeleted") ??
                                         "has been successfully deleted.";
        var fileDeleted = (string)Application.Current.TryFindResource("Filedeleted") ?? "File deleted";
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            ShowMsg();
        else
            dispatcher.Invoke(ShowMsg);
        return;

        void ShowMsg()
        {
            MessageBox.Show($"{thefile} '{fileNameWithExtension}' {hasbeensuccessfullydeleted}",
                fileDeleted, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void FileCouldNotBeDeletedMessageBox(string fileNameWithExtension)
    {
        var anerroroccurredwhiletryingtodelete =
            (string)Application.Current.TryFindResource("Anerroroccurredwhiletryingtodelete") ??
            "An error occurred while trying to delete the file";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            ShowMsg();
        else
            dispatcher.Invoke(ShowMsg);
        return;

        void ShowMsg()
        {
            MessageBox.Show($"{anerroroccurredwhiletryingtodelete} '{fileNameWithExtension}'.\n\n" +
                            $"{theerrorwasreportedtothedeveloper}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void UnableToLoadImageMessageBox(string imageFileName)
    {
        var unabletoloadimage =
            (string)Application.Current.TryFindResource("Unabletoloadimage") ?? "Unable to load image";
        var thisimagemaybecorrupted = (string)Application.Current.TryFindResource("Thisimagemaybecorrupted") ??
                                      "This image may be corrupted.";
        var thedefaultimagewillbedisplayed =
            (string)Application.Current.TryFindResource("Thedefaultimagewillbedisplayed") ??
            "The default image will be displayed instead.";
        var imageloadingerror =
            (string)Application.Current.TryFindResource("Imageloadingerror") ?? "Image loading error";
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            ShowMsg();
        else
            dispatcher.Invoke(ShowMsg);
        return;

        void ShowMsg()
        {
            MessageBox.Show($"{unabletoloadimage} '{imageFileName}'.\n\n" +
                            $"{thisimagemaybecorrupted}\n\n" +
                            $"{thedefaultimagewillbedisplayed}",
                imageloadingerror, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void DefaultImageNotFoundMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            ShowMsg();
        else
            dispatcher.Invoke(ShowMsg);
        return;

        static void ShowMsg()
        {
            var defaultpngfileismissing = (string)Application.Current.TryFindResource("defaultpngfileismissing") ??
                                          "'default.png' file is missing.";
            var doyouwanttoreinstallSimpleLauncher =
                (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ??
                "Do you want to reinstall 'Simple Launcher' to fix the issue?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var reinstall = MessageBox.Show($"{defaultpngfileismissing}\n\n" +
                                            $"{doyouwanttoreinstallSimpleLauncher}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Error);
            if (reinstall == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                var pleasereinstallSimpleLauncher =
                    (string)Application.Current.TryFindResource("PleasereinstallSimpleLauncher") ??
                    "Please reinstall 'Simple Launcher' manually to fix the issue.";
                var theapplicationwillshutdown =
                    (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ??
                    "The application will shutdown.";

                MessageBox.Show($"{pleasereinstallSimpleLauncher}\n\n" +
                                $"{theapplicationwillshutdown}",
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
                QuitApplication.SimpleQuitApplication();
            }
        }
    }

    internal static void GlobalSearchErrorMessageBox()
    {
        var therewasanerrorusingtheGlobal =
            (string)Application.Current.TryFindResource("TherewasanerrorusingtheGlobal") ??
            "There was an error using the Global Search.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            ShowMsg();
        else
            dispatcher.Invoke(ShowMsg);
        return;

        void ShowMsg()
        {
            MessageBox.Show($"{therewasanerrorusingtheGlobal}\n\n" +
                            $"{theerrorwasreportedtothedeveloper}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void PleaseEnterSearchTermMessageBox()
    {
        var pleaseenterasearchterm = (string)Application.Current.TryFindResource("Pleaseenterasearchterm") ??
                                     "Please enter a search term.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";

        if (Application.Current.Dispatcher.CheckAccess())
            Show();
        else
            Application.Current.Dispatcher.Invoke(Show);
        return;

        void Show()
        {
            MessageBox.Show(pleaseenterasearchterm, warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void ErrorLaunchingGameMessageBox(string logPath)
    {
        if (Application.Current.Dispatcher.CheckAccess())
            Show();
        else
            Application.Current.Dispatcher.Invoke(Show);
        return;

        void Show()
        {
            var therewasanerrorlaunchingtheselected =
                (string)Application.Current.TryFindResource("Therewasanerrorlaunchingtheselected") ??
                "There was an error launching the selected game.";
            var dowanttoopenthefileerroruserlog =
                (string)Application.Current.TryFindResource("Dowanttoopenthefileerroruserlog") ??
                "Do want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{therewasanerrorlaunchingtheselected}\n\n{dowanttoopenthefileerroruserlog}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                var thefileerroruserlogwasnotfound =
                    (string)Application.Current.TryFindResource("Thefileerroruserlogwasnotfound") ??
                    "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlogwasnotfound, error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void SelectAGameToLaunchMessageBox()
    {
        var pleaseselectagametolaunch = (string)Application.Current.TryFindResource("Pleaseselectagametolaunch") ??
                                        "Please select a game to launch.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";

        if (Application.Current.Dispatcher.CheckAccess())
            Show();
        else
            Application.Current.Dispatcher.Invoke(Show);
        return;

        void Show()
        {
            MessageBox.Show(pleaseselectagametolaunch, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ErrorLoadingSystemConfigMessageBox()
    {
        var therewasanerrorloadingthesystemConfig =
            (string)Application.Current.TryFindResource("TherewasanerrorloadingthesystemConfig") ??
            "There was an error loading the systemConfig.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
            Show();
        else
            Application.Current.Dispatcher.Invoke(Show);
        return;

        void Show()
        {
            MessageBox.Show($"{therewasanerrorloadingthesystemConfig}\n\n{theerrorwasreportedtothedeveloper}", error,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FileAddedToFavoritesMessageBox(string fileNameWithoutExtension)
    {
        var hasbeenaddedtofavorites = (string)Application.Current.TryFindResource("hasbeenaddedtofavorites") ??
                                      "has been added to favorites.";
        var success = (string)Application.Current.TryFindResource("Success") ?? "Success";

        if (Application.Current.Dispatcher.CheckAccess())
            Show();
        else
            Application.Current.Dispatcher.Invoke(Show);
        return;

        void Show()
        {
            MessageBox.Show($"{fileNameWithoutExtension} {hasbeenaddedtofavorites}", success,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void FileRemovedFromFavoritesMessageBox(string fileNameWithoutExtension)
    {
        var wasremovedfromfavorites = (string)Application.Current.TryFindResource("wasremovedfromfavorites") ??
                                      "was removed from favorites.";
        var success = (string)Application.Current.TryFindResource("Success") ?? "Success";

        if (Application.Current.Dispatcher.CheckAccess())
            Show();
        else
            Application.Current.Dispatcher.Invoke(Show);
        return;

        void Show()
        {
            MessageBox.Show($"{fileNameWithoutExtension} {wasremovedfromfavorites}", success,
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void CouldNotLaunchThisGameMessageBox(string logPath)
    {
        if (Application.Current.Dispatcher.CheckAccess())
            Show();
        else
            Application.Current.Dispatcher.Invoke(Show);
        return;

        void Show()
        {
            var simpleLaunchercouldnotlaunchthisgame =
                (string)Application.Current.TryFindResource("SimpleLaunchercouldnotlaunchthisgame") ??
                "'Simple Launcher' could not launch this game.";
            var dowanttoopenthefileerroruserlog =
                (string)Application.Current.TryFindResource("Dowanttoopenthefileerroruserlog") ??
                "Do want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{simpleLaunchercouldnotlaunchthisgame}\n\n{dowanttoopenthefileerroruserlog}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                var thefileerroruserlogwasnotfound =
                    (string)Application.Current.TryFindResource("Thefileerroruserlogwasnotfound") ??
                    "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlogwasnotfound, error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void ErrorCalculatingStatsMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            Action();
        else
            dispatcher.Invoke(Action);
        return;

        static void Action()
        {
            var anerroroccurredwhilecalculatingtheGlobal =
                (string)Application.Current.TryFindResource("AnerroroccurredwhilecalculatingtheGlobal") ??
                "An error occurred while calculating the Global Statistics.";
            var theerrorwasreportedtothedeveloper =
                (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
                "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            MessageBox.Show($"{anerroroccurredwhilecalculatingtheGlobal}\n\n{theerrorwasreportedtothedeveloper}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FailedSaveReportMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            Action();
        else
            dispatcher.Invoke(Action);
        return;

        static void Action()
        {
            var failedtosavethereport =
                (string)Application.Current.TryFindResource("Failedtosavethereport") ??
                "Failed to save the report.";
            var theerrorwasreportedtothedeveloper =
                (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
                "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            MessageBox.Show($"{failedtosavethereport}\n\n{theerrorwasreportedtothedeveloper}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ReportSavedMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            Action();
        else
            dispatcher.Invoke(Action);
        return;

        static void Action()
        {
            var reportsavedsuccessfully =
                (string)Application.Current.TryFindResource("Reportsavedsuccessfully") ??
                "Report saved successfully.";
            var success = (string)Application.Current.TryFindResource("Success") ?? "Success";

            MessageBox.Show(reportsavedsuccessfully, success, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void NoStatsToSaveMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            Action();
        else
            dispatcher.Invoke(Action);
        return;

        static void Action()
        {
            var nostatisticsavailabletosave =
                (string)Application.Current.TryFindResource("Nostatisticsavailabletosave") ??
                "No statistics available to save.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show(nostatisticsavailabletosave, error, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void ErrorLaunchingToolMessageBox(string logPath)
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            Action();
        else
            dispatcher.Invoke(Action);
        return;

        void Action()
        {
            var anerroroccurredwhilelaunchingtheselectedtool =
                (string)Application.Current.TryFindResource("Anerroroccurredwhilelaunchingtheselectedtool") ??
                "An error occurred while launching the selected tool.";
            var grantSimpleLauncheradministrative =
                (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ??
                "Grant 'Simple Launcher' administrative access and try again.";
            var temporarilydisableyourantivirussoftware =
                (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ??
                "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
            var dowanttoopenthefileerroruserlog =
                (string)Application.Current.TryFindResource("Dowanttoopenthefileerroruserlog") ??
                "Do want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = MessageBox.Show(
                $"{anerroroccurredwhilelaunchingtheselectedtool}\n\n" +
                $"{grantSimpleLauncheradministrative}\n\n" +
                $"{temporarilydisableyourantivirussoftware}\n\n" +
                $"{dowanttoopenthefileerroruserlog}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                var thefileerroruserlogwasnotfound =
                    (string)Application.Current.TryFindResource("Thefileerroruserlogwasnotfound") ??
                    "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlogwasnotfound, error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void SelectedToolNotFoundMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            Action();
        else
            dispatcher.Invoke(Action);
        return;

        static void Action()
        {
            var theselectedtoolwasnotfound =
                (string)Application.Current.TryFindResource("Theselectedtoolwasnotfound") ??
                "The selected tool was not found in the expected path.";
            var doyouwanttoreinstallSimpleLauncher =
                (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ??
                "Do you want to reinstall 'Simple Launcher' to fix the issue?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var reinstall = MessageBox.Show(
                $"{theselectedtoolwasnotfound}\n\n{doyouwanttoreinstallSimpleLauncher}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (reinstall == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                var pleasereinstallSimpleLaunchermanually =
                    (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ??
                    "Please reinstall 'Simple Launcher' manually to fix the issue.";

                MessageBox.Show(pleasereinstallSimpleLaunchermanually, error, MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }

    internal static void ErrorMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            Action();
        else
            dispatcher.Invoke(Action);
        return;

        static void Action()
        {
            var therewasanerror =
                (string)Application.Current.TryFindResource("Therewasanerror") ?? "There was an error.";
            var theerrorwasreportedtothedeveloper =
                (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
                "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show($"{therewasanerror}\n\n{theerrorwasreportedtothedeveloper}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void NoFavoriteFoundMessageBox()
    {
        var nofavoritegamesfoundfortheselectedsystem =
            (string)Application.Current.TryFindResource("Nofavoritegamesfoundfortheselectedsystem") ??
            "No favorite games found for the selected system.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        // Only invoke if needed
        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show(nofavoritegamesfoundfortheselectedsystem, info, MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
                MessageBox.Show(nofavoritegamesfoundfortheselectedsystem, info, MessageBoxButton.OK,
                    MessageBoxImage.Information));
        }
    }

    internal static void MoveToWritableFolderMessageBox()
    {
        var itlookslikeSimpleLauncherisinstalled = (string)Application.Current.TryFindResource("ItlookslikeSimpleLauncherisinstalled") ?? "It looks like 'Simple Launcher' is installed in a restricted folder (e.g., Program Files), where it does not have write access.";
        var itneedswriteaccesstoitsfolder = (string)Application.Current.TryFindResource("Itneedswriteaccesstoitsfolder") ?? "It needs write access to its folder.";
        var pleasemovetheapplicationfolder = (string)Application.Current.TryFindResource("Pleasemovetheapplicationfolder") ?? "Please move the application folder to a writable location like the 'Documents' folder.";
        var ifpossiblerunitwithadministrative = (string)Application.Current.TryFindResource("Ifpossiblerunitwithadministrative") ?? "If possible, run it with administrative privileges.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessage);
        }

        return;

        void ShowMessage()
        {
            MessageBox.Show(
                $"{itlookslikeSimpleLauncherisinstalled}\n\n" + $"{itneedswriteaccesstoitsfolder}\n\n" +
                $"{pleasemovetheapplicationfolder}\n\n" + $"{ifpossiblerunitwithadministrative}", warning,
                MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void InvalidSystemConfigMessageBox()
    {
        var therewasanerrorwhileloading = (string)Application.Current.TryFindResource("Therewasanerrorwhileloading") ??
                                          "There was an error while loading the system configuration for this system.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessage);
        }

        return;

        void ShowMessage()
        {
            MessageBox.Show($"{therewasanerrorwhileloading}\n\n{theerrorwasreportedtothedeveloper}", error,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorMethodLoadGameFilesAsyncMessageBox()
    {
        var therewasanerrorloadingthegame =
            (string)Application.Current.TryFindResource("Therewasanerrorloadingthegame") ??
            "There was an error loading the game list.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessage);
        }

        return;

        void ShowMessage()
        {
            MessageBox.Show($"{therewasanerrorloadingthegame}\n\n{theerrorwasreportedtothedeveloper}", error,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorOpeningDonationLinkMessageBox()
    {
        var therewasanerroropeningthedonation =
            (string)Application.Current.TryFindResource("Therewasanerroropeningthedonation") ??
            "There was an error opening the Donation Link.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessage);
        }

        return;

        void ShowMessage()
        {
            MessageBox.Show($"{therewasanerroropeningthedonation}\n\n{theerrorwasreportedtothedeveloper}", error,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ToggleGamepadFailureMessageBox()
    {
        var failedtotogglegamepad = (string)Application.Current.TryFindResource("Failedtotogglegamepad") ??
                                    "Failed to toggle gamepad.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessage);
        }

        return;

        void ShowMessage()
        {
            MessageBox.Show($"{failedtotogglegamepad}\n\n{theerrorwasreportedtothedeveloper}", error,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FindRomCoverMissingMessageBox()
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessage);
        }

        return;

        static void ShowMessage()
        {
            var findRomCoverexewasnotfound = (string)Application.Current.TryFindResource("FindRomCoverexewasnotfound") ?? "'FindRomCover.exe' was not found in the expected path.";
            var doyouwanttoreinstall = (string)Application.Current.TryFindResource("Doyouwanttoreinstall") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var reinstall = MessageBox.Show($"{findRomCoverexewasnotfound}\n\n{doyouwanttoreinstall}", error, MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (reinstall == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                var pleasereinstallSimpleLaunchermanually = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
                MessageBox.Show(pleasereinstallSimpleLaunchermanually, error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void FindRomCoverLaunchWasCanceledByUserMessageBox()
    {
        var thelaunchofFindRomCoverexewascanceled =
            (string)Application.Current.TryFindResource("ThelaunchofFindRomCoverexewascanceled") ??
            "The launch of 'FindRomCover.exe' was canceled by the user.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessage);
        }

        return;

        void ShowMessage()
        {
            MessageBox.Show(thelaunchofFindRomCoverexewascanceled, info, MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    internal static void FindRomCoverLaunchWasBlockedMessageBox(string logPath)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessage);
        }

        return;

        void ShowMessage()
        {
            var anerroroccurredwhiletryingtolaunch = (string)Application.Current.TryFindResource("Anerroroccurredwhiletryingtolaunch") ?? "An error occurred while trying to launch 'FindRomCover.exe'.";
            var grantSimpleLauncheradministrative = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
            var temporarilydisableyourantivirus = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
            var wouldyouliketoopentheerroruserlog = (string)Application.Current.TryFindResource("Wouldyouliketoopentheerroruserlog") ?? "Would you like to open the 'error_user.log' file to investigate the issue?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{anerroroccurredwhiletryingtolaunch}\n\n" + $"{grantSimpleLauncheradministrative}\n\n" + $"{temporarilydisableyourantivirus}\n\n" + $"{wouldyouliketoopentheerroruserlog}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                Process.Start(new ProcessStartInfo { FileName = logPath, UseShellExecute = true });
            }
            catch (Exception)
            {
                var thefileerroruserlog = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlog, error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void ErrorChangingViewModeMessageBox()
    {
        var therewasanerrorwhilechangingtheviewmode =
            (string)Application.Current.TryFindResource("Therewasanerrorwhilechangingtheviewmode") ??
            "There was an error while changing the view mode.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessage);
        }

        return;

        void ShowMessage()
        {
            MessageBox.Show($"{therewasanerrorwhilechangingtheviewmode}\n\n{theerrorwasreportedtothedeveloper}", error,
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void NavigationButtonErrorMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            Show();
        else
            dispatcher.Invoke(Show);
        return;

        static void Show()
        {
            var msg1 = (string)Application.Current.TryFindResource("Therewasanerrorinthenavigationbutton") ??
                       "There was an error in the navigation button.";
            var msg2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
                       "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show($"{msg1}\n\n{msg2}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SelectSystemBeforeSearchMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            Show();
        else
            dispatcher.Invoke(Show);
        return;

        static void Show()
        {
            var msg = (string)Application.Current.TryFindResource("Pleaseselectasystembeforesearching") ??
                      "Please select a system before searching.";
            var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            MessageBox.Show(msg, warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void EnterSearchQueryMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            Show();
        else
            dispatcher.Invoke(Show);
        return;

        static void Show()
        {
            var msg = (string)Application.Current.TryFindResource("Pleaseenterasearchquery") ??
                      "Please enter a search query.";
            var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            MessageBox.Show(msg, warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void ErrorWhileLoadingHelpUserXmlMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            Show();
        else
            dispatcher.Invoke(Show);
        return;

        static void Show()
        {
            var msg1 = (string)Application.Current.TryFindResource("Unexpectederrorwhileloadinghelpuserxml") ??
                       "Unexpected error while loading 'helpuser.xml'.";
            var msg2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ??
                       "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{msg1}\n\n{msg2}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
        }
    }

    internal static void NoSystemInHelpUserXmlMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            Show();
        else
            dispatcher.Invoke(Show);
        return;

        static void Show()
        {
            var msg1 = (string)Application.Current.TryFindResource("Novalidsystemsfoundinthefilehelpuserxml") ??
                       "No valid systems found in the file 'helpuser.xml'.";
            var msg2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ??
                       "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{msg1}\n\n{msg2}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
        }
    }

    internal static bool CouldNotLoadHelpUserXmlMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        return dispatcher.CheckAccess() ? Show() : dispatcher.Invoke(Show);

        static bool Show()
        {
            var msg1 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotloadhelpuserxml") ?? "'Simple Launcher' could not load 'helpuser.xml'.";
            var msg2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{msg1}\n\n{msg2}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    internal static void FailedToLoadHelpUserXmlMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        var showAction = Show; // Define an Action delegate for the local Show method

        if (dispatcher.CheckAccess())
        {
            showAction();
        }
        else
        {
            dispatcher.Invoke(showAction);
        }

        return;

        static void Show() // Changed return type from bool to void
        {
            var msg1 = (string)Application.Current.TryFindResource("Unabletoloadhelpuserxml") ?? "Unable to load 'helpuser.xml'. The file may be corrupted.";
            var msg2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{msg1}\n\n{msg2}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
                // No return false needed
            }
            // No return true needed
        }
    }

    internal static void FileHelpUserXmlIsMissingMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        var showAction = Show; // Define an Action delegate for the local Show method

        if (dispatcher.CheckAccess())
        {
            showAction();
        }
        else
        {
            dispatcher.Invoke(showAction);
        }

        return;

        static void Show() // Changed return type from bool to void
        {
            var msg1 = (string)Application.Current.TryFindResource("Thefilehelpuserxmlismissing") ?? "The file 'helpuser.xml' is missing.";
            var msg2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{msg1}\n\n{msg2}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
                // No return false needed
            }
            // No return true needed
        }
    }

    internal static void ImageViewerErrorMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            Show();
        else
            dispatcher.Invoke(Show);
        return;

        static void Show()
        {
            var msg1 = (string)Application.Current.TryFindResource("FailedtoloadtheimageintheImage") ??
                       "Failed to load the image in the Image Viewer window.";
            var msg2 = (string)Application.Current.TryFindResource("Theimagemaybecorruptedorinaccessible") ??
                       "The image may be corrupted or inaccessible.";
            var msg3 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
                       "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show($"{msg1}\n\n{msg2}\n\n{msg3}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ReinstallSimpleLauncherFileCorruptedMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            Show();
        else
            dispatcher.Invoke(Show);
        return;

        static void Show()
        {
            var msg1 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotloadthefilemamedat") ??
                       "'Simple Launcher' could not load the file 'mame.dat' or it is corrupted.";
            var msg2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ??
                       "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{msg1}\n\n{msg2}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                var msg3 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ??
                           "Please reinstall 'Simple Launcher' manually to fix the issue.";
                var msg4 = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ??
                           "The application will shutdown.";
                MessageBox.Show($"{msg3}\n\n{msg4}", error, MessageBoxButton.OK, MessageBoxImage.Error);
                QuitApplication.SimpleQuitApplication();
            }
        }
    }

    internal static void ReinstallSimpleLauncherFileMissingMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess()) ShowMsgBox();
        else dispatcher.Invoke((Action)ShowMsgBox);
        return;

        static void ShowMsgBox()
        {
            var thefilemamedatcouldnotbefound = (string)Application.Current.TryFindResource("Thefilemamedatcouldnotbefound") ?? "The file 'mame.dat' could not be found in the application folder.";
            var doyouwanttoautomaticreinstall = (string)Application.Current.TryFindResource("Doyouwanttoautomaticreinstall") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{thefilemamedatcouldnotbefound}\n\n" + $"{doyouwanttoautomaticreinstall}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                var pleasereinstallSimpleLaunchermanually = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
                var theapplicationwillshutdown = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
                MessageBox.Show($"{pleasereinstallSimpleLaunchermanually}\n\n" + $"{theapplicationwillshutdown}", error, MessageBoxButton.OK, MessageBoxImage.Error);

                QuitApplication.SimpleQuitApplication();
            }
        }
    }

    internal static void ErrorCheckingForUpdatesMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        var anerroroccurredwhilecheckingforupdates =
            (string)Application.Current.TryFindResource("Anerroroccurredwhilecheckingforupdates") ??
            "An error occurred while checking for updates.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (dispatcher.CheckAccess()) ShowMsgBox();
        else dispatcher.Invoke((Action)ShowMsgBox);
        return;

        void ShowMsgBox()
        {
            MessageBox.Show($"{anerroroccurredwhilecheckingforupdates}\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void UpdaterNotFoundMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        var updaterexenotfound = (string)Application.Current.TryFindResource("Updaterexenotfound") ??
                                 "'Updater.exe' not found.";
        var pleasereinstallSimpleLaunchermanually =
            (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ??
            "Please reinstall 'Simple Launcher' manually to fix the issue.";
        var theapplicationwillnowshutdown =
            (string)Application.Current.TryFindResource("Theapplicationwillnowshutdown") ??
            "The application will now shutdown.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (dispatcher.CheckAccess()) ShowMsgBox();
        else dispatcher.Invoke((Action)ShowMsgBox);
        return;

        void ShowMsgBox()
        {
            MessageBox.Show($"{updaterexenotfound}\n\n" + $"{pleasereinstallSimpleLaunchermanually}\n\n" + $"{theapplicationwillnowshutdown}", error, MessageBoxButton.OK, MessageBoxImage.Question);
            QuitApplication.SimpleQuitApplication();
        }
    }

    internal static void ErrorLoadingRomHistoryMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        var anerroroccurredwhileloadingRoMhistory =
            (string)Application.Current.TryFindResource("AnerroroccurredwhileloadingROMhistory") ??
            "An error occurred while loading ROM history.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (dispatcher.CheckAccess()) ShowMsgBox();
        else dispatcher.Invoke((Action)ShowMsgBox);
        return;

        void ShowMsgBox()
        {
            MessageBox.Show($"{anerroroccurredwhileloadingRoMhistory}\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void NoHistoryXmlFoundMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess()) ShowMsgBox();
        else dispatcher.Invoke((Action)ShowMsgBox);
        return;

        static void ShowMsgBox()
        {
            var nohistoryxmlfilefound = (string)Application.Current.TryFindResource("Nohistoryxmlfilefound") ?? "No 'history.xml' file found in the application folder.";
            var doyouwanttoreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{nohistoryxmlfilefound}\n\n" + $"{doyouwanttoreinstallSimpleLauncher}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
        }
    }

    internal static void ErrorOpeningBrowserMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        var anerroroccurredwhileopeningthebrowser =
            (string)Application.Current.TryFindResource("Anerroroccurredwhileopeningthebrowser") ??
            "An error occurred while opening the browser.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (dispatcher.CheckAccess()) ShowMsgBox();
        else dispatcher.Invoke((Action)ShowMsgBox);
        return;

        void ShowMsgBox()
        {
            MessageBox.Show($"{anerroroccurredwhileopeningthebrowser}\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SimpleLauncherNeedMorePrivilegesMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        var simpleLauncherlackssufficientprivilegestowrite = (string)Application.Current.TryFindResource("SimpleLauncherlackssufficientprivilegestowrite") ?? "'Simple Launcher' lacks sufficient privileges to write to the 'settings.xml' file.";
        var areyourunningasecondinstance = (string)Application.Current.TryFindResource("areyourunningasecondinstance") ?? "Are you running a second instance of 'Simple Launcher'? If yes, please open only one instance at a time or you may encounter issues.";
        var grantSimpleLauncheradministrative = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var temporarilydisableyourantivirus = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var ensurethattheSimpleLauncherfolderislocatedinawritable = (string)Application.Current.TryFindResource("EnsurethattheSimpleLauncherfolderislocatedinawritable") ?? "Ensure that the 'Simple Launcher' folder is located in a writable directory.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (dispatcher.CheckAccess()) ShowMsgBox();
        else dispatcher.Invoke((Action)ShowMsgBox);
        return;

        void ShowMsgBox()
        {
            MessageBox.Show($"{simpleLauncherlackssufficientprivilegestowrite}\n\n" + $"{areyourunningasecondinstance}\n\n" + $"{grantSimpleLauncheradministrative}\n\n" + $"{ensurethattheSimpleLauncherfolderislocatedinawritable}\n\n" + $"{temporarilydisableyourantivirus}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void DepViolationMessageBox(string logPath)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessageBox();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }

        return;

        void ShowMessageBox()
        {
            var depViolationError = (string)Application.Current.TryFindResource("DEPViolationError") ?? "A Data Execution Prevention (DEP) violation occurred while running the emulator, which is a Windows security feature that prevents applications from executing code from non-executable memory regions. This commonly happens with older emulators or ones that use specific memory access techniques that modern security systems flag as potentially dangerous.";
            var whatIsDep = (string)Application.Current.TryFindResource("WhatIsDEP") ?? "DEP is a security feature that helps prevent damage from viruses and other security threats.";
            var howToFixDep = (string)Application.Current.TryFindResource("HowToFixDEP") ?? "You can try the following solutions:";
            var solution1 = (string)Application.Current.TryFindResource("DEPSolution1") ?? "1. Run 'Simple Launcher' with administrator privileges.";
            var solution2 = (string)Application.Current.TryFindResource("DEPSolution2") ?? "2. Add the emulator to DEP exceptions in Windows Security settings.";
            var solution3 = (string)Application.Current.TryFindResource("DEPSolution3") ?? "3. Try using a different emulator compatible with your system.";
            var solution4 = (string)Application.Current.TryFindResource("DEPSolution4") ?? "4. Update your emulator to the latest version.";
            var youcanturnoffthistypeoferrormessageinExpertmode = (string)Application.Current.TryFindResource("YoucanturnoffthistypeoferrormessageinExpertmode") ?? "You can turn off this type of error message in Expert mode.";
            var doyouwanttoopenthefile = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = MessageBox.Show($"{depViolationError}\n\n" +
                                         $"{whatIsDep}\n\n" +
                                         $"{howToFixDep}\n" +
                                         $"{solution1}\n" +
                                         $"{solution2}\n" +
                                         $"{solution3}\n" +
                                         $"{solution4}\n\n" +
                                         $"{youcanturnoffthistypeoferrormessageinExpertmode}\n\n" +
                                         $"{doyouwanttoopenthefile}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                var thefileerroruserlog = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlog, error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void CheckForMemoryAccessViolation(string logPath)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessageBox();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }

        return;

        void ShowMessageBox()
        {
            var memoryViolationError = (string)Application.Current.TryFindResource("MemoryAccessViolationError") ?? "A Memory Access Violation occurred while running the emulator. This happens when a program attempts to access memory that it either doesn't have permission to access or memory that has been freed/doesn't exist. This is common in emulators that manipulate memory directly to achieve accurate emulation of other systems.";
            var whatIsMemoryAccess = (string)Application.Current.TryFindResource("WhatIsMemoryAccess") ?? "Memory Access Violations are security mechanisms that prevent programs from accessing or modifying memory outside their allocated space, which could potentially crash your system or create security vulnerabilities.";
            var howToFixMemoryAccess = (string)Application.Current.TryFindResource("HowToFixMemoryAccess") ?? "You can try the following solutions:";
            var solution1 = (string)Application.Current.TryFindResource("MemorySolution1") ?? "1. Run the application with administrator privileges.";
            var solution2 = (string)Application.Current.TryFindResource("MemorySolution2") ?? "2. Check if your antivirus is blocking the emulator's memory operations.";
            var solution3 = (string)Application.Current.TryFindResource("MemorySolution3") ?? "3. Update your emulator to the latest version which may have fixed memory handling issues.";
            var solution4 = (string)Application.Current.TryFindResource("MemorySolution4") ?? "4. Try adjusting memory allocation settings in the emulator configuration if available.";
            var youcanturnoffthistypeoferrormessageinExpertmode = (string)Application.Current.TryFindResource("YoucanturnoffthistypeoferrormessageinExpertmode") ?? "You can turn off this type of error message in Expert mode.";
            var doyouwanttoopenthefileerroruserlog = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{memoryViolationError}\n\n" +
                                         $"{whatIsMemoryAccess}\n\n" +
                                         $"{howToFixMemoryAccess}\n" +
                                         $"{solution1}\n" +
                                         $"{solution2}\n" +
                                         $"{solution3}\n" +
                                         $"{solution4}\n\n" +
                                         $"{youcanturnoffthistypeoferrormessageinExpertmode}\n\n" +
                                         $"{doyouwanttoopenthefileerroruserlog}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                var thefileerroruserlog = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlog, error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void SystemXmlIsCorruptedMessageBox(string logPath)
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess()) ShowMsgBox();
        else dispatcher.Invoke((Action)ShowMsgBox);
        return;

        void ShowMsgBox()
        {
            var systemxmliscorrupted = (string)Application.Current.TryFindResource("systemxmliscorrupted") ?? "'system.xml' is corrupted or could not be opened.";
            var pleasefixitmanuallyordeleteit = (string)Application.Current.TryFindResource("Pleasefixitmanuallyordeleteit") ?? "Please fix it manually or delete it.";
            var ifyouchoosetodeleteit = (string)Application.Current.TryFindResource("Ifyouchoosetodeleteit") ?? "If you choose to delete it, 'Simple Launcher' will create a new one for you.";
            var theapplicationwillshutdown = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
            var wouldyouliketoopentheerroruserlog = (string)Application.Current.TryFindResource("Wouldyouliketoopentheerroruserlog") ?? "Would you like to open the 'error_user.log' file to investigate the issue?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{systemxmliscorrupted} {pleasefixitmanuallyordeleteit}\n\n" + $"{ifyouchoosetodeleteit}\n\n" + $"{theapplicationwillshutdown}" + $"{wouldyouliketoopentheerroruserlog}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = logPath, UseShellExecute = true });
                }
                catch (Exception)
                {
                    var thefileerroruserlog = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
                    MessageBox.Show(thefileerroruserlog, error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            QuitApplication.SimpleQuitApplication();
        }
    }

    internal static void FileSystemXmlIsCorruptedMessageBox(string logPath)
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
            Show();
        else
            dispatcher.Invoke(Show);
        return;

        void Show()
        {
            var thefilesystemxmlisbadlycorrupted =
                (string)Application.Current.TryFindResource("Thefilesystemxmlisbadlycorrupted") ??
                "The file 'system.xml' is badly corrupted.";
            var wouldyouliketoopentheerroruserlog =
                (string)Application.Current.TryFindResource("Wouldyouliketoopentheerroruserlog") ??
                "Would you like to open the 'error_user.log' file to investigate the issue?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{thefilesystemxmlisbadlycorrupted}\n\n" +
                                         $"{wouldyouliketoopentheerroruserlog}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                var thefileerroruserlog = (string)Application.Current.TryFindResource("Thefileerroruserlog") ??
                                          "The file 'error_user.log' was not found!";

                MessageBox.Show(thefileerroruserlog,
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void InstallUpdateManuallyMessageBox(string repoOwner, string repoName)
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
            Show();
        else
            dispatcher.Invoke(Show);
        return;

        void Show()
        {
            var therewasanerrorinstallingorupdating = (string)Application.Current.TryFindResource("Therewasanerrorinstallingorupdating") ?? "There was an error installing or updating the application.";
            var wouldyouliketoberedirectedtothedownloadpage = (string)Application.Current.TryFindResource("Wouldyouliketoberedirectedtothedownloadpage") ?? "Would you like to be redirected to the download page to install or update it manually?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var messageBoxResult = MessageBox.Show(
                $"{therewasanerrorinstallingorupdating}\n\n" +
                $"{wouldyouliketoberedirectedtothedownloadpage}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult != MessageBoxResult.Yes) return;

            var downloadPageUrl = $"https://github.com/{repoOwner}/{repoName}/releases/latest";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = downloadPageUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Error in method InstallUpdateManuallyMessageBox");
                // Notify user
                var anerroroccurredwhileopeningthebrowser = (string)Application.Current.TryFindResource("Anerroroccurredwhileopeningthebrowser") ?? "An error occurred while opening the browser.";
                var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
                MessageBox.Show($"{anerroroccurredwhileopeningthebrowser}\n\n" +
                                $"{theerrorwasreportedtothedeveloper}",
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void DownloadManuallyMessageBox(string repoOwner, string repoName)
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
            Show();
        else
            dispatcher.Invoke(Show);
        return;

        void Show()
        {
            var updaterexenotfoundintheapplication =
                (string)Application.Current.TryFindResource("Updaterexenotfoundintheapplication") ??
                "'Updater.exe' not found in the application directory.";
            var wouldyouliketoberedirectedtotheSimpleLauncherdownloadpage =
                (string)Application.Current.TryFindResource(
                    "WouldyouliketoberedirectedtotheSimpleLauncherdownloadpage") ??
                "Would you like to be redirected to the 'Simple Launcher' download page to download it manually?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var messageBoxResult = MessageBox.Show($"{updaterexenotfoundintheapplication}\n\n" +
                                                   $"{wouldyouliketoberedirectedtotheSimpleLauncherdownloadpage}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult != MessageBoxResult.Yes) return;

            var downloadPageUrl = $"https://github.com/{repoOwner}/{repoName}/releases/latest";
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = downloadPageUrl,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Error in method DownloadManuallyMessageBox");
                // Notify user
                var anerroroccurredwhileopeningthebrowser =
                    (string)Application.Current.TryFindResource("Anerroroccurredwhileopeningthebrowser") ??
                    "An error occurred while opening the browser.";
                var theerrorwasreportedtothedeveloper =
                    (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
                    "The error was reported to the developer who will try to fix the issue.";
                MessageBox.Show($"{anerroroccurredwhileopeningthebrowser}\n\n" +
                                $"{theerrorwasreportedtothedeveloper}",
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void RequiredFileMissingMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
            Show();
        else
            dispatcher.Invoke(Show);
        return;

        static void Show()
        {
            var fileappsettingsjsonismissing =
                (string)Application.Current.TryFindResource("Fileappsettingsjsonismissing") ??
                "File 'appsettings.json' is missing.";
            var theapplicationwillnotbeabletosendthesupportrequest =
                (string)Application.Current.TryFindResource("Theapplicationwillnotbeabletosendthesupportrequest") ??
                "The application will not be able to send the support request.";
            var doyouwanttoautomaticallyreinstall =
                (string)Application.Current.TryFindResource("Doyouwanttoautomaticallyreinstall") ??
                "Do you want to automatically reinstall 'Simple Launcher' to fix the problem?";
            var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            var messageBoxResult = MessageBox.Show(
                $"{fileappsettingsjsonismissing}\n\n" +
                $"{theapplicationwillnotbeabletosendthesupportrequest}\n\n" +
                $"{doyouwanttoautomaticallyreinstall}",
                warning, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                var pleasereinstallSimpleLauncher =
                    (string)Application.Current.TryFindResource("PleasereinstallSimpleLauncher") ??
                    "Please reinstall 'Simple Launcher' manually to fix the issue.";
                MessageBox.Show(pleasereinstallSimpleLauncher,
                    warning, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    internal static void EnterSupportRequestMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
            Show();
        else
            dispatcher.Invoke(Show);
        return;

        static void Show()
        {
            var pleaseenterthedetailsofthesupportrequest =
                (string)Application.Current.TryFindResource("Pleaseenterthedetailsofthesupportrequest") ??
                "Please enter the details of the support request.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            MessageBox.Show(pleaseenterthedetailsofthesupportrequest,
                info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void EnterNameMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
            Show();
        else
            dispatcher.Invoke(Show);
        return;

        static void Show()
        {
            var pleaseenterthename = (string)Application.Current.TryFindResource("Pleaseenterthename") ?? "Please enter the name.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            MessageBox.Show(pleaseenterthename,
                info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void EnterEmailMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
            Show();
        else
            dispatcher.Invoke(Show);
        return;

        static void Show()
        {
            var pleaseentertheemail = (string)Application.Current.TryFindResource("Pleaseentertheemail") ?? "Please enter the email.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            MessageBox.Show(pleaseentertheemail,
                info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ApiKeyErrorMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
            Show();
        else
            dispatcher.Invoke(Show);
        return;

        static void Show()
        {
            var therewasanerrorintheApiKey =
                (string)Application.Current.TryFindResource("TherewasanerrorintheAPIKey") ??
                "There was an error in the API Key of this form.";
            var theerrorwasreportedtothedeveloper =
                (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
                "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show($"{therewasanerrorintheApiKey}\n\n" +
                            $"{theerrorwasreportedtothedeveloper}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SupportRequestSuccessMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
            Show();
        else
            dispatcher.Invoke(Show);
        return;

        static void Show()
        {
            var supportrequestsentsuccessfully = (string)Application.Current.TryFindResource("Supportrequestsentsuccessfully") ?? "Support request sent successfully.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            MessageBox.Show(supportrequestsentsuccessfully,
                info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void SupportRequestSendErrorMessageBox()
    {
        var anerroroccurredwhilesendingthesupportrequest =
            (string)Application.Current.TryFindResource("Anerroroccurredwhilesendingthesupportrequest") ??
            "An error occurred while sending the support request.";
        var thebugwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Thebugwasreportedtothedeveloper") ??
            "The bug was reported to the developer that will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
            Show();
        else
            Application.Current.Dispatcher.Invoke((Action)Show);
        return;

        void Show()
        {
            MessageBox.Show($"{anerroroccurredwhilesendingthesupportrequest}\n\n" + $"{thebugwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ExtractionFailedMessageBox()
    {
        var extractionfailed = (string)Application.Current.TryFindResource("Extractionfailed") ?? "Extraction failed.";
        var ensurethefileisnotcorrupted = (string)Application.Current.TryFindResource("Ensurethefileisnotcorrupted") ?? "Ensure the file is not corrupted.";
        var ensureyouhaveenoughspaceintheHdd = (string)Application.Current.TryFindResource("EnsureyouhaveenoughspaceintheHDD") ?? "Ensure you have enough space in the HDD to extract the file.";
        var grantSimpleLauncheradministrative = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var ensuretheSimpleLauncherfolder = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncherfolder") ?? "Ensure the 'Simple Launcher' folder is a writable directory.";
        var temporarilydisableyourantivirus = (string)Application.Current.TryFindResource("Temporarilydisableyourantivirus") ?? "Temporarily disable your antivirus software and try again.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
            Show();
        else
            Application.Current.Dispatcher.Invoke((Action)Show);
        return;

        void Show()
        {
            MessageBox.Show($"{extractionfailed}\n\n" + $"{ensurethefileisnotcorrupted}\n" + $"{ensureyouhaveenoughspaceintheHdd}\n" + $"{grantSimpleLauncheradministrative}\n" + $"{ensuretheSimpleLauncherfolder}\n" + $"{temporarilydisableyourantivirus}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FileNeedToBeCompressedMessageBox()
    {
        var theselectedfilecannotbe = (string)Application.Current.TryFindResource("Theselectedfilecannotbe") ??
                                      "The selected file cannot be extracted.";
        var toextractafileitneedstobe = (string)Application.Current.TryFindResource("Toextractafileitneedstobe") ??
                                        "To extract a file, it needs to be a 7z, zip, or rar file.";
        var pleasefixthatintheEditwindow =
            (string)Application.Current.TryFindResource("PleasefixthatintheEditwindow") ??
            "Please fix that in the Edit window.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";

        if (Application.Current.Dispatcher.CheckAccess())
            Show();
        else
            Application.Current.Dispatcher.Invoke((Action)Show);
        return;

        void Show()
        {
            MessageBox.Show($"{theselectedfilecannotbe}\n\n" + $"{toextractafileitneedstobe}\n\n" + $"{pleasefixthatintheEditwindow}", warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void DownloadedFileIsMissingMessageBox()
    {
        var downloadedfileismissing = (string)Application.Current.TryFindResource("Downloadedfileismissing") ??
                                      "Downloaded file is missing.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
            Show();
        else
            Application.Current.Dispatcher.Invoke((Action)Show);
        return;

        void Show()
        {
            MessageBox.Show($"{downloadedfileismissing}\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FileIsLockedMessageBox()
    {
        var downloadedfileislocked = (string)Application.Current.TryFindResource("Downloadedfileislocked") ??
                                     "Downloaded file is locked.";
        var grantSimpleLauncheradministrative =
            (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ??
            "Grant 'Simple Launcher' administrative access and try again.";
        var temporarilydisableyourantivirussoftware =
            (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ??
            "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var ensuretheSimpleLauncher = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncher") ??
                                      "Ensure the 'Simple Launcher' folder is a writable directory.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
            Show();
        else
            Application.Current.Dispatcher.Invoke((Action)Show);
        return;

        void Show()
        {
            MessageBox.Show($"{downloadedfileislocked}\n\n" + $"{grantSimpleLauncheradministrative}\n\n" + $"{temporarilydisableyourantivirussoftware}\n\n" + $"{ensuretheSimpleLauncher}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ImagePackDownloadExtractionFailedMessageBox()
    {
        var imagePackdownloadorextraction =
            (string)Application.Current.TryFindResource("ImagePackdownloadorextraction") ??
            "Image Pack download or extraction failed!";
        var grantSimpleLauncheradministrative =
            (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ??
            "Grant 'Simple Launcher' administrative access and try again.";
        var ensuretheSimpleLauncher = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncher") ??
                                      "Ensure the 'Simple Launcher' folder is a writable directory.";
        var temporarilydisableyourantivirussoftware =
            (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ??
            "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";

        if (Application.Current.Dispatcher.CheckAccess())
            Show();
        else
            Application.Current.Dispatcher.Invoke((Action)Show);
        return;

        void Show()
        {
            MessageBox.Show($"{imagePackdownloadorextraction}\n\n" + $"{grantSimpleLauncheradministrative}\n\n" + $"{ensuretheSimpleLauncher}\n\n" + $"{temporarilydisableyourantivirussoftware}", info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void DownloadExtractionSuccessfullyMessageBox(string extractionFolder)
    {
        var theimagepackwassuccessfullyextractedintothefolder = (string)Application.Current.TryFindResource("Theimagepackwassuccessfullyextractedintothefolder") ?? "The image pack was successfully extracted into the folder";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";

        if (Application.Current.Dispatcher.CheckAccess())
            Show();
        else
            Application.Current.Dispatcher.Invoke((Action)Show);
        return;

        void Show()
        {
            MessageBox.Show($"{theimagepackwassuccessfullyextractedintothefolder} {extractionFolder}", info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ImagePackDownloadErrorOfferRedirectMessageBox(EasyModeSystemConfig selectedSystem)
    {
        if (Application.Current.Dispatcher.CheckAccess())
            Show();
        else
            Application.Current.Dispatcher.Invoke((Action)Show);
        return;

        void Show()
        {
            var downloaderror = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
            var wouldyouliketoberedirected = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{downloaderror}\n\n" + $"{wouldyouliketoberedirected}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                if (selectedSystem?.Emulators?.Emulator?.ImagePackDownloadLink == null) return;

                Process.Start(new ProcessStartInfo
                {
                    FileName = selectedSystem.Emulators.Emulator.ImagePackDownloadLink,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error opening the Browser.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
                // Notify user
                var simpleLaunchercouldnotopentheImage = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotopentheImage") ?? "'Simple Launcher' could not open the Image Pack download link.";
                MessageBox.Show(simpleLaunchercouldnotopentheImage, error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void ErrorLoadingEasyModeXmlMessageBox()
    {
        if (Application.Current.Dispatcher.CheckAccess())
            Show();
        else
            Application.Current.Dispatcher.Invoke((Action)Show);
        return;

        static void Show()
        {
            var errorloadingthefileeasymodexml = (string)Application.Current.TryFindResource("Errorloadingthefileeasymodexml") ?? "Error loading the file 'easymode.xml'.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var doyouwanttoreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{errorloadingthefileeasymodexml}\n\n" + $"{theerrorwasreportedtothedeveloper}\n" + $"{doyouwanttoreinstallSimpleLauncher}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                var pleasereinstallSimpleLaunchermanually = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
                MessageBox.Show(pleasereinstallSimpleLaunchermanually, error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void LinksSavedMessageBox()
    {
        var linkssavedsuccessfully = (string)Application.Current.TryFindResource("Linkssavedsuccessfully") ??
                                     "Links saved successfully.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";

        if (Application.Current.Dispatcher.CheckAccess())
            Show();
        else
            Application.Current.Dispatcher.Invoke((Action)Show);
        return;

        void Show()
        {
            MessageBox.Show(linkssavedsuccessfully, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void DeadZonesSavedMessageBox()
    {
        var deadZonevaluessavedsuccessfully =
            (string)Application.Current.TryFindResource("DeadZonevaluessavedsuccessfully") ??
            "DeadZone values saved successfully.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
        {
            MessageBox.Show(deadZonevaluessavedsuccessfully, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            dispatcher.Invoke(() =>
            {
                MessageBox.Show(deadZonevaluessavedsuccessfully, info, MessageBoxButton.OK,
                    MessageBoxImage.Information);
            });
        }
    }

    internal static void LinksRevertedMessageBox()
    {
        var linksreverted = (string)Application.Current.TryFindResource("Linksrevertedtodefaultvalues") ??
                            "Links reverted to default values.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
        {
            MessageBox.Show(linksreverted, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            dispatcher.Invoke(() =>
            {
                MessageBox.Show(linksreverted, info, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
    }

    internal static void MainWindowSearchEngineErrorMessageBox()
    {
        var therewasanerrorwiththesearchengine =
            (string)Application.Current.TryFindResource("Therewasanerrorwiththesearchengine") ??
            "There was an error with the search engine.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
        {
            MessageBox.Show(
                $"{therewasanerrorwiththesearchengine}\n\n{theerrorwasreportedtothedeveloper}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"{therewasanerrorwiththesearchengine}\n\n{theerrorwasreportedtothedeveloper}",
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    internal static void DownloadExtractionFailedMessageBox()
    {
        var downloadorextractionfailed = (string)Application.Current.TryFindResource("DownloadorExtractionFailed") ??
                                         "Download or extraction failed.";
        var grantSimpleLauncheradministrativeaccess =
            (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrativeaccess") ??
            "Grant 'Simple Launcher' administrative access and try again.";
        var ensuretheSimpleLauncherfolder =
            (string)Application.Current.TryFindResource("EnsuretheSimpleLauncherfolder") ??
            "Ensure the 'Simple Launcher' folder is a writable directory.";
        var temporarilydisableyourantivirus =
            (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ??
            "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
        {
            MessageBox.Show(
                $"{downloadorextractionfailed}\n\n{grantSimpleLauncheradministrativeaccess}\n\n{ensuretheSimpleLauncherfolder}\n\n{temporarilydisableyourantivirus}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    $"{downloadorextractionfailed}\n\n{grantSimpleLauncheradministrativeaccess}\n\n{ensuretheSimpleLauncherfolder}\n\n{temporarilydisableyourantivirus}",
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    internal static void DownloadAndExtrationWereSuccessfulMessageBox()
    {
        var downloadingandextractionweresuccessful =
            (string)Application.Current.TryFindResource("Downloadingandextractionweresuccessful") ??
            "Downloading and extraction were successful.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
        {
            MessageBox.Show(downloadingandextractionweresuccessful, info, MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        else
        {
            dispatcher.Invoke(() =>
            {
                MessageBox.Show(downloadingandextractionweresuccessful, info, MessageBoxButton.OK,
                    MessageBoxImage.Information);
            });
        }
    }

    internal static async Task EmulatorDownloadErrorMessageBox(EasyModeSystemConfig selectedSystem)
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
        {
            ShowEmulatorDownloadErrorBox(selectedSystem);
        }
        else
        {
            await dispatcher.InvokeAsync(() => ShowEmulatorDownloadErrorBox(selectedSystem));
        }
    }

    private static void ShowEmulatorDownloadErrorBox(EasyModeSystemConfig selectedSystem)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            Operation();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)Operation);
        }

        return;

        void Operation()
        {
            var downloaderror = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
            var wouldyouliketoberedirected = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = MessageBox.Show($"{downloaderror}\n\n{wouldyouliketoberedirected}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo { FileName = selectedSystem.Emulators.Emulator.EmulatorDownloadLink, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                const string contextMessage = "Error opening the download link.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage); // Assuming LogErrorAsync handles its own threading

                var erroropeningthedownloadlink = (string)Application.Current.TryFindResource("Erroropeningthedownloadlink") ?? "Error opening the download link.";
                var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";

                MessageBox.Show($"{erroropeningthedownloadlink}\n\n{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static async Task CoreDownloadErrorMessageBox(EasyModeSystemConfig selectedSystem)
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
        {
            ShowCoreDownloadErrorBox(selectedSystem);
        }
        else
        {
            await dispatcher.InvokeAsync(() => ShowCoreDownloadErrorBox(selectedSystem));
        }
    }

    private static void ShowCoreDownloadErrorBox(EasyModeSystemConfig selectedSystem)
    {
        var downloaderror = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
        var wouldyouliketoberedirected = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ??
                                         "Would you like to be redirected to the download page?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = MessageBoxResult.None;
        if (Application.Current.Dispatcher.CheckAccess())
        {
            result = MessageBox.Show($"{downloaderror}\n\n{wouldyouliketoberedirected}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Question);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                result = MessageBox.Show($"{downloaderror}\n\n{wouldyouliketoberedirected}",
                    error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            });
        }

        if (result != MessageBoxResult.Yes)
        {
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = selectedSystem.Emulators.Emulator.CoreDownloadLink,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            const string contextMessage = "Error opening the download link.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage); // Assuming LogErrorAsync can be called from any thread
            var erroropeningthedownloadlink =
                (string)Application.Current.TryFindResource("Erroropeningthedownloadlink") ??
                "Error opening the download link.";
            var theerrorwasreportedtothedeveloper =
                (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
                "The error was reported to the developer who will try to fix the issue.";

            if (Application.Current.Dispatcher.CheckAccess())
            {
                MessageBox.Show($"{erroropeningthedownloadlink}\n\n{theerrorwasreportedtothedeveloper}",
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    MessageBox.Show($"{erroropeningthedownloadlink}\n\n{theerrorwasreportedtothedeveloper}",
                        error, MessageBoxButton.OK, MessageBoxImage.Error);
                });
            }
        }
    }

    internal static void SelectAHistoryItemToRemoveMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("SelectAHistoryItemToRemove") ?? "Please select a history item to remove.";
        var pleaseselectaitem = (string)Application.Current.TryFindResource("Pleaseselectaitem") ?? "Please select a item";
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
        {
            MessageBox.Show(message, pleaseselectaitem, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, pleaseselectaitem, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
    }

    internal static MessageBoxResult ReallyWantToRemoveAllPlayHistoryMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
        {
            var message = (string)Application.Current.TryFindResource("AreYouSureYouWantToRemoveAllHistory") ??
                          "Are you sure you want to remove all play history?";
            var confirmation = (string)Application.Current.TryFindResource("Confirmation") ?? "Confirmation";
            return MessageBox.Show(message, confirmation, MessageBoxButton.YesNo, MessageBoxImage.Question);
        }
        else
        {
            return dispatcher.Invoke(static () =>
            {
                var message = (string)Application.Current.TryFindResource("AreYouSureYouWantToRemoveAllHistory") ??
                              "Are you sure you want to remove all play history?";
                var confirmation = (string)Application.Current.TryFindResource("Confirmation") ?? "Confirmation";
                return MessageBox.Show(message, confirmation, MessageBoxButton.YesNo, MessageBoxImage.Question);
            });
        }
    }

    internal static async Task ImagePackDownloadErrorMessageBox(EasyModeSystemConfig selectedSystem)
    {
        if (selectedSystem?.Emulators?.Emulator?.ImagePackDownloadLink == null)
            return;

        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
        {
            ShowImagePackDownloadErrorBox(selectedSystem);
        }
        else
        {
            await dispatcher.InvokeAsync(() => ShowImagePackDownloadErrorBox(selectedSystem));
        }
    }

    private static void ShowImagePackDownloadErrorBox(EasyModeSystemConfig selectedSystem)
    {
        var downloadError = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
        var wouldYouLikeToBeRedirected = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ??
                                         "Would you like to be redirected to the download page?";
        var errorCaption = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            Action();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)Action);
        }

        return;

        void Action()
        {
            var result = MessageBox.Show($"{downloadError}\n\n{wouldYouLikeToBeRedirected}", errorCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo { FileName = selectedSystem.Emulators.Emulator.ImagePackDownloadLink, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                const string contextMessage = "Error opening the download link.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
                var errorOpeningDownloadLink = (string)Application.Current.TryFindResource("Erroropeningthedownloadlink") ?? "Error opening the download link.";
                var errorWasReported = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
                MessageBox.Show($"{errorOpeningDownloadLink}\n\n{errorWasReported}", errorCaption, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void SystemAddedMessageBox(string systemFolder, string fullImageFolderPathForMessage,
        EasyModeSystemConfig selectedSystem)
    {
        var thesystem = (string)Application.Current.TryFindResource("Thesystem") ?? "The system";
        var hasbeenaddedsuccessfully = (string)Application.Current.TryFindResource("hasbeenaddedsuccessfully") ??
                                       "has been added successfully.";
        var putRoMsorIsOsforthissysteminside =
            (string)Application.Current.TryFindResource("PutROMsorISOsforthissysteminside") ??
            "Put ROMs or ISOs for this system inside";
        var putcoverimagesforthissysteminside =
            (string)Application.Current.TryFindResource("Putcoverimagesforthissysteminside") ??
            "Put cover images for this system inside";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            Action();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)Action);
        }

        return;

        void Action()
        {
            MessageBox.Show($"{thesystem} '{selectedSystem.SystemName}' {hasbeenaddedsuccessfully}\n\n" + $"{putRoMsorIsOsforthissysteminside} '{systemFolder}'\n\n" + $"{putcoverimagesforthissysteminside} '{fullImageFolderPathForMessage}'.", info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void AddSystemFailedMessageBox(string details = null)
    {
        var therewasanerroradding = (string)Application.Current.TryFindResource("Therewasanerroradding") ?? "There was an error adding this system.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var errorDetails = (string)Application.Current.TryFindResource("ErrorDetails") ?? "Details:";

        var message = $"{therewasanerroradding}\n\n{theerrorwasreportedtothedeveloper}";
        if (!string.IsNullOrEmpty(details))
        {
            message += $"\n\n{errorDetails} {details}";
        }

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show(message, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, error, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    internal static void RightClickContextMenuErrorMessageBox()
    {
        var therewasanerrorintherightclick =
            (string)Application.Current.TryFindResource("Therewasanerrorintherightclick") ??
            "There was an error in the right-click context menu.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            Action();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)Action);
        }

        return;

        void Action()
        {
            MessageBox.Show($"{therewasanerrorintherightclick}\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void GameFileDoesNotExistMessageBox()
    {
        var thegamefiledoesnotexist = (string)Application.Current.TryFindResource("Thegamefiledoesnotexist") ??
                                      "The game file does not exist!";
        var thefavoritehasbeenremovedfromthelist =
            (string)Application.Current.TryFindResource("Thefavoritehasbeenremovedfromthelist") ??
            "The favorite has been removed from the list.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            Action();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)Action);
        }

        return;

        void Action()
        {
            MessageBox.Show($"{thegamefiledoesnotexist}\n\n" + $"{thefavoritehasbeenremovedfromthelist}", info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void CouldNotOpenHistoryWindowMessageBox()
    {
        var therewasaproblemopeningtheHistorywindow =
            (string)Application.Current.TryFindResource("TherewasaproblemopeningtheHistorywindow") ??
            "There was a problem opening the History window.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            Action();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)Action);
        }

        return;

        void Action()
        {
            MessageBox.Show($"{therewasaproblemopeningtheHistorywindow}\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorOpeningCoverImageMessageBox()
    {
        var therewasanerrortryingtoopenthe =
            (string)Application.Current.TryFindResource("Therewasanerrortryingtoopenthe") ?? "There was an error trying to open the cover image.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            Action();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)Action);
        }

        return;

        void Action()
        {
            MessageBox.Show($"{therewasanerrortryingtoopenthe}\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void CouldNotOpenWalkthroughMessageBox()
    {
        var failedtoopenthewalkthroughfile =
            (string)Application.Current.TryFindResource("Failedtoopenthewalkthroughfile") ??
            "Failed to open the walkthrough file.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            Action();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)Action);
        }

        return;

        void Action()
        {
            MessageBox.Show($"{failedtoopenthewalkthroughfile}\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SelectAFavoriteToRemoveMessageBox()
    {
        var pleaseselectafavoritetoremove =
            (string)Application.Current.TryFindResource("Pleaseselectafavoritetoremove") ??
            "Please select a favorite to remove.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            Action();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)Action);
        }

        return;

        void Action()
        {
            MessageBox.Show(pleaseselectafavoritetoremove, warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void SystemXmlNotFoundMessageBox()
    {
        var systemxmlnotfound = (string)Application.Current.TryFindResource("systemxmlnotfound") ??
                                "'system.xml' not found inside the application folder.";
        var pleaserestartSimpleLauncher = (string)Application.Current.TryFindResource("PleaserestartSimpleLauncher") ??
                                          "Please restart 'Simple Launcher'.";
        var ifthatdoesnotwork = (string)Application.Current.TryFindResource("Ifthatdoesnotwork") ??
                                "If that does not work, please reinstall 'Simple Launcher'.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            Action();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)Action);
        }

        return;

        void Action()
        {
            MessageBox.Show($"{systemxmlnotfound}\n\n" + $"{pleaserestartSimpleLauncher}\n\n" + $"{ifthatdoesnotwork}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void YouCanAddANewSystemMessageBox()
    {
        var youcanaddanewsystem = (string)Application.Current.TryFindResource("Youcanaddanewsystem") ??
                                  "You can add a new system now.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show(youcanaddanewsystem,
                info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(youcanaddanewsystem,
                    info, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
    }

    internal static void EmulatorNameRequiredMessageBox(int i)
    {
        var emulator = (string)Application.Current.TryFindResource("Emulator") ?? "Emulator";
        var nameisrequiredbecauserelateddata = (string)Application.Current.TryFindResource("nameisrequiredbecauserelateddata") ?? "name is required because related data has been provided.";
        var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show($"{emulator} {i + 2} {nameisrequiredbecauserelateddata}\n\n" +
                            $"{pleasefixthisfield}",
                info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{emulator} {i + 2} {nameisrequiredbecauserelateddata}\n\n" +
                                $"{pleasefixthisfield}",
                    info, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
    }

    internal static void EmulatorNameIsRequiredMessageBox()
    {
        var emulatornameisrequired = (string)Application.Current.TryFindResource("Emulatornameisrequired") ?? "Emulator name is required.";
        var pleasefixthat = (string)Application.Current.TryFindResource("Pleasefixthat") ?? "Please fix that.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show($"{emulatornameisrequired}\n\n" +
                            $"{pleasefixthat}",
                error, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{emulatornameisrequired}\n\n" +
                                $"{pleasefixthat}",
                    error, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
    }

    internal static void EmulatorNameMustBeUniqueMessageBox(string emulatorName)
    {
        var thename = (string)Application.Current.TryFindResource("Thename") ?? "The name";
        var isusedmultipletimes = (string)Application.Current.TryFindResource("isusedmultipletimes") ??
                                  "is used multiple times. Each emulator name must be unique.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show($"{thename} '{emulatorName}' {isusedmultipletimes}",
                info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{thename} '{emulatorName}' {isusedmultipletimes}",
                    info, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
    }

    internal static void SystemSavedSuccessfullyMessageBox()
    {
        var systemsavedsuccessfully = (string)Application.Current.TryFindResource("Systemsavedsuccessfully") ??
                                      "System saved successfully.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show(systemsavedsuccessfully,
                info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(systemsavedsuccessfully,
                    info, MessageBoxButton.OK, MessageBoxImage.Information);
            });
        }
    }

    internal static void PathOrParameterInvalidMessageBox()
    {
        var oneormorepathsorparameters = (string)Application.Current.TryFindResource("Oneormorepathsorparameters") ??
                                         "One or more paths or parameters are invalid.";
        var pleasefixthemtoproceed = (string)Application.Current.TryFindResource("Pleasefixthemtoproceed") ??
                                     "Please fix them to proceed.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show($"{oneormorepathsorparameters}\n\n" +
                            $"{pleasefixthemtoproceed}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{oneormorepathsorparameters}\n\n" +
                                $"{pleasefixthemtoproceed}",
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    internal static void Emulator1RequiredMessageBox()
    {
        var emulator1Nameisrequired = (string)Application.Current.TryFindResource("Emulator1Nameisrequired") ??
                                      "'Emulator 1 Name' is required.";
        var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ??
                                 "Please fix this field.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show($"{emulator1Nameisrequired}\n\n" +
                            $"{pleasefixthisfield}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{emulator1Nameisrequired}\n\n" +
                                $"{pleasefixthisfield}",
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    internal static void ExtensionToLaunchIsRequiredMessageBox()
    {
        var extensiontoLaunchAfterExtraction =
            (string)Application.Current.TryFindResource("ExtensiontoLaunchAfterExtraction") ??
            "'Extension to Launch After Extraction' is required when 'Extract File Before Launch' is set to true.";
        var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ??
                                 "Please fix this field.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show($"{extensiontoLaunchAfterExtraction}\n\n" +
                            $"{pleasefixthisfield}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{extensiontoLaunchAfterExtraction}\n\n" +
                                $"{pleasefixthisfield}",
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    internal static void ExtensionToSearchIsRequiredMessageBox()
    {
        var extensiontoSearchintheSystemFolder =
            (string)Application.Current.TryFindResource("ExtensiontoSearchintheSystemFolder") ??
            "'Extension to Search in the System Folder' cannot be empty or contain only spaces.";
        var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ??
                                 "Please fix this field.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show($"{extensiontoSearchintheSystemFolder}\n\n" +
                            $"{pleasefixthisfield}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{extensiontoSearchintheSystemFolder}\n\n" +
                                $"{pleasefixthisfield}",
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    internal static void FileMustBeCompressedMessageBox()
    {
        var whenExtractFileBeforeLaunch = (string)Application.Current.TryFindResource("WhenExtractFileBeforeLaunch") ??
                                          "When 'Extract File Before Launch' is set to true, 'Extension to Search in the System Folder' must include 'zip', '7z', or 'rar'.";
        var itwillnotacceptotherextensions =
            (string)Application.Current.TryFindResource("Itwillnotacceptotherextensions") ??
            "It will not accept other extensions.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show($"{whenExtractFileBeforeLaunch}\n\n{itwillnotacceptotherextensions}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{whenExtractFileBeforeLaunch}\n\n{itwillnotacceptotherextensions}",
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    internal static void SystemImageFolderCanNotBeEmptyMessageBox()
    {
        var systemImageFoldercannotbeempty =
            (string)Application.Current.TryFindResource("SystemImageFoldercannotbeempty") ??
            "'System Image Folder' cannot be empty or contain only spaces.";
        var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ??
                                 "Please fix this field.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show($"{systemImageFoldercannotbeempty}\n\n{pleasefixthisfield}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{systemImageFoldercannotbeempty}\n\n{pleasefixthisfield}",
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    internal static void SystemFolderCanNotBeEmptyMessageBox()
    {
        var systemFoldercannotbeempty = (string)Application.Current.TryFindResource("SystemFoldercannotbeempty") ??
                                        "'System Folder' cannot be empty or contain only spaces.";
        var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ??
                                 "Please fix this field.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke(ShowMessage);
        }

        return;

        void ShowMessage()
        {
            MessageBox.Show($"{systemFoldercannotbeempty}\n\n{pleasefixthisfield}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SystemNameCanNotBeEmptyMessageBox()
    {
        var systemNamecannotbeemptyor = (string)Application.Current.TryFindResource("SystemNamecannotbeemptyor") ??
                                        "'System Name' cannot be empty or contain only spaces.";
        var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ??
                                 "Please fix this field.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke(ShowMessage);
        }

        return;

        void ShowMessage()
        {
            MessageBox.Show($"{systemNamecannotbeemptyor}\n\n" +
                            $"{pleasefixthisfield}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FolderCreatedMessageBox(string systemNameText)
    {
        var simpleLaunchercreatedaimagefolder = (string)Application.Current.TryFindResource("SimpleLaunchercreatedaimagefolder") ?? "'Simple Launcher' created a image folder for this system at";
        var youmayplacethecoverimagesforthissystem = (string)Application.Current.TryFindResource("Youmayplacethecoverimagesforthissysteminside") ?? "You may place the cover images for this system inside this folder.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke(ShowMessage);
        }

        return;

        void ShowMessage()
        {
            MessageBox.Show($"{simpleLaunchercreatedaimagefolder} '.\\images\\{systemNameText}'.\n\n" +
                            $"{youmayplacethecoverimagesforthissystem}\n\n",
                info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void FolderCreationFailedMessageBox()
    {
        var simpleLauncherfailedtocreatethe =
            (string)Application.Current.TryFindResource("SimpleLauncherfailedtocreatethe") ??
            "'Simple Launcher' failed to create the necessary folders for this system.";
        var grantSimpleLauncheradministrative =
            (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ??
            "Grant 'Simple Launcher' administrative access and try again.";
        var temporarilydisableyourantivirus =
            (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ??
            "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var ensurethattheSimpleLauncherfolderislocatedinawritable =
            (string)Application.Current.TryFindResource("EnsurethattheSimpleLauncherfolderislocatedinawritable") ??
            "Ensure that the 'Simple Launcher' folder is located in a writable directory.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke(ShowMessage);
        }

        return;

        void ShowMessage()
        {
            MessageBox.Show($"{simpleLauncherfailedtocreatethe}\n\n" +
                            $"{grantSimpleLauncheradministrative}\n\n" +
                            $"{temporarilydisableyourantivirus}\n\n" +
                            $"{ensurethattheSimpleLauncherfolderislocatedinawritable}",
                info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void SelectASystemToDeleteMessageBox()
    {
        var pleaseselectasystemtodelete = (string)Application.Current.TryFindResource("Pleaseselectasystemtodelete") ??
                                          "Please select a system to delete.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke(ShowMessage);
        }

        return;

        void ShowMessage()
        {
            MessageBox.Show(pleaseselectasystemtodelete,
                warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void SystemNotFoundInTheXmlMessageBox()
    {
        var selectedsystemnotfound = (string)Application.Current.TryFindResource("Selectedsystemnotfound") ??
                                     "Selected system not found in the XML document!";
        var alert = (string)Application.Current.TryFindResource("Alert") ?? "Alert";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke(ShowMessage);
        }

        return;

        void ShowMessage()
        {
            MessageBox.Show(selectedsystemnotfound,
                alert, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }

    internal static void ErrorFindingGameFilesMessageBox(string logPath)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke(ShowMessage);
        }

        return;

        void ShowMessage()
        {
            var therewasanerrorfinding = (string)Application.Current.TryFindResource("Therewasanerrorfinding") ??
                                         "There was an error finding the game files.";
            var doyouwanttoopenthefileerroruserlog =
                (string)Application.Current.TryFindResource("Doyouwanttoopenthefileerroruserlog") ??
                "Do you want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{therewasanerrorfinding}\n\n" +
                                         $"{doyouwanttoopenthefileerroruserlog}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                // Notify user
                var thefileerroruserlogwas = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ??
                                             "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlogwas,
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void ErrorWhileCountingFilesMessageBox(string logPath)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke(ShowMessage);
        }

        return;

        void ShowMessage()
        {
            var anerroroccurredwhilecounting =
                (string)Application.Current.TryFindResource("Anerroroccurredwhilecounting") ??
                "An error occurred while counting files.";
            var doyouwanttoopenthefileerroruserlog =
                (string)Application.Current.TryFindResource("Doyouwanttoopenthefileerroruserlog") ??
                "Do you want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{anerroroccurredwhilecounting}\n\n" +
                                         $"{doyouwanttoopenthefileerroruserlog}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                var thefileerroruserlog = (string)Application.Current.TryFindResource("Thefileerroruserlog") ??
                                          "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlog,
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void GamePadErrorMessageBox(string logPath)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke(ShowMessage);
        }

        return;

        void ShowMessage()
        {
            var therewasanerrorwiththeGamePadController =
                (string)Application.Current.TryFindResource("TherewasanerrorwiththeGamePadController") ??
                "There was an error with the GamePad Controller.";
            var grantSimpleLauncheradministrative =
                (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ??
                "Grant 'Simple Launcher' administrative access and try again.";
            var temporarilydisableyourantivirus =
                (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ??
                "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
            var doyouwanttoopenthefile = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ??
                                         "Do you want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{therewasanerrorwiththeGamePadController}\n\n" +
                                         $"{grantSimpleLauncheradministrative}\n\n" +
                                         $"{temporarilydisableyourantivirus}\n\n" +
                                         $"{doyouwanttoopenthefile}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                var thefileerroruserlogwas = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ??
                                             "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlogwas,
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void CouldNotLaunchGameMessageBox(string logPath)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessage();
        }
        else
        {
            Application.Current.Dispatcher.Invoke(ShowMessage);
        }

        return;

        void ShowMessage()
        {
            var simpleLaunchercouldnotlaunch =
                (string)Application.Current.TryFindResource("SimpleLaunchercouldnotlaunch") ?? "'Simple Launcher' could not launch the selected game.";
            var ifyouaretryingtorunMamEensurethatyourRom = (string)Application.Current.TryFindResource("IfyouaretryingtorunMAMEensurethatyourROM") ?? "If you are trying to run MAME, ensure that your ROM collection is compatible with the MAME version you are using.";
            var ifyouaretryingtorunRetroarchensurethattheBios = (string)Application.Current.TryFindResource("IfyouaretryingtorunRetroarchensurethattheBIOS") ?? "If you are trying to run Retroarch, ensure that the BIOS or required files for the core are installed.";
            var alsomakesureyouarecallingtheemulator = (string)Application.Current.TryFindResource("Alsomakesureyouarecallingtheemulator") ?? "Also, make sure you are calling the emulator with the correct parameter.";
            var youcanturnoffthistypeoferrormessageinExpertmode = (string)Application.Current.TryFindResource("YoucanturnoffthistypeoferrormessageinExpertmode") ?? "You can turn off this type of error message in Expert mode.";
            var doyouwanttoopenthefile = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show(
                $"{simpleLaunchercouldnotlaunch}\n\n" +
                $"{ifyouaretryingtorunMamEensurethatyourRom}\n" +
                $"{ifyouaretryingtorunRetroarchensurethattheBios}\n" +
                $"{alsomakesureyouarecallingtheemulator}\n\n" +
                $"{youcanturnoffthistypeoferrormessageinExpertmode}\n\n" +
                $"{doyouwanttoopenthefile}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                var thefileerroruserlogwas = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ??
                                             "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlogwas,
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void InvalidOperationExceptionMessageBox(string logPath)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessageBox();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }

        return;

        void ShowMessageBox()
        {
            var failedtostarttheemulator = (string)Application.Current.TryFindResource("Failedtostarttheemulator") ?? "Failed to start the emulator or it has not exited as expected.";
            var checktheintegrityoftheemulatoranditsdependencies = (string)Application.Current.TryFindResource("Checktheintegrityoftheemulatoranditsdependencies") ?? "Check the integrity of the emulator and its dependencies.";
            var youcanturnoffthistypeoferrormessageinExpertmode = (string)Application.Current.TryFindResource("YoucanturnoffthistypeoferrormessageinExpertmode") ?? "You can turn off this type of error message in Expert mode.";
            var doyouwanttoopenthefile = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = MessageBox.Show($"{failedtostarttheemulator}\n\n" +
                                         $"{checktheintegrityoftheemulatoranditsdependencies}\n\n" +
                                         $"{youcanturnoffthistypeoferrormessageinExpertmode}\n\n" +
                                         $"{doyouwanttoopenthefile}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                var thefileerroruserlog = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlog,
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void ThereWasAnErrorLaunchingThisGameMessageBox(string logPath)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessageBox();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }

        return;

        void ShowMessageBox()
        {
            var therewasanerrorlaunchingthisgame = (string)Application.Current.TryFindResource("Therewasanerrorlaunchingthisgame") ?? "There was an error launching this game.";
            var youcanturnoffthistypeoferrormessageinExpertmode = (string)Application.Current.TryFindResource("YoucanturnoffthistypeoferrormessageinExpertmode") ?? "You can turn off this type of error message in Expert mode.";
            var doyouwanttoopenthefileerroruserlog = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{therewasanerrorlaunchingthisgame}\n\n" +
                                         $"{youcanturnoffthistypeoferrormessageinExpertmode}\n\n" +
                                         $"{doyouwanttoopenthefileerroruserlog}",
                error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                Process.Start(new ProcessStartInfo { FileName = logPath, UseShellExecute = true });
            }
            catch (Exception)
            {
                var thefileerroruserlog = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlog, error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void CannotExtractThisFileMessageBox(string filePath)
    {
        var theselectedfile = (string)Application.Current.TryFindResource("Theselectedfile") ?? "The selected file";
        var cannotbeextracted =
            (string)Application.Current.TryFindResource("cannotbeextracted") ?? "can not be extracted.";
        var toextractafileitneedstobe = (string)Application.Current.TryFindResource("Toextractafileitneedstobe") ??
                                        "To extract a file, it needs to be a 7z, zip, or rar file.";
        var pleasegotoEditSystem = (string)Application.Current.TryFindResource("PleasegotoEditSystem") ??
                                   "Please go to Edit System - Expert Mode and edit this system.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessageBox();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }

        return;

        void ShowMessageBox()
        {
            MessageBox.Show($"{theselectedfile} '{filePath}' {cannotbeextracted}\n\n" + $"{toextractafileitneedstobe}\n\n" + $"{pleasegotoEditSystem}", warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void EmulatorCouldNotOpenXboxXblaSimpleMessageBox(string logPath)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessageBox();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }

        return;

        void ShowMessageBox()
        {
            var theemulatorcouldnotopenthegame = (string)Application.Current.TryFindResource("Theemulatorcouldnotopenthegame") ?? "The emulator could not open the game with the provided parameters.";
            var doyouwanttoopenthefileerror = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = MessageBox.Show($"{theemulatorcouldnotopenthegame}\n\n" + $"{doyouwanttoopenthefileerror}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                Process.Start(new ProcessStartInfo { FileName = logPath, UseShellExecute = true });
            }
            catch (Exception)
            {
                var thefileerroruser = (string)Application.Current.TryFindResource("Thefileerroruser") ?? "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruser, error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void NullFileExtensionMessageBox()
    {
        var thereisnoExtension = (string)Application.Current.TryFindResource("ThereisnoExtension") ?? "There is no 'Extension to Launch After Extraction' set in the system configuration.";
        var pleaseeditthissystemto = (string)Application.Current.TryFindResource("Pleaseeditthissystemto") ?? "Please edit this system to fix that.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessageBox();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }

        return;

        void ShowMessageBox()
        {
            MessageBox.Show($"{thereisnoExtension}\n\n" + $"{pleaseeditthissystemto}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void CouldNotFindAFileMessageBox()
    {
        var couldnotfindafilewiththeextensiondefined =
            (string)Application.Current.TryFindResource("Couldnotfindafilewiththeextensiondefined") ?? "Could not find a file with the extension defined in 'Extension to Launch After Extraction' inside the extracted folder.";
        var pleaseeditthissystemtofix = (string)Application.Current.TryFindResource("Pleaseeditthissystemto") ?? "Please edit this system to fix that.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessageBox();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }

        return;

        void ShowMessageBox()
        {
            MessageBox.Show($"{couldnotfindafilewiththeextensiondefined}\n\n" + $"{pleaseeditthissystemtofix}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static MessageBoxResult SearchOnlineForRomHistoryMessageBox()
    {
        static MessageBoxResult ShowMessageBox()
        {
            var thereisnoRoMhistoryinthelocaldatabase = (string)Application.Current.TryFindResource("ThereisnoROMhistoryinthelocaldatabase") ?? "There is no ROM history in the local database for this file.";
            var doyouwanttosearchonline = (string)Application.Current.TryFindResource("Doyouwanttosearchonline") ?? "Do you want to search online for the ROM history?";
            var rOmHistoryNotFound = (string)Application.Current.TryFindResource("ROMHistorynotfound") ?? "ROM History not found";
            var result = MessageBox.Show($"{thereisnoRoMhistoryinthelocaldatabase}\n\n" + $"{doyouwanttosearchonline}", rOmHistoryNotFound, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result;
        }

        if (Application.Current.Dispatcher.CheckAccess())
        {
            return ShowMessageBox();
        }
        else
        {
            return Application.Current.Dispatcher.Invoke((Func<MessageBoxResult>)ShowMessageBox);
        }
    }

    internal static void SystemHasBeenDeletedMessageBox(string selectedSystemName)
    {
        var system = (string)Application.Current.TryFindResource("System") ?? "System";
        var hasbeendeleted = (string)Application.Current.TryFindResource("hasbeendeleted") ?? "has been deleted.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessageBox();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }

        return;

        void ShowMessageBox()
        {
            MessageBox.Show($"{system} '{selectedSystemName}' {hasbeendeleted}", info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static MessageBoxResult AreYouSureDoYouWantToDeleteThisSystemMessageBox()
    {
        static MessageBoxResult ShowMessageBox()
        {
            var areyousureyouwanttodeletethis = (string)Application.Current.TryFindResource("Areyousureyouwanttodeletethis") ?? "Are you sure you want to delete this system?";
            var confirmation = (string)Application.Current.TryFindResource("Confirmation") ?? "Confirmation";
            var result = MessageBox.Show(areyousureyouwanttodeletethis, confirmation, MessageBoxButton.YesNo, MessageBoxImage.Question);

            return result;
        }

        if (Application.Current.Dispatcher.CheckAccess())
        {
            return ShowMessageBox();
        }
        else
        {
            return Application.Current.Dispatcher.Invoke((Func<MessageBoxResult>)ShowMessageBox);
        }
    }

    internal static void ThereWasAnErrorDeletingTheFileMessageBox()
    {
        var therewasanerrordeletingthefile =
            (string)Application.Current.TryFindResource("Therewasanerrordeletingthefile") ??
            "There was an error deleting the file.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessageBox();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }

        return;

        void ShowMessageBox()
        {
            MessageBox.Show($"{therewasanerrordeletingthefile}\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static MessageBoxResult AreYouSureYouWantToDeleteTheFileMessageBox(string fileNameWithExtension)
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            var areyousureyouwanttodeletethefile =
                (string)Application.Current.TryFindResource("Areyousureyouwanttodeletethefile") ??
                "Are you sure you want to delete the file";
            var thisactionwilldelete = (string)Application.Current.TryFindResource("Thisactionwilldelete") ??
                                       "This action will delete the file from the HDD and cannot be undone.";
            var confirmDeletion = (string)Application.Current.TryFindResource("ConfirmDeletion") ?? "Confirm Deletion";
            var result = MessageBox.Show($"{areyousureyouwanttodeletethefile} '{fileNameWithExtension}'?\n\n" +
                                         $"{thisactionwilldelete}",
                confirmDeletion, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result;
        }
        else
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                var areyousureyouwanttodeletethefile =
                    (string)Application.Current.TryFindResource("Areyousureyouwanttodeletethefile") ??
                    "Are you sure you want to delete the file";
                var thisactionwilldelete = (string)Application.Current.TryFindResource("Thisactionwilldelete") ??
                                           "This action will delete the file from the HDD and cannot be undone.";
                var confirmDeletion = (string)Application.Current.TryFindResource("ConfirmDeletion") ??
                                      "Confirm Deletion";
                var result = MessageBox.Show($"{areyousureyouwanttodeletethefile} '{fileNameWithExtension}'?\n\n" +
                                             $"{thisactionwilldelete}",
                    confirmDeletion, MessageBoxButton.YesNo, MessageBoxImage.Question);
                return result;
            });
        }
    }

    internal static MessageBoxResult WoulYouLikeToSaveAReportMessageBox()
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            var wouldyouliketosaveareport = (string)Application.Current.TryFindResource("Wouldyouliketosaveareport") ??
                                            "Would you like to save a report with the results?";
            var saveReport = (string)Application.Current.TryFindResource("SaveReport") ?? "Save Report";
            var result = MessageBox.Show(wouldyouliketosaveareport,
                saveReport, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return result;
        }
        else
        {
            return Application.Current.Dispatcher.Invoke(() =>
            {
                var wouldyouliketosaveareport =
                    (string)Application.Current.TryFindResource("Wouldyouliketosaveareport") ??
                    "Would you like to save a report with the results?";
                var saveReport = (string)Application.Current.TryFindResource("SaveReport") ?? "Save Report";
                var result = MessageBox.Show(wouldyouliketosaveareport,
                    saveReport, MessageBoxButton.YesNo, MessageBoxImage.Question);
                return result;
            });
        }
    }

    internal static void SimpleLauncherWasUnableToRestoreBackupMessageBox()
    {
        var simpleLauncherwasunabletorestore =
            (string)Application.Current.TryFindResource("SimpleLauncherwasunabletorestore") ??
            "'Simple Launcher' was unable to restore the last backup.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show(simpleLauncherwasunabletorestore,
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(simpleLauncherwasunabletorestore,
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    internal static MessageBoxResult WouldYouLikeToRestoreTheLastBackupMessageBox()
    {
        if (Application.Current.Dispatcher.CheckAccess())
        {
            var icouldnotfindthefilesystemxml =
                (string)Application.Current.TryFindResource("Icouldnotfindthefilesystemxml") ??
                "I could not find the file 'system.xml', which is required to start the application.";
            var butIfoundabackupfile = (string)Application.Current.TryFindResource("ButIfoundabackupfile") ??
                                       "But I found a backup file.";
            var wouldyouliketorestore = (string)Application.Current.TryFindResource("Wouldyouliketorestore") ??
                                        "Would you like to restore the last backup?";
            var restoreBackup = (string)Application.Current.TryFindResource("RestoreBackup") ?? "Restore Backup?";
            return MessageBox.Show($"{icouldnotfindthefilesystemxml}\n\n" +
                                   $"{butIfoundabackupfile}\n\n" +
                                   $"{wouldyouliketorestore}",
                restoreBackup, MessageBoxButton.YesNo, MessageBoxImage.Question);
        }
        else
        {
            return Application.Current.Dispatcher.Invoke(static () =>
            {
                var icouldnotfindthefilesystemxml =
                    (string)Application.Current.TryFindResource("Icouldnotfindthefilesystemxml") ??
                    "I could not find the file 'system.xml', which is required to start the application.";
                var butIfoundabackupfile = (string)Application.Current.TryFindResource("ButIfoundabackupfile") ??
                                           "But I found a backup file.";
                var wouldyouliketorestore = (string)Application.Current.TryFindResource("Wouldyouliketorestore") ??
                                            "Would you like to restore the last backup?";
                var restoreBackup = (string)Application.Current.TryFindResource("RestoreBackup") ?? "Restore Backup?";
                return MessageBox.Show($"{icouldnotfindthefilesystemxml}\n\n" +
                                       $"{butIfoundabackupfile}\n\n" +
                                       $"{wouldyouliketorestore}",
                    restoreBackup, MessageBoxButton.YesNo, MessageBoxImage.Question);
            });
        }
    }

    internal static void FailedToLoadLanguageResourceMessageBox()
    {
        var failedtoloadlanguageresources =
            (string)Application.Current.TryFindResource("Failedtoloadlanguageresources") ??
            "Failed to load language resources.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var languageLoadingError = (string)Application.Current.TryFindResource("LanguageLoadingError") ??
                                   "Language Loading Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show($"{failedtoloadlanguageresources}\n\n" +
                            $"{theerrorwasreportedtothedeveloper}",
                languageLoadingError, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{failedtoloadlanguageresources}\n\n" +
                                $"{theerrorwasreportedtothedeveloper}",
                    languageLoadingError, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    internal static void InvalidSystemConfigurationMessageBox(string error)
    {
        var invalidSystemConfiguration = (string)Application.Current.TryFindResource("InvalidSystemConfiguration") ??
                                         "Invalid System Configuration";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show(error,
                invalidSystemConfiguration, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(error,
                    invalidSystemConfiguration, MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }
    }

    internal static void ExtractionFolderCannotBeCreatedMessageBox(Exception ex)
    {
        var cannotcreateoraccesstheextractionfolder = (string)Application.Current.TryFindResource("Cannotcreateoraccesstheextractionfolder") ?? "Cannot create or access the extraction folder";
        var invalidExtractionFolder = (string)Application.Current.TryFindResource("InvalidExtractionFolder") ?? "Invalid Extraction Folder";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show($"{cannotcreateoraccesstheextractionfolder}: {ex.Message}",
                invalidExtractionFolder, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{cannotcreateoraccesstheextractionfolder}: {ex.Message}",
                    invalidExtractionFolder, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    internal static void DownloadUrlIsNullMessageBox()
    {
        var theselectedsystemdoesnothaveavaliddownloadlink =
            (string)Application.Current.TryFindResource("Theselectedsystemdoesnothaveavaliddownloadlink") ??
            "The selected system does not have a valid download link.";
        var invalidDownloadLink = (string)Application.Current.TryFindResource("InvalidDownloadLink") ??
                                  "Invalid Download Link";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show(theselectedsystemdoesnothaveavaliddownloadlink,
                invalidDownloadLink, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(theselectedsystemdoesnothaveavaliddownloadlink,
                    invalidDownloadLink, MessageBoxButton.OK, MessageBoxImage.Warning);
            });
        }
    }

    internal static void UnableToOpenLinkMessageBox()
    {
        var unabletoopenthelink = (string)Application.Current.TryFindResource("Unabletoopenthelink") ??
                                  "Unable to open the link.";
        var theerrorwasreportedtothedeveloper =
            (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ??
            "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show($"{unabletoopenthelink}\n\n" +
                            $"{theerrorwasreportedtothedeveloper}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show($"{unabletoopenthelink}\n\n" +
                                $"{theerrorwasreportedtothedeveloper}",
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    internal static void SelectedSystemIsNullMessageBox()
    {
        var couldnotfindtheselectedsystemintheconfiguration =
            (string)Application.Current.TryFindResource("Couldnotfindtheselectedsystemintheconfiguration") ??
            "Could not find the selected system in the configuration.";
        var systemNotFound = (string)Application.Current.TryFindResource("SystemNotFound") ?? "System Not Found";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show(couldnotfindtheselectedsystemintheconfiguration,
                systemNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(couldnotfindtheselectedsystemintheconfiguration,
                    systemNotFound, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    internal static void SystemNameIsNullMessageBox()
    {
        var pleaseselectasystemfromthedropdown =
            (string)Application.Current.TryFindResource("Pleaseselectasystemfromthedropdown") ??
            "Please select a system from the dropdown.";
        var selectionRequired =
            (string)Application.Current.TryFindResource("SelectionRequired") ?? "Selection Required";

        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }
        else
        {
            ShowMessageBox();
        }

        return;

        void ShowMessageBox()
        {
            MessageBox.Show(pleaseselectasystemfromthedropdown, selectionRequired, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void NoGameFoundInTheRandomSelectionMessageBox()
    {
        var nogamesfoundtorandomlyselectfrom =
            (string)Application.Current.TryFindResource("Nogamesfoundtorandomlyselectfrom") ??
            "No games found to randomly select from. Please check your system selection.";
        var feelingLucky = (string)Application.Current.TryFindResource("FeelingLucky") ?? "Feeling Lucky";

        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }
        else
        {
            ShowMessageBox();
        }

        return;

        void ShowMessageBox()
        {
            MessageBox.Show(nogamesfoundtorandomlyselectfrom, feelingLucky, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void PleaseSelectASystemBeforeMessageBox()
    {
        var pleaseselectasystembeforeusingtheFeeling =
            (string)Application.Current.TryFindResource("PleaseselectasystembeforeusingtheFeeling") ??
            "Please select a system before using the Feeling Lucky feature.";
        var feelingLucky = (string)Application.Current.TryFindResource("FeelingLucky") ?? "Feeling Lucky";

        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }
        else
        {
            ShowMessageBox();
        }

        return;

        void ShowMessageBox()
        {
            MessageBox.Show(pleaseselectasystembeforeusingtheFeeling, feelingLucky, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ParameterPathsInvalidWarningMessageBox(List<string> invalidPaths)
    {
        var warningMessage = (string)Application.Current.TryFindResource("ParameterPathsInvalidWarning") ?? "Some paths in the emulator parameters appear to be invalid or missing. Please double check these fields.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        var invalidPathsTitle = (string)Application.Current.TryFindResource("ParameterPathsInvalidPathsTitle") ?? "Potentially invalid paths:";
        var morePaths = (string)Application.Current.TryFindResource("ParameterPathsInvalidPathsMore") ?? "...and {0} more";
        var warningFooter = (string)Application.Current.TryFindResource("ParameterPathsInvalidWarningFooter") ?? "You can still save, but these paths may cause issues when launching games.";

        var finalWarningMessage = new StringBuilder(warningMessage);
        if (invalidPaths.Count > 0)
        {
            finalWarningMessage.Append("\n\n").Append(invalidPathsTitle);
            foreach (var path in invalidPaths.Take(5))
            {
                finalWarningMessage.Append(CultureInfo.InvariantCulture, $"\n• {path}");
            }

            if (invalidPaths.Count > 5)
            {
                finalWarningMessage.Append("\n• ").AppendFormat(CultureInfo.InvariantCulture,
                    morePaths, invalidPaths.Count - 5);
            }
        }

        finalWarningMessage.Append("\n\n").Append(warningFooter);

        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }
        else
        {
            ShowMessageBox();
        }

        return;

        void ShowMessageBox()
        {
            MessageBox.Show(finalWarningMessage.ToString(), warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static bool AskUserToProceedWithInvalidPath(List<string> invalidPaths = null)
    {
        var issuesWithEmulator = (string)Application.Current.TryFindResource("IssuesWithEmulator") ?? "There are issues with the emulator configuration:";
        var invalidPathsLabel = (string)Application.Current.TryFindResource("InvalidPathsLabel") ?? "• The following paths in parameters may be invalid:";
        var proceedQuestion = (string)Application.Current.TryFindResource("ProceedWithLaunchQuestion") ?? "Do you want to proceed with launching anyway?";
        var configEditHint = (string)Application.Current.TryFindResource("ConfigEditHint") ?? "You can edit this system configuration later to fix these issues.";
        var warningCaption = (string)Application.Current.TryFindResource("PathValidationWarningCaption") ?? "Path Validation Warning";

        var messageBuilder = new StringBuilder(issuesWithEmulator);
        if (invalidPaths is { Count: > 0 })
        {
            messageBuilder.Append("\n\n").Append(invalidPathsLabel);
            foreach (var path in invalidPaths)
            {
                messageBuilder.Append(CultureInfo.InvariantCulture, $"\n  - \"{path}\"");
            }
        }

        messageBuilder.Append("\n\n").Append(proceedQuestion).Append('\n');
        messageBuilder.Append('\n').Append(configEditHint);
        var message = messageBuilder.ToString();

        bool ShowMessageBoxAndReturnResult()
        {
            var result = MessageBox.Show(message, warningCaption, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            return result == MessageBoxResult.Yes;
        }

        if (!Application.Current.Dispatcher.CheckAccess())
        {
            return Application.Current.Dispatcher.Invoke((Func<bool>)ShowMessageBoxAndReturnResult);
        }
        else
        {
            return ShowMessageBoxAndReturnResult();
        }
    }

    internal static void SetFuzzyMatchingThresholdFailureMessageBox()
    {
        var therewasanerrorsettingupthefuzzymatchingthreshold =
            (string)Application.Current.TryFindResource("Therewasanerrorsettingupthefuzzymatchingthreshold") ??
            "There was an error setting up the fuzzy matching threshold.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }
        else
        {
            ShowMessageBox();
        }

        return;

        void ShowMessageBox()
        {
            MessageBox.Show(therewasanerrorsettingupthefuzzymatchingthreshold, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ToggleFuzzyMatchingFailureMessageBox()
    {
        var therewasanerrortogglingthefuzzymatchinglogic =
            (string)Application.Current.TryFindResource("Therewasanerrortogglingthefuzzymatchinglogic") ??
            "There was an error toggling the fuzzy matching logic.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }
        else
        {
            ShowMessageBox();
        }

        return;

        void ShowMessageBox()
        {
            MessageBox.Show(therewasanerrortogglingthefuzzymatchinglogic, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FuzzyMatchingErrorValueOutsideValidRangeMessageBox()
    {
        var invalidInputMessageText = (string)Application.Current.TryFindResource("InvalidInputMessageText") ??
                                      "The selected threshold is outside the valid range (70% to 95%).";
        var invalidInputMessageTitle =
            (string)Application.Current.TryFindResource("InvalidInputMessageTitle") ?? "Invalid Input";

        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }
        else
        {
            ShowMessageBox();
        }

        return;

        void ShowMessageBox()
        {
            MessageBox.Show(invalidInputMessageText, invalidInputMessageTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FuzzyMatchingErrorFailToSetThresholdMessageBox()
    {
        var errorMessage =
            (string)Application.Current.TryFindResource("SetFuzzyMatchingThresholdFailureMessageBoxText") ??
            "Failed to set fuzzy matching threshold.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }
        else
        {
            ShowMessageBox();
        }

        return;

        void ShowMessageBox()
        {
            MessageBox.Show(errorMessage, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ListOfErrorsMessageBox(StringBuilder errorMessages)
    {
        var editSystemtofixit =
            (string)Application.Current.TryFindResource("EditSystemtofixit") ?? "Edit System to fix it.";
        var validationerrors = (string)Application.Current.TryFindResource("Validationerrors") ?? "Validation errors";
        var fullMessage = errorMessages + editSystemtofixit;

        if (!Application.Current.Dispatcher.CheckAccess())
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBox);
        }
        else
        {
            ShowMessageBox();
        }

        return;

        void ShowMessageBox()
        {
            MessageBox.Show(fullMessage, validationerrors, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ThereIsNoUpdateAvailableMessageBox(Window mainWindow, string currentVersion)
    {
        var thereisnoupdateavailable = (string)Application.Current.TryFindResource("thereisnoupdateavailable") ??
                                       "There is no update available.";
        var thecurrentversionis = (string)Application.Current.TryFindResource("Thecurrentversionis") ??
                                  "The current version is";
        var noupdateavailable =
            (string)Application.Current.TryFindResource("Noupdateavailable") ?? "No update available";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessageBoxAction();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBoxAction);
        }

        return;

        void ShowMessageBoxAction()
        {
            MessageBox.Show(mainWindow, $"{thereisnoupdateavailable}\n\n" + $"{thecurrentversionis} {currentVersion}", noupdateavailable, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ErrorCheckingForUpdatesMessageBox(Window mainWindow)
    {
        var therewasanerrorcheckingforupdates =
            (string)Application.Current.TryFindResource("Therewasanerrorcheckingforupdates") ??
            "There was an error checking for updates.";
        var maybethereisaproblemwithyourinternet =
            (string)Application.Current.TryFindResource("Maybethereisaproblemwithyourinternet") ??
            "Maybe there is a problem with your internet access or the GitHub server is offline.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessageBoxAction();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBoxAction);
        }

        return;

        void ShowMessageBoxAction()
        {
            MessageBox.Show(mainWindow, $"{therewasanerrorcheckingforupdates}\n\n" + $"{maybethereisaproblemwithyourinternet}", error, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }

    internal static void AnotherInstanceIsRunningMessageBox()
    {
        var anotherinstanceofSimpleLauncherisalreadyrunning = (string)Application.Current.TryFindResource("AnotherinstanceofSimpleLauncherisalreadyrunning") ?? "Another instance of Simple Launcher is already running.";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessageBoxAction();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBoxAction);
        }

        return;

        void ShowMessageBoxAction()
        {
            MessageBox.Show(anotherinstanceofSimpleLauncherisalreadyrunning, "Simple Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void FailedToStartSimpleLauncherMessageBox()
    {
        var failedtostartSimpleLauncherAnerroroccurred = (string)Application.Current.TryFindResource("FailedtostartSimpleLauncherAnerroroccurred") ?? "Failed to start Simple Launcher. An error occurred while checking for existing instances.";
        var simpleLauncherError = (string)Application.Current.TryFindResource("SimpleLauncherError") ?? "Simple Launcher Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessageBoxAction();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBoxAction);
        }

        return;

        void ShowMessageBoxAction()
        {
            MessageBox.Show(failedtostartSimpleLauncherAnerroroccurred, simpleLauncherError, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void UnsupportedArchitectureMessageBox()
    {
        var unsupportedarchitecturefor7Zextraction = (string)Application.Current.TryFindResource("Unsupportedarchitecturefor7zextraction") ?? "Unsupported architecture for 7z extraction.";
        var simpleLauncherrequires64Bitor32BitWindowstorun = (string)Application.Current.TryFindResource("SimpleLauncherrequires64bitor32bitWindowstorun") ?? "'Simple Launcher' requires 64-bit or 32-bit Windows to run.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessageBoxAction();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBoxAction);
        }

        return;

        void ShowMessageBoxAction()
        {
            MessageBox.Show($"{unsupportedarchitecturefor7Zextraction}\n\n" + $"{simpleLauncherrequires64Bitor32BitWindowstorun}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FailedToRestartMessageBox()
    {
        var failedtorestarttheapplication = (string)Application.Current.TryFindResource("Failedtorestarttheapplication") ?? "Failed to restart the application.";
        var restartError = (string)Application.Current.TryFindResource("RestartError") ?? "Restart Error";

        if (Application.Current.Dispatcher.CheckAccess())
        {
            ShowMessageBoxAction();
        }
        else
        {
            Application.Current.Dispatcher.Invoke((Action)ShowMessageBoxAction);
        }

        return;

        void ShowMessageBoxAction()
        {
            MessageBox.Show(failedtorestarttheapplication, restartError, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static MessageBoxResult DoYouWantToUpdateMessageBox(string currentVersion, string latestVersion,
        Window owner)
    {
        var dispatcher = Application.Current.Dispatcher;

        return dispatcher.CheckAccess() ? ShowMessageBox() : dispatcher.Invoke(ShowMessageBox);

        // Helper function to generate message box content and show dialog
        MessageBoxResult ShowMessageBox()
        {
            var thereIsAsoftwareUpdateAvailable = (string)Application.Current.TryFindResource("Thereisasoftwareupdateavailable") ?? "There is a software update available.";
            var theCurrentVersionIs = (string)Application.Current.TryFindResource("Thecurrentversionis") ?? "The current version is";
            var theUpdateVersionIs = (string)Application.Current.TryFindResource("Theupdateversionis") ?? "The update version is";
            var doYouWantToDownloadAndInstall = (string)Application.Current.TryFindResource("Doyouwanttodownloadandinstall") ?? "Do you want to download and install the latest version automatically?";
            var updateAvailable = (string)Application.Current.TryFindResource("UpdateAvailable") ?? "Update Available";
            var message = $"{thereIsAsoftwareUpdateAvailable}\n" +
                          $"{theCurrentVersionIs} {currentVersion}\n" +
                          $"{theUpdateVersionIs} {latestVersion}\n\n" +
                          $"{doYouWantToDownloadAndInstall}";
            return MessageBox.Show(owner, message, updateAvailable, MessageBoxButton.YesNo,
                MessageBoxImage.Information);
        }
    }

    internal static void HandleMissingRequiredFilesMessageBox(string fileList)
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            ShowMissingFiles();
        else
            dispatcher.Invoke(ShowMissingFiles);
        return;

        void ShowMissingFiles()
        {
            var thefollowingrequiredfilesaremissing = (string)Application.Current.TryFindResource("Thefollowingrequiredfilesaremissing") ?? "The following required file(s) are missing:";
            var missingRequiredFiles = (string)Application.Current.TryFindResource("MissingRequiredFiles") ?? "Missing Required Files";
            var doyouwanttoreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix it?";
            var reinstall = MessageBox.Show($"{thefollowingrequiredfilesaremissing}\n{fileList}\n\n{doyouwanttoreinstallSimpleLauncher}",
                missingRequiredFiles, MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (reinstall == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                var pleasereinstallSimpleLauncher =
                    (string)Application.Current.TryFindResource("PleasereinstallSimpleLauncher") ??
                    "Please reinstall 'Simple Launcher' manually to fix the issue.";
                var theapplicationwillshutdown =
                    (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ??
                    "The application will shutdown.";
                MessageBox.Show($"{pleasereinstallSimpleLauncher}\n\n{theapplicationwillshutdown}",
                    missingRequiredFiles, MessageBoxButton.OK, MessageBoxImage.Error);
                QuitApplication.SimpleQuitApplication();
            }
        }
    }

    internal static void HandleApiConfigErrorMessageBox(string reason)
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            ShowApiConfigError();
        else
            dispatcher.Invoke(ShowApiConfigError);
        return;

        void ShowApiConfigError()
        {
            var apiConfigErrorTitle = (string)Application.Current.TryFindResource("ApiConfigErrorTitle") ?? "API Configuration Error";
            var apiConfigErrorMessage = (string)Application.Current.TryFindResource("ApiConfigErrorMessage") ?? "'Simple Launcher' encountered an error loading its API configuration.";
            var reasonLabel = (string)Application.Current.TryFindResource("ReasonLabel") ?? "Reason:";
            var reinstallSuggestion = (string)Application.Current.TryFindResource("ReinstallSuggestion") ?? "This might prevent some features (like automatic bug reporting) from working correctly. Would you like to reinstall Simple Launcher to fix this?";
            var manualReinstallSuggestion = (string)Application.Current.TryFindResource("ManualReinstallSuggestion") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            var applicationWillShutdown = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
            var message = $"{apiConfigErrorMessage}\n\n{reasonLabel} {reason}\n\n{reinstallSuggestion}";
            var result = MessageBox.Show(message, apiConfigErrorTitle, MessageBoxButton.YesNo, MessageBoxImage.Error);
            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                MessageBox.Show($"{manualReinstallSuggestion}\n\n{applicationWillShutdown}",
                    apiConfigErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                QuitApplication.SimpleQuitApplication();
            }
        }
    }

    internal static void DiskSpaceErrorMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;

        if (dispatcher.CheckAccess())
            ShowDiskSpaceError();
        else
            dispatcher.Invoke(ShowDiskSpaceError);
        return;

        static void ShowDiskSpaceError()
        {
            var notenoughdiskspaceforextraction = (string)Application.Current.TryFindResource("Notenoughdiskspaceforextraction") ?? "Not enough disk space for extraction.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show(notenoughdiskspaceforextraction, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void CouldNotCheckForDiskSpaceMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
        {
            ShowMessageBox();
        }
        else
        {
            dispatcher.Invoke(ShowMessageBox);
        }

        return;

        static void ShowMessageBox()
        {
            var message = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotcheckdiskspace") ?? "'Simple Launcher' could not check disk space for the specified path. Please check the path and try again.";
            var caption = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SaveSystemFailedMessageBox(string details = null)
    {
        var failedToSaveSystem = (string)Application.Current.TryFindResource("FailedToSaveSystem") ?? "Failed to save system configuration.";
        var checkPermissions = (string)Application.Current.TryFindResource("CheckFilePermissions") ?? "Please check file permissions and ensure the file is not locked.";
        var errorDetails = (string)Application.Current.TryFindResource("ErrorDetails") ?? "Details:";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var message = $"{failedToSaveSystem}\n\n{checkPermissions}";
        if (!string.IsNullOrEmpty(details))
        {
            message += $"\n\n{errorDetails} {details}";
        }

        if (Application.Current.Dispatcher.CheckAccess())
        {
            MessageBox.Show(message, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        else
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(message, error, MessageBoxButton.OK, MessageBoxImage.Error);
            });
        }
    }

    internal static void CouldNotOpenTheDownloadLink()
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
        {
            ShowMessageBox();
        }
        else
        {
            dispatcher.Invoke(ShowMessageBox);
        }

        return;

        static void ShowMessageBox()
        {
            var simpleLaunchercouldnotopenthedownloadlink = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotopenthedownloadlink") ?? "'Simple Launcher' could not open the download link.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show(simpleLaunchercouldnotopenthedownloadlink,
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void CouldNotTakeScreenshotMessageBox()
    {
        var dispatcher = Application.Current.Dispatcher;
        var couldnottakethescreenshot = (string)Application.Current.TryFindResource("Couldnottakethescreenshot") ?? "Could not take the screenshot.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        if (dispatcher.CheckAccess())
            ShowMsg();
        else
            dispatcher.Invoke(ShowMsg);
        return;

        void ShowMsg()
        {
            MessageBox.Show(
                $"{couldnottakethescreenshot}\n\n" +
                $"{theerrorwasreportedtothedeveloper}",
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}