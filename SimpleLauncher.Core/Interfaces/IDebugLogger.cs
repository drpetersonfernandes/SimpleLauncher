namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Defines the contract for debug logging services.
/// </summary>
public interface IDebugLogger
{
    void Log(string message);
    void LogException(Exception ex, string? contextMessage = null);
    void OpenDebugWindow();
}