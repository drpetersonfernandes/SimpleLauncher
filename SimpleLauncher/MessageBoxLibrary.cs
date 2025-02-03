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

    internal static void CouldNotOpenWalkthroughMessageBox()
    {
        string failedtoopenthewalkthrough2 = (string)Application.Current.TryFindResource("Failedtoopenthewalkthrough") ?? "Failed to open the walkthrough.";
        string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        MessageBox.Show($"{failedtoopenthewalkthrough2}\n\n" +
                        $"{theerrorwasreportedtothedeveloper2}",
            error2, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    internal static void ThereIsNoWalkthroughMessageBox()
    {
        string thereisnowalkthrough2 = (string)Application.Current.TryFindResource("Thereisnowalkthrough") ?? "There is no walkthrough file associated with this game.";
        string walkthroughnotfound2 = (string)Application.Current.TryFindResource("Walkthroughnotfound") ?? "Walkthrough not found";
        MessageBox.Show(thereisnowalkthrough2, walkthroughnotfound2, MessageBoxButton.OK, MessageBoxImage.Information);
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
        string fileDeleted2 = (string)Application.Current.TryFindResource("FileDeleted") ?? "File Deleted";
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
        string nodefaultpngfilefoundintheimages2 = (string)Application.Current.TryFindResource("Nodefaultpngfilefoundintheimages") ?? "No 'default.png' file found in the images folder.";
        string doyouwanttoreinstallSimpleLauncher2 = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
        string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var reinstall = MessageBox.Show($"{nodefaultpngfilefoundintheimages2}\n\n" +
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
        MessageBox.Show($"{fileNameWithoutExtension} {isalreadyinfavorites2}", info2, MessageBoxButton.OK, MessageBoxImage.Information);
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
        MessageBox.Show(pleaseselectasystembeforesearching, systemNotSelected, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    internal static void EnterSearchQueryMessageBox()
    {
        string pleaseenterasearchquery = (string)Application.Current.TryFindResource("Pleaseenterasearchquery") ?? "Please enter a search query.";
        string searchQueryRequired = (string)Application.Current.TryFindResource("SearchQueryRequired") ?? "Search Query Required";
        MessageBox.Show(pleaseenterasearchquery, searchQueryRequired, MessageBoxButton.OK, MessageBoxImage.Warning);
    }
    
    internal static void ErrorWhileLoadingHelpUserXmlMessageBox()
    {
        var result = MessageBox.Show("Unexpected error while loading 'helpuser.xml'.\n\n" +
                                     "Do you want to automatic reinstall 'Simple Launcher' to fix it.",
            "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();   
        }
    }

    internal static void NoSystemInHelpUserXmlMessageBox()
    {
        var result = MessageBox.Show("No valid systems found in the file 'helpuser.xml'.\n\n" +
                                     "Do you want to automatic reinstall 'Simple Launcher' to fix it.",
            "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();   
        }
    }

    internal static bool CouldNotLoadHelpUserXmlMessageBox()
    {
        var result = MessageBox.Show("'Simple Launcher' could not load 'helpuser.xml'.\n\n" +
                                     "Do you want to automatic reinstall 'Simple Launcher' to fix it.",
            "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
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
        var result = MessageBox.Show("Unable to load 'helpuser.xml'. The file may be corrupted.\n\n" +
                                     "Do you want to automatic reinstall 'Simple Launcher' to fix it.",
            "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
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
        var result = MessageBox.Show("The file 'helpuser.xml' is missing.\n\n" +
                                     "Do you want to automatic reinstall 'Simple Launcher' to fix it.",
            "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
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
        MessageBox.Show("Failed to load the image in the Image Viewer window.\n\n" +
                        "The image may be corrupted or inaccessible." +
                        "The error was reported to the developer that will fix the issue.",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void ReinstallSimpleLauncherFileCorruptedMessageBox()
    {
        var result = MessageBox.Show("The application could not load the file 'mame.xml' or it is corrupted.\n\n" +
                                     "Do you want to automatic reinstall 'Simple Launcher' to fix it.",
            "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();   
        }
        else
        {
            MessageBox.Show("Please reinstall 'Simple Launcher' manually to fix the issue.\n\n" +
                            "The application will Shutdown",
                "Please Reinstall", MessageBoxButton.OK, MessageBoxImage.Error);
                    
            // Shutdown the application and exit
            Application.Current.Shutdown();
            Environment.Exit(0);    
        }
    }

    internal static void ReinstallSimpleLauncherFileMissingMessageBox()
    {
        var result = MessageBox.Show("The file 'mame.xml' could not be found in the application folder.\n\n" +
                                     "Do you want to automatic reinstall 'Simple Launcher' to fix it.",
            "File Missing", MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();   
        }
        else
        {
            MessageBox.Show("Please reinstall 'Simple Launcher' manually to fix the issue.\n\n" +
                            "The application will Shutdown",
                "Please Reinstall", MessageBoxButton.OK, MessageBoxImage.Error);
                    
            // Shutdown the application and exit
            Application.Current.Shutdown();
            Environment.Exit(0);    
        }
    }
    
    internal static void UpdaterNotFoundMessageBox()
    {
        MessageBox.Show("'Updater.exe' not found.\n\n" +
                        "Please reinstall 'Simple Launcher' manually to fix the problem.\n\n" +
                        "The application will now shut down.",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);

        // Shutdown the application and exit
        Application.Current.Shutdown();
        Environment.Exit(0);
    }
    
    internal static void ErrorLoadingRomHistoryMessageBox()
    {
        MessageBox.Show("An error occurred while loading ROM history.\n\n" +
                        "The error was reported to the developer who will try to fix the issue.",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void NoHistoryXmlFoundMessageBox()
    {
        var result = MessageBox.Show("No 'history.xml' file found in the application folder.\n\n" +
                                     "Do you want to reinstall 'Simple Launcher' to fix this issue?",
            "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
    }
    
    internal static void ErrorOpeningBrowserMessageBox()
    {
        MessageBox.Show("An error occurred while opening the browser.\n\n" +
                        "The error was reported to the developer who will try to fix the issue.",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void SimpleLauncherNeedMorePrivilegesMessageBox()
    {
        MessageBox.Show("'Simple Launcher' lacks sufficient privileges to write to the 'settings.xml' file.\n\n" +
                        "Please grant 'Simple Launcher' administrative access.\n\n" +
                        "Ensure that the 'Simple Launcher' folder is located in a writable directory.\n\n" +
                        "If necessary, temporarily disable your antivirus software.",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void SystemXmlIsCorruptedMessageBox()
    {
        MessageBox.Show("'system.xml' is corrupted or could not be opened.\n" +
                        "Please fix it manually or delete it.\n" +
                        "If you choose to delete it, 'Simple Launcher' will create a new one for you.\n\n" +
                        "If you want to debug the error yourself, check the 'error_user.log' file inside the 'Simple Launcher' folder.\n\n" +
                        "The application will shut down.",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    
        // Shutdown the application and exit
        Application.Current.Shutdown();
        Environment.Exit(0);
    }

    internal static void SystemModelXmlIsMissingMessageBox()
    {
        var messageBoxResult = MessageBox.Show("The file 'system_model.xml' is missing.\n\n" +
                                               "'Simple Launcher' cannot work properly without this file.\n\n" +
                                               "Do you want to automatically reinstall 'Simple Launcher' to fix the problem?",
            "Missing File", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (messageBoxResult == MessageBoxResult.Yes)
        {
            ReinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            MessageBox.Show("Please reinstall 'Simple Launcher' manually to fix the problem.\n\n" +
                            "The application will shut down.", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);

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
            Process.Start(new ProcessStartInfo
            {
                FileName = downloadPageUrl,
                UseShellExecute = true
            });
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
            Process.Start(new ProcessStartInfo
            {
                FileName = downloadPageUrl,
                UseShellExecute = true
            });
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
        MessageBox.Show(bugreportsent2, success2, MessageBoxButton.OK, MessageBoxImage.Information);
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

    internal static void DownloadCanceledMessageBox()
    {
        string downloadwascanceled2 = (string)Application.Current.TryFindResource("Downloadwascanceled") ?? "Download was canceled.";
        string downloadCanceled2 = (string)Application.Current.TryFindResource("DownloadCanceled") ?? "Download Canceled";
        MessageBox.Show(downloadwascanceled2,
            downloadCanceled2, MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void DownloadErrorOfferRedirectMessageBox(EasyModeSystemConfig selectedSystem)
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
                MessageBox.Show("'Simple Launcher' could not open the Image Pack download link.",
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
}