using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Avalonia.Services;

public class AvaloniaMessageBoxLibraryService : IMessageBoxLibraryService
{
    private readonly IMessageDialogService _messageDialog;
    private readonly IResourceProvider _resources;

    public AvaloniaMessageBoxLibraryService(IMessageDialogService messageDialog, IResourceProvider resources)
    {
        _messageDialog = messageDialog;
        _resources = resources;
    }

    private string R(string key, string fallback) => _resources.GetString(key, fallback);

    public Task TakeScreenShotMessageBox() =>
        _messageDialog.ShowInfoAsync(
            $"{R("Thegamewilllaunchnow", "The game will launch now.")}\n\n" +
            $"{R("Setthegamewindowto", "Set the game window to non-fullscreen. This is important.")}\n\n" +
            $"{R("Youshouldchangetheemulatorparameters", "You should change the emulator parameters to prevent the emulator from starting in fullscreen.")}\n\n" +
            $"{R("AselectionwindowwillopeninSimpleLauncherallowingyou", "A selection window will open in 'Simple Launcher', allowing you to choose the desired window to capture.")}\n\n" +
            $"{R("assoonasyouselectawindow", "As soon as you select a window, a screenshot will be taken and saved in the image folder of the selected system.")}",
            R("TakeScreenshot", "Take Screenshot"));

    public Task CouldNotSaveScreenshotMessageBox() =>
        _messageDialog.ShowErrorAsync(
            $"{R("Failedtosavescreenshot", "Failed to save screenshot.")}\n\n{R("Theerrorwasreportedtothedeveloper", "The error was reported to the developer who will try to fix the issue.")}",
            R("Error", "Error"));

    public Task GameIsAlreadyInFavoritesMessageBox(string fileNameWithExtension) =>
        _messageDialog.ShowInfoAsync($"{fileNameWithExtension} {R("isalreadyinfavorites", "is already in favorites.")}", R("Info", "Info"));

    public Task ErrorWhileAddingFavoritesMessageBox() =>
        _messageDialog.ShowErrorAsync(
            $"{R("Anerroroccurredwhileaddingthisgame", "An error occurred while adding this game to the favorites.")}\n\n{R("Theerrorwasreportedtothedeveloper", "The error was reported to the developer who will try to fix the issue.")}",
            R("Error", "Error"));

    public Task ErrorWhileRemovingGameFromFavoriteMessageBox() =>
        _messageDialog.ShowErrorAsync(
            $"{R("Anerroroccurredwhileremoving", "An error occurred while removing this game from favorites.")}\n\n{R("Theerrorwasreportedtothedeveloper", "The error was reported to the developer who will try to fix the issue.")}",
            R("Error", "Error"));

    public Task ErrorOpeningTheUpdateHistoryWindowMessageBox() =>
        _messageDialog.ShowErrorAsync(
            $"{R("ErroropeningtheUpdateHistorywindow", "Error opening the Update History window.")}\n\n{R("Theerrorwasreportedtothedeveloper", "The error was reported to the developer who will try to fix the issue.")}",
            R("Error", "Error"));

    public Task ErrorOpeningVideoLinkMessageBox() =>
        _messageDialog.ShowErrorAsync(
            $"{R("TherewasaproblemopeningtheVideo", "There was a problem opening the Video Link.")}\n\n{R("Ensureyouhaveadefaultbrowserinstalled", "Ensure you have a default browser installed and configured correctly on your system.")}",
            R("Error", "Error"));

    public Task ProblemOpeningInfoLinkMessageBox() =>
        _messageDialog.ShowErrorAsync(
            $"{R("Therewasaproblemopeningtheinfolink", "There was a problem opening the Info Link.")}\n\n{R("Ensureyouhaveadefaultbrowserinstalled", "Ensure you have a default browser installed and configured correctly on your system.")}",
            R("Error", "Error"));

    public Task ErrorOpeningUrlMessageBox() =>
        _messageDialog.ShowErrorAsync(
            $"{R("TherewasaproblemopeningtheURL", "There was a problem opening the URL.")}\n\n{R("Ensureyouhaveadefaultbrowserinstalled", "Ensure you have a default browser installed and configured correctly on your system.")}",
            R("Error", "Error"));

    public Task ThereIsNoCoverMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Thereisnocoverimageavailable", "There is no cover image available."), R("Info", "Info"));

    public Task ThereIsNoTitleSnapshotMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Thereisnotitlesnapshotavailable", "There is no title snapshot available."), R("Info", "Info"));

    public Task ThereIsNoGameplaySnapshotMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Thereisnogameplaysnapshotavailable", "There is no gameplay snapshot available."), R("Info", "Info"));

    public Task ThereIsNoCartMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Thereisnocartimageavailable", "There is no cart image available."), R("Info", "Info"));

    public Task ThereIsNoVideoFileMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Thereisnovideofileavailable", "There is no video file available."), R("Info", "Info"));

    public Task CouldNotOpenManualMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Couldnotopenthemanual", "Could not open the manual."), R("Error", "Error"));

    public Task NoPdfViewerInstalledMessageBox() =>
        _messageDialog.ShowErrorAsync(R("NoPDFviewerinstalled", "No PDF viewer is installed on this system."), R("Error", "Error"));

    public Task ThereIsNoManualMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Thereisnomanualavailable", "There is no manual available."), R("Info", "Info"));

    public Task ThereIsNoWalkthroughMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Thereisnowalkthroughavailable", "There is no walkthrough available."), R("Info", "Info"));

    public Task ThereIsNoCabinetMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Thereisnocabinetimageavailable", "There is no cabinet image available."), R("Info", "Info"));

    public Task ThereIsNoFlyerMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Thereisnoflyerimageavailable", "There is no flyer image available."), R("Info", "Info"));

    public Task ThereIsNoPcbMessageBox() =>
        _messageDialog.ShowInfoAsync(R("ThereisnoPCBimageavailable", "There is no PCB image available."), R("Info", "Info"));

    public Task FileSuccessfullyDeletedMessageBox(string fileNameWithExtension) =>
        _messageDialog.ShowInfoAsync($"{fileNameWithExtension} {R("wassuccessfullydeleted", "was successfully deleted.")}", R("Info", "Info"));

    public Task FileCouldNotBeDeletedMessageBox(string fileNameWithExtension) =>
        _messageDialog.ShowErrorAsync($"{R("Thefile", "The file")} {fileNameWithExtension} {R("couldnotbedeleted", "could not be deleted.")}", R("Error", "Error"));

    public Task DefaultImageNotFoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Defaultimagenotfound", "Default image not found."), R("Error", "Error"));

    public Task GlobalSearchErrorMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Anerroroccurredduringthesearch", "An error occurred during the search."), R("Error", "Error"));

    public Task PleaseEnterSearchTermMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Pleaseenterasearchterm", "Please enter a search term."), R("Info", "Info"));

    public Task ErrorLaunchingGameMessageBox(string logPath) =>
        _messageDialog.ShowErrorAsync($"{R("Errorlaunchingthegame", "Error launching the game.")}\n\n{R("Checkthelogfile", "Check the log file for details.")}: {logPath}", R("Error", "Error"));

    public Task SelectAGameToLaunchMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Selectagametolaunch", "Select a game to launch."), R("Info", "Info"));

    public Task FileAddedToFavoritesMessageBox(string fileNameWithoutExtension) =>
        _messageDialog.ShowInfoAsync($"{fileNameWithoutExtension} {R("wasaddedtofavorites", "was added to favorites.")}", R("Info", "Info"));

    public Task FileRemovedFromFavoritesMessageBox(string fileNameWithoutExtension) =>
        _messageDialog.ShowInfoAsync($"{fileNameWithoutExtension} {R("wasremovedfromfavorites", "was removed from favorites.")}", R("Info", "Info"));

    public Task CouldNotLaunchThisGameMessageBox(string logPath) =>
        _messageDialog.ShowErrorAsync($"{R("Couldnotlaunchthisgame", "Could not launch this game.")}\n\n{R("Checkthelogfile", "Check the log file for details.")}: {logPath}", R("Error", "Error"));

    public Task ProtocolHandlerNotRegisteredMessageBox(string protocol) =>
        _messageDialog.ShowWarningAsync($"{R("Theprotocolhandler", "The protocol handler")} '{protocol}' {R("isnotregistered", "is not registered.")}", R("Warning", "Warning"));

    public Task EmulatorPathNotConfiguredMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Emulatorpathnotconfigured", "Emulator path is not configured."), R("Warning", "Warning"));

    public Task ErrorCalculatingStatsMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Errorcalculatingstatistics", "Error calculating statistics."), R("Error", "Error"));

    public Task FailedSaveReportMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtosavethebugreport", "Failed to save the bug report."), R("Error", "Error"));

    public Task ReportSavedMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Reportsavedsuccessfully", "Report saved successfully."), R("Info", "Info"));

    public Task NoStatsToSaveMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Nostatisticstosave", "No statistics to save."), R("Info", "Info"));

    public Task ErrorLaunchingToolMessageBox(string logPath) =>
        _messageDialog.ShowErrorAsync($"{R("Errorlaunchingthetool", "Error launching the tool.")}\n\n{R("Checkthelogfile", "Check the log file for details.")}: {logPath}", R("Error", "Error"));

    public Task SelectedToolNotFoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Selectedtoolnotfound", "Selected tool not found."), R("Error", "Error"));

    public Task ErrorMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Anerroroccurred", "An error occurred."), R("Error", "Error"));

    public Task NoFavoriteFoundMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Nofavoritesfound", "No favorites found."), R("Info", "Info"));

    public Task MoveToWritableFolderMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Movetowritablefolder", "Please move the application to a writable folder."), R("Warning", "Warning"));

    public Task InvalidSystemConfigMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Invalidsystemconfiguration", "Invalid system configuration."), R("Error", "Error"));

    public Task ErrorMethodLoadGameFilesAsyncMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Errorloadinggamefiles", "Error loading game files."), R("Error", "Error"));

    public Task ErrorOpeningDonationLinkMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Erroropeningdonationlink", "Error opening donation link."), R("Error", "Error"));

    public Task ToggleGamepadFailureMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Togglegamepadfailure", "Failed to toggle gamepad."), R("Error", "Error"));

    public Task ToolLaunchWasCanceledByUserMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Toolaunchcanceled", "Tool launch was canceled by the user."), R("Info", "Info"));

    public Task ErrorChangingViewModeMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Errorchangingviewmode", "Error changing view mode."), R("Error", "Error"));

    public Task NavigationButtonErrorMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Navigationbuttonerror", "Navigation button error."), R("Error", "Error"));

    public Task SelectSystemBeforeSearchMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Selectsystembeforesearch", "Please select a system before searching."), R("Info", "Info"));

    public Task EnterSearchQueryMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Entersearchquery", "Please enter a search query."), R("Info", "Info"));

    public Task ErrorWhileLoadingHelpUserXmlMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Errorloadinghelpuserxml", "Error loading help user XML."), R("Error", "Error"));

    public Task NoSystemInHelpUserXmlMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Nosysteminhelpuserxml", "No system found in help user XML."), R("Info", "Info"));

    public Task<MessageBoxResult> CouldNotLoadHelpUserXmlMessageBox() =>
        _messageDialog.ShowAsync(R("Couldnotloadhelpuserxml", "Could not load help user XML."), R("Error", "Error"), MessageBoxButton.Ok, MessageBoxImage.Error);

    public Task FailedToLoadHelpUserXmlMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoloadhelpuserxml", "Failed to load help user XML."), R("Error", "Error"));

    public Task FileHelpUserXmlIsMissingMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Filehelpuserxmlismissing", "Help user XML file is missing."), R("Error", "Error"));

    public Task ErrorWhileLoadingParametersMdMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Errorloadingparametersmd", "Error loading parameters.md."), R("Error", "Error"));

    public Task NoSystemInParametersMdMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Nosysteminparametersmd", "No system found in parameters.md."), R("Info", "Info"));

    public Task FailedToLoadParametersMdMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoloadparametersmd", "Failed to load parameters.md."), R("Error", "Error"));

    public Task FileParametersMdIsMissingMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Fileparametersmdismissing", "Parameters.md file is missing."), R("Error", "Error"));

    public Task FileParametersMdIsEmptyMessageBox() =>
        _messageDialog.ShowErrorAsync(R("FileparametersmDisempty", "Parameters.md file is empty."), R("Error", "Error"));

    public Task ImageViewerErrorMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Imageviewererror", "Image viewer error."), R("Error", "Error"));

    public Task ReinstallSimpleLauncherFileCorruptedMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Reinstallfilecorrupted", "SimpleLauncher file is corrupted. Please reinstall."), R("Error", "Error"));

    public Task ReinstallSimpleLauncherFileMissingMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Reinstallfilemissing", "SimpleLauncher file is missing. Please reinstall."), R("Error", "Error"));

    public Task ErrorCheckingForUpdatesMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Errorcheckingforupdates", "Error checking for updates."), R("Error", "Error"));

    public Task ErrorLoadingRomHistoryMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Errorloadingromhistory", "Error loading ROM history."), R("Error", "Error"));

    public Task NoHistoryXmlOrDatFoundMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Nohistoryxmlordatfound", "No history XML or DAT file found."), R("Info", "Info"));

    public Task ErrorOpeningBrowserMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Erroropeningbrowser", "Error opening browser."), R("Error", "Error"));

    public Task SystemXmlIsCorruptedMessageBox(string logPath) =>
        _messageDialog.ShowErrorAsync($"{R("Systemxmliscorrupted", "System XML is corrupted.")}\n\n{logPath}", R("Error", "Error"));

    public Task WouldYouLikeToOpenTheLogMessageBox(string logPath) =>
        _messageDialog.ShowConfirmAsync($"{R("Wouldyouliketoopenthelog", "Would you like to open the log file?")}\n\n{logPath}", R("Info", "Info"));

    public Task FileSystemXmlIsCorruptedMessageBox(string logPath) =>
        _messageDialog.ShowErrorAsync($"{R("Filesystemxmliscorrupted", "File system XML is corrupted.")}\n\n{logPath}", R("Error", "Error"));

    public Task InstallUpdateManuallyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Installupdateanually", "Please install the update manually."), R("Info", "Info"));

    public Task UpdaterLaunchFailedMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Updaterlaunchfailed", "Updater launch failed."), R("Error", "Error"));

    public Task RequiredFileMissingMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Requiredfilemissing", "Required file is missing."), R("Error", "Error"));

    public Task EnterSupportRequestMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Entersupportrequest", "Please enter a support request."), R("Info", "Info"));

    public Task EnterNameMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Entername", "Please enter your name."), R("Info", "Info"));

    public Task EnterEmailMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Enteremail", "Please enter your email."), R("Info", "Info"));

    public Task ApiKeyErrorMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Apikeyerror", "API key error."), R("Error", "Error"));

    public Task SupportRequestSuccessMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Supportrequestsuccess", "Support request sent successfully."), R("Info", "Info"));

    public Task SupportRequestSendErrorMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Supportrequestsenderror", "Error sending support request."), R("Error", "Error"));

    public Task ExtractionFailedMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Extractionfailed", "Extraction failed."), R("Error", "Error"));

    public Task FileNeedToBeCompressedMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Fileneedstobecompressed", "The file needs to be compressed."), R("Warning", "Warning"));

    public Task DownloadedFileIsMissingMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Downloadedfileismissing", "Downloaded file is missing."), R("Error", "Error"));

    public Task FileIsLockedMessageBox(string tempFolderPath) =>
        _messageDialog.ShowErrorAsync($"{R("Fileislocked", "File is locked.")}\n\n{tempFolderPath}", R("Error", "Error"));

    public Task LinksSavedMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Linkssaved", "Links saved successfully."), R("Info", "Info"));

    public Task DeadZonesSavedMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Deadzonessaved", "Dead zones saved successfully."), R("Info", "Info"));

    public Task LinksRevertedMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Linksreverted", "Links reverted to defaults."), R("Info", "Info"));

    public Task MainWindowSearchEngineErrorMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Searchengineerror", "Search engine error."), R("Error", "Error"));

    public Task DownloadExtractionFailedMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Downloadextractionfailed", "Download extraction failed."), R("Error", "Error"));

    public Task DownloadAndExtrationWereSuccessfulMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Downloadandextractionweresuccessful", "Download and extraction were successful."), R("Info", "Info"));

    public Task ShowEmulatorDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem) =>
        _messageDialog.ShowErrorAsync($"{R("Emulatordownloaderror", "Error downloading emulator for")} {selectedSystem.SystemName}.", R("Error", "Error"));

    public Task ShowCoreDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem) =>
        _messageDialog.ShowErrorAsync($"{R("Coredownloaderror", "Error downloading core for")} {selectedSystem.SystemName}.", R("Error", "Error"));

    public Task ShowImagePackDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem) =>
        _messageDialog.ShowErrorAsync($"{R("Imagepackdownloaderror", "Error downloading image pack for")} {selectedSystem.SystemName}.", R("Error", "Error"));

    public Task SelectAHistoryItemToRemoveMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Selectahistoryitemtoremove", "Select a history item to remove."), R("Info", "Info"));

    public Task<MessageBoxResult> ReallyWantToRemoveAllPlayHistoryMessageBox() =>
        _messageDialog.ShowAsync(R("Reallywanttoremoveallplayhistory", "Do you really want to remove all play history?"), R("Confirm", "Confirm"), MessageBoxButton.YesNo, MessageBoxImage.Question);

    public Task SystemAddedMessageBox(string systemName, string resolvedSystemFolder, string resolvedSystemImageFolder) =>
        _messageDialog.ShowInfoAsync($"{R("System", "System")} '{systemName}' {R("wasaddedsuccessfully", "was added successfully.")}", R("Info", "Info"));

    public Task AddSystemFailedMessageBox(string? details = null) =>
        _messageDialog.ShowErrorAsync($"{R("Addsystemfailed", "Failed to add system.")}\n\n{details}", R("Error", "Error"));

    public Task RightClickContextMenuErrorMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Rightclickcontextmenuerror", "Right-click context menu error."), R("Error", "Error"));

    public Task GameFileDoesNotExistMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Gamefiledoesnotexist", "Game file does not exist."), R("Error", "Error"));

    public Task<MessageBoxResult> GameFileDoesNotExistAskToDeleteMessageBox(string filePath) =>
        _messageDialog.ShowAsync($"{R("Gamefiledoesnotexist", "Game file does not exist.")}\n\n{filePath}\n\n{R("Wouldyouliketoremoveit", "Would you like to remove it from the list?")}", R("Confirm", "Confirm"), MessageBoxButton.YesNo, MessageBoxImage.Question);

    public Task<MessageBoxResult> FavoriteFileDoesNotExistAskToDeleteMessageBox(string filePath) =>
        _messageDialog.ShowAsync($"{R("Favoritefiledoesnotexist", "Favorite file does not exist.")}\n\n{filePath}\n\n{R("Wouldyouliketoremoveit", "Would you like to remove it from the list?")}", R("Confirm", "Confirm"), MessageBoxButton.YesNo, MessageBoxImage.Question);

    public Task CouldNotOpenHistoryWindowMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Couldnotopenhistorywindow", "Could not open history window."), R("Error", "Error"));

    public Task CouldNotOpenWalkthroughMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Couldnotopenwalkthrough", "Could not open walkthrough."), R("Error", "Error"));

    public Task SelectAFavoriteToRemoveMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Selectafavoritetoremove", "Select a favorite to remove."), R("Info", "Info"));

    public Task SystemXmlNotFoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Systemxmlnotfound", "System XML not found."), R("Error", "Error"));

    public Task YouCanAddANewSystemMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Youcanaddanewsystem", "You can add a new system."), R("Info", "Info"));

    public Task EmulatorNameRequiredMessageBox(int i) =>
        _messageDialog.ShowWarningAsync($"{R("Emulatornameisrequired", "Emulator name is required for emulator")} {i}.", R("Warning", "Warning"));

    public Task EmulatorNameIsRequiredMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Emulatornameisrequired", "Emulator name is required."), R("Warning", "Warning"));

    public Task EmulatorNameMustBeUniqueMessageBox(string emulatorName) =>
        _messageDialog.ShowWarningAsync($"{R("Emulatornamemustbeunique", "Emulator name must be unique:")} '{emulatorName}'.", R("Warning", "Warning"));

    public Task SystemSavedSuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Systemsavedsuccessfully", "System saved successfully."), R("Info", "Info"));

    public Task PathOrParameterInvalidMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Pathorparameterinvalid", "Path or parameter is invalid."), R("Warning", "Warning"));

    public Task Emulator1RequiredMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Emulator1isrequired", "Emulator 1 is required."), R("Warning", "Warning"));

    public Task ExtensionToLaunchIsRequiredMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Extensiontolaunchisrequired", "Extension to launch is required."), R("Warning", "Warning"));

    public Task ExtensionToSearchIsRequiredMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Extensiontosearchisrequired", "Extension to search is required."), R("Warning", "Warning"));

    public Task FileMustBeCompressedMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Filemustbecompressed", "File must be compressed."), R("Warning", "Warning"));

    public Task SystemImageFolderCanNotBeEmptyMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Systemimagefoldercannotbeempty", "System image folder cannot be empty."), R("Warning", "Warning"));

    public Task SystemFolderCanNotBeEmptyMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Systemfoldercannotbeempty", "System folder cannot be empty."), R("Warning", "Warning"));

    public Task SystemNameCanNotBeEmptyMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Systemnamecannotbeempty", "System name cannot be empty."), R("Warning", "Warning"));

    public Task InvalidSystemNameCharactersMessageBox(string invalidChars) =>
        _messageDialog.ShowWarningAsync($"{R("Invalidsystemnamecharacters", "Invalid system name characters:")} {invalidChars}", R("Warning", "Warning"));

    public Task InvalidFolderCharactersMessageBox(string invalidChars) =>
        _messageDialog.ShowWarningAsync($"{R("Invalidfoldercharacters", "Invalid folder characters:")} {invalidChars}", R("Warning", "Warning"));

    public Task FolderCreationFailedMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Foldercreationfailed", "Folder creation failed."), R("Error", "Error"));

    public Task SelectASystemToDeleteMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Selectasystemtodelete", "Select a system to delete."), R("Info", "Info"));

    public Task SystemNotFoundInTheXmlMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Systemnotfoundinthexml", "System not found in the XML."), R("Error", "Error"));

    public Task ErrorFindingGameFilesMessageBox(string logPath) =>
        _messageDialog.ShowErrorAsync($"{R("Errorfindinggamefiles", "Error finding game files.")}\n\n{logPath}", R("Error", "Error"));

    public Task GamePadErrorMessageBox(string logPath) =>
        _messageDialog.ShowErrorAsync($"{R("Gamepaderror", "Gamepad error.")}\n\n{logPath}", R("Error", "Error"));

    public Task CouldNotLaunchGameMessageBox(string logPath) =>
        _messageDialog.ShowErrorAsync($"{R("Couldnotlaunchgame", "Could not launch game.")}\n\n{logPath}", R("Error", "Error"));

    public Task InvalidOperationExceptionMessageBox(string logPath) =>
        _messageDialog.ShowErrorAsync($"{R("Invalidoperationexception", "Invalid operation exception.")}\n\n{logPath}", R("Error", "Error"));

    public Task ThereWasAnErrorLaunchingThisGameMessageBox(string logPath) =>
        _messageDialog.ShowErrorAsync($"{R("Therewasanerrorlaunchingthisgame", "There was an error launching this game.")}\n\n{logPath}", R("Error", "Error"));

    public Task BatchFileFailedMessageBox(string batchFilePath, string errorDetail, string logPath, int? exitCode = null) =>
        _messageDialog.ShowErrorAsync($"{R("Batchfilefailed", "Batch file failed.")}\n\n{batchFilePath}\n\n{errorDetail}", R("Error", "Error"));

    public Task<bool> BatchFilePathsMissingMessageBox(List<string> missingPaths) =>
        _messageDialog.ShowYesNoAsync($"{R("Batchfilepathsmissing", "Batch file paths are missing:")}\n\n{string.Join("\n", missingPaths)}", R("Warning", "Warning"));

    public Task ElevationRequiredMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Elevationrequired", "This operation requires elevation (run as administrator)."), R("Warning", "Warning"));

    public Task NullFileExtensionMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Nullfileextension", "File extension is null."), R("Error", "Error"));

    public Task CouldNotFindAFileMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Couldnotafile", "Could not find a file."), R("Error", "Error"));

    public Task<MessageBoxResult> SearchOnlineForRomHistoryMessageBox() =>
        _messageDialog.ShowAsync(R("Searchonlineforromhistory", "Would you like to search online for ROM history?"), R("Confirm", "Confirm"), MessageBoxButton.YesNo, MessageBoxImage.Question);

    public Task SystemHasBeenDeletedMessageBox(string selectedSystemName) =>
        _messageDialog.ShowInfoAsync($"{R("System", "System")} '{selectedSystemName}' {R("hasbeendeleted", "has been deleted.")}", R("Info", "Info"));

    public Task<MessageBoxResult> AreYouSureDoYouWantToDeleteThisSystemMessageBox() =>
        _messageDialog.ShowAsync(R("Areyousureyouwanttodeletethissystem", "Are you sure you want to delete this system?"), R("Confirm", "Confirm"), MessageBoxButton.YesNo, MessageBoxImage.Question);

    public Task ThereWasAnErrorDeletingTheGameMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Therewasanerrordeletingthegame", "There was an error deleting the game."), R("Error", "Error"));

    public Task ThereWasAnErrorDeletingTheCoverImageMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Therewasanerrordeletingthecoverimage", "There was an error deleting the cover image."), R("Error", "Error"));

    public Task<MessageBoxResult> AreYouSureYouWantToDeleteTheGameMessageBox(string fileNameWithExtension) =>
        _messageDialog.ShowAsync($"{R("Areyousureyouwanttodeletethegame", "Are you sure you want to delete the game?")}\n\n{fileNameWithExtension}", R("Confirm", "Confirm"), MessageBoxButton.YesNo, MessageBoxImage.Question);

    public Task<MessageBoxResult> AreYouSureYouWantToDeleteTheCoverImageMessageBox(string fileNameWithoutExtension) =>
        _messageDialog.ShowAsync($"{R("Areyousureyouwanttodeletethecoverimage", "Are you sure you want to delete the cover image?")}\n\n{fileNameWithoutExtension}", R("Confirm", "Confirm"), MessageBoxButton.YesNo, MessageBoxImage.Question);

    public Task<MessageBoxResult> WoulYouLikeToSaveAReportMessageBox() =>
        _messageDialog.ShowAsync(R("Wouldyouliketosaveareport", "Would you like to save a report?"), R("Confirm", "Confirm"), MessageBoxButton.YesNo, MessageBoxImage.Question);

    public Task SimpleLauncherWasUnableToRestoreBackupMessageBox() =>
        _messageDialog.ShowErrorAsync(R("SimpleLauncherwasunabletorestorebackup", "SimpleLauncher was unable to restore backup."), R("Error", "Error"));

    public Task<MessageBoxResult> WouldYouLikeToRestoreTheLastBackupMessageBox() =>
        _messageDialog.ShowAsync(R("Wouldyouliketorestorethelastbackup", "Would you like to restore the last backup?"), R("Confirm", "Confirm"), MessageBoxButton.YesNo, MessageBoxImage.Question);

    public Task FailedToLoadLanguageResourceMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoloadlanguageresource", "Failed to load language resource."), R("Error", "Error"));

    public Task InvalidSystemConfigurationMessageBox(string errorMessage) =>
        _messageDialog.ShowErrorAsync($"{R("Invalidsystemconfiguration", "Invalid system configuration.")}\n\n{errorMessage}", R("Error", "Error"));

    public Task UnableToOpenLinkMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Unabletoopenlink", "Unable to open link."), R("Error", "Error"));

    public Task NoGameFoundInTheRandomSelectionMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Nogamefoundintherandomselection", "No game found in the random selection."), R("Info", "Info"));

    public Task PleaseSelectASystemBeforeMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Pleaseselectasystembefore", "Please select a system before proceeding."), R("Info", "Info"));

    public Task ToggleFuzzyMatchingFailureMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Togglefuzzymatchingfailure", "Failed to toggle fuzzy matching."), R("Error", "Error"));

    public Task FuzzyMatchingErrorFailToSetThresholdMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Fuzzymatchingerror", "Fuzzy matching error: failed to set threshold."), R("Error", "Error"));

    public Task ListOfErrorsMessageBox(StringBuilder errorMessages) =>
        _messageDialog.ShowErrorAsync($"{R("Listoferrors", "List of errors:")}\n\n{errorMessages}", R("Error", "Error"));

    public Task ThereIsNoUpdateAvailableMessageBox(string currentVersion) =>
        _messageDialog.ShowInfoAsync($"{R("Thereisnoupdateavailable", "There is no update available.")}\n\n{R("Currentversion", "Current version")}: {currentVersion}", R("Info", "Info"));

    public Task AnotherInstanceIsRunningMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Anotherinstanceisrunning", "Another instance is already running."), R("Warning", "Warning"));

    public Task FailedToStartSimpleLauncherMessageBox() =>
        _messageDialog.ShowErrorAsync(R("FailedtostartSimpleLauncher", "Failed to start SimpleLauncher."), R("Error", "Error"));

    public Task FailedToRestartMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtorestart", "Failed to restart."), R("Error", "Error"));

    public Task<MessageBoxResult> DoYouWantToUpdateMessageBox(string currentVersion, string latestVersion) =>
        _messageDialog.ShowAsync($"{R("Doyouwanttoupdate", "Do you want to update?")}\n\n{R("Currentversion", "Current version")}: {currentVersion}\n{R("Latestversion", "Latest version")}: {latestVersion}", R("Update", "Update"), MessageBoxButton.YesNo, MessageBoxImage.Question);

    public Task HandleMissingRequiredFilesMessageBox(string fileList) =>
        _messageDialog.ShowErrorAsync($"{R("Missingrequiredfiles", "Missing required files:")}\n\n{fileList}", R("Error", "Error"));

    public Task HandleApiConfigErrorMessageBox(string reason) =>
        _messageDialog.ShowErrorAsync($"{R("Apiconfigerror", "API configuration error:")}\n\n{reason}", R("Error", "Error"));

    public Task DiskSpaceErrorMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Diskspaceerror", "Insufficient disk space."), R("Error", "Error"));

    public Task CouldNotCheckForDiskSpaceMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Couldnotcheckfordiskspace", "Could not check for disk space."), R("Error", "Error"));

    public Task SaveSystemFailedMessageBox(string? details = null) =>
        _messageDialog.ShowErrorAsync($"{R("Savesystemfailed", "Failed to save system.")}\n\n{details}", R("Error", "Error"));

    public Task CouldNotOpenTheDownloadLinkMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Couldnotopenthedownloadlink", "Could not open the download link."), R("Error", "Error"));

    public Task ErrorLoadingAppSettingsMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Errorloadingappsettings", "Error loading application settings."), R("Error", "Error"));

    public Task PotentialPathManipulationDetectedMessageBox(string archivePath) =>
        _messageDialog.ShowWarningAsync($"{R("Potentialpathmanipulationdetected", "Potential path manipulation detected:")}\n\n{archivePath}", R("Warning", "Warning"));

    public Task CouldNotOpenSoundConfigurationWindowMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Couldnotopensoundconfigurationwindow", "Could not open sound configuration window."), R("Error", "Error"));

    public Task ErrorSettingSoundFileMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Errorsettingsoundfile", "Error setting sound file."), R("Error", "Error"));

    public Task NotificationSoundIsDisableMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Notificationsoundisdisabled", "Notification sound is disabled."), R("Info", "Info"));

    public Task NoSoundFileIsSelectedMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Nosoundfileisselected", "No sound file is selected."), R("Info", "Info"));

    public Task SettingsSavedSuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Settingssavedsuccessfully", "Settings saved successfully."), R("Info", "Info"));

    public Task FailedToSaveSettingsMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtosavesettings", "Failed to save settings."), R("Error", "Error"));

    public Task FilePathIsInvalidMessageBox(string logPath) =>
        _messageDialog.ShowErrorAsync($"{R("Filepathisinvalid", "File path is invalid.")}\n\n{logPath}", R("Error", "Error"));

    public Task ThereWasAnErrorMountingTheFileMessageBox(int? exitCode = null) =>
        _messageDialog.ShowErrorAsync(R("Therewasanerrormountingthefile", "There was an error mounting the file."), R("Error", "Error"));

    public Task DokanDriverNotInstalledMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Dokannotinstalled", "Dokan driver is not installed."), R("Error", "Error"));

    public Task LaunchToolInformationMessageBox(string info) =>
        _messageDialog.ShowInfoAsync(info, R("Info", "Info"));

    public Task CannotScreenshotMinimizedWindowMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Cannotscreenshotminimizedwindow", "Cannot take a screenshot of a minimized window."), R("Warning", "Warning"));

    public Task FailedToCopyLogContentMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtocopylogcontent", "Failed to copy log content."), R("Error", "Error"));

    public Task CouldNotFindUpdaterOnGitHubMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Couldnotfindupdaterongithub", "Could not find updater on GitHub."), R("Error", "Error"));

    public Task CouldNotOpenAchievementsWindowMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Couldnotopenachievementswindow", "Could not open achievements window."), R("Error", "Error"));

    public Task<MessageBoxResult> GameNotSupportedByRetroAchievementsMessageBox() =>
        _messageDialog.ShowAsync(R("Gamenotsupportedbyretroachievements", "This game is not supported by RetroAchievements."), R("Info", "Info"), MessageBoxButton.Ok, MessageBoxImage.Information);

    public Task GameLaunchTimeoutMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Gamelaunchtimeout", "Game launch timed out."), R("Warning", "Warning"));

    public Task AddRaLoginMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Addralogin", "Please add your RetroAchievements login."), R("Info", "Info"));

    public Task NoDefaultBrowserConfiguredMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Nodefaultbrowserconfigured", "No default browser configured."), R("Warning", "Warning"));

    public Task<MessageBoxResult> WarnUserAboutMemoryConsumptionMessageBox() =>
        _messageDialog.ShowAsync(R("Warnuseraboutmemoryconsumption", "This operation may consume a lot of memory. Continue?"), R("Warning", "Warning"), MessageBoxButton.YesNo, MessageBoxImage.Warning);

    public Task GroupByFolderOnlyForMameAndDosBoxMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Groupbyfolderonlyformameanddosbox", "Group by folder is only available for MAME and DOSBox."), R("Info", "Info"));

    public Task<MessageBoxResult> GroupByFolderWarningMessageBox() =>
        _messageDialog.ShowAsync(R("Groupbyfolderwarning", "Group by folder may affect performance. Continue?"), R("Warning", "Warning"), MessageBoxButton.YesNo, MessageBoxImage.Warning);

    public Task<MessageBoxResult> FirstRunWelcomeMessageBox() =>
        _messageDialog.ShowAsync(R("Firstrunwelcome", "Welcome to SimpleLauncher!"), R("Welcome", "Welcome"), MessageBoxButton.Ok, MessageBoxImage.Information);

    public Task Emulator1LocationRequiredMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Emulator1locationrequired", "Emulator 1 location is required."), R("Warning", "Warning"));

    public Task Emulator2LocationRequiredMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Emulator2locationrequired", "Emulator 2 location is required."), R("Warning", "Warning"));

    public Task Emulator3LocationRequiredMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Emulator3locationrequired", "Emulator 3 location is required."), R("Warning", "Warning"));

    public Task Emulator4LocationRequiredMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Emulator4locationrequired", "Emulator 4 location is required."), R("Warning", "Warning"));

    public Task Emulator5LocationRequiredMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Emulator5locationrequired", "Emulator 5 location is required."), R("Warning", "Warning"));

    public Task ImagePackDownloaderUnavailableMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Imagepackdownloaderunavailable", "Image pack downloader is unavailable."), R("Warning", "Warning"));

    public Task EasyModeUnavailableMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Easymodeunavailable", "Easy Mode is unavailable."), R("Warning", "Warning"));

    public Task SimpleLauncherDoesNotSupportRaHashOfSystemGroupedByFolderMessageBox() =>
        _messageDialog.ShowWarningAsync(R("SimpleLauncherdoesnotsupportrahashofsystemgroupedbyfolder", "SimpleLauncher does not support RetroAchievements hashing for systems grouped by folder."), R("Warning", "Warning"));

    public Task UnsupportedArchitectureMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Unsupportedarchitecture", "Unsupported architecture."), R("Error", "Error"));

    public Task SevenZipDllNotFoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Sevenzipdllnotfound", "7z.dll not found."), R("Error", "Error"));

    public Task FailedToInitializeSevenZipMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinitializesevenzip", "Failed to initialize 7-Zip."), R("Error", "Error"));

    public Task ShowExtractionFailedMessageBoxAsync(string tempFolderPath) =>
        _messageDialog.ShowErrorAsync($"{R("Extractionfailed", "Extraction failed.")}\n\n{tempFolderPath}", R("Error", "Error"));

    public Task ShowDownloadFileLockedMessageBoxAsync(string tempFolderPath) =>
        _messageDialog.ShowErrorAsync($"{R("Downloadfilelocked", "Download file is locked.")}\n\n{tempFolderPath}", R("Error", "Error"));

    public Task ShowCustomMessageBox(string message, string launchError, string logPath) =>
        _messageDialog.ShowErrorAsync($"{message}\n\n{launchError}\n\n{logPath}", R("Error", "Error"));

    public Task EnterValidSearchTermsMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Entervalidsearchterms", "Please enter valid search terms."), R("Info", "Info"));

    public Task OperationCancelledMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Operationcancelled", "Operation was cancelled."), R("Info", "Info"));

    public Task<MessageBoxResult> DoYouWantToCancelAndCloseMessageBox() =>
        _messageDialog.ShowAsync(R("Doyouwanttocancelandclose", "Do you want to cancel and close?"), R("Confirm", "Confirm"), MessageBoxButton.YesNo, MessageBoxImage.Question);

    public Task CouldNotOpenBrowserForAiSupportMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Couldnotopenbrowserforaisupport", "Could not open browser for AI support."), R("Error", "Error"));

    public Task PowerShellExecutionPolicyRestrictionsMessageBox() =>
        _messageDialog.ShowErrorAsync(R("PowerShellexecutionpolicyrestrictions", "PowerShell execution policy restrictions."), R("Error", "Error"));

    public Task UnabletomountIsOfileMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Unabletomountisofile", "Unable to mount ISO file."), R("Error", "Error"));

    public Task UnabletoDismountIsOfileMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Unabletodismountisofile", "Unable to dismount ISO file."), R("Error", "Error"));

    public Task ApplicationControlPolicyBlockedMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Applicationcontrolpolicyblocked", "Application control policy blocked this action."), R("Error", "Error"));

    public Task ApplicationControlPolicyBlockedManualLinkMessageBox(string url) =>
        _messageDialog.ShowErrorAsync($"{R("Applicationcontrolpolicyblockedmanuallink", "Application control policy blocked manual link.")}\n\n{url}", R("Error", "Error"));

    public Task EnterYourRetroAchievementsUsernameMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Enteryourretroachievementsusername", "Please enter your RetroAchievements username."), R("Info", "Info"));

    public Task EmulatorConfiguredSuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Emulatorconfiguredsuccessfully", "Emulator configured successfully."), R("Info", "Info"));

    public Task FailedToConfigureTheEmulatorMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoconfiguretheemulator", "Failed to configure the emulator."), R("Error", "Error"));

    public Task AnErrorOccurredWhileConfiguringTheEmulatorMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Anerroroccurredwhileconfiguringtheemulator", "An error occurred while configuring the emulator."), R("Error", "Error"));

    public Task FailedToLoginToRetroAchievementsMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtologintoretroachievements", "Failed to login to RetroAchievements."), R("Error", "Error"));

    public Task FileSystemXmlIsLockedMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Filesystemxmlislocked", "File system XML is locked."), R("Error", "Error"));

    public Task FailedToInjectMameConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectmameconfiguration", "Failed to inject MAME configuration."), R("Error", "Error"));

    public Task MamEconfigurationinjectedsuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Mameconfigurationinjectedsuccessfully", "MAME configuration injected successfully."), R("Info", "Info"));

    public Task FailedtoinjectMamEconfiguration2MessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectmameconfiguration2", "Failed to inject MAME configuration (2)."), R("Error", "Error"));

    public Task MameEmulatorPathNotFoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Mameemulatorpathnotfound", "MAME emulator path not found."), R("Error", "Error"));

    public Task RetroArchemulatorpathnotfoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("RetroArchemulatorpathnotfound", "RetroArch emulator path not found."), R("Error", "Error"));

    public Task FailedtoinjectRetroArchconfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectretroarchconfiguration", "Failed to inject RetroArch configuration."), R("Error", "Error"));

    public Task RetroArchconfigurationinjectedsuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("RetroArchconfigurationinjectedsuccessfully", "RetroArch configuration injected successfully."), R("Info", "Info"));

    public Task FailedtoinjectRetroArchconfiguration2MessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectretroarchconfiguration2", "Failed to inject RetroArch configuration (2)."), R("Error", "Error"));

    public Task XeniaemulatorpathnotfoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Xeniaemulatorpathnotfound", "Xenia emulator path not found."), R("Error", "Error"));

    public Task FailedtoinjectXeniaconfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectxeniaconfiguration", "Failed to inject Xenia configuration."), R("Error", "Error"));

    public Task XeniaconfigurationinjectedsuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Xeniaconfigurationinjectedsuccessfully", "Xenia configuration injected successfully."), R("Info", "Info"));

    public Task FailedtoinjectXeniaconfiguration2MessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectxeniaconfiguration2", "Failed to inject Xenia configuration (2)."), R("Error", "Error"));

    public Task EnterUsernamePasswordMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Enterusernamepassword", "Please enter username and password."), R("Info", "Info"));

    public Task AresemulatornotfoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Aresemulatornotfound", "Ares emulator not found."), R("Error", "Error"));

    public Task DaphnesettingssavedsuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Daphnesettingssavedsuccessfully", "Daphne settings saved successfully."), R("Info", "Info"));

    public Task Pcsx2SettingssavedMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Pcsx2settingssaved", "PCSX2 settings saved."), R("Info", "Info"));

    public Task SettingsSavedMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Settingssaved", "Settings saved."), R("Info", "Info"));

    public Task CemuemulatornotfoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Cemuemulatornotfound", "Cemu emulator not found."), R("Error", "Error"));

    public Task FailedtoinjectAresconfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectaresconfiguration", "Failed to inject Ares configuration."), R("Error", "Error"));

    public Task CemuConfigurationSavedMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Cemuconfigurationsaved", "Cemu configuration saved."), R("Info", "Info"));

    public Task FlycastEmulatorNotFoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Flycastemulatornotfound", "Flycast emulator not found."), R("Error", "Error"));

    public Task AresConfigurationSavedSuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Aresconfigurationsavedsuccessfully", "Ares configuration saved successfully."), R("Info", "Info"));

    public Task FailedToSaveAresConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtosavearesconfiguration", "Failed to save Ares configuration."), R("Error", "Error"));

    public Task FailedToInjectFlycastConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectflycastconfiguration", "Failed to inject Flycast configuration."), R("Error", "Error"));

    public Task FlycastConfigurationSavedSuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Flycastconfigurationsavedsuccessfully", "Flycast configuration saved successfully."), R("Info", "Info"));

    public Task DolphinEmulatorNotFoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Dolphinemulatornotfound", "Dolphin emulator not found."), R("Error", "Error"));

    public Task FailedToSaveFlycastConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtosaveflycastconfiguration", "Failed to save Flycast configuration."), R("Error", "Error"));

    public Task FailedToInjectDolphinConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectdolphinconfiguration", "Failed to inject Dolphin configuration."), R("Error", "Error"));

    public Task DolphinConfigurationSavedSuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Dolphinconfigurationsavedsuccessfully", "Dolphin configuration saved successfully."), R("Info", "Info"));

    public Task FailedToSaveDolphinConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtosavedolphinconfiguration", "Failed to save Dolphin configuration."), R("Error", "Error"));

    public Task SegaModel2EmulatorNotFoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Segamodel2emulatornotfound", "Sega Model 2 emulator not found."), R("Error", "Error"));

    public Task FailedToInjectSegaModel2ConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectsegamodel2configuration", "Failed to inject Sega Model 2 configuration."), R("Error", "Error"));

    public Task SegaModel2ConfigurationSavedSuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Segamodel2configurationsavedsuccessfully", "Sega Model 2 configuration saved successfully."), R("Info", "Info"));

    public Task BlastemEmulatorNotFoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Blastemulatornotfound", "BlastEm emulator not found."), R("Error", "Error"));

    public Task FailedToInjectBlastemConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectblastemconfiguration", "Failed to inject BlastEm configuration."), R("Error", "Error"));

    public Task BlastemConfigurationSavedSuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Blastemconfigurationsavedsuccessfully", "BlastEm configuration saved successfully."), R("Info", "Info"));

    public Task FailedToSaveSegaModel2ConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtosavesegamodel2configuration", "Failed to save Sega Model 2 configuration."), R("Error", "Error"));

    public Task FailedToSaveBlastemConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtosaveblastemconfiguration", "Failed to save BlastEm configuration."), R("Error", "Error"));

    public Task Rpcs3EmulatorNotFoundPleaseLocateMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Rpcs3emulatornotfoundpleaselocate", "RPCS3 emulator not found. Please locate it."), R("Warning", "Warning"));

    public Task FailedToInjectRpcs3ConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectrpcs3configuration", "Failed to inject RPCS3 configuration."), R("Error", "Error"));

    public Task Rpcs3ConfigurationSavedSuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Rpcs3configurationsavedsuccessfully", "RPCS3 configuration saved successfully."), R("Info", "Info"));

    public Task FailedToSaveRpcs3ConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtosaverpcs3configuration", "Failed to save RPCS3 configuration."), R("Error", "Error"));

    public Task StellaEmulatorNotFoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Stellaemulatornotfound", "Stella emulator not found."), R("Error", "Error"));

    public Task FailedToInjectStellaConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectstellaconfiguration", "Failed to inject Stella configuration."), R("Error", "Error"));

    public Task SupermodelEmulatorNotFoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Supermodelemulatornotfound", "Supermodel emulator not found."), R("Error", "Error"));

    public Task StellaConfigurationSavedSuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Stellaconfigurationsavedsuccessfully", "Stella configuration saved successfully."), R("Info", "Info"));

    public Task FailedToInjectSupermodelConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectsupermodelconfiguration", "Failed to inject Supermodel configuration."), R("Error", "Error"));

    public Task FailedToSaveStellaConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtosavestellaconfiguration", "Failed to save Stella configuration."), R("Error", "Error"));

    public Task SupermodelConfigurationSavedSuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Supermodelconfigurationsavedsuccessfully", "Supermodel configuration saved successfully."), R("Info", "Info"));

    public Task FailedToSaveSupermodelConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtosavesupermodelconfiguration", "Failed to save Supermodel configuration."), R("Error", "Error"));

    public Task MednafenEmulatorNotFoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Mednafenemulatornotfound", "Mednafen emulator not found."), R("Error", "Error"));

    public Task MesenEmulatorNotFoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Mesenemulatornotfound", "Mesen emulator not found."), R("Error", "Error"));

    public Task FailedToInjectMednafenConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectmednafenconfiguration", "Failed to inject Mednafen configuration."), R("Error", "Error"));

    public Task FailedToInjectMesenConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectmesenconfiguration", "Failed to inject Mesen configuration."), R("Error", "Error"));

    public Task DuckStationEmulatorNotFoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Duckstationemulatornotfound", "DuckStation emulator not found."), R("Error", "Error"));

    public Task MednafenConfigurationSavedSuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Mednafenconfigurationsavedsuccessfully", "Mednafen configuration saved successfully."), R("Info", "Info"));

    public Task FailedToSaveMednafenConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtosavemednafenconfiguration", "Failed to save Mednafen configuration."), R("Error", "Error"));

    public Task FailedToInjectDuckStationConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectduckstationconfiguration", "Failed to inject DuckStation configuration."), R("Error", "Error"));

    public Task DuckStationConfigurationSavedSuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Duckstationconfigurationsavedsuccessfully", "DuckStation configuration saved successfully."), R("Info", "Info"));

    public Task FailedToSaveMesenConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtosavemesenconfiguration", "Failed to save Mesen configuration."), R("Error", "Error"));

    public Task FailedToSaveDuckStationConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtosaveduckstationconfiguration", "Failed to save DuckStation configuration."), R("Error", "Error"));

    public Task MesenConfigurationSavedSuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Mesenconfigurationsavedsuccessfully", "Mesen configuration saved successfully."), R("Info", "Info"));

    public Task FailedToInjectYumirConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectyumirconfiguration", "Failed to inject Ymir configuration."), R("Error", "Error"));

    public Task YumirConfigurationSavedSuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Yumirconfigurationsavedsuccessfully", "Ymir configuration saved successfully."), R("Info", "Info"));

    public Task RaineSettingsSavedAndInjectedMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Rainesettingssavedandinjected", "Raine settings saved and injected."), R("Info", "Info"));

    public Task RaineExecutableNotFoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Raineexecutablenotfound", "Raine executable not found."), R("Error", "Error"));

    public Task YumirEmulatorNotFoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Yumiremulatornotfound", "Ymir emulator not found."), R("Error", "Error"));

    public Task ReDreamEmulatorPathNotFoundMessageBox() =>
        _messageDialog.ShowErrorAsync(R("ReDreamemulatorpathnotfound", "ReDream emulator path not found."), R("Error", "Error"));

    public Task FailedToInjectReDreamConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtoinjectredreamconfiguration", "Failed to inject ReDream configuration."), R("Error", "Error"));

    public Task ReDreamConfigurationInjectedSuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("ReDreamconfigurationinjectedsuccessfully", "ReDream configuration injected successfully."), R("Info", "Info"));

    public Task CouldNotLaunchGameDueToDepViolationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Couldnotlaunchgameduetodepviolation", "Could not launch game due to DEP violation."), R("Error", "Error"));

    public Task MameRomSetErrorMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Mameromseterror", "MAME ROM set error."), R("Error", "Error"));

    public Task MameUnknownSystemErrorMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Mameunknownsystemerror", "MAME unknown system error."), R("Error", "Error"));

    public Task MameUnableToLoadImageMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Mameunabletoloadimage", "MAME unable to load image."), R("Error", "Error"));

    public Task OotakeDoesNotSupportImageFilesMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Ootakedoesnotsupportimagefiles", "Ootake does not support image files."), R("Warning", "Warning"));

    public Task GeolithDoesNotSupportCompressedFilesMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Geolithdoesnotsupportcompressedfiles", "Geolith does not support compressed files."), R("Warning", "Warning"));

    public Task RetroArchParameterShouldContainLMessageBox() =>
        _messageDialog.ShowWarningAsync(R("RetroArchparametershouldcontainL", "RetroArch parameter should contain -L."), R("Warning", "Warning"));

    public Task RetroArchParameterIssueMessageBox(string logPath) =>
        _messageDialog.ShowWarningAsync($"{R("RetroArchparameterissue", "RetroArch parameter issue.")}\n\n{logPath}", R("Warning", "Warning"));

    public Task RetroArchSpecialCharactersInPathMessageBox() =>
        _messageDialog.ShowWarningAsync(R("RetroArchspecialcharactersinpath", "RetroArch path contains special characters."), R("Warning", "Warning"));

    public Task AzaharConfigurationInjectionPermissionErrorMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Azaharconfigurationinjectionpermissionerror", "Azahar configuration injection permission error."), R("Error", "Error"));

    public Task AzaharConfigurationSavedSuccessfullyMessageBox() =>
        _messageDialog.ShowInfoAsync(R("Azaharconfigurationsavedsuccessfully", "Azahar configuration saved successfully."), R("Info", "Info"));

    public Task FailedToSaveAzaharConfigurationMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Failedtosaveazaharconfiguration", "Failed to save Azahar configuration."), R("Error", "Error"));

    public Task XemuParameterShouldContainDvdPathMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Xemuparametershouldcontaindvdpath", "Xemu parameter should contain DVD path."), R("Warning", "Warning"));

    public Task PleaseExtractApplicationFirstMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Pleaseextractapplicationfirst", "Please extract the application first."), R("Warning", "Warning"));

    public Task InjectionFailedGenericMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Injectionfailedgeneric", "Injection failed."), R("Error", "Error"));

    public Task DaphneConfigurationSaveFailedMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Daphneconfigurationsavefailed", "Daphne configuration save failed."), R("Error", "Error"));

    public Task ShowImageDownloadTimeoutMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Imagedownloadtimeout", "Image download timed out."), R("Warning", "Warning"));

    public Task SystemNameRequiredBeforeChoosingImageMessageBox() =>
        _messageDialog.ShowWarningAsync(R("Systemnamerequiredbeforechoosingimage", "System name is required before choosing an image."), R("Warning", "Warning"));

    public Task InvalidImageFormatMessageBox() =>
        _messageDialog.ShowErrorAsync(R("Invalidimageformat", "Invalid image format."), R("Error", "Error"));

    public Task FailedToCopySystemImageMessageBox(string errorMessage) =>
        _messageDialog.ShowErrorAsync($"{R("Failedtocopysystemimage", "Failed to copy system image.")}\n\n{errorMessage}", R("Error", "Error"));

    public Task WarningMessageBox(string message) =>
        _messageDialog.ShowWarningAsync(message, R("Warning", "Warning"));

    public Task CustomErrorMessageBox(string message, string title) =>
        _messageDialog.ShowErrorAsync(message, title);

    public Task<bool> CustomQuestionMessageBox(string title, string message) =>
        _messageDialog.ShowYesNoAsync(message, title);
}
