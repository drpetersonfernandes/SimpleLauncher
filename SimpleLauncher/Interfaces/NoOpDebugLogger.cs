namespace SimpleLauncher.Interfaces;

/// <summary>
/// A no-op implementation of IDebugLogger for use as a fallback when the service provider is unavailable.
/// </summary>
public sealed class NoOpDebugLogger : IDebugLogger
{
    public void Log(string message) { }
    public void LogException(Exception ex, string? contextMessage = null) { }
    public void OpenDebugWindow() { }
}
