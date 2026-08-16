using RetroAchievementsSharp;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using PathHelper = SimpleLauncher.Core.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Core.Services.RetroAchievements;

/// <summary>
/// Scans game paths in the background and calculates RetroAchievements hashes for
/// every game file using <see cref="IRetroAchievementsFileHasher"/>. Results are
/// persisted per system through <see cref="IRetroAchievementsHashStore"/>.
/// Only one scan runs at a time to protect the RVZ global filereader state and to
/// prevent parallel scans from crashing the application.
/// </summary>
public class RetroAchievementsHashScanner : IRetroAchievementsHashScanner
{
    private readonly ILogger _logger;
    private readonly IRetroAchievementsSystemMatcher _systemMatcher;
    private readonly IRetroAchievementsFileHasher _fileHasher;
    private readonly IGetListOfFilesService _getListOfFiles;
    private readonly IRetroAchievementsHashStore _hashStore;

    private int _isScanningFlag;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetroAchievementsHashScanner"/> class.
    /// </summary>
    /// <param name="logErrors">The logger instance used for debugging and error output.</param>
    /// <param name="systemMatcher">The system matcher used to resolve system names to RetroAchievements console IDs.</param>
    /// <param name="fileHasher">The file hasher that delegates hash calculation to the RetroAchievementsSharp library.</param>
    /// <param name="getListOfFiles">The service used to enumerate game files in the configured folders.</param>
    /// <param name="hashStore">The store used to persist the calculated hashes.</param>
    public RetroAchievementsHashScanner(
        ILogger logErrors,
        IRetroAchievementsSystemMatcher systemMatcher,
        IRetroAchievementsFileHasher fileHasher,
        IGetListOfFilesService getListOfFiles,
        IRetroAchievementsHashStore hashStore)
    {
        _logger = logErrors;
        _systemMatcher = systemMatcher;
        _fileHasher = fileHasher;
        _getListOfFiles = getListOfFiles;
        _hashStore = hashStore;
    }

    /// <summary>
    /// Gets a value indicating whether a hash scan is currently running.
    /// </summary>
    public bool IsScanning => Volatile.Read(ref _isScanningFlag) == 1;

    /// <summary>
    /// Determines whether the given system can be hashed for RetroAchievements.
    /// Systems without a usable console ID (including the "unsupported" pseudo-system, ID 102)
    /// are not scannable.
    /// </summary>
    public bool IsSystemScannable(string systemName)
    {
        var matchedName = ResolveSystemName(systemName);
        var systemId = _systemMatcher.GetSystemId(matchedName);
        return systemId is > 0 and <= ConsoleIds.RcConsoleMax;
    }

    /// <summary>
    /// Scans the game folders of a single system and persists the calculated hashes.
    /// </summary>
    public Task<bool> ScanSystemAsync(
        string systemName,
        IList<string> systemFolders,
        IList<string> fileFormatsToSearch,
        bool disableRecursiveSearch,
        bool groupByFolder,
        Action<string>? onCompleted = null,
        CancellationToken cancellationToken = default)
    {
        var target = new RaHashScanTarget
        {
            SystemName = systemName,
            SystemFolders = systemFolders,
            FileFormatsToSearch = fileFormatsToSearch,
            DisableRecursiveSearch = disableRecursiveSearch,
            GroupByFolder = groupByFolder
        };

        return ScanAllSystemsAsync([target], onCompleted, cancellationToken);
    }

    /// <summary>
    /// Scans the game folders of multiple systems sequentially and persists the calculated hashes.
    /// The whole operation runs on a thread-pool thread so the UI thread is never blocked.
    /// </summary>
    public async Task<bool> ScanAllSystemsAsync(
        IEnumerable<RaHashScanTarget> targets,
        Action<string>? onCompleted = null,
        CancellationToken cancellationToken = default)
    {
        // Prevent parallel hash calculations (they could crash the application)
        if (Interlocked.CompareExchange(ref _isScanningFlag, 1, 0) != 0)
        {
            _logger.Information("[RA Hash Scanner] A hash scan is already in progress. Ignoring the new request.");
            return false;
        }

        try
        {
            await Task.Run(() => ScanCoreAsync(targets.ToList(), onCompleted, cancellationToken), cancellationToken);
            return true;
        }
        finally
        {
            Volatile.Write(ref _isScanningFlag, 0);
        }
    }

    private async Task ScanCoreAsync(
        IList<RaHashScanTarget> targets,
        Action<string>? onCompleted,
        CancellationToken cancellationToken)
    {
        foreach (var target in targets)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var result = await ScanSystemCoreAsync(target, cancellationToken);
                if (result == HashScanResult.Completed)
                {
                    onCompleted?.Invoke(target.SystemName);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Debug($"[RA Hash Scanner] Scan canceled for '{target.SystemName}'.");
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, $"[RA Hash Scanner] Failed to scan system '{target.SystemName}'.");
            }
        }
    }

    private async Task<HashScanResult> ScanSystemCoreAsync(RaHashScanTarget target, CancellationToken cancellationToken)
    {
        var matchedSystemName = ResolveSystemName(target.SystemName);
        var systemId = _systemMatcher.GetSystemId(matchedSystemName);
        if (systemId is <= 0 or > ConsoleIds.RcConsoleMax)
        {
            _logger.Information($"[RA Hash Scanner] System '{target.SystemName}' is not supported for RetroAchievements hashing. Skipping.");
            return HashScanResult.NotScannable;
        }

        // Enumerate all game files across the configured folders (same logic as the game list cache)
        var uniqueFiles = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var folder in target.SystemFolders)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var resolvedPath = PathHelper.ResolveRelativeToAppDirectory(folder);
            if (string.IsNullOrEmpty(resolvedPath) ||
                !Directory.Exists(resolvedPath) ||
                target.FileFormatsToSearch == null) continue;

            var filesInFolder = await _getListOfFiles.GetFilesAsync(
                resolvedPath, target.FileFormatsToSearch, target.DisableRecursiveSearch, target.GroupByFolder, cancellationToken);

            foreach (var file in filesInFolder)
            {
                uniqueFiles.TryAdd(Path.GetFileName(file), file);
            }
        }

        // Only recalculate hashes when the number of games in the ROM path has changed;
        // there is no need to hash again if no new game was added or removed.
        var existing = _hashStore.LoadSystemHashes(target.SystemName);
        if (existing != null && existing.FileCount == uniqueFiles.Count)
        {
            _logger.Information($"[RA Hash Scanner] Hash scan is up to date for '{target.SystemName}' ({uniqueFiles.Count} files). Skipping re-hashing.");
            return HashScanResult.UpToDate;
        }

        _logger.Debug($"[RA Hash Scanner] Calculating hashes for '{target.SystemName}' ({uniqueFiles.Count} files, system id {systemId}).");

        // Hash files sequentially: the RVZ filereader used for .rvz/.wia files is
        // process-wide global state, so parallel hashing must be avoided.
        var hashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var filePath in uniqueFiles.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hash = await _fileHasher.CalculateHashAsync(filePath, matchedSystemName);
            if (!string.IsNullOrEmpty(hash))
            {
                hashes[filePath] = hash;
            }
        }

        var result = new RaSystemHashes
        {
            SystemName = target.SystemName,
            ScannedAtUtc = DateTime.UtcNow,
            FileCount = uniqueFiles.Count,
            Hashes = hashes
        };

        _hashStore.SaveSystemHashes(result);

        _logger.Information($"[RA Hash Scanner] Completed hash scan for '{target.SystemName}': {hashes.Count}/{uniqueFiles.Count} files hashed.");

        return HashScanResult.Completed;
    }

    /// <summary>
    /// Resolves a local system name to its official RetroAchievements system name.
    /// </summary>
    private string ResolveSystemName(string systemName)
    {
        if (string.IsNullOrWhiteSpace(systemName)) return "";

        return _systemMatcher.GetExactAliasMatch(systemName) ?? _systemMatcher.GetBestMatchSystemName(systemName);
    }

    /// <summary>
    /// The outcome of scanning a single system.
    /// </summary>
    private enum HashScanResult
    {
        /// <summary>The system cannot be hashed for RetroAchievements.</summary>
        NotScannable,

        /// <summary>The stored hash scan is still valid (the game count has not changed).</summary>
        UpToDate,

        /// <summary>The system was re-scanned and its hashes were persisted.</summary>
        Completed
    }
}