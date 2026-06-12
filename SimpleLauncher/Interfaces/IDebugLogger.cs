namespace SimpleLauncher.Interfaces;

public interface IDebugLogger
{
    void Log(string message);
    void LogException(Exception ex, string contextMessage = null);
}
