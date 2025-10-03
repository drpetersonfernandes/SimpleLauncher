using System;
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
public static class FileHasher
{
    /// <summary>
    /// Calculates the appropriate hash for a given file based on the console/system name.
    /// This method follows the specifications from the RetroAchievements documentation.
    /// </summary>
    /// <param name="filePath">The full path to the ROM file.</param>
    /// <param name="systemName">The name of the system/console the ROM belongs to.</param>
    /// <returns>The calculated hash as a lowercase string, or null if hashing fails or is not supported for the system.</returns>
    public static async Task<string> GetRetroAchievementsHashAsync(string filePath, string systemName)
    {
        if (!File.Exists(filePath))
        {
            _ = LogErrors.LogErrorAsync(new FileNotFoundException("File not found for hashing.", filePath), $"FileHasher: File not found at {filePath}");
            return null;
        }

        // Normalize system name for matching
        var normalizedSystemName = systemName.ToLowerInvariant();

        // Dispatch to the correct hashing method based on the system name
        // See: https://docs.retroachievements.org/game-identification/
        switch (normalizedSystemName)
        {
            // --- Custom Hash: Filename ---
            case "arcade":
                return CalculateArcadeHash(filePath);

            // --- Custom Hash: Nintendo 64 Byte-Swapping ---
            case "nintendo 64":
                return await CalculateN64HashAsync(filePath);

            // --- Header Skip: Magic Bytes ---
            case "atari 7800":
                var header7800 = new byte[] { 0x01, 0x41, 0x54, 0x41, 0x52, 0x49, 0x37, 0x38, 0x30, 0x30 }; // \1ATARI7800
                return await CalculateMd5WithHeaderCheckAsync(filePath, 128, header7800);

            case "atari lynx":
                var headerLynx = "LYNX\0"u8.ToArray(); // LYNX\0
                return await CalculateMd5WithHeaderCheckAsync(filePath, 64, headerLynx);

            case "famicom disk system":
                var headerFds = new byte[] { 0x46, 0x44, 0x53, 0x1a }; // FDS\1a
                return await CalculateMd5WithHeaderCheckAsync(filePath, 16, headerFds);

            case "nintendo entertainment system":
            case "famicom":
                var headerNes = new byte[] { 0x4E, 0x45, 0x53, 0x1a }; // NES\1a
                return await CalculateMd5WithHeaderCheckAsync(filePath, 16, headerNes);

            // --- Header Skip: File Size ---
            case "pc engine":
            case "turbografx 16":
            case "supergrafx":
                var fileInfoPc = new FileInfo(filePath);
                return fileInfoPc.Length % 131072 == 512
                    ? await CalculateMd5WithOffsetAsync(filePath, 512)
                    : await CalculateStandardMd5Async(filePath);

            case "super nintendo entertainment system":
            case "super nintendo":
            case "super famicom":
            case "satellaview":
            case "sufami turbo":
                var fileInfoSnes = new FileInfo(filePath);
                return fileInfoSnes.Length % 8192 == 512
                    ? await CalculateMd5WithOffsetAsync(filePath, 512)
                    : await CalculateStandardMd5Async(filePath);

            // --- Systems with Complex/Unimplemented Hashing ---
            // These require parsing disc formats (ISO, CUE/BIN, etc.) which is out of scope for now.
            case "3do interactive multiplayer":
            case "arduboy":
            case "atari jaguar cd":
            case "pc engine cd":
            case "turbografx-cd":
            case "pc-fx":
            case "gamecube":
            case "nintendo ds":
            case "neo geo cd":
            case "dreamcast":
            case "saturn":
            case "sega cd":
            case "playstation":
            case "playstation 2":
            case "playstation portable":
                DebugLogger.Log($"[FileHasher] Hashing for '{systemName}' is not yet implemented.");
                return null;

            // --- Standard MD5 Checksum ---
            // This is the default for most cartridge-based systems.
            // case "amstrad cpc":
            // case "apple ii":
            // case "atari 2600":
            // case "atari jaguar":
            // case "wonderswan":
            // case "wonderswan color":
            // case "colecovision":
            // case "channel f":
            // case "vectrex":
            // case "odyssey2":
            // case "intellivision":
            // case "msx":
            // case "msx2":
            // case "pc-8001":
            // case "pc-8801":
            // case "game boy":
            // case "game boy advance":
            // case "game boy color":
            // case "pokemon mini":
            // case "virtual boy":
            // case "neo geo pocket":
            // case "neo geo pocket color":
            // case "32x":
            // case "game gear":
            // case "master system":
            // case "mega drive":
            // case "genesis":
            // case "sg-1000":
            // case "wasm-4":
            // case "supervision":
            // case "mega duck":
            default:
                if (normalizedSystemName.Contains("playstation") || normalizedSystemName.Contains("cd") || normalizedSystemName.Contains("dreamcast") || normalizedSystemName.Contains("saturn"))
                {
                    DebugLogger.Log($"[FileHasher] Hashing for '{systemName}' is complex and not yet implemented.");
                    return null;
                }

                return await CalculateStandardMd5Async(filePath);
        }
    }

    /// <summary>
    /// Calculates the MD5 hash of the entire file.
    /// </summary>
    private static async Task<string> CalculateStandardMd5Async(string filePath)
    {
        return await CalculateMd5WithOffsetAsync(filePath, 0);
    }

    /// <summary>
    /// Calculates the MD5 hash of a file, starting from a specific offset.
    /// </summary>
    private static async Task<string> CalculateMd5WithOffsetAsync(string filePath, long offset)
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
    private static async Task<string> CalculateMd5WithHeaderCheckAsync(string filePath, int headerSize, byte[] magicBytes)
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
    private static string CalculateArcadeHash(string filePath)
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
    private static async Task<string> CalculateN64HashAsync(string filePath)
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
                    // case ".z64":
                    // default:
                    //     break;
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
