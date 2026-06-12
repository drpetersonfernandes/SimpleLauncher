using System.Windows;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.InjectEmulatorConfig;

public static class InjectionErrorHandler
{
    public static void HandleRunButtonFailure(ILogErrors logErrors, Exception ex, string emulatorName, string emulatorPath, Window window, IMessageBoxLibraryService messageBox)
    {
        // Notify user
        ShowGenericInjectionError(messageBox);

        // Notify developer
        logErrors.LogAndForget(ex, $"Run button failed for {emulatorName} at path: {emulatorPath}");

        // Close injection window
        window?.Close();
    }

    public static void HandleSaveButtonFailure(ILogErrors logErrors, Exception ex, string emulatorName, string emulatorPath, Window window, IMessageBoxLibraryService messageBox)
    {
        // Notify user
        ShowGenericInjectionError(messageBox);

        // Notify developer
        logErrors.LogAndForget(ex, $"Save button failed for {emulatorName} at path: {emulatorPath}");

        // Close injection window
        window?.Close();
    }

    private static void ShowGenericInjectionError(IMessageBoxLibraryService messageBox)
    {
        _ = messageBox.InjectionFailedGenericMessageBoxAsync();
    }

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
