using System;
using System.Diagnostics;
using System.Globalization;

namespace SimpleLauncher.Services;

public static class DebugLogger
{
    private static bool _isDebugMode;
    private static LogWindow _logWindowInstance;

    /// <summary>
    /// Initializes the DebugLogger based on whether debug mode is enabled.
    /// </summary>
    /// <param name="isDebugModeEnabled">True if the application is running in debug mode (e.g., via command-line argument).</param>
    public static void Initialize(bool isDebugModeEnabled)
    {
        _isDebugMode = isDebugModeEnabled;

        if (!_isDebugMode) return;

        // Initialize and show the LogWindow
        LogWindow.Initialize();

        _logWindowInstance = LogWindow.Instance; // Store the instance reference
        Log("Debug logging initialized."); // Log the initialization
    }

    /// <summary>
    /// Logs a debug message.
    /// </summary>
    /// <param name="message">The debug message.</param>
    public static void Log(string message)
    {
        // Always write to Debug output, regardless of _isDebugMode
        Debug.WriteLine($"[DEBUG] {DateTime.Now:HH:mm:ss.fff} - {message}");

        if (_isDebugMode && _logWindowInstance != null)
        {
            // Append message to the LogWindow on the UI thread
            _logWindowInstance.AppendLogMessage(message);
        }
    }

    /// <summary>
    /// Logs an exception details. The message is only displayed if debug mode is enabled.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="contextMessage">An optional context message.</param>
    public static void LogException(Exception ex, string contextMessage = null)
    {
        if (ex == null)
        {
            ex = new Exception("Exception is null.");
        }

        var message = new System.Text.StringBuilder();
        if (!string.IsNullOrEmpty(contextMessage))
        {
            message.AppendLine(CultureInfo.InvariantCulture, $"Context: {contextMessage}");
        }

        message.AppendLine(CultureInfo.InvariantCulture, $"Exception Type: {ex.GetType().FullName}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Message: {ex.Message}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Source: {ex.Source}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Stack Trace: {ex.StackTrace}");

        if (ex.InnerException != null)
        {
            message.AppendLine("--- Inner Exception ---");
            message.AppendLine(CultureInfo.InvariantCulture, $"Exception Type: {ex.InnerException.GetType().FullName}");
            message.AppendLine(CultureInfo.InvariantCulture, $"Message: {ex.InnerException.Message}");
            message.AppendLine(CultureInfo.InvariantCulture, $"Source: {ex.InnerException.Source}");
            message.AppendLine(CultureInfo.InvariantCulture, $"Stack Trace: {ex.InnerException.StackTrace}");
            message.AppendLine("-----------------------");
        }

        Log($"EXCEPTION: {message}");
    }
}