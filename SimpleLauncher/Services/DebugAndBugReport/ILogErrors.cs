namespace SimpleLauncher.Services.DebugAndBugReport;

public interface ILogErrors
{
    Task LogErrorAsync(Exception ex, string contextMessage = null);
}