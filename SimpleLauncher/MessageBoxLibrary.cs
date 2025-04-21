using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public static class MessageBoxLibrary
{
    internal static void TakeScreenShotMessageBox()
    {
        var thegamewilllaunchnow2 = (string)Application.Current.TryFindResource("Thegamewilllaunchnow") ?? "The game will launch now.";
        var setthegamewindowto2 = (string)Application.Current.TryFindResource("Setthegamewindowto") ?? "Set the game window to non-fullscreen. This is important.";
        var youshouldchangetheemulatorparameters2 = (string)Application.Current.TryFindResource("Youshouldchangetheemulatorparameters") ?? "You should change the emulator parameters to prevent the emulator from starting in fullscreen.";
        var aselectionwindowwillopeninSimpleLauncherallowingyou2 = (string)Application.Current.TryFindResource("AselectionwindowwillopeninSimpleLauncherallowingyou") ?? "A selection window will open in 'Simple Launcher', allowing you to choose the desired window to capture.";
        var assoonasyouselectawindow2 = (string)Application.Current.TryFindResource("assoonasyouselectawindow") ?? "As soon as you select a window, a screenshot will be taken and saved in the image folder of the selected system.";
        var takeScreenshot2 = (string)Application.Current.TryFindResource("TakeScreenshot") ?? "Take Screenshot";
        MessageBox.Show($"{thegamewilllaunchnow2}\n\n" +
                        $"{setthegamewindowto2}\n\n" +
                        $"{youshouldchangetheemulatorparameters2}\n\n" +
                        $"{aselectionwindowwillopeninSimpleLauncherallowingyou2}\n\n" +
                        $"{assoonasyouselectawindow2}",
            takeScreenshot2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void CouldNotSaveScreenshotMessageBox()
    {
        var failedtosavescreenshot2 = (string)Application.Current.TryFindResource("Failedtosavescreenshot") ?? "Failed to save screenshot.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{failedtosavescreenshot2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void GameIsAlreadyInFavoritesMessageBox(string fileNameWithExtension)
    {
        var isalreadyinfavorites2 = (string)Application.Current.TryFindResource("isalreadyinfavorites") ?? "is already in favorites.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show($"{fileNameWithExtension} {isalreadyinfavorites2}",
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ErrorWhileAddingFavoritesMessageBox()
    {
        var anerroroccurredwhileaddingthisgame2 = (string)Application.Current.TryFindResource("Anerroroccurredwhileaddingthisgame") ?? "An error occurred while adding this game to the favorites.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{anerroroccurredwhileaddingthisgame2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ErrorWhileRemovingGameFromFavoriteMessageBox()
    {
        var anerroroccurredwhileremoving2 = (string)Application.Current.TryFindResource("Anerroroccurredwhileremoving") ?? "An error occurred while removing this game from favorites.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{anerroroccurredwhileremoving2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ErrorOpeningTheUpdateHistoryWindowMessageBox()
    {
        var erroropeningtheUpdateHistorywindow2 = (string)Application.Current.TryFindResource("ErroropeningtheUpdateHistorywindow") ?? "Error opening the Update History window.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{erroropeningtheUpdateHistorywindow2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ErrorOpeningVideoLinkMessageBox()
    {
        var therewasaproblemopeningtheVideo2 = (string)Application.Current.TryFindResource("TherewasaproblemopeningtheVideo") ?? "There was a problem opening the Video Link.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasaproblemopeningtheVideo2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ProblemOpeningInfoLinkMessageBox()
    {
        var therewasaproblemopeningthe2 = (string)Application.Current.TryFindResource("Therewasaproblemopeningthe") ?? "There was a problem opening the Info Link.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasaproblemopeningthe2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ThereIsNoCoverMessageBox()
    {
        var thereisnocoverfileassociated2 = (string)Application.Current.TryFindResource("Thereisnocoverfileassociated") ?? "There is no cover file associated with this game.";
        var covernotfound2 = (string)Application.Current.TryFindResource("Covernotfound") ?? "Cover not found";
        MessageBox.Show(thereisnocoverfileassociated2,
            covernotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ThereIsNoTitleSnapshotMessageBox()
    {
        var thereisnotitlesnapshot2 = (string)Application.Current.TryFindResource("Thereisnotitlesnapshot") ?? "There is no title snapshot file associated with this game.";
        var titleSnapshotnotfound2 = (string)Application.Current.TryFindResource("TitleSnapshotnotfound") ?? "Title Snapshot not found";
        MessageBox.Show(thereisnotitlesnapshot2,
            titleSnapshotnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ThereIsNoGameplaySnapshotMessageBox()
    {
        var thereisnogameplaysnapshot2 = (string)Application.Current.TryFindResource("Thereisnogameplaysnapshot") ?? "There is no gameplay snapshot file associated with this game.";
        var gameplaySnapshotnotfound2 = (string)Application.Current.TryFindResource("GameplaySnapshotnotfound") ?? "Gameplay Snapshot not found";
        MessageBox.Show(thereisnogameplaysnapshot2,
            gameplaySnapshotnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ThereIsNoCartMessageBox()
    {
        var thereisnocartfile2 = (string)Application.Current.TryFindResource("Thereisnocartfile") ?? "There is no cart file associated with this game.";
        var cartnotfound2 = (string)Application.Current.TryFindResource("Cartnotfound") ?? "Cart not found";
        MessageBox.Show(thereisnocartfile2,
            cartnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ThereIsNoVideoFileMessageBox()
    {
        var thereisnovideofile2 = (string)Application.Current.TryFindResource("Thereisnovideofile") ?? "There is no video file associated with this game.";
        var videonotfound2 = (string)Application.Current.TryFindResource("Videonotfound") ?? "Video not found";
        MessageBox.Show(thereisnovideofile2,
            videonotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void CouldNotOpenManualMessageBox()
    {
        var failedtoopenthemanual2 = (string)Application.Current.TryFindResource("Failedtoopenthemanual") ?? "Failed to open the manual.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{failedtoopenthemanual2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ThereIsNoManualMessageBox()
    {
        var thereisnomanual2 = (string)Application.Current.TryFindResource("Thereisnomanual") ?? "There is no manual associated with this file.";
        var manualNotFound2 = (string)Application.Current.TryFindResource("Manualnotfound") ?? "Manual not found";
        MessageBox.Show(thereisnomanual2,
            manualNotFound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ThereIsNoWalkthroughMessageBox()
    {
        var thereisnowalkthrough2 = (string)Application.Current.TryFindResource("Thereisnowalkthrough") ?? "There is no walkthrough file associated with this game.";
        var walkthroughnotfound2 = (string)Application.Current.TryFindResource("Walkthroughnotfound") ?? "Walkthrough not found";
        MessageBox.Show(thereisnowalkthrough2,
            walkthroughnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ThereIsNoCabinetMessageBox()
    {
        var thereisnocabinetfile2 = (string)Application.Current.TryFindResource("Thereisnocabinetfile") ?? "There is no cabinet file associated with this game.";
        var cabinetnotfound2 = (string)Application.Current.TryFindResource("Cabinetnotfound") ?? "Cabinet not found";
        MessageBox.Show(thereisnocabinetfile2,
            cabinetnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ThereIsNoFlyerMessageBox()
    {
        var thereisnoflyer2 = (string)Application.Current.TryFindResource("Thereisnoflyer") ?? "There is no flyer file associated with this game.";
        var flyernotfound2 = (string)Application.Current.TryFindResource("Flyernotfound") ?? "Flyer not found";
        MessageBox.Show(thereisnoflyer2,
            flyernotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ThereIsNoPcbMessageBox()
    {
        var thereisnoPcBfile2 = (string)Application.Current.TryFindResource("ThereisnoPCBfile") ?? "There is no PCB file associated with this game.";
        var pCBnotfound2 = (string)Application.Current.TryFindResource("PCBnotfound") ?? "PCB not found";
        MessageBox.Show(thereisnoPcBfile2,
            pCBnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void FileSuccessfullyDeletedMessageBox(string fileNameWithExtension)
    {
        var thefile2 = (string)Application.Current.TryFindResource("Thefile") ?? "The file";
        var hasbeensuccessfullydeleted2 = (string)Application.Current.TryFindResource("hasbeensuccessfullydeleted") ?? "has been successfully deleted.";
        var fileDeleted2 = (string)Application.Current.TryFindResource("Filedeleted2") ?? "File deleted";
        MessageBox.Show($"{thefile2} '{fileNameWithExtension}' {hasbeensuccessfullydeleted2}",
            fileDeleted2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void FileCouldNotBeDeletedMessageBox(string fileNameWithExtension)
    {
        var anerroroccurredwhiletryingtodelete2 = (string)Application.Current.TryFindResource("Anerroroccurredwhiletryingtodelete") ?? "An error occurred while trying to delete the file";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{anerroroccurredwhiletryingtodelete2} '{fileNameWithExtension}'.\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void UnableToLoadImageMessageBox(string imageFileName)
    {
        var unabletoloadimage2 = (string)Application.Current.TryFindResource("Unabletoloadimage") ?? "Unable to load image";
        var thisimagemaybecorrupted2 = (string)Application.Current.TryFindResource("Thisimagemaybecorrupted") ?? "This image may be corrupted.";
        var thedefaultimagewillbedisplayed2 = (string)Application.Current.TryFindResource("Thedefaultimagewillbedisplayed") ?? "The default image will be displayed instead.";
        var imageloadingerror2 = (string)Application.Current.TryFindResource("Imageloadingerror") ?? "Image loading error";
        MessageBox.Show($"{unabletoloadimage2} '{imageFileName}'.\n\n" +
                        $"{thisimagemaybecorrupted2}\n\n" +
                        $"{thedefaultimagewillbedisplayed2}",
            imageloadingerror2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static void DefaultImageNotFoundMessageBox()
    {
        var defaultpngfileismissing2 = (string)Application.Current.TryFindResource("defaultpngfileismissing") ?? "'default.png' file is missing.";
        var doyouwanttoreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var reinstall = MessageBox.Show($"{defaultpngfileismissing2}\n\n" +
                                        $"{doyouwanttoreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);

        if (reinstall == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            var pleasereinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLauncher") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            var theapplicationwillshutdown2 = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
            MessageBox.Show($"{pleasereinstallSimpleLauncher2}\n\n" +
                            $"{theapplicationwillshutdown2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Error);

            QuitApplication.SimpleQuitApplication();
        }
    }

    internal static void GlobalSearchErrorMessageBox()
    {
        var therewasanerrorusingtheGlobal2 = (string)Application.Current.TryFindResource("TherewasanerrorusingtheGlobal") ?? "There was an error using the Global Search.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorusingtheGlobal2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void PleaseEnterSearchTermMessageBox()
    {
        var pleaseenterasearchterm2 = (string)Application.Current.TryFindResource("Pleaseenterasearchterm") ?? "Please enter a search term.";
        var warning2 = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        MessageBox.Show(pleaseenterasearchterm2,
            warning2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static void ErrorLaunchingGameMessageBox(string logPath)
    {
        var therewasanerrorlaunchingtheselected2 = (string)Application.Current.TryFindResource("Therewasanerrorlaunchingtheselected") ?? "There was an error launching the selected game.";
        var dowanttoopenthefileerroruserlog2 = (string)Application.Current.TryFindResource("Dowanttoopenthefileerroruserlog") ?? "Do want to open the file 'error_user.log' to debug the error?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{therewasanerrorlaunchingtheselected2}\n\n" +
                                     $"{dowanttoopenthefileerroruserlog2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Exclamation);

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
            var thefileerroruserlogwasnotfound2 = (string)Application.Current.TryFindResource("Thefileerroruserlogwasnotfound") ?? "The file 'error_user.log' was not found!";
            MessageBox.Show(thefileerroruserlogwasnotfound2,
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SelectAGameToLaunchMessageBox()
    {
        var pleaseselectagametolaunch2 = (string)Application.Current.TryFindResource("Pleaseselectagametolaunch") ?? "Please select a game to launch.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(pleaseselectagametolaunch2,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ErrorRightClickContextMenuMessageBox()
    {
        var therewasanerrorintherightclick2 = (string)Application.Current.TryFindResource("Therewasanerrorintherightclick") ?? "There was an error in the right-click context menu.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorintherightclick2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ErrorLoadingSystemConfigMessageBox()
    {
        var therewasanerrorloadingthesystemConfig2 = (string)Application.Current.TryFindResource("TherewasanerrorloadingthesystemConfig") ?? "There was an error loading the systemConfig.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorloadingthesystemConfig2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void FileAddedToFavoritesMessageBox(string fileNameWithoutExtension)
    {
        var hasbeenaddedtofavorites2 = (string)Application.Current.TryFindResource("hasbeenaddedtofavorites") ?? "has been added to favorites.";
        var success2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        MessageBox.Show($"{fileNameWithoutExtension} {hasbeenaddedtofavorites2}",
            success2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void FileRemovedFromFavoritesMessageBox(string fileNameWithoutExtension)
    {
        var wasremovedfromfavorites2 = (string)Application.Current.TryFindResource("wasremovedfromfavorites") ?? "was removed from favorites.";
        var success2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        MessageBox.Show($"{fileNameWithoutExtension} {wasremovedfromfavorites2}",
            success2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void CouldNotLaunchThisGameMessageBox(string logPath)
    {
        var simpleLaunchercouldnotlaunchthisgame2 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotlaunchthisgame") ?? "'Simple Launcher' could not launch this game.";
        var dowanttoopenthefileerroruserlog2 = (string)Application.Current.TryFindResource("Dowanttoopenthefileerroruserlog") ?? "Do want to open the file 'error_user.log' to debug the error?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{simpleLaunchercouldnotlaunchthisgame2}\n\n" +
                                     $"{dowanttoopenthefileerroruserlog2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
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
            var thefileerroruserlogwasnotfound2 = (string)Application.Current.TryFindResource("Thefileerroruserlogwasnotfound") ?? "The file 'error_user.log' was not found!";
            MessageBox.Show(thefileerroruserlogwasnotfound2,
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorCalculatingStatsMessageBox()
    {
        var anerroroccurredwhilecalculatingtheGlobal2 = (string)Application.Current.TryFindResource("AnerroroccurredwhilecalculatingtheGlobal") ?? "An error occurred while calculating the Global Statistics.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{anerroroccurredwhilecalculatingtheGlobal2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void FailedSaveReportMessageBox()
    {
        var failedtosavethereport2 = (string)Application.Current.TryFindResource("Failedtosavethereport") ?? "Failed to save the report.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{failedtosavethereport2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ReportSavedMessageBox()
    {
        var reportsavedsuccessfully2 = (string)Application.Current.TryFindResource("Reportsavedsuccessfully") ?? "Report saved successfully.";
        var success2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        MessageBox.Show(reportsavedsuccessfully2,
            success2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void NoStatsToSaveMessageBox()
    {
        var nostatisticsavailabletosave2 = (string)Application.Current.TryFindResource("Nostatisticsavailabletosave") ?? "No statistics available to save.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show(nostatisticsavailabletosave2,
            error2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static void ErrorLaunchingToolMessageBox(string logPath)
    {
        var anerroroccurredwhilelaunchingtheselectedtool2 = (string)Application.Current.TryFindResource("Anerroroccurredwhilelaunchingtheselectedtool") ?? "An error occurred while launching the selected tool.";
        var grantSimpleLauncheradministrative2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var temporarilydisableyourantivirussoftware2 = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var dowanttoopenthefileerroruserlog2 = (string)Application.Current.TryFindResource("Dowanttoopenthefileerroruserlog") ?? "Do want to open the file 'error_user.log' to debug the error?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{anerroroccurredwhilelaunchingtheselectedtool2}\n\n" +
                                     $"{grantSimpleLauncheradministrative2}\n\n" +
                                     $"{temporarilydisableyourantivirussoftware2}\n\n" +
                                     $"{dowanttoopenthefileerroruserlog2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
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
            var thefileerroruserlogwasnotfound2 = (string)Application.Current.TryFindResource("Thefileerroruserlogwasnotfound") ?? "The file 'error_user.log' was not found!";
            MessageBox.Show(thefileerroruserlogwasnotfound2,
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void SelectedToolNotFoundMessageBox()
    {
        var theselectedtoolwasnotfound2 = (string)Application.Current.TryFindResource("Theselectedtoolwasnotfound") ?? "The selected tool was not found in the expected path.";
        var doyouwanttoreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var reinstall = MessageBox.Show(
            $"{theselectedtoolwasnotfound2}\n\n" +
            $"{doyouwanttoreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (reinstall == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            var pleasereinstallSimpleLaunchermanually2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            MessageBox.Show(pleasereinstallSimpleLaunchermanually2,
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorMessageBox()
    {
        var therewasanerror2 = (string)Application.Current.TryFindResource("Therewasanerror") ?? "There was an error.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerror2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void NoFavoriteFoundMessageBox()
    {
        var nofavoritegamesfoundfortheselectedsystem = (string)Application.Current.TryFindResource("Nofavoritegamesfoundfortheselectedsystem") ?? "No favorite games found for the selected system.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(nofavoritegamesfoundfortheselectedsystem,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void MoveToWritableFolderMessageBox()
    {
        var itlookslikeSimpleLauncherisinstalled2 = (string)Application.Current.TryFindResource("ItlookslikeSimpleLauncheris2") ?? "It looks like 'Simple Launcher' is installed in a restricted folder (e.g., Program Files), where it does not have write access.";
        var itneedswriteaccesstoitsfolder2 = (string)Application.Current.TryFindResource("Itneedswriteaccesstoitsfolder2") ?? "It needs write access to its folder.";
        var pleasemovetheapplicationfolder2 = (string)Application.Current.TryFindResource("Pleasemovetheapplicationfolder2") ?? "Please move the application folder to a writable location like the 'Documents' folder.";
        var ifpossiblerunitwithadministrative2 = (string)Application.Current.TryFindResource("Ifpossiblerunitwithadministrative") ?? "If possible, run it with administrative privileges.";
        var warning2 = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        MessageBox.Show(
            $"{itlookslikeSimpleLauncherisinstalled2}\n\n" +
            $"{itneedswriteaccesstoitsfolder2}\n\n" +
            $"{pleasemovetheapplicationfolder2}\n\n" +
            $"{ifpossiblerunitwithadministrative2}",
            warning2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static void InvalidSystemConfigMessageBox()
    {
        var therewasanerrorwhileloading2 = (string)Application.Current.TryFindResource("Therewasanerrorwhileloading") ?? "There was an error while loading the system configuration for this system.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorwhileloading2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ErrorMethodLoadGameFilesAsyncMessageBox()
    {
        var therewasanerrorloadingthegame2 = (string)Application.Current.TryFindResource("Therewasanerrorloadingthegame") ?? "There was an error loading the game list.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorloadingthegame2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ErrorOpeningDonationLinkMessageBox()
    {
        var therewasanerroropeningthedonation2 = (string)Application.Current.TryFindResource("Therewasanerroropeningthedonation") ?? "There was an error opening the Donation Link.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerroropeningthedonation2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ToggleGamepadFailureMessageBox()
    {
        var failedtotogglegamepad2 = (string)Application.Current.TryFindResource("Failedtotogglegamepad") ?? "Failed to toggle gamepad.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{failedtotogglegamepad2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void FindRomCoverMissingMessageBox()
    {
        var findRomCoverexewasnotfound = (string)Application.Current.TryFindResource("FindRomCoverexewasnotfound") ?? "'FindRomCover.exe' was not found in the expected path.";
        var doyouwanttoreinstall = (string)Application.Current.TryFindResource("Doyouwanttoreinstall") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var reinstall = MessageBox.Show(
            $"{findRomCoverexewasnotfound}\n\n" +
            $"{doyouwanttoreinstall}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (reinstall == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            var pleasereinstallSimpleLaunchermanually = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            MessageBox.Show(pleasereinstallSimpleLaunchermanually,
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FindRomCoverLaunchWasCanceledByUserMessageBox()
    {
        var thelaunchofFindRomCoverexewascanceled = (string)Application.Current.TryFindResource("ThelaunchofFindRomCoverexewascanceled") ?? "The launch of 'FindRomCover.exe' was canceled by the user.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(thelaunchofFindRomCoverexewascanceled,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void FindRomCoverLaunchWasBlockedMessageBox(string logPath)
    {
        var anerroroccurredwhiletryingtolaunch = (string)Application.Current.TryFindResource("Anerroroccurredwhiletryingtolaunch") ?? "An error occurred while trying to launch 'FindRomCover.exe'.";
        var grantSimpleLauncheradministrative2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var temporarilydisableyourantivirus2 = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var wouldyouliketoopentheerroruserlog = (string)Application.Current.TryFindResource("Wouldyouliketoopentheerroruserlog") ?? "Would you like to open the 'error_user.log' file to investigate the issue?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show(
            $"{anerroroccurredwhiletryingtolaunch}\n\n" +
            $"{grantSimpleLauncheradministrative2}\n\n" +
            $"{temporarilydisableyourantivirus2}\n\n" +
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
            var thefileerroruserlog = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
            MessageBox.Show(thefileerroruserlog,
                error, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorChangingViewModeMessageBox()
    {
        var therewasanerrorwhilechangingtheviewmode2 = (string)Application.Current.TryFindResource("Therewasanerrorwhilechangingtheviewmode") ?? "There was an error while changing the view mode.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorwhilechangingtheviewmode2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void NavigationButtonErrorMessageBox()
    {
        var therewasanerrorinthenavigationbutton2 = (string)Application.Current.TryFindResource("Therewasanerrorinthenavigationbutton") ?? "There was an error in the navigation button.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorinthenavigationbutton2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void SelectSystemBeforeSearchMessageBox()
    {
        var pleaseselectasystembeforesearching = (string)Application.Current.TryFindResource("Pleaseselectasystembeforesearching") ?? "Please select a system before searching.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        MessageBox.Show(pleaseselectasystembeforesearching,
            warning, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static void EnterSearchQueryMessageBox()
    {
        var pleaseenterasearchquery = (string)Application.Current.TryFindResource("Pleaseenterasearchquery") ?? "Please enter a search query.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        MessageBox.Show(pleaseenterasearchquery,
            warning, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static void ErrorWhileLoadingHelpUserXmlMessageBox()
    {
        var unexpectederrorwhileloadinghelpuserxml2 = (string)Application.Current.TryFindResource("Unexpectederrorwhileloadinghelpuserxml") ?? "Unexpected error while loading 'helpuser.xml'.";
        var doyouwanttoautomaticreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{unexpectederrorwhileloadinghelpuserxml2}\n\n" +
                                     $"{doyouwanttoautomaticreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
    }

    internal static void NoSystemInHelpUserXmlMessageBox()
    {
        var novalidsystemsfoundinthefilehelpuserxml2 = (string)Application.Current.TryFindResource("Novalidsystemsfoundinthefilehelpuserxml") ?? "No valid systems found in the file 'helpuser.xml'.";
        var doyouwanttoautomaticreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{novalidsystemsfoundinthefilehelpuserxml2}\n\n" +
                                     $"{doyouwanttoautomaticreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
    }

    internal static bool CouldNotLoadHelpUserXmlMessageBox()
    {
        var simpleLaunchercouldnotloadhelpuserxml2 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotloadhelpuserxml") ?? "'Simple Launcher' could not load 'helpuser.xml'.";
        var doyouwanttoautomaticreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{simpleLaunchercouldnotloadhelpuserxml2}\n\n" +
                                     $"{doyouwanttoautomaticreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            return true;
        }

        return false;
    }

    internal static bool FailedToLoadHelpUserXmlMessageBox()
    {
        var unabletoloadhelpuserxml2 = (string)Application.Current.TryFindResource("Unabletoloadhelpuserxml") ?? "Unable to load 'helpuser.xml'. The file may be corrupted.";
        var doyouwanttoautomaticreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{unabletoloadhelpuserxml2}\n\n" +
                                     $"{doyouwanttoautomaticreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            return true;
        }

        return false;
    }

    internal static bool FileHelpUserXmlIsMissingMessageBox()
    {
        var thefilehelpuserxmlismissing2 = (string)Application.Current.TryFindResource("Thefilehelpuserxmlismissing") ?? "The file 'helpuser.xml' is missing.";
        var doyouwanttoautomaticreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{thefilehelpuserxmlismissing2}\n\n" +
                                     $"{doyouwanttoautomaticreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            return true;
        }

        return false;
    }

    internal static void ImageViewerErrorMessageBox()
    {
        var failedtoloadtheimageintheImage2 = (string)Application.Current.TryFindResource("FailedtoloadtheimageintheImage") ?? "Failed to load the image in the Image Viewer window.";
        var theimagemaybecorruptedorinaccessible2 = (string)Application.Current.TryFindResource("Theimagemaybecorruptedorinaccessible") ?? "The image may be corrupted or inaccessible.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{failedtoloadtheimageintheImage2}\n\n" +
                        $"{theimagemaybecorruptedorinaccessible2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ReinstallSimpleLauncherFileCorruptedMessageBox()
    {
        var simpleLaunchercouldnotloadthefilemamedat2 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotloadthefilemamedat") ?? "'Simple Launcher' could not load the file 'mame.dat' or it is corrupted.";
        var doyouwanttoautomaticreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{simpleLaunchercouldnotloadthefilemamedat2}\n\n" +
                                     $"{doyouwanttoautomaticreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            var pleasereinstallSimpleLaunchermanually2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            var theapplicationwillshutdown2 = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
            MessageBox.Show($"{pleasereinstallSimpleLaunchermanually2}\n\n" +
                            $"{theapplicationwillshutdown2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Error);

            QuitApplication.SimpleQuitApplication();
        }
    }

    internal static void ReinstallSimpleLauncherFileMissingMessageBox()
    {
        var thefilemamedatcouldnotbefound2 = (string)Application.Current.TryFindResource("Thefilemamedatcouldnotbefound") ?? "The file 'mame.dat' could not be found in the application folder.";
        var doyouwanttoautomaticreinstall2 = (string)Application.Current.TryFindResource("Doyouwanttoautomaticreinstall") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{thefilemamedatcouldnotbefound2}\n\n" +
                                     $"{doyouwanttoautomaticreinstall2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            var pleasereinstallSimpleLaunchermanually2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            var theapplicationwillshutdown2 = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
            MessageBox.Show($"{pleasereinstallSimpleLaunchermanually2}\n\n" +
                            $"{theapplicationwillshutdown2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Error);

            QuitApplication.SimpleQuitApplication();
        }
    }

    internal static void ErrorCheckingForUpdatesMessageBox()
    {
        var anerroroccurredwhilecheckingforupdates2 = (string)Application.Current.TryFindResource("Anerroroccurredwhilecheckingforupdates") ?? "An error occurred while checking for updates.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{anerroroccurredwhilecheckingforupdates2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void UpdaterNotFoundMessageBox()
    {
        var updaterexenotfound2 = (string)Application.Current.TryFindResource("Updaterexenotfound") ?? "'Updater.exe' not found.";
        var pleasereinstallSimpleLaunchermanually2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
        var theapplicationwillnowshutdown2 = (string)Application.Current.TryFindResource("Theapplicationwillnowshutdown") ?? "The application will now shutdown.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{updaterexenotfound2}\n\n" +
                        $"{pleasereinstallSimpleLaunchermanually2}\n\n" +
                        $"{theapplicationwillnowshutdown2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Question);

        QuitApplication.SimpleQuitApplication();
    }

    internal static void ErrorLoadingRomHistoryMessageBox()
    {
        var anerroroccurredwhileloadingRoMhistory2 = (string)Application.Current.TryFindResource("AnerroroccurredwhileloadingROMhistory") ?? "An error occurred while loading ROM history.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{anerroroccurredwhileloadingRoMhistory2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void NoHistoryXmlFoundMessageBox()
    {
        var nohistoryxmlfilefound2 = (string)Application.Current.TryFindResource("Nohistoryxmlfilefound") ?? "No 'history.xml' file found in the application folder.";
        var doyouwanttoreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{nohistoryxmlfilefound2}\n\n" +
                                     $"{doyouwanttoreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
    }

    internal static void ErrorOpeningBrowserMessageBox()
    {
        var anerroroccurredwhileopeningthebrowser2 = (string)Application.Current.TryFindResource("Anerroroccurredwhileopeningthebrowser") ?? "An error occurred while opening the browser.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{anerroroccurredwhileopeningthebrowser2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void SimpleLauncherNeedMorePrivilegesMessageBox()
    {
        var simpleLauncherlackssufficientprivilegestowrite2 = (string)Application.Current.TryFindResource("SimpleLauncherlackssufficientprivilegestowrite") ?? "'Simple Launcher' lacks sufficient privileges to write to the 'settings.xml' file.";
        var areyourunningasecondinstance2 = (string)Application.Current.TryFindResource("areyourunningasecondinstance") ?? "Are you running a second instance of 'Simple Launcher'? If yes, please open only one instance at a time or you may encounter issues.";
        var grantSimpleLauncheradministrative2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative2") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var temporarilydisableyourantivirus2 = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var ensurethattheSimpleLauncherfolderislocatedinawritable2 = (string)Application.Current.TryFindResource("EnsurethattheSimpleLauncherfolderislocatedinawritable") ?? "Ensure that the 'Simple Launcher' folder is located in a writable directory.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{simpleLauncherlackssufficientprivilegestowrite2}\n\n" +
                        $"{areyourunningasecondinstance2}\n\n" +
                        $"{grantSimpleLauncheradministrative2}\n\n" +
                        $"{ensurethattheSimpleLauncherfolderislocatedinawritable2}\n\n" +
                        $"{temporarilydisableyourantivirus2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void DepViolationMessageBox()
    {
        var depViolationError = (string)Application.Current.TryFindResource("DEPViolationError") ??
                                "A Data Execution Prevention (DEP) violation occurred while running the emulator, which is a Windows security feature that prevents applications from executing code from non-executable memory regions. This commonly happens with older emulators or ones that use specific memory access techniques that modern security systems flag as potentially dangerous.";
        var whatIsDep = (string)Application.Current.TryFindResource("WhatIsDEP") ??
                        "DEP is a security feature that helps prevent damage from viruses and other security threats.";
        var howToFixDep = (string)Application.Current.TryFindResource("HowToFixDEP") ??
                          "You can try the following solutions:";
        var solution1 = (string)Application.Current.TryFindResource("DEPSolution1") ??
                        "1. Run 'Simple Launcher' with administrator privileges.";
        var solution2 = (string)Application.Current.TryFindResource("DEPSolution2") ??
                        "2. Add the emulator to DEP exceptions in Windows Security settings.";
        var solution3 = (string)Application.Current.TryFindResource("DEPSolution3") ??
                        "3. Try using a different emulator compatible with your system.";
        var solution4 = (string)Application.Current.TryFindResource("DEPSolution4") ??
                        "4. Update your emulator to the latest version.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        MessageBox.Show($"{depViolationError}\n\n" +
                        $"{whatIsDep}\n\n" +
                        $"{howToFixDep}\n" +
                        $"{solution1}\n" +
                        $"{solution2}\n" +
                        $"{solution3}\n" +
                        $"{solution4}",
            error, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void CheckForMemoryAccessViolation()
    {
        var memoryViolationError = (string)Application.Current.TryFindResource("MemoryAccessViolationError") ??
                                   "A Memory Access Violation occurred while running the emulator. This happens when a program attempts to access memory that it either doesn't have permission to access or memory that has been freed/doesn't exist. This is common in emulators that manipulate memory directly to achieve accurate emulation of other systems.";
        var whatIsMemoryAccess = (string)Application.Current.TryFindResource("WhatIsMemoryAccess") ??
                                 "Memory Access Violations are security mechanisms that prevent programs from accessing or modifying memory outside their allocated space, which could potentially crash your system or create security vulnerabilities.";
        var howToFixMemoryAccess = (string)Application.Current.TryFindResource("HowToFixMemoryAccess") ??
                                   "You can try the following solutions:";
        var solution1 = (string)Application.Current.TryFindResource("MemorySolution1") ??
                        "1. Run the application with administrator privileges.";
        var solution2 = (string)Application.Current.TryFindResource("MemorySolution2") ??
                        "2. Check if your antivirus is blocking the emulator's memory operations.";
        var solution3 = (string)Application.Current.TryFindResource("MemorySolution3") ??
                        "3. Update your emulator to the latest version which may have fixed memory handling issues.";
        var solution4 = (string)Application.Current.TryFindResource("MemorySolution4") ??
                        "4. Try adjusting memory allocation settings in the emulator configuration if available.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        MessageBox.Show($"{memoryViolationError}\n\n" +
                        $"{whatIsMemoryAccess}\n\n" +
                        $"{howToFixMemoryAccess}\n" +
                        $"{solution1}\n" +
                        $"{solution2}\n" +
                        $"{solution3}\n" +
                        $"{solution4}",
            error, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void SystemXmlIsCorruptedMessageBox(string logPath)
    {
        var systemxmliscorrupted2 = (string)Application.Current.TryFindResource("systemxmliscorrupted") ?? "'system.xml' is corrupted or could not be opened.";
        var pleasefixitmanuallyordeleteit2 = (string)Application.Current.TryFindResource("Pleasefixitmanuallyordeleteit") ?? "Please fix it manually or delete it.";
        var ifyouchoosetodeleteit2 = (string)Application.Current.TryFindResource("Ifyouchoosetodeleteit") ?? "If you choose to delete it, 'Simple Launcher' will create a new one for you.";
        var theapplicationwillshutdown2 = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
        var wouldyouliketoopentheerroruserlog = (string)Application.Current.TryFindResource("Wouldyouliketoopentheerroruserlog") ?? "Would you like to open the 'error_user.log' file to investigate the issue?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{systemxmliscorrupted2} {pleasefixitmanuallyordeleteit2}\n\n" +
                                     $"{ifyouchoosetodeleteit2}\n\n" +
                                     $"{theapplicationwillshutdown2}" +
                                     $"{wouldyouliketoopentheerroruserlog}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
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
                MessageBox.Show(thefileerroruserlog,
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        QuitApplication.SimpleQuitApplication();
    }

    internal static void SystemModelXmlIsMissingMessageBox()
    {
        var systemmodelxmlismissing2 = (string)Application.Current.TryFindResource("systemmodelxmlismissing") ?? "'system_model.xml' is missing.";
        var simpleLaunchercannotworkproperly2 = (string)Application.Current.TryFindResource("SimpleLaunchercannotworkproperly") ?? "'Simple Launcher' cannot work properly without this file.";
        var doyouwanttoautomaticallyreinstall2 = (string)Application.Current.TryFindResource("Doyouwanttoautomaticallyreinstall") ?? "Do you want to automatically reinstall 'Simple Launcher' to fix the problem?";
        var warning2 = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        var messageBoxResult = MessageBox.Show($"{systemmodelxmlismissing2}\n\n" +
                                               $"{simpleLaunchercannotworkproperly2}\n\n" +
                                               $"{doyouwanttoautomaticallyreinstall2}",
            warning2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (messageBoxResult == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            var pleasereinstallSimpleLaunchermanually2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            var theapplicationwillshutdown2 = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
            MessageBox.Show($"{pleasereinstallSimpleLaunchermanually2}\n\n" +
                            $"{theapplicationwillshutdown2}",
                warning2, MessageBoxButton.OK, MessageBoxImage.Warning);

            QuitApplication.SimpleQuitApplication();
        }
    }

    internal static void FiLeSystemXmlIsCorruptedMessageBox(string logPath)
    {
        var thefilesystemxmlisbadlycorrupted2 = (string)Application.Current.TryFindResource("Thefilesystemxmlisbadlycorrupted") ?? "The file 'system.xml' is badly corrupted.";
        var wouldyouliketoopentheerroruserlog = (string)Application.Current.TryFindResource("Wouldyouliketoopentheerroruserlog") ?? "Would you like to open the 'error_user.log' file to investigate the issue?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{thefilesystemxmlisbadlycorrupted2}\n\n" +
                                     $"{wouldyouliketoopentheerroruserlog}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
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
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void InstallUpdateManuallyMessageBox(string repoOwner, string repoName)
    {
        var therewasanerrorinstallingorupdating2 = (string)Application.Current.TryFindResource("Therewasanerrorinstallingorupdating") ?? "There was an error installing or updating the application.";
        var wouldyouliketoberedirectedtothedownloadpage2 = (string)Application.Current.TryFindResource("Wouldyouliketoberedirectedtothedownloadpage") ?? "Would you like to be redirected to the download page to install or update it manually?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var messageBoxResult = MessageBox.Show(
            $"{therewasanerrorinstallingorupdating2}\n\n" +
            $"{wouldyouliketoberedirectedtothedownloadpage2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
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
        catch (Exception)
        {
            ErrorOpeningBrowserMessageBox();
        }
    }

    internal static void DownloadManuallyMessageBox(string repoOwner, string repoName)
    {
        var updaterexenotfoundintheapplication2 = (string)Application.Current.TryFindResource("Updaterexenotfoundintheapplication") ?? "'Updater.exe' not found in the application directory.";
        var wouldyouliketoberedirectedtotheSimpleLauncherdownloadpage2 = (string)Application.Current.TryFindResource("WouldyouliketoberedirectedtotheSimpleLauncherdownloadpage") ?? "Would you like to be redirected to the 'Simple Launcher' download page to download it manually?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var messageBoxResult = MessageBox.Show($"{updaterexenotfoundintheapplication2}\n\n" +
                                               $"{wouldyouliketoberedirectedtotheSimpleLauncherdownloadpage2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
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
        catch (Exception)
        {
            ErrorOpeningBrowserMessageBox();
        }
    }

    internal static void RequiredFileMissingMessageBox()
    {
        var fileappsettingsjsonismissing2 = (string)Application.Current.TryFindResource("Fileappsettingsjsonismissing") ?? "File 'appsettings.json' is missing.";
        var theapplicationwillnotbeabletosendthesupportrequest2 = (string)Application.Current.TryFindResource("Theapplicationwillnotbeabletosendthesupportrequest") ?? "The application will not be able to send the support request.";
        var doyouwanttoautomaticallyreinstall2 = (string)Application.Current.TryFindResource("Doyouwanttoautomaticallyreinstall") ?? "Do you want to automatically reinstall 'Simple Launcher' to fix the problem?";
        var warning2 = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        var messageBoxResult = MessageBox.Show(
            $"{fileappsettingsjsonismissing2}\n\n" +
            $"{theapplicationwillnotbeabletosendthesupportrequest2}\n\n" +
            $"{doyouwanttoautomaticallyreinstall2}",
            warning2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (messageBoxResult == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            var pleasereinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLauncher") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            MessageBox.Show(pleasereinstallSimpleLauncher2,
                warning2, MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    internal static void EnterSupportRequestMessageBox()
    {
        var pleaseenterthedetailsofthesupportrequest2 = (string)Application.Current.TryFindResource("Pleaseenterthedetailsofthesupportrequest") ?? "Please enter the details of the support request.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(pleaseenterthedetailsofthesupportrequest2,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void EnterNameMessageBox()
    {
        var pleaseenterthename2 = (string)Application.Current.TryFindResource("Pleaseenterthename") ?? "Please enter the name.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(pleaseenterthename2,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void EnterEmailMessageBox()
    {
        var pleaseentertheemail2 = (string)Application.Current.TryFindResource("Pleaseentertheemail") ?? "Please enter the email.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(pleaseentertheemail2,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ApiKeyErrorMessageBox()
    {
        var therewasanerrorintheApiKey2 = (string)Application.Current.TryFindResource("TherewasanerrorintheAPIKey") ?? "There was an error in the API Key of this form.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorintheApiKey2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void SupportRequestSuccessMessageBox()
    {
        var supportrequestsentsuccessfully2 = (string)Application.Current.TryFindResource("Supportrequestsentsuccessfully") ?? "Support request sent successfully.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(supportrequestsentsuccessfully2,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void SupportRequestSendErrorMessageBox()
    {
        var anerroroccurredwhilesendingthesupportrequest2 = (string)Application.Current.TryFindResource("Anerroroccurredwhilesendingthesupportrequest") ?? "An error occurred while sending the support request.";
        var thebugwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Thebugwasreportedtothedeveloper") ?? "The bug was reported to the developer that will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{anerroroccurredwhilesendingthesupportrequest2}\n\n" +
                        $"{thebugwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ExtractionFailedMessageBox()
    {
        var extractionfailed2 = (string)Application.Current.TryFindResource("Extractionfailed") ?? "Extraction failed.";
        var ensurethefileisnotcorrupted2 = (string)Application.Current.TryFindResource("Ensurethefileisnotcorrupted") ?? "Ensure the file is not corrupted.";
        var ensureyouhaveenoughspaceintheHdd2 = (string)Application.Current.TryFindResource("EnsureyouhaveenoughspaceintheHDD") ?? "Ensure you have enough space in the HDD to extract the file.";
        var grantSimpleLauncheradministrative2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var ensuretheSimpleLauncherfolder2 = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncherfolder") ?? "Ensure the 'Simple Launcher' folder is a writable directory.";
        var temporarilydisableyourantivirus2 = (string)Application.Current.TryFindResource("Temporarilydisableyourantivirus") ?? "Temporarily disable your antivirus software and try again.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{extractionfailed2}\n\n" +
                        $"{ensurethefileisnotcorrupted2}\n" +
                        $"{ensureyouhaveenoughspaceintheHdd2}\n" +
                        $"{grantSimpleLauncheradministrative2}\n" +
                        $"{ensuretheSimpleLauncherfolder2}\n" +
                        $"{temporarilydisableyourantivirus2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void FileNeedToBeCompressedMessageBox()
    {
        var theselectedfilecannotbe2 = (string)Application.Current.TryFindResource("Theselectedfilecannotbe") ?? "The selected file cannot be extracted.";
        var toextractafileitneedstobe2 = (string)Application.Current.TryFindResource("Toextractafileitneedstobe") ?? "To extract a file, it needs to be a 7z, zip, or rar file.";
        var pleasefixthatintheEditwindow2 = (string)Application.Current.TryFindResource("PleasefixthatintheEditwindow") ?? "Please fix that in the Edit window.";
        var warning2 = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        MessageBox.Show($"{theselectedfilecannotbe2}\n\n" +
                        $"{toextractafileitneedstobe2}\n\n" +
                        $"{pleasefixthatintheEditwindow2}",
            warning2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static void DownloadedFileIsMissingMessageBox()
    {
        var downloadedfileismissing2 = (string)Application.Current.TryFindResource("Downloadedfileismissing") ?? "Downloaded file is missing.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{downloadedfileismissing2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void FileIsLockedMessageBox()
    {
        var downloadedfileislocked2 = (string)Application.Current.TryFindResource("Downloadedfileislocked") ?? "Downloaded file is locked.";
        var grantSimpleLauncheradministrative2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var temporarilydisableyourantivirussoftware2 = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var ensuretheSimpleLauncher2 = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncher") ?? "Ensure the 'Simple Launcher' folder is a writable directory.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{downloadedfileislocked2}\n\n" +
                        $"{grantSimpleLauncheradministrative2}\n\n" +
                        $"{temporarilydisableyourantivirussoftware2}\n\n" +
                        $"{ensuretheSimpleLauncher2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ImagePackDownloadExtractionFailedMessageBox()
    {
        var imagePackdownloadorextraction2 = (string)Application.Current.TryFindResource("ImagePackdownloadorextraction") ?? "Image Pack download or extraction failed!";
        var grantSimpleLauncheradministrative2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var ensuretheSimpleLauncher2 = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncher") ?? "Ensure the 'Simple Launcher' folder is a writable directory.";
        var temporarilydisableyourantivirussoftware2 = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show($"{imagePackdownloadorextraction2}\n\n" +
                        $"{grantSimpleLauncheradministrative2}\n\n" +
                        $"{ensuretheSimpleLauncher2}\n\n" +
                        $"{temporarilydisableyourantivirussoftware2}",
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void DownloadExtractionSuccessfullyMessageBox()
    {
        var thedownloadandextractionweresuccessful2 = (string)Application.Current.TryFindResource("Thedownloadandextractionweresuccessful") ?? "The download and extraction were successful.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(thedownloadandextractionweresuccessful2,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ImagePackDownloadErrorOfferRedirectMessageBox(EasyModeSystemConfig selectedSystem)
    {
        var downloaderror2 = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
        var wouldyouliketoberedirected2 = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{downloaderror2}\n\n" +
                                     $"{wouldyouliketoberedirected2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = selectedSystem.Emulators.Emulator.ExtrasDownloadLink,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error opening the Browser.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            var simpleLaunchercouldnotopentheImage2 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotopentheImage") ?? "'Simple Launcher' could not open the Image Pack download link.";
            MessageBox.Show(simpleLaunchercouldnotopentheImage2,
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorLoadingEasyModeXmlMessageBox()
    {
        var errorloadingthefileeasymodexml2 = (string)Application.Current.TryFindResource("Errorloadingthefileeasymodexml") ?? "Error loading the file 'easymode.xml'.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var doyouwanttoreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{errorloadingthefileeasymodexml2}\n\n" +
                                     $"{theerrorwasreportedtothedeveloper2}\n" +
                                     $"{doyouwanttoreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            var pleasereinstallSimpleLaunchermanually2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            MessageBox.Show(pleasereinstallSimpleLaunchermanually2,
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void LinksSavedMessageBox()
    {
        var linkssavedsuccessfully2 = (string)Application.Current.TryFindResource("Linkssavedsuccessfully") ?? "Links saved successfully.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(linkssavedsuccessfully2,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void DeadZonesSavedMessageBox()
    {
        var deadZonevaluessavedsuccessfully2 = (string)Application.Current.TryFindResource("DeadZonevaluessavedsuccessfully") ?? "DeadZone values saved successfully.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(deadZonevaluessavedsuccessfully2,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void LinksRevertedMessageBox()
    {
        var linksreverted2 = (string)Application.Current.TryFindResource("Linksrevertedtodefaultvalues") ?? "Links reverted to default values.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(linksreverted2,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void MainWindowSearchEngineErrorMessageBox()
    {
        var therewasanerrorwiththesearchengine2 = (string)Application.Current.TryFindResource("Therewasanerrorwiththesearchengine") ?? "There was an error with the search engine.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorwiththesearchengine2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void DownloadExtractionFailedMessageBox()
    {
        var downloadorextractionfailed2 = (string)Application.Current.TryFindResource("DownloadorExtractionFailed") ?? "Download or extraction failed.";
        var grantSimpleLauncheradministrativeaccess2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrativeaccess") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var ensuretheSimpleLauncherfolder2 = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncherfolder") ?? "Ensure the 'Simple Launcher' folder is a writable directory.";
        var temporarilydisableyourantivirus2 = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{downloadorextractionfailed2}\n\n" +
                        $"{grantSimpleLauncheradministrativeaccess2}\n\n" +
                        $"{ensuretheSimpleLauncherfolder2}\n\n" +
                        $"{temporarilydisableyourantivirus2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void DownloadAndExtrationWereSuccessfulMessageBox()
    {
        var downloadingandextractionweresuccessful2 = (string)Application.Current.TryFindResource("Downloadingandextractionweresuccessful") ?? "Downloading and extraction were successful.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show($"{downloadingandextractionweresuccessful2}",
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static Task EmulatorDownloadErrorMessageBox(EasyModeSystemConfig selectedSystem)
    {
        var downloaderror2 = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
        var wouldyouliketoberedirected2 = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{downloaderror2}\n\n" +
                                     $"{wouldyouliketoberedirected2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return Task.CompletedTask;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = selectedSystem.Emulators.Emulator.EmulatorDownloadLink,
                UseShellExecute = true
            });
        }
        catch (Exception ex2)
        {
            // Notify developer
            const string contextMessage2 = "Error opening the download link.";
            _ = LogErrors.LogErrorAsync(ex2, contextMessage2);

            // Notify user
            var erroropeningthedownloadlink2 = (string)Application.Current.TryFindResource("Erroropeningthedownloadlink") ?? "Error opening the download link.";
            var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            MessageBox.Show($"{erroropeningthedownloadlink2}\n\n" +
                            $"{theerrorwasreportedtothedeveloper2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return Task.CompletedTask;
    }

    internal static Task CoreDownloadErrorMessageBox(EasyModeSystemConfig selectedSystem)
    {
        var downloaderror2 = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
        var wouldyouliketoberedirected2 = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{downloaderror2}\n\n" +
                                     $"{wouldyouliketoberedirected2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return Task.CompletedTask;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = selectedSystem.Emulators.Emulator.CoreDownloadLink,
                UseShellExecute = true
            });
        }
        catch (Exception ex2)
        {
            // Notify developer
            const string contextMessage2 = "Error opening the download link.";
            _ = LogErrors.LogErrorAsync(ex2, contextMessage2);

            // Notify user
            var erroropeningthedownloadlink2 = (string)Application.Current.TryFindResource("Erroropeningthedownloadlink") ?? "Error opening the download link.";
            var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            MessageBox.Show($"{erroropeningthedownloadlink2}\n\n" +
                            $"{theerrorwasreportedtothedeveloper2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return Task.CompletedTask;
    }

    internal static void SelectAHistoryItemToRemoveMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("SelectAHistoryItemToRemove") ?? "Please select a history item to remove.";
        MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void SelectAHistoryItemMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("SelectAHistoryItemToRemove") ?? "Please select a history item to remove.";
        MessageBox.Show(message, "Information", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static MessageBoxResult ReallyWantToRemoveAllPlayHistoryMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("AreYouSureYouWantToRemoveAllHistory") ?? "Are you sure you want to remove all play history?";
        var result = MessageBox.Show(message, "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result;
    }

    internal static Task ImagePackDownloadErrorMessageBox(EasyModeSystemConfig selectedSystem)
    {
        var downloaderror2 = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
        var wouldyouliketoberedirected2 = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{downloaderror2}\n\n" +
                                     $"{wouldyouliketoberedirected2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return Task.CompletedTask;

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = selectedSystem.Emulators.Emulator.ExtrasDownloadLink,
                UseShellExecute = true
            });
        }
        catch (Exception ex2)
        {
            // Notify developer
            const string contextMessage2 = "Error opening the download link.";
            _ = LogErrors.LogErrorAsync(ex2, contextMessage2);

            // Notify user
            var erroropeningthedownloadlink2 = (string)Application.Current.TryFindResource("Erroropeningthedownloadlink") ?? "Error opening the download link.";
            var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            MessageBox.Show($"{erroropeningthedownloadlink2}\n\n" +
                            $"{theerrorwasreportedtothedeveloper2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        return Task.CompletedTask;
    }

    internal static bool OverwriteSystemMessageBox(EasyModeSystemConfig selectedSystem)
    {
        var thesystem3 = (string)Application.Current.TryFindResource("Thesystem") ?? "The system";
        var alreadyexists2 = (string)Application.Current.TryFindResource("alreadyexists") ?? "already exists.";
        var doyouwanttooverwriteit2 = (string)Application.Current.TryFindResource("Doyouwanttooverwriteit") ?? "Do you want to overwrite it?";
        var systemAlreadyExists2 = (string)Application.Current.TryFindResource("SystemAlreadyExists") ?? "System Already Exists";
        var result = MessageBox.Show($"{thesystem3} '{selectedSystem.SystemName}' {alreadyexists2}\n\n" +
                                     $"{doyouwanttooverwriteit2}",
            systemAlreadyExists2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.No;
    }

    internal static void SystemAddedMessageBox(string systemFolder, string fullImageFolderPathForMessage, EasyModeSystemConfig selectedSystem)
    {
        var thesystem2 = (string)Application.Current.TryFindResource("Thesystem") ?? "The system";
        var hasbeenaddedsuccessfully2 = (string)Application.Current.TryFindResource("hasbeenaddedsuccessfully") ?? "has been added successfully.";
        var putRoMsorIsOsforthissysteminside2 = (string)Application.Current.TryFindResource("PutROMsorISOsforthissysteminside") ?? "Put ROMs or ISOs for this system inside";
        var putcoverimagesforthissysteminside2 = (string)Application.Current.TryFindResource("Putcoverimagesforthissysteminside") ?? "Put cover images for this system inside";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show($"{thesystem2} '{selectedSystem.SystemName}' {hasbeenaddedsuccessfully2}\n\n" +
                        $"{putRoMsorIsOsforthissysteminside2} '{systemFolder}'\n\n" +
                        $"{putcoverimagesforthissysteminside2} '{fullImageFolderPathForMessage}'.",
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void AddSystemFailedMessageBox()
    {
        var therewasanerroradding2 = (string)Application.Current.TryFindResource("Therewasanerroradding") ?? "There was an error adding this system.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerroradding2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void RightClickContextMenuErrorMessageBox()
    {
        var therewasanerrorintherightclick2 = (string)Application.Current.TryFindResource("Therewasanerrorintherightclick") ?? "There was an error in the right-click context menu.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorintherightclick2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void SelectGameToLaunchMessageBox()
    {
        var pleaseselectagametolaunch2 = (string)Application.Current.TryFindResource("Pleaseselectagametolaunch") ?? "Please select a game to launch.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(pleaseselectagametolaunch2,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void GameFileDoesNotExistMessageBox()
    {
        var thegamefiledoesnotexist2 = (string)Application.Current.TryFindResource("Thegamefiledoesnotexist") ?? "The game file does not exist!";
        var thefavoritehasbeenremovedfromthelist2 = (string)Application.Current.TryFindResource("Thefavoritehasbeenremovedfromthelist") ?? "The favorite has been removed from the list.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show($"{thegamefiledoesnotexist2}\n\n" +
                        $"{thefavoritehasbeenremovedfromthelist2}",
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void CouldNotOpenHistoryWindowMessageBox()
    {
        var therewasaproblemopeningtheHistorywindow2 = (string)Application.Current.TryFindResource("TherewasaproblemopeningtheHistorywindow") ?? "There was a problem opening the History window.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasaproblemopeningtheHistorywindow2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ErrorOpeningCoverImageMessageBox()
    {
        var therewasanerrortryingtoopenthe2 = (string)Application.Current.TryFindResource("Therewasanerrortryingtoopenthe") ?? "There was an error trying to open the cover image.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrortryingtoopenthe2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void CouldNotOpenWalkthroughMessageBox()
    {
        var failedtoopenthewalkthroughfile2 = (string)Application.Current.TryFindResource("Failedtoopenthewalkthroughfile") ?? "Failed to open the walkthrough file.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{failedtoopenthewalkthroughfile2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void SelectAFavoriteToRemoveMessageBox()
    {
        var pleaseselectafavoritetoremove2 = (string)Application.Current.TryFindResource("Pleaseselectafavoritetoremove") ?? "Please select a favorite to remove.";
        var warning2 = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        MessageBox.Show(pleaseselectafavoritetoremove2,
            warning2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static void SystemXmlNotFoundMessageBox()
    {
        var systemxmlnotfound2 = (string)Application.Current.TryFindResource("systemxmlnotfound") ?? "'system.xml' not found inside the application folder.";
        var pleaserestartSimpleLauncher2 = (string)Application.Current.TryFindResource("PleaserestartSimpleLauncher") ?? "Please restart 'Simple Launcher'.";
        var ifthatdoesnotwork2 = (string)Application.Current.TryFindResource("Ifthatdoesnotwork") ?? "If that does not work, please reinstall 'Simple Launcher'.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{systemxmlnotfound2}\n\n" +
                        $"{pleaserestartSimpleLauncher2}\n\n" +
                        $"{ifthatdoesnotwork2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void YouCanAddANewSystemMessageBox()
    {
        var youcanaddanewsystem2 = (string)Application.Current.TryFindResource("Youcanaddanewsystem") ?? "You can add a new system now.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(youcanaddanewsystem2,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void EmulatorNameMustBeUniqueMessageBox(string emulator1NameText)
    {
        var thename2 = (string)Application.Current.TryFindResource("Thename") ?? "The name";
        var isusedmultipletimes2 = (string)Application.Current.TryFindResource("isusedmultipletimes") ?? "is used multiple times. Each emulator name must be unique.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show($"{thename2} '{emulator1NameText}' {isusedmultipletimes2}",
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void EmulatorNameRequiredMessageBox(int i)
    {
        var emulator2 = (string)Application.Current.TryFindResource("Emulator") ?? "Emulator";
        var nameisrequiredbecauserelateddata2 = (string)Application.Current.TryFindResource("nameisrequiredbecauserelateddata") ?? "name is required because related data has been provided.";
        var pleasefixthisfield2 = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show($"{emulator2} {i + 2} {nameisrequiredbecauserelateddata2}\n\n" +
                        $"{pleasefixthisfield2}",
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void EmulatorNameMustBeUniqueMessageBox2(string emulatorName)
    {
        var thename2 = (string)Application.Current.TryFindResource("Thename") ?? "The name";
        var isusedmultipletimes2 = (string)Application.Current.TryFindResource("isusedmultipletimes") ?? "is used multiple times. Each emulator name must be unique.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show($"{thename2} '{emulatorName}' {isusedmultipletimes2}",
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void SystemSavedSuccessfullyMessageBox()
    {
        var systemsavedsuccessfully2 = (string)Application.Current.TryFindResource("Systemsavedsuccessfully") ?? "System saved successfully.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(systemsavedsuccessfully2,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void PathOrParameterInvalidMessageBox()
    {
        var oneormorepathsorparameters2 = (string)Application.Current.TryFindResource("Oneormorepathsorparameters") ?? "One or more paths or parameters are invalid.";
        var pleasefixthemtoproceed2 = (string)Application.Current.TryFindResource("Pleasefixthemtoproceed") ?? "Please fix them to proceed.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{oneormorepathsorparameters2}\n\n" +
                        $"{pleasefixthemtoproceed2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void Emulator1RequiredMessageBox()
    {
        var emulator1Nameisrequired2 = (string)Application.Current.TryFindResource("Emulator1Nameisrequired") ?? "'Emulator 1 Name' is required.";
        var pleasefixthisfield2 = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{emulator1Nameisrequired2}\n\n" +
                        $"{pleasefixthisfield2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ExtensionToLaunchIsRequiredMessageBox()
    {
        var extensiontoLaunchAfterExtraction2 = (string)Application.Current.TryFindResource("ExtensiontoLaunchAfterExtraction") ?? "'Extension to Launch After Extraction' is required when 'Extract File Before Launch' is set to true.";
        var pleasefixthisfield2 = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{extensiontoLaunchAfterExtraction2}\n\n" +
                        $"{pleasefixthisfield2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ExtensionToSearchIsRequiredMessageBox()
    {
        var extensiontoSearchintheSystemFolder2 = (string)Application.Current.TryFindResource("ExtensiontoSearchintheSystemFolder") ?? "'Extension to Search in the System Folder' cannot be empty or contain only spaces.";
        var pleasefixthisfield2 = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{extensiontoSearchintheSystemFolder2}\n\n" +
                        $"{pleasefixthisfield2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void FileMustBeCompressedMessageBox()
    {
        var whenExtractFileBeforeLaunch2 = (string)Application.Current.TryFindResource("WhenExtractFileBeforeLaunch") ?? "When 'Extract File Before Launch' is set to true, 'Extension to Search in the System Folder' must include 'zip', '7z', or 'rar'.";
        var itwillnotacceptotherextensions2 = (string)Application.Current.TryFindResource("Itwillnotacceptotherextensions") ?? "It will not accept other extensions.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{whenExtractFileBeforeLaunch2}\n\n{itwillnotacceptotherextensions2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void SystemImageFolderCanNotBeEmptyMessageBox()
    {
        var systemImageFoldercannotbeempty2 = (string)Application.Current.TryFindResource("SystemImageFoldercannotbeempty") ?? "'System Image Folder' cannot be empty or contain only spaces.";
        var pleasefixthisfield2 = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{systemImageFoldercannotbeempty2}\n\n{pleasefixthisfield2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void SystemFolderCanNotBeEmptyMessageBox()
    {
        var systemFoldercannotbeempty2 = (string)Application.Current.TryFindResource("SystemFoldercannotbeempty") ?? "'System Folder' cannot be empty or contain only spaces.";
        var pleasefixthisfield2 = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{systemFoldercannotbeempty2}\n\n{pleasefixthisfield2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void SystemNameCanNotBeEmptyMessageBox()
    {
        var systemNamecannotbeemptyor2 = (string)Application.Current.TryFindResource("SystemNamecannotbeemptyor") ?? "'System Name' cannot be empty or contain only spaces.";
        var pleasefixthisfield2 = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{systemNamecannotbeemptyor2}\n\n" +
                        $"{pleasefixthisfield2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void FolderCreatedMessageBox(string systemNameText)
    {
        var simpleLaunchercreatedaimagefolder2 = (string)Application.Current.TryFindResource("SimpleLaunchercreatedaimagefolder") ?? "'Simple Launcher' created a image folder for this system at";
        var youmayplacethecoverimagesforthissystem2 = (string)Application.Current.TryFindResource("Youmayplacethecoverimagesforthissysteminside") ?? "You may place the cover images for this system inside this folder.";
        var italsocreatedfoldersfor2 = (string)Application.Current.TryFindResource("Italsocreatedfoldersfor") ?? "It also created folders for";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show($"{simpleLaunchercreatedaimagefolder2} '.\\images\\{systemNameText}'.\n\n" +
                        $"{youmayplacethecoverimagesforthissystem2}\n\n" +
                        $"{italsocreatedfoldersfor2} 'title_snapshots', 'gameplay_snapshots', 'videos', 'manuals', 'walkthrough', 'cabinets', 'flyers', 'pcbs', 'carts'.",
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void FolderCreationFailedMessageBox()
    {
        var simpleLauncherfailedtocreatethe2 = (string)Application.Current.TryFindResource("SimpleLauncherfailedtocreatethe") ?? "'Simple Launcher' failed to create the necessary folders for this system.";
        var grantSimpleLauncheradministrative2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var temporarilydisableyourantivirus2 = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var ensurethattheSimpleLauncherfolderislocatedinawritable2 = (string)Application.Current.TryFindResource("EnsurethattheSimpleLauncherfolderislocatedinawritable") ?? "Ensure that the 'Simple Launcher' folder is located in a writable directory.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show($"{simpleLauncherfailedtocreatethe2}\n\n" +
                        $"{grantSimpleLauncheradministrative2}\n\n" +
                        $"{temporarilydisableyourantivirus2}\n\n" +
                        $"{ensurethattheSimpleLauncherfolderislocatedinawritable2}",
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void SelectASystemToDeleteMessageBox()
    {
        var pleaseselectasystemtodelete2 = (string)Application.Current.TryFindResource("Pleaseselectasystemtodelete") ?? "Please select a system to delete.";
        var warning2 = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        MessageBox.Show(pleaseselectasystemtodelete2,
            warning2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static void SystemNotFoundInTheXmlMessageBox()
    {
        var selectedsystemnotfound2 = (string)Application.Current.TryFindResource("Selectedsystemnotfound") ?? "Selected system not found in the XML document!";
        var alert2 = (string)Application.Current.TryFindResource("Alert") ?? "Alert";
        MessageBox.Show(selectedsystemnotfound2,
            alert2, MessageBoxButton.OK, MessageBoxImage.Exclamation);
    }

    internal static void ErrorFindingGameFilesMessageBox(string logPath)
    {
        var therewasanerrorfinding2 = (string)Application.Current.TryFindResource("Therewasanerrorfinding") ?? "There was an error finding the game files.";
        var doyouwanttoopenthefileerroruserlog2 = (string)Application.Current.TryFindResource("Doyouwanttoopenthefileerroruserlog") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{therewasanerrorfinding2}\n\n" +
                                     $"{doyouwanttoopenthefileerroruserlog2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
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
            var thefileerroruserlogwas2 = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ?? "The file 'error_user.log' was not found!";
            MessageBox.Show(thefileerroruserlogwas2,
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void ErrorWhileCountingFilesMessageBox(string logPath)
    {
        var anerroroccurredwhilecounting2 = (string)Application.Current.TryFindResource("Anerroroccurredwhilecounting") ?? "An error occurred while counting files.";
        var doyouwanttoopenthefileerroruserlog2 = (string)Application.Current.TryFindResource("Doyouwanttoopenthefileerroruserlog") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{anerroroccurredwhilecounting2}\n\n" +
                                     $"{doyouwanttoopenthefileerroruserlog2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
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
            var thefileerroruserlog2 = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
            MessageBox.Show(thefileerroruserlog2,
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void GamePadErrorMessageBox(string logPath)
    {
        var therewasanerrorwiththeGamePadController2 = (string)Application.Current.TryFindResource("TherewasanerrorwiththeGamePadController") ?? "There was an error with the GamePad Controller.";
        var grantSimpleLauncheradministrative2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var temporarilydisableyourantivirus2 = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var doyouwanttoopenthefile2 = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{therewasanerrorwiththeGamePadController2}\n\n" +
                                     $"{grantSimpleLauncheradministrative2}\n\n" +
                                     $"{temporarilydisableyourantivirus2}\n\n" +
                                     $"{doyouwanttoopenthefile2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
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
            var thefileerroruserlogwas2 = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ?? "The file 'error_user.log' was not found!";
            MessageBox.Show(thefileerroruserlogwas2,
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void CouldNotLaunchGameMessageBox(string logPath)
    {
        var simpleLaunchercouldnotlaunch2 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotlaunch") ?? "'Simple Launcher' could not launch the selected game.";
        var ifyouaretryingtorunMamEensurethatyourRom2 = (string)Application.Current.TryFindResource("IfyouaretryingtorunMAMEensurethatyourROM") ?? "If you are trying to run MAME, ensure that your ROM collection is compatible with the MAME version you are using.";
        var ifyouaretryingtorunRetroarchensurethattheBios2 = (string)Application.Current.TryFindResource("IfyouaretryingtorunRetroarchensurethattheBIOS") ?? "If you are trying to run Retroarch, ensure that the BIOS or required files for the core are installed.";
        var alsomakesureyouarecallingtheemulator2 = (string)Application.Current.TryFindResource("Alsomakesureyouarecallingtheemulator") ?? "Also, make sure you are calling the emulator with the correct parameter.";
        var doyouwanttoopenthefile2 = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show(
            $"{simpleLaunchercouldnotlaunch2}\n\n" +
            $"{ifyouaretryingtorunMamEensurethatyourRom2}\n\n" +
            $"{ifyouaretryingtorunRetroarchensurethattheBios2}\n\n" +
            $"{alsomakesureyouarecallingtheemulator2}\n\n" +
            $"{doyouwanttoopenthefile2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
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
            var thefileerroruserlogwas2 = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ?? "The file 'error_user.log' was not found!";
            MessageBox.Show(thefileerroruserlogwas2,
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void InvalidOperationExceptionMessageBox()
    {
        var failedtostarttheemulator2 = (string)Application.Current.TryFindResource("Failedtostarttheemulator") ?? "Failed to start the emulator or it has not exited as expected.";
        var grantSimpleLauncheradministrative2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var temporarilydisableyourantivirus2 = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var alsochecktheintegrityoftheemulator2 = (string)Application.Current.TryFindResource("Alsochecktheintegrityoftheemulator") ?? "Also, check the integrity of the emulator and its dependencies.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{failedtostarttheemulator2}\n\n" +
                        $"{grantSimpleLauncheradministrative2}\n\n" +
                        $"{temporarilydisableyourantivirus2}\n\n" +
                        $"{alsochecktheintegrityoftheemulator2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ThereWasAnErrorLaunchingThisGameMessageBox(string logPath)
    {
        var therewasanerrorlaunchingthisgame2 = (string)Application.Current.TryFindResource("Therewasanerrorlaunchingthisgame") ?? "There was an error launching this game.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var doyouwanttoopenthefileerroruserlog2 = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{therewasanerrorlaunchingthisgame2}\n\n" +
                                     $"{theerrorwasreportedtothedeveloper2}\n\n" +
                                     $"{doyouwanttoopenthefileerroruserlog2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
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
            var thefileerroruserlog2 = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
            MessageBox.Show(thefileerroruserlog2,
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void CannotExtractThisFileMessageBox(string filePath)
    {
        var theselectedfile2 = (string)Application.Current.TryFindResource("Theselectedfile") ?? "The selected file";
        var cannotbeextracted2 = (string)Application.Current.TryFindResource("cannotbeextracted") ?? "can not be extracted.";
        var toextractafileitneedstobe2 = (string)Application.Current.TryFindResource("Toextractafileitneedstobe") ?? "To extract a file, it needs to be a 7z, zip, or rar file.";
        var pleasegotoEditSystem2 = (string)Application.Current.TryFindResource("PleasegotoEditSystem") ?? "Please go to Edit System - Expert Mode and edit this system.";
        var warning2 = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        MessageBox.Show($"{theselectedfile2} '{filePath}' {cannotbeextracted2}\n\n" +
                        $"{toextractafileitneedstobe2}\n\n" +
                        $"{pleasegotoEditSystem2}",
            warning2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static void EmulatorCouldNotOpenXboxXblaSimpleMessageBox(string logPath)
    {
        var theemulatorcouldnotopenthegame2 = (string)Application.Current.TryFindResource("Theemulatorcouldnotopenthegame") ?? "The emulator could not open the game with the provided parameters.";
        var doyouwanttoopenthefileerror2 = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show(
            $"{theemulatorcouldnotopenthegame2}\n\n" +
            $"{doyouwanttoopenthefileerror2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Question);
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
            var thefileerroruser2 = (string)Application.Current.TryFindResource("Thefileerroruser") ?? "The file 'error_user.log' was not found!";
            MessageBox.Show(thefileerroruser2,
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void NullFileExtensionMessageBox()
    {
        var thereisnoExtension2 = (string)Application.Current.TryFindResource("ThereisnoExtension") ?? "There is no 'Extension to Launch After Extraction' set in the system configuration.";
        var pleaseeditthissystemto2 = (string)Application.Current.TryFindResource("Pleaseeditthissystemto") ?? "Please edit this system to fix that.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{thereisnoExtension2}\n\n" +
                        $"{pleaseeditthissystemto2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void CouldNotFindAFileMessageBox()
    {
        var couldnotfindafilewiththeextensiondefined2 = (string)Application.Current.TryFindResource("Couldnotfindafilewiththeextensiondefined") ?? "Could not find a file with the extension defined in 'Extension to Launch After Extraction' inside the extracted folder.";
        var pleaseeditthissystemtofix2 = (string)Application.Current.TryFindResource("Pleaseeditthissystemto") ?? "Please edit this system to fix that.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{couldnotfindafilewiththeextensiondefined2}\n\n" +
                        $"{pleaseeditthissystemtofix2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static MessageBoxResult SearchOnlineForRomHistoryMessageBox()
    {
        var thereisnoRoMhistoryinthelocaldatabase2 = (string)Application.Current.TryFindResource("ThereisnoROMhistoryinthelocaldatabase") ?? "There is no ROM history in the local database for this file.";
        var doyouwanttosearchonline2 = (string)Application.Current.TryFindResource("Doyouwanttosearchonline") ?? "Do you want to search online for the ROM history?";
        var rOmHistoryNotFound2 = (string)Application.Current.TryFindResource("ROMHistorynotfound") ?? "ROM History not found";
        var result = MessageBox.Show(
            $"{thereisnoRoMhistoryinthelocaldatabase2}\n\n" +
            $"{doyouwanttosearchonline2}",
            rOmHistoryNotFound2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result;
    }

    internal static void SystemHasBeenDeletedMessageBox(string selectedSystemName)
    {
        var system2 = (string)Application.Current.TryFindResource("System2") ?? "System";
        var hasbeendeleted2 = (string)Application.Current.TryFindResource("hasbeendeleted") ?? "has been deleted.";
        var info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show($"{system2} '{selectedSystemName}' {hasbeendeleted2}",
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static MessageBoxResult AreYouSureDoYouWantToDeleteThisSystemMessageBox()
    {
        var areyousureyouwanttodeletethis2 = (string)Application.Current.TryFindResource("Areyousureyouwanttodeletethis") ?? "Are you sure you want to delete this system?";
        var confirmation2 = (string)Application.Current.TryFindResource("Confirmation") ?? "Confirmation";
        var result = MessageBox.Show(areyousureyouwanttodeletethis2,
            confirmation2, MessageBoxButton.YesNo, MessageBoxImage.Question);

        return result;
    }

    internal static void ThereWasAnErrorDeletingTheFileMessageBox()
    {
        var therewasanerrordeletingthefile2 = (string)Application.Current.TryFindResource("Therewasanerrordeletingthefile") ?? "There was an error deleting the file.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrordeletingthefile2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static MessageBoxResult AreYouSureYouWantToDeleteTheFileMessageBox(string fileNameWithExtension)
    {
        var areyousureyouwanttodeletethefile2 = (string)Application.Current.TryFindResource("Areyousureyouwanttodeletethefile") ?? "Are you sure you want to delete the file";
        var thisactionwilldelete2 = (string)Application.Current.TryFindResource("Thisactionwilldelete") ?? "This action will delete the file from the HDD and cannot be undone.";
        var confirmDeletion2 = (string)Application.Current.TryFindResource("ConfirmDeletion") ?? "Confirm Deletion";
        var result = MessageBox.Show($"{areyousureyouwanttodeletethefile2} '{fileNameWithExtension}'?\n\n" +
                                     $"{thisactionwilldelete2}",
            confirmDeletion2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result;
    }

    internal static MessageBoxResult WoulYouLikeToSaveAReportMessageBox()
    {
        var wouldyouliketosaveareport2 = (string)Application.Current.TryFindResource("Wouldyouliketosaveareport") ?? "Would you like to save a report with the results?";
        var saveReport2 = (string)Application.Current.TryFindResource("SaveReport") ?? "Save Report";
        var result = MessageBox.Show(wouldyouliketosaveareport2,
            saveReport2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result;
    }

    internal static void SimpleLauncherWasUnableToRestoreBackupMessageBox()
    {
        var simpleLauncherwasunabletorestore2 = (string)Application.Current.TryFindResource("SimpleLauncherwasunabletorestore") ?? "'Simple Launcher' was unable to restore the last backup.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show(simpleLauncherwasunabletorestore2,
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static MessageBoxResult WouldYouLikeToRestoreTheLastBackupMessageBox()
    {
        var icouldnotfindthefilesystemxml2 = (string)Application.Current.TryFindResource("Icouldnotfindthefilesystemxml") ?? "I could not find the file 'system.xml', which is required to start the application.";
        var butIfoundabackupfile2 = (string)Application.Current.TryFindResource("ButIfoundabackupfile") ?? "But I found a backup file.";
        var wouldyouliketorestore2 = (string)Application.Current.TryFindResource("Wouldyouliketorestore") ?? "Would you like to restore the last backup?";
        var restoreBackup2 = (string)Application.Current.TryFindResource("RestoreBackup") ?? "Restore Backup?";
        var restoreResult = MessageBox.Show($"{icouldnotfindthefilesystemxml2}\n\n" +
                                            $"{butIfoundabackupfile2}\n\n" +
                                            $"{wouldyouliketorestore2}",
            restoreBackup2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return restoreResult;
    }

    internal static void FailedToLoadLanguageResourceMessageBox()
    {
        var failedtoloadlanguageresources2 = (string)Application.Current.TryFindResource("Failedtoloadlanguageresources") ?? "Failed to load language resources.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var languageLoadingError2 = (string)Application.Current.TryFindResource("LanguageLoadingError") ?? "Language Loading Error";
        MessageBox.Show($"{failedtoloadlanguageresources2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            languageLoadingError2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void InvalidSystemConfigurationMessageBox(string error)
    {
        var invalidSystemConfiguration2 = (string)Application.Current.TryFindResource("InvalidSystemConfiguration") ?? "Invalid System Configuration";
        MessageBox.Show(error,
            invalidSystemConfiguration2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static void ExtractionFolderCannotBeCreatedMessageBox(Exception ex)
    {
        var cannotcreateoraccesstheextractionfolder2 = (string)Application.Current.TryFindResource("Cannotcreateoraccesstheextractionfolder") ?? "Cannot create or access the extraction folder";
        var invalidExtractionFolder2 = (string)Application.Current.TryFindResource("InvalidExtractionFolder") ?? "Invalid Extraction Folder";
        MessageBox.Show($"{cannotcreateoraccesstheextractionfolder2}: {ex.Message}",
            invalidExtractionFolder2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ExtractionFolderIsNullMessageBox()
    {
        var pleaseselectanextractionfolder2 = (string)Application.Current.TryFindResource("Pleaseselectanextractionfolder") ?? "Please select an extraction folder.";
        var extractionFolderRequired2 = (string)Application.Current.TryFindResource("ExtractionFolderRequired") ?? "Extraction Folder Required";
        MessageBox.Show(pleaseselectanextractionfolder2,
            extractionFolderRequired2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static void DownloadUrlIsNullMessageBox()
    {
        var theselectedsystemdoesnothaveavaliddownloadlink2 = (string)Application.Current.TryFindResource("Theselectedsystemdoesnothaveavaliddownloadlink") ?? "The selected system does not have a valid download link.";
        var invalidDownloadLink2 = (string)Application.Current.TryFindResource("InvalidDownloadLink") ?? "Invalid Download Link";
        MessageBox.Show(theselectedsystemdoesnothaveavaliddownloadlink2,
            invalidDownloadLink2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static void UnableToOpenLinkMessageBox()
    {
        var unabletoopenthelink2 = (string)Application.Current.TryFindResource("Unabletoopenthelink") ?? "Unable to open the link.";
        var theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{unabletoopenthelink2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void SelectedSystemIsNullMessageBox()
    {
        var couldnotfindtheselectedsystemintheconfiguration2 = (string)Application.Current.TryFindResource("Couldnotfindtheselectedsystemintheconfiguration") ?? "Could not find the selected system in the configuration.";
        var systemNotFound2 = (string)Application.Current.TryFindResource("SystemNotFound") ?? "System Not Found";
        MessageBox.Show(couldnotfindtheselectedsystemintheconfiguration2,
            systemNotFound2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void SystemNameIsNullMessageBox()
    {
        var pleaseselectasystemfromthedropdown2 = (string)Application.Current.TryFindResource("Pleaseselectasystemfromthedropdown") ?? "Please select a system from the dropdown.";
        var selectionRequired2 = (string)Application.Current.TryFindResource("SelectionRequired") ?? "Selection Required";
        MessageBox.Show(pleaseselectasystemfromthedropdown2,
            selectionRequired2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static void NoGameFoundInTheRandomSelectionMessageBox()
    {
        var nogamesfoundtorandomlyselectfrom2 = (string)Application.Current.TryFindResource("Nogamesfoundtorandomlyselectfrom") ?? "No games found to randomly select from. Please check your system selection.";
        var feelingLucky2 = (string)Application.Current.TryFindResource("FeelingLucky") ?? "Feeling Lucky";
        MessageBox.Show(nogamesfoundtorandomlyselectfrom2,
            feelingLucky2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void PleaseSelectASystemBeforeMessageBox()
    {
        var pleaseselectasystembeforeusingtheFeeling2 = (string)Application.Current.TryFindResource("PleaseselectasystembeforeusingtheFeeling") ?? "Please select a system before using the Feeling Lucky feature.";
        var feelingLucky2 = (string)Application.Current.TryFindResource("FeelingLucky") ?? "Feeling Lucky";
        MessageBox.Show(pleaseselectasystembeforeusingtheFeeling2,
            feelingLucky2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ParameterPathsInvalidWarningMessageBox(List<string> invalidPaths)
    {
        var warningMessage = (string)Application.Current.TryFindResource("ParameterPathsInvalidWarning") ??
                             "Some paths in the emulator parameters appear to be invalid or missing. Please double check these fields.";

        // Add details about which paths are invalid
        if (invalidPaths.Count > 0)
        {
            warningMessage += "\n\nPotentially invalid paths:";
            foreach (var path in invalidPaths.Take(5)) // Show at most 5 invalid paths
            {
                warningMessage += $"\n• {path}";
            }

            if (invalidPaths.Count > 5)
            {
                warningMessage += $"\n• ...and {invalidPaths.Count - 5} more";
            }
        }

        warningMessage += "\n\nYou can still save, but these paths may cause issues when launching games.";

        MessageBox.Show(warningMessage, "Parameter Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static bool AskUserToProceedWithInvalidPath(string programLocation, List<string> invalidPaths = null)
    {
        var message = "There are issues with the emulator configuration:";

        if (!string.IsNullOrEmpty(programLocation))
        {
            message += $"\n\n• Program location may be invalid or inaccessible: \"{programLocation}\"";
        }

        if (invalidPaths is { Count: > 0 })
        {
            message += "\n\n• The following paths in parameters may be invalid:";
            foreach (var path in invalidPaths)
            {
                message += $"\n  - \"{path}\"";
            }
        }

        message += "\n\nDo you want to proceed with launching anyway?\n";
        message += "\nYou can edit this system configuration later to fix these issues.";

        var result = MessageBox.Show(
            message,
            "Path Validation Warning",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        return result == MessageBoxResult.Yes;
    }
}