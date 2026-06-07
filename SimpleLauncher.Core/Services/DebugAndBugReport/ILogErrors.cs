using System.Diagnostics;

namespace SimpleLauncher.Core.Services.DebugAndBugReport;

public interface ILogErrors
{
    Task LogErrorAsync(Exception ex, string contextMessage = null);
}

public static class LogErrorsExtensions
{
    public static void LogAndForget(this ILogErrors logErrors, Exception ex, string contextMessage = null)
    {
        if (logErrors == null) return;

        _ = Task.Run(async () =>
        {
            try
            {
                await logErrors.LogErrorAsync(ex, contextMessage);
            }
            catch (Exception fireForgetEx)
            {
                Debug.WriteLine($"[LogAndForget] Failed to log error: {contextMessage}. Exception: {fireForgetEx.Message}");
            }
        });
    }
}