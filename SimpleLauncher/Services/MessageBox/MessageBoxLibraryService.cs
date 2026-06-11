using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.QuitOrReinstall;
using CoreMessageBoxResult = SimpleLauncher.Interfaces.MessageBoxResult;
using CoreMessageBoxButton = SimpleLauncher.Interfaces.MessageBoxButton;
using CoreMessageBoxImage = SimpleLauncher.Interfaces.MessageBoxImage;

namespace SimpleLauncher.Services.MessageBox;

public class MessageBoxLibraryService : IMessageBoxLibraryService
{
    private readonly IMessageDialogService _messageDialog;
    private readonly ReinstallSimpleLauncher _reinstallSimpleLauncher;
    private readonly QuitSimpleLauncher _quitSimpleLauncher;

    public MessageBoxLibraryService(IMessageDialogService messageDialog, ReinstallSimpleLauncher reinstallSimpleLauncher, QuitSimpleLauncher quitSimpleLauncher)
    {
        _messageDialog = messageDialog;
        _reinstallSimpleLauncher = reinstallSimpleLauncher;
        _quitSimpleLauncher = quitSimpleLauncher;
    }

    public Task TakeScreenShotMessageBox()
    {
        var thegamewilllaunchnow = (string)Application.Current.TryFindResource("Thegamewilllaunchnow") ?? "The game will launch now.";
        var setthegamewindowto = (string)Application.Current.TryFindResource("Setthegamewindowto") ?? "Set the game window to non-fullscreen. This is important.";
        var youshouldchangetheemulatorparameters = (string)Application.Current.TryFindResource("Youshouldchangetheemulatorparameters") ?? "You should change the emulator parameters to prevent the emulator from starting in fullscreen.";
        var aselectionwindowwillopeninSimpleLauncherallowingyou = (string)Application.Current.TryFindResource("AselectionwindowwillopeninSimpleLauncherallowingyou") ?? "A selection window will open in 'Simple Launcher', allowing you to choose the desired window to capture.";
        var assoonasyouselectawindow = (string)Application.Current.TryFindResource("assoonasyouselectawindow") ?? "As soon as you select a window, a screenshot will be taken and saved in the image folder of the selected system.";
        var takeScreenshot = (string)Application.Current.TryFindResource("TakeScreenshot") ?? "Take Screenshot";

        return _messageDialog.ShowInfoAsync($"{thegamewilllaunchnow}\n\n" +
                                            $"{setthegamewindowto}\n\n" +
                                            $"{youshouldchangetheemulatorparameters}\n\n" +
                                            $"{aselectionwindowwillopeninSimpleLauncherallowingyou}\n\n" +
                                            $"{assoonasyouselectawindow}", takeScreenshot);
    }

    public Task CouldNotSaveScreenshotMessageBox()
    {
        var failedtosavescreenshot = (string)Application.Current.TryFindResource("Failedtosavescreenshot") ?? "Failed to save screenshot.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        return _messageDialog.ShowErrorAsync($"{failedtosavescreenshot}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task GameIsAlreadyInFavoritesMessageBox(string fileNameWithExtension)
    {
        var isalreadyinfavorites = (string)Application.Current.TryFindResource("isalreadyinfavorites") ?? "is already in favorites.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";

        return _messageDialog.ShowInfoAsync($"{fileNameWithExtension} {isalreadyinfavorites}", info);
    }

    public Task ErrorWhileAddingFavoritesMessageBox()
    {
        var anerroroccurredwhileaddingthisgame = (string)Application.Current.TryFindResource("Anerroroccurredwhileaddingthisgame") ?? "An error occurred while adding this game to the favorites.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        return _messageDialog.ShowErrorAsync($"{anerroroccurredwhileaddingthisgame}\n\n" + $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task ErrorWhileRemovingGameFromFavoriteMessageBox()
    {
        var anerroroccurredwhileremoving = (string)Application.Current.TryFindResource("Anerroroccurredwhileremoving") ?? "An error occurred while removing this game from favorites.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        return _messageDialog.ShowErrorAsync($"{anerroroccurredwhileremoving}\n\n" + $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task ErrorOpeningTheUpdateHistoryWindowMessageBox()
    {
        var erroropeningtheUpdateHistorywindow = (string)Application.Current.TryFindResource("ErroropeningtheUpdateHistorywindow") ?? "Error opening the Update History window.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        return _messageDialog.ShowErrorAsync($"{erroropeningtheUpdateHistorywindow}\n\n" + $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task ErrorOpeningVideoLinkMessageBox()
    {
        var therewasaproblemopeningtheVideo = (string)Application.Current.TryFindResource("TherewasaproblemopeningtheVideo") ?? "There was a problem opening the Video Link.";
        var ensureyouhaveadefaultbrowserinstalled = (string)Application.Current.TryFindResource("Ensureyouhaveadefaultbrowserinstalled") ?? "Ensure you have a default browser installed and configured correctly on your system.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        return _messageDialog.ShowErrorAsync($"{therewasaproblemopeningtheVideo}\n\n" +
                                             $"{ensureyouhaveadefaultbrowserinstalled}", error);
    }

    public Task ProblemOpeningInfoLinkMessageBox()
    {
        var therewasaproblemopeningthe = (string)Application.Current.TryFindResource("Therewasaproblemopeningthe") ?? "There was a problem opening the Info Link.";
        var ensureyouhaveadefaultbrowserinstalled = (string)Application.Current.TryFindResource("Ensureyouhaveadefaultbrowserinstalled") ?? "Ensure you have a default browser installed and configured correctly on your system.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        return _messageDialog.ShowErrorAsync($"{therewasaproblemopeningthe}\n\n" +
                                             $"{ensureyouhaveadefaultbrowserinstalled}", error);
    }

    public Task ErrorOpeningUrlMessageBox()
    {
        var therewasaproblemopeningtheUrl = (string)Application.Current.TryFindResource("TherewasaproblemopeningtheUrl") ?? "There was a problem opening the Url.";
        var ensureyouhaveadefaultbrowserinstalled = (string)Application.Current.TryFindResource("Ensureyouhaveadefaultbrowserinstalled") ?? "Ensure you have a default browser installed and configured correctly on your system.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        return _messageDialog.ShowErrorAsync($"{therewasaproblemopeningtheUrl}\n\n" +
                                             $"{ensureyouhaveadefaultbrowserinstalled}", error);
    }

    public Task ThereIsNoCoverMessageBox()
    {
        var thereisnocoverfileassociated = (string)Application.Current.TryFindResource("Thereisnocoverfileassociated") ?? "There is no cover file associated with this game.";
        var covernotfound = (string)Application.Current.TryFindResource("Covernotfound") ?? "Cover not found";

        return _messageDialog.ShowInfoAsync(thereisnocoverfileassociated, covernotfound);
    }

    public Task ThereIsNoTitleSnapshotMessageBox()
    {
        var thereisnotitlesnapshot = (string)Application.Current.TryFindResource("Thereisnotitlesnapshot") ?? "There is no title snapshot file associated with this game.";
        var titleSnapshotnotfound = (string)Application.Current.TryFindResource("TitleSnapshotnotfound") ?? "Title Snapshot not found";

        return _messageDialog.ShowInfoAsync(thereisnotitlesnapshot, titleSnapshotnotfound);
    }

    public Task ThereIsNoGameplaySnapshotMessageBox()
    {
        var thereisnogameplaysnapshot = (string)Application.Current.TryFindResource("Thereisnogameplaysnapshot") ?? "There is no gameplay snapshot file associated with this game.";
        var gameplaySnapshotnotfound = (string)Application.Current.TryFindResource("GameplaySnapshotnotfound") ?? "Gameplay Snapshot not found";

        return _messageDialog.ShowInfoAsync(thereisnogameplaysnapshot, gameplaySnapshotnotfound);
    }

    public Task ThereIsNoCartMessageBox()
    {
        var thereisnocartfile = (string)Application.Current.TryFindResource("Thereisnocartfile") ?? "There is no cart file associated with this game.";
        var cartnotfound = (string)Application.Current.TryFindResource("Cartnotfound") ?? "Cart not found";

        return _messageDialog.ShowInfoAsync(thereisnocartfile, cartnotfound);
    }

    public Task ThereIsNoVideoFileMessageBox()
    {
        var thereisnovideofile = (string)Application.Current.TryFindResource("Thereisnovideofile") ?? "There is no video file associated with this game.";
        var videonotfound = (string)Application.Current.TryFindResource("Videonotfound") ?? "Video not found";

        return _messageDialog.ShowInfoAsync(thereisnovideofile, videonotfound);
    }

    public Task CouldNotOpenManualMessageBox()
    {
        var failedtoopenthemanual = (string)Application.Current.TryFindResource("Failedtoopenthemanual") ?? "Failed to open the manual.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        return _messageDialog.ShowErrorAsync($"{failedtoopenthemanual}\n\n" + $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task NoPdfViewerInstalledMessageBox()
    {
        var nopdfviewerinstalled = (string)Application.Current.TryFindResource("NoPDFViewerInstalled") ?? "No PDF viewer is installed on your system.";
        var pleaseinstallapdfviewer = (string)Application.Current.TryFindResource("PleaseInstallAPDFViewer") ?? "Please install a PDF viewer (such as Adobe Acrobat Reader, Sumatra PDF, or Microsoft Edge) to open this file.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        return _messageDialog.ShowErrorAsync($"{nopdfviewerinstalled}\n\n{pleaseinstallapdfviewer}", error);
    }

    public Task ThereIsNoManualMessageBox()
    {
        var thereisnomanual = (string)Application.Current.TryFindResource("Thereisnomanual") ?? "There is no manual associated with this file.";
        var manualNotFound = (string)Application.Current.TryFindResource("Manualnotfound") ?? "Manual not found";

        return _messageDialog.ShowInfoAsync(thereisnomanual, manualNotFound);
    }

    public Task ThereIsNoWalkthroughMessageBox()
    {
        var thereisnowalkthrough = (string)Application.Current.TryFindResource("Thereisnowalkthrough") ?? "There is no walkthrough file associated with this game.";
        var walkthroughnotfound = (string)Application.Current.TryFindResource("Walkthroughnotfound") ?? "Walkthrough not found";

        return _messageDialog.ShowInfoAsync(thereisnowalkthrough, walkthroughnotfound);
    }

    public Task ThereIsNoCabinetMessageBox()
    {
        var thereisnocabinetfile = (string)Application.Current.TryFindResource("Thereisnocabinetfile") ?? "There is no cabinet file associated with this game.";
        var cabinetnotfound = (string)Application.Current.TryFindResource("Cabinetnotfound") ?? "Cabinet not found";

        return _messageDialog.ShowInfoAsync(thereisnocabinetfile, cabinetnotfound);
    }

    public Task ThereIsNoFlyerMessageBox()
    {
        var thereisnoflyer = (string)Application.Current.TryFindResource("Thereisnoflyer") ?? "There is no flyer file associated with this game.";
        var flyernotfound = (string)Application.Current.TryFindResource("Flyernotfound") ?? "Flyer not found";

        return _messageDialog.ShowInfoAsync(thereisnoflyer, flyernotfound);
    }

    public Task ThereIsNoPcbMessageBox()
    {
        var thereisnoPcBfile = (string)Application.Current.TryFindResource("ThereisnoPCBfile") ?? "There is no PCB file associated with this game.";
        var pCBnotfound = (string)Application.Current.TryFindResource("PCBnotfound") ?? "PCB not found";

        return _messageDialog.ShowInfoAsync(thereisnoPcBfile, pCBnotfound);
    }

    public Task FileSuccessfullyDeletedMessageBox(string fileNameWithExtension)
    {
        var thefile = (string)Application.Current.TryFindResource("Thefile") ?? "The file";
        var hasbeensuccessfullydeleted = (string)Application.Current.TryFindResource("hasbeensuccessfullydeleted") ?? "has been successfully deleted.";
        var fileDeleted = (string)Application.Current.TryFindResource("Filedeleted") ?? "File deleted";

        return _messageDialog.ShowInfoAsync($"{thefile} '{fileNameWithExtension}' {hasbeensuccessfullydeleted}", fileDeleted);
    }

    public Task FileCouldNotBeDeletedMessageBox(string fileNameWithExtension)
    {
        var anerroroccurredwhiletryingtodelete = (string)Application.Current.TryFindResource("Anerroroccurredwhiletryingtodelete") ?? "An error occurred while trying to delete the file";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        return _messageDialog.ShowErrorAsync($"{anerroroccurredwhiletryingtodelete} '{fileNameWithExtension}'.\n\n" + $"{theerrorwasreportedtothedeveloper}", error);
    }

    public async Task DefaultImageNotFoundMessageBox()
    {
        var defaultpngfileismissing = (string)Application.Current.TryFindResource("defaultpngfileismissing") ?? "'default.png' file is missing.";
        var doyouwanttoreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var reinstall = await _messageDialog.ShowYesNoAsync($"{defaultpngfileismissing}\n\n" + $"{doyouwanttoreinstallSimpleLauncher}", error);

        if (reinstall)
        {
            _reinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            var pleasereinstallSimpleLauncher = (string)Application.Current.TryFindResource("PleasereinstallSimpleLauncher") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";

            await _messageDialog.ShowErrorAsync(pleasereinstallSimpleLauncher, error);
        }
    }

    public Task GlobalSearchErrorMessageBox()
    {
        var therewasanerrorusingtheGlobal = (string)Application.Current.TryFindResource("TherewasanerrorusingtheGlobal") ?? "There was an error using the Global Search.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        return _messageDialog.ShowErrorAsync($"{therewasanerrorusingtheGlobal}\n\n" + $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task PleaseEnterSearchTermMessageBox()
    {
        var pleaseenterasearchterm = (string)Application.Current.TryFindResource("Pleaseenterasearchterm") ?? "Please enter a search term.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";

        return _messageDialog.ShowWarningAsync(pleaseenterasearchterm, warning);
    }

    public async Task ErrorLaunchingGameMessageBox(string logPath)
    {
        var therewasanerrorlaunchingtheselected = (string)Application.Current.TryFindResource("Therewasanerrorlaunchingtheselected") ?? "There was an error launching the selected game.";
        var dowanttoopenthefileerroruserlog = (string)Application.Current.TryFindResource("Dowanttoopenthefileerroruserlog") ?? "Do want to open the file 'error_user.log' to debug the error?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{therewasanerrorlaunchingtheselected}\n\n" + $"{dowanttoopenthefileerroruserlog}", error);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Failed to open the error log file from a message box.");
                var thefileerroruserlogwasnotfound = (string)Application.Current.TryFindResource("Thefileerroruserlogwasnotfound") ?? "The file 'error_user.log' was not found!";

                await _messageDialog.ShowErrorAsync(thefileerroruserlogwasnotfound, error);
            }
        }
    }

    public Task SelectAGameToLaunchMessageBox()
    {
        var pleaseselectagametolaunch = (string)Application.Current.TryFindResource("Pleaseselectagametolaunch") ?? "Please select a game to launch.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync(pleaseselectagametolaunch, info);
    }

    public Task FileAddedToFavoritesMessageBox(string fileNameWithoutExtension)
    {
        var hasbeenaddedtofavorites = (string)Application.Current.TryFindResource("hasbeenaddedtofavorites") ?? "has been added to favorites.";
        var success = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync($"{fileNameWithoutExtension} {hasbeenaddedtofavorites}", success);
    }

    public Task FileRemovedFromFavoritesMessageBox(string fileNameWithoutExtension)
    {
        var wasremovedfromfavorites = (string)Application.Current.TryFindResource("wasremovedfromfavorites") ?? "was removed from favorites.";
        var success = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync($"{fileNameWithoutExtension} {wasremovedfromfavorites}", success);
    }

    public async Task CouldNotLaunchThisGameMessageBox(string logPath)
    {
        var simpleLaunchercouldnotlaunchthisgame = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotlaunchthisgame") ?? "'Simple Launcher' could not launch this game.";
        var dowanttoopenthefileerroruserlog = (string)Application.Current.TryFindResource("Dowanttoopenthefileerroruserlog") ?? "Do want to open the file 'error_user.log' to debug the error?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = await _messageDialog.ShowYesNoAsync($"{simpleLaunchercouldnotlaunchthisgame}\n\n" +
                                                         $"{dowanttoopenthefileerroruserlog}", error);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Failed to open the error log file from a message box.");
                var thefileerroruserlogwasnotfound = (string)Application.Current.TryFindResource("Thefileerroruserlogwasnotfound") ?? "The file 'error_user.log' was not found!";
                await _messageDialog.ShowErrorAsync(thefileerroruserlogwasnotfound, error);
            }
        }
    }

    public Task ProtocolHandlerNotRegisteredMessageBox(string protocol)
    {
        var protocolHandlerNotRegistered = (string)Application.Current.TryFindResource("ProtocolHandlerNotRegistered") ?? "Protocol handler for '{0}://' is not registered. Please ensure the associated application is installed.";
        var launchErrorTitle = (string)Application.Current.TryFindResource("LaunchErrorTitle") ?? "Launch Error";

        return _messageDialog.ShowWarningAsync(string.Format(CultureInfo.InvariantCulture, protocolHandlerNotRegistered, protocol), launchErrorTitle);
    }

    public Task EmulatorPathNotConfiguredMessageBox()
    {
        var emulatorPathNotConfigured = (string)Application.Current.TryFindResource("EmulatorPathNotConfigured") ?? "The emulator path is not configured.";
        var emulatorPathNotConfiguredDetails1 = (string)Application.Current.TryFindResource("EmulatorPathNotConfiguredDetails1") ?? "The emulator you are using does not have a valid executable path configured.";
        var emulatorPathNotConfiguredDetails2 = (string)Application.Current.TryFindResource("EmulatorPathNotConfiguredDetails2") ?? "This typically happens when:";
        var emulatorPathNotConfiguredDetails3 = (string)Application.Current.TryFindResource("EmulatorPathNotConfiguredDetails3") ?? "- The system was configured to run directly executable files (.bat, .exe, .lnk)";
        var emulatorPathNotConfiguredDetails4 = (string)Application.Current.TryFindResource("EmulatorPathNotConfiguredDetails4") ?? "- But you are trying to launch a file that requires an emulator";
        var emulatorPathNotConfiguredDetails5 = (string)Application.Current.TryFindResource("EmulatorPathNotConfiguredDetails5") ?? "Please edit the system configuration and provide a valid emulator path.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowWarningAsync($"{emulatorPathNotConfigured}\n" +
                                               $"{emulatorPathNotConfiguredDetails1}\n\n" +
                                               $"{emulatorPathNotConfiguredDetails2}\n" +
                                               $"{emulatorPathNotConfiguredDetails3}\n" +
                                               $"{emulatorPathNotConfiguredDetails4}\n\n" +
                                               $"{emulatorPathNotConfiguredDetails5}", error);
    }

    public Task ErrorCalculatingStatsMessageBox()
    {
        var anerroroccurredwhilecalculatingtheGlobal = (string)Application.Current.TryFindResource("AnerroroccurredwhilecalculatingtheGlobal") ?? "An error occurred while calculating the Global Statistics.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        return _messageDialog.ShowErrorAsync($"{anerroroccurredwhilecalculatingtheGlobal}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task FailedSaveReportMessageBox()
    {
        var failedtosavethereport = (string)Application.Current.TryFindResource("Failedtosavethereport") ?? "Failed to save the report.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        return _messageDialog.ShowErrorAsync($"{failedtosavethereport}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task ReportSavedMessageBox()
    {
        var reportsavedsuccessfully = (string)Application.Current.TryFindResource("Reportsavedsuccessfully") ?? "Report saved successfully.";
        var success = (string)Application.Current.TryFindResource("Success") ?? "Success";

        return _messageDialog.ShowInfoAsync(reportsavedsuccessfully, success);
    }

    public Task NoStatsToSaveMessageBox()
    {
        var nostatisticsavailabletosave = (string)Application.Current.TryFindResource("Nostatisticsavailabletosave") ?? "No statistics available to save.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        return _messageDialog.ShowWarningAsync(nostatisticsavailabletosave, error);
    }

    public async Task ErrorLaunchingToolMessageBox(string logPath)
    {
        var anerroroccurredwhilelaunchingtheselectedtool = (string)Application.Current.TryFindResource("Anerroroccurredwhilelaunchingtheselectedtool") ?? "An error occurred while launching the selected tool.";
        var grantSimpleLauncheradministrative = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var temporarilydisableyourantivirussoftware = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var dowanttoopenthefileerroruserlog = (string)Application.Current.TryFindResource("Dowanttoopenthefileerroruserlog") ?? "Do want to open the file 'error_user.log' to debug the error?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = await _messageDialog.ShowYesNoAsync($"{anerroroccurredwhilelaunchingtheselectedtool}\n\n" +
                                                         $"{grantSimpleLauncheradministrative}\n\n" +
                                                         $"{temporarilydisableyourantivirussoftware}\n\n" +
                                                         $"{dowanttoopenthefileerroruserlog}", error);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Failed to open the error log file from a message box.");
                var thefileerroruserlogwasnotfound = (string)Application.Current.TryFindResource("Thefileerroruserlogwasnotfound") ?? "The file 'error_user.log' was not found!";
                await _messageDialog.ShowErrorAsync(thefileerroruserlogwasnotfound, error);
            }
        }
    }

    public async Task SelectedToolNotFoundMessageBox()
    {
        var theselectedtoolwasnotfound = (string)Application.Current.TryFindResource("Theselectedtoolwasnotfound") ?? "The selected tool was not found in the expected path.";
        var doyouwanttoreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var reinstall = await _messageDialog.ShowYesNoAsync($"{theselectedtoolwasnotfound}\n\n" +
                                                            $"{doyouwanttoreinstallSimpleLauncher}", error);

        if (reinstall)
        {
            _reinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            var pleasereinstallSimpleLaunchermanually = (string)Application.Current.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            await _messageDialog.ShowErrorAsync(pleasereinstallSimpleLaunchermanually, error);
        }
    }

    public Task ErrorMessageBox()
    {
        var therewasanerror = (string)Application.Current.TryFindResource("Therewasanerror") ?? "There was an error.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{therewasanerror}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task NoFavoriteFoundMessageBox()
    {
        var thereisnoFavoriteforthissystem = (string)Application.Current.TryFindResource("ThereisnoFavoriteforthissystem") ?? "There is no Favorite for this system, or you have not chosen a system.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowInfoAsync(thereisnoFavoriteforthissystem, warning);
    }

    public Task MoveToWritableFolderMessageBox()
    {
        var itlookslikeSimpleLauncherisinstalled = (string)Application.Current.TryFindResource("ItlookslikeSimpleLauncherisinstalled") ?? "It looks like 'Simple Launcher' is installed in a restricted folder (e.g., Program Files), where it does not have write access.";
        var itneedswriteaccesstoitsfolder = (string)Application.Current.TryFindResource("Itneedswriteaccesstoitsfolder") ?? "It needs write access to its folder.";
        var pleasemovetheapplicationfolder = (string)Application.Current.TryFindResource("Pleasemovetheapplicationfolder") ?? "Please move the application folder to a writable location like the 'Documents' folder.";
        var ifpossiblerunitwithadministrative = (string)Application.Current.TryFindResource("Ifpossiblerunitwithadministrative") ?? "If possible, run it with administrative privileges.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowWarningAsync($"{itlookslikeSimpleLauncherisinstalled}\n\n" +
                                               $"{itneedswriteaccesstoitsfolder}\n\n" +
                                               $"{pleasemovetheapplicationfolder}\n\n" +
                                               $"{ifpossiblerunitwithadministrative}", warning);
    }

    public Task InvalidSystemConfigMessageBox()
    {
        var therewasanerrorwhileloading = (string)Application.Current.TryFindResource("Therewasanerrorwhileloading") ?? "There was an error while loading the system configuration for this system.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{therewasanerrorwhileloading}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task ErrorMethodLoadGameFilesAsyncMessageBox()
    {
        var therewasanerrorloadingthegame = (string)Application.Current.TryFindResource("Therewasanerrorloadingthegame") ?? "There was an error loading the game list.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{therewasanerrorloadingthegame}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task ErrorOpeningDonationLinkMessageBox()
    {
        var therewasanerroropeningthedonation = (string)Application.Current.TryFindResource("Therewasanerroropeningthedonation") ?? "There was an error opening the Donation Link.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{therewasanerroropeningthedonation}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task ToggleGamepadFailureMessageBox()
    {
        var failedtotogglegamepad = (string)Application.Current.TryFindResource("Failedtotogglegamepad") ?? "Failed to toggle gamepad.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{failedtotogglegamepad}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task ToolLaunchWasCanceledByUserMessageBox()
    {
        var thelaunchoftheselectedtoolwascanceledbytheuser = (string)Application.Current.TryFindResource("thelaunchoftheselectedtoolwascanceledbytheuser") ?? "The launch of the selected tool was canceled by the user.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync(thelaunchoftheselectedtoolwascanceledbytheuser, info);
    }

    public Task ErrorChangingViewModeMessageBox()
    {
        var therewasanerrorwhilechangingtheviewmode = (string)Application.Current.TryFindResource("Therewasanerrorwhilechangingtheviewmode") ?? "There was an error while changing the view mode.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{therewasanerrorwhilechangingtheviewmode}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task NavigationButtonErrorMessageBox()
    {
        var therewasanerrorinthenavigationbutton = (string)Application.Current.TryFindResource("Therewasanerrorinthenavigationbutton") ?? "There was an error in the navigation button.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{therewasanerrorinthenavigationbutton}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task SelectSystemBeforeSearchMessageBox()
    {
        var pleaseselectasystembeforesearching = (string)Application.Current.TryFindResource("Pleaseselectasystembeforesearching") ?? "Please select a system before searching.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowWarningAsync(pleaseselectasystembeforesearching, warning);
    }

    public Task EnterSearchQueryMessageBox()
    {
        var pleaseenterasearchquery = (string)Application.Current.TryFindResource("Pleaseenterasearchquery") ?? "Please enter a search query.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowWarningAsync(pleaseenterasearchquery, warning);
    }

    public async Task ErrorWhileLoadingHelpUserXmlMessageBox()
    {
        var unexpectederrorwhileloadinghelpuserxml = (string)Application.Current.TryFindResource("Unexpectederrorwhileloadinghelpuserxml") ?? "Unexpected error while loading 'helpuser.xml'.";
        var doyouwanttoautomaticreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{unexpectederrorwhileloadinghelpuserxml}\n\n" +
                                                         $"{doyouwanttoautomaticreinstallSimpleLauncher}", error);

        if (result)
        {
            _reinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
    }

    public async Task NoSystemInHelpUserXmlMessageBox()
    {
        var novalidsystemsfoundinthefilehelpuserxml = (string)Application.Current.TryFindResource("Novalidsystemsfoundinthefilehelpuserxml") ?? "No valid systems found in the file 'helpuser.xml'.";
        var doyouwanttoautomaticreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{novalidsystemsfoundinthefilehelpuserxml}\n\n" +
                                                         $"{doyouwanttoautomaticreinstallSimpleLauncher}", error);

        if (result)
        {
            _reinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
    }

    public Task<CoreMessageBoxResult> CouldNotLoadHelpUserXmlMessageBox()
    {
        return Task.FromResult(CoreMessageBoxResult.No);
    }

    public async Task FailedToLoadHelpUserXmlMessageBox()
    {
        var unabletoloadhelpuserxml = (string)Application.Current.TryFindResource("Unabletoloadhelpuserxml") ?? "Unable to load 'helpuser.xml'. The file may be corrupted.";
        var doyouwanttoautomaticreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{unabletoloadhelpuserxml}\n\n" +
                                                         $"{doyouwanttoautomaticreinstallSimpleLauncher}", error);

        if (result)
        {
            _reinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
    }

    public async Task FileHelpUserXmlIsMissingMessageBox()
    {
        var thefilehelpuserxmlismissing = (string)Application.Current.TryFindResource("Thefilehelpuserxmlismissing") ?? "The file 'helpuser.xml' is missing.";
        var doyouwanttoautomaticreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{thefilehelpuserxmlismissing}\n\n" +
                                                         $"{doyouwanttoautomaticreinstallSimpleLauncher}", error);

        if (result)
        {
            _reinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
    }

    public async Task ErrorWhileLoadingParametersMdMessageBox()
    {
        var unexpectederrorwhileloadingparametersmd = (string)Application.Current.TryFindResource("Unexpectederrorwhileloadingparametersmd") ?? "Unexpected error while loading 'parameters.md'.";
        var doyouwanttoautomaticreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{unexpectederrorwhileloadingparametersmd}\n\n" +
                                                         $"{doyouwanttoautomaticreinstallSimpleLauncher}", error);

        if (result)
        {
            _reinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
    }

    public async Task NoSystemInParametersMdMessageBox()
    {
        var novalidsystemsfoundinthefileparametersmd = (string)Application.Current.TryFindResource("Novalidsystemsfoundinthefileparametersmd") ?? "No valid systems found in the file 'parameters.md'.";
        var doyouwanttoautomaticreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{novalidsystemsfoundinthefileparametersmd}\n\n" +
                                                         $"{doyouwanttoautomaticreinstallSimpleLauncher}", error);

        if (result)
        {
            _reinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
    }

    public async Task FailedToLoadParametersMdMessageBox()
    {
        var unabletoloadparametersmd = (string)Application.Current.TryFindResource("Unabletoloadparametersmd") ?? "Unable to load 'parameters.md'. The file may be corrupted or in use.";
        var doyouwanttoautomaticreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{unabletoloadparametersmd}\n\n" +
                                                         $"{doyouwanttoautomaticreinstallSimpleLauncher}", error);

        if (result)
        {
            _reinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
    }

    public async Task FileParametersMdIsMissingMessageBox()
    {
        var thefileparametersmdismissing = (string)Application.Current.TryFindResource("Thefileparametersmdismissing") ?? "The file 'parameters.md' is missing.";
        var doyouwanttoautomaticreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{thefileparametersmdismissing}\n\n" +
                                                         $"{doyouwanttoautomaticreinstallSimpleLauncher}", error);

        if (result)
        {
            _reinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
    }

    public async Task FileParametersMdIsEmptyMessageBox()
    {
        var thefileparametersmdisempty = (string)Application.Current.TryFindResource("Thefileparametersmdisempty") ?? "The file 'parameters.md' is empty.";
        var doyouwanttoautomaticreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{thefileparametersmdisempty}\n\n" +
                                                         $"{doyouwanttoautomaticreinstallSimpleLauncher}", error);

        if (result)
        {
            _reinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
    }

    public Task ImageViewerErrorMessageBox()
    {
        var failedtoloadtheimageintheImage = (string)Application.Current.TryFindResource("FailedtoloadtheimageintheImage") ?? "Failed to load the image in the Image Viewer window.";
        var theimagemaybecorruptedorinaccessible = (string)Application.Current.TryFindResource("Theimagemaybecorruptedorinaccessible") ?? "The image may be corrupted or inaccessible.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{failedtoloadtheimageintheImage}\n\n" +
                                             $"{theimagemaybecorruptedorinaccessible}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public async Task ReinstallSimpleLauncherFileCorruptedMessageBox()
    {
        if (Application.Current == null)
        {
        }

        {
            var simpleLaunchercouldnotloadthefilemamedat = (string)Application.Current?.TryFindResource("SimpleLaunchercouldnotloadthefilemamedat") ?? "'Simple Launcher' could not load the file 'mame.dat' or it is corrupted.";
            var doyouwanttoautomaticreinstallSimpleLauncher = (string)Application.Current?.TryFindResource("DoyouwanttoautomaticreinstallSimpleLauncher") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
            var error = (string)Application.Current?.TryFindResource("Error") ?? "Error";

            var result = await _messageDialog.ShowYesNoAsync($"{simpleLaunchercouldnotloadthefilemamedat}\n\n" +
                                                             $"{doyouwanttoautomaticreinstallSimpleLauncher}", error);

            if (result)
            {
                _reinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
            else
            {
                var pleasereinstallSimpleLaunchermanually = (string)Application.Current?.TryFindResource("PleasereinstallSimpleLaunchermanually") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
                var theapplicationwillshutdown = (string)Application.Current?.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
                await _messageDialog.ShowErrorAsync($"{pleasereinstallSimpleLaunchermanually}\n\n" +
                                                    $"{theapplicationwillshutdown}", error);

                _quitSimpleLauncher.SimpleQuitApplication();
            }
        }
    }

    public async Task ReinstallSimpleLauncherFileMissingMessageBox()
    {
        if (Application.Current == null)
        {
        }

        {
            var thefilemamedatcouldnotbefound = (string)Application.Current?.TryFindResource("Thefilemamedatcouldnotbefound") ?? "The file 'mame.dat' could not be found in the application folder.";
            var doyouwanttoautomaticreinstall = (string)Application.Current?.TryFindResource("Doyouwanttoautomaticreinstall") ?? "Do you want to automatic reinstall 'Simple Launcher' to fix it.";
            var error = (string)Application.Current?.TryFindResource("Error") ?? "Error";

            var result = await _messageDialog.ShowYesNoAsync($"{thefilemamedatcouldnotbefound}\n\n"
                                                             + $"{doyouwanttoautomaticreinstall}", error);

            if (result)
            {
                _reinstallSimpleLauncher.StartUpdaterAndShutdown();
            }
        }
    }

    public Task ErrorCheckingForUpdatesMessageBox()
    {
        var anerroroccurredwhilecheckingforupdates = (string)Application.Current.TryFindResource("Anerroroccurredwhilecheckingforupdates") ?? "An error occurred while checking for updates.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{anerroroccurredwhilecheckingforupdates}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task ErrorLoadingRomHistoryMessageBox()
    {
        var anerroroccurredwhileloadingRoMhistory = (string)Application.Current.TryFindResource("AnerroroccurredwhileloadingROMhistory") ?? "An error occurred while loading ROM history.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        return _messageDialog.ShowErrorAsync($"{anerroroccurredwhileloadingRoMhistory}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public async Task NoHistoryXmlOrDatFoundMessageBox()
    {
        var nohistoryxmlfilefound = (string)Application.Current.TryFindResource("Nohistoryxmlfilefound2") ?? "No 'history.dat' or 'history.xml' file found in the application folder.";
        var doyouwanttoreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{nohistoryxmlfilefound}\n\n" +
                                                         $"{doyouwanttoreinstallSimpleLauncher}", error);

        if (result)
        {
            _reinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
    }

    public Task ErrorOpeningBrowserMessageBox()
    {
        var anerroroccurredwhileopeningthebrowser = (string)Application.Current.TryFindResource("Anerroroccurredwhileopeningthebrowser") ?? "An error occurred while opening the browser.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        return _messageDialog.ShowErrorAsync($"{anerroroccurredwhileopeningthebrowser}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public async Task SystemXmlIsCorruptedMessageBox(string logPath)
    {
        var systemxmliscorrupted = (string)Application.Current.TryFindResource("systemxmliscorrupted") ?? "'system.xml' is corrupted or could not be opened.";
        var pleasefixitmanuallyordeleteit = (string)Application.Current.TryFindResource("Pleasefixitmanuallyordeleteit") ?? "Please fix it manually or delete it.";
        var ifyouchoosetodeleteit = (string)Application.Current.TryFindResource("Ifyouchoosetodeleteit") ?? "If you choose to delete it, 'Simple Launcher' will create a new one for you.";
        var theapplicationwillshutdown = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
        var wouldyouliketoopentheerroruserlog = (string)Application.Current.TryFindResource("Wouldyouliketoopentheerroruserlog") ?? "Would you like to open the 'error_user.log' file to investigate the issue?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{systemxmliscorrupted} {pleasefixitmanuallyordeleteit}\n\n" +
                                                         $"{ifyouchoosetodeleteit}\n\n" +
                                                         $"{theapplicationwillshutdown}" +
                                                         $"{wouldyouliketoopentheerroruserlog}", error);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Failed to open the error log file from a message box.");
                var thefileerroruserlog = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
                await _messageDialog.ShowErrorAsync(thefileerroruserlog, error);
            }
        }

        _quitSimpleLauncher.SimpleQuitApplication();
    }

    public async Task WouldYouLikeToOpenTheLogMessageBox(string logPath)
    {
        var simpleLauncherWasUnableToLaunchThisGame = (string)Application.Current.TryFindResource("SimpleLauncherWasUnableToLaunchThisGame") ?? "'Simple Launcher' was unable to launch this game.";
        var wouldyouliketoopentheerroruserlogfiletodebug = (string)Application.Current.TryFindResource("Wouldyouliketoopentheerroruserlogfiletodebug") ?? "Would you like to open the 'error_user.log' file to debug the error?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = await _messageDialog.ShowYesNoAsync($"{simpleLauncherWasUnableToLaunchThisGame}\n\n" +
                                                         $"{wouldyouliketoopentheerroruserlogfiletodebug}", error);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Failed to open the 'error_user.log' file.");
                var thefileerroruserlog = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
                await _messageDialog.ShowErrorAsync(thefileerroruserlog, error);
            }
        }
    }

    public async Task FileSystemXmlIsCorruptedMessageBox(string logPath)
    {
        var thefilesystemxmlisbadlycorrupted = (string)Application.Current.TryFindResource("Thefilesystemxmlisbadlycorrupted") ?? "The file 'system.xml' is badly corrupted.";
        var wouldyouliketoopentheerroruserlog = (string)Application.Current.TryFindResource("Wouldyouliketoopentheerroruserlog") ?? "Would you like to open the 'error_user.log' file to investigate the issue?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{thefilesystemxmlisbadlycorrupted}\n\n" +
                                                         $"{wouldyouliketoopentheerroruserlog}", error);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Failed to open the error log file from a message box.");
                var thefileerroruserlog = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
                await _messageDialog.ShowErrorAsync(thefileerroruserlog, error);
            }
        }
    }

    public async Task InstallUpdateManuallyMessageBox()
    {
        var therewasanerrorinstallingorupdating = (string)Application.Current.TryFindResource("Therewasanerrorinstallingorupdating") ?? "There was an error installing or updating the application.";
        var wouldyouliketoberedirectedtothedownloadpage = (string)Application.Current.TryFindResource("Wouldyouliketoberedirectedtothedownloadpage") ?? "Would you like to be redirected to the download page to install or update it manually?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var messageBoxResult = await _messageDialog.ShowYesNoAsync($"{therewasanerrorinstallingorupdating}\n\n" +
                                                                   $"{wouldyouliketoberedirectedtothedownloadpage}", error);

        if (messageBoxResult)
        {
            var downloadPageUrl = App.ServiceProvider.GetRequiredService<IConfiguration>().GetValue<string>("Urls:GitHubReleases") ?? "https://github.com/drpetersonfernandes/SimpleLauncher/releases/latest/";

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
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Error in method InstallUpdateManuallyMessageBox");

                // Notify user
                var anerroroccurredwhileopeningthebrowser = (string)Application.Current.TryFindResource("Anerroroccurredwhileopeningthebrowser") ?? "An error occurred while opening the browser.";
                var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
                await _messageDialog.ShowErrorAsync($"{anerroroccurredwhileopeningthebrowser}\n\n" +
                                                    $"{theerrorwasreportedtothedeveloper}", error);
            }
        }
    }

    public async Task UpdaterLaunchFailedMessageBox()
    {
        var updaterLaunchFailed = (string)Application.Current.TryFindResource("UpdaterLaunchFailed") ?? "Failed to launch the Updater.";
        var accessDeniedExplanation = (string)Application.Current.TryFindResource("AccessDeniedExplanation") ?? "This may be due to insufficient permissions or Windows security settings blocking the file.";
        var wouldyouliketoberedirectedtothedownloadpage = (string)Application.Current.TryFindResource("Wouldyouliketoberedirectedtothedownloadpage") ?? "Would you like to be redirected to the download page to install or update it manually?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var messageBoxResult = await _messageDialog.ShowYesNoAsync($"{updaterLaunchFailed}\n\n" +
                                                                   $"{accessDeniedExplanation}\n\n" +
                                                                   $"{wouldyouliketoberedirectedtothedownloadpage}", error);

        if (messageBoxResult)
        {
            var downloadPageUrl = App.ServiceProvider.GetRequiredService<IConfiguration>().GetValue<string>("Urls:GitHubReleases") ?? "https://github.com/drpetersonfernandes/SimpleLauncher/releases/latest/";

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
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Error in method UpdaterLaunchFailedMessageBox");

                // Notify user
                var anerroroccurredwhileopeningthebrowser = (string)Application.Current.TryFindResource("Anerroroccurredwhileopeningthebrowser") ?? "An error occurred while opening the browser.";
                var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
                await _messageDialog.ShowErrorAsync($"{anerroroccurredwhileopeningthebrowser}\n\n" +
                                                    $"{theerrorwasreportedtothedeveloper}", error);
            }
        }
    }

    public async Task RequiredFileMissingMessageBox()
    {
        var fileappsettingsjsonismissing = (string)Application.Current.TryFindResource("Fileappsettingsjsonismissing") ?? "File 'appsettings.json' is missing.";
        var theapplicationwillnotbeabletosendthesupportrequest = (string)Application.Current.TryFindResource("Theapplicationwillnotbeabletosendthesupportrequest") ?? "The application will not be able to send the support request.";
        var doyouwanttoautomaticallyreinstall = (string)Application.Current.TryFindResource("Doyouwanttoautomaticallyreinstall") ?? "Do you want to automatically reinstall 'Simple Launcher' to fix the problem?";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";

        var messageBoxResult = await _messageDialog.ShowYesNoAsync($"{fileappsettingsjsonismissing}\n\n" +
                                                                   $"{theapplicationwillnotbeabletosendthesupportrequest}\n\n" +
                                                                   $"{doyouwanttoautomaticallyreinstall}", warning);

        if (messageBoxResult)
        {
            _reinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            var pleasereinstallSimpleLauncher = (string)Application.Current.TryFindResource("PleasereinstallSimpleLauncher") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            await _messageDialog.ShowWarningAsync(pleasereinstallSimpleLauncher, warning);
        }
    }

    public Task EnterSupportRequestMessageBox()
    {
        var pleaseenterthedetailsofthesupportrequest = (string)Application.Current.TryFindResource("Pleaseenterthedetailsofthesupportrequest") ?? "Please enter the details of the support request.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync(pleaseenterthedetailsofthesupportrequest, info);
    }

    public Task EnterNameMessageBox()
    {
        var pleaseenterthename = (string)Application.Current.TryFindResource("Pleaseenterthename") ?? "Please enter the name.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync(pleaseenterthename, info);
    }

    public Task EnterEmailMessageBox()
    {
        var pleaseentertheemail = (string)Application.Current.TryFindResource("Pleaseentertheemail") ?? "Please enter the email.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync(pleaseentertheemail, info);
    }

    public Task ApiKeyErrorMessageBox()
    {
        var therewasanerrorintheApiKey = (string)Application.Current.TryFindResource("TherewasanerrorintheAPIKey") ?? "There was an error in the API Key of this form.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{therewasanerrorintheApiKey}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task SupportRequestSuccessMessageBox()
    {
        var supportrequestsentsuccessfully = (string)Application.Current.TryFindResource("Supportrequestsentsuccessfully") ?? "Support request sent successfully.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync(supportrequestsentsuccessfully, info);
    }

    public Task SupportRequestSendErrorMessageBox()
    {
        var anerroroccurredwhilesendingthesupportrequest = (string)Application.Current.TryFindResource("Anerroroccurredwhilesendingthesupportrequest") ?? "An error occurred while sending the support request.";
        var thebugwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Thebugwasreportedtothedeveloper") ?? "The bug was reported to the developer that will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowInfoAsync($"{anerroroccurredwhilesendingthesupportrequest}\n\n" +
                                            $"{thebugwasreportedtothedeveloper}", error);
    }

    public Task ExtractionFailedMessageBox()
    {
        var extractionfailed = (string)Application.Current.TryFindResource("Extractionfailed") ?? "Extraction failed.";
        var ensurethefileisnotcorrupted = (string)Application.Current.TryFindResource("Ensurethefileisnotcorrupted") ?? "Ensure the file is not corrupted.";
        var ensureyouhaveenoughspaceintheHdd = (string)Application.Current.TryFindResource("EnsureyouhaveenoughspaceintheHDD") ?? "Ensure you have enough space in the HDD to extract the file.";
        var grantSimpleLauncheradministrative = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var ensuretheSimpleLauncherfolder = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncherfolder") ?? "Ensure the 'Simple Launcher' folder is a writable directory.";
        var temporarilydisableyourantivirus = (string)Application.Current.TryFindResource("Temporarilydisableyourantivirus") ?? "Temporarily disable your antivirus software and try again.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{extractionfailed}\n\n" +
                                             $"{ensurethefileisnotcorrupted}\n" +
                                             $"{ensureyouhaveenoughspaceintheHdd}\n" +
                                             $"{grantSimpleLauncheradministrative}\n" +
                                             $"{ensuretheSimpleLauncherfolder}\n" +
                                             $"{temporarilydisableyourantivirus}", error);
    }

    public Task FileNeedToBeCompressedMessageBox()
    {
        var theselectedfilecannotbe = (string)Application.Current.TryFindResource("Theselectedfilecannotbe") ?? "The selected file cannot be extracted.";
        var toextractafileitneedstobe = (string)Application.Current.TryFindResource("Toextractafileitneedstobe") ?? "To extract a file, it needs to be a 7z, zip, or rar file.";
        var pleasefixthatintheEditwindow = (string)Application.Current.TryFindResource("PleasefixthatintheEditwindow") ?? "Please fix that in the Edit window.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowWarningAsync($"{theselectedfilecannotbe}\n\n" +
                                               $"{toextractafileitneedstobe}\n\n" +
                                               $"{pleasefixthatintheEditwindow}", warning);
    }

    public Task DownloadedFileIsMissingMessageBox()
    {
        var downloadedfileismissing = (string)Application.Current.TryFindResource("Downloadedfileismissing") ?? "Downloaded file is missing.";
        var oneDriveIssue = (string)Application.Current.TryFindResource("oneDriveIssue") ?? "If the file is in OneDrive, ensure it is synced and downloaded to your device. Right-click the file in File Explorer and select 'Always keep on this device'.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{downloadedfileismissing}\n\n" +
                                             $"{oneDriveIssue}", error);
    }

    public async Task FileIsLockedMessageBox(string tempFolderPath)
    {
        var downloadedfileislocked = (string)Application.Current.TryFindResource("Downloadedfileislocked") ?? "Downloaded file is locked.";
        var grantSimpleLauncheradministrative = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var temporarilydisableyourantivirussoftware = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var ensuretheSimpleLauncher = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncher") ?? "Ensure the 'Simple Launcher' folder is a writable directory.";
        var openTempFolderQuestion = (string)Application.Current.TryFindResource("OpenTempFolderQuestion") ?? "Would you like to open the temporary folder to inspect the file?"; // New line
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{downloadedfileislocked}\n\n" +
                                                         $"{grantSimpleLauncheradministrative}\n\n" +
                                                         $"{temporarilydisableyourantivirussoftware}\n\n" +
                                                         $"{ensuretheSimpleLauncher}\n\n" +
                                                         $"{openTempFolderQuestion}", error); // Changed to YesNo

        if (result)
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
                await _messageDialog.ShowErrorAsync(errorOpeningFolderMessage, errorOpeningFolderTitle);
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, $"Failed to open temp folder: {tempFolderPath}");
            }
        }
    }

    public Task LinksSavedMessageBox()
    {
        var linkssavedsuccessfully = (string)Application.Current.TryFindResource("Linkssavedsuccessfully") ?? "Links saved successfully.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync(linkssavedsuccessfully, info);
    }

    public Task DeadZonesSavedMessageBox()
    {
        var deadZonevaluessavedsuccessfully = (string)Application.Current.TryFindResource("DeadZonevaluessavedsuccessfully") ?? "DeadZone values saved successfully.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync(deadZonevaluessavedsuccessfully, info);
    }

    public Task LinksRevertedMessageBox()
    {
        var linksreverted = (string)Application.Current.TryFindResource("Linksrevertedtodefaultvalues") ?? "Links reverted to default values.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync(linksreverted, info);
    }

    public Task MainWindowSearchEngineErrorMessageBox()
    {
        var therewasanerrorwiththesearchengine = (string)Application.Current.TryFindResource("Therewasanerrorwiththesearchengine") ?? "There was an error with the search engine.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{therewasanerrorwiththesearchengine}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task DownloadExtractionFailedMessageBox()
    {
        var downloadorextractionfailed = (string)Application.Current.TryFindResource("DownloadorExtractionFailed") ?? "Download or extraction failed.";
        var grantSimpleLauncheradministrativeaccess = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrativeaccess") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var ensuretheSimpleLauncherfolder = (string)Application.Current.TryFindResource("EnsuretheSimpleLauncherfolder") ?? "Ensure the 'Simple Launcher' folder is a writable directory.";
        var temporarilydisableyourantivirus = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{downloadorextractionfailed}\n\n" +
                                             $"{grantSimpleLauncheradministrativeaccess}\n\n" +
                                             $"{ensuretheSimpleLauncherfolder}\n\n" +
                                             $"{temporarilydisableyourantivirus}", error);
    }

    public Task DownloadAndExtractionWereSuccessfulMessageBox()
    {
        var downloadandextractioncompletedsuccessfully = (string)Application.Current.TryFindResource("Downloadandextractioncompletedsuccessfully") ?? "Download and extraction completed successfully.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync(downloadandextractioncompletedsuccessfully, info);
    }

    public async Task ShowEmulatorDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem)
    {
        var downloaderror = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
        var wouldyouliketoberedirected = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{downloaderror}\n\n" +
                                                         $"{wouldyouliketoberedirected}", error);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = selectedSystem.Emulators.Emulator.EmulatorDownloadLink, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                // Notify developer
                const string contextMessage = "Error opening the download link.";
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, contextMessage);

                // Notify user
                var erroropeningthedownloadlink = (string)Application.Current.TryFindResource("Erroropeningthedownloadlink") ?? "Error opening the download link.";
                var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
                await _messageDialog.ShowErrorAsync($"{erroropeningthedownloadlink}\n\n" +
                                                    $"{theerrorwasreportedtothedeveloper}", error);
            }
        }
    }

    public async Task ShowCoreDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem)
    {
        var downloaderror = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
        var wouldyouliketoberedirected =
            (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{downloaderror}\n\n" +
                                                         $"{wouldyouliketoberedirected}", error);

        if (result)
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
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, contextMessage);

                // Notify user
                var erroropeningthedownloadlink = (string)Application.Current.TryFindResource("Erroropeningthedownloadlink") ?? "Error opening the download link.";
                var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
                await _messageDialog.ShowErrorAsync($"{erroropeningthedownloadlink}\n\n" +
                                                    $"{theerrorwasreportedtothedeveloper}", error);
            }
        }
    }

    public async Task ShowImagePackDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem)
    {
        if (selectedSystem?.Emulators?.Emulator?.ImagePackDownloadLink == null)
        {
        }

        {
            var downloadError = (string)Application.Current.TryFindResource("Downloaderror") ?? "Download error.";
            var wouldYouLikeToBeRedirected = (string)Application.Current.TryFindResource("Wouldyouliketoberedirected") ?? "Would you like to be redirected to the download page?";
            var errorCaption = (string)Application.Current.TryFindResource("Error") ?? "Error";

            var result = await _messageDialog.ShowYesNoAsync($"{downloadError}\n\n" +
                                                             $"{wouldYouLikeToBeRedirected}", errorCaption);

            if (result)
            {
                try
                {
                    Process.Start(new ProcessStartInfo { FileName = selectedSystem?.Emulators?.Emulator?.ImagePackDownloadLink, UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    // Notify developer
                    const string contextMessage = "Error opening the download link.";
                    App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, contextMessage);

                    // Notify user
                    var errorOpeningDownloadLink = (string)Application.Current.TryFindResource("Erroropeningthedownloadlink") ?? "Error opening the download link.";
                    var errorWasReported = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
                    await _messageDialog.ShowErrorAsync($"{errorOpeningDownloadLink}\n\n{errorWasReported}", errorCaption);
                }
            }
        }
    }

    public Task SelectAHistoryItemToRemoveMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("SelectAHistoryItemToRemove") ?? "Please select a history item to remove.";
        var pleaseselectaitem = (string)Application.Current.TryFindResource("Pleaseselectaitem") ?? "Please select a item";
        return _messageDialog.ShowInfoAsync(message, pleaseselectaitem);
    }

    public Task<CoreMessageBoxResult> ReallyWantToRemoveAllPlayHistoryMessageBox()
    {
        return Task.FromResult(CoreMessageBoxResult.No);
    }

    public Task SystemAddedMessageBox(string systemName, string resolvedSystemFolder, string resolvedSystemImageFolder)
    {
        var thesystem = (string)Application.Current.TryFindResource("Thesystem") ?? "The system";
        var hasbeenaddedsuccessfully = (string)Application.Current.TryFindResource("hasbeenaddedsuccessfully") ?? "has been added successfully.";
        var putRoMsorIsOsforthissysteminside = (string)Application.Current.TryFindResource("PutROMsorISOsforthissysteminside") ?? "Put ROMs or ISOs for this system inside";
        var putcoverimagesforthissysteminside = (string)Application.Current.TryFindResource("Putcoverimagesforthissysteminside") ?? "Put cover images for this system inside";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync($"{thesystem} '{systemName}' {hasbeenaddedsuccessfully}\n\n"
                                            + $"{putRoMsorIsOsforthissysteminside} '{resolvedSystemFolder}'\n\n"
                                            + $"{putcoverimagesforthissysteminside} '{resolvedSystemImageFolder}'.", info);
    }

    public Task AddSystemFailedMessageBox(string details = null)
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

        return _messageDialog.ShowErrorAsync(message, error);
    }

    public Task RightClickContextMenuErrorMessageBox()
    {
        var therewasanerrorintherightclick = (string)Application.Current.TryFindResource("Therewasanerrorintherightclick") ?? "There was an error in the right-click context menu.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{therewasanerrorintherightclick}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task GameFileDoesNotExistMessageBox()
    {
        var thegamefiledoesnotexist = (string)Application.Current.TryFindResource("Thegamefiledoesnotexist") ?? "The game file does not exist!";
        var thefilehasbeenremovedfromthelist = (string)Application.Current.TryFindResource("Thefilehasbeenremovedfromthelist") ?? "The file has been removed from the list.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync($"{thegamefiledoesnotexist}\n\n" +
                                            $"{thefilehasbeenremovedfromthelist}", info);
    }

    public Task<CoreMessageBoxResult> GameFileDoesNotExistAskToDeleteMessageBox(string filePath)
    {
        return Task.FromResult(CoreMessageBoxResult.No);
    }

    public Task<CoreMessageBoxResult> FavoriteFileDoesNotExistAskToDeleteMessageBox(string filePath)
    {
        return Task.FromResult(CoreMessageBoxResult.No);
    }

    public Task CouldNotOpenHistoryWindowMessageBox()
    {
        var therewasaproblemopeningtheHistorywindow = (string)Application.Current.TryFindResource("TherewasaproblemopeningtheHistorywindow") ?? "There was a problem opening the History window.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{therewasaproblemopeningtheHistorywindow}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task CouldNotOpenWalkthroughMessageBox()
    {
        var failedtoopenthewalkthroughfile = (string)Application.Current.TryFindResource("Failedtoopenthewalkthroughfile") ?? "Failed to open the walkthrough file.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{failedtoopenthewalkthroughfile}\n\n"
                                             + $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task SelectAFavoriteToRemoveMessageBox()
    {
        var pleaseselectafavoritetoremove = (string)Application.Current.TryFindResource("Pleaseselectafavoritetoremove") ?? "Please select a favorite to remove.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowWarningAsync(pleaseselectafavoritetoremove, warning);
    }

    public Task SystemXmlNotFoundMessageBox()
    {
        var systemxmlnotfound = (string)Application.Current.TryFindResource("systemxmlnotfound") ?? "'system.xml' not found inside the application folder.";
        var pleaserestartSimpleLauncher = (string)Application.Current.TryFindResource("PleaserestartSimpleLauncher") ?? "Please restart 'Simple Launcher'.";
        var ifthatdoesnotwork = (string)Application.Current.TryFindResource("Ifthatdoesnotwork") ?? "If that does not work, please reinstall 'Simple Launcher'.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{systemxmlnotfound}\n\n" +
                                             $"{pleaserestartSimpleLauncher}\n\n" +
                                             $"{ifthatdoesnotwork}", error);
    }

    public Task YouCanAddANewSystemMessageBox()
    {
        var youcanaddanewsystem = (string)Application.Current.TryFindResource("Youcanaddanewsystem") ?? "You can add a new system now.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync(youcanaddanewsystem, info);
    }

    public Task EmulatorNameRequiredMessageBox(int i)
    {
        var emulator = (string)Application.Current.TryFindResource("Emulator") ?? "Emulator";
        var nameisrequiredbecauserelateddata = (string)Application.Current.TryFindResource("nameisrequiredbecauserelateddata") ?? "name is required because related data has been provided.";
        var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync($"{emulator} {i} {nameisrequiredbecauserelateddata}\n\n" +
                                            $"{pleasefixthisfield}", info);
    }

    public Task EmulatorNameIsRequiredMessageBox()
    {
        var emulatornameisrequired = (string)Application.Current.TryFindResource("Emulatornameisrequired") ?? "Emulator name is required.";
        var pleasefixthat = (string)Application.Current.TryFindResource("Pleasefixthat") ?? "Please fix that.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowInfoAsync($"{emulatornameisrequired}\n\n" +
                                            $"{pleasefixthat}", error);
    }

    public Task EmulatorNameMustBeUniqueMessageBox(string emulatorName)
    {
        var thename = (string)Application.Current.TryFindResource("Thename") ?? "The name";
        var isusedmultipletimes = (string)Application.Current.TryFindResource("isusedmultipletimes") ?? "is used multiple times. Each emulator name must be unique.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync($"{thename} '{emulatorName}' {isusedmultipletimes}", info);
    }

    public Task SystemSavedSuccessfullyMessageBox()
    {
        var systemsavedsuccessfully = (string)Application.Current.TryFindResource("Systemsavedsuccessfully") ?? "System saved successfully.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync(systemsavedsuccessfully, info);
    }

    public Task PathOrParameterInvalidMessageBox()
    {
        var oneormorepathsorparameters = (string)Application.Current.TryFindResource("Oneormorepathsorparameters") ?? "One or more paths or parameters are invalid.";
        var pleasefixthemtoproceed = (string)Application.Current.TryFindResource("Pleasefixthemtoproceed") ?? "Please fix them to proceed.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{oneormorepathsorparameters}\n\n" +
                                             $"{pleasefixthemtoproceed}", error);
    }

    public Task Emulator1RequiredMessageBox()
    {
        var emulator1Nameisrequired = (string)Application.Current.TryFindResource("Emulator1Nameisrequired") ?? "'Emulator 1 Name' is required.";
        var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{emulator1Nameisrequired}\n\n" +
                                             $"{pleasefixthisfield}", error);
    }

    public Task ExtensionToLaunchIsRequiredMessageBox()
    {
        var extensiontoLaunchAfterExtraction = (string)Application.Current.TryFindResource("ExtensiontoLaunchAfterExtraction") ?? "'Extension to Launch After Extraction' is required when 'Extract File Before Launch' is set to true.";
        var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{extensiontoLaunchAfterExtraction}\n\n" +
                                             $"{pleasefixthisfield}", error);
    }

    public Task ExtensionToSearchIsRequiredMessageBox()
    {
        var extensiontoSearchintheSystemFolder = (string)Application.Current.TryFindResource("ExtensiontoSearchintheSystemFolder") ?? "'Extension to Search in the System Folder' cannot be empty or contain only spaces.";
        var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{extensiontoSearchintheSystemFolder}\n\n" +
                                             $"{pleasefixthisfield}", error);
    }

    public Task FileMustBeCompressedMessageBox()
    {
        var whenExtractFileBeforeLaunch = (string)Application.Current.TryFindResource("WhenExtractFileBeforeLaunch") ?? "When 'Extract File Before Launch' is set to true, 'Extension to Search in the System Folder' must include 'zip', '7z', or 'rar'.";
        var itwillnotacceptotherextensions = (string)Application.Current.TryFindResource("Itwillnotacceptotherextensions") ?? "It will not accept other extensions.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{whenExtractFileBeforeLaunch}\n\n" +
                                             $"{itwillnotacceptotherextensions}", error);
    }

    public Task SystemImageFolderCanNotBeEmptyMessageBox()
    {
        var systemImageFoldercannotbeempty = (string)Application.Current.TryFindResource("SystemImageFoldercannotbeempty") ?? "'System Image Folder' cannot be empty or contain only spaces.";
        var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{systemImageFoldercannotbeempty}\n\n" +
                                             $"{pleasefixthisfield}", error);
    }

    public Task SystemFolderCanNotBeEmptyMessageBox()
    {
        var systemFoldercannotbeempty = (string)Application.Current.TryFindResource("SystemFoldercannotbeempty") ?? "'System Folder' cannot be empty or contain only spaces.";
        var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{systemFoldercannotbeempty}\n\n" +
                                             $"{pleasefixthisfield}", error);
    }

    public Task SystemNameCanNotBeEmptyMessageBox()
    {
        var systemNamecannotbeemptyor = (string)Application.Current.TryFindResource("SystemNamecannotbeemptyor") ?? "'System Name' cannot be empty or contain only spaces.";
        var pleasefixthisfield = (string)Application.Current.TryFindResource("Pleasefixthisfield") ?? "Please fix this field.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{systemNamecannotbeemptyor}\n\n" +
                                             $"{pleasefixthisfield}", error);
    }

    public Task InvalidSystemNameCharactersMessageBox(string invalidChars)
    {
        var systemNamecontainsinvalid = (string)Application.Current.TryFindResource("SystemNamecontainsinvalid") ?? "'System Name' contains invalid characters:";
        var pleaseRemoveTheseCharacters = (string)Application.Current.TryFindResource("PleaseRemoveTheseCharacters") ?? "Please remove these characters and try again.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{systemNamecontainsinvalid}\n\n{invalidChars}\n\n" +
                                             $"{pleaseRemoveTheseCharacters}", error);
    }

    public Task InvalidFolderCharactersMessageBox(string invalidChars)
    {
        var systemFoldercontainsinvalid = (string)Application.Current.TryFindResource("SystemFoldercontainsinvalid") ?? "'System Folder' contains invalid characters:";
        var pleaseRemoveTheseCharacters = (string)Application.Current.TryFindResource("PleaseRemoveTheseCharacters") ?? "Please remove these characters and try again.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{systemFoldercontainsinvalid}\n\n{invalidChars}\n\n" +
                                             $"{pleaseRemoveTheseCharacters}", error);
    }

    public Task FolderCreationFailedMessageBox()
    {
        var simpleLauncherfailedtocreatethe = (string)Application.Current.TryFindResource("SimpleLauncherfailedtocreatethe") ?? "'Simple Launcher' failed to create the necessary folders for this system.";
        var grantSimpleLauncheradministrative = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var temporarilydisableyourantivirus = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var ensurethattheSimpleLauncherfolderislocatedinawritable = (string)Application.Current.TryFindResource("EnsurethattheSimpleLauncherfolderislocatedinawritable") ?? "Ensure that the 'Simple Launcher' folder is located in a writable directory.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync($"{simpleLauncherfailedtocreatethe}\n\n" +
                                            $"{grantSimpleLauncheradministrative}\n\n" +
                                            $"{temporarilydisableyourantivirus}\n\n" +
                                            $"{ensurethattheSimpleLauncherfolderislocatedinawritable}", info);
    }

    public Task SelectASystemToDeleteMessageBox()
    {
        var pleaseselectasystemtodelete = (string)Application.Current.TryFindResource("Pleaseselectasystemtodelete") ?? "Please select a system to delete.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowWarningAsync(pleaseselectasystemtodelete, warning);
    }

    public Task SystemNotFoundInTheXmlMessageBox()
    {
        var selectedsystemnotfound = (string)Application.Current.TryFindResource("Selectedsystemnotfound") ?? "Selected system not found in the XML document!";
        var alert = (string)Application.Current.TryFindResource("Alert") ?? "Alert";
        return _messageDialog.ShowWarningAsync(selectedsystemnotfound, alert);
    }

    public async Task ErrorFindingGameFilesMessageBox(string logPath)
    {
        var therewasanerrorfinding = (string)Application.Current.TryFindResource("Therewasanerrorfinding") ?? "There was an error finding the game files.";
        var doyouwanttoopenthefileerroruserlog = (string)Application.Current.TryFindResource("Doyouwanttoopenthefileerroruserlog") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = await _messageDialog.ShowYesNoAsync($"{therewasanerrorfinding}\n\n" +
                                                         $"{doyouwanttoopenthefileerroruserlog}", error);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Failed to open the error log file from a message box.");
                // Notify user
                var thefileerroruserlogwas = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ?? "The file 'error_user.log' was not found!";
                await _messageDialog.ShowErrorAsync(thefileerroruserlogwas, error);
            }
        }
    }

    public async Task GamePadErrorMessageBox(string logPath)
    {
        var therewasanerrorwiththeGamePadController = (string)Application.Current.TryFindResource("TherewasanerrorwiththeGamePadController") ?? "There was an error with the GamePad Controller.";
        var grantSimpleLauncheradministrative = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var temporarilydisableyourantivirus = (string)Application.Current.TryFindResource("Youcanalsotemporarilydisableyourantivirussoftware") ?? "You can also temporarily disable your antivirus software or add 'Simple Launcher' folder to the antivirus exclusion list.";
        var doyouwanttoopenthefile = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = await _messageDialog.ShowYesNoAsync($"{therewasanerrorwiththeGamePadController}\n\n" +
                                                         $"{grantSimpleLauncheradministrative}\n\n" +
                                                         $"{temporarilydisableyourantivirus}\n\n" +
                                                         $"{doyouwanttoopenthefile}", error);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Failed to open the error log file from a message box.");
                var thefileerroruserlogwas = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ??
                                             "The file 'error_user.log' was not found!";
                await _messageDialog.ShowErrorAsync(thefileerroruserlogwas, error);
            }
        }
    }

    public async Task CouldNotLaunchGameMessageBox(string logPath)
    {
        var simpleLaunchercouldnotlaunch = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotlaunch") ?? "'Simple Launcher' could not launch the selected game.";
        var makesuretheRoMorIsOyouretrying = (string)Application.Current.TryFindResource("MakesuretheROMorISOyouretrying") ?? "Make sure the ROM or ISO you're trying to run is not corrupted.";
        var ifyouaretryingtorunRetroarchensurethattheBios = (string)Application.Current.TryFindResource("IfyouaretryingtorunRetroarchensurethattheBIOS") ?? "If you are trying to run Retroarch, ensure that the BIOS or required files for the core are installed.";
        var alsomakesureyouarecallingtheemulator = (string)Application.Current.TryFindResource("Alsomakesureyouarecallingtheemulator") ?? "Also, make sure you are calling the emulator with the correct parameter.";
        var youcanturnoffthistypeoferrormessageinExpertmode = (string)Application.Current.TryFindResource("YoucanturnoffthiserrormessageinExpertmode") ?? "You can turn off this error message in Expert mode.";
        var doyouwanttoopenthefile = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = await _messageDialog.ShowYesNoAsync($"{simpleLaunchercouldnotlaunch}\n\n" +
                                                         $"{makesuretheRoMorIsOyouretrying}\n" +
                                                         $"{ifyouaretryingtorunRetroarchensurethattheBios}\n" +
                                                         $"{alsomakesureyouarecallingtheemulator}\n\n" +
                                                         $"{youcanturnoffthistypeoferrormessageinExpertmode}\n\n" +
                                                         $"{doyouwanttoopenthefile}", error);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Failed to open the error log file from a message box.");
                var thefileerroruserlogwas = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ?? "The file 'error_user.log' was not found!";
                await _messageDialog.ShowErrorAsync(thefileerroruserlogwas, error);
            }
        }
    }

    public async Task InvalidOperationExceptionMessageBox(string logPath)
    {
        var simpleLaunchercouldnotlaunch = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotlaunch") ?? "'Simple Launcher' could not launch the selected game.";
        var makesuretheRoMorIsOyouretrying = (string)Application.Current.TryFindResource("MakesuretheROMorISOyouretrying") ?? "Make sure the ROM or ISO you're trying to run is not corrupted.";
        var ifyouaretryingtorunRetroarchensurethattheBios = (string)Application.Current.TryFindResource("IfyouaretryingtorunRetroarchensurethattheBIOS") ?? "If you are trying to run Retroarch, ensure that the BIOS or required files for the core are installed.";
        var alsomakesureyouarecallingtheemulator = (string)Application.Current.TryFindResource("Alsomakesureyouarecallingtheemulator") ?? "Also, make sure you are calling the emulator with the correct parameter.";
        var youcanturnoffthistypeoferrormessageinExpertmode = (string)Application.Current.TryFindResource("YoucanturnoffthiserrormessageinExpertmode") ?? "You can turn off this error message in Expert mode.";
        var doyouwanttoopenthefile = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var result = await _messageDialog.ShowYesNoAsync($"{simpleLaunchercouldnotlaunch}\n\n" +
                                                         $"{makesuretheRoMorIsOyouretrying}\n" +
                                                         $"{ifyouaretryingtorunRetroarchensurethattheBios}\n" +
                                                         $"{alsomakesureyouarecallingtheemulator}\n\n" +
                                                         $"{youcanturnoffthistypeoferrormessageinExpertmode}\n\n" +
                                                         $"{doyouwanttoopenthefile}", error);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Failed to open the error log file from a message box.");
                var thefileerroruserlogwas = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ?? "The file 'error_user.log' was not found!";
                await _messageDialog.ShowErrorAsync(thefileerroruserlogwas, error);
            }
        }
    }

    public async Task ThereWasAnErrorLaunchingThisGameMessageBox(string logPath)
    {
        var therewasanerrorlaunchingthisgame = (string)Application.Current.TryFindResource("Therewasanerrorlaunchingthisgame") ?? "There was an error launching this game.";
        var youcanturnoffthistypeoferrormessageinExpertmode = (string)Application.Current.TryFindResource("YoucanturnoffthiserrormessageinExpertmode") ?? "You can turn off this error message in Expert mode.";
        var doyouwanttoopenthefileerroruserlog = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{therewasanerrorlaunchingthisgame}\n\n" +
                                                         $"{youcanturnoffthistypeoferrormessageinExpertmode}\n\n" +
                                                         $"{doyouwanttoopenthefileerroruserlog}", error);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Failed to open the error log file from a message box.");
                var thefileerroruserlog = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
                await _messageDialog.ShowErrorAsync(thefileerroruserlog, error);
            }
        }
    }

    public async Task BatchFileFailedMessageBox(string batchFilePath, string errorDetail, string logPath, int? exitCode = null)
    {
        var batchFileName = Path.GetFileName(batchFilePath);
        var batchfilefailed = (string)Application.Current.TryFindResource("Batchfilefailed") ?? "The batch file failed to run.";
        var batchNameMessage = $"{batchfilefailed}\n\n{batchFileName}";
        var errorMessage = !string.IsNullOrEmpty(errorDetail)
            ? $"Error: {errorDetail}\n\n"
            : "";
        var exitCodeMessage = exitCode.HasValue
            ? $"Exit code: {exitCode.Value}\n\n"
            : "";
        var explanation = exitCode is < 0
            ? (string)Application.Current.TryFindResource("Theprogramlaunchedbythisbatch") ?? "The program launched by this batch file may have crashed or been terminated unexpectedly. Negative exit codes typically indicate system-level failures."
            : (string)Application.Current.TryFindResource("Batchfilefailedexplanation") ?? "This usually means a path referenced inside the batch file no longer exists or is incorrect.";
        var youcanturnoff = (string)Application.Current.TryFindResource("YoucanturnoffthiserrormessageinExpertmode") ?? "You can turn off this error message in Expert mode.";
        var doyouwanttoopen = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var message = $"{batchNameMessage}\n\n" +
                      $"{exitCodeMessage}{errorMessage}{explanation}\n\n" +
                      $"{youcanturnoff}\n\n" +
                      $"{doyouwanttoopen}";

        var result = await _messageDialog.ShowYesNoAsync(message, error);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Failed to open the error log file from a batch file error message box.");
                var notFound = (string)Application.Current.TryFindResource("Thefileerroruserlog") ?? "The file 'error_user.log' was not found!";
                await _messageDialog.ShowErrorAsync(notFound, error);
            }
        }
    }

    public Task<bool> BatchFilePathsMissingMessageBox(List<string> missingPaths)
    {
        try
        {
            const bool result = false;
            return Task.FromResult(result);
        }
        catch (Exception exception)
        {
            return Task.FromException<bool>(exception);
        }
    }

    public Task ElevationRequiredMessageBox()
    {
        var therewasanerrorlaunchingthisgame = (string)Application.Current.TryFindResource("Therewasanerrorlaunchingthisgame") ?? "There was an error launching this game.";
        var elevationrequired = (string)Application.Current.TryFindResource("ElevationRequired") ?? "The requested operation requires elevation (Administrator privileges).";
        var grantSimpleLauncheradministrative = (string)Application.Current.TryFindResource("GrantSimpleLauncheradministrative") ?? "Grant 'Simple Launcher' administrative access and try again.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        return _messageDialog.ShowErrorAsync($"{therewasanerrorlaunchingthisgame}\n\n" +
                                             $"{elevationrequired}\n\n" +
                                             $"{grantSimpleLauncheradministrative}", error);
    }

    public Task NullFileExtensionMessageBox()
    {
        var thereisnoExtension = (string)Application.Current.TryFindResource("ThereisnoExtension") ?? "There is no 'Extension to Launch After Extraction' set in the system configuration.";
        var pleaseeditthissystemto = (string)Application.Current.TryFindResource("Pleaseeditthissystemto") ?? "Please edit this system to fix that.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{thereisnoExtension}\n\n" +
                                             $"{pleaseeditthissystemto}", error);
    }

    public Task CouldNotFindAFileMessageBox()
    {
        var couldnotfindafilewiththeextensiondefined = (string)Application.Current.TryFindResource("Couldnotfindafilewiththeextensiondefined") ?? "Could not find a file with the extension defined in 'Extension to Launch After Extraction' inside the extracted folder.";
        var pleaseeditthissystemtofix = (string)Application.Current.TryFindResource("Pleaseeditthissystemto") ?? "Please edit this system to fix that.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{couldnotfindafilewiththeextensiondefined}\n\n" +
                                             $"{pleaseeditthissystemtofix}", error);
    }

    public Task<CoreMessageBoxResult> SearchOnlineForRomHistoryMessageBox()
    {
        return Task.FromResult(CoreMessageBoxResult.No);
    }

    public Task SystemHasBeenDeletedMessageBox(string selectedSystemName)
    {
        var system = (string)Application.Current.TryFindResource("System") ?? "System";
        var hasbeendeleted = (string)Application.Current.TryFindResource("hasbeendeleted") ?? "has been deleted.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync($"{system} '{selectedSystemName}' {hasbeendeleted}", info);
    }

    public Task<CoreMessageBoxResult> AreYouSureDoYouWantToDeleteThisSystemMessageBox()
    {
        return Task.FromResult(CoreMessageBoxResult.No);
    }

    public Task ThereWasAnErrorDeletingTheGameMessageBox()
    {
        var therewasanerrordeletingthefile = (string)Application.Current.TryFindResource("Therewasanerrordeletingthefile") ?? "There was an error deleting the file.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{therewasanerrordeletingthefile}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task ThereWasAnErrorDeletingTheCoverImageMessageBox()
    {
        var therewasanerrordeletingthecoverimage = (string)Application.Current.TryFindResource("Therewasanerrordeletingthecoverimage") ?? "There was an error deleting the cover image.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{therewasanerrordeletingthecoverimage}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task<CoreMessageBoxResult> AreYouSureYouWantToDeleteTheGameMessageBox(string fileNameWithExtension)
    {
        return Task.FromResult(CoreMessageBoxResult.No);
    }

    public Task<CoreMessageBoxResult> AreYouSureYouWantToDeleteTheCoverImageMessageBox(string fileNameWithoutExtension)
    {
        return Task.FromResult(CoreMessageBoxResult.No);
    }

    public Task<CoreMessageBoxResult> WouldYouLikeToSaveAReportMessageBox()
    {
        return Task.FromResult(CoreMessageBoxResult.No);
    }

    public Task SimpleLauncherWasUnableToRestoreBackupMessageBox()
    {
        var simpleLauncherwasunabletorestore = (string)Application.Current.TryFindResource("SimpleLauncherwasunabletorestore") ?? "'Simple Launcher' was unable to restore the last backup.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(simpleLauncherwasunabletorestore, error);
    }

    public Task<CoreMessageBoxResult> WouldYouLikeToRestoreTheLastBackupMessageBox()
    {
        return Task.FromResult(CoreMessageBoxResult.No);
    }

    public Task FailedToLoadLanguageResourceMessageBox()
    {
        var failedtoloadlanguageresources = (string)Application.Current.TryFindResource("Failedtoloadlanguageresources") ?? "Failed to load language resources.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var languageLoadingError = (string)Application.Current.TryFindResource("LanguageLoadingError") ?? "Language Loading Error";
        return _messageDialog.ShowErrorAsync($"{failedtoloadlanguageresources}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", languageLoadingError);
    }

    public Task InvalidSystemConfigurationMessageBox(string errorMessage)
    {
        var invalidSystemConfiguration = (string)Application.Current.TryFindResource("InvalidSystemConfiguration") ?? "Invalid System Configuration";
        return _messageDialog.ShowWarningAsync(errorMessage, invalidSystemConfiguration);
    }

    public Task UnableToOpenLinkMessageBox()
    {
        var unabletoopenthelink = (string)Application.Current.TryFindResource("Unabletoopenthelink") ?? "Unable to open the link.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{unabletoopenthelink}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task NoGameFoundInTheRandomSelectionMessageBox()
    {
        var nogamesfoundtorandomlyselectfrom = (string)Application.Current.TryFindResource("Nogamesfoundtorandomlyselectfrom") ?? "No games found to randomly select from. Please check your system selection.";
        var feelingLucky = (string)Application.Current.TryFindResource("FeelingLucky") ?? "Feeling Lucky";
        return _messageDialog.ShowInfoAsync(nogamesfoundtorandomlyselectfrom, feelingLucky);
    }

    public Task PleaseSelectASystemBeforeMessageBox()
    {
        var pleaseselectasystembeforeusingtheFeeling = (string)Application.Current.TryFindResource("PleaseselectasystembeforeusingtheFeeling") ?? "Please select a system before using the Feeling Lucky feature.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowInfoAsync(pleaseselectasystembeforeusingtheFeeling, warning);
    }

    public Task ToggleFuzzyMatchingFailureMessageBox()
    {
        var therewasanerrortogglingthefuzzymatchinglogic = (string)Application.Current.TryFindResource("Therewasanerrortogglingthefuzzymatchinglogic") ?? "There was an error toggling the fuzzy matching logic.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(therewasanerrortogglingthefuzzymatchinglogic, error);
    }

    public Task FuzzyMatchingErrorFailToSetThresholdMessageBox()
    {
        var errorMessage = (string)Application.Current.TryFindResource("SetFuzzyMatchingThresholdFailureMessageBoxText") ?? "Failed to set fuzzy matching threshold.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(errorMessage, error);
    }

    public Task ListOfErrorsMessageBox(StringBuilder errorMessages)
    {
        var editSystemtofixit = (string)Application.Current.TryFindResource("EditSystemtofixit") ?? "Edit System to fix it.";
        var validationerrors = (string)Application.Current.TryFindResource("Validationerrors") ?? "Validation errors";
        var fullMessage = errorMessages + editSystemtofixit;
        return _messageDialog.ShowErrorAsync(fullMessage, validationerrors);
    }

    public Task ThereIsNoUpdateAvailableMessageBox(string currentVersion)
    {
        var thereisnoupdateavailable = (string)Application.Current.TryFindResource("thereisnoupdateavailable") ?? "There is no update available.";
        var thecurrentversionis = (string)Application.Current.TryFindResource("Thecurrentversionis") ?? "The current version is";
        var noupdateavailable = (string)Application.Current.TryFindResource("Noupdateavailable") ?? "No update available";
        return _messageDialog.ShowInfoAsync($"{thereisnoupdateavailable}\n\n" +
                                            $"{thecurrentversionis} {currentVersion}", noupdateavailable);
    }

    public Task AnotherInstanceIsRunningMessageBox()
    {
        var anotherinstanceofSimpleLauncherisalreadyrunning = (string)Application.Current.TryFindResource("AnotherinstanceofSimpleLauncherisalreadyrunning") ?? "Another instance of 'Simple Launcher' is already running.";
        return _messageDialog.ShowInfoAsync(anotherinstanceofSimpleLauncherisalreadyrunning, "Simple Launcher");
    }

    public Task FailedToStartSimpleLauncherMessageBox()
    {
        var failedtostartSimpleLauncherAnerroroccurred = (string)Application.Current.TryFindResource("FailedtostartSimpleLauncherAnerroroccurred") ?? "Failed to start 'Simple Launcher'. An error occurred while checking for existing instances.";
        var simpleLauncherError = (string)Application.Current.TryFindResource("SimpleLauncherError") ?? "Simple Launcher Error";
        return _messageDialog.ShowErrorAsync(failedtostartSimpleLauncherAnerroroccurred, simpleLauncherError);
    }

    public Task FailedToRestartMessageBox()
    {
        var failedtorestarttheapplication = (string)Application.Current.TryFindResource("Failedtorestarttheapplication") ?? "Failed to restart the application.";
        var restartError = (string)Application.Current.TryFindResource("RestartError") ?? "Restart Error";
        return _messageDialog.ShowErrorAsync(failedtorestarttheapplication, restartError);
    }

    public Task<CoreMessageBoxResult> DoYouWantToUpdateMessageBox(string currentVersion, string latestVersion)
    {
        return Task.FromResult(CoreMessageBoxResult.No);
    }

    public async Task HandleMissingRequiredFilesMessageBox(string fileList)
    {
        var thefollowingrequiredfilesaremissing = (string)Application.Current.TryFindResource("Thefollowingrequiredfilesaremissing") ?? "The following required file(s) are missing:";
        var missingRequiredFiles = (string)Application.Current.TryFindResource("MissingRequiredFiles") ?? "Missing Required Files";
        var doyouwanttoreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
        var reinstall = await _messageDialog.ShowYesNoAsync($"{thefollowingrequiredfilesaremissing}\n" +
                                                            $"{fileList}\n\n" +
                                                            $"{doyouwanttoreinstallSimpleLauncher}", missingRequiredFiles);

        if (reinstall)
        {
            _reinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            var pleasereinstallSimpleLauncher = (string)Application.Current.TryFindResource("PleasereinstallSimpleLauncher") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            var theapplicationwillshutdown = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";
            await _messageDialog.ShowErrorAsync($"{pleasereinstallSimpleLauncher}\n\n{theapplicationwillshutdown}", missingRequiredFiles);

            _quitSimpleLauncher.SimpleQuitApplication();
        }
    }

    public async Task HandleApiConfigErrorMessageBox(string reason)
    {
        var apiConfigErrorTitle = (string)Application.Current.TryFindResource("ApiConfigErrorTitle") ?? "API Configuration Error";
        var apiConfigErrorMessage = (string)Application.Current.TryFindResource("ApiConfigErrorMessage") ?? "'Simple Launcher' encountered an error loading its API configuration.";
        var reasonLabel = (string)Application.Current.TryFindResource("ReasonLabel") ?? "Reason:";
        var reinstallSuggestion = (string)Application.Current.TryFindResource("ReinstallSuggestion") ?? "This might prevent some features (like automatic bug reporting) from working correctly. Would you like to reinstall 'Simple Launcher' to fix this?";

        var result = await _messageDialog.ShowYesNoAsync($"{apiConfigErrorMessage}\n\n" +
                                                         $"{reasonLabel} {reason}\n\n" +
                                                         $"{reinstallSuggestion}", apiConfigErrorTitle);

        if (result)
        {
            _reinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            var manualReinstallSuggestion = (string)Application.Current.TryFindResource("ManualReinstallSuggestion") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            var applicationWillShutdown = (string)Application.Current.TryFindResource("Theapplicationwillshutdown") ?? "The application will shutdown.";

            await _messageDialog.ShowErrorAsync($"{manualReinstallSuggestion}\n\n" +
                                                $"{applicationWillShutdown}", apiConfigErrorTitle);

            _quitSimpleLauncher.SimpleQuitApplication();
        }
    }

    public Task DiskSpaceErrorMessageBox()
    {
        var notenoughdiskspaceforextraction = (string)Application.Current.TryFindResource("Notenoughdiskspaceforextraction") ?? "Not enough disk space for extraction.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(notenoughdiskspaceforextraction, error);
    }

    public Task CouldNotCheckForDiskSpaceMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotcheckdiskspace") ?? "'Simple Launcher' could not check disk space for the specified path. Please check the path and try again.";
        var caption = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(message, caption);
    }

    public Task SaveSystemFailedMessageBox(string details = null)
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

        return _messageDialog.ShowErrorAsync(message, error);
    }

    public Task CouldNotOpenTheDownloadLinkMessageBox()
    {
        var simpleLaunchercouldnotopenthedownloadlink = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotopenthedownloadlink") ?? "'Simple Launcher' could not open the download link.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(simpleLaunchercouldnotopenthedownloadlink, error);
    }

    public Task ErrorLoadingAppSettingsMessageBox()
    {
        var therewasanerrorloadingconfiguration = (string)Application.Current.TryFindResource("Therewasanerrorloadingconfiguration") ?? "There was an error loading 'appsettings.json'.";
        var theerrorwasreportedtothedeveloper = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{therewasanerrorloadingconfiguration}\n\n" +
                                             $"{theerrorwasreportedtothedeveloper}", error);
    }

    public Task PotentialPathManipulationDetectedMessageBox(string archivePath)
    {
        var title = (string)Application.Current.TryFindResource("SecurityWarning") ?? "Security Warning";
        var pathManipulationDetected = (string)Application.Current.TryFindResource("PathManipulationDetected") ?? "Potential Path Manipulation Detected";
        var zipSlipExplanation = (string)Application.Current.TryFindResource("ZipSlipExplanation") ?? "A security vulnerability called 'Zip Slip' was detected in the archive file. This is a path traversal vulnerability that could allow an attacker to write files outside of the intended extraction directory.";
        var archivePathMessage = (string)Application.Current.TryFindResource("ArchivePathMessage") ?? "Archive file:";
        var actionTaken = (string)Application.Current.TryFindResource("ActionTaken") ?? "For your security, the extraction process has been properly handle and the issue has been logged.";
        var reportedToDeveloper = (string)Application.Current.TryFindResource("ReportedToDeveloper") ?? "This security issue has been reported to the developer team.";
        return _messageDialog.ShowWarningAsync($"{pathManipulationDetected}\n\n" +
                                               $"{zipSlipExplanation}\n\n" +
                                               $"{archivePathMessage} {archivePath}\n\n" +
                                               $"{actionTaken}\n\n" +
                                               $"{reportedToDeveloper}", title);
    }

    public Task CouldNotOpenSoundConfigurationWindowMessageBox()
    {
        var couldNotOpenSoundConfigurationWindow = (string)Application.Current.TryFindResource("CouldNotOpenSoundConfigurationWindow") ?? "Could not open sound configuration window";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowWarningAsync(couldNotOpenSoundConfigurationWindow, warning);
    }

    public Task ErrorSettingSoundFileMessageBox()
    {
        var errorSettingSoundFile = (string)Application.Current.TryFindResource("errorSettingSoundFile") ?? "Error choosing or copying sound file.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowWarningAsync(errorSettingSoundFile, warning);
    }

    public Task NotificationSoundIsDisableMessageBox()
    {
        var notificationSoundIsDisable = (string)Application.Current.TryFindResource("NotificationSoundIsDisable") ?? "Notification sound is disable";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync(notificationSoundIsDisable, info);
    }

    public Task NoSoundFileIsSelectedMessageBox()
    {
        var noSoundFileSelectedWarning = (string)Application.Current.TryFindResource("NoSoundFileSelectedWarning") ?? "No sound file is selected.";
        var warning = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowWarningAsync(noSoundFileSelectedWarning, warning);
    }

    public Task SettingsSavedSuccessfullyMessageBox()
    {
        var settingsSavedSuccessfully = (string)Application.Current.TryFindResource("SettingsSavedSuccessfully") ?? "Settings saved successfully.";
        var info = (string)Application.Current.TryFindResource("Info") ?? "Info";
        return _messageDialog.ShowInfoAsync(settingsSavedSuccessfully, info);
    }

    public Task FailedToSaveSettingsMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("FailedToSaveSettings") ?? "Failed to save settings. Please check that the application folder is writable and not locked by another process.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(message, error);
    }

    public async Task FilePathIsInvalidMessageBox(string logPath)
    {
        var simpleLaunchercouldnotlaunch = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotlaunch") ?? "'Simple Launcher' could not launch the selected game.";
        var thefilepathisinvalid = (string)Application.Current.TryFindResource("Thefilepathisinvalid") ?? "The filepath is invalid or the file does not exist!";
        var networkPathIssue = (string)Application.Current.TryFindResource("networkPathIssue") ?? "If the file is on a network drive ensure your computer is still connected to that drive.";
        var usbDeviceIssue = (string)Application.Current.TryFindResource("usbDeviceIssue") ?? "If the file is on a portable USB device ensure it is still connected to your computer.";
        var oneDriveIssue = (string)Application.Current.TryFindResource("oneDriveIssue") ?? "If the file is in OneDrive, ensure it is synced and downloaded to your device. Right-click the file in File Explorer and select 'Always keep on this device'.";
        var avoidusingspecialcharactersinthefilepath = (string)Application.Current.TryFindResource("Avoidusingspecialcharactersinthefilepath") ?? "Avoid using special characters in the filepath, such as @, !, ?, ~, or any other special characters.";
        var youcanturnoffthistypeoferrormessageinExpertmode = (string)Application.Current.TryFindResource("YoucanturnoffthiserrormessageinExpertmode") ?? "You can turn off this error message in Expert mode.";
        var doyouwanttoopenthefile = (string)Application.Current.TryFindResource("Doyouwanttoopenthefile") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var result = await _messageDialog.ShowYesNoAsync($"{simpleLaunchercouldnotlaunch}\n\n" +
                                                         $"{thefilepathisinvalid}\n" +
                                                         $"{networkPathIssue}\n" +
                                                         $"{usbDeviceIssue}\n" +
                                                         $"{oneDriveIssue}\n" +
                                                         $"{avoidusingspecialcharactersinthefilepath}\n\n" +
                                                         $"{youcanturnoffthistypeoferrormessageinExpertmode}\n\n" +
                                                         $"{doyouwanttoopenthefile}", error);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Failed to open the error log file from a message box.");
                var thefileerroruserlogwas = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ?? "The file 'error_user.log' was not found!";
                await _messageDialog.ShowErrorAsync(thefileerroruserlogwas, error);
            }
        }
    }

    public async Task ThereWasAnErrorMountingTheFileMessageBox(int? exitCode = null)
    {
        var simpleLaunchercouldnotmount = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotmount") ?? "'Simple Launcher' could not mount the selected game.";
        var reasonMessage = exitCode switch
        {
            -1073741510 => (string)Application.Current.TryFindResource("ThisDokanVersionIncompatible") ?? "The installed version of Dokan may be incompatible. Try reinstalling or updating Dokan.",
            -1073741515 => (string)Application.Current.TryFindResource("Dokannotinstalled") ?? "Dokan library is not installed. Dokan is required for mounting ZIP, CHD and disk image files.",
            _ => (string)Application.Current.TryFindResource("ThismaybeduetoDokannotbeinginstalled2") ?? "This may be due to Dokan not being installed. Dokan is required for mounting ZIP, CHD and disk image files."
        };
        var doyouwanttoopenthefile = (string)Application.Current.TryFindResource("DoyouwanttoopenyourbrowsertodownloadDokan") ?? "Do you want to open your browser to download Dokan?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var messageBoxResult = await _messageDialog.ShowYesNoAsync($"{simpleLaunchercouldnotmount}\n\n" +
                                                                   $"{reasonMessage}\n\n" +
                                                                   $"{doyouwanttoopenthefile}", error);

        if (messageBoxResult)
        {
            var downloadPageUrl = App.ServiceProvider.GetRequiredService<IConfiguration>().GetValue<string>("Urls:DokanyWebsite") ?? "https://github.com/dokan-dev/dokany";

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
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Could not open the Dokan website.");

                // Notify user
                var anerroroccurredwhileopeningthebrowser = (string)Application.Current.TryFindResource("Anerroroccurredwhileopeningyourbrowser") ?? "An error occurred while opening your browser.";
                await _messageDialog.ShowErrorAsync(anerroroccurredwhileopeningthebrowser, error);
            }
        }
    }

    public async Task DokanDriverNotInstalledMessageBox()
    {
        var dokanDriverNotFound = (string)Application.Current.TryFindResource("DokanDriverNotFound") ?? "The Dokan file system driver (dokan2.dll) is required to mount archives as virtual drives. It does not appear to be installed on this system.";
        var doYouWantToOpenBrowser = (string)Application.Current.TryFindResource("DoyouwanttoopenyourbrowsertodownloadDokan") ?? "Do you want to open your browser to download Dokan?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var messageBoxResult = await _messageDialog.ShowYesNoAsync($"{dokanDriverNotFound}\n\n{doYouWantToOpenBrowser}", error);

        if (messageBoxResult)
        {
            var downloadPageUrl = App.ServiceProvider.GetRequiredService<IConfiguration>().GetValue<string>("Urls:DokanyWebsite") ?? "https://github.com/dokan-dev/dokany";

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
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Could not open the Dokan website.");
                var anerroroccurredwhileopeningthebrowser = (string)Application.Current.TryFindResource("Anerroroccurredwhileopeningyourbrowser") ?? "An error occurred while opening your browser.";
                await _messageDialog.ShowErrorAsync(anerroroccurredwhileopeningthebrowser, error);
            }
        }
    }

    public Task LaunchToolInformationMessageBox(string info)
    {
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowInfoAsync(info, error);
    }

    public Task CannotScreenshotMinimizedWindowMessageBox()
    {
        var cannottakeascreenshotofaminimizedwindow = (string)Application.Current.TryFindResource("Cannottakeascreenshotofaminimizedwindow") ?? "Cannot take a screenshot of a minimized window.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(cannottakeascreenshotofaminimizedwindow, error);
    }

    public Task FailedToCopyLogContentMessageBox()
    {
        var failedtocopylogcontent = (string)Application.Current.TryFindResource("Failedtocopylogcontent") ?? "Failed to copy log content.";
        var copyError = (string)Application.Current.TryFindResource("CopyError") ?? "Copy Error";
        return _messageDialog.ShowErrorAsync(failedtocopylogcontent, copyError);
    }

    public Task CouldNotFindUpdaterOnGitHubMessageBox()
    {
        var simpleLaunchercouldnotfindtheupdater = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotfindtheupdater") ?? "'Simple Launcher' could not find the updater application on GitHub.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(simpleLaunchercouldnotfindtheupdater, error);
    }

    public Task CouldNotOpenAchievementsWindowMessageBox()
    {
        var couldNotOpenAchievementsWindow = (string)Application.Current.TryFindResource("CouldNotOpenAchievementsWindow") ?? "Could not open the achievements window.";
        var theErrorWasReported = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{couldNotOpenAchievementsWindow}\n\n{theErrorWasReported}", error);
    }

    public Task<CoreMessageBoxResult> GameNotSupportedByRetroAchievementsMessageBox()
    {
        return Task.FromResult(CoreMessageBoxResult.No);
    }

    public Task GameLaunchTimeoutMessageBox()
    {
        var gamelaunchtimedoutPleasetryagainorcheckiftheemulatorstarted = (string)Application.Current.TryFindResource("GamelaunchtimedoutPleasetryagainorcheckiftheemulatorstarted") ?? "Game launch timed out. Please try again or check if the emulator started.";
        var gamelaunchtimedout = (string)Application.Current.TryFindResource("Gamelaunchtimedout") ?? "Game launch timed out";
        return _messageDialog.ShowErrorAsync(gamelaunchtimedoutPleasetryagainorcheckiftheemulatorstarted, gamelaunchtimedout);
    }

    public Task AddRaLoginMessageBox()
    {
        var youneedtoaddRetroAchievementlogin = (string)Application.Current.TryFindResource("YouneedtoaddRetroAchievementlogin") ?? "You need to add RetroAchievement login information to use this feature.";
        var attention = (string)Application.Current.TryFindResource("Attention") ?? "Attention";
        return _messageDialog.ShowInfoAsync(youneedtoaddRetroAchievementlogin, attention);
    }

    public Task NoDefaultBrowserConfiguredMessageBox()
    {
        var noDefaultBrowserConfiguredMessage = (string)Application.Current.TryFindResource("NoDefaultBrowserConfiguredMessage") ??
                                                "Your operating system does not have a default web browser configured. Please set one in Windows Settings (Apps > Default apps) to open web links.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(noDefaultBrowserConfiguredMessage, error);
    }

    public Task<CoreMessageBoxResult> WarnUserAboutMemoryConsumptionMessageBox()
    {
        var warningMessage = (string)Application.Current.TryFindResource("WarningSettingupaveryhighnumberofgamesperpage") ?? "Warning! Setting a very high number of games per page will significantly increase system memory usage when in Grid mode. If the number is too high, this may cause the application to crash. Please proceed with caution.";
        var proceedQuestion = (string)Application.Current.TryFindResource("AreYouSureYouWantToProceed") ?? "Are you sure you want to proceed?";
        var warningTitle = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowAsync($"{warningMessage}\n\n{proceedQuestion}", warningTitle, CoreMessageBoxButton.YesNo, CoreMessageBoxImage.Warning);
    }

    public Task GroupByFolderOnlyForMameAndDosBoxMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("TheGroupFilesbyFolderoptionisonlycompatiblewith") ?? "The 'Group Files by Folder' option is only compatible with MAME emulators (Software List CHDs) or DOSBox emulators (uncompressed DOS game folders). To use a different emulator, please edit the system settings and disable this option.";
        var title = (string)Application.Current.TryFindResource("CompatibilityWarning") ?? "Compatibility Warning";
        return _messageDialog.ShowWarningAsync(message, title);
    }

    public Task<CoreMessageBoxResult> GroupByFolderWarningMessageBox()
    {
        return Task.FromResult(CoreMessageBoxResult.No);
    }

    public Task<CoreMessageBoxResult> FirstRunWelcomeMessageBox()
    {
        return Task.FromResult(CoreMessageBoxResult.No);
    }

    public Task Emulator1LocationRequiredMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("Emulator1pathisrequired") ?? "Emulator 1 path is required.";
        var title = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowWarningAsync(message, title);
    }

    public Task Emulator2LocationRequiredMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("Emulator2pathisrequired") ?? "Emulator 2 path is required.";
        var title = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowWarningAsync(message, title);
    }

    public Task Emulator3LocationRequiredMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("Emulator3pathisrequired") ?? "Emulator 3 path is required.";
        var title = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowWarningAsync(message, title);
    }

    public Task Emulator4LocationRequiredMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("Emulator4pathisrequired") ?? "Emulator 4 path is required.";
        var title = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowWarningAsync(message, title);
    }

    public Task Emulator5LocationRequiredMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("Emulator5pathisrequired") ?? "Emulator 5 path is required.";
        var title = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowWarningAsync(message, title);
    }

    public Task ImagePackDownloaderUnavailableMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotaccesstheWebAPI") ?? "'Simple Launcher' could not access the Web API to download the updated URLs. Please try again later.";
        var title = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(message, title);
    }

    public Task EasyModeUnavailableMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotaccesstheWebAPIToDownloadTheUpdatedConfiguration") ?? "'Simple Launcher' could not access the Web API to download the updated configuration.";
        var message2 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotaccesstheWebAPIToDownloadTheUpdatedConfiguration2") ?? "This could be due to:";
        var message3 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotaccesstheWebAPIToDownloadTheUpdatedConfiguration3") ?? "• A government firewall or internet restriction in your region";
        var message4 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotaccesstheWebAPIToDownloadTheUpdatedConfiguration4") ?? "• Network connectivity issues";
        var message5 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotaccesstheWebAPIToDownloadTheUpdatedConfiguration5") ?? "To resolve this issue, you can:";
        var message6 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotaccesstheWebAPIToDownloadTheUpdatedConfiguration6") ?? "1. Enable a VPN connection and try again";
        var message7 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotaccesstheWebAPIToDownloadTheUpdatedConfiguration7") ?? "2. Check your internet connection";
        var message8 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotaccesstheWebAPIToDownloadTheUpdatedConfiguration8") ?? "3. Configure systems manually using the Edit System feature";
        var message9 = (string)Application.Current.TryFindResource("SimpleLaunchercouldnotaccesstheWebAPIToDownloadTheUpdatedConfiguration9") ?? "Note: A VPN may be required if you are located in a country with internet restrictions.";
        var title = (string)Application.Current.TryFindResource("EasyModeUnavailable") ?? "Easy Mode Unavailable";
        return _messageDialog.ShowWarningAsync($"{message}\n\n" +
                                               $"{message2}\n" +
                                               $"{message3}\n" +
                                               $"{message4}\n\n" +
                                               $"{message5}\n" +
                                               $"{message6}\n" +
                                               $"{message7}\n" +
                                               $"{message8}\n\n" +
                                               $"{message9}", title);
    }

    public Task SimpleLauncherDoesNotSupportRaHashOfSystemGroupedByFolderMessageBox()
    {
        var simpleLauncherdoesnotsupportRetroAchievementshashofSystems = (string)Application.Current.TryFindResource("simpleLauncherdoesnotsupportRetroAchievementshashofSystems") ?? "'Simple Launcher' does not support RetroAchievements hash of systems Grouped by Folder.";
        var pleaseedittheSystemsettingsanddisablethe = (string)Application.Current.TryFindResource("pleaseedittheSystemsettingsanddisablethe") ?? "Please edit the system settings and disable the 'Group Files by Folder' option.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{simpleLauncherdoesnotsupportRetroAchievementshashofSystems}\n\n" +
                                             $"{pleaseedittheSystemsettingsanddisablethe}", error);
    }

    public Task UnsupportedArchitectureMessageBox()
    {
        var simpleLauncherdoesnotsupportthecurrentprocessorarchitecture = (string)Application.Current.TryFindResource("SimpleLauncherdoesnotsupportthecurrentprocessorarchitecture") ?? "'Simple Launcher' does not support the current processor architecture. We only support 64-bit (x64) or ARM64. The application will now close.";
        var unsupportedArchitecture = (string)Application.Current.TryFindResource("UnsupportedArchitecture") ?? "Unsupported Architecture";
        return _messageDialog.ShowErrorAsync(simpleLauncherdoesnotsupportthecurrentprocessorarchitecture, unsupportedArchitecture);
    }

    public async Task SevenZipDllNotFoundMessageBox()
    {
        var the7Zdllismissingfromtheapplicationfolder = (string)Application.Current.TryFindResource("The7zdllismissingfromtheapplicationfolder") ?? "The 7z dll is missing from the application folder!";
        var doyouwanttoreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var reinstall = await _messageDialog.ShowYesNoAsync($"{the7Zdllismissingfromtheapplicationfolder}\n\n" + $"{doyouwanttoreinstallSimpleLauncher}", error);

        if (reinstall)
        {
            _reinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            var pleasereinstallSimpleLauncher = (string)Application.Current.TryFindResource("PleasereinstallSimpleLauncher") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            await _messageDialog.ShowErrorAsync(pleasereinstallSimpleLauncher, error);

            Application.Current.Shutdown();
        }
    }

    public async Task FailedToInitializeSevenZipMessageBox()
    {
        var anunexpectederroroccurredwhileinitializingthe7Ziplibrary = (string)Application.Current.TryFindResource("Anunexpectederroroccurredwhileinitializingthe7Ziplibrary") ?? "An unexpected error occurred while initializing the 7-Zip library.";
        var doyouwanttoreinstallSimpleLauncher = (string)Application.Current.TryFindResource("DoyouwanttoreinstallSimpleLauncher") ?? "Do you want to reinstall 'Simple Launcher' to fix the issue?";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";

        var reinstall = await _messageDialog.ShowYesNoAsync($"{anunexpectederroroccurredwhileinitializingthe7Ziplibrary}\n\n" + $"{doyouwanttoreinstallSimpleLauncher}", error);

        if (reinstall)
        {
            _reinstallSimpleLauncher.StartUpdaterAndShutdown();
        }
        else
        {
            var pleasereinstallSimpleLauncher = (string)Application.Current.TryFindResource("PleasereinstallSimpleLauncher") ?? "Please reinstall 'Simple Launcher' manually to fix the issue.";
            await _messageDialog.ShowErrorAsync(pleasereinstallSimpleLauncher, error);

            Application.Current.Shutdown();
        }
    }

    public Task ShowExtractionFailedMessageBoxAsync(string tempFolderPath)
    {
        return Task.CompletedTask;
    }

    public Task ShowDownloadFileLockedMessageBoxAsync(string tempFolderPath)
    {
        return Task.CompletedTask;
    }

    public async Task ShowCustomMessageBox(string message, string launchError, string logPath)
    {
        var therewasanerrorlaunchingtheselected = (string)Application.Current.TryFindResource("Therewasanerrorlaunchingtheselected") ?? "There was an error launching the selected game.";
        var dowanttoopenthefileerroruserlog = (string)Application.Current.TryFindResource("Dowanttoopenthefileerroruserlog") ?? "Do want to open the file 'error_user.log' to debug the error?";

        var result = await _messageDialog.ShowYesNoAsync($"{therewasanerrorlaunchingtheselected}\n\n" +
                                                         $"{message}\n\n" +
                                                         $"{dowanttoopenthefileerroruserlog}", launchError);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Failed to open the error log file from a message box.");
                var thefileerroruserlogwasnotfound = (string)Application.Current.TryFindResource("Thefileerroruserlogwasnotfound") ?? "The file 'error_user.log' was not found!";

                await _messageDialog.ShowErrorAsync(thefileerroruserlogwasnotfound, launchError);
            }
        }
    }

    public Task EnterValidSearchTermsMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("EnterValidSearchTerms") ?? "Please enter valid search terms.";
        var title = (string)Application.Current.TryFindResource("InvalidSearch") ?? "Invalid Search";
        return _messageDialog.ShowWarningAsync(message, title);
    }

    public Task OperationCancelledMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("OperationCancelledMessage") ?? "The operation was cancelled.";
        var title = (string)Application.Current.TryFindResource("OperationCancelled") ?? "Operation Cancelled";
        return _messageDialog.ShowInfoAsync(message, title);
    }

    public Task<CoreMessageBoxResult> DoYouWantToCancelAndCloseMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("ProcessingStillRunningMessage") ?? "Processing is still running. Do you want to cancel and close?";
        var title = (string)Application.Current.TryFindResource("ConfirmClose") ?? "Confirm Close";
        return _messageDialog.ShowAsync(message, title, CoreMessageBoxButton.YesNo, CoreMessageBoxImage.Question);
    }

    public Task CouldNotOpenBrowserForAiSupportMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("CouldnotopenbrowserforAIsupport") ?? "Could not open browser for AI support.";
        var error = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(message, error);
    }

    public Task PowerShellExecutionPolicyRestrictionsMessageBox()
    {
        var unabletoscanMicrosoftStoregames = (string)Application.Current.TryFindResource("UnabletoscanMicrosoftStoregames") ?? "Unable to scan Microsoft Store games due to PowerShell execution policy restrictions.";
        var thisistypicallycausedbyGroupPolicy = (string)Application.Current.TryFindResource("ThisistypicallycausedbyGroupPolicy") ?? "This is typically caused by Group Policy settings on corporate or managed PCs.";
        var simpleLaunchercannotperform = (string)Application.Current.TryFindResource("SimpleLaunchercannotperform") ?? "'Simple Launcher' cannot perform the requested task.";
        var powerShellRestricted = (string)Application.Current.TryFindResource("PowerShellRestricted") ?? "PowerShell Restricted";
        return _messageDialog.ShowWarningAsync($"{unabletoscanMicrosoftStoregames}\n\n" +
                                               $"{thisistypicallycausedbyGroupPolicy}\n\n" +
                                               $"{simpleLaunchercannotperform}", powerShellRestricted);
    }

    public Task UnabletomountIsOfileMessageBox()
    {
        var unabletomountIsOfile = (string)Application.Current.TryFindResource("UnabletomountISOfile") ?? "Unable to mount ISO file due to PowerShell execution policy restrictions.";
        var thisistypicallycausedbyGroup = (string)Application.Current.TryFindResource("ThisistypicallycausedbyGroup") ?? "This is typically caused by Group Policy settings on corporate or managed PCs.";
        var simpleLaunchercannotperform = (string)Application.Current.TryFindResource("SimpleLaunchercannotperform") ?? "'Simple Launcher' cannot perform the requested task.";
        var powerShellRestricted = (string)Application.Current.TryFindResource("PowerShellRestricted") ?? "PowerShell Restricted";
        return _messageDialog.ShowWarningAsync($"{unabletomountIsOfile}\n\n" +
                                               $"{thisistypicallycausedbyGroup}\n\n" +
                                               $"{simpleLaunchercannotperform}", powerShellRestricted);
    }

    public Task UnabletoDismountIsOfileMessageBox()
    {
        var unabletodismountIsOfile = (string)Application.Current.TryFindResource("UnabletoDismountISOfile") ?? "Unable to dismount ISO file due to PowerShell execution policy restrictions.";
        var thisistypicallycausedbyGroup = (string)Application.Current.TryFindResource("ThisistypicallycausedbyGroup") ?? "This is typically caused by Group Policy settings on corporate or managed PCs.";
        var simpleLaunchercannotperform = (string)Application.Current.TryFindResource("SimpleLaunchercannotperform") ?? "'Simple Launcher' cannot perform the requested task.";
        var powerShellRestricted = (string)Application.Current.TryFindResource("PowerShellRestricted") ?? "PowerShell Restricted";
        return _messageDialog.ShowWarningAsync($"{unabletodismountIsOfile}\n\n" +
                                               $"{thisistypicallycausedbyGroup}\n\n" +
                                               $"{simpleLaunchercannotperform}", powerShellRestricted);
    }

    public Task ApplicationControlPolicyBlockedMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("ApplicationControlPolicyBlockedFile") ?? "An application control policy blocked this file or link.";
        var simpleLaunchercannotperform = (string)Application.Current.TryFindResource("SimpleLaunchercannotperform") ?? "'Simple Launcher' cannot perform the requested task.";
        var securityPolicyBlocked = (string)Application.Current.TryFindResource("SecurityPolicyBlocked") ?? "Security Policy Blocked";
        return _messageDialog.ShowWarningAsync($"{message}\n\n" +
                                               $"{simpleLaunchercannotperform}\n\n", securityPolicyBlocked);
    }

    public async Task ApplicationControlPolicyBlockedManualLinkMessageBox(string url)
    {
        var message = (string)Application.Current.TryFindResource("ApplicationControlPolicyBlockedFileManualLink") ?? "An application control policy blocked this link.";
        var simpleLaunchercannotperform = (string)Application.Current.TryFindResource("SimpleLaunchercannotperform") ?? "'Simple Launcher' cannot perform the requested task.";
        var theUrLwascopiedtotheclipboard = (string)Application.Current.TryFindResource("TheURLwascopiedtotheclipboard") ?? "The URL was copied to the clipboard for your convenience. You can paste it into your browser.";
        var securityPolicyBlocked = (string)Application.Current.TryFindResource("SecurityPolicyBlocked") ?? "Security Policy Blocked";
        await _messageDialog.ShowWarningAsync($"{message}\n\n" +
                                              $"{simpleLaunchercannotperform}\n\n" +
                                              $"{theUrLwascopiedtotheclipboard}", securityPolicyBlocked);
        Clipboard.SetText(url); // Copy URL to clipboard
    }

    public Task EnterYourRetroAchievementsUsernameMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("PleaseenteryourRetroAchievements") ?? "Please enter your RetroAchievements username, API key, and password before configuring an emulator.";
        var message2 = (string)Application.Current.TryFindResource("CredentialsRequired") ?? "Credentials Required";
        return _messageDialog.ShowWarningAsync(message1, message2);
    }

    public Task EmulatorConfiguredSuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("Emulatorconfiguredsuccessfullyfor") ?? "Emulator configured successfully for RetroAchievements!";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task FailedToConfigureTheEmulatorMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("Failedtoconfiguretheemulator") ?? "Failed to configure the emulator. The configuration file might be missing, in an unexpected location, or read-only.";
        var message2 = (string)Application.Current.TryFindResource("ConfigurationFailed") ?? "Configuration Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task AnErrorOccurredWhileConfiguringTheEmulatorMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("Anerroroccurredwhileconfiguringtheemulator") ?? "An error occurred while configuring the emulator.";
        var message2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FailedToLoginToRetroAchievementsMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedtologintoRetroAchievements") ?? "Failed to log in to RetroAchievements. Please check your username and password.";
        var message2 = (string)Application.Current.TryFindResource("LoginFailed") ?? "Login Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FileSystemXmlIsLockedMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("Thefilesystemxmlislocked") ?? "The file 'system.xml' is locked or inaccessible by another process.";
        var message2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FailedToInjectMameConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedtoinjectMAMEconfiguration") ?? "Failed to inject MAME configuration. The error has been logged. Please check the emulator path and try again.";
        var message2 = (string)Application.Current.TryFindResource("InjectionError") ?? "Injection Error";
        return _messageDialog.ShowWarningAsync(message1, message2);
    }

    public Task MameConfigurationInjectedSuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("MAMEconfigurationinjectedsuccessfully") ?? "MAME configuration injected successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task FailedtoinjectMamEconfiguration2MessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedtoinjectMAMEconfigurationTheerror") ?? "Failed to inject MAME configuration. The error has been logged.";
        var message2 = (string)Application.Current.TryFindResource("InjectionError") ?? "Injection Error";
        return _messageDialog.ShowWarningAsync(message1, message2);
    }

    public Task MameEmulatorPathNotFoundMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("MAMEemulatorpathnotfoundPleaseselect") ?? "MAME emulator path not found. Please select 'mame.exe' or 'mame64.exe' to apply these settings.";
        var message2 = (string)Application.Current.TryFindResource("EmulatorRequired") ?? "Emulator Required";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task RetroArchemulatorpathnotfoundMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("RetroArchemulatorpathnotfoundPlease") ?? "RetroArch emulator path not found. Please select 'retroarch.exe' to apply these settings.";
        var message2 = (string)Application.Current.TryFindResource("EmulatorRequired") ?? "Emulator Required";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task FailedtoinjectRetroArchconfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedtoinjectRetroArchconfigurationTheerror") ?? "Failed to inject RetroArch configuration. The error has been logged. Please check the emulator path and try again.";
        var message2 = (string)Application.Current.TryFindResource("InjectionError") ?? "Injection Error";
        return _messageDialog.ShowWarningAsync(message1, message2);
    }

    public Task RetroArchConfigurationInjectedSuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("RetroArchconfigurationinjectedsuccessfully") ?? "RetroArch configuration injected successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task FailedtoinjectRetroArchconfiguration2MessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedtoinjectRetroArchconfigurationTheerrorhas") ?? "Failed to inject RetroArch configuration. The error has been logged.";
        var message2 = (string)Application.Current.TryFindResource("InjectionError") ?? "Injection Error";
        return _messageDialog.ShowWarningAsync(message1, message2);
    }

    public Task XeniaemulatorpathnotfoundMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("Xeniaemulatorpathnotfound") ?? "Xenia emulator path not found. Please select 'xenia.exe' or 'xenia_canary.exe' to apply these settings.";
        var message2 = (string)Application.Current.TryFindResource("EmulatorRequired") ?? "Emulator Required";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task FailedtoinjectXeniaconfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedtoinjectXeniaconfigurationTheerrorPleasecheck") ?? "Failed to inject Xenia configuration. The error has been logged. Please check the emulator path and try again.";
        var message2 = (string)Application.Current.TryFindResource("InjectionError") ?? "Injection Error";
        return _messageDialog.ShowWarningAsync(message1, message2);
    }

    public Task XeniaconfigurationinjectedsuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("Xeniaconfigurationinjectedsuccessfully") ?? "Xenia configuration injected successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task FailedtoinjectXeniaconfiguration2MessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedtoinjectXeniaconfigurationTheerror") ?? "Failed to inject Xenia configuration. The error has been logged.";
        var message2 = (string)Application.Current.TryFindResource("InjectionError") ?? "Injection Error";
        return _messageDialog.ShowWarningAsync(message1, message2);
    }

    public Task EnterUsernamePasswordMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("EnterUsernamePassword") ?? "Please enter your RetroAchievements username and password first.";
        var message2 = (string)Application.Current.TryFindResource("MissingInformation") ?? "Missing Information";
        return _messageDialog.ShowWarningAsync(message1, message2);
    }

    public Task AresemulatornotfoundMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("Aresemulatornotfound") ?? "Ares emulator not found. Please locate 'ares.exe'.";
        var message2 = (string)Application.Current.TryFindResource("EmulatorNotFound") ?? "Emulator Not Found";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task DaphnesettingssavedsuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("Daphnesettingssavedsuccessfully") ?? "Daphne settings saved successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task Pcsx2SettingssavedMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("PCSX2settingssaved") ?? "PCSX2 settings saved.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task SettingsSavedMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("SettingsSaved") ?? "Settings saved.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task CemuEmulatorNotFoundMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("Cemuemulatornotfound") ?? "Cemu emulator not found. Please locate 'Cemu.exe'.";
        var message2 = (string)Application.Current.TryFindResource("EmulatorNotFound") ?? "Emulator Not Found";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FailedtoinjectAresconfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedtoinjectAresconfiguration") ?? "Failed to inject Ares configuration. Please check file permissions and try again.";
        var message2 = (string)Application.Current.TryFindResource("InjectionFailed") ?? "Injection Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task CemuConfigurationSavedMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("CemuConfigurationSaved") ?? "Cemu configuration saved.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task FlycastEmulatorNotFoundMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("Flycastemulatornotfound") ?? "Flycast emulator not found. Please locate 'flycast.exe'.";
        var message2 = (string)Application.Current.TryFindResource("EmulatorNotFound") ?? "Emulator Not Found";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task AresConfigurationSavedSuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("AresConfigurationSavedSuccessfully") ?? "Ares configuration saved successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task FailedToSaveAresConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToSaveAresConfiguration") ?? "Failed to save Ares configuration. Please check file permissions.";
        var message2 = (string)Application.Current.TryFindResource("SaveFailed") ?? "Save Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FailedToInjectFlycastConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToInjectFlycastConfiguration") ?? "Failed to inject Flycast configuration. Please check file permissions and try again.";
        var message2 = (string)Application.Current.TryFindResource("InjectionFailed") ?? "Injection Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FlycastConfigurationSavedSuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FlycastConfigurationSavedSuccessfully") ?? "Flycast configuration saved successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task DolphinEmulatorNotFoundMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("DolphinEmulatorNotFound") ?? "Dolphin emulator not found. Please locate 'Dolphin.exe'.";
        var message2 = (string)Application.Current.TryFindResource("EmulatorNotFound") ?? "Emulator Not Found";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FailedToSaveFlycastConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToSaveFlycastConfiguration") ?? "Failed to save Flycast configuration. Please check file permissions.";
        var message2 = (string)Application.Current.TryFindResource("SaveFailed") ?? "Save Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FailedToInjectDolphinConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToInjectDolphinConfiguration") ?? "Failed to inject Dolphin configuration. Please check file permissions and try again.";
        var message2 = (string)Application.Current.TryFindResource("InjectionFailed") ?? "Injection Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task DolphinConfigurationSavedSuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("DolphinConfigurationSavedSuccessfully") ?? "Dolphin configuration saved successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task FailedToSaveDolphinConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToSaveDolphinConfiguration") ?? "Failed to save Dolphin configuration. Please check file permissions.";
        var message2 = (string)Application.Current.TryFindResource("SaveFailed") ?? "Save Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task SegaModel2EmulatorNotFoundMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("SEGAModel2EmulatorNotFound") ?? "SEGA Model 2 emulator not found. Please locate 'emulator.exe'.";
        var message2 = (string)Application.Current.TryFindResource("EmulatorNotFound") ?? "Emulator Not Found";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FailedToInjectSegaModel2ConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToInjectSEGAModel2Configuration") ?? "Failed to inject SEGA Model 2 configuration. Please check file permissions and try again.";
        var message2 = (string)Application.Current.TryFindResource("InjectionFailed") ?? "Injection Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task SegaModel2ConfigurationSavedSuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("SEGAModel2ConfigurationSavedSuccessfully") ?? "SEGA Model 2 configuration saved successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task BlastemEmulatorNotFoundMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("BlastememulatornotfoundPleaselocate") ?? "Blastem emulator not found. Please locate 'blastem.exe'.";
        var message2 = (string)Application.Current.TryFindResource("EmulatorNotFound") ?? "Emulator Not Found";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FailedToInjectBlastemConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToInjectBlastemConfiguration") ?? "Failed to inject Blastem configuration. Please check file permissions and try again.";
        var message2 = (string)Application.Current.TryFindResource("InjectionFailed") ?? "Injection Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task BlastemConfigurationSavedSuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("BlastemConfigurationSavedSuccessfully") ?? "Blastem configuration saved successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task FailedToSaveSegaModel2ConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToSaveSEGAModel2Configuration") ?? "Failed to save SEGA Model 2 configuration. Please check file permissions.";
        var message2 = (string)Application.Current.TryFindResource("SaveFailed") ?? "Save Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FailedToSaveBlastemConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToSaveBlastemConfiguration") ?? "Failed to save Blastem configuration. Please check file permissions.";
        var message2 = (string)Application.Current.TryFindResource("SaveFailed") ?? "Save Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task Rpcs3EmulatorNotFoundPleaseLocateMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("RPCS3emulatornotfoundPleaselocate") ?? "RPCS3 emulator not found. Please locate 'rpcs3.exe'.";
        var message2 = (string)Application.Current.TryFindResource("EmulatorNotFound") ?? "Emulator Not Found";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FailedToInjectRpcs3ConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToInjectRPCS3Configuration") ?? "Failed to inject RPCS3 configuration. Please check file permissions and try again.";
        var message2 = (string)Application.Current.TryFindResource("InjectionFailed") ?? "Injection Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task Rpcs3ConfigurationSavedSuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("RPCS3ConfigurationSavedSuccessfully") ?? "RPCS3 configuration saved successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task FailedToSaveRpcs3ConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedtosaveRPCS3configurationPleasecheck") ?? "Failed to save RPCS3 configuration. Please check file permissions.";
        var message2 = (string)Application.Current.TryFindResource("SaveFailed") ?? "Save Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task StellaEmulatorNotFoundMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("StellaemulatornotfoundPleaselocate") ?? "Stella emulator not found. Please locate 'stella.exe'.";
        var message2 = (string)Application.Current.TryFindResource("EmulatorNotFound") ?? "Emulator Not Found";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FailedToInjectStellaConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedtoinjectStellaconfiguration") ?? "Failed to inject Stella configuration. Please check file permissions and try again.";
        var message2 = (string)Application.Current.TryFindResource("InjectionFailed") ?? "Injection Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task SupermodelEmulatorNotFoundMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("SupermodelEmulatorNotFound") ?? "Supermodel emulator not found. Please locate 'Supermodel.exe'.";
        var message2 = (string)Application.Current.TryFindResource("EmulatorNotFound") ?? "Emulator Not Found";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task StellaConfigurationSavedSuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("StellaConfigurationSavedSuccessfully") ?? "Stella configuration saved successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task FailedToInjectSupermodelConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedtoinjectSupermodelconfiguration") ?? "Failed to inject Supermodel configuration. Please check file permissions and try again.";
        var message2 = (string)Application.Current.TryFindResource("InjectionFailed") ?? "Injection Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FailedToSaveStellaConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToSaveStellaConfiguration") ?? "Failed to save Stella configuration. Please check file permissions.";
        var message2 = (string)Application.Current.TryFindResource("SaveFailed") ?? "Save Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task SupermodelConfigurationSavedSuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("Supermodelconfigurationsavedsuccessfully") ?? "Supermodel configuration saved successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task FailedToSaveSupermodelConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToSaveSupermodelConfiguration") ?? "Failed to save Supermodel configuration. Please check file permissions.";
        var message2 = (string)Application.Current.TryFindResource("SaveFailed") ?? "Save Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task MednafenEmulatorNotFoundMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("Mednafenemulatornotfound") ?? "Mednafen emulator not found. Please locate 'mednafen.exe'.";
        var message2 = (string)Application.Current.TryFindResource("EmulatorNotFound") ?? "Emulator Not Found";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task MesenEmulatorNotFoundMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("Mesenemulatornotfound") ?? "Mesen emulator not found. Please locate 'Mesen.exe'.";
        var message2 = (string)Application.Current.TryFindResource("EmulatorNotFound") ?? "Emulator Not Found";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FailedToInjectMednafenConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedtoinjectMednafenconfiguration") ?? "Failed to inject Mednafen configuration. Please check file permissions and try again.";
        var message2 = (string)Application.Current.TryFindResource("InjectionFailed") ?? "Injection Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FailedToInjectMesenConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedtoinjectMesenconfiguration") ?? "Failed to inject Mesen configuration. Please check file permissions and try again.";
        var message2 = (string)Application.Current.TryFindResource("InjectionFailed") ?? "Injection Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task DuckStationEmulatorNotFoundMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("DuckStationemulatornotfound") ?? "DuckStation emulator not found. Please locate the DuckStation executable.";
        var message2 = (string)Application.Current.TryFindResource("EmulatorNotFound") ?? "Emulator Not Found";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task MednafenConfigurationSavedSuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("MednafenConfigurationSavedSuccessfully") ?? "Mednafen configuration saved successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task FailedToSaveMednafenConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToSaveMednafenConfiguration") ?? "Failed to save Mednafen configuration. Please check file permissions.";
        var message2 = (string)Application.Current.TryFindResource("SaveFailed") ?? "Save Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FailedToInjectDuckStationConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToInjectDuckStationConfiguration") ?? "Failed to inject DuckStation configuration. Please check file permissions and try again.";
        var message2 = (string)Application.Current.TryFindResource("InjectionFailed") ?? "Injection Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task DuckStationConfigurationSavedSuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("DuckStationConfigurationSavedSuccessfully") ?? "DuckStation configuration saved successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task FailedToSaveMesenConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToSaveMesenConfiguration") ?? "Failed to save Mesen configuration. Please check file permissions.";
        var message2 = (string)Application.Current.TryFindResource("SaveFailed") ?? "Save Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FailedToSaveDuckStationConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToSaveDuckStationConfiguration") ?? "Failed to save DuckStation configuration. Please check file permissions.";
        var message2 = (string)Application.Current.TryFindResource("SaveFailed") ?? "Save Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task MesenConfigurationSavedSuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("MesenConfigurationSavedSuccessfully") ?? "Mesen configuration saved successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task FailedToInjectYumirConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToSaveYumirConfiguration") ?? "Failed to save Yumir configuration. Please check file permissions.";
        var message2 = (string)Application.Current.TryFindResource("SaveFailed") ?? "Save Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task YumirConfigurationSavedSuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("YumirConfigurationSavedSuccessfully") ?? "Yumir configuration saved successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task RaineSettingsSavedAndInjectedMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("RaineSettingsSavedAndInjectedSuccessfully") ?? "Raine configuration has been successfully injected.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task RaineExecutableNotFoundMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("RaineConfig_PathNotFound") ?? "Raine executable not found. Please select it.";
        var message2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task YumirEmulatorNotFoundMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("YumirConfig_PathNotFound") ?? "Yumir executable not found. Please select it.";
        var message2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task ReDreamEmulatorPathNotFoundMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("ReDreamConfig_PathNotFound") ?? "ReDream executable not found. Please select it.";
        var message2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task FailedToInjectReDreamConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToSaveReDreamConfiguration") ?? "Failed to save ReDream configuration. Please check file permissions.";
        var message2 = (string)Application.Current.TryFindResource("SaveFailed") ?? "Save Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task ReDreamConfigurationInjectedSuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("ReDreamConfigurationSavedSuccessfully") ?? "ReDream configuration saved successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task CouldNotLaunchGameDueToDepViolationMessageBox()
    {
        var title = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var message = (string)Application.Current.TryFindResource("CouldNotLaunchGameDueToDepViolation") ?? "The game failed to launch due to a DEP (Data Execution Prevention) violation.";
        var message2 = (string)Application.Current.TryFindResource("CouldNotLaunchGameDueToDepViolation2") ?? "This is a Windows security feature that prevents programs from executing code in protected memory regions.";
        var message3 = (string)Application.Current.TryFindResource("CouldNotLaunchGameDueToDepViolation3") ?? "Ensure you're using the latest emulator version with improved security compatibility.";
        var message4 = (string)Application.Current.TryFindResource("CouldNotLaunchGameDueToDepViolation4") ?? "You can also try to switch to a different emulator or core.";
        return _messageDialog.ShowErrorAsync($"{message}\n\n" +
                                             $"{message2}\n\n" +
                                             $"{message3}\n\n" +
                                             $"{message4}", title);
    }

    public async Task MameRomSetErrorMessageBox()
    {
        var title = (string)Application.Current.TryFindResource("ROMFilesNotFound") ?? "ROM Files Not Found";
        var message1 = (string)Application.Current.TryFindResource("MameRomSetError1") ?? "MAME emulator could not find required files to launch this game.";
        var message2 = (string)Application.Current.TryFindResource("MameRomSetError2x") ?? "MAME is very restrictive about the filename of the game.";
        var message3 = (string)Application.Current.TryFindResource("MameRomSetError3") ?? "Please ensure you are running a compatible ROM set.";
        var message4 = (string)Application.Current.TryFindResource("MameRomSetError4") ?? "Would you like to visit the PleasureDome website to download a compatible ROM set?";
        var result = await _messageDialog.ShowYesNoAsync($"{message1}\n\n" +
                                                         $"{message2}\n\n" +
                                                         $"{message3}\n\n" +
                                                         $"{message4}", title);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = App.ServiceProvider.GetRequiredService<IConfiguration>().GetValue<string>("Urls:PleasureDomeWebsite") ?? "https://pleasuredome.github.io/pleasuredome/index.html",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                await _messageDialog.ShowErrorAsync($"Could not open browser: {ex.Message}", "Error");
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Could not open browser");
            }
        }
    }

    public async Task MameUnknownSystemErrorMessageBox()
    {
        var title = (string)Application.Current.TryFindResource("UnknownSystemError") ?? "Unknown System Error";
        var message1 = (string)Application.Current.TryFindResource("MameUnknownSystemError1") ?? "MAME emulator could not find a matching compatible system to launch.";
        var message2 = (string)Application.Current.TryFindResource("MameUnknownSystemError2") ?? "MAME is very restrictive about the filename of the game.";
        var message3 = (string)Application.Current.TryFindResource("MameUnknownSystemError3") ?? "The filename of your game must match the expected filename to run on MAME.";
        var message4 = (string)Application.Current.TryFindResource("MameUnknownSystemError4") ?? "Please ensure you are running a compatible ROM set.";
        var message5 = (string)Application.Current.TryFindResource("MameUnknownSystemError5") ?? "Would you like to visit the PleasureDome website to download a compatible ROM set?";
        var result = await _messageDialog.ShowYesNoAsync($"{message1}\n\n" +
                                                         $"{message2}\n\n" +
                                                         $"{message3}\n\n" +
                                                         $"{message4}\n\n" +
                                                         $"{message5}", title);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = App.ServiceProvider.GetRequiredService<IConfiguration>().GetValue<string>("Urls:PleasureDomeWebsite") ?? "https://pleasuredome.github.io/pleasuredome/index.html",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                await _messageDialog.ShowErrorAsync($"Could not open browser: {ex.Message}", "Error");
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Could not open browser");
            }
        }
    }

    public async Task MameUnableToLoadImageMessageBox()
    {
        var title = (string)Application.Current.TryFindResource("UnableToLoadImage") ?? "Unable to load image";
        var message1 = (string)Application.Current.TryFindResource("MameUnableToLoadImageError1") ?? "MAME emulator could not load the image file.";
        var message2 = (string)Application.Current.TryFindResource("MameUnableToLoadImageError2") ?? "MAME is very restrictive about the filename of the game.";
        var message3 = (string)Application.Current.TryFindResource("MameUnableToLoadImageError3") ?? "The filename of your game must match the expected filename to run on MAME.";
        var message4 = (string)Application.Current.TryFindResource("MameUnableToLoadImageError4") ?? "Please ensure you are running a compatible ROM set.";
        var message5 = (string)Application.Current.TryFindResource("MameUnableToLoadImageError5") ?? "Would you like to visit the PleasureDome website to download a compatible ROM set?";
        var result = await _messageDialog.ShowYesNoAsync($"{message1}\n\n" +
                                                         $"{message2}\n\n" +
                                                         $"{message3}\n\n" +
                                                         $"{message4}\n\n" +
                                                         $"{message5}", title);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = App.ServiceProvider.GetRequiredService<IConfiguration>().GetValue<string>("Urls:PleasureDomeWebsite") ?? "https://pleasuredome.github.io/pleasuredome/index.html",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                await _messageDialog.ShowErrorAsync($"Could not open browser: {ex.Message}", "Error");
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Could not open browser");
            }
        }
    }

    public Task OotakeDoesNotSupportImageFilesMessageBox()
    {
        var title = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var message = (string)Application.Current.TryFindResource("OotakeemulatordoesnotsupportCHD") ?? "Ootake emulator does not support CHD, ISO, CUE/BIN files.";
        return _messageDialog.ShowErrorAsync(message, title);
    }

    public async Task GeolithDoesNotSupportCompressedFilesMessageBox()
    {
        var title = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var message1 = (string)Application.Current.TryFindResource("GeolithLibretroDllDoesNotSupportZIP1") ?? "'geolith_libretro.dll' does not support ZIP, 7Z or RAR files.";
        var message2 = (string)Application.Current.TryFindResource("GeolithLibretroDllDoesNotSupportZIP2") ?? "It only support NEO files.";
        var message3 = (string)Application.Current.TryFindResource("GeolithLibretroDllDoesNotSupportZIP3") ?? "Please ensure you are running a compatible ROM set.";
        var message4 = (string)Application.Current.TryFindResource("GeolithLibretroDllDoesNotSupportZIP4") ?? "Would you like to visit the url 'wiki.terraonion.com' to get more info about that?";
        var result = await _messageDialog.ShowYesNoAsync($"{message1}\n\n" +
                                                         $"{message2}\n\n" +
                                                         $"{message3}\n\n" +
                                                         $"{message4}", title);

        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = "https://wiki.terraonion.com/index.php/Neobuilder_Guide",
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                await _messageDialog.ShowErrorAsync($"Could not open browser: {ex.Message}", "Error");
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Could not open browser");
            }
        }
    }

    public Task RetroArchParameterShouldContainLMessageBox()
    {
        var title = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var message = (string)Application.Current.TryFindResource("RetroArchParameterShouldContainL") ?? "The RetroArch parameter should contain -L to properly point to the desired core.";
        var message2 = (string)Application.Current.TryFindResource("EditthissysteminExpertModeandfixtheparameter") ?? "Edit this system in 'Expert Mode' and fix the parameter field for this emulator.";
        return _messageDialog.ShowErrorAsync($"{message}\n\n" +
                                             $"{message2}", title);
    }

    public async Task RetroArchParameterIssueMessageBox(string logPath)
    {
        var title = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var message = (string)Application.Current.TryFindResource("RetroArchParameterIssue") ?? "RetroArch could not launch your game.";
        var message2 = (string)Application.Current.TryFindResource("RetroArchParameterIssue2") ?? "99% of the launch failures are due to incorrect parameters.";
        var message3 = (string)Application.Current.TryFindResource("RetroArchParameterIssue3") ?? "Go back to 'Expert Mode' and double-check the parameter field for this emulator. Double-check the path to the desired core. Read the recommendations from the 'Simple Launcher' developer for the specific system.";
        var message4 = (string)Application.Current.TryFindResource("RetroArchParameterIssue4") ?? "Check the core requirements to run it. Some cores require a BIOS file to work. Read the core documentation to figure out what the requirements are for that specific core.";
        var doyouwanttoopenthefileerroruserlog = (string)Application.Current.TryFindResource("Doyouwanttoopenthefileerroruserlog") ?? "Do you want to open the file 'error_user.log' to debug the error?";
        var result = await _messageDialog.ShowYesNoAsync($"{message}\n\n" +
                                                         $"{message2}\n\n" +
                                                         $"{message3}\n\n" +
                                                         $"{message4}\n\n" +
                                                         $"{doyouwanttoopenthefileerroruserlog}", title);
        if (result)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = logPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                App.ServiceProvider.GetRequiredService<ILogErrors>().LogAndForget(ex, "Failed to open the error log file from a message box.");
                // Notify user
                var thefileerroruserlogwas = (string)Application.Current.TryFindResource("Thefileerroruserlogwas") ?? "The file 'error_user.log' was not found!";
                await _messageDialog.ShowErrorAsync(thefileerroruserlogwas, title);
            }
        }
    }

    public Task RetroArchSpecialCharactersInPathMessageBox()
    {
        var title = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var message = (string)Application.Current.TryFindResource("RetroArchSpecialCharactersInPath1") ?? "The emulator could not launch the game because the file path contains special characters (for example: ´, `, ~, !, ?).";
        var message2 = (string)Application.Current.TryFindResource("RetroArchSpecialCharactersInPath2") ?? "RetroArch cannot create its required folders in paths with these characters.";
        var message3 = (string)Application.Current.TryFindResource("RetroArchSpecialCharactersInPath3") ?? "To fix this, please move your emulator and your game files to a folder that uses only standard letters and numbers, such as C:\\Games\\.";
        return _messageDialog.ShowErrorAsync($"{message}\n\n" +
                                             $"{message2}\n\n" +
                                             $"{message3}", title);
    }

    public Task AzaharConfigurationInjectionPermissionErrorMessageBox()
    {
        var title = (string)Application.Current.TryFindResource("InjectionFailed") ?? "Injection Failed";
        var message1 = (string)Application.Current.TryFindResource("AzaharConfigPermissionError1") ?? "Failed to inject Azahar configuration. The emulator is installed in a protected system directory.";
        var message2 = (string)Application.Current.TryFindResource("AzaharConfigPermissionError2") ?? "The configuration file could not be modified due to insufficient permissions.";
        var message3 = (string)Application.Current.TryFindResource("AzaharConfigPermissionError3") ?? "To fix this, either:";
        var message4 = (string)Application.Current.TryFindResource("AzaharConfigPermissionError4") ?? "1. Run Simple Launcher as administrator, or";
        var message5 = (string)Application.Current.TryFindResource("AzaharConfigPermissionError5") ?? "2. Install Azahar in a user directory (e.g., C:\\Users\\YourName\\Azahar)";
        var message6 = (string)Application.Current.TryFindResource("AzaharConfigPermissionError6") ?? "The game will launch with the emulator's default settings.";
        return _messageDialog.ShowWarningAsync($"{message1}\n\n" +
                                               $"{message2}\n\n" +
                                               $"{message3}\n" +
                                               $"{message4}\n" +
                                               $"{message5}\n\n" +
                                               $"{message6}", title);
    }

    public Task AzaharConfigurationSavedSuccessfullyMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("AzaharConfigurationSavedSuccessfully") ?? "Azahar configuration saved successfully.";
        var message2 = (string)Application.Current.TryFindResource("Success") ?? "Success";
        return _messageDialog.ShowInfoAsync(message1, message2);
    }

    public Task FailedToSaveAzaharConfigurationMessageBox()
    {
        var message1 = (string)Application.Current.TryFindResource("FailedToSaveAzaharConfiguration") ?? "Failed to save Azahar configuration. Please check file permissions.";
        var message2 = (string)Application.Current.TryFindResource("SaveFailed") ?? "Save Failed";
        return _messageDialog.ShowErrorAsync(message1, message2);
    }

    public Task XemuParameterShouldContainDvdPathMessageBox()
    {
        var title = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var message = (string)Application.Current.TryFindResource("XemuParameterShouldContainDvdPath") ?? "The Xemu parameter should contain '-dvd_path'.";
        var message2 = (string)Application.Current.TryFindResource("EditthissysteminExpertModeandfixtheparameter") ?? "Edit this system in 'Expert Mode' and fix the parameter field for this emulator.";
        return _messageDialog.ShowErrorAsync($"{message}\n\n" +
                                             $"{message2}", title);
    }

    public Task PleaseExtractApplicationFirstMessageBox()
    {
        var title = (string)Application.Current.TryFindResource("Error") ?? "Error";
        var message = (string)Application.Current.TryFindResource("SimpleLaunchercannotrunfromatemporary") ?? "'Simple Launcher' cannot run from a temporary folder.";
        var message2 = (string)Application.Current.TryFindResource("Pleaseextracttheapplicationtoapermanentfolder") ?? "Please extract the application to a permanent folder before running it.";
        return _messageDialog.ShowErrorAsync($"{message}\n\n" +
                                             $"{message2}", title);
    }

    public Task InjectionFailedGenericMessageBox()
    {
        var errorMessage = (string)Application.Current.TryFindResource("Failedtoinjectconfiguration") ?? "Failed to inject configuration. The error has been logged to the developer.";
        var errorTitle = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(errorMessage, errorTitle);
    }

    public Task DaphneConfigurationSaveFailedMessageBox()
    {
        var errorMessage = (string)Application.Current.TryFindResource("Failedtosaveconfiguration") ?? "Failed to save configuration. The error has been logged to the developer.";
        var errorTitle = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync(errorMessage, errorTitle);
    }

    public Task ShowImageDownloadTimeoutMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SystemNameRequiredBeforeChoosingImageMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("SystemNameRequiredBeforeChoosingImage") ?? "Please enter a system name before choosing an image.";
        var title = (string)Application.Current.TryFindResource("SystemNameRequired") ?? "System Name Required";
        return _messageDialog.ShowInfoAsync(message, title);
    }

    public Task InvalidImageFormatMessageBox()
    {
        var message = (string)Application.Current.TryFindResource("InvalidImageFormat") ?? "Only PNG, JPG, and JPEG images are supported.";
        var title = (string)Application.Current.TryFindResource("InvalidImageFormatTitle") ?? "Invalid Image Format";
        return _messageDialog.ShowWarningAsync(message, title);
    }

    public Task FailedToCopySystemImageMessageBox(string errorMessage)
    {
        var baseMessage = (string)Application.Current.TryFindResource("FailedToCopySystemImage") ?? "Failed to copy the image:";
        var title = (string)Application.Current.TryFindResource("Error") ?? "Error";
        return _messageDialog.ShowErrorAsync($"{baseMessage} {errorMessage}", title);
    }

    public Task WarningMessageBox(string message)
    {
        var title = (string)Application.Current.TryFindResource("Warning") ?? "Warning";
        return _messageDialog.ShowWarningAsync(message, title);
    }

    public Task CustomErrorMessageBox(string message, string title)
    {
        return _messageDialog.ShowErrorAsync(message, title);
    }

    public async Task<bool> CustomQuestionMessageBox(string title, string message)
    {
        var result = await _messageDialog.ShowYesNoAsync(message, title);
        return result;
    }
}