using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace SimpleLauncher.Services;

public static class FileHasher
{
    public static async Task<string> CalculateMd5Async(string filePath)
    {
        if (!File.Exists(filePath))
        {
            _ = LogErrors.LogErrorAsync(new FileNotFoundException("File not found for hashing.", filePath), $"FileHasher: File not found at {filePath}");
            return null;
        }

        try
        {
            using var md5 = MD5.Create();
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var hashBytes = await md5.ComputeHashAsync(stream);
            var sb = new StringBuilder();
            foreach (var b in hashBytes)
            {
                sb.Append(b.ToString("x2", CultureInfo.InvariantCulture));
            }

            return sb.ToString();
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"Failed to calculate MD5 for {filePath}");
            return null;
        }
    }
}