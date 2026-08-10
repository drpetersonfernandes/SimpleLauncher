namespace SimpleLauncher.Core.Services;

/// <summary>
/// Resolves the SimpleLauncher application-data folder robustly across platforms.
/// </summary>
/// <remarks>
/// On Linux, <see cref="Environment.SpecialFolder.LocalApplicationData"/> can return an
/// empty or relative value when XDG_DATA_HOME is unset, which would silently relocate
/// logs/data into the process working directory. This helper falls back to the XDG
/// default (<c>~/.local/share</c>) and, as a last resort, the app base directory.
/// </remarks>
public static class AppDataPaths
{
    /// <summary>
    /// Gets the SimpleLauncher data folder (logs, window bounds, data files).
    /// </summary>
    public static string SimpleLauncherDataFolder => GetSimpleLauncherDataFolder();

    /// <summary>
    /// Resolves the SimpleLauncher data folder.
    /// </summary>
    /// <returns>An absolute path to the SimpleLauncher data folder.</returns>
    public static string GetSimpleLauncherDataFolder()
    {
        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        if (string.IsNullOrWhiteSpace(localAppData) || !Path.IsPathRooted(localAppData))
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            if (!string.IsNullOrWhiteSpace(home))
            {
                localAppData = OperatingSystem.IsWindows()
                    ? Path.Combine(home, "AppData", "Local")
                    : Path.Combine(home, ".local", "share");
            }
        }

        if (string.IsNullOrWhiteSpace(localAppData) || !Path.IsPathRooted(localAppData))
        {
            localAppData = AppDomain.CurrentDomain.BaseDirectory; // last resort
        }

        return Path.Combine(localAppData, "SimpleLauncher");
    }
}
