using System.Diagnostics;
using System.Globalization;

namespace SimpleLauncher.Services.DebugAndBugReport;

public class DebugLogger : IDebugLogger
{
    private readonly bool _isDebugMode;
    private readonly DebugWindow _logWindowInstance;

    public DebugLogger(bool isDebugModeEnabled)
    {
        _isDebugMode = isDebugModeEnabled;

        if (!_isDebugMode) return;

        DebugWindow.Initialize();
        _logWindowInstance = DebugWindow.Instance;
        Log("Debug logging initialized.");
    }

    public void Log(string message)
    {
        Debug.WriteLine($"[DEBUG] {DateTime.Now:HH:mm:ss.fff} - {message}");

        if (_isDebugMode && _logWindowInstance != null)
        {
            _logWindowInstance.AppendLogMessage(message);
        }
    }

    public void LogException(Exception ex, string contextMessage = null)
    {
        if (ex == null)
        {
            ex = new ArgumentNullException(nameof(ex), @"Exception is null.");
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
