using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleLauncher.Services;

/// <summary>
/// Represents the result of a RetroAchievements hash calculation, including the hash and any temporary extraction path.
/// </summary>
public struct RaHashResult
{
    public string Hash { get; }
    public string TempExtractionPath { get; }

    public RaHashResult(string hash, string tempExtractionPath)
    {
        Hash = hash;
        TempExtractionPath = tempExtractionPath;
    }
}

/// <summary>
/// A helper class to execute the external RAHasher.exe tool for generating game file hashes,
/// and to encapsulate various RetroAchievements hashing logic.
/// </summary>
public static class RetroAchievementsHasherTool
{
    private static readonly string HasherPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "RAHasher", "RAHasher.exe");

    // Define system categories for hashing logic
    private static readonly List<string> SystemWithSimpleHashLogic =
    [
        "amstrad cpc", "apple ii", "atari 2600", "atari jaguar", "wonderswan", "colecovision",
        "vectrex", "magnavox odyssey 2", "intellivision", "msx", "game boy", "game boy advance", "game boy color",
        "pokemon mini", "virtual boy", "neo geo pocket", "32x", "game gear", "master system", "genesis/mega drive",
        "sg-1000", "wasm-4", "watara supervision", "mega duck"
    ];

    private static readonly List<string> SystemWithComplexOrUnknowHashLogic =
    [
        "3do interactive multiplayer", "arduboy", "atari jaguar cd", "pc engine cd/turbografx-cd",
        "pc-fx", "gamecube", "nintendo ds", "neo geo cd", "dreamcast", "saturn", "sega cd",
        "playstation", "playstation 2", "playstation portable", "Arudboy", "nintendo dsi", "atari 5200",
        "wii", "wii u", "nintendo 3ds", "sega pico", "atari st", "pc-8000/8800", "commodore 64", "amiga",
        "zx spectrum", "fairchild channel f", "philips cd-i", "sharp x68000", "sharp x1", "oric",
        "thomson to8", "cassette vision", "super cassette vision", "uzebox", "tic-80", "ti-83",
        "nokia n-gage", "vic-20", "zx81", "pc-6000", "game & watch", "elektor tv games computer",
        "interton vc 4000", "arcadia 2001", "fm towns", "hubs", "events", "standalone"
    ];

    private static readonly List<string> SystemWithFileNameHashLogic = ["arcade"];
    private static readonly List<string> SystemWithByteSwappingHashLogic = ["nintendo 64"];

    private static readonly List<string> SystemWithHeaderCheckHashLogic =
    [
        "atari 7800", "atari lynx", "famicom disk system", "nintendo entertainment system", "pc engine/turbografx-16",
        "supergrafx", "super nintendo entertainment system"
    ];

    /// <summary>
    /// Gets the hash for a given file using the external RAHasher.exe tool.
    /// </summary>
    /// <param name="filePath">The full path to the game file to be hashed.</param>
    /// <param name="systemId">The RetroAchievements console ID.</param>
    /// <returns>The calculated hash as a string, or null if an error occurs.</returns>
    private static async Task<string> GetHashAsync(string filePath, int systemId)
    {
        if (!File.Exists(HasherPath))
        {
            DebugLogger.Log($"[RAHasher] RAHasher.exe not found at {HasherPath}");
            // Removed MessageBoxLibrary.RaHasherNotFoundMessageBox(); as per refactoring plan.
            _ = LogErrors.LogErrorAsync(null, $"[RAHasher] RAHasher.exe not found at {HasherPath}");
            return null;
        }

        if (!File.Exists(filePath))
        {
            DebugLogger.Log($"[RAHasher] File to hash not found: {filePath}");
            _ = LogErrors.LogErrorAsync(null, $"[RAHasher] File to hash not found: {filePath}");
            return null;
        }

        var processStartInfo = new ProcessStartInfo
        {
            FileName = HasherPath,
            Arguments = $"{systemId} \"{filePath}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(HasherPath) ?? string.Empty
        };

        try
        {
            using var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();

            // Read output and error streams asynchronously
            var outputTask = process.StandardOutput.ReadToEndAsync();
            var errorTask = process.StandardError.ReadToEndAsync();

            // Wait for the process to exit and for streams to be read
            await process.WaitForExitAsync();
            var output = await outputTask;
            var error = await errorTask;

            // Prioritize parsing the output for a hash, as RAHasher might return a non-zero
            // exit code even when it successfully produces a hash.
            var lines = output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                var potentialHash = line.Trim();
                if (potentialHash.Length == 32 && potentialHash.All(static c => "0123456789abcdefABCDEF".Contains(c)))
                {
                    DebugLogger.Log($"[RAHasher] Successfully parsed hash '{potentialHash}' for '{Path.GetFileName(filePath)}' (System ID: {systemId}). Exit code was {process.ExitCode}.");
                    return potentialHash.ToLowerInvariant();
                }
            }

            // If no hash was found in the output, then a non-zero exit code is a genuine error.
            if (process.ExitCode != 0)
            {
                DebugLogger.Log($"[RAHasher] Error executing RAHasher.exe. No hash found in output. Exit code: {process.ExitCode}");
                DebugLogger.Log($"[RAHasher] Stderr: {error}");
                DebugLogger.Log($"[RAHasher] Stdout: {output}");
                _ = LogErrors.LogErrorAsync(null, $"[RAHasher] RAHasher.exe failed for {filePath}. Exit code: {process.ExitCode}. Stderr: {error}");
                return null;
            }

            // This case handles when exit code is 0 but output is empty or unparseable.
            DebugLogger.Log($"[RAHasher] Could not parse a valid hash from RAHasher output, despite exit code 0: {output}");
            _ = LogErrors.LogErrorAsync(null, $"[RAHasher] Could not parse hash from RAHasher output for {filePath}. Output: {output}");
            return null;
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"[RAHasher] An exception occurred while running RAHasher.exe for {filePath}");
            return null;
        }
    }

    /// <summary>
    /// Calculates the RetroAchievements hash for a given game file based on its system and file type.
    /// Handles extraction of compressed files and delegates to appropriate hashing methods.
    /// </summary>
    /// <param name="filePath">The full path to the game file.</param>
    /// <param name="systemName">The RetroAchievements-normalized system name.</param>
    /// <param name="fileFormatsToLaunch">A list of file extensions that can be launched for this system, used for extraction.</param>
    /// <returns>A <see cref="RaHashResult"/> containing the calculated hash and the path to any temporary extraction directory, or null if hashing fails or the system is not supported.</returns>
    [SuppressMessage("ReSharper", "RedundantEmptySwitchSection")]
    public static async Task<RaHashResult> GetGameHashForRetroAchievementsAsync(string filePath, string systemName, List<string> fileFormatsToLaunch)
    {
        string tempExtractionPath = null;
        string hash = null;

        if (!File.Exists(filePath))
        {
            DebugLogger.Log($"[RA Hasher Tool] File not found at {filePath}");
            _ = LogErrors.LogErrorAsync(null, $"[RA Hasher Tool] File not found at {filePath}");
            return new RaHashResult(null, null); // Return null hash
        }

        if (string.IsNullOrWhiteSpace(systemName))
        {
            DebugLogger.Log("[RA Hasher Tool] SystemName is null or empty.");
            _ = LogErrors.LogErrorAsync(null, "[RA Hasher Tool] SystemName is null or empty.");
            return new RaHashResult(null, null); // Return null hash
        }

        var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();

        // --- Determine Hashing Type ---
        string hashCalculationType;
        if (SystemWithSimpleHashLogic.Contains(systemName, StringComparer.OrdinalIgnoreCase))
        {
            hashCalculationType = "Simple";
        }
        else if (SystemWithComplexOrUnknowHashLogic.Contains(systemName, StringComparer.OrdinalIgnoreCase))
        {
            hashCalculationType = "Complex";
        }
        else if (SystemWithFileNameHashLogic.Contains(systemName, StringComparer.OrdinalIgnoreCase))
        {
            hashCalculationType = "HashFileName";
        }
        else if (SystemWithByteSwappingHashLogic.Contains(systemName, StringComparer.OrdinalIgnoreCase))
        {
            hashCalculationType = "HashWithByteSwapping";
        }
        else if (SystemWithHeaderCheckHashLogic.Contains(systemName, StringComparer.OrdinalIgnoreCase))
        {
            hashCalculationType = "HashWithHeaderCheck";
        }
        else
        {
            DebugLogger.Log($"[RA Hasher Tool] System '{systemName}' is not explicitly supported for RetroAchievements hashing.");
            _ = LogErrors.LogErrorAsync(null, $"[RA Hasher Tool] System '{systemName}' is not explicitly supported for RetroAchievements hashing.");
            return new RaHashResult(null, null); // Return null hash
        }

        // --- Pre-processing: Extract if necessary ---
        var fileToProcess = filePath; // By default, process the original file
        var isCompressed = fileExtension is ".zip" or ".7z" or ".rar";
        var requiresExtraction = hashCalculationType is "Simple" or "HashWithByteSwapping" or "HashWithHeaderCheck";

        if (isCompressed && requiresExtraction)
        {
            DebugLogger.Log($"[RA Hasher Tool] Compressed file detected for hashing: {filePath}. Extracting...");
            var extractor = new ExtractCompressedFile();
            tempExtractionPath = await extractor.ExtractWithSevenZipSharpToTempAsync(filePath);

            if (string.IsNullOrEmpty(tempExtractionPath))
            {
                _ = LogErrors.LogErrorAsync(null, $"[RA Hasher Tool] Failed to extract archive for hashing: {filePath}");
                DebugLogger.Log($"[RA Hasher Tool] Failed to extract archive for hashing: {filePath}");
                return new RaHashResult(null, null); // Return null hash
            }

            string foundRomFile = null;
            if (fileFormatsToLaunch is { Count: > 0 })
            {
                foreach (var format in fileFormatsToLaunch)
                {
                    var searchPattern = $"*.{format.TrimStart('.')}";
                    var files = Directory.GetFiles(tempExtractionPath, searchPattern, SearchOption.AllDirectories);
                    if (files.Length > 0)
                    {
                        foundRomFile = files[0];
                        DebugLogger.Log($"[RA Hasher Tool] Found file to hash after extraction: {foundRomFile}");
                        break;
                    }
                }
            }

            if (string.IsNullOrEmpty(foundRomFile))
            {
                var allExtractedFiles = Directory.GetFiles(tempExtractionPath, "*", SearchOption.AllDirectories);
                if (allExtractedFiles.Length > 0)
                {
                    foundRomFile = allExtractedFiles[0];
                    DebugLogger.Log($"[RA Hasher Tool] No specific launch format file found. Picking first extracted file: {foundRomFile}");
                }
            }

            if (string.IsNullOrEmpty(foundRomFile))
            {
                DebugLogger.Log($"[RA Hasher Tool] Could not find any suitable file to hash after extracting {filePath}.");
                _ = LogErrors.LogErrorAsync(null, $"[RA Hasher Tool] Could not find any suitable file to hash after extracting {filePath}.");
                return new RaHashResult(null, tempExtractionPath); // Return null hash, but keep temp path for cleanup
            }

            fileToProcess = foundRomFile; // Update the path to the extracted file
        }

        // --- Perform Hashing ---
        try
        {
            switch (hashCalculationType)
            {
                case "Simple":
                {
                    hash = await RetroAchievementsFileHasher.CalculateStandardMd5Async(fileToProcess);
                    DebugLogger.Log($"[RA Hasher Tool] Calculated simple hash: {hash}");
                    break;
                }

                case "Complex":
                {
                    var systemId = RetroAchievementsSystemMatcher.GetSystemId(systemName);
                    if (systemId > 0)
                    {
                        DebugLogger.Log($"[RA Hasher Tool] Using RAHasher.exe for system '{systemName}' (ID: {systemId})...");
                        // For complex/disc-based systems, the original file path (even if compressed) is passed to the tool.
                        // The tool handles reading from archives/disc images internally.
                        hash = await GetHashAsync(filePath, systemId); // Call the existing GetHashAsync
                    }
                    else
                    {
                        DebugLogger.Log($"[RA Hasher Tool] Could not find system ID for '{systemName}'. Cannot use RAHasher.exe.");
                        _ = LogErrors.LogErrorAsync(null, $"[RA Hasher Tool] Could not find system ID for '{systemName}'. Cannot use RAHasher.exe.");
                    }

                    break;
                }

                case "HashFileName":
                {
                    hash = RetroAchievementsFileHasher.CalculateFilenameHash(fileToProcess);
                    DebugLogger.Log($"[RA Hasher Tool] Calculated hash for filename: {hash}");
                    break;
                }

                case "HashWithByteSwapping":
                {
                    // Use the external RAHasher.exe
                    var systemId = RetroAchievementsSystemMatcher.GetSystemId(systemName);
                    if (systemId > 0)
                    {
                        DebugLogger.Log($"[RA Hasher Tool] Using RAHasher.exe for system '{systemName}' (ID: {systemId})...");
                        hash = await GetHashAsync(fileToProcess, systemId); // Call the existing GetHashAsync
                    }
                    else
                    {
                        DebugLogger.Log($"[RA Hasher Tool] Could not find system ID for '{systemName}'. Cannot use RAHasher.exe.");
                        _ = LogErrors.LogErrorAsync(null, $"[RA Hasher Tool] Could not find system ID for '{systemName}'. Cannot use RAHasher.exe.");
                    }

                    break;
                }

                case "HashWithHeaderCheck":
                {
                    DebugLogger.Log($"[RA Hasher Tool] Calculating header-based hash for system '{systemName}' on file '{Path.GetFileName(fileToProcess)}'...");
                    hash = await RetroAchievementsFileHasher.CalculateHeaderBasedMd5Async(fileToProcess, systemName);
                    DebugLogger.Log($"[RA Hasher Tool] Calculated header-based hash: {hash}");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"[RA Hasher Tool] An error occurred during hash calculation for {filePath} (System: {systemName}).");
            DebugLogger.Log($"[RA Hasher Tool] An error occurred during hash calculation for {filePath} (System: {systemName}).");
            return new RaHashResult(null, tempExtractionPath); // Return null hash, but keep temp path for cleanup
        }

        return new RaHashResult(hash, tempExtractionPath);
    }
}