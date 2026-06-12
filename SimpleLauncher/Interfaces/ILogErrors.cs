using System.Diagnostics;

namespace SimpleLauncher.Interfaces;

public interface ILogErrors
{
    Task LogErrorAsync(Exception ex, string contextMessage = null);
}

/// <summary>
/// Extension methods for <see cref="ILogErrors"/> to provide fire-and-forget error logging.
/// </summary>
public static class LogErrorsExtensions
{
    /// <summary>
    /// Logs an error asynchronously in a fire-and-forget manner, swallowing any exceptions that occur during logging.
    /// </summary>
    /// <param name="logErrors">The <see cref="ILogErrors"/> instance to log the error on.</param>
    /// <param name="ex">The exception to log.</param>
    /// <param name="contextMessage">An optional context message describing the error scenario.</param>
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
