using System.Diagnostics;
using System.Globalization;
using System.Text;
using Avalonia.Controls;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Avalonia.Services.QuitOrReinstall;
using SimpleLauncher.Avalonia.Views;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Avalonia.Services.AvaloniaServices;

public class MessageBoxLibraryService : IMessageBoxLibraryService
{
    private readonly IConfiguration _configuration;
    private readonly IWindowContext _ctx;

    public MessageBoxLibraryService(IWindowContext c, IConfiguration configuration)
    {
        _ctx = c;
        _configuration = configuration;
    }

    private Window? O => _ctx.PlatformWindow as Window;


    public async Task ListOfErrorsMessageBoxAsync(StringBuilder errorMessages)
    {
        if (O != null) await ShowAsync(O, errorMessages.ToString(), "Errors", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FailedToInjectYumirConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to save Yumir configuration. Please check file permissions.", "Save Failed",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task Pcsx2SettingssavedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "PCSX2 settings saved.", "Success", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ShowEmulatorDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task InvalidSystemConfigurationMessageBoxAsync(string errorMessage)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, errorMessage, "Invalid System Configuration", MessageButtons.Ok, MessageIcon.Warning);
    }

    public async Task<MessageBoxResult> CouldNotLoadHelpUserXmlMessageBoxAsync()
    {
        if (O == null) return MessageBoxResult.Cancel;
        return await ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task MednafenConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Mednafen configuration saved successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public async Task CustomErrorMessageBoxAsync(string message, string title)
    {
        if (O != null) await ShowAsync(O, message, title, MessageButtons.Ok, MessageIcon.Error);
    }


    public async Task CustomInfoMessageBoxAsync(string title, string message)
    {
        if (O != null) await ShowAsync(O, message, title, MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ErrorOpeningTheUpdateHistoryWindowMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            $"Error opening the Update History window.\n\n" +
            $"The error was reported to the developer who will try to fix the issue.", "Error", MessageButtons.Ok,
            MessageIcon.Error);
    }


    public Task RaineExecutableNotFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Raine executable not found. Please select it.", "Error", MessageButtons.Ok,
            MessageIcon.Error);
    }


    public Task PleaseExtractApplicationFirstMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ApplicationControlPolicyBlockedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ThereIsNoFlyerMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "There is no flyer file associated with this game.", "Flyer not found", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task FailedToSaveMesenConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to save Mesen configuration. Please check file permissions.", "Save Failed",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task SystemNotFoundInTheXmlMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Selected system not found in the XML document!", "Alert", MessageButtons.Ok,
            MessageIcon.Warning);
    }


    public Task PotentialPathManipulationDetectedMessageBoxAsync(string archivePath)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ExtractionFailedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public async Task ErrorWhileRemovingGameFromFavoriteMessageBoxAsync()
    {
        if (O != null)
            await ShowAsync(O, "Error removing from favorites.", "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public async Task FileAddedToFavoritesMessageBoxAsync(string fileNameWithoutExtension)
    {
        if (O != null)
            await ShowAsync(O, fileNameWithoutExtension + " added to favorites.", "Added", MessageButtons.Ok,
                MessageIcon.Information);
    }


    public async Task FileRemovedFromFavoritesMessageBoxAsync(string fileNameWithoutExtension)
    {
        if (O != null)
            await ShowAsync(O, fileNameWithoutExtension + " removed from favorites.", "Removed", MessageButtons.Ok,
                MessageIcon.Information);
    }


    public Task GameFileDoesNotExistMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task CouldNotCheckForDiskSpaceMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "'Simple Launcher' could not check disk space for the specified path. Please check the path and try again.",
            "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public async Task ReinstallSimpleLauncherFileMissingMessageBoxAsync()
    {
        if (O == null) return;

        var result = await ShowAsync(O,
            "The file 'mame.dat' could not be found in the application folder.\n\n" +
            "Do you want to automatically reinstall 'Simple Launcher' to fix it?",
            "Error", MessageButtons.YesNo, MessageIcon.Question);

        if (result == MessageBoxResult.Yes)
            _ = App.ServiceProvider.GetRequiredService<AvaloniaCheckForUpdatesService>().ReinstallAndShutdownAsync();
    }

    public Task FailedToStartSimpleLauncherMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "Failed to start 'Simple Launcher'. An error occurred while checking for existing instances.",
            "Simple Launcher Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task RequiredFileMissingMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Please reinstall 'Simple Launcher' manually to fix the issue.", "Warning",
            MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task CouldNotOpenBrowserForAiSupportMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Could not open browser for AI support.", "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task StellaEmulatorNotFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Stella emulator not found. Please locate 'stella.exe'.", "Emulator Not Found",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task ThereWasAnErrorDeletingTheGameMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task PleaseEnterSearchTermMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Please enter a search term.", "Warning", MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task ShowCoreDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }

    public async Task<MessageBoxResult> WarnUserAboutMemoryConsumptionMessageBoxAsync()
    {
        if (O == null) return MessageBoxResult.Cancel;
        return await ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ErrorLoadingAppSettingsMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task CouldNotSaveScreenshotMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task NoHistoryXmlOrDatFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task SystemNameCanNotBeEmptyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task SupermodelEmulatorNotFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Supermodel emulator not found. Please locate 'Supermodel.exe'.", "Emulator Not Found",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task ToolLaunchWasCanceledByUserMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "The launch of the selected tool was canceled by the user.", "Info", MessageButtons.Ok,
            MessageIcon.Information);
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
            try
            {
                Process.Start(new ProcessStartInfo { FileName = logPath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open the error log file.");
            }
    }


    public Task FailedToInjectMednafenConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to inject Mednafen configuration. Please check file permissions and try again.",
            "Injection Failed", MessageButtons.Ok, MessageIcon.Error);
    }


    public async Task ErrorLaunchingGameMessageBoxAsync(string? logPath)
    {
        var msg = string.IsNullOrEmpty(logPath) ? "An unknown error occurred." : logPath;
        if (O != null) await ShowAsync(O, msg, "Launch Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task ReDreamConfigurationInjectedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "ReDream configuration saved successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task FailedToLoadParametersMdMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task CouldNotLaunchThisGameMessageBoxAsync(string? logPath)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "The file 'error_user.log' was not found!", "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task EnterYourRetroAchievementsUsernameMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "Please enter your RetroAchievements username, API key, and password before configuring an emulator.",
            "Credentials Required", MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task FailedtoinjectRetroArchconfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "Failed to inject RetroArch configuration. The error has been logged. Please check the emulator path and try again.",
            "Injection Error", MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task InvalidOperationExceptionMessageBoxAsync(string? logPath)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "The file 'error_user.log' was not found!", "Error", MessageButtons.Ok, MessageIcon.Error);
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
            try
            {
                Process.Start(new ProcessStartInfo { FileName = logPath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open the error log file.");
                await ShowAsync(O, "The file 'error_user.log' was not found!", "Error", MessageButtons.Ok,
                    MessageIcon.Error);
            }
    }


    public Task DaphneConfigurationSaveFailedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to save configuration. The error has been logged to the developer.", "Error",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FailedToInjectMesenConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to inject Mesen configuration. Please check file permissions and try again.",
            "Injection Failed", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task EnterSearchQueryMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Please enter a search query.", "Warning", MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task SegaModel2ConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "SEGA Model 2 configuration saved successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task SevenZipDllNotFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            $"The 7z dll is missing from the application folder!\n\n" +
            $"Do you want to reinstall 'Simple Launcher' to fix the issue?", "Error", MessageButtons.YesNo,
            MessageIcon.Question);
    }


    public Task PathOrParameterInvalidMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task NoSystemInHelpUserXmlMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task AnErrorOccurredWhileConfiguringTheEmulatorMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "An error occurred while configuring the emulator.", "Error", MessageButtons.Ok,
            MessageIcon.Error);
    }


    public Task NavigationButtonErrorMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task XemuParameterShouldContainDvdPathMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }

    public async Task<MessageBoxResult> GameFileDoesNotExistAskToDeleteMessageBoxAsync(string filePath)
    {
        if (O == null) return MessageBoxResult.Cancel;
        return await ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task EmulatorLocationRequiredMessageBoxAsync(int emulatorNumber)
    {
        if (O == null) return Task.CompletedTask;
        var message = $"Emulator {emulatorNumber} path is required.";
        return ShowAsync(O, message, "Warning", MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task ThereWasAnErrorLaunchingThisGameMessageBoxAsync(string? logPath)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "The file 'error_user.log' was not found!", "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task SystemImageFolderCanNotBeEmptyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }

    public async Task<MessageBoxResult> SearchOnlineForRomHistoryMessageBoxAsync()
    {
        if (O == null) return MessageBoxResult.Cancel;
        return await ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task CouldNotOpenHistoryWindowMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task FailedToSaveAzaharConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to save Azahar configuration. Please check file permissions.", "Save Failed",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FailedToLoginToRetroAchievementsMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to log in to RetroAchievements. Please check your username and password.",
            "Login Failed", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FailedToInjectReDreamConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to save ReDream configuration. Please check file permissions.", "Save Failed",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FailedtoinjectMamEconfiguration2MessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to inject MAME configuration. The error has been logged.", "Injection Error",
            MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task LinksSavedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Links saved successfully.", "Info", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task CouldNotOpenManualMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            $"Failed to open the manual.\n\n" +
            $"The error was reported to the developer who will try to fix the issue.", "Error", MessageButtons.Ok,
            MessageIcon.Error);
    }


    public Task ShowCustomMessageBoxAsync(string message, string launchError, string? logPath)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "The file 'error_user.log' was not found!", launchError, MessageButtons.Ok,
            MessageIcon.Error);
    }


    public Task NullFileExtensionMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }

    public async Task<MessageBoxResult> DoYouWantToCancelAndCloseMessageBoxAsync()
    {
        if (O == null) return MessageBoxResult.Cancel;
        return await ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ThereWasAnErrorMountingTheFileMessageBoxAsync(int? exitCode = null)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "An error occurred while opening your browser.", "Error", MessageButtons.Ok,
            MessageIcon.Error);
    }


    public Task FailedtoinjectXeniaconfiguration2MessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to inject Xenia configuration. The error has been logged.", "Injection Error",
            MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task SimpleLauncherDoesNotSupportRaHashOfSystemGroupedByFolderMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task FailedToSaveStellaConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to save Stella configuration. Please check file permissions.", "Save Failed",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task ExtensionToSearchIsRequiredMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task YouCanAddANewSystemMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "You can add a new system now.", "Info", MessageButtons.Ok, MessageIcon.Information);
    }

    public async Task<MessageBoxResult> ReallyWantToRemoveAllPlayHistoryMessageBoxAsync()
    {
        if (O == null) return MessageBoxResult.Cancel;
        return await ShowAsync(O,
            "Are you sure you want to remove all play history?",
            "Confirmation", MessageButtons.YesNo, MessageIcon.Question);
    }

    public async Task<MessageBoxResult> AreYouSureYouWantToDeleteTheGameMessageBoxAsync(string fileNameWithExtension)
    {
        if (O == null) return MessageBoxResult.Cancel;
        var loc = App.ServiceProvider?.GetService<LocalizationService>();
        var areYouSure =
            loc?.GetString("Areyousureyouwanttodeletethefile") is { } s1 && s1 != "Areyousureyouwanttodeletethefile"
                ? s1
                : "Are you sure you want to delete the file";
        var thisAction = loc?.GetString("Thisactionwilldelete") is { } s2 && s2 != "Thisactionwilldelete"
            ? s2
            : "This action will delete the file from the HDD and cannot be undone.";
        var confirm = loc?.GetString("ConfirmDeletion") is { } s3 && s3 != "ConfirmDeletion" ? s3 : "Confirm Deletion";
        return await ShowAsync(O, $"{areYouSure} '{fileNameWithExtension}'?\n\n{thisAction}", confirm,
            MessageButtons.YesNo, MessageIcon.Question);
    }


    public Task ReDreamEmulatorPathNotFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "ReDream executable not found. Please select it.", "Error", MessageButtons.Ok,
            MessageIcon.Error);
    }


    public Task FailedToCopySystemImageMessageBoxAsync(string errorMessage)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, $"Failed to copy the image: {errorMessage}", "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task SupportRequestSuccessMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Support request sent successfully.", "Info", MessageButtons.Ok, MessageIcon.Information);
    }

    public async Task<MessageBoxResult> AreYouSureYouWantToDeleteTheCoverImageMessageBoxAsync(
        string fileNameWithoutExtension)
    {
        if (O == null) return MessageBoxResult.Cancel;
        var loc = App.ServiceProvider?.GetService<LocalizationService>();
        var areYouSure =
            loc?.GetString("Areyousureyouwanttodeletethecoverimageof") is { } s1 &&
            s1 != "Areyousureyouwanttodeletethecoverimageof"
                ? s1
                : "Are you sure you want to delete the cover image of";
        var thisAction = loc?.GetString("Thisactionwilldelete") is { } s2 && s2 != "Thisactionwilldelete"
            ? s2
            : "This action will delete the file from the HDD and cannot be undone.";
        var confirm = loc?.GetString("ConfirmDeletion") is { } s3 && s3 != "ConfirmDeletion" ? s3 : "Confirm Deletion";
        return await ShowAsync(O, $"{areYouSure} '{fileNameWithoutExtension}'?\n\n{thisAction}", confirm,
            MessageButtons.YesNo, MessageIcon.Question);
    }


    public Task ThereWasAnErrorDeletingTheCoverImageMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task SystemNameRequiredBeforeChoosingImageMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Please enter a system name before choosing an image.", "System Name Required",
            MessageButtons.Ok, MessageIcon.Information);
    }


    public Task MainWindowSearchEngineErrorMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task BlastemEmulatorNotFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Blastem emulator not found. Please locate 'blastem.exe'.", "Emulator Not Found",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task NoSoundFileIsSelectedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "No sound file is selected.", "Warning", MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task NotificationSoundIsDisableMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Notification sound is disable", "Info", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task SelectASystemToDeleteMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Please select a system to delete.", "Warning", MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task FlycastConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Flycast configuration saved successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task AnotherInstanceIsRunningMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Another instance of 'Simple Launcher' is already running.", "Simple Launcher",
            MessageButtons.Ok, MessageIcon.Information);
    }


    public Task AzaharConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Azahar configuration saved successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task FailedToLoadLanguageResourceMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task SystemHasBeenDeletedMessageBoxAsync(string selectedSystemName)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, $"System '{selectedSystemName}' has been deleted.", "Info", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task UnabletomountIsOfileMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task FailedToInjectSupermodelConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to inject Supermodel configuration. Please check file permissions and try again.",
            "Injection Failed", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task ShowImageDownloadTimeoutMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ErrorFindingGameFilesMessageBoxAsync(string logPath)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "The file 'error_user.log' was not found!", "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FailedtoinjectXeniaconfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "Failed to inject Xenia configuration. The error has been logged. Please check the emulator path and try again.",
            "Injection Error", MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task PleaseSelectASystemBeforeMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Please select a system before using the Feeling Lucky feature.", "Warning",
            MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ErrorOpeningDonationLinkMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ErrorMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task NoGameFoundInTheRandomSelectionMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "No games found to randomly select from. Please check your system selection.",
            "Feeling Lucky", MessageButtons.Ok, MessageIcon.Information);
    }


    public async Task FileCouldNotBeDeletedMessageBoxAsync(string fileNameWithExtension)
    {
        if (O != null)
            await ShowAsync(O, "Could not delete " + fileNameWithExtension, "Error", MessageButtons.Ok,
                MessageIcon.Error);
    }


    public Task CemuConfigurationSavedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Cemu configuration saved.", "Success", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task InjectionFailedGenericMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to inject configuration. The error has been logged to the developer.", "Error",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FolderCreationFailedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task FailedToSaveBlastemConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to save Blastem configuration. Please check file permissions.", "Save Failed",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FailedToSaveSupermodelConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to save Supermodel configuration. Please check file permissions.", "Save Failed",
            MessageButtons.Ok, MessageIcon.Error);
    }

    public async Task<MessageBoxResult> AreYouSureDoYouWantToDeleteThisSystemMessageBoxAsync()
    {
        if (O == null) return MessageBoxResult.Cancel;
        return await ShowAsync(O, "Are you sure you want to delete this system?", "Confirmation", MessageButtons.YesNo,
            MessageIcon.Question);
    }


    public Task AddRaLoginMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "You need to add RetroAchievement login information to use this feature.", "Attention",
            MessageButtons.Ok, MessageIcon.Information);
    }


    public Task FailedSaveReportMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ApiKeyErrorMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task LinksRevertedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Links reverted to default values.", "Info", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task FailedToRestartMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to restart the application.", "Restart Error", MessageButtons.Ok,
            MessageIcon.Error);
    }


    public Task HandleMissingRequiredFilesMessageBoxAsync(string fileList)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "Please reinstall 'Simple Launcher' manually to fix the issue.\n\nThe application will shutdown.",
            "Missing Required Files", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task Rpcs3EmulatorNotFoundPleaseLocateMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "RPCS3 emulator not found. Please locate 'rpcs3.exe'.", "Emulator Not Found",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FailedToCopyLogContentMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to copy log content.", "Copy Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task OperationCancelledMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "The operation was cancelled.", "Operation Cancelled", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task FailedToSaveAresConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to save Ares configuration. Please check file permissions.", "Save Failed",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FailedToSaveRpcs3ConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to save RPCS3 configuration. Please check file permissions.", "Save Failed",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FailedToInjectRpcs3ConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to inject RPCS3 configuration. Please check file permissions and try again.",
            "Injection Failed", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task AzaharConfigurationInjectionPermissionErrorMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task FlycastEmulatorNotFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Flycast emulator not found. Please locate 'flycast.exe'.", "Emulator Not Found",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task CemuEmulatorNotFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Cemu emulator not found. Please locate 'Cemu.exe'.", "Emulator Not Found",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FailedToSaveDuckStationConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to save DuckStation configuration. Please check file permissions.", "Save Failed",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task ThereIsNoUpdateAvailableMessageBoxAsync(string currentVersion)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task GroupByFolderOnlyForMameAndDosBoxMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "The 'Group Files by Folder' option is only compatible with MAME emulators (Software List CHDs) or DOSBox emulators (uncompressed DOS game folders). To use a different emulator, please edit the system settings and disable this option.",
            "Compatibility Warning", MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task CouldNotFindAFileMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task RaineSettingsSavedAndInjectedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Raine configuration has been successfully injected.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task AresemulatornotfoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Ares emulator not found. Please locate 'ares.exe'.", "Emulator Not Found",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FilePathIsInvalidMessageBoxAsync(string? logPath)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "The file 'error_user.log' was not found!", "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task SettingsSavedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Settings saved successfully.", "Info", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task SystemXmlIsCorruptedMessageBoxAsync(string? logPath)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "The file 'error_user.log' was not found!", "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task RetroArchParameterShouldContainLMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ThereIsNoPcbMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "There is no PCB file associated with this game.", "PCB not found", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task SegaModel2EmulatorNotFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "SEGA Model 2 emulator not found. Please locate 'emulator.exe'.", "Emulator Not Found",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task ErrorCalculatingStatsMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public async Task WouldYouLikeToOpenTheLogMessageBoxAsync(string? logPath)
    {
        if (O == null) return;

        var result = await ShowAsync(O,
            "'Simple Launcher' was unable to launch this game.\n\n" +
            "Would you like to open the 'error_user.log' file to debug the error?",
            "Error", MessageButtons.YesNo, MessageIcon.Error);

        if (result == MessageBoxResult.Yes && !string.IsNullOrEmpty(logPath))
            try
            {
                Process.Start(new ProcessStartInfo { FileName = logPath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open the 'error_user.log' file.");
                await ShowAsync(O, "The file 'error_user.log' was not found!", "Error", MessageButtons.Ok,
                    MessageIcon.Error);
            }
    }


    public Task CouldNotOpenTheDownloadLinkMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "'Simple Launcher' could not open the download link.", "Error", MessageButtons.Ok,
            MessageIcon.Error);
    }


    public async Task FileNoLongerExistsMessageBoxAsync(string fileNameWithExtension)
    {
        if (O != null)
            await ShowAsync(O, fileNameWithExtension + " no longer exists.", "Not Found", MessageButtons.Ok,
                MessageIcon.Warning);
    }


    public Task SystemSavedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "System saved successfully.", "Info", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task UnsupportedArchitectureMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "'Simple Launcher' does not support the current processor architecture. We only support 64-bit (x64) or ARM64. The application will now close.",
            "Unsupported Architecture", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FailedToInjectBlastemConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to inject Blastem configuration. Please check file permissions and try again.",
            "Injection Failed", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task DownloadExtractionFailedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ProblemOpeningInfoLinkMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task FileParametersMdIsEmptyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task DuckStationEmulatorNotFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "DuckStation emulator not found. Please locate the DuckStation executable.",
            "Emulator Not Found", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task SelectAFavoriteToRemoveMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Please select a favorite to remove.", "Warning", MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task FileMustBeCompressedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }

    public async Task<MessageBoxResult> WouldYouLikeToSaveAReportMessageBoxAsync()
    {
        if (O == null) return MessageBoxResult.Cancel;
        return await ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task EmulatorConfiguredSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Emulator configured successfully for RetroAchievements!", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public async Task GeolithDoesNotSupportCompressedFilesMessageBoxAsync()
    {
        if (O == null) return;
        var title = "Error";
        var message1 = "'geolith_libretro.dll' does not support ZIP, 7Z or RAR files.";
        var message2 = "It only support NEO files.";
        var message3 = "Please ensure you are running a compatible ROM set.";
        var message4 = "Would you like to visit the url 'wiki.terraonion.com' to get more info about that?";
        var result = await ShowAsync(O, $"{message1}\n\n{message2}\n\n{message3}\n\n{message4}", title,
            MessageButtons.YesNo, MessageIcon.Warning);
        if (result == MessageBoxResult.Yes)
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
                Log.Error(ex, "Could not open browser");
                await ShowAsync(O, "Could not open browser: " + ex.Message, "Error", MessageButtons.Ok,
                    MessageIcon.Error);
            }
    }


    public Task SaveSystemFailedMessageBoxAsync(string? details = null)
    {
        if (O == null) return Task.CompletedTask;
        var message =
            "Failed to save system configuration.\n\nPlease check file permissions and ensure the file is not locked.";
        if (!string.IsNullOrEmpty(details)) message += $"\n\nDetails: {details}";

        return ShowAsync(O, message, "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task Pcsx2ConfigurationInjectionPermissionErrorMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task FailedToLoadHelpUserXmlMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ElevationRequiredMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task FileNeedToBeCompressedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task NoStatsToSaveMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "No statistics available to save.", "Error", MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task AresConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Ares configuration saved successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task FailedToInitializeSevenZipMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            $"An unexpected error occurred while initializing the 7-Zip library.\n\n" +
            $"Do you want to reinstall 'Simple Launcher' to fix the issue?", "Error", MessageButtons.YesNo,
            MessageIcon.Question);
    }


    public Task GameLaunchTimeoutMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Game launch timed out. Please try again or check if the emulator started.",
            "Game launch timed out", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task CouldNotFindUpdaterOnGitHubMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "'Simple Launcher' could not find the updater application on GitHub.", "Error",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task ImageViewerErrorMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task FailedToInjectStellaConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to inject Stella configuration. Please check file permissions and try again.",
            "Injection Failed", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task ErrorMethodLoadGameFilesAsyncMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task FailedToSaveSettingsMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "Failed to save settings. Please check that the application folder is writable and not locked by another process.",
            "Error", MessageButtons.Ok, MessageIcon.Error);
    }

    public async Task TakeScreenShotMessageBoxAsync()
    {
        if (O != null)
            await ShowAsync(O, "Press Print Screen to capture a screenshot.", "Screenshot", MessageButtons.Ok,
                MessageIcon.Information);
    }


    public Task ThereIsNoCoverMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "There is no cover file associated with this game.", "Cover not found", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task FailedToSaveDolphinConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to save Dolphin configuration. Please check file permissions.", "Save Failed",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public async Task ErrorOpeningUrlMessageBoxAsync()
    {
        if (O != null) await ShowAsync(O, "Could not open the link.", "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public async Task ReinstallSimpleLauncherFileCorruptedMessageBoxAsync()
    {
        if (O == null) return;

        var result = await ShowAsync(O,
            "'Simple Launcher' could not load the file 'mame.dat' or it is corrupted.\n\n" +
            "Do you want to automatically reinstall 'Simple Launcher' to fix it?",
            "Error", MessageButtons.YesNo, MessageIcon.Question);

        if (result == MessageBoxResult.Yes)
            _ = App.ServiceProvider.GetRequiredService<AvaloniaCheckForUpdatesService>().ReinstallAndShutdownAsync();
        else
            App.ServiceProvider.GetRequiredService<AvaloniaQuitSimpleLauncher>().SimpleQuitApplication();
    }


    public Task ErrorLoadingRomHistoryMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public async Task GlobalSearchErrorMessageBoxAsync()
    {
        if (O != null) await ShowAsync(O, "Search error.", "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task SystemAddedMessageBoxAsync(string systemName, string resolvedSystemFolder,
        string resolvedSystemImageFolder)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task StellaConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Stella configuration saved successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task GamePadErrorMessageBoxAsync(string? logPath)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "The file 'error_user.log' was not found!", "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task OotakeDoesNotSupportImageFilesMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Ootake emulator does not support CHD, ISO, CUE/BIN files.", "Error", MessageButtons.Ok,
            MessageIcon.Error);
    }


    public Task DiskSpaceErrorMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Not enough disk space for extraction.", "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FailedToSaveSegaModel2ConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to save SEGA Model 2 configuration. Please check file permissions.", "Save Failed",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FailedToInjectFlycastConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to inject Flycast configuration. Please check file permissions and try again.",
            "Injection Failed", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task ThereIsNoGameplaySnapshotMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "There is no gameplay snapshot file associated with this game.",
            "Gameplay Snapshot not found", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ErrorSettingSoundFileMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Error choosing or copying sound file.", "Warning", MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task DolphinEmulatorNotFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Dolphin emulator not found. Please locate 'Dolphin.exe'.", "Emulator Not Found",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task Rpcs3ConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "RPCS3 configuration saved successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task FailedToInjectDolphinConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to inject Dolphin configuration. Please check file permissions and try again.",
            "Injection Failed", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task UnableToOpenLinkMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ProtocolHandlerNotRegisteredMessageBoxAsync(string protocol)
    {
        if (O == null) return Task.CompletedTask;
        var message = string.Format(CultureInfo.InvariantCulture,
            "Protocol handler for '{0}://' is not registered. Please ensure the associated application is installed.",
            protocol);
        return ShowAsync(O, message, "Launch Error", MessageButtons.Ok, MessageIcon.Warning);
    }

    public async Task<MessageBoxResult> DoYouWantToUpdateMessageBoxAsync(string currentVersion, string latestVersion)
    {
        if (O == null) return MessageBoxResult.Cancel;
        return await ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task CannotScreenshotMinimizedWindowMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Cannot take a screenshot of a minimized window.", "Error", MessageButtons.Ok,
            MessageIcon.Error);
    }


    public Task MoveToWritableFolderMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ReportSavedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Report saved successfully.", "Success", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task NoSystemInParametersMdMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task AddSystemFailedMessageBoxAsync(string? details = null)
    {
        if (O == null) return Task.CompletedTask;
        var message =
            "There was an error adding this system.\n\nThe error was reported to the developer who will try to fix the issue.";
        if (!string.IsNullOrEmpty(details)) message += $"\n\nDetails: {details}";

        return ShowAsync(O, message, "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task InvalidSystemConfigMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task BlastemConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Blastem configuration saved successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task MesenEmulatorNotFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Mesen emulator not found. Please locate 'Mesen.exe'.", "Emulator Not Found",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task ToggleGamepadFailureMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public async Task RetroArchSpecialCharactersInPathMessageBoxAsync()
    {
        if (O == null) return;

        await ShowAsync(O,
            "The emulator could not launch the game because the file path contains special characters (for example: ´, `, ~, !, ?).\n\n" +
            "RetroArch cannot create its required folders in paths with these characters.\n\n" +
            @"To fix this, please move your emulator and your game files to a folder that uses only standard letters and numbers, such as C:\Games\.",
            "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task ExtensionToLaunchIsRequiredMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task RetroArchemulatorpathnotfoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "RetroArch emulator path not found. Please select 'retroarch.exe' to apply these settings.",
            "Emulator Required", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task EmulatorNameIsRequiredMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task EmulatorNameMustBeUniqueMessageBoxAsync(string emulatorName)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, $"The name '{emulatorName}' is used multiple times. Each emulator name must be unique.",
            "Info", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task FailedToInjectMameConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "Failed to inject MAME configuration. The error has been logged. Please check the emulator path and try again.",
            "Injection Error", MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task ImagePackDownloaderUnavailableMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "'Simple Launcher' could not access the Web API to download the updated URLs. Please try again later.",
            "Error", MessageButtons.Ok, MessageIcon.Error);
    }

    public async Task<bool> BatchFilePathsMissingMessageBoxAsync(IList<string> missingPaths)
    {
        if (O == null) return false;
        var pathsList = string.Join("\n", missingPaths.Select(static p => $"  - {p}"));
        var message = $"The batch file references paths that do not exist:\n\n{pathsList}\n\n" +
                      "This may cause the batch file to fail. Not all paths may be detected - this is a best-effort check.\n\n" +
                      "Do you want to continue anyway?";
        var result = await ShowAsync(O, message, "Warning", MessageButtons.YesNo, MessageIcon.Question);
        return result == MessageBoxResult.Yes;
    }


    public Task SelectAGameToLaunchMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Please select a game to launch.", "Info", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task YumirEmulatorNotFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Yumir executable not found. Please select it.", "Error", MessageButtons.Ok,
            MessageIcon.Error);
    }


    public Task DokanDriverNotInstalledMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "The Dokan file system driver (dokan2.dll) is required to mount archives as virtual drives. It does not appear to be installed on this system.\n\nDo you want to open your browser to download Dokan?",
            "Error", MessageButtons.YesNo, MessageIcon.Question);
    }


    public Task FileIsLockedMessageBoxAsync(string? tempFolderPath)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Could not open the temporary folder.", "Error Opening Folder", MessageButtons.Ok,
            MessageIcon.Error);
    }


    public Task SupermodelConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Supermodel configuration saved successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task SupportRequestSendErrorMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ErrorWhileLoadingParametersMdMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ThereIsNoWalkthroughMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "There is no walkthrough file associated with this game.", "Walkthrough not found",
            MessageButtons.Ok, MessageIcon.Information);
    }


    public async Task ErrorWhileAddingFavoritesMessageBoxAsync()
    {
        if (O != null) await ShowAsync(O, "Error adding to favorites.", "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task MameConfigurationInjectedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "MAME configuration injected successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task FileHelpUserXmlIsMissingMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public async Task GameIsAlreadyInFavoritesMessageBoxAsync(string fileNameWithExtension)
    {
        if (O != null)
            await ShowAsync(O, fileNameWithExtension + " is already in favorites.", "Already Favorited",
                MessageButtons.Ok, MessageIcon.Information);
    }


    public Task DownloadedFileIsMissingMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task DaphnesettingssavedsuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Daphne settings saved successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task ApplicationControlPolicyBlockedManualLinkMessageBoxAsync(string url)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task DeadZonesRevertedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "DeadZone values reverted to default values.", "Info", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task ShowDownloadFileLockedMessageBoxAsync(string tempFolderPath)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "The download could not be completed because the temporary file is locked by another process (e.g., antivirus software).\n\nWould you like to open the temporary folder to inspect the file?",
            "Download Failed", MessageButtons.YesNo, MessageIcon.Question);
    }

    public async Task<MessageBoxResult> FirstRunWelcomeMessageBoxAsync()
    {
        if (O == null) return MessageBoxResult.Cancel;
        return await ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task SettingsSavedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Settings saved.", "Success", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task CouldNotOpenWalkthroughMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task XeniaconfigurationinjectedsuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Xenia configuration injected successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task FileSystemXmlIsLockedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "The file 'system.xml' is locked or inaccessible by another process.", "Error",
            MessageButtons.Ok, MessageIcon.Error);
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


    public Task NoPdfViewerInstalledMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "No PDF viewer is installed on your system.\n\nPlease install a PDF viewer (such as Adobe Acrobat Reader, Sumatra PDF, or Microsoft Edge) to open this file.",
            "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task SelectAHistoryItemToRemoveMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Please select a history item to remove.", "Please select a item", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task XeniaemulatorpathnotfoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "Xenia emulator path not found. Please select 'xenia.exe' or 'xenia_canary.exe' to apply these settings.",
            "Emulator Required", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task EnterEmailMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Please enter the email.", "Info", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task FileParametersMdIsMissingMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task PowerShellExecutionPolicyRestrictionsMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task FailedToInjectSegaModel2ConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to inject SEGA Model 2 configuration. Please check file permissions and try again.",
            "Injection Failed", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task Emulator1RequiredMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }

    public async Task<MessageBoxResult> WouldYouLikeToRestoreTheLastBackupMessageBoxAsync()
    {
        if (O == null) return MessageBoxResult.Cancel;
        return await ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task InstallUpdateManuallyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ToggleFuzzyMatchingFailureMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "There was an error toggling the fuzzy matching logic.", "Error", MessageButtons.Ok,
            MessageIcon.Error);
    }


    public Task NoDefaultBrowserConfiguredMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "Your operating system does not have a default web browser configured. Please set one in Windows Settings (Apps > Default apps) to open web links.",
            "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task ErrorWhileLoadingHelpUserXmlMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task RightClickContextMenuErrorMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task YumirConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Yumir configuration saved successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task FailedtoinjectAresconfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to inject Ares configuration. Please check file permissions and try again.",
            "Injection Failed", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task ThereIsNoCartMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "There is no cart file associated with this game.", "Cart not found", MessageButtons.Ok,
            MessageIcon.Information);
    }

    public async Task<MessageBoxResult> GroupByFolderWarningMessageBoxAsync()
    {
        if (O == null) return MessageBoxResult.Cancel;
        return await ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task LaunchToolInformationMessageBoxAsync(string info)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, info, "Error", MessageButtons.Ok, MessageIcon.Information);
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


    public Task SystemFolderCanNotBeEmptyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task InvalidImageFormatMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Only PNG, JPG, and JPEG images are supported.", "Invalid Image Format", MessageButtons.Ok,
            MessageIcon.Warning);
    }


    public Task FailedToSaveMednafenConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to save Mednafen configuration. Please check file permissions.", "Save Failed",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FileSystemXmlIsCorruptedMessageBoxAsync(string? logPath)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "The file 'error_user.log' was not found!", "Error", MessageButtons.Ok, MessageIcon.Error);
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


    public Task MameEmulatorPathNotFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "MAME emulator path not found. Please select 'mame.exe' or 'mame64.exe' to apply these settings.",
            "Emulator Required", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task EnterUsernamePasswordMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Please enter your RetroAchievements username and password first.", "Missing Information",
            MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task InvalidFolderCharactersMessageBoxAsync(string invalidChars)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task CouldNotOpenSoundConfigurationWindowMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Could not open sound configuration window", "Warning", MessageButtons.Ok,
            MessageIcon.Warning);
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


    public Task ThereIsNoManualMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "There is no manual associated with this file.", "Manual not found", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task MesenConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Mesen configuration saved successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task FailedToSaveFlycastConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to save Flycast configuration. Please check file permissions.", "Save Failed",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task FailedtoinjectRetroArchconfiguration2MessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to inject RetroArch configuration. The error has been logged.", "Injection Error",
            MessageButtons.Ok, MessageIcon.Warning);
    }


    public async Task<MessageBoxResult> ScanGamePathForRetroAchievementsMessageBoxAsync()
    {
        if (O != null)
            return await ShowAsync(O,
                "We need to scan your game path to see what game is compatible with RetroAchievements.",
                "RetroAchievements", MessageButtons.YesNo, MessageIcon.Question);

        return MessageBoxResult.Cancel;
    }


    public Task SelectSystemBeforeSearchMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Please select a system before searching.", "Warning", MessageButtons.Ok,
            MessageIcon.Warning);
    }


    public Task UpdaterLaunchFailedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
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


    public Task EnterSupportRequestMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Please enter the details of the support request.", "Info", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task FuzzyMatchingErrorFailToSetThresholdMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to set fuzzy matching threshold.", "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task ErrorChangingViewModeMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task DownloadAndExtractionWereSuccessfulMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Download and extraction completed successfully.", "Info", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task EmulatorPathNotConfiguredMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ThereIsNoCabinetMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "There is no cabinet file associated with this game.", "Cabinet not found",
            MessageButtons.Ok, MessageIcon.Information);
    }


    public Task CouldNotLaunchGameDueToDepViolationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task MednafenEmulatorNotFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Mednafen emulator not found. Please locate 'mednafen.exe'.", "Emulator Not Found",
            MessageButtons.Ok, MessageIcon.Error);
    }


    public Task EnterValidSearchTermsMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Please enter valid search terms.", "Invalid Search", MessageButtons.Ok,
            MessageIcon.Warning);
    }


    public Task DeadZonesSavedMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "DeadZone values saved successfully.", "Info", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task EmulatorNameRequiredMessageBoxAsync(int i)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task HandleApiConfigErrorMessageBoxAsync(string reason)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ShowImagePackDownloadErrorMessageBoxAsync(EasyModeSystemConfig selectedSystem)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "Error opening the download link.\n\nThe error was reported to the developer who will try to fix the issue.",
            "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public async Task DefaultImageNotFoundMessageBoxAsync()
    {
        if (O != null)
            await ShowAsync(O, "Default cover image not found.", "Missing Image", MessageButtons.Ok,
                MessageIcon.Warning);
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


    public Task UnabletoDismountIsOfileMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ErrorCheckingForUpdatesMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task SimpleLauncherWasUnableToRestoreBackupMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "'Simple Launcher' was unable to restore the last backup.", "Error", MessageButtons.Ok,
            MessageIcon.Error);
    }


    public Task InvalidSystemNameCharactersMessageBoxAsync(string invalidChars)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task FailedToInjectDuckStationConfigurationMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Failed to inject DuckStation configuration. Please check file permissions and try again.",
            "Injection Failed", MessageButtons.Ok, MessageIcon.Error);
    }

    public async Task<MessageBoxResult> FavoriteFileDoesNotExistAskToDeleteMessageBoxAsync(string filePath)
    {
        if (O == null) return MessageBoxResult.Cancel;
        return await ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task CouldNotOpenAchievementsWindowMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "Could not open the achievements window.\n\nThe error was reported to the developer who will try to fix the issue.",
            "Error", MessageButtons.Ok, MessageIcon.Error);
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


    public Task DolphinConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Dolphin configuration saved successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task ShowExtractionFailedMessageBoxAsync(string tempFolderPath)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "The file was downloaded successfully, but automatic extraction failed. This can happen if an antivirus program is scanning or locking the file.\n\nWould you like to open the temporary folder to inspect the file?",
            "Extraction Failed", MessageButtons.YesNo, MessageIcon.Question);
    }


    public Task NoFavoriteFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "There is no Favorite for this system, or you have not chosen a system.", "Warning",
            MessageButtons.Ok, MessageIcon.Information);
    }


    public async Task FileSuccessfullyDeletedMessageBoxAsync(string fileNameWithExtension)
    {
        if (O != null)
            await ShowAsync(O, fileNameWithExtension + " deleted.", "Deleted", MessageButtons.Ok,
                MessageIcon.Information);
    }


    public async Task WarningMessageBoxAsync(string message)
    {
        if (O != null) await ShowAsync(O, message, "Warning", MessageButtons.Ok, MessageIcon.Warning);
    }


    public Task FailedToConfigureTheEmulatorMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O,
            "Failed to configure the emulator. The configuration file might be missing, in an unexpected location, or read-only.",
            "Configuration Failed", MessageButtons.Ok, MessageIcon.Error);
    }


    public Task DuckStationConfigurationSavedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "DuckStation configuration saved successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task ErrorOpeningBrowserMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task EnterNameMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Please enter the name.", "Info", MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ThereIsNoVideoFileMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "There is no video file associated with this game.", "Video not found", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task ErrorLaunchingToolMessageBoxAsync(string? logPath)
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "The file 'error_user.log' was not found!", "Error", MessageButtons.Ok, MessageIcon.Error);
    }


    public async Task<MessageBoxResult> GameNotSupportedByRetroAchievementsMessageBoxAsync()
    {
        if (O != null)
            return await ShowAsync(O,
                "'Simple Launcher' could not calculate the hash value of this game or this game is not yet supported by RetroAchievements.\n\n" +
                "Do you want to open the global RetroAchievements window?",
                "RetroAchievements", MessageButtons.YesNo, MessageIcon.Question);

        return MessageBoxResult.Cancel;
    }


    public async Task BatchFileFailedMessageBoxAsync(string batchFilePath, string errorDetail, string? logPath,
        int? exitCode = null)
    {
        if (O == null) return;
        var batchFileName = Path.GetFileName(batchFilePath);
        var batchNameMessage = $"The batch file failed to run.\n\n{batchFileName}";
        var errorMessage = !string.IsNullOrEmpty(errorDetail) ? $"Error: {errorDetail}\n\n" : "";
        var exitCodeMessage = exitCode.HasValue ? $"Exit code: {exitCode.Value}\n\n" : "";
        var explanation = exitCode < 0
            ? "The program launched by this batch file may have crashed or been terminated unexpectedly. Negative exit codes typically indicate system-level failures."
            : "This usually means a path referenced inside the batch file no longer exists or is incorrect.";
        var message = $"{batchNameMessage}\n\n{exitCodeMessage}{errorMessage}{explanation}\n\n" +
                      "You can turn off this error message in Expert mode.\n\n" +
                      "Do you want to open the file 'error_user.log' to debug the error?";
        var result = await ShowAsync(O, message, "Error", MessageButtons.YesNo, MessageIcon.Error);
        if (result == MessageBoxResult.Yes)
            try
            {
                Process.Start(new ProcessStartInfo { FileName = logPath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to open the error log file from a batch file error message box.");
                await ShowAsync(O, "The file 'error_user.log' was not found!", "Error", MessageButtons.Ok,
                    MessageIcon.Error);
            }
    }


    public Task RetroArchConfigurationInjectedSuccessfullyMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "RetroArch configuration injected successfully.", "Success", MessageButtons.Ok,
            MessageIcon.Information);
    }


    public Task SelectedToolNotFoundMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "Please reinstall 'Simple Launcher' manually to fix the issue.", "Error", MessageButtons.Ok,
            MessageIcon.Error);
    }


    public Task ThereIsNoTitleSnapshotMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "There is no title snapshot file associated with this game.", "Title Snapshot not found",
            MessageButtons.Ok, MessageIcon.Information);
    }


    public Task ErrorOpeningVideoLinkMessageBoxAsync()
    {
        if (O == null) return Task.CompletedTask;
        return ShowAsync(O, "", "", MessageButtons.Ok, MessageIcon.Information);
    }

    private static async Task<MessageBoxResult> ShowAsync(
        Window? owner, string message, string caption,
        MessageButtons buttons, MessageIcon icon)
    {
        if (owner is null) return MessageBoxResult.Cancel;

        return await MessageDialogWindow.ShowAsync(owner, message, caption, buttons, icon);
    }
}