using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models.RetroAchievements;

namespace SimpleLauncher.Services.RetroAchievements;

/// <summary>
/// A helper class to execute the external RAHasher.exe tool for generating game file hashes,
/// and to encapsulate various RetroAchievements hashing logic.
/// </summary>
public static class RetroAchievementsHasherTool
{
    private static readonly string HasherPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "RAHasher", "RAHasher.exe");

    private static readonly List<string> SystemWithSimpleHashLogic =
    [
        "amstrad cpc", "apple ii", "atari 2600", "atari jaguar", "wonderswan", "colecovision",
        "vectrex", "magnavox odyssey 2", "intellivision", "msx", "game boy", "game boy advance", "game boy color",
        "pokemon mini", "virtual boy", "neo geo pocket", "32x", "game gear", "master system", "genesis/mega drive",
        "sg-1000", "wasm-4", "watara supervision", "mega duck"
    ];

    private static readonly List<string> SystemWithComplexHashLogic =
    [
        "3do interactive multiplayer", "atari jaguar cd", "pc engine cd/turbografx-cd",
        "pc-fx", "nintendo ds", "neo geo cd", "dreamcast", "saturn", "sega cd",
        "playstation", "playstation 2", "playstation portable"
    ];

    private static readonly List<string> SystemWithUnknowHashLogic =
    [
        "atari 5200", "Arduboy", "nintendo dsi", "wii", "wii u", "nintendo 3ds", "sega pico",
        "atari st", "pc-8000/8800", "commodore 64", "amiga", "zx spectrum", "fairchild channel f",
        "philips cd-i", "sharp x68000", "sharp x1", "oric", "thomson to8", "cassette vision",
        "super cassette vision", "uzebox", "tic-80", "ti-83", "nokia n-gage", "vic-20", "zx81",
        "pc-6000", "game & watch", "elektor tv games computer", "interton vc 4000",
        "arcadia 2001", "fm towns", "hubs", "events", "standalone"
    ];

    private static readonly List<string> SystemWithFileNameHashLogic = ["arcade"];
    private static readonly List<string> SystemWithByteSwappingHashLogic = ["nintendo 64"];

    private static readonly List<string> SystemWithHeaderCheckHashLogic =
    [
        "atari 7800", "atari lynx", "famicom disk system", "nintendo entertainment system", "pc engine/turbografx-16",
        "supergrafx", "super nintendo entertainment system"
    ];

    private static readonly List<string> SystemWithLineEndingNormalizationLogic = ["arduboy"];

    // Add GameCube to its own logic list or handle explicitly
    private static readonly List<string> SystemWithGameCubeLogic = ["gamecube"];

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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"[RAHasher] RAHasher.exe not found at {HasherPath}");
            return null;
        }

        if (!File.Exists(filePath))
        {
            DebugLogger.Log($"[RAHasher] File to hash not found: {filePath}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"[RAHasher] File to hash not found: {filePath}");
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
        using var process = new Process();

        try
        {
            process.StartInfo = processStartInfo;
            process.Start();

            // Read output and error streams asynchronously
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20)); // 20-second timeout
            var outputTask = process.StandardOutput.ReadToEndAsync(cts.Token);
            var errorTask = process.StandardError.ReadToEndAsync(cts.Token);

            // Wait for the process to exit and for streams to be read
            await process.WaitForExitAsync(cts.Token);

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
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"[RAHasher] RAHasher.exe failed for {filePath}. Exit code: {process.ExitCode}. Stderr: {error}");
                return null;
            }

            // This case handles when exit code is 0 but output is empty or unparseable.
            DebugLogger.Log($"[RAHasher] Could not parse a valid hash from RAHasher output, despite exit code 0: {output}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"[RAHasher] Could not parse hash from RAHasher output for {filePath}. Output: {output}");
            return null;
        }
        catch (OperationCanceledException)
        {
            // This means the WaitForExitAsync or ReadToEndAsync timed out
            DebugLogger.Log($"[RAHasher] RAHasher.exe timed out (10s) for '{Path.GetFileName(filePath)}'.");
            if (!process.HasExited)
            {
                try
                {
                    process.Kill(true); // Attempt to kill the hanging process
                }
                catch (Exception killEx)
                {
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(killEx, $"[RAHasher] Failed to kill hanging RAHasher.exe process for '{filePath}'.");
                }
            }

            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"[RAHasher] RAHasher.exe timed out (10s) for {filePath}.");
            return null;
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"[RAHasher] An exception occurred while running RAHasher.exe for {filePath}");
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
        var isExtractionSuccessful = true; // Assume success initially
        string extractionErrorMessage = null;

        if (!File.Exists(filePath))
        {
            DebugLogger.Log($"[RA Hasher Tool] File not found at {filePath}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"[RA Hasher Tool] File not found at {filePath}");
            return new RaHashResult(null, null, false, "Game file not found.");
        }

        if (string.IsNullOrWhiteSpace(systemName))
        {
            DebugLogger.Log("[RA Hasher Tool] SystemName is null or empty.");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, "[RA Hasher Tool] SystemName is null or empty.");
            return new RaHashResult(null, null, false, "System name is missing.");
        }

        var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();

        // --- Determine Hashing Type ---
        string hashCalculationType;
        if (SystemWithSimpleHashLogic.Contains(systemName, StringComparer.OrdinalIgnoreCase))
        {
            hashCalculationType = "Simple";
        }
        else if (SystemWithComplexHashLogic.Contains(systemName, StringComparer.OrdinalIgnoreCase))
        {
            hashCalculationType = "Complex";
        }
        else if (SystemWithGameCubeLogic.Contains(systemName, StringComparer.OrdinalIgnoreCase))
        {
            hashCalculationType = "GameCube";
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
        else if (SystemWithLineEndingNormalizationLogic.Contains(systemName, StringComparer.OrdinalIgnoreCase))
        {
            hashCalculationType = "HashWithLineEndingNormalization";
        }
        else
        {
            DebugLogger.Log($"[RA Hasher Tool] System '{systemName}' is not explicitly supported for RetroAchievements hashing.");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"[RA Hasher Tool] System '{systemName}' is not explicitly supported for RetroAchievements hashing. This is expected for systems in the 'UnknowHashLogic' list.");
            return new RaHashResult(null, null, false, $"System '{systemName}' is not supported for RetroAchievements hashing.");
        }

        // --- Pre-processing: Extract if necessary ---
        var fileToProcess = filePath; // By default, process the original file
        var isCompressed = fileExtension is ".zip" or ".7z" or ".rar";
        var requiresExtraction = hashCalculationType is "Simple" or "Complex" or "HashWithByteSwapping" or "HashWithHeaderCheck" or "HashWithLineEndingNormalization" or "GameCube";

        if (isCompressed && requiresExtraction)
        {
            DebugLogger.Log($"[RA Hasher Tool] Compressed file detected for hashing: {filePath}. Extracting...");
            var extractionService = App.ServiceProvider.GetRequiredService<IExtractionService>();
            var (extractedGameFilePath, extractedTempDirPath) = await extractionService.ExtractToTempAndGetLaunchFileAsync(filePath, fileFormatsToLaunch);
            tempExtractionPath = extractedTempDirPath;

            if (string.IsNullOrEmpty(extractedGameFilePath))
            {
                isExtractionSuccessful = false;
                extractionErrorMessage = $"Failed to extract or find a suitable file in archive for hashing: {filePath}.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"[RA Hasher Tool] {extractionErrorMessage}");
                DebugLogger.Log($"[RA Hasher Tool] {extractionErrorMessage}");
                return new RaHashResult(null, tempExtractionPath, isExtractionSuccessful, extractionErrorMessage);
            }

            fileToProcess = extractedGameFilePath;
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
                        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"[RA Hasher Tool] Could not find system ID for '{systemName}'. Cannot use RAHasher.exe.");
                        isExtractionSuccessful = false; // Treat as a hashing failure
                        extractionErrorMessage = $"Could not find RetroAchievements System ID for '{systemName}'.";
                    }

                    break;
                }

                case "GameCube":
                {
                    var systemId = RetroAchievementsSystemMatcher.GetSystemId(systemName);
                    if (systemId <= 0)
                    {
                        extractionErrorMessage = $"Could not find RetroAchievements System ID for '{systemName}'.";
                        isExtractionSuccessful = false;
                        break;
                    }

                    string tempIsoPath = null;
                    try
                    {
                        // Handle RVZ conversion if necessary
                        if (Path.GetExtension(fileToProcess).Equals(".rvz", StringComparison.OrdinalIgnoreCase))
                        {
                            DebugLogger.Log($"[RA Hasher Tool] RVZ detected. Converting to ISO for hashing: {fileToProcess}");
                            tempIsoPath = await ConvertRvzToIsoAsync(fileToProcess);
                            if (!string.IsNullOrEmpty(tempIsoPath))
                            {
                                fileToProcess = tempIsoPath;
                            }
                            else
                            {
                                extractionErrorMessage = "Failed to convert RVZ to ISO.";
                                isExtractionSuccessful = false;
                            }
                        }

                        if (isExtractionSuccessful)
                        {
                            DebugLogger.Log($"[RA Hasher Tool] Using RAHasher.exe for GameCube (ID: {systemId}) on '{Path.GetFileName(fileToProcess)}'...");
                            hash = await GetHashAsync(fileToProcess, systemId);
                            DebugLogger.Log($"[RA Hasher Tool] RAHasher result: {hash}");
                        }
                    }
                    finally
                    {
                        // Cleanup temp ISO if we created one
                        if (!string.IsNullOrEmpty(tempIsoPath) && File.Exists(tempIsoPath))
                        {
                            try
                            {
                                File.Delete(tempIsoPath);
                            }
                            catch
                            {
                                /* ignore */
                            }
                        }
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
                    DebugLogger.Log($"[RA Hasher Tool] Calculating N64 hash for '{Path.GetFileName(fileToProcess)}'...");
                    hash = await RetroAchievementsFileHasher.CalculateN64HashAsync(fileToProcess);
                    DebugLogger.Log($"[RA Hasher Tool] Calculated N64 hash: {hash}");
                    break;
                }

                case "HashWithHeaderCheck":
                {
                    DebugLogger.Log($"[RA Hasher Tool] Calculating header-based hash for system '{systemName}' on file '{Path.GetFileName(fileToProcess)}'...");
                    hash = await RetroAchievementsFileHasher.CalculateHeaderBasedMd5Async(fileToProcess, systemName);
                    DebugLogger.Log($"[RA Hasher Tool] Calculated header-based hash: {hash}");
                    break;
                }
                case "HashWithLineEndingNormalization":
                {
                    DebugLogger.Log($"[RA Hasher Tool] Calculating Arduboy hash for '{Path.GetFileName(fileToProcess)}'...");
                    hash = await RetroAchievementsFileHasher.CalculateArduboyHashAsync(fileToProcess);
                    DebugLogger.Log($"[RA Hasher Tool] Calculated Arduboy hash: {hash}");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"[RA Hasher Tool] An error occurred during hash calculation for {filePath} (System: {systemName}).");
            DebugLogger.Log($"[RA Hasher Tool] An error occurred during hash calculation for {filePath} (System: {systemName}).");
            return new RaHashResult(null, tempExtractionPath, false, $"Error during hash calculation: {ex.Message}"); // Return null hash, but keep temp path for cleanup
        }

        return new RaHashResult(hash, tempExtractionPath, isExtractionSuccessful, extractionErrorMessage);
    }

    /// <summary>
    /// Converts an RVZ file to a temporary ISO using DolphinTool.exe.
    /// </summary>
    private static async Task<string> ConvertRvzToIsoAsync(string rvzPath)
    {
        try
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            var exeName = arch == Architecture.Arm64 ? "DolphinTool_arm64.exe" : "DolphinTool.exe";
            var dolphinToolPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tools", "BatchConvertToRVZ", exeName);

            if (!File.Exists(dolphinToolPath))
            {
                DebugLogger.Log($"[RA Hasher Tool] DolphinTool not found at {dolphinToolPath}. Cannot convert RVZ.");
                return null;
            }

            var dolphinDir = Path.GetDirectoryName(dolphinToolPath);

            var tempIsoPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.iso");

            // Arguments: convert --format=iso --input="in.rvz" --output="out.iso"
            var args = $"convert --format=iso --input=\"{rvzPath}\" --output=\"{tempIsoPath}\"";

            var processStartInfo = new ProcessStartInfo
            {
                FileName = dolphinToolPath,
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = dolphinDir
            };

            using var process = new Process();
            process.StartInfo = processStartInfo;
            process.Start();

            await process.WaitForExitAsync();

            if (process.ExitCode == 0 && File.Exists(tempIsoPath))
            {
                return tempIsoPath;
            }

            var error = await process.StandardError.ReadToEndAsync();
            DebugLogger.Log($"[RA Hasher Tool] DolphinTool failed. ExitCode: {process.ExitCode}. Error: {error}");
            return null;
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "[RA Hasher Tool] Error converting RVZ to ISO.");
            return null;
        }
    }
}