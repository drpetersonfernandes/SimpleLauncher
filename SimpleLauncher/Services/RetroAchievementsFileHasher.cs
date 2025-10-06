using System;
using System.Globalization;
using System.IO;
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
