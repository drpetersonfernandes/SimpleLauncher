using System.Text;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;

namespace SimpleLauncher.Tests.TestHelpers;

public class NoOpMessageBoxLibraryService : IMessageBoxLibraryService
{
    /// <summary>
    /// Does nothing. Does not display the take-screenshot message box.
    /// </summary>
    public Task TakeScreenShotMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the could-not-save-screenshot message box.
    /// </summary>
    public Task CouldNotSaveScreenshotMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the game-already-in-favorites message box.
    /// </summary>
    /// <param name="fileNameWithExtension">The game file name.</param>
    public Task GameIsAlreadyInFavoritesMessageBox(string fileNameWithExtension)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-adding-favorites message box.
    /// </summary>
    public Task ErrorWhileAddingFavoritesMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-removing-favorite message box.
    /// </summary>
    public Task ErrorWhileRemovingGameFromFavoriteMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-opening-update-history message box.
    /// </summary>
    public Task ErrorOpeningTheUpdateHistoryWindowMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-opening-video-link message box.
    /// </summary>
    public Task ErrorOpeningVideoLinkMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the problem-opening-info-link message box.
    /// </summary>
    public Task ProblemOpeningInfoLinkMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-opening-URL message box.
    /// </summary>
    public Task ErrorOpeningUrlMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-cover message box.
    /// </summary>
    public Task ThereIsNoCoverMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-title-snapshot message box.
    /// </summary>
    public Task ThereIsNoTitleSnapshotMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-gameplay-snapshot message box.
    /// </summary>
    public Task ThereIsNoGameplaySnapshotMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-cart message box.
    /// </summary>
    public Task ThereIsNoCartMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-video-file message box.
    /// </summary>
    public Task ThereIsNoVideoFileMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the could-not-open-manual message box.
    /// </summary>
    public Task CouldNotOpenManualMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-PDF-viewer-installed message box.
    /// </summary>
    public Task NoPdfViewerInstalledMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-manual message box.
    /// </summary>
    public Task ThereIsNoManualMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-walkthrough message box.
    /// </summary>
    public Task ThereIsNoWalkthroughMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-cabinet message box.
    /// </summary>
    public Task ThereIsNoCabinetMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-flyer message box.
    /// </summary>
    public Task ThereIsNoFlyerMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-PCB message box.
    /// </summary>
    public Task ThereIsNoPcbMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the file-successfully-deleted message box.
    /// </summary>
    /// <param name="fileNameWithExtension">The deleted file name.</param>
    public Task FileSuccessfullyDeletedMessageBox(string fileNameWithExtension)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the file-could-not-be-deleted message box.
    /// </summary>
    /// <param name="fileNameWithExtension">The file name that could not be deleted.</param>
    public Task FileCouldNotBeDeletedMessageBox(string fileNameWithExtension)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the default-image-not-found message box.
    /// </summary>
    public Task DefaultImageNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the global-search-error message box.
    /// </summary>
    public Task GlobalSearchErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the please-enter-search-term message box.
    /// </summary>
    public Task PleaseEnterSearchTermMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-launching-game message box.
    /// </summary>
    /// <param name="logPath">The path to the log file.</param>
    public Task ErrorLaunchingGameMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the select-a-game-to-launch message box.
    /// </summary>
    public Task SelectAGameToLaunchMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the file-added-to-favorites message box.
    /// </summary>
    /// <param name="fileNameWithoutExtension">The file name without extension.</param>
    public Task FileAddedToFavoritesMessageBox(string fileNameWithoutExtension)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the file-removed-from-favorites message box.
    /// </summary>
    /// <param name="fileNameWithoutExtension">The file name without extension.</param>
    public Task FileRemovedFromFavoritesMessageBox(string fileNameWithoutExtension)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the could-not-launch-game message box.
    /// </summary>
    /// <param name="logPath">The path to the log file.</param>
    public Task CouldNotLaunchThisGameMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the protocol-handler-not-registered message box.
    /// </summary>
    /// <param name="protocol">The protocol name.</param>
    public Task ProtocolHandlerNotRegisteredMessageBox(string protocol)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the emulator-path-not-configured message box.
    /// </summary>
    public Task EmulatorPathNotConfiguredMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-calculating-stats message box.
    /// </summary>
    public Task ErrorCalculatingStatsMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-save-report message box.
    /// </summary>
    public Task FailedSaveReportMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the report-saved message box.
    /// </summary>
    public Task ReportSavedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-stats-to-save message box.
    /// </summary>
    public Task NoStatsToSaveMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-launching-tool message box.
    /// </summary>
    /// <param name="logPath">The path to the log file.</param>
    public Task ErrorLaunchingToolMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the selected-tool-not-found message box.
    /// </summary>
    public Task SelectedToolNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the generic error message box.
    /// </summary>
    public Task ErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-favorite-found message box.
    /// </summary>
    public Task NoFavoriteFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the move-to-writable-folder message box.
    /// </summary>
    public Task MoveToWritableFolderMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the invalid-system-config message box.
    /// </summary>
    public Task InvalidSystemConfigMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-loading-game-files message box.
    /// </summary>
    public Task ErrorMethodLoadGameFilesAsyncMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-opening-donation-link message box.
    /// </summary>
    public Task ErrorOpeningDonationLinkMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the toggle-gamepad-failure message box.
    /// </summary>
    public Task ToggleGamepadFailureMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the tool-launch-canceled-by-user message box.
    /// </summary>
    public Task ToolLaunchWasCanceledByUserMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-changing-view-mode message box.
    /// </summary>
    public Task ErrorChangingViewModeMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the navigation-button-error message box.
    /// </summary>
    public Task NavigationButtonErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the select-system-before-search message box.
    /// </summary>
    public Task SelectSystemBeforeSearchMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the enter-search-query message box.
    /// </summary>
    public Task EnterSearchQueryMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-loading-help-user-xml message box.
    /// </summary>
    public Task ErrorWhileLoadingHelpUserXmlMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-system-in-help-user-xml message box.
    /// </summary>
    public Task NoSystemInHelpUserXmlMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns <see cref="MessageBoxResult.No"/> without displaying a message box.
    /// </summary>
    /// <returns><see cref="MessageBoxResult.No"/>.</returns>
    public Task<MessageBoxResult> CouldNotLoadHelpUserXmlMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-load-help-user-xml message box.
    /// </summary>
    public Task FailedToLoadHelpUserXmlMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the help-user-xml-missing message box.
    /// </summary>
    public Task FileHelpUserXmlIsMissingMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-loading-parameters-md message box.
    /// </summary>
    public Task ErrorWhileLoadingParametersMdMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-system-in-parameters-md message box.
    /// </summary>
    public Task NoSystemInParametersMdMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-load-parameters-md message box.
    /// </summary>
    public Task FailedToLoadParametersMdMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the parameters-md-missing message box.
    /// </summary>
    public Task FileParametersMdIsMissingMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the parameters-md-empty message box.
    /// </summary>
    public Task FileParametersMdIsEmptyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the image-viewer-error message box.
    /// </summary>
    public Task ImageViewerErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the reinstall-file-corrupted message box.
    /// </summary>
    public Task ReinstallSimpleLauncherFileCorruptedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the reinstall-file-missing message box.
    /// </summary>
    public Task ReinstallSimpleLauncherFileMissingMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-checking-for-updates message box.
    /// </summary>
    public Task ErrorCheckingForUpdatesMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-loading-rom-history message box.
    /// </summary>
    public Task ErrorLoadingRomHistoryMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-history-xml-or-dat-found message box.
    /// </summary>
    public Task NoHistoryXmlOrDatFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-opening-browser message box.
    /// </summary>
    public Task ErrorOpeningBrowserMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the system-xml-corrupted message box.
    /// </summary>
    /// <param name="logPath">The path to the log file.</param>
    public Task SystemXmlIsCorruptedMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the would-you-like-to-open-log message box.
    /// </summary>
    /// <param name="logPath">The path to the log file.</param>
    public Task WouldYouLikeToOpenTheLogMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the file-system-xml-corrupted message box.
    /// </summary>
    /// <param name="logPath">The path to the log file.</param>
    public Task FileSystemXmlIsCorruptedMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the install-update-manually message box.
    /// </summary>
    public Task InstallUpdateManuallyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the updater-launch-failed message box.
    /// </summary>
    public Task UpdaterLaunchFailedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the required-file-missing message box.
    /// </summary>
    public Task RequiredFileMissingMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the enter-support-request message box.
    /// </summary>
    public Task EnterSupportRequestMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the enter-name message box.
    /// </summary>
    public Task EnterNameMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the enter-email message box.
    /// </summary>
    public Task EnterEmailMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the API-key-error message box.
    /// </summary>
    public Task ApiKeyErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the support-request-success message box.
    /// </summary>
    public Task SupportRequestSuccessMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the support-request-send-error message box.
    /// </summary>
    public Task SupportRequestSendErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the extraction-failed message box.
    /// </summary>
    public Task ExtractionFailedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the file-need-to-be-compressed message box.
    /// </summary>
    public Task FileNeedToBeCompressedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the downloaded-file-is-missing message box.
    /// </summary>
    public Task DownloadedFileIsMissingMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the file-is-locked message box.
    /// </summary>
    /// <param name="tempFolderPath">The temporary folder path.</param>
    public Task FileIsLockedMessageBox(string tempFolderPath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the links-saved message box.
    /// </summary>
    public Task LinksSavedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the dead-zones-saved message box.
    /// </summary>
    public Task DeadZonesSavedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the links-reverted message box.
    /// </summary>
    public Task LinksRevertedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the main-window-search-engine-error message box.
    /// </summary>
    public Task MainWindowSearchEngineErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the download-extraction-failed message box.
    /// </summary>
    public Task DownloadExtractionFailedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the download-and-extraction-were-successful message box.
    /// </summary>
    public Task DownloadAndExtractionWereSuccessfulMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the emulator-download-error message box.
    /// </summary>
    /// <param name="selectedSystem">The selected system configuration.</param>
    public Task ShowEmulatorDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the core-download-error message box.
    /// </summary>
    /// <param name="selectedSystem">The selected system configuration.</param>
    public Task ShowCoreDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the image-pack-download-error message box.
    /// </summary>
    /// <param name="selectedSystem">The selected system configuration.</param>
    public Task ShowImagePackDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the select-a-history-item-to-remove message box.
    /// </summary>
    public Task SelectAHistoryItemToRemoveMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns <see cref="MessageBoxResult.No"/> without displaying a message box.
    /// </summary>
    /// <returns><see cref="MessageBoxResult.No"/>.</returns>
    public Task<MessageBoxResult> ReallyWantToRemoveAllPlayHistoryMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    /// <summary>
    /// Does nothing. Does not display the system-added message box.
    /// </summary>
    /// <param name="systemName">The system name.</param>
    /// <param name="resolvedSystemFolder">The resolved system folder path.</param>
    /// <param name="resolvedSystemImageFolder">The resolved system image folder path.</param>
    public Task SystemAddedMessageBox(string systemName, string resolvedSystemFolder, string resolvedSystemImageFolder)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the add-system-failed message box.
    /// </summary>
    /// <param name="details">Optional error details.</param>
    public Task AddSystemFailedMessageBox(string? details = null)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the right-click-context-menu-error message box.
    /// </summary>
    public Task RightClickContextMenuErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the game-file-does-not-exist message box.
    /// </summary>
    public Task GameFileDoesNotExistMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns <see cref="MessageBoxResult.No"/> without displaying a message box.
    /// </summary>
    /// <param name="filePath">The file path that does not exist.</param>
    /// <returns><see cref="MessageBoxResult.No"/>.</returns>
    public Task<MessageBoxResult> GameFileDoesNotExistAskToDeleteMessageBox(string filePath)
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    /// <summary>
    /// Returns <see cref="MessageBoxResult.No"/> without displaying a message box.
    /// </summary>
    /// <param name="filePath">The favorite file path that does not exist.</param>
    /// <returns><see cref="MessageBoxResult.No"/>.</returns>
    public Task<MessageBoxResult> FavoriteFileDoesNotExistAskToDeleteMessageBox(string filePath)
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    /// <summary>
    /// Does nothing. Does not display the could-not-open-history-window message box.
    /// </summary>
    public Task CouldNotOpenHistoryWindowMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the could-not-open-walkthrough message box.
    /// </summary>
    public Task CouldNotOpenWalkthroughMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the select-a-favorite-to-remove message box.
    /// </summary>
    public Task SelectAFavoriteToRemoveMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the system-xml-not-found message box.
    /// </summary>
    public Task SystemXmlNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the you-can-add-a-new-system message box.
    /// </summary>
    public Task YouCanAddANewSystemMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the emulator-name-required message box.
    /// </summary>
    /// <param name="i">The emulator index.</param>
    public Task EmulatorNameRequiredMessageBox(int i)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the emulator-name-is-required message box.
    /// </summary>
    public Task EmulatorNameIsRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the emulator-name-must-be-unique message box.
    /// </summary>
    /// <param name="emulatorName">The duplicate emulator name.</param>
    public Task EmulatorNameMustBeUniqueMessageBox(string emulatorName)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the system-saved-successfully message box.
    /// </summary>
    public Task SystemSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the path-or-parameter-invalid message box.
    /// </summary>
    public Task PathOrParameterInvalidMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the emulator-1-required message box.
    /// </summary>
    public Task Emulator1RequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the extension-to-launch-is-required message box.
    /// </summary>
    public Task ExtensionToLaunchIsRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the extension-to-search-is-required message box.
    /// </summary>
    public Task ExtensionToSearchIsRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the file-must-be-compressed message box.
    /// </summary>
    public Task FileMustBeCompressedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the system-image-folder-can-not-be-empty message box.
    /// </summary>
    public Task SystemImageFolderCanNotBeEmptyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the system-folder-can-not-be-empty message box.
    /// </summary>
    public Task SystemFolderCanNotBeEmptyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the system-name-can-not-be-empty message box.
    /// </summary>
    public Task SystemNameCanNotBeEmptyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the invalid-system-name-characters message box.
    /// </summary>
    /// <param name="invalidChars">The invalid characters found.</param>
    public Task InvalidSystemNameCharactersMessageBox(string invalidChars)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the invalid-folder-characters message box.
    /// </summary>
    /// <param name="invalidChars">The invalid characters found.</param>
    public Task InvalidFolderCharactersMessageBox(string invalidChars)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the folder-creation-failed message box.
    /// </summary>
    public Task FolderCreationFailedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the select-a-system-to-delete message box.
    /// </summary>
    public Task SelectASystemToDeleteMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the system-not-found-in-xml message box.
    /// </summary>
    public Task SystemNotFoundInTheXmlMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-finding-game-files message box.
    /// </summary>
    /// <param name="logPath">The path to the log file.</param>
    public Task ErrorFindingGameFilesMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the gamepad-error message box.
    /// </summary>
    /// <param name="logPath">The path to the log file.</param>
    public Task GamePadErrorMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the could-not-launch-game message box.
    /// </summary>
    /// <param name="logPath">The path to the log file.</param>
    public Task CouldNotLaunchGameMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the invalid-operation message box.
    /// </summary>
    /// <param name="logPath">The path to the log file.</param>
    public Task InvalidOperationExceptionMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-launching-this-game message box.
    /// </summary>
    /// <param name="logPath">The path to the log file.</param>
    public Task ThereWasAnErrorLaunchingThisGameMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the batch-file-failed message box.
    /// </summary>
    /// <param name="batchFilePath">The batch file path.</param>
    /// <param name="errorDetail">The error detail.</param>
    /// <param name="logPath">The path to the log file.</param>
    /// <param name="exitCode">The optional exit code.</param>
    public Task BatchFileFailedMessageBox(string batchFilePath, string errorDetail, string logPath, int? exitCode = null)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns <see langword="false"/> without displaying a message box.
    /// </summary>
    /// <param name="missingPaths">The list of missing paths.</param>
    /// <returns><see langword="false"/>.</returns>
    public Task<bool> BatchFilePathsMissingMessageBox(List<string> missingPaths)
    {
        return Task.FromResult(false);
    }

    /// <summary>
    /// Does nothing. Does not display the elevation-required message box.
    /// </summary>
    public Task ElevationRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the null-file-extension message box.
    /// </summary>
    public Task NullFileExtensionMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the could-not-find-a-file message box.
    /// </summary>
    public Task CouldNotFindAFileMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns <see cref="MessageBoxResult.No"/> without displaying a message box.
    /// </summary>
    /// <returns><see cref="MessageBoxResult.No"/>.</returns>
    public Task<MessageBoxResult> SearchOnlineForRomHistoryMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    /// <summary>
    /// Does nothing. Does not display the system-has-been-deleted message box.
    /// </summary>
    /// <param name="selectedSystemName">The deleted system name.</param>
    public Task SystemHasBeenDeletedMessageBox(string selectedSystemName)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns <see cref="MessageBoxResult.No"/> without displaying a message box.
    /// </summary>
    /// <returns><see cref="MessageBoxResult.No"/>.</returns>
    public Task<MessageBoxResult> AreYouSureDoYouWantToDeleteThisSystemMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    /// <summary>
    /// Does nothing. Does not display the error-deleting-game message box.
    /// </summary>
    public Task ThereWasAnErrorDeletingTheGameMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-deleting-cover-image message box.
    /// </summary>
    public Task ThereWasAnErrorDeletingTheCoverImageMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns <see cref="MessageBoxResult.No"/> without displaying a message box.
    /// </summary>
    /// <param name="fileNameWithExtension">The game file name.</param>
    /// <returns><see cref="MessageBoxResult.No"/>.</returns>
    public Task<MessageBoxResult> AreYouSureYouWantToDeleteTheGameMessageBox(string fileNameWithExtension)
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    /// <summary>
    /// Returns <see cref="MessageBoxResult.No"/> without displaying a message box.
    /// </summary>
    /// <param name="fileNameWithoutExtension">The cover image file name.</param>
    /// <returns><see cref="MessageBoxResult.No"/>.</returns>
    public Task<MessageBoxResult> AreYouSureYouWantToDeleteTheCoverImageMessageBox(string fileNameWithoutExtension)
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    /// <summary>
    /// Returns <see cref="MessageBoxResult.No"/> without displaying a message box.
    /// </summary>
    /// <returns><see cref="MessageBoxResult.No"/>.</returns>
    public Task<MessageBoxResult> WouldYouLikeToSaveAReportMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    /// <summary>
    /// Does nothing. Does not display the unable-to-restore-backup message box.
    /// </summary>
    public Task SimpleLauncherWasUnableToRestoreBackupMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns <see cref="MessageBoxResult.No"/> without displaying a message box.
    /// </summary>
    /// <returns><see cref="MessageBoxResult.No"/>.</returns>
    public Task<MessageBoxResult> WouldYouLikeToRestoreTheLastBackupMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-load-language-resource message box.
    /// </summary>
    public Task FailedToLoadLanguageResourceMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the invalid-system-configuration message box.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    public Task InvalidSystemConfigurationMessageBox(string errorMessage)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the unable-to-open-link message box.
    /// </summary>
    public Task UnableToOpenLinkMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-game-found-in-random-selection message box.
    /// </summary>
    public Task NoGameFoundInTheRandomSelectionMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the please-select-a-system-before message box.
    /// </summary>
    public Task PleaseSelectASystemBeforeMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the toggle-fuzzy-matching-failure message box.
    /// </summary>
    public Task ToggleFuzzyMatchingFailureMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the fuzzy-matching-error-set-threshold message box.
    /// </summary>
    public Task FuzzyMatchingErrorFailToSetThresholdMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the list-of-errors message box.
    /// </summary>
    /// <param name="errorMessages">The error messages.</param>
    public Task ListOfErrorsMessageBox(StringBuilder errorMessages)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-update-available message box.
    /// </summary>
    /// <param name="currentVersion">The current version string.</param>
    public Task ThereIsNoUpdateAvailableMessageBox(string currentVersion)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the another-instance-is-running message box.
    /// </summary>
    public Task AnotherInstanceIsRunningMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-start message box.
    /// </summary>
    public Task FailedToStartSimpleLauncherMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-restart message box.
    /// </summary>
    public Task FailedToRestartMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns <see cref="MessageBoxResult.No"/> without displaying a message box.
    /// </summary>
    /// <param name="currentVersion">The current version string.</param>
    /// <param name="latestVersion">The latest version string.</param>
    /// <returns><see cref="MessageBoxResult.No"/>.</returns>
    public Task<MessageBoxResult> DoYouWantToUpdateMessageBox(string currentVersion, string latestVersion)
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    /// <summary>
    /// Does nothing. Does not display the handle-missing-required-files message box.
    /// </summary>
    /// <param name="fileList">The list of missing files.</param>
    public Task HandleMissingRequiredFilesMessageBox(string fileList)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the API-config-error message box.
    /// </summary>
    /// <param name="reason">The error reason.</param>
    public Task HandleApiConfigErrorMessageBox(string reason)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the disk-space-error message box.
    /// </summary>
    public Task DiskSpaceErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the could-not-check-disk-space message box.
    /// </summary>
    public Task CouldNotCheckForDiskSpaceMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the save-system-failed message box.
    /// </summary>
    /// <param name="details">Optional error details.</param>
    public Task SaveSystemFailedMessageBox(string? details = null)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the could-not-open-download-link message box.
    /// </summary>
    public Task CouldNotOpenTheDownloadLinkMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-loading-app-settings message box.
    /// </summary>
    public Task ErrorLoadingAppSettingsMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the path-manipulation-detected message box.
    /// </summary>
    /// <param name="archivePath">The archive path that triggered the detection.</param>
    public Task PotentialPathManipulationDetectedMessageBox(string archivePath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the could-not-open-sound-configuration-window message box.
    /// </summary>
    public Task CouldNotOpenSoundConfigurationWindowMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-setting-sound-file message box.
    /// </summary>
    public Task ErrorSettingSoundFileMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the notification-sound-disabled message box.
    /// </summary>
    public Task NotificationSoundIsDisableMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-sound-file-selected message box.
    /// </summary>
    public Task NoSoundFileIsSelectedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the settings-saved-successfully message box.
    /// </summary>
    public Task SettingsSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-save-settings message box.
    /// </summary>
    public Task FailedToSaveSettingsMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the file-path-is-invalid message box.
    /// </summary>
    /// <param name="logPath">The path to the log file.</param>
    public Task FilePathIsInvalidMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-mounting-file message box.
    /// </summary>
    /// <param name="exitCode">The optional exit code.</param>
    public Task ThereWasAnErrorMountingTheFileMessageBox(int? exitCode = null)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Dokan-driver-not-installed message box.
    /// </summary>
    public Task DokanDriverNotInstalledMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the launch-tool-information message box.
    /// </summary>
    /// <param name="info">The information text.</param>
    public Task LaunchToolInformationMessageBox(string info)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the cannot-screenshot-minimized-window message box.
    /// </summary>
    public Task CannotScreenshotMinimizedWindowMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-copy-log-content message box.
    /// </summary>
    public Task FailedToCopyLogContentMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the could-not-find-updater-on-GitHub message box.
    /// </summary>
    public Task CouldNotFindUpdaterOnGitHubMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the could-not-open-achievements-window message box.
    /// </summary>
    public Task CouldNotOpenAchievementsWindowMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns <see cref="MessageBoxResult.No"/> without displaying a message box.
    /// </summary>
    /// <returns><see cref="MessageBoxResult.No"/>.</returns>
    public Task<MessageBoxResult> GameNotSupportedByRetroAchievementsMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    /// <summary>
    /// Does nothing. Does not display the game-launch-timeout message box.
    /// </summary>
    public Task GameLaunchTimeoutMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the add-RA-login message box.
    /// </summary>
    public Task AddRaLoginMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the no-default-browser-configured message box.
    /// </summary>
    public Task NoDefaultBrowserConfiguredMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns <see cref="MessageBoxResult.No"/> without displaying a message box.
    /// </summary>
    /// <returns><see cref="MessageBoxResult.No"/>.</returns>
    public Task<MessageBoxResult> WarnUserAboutMemoryConsumptionMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    /// <summary>
    /// Does nothing. Does not display the group-by-folder-only-for-MAME-and-DOSBox message box.
    /// </summary>
    public Task GroupByFolderOnlyForMameAndDosBoxMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns <see cref="MessageBoxResult.No"/> without displaying a message box.
    /// </summary>
    /// <returns><see cref="MessageBoxResult.No"/>.</returns>
    public Task<MessageBoxResult> GroupByFolderWarningMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    /// <summary>
    /// Returns <see cref="MessageBoxResult.No"/> without displaying a message box.
    /// </summary>
    /// <returns><see cref="MessageBoxResult.No"/>.</returns>
    public Task<MessageBoxResult> FirstRunWelcomeMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    /// <summary>
    /// Does nothing. Does not display the emulator-1-location-required message box.
    /// </summary>
    public Task Emulator1LocationRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the emulator-2-location-required message box.
    /// </summary>
    public Task Emulator2LocationRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the emulator-3-location-required message box.
    /// </summary>
    public Task Emulator3LocationRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the emulator-4-location-required message box.
    /// </summary>
    public Task Emulator4LocationRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the emulator-5-location-required message box.
    /// </summary>
    public Task Emulator5LocationRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the image-pack-downloader-unavailable message box.
    /// </summary>
    public Task ImagePackDownloaderUnavailableMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the easy-mode-unavailable message box.
    /// </summary>
    public Task EasyModeUnavailableMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the RA-hash-not-supported-for-grouped-system message box.
    /// </summary>
    public Task SimpleLauncherDoesNotSupportRaHashOfSystemGroupedByFolderMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the unsupported-architecture message box.
    /// </summary>
    public Task UnsupportedArchitectureMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the 7zip-dll-not-found message box.
    /// </summary>
    public Task SevenZipDllNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-initialize-7zip message box.
    /// </summary>
    public Task FailedToInitializeSevenZipMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the extraction-failed message box.
    /// </summary>
    /// <param name="tempFolderPath">The temporary folder path.</param>
    public Task ShowExtractionFailedMessageBoxAsync(string tempFolderPath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the download-file-locked message box.
    /// </summary>
    /// <param name="tempFolderPath">The temporary folder path.</param>
    public Task ShowDownloadFileLockedMessageBoxAsync(string tempFolderPath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display a custom message box.
    /// </summary>
    /// <param name="message">The message text.</param>
    /// <param name="launchError">The launch error text.</param>
    /// <param name="logPath">The path to the log file.</param>
    public Task ShowCustomMessageBox(string message, string launchError, string logPath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the enter-valid-search-terms message box.
    /// </summary>
    public Task EnterValidSearchTermsMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the operation-cancelled message box.
    /// </summary>
    public Task OperationCancelledMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns <see cref="MessageBoxResult.No"/> without displaying a message box.
    /// </summary>
    /// <returns><see cref="MessageBoxResult.No"/>.</returns>
    public Task<MessageBoxResult> DoYouWantToCancelAndCloseMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    /// <summary>
    /// Does nothing. Does not display the could-not-open-browser-for-AI-support message box.
    /// </summary>
    public Task CouldNotOpenBrowserForAiSupportMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the PowerShell-execution-policy-restrictions message box.
    /// </summary>
    public Task PowerShellExecutionPolicyRestrictionsMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the unable-to-mount-ISO message box.
    /// </summary>
    public Task UnabletomountIsOfileMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the unable-to-dismount-ISO message box.
    /// </summary>
    public Task UnabletoDismountIsOfileMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the application-control-policy-blocked message box.
    /// </summary>
    public Task ApplicationControlPolicyBlockedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the application-control-policy-blocked-manual-link message box.
    /// </summary>
    /// <param name="url">The blocked URL.</param>
    public Task ApplicationControlPolicyBlockedManualLinkMessageBox(string url)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the enter-RA-username message box.
    /// </summary>
    public Task EnterYourRetroAchievementsUsernameMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the emulator-configured-successfully message box.
    /// </summary>
    public Task EmulatorConfiguredSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-configure-emulator message box.
    /// </summary>
    public Task FailedToConfigureTheEmulatorMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the error-configuring-emulator message box.
    /// </summary>
    public Task AnErrorOccurredWhileConfiguringTheEmulatorMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-login-to-RA message box.
    /// </summary>
    public Task FailedToLoginToRetroAchievementsMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the file-system-xml-is-locked message box.
    /// </summary>
    public Task FileSystemXmlIsLockedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-MAME-configuration message box.
    /// </summary>
    public Task FailedToInjectMameConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the MAME-configuration-injected-successfully message box.
    /// </summary>
    public Task MameConfigurationInjectedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-MAME-configuration-2 message box.
    /// </summary>
    public Task FailedtoinjectMamEconfiguration2MessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the MAME-emulator-path-not-found message box.
    /// </summary>
    public Task MameEmulatorPathNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the RetroArch-emulator-path-not-found message box.
    /// </summary>
    public Task RetroArchemulatorpathnotfoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-RetroArch-configuration message box.
    /// </summary>
    public Task FailedtoinjectRetroArchconfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the RetroArch-configuration-injected-successfully message box.
    /// </summary>
    public Task RetroArchConfigurationInjectedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-RetroArch-configuration-2 message box.
    /// </summary>
    public Task FailedtoinjectRetroArchconfiguration2MessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Xenia-emulator-path-not-found message box.
    /// </summary>
    public Task XeniaemulatorpathnotfoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-Xenia-configuration message box.
    /// </summary>
    public Task FailedtoinjectXeniaconfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Xenia-configuration-injected-successfully message box.
    /// </summary>
    public Task XeniaconfigurationinjectedsuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-Xenia-configuration-2 message box.
    /// </summary>
    public Task FailedtoinjectXeniaconfiguration2MessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the enter-username-password message box.
    /// </summary>
    public Task EnterUsernamePasswordMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Ares-emulator-not-found message box.
    /// </summary>
    public Task AresemulatornotfoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Daphne-settings-saved-successfully message box.
    /// </summary>
    public Task DaphnesettingssavedsuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the PCSX2-settings-saved message box.
    /// </summary>
    public Task Pcsx2SettingssavedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the settings-saved message box.
    /// </summary>
    public Task SettingsSavedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Cemu-emulator-not-found message box.
    /// </summary>
    public Task CemuEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-Ares-configuration message box.
    /// </summary>
    public Task FailedtoinjectAresconfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Cemu-configuration-saved message box.
    /// </summary>
    public Task CemuConfigurationSavedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Flycast-emulator-not-found message box.
    /// </summary>
    public Task FlycastEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Ares-configuration-saved-successfully message box.
    /// </summary>
    public Task AresConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-save-Ares-configuration message box.
    /// </summary>
    public Task FailedToSaveAresConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-Flycast-configuration message box.
    /// </summary>
    public Task FailedToInjectFlycastConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Flycast-configuration-saved-successfully message box.
    /// </summary>
    public Task FlycastConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Dolphin-emulator-not-found message box.
    /// </summary>
    public Task DolphinEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-save-Flycast-configuration message box.
    /// </summary>
    public Task FailedToSaveFlycastConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-Dolphin-configuration message box.
    /// </summary>
    public Task FailedToInjectDolphinConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Dolphin-configuration-saved-successfully message box.
    /// </summary>
    public Task DolphinConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-save-Dolphin-configuration message box.
    /// </summary>
    public Task FailedToSaveDolphinConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Sega-Model-2-emulator-not-found message box.
    /// </summary>
    public Task SegaModel2EmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-Sega-Model-2-configuration message box.
    /// </summary>
    public Task FailedToInjectSegaModel2ConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Sega-Model-2-configuration-saved-successfully message box.
    /// </summary>
    public Task SegaModel2ConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the BlastEm-emulator-not-found message box.
    /// </summary>
    public Task BlastemEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-BlastEm-configuration message box.
    /// </summary>
    public Task FailedToInjectBlastemConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the BlastEm-configuration-saved-successfully message box.
    /// </summary>
    public Task BlastemConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-save-Sega-Model-2-configuration message box.
    /// </summary>
    public Task FailedToSaveSegaModel2ConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-save-BlastEm-configuration message box.
    /// </summary>
    public Task FailedToSaveBlastemConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the RPCS3-emulator-not-found message box.
    /// </summary>
    public Task Rpcs3EmulatorNotFoundPleaseLocateMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-RPCS3-configuration message box.
    /// </summary>
    public Task FailedToInjectRpcs3ConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the RPCS3-configuration-saved-successfully message box.
    /// </summary>
    public Task Rpcs3ConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-save-RPCS3-configuration message box.
    /// </summary>
    public Task FailedToSaveRpcs3ConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Stella-emulator-not-found message box.
    /// </summary>
    public Task StellaEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-Stella-configuration message box.
    /// </summary>
    public Task FailedToInjectStellaConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Supermodel-emulator-not-found message box.
    /// </summary>
    public Task SupermodelEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Stella-configuration-saved-successfully message box.
    /// </summary>
    public Task StellaConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-Supermodel-configuration message box.
    /// </summary>
    public Task FailedToInjectSupermodelConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-save-Stella-configuration message box.
    /// </summary>
    public Task FailedToSaveStellaConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Supermodel-configuration-saved-successfully message box.
    /// </summary>
    public Task SupermodelConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-save-Supermodel-configuration message box.
    /// </summary>
    public Task FailedToSaveSupermodelConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Mednafen-emulator-not-found message box.
    /// </summary>
    public Task MednafenEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Mesen-emulator-not-found message box.
    /// </summary>
    public Task MesenEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-Mednafen-configuration message box.
    /// </summary>
    public Task FailedToInjectMednafenConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-Mesen-configuration message box.
    /// </summary>
    public Task FailedToInjectMesenConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the DuckStation-emulator-not-found message box.
    /// </summary>
    public Task DuckStationEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Mednafen-configuration-saved-successfully message box.
    /// </summary>
    public Task MednafenConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-save-Mednafen-configuration message box.
    /// </summary>
    public Task FailedToSaveMednafenConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-DuckStation-configuration message box.
    /// </summary>
    public Task FailedToInjectDuckStationConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the DuckStation-configuration-saved-successfully message box.
    /// </summary>
    public Task DuckStationConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-save-Mesen-configuration message box.
    /// </summary>
    public Task FailedToSaveMesenConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-save-DuckStation-configuration message box.
    /// </summary>
    public Task FailedToSaveDuckStationConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Mesen-configuration-saved-successfully message box.
    /// </summary>
    public Task MesenConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-Ymir-configuration message box.
    /// </summary>
    public Task FailedToInjectYumirConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Ymir-configuration-saved-successfully message box.
    /// </summary>
    public Task YumirConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Raine-settings-saved-and-injected message box.
    /// </summary>
    public Task RaineSettingsSavedAndInjectedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Raine-executable-not-found message box.
    /// </summary>
    public Task RaineExecutableNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Ymir-emulator-not-found message box.
    /// </summary>
    public Task YumirEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the ReDream-emulator-path-not-found message box.
    /// </summary>
    public Task ReDreamEmulatorPathNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-inject-ReDream-configuration message box.
    /// </summary>
    public Task FailedToInjectReDreamConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the ReDream-configuration-injected-successfully message box.
    /// </summary>
    public Task ReDreamConfigurationInjectedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the could-not-launch-game-due-to-DEP-violation message box.
    /// </summary>
    public Task CouldNotLaunchGameDueToDepViolationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the MAME-ROM-set-error message box.
    /// </summary>
    public Task MameRomSetErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the MAME-unknown-system-error message box.
    /// </summary>
    public Task MameUnknownSystemErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the MAME-unable-to-load-image message box.
    /// </summary>
    public Task MameUnableToLoadImageMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Ootake-does-not-support-image-files message box.
    /// </summary>
    public Task OotakeDoesNotSupportImageFilesMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Geolith-does-not-support-compressed-files message box.
    /// </summary>
    public Task GeolithDoesNotSupportCompressedFilesMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the RetroArch-parameter-should-contain-L message box.
    /// </summary>
    public Task RetroArchParameterShouldContainLMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the RetroArch-parameter-issue message box.
    /// </summary>
    /// <param name="logPath">The path to the log file.</param>
    public Task RetroArchParameterIssueMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the RetroArch-special-characters-in-path message box.
    /// </summary>
    public Task RetroArchSpecialCharactersInPathMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Azahar-configuration-injection-permission-error message box.
    /// </summary>
    public Task AzaharConfigurationInjectionPermissionErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Azahar-configuration-saved-successfully message box.
    /// </summary>
    public Task AzaharConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-save-Azahar-configuration message box.
    /// </summary>
    public Task FailedToSaveAzaharConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Xemu-parameter-should-contain-DVD-path message box.
    /// </summary>
    public Task XemuParameterShouldContainDvdPathMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the please-extract-application-first message box.
    /// </summary>
    public Task PleaseExtractApplicationFirstMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the injection-failed-generic message box.
    /// </summary>
    public Task InjectionFailedGenericMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the Daphne-configuration-save-failed message box.
    /// </summary>
    public Task DaphneConfigurationSaveFailedMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the image-download-timeout message box.
    /// </summary>
    public Task ShowImageDownloadTimeoutMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the system-name-required-before-choosing-image message box.
    /// </summary>
    public Task SystemNameRequiredBeforeChoosingImageMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the invalid-image-format message box.
    /// </summary>
    public Task InvalidImageFormatMessageBox()
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display the failed-to-copy-system-image message box.
    /// </summary>
    /// <param name="errorMessage">The error message.</param>
    public Task FailedToCopySystemImageMessageBox(string errorMessage)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display a warning message box.
    /// </summary>
    /// <param name="message">The warning message.</param>
    public Task WarningMessageBox(string message)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Does nothing. Does not display a custom error message box.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="title">The message box title.</param>
    public Task CustomErrorMessageBox(string message, string title)
    {
        return Task.CompletedTask;
    }

    /// <summary>
    /// Returns <see langword="false"/> without displaying a message box.
    /// </summary>
    /// <param name="title">The message box title.</param>
    /// <param name="message">The question message.</param>
    /// <returns><see langword="false"/>.</returns>
    public Task<bool> CustomQuestionMessageBox(string title, string message)
    {
        return Task.FromResult(false);
    }
}
