namespace SimpleLauncher.Services.DebugAndBugReport;

public class DebugLoggerAdapter : IDebugLogger
{
    public void Log(string message)
    {
        DebugLogger.Log(message);
    }

    public void LogException(Exception ex, string contextMessage = null)
    {
        DebugLogger.LogException(ex, contextMessage);
    }
}
