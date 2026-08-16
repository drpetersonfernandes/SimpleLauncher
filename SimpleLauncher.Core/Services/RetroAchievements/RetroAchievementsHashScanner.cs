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
    /// <param name="fileHasher">The file hasher that delegates hash calculation to the RetroAchievementsSharp library.</param>
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
        return systemId is > 0 and <= ConsoleIds.RcConsoleMax;
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

        // Only recalculate hashes when the number of games in the ROM path has changed
        // or the stored scan was produced by older hash logic; there is no need to
        // hash again if nothing changed.
        var existing = _hashStore.LoadSystemHashes(target.SystemName);
        if (existing != null && existing.FileCount == uniqueFiles.Count && existing.HashVersion == CurrentHashVersion)
        {
            _logger.Information($"[RA Hash Scanner] Hash scan is up to date for '{target.SystemName}' ({uniqueFiles.Count} files). Skipping re-hashing.");
            return HashScanResult.UpToDate;
        }

        _logger.Debug($"[RA Hash Scanner] Calculating hashes for '{target.SystemName}' ({uniqueFiles.Count} files, system id {systemId}).");

        // Arcade games are hashed by file name; every other system hashes file content.
        var isFileNameHashSystem = matchedSystemName.Equals("arcade", StringComparison.OrdinalIgnoreCase);

        // Hash files sequentially: the RVZ filereader used for .rvz/.wia files is
        // process-wide global state, so parallel hashing must be avoided.
        var hashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var filePath in uniqueFiles.Values)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var hash = await CalculateHashForFileAsync(filePath, matchedSystemName, target.FileFormatsToLaunch, isFileNameHashSystem);
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
            HashVersion = CurrentHashVersion,
            Hashes = hashes
        };

        _hashStore.SaveSystemHashes(result);

        _logger.Information($"[RA Hash Scanner] Completed hash scan for '{target.SystemName}': {hashes.Count}/{uniqueFiles.Count} files hashed.");

        return HashScanResult.Completed;
    }

    /// <summary>
    /// Calculates the RetroAchievements hash for a single game file through the
    /// RetroAchievementsSharp library (<see cref="IRetroAchievementsFileHasher"/>).
    /// .zip archives are handled by the library without extracting to disk
    /// (<see cref="HashZipFileAsync"/>); only .7z/.rar archives are extracted to a
    /// temporary folder first.
    /// </summary>
    /// <param name="gamePath">The full path to the game file.</param>
    /// <param name="matchedSystemName">The resolved RetroAchievements system name.</param>
    /// <param name="fileFormatsToLaunch">The extensions to look for inside .7z/.rar archives.</param>
    /// <param name="isFileNameHashSystem">True for arcade (filename-hashed) systems, which never extract.</param>
    /// <returns>The 32-character hash, or null if the file could not be hashed.</returns>
    private async Task<string?> CalculateHashForFileAsync(
        string gamePath,
        string matchedSystemName,
        IList<string> fileFormatsToLaunch,
        bool isFileNameHashSystem)
    {
        var fileExtension = Path.GetExtension(gamePath).ToLowerInvariant();

        // .zip files are handled by the library itself (same semantics as the
        // RetroAchievementsSharp CLI: load the entry and hash from a buffer)
        if (string.Equals(fileExtension, ".zip", StringComparison.OrdinalIgnoreCase) && !isFileNameHashSystem)
        {
            return await HashZipFileAsync(gamePath, matchedSystemName);
        }

        string? tempExtractionPath = null;
        var fileToProcess = gamePath;

        try
        {
            var isCompressed = fileExtension is ".7z" or ".rar";

            if (isCompressed && !isFileNameHashSystem && fileFormatsToLaunch is { Count: > 0 })
            {
                var (extractedGameFilePath, extractedTempDirPath) = await _extractionService.ExtractToTempAndGetLaunchFileAsync(gamePath, fileFormatsToLaunch);
                tempExtractionPath = extractedTempDirPath;

                if (string.IsNullOrEmpty(extractedGameFilePath))
                {
                    _logger.Information($"[RA Hash Scanner] Failed to extract a suitable file from archive for hashing: {gamePath}.");
                    return null;
                }

                fileToProcess = extractedGameFilePath;
            }

            return await _fileHasher.CalculateHashAsync(fileToProcess, matchedSystemName);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"[RA Hash Scanner] An exception occurred while hashing '{gamePath}'.");
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
                    _logger.Debug($"[RA Hash Scanner] Failed to clean up temporary extraction folder '{tempExtractionPath}': {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// Hashes a .zip archive through the RetroAchievementsSharp library without
    /// extracting to disk, mirroring the library CLI zip pre-load:
    /// single-entry zips hash the first entry's content from a buffer; multi-entry
    /// zips hash the whole archive; entries too large for a byte[] fall back to a
    /// temporary file that is deleted afterwards.
    /// </summary>
    private async Task<string?> HashZipFileAsync(string zipPath, string matchedSystemName)
    {
        string? tempZipPath = null;

        try
        {
            var data = FileUtil.LoadZippedFile(zipPath, out _);
            if (data != null)
            {
                return await _fileHasher.CalculateHashFromBufferAsync(data, matchedSystemName);
            }

            // The entry is too large for an in-memory buffer — hash from a temp file
            tempZipPath = FileUtil.LoadZippedFileToTemp(zipPath, out _);
            if (tempZipPath == null)
            {
                _logger.Information($"[RA Hash Scanner] Could not load the content of '{zipPath}' for hashing.");
                return null;
            }

            return await _fileHasher.CalculateHashAsync(tempZipPath, matchedSystemName);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"[RA Hash Scanner] An exception occurred while hashing zip file '{zipPath}'.");
            return null;
        }
        finally
        {
            if (!string.IsNullOrEmpty(tempZipPath))
            {
                try
                {
                    if (File.Exists(tempZipPath))
                    {
                        File.Delete(tempZipPath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Debug($"[RA Hash Scanner] Failed to clean up temporary zip file '{tempZipPath}': {ex.Message}");
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