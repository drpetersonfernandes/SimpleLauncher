using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.RetroAchievements;

public interface IRetroAchievementsFileHasher
{
    Task<string> CalculateStandardMd5Async(string filePath);
    string CalculateFilenameHash(string filePath);
    Task<string> CalculateHeaderBasedMd5Async(string filePath, string systemName);
    Task<string> CalculateArduboyHashAsync(string filePath);
    Task<string> CalculateN64HashAsync(string filePath);
}

/// <summary>
/// Provides methods for hashing ROM files according to RetroAchievements' specifications.
/// </summary>
public class RetroAchievementsFileHasher : IRetroAchievementsFileHasher
{
    private readonly ILogErrors _logErrors;

    public RetroAchievementsFileHasher(ILogErrors logErrors)
    {
        _logErrors = logErrors;
    }

    /// <summary>
    /// Calculates the MD5 hash of the entire file.
    /// </summary>
    public Task<string> CalculateStandardMd5Async(string filePath)
    {
        return CalculateMd5WithOffsetAsync(filePath, 0);
    }

    private async Task<string> CalculateMd5WithOffsetAsync(string filePath, long offset)
    {
        try
        {
            using var md5 = MD5.Create();
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            if (offset > 0 && stream.Length > offset)
            {
                stream.Seek(offset, SeekOrigin.Begin);
            }

            var hashBytes = await md5.ComputeHashAsync(stream);
            return ToHexString(hashBytes);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Failed to calculate MD5 for {filePath} with offset {offset}");
            return null;
        }
    }

    /// <summary>
    /// Calculates the hash for Arcade games by hashing the filename without its extension.
    /// </summary>
    public string CalculateFilenameHash(string filePath)
    {
        try
        {
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            var inputBytes = Encoding.UTF8.GetBytes(fileNameWithoutExtension);
            var hashBytes = MD5.HashData(inputBytes);
            return ToHexString(hashBytes);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Failed to calculate Arcade hash for {filePath}");
            return null;
        }
    }

    /// <summary>
    /// Checks if a file starts with a specific byte sequence (header).
    /// </summary>
    private static async Task<bool> FileStartsWithAsync(string filePath, byte[] expectedHeader)
    {
        if (!File.Exists(filePath) || new FileInfo(filePath).Length < expectedHeader.Length)
        {
            return false;
        }

        var fileHeader = new byte[expectedHeader.Length];
        await using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        var bytesRead = await fs.ReadAsync(fileHeader);

        return bytesRead == expectedHeader.Length && fileHeader.SequenceEqual(expectedHeader);
    }

    /// <summary>
    /// Calculates the MD5 hash for systems that may have a header that needs to be skipped.
    /// The logic is based on the system name and either the file's magic number or its size.
    /// </summary>
    /// <param name="filePath">The full path to the game file.</param>
    /// <param name="systemName">The normalized RetroAchievements system name.</param>
    /// <returns>The calculated hash as a string, or null if an error occurs.</returns>
    public async Task<string> CalculateHeaderBasedMd5Async(string filePath, string systemName)
    {
        long offset = 0;
        try
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                _logErrors.LogAndForget(null, $"[RA File Hasher] File not found for header-based hashing: {filePath}");
                return null;
            }

            switch (systemName.ToLowerInvariant())
            {
                case "atari 7800":
                    // Header: \x01ATARI7800, Offset: 128 bytes
                    byte[] atari7800Header = [0x01, 0x41, 0x54, 0x41, 0x52, 0x49, 0x37, 0x38, 0x30, 0x30];
                    if (fileInfo.Length > 128 && await FileStartsWithAsync(filePath, atari7800Header))
                    {
                        offset = 128;
                    }

                    break;

                case "atari lynx":
                    // Header: LYNX\0, Offset: 64 bytes
                    var lynxHeader = "LYNX\0"u8.ToArray();
                    if (fileInfo.Length > 64 && await FileStartsWithAsync(filePath, lynxHeader))
                    {
                        offset = 64;
                    }

                    break;

                case "famicom disk system":
                    // Header: FDS\x1a, Offset: 16 bytes
                    byte[] fdsHeader = [0x46, 0x44, 0x53, 0x1a];
                    if (fileInfo.Length > 16 && await FileStartsWithAsync(filePath, fdsHeader))
                    {
                        offset = 16;
                    }

                    break;

                case "nintendo entertainment system":
                    // Header: NES\x1a, Offset: 16 bytes
                    byte[] nesHeader = [0x4E, 0x45, 0x53, 0x1a];
                    if (fileInfo.Length > 16 && await FileStartsWithAsync(filePath, nesHeader))
                    {
                        offset = 16;
                    }

                    break;

                case "pc engine/turbografx-16":
                case "supergrafx":
                    // Size-based check: if size is 512 bytes more than a multiple of 128KB (131072), offset is 512
                    if (fileInfo.Length > 512 && fileInfo.Length % 131072 == 512)
                    {
                        offset = 512;
                    }

                    break;

                case "super nintendo entertainment system":
                    // Size-based check: if size is 512 bytes more than a multiple of 8KB (8192), offset is 512
                    if (fileInfo.Length > 512 && fileInfo.Length % 8192 == 512)
                    {
                        offset = 512;
                    }

                    break;

                default:
                    // Fallback for any unexpected system name, hash the whole file.
                    offset = 0;
                    _logErrors.LogAndForget(null, $"[RA File Hasher] CalculateHeaderBasedMd5Async called with an unsupported or unexpected system: {systemName}. Hashing entire file.");
                    break;
            }

            return await CalculateMd5WithOffsetAsync(filePath, offset);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Failed to calculate header-based MD5 for {filePath} (System: {systemName})");
            return null;
        }
    }

    /// <summary>
    /// Calculates the hash for Arduboy files by normalizing line endings.
    /// </summary>
    public async Task<string> CalculateArduboyHashAsync(string filePath)
    {
        try
        {
            var content = await File.ReadAllTextAsync(filePath, Encoding.ASCII);
            var normalizedContent = content.Replace("\r\n", "\n");
            var inputBytes = Encoding.UTF8.GetBytes(normalizedContent);
            var hashBytes = MD5.HashData(inputBytes);
            return ToHexString(hashBytes);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Failed to calculate Arduboy hash for {filePath}");
            return null;
        }
    }

    /// <summary>
    /// Calculates the hash for Nintendo 64 ROMs, handling different byte orders based on file extension.
    /// .z64 (Big Endian) is hashed directly.
    /// .v64 (Byte Swapped) and .n64 (Little Endian) are byte-swapped to Big Endian before hashing.
    /// </summary>
    public async Task<string> CalculateN64HashAsync(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        try
        {
            switch (extension)
            {
                case ".z64": // Big Endian (standard)
                    return await CalculateStandardMd5Async(filePath);

                case ".v64": // Byte-swapped
                case ".n64": // Little-endian
                {
                    return await CalculateByteSwappedMd5Async(filePath);
                }

                default:
                    // Fallback for unknown extensions like .rom, treat as standard.
                    return await CalculateStandardMd5Async(filePath);
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Failed to calculate N64 hash for {filePath}");
            return null;
        }
    }

    /// <summary>
    /// Calculates the MD5 hash of a file with byte-swapping, streaming to avoid loading the entire file into memory.
    /// </summary>
    private async Task<string> CalculateByteSwappedMd5Async(string filePath)
    {
        try
        {
            using var md5 = MD5.Create();
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var buffer = new byte[81920]; // 80 KB buffer
            int bytesRead;

            while ((bytesRead = await stream.ReadAsync(buffer)) > 0)
            {
                // Ensure even count for pair swapping
                var processCount = bytesRead % 2 == 0 ? bytesRead : bytesRead - 1;

                // Swap adjacent bytes in-place within the buffer
                for (var i = 0; i < processCount; i += 2)
                {
                    (buffer[i], buffer[i + 1]) = (buffer[i + 1], buffer[i]);
                }

                md5.TransformBlock(buffer, 0, processCount, null, 0);

                // If there was an odd trailing byte, hash it as-is
                if (bytesRead != processCount)
                {
                    md5.TransformBlock(buffer, processCount, 1, null, 0);
                }
            }

            md5.TransformFinalBlock([], 0, 0);
            return ToHexString(md5.Hash);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Failed to calculate byte-swapped MD5 for {filePath}");
            return null;
        }
    }

    /// <summary>
    /// Converts a byte array to its lowercase hexadecimal string representation.
    /// </summary>
    private static string ToHexString(byte[] bytes)
    {
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
        }

        return sb.ToString();
    }
}