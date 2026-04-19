using System.ComponentModel;

namespace SimpleLauncher.Services.CheckApplicationControlPolicy;

public static class CheckApplicationControlPolicy
{
    /// <summary>
    /// Checks if the given exception is a Win32Exception indicating an application control policy block.
    /// </summary>
    /// <param name="ex">The exception to check.</param>
    /// <returns>True if the exception indicates an application control policy block, false otherwise.</returns>
    public static bool IsApplicationControlPolicyBlocked(Exception ex)
    {
        if (ex is Win32Exception win32Ex)
        {
            // NativeErrorCode 5 (Access Denied) is a common manifestation of AppLocker/WDAC.
            // The message content is a more specific indicator.
            var message = win32Ex.Message;
            return win32Ex.NativeErrorCode == 5 &&
                   (message.Contains("Control de aplicaciones bloqueó", StringComparison.OrdinalIgnoreCase) ||
                    message.Contains("Application Control policy blocked", StringComparison.OrdinalIgnoreCase));
        }

        return false;
    }

    /// <summary>
    /// Checks if the given exception is a Win32Exception indicating that elevation is required.
    /// </summary>
    /// <param name="ex">The exception to check.</param>
    /// <returns>True if elevation is required, false otherwise.</returns>
    public static bool IsElevationRequired(Exception ex)
    {
        return ex is Win32Exception { NativeErrorCode: 740 };
    }

    /// <summary>
    /// Checks if the given exception is a Win32Exception indicating the operation was canceled by the user.
    /// This typically occurs when a user cancels a UAC (User Account Control) prompt.
    /// </summary>
    /// <param name="ex">The exception to check.</param>
    /// <returns>True if the operation was canceled by the user, false otherwise.</returns>
    public static bool IsOperationCanceledByUser(Exception ex)
    {
        // Win32 error code 1223 (ERROR_CANCELLED) indicates the user canceled the operation.
        // This commonly happens when the user clicks "Cancel" on a UAC dialog.
        return ex is Win32Exception { NativeErrorCode: 1223 };
    }
}