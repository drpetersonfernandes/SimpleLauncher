using RetroAchievementsSharp;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Core.Services.RetroAchievements;

/// <summary>
/// Calculates RetroAchievements hashes for game files. All hash computation is
/// delegated to the RetroAchievementsSharp library (a 1:1 port of the rcheevos
/// hashing engine), which produces the exact same hashes as RAHasher.
/// </summary>
public class RetroAchievementsFileHasher : IRetroAchievementsFileHasher
{
    private readonly ILogger _logger;
    private readonly IRetroAchievementsSystemMatcher _systemMatcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="RetroAchievementsFileHasher"/> class.
    /// </summary>
    /// <param name="logErrors">The logger instance for error logging.</param>
    /// <param name="systemMatcher">The system matcher used to resolve system names to RetroAchievements console IDs.</param>
    public RetroAchievementsFileHasher(ILogger logErrors, IRetroAchievementsSystemMatcher systemMatcher)
    {
        _logger = logErrors;
        _systemMatcher = systemMatcher;
    }

    /// <summary>
    /// Calculates the RetroAchievements hash for a game file using the console ID of the given system.
    /// </summary>
    /// <param name="filePath">The full path to the game file.</param>
    /// <param name="systemName">The RetroAchievements system name (resolved to a console ID internally).</param>
    /// <returns>The 32-character lowercase hex hash, or null if the file could not be hashed.</returns>
    public async Task<string?> CalculateHashAsync(string filePath, string systemName)
    {
        if (!File.Exists(filePath))
        {
            _logger.Information($"[RA File Hasher] File not found for hashing: {filePath}");
            return null;
        }

        var systemId = _systemMatcher.GetSystemId(systemName);
        if (systemId <= 0)
        {
            _logger.Information($"[RA File Hasher] No RetroAchievements console ID found for system '{systemName}'. Skipping hashing.");
            return null;
        }

        // RVZ/WIA images are decoded live through RVZSharp. The RVZ filereader is
        // process-wide global state, so it must be installed only while hashing such
        // files and restored immediately afterwards.
        var extension = Path.GetExtension(filePath);
        var isRvzContainer = extension.Equals(".rvz", StringComparison.OrdinalIgnoreCase) ||
                             extension.Equals(".wia", StringComparison.OrdinalIgnoreCase);

        if (isRvzContainer)
        {
            RvzFilereader.InitRvzFilereader();
        }

        try
        {
            return await Task.Run(() =>
            {
                if (RcHash.GenerateFromFile(out var hash, (uint)systemId, filePath))
                {
                    _logger.Debug($"[RA File Hasher] Calculated hash '{hash}' for '{Path.GetFileName(filePath)}' (System: '{systemName}', ID: {systemId}).");
                    return hash;
                }

                _logger.Information($"[RA File Hasher] Could not hash '{filePath}' for system '{systemName}' (ID: {systemId}). The file format may be unsupported, or 3DS decryption keys may be missing.");
                return null;
            });
        }
        catch (Exception ex)
        {
            _logger.Error(ex, $"[RA File Hasher] An exception occurred while hashing {filePath} for system '{systemName}' (ID: {systemId}).");
            return null;
        }
        finally
        {
            if (isRvzContainer)
            {
                RcHash.InitCustomFilereader(null);
            }
        }
    }
}