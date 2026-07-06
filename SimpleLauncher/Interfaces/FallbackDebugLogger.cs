namespace SimpleLauncher.Interfaces;

/// <summary>
/// A fallback implementation of IDebugLogger for use when the service provider is unavailable.
/// </summary>
internal sealed class FallbackDebugLogger : IDebugLogger
{
    public void Log(string message) { }
    public void LogException(Exception ex, string contextMessage = null) { }
    public void OpenDebugWindow() { }
}
