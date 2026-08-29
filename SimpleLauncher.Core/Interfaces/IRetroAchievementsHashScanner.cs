using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Core.Interfaces;

/// <summary>
///     Scans game paths in the background and calculates RetroAchievements hashes
///     for every game file, persisting the results through <see cref="IRetroAchievementsHashStore" />.
///     Only one scan can run at a time; concurrent requests are rejected.
/// </summary>
public interface IRetroAchievementsHashScanner
{
    /// <summary>
    ///     Gets a value indicating whether a hash scan is currently running.
    /// </summary>
    bool IsScanning { get; }

    /// <summary>
    ///     Determines whether the given system can be hashed for RetroAchievements.
    /// </summary>
    /// <param name="systemName">The name of the system.</param>
    /// <returns>True if the system has a valid RetroAchievements console ID; otherwise, false.</returns>
    bool IsSystemScannable(string systemName);

    /// <summary>
    ///     Determines whether an existing hash scan for the given system was produced by the
    ///     current hash logic (same <see cref="RaSystemHashes.HashVersion" />).
    /// </summary>
    /// <param name="systemName">The name of the system.</param>
    /// <returns>True if the stored scan is up to date; false if missing or stale.</returns>
    bool IsScanUpToDate(string systemName);

    /// <summary>
    ///     Scans the game folders of a single system and persists the calculated hashes.
    /// </summary>
    /// <param name="systemName">The name of the system to scan.</param>
    /// <param name="systemFolders">The list of configured system folders (relative or absolute).</param>
    /// <param name="fileFormatsToSearch">The list of file extensions to search for.</param>
    /// <param name="fileFormatsToLaunch">The list of file extensions to look for inside compressed files before hashing.</param>
    /// <param name="disableRecursiveSearch">True to skip subfolders during the scan.</param>
    /// <param name="groupByFolder">True if the system groups game entries by folder.</param>
    /// <param name="onCompleted">Optional callback invoked (on a background thread) when the scan completes.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if the scan started; false if a scan is already running or the system is not scannable.</returns>
    Task<bool> ScanSystemAsync(
        string systemName,
        IList<string> systemFolders,
        IList<string> fileFormatsToSearch,
        IList<string> fileFormatsToLaunch,
        bool disableRecursiveSearch,
        bool groupByFolder,
        Action<string>? onCompleted = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    ///     Scans the game folders of multiple systems sequentially and persists the calculated hashes.
    /// </summary>
    /// <param name="targets">The systems to scan, in order.</param>
    /// <param name="onCompleted">Optional callback invoked (on a background thread) after each system completes.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>True if the scan started; false if a scan is already running.</returns>
    Task<bool> ScanAllSystemsAsync(
        IEnumerable<RaHashScanTarget> targets,
        Action<string>? onCompleted = null,
        CancellationToken cancellationToken = default);
}