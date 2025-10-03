using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLauncher.Services;

/// <summary>
/// Provides methods for hashing ROM files according to RetroAchievements' specifications.
/// </summary>
public static class RetroAchievementsFileHasher
{
    /// <summary>
    /// Calculates the MD5 hash of the entire file.
    /// </summary>
    public static async Task<string> CalculateStandardMd5Async(string filePath)
    {
        return await CalculateMd5WithOffsetAsync(filePath, 0);
    }

    /// <summary>
    /// Calculates the MD5 hash of a file, starting from a specific offset.
    /// </summary>
    public static async Task<string> CalculateMd5WithOffsetAsync(string filePath, long offset)
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
            _ = LogErrors.LogErrorAsync(ex, $"Failed to calculate MD5 for {filePath} with offset {offset}");
            return null;
        }
    }

    /// <summary>
    /// Checks if a file starts with a specific byte sequence (header/magic bytes).
    /// If it does, calculates the MD5 hash skipping the specified header size.
    /// Otherwise, calculates the MD5 hash of the entire file.
    /// </summary>
    public static async Task<string> CalculateMd5WithHeaderCheckAsync(string filePath, int headerSize, byte[] magicBytes)
    {
        try
        {
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            if (stream.Length < magicBytes.Length)
            {
                return await CalculateStandardMd5Async(filePath); // File is too small to have the header
            }

            var buffer = new byte[magicBytes.Length];
            _ = await stream.ReadAsync(buffer);

            return buffer.SequenceEqual(magicBytes)
                ? await CalculateMd5WithOffsetAsync(filePath, headerSize)
                : await CalculateStandardMd5Async(filePath);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"Failed header check for {filePath}");
            return null;
        }
    }

    /// <summary>
    /// Calculates the hash for Arcade games by hashing the filename without its extension.
    /// </summary>
    public static string CalculateFilenameHash(string filePath)
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
            _ = LogErrors.LogErrorAsync(ex, $"Failed to calculate Arcade hash for {filePath}");
            return null;
        }
    }

    /// <summary>
    /// Calculates the MD5 hash for Nintendo 64 ROMs, handling different byte orders.
    /// Converts byte-swapped (.v64) and little-endian (.n64) ROMs to big-endian (.z64) in memory before hashing.
    /// </summary>
    [SuppressMessage("ReSharper", "RedundantCaseLabel")]
    public static async Task<string> CalculateHashWithByteSwappingAsync(string filePath)
    {
        try
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            var romData = await File.ReadAllBytesAsync(filePath);

            switch (extension)
            {
                // .v64 / .b64 (Byte-swapped) -> swap every 2 bytes
                case ".v64":
                case ".b64":
                    for (var i = 0; i < romData.Length; i += 2)
                    {
                        (romData[i], romData[i + 1]) = (romData[i + 1], romData[i]);
                    }

                    break;

                // .n64 (Little-endian) -> swap every 4 bytes
                case ".n64":
                    for (var i = 0; i < romData.Length; i += 4)
                    {
                        (romData[i], romData[i + 3]) = (romData[i + 3], romData[i]);
                        (romData[i + 1], romData[i + 2]) = (romData[i + 2], romData[i + 1]);
                    }

                    break;

                    // .z64 (Big-endian) is the target format, no changes needed.
                    case ".z64":
                    default:
                        break;
            }

            var hashBytes = MD5.HashData(romData);
            return ToHexString(hashBytes);
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"Failed to calculate N64 hash for {filePath}");
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
