namespace SimpleLauncher.Interfaces;

/// <summary>
/// Calculates RetroAchievements hashes for game files using various hash algorithms.
/// </summary>
public interface IRetroAchievementsFileHasher
{
    /// <summary>
    /// Calculates the MD5 hash of the entire file.
    /// </summary>
    /// <param name="filePath">The full path to the game file.</param>
    /// <returns>The MD5 hash as a lowercase hex string, or null if an error occurs.</returns>
    Task<string?> CalculateStandardMd5Async(string filePath);

    /// <summary>
    /// Calculates the hash for Arcade games by hashing the filename without its extension.
    /// </summary>
    /// <param name="filePath">The full path to the game file.</param>
    /// <returns>The MD5 hash of the filename as a lowercase hex string, or null if an error occurs.</returns>
    string? CalculateFilenameHash(string filePath);

    /// <summary>
    /// Calculates the MD5 hash for systems that may have a header that needs to be skipped.
    /// The logic is based on the system name and either the file's magic number or its size.
    /// </summary>
    /// <param name="filePath">The full path to the game file.</param>
    /// <param name="systemName">The normalized RetroAchievements system name.</param>
    /// <returns>The calculated hash as a string, or null if an error occurs.</returns>
    Task<string?> CalculateHeaderBasedMd5Async(string filePath, string systemName);

    /// <summary>
    /// Calculates the hash for Arduboy files by normalizing line endings.
    /// </summary>
    /// <param name="filePath">The full path to the game file.</param>
    /// <returns>The calculated hash as a string, or null if an error occurs.</returns>
    Task<string?> CalculateArduboyHashAsync(string filePath);

    /// <summary>
    /// Calculates the hash for Nintendo 64 ROMs, handling different byte orders based on file extension.
    /// .z64 (Big Endian) is hashed directly.
    /// .v64 (Byte Swapped) and .n64 (Little Endian) are byte-swapped to Big Endian before hashing.
    /// </summary>
    /// <param name="filePath">The full path to the game file.</param>
    /// <returns>The calculated hash as a string, or null if an error occurs.</returns>
    Task<string?> CalculateN64HashAsync(string filePath);
}
