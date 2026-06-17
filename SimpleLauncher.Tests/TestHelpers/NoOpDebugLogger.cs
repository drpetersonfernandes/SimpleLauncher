using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Tests.TestHelpers;

/// <summary>
/// No-op implementation of <see cref="IDebugLogger"/> for unit tests.
/// All logging calls are silently discarded.
/// </summary>
public class NoOpDebugLogger : IDebugLogger
{
    /// <summary>
    /// Does nothing. Discards the log message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Log(string message)
    {
    }

    /// <summary>
    /// Does nothing. Discards the exception information.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="contextMessage">Optional context message.</param>
    public void LogException(Exception ex, string? contextMessage = null)
    {
    }

    /// <summary>
    /// Does nothing.
    /// </summary>
    public void OpenDebugWindow()
    {
    }
}
