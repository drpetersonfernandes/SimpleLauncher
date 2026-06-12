using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.DebugAndBugReport;

/// <summary>
/// Retrieves the current Windows version as a human-readable string.
/// </summary>
public class WindowsVersionService : IWindowsVersionService
{
    /// <summary>
    /// Returns a human-readable string identifying the current Windows version.
    /// </summary>
    /// <returns>A string such as "Windows 10 or Windows 11", "Windows 8.1", "Windows 8", "Windows 7", or an unknown version message.</returns>
    public string GetVersion()
    {
        var version = Environment.OSVersion.Version;
        return version switch
        {
            { Major: 10, Minor: 0 } => "Windows 10 or Windows 11",
            { Major: 6, Minor: 3 } => "Windows 8.1",
            { Major: 6, Minor: 2 } => "Windows 8",
            { Major: 6, Minor: 1 } => "Windows 7",
            _ => $"Unknown Windows Version ({version})"
        };
    }
}
