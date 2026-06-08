using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Tests.TestHelpers;

public class NoOpDebugLogger : IDebugLogger
{
    public void Log(string message)
    {
    }

    public void LogException(Exception ex, string? contextMessage = null)
    {
    }
}
