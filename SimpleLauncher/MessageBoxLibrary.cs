using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleLauncher;

public static class MessageBoxLibrary
{
    internal static void TakeScreenShotMessageBox()
    {
        string thegamewilllaunchnow2 = (string)Application.Current.TryFindResource("Thegamewilllaunchnow") ?? "The game will launch now.";
        string setthegamewindowto2 = (string)Application.Current.TryFindResource("Setthegamewindowto") ?? "Set the game window to non-fullscreen. This is important.";
        string youshouldchangetheemulatorparameters2 = (string)Application.Current.TryFindResource("Youshouldchangetheemulatorparameters") ?? "You should change the emulator parameters to prevent the emulator from starting in fullscreen.";
        string aselectionwindowwillopeninSimpleLauncherallowingyou2 = (string)Application.Current.TryFindResource("AselectionwindowwillopeninSimpleLauncherallowingyou") ?? "A selection window will open in 'Simple Launcher', allowing you to choose the desired window to capture.";
        string assoonasyouselectawindow2 = (string)Application.Current.TryFindResource("assoonasyouselectawindow") ?? "As soon as you select a window, a screenshot will be taken and saved in the image folder of the selected system.";
        string takeScreenshot2 = (string)Application.Current.TryFindResource("TakeScreenshot") ?? "Take Screenshot";
        MessageBox.Show($"{thegamewilllaunchnow2}\n\n" +
                        $"{setthegamewindowto2}\n\n" +
                        $"{youshouldchangetheemulatorparameters2}\n\n" +
                        $"{aselectionwindowwillopeninSimpleLauncherallowingyou2}\n\n" +
                        $"{assoonasyouselectawindow2}",
            takeScreenshot2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void GameIsAlreadyInFavoritesMessageBox(string fileNameWithExtension)
    {
        string isalreadyinfavorites2 = (string)Application.Current.TryFindResource("isalreadyinfavorites") ?? "is already in favorites.";
        string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show($"{fileNameWithExtension} {isalreadyinfavorites2}",
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ErrorWhileAddingFavoritesMessageBox()
    {
        string anerroroccurredwhileaddingthisgame2 = (string)Application.Current.TryFindResource("Anerroroccurredwhileaddingthisgame") ?? "An error occurred while adding this game to the favorites.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{anerroroccurredwhileaddingthisgame2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void FileIsNotInFavoritesMessageBox(string fileNameWithExtension)
    {
        string isnotinfavorites2 = (string)Application.Current.TryFindResource("isnotinfavorites") ?? "is not in favorites.";
        string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show($"{fileNameWithExtension} {isnotinfavorites2}",
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ErrorWhileRemovingGameFromFavoriteMessageBox()
    {
        string anerroroccurredwhileremoving2 = (string)Application.Current.TryFindResource("Anerroroccurredwhileremoving") ?? "An error occurred while removing this game from favorites.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{anerroroccurredwhileremoving2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ErrorOpeningVideoLinkMessageBox()
    {
        string therewasaproblemopeningtheVideo2 = (string)Application.Current.TryFindResource("TherewasaproblemopeningtheVideo") ?? "There was a problem opening the Video Link.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasaproblemopeningtheVideo2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ProblemOpeningInfoLinkMessageBox()
    {
        string therewasaproblemopeningthe2 = (string)Application.Current.TryFindResource("Therewasaproblemopeningthe") ?? "There was a problem opening the Info Link.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasaproblemopeningthe2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ProblemOpeningHistoryWindowMessageBox()
    {
        string therewasaproblemopeningtheHistory2 = (string)Application.Current.TryFindResource("TherewasaproblemopeningtheHistory") ?? "There was a problem opening the History window.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasaproblemopeningtheHistory2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ThereIsNoCoverMessageBox()
    {
        string thereisnocoverfileassociated2 = (string)Application.Current.TryFindResource("Thereisnocoverfileassociated") ?? "There is no cover file associated with this game.";
        string covernotfound2 = (string)Application.Current.TryFindResource("Covernotfound") ?? "Cover not found";
        MessageBox.Show(thereisnocoverfileassociated2,
            covernotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ThereIsNoTitleSnapshotMessageBox()
    {
        string thereisnotitlesnapshot2 = (string)Application.Current.TryFindResource("Thereisnotitlesnapshot") ?? "There is no title snapshot file associated with this game.";
        string titleSnapshotnotfound2 = (string)Application.Current.TryFindResource("TitleSnapshotnotfound") ?? "Title Snapshot not found";
        MessageBox.Show(thereisnotitlesnapshot2,
            titleSnapshotnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ThereIsNoGameplaySnapshotMessageBox()
    {
        string thereisnogameplaysnapshot2 = (string)Application.Current.TryFindResource("Thereisnogameplaysnapshot") ?? "There is no gameplay snapshot file associated with this game.";
        string gameplaySnapshotnotfound2 = (string)Application.Current.TryFindResource("GameplaySnapshotnotfound") ?? "Gameplay Snapshot not found";
        MessageBox.Show(thereisnogameplaysnapshot2,
            gameplaySnapshotnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ThereIsNoCartMessageBox()
    {
        string thereisnocartfile2 = (string)Application.Current.TryFindResource("Thereisnocartfile") ?? "There is no cart file associated with this game.";
        string cartnotfound2 = (string)Application.Current.TryFindResource("Cartnotfound") ?? "Cart not found";
        MessageBox.Show(thereisnocartfile2,
            cartnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ThereIsNoVideoFileMessageBox()
    {
        string thereisnovideofile2 = (string)Application.Current.TryFindResource("Thereisnovideofile") ?? "There is no video file associated with this game.";
        string videonotfound2 = (string)Application.Current.TryFindResource("Videonotfound") ?? "Video not found";
        MessageBox.Show(thereisnovideofile2,
            videonotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void CouldNotOpenManualMessageBox()
    {
        string failedtoopenthemanual2 = (string)Application.Current.TryFindResource("Failedtoopenthemanual") ?? "Failed to open the manual.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{failedtoopenthemanual2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}", 
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ThereIsNoManualMessageBox()
    {
        string thereisnomanual2 = (string)Application.Current.TryFindResource("Thereisnomanual") ?? "There is no manual associated with this file.";
        string manualNotFound2 = (string)Application.Current.TryFindResource("Manualnotfound") ?? "Manual not found";
        MessageBox.Show(thereisnomanual2,
            manualNotFound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ThereIsNoWalkthroughMessageBox()
    {
        string thereisnowalkthrough2 = (string)Application.Current.TryFindResource("Thereisnowalkthrough") ?? "There is no walkthrough file associated with this game.";
        string walkthroughnotfound2 = (string)Application.Current.TryFindResource("Walkthroughnotfound") ?? "Walkthrough not found";
        MessageBox.Show(thereisnowalkthrough2,
            walkthroughnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ThereIsNoCabinetMessageBox()
    {
        string thereisnocabinetfile2 = (string)Application.Current.TryFindResource("Thereisnocabinetfile") ?? "There is no cabinet file associated with this game.";
        string cabinetnotfound2 = (string)Application.Current.TryFindResource("Cabinetnotfound") ?? "Cabinet not found";
        MessageBox.Show(thereisnocabinetfile2,
            cabinetnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ThereIsNoFlyerMessageBox()
    {
        string thereisnoflyer2 = (string)Application.Current.TryFindResource("Thereisnoflyer") ?? "There is no flyer file associated with this game.";
        string flyernotfound2 = (string)Application.Current.TryFindResource("Flyernotfound") ?? "Flyer not found";
        MessageBox.Show(thereisnoflyer2,
            flyernotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ThereIsNoPcbMessageBox()
    {
        string thereisnoPcBfile2 = (string)Application.Current.TryFindResource("ThereisnoPCBfile") ?? "There is no PCB file associated with this game.";
        string pCBnotfound2 = (string)Application.Current.TryFindResource("PCBnotfound") ?? "PCB not found";
        MessageBox.Show(thereisnoPcBfile2,
            pCBnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void CouldNotSaveScreenshotMessageBox()
    {
        string failedtosavescreenshot2 = (string)Application.Current.TryFindResource("Failedtosavescreenshot") ?? "Failed to save screenshot.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{failedtosavescreenshot2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void FileSuccessfullyDeletedMessageBox(string fileNameWithExtension)
    {
        string thefile2 = (string)Application.Current.TryFindResource("Thefile") ?? "The file";
        string hasbeensuccessfullydeleted2 = (string)Application.Current.TryFindResource("hasbeensuccessfullydeleted") ?? "has been successfully deleted.";
        string fileDeleted2 = (string)Application.Current.TryFindResource("Filedeleted2") ?? "File deleted";
        MessageBox.Show($"{thefile2} '{fileNameWithExtension}' {hasbeensuccessfullydeleted2}",
            fileDeleted2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void FileCouldNotBeDeletedMessageBox(string fileNameWithExtension)
    {
        string anerroroccurredwhiletryingtodelete2 = (string)Application.Current.TryFindResource("Anerroroccurredwhiletryingtodelete") ?? "An error occurred while trying to delete the file";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{anerroroccurredwhiletryingtodelete2} '{fileNameWithExtension}'.\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void UnableToLoadImageMessageBox(string imageFileName)
    {
        string unabletoloadimage2 = (string)Application.Current.TryFindResource("Unabletoloadimage") ?? "Unable to load image";
        string thisimagemaybecorrupted2 = (string)Application.Current.TryFindResource("Thisimagemaybecorrupted") ?? "This image may be corrupted.";
        string thedefaultimagewillbedisplayed2 = (string)Application.Current.TryFindResource("Thedefaultimagewillbedisplayed") ?? "The default image will be displayed instead.";
        string imageloadingerror2 = (string)Application.Current.TryFindResource("Imageloadingerror") ?? "Image loading error";
        MessageBox.Show($"{unabletoloadimage2} '{imageFileName}'.\n\n" +
                        $"{thisimagemaybecorrupted2}\n\n" +
                        $"{thedefaultimagewillbedisplayed2}",
            imageloadingerror2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static void DefaultImageNotFoundMessageBox()
    {
        string defaultpngfileismissing2 = (string)Application.Current.TryFindResource("defaultpngfileismissing") ?? "'default.png' file is missing.";
        string doyouwanttoreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var reinstall = MessageBox.Show($"{defaultpngfileismissing2}\n\n" +
                                        $"{doyouwanttoreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);

        if (reinstall == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();   
        }
        else
        {
            string pleasereinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLauncher") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            string theapplicationwillshutdown2 = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
            MessageBox.Show($"{pleasereinstallSimpleLauncher2}\n\n" +
                            $"{theapplicationwillshutdown2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
                
            Application.Current.Shutdown();
            Environment.Exit(0);
        }
    }
    
    internal static void GlobalSearchErrorMessageBox()
    {
        string therewasanerrorusingtheGlobal2 = (string)Application.Current.TryFindResource("TherewasanerrorusingtheGlobal") ?? "There was an error using the Global Search.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorusingtheGlobal2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void PleaseEnterSearchTermMessageBox()
    {
        string pleaseenterasearchterm2 = (string)Application.Current.TryFindResource("Pleaseenterasearchterm") ?? "Please enter a search term.";
        string warning2 = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        MessageBox.Show(pleaseenterasearchterm2,
            warning2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
    
    internal static void ErrorLaunchingGameMessageBox()
    {
        string therewasanerrorlaunchingtheselected2 = (string)Application.Current.TryFindResource("Therewasanerrorlaunchingtheselected") ?? "There was an error launching the selected game.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorlaunchingtheselected2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void SelectAGameToLaunchMessageBox()
    {
        string pleaseselectagametolaunch2 = (string)Application.Current.TryFindResource("Pleaseselectagametolaunch") ?? "Please select a game to launch.";
        string information2 = (string)Application.Current.TryFindResource("Information") ?? "Information";
        MessageBox.Show(pleaseselectagametolaunch2,
            information2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void ErrorRightClickContextMenuMessageBox()
    {
        string therewasanerrorintherightclick2 = (string)Application.Current.TryFindResource("Therewasanerrorintherightclick") ?? "There was an error in the right-click context menu.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorintherightclick2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void ErrorLoadingSystemConfigMessageBox()
    {
        string therewasanerrorloadingthesystemConfig2 = (string)Application.Current.TryFindResource("TherewasanerrorloadingthesystemConfig") ?? "There was an error loading the systemConfig.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorloadingthesystemConfig2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void GameAlreadyInFavoritesMessageBox(string fileNameWithoutExtension)
    {
        string isalreadyinfavorites2 = (string)Application.Current.TryFindResource("isalreadyinfavorites") ?? "is already in favorites.";
        string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show($"{fileNameWithoutExtension} {isalreadyinfavorites2}",
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void FileAddedToFavoritesMessageBox(string fileNameWithoutExtension)
    {
        string hasbeenaddedtofavorites2 = (string)Application.Current.TryFindResource("hasbeenaddedtofavorites") ?? "has been added to favorites.";
        string success2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        MessageBox.Show($"{fileNameWithoutExtension} {hasbeenaddedtofavorites2}",
            success2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void ProblemOpeningCoverImageMessageBox()
    {
        string therewasaproblemopeningtheCoverImage2 = (string)Application.Current.TryFindResource("Therewasaproblemopeningthecoverimage") ?? "There was a problem opening the cover image for this game.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasaproblemopeningtheCoverImage2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void ScreenshotSavedMessageBox(string screenshotPath)
    {
        string screenshotsavedsuccessfullyat2 = (string)Application.Current.TryFindResource("Screenshotsavedsuccessfullyat") ?? "Screenshot saved successfully at:";
        string success2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        MessageBox.Show($"{screenshotsavedsuccessfullyat2}\n\n" +
                        $"{screenshotPath}",
            success2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void CouldNotLaunchThisGameMessageBox()
    {
        string simpleLaunchercouldnotlaunchthisgame2 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotlaunchthisgame") ?? "'Simple Launcher' could not launch this game.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{simpleLaunchercouldnotlaunchthisgame2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void ErrorCalculatingStatsMessageBox()
    {
        string anerroroccurredwhilecalculatingtheGlobal2 = (string)Application.Current.TryFindResource("AnerroroccurredwhilecalculatingtheGlobal") ?? "An error occurred while calculating the Global Statistics.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{anerroroccurredwhilecalculatingtheGlobal2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void FailedSaveReportMessageBox()
    {
        string failedtosavethereport2 = (string)Application.Current.TryFindResource("Failedtosavethereport") ?? "Failed to save the report.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{failedtosavethereport2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ReportSavedMessageBox()
    {
        string reportsavedsuccessfully2 = (string)Application.Current.TryFindResource("Reportsavedsuccessfully") ?? "Report saved successfully.";
        string success2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        MessageBox.Show(reportsavedsuccessfully2,
            success2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void NoStatsToSaveMessageBox()
    {
        string nostatisticsavailabletosave2 = (string)Application.Current.TryFindResource("Nostatisticsavailabletosave") ?? "No statistics available to save.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show(nostatisticsavailabletosave2,
            error2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
    
    internal static void ErrorLaunchingToolMessageBox(string logPath)
    {
        string anerroroccurredwhilelaunchingtheselectedtool2 = (string)Application.Current.TryFindResource("Anerroroccurredwhilelaunchingtheselectedtool") ?? "An error occurred while launching the selected tool.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string dowanttoopenthefileerroruserlog2 = (string)Application.Current.TryFindResource("Dowanttoopenthefileerroruserlog") ?? "Do want to open the file 'error_user.log' to debug the error?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{anerroroccurredwhilelaunchingtheselectedtool2}\n\n" +
                                     $"{theerrorwasreportedtothedeveloper2}\n\n" +
                                     $"{dowanttoopenthefileerroruserlog2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        
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
                string thefileerroruserlogwasnotfound2 = (string)Application.Current.TryFindResource("Thefileerroruserlogwasnotfound") ?? "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlogwasnotfound2,
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            
        }
    }

    internal static void SelectedToolNotFoundMessageBox()
    {
        string theselectedtoolwasnotfound2 = (string)Application.Current.TryFindResource("Theselectedtoolwasnotfound") ?? "The selected tool was not found in the expected path.";
        string doyouwanttoreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
        string fileNotFound2 = (string)Application.Current.TryFindResource("FileNotFound") ?? "File Not Found";
        MessageBoxResult reinstall = MessageBox.Show(
            $"{theselectedtoolwasnotfound2}\n\n" +
            $"{doyouwanttoreinstallSimpleLauncher2}",
            fileNotFound2, MessageBoxButton.YesNo, MessageBoxImage.Error);

        if (reinstall == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            string pleasereinstallSimpleLaunchermanually2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            string pleaseReinstall2 = (string)Application.Current.TryFindResource("PleaseReinstall") ?? "Please Reinstall";
            MessageBox.Show(pleasereinstallSimpleLaunchermanually2,
                pleaseReinstall2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    internal static void MethodErrorMessageBox()
    {
        string therewasanerrorwiththismethod2 = (string)Application.Current.TryFindResource("Therewasanerrorwiththismethod") ?? "There was an error with this method.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorwiththismethod2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void SystemXmlCorruptedMessageBox()
    {
        string thefilesystemxmliscorrupted2 = (string)Application.Current.TryFindResource("Thefilesystemxmliscorrupted") ?? "The file 'system.xml' is corrupted.";
        string youneedtofixitmanually2 = (string)Application.Current.TryFindResource("Youneedtofixitmanually") ?? "You need to fix it manually or delete it.";
        string theapplicationwillbeshutdown2 = (string)Application.Current.TryFindResource("Theapplicationwillbeshutdown") ?? "The application will be shutdown.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{thefilesystemxmliscorrupted2}\n\n" +
                        $"{youneedtofixitmanually2}\n\n" +
                        $"{theapplicationwillbeshutdown2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
            
        // Shutdown current application instance
        Application.Current.Shutdown();
        Environment.Exit(0);
    }
        
    internal static void NoFavoriteFoundMessageBox()
    {
        string nofavoritegamesfoundfortheselectedsystem = (string)Application.Current.TryFindResource("Nofavoritegamesfoundfortheselectedsystem") ?? "No favorite games found for the selected system.";
        string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(nofavoritegamesfoundfortheselectedsystem, 
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void MoveToWritableFolderMessageBox()
    {
        string itlookslikeSimpleLauncherisinstalled2 = (string)Application.Current.TryFindResource("ItlookslikeSimpleLauncheris2") ?? "It looks like 'Simple Launcher' is installed in a restricted folder (e.g., Program Files), where it does not have write access.";
        string itneedswriteaccesstoitsfolder2 = (string)Application.Current.TryFindResource("Itneedswriteaccesstoitsfolder2") ?? "It needs write access to its folder.";
        string pleasemovetheapplicationfolder2 = (string)Application.Current.TryFindResource("Pleasemovetheapplicationfolder2") ?? "Please move the application folder to a writable location like the 'Documents' folder.";
        string ifpossiblerunitwithadministrative2 = (string)Application.Current.TryFindResource("Ifpossiblerunitwithadministrative") ?? "If possible, run it with administrative privileges.";
        string accessIssue2 = (string)Application.Current.TryFindResource("AccessIssue") ?? "Access Issue";
        MessageBox.Show(
            $"{itlookslikeSimpleLauncherisinstalled2}\n\n" +
            $"{itneedswriteaccesstoitsfolder2}\n\n" +
            $"{pleasemovetheapplicationfolder2}\n\n" +
            $"{ifpossiblerunitwithadministrative2}",
            accessIssue2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
    
    internal static void InvalidSystemConfigMessageBox()
    {
        string therewasanerrorwhileloading2 = (string)Application.Current.TryFindResource("Therewasanerrorwhileloading") ?? "There was an error while loading the system configuration for this system.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorwhileloading2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ErrorMethodLoadGameFilesAsyncMessageBox()
    {
        string therewasanerrorloadingthegame2 = (string)Application.Current.TryFindResource("Therewasanerrorloadingthegame") ?? "There was an error loading the game list.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorloadingthegame2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void ErrorOpeningDonationLinkMessageBox()
    {
        string therewasanerroropeningthedonation2 = (string)Application.Current.TryFindResource("Therewasanerroropeningthedonation") ?? "There was an error opening the Donation Link.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerroropeningthedonation2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void ToggleGamepadFailureMessageBox()
    {
        string failedtotogglegamepad2 = (string)Application.Current.TryFindResource("Failedtotogglegamepad") ?? "Failed to toggle gamepad.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{failedtotogglegamepad2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void FindRomCoverMissingMessageBox()
    {
        string findRomCoverexewasnotfound = (string)Application.Current.TryFindResource("FindRomCoverexewasnotfound") ?? "'FindRomCover.exe' was not found in the expected path.";
        string doyouwanttoreinstall = (string)Application.Current.TryFindResource("Doyouwanttoreinstall") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
        string fileNotFound = (string)Application.Current.TryFindResource("FileNotFound") ?? "File Not Found";
        MessageBoxResult reinstall = MessageBox.Show(
            $"{findRomCoverexewasnotfound}\n\n" +
            $"{doyouwanttoreinstall}",
            fileNotFound, MessageBoxButton.YesNo, MessageBoxImage.Error);

        if (reinstall == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            string pleasereinstallSimpleLaunchermanually = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            string pleaseReinstall = (string)Application.Current.TryFindResource("PleaseReinstall") ?? "Please Reinstall";
            MessageBox.Show(pleasereinstallSimpleLaunchermanually,
                pleaseReinstall, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    internal static void FindRomCoverLaunchWasCanceledByUserMessageBox()
    {
        string thelaunchofFindRomCoverexewascanceled = (string)Application.Current.TryFindResource("ThelaunchofFindRomCoverexewascanceled") ?? "The launch of 'FindRomCover.exe' was canceled by the user.";
        string operationCanceled = (string)Application.Current.TryFindResource("OperationCanceled") ?? "Operation Canceled";
        MessageBox.Show(thelaunchofFindRomCoverexewascanceled,
            operationCanceled, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void FindRomCoverLaunchWasBlockedMessageBox(string logPath)
    {
        string anerroroccurredwhiletryingtolaunch = (string)Application.Current.TryFindResource("Anerroroccurredwhiletryingtolaunch") ?? "An error occurred while trying to launch 'FindRomCover.exe'.";
        string yourcomputermaynothavegranted = (string)Application.Current.TryFindResource("Yourcomputermaynothavegranted") ?? "Your computer may not have granted the necessary permissions for 'Simple Launcher' to execute the file 'FindRomCover.exe'. Please ensure that 'Simple Launcher' has the required administrative privileges.";
        string alternativelythelaunchmayhavebeenblockedby = (string)Application.Current.TryFindResource("Alternativelythelaunchmayhavebeenblockedby") ?? "Alternatively, the launch may have been blocked by your antivirus software. If so, please configure your antivirus settings to allow 'FindRomCover.exe' to run.";
        string wouldyouliketoopentheerroruserlog = (string)Application.Current.TryFindResource("Wouldyouliketoopentheerroruserlog") ?? "Would you like to open the 'error_user.log' file to investigate the issue?";
        string error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show(
            $"{anerroroccurredwhiletryingtolaunch}\n\n" +
            $"{yourcomputermaynothavegranted}\n\n" +
            $"{alternativelythelaunchmayhavebeenblockedby}\n\n" +
            $"{wouldyouliketoopentheerroruserlog}",
            error, MessageBoxButton.YesNo, MessageBoxImage.Error);
        
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
                string thefileerroruserlog = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlog,
                    error, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
        
    internal static void ErrorChangingViewModeMessageBox()
    {
        string therewasanerrorwhilechangingtheviewmode2 = (string)Application.Current.TryFindResource("Therewasanerrorwhilechangingtheviewmode") ?? "There was an error while changing the view mode.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorwhilechangingtheviewmode2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
        
    internal static void NavigationButtonErrorMessageBox()
    {
        string therewasanerrorinthenavigationbutton2 = (string)Application.Current.TryFindResource("Therewasanerrorinthenavigationbutton") ?? "There was an error in the navigation button.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorinthenavigationbutton2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
        
    internal static void SelectSystemBeforeSearchMessageBox()
    {
        string pleaseselectasystembeforesearching = (string)Application.Current.TryFindResource("Pleaseselectasystembeforesearching") ?? "Please select a system before searching.";
        string systemNotSelected = (string)Application.Current.TryFindResource("SystemNotSelected") ?? "System Not Selected";
        MessageBox.Show(pleaseselectasystembeforesearching,
            systemNotSelected, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static void EnterSearchQueryMessageBox()
    {
        string pleaseenterasearchquery = (string)Application.Current.TryFindResource("Pleaseenterasearchquery") ?? "Please enter a search query.";
        string searchQueryRequired = (string)Application.Current.TryFindResource("SearchQueryRequired") ?? "Search Query Required";
        MessageBox.Show(pleaseenterasearchquery, 
            searchQueryRequired, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
    
    internal static void ErrorWhileLoadingHelpUserXmlMessageBox()
    {
        string unexpectederrorwhileloadinghelpuserxml2 = (string)Application.Current.TryFindResource("Unexpectederrorwhileloadinghelpuserxml") ?? "Unexpected error while loading 'helpuser.xml'.";
        string doyouwanttoautomaticreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{unexpectederrorwhileloadinghelpuserxml2}\n\n" +
                                     $"{doyouwanttoautomaticreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();   
        }
    }

    internal static void NoSystemInHelpUserXmlMessageBox()
    {
        string novalidsystemsfoundinthefilehelpuserxml2 = (string)Application.Current.TryFindResource("Novalidsystemsfoundinthefilehelpuserxml") ?? "No valid systems found in the file 'helpuser.xml'.";
        string doyouwanttoautomaticreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{novalidsystemsfoundinthefilehelpuserxml2}\n\n" +
                                     $"{doyouwanttoautomaticreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();   
        }
    }

    internal static bool CouldNotLoadHelpUserXmlMessageBox()
    {
        string simpleLaunchercouldnotloadhelpuserxml2 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotloadhelpuserxml") ?? "'Simple Launcher' could not load 'helpuser.xml'.";
        string doyouwanttoautomaticreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{simpleLaunchercouldnotloadhelpuserxml2}\n\n" +
                                     $"{doyouwanttoautomaticreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        
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
        string unabletoloadhelpuserxml2 = (string)Application.Current.TryFindResource("Unabletoloadhelpuserxml") ?? "Unable to load 'helpuser.xml'. The file may be corrupted.";
        string doyouwanttoautomaticreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{unabletoloadhelpuserxml2}\n\n" +
                                     $"{doyouwanttoautomaticreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        
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
        string thefilehelpuserxmlismissing2 = (string)Application.Current.TryFindResource("Thefilehelpuserxmlismissing") ?? "The file 'helpuser.xml' is missing.";
        string doyouwanttoautomaticreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{thefilehelpuserxmlismissing2}\n\n" +
                                     $"{doyouwanttoautomaticreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        
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
        string failedtoloadtheimageintheImage2 = (string)Application.Current.TryFindResource("FailedtoloadtheimageintheImage") ?? "Failed to load the image in the Image Viewer window.";
        string theimagemaybecorruptedorinaccessible2 = (string)Application.Current.TryFindResource("Theimagemaybecorruptedorinaccessible") ?? "The image may be corrupted or inaccessible.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{failedtoloadtheimageintheImage2}\n\n" +
                        $"{theimagemaybecorruptedorinaccessible2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void ReinstallSimpleLauncherFileCorruptedMessageBox()
    {
        string simpleLaunchercouldnotloadthefilemamexml2 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotloadthefilemamexml") ?? "'Simple Launcher' could not load the file 'mame.xml' or it is corrupted.";
        string doyouwanttoautomaticreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{simpleLaunchercouldnotloadthefilemamexml2}\n\n" +
                                     $"{doyouwanttoautomaticreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();   
        }
        else
        {
            string pleasereinstallSimpleLaunchermanually2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            string theapplicationwillshutdown2 = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
            string pleasereinstall2 = (string)Application.Current.TryFindResource("Pleasereinstall") ?? "Please reinstall";
            MessageBox.Show($"{pleasereinstallSimpleLaunchermanually2}\n\n" +
                            $"{theapplicationwillshutdown2}",
                pleasereinstall2, MessageBoxButton.OK, MessageBoxImage.Error);
                    
            // Shutdown the application and exit
            Application.Current.Shutdown();
            Environment.Exit(0);    
        }
    }

    internal static void ReinstallSimpleLauncherFileMissingMessageBox()
    {
        string thefilemamexmlcouldnotbefound2 = (string)Application.Current.TryFindResource("Thefilemamexmlcouldnotbefound") ?? "The file 'mame.xml' could not be found in the application folder.";
        string doyouwanttoautomaticreinstall2 = (string)Application.Current.TryFindResource("Doyouwanttoautomaticreinstall") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{thefilemamexmlcouldnotbefound2}\n\n" +
                                     $"{doyouwanttoautomaticreinstall2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();   
        }
        else
        {
            string pleasereinstallSimpleLaunchermanually2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            string theapplicationwillshutdown2 = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
            MessageBox.Show($"{pleasereinstallSimpleLaunchermanually2}\n\n" +
                            $"{theapplicationwillshutdown2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
                    
            // Shutdown the application and exit
            Application.Current.Shutdown();
            Environment.Exit(0);    
        }
    }

    internal static void UpdaterNotFoundMessageBox()
    {
        string updaterexenotfound2 = (string)Application.Current.TryFindResource("Updaterexenotfound") ?? "'Updater.exe' not found.";
        string pleasereinstallSimpleLaunchermanually2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the problem.";
        string theapplicationwillnowshutdown2 = (string)Application.Current.TryFindResource("Theapplicationwillnowshutdown") ?? "The application will now shutdown.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{updaterexenotfound2}\n\n" +
                        $"{pleasereinstallSimpleLaunchermanually2}\n\n" +
                        $"{theapplicationwillnowshutdown2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);

        // Shutdown the application and exit
        Application.Current.Shutdown();
        Environment.Exit(0);
    }

    internal static void ErrorLoadingRomHistoryMessageBox()
    {
        string anerroroccurredwhileloadingRoMhistory2 = (string)Application.Current.TryFindResource("AnerroroccurredwhileloadingROMhistory") ?? "An error occurred while loading ROM history.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{anerroroccurredwhileloadingRoMhistory2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void NoHistoryXmlFoundMessageBox()
    {
        string nohistoryxmlfilefound2 = (string)Application.Current.TryFindResource("Nohistoryxmlfilefound") ?? "No 'history.xml' file found in the application folder.";
        string doyouwanttoreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix this issue?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{nohistoryxmlfilefound2}\n\n" +
                                     $"{doyouwanttoreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);

        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
    }
    
    internal static void ErrorOpeningBrowserMessageBox()
    {
        string anerroroccurredwhileopeningthebrowser2 = (string)Application.Current.TryFindResource("Anerroroccurredwhileopeningthebrowser") ?? "An error occurred while opening the browser.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{anerroroccurredwhileopeningthebrowser2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void SimpleLauncherNeedMorePrivilegesMessageBox()
    {
        string simpleLauncherlackssufficientprivilegestowrite2 = (string)Application.Current.TryFindResource("SimpleLauncherlackssufficientprivilegestowrite") ?? "'Simple Launcher' lacks sufficient privileges to write to the 'settings.xml' file.";
        string pleasegrantSimpleLauncheradministrativeaccess2 = (string)Application.Current.TryFindResource("PleasegrantSimpleLauncheradministrativeaccess") ?? "Please grant 'Simple Launcher' administrative access.";
        string ensurethattheSimpleLauncherfolderislocatedinawritable2 = (string)Application.Current.TryFindResource("EnsurethattheSimpleLauncherfolderislocatedinawritable") ?? "Ensure that the 'Simple Launcher' folder is located in a writable directory.";
        string ifnecessarytemporarilydisableyourantivirus2 = (string)Application.Current.TryFindResource("Ifnecessarytemporarilydisableyourantivirus") ?? "If necessary, temporarily disable your antivirus software.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{simpleLauncherlackssufficientprivilegestowrite2}\n\n" +
                        $"{pleasegrantSimpleLauncheradministrativeaccess2}\n\n" +
                        $"{ensurethattheSimpleLauncherfolderislocatedinawritable2}\n\n" +
                        $"{ifnecessarytemporarilydisableyourantivirus2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void SystemXmlIsCorruptedMessageBox()
    {
        string systemxmliscorrupted2 = (string)Application.Current.TryFindResource("systemxmliscorrupted") ?? "'system.xml' is corrupted or could not be opened.";
        string pleasefixitmanuallyordeleteit2 = (string)Application.Current.TryFindResource("Pleasefixitmanuallyordeleteit") ?? "Please fix it manually or delete it.";
        string ifyouchoosetodeleteit2 = (string)Application.Current.TryFindResource("Ifyouchoosetodeleteit") ?? "If you choose to delete it, 'Simple Launcher' will create a new one for you.";
        string ifyouwanttodebugtheerroryourself2 = (string)Application.Current.TryFindResource("Ifyouwanttodebugtheerroryourself") ?? "If you want to debug the error yourself, check the 'error_user.log' file inside the 'Simple Launcher' folder.";
        string theapplicationwillshutdown2 = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{systemxmliscorrupted2}\n\n" +
                        $"{pleasefixitmanuallyordeleteit2}\n\n" +
                        $"{ifyouchoosetodeleteit2}\n\n" +
                        $"{ifyouwanttodebugtheerroryourself2}\n\n" +
                        $"{theapplicationwillshutdown2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
                    
        // Shutdown the application and exit
        Application.Current.Shutdown();
        Environment.Exit(0);
    }

    internal static void SystemModelXmlIsMissingMessageBox()
    {
        string systemmodelxmlismissing2 = (string)Application.Current.TryFindResource("systemmodelxmlismissing") ?? "'system_model.xml' is missing.";
        string simpleLaunchercannotworkproperly2 = (string)Application.Current.TryFindResource("SimpleLaunchercannotworkproperly") ?? "'Simple Launcher' cannot work properly without this file.";
        string doyouwanttoautomaticallyreinstall2 = (string)Application.Current.TryFindResource("Doyouwanttoautomaticallyreinstall") ?? "Do you want to automatically reinstall 'Simple Launcher' to fix the problem?";
        string missingfile2 = (string)Application.Current.TryFindResource("Missingfile") ?? "Missing file";
        var messageBoxResult = MessageBox.Show($"{systemmodelxmlismissing2}\n\n" +
                                               $"{simpleLaunchercannotworkproperly2}\n\n" +
                                               $"{doyouwanttoautomaticallyreinstall2}",
            missingfile2, MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (messageBoxResult == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            string pleasereinstallSimpleLaunchermanually2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the problem.";
            string theapplicationwillshutdown2 = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
            string warning2 = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
            MessageBox.Show($"{pleasereinstallSimpleLaunchermanually2}\n\n" +
                            $"{theapplicationwillshutdown2}",
                warning2, MessageBoxButton.OK, MessageBoxImage.Warning);

            // Shutdown the application and exit
            Application.Current.Shutdown();
            Environment.Exit(0);
        }
    }
    
    internal static void FiLeSystemXmlIsCorruptedMessageBox()
    {
        string thefilesystemxmlisbadlycorrupted2 = (string)Application.Current.TryFindResource("Thefilesystemxmlisbadlycorrupted") ?? "The file 'system.xml' is badly corrupted.";
        string toseethedetailschecktheerroruserlog2 = (string)Application.Current.TryFindResource("Toseethedetailschecktheerroruserlog") ?? "To see the details, check the 'error_user.log' file inside the 'Simple Launcher' folder.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{thefilesystemxmlisbadlycorrupted2}\n\n" +
                        $"{toseethedetailschecktheerroruserlog2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void InstallUpdateManuallyMessageBox(string repoOwner, string repoName)
    {
        string therewasanerrorinstallingorupdating2 = (string)Application.Current.TryFindResource("Therewasanerrorinstallingorupdating") ?? "There was an error installing or updating the application.";
        string wouldyouliketoberedirectedtothedownloadpage2 = (string)Application.Current.TryFindResource("Wouldyouliketoberedirectedtothedownloadpage") ?? "Would you like to be redirected to the download page to install or update it manually?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var messageBoxResult = MessageBox.Show(
            $"{therewasanerrorinstallingorupdating2}\n\n" +
            $"{wouldyouliketoberedirectedtothedownloadpage2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);

        if (messageBoxResult == MessageBoxResult.Yes)
        {
            string downloadPageUrl = $"https://github.com/{repoOwner}/{repoName}/releases/latest";
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
    }

    internal static void DownloadManuallyMessageBox(string repoOwner, string repoName)
    {
        string updaterexenotfoundintheapplication2 = (string)Application.Current.TryFindResource("Updaterexenotfoundintheapplication") ?? "'Updater.exe' not found in the application directory.";
        string wouldyouliketoberedirectedtotheSimpleLauncherdownloadpage2 = (string)Application.Current.TryFindResource("WouldyouliketoberedirectedtotheSimpleLauncherdownloadpage") ?? "Would you like to be redirected to the 'Simple Launcher' download page to download it manually?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var messageBoxResult = MessageBox.Show($"{updaterexenotfoundintheapplication2}\n\n" +
                                               $"{wouldyouliketoberedirectedtotheSimpleLauncherdownloadpage2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);

        if (messageBoxResult == MessageBoxResult.Yes)
        {
            string downloadPageUrl = $"https://github.com/{repoOwner}/{repoName}/releases/latest";
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
    }

    internal static void RequiredFileMissingMessageBox()
    {
        string fileappsettingsjsonismissing2 = (string)Application.Current.TryFindResource("Fileappsettingsjsonismissing") ?? "File 'appsettings.json' is missing.";
        string theapplicationwillnotbeableto2 = (string)Application.Current.TryFindResource("Theapplicationwillnotbeableto") ?? "The application will not be able to send the Bug Report.";
        string doyouwanttoautomaticallyreinstall2 = (string)Application.Current.TryFindResource("Doyouwanttoautomaticallyreinstall") ?? "Do you want to automatically reinstall 'Simple Launcher' to fix the problem?";
        string warning2 = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        var messageBoxResult = MessageBox.Show(
            $"{fileappsettingsjsonismissing2}\n\n" +
            $"{theapplicationwillnotbeableto2}\n\n" +
            $"{doyouwanttoautomaticallyreinstall2}",
            warning2, MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (messageBoxResult == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            string pleasereinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLauncher") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            MessageBox.Show(pleasereinstallSimpleLauncher2,
                warning2, MessageBoxButton.OK,MessageBoxImage.Warning);
        }
    }
    
    internal static void EnterBugDetailsMessageBox()
    {
        string pleaseenterthedetailsofthebug2 = (string)Application.Current.TryFindResource("Pleaseenterthedetailsofthebug") ?? "Please enter the details of the bug.";
        string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(pleaseenterthedetailsofthebug2,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void ApiKeyErrorMessageBox()
    {
        string therewasanerrorintheApiKey2 = (string)Application.Current.TryFindResource("TherewasanerrorintheAPIKey") ?? "There was an error in the API Key of this form.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorintheApiKey2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void BugReportSuccessMessageBox()
    {
        string bugreportsent2 = (string)Application.Current.TryFindResource("Bugreportsent") ?? "Bug report sent successfully.";
        string success2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        MessageBox.Show(bugreportsent2, 
            success2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void BugReportSendErrorMessageBox()
    {
        string anerroroccurredwhilesending2 = (string)Application.Current.TryFindResource("Anerroroccurredwhilesending") ?? "An error occurred while sending the bug report.";
        string thebugwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Thebugwasreportedtothedeveloper") ?? "The bug was reported to the developer that will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{anerroroccurredwhilesending2}\n\n" +
                        $"{thebugwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void ExtractionFailedMessageBox()
    {
        string extractionfailed2 = (string)Application.Current.TryFindResource("Extractionfailed") ?? "Extraction failed.";
        string ensurethefileisnotcorrupted2 = (string)Application.Current.TryFindResource("Ensurethefileisnotcorrupted") ?? "Ensure the file is not corrupted.";
        string grantadministrativeaccesstoSimple2 = (string)Application.Current.TryFindResource("GrantadministrativeaccesstoSimple") ?? "Grant administrative access to 'Simple Launcher'.";
        string ensureSimpleLauncherisinawritable2 = (string)Application.Current.TryFindResource("EnsureSimpleLauncherisinawritable") ?? "Ensure 'Simple Launcher' is in a writable folder.";
        string temporarilydisableyourantivirussoftware2 = (string)Application.Current.TryFindResource("Temporarilydisableyourantivirussoftware") ?? "Temporarily disable your antivirus software and try again.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{extractionfailed2}\n\n" +
                        $"{ensurethefileisnotcorrupted2}\n" +
                        $"{grantadministrativeaccesstoSimple2}\n" +
                        $"{ensureSimpleLauncherisinawritable2}\n" +
                        $"{temporarilydisableyourantivirussoftware2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void FileNeedToBeCompressedMessageBox()
    {
        string theselectedfilecannotbe2 = (string)Application.Current.TryFindResource("Theselectedfilecannotbe") ?? "The selected file cannot be extracted.";
        string toextractafileitneedstobe2 = (string)Application.Current.TryFindResource("Toextractafileitneedstobe") ?? "To extract a file, it needs to be a 7z, zip, or rar file.";
        string pleasefixthatintheEditwindow2 = (string)Application.Current.TryFindResource("PleasefixthatintheEditwindow") ?? "Please fix that in the Edit window.";
        string invalidFile2 = (string)Application.Current.TryFindResource("InvalidFile") ?? "Invalid File";
        MessageBox.Show($"{theselectedfilecannotbe2}\n\n" +
                        $"{toextractafileitneedstobe2}\n\n" +
                        $"{pleasefixthatintheEditwindow2}",
            invalidFile2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
    
    internal static void DownloadedFileIsMissingMessageBox()
    {
        string downloadedfileismissing2 = (string)Application.Current.TryFindResource("Downloadedfileismissing") ?? "Downloaded file is missing.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{downloadedfileismissing2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void FileIsLockedMessageBox()
    {
        string downloadedfileislocked2 = (string)Application.Current.TryFindResource("Downloadedfileislocked") ?? "Downloaded file is locked.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{downloadedfileislocked2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void ImagePackDownloadExtractionFailedMessageBox()
    {
        string imagePackdownloadorextraction2 = (string)Application.Current.TryFindResource("ImagePackdownloadorextraction") ?? "Image Pack download or extraction failed!";
        string grantSimpleLauncheradministrative2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        string ensuretheSimpleLauncher2 = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncher") ?? "Ensure the 'Simple Launcher' folder is a writable directory.";
        string temporarilydisableyourantivirussoftware2 = (string)Application.Current.TryFindResource("Temporarilydisableyourantivirussoftware") ?? "Temporarily disable your antivirus software and try again.";
        string downloadorExtractionFailed2 = (string)Application.Current.TryFindResource("DownloadorExtractionFailed") ?? "Download or extraction failed.";
        MessageBox.Show($"{imagePackdownloadorextraction2}\n\n" +
                        $"{grantSimpleLauncheradministrative2}\n\n" +
                        $"{ensuretheSimpleLauncher2}\n\n" +
                        $"{temporarilydisableyourantivirussoftware2}",
            downloadorExtractionFailed2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void DownloadExtractionSuccessfullyMessageBox()
    {
        string thedownloadandextractionweresuccessful2 = (string)Application.Current.TryFindResource("Thedownloadandextractionweresuccessful") ?? "The download and extraction were successful.";
        string success2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        MessageBox.Show(thedownloadandextractionweresuccessful2,
            success2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void ImagePackDownloadErrorOfferRedirectMessageBox(EasyModeSystemConfig selectedSystem)
    {
        string downloaderror2 = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
        string wouldyouliketoberedirected2 = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBoxResult result = MessageBox.Show($"{downloaderror2}\n\n" +
                                                  $"{wouldyouliketoberedirected2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
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
                string formattedException = $"Error opening the Browser.\n\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                // Notify user
                string simpleLaunchercouldnotopentheImage2 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotopentheImage") ?? "'Simple Launcher' could not open the Image Pack download link.";
                MessageBox.Show(simpleLaunchercouldnotopentheImage2,
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    internal static void IoExceptionMessageBox(string tempFolder)
    {
        string afilereadwriteerroroccurred2 = (string)Application.Current.TryFindResource("Afilereadwriteerroroccurred") ?? "A file read/write error occurred after the file was downloaded.";
        string thiserrormayoccurifanantivirus2 = (string)Application.Current.TryFindResource("Thiserrormayoccurifanantivirus") ?? "This error may occur if an antivirus program is locking or scanning the newly downloaded files, causing access issues. Try temporarily disabling real-time protection.";
        string additionallygrantSimpleLauncher2 = (string)Application.Current.TryFindResource("AdditionallygrantSimpleLauncher") ?? "Additionally, grant 'Simple Launcher' administrative access to enable file writing.";
        string makesuretheSimpleLauncher2 = (string)Application.Current.TryFindResource("MakesuretheSimpleLauncher") ?? "Make sure the 'Simple Launcher' folder is located in a writable directory.";
        string wouldyouliketoopenthetemp2 = (string)Application.Current.TryFindResource("Wouldyouliketoopenthetemp") ?? "Would you like to open the 'temp' folder to view the downloaded file?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";            
        var result = MessageBox.Show($"{afilereadwriteerroroccurred2}\n\n" +
                                     $"{thiserrormayoccurifanantivirus2}\n\n" +
                                     $"{additionallygrantSimpleLauncher2}\n\n" +
                                     $"{makesuretheSimpleLauncher2}\n\n" +
                                     $"{wouldyouliketoopenthetemp2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = tempFolder,
                    UseShellExecute = true
                });
            }
            catch (Exception)
            {
                string simpleLauncherwasunabletoopenthe2 = (string)Application.Current.TryFindResource("SimpleLauncherwasunabletoopenthe") ?? "'Simple Launcher' was unable to open the 'temp' folder due to access issues.";
                MessageBox.Show($"{simpleLauncherwasunabletoopenthe2}\n\n" +
                                $"{tempFolder}",
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static void DownloadErrorMessageBox()
    {
        string downloaderror2 = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{downloaderror2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void ErrorLoadingEasyModeXmlMessageBox()
    {
        string errorloadingthefileeasymodexml2 = (string)Application.Current.TryFindResource("Errorloadingthefileeasymodexml") ?? "Error loading the file 'easymode.xml'.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string doyouwanttoreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{errorloadingthefileeasymodexml2}\n\n" +
                                     $"{theerrorwasreportedtothedeveloper2}\n" +
                                     $"{doyouwanttoreinstallSimpleLauncher2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            string pleasereinstallSimpleLaunchermanually2 = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            MessageBox.Show(pleasereinstallSimpleLaunchermanually2, 
                error2, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    internal static void LinksSavedMessageBox()
    {
        string linkssavedsuccessfully2 = (string)Application.Current.TryFindResource("Linkssavedsuccessfully") ?? "Links saved successfully.";
        string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(linkssavedsuccessfully2,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void LinksRevertedMessageBox()
    {
        string linksreverted2 = (string)Application.Current.TryFindResource("Linksrevertedtodefaultvalues") ?? "Links reverted to default values.";
        string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(linksreverted2, 
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void MainWindowSearchEngineErrorMessageBox()
    {
        string therewasanerrorwiththesearchengine2 = (string)Application.Current.TryFindResource("Therewasanerrorwiththesearchengine") ?? "There was an error with the search engine.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorwiththesearchengine2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void DownloadExtractionFailedMessageBox()
    {
        string downloadorextractionfailed2 = (string)Application.Current.TryFindResource("DownloadorExtractionFailed") ?? "Download or extraction failed.";
        string grantSimpleLauncheradministrativeaccess2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrativeaccess") ?? "Grant 'Simple Launcher' administrative access and try again.";
        string ensuretheSimpleLauncherfolder2 = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncherfolder") ?? "Ensure the 'Simple Launcher' folder is a writable directory.";
        string temporarilydisableyourantivirus2 = (string)Application.Current.TryFindResource("Temporarilydisableyourantivirus") ?? "Temporarily disable your antivirus software and try again.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{downloadorextractionfailed2}\n\n" +
                        $"{grantSimpleLauncheradministrativeaccess2}\n\n" +
                        $"{ensuretheSimpleLauncherfolder2}\n\n" +
                        $"{temporarilydisableyourantivirus2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void DownloadCanceledMessageBox()
    {
        string downloadwascanceled2 = (string)Application.Current.TryFindResource("Downloadwascanceled") ?? "Download was canceled.";
        string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(downloadwascanceled2,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void DownloadFailedMessageBox()
    {
        string downloadfailed2 = (string)Application.Current.TryFindResource("Downloadfailed") ?? "Download failed.";
        string grantSimpleLauncheradministrative2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        string ensuretheSimpleLauncherfolder2 = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncherfolder") ?? "Ensure the 'Simple Launcher' folder is a writable directory.";
        string temporarilydisableyourantivirus2 = (string)Application.Current.TryFindResource("Temporarilydisableyourantivirus") ?? "Temporarily disable your antivirus software and try again.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{downloadfailed2}\n\n" +
                        $"{grantSimpleLauncheradministrative2}\n\n" +
                        $"{ensuretheSimpleLauncherfolder2}\n\n" +
                        $"{temporarilydisableyourantivirus2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void DownloadAndExtrationWereSuccessfulMessageBox()
    {
        string downloadingandextractionweresuccessful2 = (string)Application.Current.TryFindResource("Downloadingandextractionweresuccessful") ?? "Downloading and extraction were successful.";
        string success2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        MessageBox.Show($"{downloadingandextractionweresuccessful2}",
            success2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static async Task EmulatorDownloadErrorMessageBox(EasyModeSystemConfig selectedSystem, Exception ex)
    {
        string downloaderror2 = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
        string wouldyouliketoberedirected2 = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBoxResult result = MessageBox.Show($"{downloaderror2}\n\n" +
                                                  $"{wouldyouliketoberedirected2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
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
                string formattedException2 = $"Error opening the download link.\n\n" +
                                             $"Exception type: {ex.GetType().Name}\n" +
                                             $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex2, formattedException2);
                            
                // Notify user
                string erroropeningthedownloadlink2 = (string)Application.Current.TryFindResource("Erroropeningthedownloadlink") ?? "Error opening the download link.";
                string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
                string error3 = (string)Application.Current.TryFindResource("Error") ?? "Error";
                MessageBox.Show($"{erroropeningthedownloadlink2}\n\n" +
                                $"{theerrorwasreportedtothedeveloper2}",
                    error3, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static async Task CoreDownloadErrorMessageBox(EasyModeSystemConfig selectedSystem, Exception ex)
    {
        string downloaderror2 = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
        string wouldyouliketoberedirected2 = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBoxResult result = MessageBox.Show($"{downloaderror2}\n\n" +
                                                  $"{wouldyouliketoberedirected2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
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
            catch (Exception ex2)
            {
                // Notify developer
                string formattedException2 = $"Error opening the download link.\n\n" +
                                             $"Exception type: {ex.GetType().Name}\n" +
                                             $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex2, formattedException2);
                            
                // Notify user
                string erroropeningthedownloadlink2 = (string)Application.Current.TryFindResource("Erroropeningthedownloadlink") ?? "Error opening the download link.";
                string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
                string error3 = (string)Application.Current.TryFindResource("Error") ?? "Error";
                MessageBox.Show($"{erroropeningthedownloadlink2}\n\n" +
                                $"{theerrorwasreportedtothedeveloper2}",
                    error3, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    internal static async Task ImagePackDownloadErrorMessageBox(EasyModeSystemConfig selectedSystem, Exception ex)
    {
        string downloaderror2 = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
        string wouldyouliketoberedirected2 = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBoxResult result = MessageBox.Show($"{downloaderror2}\n\n" +
                                                  $"{wouldyouliketoberedirected2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
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
                string formattedException2 = $"Error opening the download link.\n\n" +
                                             $"Exception type: {ex.GetType().Name}\n" +
                                             $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex2, formattedException2);
                            
                // Notify user
                string erroropeningthedownloadlink2 = (string)Application.Current.TryFindResource("Erroropeningthedownloadlink") ?? "Error opening the download link.";
                string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
                string error3 = (string)Application.Current.TryFindResource("Error") ?? "Error";
                MessageBox.Show($"{erroropeningthedownloadlink2}\n\n" +
                                $"{theerrorwasreportedtothedeveloper2}",
                    error3, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    internal static bool OverwriteSystemMessageBox(EasyModeSystemConfig selectedSystem)
    {
        string thesystem3 = (string)Application.Current.TryFindResource("Thesystem") ?? "The system";
        string alreadyexists2 = (string)Application.Current.TryFindResource("alreadyexists") ?? "already exists.";
        string doyouwanttooverwriteit2 = (string)Application.Current.TryFindResource("Doyouwanttooverwriteit") ?? "Do you want to overwrite it?";
        string systemAlreadyExists2 = (string)Application.Current.TryFindResource("SystemAlreadyExists") ?? "System Already Exists";
        MessageBoxResult result = MessageBox.Show($"{thesystem3} '{selectedSystem.SystemName}' {alreadyexists2}\n\n" +
                                                  $"{doyouwanttooverwriteit2}", systemAlreadyExists2, MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result == MessageBoxResult.No)
        {
            return true;
        }

        return false;
    }
    
    internal static void SystemAddedMessageBox(string systemFolder, string fullImageFolderPathForMessage, EasyModeSystemConfig selectedSystem)
    {
        string thesystem2 = (string)Application.Current.TryFindResource("Thesystem") ?? "The system";
        string hasbeenaddedsuccessfully2 = (string)Application.Current.TryFindResource("hasbeenaddedsuccessfully") ?? "has been added successfully.";
        string putRoMsorIsOsforthissysteminside2 = (string)Application.Current.TryFindResource("PutROMsorISOsforthissysteminside") ?? "Put ROMs or ISOs for this system inside";
        string putcoverimagesforthissysteminside2 = (string)Application.Current.TryFindResource("Putcoverimagesforthissysteminside") ?? "Put cover images for this system inside";
        string systemAdded2 = (string)Application.Current.TryFindResource("SystemAdded") ?? "System Added";
        MessageBox.Show($"{thesystem2} '{selectedSystem.SystemName}' {hasbeenaddedsuccessfully2}\n\n" +
                        $"{putRoMsorIsOsforthissysteminside2} '{systemFolder}'\n\n" +
                        $"{putcoverimagesforthissysteminside2} '{fullImageFolderPathForMessage}'.",
            systemAdded2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void AddSystemFailedMessageBox()
    {
        string therewasanerroradding2 = (string)Application.Current.TryFindResource("Therewasanerroradding") ?? "There was an error adding this system.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerroradding2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void RightClickContextMenuErrorMessageBox()
    {
        string therewasanerrorintherightclick2 = (string)Application.Current.TryFindResource("Therewasanerrorintherightclick") ?? "There was an error in the right-click context menu.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorintherightclick2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void SelectGameToLaunchMessageBox()
    {
        string pleaseselectagametolaunch2 = (string)Application.Current.TryFindResource("Pleaseselectagametolaunch") ?? "Please select a game to launch.";
        string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(pleaseselectagametolaunch2,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void GameFileDoesNotExistMessageBox()
    {
        string thegamefiledoesnotexist2 = (string)Application.Current.TryFindResource("Thegamefiledoesnotexist") ?? "The game file does not exist!";
        string thefavoritehasbeenremovedfromthelist2 = (string)Application.Current.TryFindResource("Thefavoritehasbeenremovedfromthelist") ?? "The favorite has been removed from the list.";
        string fileNotFound2 = (string)Application.Current.TryFindResource("FileNotFound") ?? "File Not Found";
        MessageBox.Show($"{thegamefiledoesnotexist2}\n\n" +
                        $"{thefavoritehasbeenremovedfromthelist2}",
            fileNotFound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void CouldNotOpenVideoLinkMessageBox()
    {
        string therewasaproblemopeningtheVideoLink2 = (string)Application.Current.TryFindResource("TherewasaproblemopeningtheVideoLink") ?? "There was a problem opening the Video Link.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasaproblemopeningtheVideoLink2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void CouldNotOpenInfoLinkMessageBox()
    {
        string therewasaproblemopeningtheInfoLink2 = (string)Application.Current.TryFindResource("TherewasaproblemopeningtheInfoLink") ?? "There was a problem opening the Info Link.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasaproblemopeningtheInfoLink2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void CouldNotOpenHistoryWindowMessageBox()
    {
        string therewasaproblemopeningtheHistorywindow2 = (string)Application.Current.TryFindResource("TherewasaproblemopeningtheHistorywindow") ?? "There was a problem opening the History window.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasaproblemopeningtheHistorywindow2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ErrorOpeningCoverImageMessageBox()
    {
        string therewasanerrortryingtoopenthe2 = (string)Application.Current.TryFindResource("Therewasanerrortryingtoopenthe") ?? "There was an error trying to open the cover image.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrortryingtoopenthe2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
   
    internal static void NoGameplaySnapshotMessageBox()
    {
        string thereisnogameplaysnapshotassociated2 = (string)Application.Current.TryFindResource("Thereisnogameplaysnapshotassociated") ?? "There is no gameplay snapshot associated with this favorite.";
        string gameplaySnapshotnotfound2 = (string)Application.Current.TryFindResource("GameplaySnapshotnotfound") ?? "Gameplay Snapshot not found";
        MessageBox.Show(thereisnogameplaysnapshotassociated2,
            gameplaySnapshotnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    internal static void CouldNotOpenWalkthroughMessageBox()
    {
        string failedtoopenthewalkthroughfile2 = (string)Application.Current.TryFindResource("Failedtoopenthewalkthroughfile") ?? "Failed to open the walkthrough file.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{failedtoopenthewalkthroughfile2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void TakeScreenShotErrorMessageBox()
    {
        string simpleLaunchercouldnottakethescreenshot2 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnottakethescreenshot") ?? "'Simple Launcher' could not take the screenshot.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{simpleLaunchercouldnottakethescreenshot2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void FileDeletedMessageBox(string fileNameWithExtension)
    {
        string thefile2 = (string)Application.Current.TryFindResource("Thefile") ?? "The file";
        string hasbeensuccessfullydeleted2 = (string)Application.Current.TryFindResource("hasbeensuccessfullydeleted") ?? "has been successfully deleted.";
        string fileDeleted2 = (string)Application.Current.TryFindResource("FileDeleted") ?? "File Deleted";
        MessageBox.Show($"{thefile2} \"{fileNameWithExtension}\" {hasbeensuccessfullydeleted2}",
            fileDeleted2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void CouldNotDeleteTheFileMessageBox()
    {
        string anerroroccurredwhiletrying2 = (string)Application.Current.TryFindResource("Anerroroccurredwhiletrying") ?? "An error occurred while trying to delete the file.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{anerroroccurredwhiletrying2}" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void SelectAFavoriteToRemoveMessageBox()
    {
        string pleaseselectafavoritetoremove2 = (string)Application.Current.TryFindResource("Pleaseselectafavoritetoremove") ?? "Please select a favorite to remove.";
        string warning2 = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        MessageBox.Show(pleaseselectafavoritetoremove2,
            warning2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
    
    internal static void SystemXmlNotFoundMessageBox()
    {
        string systemxmlnotfound2 = (string)Application.Current.TryFindResource("systemxmlnotfound") ?? "'system.xml' not found inside the application folder.";
        string pleaserestartSimpleLauncher2 = (string)Application.Current.TryFindResource("PleaserestartSimpleLauncher") ?? "Please restart 'Simple Launcher'.";
        string ifthatdoesnotwork2 = (string)Application.Current.TryFindResource("Ifthatdoesnotwork") ?? "If that does not work, please reinstall 'Simple Launcher'.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{systemxmlnotfound2}\n\n" +
                        $"{pleaserestartSimpleLauncher2}\n\n" +
                        $"{ifthatdoesnotwork2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void YouCanAddANewSystemMessageBox()
    {
        string youcanaddanewsystem2 = (string)Application.Current.TryFindResource("Youcanaddanewsystem") ?? "You can add a new system now.";
        string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(youcanaddanewsystem2,
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void EmulatorNameMustBeUniqueMessageBox(string emulator1NameText)
    {
        string thename2 = (string)Application.Current.TryFindResource("Thename") ?? "The name";
        string isusedmultipletimes2 = (string)Application.Current.TryFindResource("isusedmultipletimes") ?? "is used multiple times. Each emulator name must be unique.";
        string validationError2 = (string)Application.Current.TryFindResource("ValidationError") ?? "Validation Error";
        MessageBox.Show($"{thename2} '{emulator1NameText}' {isusedmultipletimes2}",
            validationError2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void EmulatorNameRequiredMessageBox(int i)
    {
        string emulator2 = (string)Application.Current.TryFindResource("Emulator") ?? "Emulator";
        string nameisrequiredbecauserelateddata2 = (string)Application.Current.TryFindResource("nameisrequiredbecauserelateddata") ?? "name is required because related data has been provided.";
        string pleasefixthisfield2 = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        string validationError2 = (string)Application.Current.TryFindResource("ValidationError") ?? "Validation Error";
        MessageBox.Show($"{emulator2} {i + 2} {nameisrequiredbecauserelateddata2}\n\n" +
                        $"{pleasefixthisfield2}",
            validationError2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void EmulatorNameMustBeUniqueMessageBox2(string emulatorName)
    {
        string thename2 = (string)Application.Current.TryFindResource("Thename") ?? "The name";
        string isusedmultipletimes2 = (string)Application.Current.TryFindResource("isusedmultipletimes") ?? "is used multiple times. Each emulator name must be unique.";
        string validationError2 = (string)Application.Current.TryFindResource("ValidationError") ?? "Validation Error";
        MessageBox.Show($"{thename2} '{emulatorName}' {isusedmultipletimes2}",
            validationError2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void SystemSavedSuccessfullyMessageBox()
    {
        string systemsavedsuccessfully2 = (string)Application.Current.TryFindResource("Systemsavedsuccessfully") ?? "System saved successfully.";
        string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show(systemsavedsuccessfully2, info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void PathOrParameterInvalidMessageBox()
    {
        string oneormorepathsorparameters2 = (string)Application.Current.TryFindResource("Oneormorepathsorparameters") ?? "One or more paths or parameters are invalid.";
        string pleasefixthemtoproceed2 = (string)Application.Current.TryFindResource("Pleasefixthemtoproceed") ?? "Please fix them to proceed.";
        string validationError2 = (string)Application.Current.TryFindResource("ValidationError") ?? "Validation Error";
        MessageBox.Show($"{oneormorepathsorparameters2}\n\n" +
                        $"{pleasefixthemtoproceed2}",
            validationError2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void Emulator1RequiredMessageBox()
    {
        string emulator1Nameisrequired2 = (string)Application.Current.TryFindResource("Emulator1Nameisrequired") ?? "'Emulator 1 Name' is required.";
        string pleasefixthisfield2 = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        string validationError2 = (string)Application.Current.TryFindResource("ValidationError") ?? "Validation Error";
        MessageBox.Show($"{emulator1Nameisrequired2}\n\n" +
                        $"{pleasefixthisfield2}",
            validationError2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void ExtensionToLaunchIsRequiredMessageBox()
    {
        string extensiontoLaunchAfterExtraction2 = (string)Application.Current.TryFindResource("ExtensiontoLaunchAfterExtraction") ?? "'Extension to Launch After Extraction' is required when 'Extract File Before Launch' is set to true.";
        string pleasefixthisfield2 = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        string validationError2 = (string)Application.Current.TryFindResource("ValidationError") ?? "Validation Error";
        MessageBox.Show($"{extensiontoLaunchAfterExtraction2}\n\n" +
                        $"{pleasefixthisfield2}", 
            validationError2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void ExtensionToSearchIsRequiredMessageBox()
    {
        string extensiontoSearchintheSystemFolder2 = (string)Application.Current.TryFindResource("ExtensiontoSearchintheSystemFolder") ?? "'Extension to Search in the System Folder' cannot be empty or contain only spaces.";
        string pleasefixthisfield2 = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        string validationError2 = (string)Application.Current.TryFindResource("ValidationError") ?? "Validation Error";
        MessageBox.Show($"{extensiontoSearchintheSystemFolder2}\n\n" +
                        $"{pleasefixthisfield2}", 
            validationError2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void FileMustBeCompressedMessageBox()
    {
        string whenExtractFileBeforeLaunch2 = (string)Application.Current.TryFindResource("WhenExtractFileBeforeLaunch") ?? "When 'Extract File Before Launch' is set to true, 'Extension to Search in the System Folder' must include 'zip', '7z', or 'rar'.";
        string itwillnotacceptotherextensions2 = (string)Application.Current.TryFindResource("Itwillnotacceptotherextensions") ?? "It will not accept other extensions.";
        string validationError2 = (string)Application.Current.TryFindResource("ValidationError") ?? "Validation Error";
        MessageBox.Show($"{whenExtractFileBeforeLaunch2}\n\n{itwillnotacceptotherextensions2}",
            validationError2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void SystemImageFolderCanNotBeEmptyMessageBox()
    {
        string systemImageFoldercannotbeempty2 = (string)Application.Current.TryFindResource("SystemImageFoldercannotbeempty") ?? "'System Image Folder' cannot be empty or contain only spaces.";
        string pleasefixthisfield2 = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        string validationError2 = (string)Application.Current.TryFindResource("ValidationError") ?? "Validation Error";
        MessageBox.Show($"{systemImageFoldercannotbeempty2}\n\n{pleasefixthisfield2}",
            validationError2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void SystemFolderCanNotBeEmptyMessageBox()
    {
        string systemFoldercannotbeempty2 = (string)Application.Current.TryFindResource("SystemFoldercannotbeempty") ?? "'System Folder' cannot be empty or contain only spaces.";
        string pleasefixthisfield2 = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        string validationError2 = (string)Application.Current.TryFindResource("ValidationError") ?? "Validation Error";
        MessageBox.Show($"{systemFoldercannotbeempty2}\n\n{pleasefixthisfield2}",
            validationError2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void SystemNameCanNotBeEmptyMessageBox()
    {
        string systemNamecannotbeemptyor2 = (string)Application.Current.TryFindResource("SystemNamecannotbeemptyor") ?? "'System Name' cannot be empty or contain only spaces.";
        string pleasefixthisfield2 = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        string validationError2 = (string)Application.Current.TryFindResource("ValidationError") ?? "Validation Error";
        MessageBox.Show($"{systemNamecannotbeemptyor2}\n\n{pleasefixthisfield2}",
            validationError2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void FolderCreatedMessageBox(string systemNameText)
    {
        string simpleLaunchercreatedaimagefolder2 = (string)Application.Current.TryFindResource("SimpleLaunchercreatedaimagefolder") ?? "'Simple Launcher' created a image folder for this system at";
        string youmayplacethecoverimagesforthissystem2 = (string)Application.Current.TryFindResource("Youmayplacethecoverimagesforthissysteminside") ?? "You may place the cover images for this system inside this folder.";
        string italsocreatedfoldersfor2 = (string)Application.Current.TryFindResource("Italsocreatedfoldersfor") ?? "It also created folders for";
        string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show($"{simpleLaunchercreatedaimagefolder2} '.\\images\\{systemNameText}'.\n\n" +
                        $"{youmayplacethecoverimagesforthissystem2}\n\n" +
                        $"{italsocreatedfoldersfor2} 'title_snapshots', 'gameplay_snapshots', 'videos', 'manuals', 'walkthrough', 'cabinets', 'flyers', 'pcbs' and 'carts'.",
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void FolderCreationFailedMessageBox()
    {
        string simpleLauncherfailedtocreatethe2 = (string)Application.Current.TryFindResource("SimpleLauncherfailedtocreatethe") ?? "'Simple Launcher' failed to create the necessary folders for this system.";
        string theapplicationmightnothave2 = (string)Application.Current.TryFindResource("Theapplicationmightnothave") ?? "The application might not have sufficient privileges. Try running it with administrative permissions.";
        string additionallyensurethatSimpleLauncher2 = (string)Application.Current.TryFindResource("AdditionallyensurethatSimpleLauncher") ?? "Additionally, ensure that 'Simple Launcher' is located in a writable folder.";
        string info2 = (string)Application.Current.TryFindResource("Info") ?? "Info";
        MessageBox.Show($"{simpleLauncherfailedtocreatethe2}\n\n" +
                        $"{theapplicationmightnothave2}\n\n" +
                        $"{additionallyensurethatSimpleLauncher2}",
            info2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void SelectASystemToDeleteMessageBox()
    {
        string pleaseselectasystemtodelete2 = (string)Application.Current.TryFindResource("Pleaseselectasystemtodelete") ?? "Please select a system to delete.";
        string warning2 = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        MessageBox.Show(pleaseselectasystemtodelete2, warning2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
    
    internal static void SystemNotFoundInTheXmlMessageBox()
    {
        string selectedsystemnotfound2 = (string)Application.Current.TryFindResource("Selectedsystemnotfound") ?? "Selected system not found in the XML document!";
        string alert2 = (string)Application.Current.TryFindResource("Alert") ?? "Alert";
        MessageBox.Show(selectedsystemnotfound2,
            alert2, MessageBoxButton.OK, MessageBoxImage.Exclamation);
    }
    
    internal static void ErrorFindingGameFilesMessageBox(string logPath)
    {
        string therewasanerrorfinding2 = (string)Application.Current.TryFindResource("Therewasanerrorfinding") ?? "There was an error finding the game files.";
        string doyouwanttoopenthefileerroruserlog2 = (string)Application.Current.TryFindResource("Doyouwanttoopenthefileerroruserlog") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{therewasanerrorfinding2}\n\n" +
                                     $"{doyouwanttoopenthefileerroruserlog2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
                
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
                string thefileerroruserlogwas2 = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ?? "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlogwas2,
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    internal static void ErrorWhileCountingFilesMessageBox(string logPath)
    {
        string anerroroccurredwhilecounting2 = (string)Application.Current.TryFindResource("Anerroroccurredwhilecounting") ?? "An error occurred while counting files.";
        string doyouwanttoopenthefileerroruserlog2 = (string)Application.Current.TryFindResource("Doyouwanttoopenthefileerroruserlog") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{anerroroccurredwhilecounting2}\n\n" +
                                     $"{doyouwanttoopenthefileerroruserlog2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
                
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
                string thefileerroruserlog2 = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlog2,
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    internal static void GamePadErrorMessageBox()
    {
        string therewasanerrorwiththeGamePadController2 = (string)Application.Current.TryFindResource("TherewasanerrorwiththeGamePadController") ?? "There was an error with the GamePad Controller.";
        string runningSimpleLauncherwithadministrative2 = (string)Application.Current.TryFindResource("RunningSimpleLauncherwithadministrative") ?? "Running 'Simple Launcher' with administrative access may fix this problem.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{therewasanerrorwiththeGamePadController2}\n\n" +
                        $"{runningSimpleLauncherwithadministrative2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void CouldNotLaunchGameMessageBox(string logPath)
    {
        string simpleLaunchercouldnotlaunch2 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotlaunch") ?? "'Simple Launcher' could not launch the selected game.";
        string ifyouaretryingtorunMamEensurethatyourRom2 = (string)Application.Current.TryFindResource("IfyouaretryingtorunMAMEensurethatyourROM") ?? "If you are trying to run MAME, ensure that your ROM collection is compatible with the MAME version you are using.";
        string ifyouaretryingtorunRetroarchensurethattheBios2 = (string)Application.Current.TryFindResource("IfyouaretryingtorunRetroarchensurethattheBIOS") ?? "If you are trying to run Retroarch, ensure that the BIOS or required files for the core are installed.";
        string alsomakesureyouarecallingtheemulator2 = (string)Application.Current.TryFindResource("Alsomakesureyouarecallingtheemulator") ?? "Also, make sure you are calling the emulator with the correct parameter.";
        string doyouwanttoopenthefile2 = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show(
            $"{simpleLaunchercouldnotlaunch2}\n\n" +
            $"{ifyouaretryingtorunMamEensurethatyourRom2}\n\n" +
            $"{ifyouaretryingtorunRetroarchensurethattheBios2}\n\n" +
            $"{alsomakesureyouarecallingtheemulator2}\n\n" +
            $"{doyouwanttoopenthefile2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
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
                string thefileerroruserlogwas2 = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ?? "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlogwas2,
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    internal static void InvalidOperationExceptionMessageBox()
    {
        string failedtostarttheemulator2 = (string)Application.Current.TryFindResource("Failedtostarttheemulator") ?? "Failed to start the emulator or it has not exited as expected.";
        string thistypeoferrorhappenswhenSimpleLauncher2 = (string)Application.Current.TryFindResource("ThistypeoferrorhappenswhenSimpleLauncher") ?? "This type of error happens when 'Simple Launcher' does not have the privileges to launch an external program, such as an emulator.";
        string grantSimpleLauncheradministrativeaccess2 = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrativeaccess") ?? "Grant 'Simple Launcher' administrative access.";
        string alsochecktheintegrityoftheemulator2 = (string)Application.Current.TryFindResource("Alsochecktheintegrityoftheemulator") ?? "Also, check the integrity of the emulator and its dependencies.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{failedtostarttheemulator2}\n\n" +
                        $"{thistypeoferrorhappenswhenSimpleLauncher2}\n\n" +
                        $"{grantSimpleLauncheradministrativeaccess2}\n\n" +
                        $"{alsochecktheintegrityoftheemulator2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void ThereWasAnErrorLaunchingThisGameMessageBox(string logPath)
    {
        string therewasanerrorlaunchingthisgame2 = (string)Application.Current.TryFindResource("Therewasanerrorlaunchingthisgame") ?? "There was an error launching this game.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string doyouwanttoopenthefileerroruserlog2 = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show($"{therewasanerrorlaunchingthisgame2}\n\n" +
                                     $"{theerrorwasreportedtothedeveloper2}\n\n" +
                                     $"{doyouwanttoopenthefileerroruserlog2}",
            error2, MessageBoxButton.YesNo, MessageBoxImage.Error);
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
                string thefileerroruserlog2 = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruserlog2,
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
   
    internal static void CannotExtractThisFileMessageBox(string filePath)
    {
        string theselectedfile2 = (string)Application.Current.TryFindResource("Theselectedfile") ?? "The selected file";
        string cannotbeextracted2 = (string)Application.Current.TryFindResource("cannotbeextracted") ?? "can not be extracted.";
        string toextractafileitneedstobe2 = (string)Application.Current.TryFindResource("Toextractafileitneedstobe") ?? "To extract a file, it needs to be a 7z, zip, or rar file.";
        string pleasegotoEditSystem2 = (string)Application.Current.TryFindResource("PleasegotoEditSystem") ?? "Please go to Edit System - Expert Mode and edit this system.";
        string invalidFile2 = (string)Application.Current.TryFindResource("InvalidFile") ?? "Invalid File";
        MessageBox.Show($"{theselectedfile2} '{filePath}' {cannotbeextracted2}\n\n" +
                        $"{toextractafileitneedstobe2}\n\n" +
                        $"{pleasegotoEditSystem2}", 
            invalidFile2, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
    
    internal static void InvalidProgramLocationMessageBox()
    {
        string invalidemulatorexecutablepath2 = (string)Application.Current.TryFindResource("Invalidemulatorexecutablepath") ?? "Invalid emulator executable path. Please check the configuration.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show(invalidemulatorexecutablepath2, error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void EmulatorCouldNotOpenXboxXblaSimpleMessageBox(string logPath)
    {
        string theemulatorcouldnotopenthegame2 = (string)Application.Current.TryFindResource("Theemulatorcouldnotopenthegame") ?? "The emulator could not open the game with the provided parameters.";
        string doyouwanttoopenthefileerror2 = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = MessageBox.Show(
            $"{theemulatorcouldnotopenthegame2}\n\n" +
            $"{doyouwanttoopenthefileerror2}", error2, MessageBoxButton.YesNo, MessageBoxImage.Error);

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
                string thefileerroruser2 = (string)Application.Current.TryFindResource("Thefileerroruser") ?? "The file 'error_user.log' was not found!";
                MessageBox.Show(thefileerroruser2,
                    error2, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
    
    internal static void NullFileExtensionMessageBox()
    {
        string thereisnoExtension2 = (string)Application.Current.TryFindResource("ThereisnoExtension") ?? "There is no 'Extension to Launch After Extraction' set in the system configuration.";
        string pleaseeditthissystemto2 = (string)Application.Current.TryFindResource("Pleaseeditthissystemto") ?? "Please edit this system to fix that.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{thereisnoExtension2}\n\n" +
                        $"{pleaseeditthissystemto2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void CouldNotFindAFileMessageBox()
    {
        string couldnotfindafilewiththeextensiondefined2 = (string)Application.Current.TryFindResource("Couldnotfindafilewiththeextensiondefined") ?? "Could not find a file with the extension defined in 'Extension to Launch After Extraction' inside the extracted folder.";
        string pleaseeditthissystemtofix2 = (string)Application.Current.TryFindResource("Pleaseeditthissystemto") ?? "Please edit this system to fix that.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{couldnotfindafilewiththeextensiondefined2}\n\n" +
                        $"{pleaseeditthissystemtofix2}", error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

}