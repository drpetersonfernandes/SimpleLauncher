using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.QuitOrReinstall;
using SimpleLauncher.SharedModels;

namespace SimpleLauncher.Services.MessageBox;

internal static class MessageBoxLibrary
{
    internal static void TakeScreenShotMessageBox()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var thegamewilllaunchnow = (string)Application.Current.TryFindResource("Thegamewilllaunchnow") ?? "The game will launch now.";
            var setthegamewindowto = (string)Application.Current.TryFindResource("Setthegamewindowto") ?? "Set the game window to non-fullscreen. This is important.";
            var youshouldchangetheemulatorparameters = (string)Application.Current.TryFindResource("Youshouldchangetheemulatorparameters") ?? "You should change the emulator parameters to prevent the emulator from starting in fullscreen.";
            var aselectionwindowwillopeninSimpleLauncherallowingyou = (string)Application.Current.TryFindResource("AselectionwindowwillopeninSimpleLauncherallowingyou") ?? "A selection window will open in 'Simple Launcher', allowing you to choose the desired window to capture.";
            var assoonasyouselectawindow = (string)Application.Current.TryFindResource("assoonasyouselectawindow") ?? "As soon as you select a window, a screenshot will be taken and saved in the image folder of the selected system.";
            var takeScreenshot = (string)Application.Current.TryFindResource("TakeScreenshot") ?? "Take Screenshot";

            System.Windows.MessageBox.Show($"{thegamewilllaunchnow}\n\n" +
                                           $"{setthegamewindowto}\n\n" +
                                           $"{youshouldchangetheemulatorparameters}\n\n" +
                                           $"{aselectionwindowwillopeninSimpleLauncherallowingyou}\n\n" +
                                           $"{assoonasyouselectawindow}", takeScreenshot, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void CouldNotSaveScreenshotMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var failedtosavescreenshot = (string)Application.Current.TryFindResource("Failedtosavescreenshot") ?? "Failed to save screenshot.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            System.Windows.MessageBox.Show($"{failedtosavescreenshot}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void GameIsAlreadyInFavoritesMessageBox(string fileNameWithExtension)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var isalreadyinfavorites = (string)Application.Current.TryFindResource("isalreadyinfavorites") ?? "is already in favorites.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";

            System.Windows.MessageBox.Show($"{fileNameWithExtension} {isalreadyinfavorites}", info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ErrorWhileAddingFavoritesMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var anerroroccurredwhileaddingthisgame = (string)Application.Current.TryFindResource("Anerroroccurredwhileaddingthisgame") ?? "An error occurred while adding this game to the favorites.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            System.Windows.MessageBox.Show($"{anerroroccurredwhileaddingthisgame}\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorWhileRemovingGameFromFavoriteMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var anerroroccurredwhileremoving = (string)Application.Current.TryFindResource("Anerroroccurredwhileremoving") ?? "An error occurred while removing this game from favorites.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            System.Windows.MessageBox.Show($"{anerroroccurredwhileremoving}\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorOpeningTheUpdateHistoryWindowMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var erroropeningtheUpdateHistorywindow = (string)Application.Current.TryFindResource("ErroropeningtheUpdateHistorywindow") ?? "Error opening the Update History window.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            System.Windows.MessageBox.Show($"{erroropeningtheUpdateHistorywindow}\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorOpeningVideoLinkMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasaproblemopeningtheVideo = (string)Application.Current.TryFindResource("TherewasaproblemopeningtheVideo") ?? "There was a problem opening the Video Link.";
            var ensureyouhaveadefaultbrowserinstalled = (string)Application.Current.TryFindResource("Ensureyouhaveadefaultbrowserinstalled") ?? "Ensure you have a default browser installed and configured correctly on your system.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            System.Windows.MessageBox.Show($"{therewasaproblemopeningtheVideo}\n\n" +
                                           $"{ensureyouhaveadefaultbrowserinstalled}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ProblemOpeningInfoLinkMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasaproblemopeningthe = (string)Application.Current.TryFindResource("Therewasaproblemopeningthe") ?? "There was a problem opening the Info Link.";
            var ensureyouhaveadefaultbrowserinstalled = (string)Application.Current.TryFindResource("Ensureyouhaveadefaultbrowserinstalled") ?? "Ensure you have a default browser installed and configured correctly on your system.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            System.Windows.MessageBox.Show($"{therewasaproblemopeningthe}\n\n" +
                                           $"{ensureyouhaveadefaultbrowserinstalled}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorOpeningUrlMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasaproblemopeningtheUrl = (string)Application.Current.TryFindResource("TherewasaproblemopeningtheUrl") ?? "There was a problem opening the Url.";
            var ensureyouhaveadefaultbrowserinstalled = (string)Application.Current.TryFindResource("Ensureyouhaveadefaultbrowserinstalled") ?? "Ensure you have a default browser installed and configured correctly on your system.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            System.Windows.MessageBox.Show($"{therewasaproblemopeningtheUrl}\n\n" +
                                           $"{ensureyouhaveadefaultbrowserinstalled}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ThereIsNoCoverMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var thereisnocoverfileassociated = (string)Application.Current.TryFindResource("Thereisnocoverfileassociated") ?? "There is no cover file associated with this game.";
            var covernotfound = (string)Application.Current.TryFindResource("Covernotfound") ?? "Cover not found";

            System.Windows.MessageBox.Show(thereisnocoverfileassociated, covernotfound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ThereIsNoTitleSnapshotMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var thereisnotitlesnapshot = (string)Application.Current.TryFindResource("Thereisnotitlesnapshot") ?? "There is no title snapshot file associated with this game.";
            var titleSnapshotnotfound = (string)Application.Current.TryFindResource("TitleSnapshotnotfound") ?? "Title Snapshot not found";

            System.Windows.MessageBox.Show(thereisnotitlesnapshot, titleSnapshotnotfound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ThereIsNoGameplaySnapshotMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var thereisnogameplaysnapshot = (string)Application.Current.TryFindResource("Thereisnogameplaysnapshot") ?? "There is no gameplay snapshot file associated with this game.";
            var gameplaySnapshotnotfound = (string)Application.Current.TryFindResource("GameplaySnapshotnotfound") ?? "Gameplay Snapshot not found";

            System.Windows.MessageBox.Show(thereisnogameplaysnapshot, gameplaySnapshotnotfound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ThereIsNoCartMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var thereisnocartfile = (string)Application.Current.TryFindResource("Thereisnocartfile") ?? "There is no cart file associated with this game.";
            var cartnotfound = (string)Application.Current.TryFindResource("Cartnotfound") ?? "Cart not found";

            System.Windows.MessageBox.Show(thereisnocartfile, cartnotfound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ThereIsNoVideoFileMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var thereisnovideofile = (string)Application.Current.TryFindResource("Thereisnovideofile") ?? "There is no video file associated with this game.";
            var videonotfound = (string)Application.Current.TryFindResource("Videonotfound") ?? "Video not found";

            System.Windows.MessageBox.Show(thereisnovideofile, videonotfound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void CouldNotOpenManualMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var failedtoopenthemanual = (string)Application.Current.TryFindResource("Failedtoopenthemanual") ?? "Failed to open the manual.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            System.Windows.MessageBox.Show($"{failedtoopenthemanual}\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ThereIsNoManualMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var thereisnomanual = (string)Application.Current.TryFindResource("Thereisnomanual") ?? "There is no manual associated with this file.";
            var manualNotFound = (string)Application.Current.TryFindResource("Manualnotfound") ?? "Manual not found";

            System.Windows.MessageBox.Show(thereisnomanual, manualNotFound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ThereIsNoWalkthroughMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var thereisnowalkthrough = (string)Application.Current.TryFindResource("Thereisnowalkthrough") ?? "There is no walkthrough file associated with this game.";
            var walkthroughnotfound = (string)Application.Current.TryFindResource("Walkthroughnotfound") ?? "Walkthrough not found";

            System.Windows.MessageBox.Show(thereisnowalkthrough, walkthroughnotfound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ThereIsNoCabinetMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var thereisnocabinetfile = (string)Application.Current.TryFindResource("Thereisnocabinetfile") ?? "There is no cabinet file associated with this game.";
            var cabinetnotfound = (string)Application.Current.TryFindResource("Cabinetnotfound") ?? "Cabinet not found";

            System.Windows.MessageBox.Show(thereisnocabinetfile, cabinetnotfound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ThereIsNoFlyerMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var thereisnoflyer = (string)Application.Current.TryFindResource("Thereisnoflyer") ?? "There is no flyer file associated with this game.";
            var flyernotfound = (string)Application.Current.TryFindResource("Flyernotfound") ?? "Flyer not found";

            System.Windows.MessageBox.Show(thereisnoflyer, flyernotfound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ThereIsNoPcbMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var thereisnoPcBfile = (string)Application.Current.TryFindResource("ThereisnoPCBfile") ?? "There is no PCB file associated with this game.";
            var pCBnotfound = (string)Application.Current.TryFindResource("PCBnotfound") ?? "PCB not found";

            System.Windows.MessageBox.Show(thereisnoPcBfile, pCBnotfound, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void FileSuccessfullyDeletedMessageBox(string fileNameWithExtension)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var thefile = (string)Application.Current.TryFindResource("Thefile") ?? "The file";
            var hasbeensuccessfullydeleted = (string)Application.Current.TryFindResource("hasbeensuccessfullydeleted") ?? "has been successfully deleted.";
            var fileDeleted = (string)Application.Current.TryFindResource("Filedeleted") ?? "File deleted";

            System.Windows.MessageBox.Show($"{thefile} '{fileNameWithExtension}' {hasbeensuccessfullydeleted}", fileDeleted, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void FileCouldNotBeDeletedMessageBox(string fileNameWithExtension)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var anerroroccurredwhiletryingtodelete = (string)Application.Current.TryFindResource("Anerroroccurredwhiletryingtodelete") ?? "An error occurred while trying to delete the file";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            System.Windows.MessageBox.Show($"{anerroroccurredwhiletryingtodelete} '{fileNameWithExtension}'.\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void DefaultImageNotFoundMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var defaultpngfileismissing = (string)Application.Current.TryFindResource("defaultpngfileismissing") ?? "'default.png' file is missing.";
            var doyouwanttoreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var reinstall = System.Windows.MessageBox.Show($"{defaultpngfileismissing}\n\n" + $"{doyouwanttoreinstallSimpleLauncher}", error, MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (reinstall == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                var pleasereinstallSimpleLauncher = (string)Application.Current.TryFindResource("PleasereinstallSimpleLauncher") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";

                System.Windows.MessageBox.Show(pleasereinstallSimpleLauncher, error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void GlobalSearchErrorMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasanerrorusingtheGlobal = (string)Application.Current.TryFindResource("TherewasanerrorusingtheGlobal") ?? "There was an error using the Global Search.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            System.Windows.MessageBox.Show($"{therewasanerrorusingtheGlobal}\n\n" + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void PleaseEnterSearchTermMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var pleaseenterasearchterm = (string)Application.Current.TryFindResource("Pleaseenterasearchterm") ?? "Please enter a search term.";
            var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";

            System.Windows.MessageBox.Show(pleaseenterasearchterm, warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void ErrorLaunchingGameMessageBox(string logPath)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var therewasanerrorlaunchingtheselected = (string)Application.Current.TryFindResource("Therewasanerrorlaunchingtheselected") ?? "There was an error launching the selected game.";
            var dowanttoopenthefileerroruserlog = (string)Application.Current.TryFindResource("Dowanttoopenthefileerroruserlog") ?? "Do want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = System.Windows.MessageBox.Show($"{therewasanerrorlaunchingtheselected}\n\n" + $"{dowanttoopenthefileerroruserlog}", error, MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (result == MessageBoxResult.Yes)
            {
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
                    var thefileerroruserlogwasnotfound = (string)Application.Current.TryFindResource("Thefileerroruserlogwasnotfound") ?? "The file 'error_user.log' was not found!";

                    System.Windows.MessageBox.Show(thefileerroruserlogwasnotfound, error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    internal static void SelectAGameToLaunchMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var pleaseselectagametolaunch = (string)Application.Current.TryFindResource("Pleaseselectagametolaunch") ?? "Please select a game to launch.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show(pleaseselectagametolaunch, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void FileAddedToFavoritesMessageBox(string fileNameWithoutExtension)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var hasbeenaddedtofavorites = (string)Application.Current.TryFindResource("hasbeenaddedtofavorites") ?? "has been added to favorites.";
            var success = (string)Application.Current.TryFindResource("Success") ?? "Success";
            System.Windows.MessageBox.Show($"{fileNameWithoutExtension} {hasbeenaddedtofavorites}", success, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void FileRemovedFromFavoritesMessageBox(string fileNameWithoutExtension)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var wasremovedfromfavorites = (string)Application.Current.TryFindResource("wasremovedfromfavorites") ?? "was removed from favorites.";
            var success = (string)Application.Current.TryFindResource("Success") ?? "Success";
            System.Windows.MessageBox.Show($"{fileNameWithoutExtension} {wasremovedfromfavorites}", success, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void CouldNotLaunchThisGameMessageBox(string logPath)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var simpleLaunchercouldnotlaunchthisgame = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotlaunchthisgame") ?? "'Simple Launcher' could not launch this game.";
            var dowanttoopenthefileerroruserlog = (string)Application.Current.TryFindResource("Dowanttoopenthefileerroruserlog") ?? "Do want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = System.Windows.MessageBox.Show($"{simpleLaunchercouldnotlaunchthisgame}\n\n" +
                                                        $"{dowanttoopenthefileerroruserlog}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
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
                    var thefileerroruserlogwasnotfound = (string)Application.Current.TryFindResource("Thefileerroruserlogwasnotfound") ?? "The file 'error_user.log' was not found!";
                    System.Windows.MessageBox.Show(thefileerroruserlogwasnotfound, error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    internal static void ErrorCalculatingStatsMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var anerroroccurredwhilecalculatingtheGlobal = (string)Application.Current.TryFindResource("AnerroroccurredwhilecalculatingtheGlobal") ?? "An error occurred while calculating the Global Statistics.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            System.Windows.MessageBox.Show($"{anerroroccurredwhilecalculatingtheGlobal}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FailedSaveReportMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var failedtosavethereport = (string)Application.Current.TryFindResource("Failedtosavethereport") ?? "Failed to save the report.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            System.Windows.MessageBox.Show($"{failedtosavethereport}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ReportSavedMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var reportsavedsuccessfully = (string)Application.Current.TryFindResource("Reportsavedsuccessfully") ?? "Report saved successfully.";
            var success = (string)Application.Current.TryFindResource("Success") ?? "Success";

            System.Windows.MessageBox.Show(reportsavedsuccessfully, success, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void NoStatsToSaveMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var nostatisticsavailabletosave = (string)Application.Current.TryFindResource("Nostatisticsavailabletosave") ?? "No statistics available to save.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            System.Windows.MessageBox.Show(nostatisticsavailabletosave, error, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void ErrorLaunchingToolMessageBox(string logPath)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var anerroroccurredwhilelaunchingtheselectedtool = (string)Application.Current.TryFindResource("Anerroroccurredwhilelaunchingtheselectedtool") ?? "An error occurred while launching the selected tool.";
            var grantSimpleLauncheradministrative = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
            var temporarilydisableyourantivirussoftware = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
            var dowanttoopenthefileerroruserlog = (string)Application.Current.TryFindResource("Dowanttoopenthefileerroruserlog") ?? "Do want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = System.Windows.MessageBox.Show($"{anerroroccurredwhilelaunchingtheselectedtool}\n\n" +
                                                        $"{grantSimpleLauncheradministrative}\n\n" +
                                                        $"{temporarilydisableyourantivirussoftware}\n\n" +
                                                        $"{dowanttoopenthefileerroruserlog}", error, MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (result == MessageBoxResult.Yes)
            {
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
                    var thefileerroruserlogwasnotfound = (string)Application.Current.TryFindResource("Thefileerroruserlogwasnotfound") ?? "The file 'error_user.log' was not found!";
                    System.Windows.MessageBox.Show(thefileerroruserlogwasnotfound, error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    internal static void SelectedToolNotFoundMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var theselectedtoolwasnotfound = (string)Application.Current.TryFindResource("Theselectedtoolwasnotfound") ?? "The selected tool was not found in the expected path.";
            var doyouwanttoreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var reinstall = System.Windows.MessageBox.Show($"{theselectedtoolwasnotfound}\n\n" +
                                                           $"{doyouwanttoreinstallSimpleLauncher}", error, MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (reinstall == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                var pleasereinstallSimpleLaunchermanually = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
                System.Windows.MessageBox.Show(pleasereinstallSimpleLaunchermanually, error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void ErrorMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasanerror = (string)Application.Current.TryFindResource("Therewasanerror") ?? "There was an error.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{therewasanerror}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void NoFavoriteFoundMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var thereisnoFavoriteforthissystem = (string)Application.Current.TryFindResource("ThereisnoFavoriteforthissystem") ?? "There is no Favorite for this system, or you have not chosen a system.";
            var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            System.Windows.MessageBox.Show(thereisnoFavoriteforthissystem, warning, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void MoveToWritableFolderMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var itlookslikeSimpleLauncherisinstalled = (string)Application.Current.TryFindResource("ItlookslikeSimpleLauncherisinstalled") ?? "It looks like 'Simple Launcher' is installed in a restricted folder (e.g., Program Files), where it does not have write access.";
            var itneedswriteaccesstoitsfolder = (string)Application.Current.TryFindResource("Itneedswriteaccesstoitsfolder") ?? "It needs write access to its folder.";
            var pleasemovetheapplicationfolder = (string)Application.Current.TryFindResource("Pleasemovetheapplicationfolder") ?? "Please move the application folder to a writable location like the 'Documents' folder.";
            var ifpossiblerunitwithadministrative = (string)Application.Current.TryFindResource("Ifpossiblerunitwithadministrative") ?? "If possible, run it with administrative privileges.";
            var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            System.Windows.MessageBox.Show($"{itlookslikeSimpleLauncherisinstalled}\n\n" +
                                           $"{itneedswriteaccesstoitsfolder}\n\n" +
                                           $"{pleasemovetheapplicationfolder}\n\n" +
                                           $"{ifpossiblerunitwithadministrative}", warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void InvalidSystemConfigMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasanerrorwhileloading = (string)Application.Current.TryFindResource("Therewasanerrorwhileloading") ?? "There was an error while loading the system configuration for this system.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{therewasanerrorwhileloading}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorMethodLoadGameFilesAsyncMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasanerrorloadingthegame = (string)Application.Current.TryFindResource("Therewasanerrorloadingthegame") ?? "There was an error loading the game list.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{therewasanerrorloadingthegame}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorOpeningDonationLinkMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasanerroropeningthedonation = (string)Application.Current.TryFindResource("Therewasanerroropeningthedonation") ?? "There was an error opening the Donation Link.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{therewasanerroropeningthedonation}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ToggleGamepadFailureMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var failedtotogglegamepad = (string)Application.Current.TryFindResource("Failedtotogglegamepad") ?? "Failed to toggle gamepad.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{failedtotogglegamepad}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ToolLaunchWasCanceledByUserMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var thelaunchoftheselectedtoolwascanceledbytheuser = (string)Application.Current.TryFindResource("thelaunchoftheselectedtoolwascanceledbytheuser") ?? "The launch of the selected tool was canceled by the user.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show(thelaunchoftheselectedtoolwascanceledbytheuser, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ErrorChangingViewModeMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasanerrorwhilechangingtheviewmode = (string)Application.Current.TryFindResource("Therewasanerrorwhilechangingtheviewmode") ?? "There was an error while changing the view mode.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{therewasanerrorwhilechangingtheviewmode}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void NavigationButtonErrorMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasanerrorinthenavigationbutton = (string)Application.Current.TryFindResource("Therewasanerrorinthenavigationbutton") ?? "There was an error in the navigation button.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{therewasanerrorinthenavigationbutton}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SelectSystemBeforeSearchMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var pleaseselectasystembeforesearching = (string)Application.Current.TryFindResource("Pleaseselectasystembeforesearching") ?? "Please select a system before searching.";
            var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            System.Windows.MessageBox.Show(pleaseselectasystembeforesearching, warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void EnterSearchQueryMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var pleaseenterasearchquery = (string)Application.Current.TryFindResource("Pleaseenterasearchquery") ?? "Please enter a search query.";
            var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            System.Windows.MessageBox.Show(pleaseenterasearchquery, warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void ErrorWhileLoadingHelpUserXmlMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var unexpectederrorwhileloadinghelpuserxml = (string)Application.Current.TryFindResource("Unexpectederrorwhileloadinghelpuserxml") ?? "Unexpected error while loading 'helpuser.xml'.";
            var doyouwanttoautomaticreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = System.Windows.MessageBox.Show($"{unexpectederrorwhileloadinghelpuserxml}\n\n" +
                                                        $"{doyouwanttoautomaticreinstallSimpleLauncher}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
        }
    }

    internal static void NoSystemInHelpUserXmlMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var novalidsystemsfoundinthefilehelpuserxml = (string)Application.Current.TryFindResource("Novalidsystemsfoundinthefilehelpuserxml") ?? "No valid systems found in the file 'helpuser.xml'.";
            var doyouwanttoautomaticreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = System.Windows.MessageBox.Show($"{novalidsystemsfoundinthefilehelpuserxml}\n\n" +
                                                        $"{doyouwanttoautomaticreinstallSimpleLauncher}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
        }
    }

    internal static MessageBoxResult CouldNotLoadHelpUserXmlMessageBox()
    {
        return Application.Current.Dispatcher.Invoke(static () =>
        {
            var simpleLaunchercouldnotloadhelpuserxml = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotloadhelpuserxml") ?? "'Simple Launcher' could not load 'helpuser.xml'.";
            var doyouwanttoautomaticreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            return System.Windows.MessageBox.Show($"{simpleLaunchercouldnotloadhelpuserxml}\n\n" +
                                                  $"{doyouwanttoautomaticreinstallSimpleLauncher}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);
        });
    }

    internal static void FailedToLoadHelpUserXmlMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var unabletoloadhelpuserxml = (string)Application.Current.TryFindResource("Unabletoloadhelpuserxml") ?? "Unable to load 'helpuser.xml'. The file may be corrupted.";
            var doyouwanttoautomaticreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = System.Windows.MessageBox.Show($"{unabletoloadhelpuserxml}\n\n" +
                                                        $"{doyouwanttoautomaticreinstallSimpleLauncher}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
        }
    }

    internal static void FileHelpUserXmlIsMissingMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var thefilehelpuserxmlismissing = (string)Application.Current.TryFindResource("Thefilehelpuserxmlismissing") ?? "The file 'helpuser.xml' is missing.";
            var doyouwanttoautomaticreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = System.Windows.MessageBox.Show($"{thefilehelpuserxmlismissing}\n\n" +
                                                        $"{doyouwanttoautomaticreinstallSimpleLauncher}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
        }
    }

    internal static void ImageViewerErrorMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var failedtoloadtheimageintheImage = (string)Application.Current.TryFindResource("FailedtoloadtheimageintheImage") ?? "Failed to load the image in the Image Viewer window.";
            var theimagemaybecorruptedorinaccessible = (string)Application.Current.TryFindResource("Theimagemaybecorruptedorinaccessible") ?? "The image may be corrupted or inaccessible.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{failedtoloadtheimageintheImage}\n\n" +
                                           $"{theimagemaybecorruptedorinaccessible}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ReinstallSimpleLauncherFileCorruptedMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var simpleLaunchercouldnotloadthefilemamedat = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotloadthefilemamedat") ?? "'Simple Launcher' could not load the file 'mame.dat' or it is corrupted.";
            var doyouwanttoautomaticreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = System.Windows.MessageBox.Show($"{simpleLaunchercouldnotloadthefilemamedat}\n\n" +
                                                        $"{doyouwanttoautomaticreinstallSimpleLauncher}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                var pleasereinstallSimpleLaunchermanually = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
                var theapplicationwillshutdown = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
                System.Windows.MessageBox.Show($"{pleasereinstallSimpleLaunchermanually}\n\n" +
                                               $"{theapplicationwillshutdown}", error, MessageBoxButton.OK, MessageBoxImage.Error);

                QuitSimpleLauncher.SimpleQuitApplication();
            }
        }
    }

    internal static void ReinstallSimpleLauncherFileMissingMessageBox()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var thefilemamedatcouldnotbefound = (string)Application.Current.TryFindResource("Thefilemamedatcouldnotbefound") ?? "The file 'mame.dat' could not be found in the application folder.";
            var doyouwanttoautomaticreinstall = (string)Application.Current.TryFindResource("Doyouwanttoautomaticreinstall") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = System.Windows.MessageBox.Show($"{thefilemamedatcouldnotbefound}\n\n"
                                                        + $"{doyouwanttoautomaticreinstall}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
        }
    }

    internal static void ErrorCheckingForUpdatesMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var anerroroccurredwhilecheckingforupdates = (string)Application.Current.TryFindResource("Anerroroccurredwhilecheckingforupdates") ?? "An error occurred while checking for updates.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{anerroroccurredwhilecheckingforupdates}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorLoadingRomHistoryMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var anerroroccurredwhileloadingRoMhistory = (string)Application.Current.TryFindResource("AnerroroccurredwhileloadingROMhistory") ?? "An error occurred while loading ROM history.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            System.Windows.MessageBox.Show($"{anerroroccurredwhileloadingRoMhistory}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void NoHistoryXmlFoundMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var nohistoryxmlfilefound = (string)Application.Current.TryFindResource("Nohistoryxmlfilefound") ?? "No 'history.xml' file found in the application folder.";
            var doyouwanttoreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = System.Windows.MessageBox.Show($"{nohistoryxmlfilefound}\n\n" +
                                                        $"{doyouwanttoreinstallSimpleLauncher}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
        }
    }

    internal static void ErrorOpeningBrowserMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var anerroroccurredwhileopeningthebrowser = (string)Application.Current.TryFindResource("Anerroroccurredwhileopeningthebrowser") ?? "An error occurred while opening the browser.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            System.Windows.MessageBox.Show($"{anerroroccurredwhileopeningthebrowser}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SystemXmlIsCorruptedMessageBox(string logPath)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var systemxmliscorrupted = (string)Application.Current.TryFindResource("systemxmliscorrupted") ?? "'system.xml' is corrupted or could not be opened.";
            var pleasefixitmanuallyordeleteit = (string)Application.Current.TryFindResource("Pleasefixitmanuallyordeleteit") ?? "Please fix it manually or delete it.";
            var ifyouchoosetodeleteit = (string)Application.Current.TryFindResource("Ifyouchoosetodeleteit") ?? "If you choose to delete it, 'Simple Launcher' will create a new one for you.";
            var theapplicationwillshutdown = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
            var wouldyouliketoopentheerroruserlog = (string)Application.Current.TryFindResource("Wouldyouliketoopentheerroruserlog") ?? "Would you like to open the 'error_user.log' file to investigate the issue?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = System.Windows.MessageBox.Show($"{systemxmliscorrupted} {pleasefixitmanuallyordeleteit}\n\n" +
                                                        $"{ifyouchoosetodeleteit}\n\n" +
                                                        $"{theapplicationwillshutdown}" +
                                                        $"{wouldyouliketoopentheerroruserlog}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
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
                    System.Windows.MessageBox.Show(thefileerroruserlog, error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }

            QuitSimpleLauncher.SimpleQuitApplication();
        }
    }

    internal static void FileSystemXmlIsCorruptedMessageBox(string logPath)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var thefilesystemxmlisbadlycorrupted = (string)Application.Current.TryFindResource("Thefilesystemxmlisbadlycorrupted") ?? "The file 'system.xml' is badly corrupted.";
            var wouldyouliketoopentheerroruserlog = (string)Application.Current.TryFindResource("Wouldyouliketoopentheerroruserlog") ?? "Would you like to open the 'error_user.log' file to investigate the issue?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = System.Windows.MessageBox.Show($"{thefilesystemxmlisbadlycorrupted}\n\n" +
                                                        $"{wouldyouliketoopentheerroruserlog}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
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
                    System.Windows.MessageBox.Show(thefileerroruserlog, error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    internal static void InstallUpdateManuallyMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var therewasanerrorinstallingorupdating = (string)Application.Current.TryFindResource("Therewasanerrorinstallingorupdating") ?? "There was an error installing or updating the application.";
            var wouldyouliketoberedirectedtothedownloadpage = (string)Application.Current.TryFindResource("Wouldyouliketoberedirectedtothedownloadpage") ?? "Would you like to be redirected to the download page to install or update it manually?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var messageBoxResult = System.Windows.MessageBox.Show($"{therewasanerrorinstallingorupdating}\n\n" +
                                                                  $"{wouldyouliketoberedirectedtothedownloadpage}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                var downloadPageUrl = App.Configuration["Urls:GitHubReleases"] ?? "https://github.com/drpetersonfernandes/SimpleLauncher/releases/latest";

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
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error in method InstallUpdateManuallyMessageBox");

                    // Notify user
                    var anerroroccurredwhileopeningthebrowser = (string)Application.Current.TryFindResource("Anerroroccurredwhileopeningthebrowser") ?? "An error occurred while opening the browser.";
                    var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
                    System.Windows.MessageBox.Show($"{anerroroccurredwhileopeningthebrowser}\n\n" +
                                                   $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    internal static void RequiredFileMissingMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var fileappsettingsjsonismissing = (string)Application.Current.TryFindResource("Fileappsettingsjsonismissing") ?? "File 'appsettings.json' is missing.";
            var theapplicationwillnotbeabletosendthesupportrequest = (string)Application.Current.TryFindResource("Theapplicationwillnotbeabletosendthesupportrequest") ?? "The application will not be able to send the support request.";
            var doyouwanttoautomaticallyreinstall = (string)Application.Current.TryFindResource("Doyouwanttoautomaticallyreinstall") ?? "Do you want to automatically reinstall 'Simple Launcher' to fix the problem?";
            var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";

            var messageBoxResult = System.Windows.MessageBox.Show($"{fileappsettingsjsonismissing}\n\n" +
                                                                  $"{theapplicationwillnotbeabletosendthesupportrequest}\n\n" +
                                                                  $"{doyouwanttoautomaticallyreinstall}", warning, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (messageBoxResult == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                var pleasereinstallSimpleLauncher = (string)Application.Current.TryFindResource("PleasereinstallSimpleLauncher") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
                System.Windows.MessageBox.Show(pleasereinstallSimpleLauncher, warning, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    internal static void EnterSupportRequestMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var pleaseenterthedetailsofthesupportrequest = (string)Application.Current.TryFindResource("Pleaseenterthedetailsofthesupportrequest") ?? "Please enter the details of the support request.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show(pleaseenterthedetailsofthesupportrequest, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void EnterNameMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var pleaseenterthename = (string)Application.Current.TryFindResource("Pleaseenterthename") ?? "Please enter the name.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show(pleaseenterthename, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void EnterEmailMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var pleaseentertheemail = (string)Application.Current.TryFindResource("Pleaseentertheemail") ?? "Please enter the email.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show(pleaseentertheemail, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ApiKeyErrorMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasanerrorintheApiKey = (string)Application.Current.TryFindResource("TherewasanerrorintheAPIKey") ?? "There was an error in the API Key of this form.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{therewasanerrorintheApiKey}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SupportRequestSuccessMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var supportrequestsentsuccessfully = (string)Application.Current.TryFindResource("Supportrequestsentsuccessfully") ?? "Support request sent successfully.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show(supportrequestsentsuccessfully, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void SupportRequestSendErrorMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var anerroroccurredwhilesendingthesupportrequest = (string)Application.Current.TryFindResource("Anerroroccurredwhilesendingthesupportrequest") ?? "An error occurred while sending the support request.";
            var thebugwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Thebugwasreportedtothedeveloper") ?? "The bug was reported to the developer that will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{anerroroccurredwhilesendingthesupportrequest}\n\n" +
                                           $"{thebugwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ExtractionFailedMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var extractionfailed = (string)Application.Current.TryFindResource("Extractionfailed") ?? "Extraction failed.";
            var ensurethefileisnotcorrupted = (string)Application.Current.TryFindResource("Ensurethefileisnotcorrupted") ?? "Ensure the file is not corrupted.";
            var ensureyouhaveenoughspaceintheHdd = (string)Application.Current.TryFindResource("EnsureyouhaveenoughspaceintheHDD") ?? "Ensure you have enough space in the HDD to extract the file.";
            var grantSimpleLauncheradministrative = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
            var ensuretheSimpleLauncherfolder = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncherfolder") ?? "Ensure the 'Simple Launcher' folder is a writable directory.";
            var temporarilydisableyourantivirus = (string)Application.Current.TryFindResource("Temporarilydisableyourantivirus") ?? "Temporarily disable your antivirus software and try again.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{extractionfailed}\n\n" +
                                           $"{ensurethefileisnotcorrupted}\n" +
                                           $"{ensureyouhaveenoughspaceintheHdd}\n" +
                                           $"{grantSimpleLauncheradministrative}\n" +
                                           $"{ensuretheSimpleLauncherfolder}\n" +
                                           $"{temporarilydisableyourantivirus}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FileNeedToBeCompressedMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var theselectedfilecannotbe = (string)Application.Current.TryFindResource("Theselectedfilecannotbe") ?? "The selected file cannot be extracted.";
            var toextractafileitneedstobe = (string)Application.Current.TryFindResource("Toextractafileitneedstobe") ?? "To extract a file, it needs to be a 7z, zip, or rar file.";
            var pleasefixthatintheEditwindow = (string)Application.Current.TryFindResource("PleasefixthatintheEditwindow") ?? "Please fix that in the Edit window.";
            var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            System.Windows.MessageBox.Show($"{theselectedfilecannotbe}\n\n" +
                                           $"{toextractafileitneedstobe}\n\n" +
                                           $"{pleasefixthatintheEditwindow}", warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void DownloadedFileIsMissingMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var downloadedfileismissing = (string)Application.Current.TryFindResource("Downloadedfileismissing") ?? "Downloaded file is missing.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{downloadedfileismissing}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FileIsLockedMessageBox(string tempFolderPath)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var downloadedfileislocked = (string)Application.Current.TryFindResource("Downloadedfileislocked") ?? "Downloaded file is locked.";
            var grantSimpleLauncheradministrative = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
            var temporarilydisableyourantivirussoftware = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
            var ensuretheSimpleLauncher = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncher") ?? "Ensure the 'Simple Launcher' folder is a writable directory.";
            var openTempFolderQuestion = (string)Application.Current.TryFindResource("OpenTempFolderQuestion") ?? "Would you like to open the temporary folder to inspect the file?"; // New line
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = System.Windows.MessageBox.Show($"{downloadedfileislocked}\n\n" +
                                                        $"{grantSimpleLauncheradministrative}\n\n" +
                                                        $"{temporarilydisableyourantivirussoftware}\n\n" +
                                                        $"{ensuretheSimpleLauncher}\n\n" +
                                                        $"{openTempFolderQuestion}", error, MessageBoxButton.YesNo, MessageBoxImage.Error); // Changed to YesNo

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tempFolderPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    var errorOpeningFolderTitle = (string)Application.Current.TryFindResource("ErrorOpeningFolderTitle") ?? "Error Opening Folder";
                    var errorOpeningFolderMessage = (string)Application.Current.TryFindResource("ErrorOpeningFolderMessage") ?? "Could not open the temporary folder.";
                    System.Windows.MessageBox.Show(errorOpeningFolderMessage, errorOpeningFolderTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to open temp folder: {tempFolderPath}");
                }
            }
        }
    }

    internal static void LinksSavedMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var linkssavedsuccessfully = (string)Application.Current.TryFindResource("Linkssavedsuccessfully") ?? "Links saved successfully.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show(linkssavedsuccessfully, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void DeadZonesSavedMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var deadZonevaluessavedsuccessfully = (string)Application.Current.TryFindResource("DeadZonevaluessavedsuccessfully") ?? "DeadZone values saved successfully.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show(deadZonevaluessavedsuccessfully, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void LinksRevertedMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var linksreverted = (string)Application.Current.TryFindResource("Linksrevertedtodefaultvalues") ?? "Links reverted to default values.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show(linksreverted, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void MainWindowSearchEngineErrorMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasanerrorwiththesearchengine = (string)Application.Current.TryFindResource("Therewasanerrorwiththesearchengine") ?? "There was an error with the search engine.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{therewasanerrorwiththesearchengine}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void DownloadExtractionFailedMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var downloadorextractionfailed = (string)Application.Current.TryFindResource("DownloadorExtractionFailed") ?? "Download or extraction failed.";
            var grantSimpleLauncheradministrativeaccess = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrativeaccess") ?? "Grant 'Simple Launcher' administrative access and try again.";
            var ensuretheSimpleLauncherfolder = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncherfolder") ?? "Ensure the 'Simple Launcher' folder is a writable directory.";
            var temporarilydisableyourantivirus = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{downloadorextractionfailed}\n\n" +
                                           $"{grantSimpleLauncheradministrativeaccess}\n\n" +
                                           $"{ensuretheSimpleLauncherfolder}\n\n" +
                                           $"{temporarilydisableyourantivirus}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void DownloadAndExtrationWereSuccessfulMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var downloadandextractioncompletedsuccessfully = (string)Application.Current.TryFindResource("Downloadandextractioncompletedsuccessfully") ?? "Download and extraction completed successfully.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show(downloadandextractioncompletedsuccessfully, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static Task ShowEmulatorDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return Task.CompletedTask;

        void ShowMessage()
        {
            var downloaderror = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
            var wouldyouliketoberedirected = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = System.Windows.MessageBox.Show($"{downloaderror}\n\n" +
                                                        $"{wouldyouliketoberedirected}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = selectedSystem.Emulators.Emulator.EmulatorDownloadLink, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "Error opening the download link.";
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                    // Notify user
                    var erroropeningthedownloadlink = (string)Application.Current.TryFindResource("Erroropeningthedownloadlink") ?? "Error opening the download link.";
                    var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
                    System.Windows.MessageBox.Show($"{erroropeningthedownloadlink}\n\n" +
                                                   $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    internal static Task ShowCoreDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return Task.CompletedTask;

        void ShowMessage()
        {
            var downloaderror = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
            var wouldyouliketoberedirected =
                (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = System.Windows.MessageBox.Show($"{downloaderror}\n\n" +
                                                        $"{wouldyouliketoberedirected}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
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
                    // Notify developer
                    const string contextMessage = "Error opening the download link.";
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                    // Notify user
                    var erroropeningthedownloadlink = (string)Application.Current.TryFindResource("Erroropeningthedownloadlink") ?? "Error opening the download link.";
                    var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
                    System.Windows.MessageBox.Show($"{erroropeningthedownloadlink}\n\n" +
                                                   $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    internal static Task ShowImagePackDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem)
    {
        if (selectedSystem?.Emulators?.Emulator?.ImagePackDownloadLink == null)
        {
            return Task.CompletedTask;
        }

        Application.Current.Dispatcher.InvokeAsync((Action)ShowMessage);
        return Task.CompletedTask;

        void ShowMessage()
        {
            var downloadError = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
            var wouldYouLikeToBeRedirected = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
            var errorCaption = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = System.Windows.MessageBox.Show($"{downloadError}\n\n" +
                                                        $"{wouldYouLikeToBeRedirected}", errorCaption, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = selectedSystem.Emulators.Emulator.ImagePackDownloadLink, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "Error opening the download link.";
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                    // Notify user
                    var errorOpeningDownloadLink = (string)Application.Current.TryFindResource("Erroropeningthedownloadlink") ?? "Error opening the download link.";
                    var errorWasReported = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
                    System.Windows.MessageBox.Show($"{errorOpeningDownloadLink}\n\n{errorWasReported}", errorCaption, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    internal static void SelectAHistoryItemToRemoveMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message = (string)Application.Current.TryFindResource("SelectAHistoryItemToRemove") ?? "Please select a history item to remove.";
            var pleaseselectaitem = (string)Application.Current.TryFindResource("Pleaseselectaitem") ?? "Please select a item";
            System.Windows.MessageBox.Show(message, pleaseselectaitem, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static MessageBoxResult ReallyWantToRemoveAllPlayHistoryMessageBox()
    {
        return Application.Current.Dispatcher.Invoke(static () =>
        {
            var message = (string)Application.Current.TryFindResource("AreYouSureYouWantToRemoveAllHistory") ?? "Are you sure you want to remove all play history?";
            var confirmation = (string)Application.Current.TryFindResource("Confirmation") ?? "Confirmation";
            return System.Windows.MessageBox.Show(message, confirmation, MessageBoxButton.YesNo, MessageBoxImage.Question);
        });
    }

    internal static void SystemAddedMessageBox(string systemName, string resolvedSystemFolder, string resolvedSystemImageFolder)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var thesystem = (string)Application.Current.TryFindResource("Thesystem") ?? "The system";
            var hasbeenaddedsuccessfully = (string)Application.Current.TryFindResource("hasbeenaddedsuccessfully") ?? "has been added successfully.";
            var putRoMsorIsOsforthissysteminside = (string)Application.Current.TryFindResource("PutROMsorISOsforthissysteminside") ?? "Put ROMs or ISOs for this system inside";
            var putcoverimagesforthissysteminside = (string)Application.Current.TryFindResource("Putcoverimagesforthissysteminside") ?? "Put cover images for this system inside";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show($"{thesystem} '{systemName}' {hasbeenaddedsuccessfully}\n\n"
                                           + $"{putRoMsorIsOsforthissysteminside} '{resolvedSystemFolder}'\n\n"
                                           + $"{putcoverimagesforthissysteminside} '{resolvedSystemImageFolder}'.", info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void AddSystemFailedMessageBox(string details = null)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var therewasanerroradding = (string)Application.Current.TryFindResource("Therewasanerroradding") ?? "There was an error adding this system.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var errorDetails = (string)Application.Current.TryFindResource("ErrorDetails") ?? "Details:";

            var message = $"{therewasanerroradding}\n\n" +
                          $"{theerrorwasreportedtothedeveloper}";

            if (!string.IsNullOrEmpty(details))
            {
                message += $"\n\n{errorDetails} {details}";
            }

            System.Windows.MessageBox.Show(message, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void RightClickContextMenuErrorMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasanerrorintherightclick = (string)Application.Current.TryFindResource("Therewasanerrorintherightclick") ?? "There was an error in the right-click context menu.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{therewasanerrorintherightclick}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void GameFileDoesNotExistMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var thegamefiledoesnotexist = (string)Application.Current.TryFindResource("Thegamefiledoesnotexist") ?? "The game file does not exist!";
            var thefilehasbeenremovedfromthelist = (string)Application.Current.TryFindResource("Thefilehasbeenremovedfromthelist") ?? "The file has been removed from the list.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show($"{thegamefiledoesnotexist}\n\n" +
                                           $"{thefilehasbeenremovedfromthelist}", info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void CouldNotOpenHistoryWindowMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasaproblemopeningtheHistorywindow = (string)Application.Current.TryFindResource("TherewasaproblemopeningtheHistorywindow") ?? "There was a problem opening the History window.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{therewasaproblemopeningtheHistorywindow}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void CouldNotOpenWalkthroughMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var failedtoopenthewalkthroughfile = (string)Application.Current.TryFindResource("Failedtoopenthewalkthroughfile") ?? "Failed to open the walkthrough file.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{failedtoopenthewalkthroughfile}\n\n"
                                           + $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SelectAFavoriteToRemoveMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var pleaseselectafavoritetoremove = (string)Application.Current.TryFindResource("Pleaseselectafavoritetoremove") ?? "Please select a favorite to remove.";
            var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            System.Windows.MessageBox.Show(pleaseselectafavoritetoremove, warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void SystemXmlNotFoundMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var systemxmlnotfound = (string)Application.Current.TryFindResource("systemxmlnotfound") ?? "'system.xml' not found inside the application folder.";
            var pleaserestartSimpleLauncher = (string)Application.Current.TryFindResource("PleaserestartSimpleLauncher") ?? "Please restart 'Simple Launcher'.";
            var ifthatdoesnotwork = (string)Application.Current.TryFindResource("Ifthatdoesnotwork") ?? "If that does not work, please reinstall 'Simple Launcher'.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{systemxmlnotfound}\n\n" +
                                           $"{pleaserestartSimpleLauncher}\n\n" +
                                           $"{ifthatdoesnotwork}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void YouCanAddANewSystemMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var youcanaddanewsystem = (string)Application.Current.TryFindResource("Youcanaddanewsystem") ?? "You can add a new system now.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show(youcanaddanewsystem, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void EmulatorNameRequiredMessageBox(int i)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var emulator = (string)Application.Current.TryFindResource("Emulator") ?? "Emulator";
            var nameisrequiredbecauserelateddata = (string)Application.Current.TryFindResource("nameisrequiredbecauserelateddata") ?? "name is required because related data has been provided.";
            var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show($"{emulator} {i} {nameisrequiredbecauserelateddata}\n\n" +
                                           $"{pleasefixthisfield}", info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void EmulatorNameIsRequiredMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var emulatornameisrequired = (string)Application.Current.TryFindResource("Emulatornameisrequired") ?? "Emulator name is required.";
            var pleasefixthat = (string)Application.Current.TryFindResource("Pleasefixthat") ?? "Please fix that.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{emulatornameisrequired}\n\n" +
                                           $"{pleasefixthat}", error, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void EmulatorNameMustBeUniqueMessageBox(string emulatorName)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var thename = (string)Application.Current.TryFindResource("Thename") ?? "The name";
            var isusedmultipletimes = (string)Application.Current.TryFindResource("isusedmultipletimes") ?? "is used multiple times. Each emulator name must be unique.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show($"{thename} '{emulatorName}' {isusedmultipletimes}", info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void SystemSavedSuccessfullyMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var systemsavedsuccessfully = (string)Application.Current.TryFindResource("Systemsavedsuccessfully") ?? "System saved successfully.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show(systemsavedsuccessfully, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void PathOrParameterInvalidMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var oneormorepathsorparameters = (string)Application.Current.TryFindResource("Oneormorepathsorparameters") ?? "One or more paths or parameters are invalid.";
            var pleasefixthemtoproceed = (string)Application.Current.TryFindResource("Pleasefixthemtoproceed") ?? "Please fix them to proceed.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{oneormorepathsorparameters}\n\n" +
                                           $"{pleasefixthemtoproceed}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void Emulator1RequiredMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var emulator1Nameisrequired = (string)Application.Current.TryFindResource("Emulator1Nameisrequired") ?? "'Emulator 1 Name' is required.";
            var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{emulator1Nameisrequired}\n\n" +
                                           $"{pleasefixthisfield}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ExtensionToLaunchIsRequiredMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var extensiontoLaunchAfterExtraction = (string)Application.Current.TryFindResource("ExtensiontoLaunchAfterExtraction") ?? "'Extension to Launch After Extraction' is required when 'Extract File Before Launch' is set to true.";
            var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{extensiontoLaunchAfterExtraction}\n\n" +
                                           $"{pleasefixthisfield}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ExtensionToSearchIsRequiredMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var extensiontoSearchintheSystemFolder = (string)Application.Current.TryFindResource("ExtensiontoSearchintheSystemFolder") ?? "'Extension to Search in the System Folder' cannot be empty or contain only spaces.";
            var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{extensiontoSearchintheSystemFolder}\n\n" +
                                           $"{pleasefixthisfield}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FileMustBeCompressedMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var whenExtractFileBeforeLaunch = (string)Application.Current.TryFindResource("WhenExtractFileBeforeLaunch") ?? "When 'Extract File Before Launch' is set to true, 'Extension to Search in the System Folder' must include 'zip', '7z', or 'rar'.";
            var itwillnotacceptotherextensions = (string)Application.Current.TryFindResource("Itwillnotacceptotherextensions") ?? "It will not accept other extensions.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{whenExtractFileBeforeLaunch}\n\n" +
                                           $"{itwillnotacceptotherextensions}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SystemImageFolderCanNotBeEmptyMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var systemImageFoldercannotbeempty = (string)Application.Current.TryFindResource("SystemImageFoldercannotbeempty") ?? "'System Image Folder' cannot be empty or contain only spaces.";
            var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{systemImageFoldercannotbeempty}\n\n" +
                                           $"{pleasefixthisfield}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SystemFolderCanNotBeEmptyMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var systemFoldercannotbeempty = (string)Application.Current.TryFindResource("SystemFoldercannotbeempty") ?? "'System Folder' cannot be empty or contain only spaces.";
            var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{systemFoldercannotbeempty}\n\n" +
                                           $"{pleasefixthisfield}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SystemNameCanNotBeEmptyMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var systemNamecannotbeemptyor = (string)Application.Current.TryFindResource("SystemNamecannotbeemptyor") ?? "'System Name' cannot be empty or contain only spaces.";
            var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{systemNamecannotbeemptyor}\n\n" +
                                           $"{pleasefixthisfield}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // internal static void FolderCreatedMessageBox(string systemNameText)
    // {
    //     Application.Current.Dispatcher.InvokeAsync(ShowMessage);
    //     return;
    //
    //     void ShowMessage()
    //     {
    //         var simpleLaunchercreatedaimagefolder = (string)Application.Current.TryFindResource("SimpleLaunchercreatedaimagefolder") ?? "'Simple Launcher' created a image folder for this system at";
    //         var youmayplacethecoverimagesforthissystem = (string)Application.Current.TryFindResource("Youmayplacethecoverimagesforthissysteminside") ?? "You may place the cover images for this system inside this folder.";
    //         var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
    //         MessageBox.Show($"{simpleLaunchercreatedaimagefolder} '.\\images\\{systemNameText}'.\n\n" +
    //                         $"{youmayplacethecoverimagesforthissystem}\n\n", info, MessageBoxButton.OK, MessageBoxImage.Information);
    //     }
    // }

    internal static void FolderCreationFailedMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var simpleLauncherfailedtocreatethe = (string)Application.Current.TryFindResource("SimpleLauncherfailedtocreatethe") ?? "'Simple Launcher' failed to create the necessary folders for this system.";
            var grantSimpleLauncheradministrative = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
            var temporarilydisableyourantivirus = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
            var ensurethattheSimpleLauncherfolderislocatedinawritable = (string)Application.Current.TryFindResource("EnsurethattheSimpleLauncherfolderislocatedinawritable") ?? "Ensure that the 'Simple Launcher' folder is located in a writable directory.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show($"{simpleLauncherfailedtocreatethe}\n\n" +
                                           $"{grantSimpleLauncheradministrative}\n\n" +
                                           $"{temporarilydisableyourantivirus}\n\n" +
                                           $"{ensurethattheSimpleLauncherfolderislocatedinawritable}", info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void SelectASystemToDeleteMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var pleaseselectasystemtodelete = (string)Application.Current.TryFindResource("Pleaseselectasystemtodelete") ?? "Please select a system to delete.";
            var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            System.Windows.MessageBox.Show(pleaseselectasystemtodelete, warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void SystemNotFoundInTheXmlMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var selectedsystemnotfound = (string)Application.Current.TryFindResource("Selectedsystemnotfound") ?? "Selected system not found in the XML document!";
            var alert = (string)Application.Current.TryFindResource("Alert") ?? "Alert";
            System.Windows.MessageBox.Show(selectedsystemnotfound, alert, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }

    internal static void ErrorFindingGameFilesMessageBox(string logPath)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var therewasanerrorfinding = (string)Application.Current.TryFindResource("Therewasanerrorfinding") ?? "There was an error finding the game files.";
            var doyouwanttoopenthefileerroruserlog = (string)Application.Current.TryFindResource("Doyouwanttoopenthefileerroruserlog") ?? "Do you want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = System.Windows.MessageBox.Show($"{therewasanerrorfinding}\n\n" +
                                                        $"{doyouwanttoopenthefileerroruserlog}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
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
                    var thefileerroruserlogwas = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ?? "The file 'error_user.log' was not found!";
                    System.Windows.MessageBox.Show(thefileerroruserlogwas, error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    internal static void GamePadErrorMessageBox(string logPath)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var therewasanerrorwiththeGamePadController = (string)Application.Current.TryFindResource("TherewasanerrorwiththeGamePadController") ?? "There was an error with the GamePad Controller.";
            var grantSimpleLauncheradministrative = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
            var temporarilydisableyourantivirus = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
            var doyouwanttoopenthefile = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = System.Windows.MessageBox.Show($"{therewasanerrorwiththeGamePadController}\n\n" +
                                                        $"{grantSimpleLauncheradministrative}\n\n" +
                                                        $"{temporarilydisableyourantivirus}\n\n" +
                                                        $"{doyouwanttoopenthefile}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
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
                    System.Windows.MessageBox.Show(thefileerroruserlogwas,
                        error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    internal static Task CouldNotLaunchGameMessageBox(string logPath)
    {
        return Application.Current.Dispatcher.InvokeAsync(() =>

        {
            var simpleLaunchercouldnotlaunch = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotlaunch") ?? "'Simple Launcher' could not launch the selected game.";
            var makesuretheRoMorIsOyouretrying = (string)Application.Current.TryFindResource("MakesuretheROMorISOyouretrying") ?? "Make sure the ROM or ISO you're trying to run is not corrupted.";
            var ifyouaretryingtorunMamEensurethatyourRom = (string)Application.Current.TryFindResource("IfyouaretryingtorunMAMEensurethatyourROM") ?? "If you are trying to run MAME, ensure that your ROM collection is compatible with the MAME version you are using.";
            var ifyouaretryingtorunRetroarchensurethattheBios = (string)Application.Current.TryFindResource("IfyouaretryingtorunRetroarchensurethattheBIOS") ?? "If you are trying to run Retroarch, ensure that the BIOS or required files for the core are installed.";
            var alsomakesureyouarecallingtheemulator = (string)Application.Current.TryFindResource("Alsomakesureyouarecallingtheemulator") ?? "Also, make sure you are calling the emulator with the correct parameter.";
            var youcanturnoffthistypeoferrormessageinExpertmode = (string)Application.Current.TryFindResource("YoucanturnoffthiserrormessageinExpertmode") ?? "You can turn off this error message in Expert mode.";
            var doyouwanttoopenthefile = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = System.Windows.MessageBox.Show($"{simpleLaunchercouldnotlaunch}\n\n" +
                                                        $"{makesuretheRoMorIsOyouretrying}\n" +
                                                        $"{ifyouaretryingtorunMamEensurethatyourRom}\n" +
                                                        $"{ifyouaretryingtorunRetroarchensurethattheBios}\n" +
                                                        $"{alsomakesureyouarecallingtheemulator}\n\n" +
                                                        $"{youcanturnoffthistypeoferrormessageinExpertmode}\n\n" +
                                                        $"{doyouwanttoopenthefile}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
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
                    var thefileerroruserlogwas = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ?? "The file 'error_user.log' was not found!";
                    System.Windows.MessageBox.Show(thefileerroruserlogwas, error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }).Task;
    }

    internal static Task InvalidOperationExceptionMessageBox(string logPath)
    {
        return Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var failedtostarttheemulator = (string)Application.Current.TryFindResource("Failedtostarttheemulator") ?? "Failed to start the emulator or it has not exited as expected.";
            var checktheintegrityoftheemulatoranditsdependencies = (string)Application.Current.TryFindResource("Checktheintegrityoftheemulatoranditsdependencies") ?? "Check the integrity of the emulator and its dependencies.";
            var youcanturnoffthistypeoferrormessageinExpertmode = (string)Application.Current.TryFindResource("YoucanturnoffthiserrormessageinExpertmode") ?? "You can turn off this error message in Expert mode.";
            var doyouwanttoopenthefile = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            var result = System.Windows.MessageBox.Show($"{failedtostarttheemulator}\n\n" +
                                                        $"{checktheintegrityoftheemulatoranditsdependencies}\n\n" +
                                                        $"{youcanturnoffthistypeoferrormessageinExpertmode}\n\n" +
                                                        $"{doyouwanttoopenthefile}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
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
                    System.Windows.MessageBox.Show(thefileerroruserlog,
                        error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }).Task;
    }

    internal static void ThereWasAnErrorLaunchingThisGameMessageBox(string logPath)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var therewasanerrorlaunchingthisgame = (string)Application.Current.TryFindResource("Therewasanerrorlaunchingthisgame") ?? "There was an error launching this game.";
            var youcanturnoffthistypeoferrormessageinExpertmode = (string)Application.Current.TryFindResource("YoucanturnoffthiserrormessageinExpertmode") ?? "You can turn off this error message in Expert mode.";
            var doyouwanttoopenthefileerroruserlog = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = System.Windows.MessageBox.Show($"{therewasanerrorlaunchingthisgame}\n\n" +
                                                        $"{youcanturnoffthistypeoferrormessageinExpertmode}\n\n" +
                                                        $"{doyouwanttoopenthefileerroruserlog}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
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
                    System.Windows.MessageBox.Show(thefileerroruserlog, error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    internal static void NullFileExtensionMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var thereisnoExtension = (string)Application.Current.TryFindResource("ThereisnoExtension") ?? "There is no 'Extension to Launch After Extraction' set in the system configuration.";
            var pleaseeditthissystemto = (string)Application.Current.TryFindResource("Pleaseeditthissystemto") ?? "Please edit this system to fix that.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{thereisnoExtension}\n\n" +
                                           $"{pleaseeditthissystemto}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void CouldNotFindAFileMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var couldnotfindafilewiththeextensiondefined = (string)Application.Current.TryFindResource("Couldnotfindafilewiththeextensiondefined") ?? "Could not find a file with the extension defined in 'Extension to Launch After Extraction' inside the extracted folder.";
            var pleaseeditthissystemtofix = (string)Application.Current.TryFindResource("Pleaseeditthissystemto") ?? "Please edit this system to fix that.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{couldnotfindafilewiththeextensiondefined}\n\n" +
                                           $"{pleaseeditthissystemtofix}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static MessageBoxResult SearchOnlineForRomHistoryMessageBox()
    {
        return Application.Current.Dispatcher.Invoke(static () =>
        {
            var thereisnoRoMhistoryinthelocaldatabase = (string)Application.Current.TryFindResource("ThereisnoROMhistoryinthelocaldatabase") ?? "There is no ROM history in the local database for this file.";
            var doyouwanttosearchonline = (string)Application.Current.TryFindResource("Doyouwanttosearchonline") ?? "Do you want to search online for the ROM history?";
            var rOmHistoryNotFound = (string)Application.Current.TryFindResource("ROMHistorynotfound") ?? "ROM History not found";
            return System.Windows.MessageBox.Show($"{thereisnoRoMhistoryinthelocaldatabase}\n\n" +
                                                  $"{doyouwanttosearchonline}", rOmHistoryNotFound, MessageBoxButton.YesNo, MessageBoxImage.Question);
        });
    }

    internal static void SystemHasBeenDeletedMessageBox(string selectedSystemName)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var system = (string)Application.Current.TryFindResource("System") ?? "System";
            var hasbeendeleted = (string)Application.Current.TryFindResource("hasbeendeleted") ?? "has been deleted.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show($"{system} '{selectedSystemName}' {hasbeendeleted}", info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static MessageBoxResult AreYouSureDoYouWantToDeleteThisSystemMessageBox()
    {
        return Application.Current.Dispatcher.Invoke(static () =>
        {
            var areyousureyouwanttodeletethis = (string)Application.Current.TryFindResource("Areyousureyouwanttodeletethis") ?? "Are you sure you want to delete this system?";
            var confirmation = (string)Application.Current.TryFindResource("Confirmation") ?? "Confirmation";
            return System.Windows.MessageBox.Show(areyousureyouwanttodeletethis, confirmation, MessageBoxButton.YesNo, MessageBoxImage.Question);
        });
    }

    internal static void ThereWasAnErrorDeletingTheGameMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasanerrordeletingthefile = (string)Application.Current.TryFindResource("Therewasanerrordeletingthefile") ?? "There was an error deleting the file.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{therewasanerrordeletingthefile}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ThereWasAnErrorDeletingTheCoverImageMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasanerrordeletingthecoverimage = (string)Application.Current.TryFindResource("Therewasanerrordeletingthecoverimage") ?? "There was an error deleting the cover image.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{therewasanerrordeletingthecoverimage}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static MessageBoxResult AreYouSureYouWantToDeleteTheGameMessageBox(string fileNameWithExtension)
    {
        return Application.Current.Dispatcher.Invoke(() =>
        {
            var areyousureyouwanttodeletethefile = (string)Application.Current.TryFindResource("Areyousureyouwanttodeletethefile") ?? "Are you sure you want to delete the file";
            var thisactionwilldelete = (string)Application.Current.TryFindResource("Thisactionwilldelete") ?? "This action will delete the file from the HDD and cannot be undone.";
            var confirmDeletion = (string)Application.Current.TryFindResource("ConfirmDeletion") ?? "Confirm Deletion";
            return System.Windows.MessageBox.Show($"{areyousureyouwanttodeletethefile} '{fileNameWithExtension}'?\n\n" +
                                                  $"{thisactionwilldelete}", confirmDeletion, MessageBoxButton.YesNo, MessageBoxImage.Question);
        });
    }

    internal static MessageBoxResult AreYouSureYouWantToDeleteTheCoverImageMessageBox(string fileNameWithoutExtension)
    {
        return Application.Current.Dispatcher.Invoke(() =>
        {
            var areyousureyouwanttodeletethecoverimageof = (string)Application.Current.TryFindResource("Areyousureyouwanttodeletethecoverimageof") ?? "Are you sure you want to delete the cover image of";
            var thisactionwilldelete = (string)Application.Current.TryFindResource("Thisactionwilldelete") ?? "This action will delete the file from the HDD and cannot be undone.";
            var confirmDeletion = (string)Application.Current.TryFindResource("ConfirmDeletion") ?? "Confirm Deletion";
            return System.Windows.MessageBox.Show($"{areyousureyouwanttodeletethecoverimageof} '{fileNameWithoutExtension}'?\n\n" +
                                                  $"{thisactionwilldelete}", confirmDeletion, MessageBoxButton.YesNo, MessageBoxImage.Question);
        });
    }

    internal static MessageBoxResult WoulYouLikeToSaveAReportMessageBox()
    {
        return Application.Current.Dispatcher.Invoke(static () =>
        {
            var wouldyouliketosaveareport = (string)Application.Current.TryFindResource("Wouldyouliketosaveareport") ?? "Would you like to save a report with the results?";
            var saveReport = (string)Application.Current.TryFindResource("SaveReport") ?? "Save Report";
            return System.Windows.MessageBox.Show(wouldyouliketosaveareport, saveReport, MessageBoxButton.YesNo, MessageBoxImage.Question);
        });
    }

    internal static void SimpleLauncherWasUnableToRestoreBackupMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var simpleLauncherwasunabletorestore = (string)Application.Current.TryFindResource("SimpleLauncherwasunabletorestore") ?? "'Simple Launcher' was unable to restore the last backup.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show(simpleLauncherwasunabletorestore, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static MessageBoxResult WouldYouLikeToRestoreTheLastBackupMessageBox()
    {
        return Application.Current.Dispatcher.Invoke(static () =>
        {
            var icouldnotfindthefilesystemxml = (string)Application.Current.TryFindResource("Icouldnotfindthefilesystemxml") ?? "I could not find the file 'system.xml', which is required to start the application.";
            var butIfoundabackupfile = (string)Application.Current.TryFindResource("ButIfoundabackupfile") ?? "But I found a backup file.";
            var wouldyouliketorestore = (string)Application.Current.TryFindResource("Wouldyouliketorestore") ?? "Would you like to restore the last backup?";
            var restoreBackup = (string)Application.Current.TryFindResource("RestoreBackup") ?? "Restore Backup?";
            return System.Windows.MessageBox.Show($"{icouldnotfindthefilesystemxml}\n\n" +
                                                  $"{butIfoundabackupfile}\n\n" +
                                                  $"{wouldyouliketorestore}", restoreBackup, MessageBoxButton.YesNo, MessageBoxImage.Question);
        });
    }

    internal static void FailedToLoadLanguageResourceMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var failedtoloadlanguageresources = (string)Application.Current.TryFindResource("Failedtoloadlanguageresources") ?? "Failed to load language resources.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var languageLoadingError = (string)Application.Current.TryFindResource("LanguageLoadingError") ?? "Language Loading Error";
            System.Windows.MessageBox.Show($"{failedtoloadlanguageresources}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", languageLoadingError, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void InvalidSystemConfigurationMessageBox(string error)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var invalidSystemConfiguration = (string)Application.Current.TryFindResource("InvalidSystemConfiguration") ?? "Invalid System Configuration";
            System.Windows.MessageBox.Show(error, invalidSystemConfiguration, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void UnableToOpenLinkMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var unabletoopenthelink = (string)Application.Current.TryFindResource("Unabletoopenthelink") ?? "Unable to open the link.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{unabletoopenthelink}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void NoGameFoundInTheRandomSelectionMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var nogamesfoundtorandomlyselectfrom = (string)Application.Current.TryFindResource("Nogamesfoundtorandomlyselectfrom") ?? "No games found to randomly select from. Please check your system selection.";
            var feelingLucky = (string)Application.Current.TryFindResource("FeelingLucky") ?? "Feeling Lucky";
            System.Windows.MessageBox.Show(nogamesfoundtorandomlyselectfrom, feelingLucky, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void PleaseSelectASystemBeforeMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var pleaseselectasystembeforeusingtheFeeling = (string)Application.Current.TryFindResource("PleaseselectasystembeforeusingtheFeeling") ?? "Please select a system before using the Feeling Lucky feature.";
            var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            System.Windows.MessageBox.Show(pleaseselectasystembeforeusingtheFeeling, warning, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ToggleFuzzyMatchingFailureMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasanerrortogglingthefuzzymatchinglogic = (string)Application.Current.TryFindResource("Therewasanerrortogglingthefuzzymatchinglogic") ?? "There was an error toggling the fuzzy matching logic.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show(therewasanerrortogglingthefuzzymatchinglogic, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FuzzyMatchingErrorFailToSetThresholdMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var errorMessage = (string)Application.Current.TryFindResource("SetFuzzyMatchingThresholdFailureMessageBoxText") ?? "Failed to set fuzzy matching threshold.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show(errorMessage, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ListOfErrorsMessageBox(StringBuilder errorMessages)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var editSystemtofixit = (string)Application.Current.TryFindResource("EditSystemtofixit") ?? "Edit System to fix it.";
            var validationerrors = (string)Application.Current.TryFindResource("Validationerrors") ?? "Validation errors";
            var fullMessage = errorMessages + editSystemtofixit;
            System.Windows.MessageBox.Show(fullMessage, validationerrors, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ThereIsNoUpdateAvailableMessageBox(Window mainWindow, string currentVersion)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var thereisnoupdateavailable = (string)Application.Current.TryFindResource("thereisnoupdateavailable") ?? "There is no update available.";
            var thecurrentversionis = (string)Application.Current.TryFindResource("Thecurrentversionis") ?? "The current version is";
            var noupdateavailable = (string)Application.Current.TryFindResource("Noupdateavailable") ?? "No update available";
            System.Windows.MessageBox.Show(mainWindow, $"{thereisnoupdateavailable}\n\n" +
                                                       $"{thecurrentversionis} {currentVersion}", noupdateavailable, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void ErrorCheckingForUpdatesMessageBox(Window mainWindow)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var therewasanerrorcheckingforupdates = (string)Application.Current.TryFindResource("Therewasanerrorcheckingforupdates") ?? "There was an error checking for updates.";
            var maybethereisaproblemwithyourinternet = (string)Application.Current.TryFindResource("Maybethereisaproblemwithyourinternet") ?? "Maybe there is a problem with your internet access or the GitHub server is offline.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show(mainWindow, $"{therewasanerrorcheckingforupdates}\n\n" +
                                                       $"{maybethereisaproblemwithyourinternet}", error, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }

    internal static void AnotherInstanceIsRunningMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var anotherinstanceofSimpleLauncherisalreadyrunning = (string)Application.Current.TryFindResource("AnotherinstanceofSimpleLauncherisalreadyrunning") ?? "Another instance of 'Simple Launcher' is already running.";
            System.Windows.MessageBox.Show(anotherinstanceofSimpleLauncherisalreadyrunning, "Simple Launcher", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void FailedToStartSimpleLauncherMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var failedtostartSimpleLauncherAnerroroccurred = (string)Application.Current.TryFindResource("FailedtostartSimpleLauncherAnerroroccurred") ?? "Failed to start 'Simple Launcher'. An error occurred while checking for existing instances.";
            var simpleLauncherError = (string)Application.Current.TryFindResource("SimpleLauncherError") ?? "Simple Launcher Error";
            System.Windows.MessageBox.Show(failedtostartSimpleLauncherAnerroroccurred, simpleLauncherError, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FailedToRestartMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var failedtorestarttheapplication = (string)Application.Current.TryFindResource("Failedtorestarttheapplication") ?? "Failed to restart the application.";
            var restartError = (string)Application.Current.TryFindResource("RestartError") ?? "Restart Error";
            System.Windows.MessageBox.Show(failedtorestarttheapplication, restartError, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static MessageBoxResult DoYouWantToUpdateMessageBox(string currentVersion, string latestVersion, Window window)
    {
        return Application.Current.Dispatcher.Invoke(() =>
        {
            var thereIsAsoftwareUpdateAvailable = (string)Application.Current.TryFindResource("Thereisasoftwareupdateavailable") ?? "There is a software update available.";
            var theCurrentVersionIs = (string)Application.Current.TryFindResource("Thecurrentversionis") ?? "The current version is";
            var theUpdateVersionIs = (string)Application.Current.TryFindResource("Theupdateversionis") ?? "The update version is";
            var doYouWantToDownloadAndInstall = (string)Application.Current.TryFindResource("Doyouwanttodownloadandinstall") ?? "Do you want to download and install the latest version automatically?";
            var updateAvailable = (string)Application.Current.TryFindResource("UpdateAvailable") ?? "Update Available";
            return System.Windows.MessageBox.Show(window, $"{thereIsAsoftwareUpdateAvailable}\n" +
                                                          $"{theCurrentVersionIs} {currentVersion}\n" +
                                                          $"{theUpdateVersionIs} {latestVersion}\n\n" +
                                                          $"{doYouWantToDownloadAndInstall}", updateAvailable, MessageBoxButton.YesNo, MessageBoxImage.Information);
        });
    }

    internal static void HandleMissingRequiredFilesMessageBox(string fileList)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var thefollowingrequiredfilesaremissing = (string)Application.Current.TryFindResource("Thefollowingrequiredfilesaremissing") ?? "The following required file(s) are missing:";
            var missingRequiredFiles = (string)Application.Current.TryFindResource("MissingRequiredFiles") ?? "Missing Required Files";
            var doyouwanttoreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
            var reinstall = System.Windows.MessageBox.Show($"{thefollowingrequiredfilesaremissing}\n" +
                                                           $"{fileList}\n\n" +
                                                           $"{doyouwanttoreinstallSimpleLauncher}", missingRequiredFiles, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (reinstall == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                var pleasereinstallSimpleLauncher = (string)Application.Current.TryFindResource("PleasereinstallSimpleLauncher") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
                var theapplicationwillshutdown = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
                System.Windows.MessageBox.Show($"{pleasereinstallSimpleLauncher}\n\n{theapplicationwillshutdown}", missingRequiredFiles, MessageBoxButton.OK, MessageBoxImage.Error);

                QuitSimpleLauncher.SimpleQuitApplication();
            }
        }
    }

    internal static void HandleApiConfigErrorMessageBox(string reason)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var apiConfigErrorTitle = (string)Application.Current.TryFindResource("ApiConfigErrorTitle") ?? "API Configuration Error";
            var apiConfigErrorMessage = (string)Application.Current.TryFindResource("ApiConfigErrorMessage") ?? "'Simple Launcher' encountered an error loading its API configuration.";
            var reasonLabel = (string)Application.Current.TryFindResource("ReasonLabel") ?? "Reason:";
            var reinstallSuggestion = (string)Application.Current.TryFindResource("ReinstallSuggestion") ?? "This might prevent some features (like automatic bug reporting) from working correctly. Would you like to reinstall 'Simple Launcher' to fix this?";

            var result = System.Windows.MessageBox.Show($"{apiConfigErrorMessage}\n\n" +
                                                        $"{reasonLabel} {reason}\n\n" +
                                                        $"{reinstallSuggestion}", apiConfigErrorTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                var manualReinstallSuggestion = (string)Application.Current.TryFindResource("ManualReinstallSuggestion") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
                var applicationWillShutdown = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";

                System.Windows.MessageBox.Show($"{manualReinstallSuggestion}\n\n" +
                                               $"{applicationWillShutdown}", apiConfigErrorTitle, MessageBoxButton.OK, MessageBoxImage.Error);

                QuitSimpleLauncher.SimpleQuitApplication();
            }
        }
    }

    internal static void DiskSpaceErrorMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var notenoughdiskspaceforextraction = (string)Application.Current.TryFindResource("Notenoughdiskspaceforextraction") ?? "Not enough disk space for extraction.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show(notenoughdiskspaceforextraction, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void CouldNotCheckForDiskSpaceMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotcheckdiskspace") ?? "'Simple Launcher' could not check disk space for the specified path. Please check the path and try again.";
            var caption = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SaveSystemFailedMessageBox(string details = null)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var failedToSaveSystem = (string)Application.Current.TryFindResource("FailedToSaveSystem") ?? "Failed to save system configuration.";
            var checkPermissions = (string)Application.Current.TryFindResource("CheckFilePermissions") ?? "Please check file permissions and ensure the file is not locked.";
            var errorDetails = (string)Application.Current.TryFindResource("ErrorDetails") ?? "Details:";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var message = $"{failedToSaveSystem}\n\n" +
                          $"{checkPermissions}";

            if (!string.IsNullOrEmpty(details))
            {
                message += $"\n\n{errorDetails} {details}";
            }

            System.Windows.MessageBox.Show(message, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void CouldNotOpenTheDownloadLink()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var simpleLaunchercouldnotopenthedownloadlink = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotopenthedownloadlink") ?? "'Simple Launcher' could not open the download link.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show(simpleLaunchercouldnotopenthedownloadlink, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorLoadingAppSettingsMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var therewasanerrorloadingconfiguration = (string)Application.Current.TryFindResource("Therewasanerrorloadingconfiguration") ?? "There was an error loading 'appsettings.json'.";
            var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{therewasanerrorloadingconfiguration}\n\n" +
                                           $"{theerrorwasreportedtothedeveloper}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void PotentialPathManipulationDetectedMessageBox(string archivePath)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var title = (string)Application.Current.TryFindResource("SecurityWarning") ?? "Security Warning";
            var pathManipulationDetected = (string)Application.Current.TryFindResource("PathManipulationDetected") ?? "Potential Path Manipulation Detected";
            var zipSlipExplanation = (string)Application.Current.TryFindResource("ZipSlipExplanation") ?? "A security vulnerability called 'Zip Slip' was detected in the archive file. This is a path traversal vulnerability that could allow an attacker to write files outside of the intended extraction directory.";
            var archivePathMessage = (string)Application.Current.TryFindResource("ArchivePathMessage") ?? "Archive file:";
            var actionTaken = (string)Application.Current.TryFindResource("ActionTaken") ?? "For your security, the extraction process has been properly handle and the issue has been logged.";
            var reportedToDeveloper = (string)Application.Current.TryFindResource("ReportedToDeveloper") ?? "This security issue has been reported to the developer team.";
            System.Windows.MessageBox.Show($"{pathManipulationDetected}\n\n" +
                                           $"{zipSlipExplanation}\n\n" +
                                           $"{archivePathMessage} {archivePath}\n\n" +
                                           $"{actionTaken}\n\n" +
                                           $"{reportedToDeveloper}", title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void CouldNotOpenSoundConfigurationWindow()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var couldNotOpenSoundConfigurationWindow = (string)Application.Current.TryFindResource("CouldNotOpenSoundConfigurationWindow") ?? "Could not open sound configuration window";
            var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            System.Windows.MessageBox.Show(couldNotOpenSoundConfigurationWindow, warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void ErrorSettingSoundFile()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var errorSettingSoundFile = (string)Application.Current.TryFindResource("errorSettingSoundFile") ?? "Error choosing or copying sound file.";
            var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            System.Windows.MessageBox.Show(errorSettingSoundFile, warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void NotificationSoundIsDisable()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var notificationSoundIsDisable = (string)Application.Current.TryFindResource("NotificationSoundIsDisable") ?? "Notification sound is disable";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show(notificationSoundIsDisable, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void NoSoundFileIsSelected()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var noSoundFileSelectedWarning = (string)Application.Current.TryFindResource("NoSoundFileSelectedWarning") ?? "No sound file is selected.";
            var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            System.Windows.MessageBox.Show(noSoundFileSelectedWarning, warning, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void SettingsSavedSuccessfully()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var settingsSavedSuccessfully = (string)Application.Current.TryFindResource("SettingsSavedSuccessfully") ?? "Settings saved successfully.";
            var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
            System.Windows.MessageBox.Show(settingsSavedSuccessfully, info, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void FilePathIsInvalid(string logPath)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var simpleLaunchercouldnotlaunch = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotlaunch") ?? "'Simple Launcher' could not launch the selected game.";
            var thefilepathisinvalid = (string)Application.Current.TryFindResource("Thefilepathisinvalid") ?? "The filepath is invalid or the file does not exist!";
            var avoidusingspecialcharactersinthefilepath = (string)Application.Current.TryFindResource("Avoidusingspecialcharactersinthefilepath") ?? "Avoid using special characters in the filepath, such as @, !, ?, ~, or any other special characters.";
            var youcanturnoffthistypeoferrormessageinExpertmode = (string)Application.Current.TryFindResource("YoucanturnoffthiserrormessageinExpertmode") ?? "You can turn off this error message in Expert mode.";
            var doyouwanttoopenthefile = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = System.Windows.MessageBox.Show($"{simpleLaunchercouldnotlaunch}\n\n" +
                                                        $"{thefilepathisinvalid}\n\n" +
                                                        $"{avoidusingspecialcharactersinthefilepath}\n\n" +
                                                        $"{youcanturnoffthistypeoferrormessageinExpertmode}\n\n" +
                                                        $"{doyouwanttoopenthefile}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
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
                    var thefileerroruserlogwas = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ?? "The file 'error_user.log' was not found!";
                    System.Windows.MessageBox.Show(thefileerroruserlogwas, error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    internal static void ThereWasAnErrorMountingTheFile(string logPath)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var simpleLaunchercouldnotmount = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotmount") ?? "'Simple Launcher' could not mount the selected game.";
            var thismaybeduetoDokannotbeinginstalled = (string)Application.Current.TryFindResource("ThismaybeduetoDokannotbeinginstalled") ?? "This may be due to Dokan not being installed. Dokan is required for mounting ZIP and disk image files.";
            var youcandownloadDokanfrom = (string)Application.Current.TryFindResource("YoucandownloadDokanfrom") ?? "You can download Dokan from: https://github.com/dokan-dev/dokany";
            var doyouwanttoopenthefile = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = System.Windows.MessageBox.Show($"{simpleLaunchercouldnotmount}\n\n" +
                                                        $"{thismaybeduetoDokannotbeinginstalled}\n\n" +
                                                        $"{youcandownloadDokanfrom}\n\n" +
                                                        $"{doyouwanttoopenthefile}", error, MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
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
                    var thefileerroruserlogwas = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ?? "The file 'error_user.log' was not found!";
                    System.Windows.MessageBox.Show(thefileerroruserlogwas, error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    internal static void LaunchToolInformation(string info)
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        void ShowMessage()
        {
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show(info, error, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void CannotScreenshotMinimizedWindowMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var cannottakeascreenshotofaminimizedwindow = (string)Application.Current.TryFindResource("Cannottakeascreenshotofaminimizedwindow") ?? "Cannot take a screenshot of a minimized window.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show(cannottakeascreenshotofaminimizedwindow, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FailedToCopyLogContent()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var failedtocopylogcontent = (string)Application.Current.TryFindResource("Failedtocopylogcontent") ?? "Failed to copy log content.";
            var copyError = (string)Application.Current.TryFindResource("CopyError") ?? "Copy Error";
            System.Windows.MessageBox.Show(failedtocopylogcontent, copyError, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void CouldNotFindUpdaterOnGitHub()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var simpleLaunchercouldnotfindtheupdater = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotfindtheupdater") ?? "'Simple Launcher' could not find the updater application on GitHub.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show(simpleLaunchercouldnotfindtheupdater, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void CouldNotOpenAchievementsWindowMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var couldNotOpenAchievementsWindow = (string)Application.Current.TryFindResource("CouldNotOpenAchievementsWindow") ?? "Could not open the achievements window.";
            var theErrorWasReported = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{couldNotOpenAchievementsWindow}\n\n{theErrorWasReported}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static MessageBoxResult GameNotSupportedByRetroAchievementsMessageBox()
    {
        return Application.Current.Dispatcher.Invoke(static () =>
        {
            var message1 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotcalculate") ?? "'Simple Launcher' could not calculate the hash value of this game or this game is not yet supported by RetroAchievements.";
            var message2 = (string)Application.Current.TryFindResource("DoyouwanttoopentheglobalRetroAchievements") ?? "Do you want to open the global RetroAchievements window?";
            var title = (string)Application.Current.TryFindResource("RetroAchievements") ?? "RetroAchievements";
            return System.Windows.MessageBox.Show($"{message1}\n\n" +
                                                  $"{message2}", title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        });
    }

    internal static void GameLaunchTimeoutMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var gamelaunchtimedoutPleasetryagainorcheckiftheemulatorstarted = (string)Application.Current.TryFindResource("GamelaunchtimedoutPleasetryagainorcheckiftheemulatorstarted") ?? "Game launch timed out. Please try again or check if the emulator started.";
            var gamelaunchtimedout = (string)Application.Current.TryFindResource("Gamelaunchtimedout") ?? "Game launch timed out";
            System.Windows.MessageBox.Show(gamelaunchtimedoutPleasetryagainorcheckiftheemulatorstarted, gamelaunchtimedout, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void AddRaLogin()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var youneedtoaddRetroAchievementlogin = (string)Application.Current.TryFindResource("YouneedtoaddRetroAchievementlogin") ?? "You need to add RetroAchievement login information to use this feature.";
            var attention = (string)Application.Current.TryFindResource("Attention") ?? "Attention";
            System.Windows.MessageBox.Show(youneedtoaddRetroAchievementlogin, attention, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void NoDefaultBrowserConfiguredMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var noDefaultBrowserConfiguredMessage = (string)Application.Current.TryFindResource("NoDefaultBrowserConfiguredMessage") ?? "Your operating system does not have a default web browser configured. Please set one in Windows Settings (Apps > Default apps) to open web links.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show(noDefaultBrowserConfiguredMessage, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static MessageBoxResult WarnUserAboutMemoryConsumption()
    {
        return Application.Current.Dispatcher.Invoke(ShowMessage);

        static MessageBoxResult ShowMessage()
        {
            var warningMessage = (string)Application.Current.TryFindResource("WarningSettingupaveryhighnumberofgamesperpage") ?? "Warning! Setting a very high number of games per page will significantly increase system memory usage when in Grid mode. If the number is too high, this may cause the application to crash. Please proceed with caution.";
            var proceedQuestion = (string)Application.Current.TryFindResource("AreYouSureYouWantToProceed") ?? "Are you sure you want to proceed?";
            var warningTitle = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            return System.Windows.MessageBox.Show($"{warningMessage}\n\n{proceedQuestion}", warningTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        }
    }

    internal static void GroupByFolderOnlyForMameMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message = (string)Application.Current.TryFindResource("GroupByFolderOnlyForMameMessage") ?? "The 'Group Files by Folder' option is only compatible with MAME emulators. To use a different emulator, please edit the system settings and disable this option.";
            var title = (string)Application.Current.TryFindResource("CompatibilityWarning") ?? "Compatibility Warning";
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static MessageBoxResult GroupByFolderMameWarningMessageBox()
    {
        return Application.Current.Dispatcher.Invoke(static () =>
        {
            var message = (string)Application.Current.TryFindResource("GroupByFolderMameWarningMessage") ?? "You have enabled 'Group Files by Folder' but have configured a non-MAME emulator. This combination is not supported and will fail at launch. Are you sure you want to save these settings?";
            var title = (string)Application.Current.TryFindResource("ConfigurationWarning") ?? "Configuration Warning";
            return System.Windows.MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        });
    }

    internal static MessageBoxResult FirstRunWelcomeMessageBox()
    {
        return Application.Current.Dispatcher.Invoke(static () =>
        {
            var welcomeToSimpleLauncher = (string)Application.Current.TryFindResource("WelcomeToSimpleLauncher") ?? "Welcome to 'Simple Launcher'!";
            var noSystemsFound = (string)Application.Current.TryFindResource("NoSystemsFound") ?? "No systems were found in your configuration.";
            var easyModeGuide = (string)Application.Current.TryFindResource("DoyouwanttoaddyourfirstsystemusingtheEasyMode") ?? "Do you want to add your first system using the Easy Mode?";
            var welcome = (string)Application.Current.TryFindResource("Welcome") ?? "Welcome";
            return System.Windows.MessageBox.Show($"{welcomeToSimpleLauncher}\n\n" +
                                                  $"{noSystemsFound}\n\n" +
                                                  $"{easyModeGuide}", welcome, MessageBoxButton.YesNo, MessageBoxImage.Question);
        });
    }

    internal static void Emulator1LocationRequiredMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message = (string)Application.Current.TryFindResource("Emulator1pathisrequired") ?? "Emulator 1 path is required.";
            var title = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void Emulator2LocationRequiredMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message = (string)Application.Current.TryFindResource("Emulator2pathisrequired") ?? "Emulator 2 path is required.";
            var title = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void Emulator3LocationRequiredMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message = (string)Application.Current.TryFindResource("Emulator3pathisrequired") ?? "Emulator 3 path is required.";
            var title = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void Emulator4LocationRequiredMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message = (string)Application.Current.TryFindResource("Emulator4pathisrequired") ?? "Emulator 4 path is required.";
            var title = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void Emulator5LocationRequiredMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message = (string)Application.Current.TryFindResource("Emulator5pathisrequired") ?? "Emulator 5 path is required.";
            var title = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void ImagePackDownloaderUnavailableMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotaccesstheWebAPI") ?? "'Simple Launcher' could not access the Web API to download the updated URLs. Please try again later.";
            var title = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void EasyModeUnavailableMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotaccesstheWebAPI") ?? "'Simple Launcher' could not access the Web API to download the updated URLs. Please try again later.";
            var title = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SimpleLauncherDoesNotSupportRaHashOfSystemGroupedByFolder()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var simpleLauncherdoesnotsupportRetroAchievementshashofSystems = (string)Application.Current.TryFindResource("simpleLauncherdoesnotsupportRetroAchievementshashofSystems") ?? "'Simple Launcher' does not support RetroAchievements hash of systems Grouped by Folder.";
            var pleaseedittheSystemsettingsanddisablethe = (string)Application.Current.TryFindResource("pleaseedittheSystemsettingsanddisablethe") ?? "Please edit the system settings and disable the 'Group Files by Folder' option.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show($"{simpleLauncherdoesnotsupportRetroAchievementshashofSystems}\n\n" +
                                           $"{pleaseedittheSystemsettingsanddisablethe}", error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void UnsupportedArchitectureMessageBox()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var simpleLauncherdoesnotsupportthecurrentprocessorarchitecture = (string)Application.Current.TryFindResource("SimpleLauncherdoesnotsupportthecurrentprocessorarchitecture") ?? "'Simple Launcher' does not support the current processor architecture. We only support 64-bit (x64) or ARM64. The application will now close.";
            var unsupportedArchitecture = (string)Application.Current.TryFindResource("UnsupportedArchitecture") ?? "Unsupported Architecture";
            System.Windows.MessageBox.Show(simpleLauncherdoesnotsupportthecurrentprocessorarchitecture, unsupportedArchitecture, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SevenZipDllNotFoundMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var the7Zdllismissingfromtheapplicationfolder = (string)Application.Current.TryFindResource("The7zdllismissingfromtheapplicationfolder") ?? "The 7z dll is missing from the application folder!";
            var doyouwanttoreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var reinstall = System.Windows.MessageBox.Show($"{the7Zdllismissingfromtheapplicationfolder}\n\n" + $"{doyouwanttoreinstallSimpleLauncher}", error, MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (reinstall == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                var pleasereinstallSimpleLauncher = (string)Application.Current.TryFindResource("PleasereinstallSimpleLauncher") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
                System.Windows.MessageBox.Show(pleasereinstallSimpleLauncher, error, MessageBoxButton.OK, MessageBoxImage.Error);

                Application.Current.Shutdown();
            }
        }
    }

    internal static void FailedToInitializeSevenZipMessageBox()
    {
        Application.Current.Dispatcher.InvokeAsync(ShowMessage);
        return;

        static void ShowMessage()
        {
            var anunexpectederroroccurredwhileinitializingthe7Ziplibrary = (string)Application.Current.TryFindResource("Anunexpectederroroccurredwhileinitializingthe7Ziplibrary") ?? "An unexpected error occurred while initializing the 7-Zip library.";
            var doyouwanttoreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var reinstall = System.Windows.MessageBox.Show($"{anunexpectederroroccurredwhileinitializingthe7Ziplibrary}\n\n" + $"{doyouwanttoreinstallSimpleLauncher}", error, MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (reinstall == MessageBoxResult.Yes)
            {
                ReinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                var pleasereinstallSimpleLauncher = (string)Application.Current.TryFindResource("PleasereinstallSimpleLauncher") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
                System.Windows.MessageBox.Show(pleasereinstallSimpleLauncher, error, MessageBoxButton.OK, MessageBoxImage.Error);

                Application.Current.Shutdown();
            }
        }
    }

    internal static async Task ShowExtractionFailedMessageBoxAsync(string tempFolderPath)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var extractionFailedTitle = (string)Application.Current.TryFindResource("ExtractionFailedTitle") ?? "Extraction Failed";
            var extractionFailedMessage = (string)Application.Current.TryFindResource("ExtractionFailedMessage") ?? "The file was downloaded successfully, but automatic extraction failed. This can happen if an antivirus program is scanning or locking the file.";
            var openTempFolderQuestion = (string)Application.Current.TryFindResource("OpenTempFolderQuestion") ?? "Would you like to open the temporary folder to inspect the file?";
            var result = System.Windows.MessageBox.Show($"{extractionFailedMessage}\n\n{openTempFolderQuestion}", extractionFailedTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tempFolderPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    var errorOpeningFolderTitle = (string)Application.Current.TryFindResource("ErrorOpeningFolderTitle") ?? "Error Opening Folder";
                    var errorOpeningFolderMessage = (string)Application.Current.TryFindResource("ErrorOpeningFolderMessage") ?? "Could not open the temporary folder.";
                    System.Windows.MessageBox.Show(errorOpeningFolderMessage, errorOpeningFolderTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to open temp folder: {tempFolderPath}");
                }
            }
        });
    }

    internal static async Task ShowDownloadFileLockedMessageBoxAsync(string tempFolderPath)
    {
        await Application.Current.Dispatcher.InvokeAsync(() =>
        {
            var downloadFailedTitle = (string)Application.Current.TryFindResource("DownloadFailedTitle") ?? "Download Failed";
            var downloadFileLockedMessage = (string)Application.Current.TryFindResource("DownloadFileLockedMessage") ?? "The download could not be completed because the temporary file is locked by another process (e.g., antivirus software).";
            var openTempFolderQuestion = (string)Application.Current.TryFindResource("OpenTempFolderQuestion") ?? "Would you like to open the temporary folder to inspect the file?";
            var result = System.Windows.MessageBox.Show($"{downloadFileLockedMessage}\n\n{openTempFolderQuestion}", downloadFailedTitle, MessageBoxButton.YesNo, MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tempFolderPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    var errorOpeningFolderTitle = (string)Application.Current.TryFindResource("ErrorOpeningFolderTitle") ?? "Error Opening Folder";
                    var errorOpeningFolderMessage = (string)Application.Current.TryFindResource("ErrorOpeningFolderMessage") ?? "Could not open the temporary folder.";
                    System.Windows.MessageBox.Show(errorOpeningFolderMessage, errorOpeningFolderTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to open temp folder: {tempFolderPath}");
                }
            }
        });
    }

    internal static void ShowCustomMessageBox(string message, string launchError, string logPath)
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        void ShowMessage()
        {
            var therewasanerrorlaunchingtheselected = (string)Application.Current.TryFindResource("Therewasanerrorlaunchingtheselected") ?? "There was an error launching the selected game.";
            var dowanttoopenthefileerroruserlog = (string)Application.Current.TryFindResource("Dowanttoopenthefileerroruserlog") ?? "Do want to open the file 'error_user.log' to debug the error?";

            var result = System.Windows.MessageBox.Show($"{therewasanerrorlaunchingtheselected}\n\n" +
                                                        $"{message}\n\n" +
                                                        $"{dowanttoopenthefileerroruserlog}", launchError, MessageBoxButton.YesNo, MessageBoxImage.Error);

            if (result == MessageBoxResult.Yes)
            {
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
                    var thefileerroruserlogwasnotfound = (string)Application.Current.TryFindResource("Thefileerroruserlogwasnotfound") ?? "The file 'error_user.log' was not found!";

                    System.Windows.MessageBox.Show(thefileerroruserlogwasnotfound, launchError, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    internal static void EnterValidSearchTerms()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message = (string)Application.Current.TryFindResource("EnterValidSearchTerms") ?? "Please enter valid search terms.";
            var title = (string)Application.Current.TryFindResource("InvalidSearch") ?? "Invalid Search";
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void OperationCancelled()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message = (string)Application.Current.TryFindResource("OperationCancelledMessage") ?? "The operation was cancelled.";
            var title = (string)Application.Current.TryFindResource("OperationCancelled") ?? "Operation Cancelled";
            System.Windows.MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static MessageBoxResult DoYouWantToCancelAndClose()
    {
        return Application.Current.Dispatcher.Invoke(ShowMessage);

        static MessageBoxResult ShowMessage()
        {
            var message = (string)Application.Current.TryFindResource("ProcessingStillRunningMessage") ?? "Processing is still running. Do you want to cancel and close?";
            var title = (string)Application.Current.TryFindResource("ConfirmClose") ?? "Confirm Close";
            return System.Windows.MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        }
    }

    internal static void CouldNotOpenBrowserForAiSupport()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message = (string)Application.Current.TryFindResource("CouldnotopenbrowserforAIsupport") ?? "Could not open browser for AI support.";
            var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show(message, error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void PowerShellExecutionPolicyRestrictions()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var unabletoscanMicrosoftStoregames = (string)Application.Current.TryFindResource("UnabletoscanMicrosoftStoregames") ?? "Unable to scan Microsoft Store games due to PowerShell execution policy restrictions.";
            var thisistypicallycausedbyGroupPolicy = (string)Application.Current.TryFindResource("ThisistypicallycausedbyGroupPolicy") ?? "This is typically caused by Group Policy settings on corporate or managed PCs.";
            var simpleLaunchercannotperform = (string)Application.Current.TryFindResource("SimpleLaunchercannotperform") ?? "'Simple Launcher' cannot perform the requested task.";
            var powerShellRestricted = (string)Application.Current.TryFindResource("PowerShellRestricted") ?? "PowerShell Restricted";
            System.Windows.MessageBox.Show($"{unabletoscanMicrosoftStoregames}\n\n" +
                                           $"{thisistypicallycausedbyGroupPolicy}\n\n" +
                                           $"{simpleLaunchercannotperform}", powerShellRestricted, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void UnabletomountIsOfile()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var unabletomountIsOfile = (string)Application.Current.TryFindResource("UnabletomountISOfile") ?? "Unable to mount ISO file due to PowerShell execution policy restrictions.";
            var thisistypicallycausedbyGroup = (string)Application.Current.TryFindResource("ThisistypicallycausedbyGroup") ?? "This is typically caused by Group Policy settings on corporate or managed PCs.";
            var simpleLaunchercannotperform = (string)Application.Current.TryFindResource("SimpleLaunchercannotperform") ?? "'Simple Launcher' cannot perform the requested task.";
            var powerShellRestricted = (string)Application.Current.TryFindResource("PowerShellRestricted") ?? "PowerShell Restricted";
            System.Windows.MessageBox.Show($"{unabletomountIsOfile}\n\n" +
                                           $"{thisistypicallycausedbyGroup}\n\n" +
                                           $"{simpleLaunchercannotperform}", powerShellRestricted, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void UnabletoDismountIsOfile()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var unabletodismountIsOfile = (string)Application.Current.TryFindResource("UnabletoDismountISOfile") ?? "Unable to dismount ISO file due to PowerShell execution policy restrictions.";
            var thisistypicallycausedbyGroup = (string)Application.Current.TryFindResource("ThisistypicallycausedbyGroup") ?? "This is typically caused by Group Policy settings on corporate or managed PCs.";
            var simpleLaunchercannotperform = (string)Application.Current.TryFindResource("SimpleLaunchercannotperform") ?? "'Simple Launcher' cannot perform the requested task.";
            var powerShellRestricted = (string)Application.Current.TryFindResource("PowerShellRestricted") ?? "PowerShell Restricted";
            System.Windows.MessageBox.Show($"{unabletodismountIsOfile}\n\n" +
                                           $"{thisistypicallycausedbyGroup}\n\n" +
                                           $"{simpleLaunchercannotperform}", powerShellRestricted, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void ApplicationControlPolicyBlockedMessageBox()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message = (string)Application.Current.TryFindResource("ApplicationControlPolicyBlockedFile") ?? "An application control policy blocked this file or link.";
            var simpleLaunchercannotperform = (string)Application.Current.TryFindResource("SimpleLaunchercannotperform") ?? "'Simple Launcher' cannot perform the requested task.";
            var securityPolicyBlocked = (string)Application.Current.TryFindResource("SecurityPolicyBlocked") ?? "Security Policy Blocked";
            System.Windows.MessageBox.Show($"{message}\n\n" +
                                           $"{simpleLaunchercannotperform}\n\n", securityPolicyBlocked, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void ApplicationControlPolicyBlockedManualLinkMessageBox(string url)
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        void ShowMessage()
        {
            var message = (string)Application.Current.TryFindResource("ApplicationControlPolicyBlockedFileManualLink") ?? "An application control policy blocked this link.";
            var simpleLaunchercannotperform = (string)Application.Current.TryFindResource("SimpleLaunchercannotperform") ?? "'Simple Launcher' cannot perform the requested task.";
            var theUrLwascopiedtotheclipboard = (string)Application.Current.TryFindResource("TheURLwascopiedtotheclipboard") ?? "The URL was copied to the clipboard for your convenience. You can paste it into your browser.";
            var securityPolicyBlocked = (string)Application.Current.TryFindResource("SecurityPolicyBlocked") ?? "Security Policy Blocked";
            System.Windows.MessageBox.Show($"{message}\n\n" +
                                           $"{simpleLaunchercannotperform}\n\n" +
                                           $"{theUrLwascopiedtotheclipboard}", securityPolicyBlocked, MessageBoxButton.OK, MessageBoxImage.Warning);
            Clipboard.SetText(url); // Copy URL to clipboard
        }
    }

    internal static void EnterYourRetroAchievementsUsername()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("PleaseenteryourRetroAchievements") ?? "Please enter your RetroAchievements username, API key, and password before configuring an emulator.";
            var message2 = (string)Application.Current.TryFindResource("CredentialsRequired") ?? "Credentials Required";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void EmulatorConfiguredSuccessfully()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("Emulatorconfiguredsuccessfullyfor") ?? "Emulator configured successfully for RetroAchievements!";
            var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void FailedToConfigureTheEmulator()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("Failedtoconfiguretheemulator") ?? "Failed to configure the emulator. The configuration file might be missing, in an unexpected location, or read-only.";
            var message2 = (string)Application.Current.TryFindResource("ConfigurationFailed") ?? "Configuration Failed";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void AnErrorOccurredWhileConfiguringTheEmulator()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("Anerroroccurredwhileconfiguringtheemulator") ?? "An error occurred while configuring the emulator.";
            var message2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FailedToLoginToRetroAchievements()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("FailedtologintoRetroAchievements") ?? "Failed to log in to RetroAchievements. Please check your username and password.";
            var message2 = (string)Application.Current.TryFindResource("LoginFailed") ?? "Login Failed";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FileSystemXmlIsLockedMessageBox()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("Thefilesystemxmlislocked") ?? "The file 'system.xml' is locked or inaccessible by another process.";
            var message2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FailedtoinjectMamEconfiguration()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("FailedtoinjectMAMEconfiguration") ?? "Failed to inject MAME configuration. The error has been logged. Please check the emulator path and try again.";
            var message2 = (string)Application.Current.TryFindResource("InjectionError") ?? "Injection Error";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void MamEconfigurationinjectedsuccessfully()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("MAMEconfigurationinjectedsuccessfully") ?? "MAME configuration injected successfully.";
            var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void FailedtoinjectMamEconfiguration2()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("FailedtoinjectMAMEconfigurationTheerror") ?? "Failed to inject MAME configuration. The error has been logged.";
            var message2 = (string)Application.Current.TryFindResource("InjectionError") ?? "Injection Error";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void MamEemulatorpathnotfound()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("MAMEemulatorpathnotfoundPleaseselect") ?? "MAME emulator path not found. Please select 'mame.exe' or 'mame64.exe' to apply these settings.";
            var message2 = (string)Application.Current.TryFindResource("EmulatorRequired") ?? "Emulator Required";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void RetroArchemulatorpathnotfound()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("RetroArchemulatorpathnotfoundPlease") ?? "RetroArch emulator path not found. Please select 'retroarch.exe' to apply these settings.";
            var message2 = (string)Application.Current.TryFindResource("EmulatorRequired") ?? "Emulator Required";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void FailedtoinjectRetroArchconfiguration()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("FailedtoinjectRetroArchconfigurationTheerror") ?? "Failed to inject RetroArch configuration. The error has been logged. Please check the emulator path and try again.";
            var message2 = (string)Application.Current.TryFindResource("InjectionError") ?? "Injection Error";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void RetroArchconfigurationinjectedsuccessfully()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("RetroArchconfigurationinjectedsuccessfully") ?? "RetroArch configuration injected successfully.";
            var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void FailedtoinjectRetroArchconfiguration2()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("FailedtoinjectRetroArchconfigurationTheerrorhas") ?? "Failed to inject RetroArch configuration. The error has been logged.";
            var message2 = (string)Application.Current.TryFindResource("InjectionError") ?? "Injection Error";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void Xeniaemulatorpathnotfound()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("Xeniaemulatorpathnotfound") ?? "Xenia emulator path not found. Please select 'xenia.exe' or 'xenia_canary.exe' to apply these settings.";
            var message2 = (string)Application.Current.TryFindResource("EmulatorRequired") ?? "Emulator Required";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void FailedtoinjectXeniaconfiguration()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("FailedtoinjectXeniaconfigurationTheerrorPleasecheck") ?? "Failed to inject Xenia configuration. The error has been logged. Please check the emulator path and try again.";
            var message2 = (string)Application.Current.TryFindResource("InjectionError") ?? "Injection Error";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void Xeniaconfigurationinjectedsuccessfully()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("Xeniaconfigurationinjectedsuccessfully") ?? "Xenia configuration injected successfully.";
            var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }

    internal static void FailedtoinjectXeniaconfiguration2()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("FailedtoinjectXeniaconfigurationTheerror") ?? "Failed to inject Xenia configuration. The error has been logged.";
            var message2 = (string)Application.Current.TryFindResource("InjectionError") ?? "Injection Error";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    public static void ShowCustomMessage(string failedToSaveSupermodelConfigurationPleaseCheckFilePermissions, string saveFailed)
    {
        // TODO
    }

    public static void EnterUsernamePassword()
    {
        Application.Current.Dispatcher.Invoke(ShowMessage);
        return;

        static void ShowMessage()
        {
            var message1 = (string)Application.Current.TryFindResource("EnterUsernamePassword") ?? "Please enter your RetroAchievements username and password first.";
            var message2 = (string)Application.Current.TryFindResource("MissingInformation") ?? "Missing Information";
            System.Windows.MessageBox.Show(message1, message2, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}