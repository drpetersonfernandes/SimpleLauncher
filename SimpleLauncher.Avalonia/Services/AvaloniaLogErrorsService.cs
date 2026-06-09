using System.Diagnostics;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Avalonia.Services;

public class AvaloniaLogErrorsService : ILogErrors
{
    public Task LogErrorAsync(Exception ex, string? contextMessage = null)
    {
        var message = string.IsNullOrWhiteSpace(contextMessage)
            ? $"[Error] {ex.Message}"
            : $"[Error] {contextMessage}: {ex.Message}";
        Debug.WriteLine(message);
        return Task.CompletedTask;
    }

    public void LogAndForget(Exception? ex, string? contextMessage = null)
    {
        var message = string.IsNullOrWhiteSpace(contextMessage)
            ? $"[Error] {ex?.Message}"
            : $"[Error] {contextMessage}: {ex?.Message}";
        Debug.WriteLine(message);
    }
}
