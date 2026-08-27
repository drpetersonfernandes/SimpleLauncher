using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using PathHelper = SimpleLauncher.Core.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Core.Services.RetroAchievements;

/// <summary>
/// Scans game paths in the background and calculates RetroAchievements hashes for
/// every game file through the bundled RetroAchievementsSharp CLI tool
/// (<see cref="IRetroAchievementsFileHasher"/>). Results are persisted per system
/// through <see cref="IRetroAchievementsHashStore"/>.
/// Only one scan runs at a time to avoid hammering the disk and the CLI process pool.
/// </summary>
public class RetroAchievementsHashScanner : IRetroAchievementsHashScanner
{
    /// <summary>
    /// Version of the hash calculation logic. Bump this whenever the hashing behavior
    /// changes (e.g. extraction rules) so existing scans are recalculated.
    /// </summary>
    private const int CurrentHashVersion = 1;

    private readonly ILogger _logger;
    private readonly IRetroAchievementsSystemMatcher _systemMatcher;
    private readonly IRetroAchievementsFileHasher _fileHasher;
    private readonly IGetListOfFilesService _getListOfFiles;
    private readonly IExtractionService _extractionService;
    private readonly IRetroAchievementsHashStore _hashStore;

    private int _isScanningFlag;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetroAchievementsHashScanner"/> class.
    /// </summary>
    /// <param name="logErrors">The logger instance used for debugging and error output.</param>
    /// <param name="systemMatcher">The system matcher used to resolve system names to RetroAchievements console IDs.</param>
    /// <param name="fileHasher">The file hasher that delegates hash calculation to the RetroAchievementsSharp CLI tool.</param>
    /// <param name="getListOfFiles">The service used to enumerate game files in the configured folders.</param>
    /// <param name="extractionService">The service used to extract compressed game files before hashing.</param>
    /// <param name="hashStore">The store used to persist the calculated hashes.</param>
    public RetroAchievementsHashScanner(
        ILogger logErrors,
        IRetroAchievementsSystemMatcher systemMatcher,
        IRetroAchievementsFileHasher fileHasher,
        IGetListOfFilesService getListOfFiles,
        IExtractionService extractionService,
        IRetroAchievementsHashStore hashStore)
    {
        _logger = logErrors;
        _systemMatcher = systemMatcher;
        _fileHasher = fileHasher;
        _getListOfFiles = getListOfFiles;
        _extractionService = extractionService;
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
        return systemId is > 0 and <= RetroAchievementsConstants.MaxConsoleId;
    }

    /// <summary>
    /// Determines whether an existing hash scan for the given system was produced by the
    /// current hash logic (same <see cref="RaSystemHashes.HashVersion"/>).
    /// </summary>
    public bool IsScanUpToDate(string systemName)
    {
        var existing = _hashStore.LoadSystemHashes(systemName);
        return existing is { HashVersion: CurrentHashVersion };
    }

    /// <summary>
    /// Scans the game folders of a single system and persists the calculated hashes.
    /// </summary>
    public Task<bool> ScanSystemAsync(
        string systemName,
        IList<string> systemFolders,
        IList<string> fileFormatsToSearch,
        IList<string> fileFormatsToLaunch,
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
            FileFormatsToLaunch = fileFormatsToLaunch,
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
        // Prevent parallel hash scans (they would spawn many CLI processes at once)
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
        if (systemId is <= 0 or > RetroAchievementsConstants.MaxConsoleId)
        {
            _logger.Information(
                $"[RA Hash Scanner] System '{target.SystemName}' is not supported for RetroAchievements hashing. Skipping.");
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
                resolvedPath, target.FileFormatsToSearch, target.DisableRecursiveSearch, target.GroupByFolder,
                cancellationToken);

            foreach (var file in filesInFolder)
            {
                uniqueFiles.TryAdd(Path.GetFileName(file), file);
            }
        }

        // Only recalculate hashes when the number of games in the ROM path has changed
        // or the stored scan was produced by older hash logic; there is no need to
        // hash again if nothing changed.
        var existing = _hashStore.LoadSystemHashes(target.SystemName);
        if (existing != null && existing.FileCount == uniqueFiles.Count && existing.HashVersion == CurrentHashVersion)
        {
            _logger.Information(
                $"[RA Hash Scanner] Hash scan is up to date for '{target.SystemName}' ({uniqueFiles.Count} files). Skipping re-hashing.");
            return HashScanResult.UpToDate;
        }

        _logger.Debug(
            $"[RA Hash Scanner] Calculating hashes for '{target.SystemName}' ({uniqueFiles.Count} files, system id {systemId}).");

        // Arcade games are hashed by file name; every other system hashes file content.
        var isFileNameHashSystem = matchedSystemName.Equals("arcade", StringComparison.OrdinalIgnoreCase);

        // Files the CLI tool can hash directly (including .zip — the tool pre-loads
        // the first entry itself); .7z/.rar archives must be extracted first.
        var directFiles = new List<string>();
        var archiveFiles = new List<string>();

        foreach (var filePath in uniqueFiles.Values)
        {
            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
            if (fileExtension is ".7z" or ".rar" && !isFileNameHashSystem)
            {
                archiveFiles.Add(filePath);
            }
            else
            {
                directFiles.Add(filePath);
            }
        }

        var hashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        // Batch-hash all directly supported files in as few CLI invocations as possible
        if (directFiles.Count > 0)
        {
            var batchHashes = await _fileHasher.CalculateHashesAsync(directFiles, matchedSystemName, cancellationToken);
            foreach (var (filePath, hash) in batchHashes)
            {
                hashes[filePath] = hash;
            }
        }

        // .7z/.rar archives are extracted to a temporary folder first, then hashed individually
        foreach (var archivePath in archiveFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hash = await HashExtractedArchiveAsync(archivePath, matchedSystemName, target.FileFormatsToLaunch);
            if (!string.IsNullOrEmpty(hash))
            {
                hashes[archivePath] = hash;
            }
        }

        var result = new RaSystemHashes
        {
            SystemName = target.SystemName,
            ScannedAtUtc = DateTime.UtcNow,
            FileCount = uniqueFiles.Count,
            HashVersion = CurrentHashVersion,
            Hashes = hashes
        };

        _hashStore.SaveSystemHashes(result);

        _logger.Information(
            $"[RA Hash Scanner] Completed hash scan for '{target.SystemName}': {hashes.Count}/{uniqueFiles.Count} files hashed.");

        return HashScanResult.Completed;
    }

    /// <summary>
    /// Extracts a .7z/.rar archive to a temporary folder and calculates the hash of
    /// the extracted game file through the RetroAchievementsSharp CLI tool
    /// (<see cref="IRetroAchievementsFileHasher"/>). The temporary folder is deleted
    /// afterwards.
    /// </summary>
    /// <param name="archivePath">The full path to the .7z/.rar archive.</param>
    /// <param name="matchedSystemName">The resolved RetroAchievements system name.</param>
    /// <param name="fileFormatsToLaunch">The extensions to look for inside the archive.</param>
    /// <returns>The 32-character hash, or null if the file could not be hashed.</returns>
    private async Task<string?> HashExtractedArchiveAsync(string archivePath, string matchedSystemName,
        IList<string> fileFormatsToLaunch)
    {
        string? tempExtractionPath = null;

        try
        {
            if (fileFormatsToLaunch is not { Count: > 0 })
            {
                _logger.Information(
                    $"[RA Hash Scanner] No launchable formats configured; skipping archive '{archivePath}'.");
                return null;
            }

            var (extractedGameFilePath, extractedTempDirPath) =
                await _extractionService.ExtractToTempAndGetLaunchFileAsync(archivePath, fileFormatsToLaunch);
            tempExtractionPath = extractedTempDirPath;

            if (string.IsNullOrEmpty(extractedGameFilePath))
            {
                _logger.Information(
                    $"[RA Hash Scanner] Failed to extract a suitable file from archive for hashing: {archivePath}.");
                return null;
            }

            return await _fileHasher.CalculateHashAsync(extractedGameFilePath, matchedSystemName);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"[RA Hash Scanner] An exception occurred while hashing archive '{archivePath}'.");
            return null;
        }
        finally
        {
            if (!string.IsNullOrEmpty(tempExtractionPath))
            {
                try
                {
                    if (Directory.Exists(tempExtractionPath))
                    {
                        Directory.Delete(tempExtractionPath, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Debug(
                        $"[RA Hash Scanner] Failed to clean up temporary extraction folder '{tempExtractionPath}': {ex.Message}");
                }
            }
        }
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