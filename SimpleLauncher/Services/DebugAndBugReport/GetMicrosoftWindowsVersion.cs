namespace SimpleLauncher.Services.DebugAndBugReport;

/// <summary>
/// Provides a static method to retrieve a human-readable Windows version string.
/// </summary>
public static class GetMicrosoftWindowsVersion
{
    /// <summary>
    /// Returns a human-readable string identifying the current Windows version.
    /// </summary>
    /// <returns>A string such as "Windows 10 or Windows 11", "Windows 8.1", "Windows 8", "Windows 7", or an unknown version message.</returns>
    public static string GetVersion()
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
