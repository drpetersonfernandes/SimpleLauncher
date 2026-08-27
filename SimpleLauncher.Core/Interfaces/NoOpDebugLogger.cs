namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// A no-op implementation of IDebugLogger for use as a fallback when the service provider is unavailable.
/// </summary>
public sealed class NoOpDebugLogger : IDebugLogger
{
    /// <summary>
    /// No-op implementation that discards the log message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Log(string message)
    {
    }

    /// <summary>
    /// No-op implementation that discards the exception log.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="contextMessage">An optional context message describing where the exception occurred.</param>
    public void LogException(Exception ex, string? contextMessage = null)
    {
    }

    /// <summary>
    /// No-op implementation that does nothing when called.
    /// </summary>
    public void OpenDebugWindow()
    {
    }
}