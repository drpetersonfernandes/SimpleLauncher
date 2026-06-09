using System.Diagnostics;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Avalonia.Services;

public class AvaloniaDebugLogger : IDebugLogger
{
    public void Log(string message)
    {
        Debug.WriteLine(message);
    }

    public void LogException(Exception ex, string? contextMessage = null)
    {
        var message = string.IsNullOrWhiteSpace(contextMessage)
            ? $"[Exception] {ex}"
            : $"[Exception] {contextMessage}: {ex}";
        Debug.WriteLine(message);
    }
}
