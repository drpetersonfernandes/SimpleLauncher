using System;
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
        MessageBox.Show("There was an error using the Global Search.\n\n" +
                        "The error was reported to the developer who will try to fix the issue.",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        MessageBox.Show("There was an error launching the selected game.\n\n" +
                        "The error was reported to the developer who will try to fix the issue.",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void SelectAGameToLaunchMessageBox()
    {
        MessageBox.Show("Please select a game to launch.",
            "Information", MessageBoxButton.OK, MessageBoxImage.Information);
    }
    
    internal static void ErrorRightClickContextMenuMessageBox()
    {
        MessageBox.Show("There was an error in the right-click context menu.\n\n" +
                        "The error was reported to the developer, who will try to fix the issue.",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
    
    internal static void ErrorLoadingSystemConfigMessageBox()
    {
        MessageBox.Show("There was an error loading the systemConfig.\n\n" +
                        "The error was reported to the developer who will try to fix the issue.",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        MessageBox.Show("There was a problem opening the Cover Image for this game.\n\n" +
                        "The error was reported to the developer who will try to fix the issue.",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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
        MessageBox.Show("'Simple Launcher' could not launch this game.\n\n" +
                        "The error was reported to the developer who will try to fix the issue.",
            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}