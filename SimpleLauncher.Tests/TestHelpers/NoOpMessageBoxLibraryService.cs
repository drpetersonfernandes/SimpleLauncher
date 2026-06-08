using System.Text;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Tests.TestHelpers;

public class NoOpMessageBoxLibraryService : IMessageBoxLibraryService
{
    public Task TakeScreenShotMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task CouldNotSaveScreenshotMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task GameIsAlreadyInFavoritesMessageBox(string fileNameWithExtension)
    {
        return Task.CompletedTask;
    }

    public Task ErrorWhileAddingFavoritesMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorWhileRemovingGameFromFavoriteMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorOpeningTheUpdateHistoryWindowMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorOpeningVideoLinkMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ProblemOpeningInfoLinkMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorOpeningUrlMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoCoverMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoTitleSnapshotMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoGameplaySnapshotMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoCartMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoVideoFileMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task CouldNotOpenManualMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task NoPdfViewerInstalledMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoManualMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoWalkthroughMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoCabinetMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoFlyerMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoPcbMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FileSuccessfullyDeletedMessageBox(string fileNameWithExtension)
    {
        return Task.CompletedTask;
    }

    public Task FileCouldNotBeDeletedMessageBox(string fileNameWithExtension)
    {
        return Task.CompletedTask;
    }

    public Task DefaultImageNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task GlobalSearchErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task PleaseEnterSearchTermMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorLaunchingGameMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    public Task SelectAGameToLaunchMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FileAddedToFavoritesMessageBox(string fileNameWithoutExtension)
    {
        return Task.CompletedTask;
    }

    public Task FileRemovedFromFavoritesMessageBox(string fileNameWithoutExtension)
    {
        return Task.CompletedTask;
    }

    public Task CouldNotLaunchThisGameMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    public Task ProtocolHandlerNotRegisteredMessageBox(string protocol)
    {
        return Task.CompletedTask;
    }

    public Task EmulatorPathNotConfiguredMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorCalculatingStatsMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedSaveReportMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ReportSavedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task NoStatsToSaveMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorLaunchingToolMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    public Task SelectedToolNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task NoFavoriteFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task MoveToWritableFolderMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task InvalidSystemConfigMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorMethodLoadGameFilesAsyncMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorOpeningDonationLinkMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ToggleGamepadFailureMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ToolLaunchWasCanceledByUserMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorChangingViewModeMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task NavigationButtonErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SelectSystemBeforeSearchMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task EnterSearchQueryMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorWhileLoadingHelpUserXmlMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task NoSystemInHelpUserXmlMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> CouldNotLoadHelpUserXmlMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    public Task FailedToLoadHelpUserXmlMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FileHelpUserXmlIsMissingMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorWhileLoadingParametersMdMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task NoSystemInParametersMdMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToLoadParametersMdMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FileParametersMdIsMissingMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FileParametersMdIsEmptyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ImageViewerErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ReinstallSimpleLauncherFileCorruptedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ReinstallSimpleLauncherFileMissingMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorCheckingForUpdatesMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorLoadingRomHistoryMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task NoHistoryXmlOrDatFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorOpeningBrowserMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SystemXmlIsCorruptedMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    public Task WouldYouLikeToOpenTheLogMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    public Task FileSystemXmlIsCorruptedMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    public Task InstallUpdateManuallyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task UpdaterLaunchFailedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task RequiredFileMissingMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task EnterSupportRequestMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task EnterNameMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task EnterEmailMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ApiKeyErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SupportRequestSuccessMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SupportRequestSendErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ExtractionFailedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FileNeedToBeCompressedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task DownloadedFileIsMissingMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FileIsLockedMessageBox(string tempFolderPath)
    {
        return Task.CompletedTask;
    }

    public Task LinksSavedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task DeadZonesSavedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task LinksRevertedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task MainWindowSearchEngineErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task DownloadExtractionFailedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task DownloadAndExtrationWereSuccessfulMessageBox()
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

    public Task SelectAHistoryItemToRemoveMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> ReallyWantToRemoveAllPlayHistoryMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    public Task SystemAddedMessageBox(string systemName, string resolvedSystemFolder, string resolvedSystemImageFolder)
    {
        return Task.CompletedTask;
    }

    public Task AddSystemFailedMessageBox(string? details = null)
    {
        return Task.CompletedTask;
    }

    public Task RightClickContextMenuErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task GameFileDoesNotExistMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> GameFileDoesNotExistAskToDeleteMessageBox(string filePath)
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    public Task<MessageBoxResult> FavoriteFileDoesNotExistAskToDeleteMessageBox(string filePath)
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    public Task CouldNotOpenHistoryWindowMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task CouldNotOpenWalkthroughMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SelectAFavoriteToRemoveMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SystemXmlNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task YouCanAddANewSystemMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task EmulatorNameRequiredMessageBox(int i)
    {
        return Task.CompletedTask;
    }

    public Task EmulatorNameIsRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task EmulatorNameMustBeUniqueMessageBox(string emulatorName)
    {
        return Task.CompletedTask;
    }

    public Task SystemSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task PathOrParameterInvalidMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task Emulator1RequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ExtensionToLaunchIsRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ExtensionToSearchIsRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FileMustBeCompressedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SystemImageFolderCanNotBeEmptyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SystemFolderCanNotBeEmptyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SystemNameCanNotBeEmptyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task InvalidSystemNameCharactersMessageBox(string invalidChars)
    {
        return Task.CompletedTask;
    }

    public Task InvalidFolderCharactersMessageBox(string invalidChars)
    {
        return Task.CompletedTask;
    }

    public Task FolderCreationFailedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SelectASystemToDeleteMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SystemNotFoundInTheXmlMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorFindingGameFilesMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    public Task GamePadErrorMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    public Task CouldNotLaunchGameMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    public Task InvalidOperationExceptionMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    public Task ThereWasAnErrorLaunchingThisGameMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    public Task BatchFileFailedMessageBox(string batchFilePath, string errorDetail, string logPath, int? exitCode = null)
    {
        return Task.CompletedTask;
    }

    public Task<bool> BatchFilePathsMissingMessageBox(List<string> missingPaths)
    {
        return Task.FromResult(false);
    }

    public Task ElevationRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task NullFileExtensionMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task CouldNotFindAFileMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> SearchOnlineForRomHistoryMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    public Task SystemHasBeenDeletedMessageBox(string selectedSystemName)
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> AreYouSureDoYouWantToDeleteThisSystemMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    public Task ThereWasAnErrorDeletingTheGameMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ThereWasAnErrorDeletingTheCoverImageMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> AreYouSureYouWantToDeleteTheGameMessageBox(string fileNameWithExtension)
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    public Task<MessageBoxResult> AreYouSureYouWantToDeleteTheCoverImageMessageBox(string fileNameWithoutExtension)
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    public Task<MessageBoxResult> WoulYouLikeToSaveAReportMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    public Task SimpleLauncherWasUnableToRestoreBackupMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> WouldYouLikeToRestoreTheLastBackupMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    public Task FailedToLoadLanguageResourceMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task InvalidSystemConfigurationMessageBox(string errorMessage)
    {
        return Task.CompletedTask;
    }

    public Task UnableToOpenLinkMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task NoGameFoundInTheRandomSelectionMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task PleaseSelectASystemBeforeMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ToggleFuzzyMatchingFailureMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FuzzyMatchingErrorFailToSetThresholdMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ListOfErrorsMessageBox(StringBuilder errorMessages)
    {
        return Task.CompletedTask;
    }

    public Task ThereIsNoUpdateAvailableMessageBox(string currentVersion)
    {
        return Task.CompletedTask;
    }

    public Task AnotherInstanceIsRunningMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToStartSimpleLauncherMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToRestartMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> DoYouWantToUpdateMessageBox(string currentVersion, string latestVersion)
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    public Task HandleMissingRequiredFilesMessageBox(string fileList)
    {
        return Task.CompletedTask;
    }

    public Task HandleApiConfigErrorMessageBox(string reason)
    {
        return Task.CompletedTask;
    }

    public Task DiskSpaceErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task CouldNotCheckForDiskSpaceMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SaveSystemFailedMessageBox(string? details = null)
    {
        return Task.CompletedTask;
    }

    public Task CouldNotOpenTheDownloadLinkMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorLoadingAppSettingsMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task PotentialPathManipulationDetectedMessageBox(string archivePath)
    {
        return Task.CompletedTask;
    }

    public Task CouldNotOpenSoundConfigurationWindowMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ErrorSettingSoundFileMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task NotificationSoundIsDisableMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task NoSoundFileIsSelectedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SettingsSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveSettingsMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FilePathIsInvalidMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    public Task ThereWasAnErrorMountingTheFileMessageBox(int? exitCode = null)
    {
        return Task.CompletedTask;
    }

    public Task DokanDriverNotInstalledMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task LaunchToolInformationMessageBox(string info)
    {
        return Task.CompletedTask;
    }

    public Task CannotScreenshotMinimizedWindowMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToCopyLogContentMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task CouldNotFindUpdaterOnGitHubMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task CouldNotOpenAchievementsWindowMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> GameNotSupportedByRetroAchievementsMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    public Task GameLaunchTimeoutMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task AddRaLoginMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task NoDefaultBrowserConfiguredMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> WarnUserAboutMemoryConsumptionMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    public Task GroupByFolderOnlyForMameAndDosBoxMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> GroupByFolderWarningMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    public Task<MessageBoxResult> FirstRunWelcomeMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    public Task Emulator1LocationRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task Emulator2LocationRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task Emulator3LocationRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task Emulator4LocationRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task Emulator5LocationRequiredMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ImagePackDownloaderUnavailableMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task EasyModeUnavailableMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SimpleLauncherDoesNotSupportRaHashOfSystemGroupedByFolderMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task UnsupportedArchitectureMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SevenZipDllNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInitializeSevenZipMessageBox()
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

    public Task ShowCustomMessageBox(string message, string launchError, string logPath)
    {
        return Task.CompletedTask;
    }

    public Task EnterValidSearchTermsMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task OperationCancelledMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task<MessageBoxResult> DoYouWantToCancelAndCloseMessageBox()
    {
        return Task.FromResult(MessageBoxResult.No);
    }

    public Task CouldNotOpenBrowserForAiSupportMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task PowerShellExecutionPolicyRestrictionsMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task UnabletomountIsOfileMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task UnabletoDismountIsOfileMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ApplicationControlPolicyBlockedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ApplicationControlPolicyBlockedManualLinkMessageBox(string url)
    {
        return Task.CompletedTask;
    }

    public Task EnterYourRetroAchievementsUsernameMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task EmulatorConfiguredSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToConfigureTheEmulatorMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task AnErrorOccurredWhileConfiguringTheEmulatorMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToLoginToRetroAchievementsMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FileSystemXmlIsLockedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectMameConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task MamEconfigurationinjectedsuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedtoinjectMamEconfiguration2MessageBox()
    {
        return Task.CompletedTask;
    }

    public Task MameEmulatorPathNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task RetroArchemulatorpathnotfoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedtoinjectRetroArchconfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task RetroArchconfigurationinjectedsuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedtoinjectRetroArchconfiguration2MessageBox()
    {
        return Task.CompletedTask;
    }

    public Task XeniaemulatorpathnotfoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedtoinjectXeniaconfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task XeniaconfigurationinjectedsuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedtoinjectXeniaconfiguration2MessageBox()
    {
        return Task.CompletedTask;
    }

    public Task EnterUsernamePasswordMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task AresemulatornotfoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task DaphnesettingssavedsuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task Pcsx2SettingssavedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SettingsSavedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task CemuemulatornotfoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedtoinjectAresconfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task CemuConfigurationSavedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FlycastEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task AresConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveAresConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectFlycastConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FlycastConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task DolphinEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveFlycastConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectDolphinConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task DolphinConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveDolphinConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SegaModel2EmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectSegaModel2ConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SegaModel2ConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task BlastemEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectBlastemConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task BlastemConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveSegaModel2ConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveBlastemConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task Rpcs3EmulatorNotFoundPleaseLocateMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectRpcs3ConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task Rpcs3ConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveRpcs3ConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task StellaEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectStellaConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SupermodelEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task StellaConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectSupermodelConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveStellaConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SupermodelConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveSupermodelConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task MednafenEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task MesenEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectMednafenConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectMesenConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task DuckStationEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task MednafenConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveMednafenConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectDuckStationConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task DuckStationConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveMesenConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveDuckStationConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task MesenConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectYumirConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task YumirConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task RaineSettingsSavedAndInjectedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task RaineExecutableNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task YumirEmulatorNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ReDreamEmulatorPathNotFoundMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToInjectReDreamConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ReDreamConfigurationInjectedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task CouldNotLaunchGameDueToDepViolationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task MameRomSetErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task MameUnknownSystemErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task MameUnableToLoadImageMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task OotakeDoesNotSupportImageFilesMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task GeolithDoesNotSupportCompressedFilesMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task RetroArchParameterShouldContainLMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task RetroArchParameterIssueMessageBox(string logPath)
    {
        return Task.CompletedTask;
    }

    public Task RetroArchSpecialCharactersInPathMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task AzaharConfigurationInjectionPermissionErrorMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task AzaharConfigurationSavedSuccessfullyMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToSaveAzaharConfigurationMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task XemuParameterShouldContainDvdPathMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task PleaseExtractApplicationFirstMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task InjectionFailedGenericMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task DaphneConfigurationSaveFailedMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task ShowImageDownloadTimeoutMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task SystemNameRequiredBeforeChoosingImageMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task InvalidImageFormatMessageBox()
    {
        return Task.CompletedTask;
    }

    public Task FailedToCopySystemImageMessageBox(string errorMessage)
    {
        return Task.CompletedTask;
    }

    public Task WarningMessageBox(string message)
    {
        return Task.CompletedTask;
    }

    public Task CustomErrorMessageBox(string message, string title)
    {
        return Task.CompletedTask;
    }

    public Task<bool> CustomQuestionMessageBox(string title, string message)
    {
        return Task.FromResult(false);
    }
}
