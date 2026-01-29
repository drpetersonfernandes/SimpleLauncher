using System;
using System.ComponentModel;

namespace SimpleLauncher.Services;

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
}