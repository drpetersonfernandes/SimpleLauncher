using System.Windows;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.New.Services.InjectEmulatorConfig;

/// <summary>
/// Centralizes error handling for emulator configuration injection windows.
/// </summary>
public static class InjectionErrorHandler
{
    /// <summary>
    /// Handles a failure from the Run button by notifying the user and developer, then closing the window.
    /// </summary>
    /// <param name="logErrors">The error logger.</param>
    /// <param name="ex">The exception that caused the failure.</param>
    /// <param name="emulatorName">The name of the emulator being configured.</param>
    /// <param name="emulatorPath">The path to the emulator executable.</param>
    /// <param name="window">The injection window to close.</param>
    /// <param name="messageBox">The message box service for user notifications.</param>
    public static async Task HandleRunButtonFailure(ILogger logErrors, Exception ex, string emulatorName, string emulatorPath, Window? window, IMessageBoxLibraryService messageBox)
    {
        // Notify user
        await ShowGenericInjectionError(messageBox);

        // Notify developer
        logErrors.Error(ex, $"Run button failed for {emulatorName} at path: {emulatorPath}");

        // Close injection window
        window?.Close();
    }

    /// <summary>
    /// Handles a failure from the Save button by notifying the user and developer, then closing the window.
    /// </summary>
    /// <param name="logErrors">The error logger.</param>
    /// <param name="ex">The exception that caused the failure.</param>
    /// <param name="emulatorName">The name of the emulator being configured.</param>
    /// <param name="emulatorPath">The path to the emulator executable.</param>
    /// <param name="window">The injection window to close.</param>
    /// <param name="messageBox">The message box service for user notifications.</param>
    public static async Task HandleSaveButtonFailure(ILogger logErrors, Exception ex, string emulatorName, string emulatorPath, Window? window, IMessageBoxLibraryService messageBox)
    {
        // Notify user
        await ShowGenericInjectionError(messageBox);

        // Notify developer
        logErrors.Error(ex, $"Save button failed for {emulatorName} at path: {emulatorPath}");

        // Close injection window
        window?.Close();
    }

    private static async Task ShowGenericInjectionError(IMessageBoxLibraryService messageBox)
    {
        try
        {
            await messageBox.InjectionFailedGenericMessageBoxAsync();
        }
        catch (Exception ex)
        {
            // Never let a failed error dialog mask the original failure
            Log.Debug(ex, "Injection error dialog failed to show");
        }
    }

    /// <summary>
    /// Derives the emulator name from the executable path or the injection window type name.
    /// </summary>
    /// <param name="emulatorPath">The path to the emulator executable.</param>
    /// <param name="windowType">The type of the injection window.</param>
    /// <returns>The emulator name.</returns>
    public static string GetEmulatorName(string emulatorPath, Type windowType)
    {
        if (!string.IsNullOrEmpty(emulatorPath))
        {
            var fileName = Path.GetFileNameWithoutExtension(emulatorPath);
            if (!string.IsNullOrEmpty(fileName))
                return fileName;
        }

        var typeName = windowType.Name;
        if (typeName.StartsWith("Inject", StringComparison.Ordinal) && typeName.EndsWith("ConfigWindow", StringComparison.Ordinal))
        {
            return typeName.Substring(6, typeName.Length - 6 - 12);
        }

        return typeName;
    }
}
