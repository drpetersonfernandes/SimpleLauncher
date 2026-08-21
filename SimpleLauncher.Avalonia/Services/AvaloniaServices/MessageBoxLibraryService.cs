using System.Diagnostics;
using System.Text;
using Avalonia.Controls;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Avalonia.Views;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Avalonia.Services.AvaloniaServices;

public class MessageBoxLibraryService : IMessageBoxLibraryService
{
    private readonly IWindowContext _ctx;
    private readonly IConfiguration _configuration;

    public MessageBoxLibraryService(IWindowContext c, IConfiguration configuration)
    {
        _ctx = c;
        _configuration = configuration;
    }

    private Window? O => _ctx.PlatformWindow as Window;

    public async Task TakeScreenShotMessageBoxAsync()
    {
        if (O != null) await ShowAsync(O, "Press Print Screen to capture a screenshot.", "Screenshot", MessageButtons.Ok, MessageIcon.Information);
    }

    public Task CouldNotSaveScreenshotMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public async Task GameIsAlreadyInFavoritesMessageBoxAsync(string fileNameWithExtension)
    {
        if (O != null) await ShowAsync(O, fileNameWithExtension + " is already in favorites.", "Already Favorited", MessageButtons.Ok, MessageIcon.Information);
    }

    public async Task ErrorWhileAddingFavoritesMessageBoxAsync()
    {
        if (O != null) await ShowAsync(O, "Error adding to favorites.", "Error", MessageButtons.Ok, MessageIcon.Error);
    }

    public async Task ErrorWhileRemovingGameFromFavoriteMessageBoxAsync()
    {
        if (O != null) await ShowAsync(O, "Error removing from favorites.", "Error", MessageButtons.Ok, MessageIcon.Error);
    }

    public Task ErrorOpeningTheUpdateHistoryWindowMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ErrorOpeningVideoLinkMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ProblemOpeningInfoLinkMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public async Task ErrorOpeningUrlMessageBoxAsync()
    {
        if (O != null) await ShowAsync(O, "Could not open the link.", "Error", MessageButtons.Ok, MessageIcon.Error);
    }

    public Task ThereIsNoCoverMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoTitleSnapshotMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoGameplaySnapshotMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoCartMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoVideoFileMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task CouldNotOpenManualMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task NoPdfViewerInstalledMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoManualMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoWalkthroughMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoCabinetMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoFlyerMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoPcbMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public async Task FileSuccessfullyDeletedMessageBoxAsync(string fileNameWithExtension)
    {
        if (O != null) await ShowAsync(O, fileNameWithExtension + " deleted.", "Deleted", MessageButtons.Ok, MessageIcon.Information);
    }

    public async Task FileCouldNotBeDeletedMessageBoxAsync(string fileNameWithExtension)
    {
        if (O != null) await ShowAsync(O, "Could not delete " + fileNameWithExtension, "Error", MessageButtons.Ok, MessageIcon.Error);
    }

    public async Task FileNoLongerExistsMessageBoxAsync(string fileNameWithExtension)
    {
        if (O != null) await ShowAsync(O, fileNameWithExtension + " no longer exists.", "Not Found", MessageButtons.Ok, MessageIcon.Warning);
    }

    public async Task DefaultImageNotFoundMessageBoxAsync()
    {
        if (O != null) await ShowAsync(O, "Default cover image not found.", "Missing Image", MessageButtons.Ok, MessageIcon.Warning);
    }

    public async Task GlobalSearchErrorMessageBoxAsync()
    {
        if (O != null) await ShowAsync(O, "Search error.", "Error", MessageButtons.Ok, MessageIcon.Error);
    }

    public Task PleaseEnterSearchTermMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public async Task ErrorLaunchingGameMessageBoxAsync(string? logPath)
    {
        var msg = string.IsNullOrEmpty(logPath) ? "An unknown error occurred." : logPath;
        if (O != null) await ShowAsync(O, msg, "Launch Error", MessageButtons.Ok, MessageIcon.Error);
    }

    public Task SelectAGameToLaunchMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public async Task FileAddedToFavoritesMessageBoxAsync(string fileNameWithoutExtension)
    {
        if (O != null) await ShowAsync(O, fileNameWithoutExtension + " added to favorites.", "Added", MessageButtons.Ok, MessageIcon.Information);
    }

    public async Task FileRemovedFromFavoritesMessageBoxAsync(string fileNameWithoutExtension)
    {
        if (O != null) await ShowAsync(O, fileNameWithoutExtension + " removed from favorites.", "Removed", MessageButtons.Ok, MessageIcon.Information);
    }

    public Task CouldNotLaunchThisGameMessageBoxAsync(string? logPath)
    {
        return Task.CompletedTask;
    }

    public Task ProtocolHandlerNotRegisteredMessageBoxAsync(string protocol)
    {
        return Task.CompletedTask;
    }

    public Task EmulatorPathNotConfiguredMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ErrorCalculatingStatsMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedSaveReportMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ReportSavedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task NoStatsToSaveMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ErrorLaunchingToolMessageBoxAsync(string? logPath)
    {
        return Task.CompletedTask;
    }

    public Task SelectedToolNotFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ErrorMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task NoFavoriteFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task MoveToWritableFolderMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task InvalidSystemConfigMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ErrorMethodLoadGameFilesAsyncMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ErrorOpeningDonationLinkMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ToggleGamepadFailureMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ToolLaunchWasCanceledByUserMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ErrorChangingViewModeMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task NavigationButtonErrorMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SelectSystemBeforeSearchMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task EnterSearchQueryMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ErrorWhileLoadingHelpUserXmlMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task NoSystemInHelpUserXmlMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> CouldNotLoadHelpUserXmlMessageBoxAsync()
    {
        return Task.FromResult(MessageBoxResult.Cancel);
    }

    public Task FailedToLoadHelpUserXmlMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FileHelpUserXmlIsMissingMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ErrorWhileLoadingParametersMdMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task NoSystemInParametersMdMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToLoadParametersMdMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FileParametersMdIsMissingMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FileParametersMdIsEmptyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ImageViewerErrorMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ReinstallSimpleLauncherFileCorruptedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ReinstallSimpleLauncherFileMissingMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ErrorCheckingForUpdatesMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ErrorLoadingRomHistoryMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task NoHistoryXmlOrDatFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ErrorOpeningBrowserMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SystemXmlIsCorruptedMessageBoxAsync(string? logPath)
    {
        return Task.CompletedTask;
    }

    public async Task WouldYouLikeToOpenTheLogMessageBoxAsync(string? logPath)
    {
        if (O == null) return;

        var result = await ShowAsync(O,
            "'Simple Launcher' was unable to launch this game.\n\n" +
            "Would you like to open the 'error_user.log' file to debug the error?",
            "Error", MessageButtons.YesNo, MessageIcon.Error);

        if (result == MessageBoxResult.Yes && !string.IsNullOrEmpty(logPath))
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = logPath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open the 'error_user.log' file.");
                await ShowAsync(O, "The file 'error_user.log' was not found!", "Error", MessageButtons.Ok, MessageIcon.Error);
            }
        }
    }

    public Task FileSystemXmlIsCorruptedMessageBoxAsync(string? logPath)
    {
        return Task.CompletedTask;
    }

    public Task InstallUpdateManuallyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task UpdaterLaunchFailedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task RequiredFileMissingMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task EnterSupportRequestMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task EnterNameMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task EnterEmailMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ApiKeyErrorMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SupportRequestSuccessMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SupportRequestSendErrorMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ExtractionFailedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FileNeedToBeCompressedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task DownloadedFileIsMissingMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FileIsLockedMessageBoxAsync(string? tempFolderPath)
    {
        return Task.CompletedTask;
    }

    public Task LinksSavedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task DeadZonesSavedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task DeadZonesRevertedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task LinksRevertedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task MainWindowSearchEngineErrorMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task DownloadExtractionFailedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task DownloadAndExtractionWereSuccessfulMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ShowEmulatorDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem)
    {
        return Task.CompletedTask;
    }

    public Task ShowCoreDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem)
    {
        return Task.CompletedTask;
    }

    public Task ShowImagePackDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem)
    {
        return Task.CompletedTask;
    }

    public Task SelectAHistoryItemToRemoveMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> ReallyWantToRemoveAllPlayHistoryMessageBoxAsync()
    {
        return Task.FromResult(MessageBoxResult.Cancel);
    }

    public Task SystemAddedMessageBoxAsync(string systemName, string resolvedSystemFolder, string resolvedSystemImageFolder)
    {
        return Task.CompletedTask;
    }

    public Task AddSystemFailedMessageBoxAsync(string? details = null)
    {
        return Task.CompletedTask;
    }

    public Task RightClickContextMenuErrorMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task GameFileDoesNotExistMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> GameFileDoesNotExistAskToDeleteMessageBoxAsync(string filePath)
    {
        return Task.FromResult(MessageBoxResult.Cancel);
    }

    public Task<MessageBoxResult> FavoriteFileDoesNotExistAskToDeleteMessageBoxAsync(string filePath)
    {
        return Task.FromResult(MessageBoxResult.Cancel);
    }

    public Task CouldNotOpenHistoryWindowMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task CouldNotOpenWalkthroughMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SelectAFavoriteToRemoveMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public async Task SystemXmlNotFoundMessageBoxAsync()
    {
        if (O != null)
            await ShowAsync(O,
                "'system.xml' not found inside the application folder.\n\n" +
                "Please restart 'Simple Launcher'.\n\n" +
                "If that does not work, please reinstall 'Simple Launcher'.",
                "Error", MessageButtons.Ok, MessageIcon.Error);
    }

    public Task YouCanAddANewSystemMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task EmulatorNameRequiredMessageBoxAsync(int i)
    {
        return Task.CompletedTask;
    }

    public Task EmulatorNameIsRequiredMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task EmulatorNameMustBeUniqueMessageBoxAsync(string emulatorName)
    {
        return Task.CompletedTask;
    }

    public Task SystemSavedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task PathOrParameterInvalidMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task Emulator1RequiredMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ExtensionToLaunchIsRequiredMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ExtensionToSearchIsRequiredMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FileMustBeCompressedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SystemImageFolderCanNotBeEmptyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SystemFolderCanNotBeEmptyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SystemNameCanNotBeEmptyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task InvalidSystemNameCharactersMessageBoxAsync(string invalidChars)
    {
        return Task.CompletedTask;
    }

    public Task InvalidFolderCharactersMessageBoxAsync(string invalidChars)
    {
        return Task.CompletedTask;
    }

    public Task FolderCreationFailedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SelectASystemToDeleteMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SystemNotFoundInTheXmlMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ErrorFindingGameFilesMessageBoxAsync(string logPath)
    {
        return Task.CompletedTask;
    }

    public Task GamePadErrorMessageBoxAsync(string? logPath)
    {
        return Task.CompletedTask;
    }

    public async Task CouldNotLaunchGameMessageBoxAsync(string? logPath)
    {
        if (O == null) return;

        var result = await ShowAsync(O,
            "'Simple Launcher' could not launch the selected game.\n\n" +
            "Make sure the ROM or ISO you're trying to run is not corrupted.\n" +
            "If you are trying to run Retroarch, ensure that the BIOS or required files for the core are installed.\n" +
            "Also, make sure you are calling the emulator with the correct parameter.\n\n" +
            "You can turn off this error message in Expert mode.\n\n" +
            "Do you want to open the file 'error_user.log' to debug the error?",
            "Error", MessageButtons.YesNo, MessageIcon.Error);

        if (result == MessageBoxResult.Yes && !string.IsNullOrEmpty(logPath))
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = logPath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open the error log file.");
            }
        }
    }

    public Task InvalidOperationExceptionMessageBoxAsync(string? logPath)
    {
        return Task.CompletedTask;
    }

    public Task ThereWasAnErrorLaunchingThisGameMessageBoxAsync(string? logPath)
    {
        return Task.CompletedTask;
    }

    public Task BatchFileFailedMessageBoxAsync(string batchFilePath, string errorDetail, string? logPath, int? exitCode = null)
    {
        return Task.CompletedTask;
    }

    public Task<bool> BatchFilePathsMissingMessageBoxAsync(IList<string> missingPaths)
    {
        return Task.FromResult(false);
    }

    public Task ElevationRequiredMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task NullFileExtensionMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task CouldNotFindAFileMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> SearchOnlineForRomHistoryMessageBoxAsync()
    {
        return Task.FromResult(MessageBoxResult.Cancel);
    }

    public Task SystemHasBeenDeletedMessageBoxAsync(string selectedSystemName)
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> AreYouSureDoYouWantToDeleteThisSystemMessageBoxAsync()
    {
        return Task.FromResult(MessageBoxResult.Cancel);
    }

    public Task ThereWasAnErrorDeletingTheGameMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ThereWasAnErrorDeletingTheCoverImageMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> AreYouSureYouWantToDeleteTheGameMessageBoxAsync(string fileNameWithExtension)
    {
        return Task.FromResult(MessageBoxResult.Cancel);
    }

    public Task<MessageBoxResult> AreYouSureYouWantToDeleteTheCoverImageMessageBoxAsync(string fileNameWithoutExtension)
    {
        return Task.FromResult(MessageBoxResult.Cancel);
    }

    public Task<MessageBoxResult> WouldYouLikeToSaveAReportMessageBoxAsync()
    {
        return Task.FromResult(MessageBoxResult.Cancel);
    }

    public Task SimpleLauncherWasUnableToRestoreBackupMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> WouldYouLikeToRestoreTheLastBackupMessageBoxAsync()
    {
        return Task.FromResult(MessageBoxResult.Cancel);
    }

    public Task FailedToLoadLanguageResourceMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task InvalidSystemConfigurationMessageBoxAsync(string errorMessage)
    {
        return Task.CompletedTask;
    }

    public Task UnableToOpenLinkMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task NoGameFoundInTheRandomSelectionMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task PleaseSelectASystemBeforeMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ToggleFuzzyMatchingFailureMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FuzzyMatchingErrorFailToSetThresholdMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public async Task ListOfErrorsMessageBoxAsync(StringBuilder errorMessages)
    {
        if (O != null) await ShowAsync(O, errorMessages.ToString(), "Errors", MessageButtons.Ok, MessageIcon.Error);
    }

    public Task ThereIsNoUpdateAvailableMessageBoxAsync(string currentVersion)
    {
        return Task.CompletedTask;
    }

    public Task AnotherInstanceIsRunningMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToStartSimpleLauncherMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToRestartMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> DoYouWantToUpdateMessageBoxAsync(string currentVersion, string latestVersion)
    {
        return Task.FromResult(MessageBoxResult.Cancel);
    }

    public Task HandleMissingRequiredFilesMessageBoxAsync(string fileList)
    {
        return Task.CompletedTask;
    }

    public Task HandleApiConfigErrorMessageBoxAsync(string reason)
    {
        return Task.CompletedTask;
    }

    public Task DiskSpaceErrorMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task CouldNotCheckForDiskSpaceMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SaveSystemFailedMessageBoxAsync(string? details = null)
    {
        return Task.CompletedTask;
    }

    public Task CouldNotOpenTheDownloadLinkMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ErrorLoadingAppSettingsMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task PotentialPathManipulationDetectedMessageBoxAsync(string archivePath)
    {
        return Task.CompletedTask;
    }

    public Task CouldNotOpenSoundConfigurationWindowMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ErrorSettingSoundFileMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task NotificationSoundIsDisableMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task NoSoundFileIsSelectedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SettingsSavedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveSettingsMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FilePathIsInvalidMessageBoxAsync(string? logPath)
    {
        return Task.CompletedTask;
    }

    public Task ThereWasAnErrorMountingTheFileMessageBoxAsync(int? exitCode = null)
    {
        return Task.CompletedTask;
    }

    public Task DokanDriverNotInstalledMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task LaunchToolInformationMessageBoxAsync(string info)
    {
        return Task.CompletedTask;
    }

    public Task CannotScreenshotMinimizedWindowMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToCopyLogContentMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task CouldNotFindUpdaterOnGitHubMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task CouldNotOpenAchievementsWindowMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public async Task<MessageBoxResult> GameNotSupportedByRetroAchievementsMessageBoxAsync()
    {
        if (O != null)
        {
            return await ShowAsync(O,
                "'Simple Launcher' could not calculate the hash value of this game or this game is not yet supported by RetroAchievements.\n\n" +
                "Do you want to open the global RetroAchievements window?",
                "RetroAchievements", MessageButtons.YesNo, MessageIcon.Question);
        }

        return MessageBoxResult.Cancel;
    }

    public Task GameLaunchTimeoutMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task AddRaLoginMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task NoDefaultBrowserConfiguredMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> WarnUserAboutMemoryConsumptionMessageBoxAsync()
    {
        return Task.FromResult(MessageBoxResult.Cancel);
    }

    public Task GroupByFolderOnlyForMameAndDosBoxMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> GroupByFolderWarningMessageBoxAsync()
    {
        return Task.FromResult(MessageBoxResult.Cancel);
    }

    public Task<MessageBoxResult> FirstRunWelcomeMessageBoxAsync()
    {
        return Task.FromResult(MessageBoxResult.Cancel);
    }

    public Task EmulatorLocationRequiredMessageBoxAsync(int emulatorNumber)
    {
        return Task.CompletedTask;
    }

    public Task ImagePackDownloaderUnavailableMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public async Task EasyModeUnavailableMessageBoxAsync()
    {
        if (O != null)
            await ShowAsync(O,
                "'Simple Launcher' could not access the Web API to download the updated configuration.\n\n" +
                "This could be due to:\n" +
                "• A government firewall or internet restriction in your region\n" +
                "• Network connectivity issues\n\n" +
                "To resolve this issue, you can:\n" +
                "1. Enable a VPN connection and try again\n" +
                "2. Check your internet connection\n" +
                "3. Configure systems manually using the Edit System feature\n\n" +
                "Note: A VPN may be required if you are located in a country with internet restrictions.",
                "Easy Mode Unavailable", MessageButtons.Ok, MessageIcon.Warning);
    }

    public Task SimpleLauncherDoesNotSupportRaHashOfSystemGroupedByFolderMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public async Task<MessageBoxResult> ScanGamePathForRetroAchievementsMessageBoxAsync()
    {
        if (O != null)
        {
            return await ShowAsync(O,
                "We need to scan your game path to see what game is compatible with RetroAchievements.",
                "RetroAchievements", MessageButtons.YesNo, MessageIcon.Question);
        }

        return MessageBoxResult.Cancel;
    }

    public Task UnsupportedArchitectureMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SevenZipDllNotFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInitializeSevenZipMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ShowExtractionFailedMessageBoxAsync(string tempFolderPath)
    {
        return Task.CompletedTask;
    }

    public Task ShowDownloadFileLockedMessageBoxAsync(string tempFolderPath)
    {
        return Task.CompletedTask;
    }

    public Task ShowCustomMessageBoxAsync(string message, string launchError, string? logPath)
    {
        return Task.CompletedTask;
    }

    public Task EnterValidSearchTermsMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task OperationCancelledMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> DoYouWantToCancelAndCloseMessageBoxAsync()
    {
        return Task.FromResult(MessageBoxResult.Cancel);
    }

    public Task CouldNotOpenBrowserForAiSupportMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task PowerShellExecutionPolicyRestrictionsMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task UnabletomountIsOfileMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task UnabletoDismountIsOfileMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ApplicationControlPolicyBlockedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ApplicationControlPolicyBlockedManualLinkMessageBoxAsync(string url)
    {
        return Task.CompletedTask;
    }

    public Task EnterYourRetroAchievementsUsernameMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task EmulatorConfiguredSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToConfigureTheEmulatorMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task AnErrorOccurredWhileConfiguringTheEmulatorMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToLoginToRetroAchievementsMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FileSystemXmlIsLockedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectMameConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task MameConfigurationInjectedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedtoinjectMamEconfiguration2MessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task MameEmulatorPathNotFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task RetroArchemulatorpathnotfoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedtoinjectRetroArchconfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task RetroArchConfigurationInjectedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedtoinjectRetroArchconfiguration2MessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task XeniaemulatorpathnotfoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedtoinjectXeniaconfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task XeniaconfigurationinjectedsuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedtoinjectXeniaconfiguration2MessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task EnterUsernamePasswordMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task AresemulatornotfoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task DaphnesettingssavedsuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task Pcsx2SettingssavedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task Pcsx2ConfigurationInjectionPermissionErrorMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SettingsSavedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task CemuEmulatorNotFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedtoinjectAresconfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task CemuConfigurationSavedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FlycastEmulatorNotFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task AresConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveAresConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectFlycastConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FlycastConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task DolphinEmulatorNotFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveFlycastConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectDolphinConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task DolphinConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveDolphinConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SegaModel2EmulatorNotFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectSegaModel2ConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SegaModel2ConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task BlastemEmulatorNotFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectBlastemConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task BlastemConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveSegaModel2ConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveBlastemConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task Rpcs3EmulatorNotFoundPleaseLocateMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectRpcs3ConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task Rpcs3ConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveRpcs3ConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task StellaEmulatorNotFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectStellaConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SupermodelEmulatorNotFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task StellaConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectSupermodelConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveStellaConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SupermodelConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveSupermodelConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task MednafenEmulatorNotFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task MesenEmulatorNotFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectMednafenConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectMesenConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task DuckStationEmulatorNotFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task MednafenConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveMednafenConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectDuckStationConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task DuckStationConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveMesenConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveDuckStationConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task MesenConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectYumirConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task YumirConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task RaineSettingsSavedAndInjectedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task RaineExecutableNotFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task YumirEmulatorNotFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ReDreamEmulatorPathNotFoundMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectReDreamConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ReDreamConfigurationInjectedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task CouldNotLaunchGameDueToDepViolationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public async Task MameRomSetErrorMessageBoxAsync()
    {
        if (O == null) return;

        var result = await ShowAsync(O,
            "MAME emulator could not find required files to launch this game.\n\n" +
            "MAME is very restrictive about the filename of the game.\n\n" +
            "Please ensure you are running a compatible ROM set.\n\n" +
            "Would you like to visit the PleasureDome website to download a compatible ROM set?",
            "ROM Files Not Found", MessageButtons.YesNo, MessageIcon.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var url = _configuration.GetValue<string>("Urls:PleasureDomeWebsite") ??
                          "https://pleasuredome.github.io/pleasuredome/index.html";
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not open browser");
            }
        }
    }

    public async Task MameUnknownSystemErrorMessageBoxAsync()
    {
        if (O == null) return;

        var result = await ShowAsync(O,
            "MAME emulator could not find a matching compatible system to launch.\n\n" +
            "MAME is very restrictive about the filename of the game.\n\n" +
            "The filename of your game must match the expected filename to run on MAME.\n\n" +
            "Please ensure you are running a compatible ROM set.\n\n" +
            "Would you like to visit the PleasureDome website to download a compatible ROM set?",
            "Unknown System Error", MessageButtons.YesNo, MessageIcon.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var url = _configuration.GetValue<string>("Urls:PleasureDomeWebsite") ??
                          "https://pleasuredome.github.io/pleasuredome/index.html";
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not open browser");
            }
        }
    }

    public async Task MameUnableToLoadImageMessageBoxAsync()
    {
        if (O == null) return;

        var result = await ShowAsync(O,
            "MAME emulator could not load the image file.\n\n" +
            "MAME is very restrictive about the filename of the game.\n\n" +
            "The filename of your game must match the expected filename to run on MAME.\n\n" +
            "Please ensure you are running a compatible ROM set.\n\n" +
            "Would you like to visit the PleasureDome website to download a compatible ROM set?",
            "Unable to Load Image", MessageButtons.YesNo, MessageIcon.Warning);

        if (result == MessageBoxResult.Yes)
        {
            try
            {
                var url = _configuration.GetValue<string>("Urls:PleasureDomeWebsite") ??
                          "https://pleasuredome.github.io/pleasuredome/index.html";
                Process.Start(new ProcessStartInfo { FileName = url, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Could not open browser");
            }
        }
    }

    public Task OotakeDoesNotSupportImageFilesMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task GeolithDoesNotSupportCompressedFilesMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task RetroArchParameterShouldContainLMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public async Task RetroArchParameterIssueMessageBoxAsync(string? logPath)
    {
        if (O == null) return;

        var result = await ShowAsync(O,
            "RetroArch could not launch your game.\n\n" +
            "99% of the launch failures are due to incorrect parameters.\n\n" +
            "Go back to 'Expert Mode' and double-check the parameter field for this emulator. " +
            "Double-check the path to the desired core. Read the recommendations from the 'Simple Launcher' developer for the specific system.\n\n" +
            "Check the core requirements to run it. Some cores require a BIOS file to work. " +
            "Read the core documentation to figure out what the requirements are for that specific core.\n\n" +
            "Do you want to open the file 'error_user.log' to debug the error?",
            "Error", MessageButtons.YesNo, MessageIcon.Error);

        if (result == MessageBoxResult.Yes && !string.IsNullOrEmpty(logPath))
        {
            try
            {
                Process.Start(new ProcessStartInfo { FileName = logPath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open the error log file.");
                await ShowAsync(O, "The file 'error_user.log' was not found!", "Error", MessageButtons.Ok, MessageIcon.Error);
            }
        }
    }

    public async Task RetroArchSpecialCharactersInPathMessageBoxAsync()
    {
        if (O == null) return;

        await ShowAsync(O,
            "The emulator could not launch the game because the file path contains special characters (for example: ´, `, ~, !, ?).\n\n" +
            "RetroArch cannot create its required folders in paths with these characters.\n\n" +
            "To fix this, please move your emulator and your game files to a folder that uses only standard letters and numbers, such as C:\\Games\\.",
            "Error", MessageButtons.Ok, MessageIcon.Error);
    }

    public Task AzaharConfigurationInjectionPermissionErrorMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task AzaharConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveAzaharConfigurationMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task XemuParameterShouldContainDvdPathMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task PleaseExtractApplicationFirstMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task InjectionFailedGenericMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task DaphneConfigurationSaveFailedMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task ShowImageDownloadTimeoutMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task SystemNameRequiredBeforeChoosingImageMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task InvalidImageFormatMessageBoxAsync()
    {
        return Task.CompletedTask;
    }

    public Task FailedToCopySystemImageMessageBoxAsync(string errorMessage)
    {
        return Task.CompletedTask;
    }

    public async Task WarningMessageBoxAsync(string message)
    {
        if (O != null) await ShowAsync(O, message, "Warning", MessageButtons.Ok, MessageIcon.Warning);
    }

    public async Task CustomErrorMessageBoxAsync(string message, string title)
    {
        if (O != null) await ShowAsync(O, message, title, MessageButtons.Ok, MessageIcon.Error);
    }

    public async Task<bool> CustomQuestionMessageBoxAsync(string title, string message)
    {
        if (O != null)
        {
            var r = await ShowAsync(O, message, title, MessageButtons.YesNo, MessageIcon.Question);
            return r == MessageBoxResult.Yes;
        }

        return false;
    }

    public async Task CustomInfoMessageBoxAsync(string title, string message)
    {
        if (O != null) await ShowAsync(O, message, title, MessageButtons.Ok, MessageIcon.Information);
    }

    public async Task<bool> AskAiToFixParametersMessageBoxAsync()
    {
        if (O != null)
        {
            var result = await ShowAsync(O,
                "Do you want Simple Launcher AI to suggest correct parameters for this emulator?",
                "AI Parameter Suggestion",
                MessageButtons.YesNo,
                MessageIcon.Question);
            return result == MessageBoxResult.Yes;
        }

        return false;
    }

    /// <summary>
    /// Shows a modal message-box-style dialog on the Avalonia UI thread.
    /// </summary>
    private static async Task<MessageBoxResult> ShowAsync(
        Window? owner, string message, string caption,
        MessageButtons buttons, MessageIcon icon)
    {
        if (owner is null)
        {
            return MessageBoxResult.Cancel;
        }

        return await MessageDialogWindow.ShowAsync(owner, message, caption, buttons, icon);
    }
}
