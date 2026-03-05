using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.Converters;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.ExtractFiles;
using SimpleLauncher.Services.LoadingInterface;
using SimpleLauncher.Services.RetroAchievements.Models;

namespace SimpleLauncher.Services.RetroAchievements;

/// <summary>
/// A helper class to execute the external RAHasher.exe tool for generating game file hashes,
/// and to encapsulate various RetroAchievements hashing logic.
/// </summary>
internal static class RetroAchievementsHasherTool
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
        "pc-fx", "nintendo ds", "nintendo dsi", "neo geo cd", "dreamcast", "saturn", "sega cd",
        "playstation", "playstation 2", "playstation portable"
    ];

    // Systems Not Supported or with UnknowHashLogic
    // These systems will not show the RetroAchievements icon and hashing will be skipped
    private static readonly List<string> SystemWithUnknowHashLogic =
    [
        "atari 5200", "Arduboy", "wii", "wii u", "nintendo 3ds", "sega pico",
        "atari st", "pc-8000/8800", "commodore 64", "amiga", "zx spectrum", "fairchild channel f",
        "philips cd-i", "sharp x68000", "sharp x1", "oric", "thomson to8", "cassette vision",
        "super cassette vision", "uzebox", "tic-80", "ti-83", "nokia n-gage", "vic-20", "zx81",
        "pc-6000", "game & watch", "elektor tv games computer", "interton vc 4000",
        "arcadia 2001", "fm towns", "hubs", "events", "standalone", "atari 800", "microsoft windows",
        "playstation 3", "ps3", "sony playstation 3", "xbox 360", "xbox one", "xbox series x", "xbox series s",
        "nintendo switch", "sega model 2", "sega model 3", "sega naomi", "sega naomi 2", "atomiswave",
        "odyssey", "odyssey2"
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
    /// Checks if a system is supported for RetroAchievements hashing.
    /// This is used to determine whether to show the RA icon and attempt hashing.
    /// Handles name variations by checking against known aliases and using fuzzy matching.
    /// </summary>
    /// <param name="systemName">The system name to check.</param>
    /// <returns>True if the system is supported for RetroAchievements hashing; otherwise, false.</returns>
    public static bool IsSystemSupportedForHashing(string systemName)
    {
        if (string.IsNullOrWhiteSpace(systemName))
            return false;

        var normalizedInput = systemName.Trim().ToLowerInvariant();

        // First, check if the input directly matches any unsupported system (including aliases)
        // This is important to catch variations like "PS3", "Sony PS3", etc.
        foreach (var unsupportedSystem in SystemWithUnknowHashLogic)
        {
            if (IsSystemNameMatch(normalizedInput, unsupportedSystem))
                return false;
        }

        // Get the best match from the system mappings (this handles fuzzy matching for supported systems)
        var matchedSystemName = RetroAchievementsSystemMatcher.GetBestMatchSystemName(systemName);

        // Check if the matched system is in the unsupported list
        if (SystemWithUnknowHashLogic.Contains(matchedSystemName, StringComparer.OrdinalIgnoreCase))
            return false;

        // Check if the system exists in the SystemMappings dictionary
        // This ensures all systems defined in RetroAchievementsSystemMatcher are considered supported
        if (RetroAchievementsSystemMatcher.IsSystemInMappings(systemName))
            return true;

        // Check if the matched system is in any of the supported lists
        return SystemWithSimpleHashLogic.Contains(matchedSystemName, StringComparer.OrdinalIgnoreCase) ||
               SystemWithComplexHashLogic.Contains(matchedSystemName, StringComparer.OrdinalIgnoreCase) ||
               SystemWithFileNameHashLogic.Contains(matchedSystemName, StringComparer.OrdinalIgnoreCase) ||
               SystemWithByteSwappingHashLogic.Contains(matchedSystemName, StringComparer.OrdinalIgnoreCase) ||
               SystemWithHeaderCheckHashLogic.Contains(matchedSystemName, StringComparer.OrdinalIgnoreCase) ||
               SystemWithLineEndingNormalizationLogic.Contains(matchedSystemName, StringComparer.OrdinalIgnoreCase) ||
               SystemWithGameCubeLogic.Contains(matchedSystemName, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks if two system names match, considering various naming conventions and variations.
    /// </summary>
    private static bool IsSystemNameMatch(string input, string pattern)
    {
        // Direct match
        if (input.Equals(pattern, StringComparison.OrdinalIgnoreCase))
            return true;

        // Check if input contains the pattern or vice versa
        if (input.Contains(pattern, StringComparison.OrdinalIgnoreCase) ||
            pattern.Contains(input, StringComparison.OrdinalIgnoreCase))
            return true;

        // Remove common separators and normalize
        var cleanInput = NormalizeSystemName(input);
        var cleanPattern = NormalizeSystemName(pattern);

        if (cleanInput.Equals(cleanPattern, StringComparison.OrdinalIgnoreCase))
            return true;

        if (cleanInput.Contains(cleanPattern, StringComparison.OrdinalIgnoreCase) ||
            cleanPattern.Contains(cleanInput, StringComparison.OrdinalIgnoreCase))
            return true;

        // Check for common abbreviations and variations
        return AreSystemAbbreviationsEquivalent(input, pattern);
    }

    /// <summary>
    /// Normalizes a system name by removing common separators and standardizing format.
    /// </summary>
    private static string NormalizeSystemName(string input)
    {
        return input
            .Replace("-", "")
            .Replace("/", "")
            .Replace("&", "")
            .Replace(" ", "")
            .Replace(".", "")
            .Replace("'", "")
            .Replace("™", "")
            .Replace("®", "")
            .ToLowerInvariant();
    }

    /// <summary>
    /// Checks if two system names are equivalent based on common abbreviations and naming conventions.
    /// </summary>
    private static bool AreSystemAbbreviationsEquivalent(string input, string pattern)
    {
        var normalizedInput = NormalizeSystemName(input);
        var normalizedPattern = NormalizeSystemName(pattern);

        // Common Sony variations
        if (normalizedInput.Contains("ps3") || normalizedInput.Contains("playstation3"))
            return normalizedPattern.Contains("ps3") || normalizedPattern.Contains("playstation3");
        if (normalizedInput.Contains("ps2") || normalizedInput.Contains("playstation2"))
            return normalizedPattern.Contains("ps2") || normalizedPattern.Contains("playstation2");
        if (normalizedInput.Contains("ps1") || normalizedInput.Contains("playstation1") || normalizedInput.Contains("psx"))
            return normalizedPattern.Contains("ps1") || normalizedPattern.Contains("playstation1") || normalizedPattern.Contains("psx");
        if (normalizedInput.Contains("psp") || normalizedInput.Contains("playstationportable"))
            return normalizedPattern.Contains("psp") || normalizedPattern.Contains("playstationportable");

        // Common Nintendo variations
        if (normalizedInput.Contains("nes") || normalizedInput.Contains("nintendoentertainmentsystem"))
            return normalizedPattern.Contains("nes") || normalizedPattern.Contains("nintendoentertainmentsystem");
        if (normalizedInput.Contains("snes") || normalizedInput.Contains("supernintendo"))
            return normalizedPattern.Contains("snes") || normalizedPattern.Contains("supernintendo");
        if (normalizedInput.Contains("n64") || normalizedInput.Contains("nintendo64"))
            return normalizedPattern.Contains("n64") || normalizedPattern.Contains("nintendo64");
        if (normalizedInput.Contains("gc") || normalizedInput.Contains("gamecube"))
            return normalizedPattern.Contains("gc") || normalizedPattern.Contains("gamecube");
        if (normalizedInput.Contains("gb") || normalizedInput.Contains("gameboy"))
            return normalizedPattern.Contains("gb") || normalizedPattern.Contains("gameboy");
        if (normalizedInput.Contains("gba") || normalizedInput.Contains("gameboyadvance"))
            return normalizedPattern.Contains("gba") || normalizedPattern.Contains("gameboyadvance");
        if (normalizedInput.Contains("gbc") || normalizedInput.Contains("gameboycolor"))
            return normalizedPattern.Contains("gbc") || normalizedPattern.Contains("gameboycolor");
        if (normalizedInput.Contains("nds") || normalizedInput.Contains("nintendods"))
            return normalizedPattern.Contains("nds") || normalizedPattern.Contains("nintendods");
        if (normalizedInput.Contains("3ds") || normalizedInput.Contains("nintendo3ds"))
            return normalizedPattern.Contains("3ds") || normalizedPattern.Contains("nintendo3ds");
        if (normalizedInput.Contains("wiiu"))
            return normalizedPattern.Contains("wiiu");
        if (normalizedInput.Contains("switch") || normalizedInput.Contains("nintendoswitch"))
            return normalizedPattern.Contains("switch") || normalizedPattern.Contains("nintendoswitch");

        // Common Sega variations
        if (normalizedInput.Contains("genesis") || normalizedInput.Contains("megadrive") || normalizedInput.Contains("segagenesis"))
            return normalizedPattern.Contains("genesis") || normalizedPattern.Contains("megadrive") || normalizedPattern.Contains("segagenesis");
        if (normalizedInput.Contains("sms") || normalizedInput.Contains("mastersystem") || normalizedInput.Contains("segamastersystem"))
            return normalizedPattern.Contains("sms") || normalizedPattern.Contains("mastersystem") || normalizedPattern.Contains("segamastersystem");
        if (normalizedInput.Contains("gg") || normalizedInput.Contains("gamegear") || normalizedInput.Contains("segagamegear"))
            return normalizedPattern.Contains("gg") || normalizedPattern.Contains("gamegear") || normalizedPattern.Contains("segagamegear");
        if (normalizedInput.Contains("saturn") || normalizedInput.Contains("segasaturn"))
            return normalizedPattern.Contains("saturn") || normalizedPattern.Contains("segasaturn");
        if (normalizedInput.Contains("dreamcast") || normalizedInput.Contains("segadreamcast"))
            return normalizedPattern.Contains("dreamcast") || normalizedPattern.Contains("segadreamcast");

        // Common Microsoft variations
        if (normalizedInput.Contains("xbox360") || normalizedInput.Contains("xbox 360") || normalizedInput.Contains("xb360"))
            return normalizedPattern.Contains("xbox360") || normalizedPattern.Contains("xbox 360") || normalizedPattern.Contains("xb360");
        if (normalizedInput.Contains("xboxone") || normalizedInput.Contains("xbox one") || normalizedInput.Contains("xbone"))
            return normalizedPattern.Contains("xboxone") || normalizedPattern.Contains("xbox one") || normalizedPattern.Contains("xbone");

        // Common arcade variations
        if (normalizedInput.Contains("mame") || normalizedInput.Contains("arcade"))
            return normalizedPattern.Contains("mame") || normalizedPattern.Contains("arcade");
        if (normalizedInput.Contains("neogeo") || normalizedInput.Contains("neo geo"))
            return normalizedPattern.Contains("neogeo") || normalizedPattern.Contains("neo geo");

        return false;
    }

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
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60)); // 60-second timeout
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

    public static async Task<RaHashResult> GetGameHashForRetroAchievementsAsync(string filePath, string systemName, List<string> fileFormatsToLaunch, ILoadingState loadingState)
    {
        // 1. Try to get a 100% certain match
        var confirmedSystem = RetroAchievementsSystemMatcher.GetExactAliasMatch(systemName);

        // 2. If not 100% certain, ask the user
        if (confirmedSystem == null)
        {
            // Get a "guess" to pre-select in the ComboBox
            DebugLogger.Log($"[GetGameHashForRetroAchievementsAsync] Received systemName: {systemName}");
            var guess = RetroAchievementsSystemMatcher.GetBestMatchSystemName(systemName);
            DebugLogger.Log($"[GetGameHashForRetroAchievementsAsync] Guess systemName: {guess}");

            // Run UI on Dispatcher
            var userSelectedSystem = await Application.Current.Dispatcher.InvokeAsync(() =>
            {
                var win = new SystemSelectionWindow(guess) { Owner = Application.Current.MainWindow };
                return win.ShowDialog() == true ? win.SelectedSystem : null;
            });
            DebugLogger.Log($"[GetGameHashForRetroAchievementsAsync] UserSelectedSystem: {userSelectedSystem}");

            if (string.IsNullOrEmpty(userSelectedSystem))
            {
                DebugLogger.Log("[GetGameHashForRetroAchievementsAsync] User did not choose a system. Returning null.");
                return new RaHashResult(null, null, false, "System selection cancelled by user.");
            }

            systemName = userSelectedSystem;
        }
        else
        {
            systemName = confirmedSystem;
        }

        string tempExtractionPath = null;
        string hash = null;
        var isExtractionSuccessful = true; // Assume success initially
        string extractionErrorMessage = null;

        // Report loading state if provided
        loadingState?.SetLoadingState(true, "Calculating game hash...");

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
                        // Use fileToProcess (the extracted file) instead of filePath (the zip)
                        hash = await GetHashAsync(fileToProcess, systemId);
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
                            tempIsoPath = await ConvertRvzToIso.ConvertRvzToIsoAsync(fileToProcess);
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
            return new RaHashResult(null, tempExtractionPath, false, $"Error during hash calculation: {ex.Message}");
        }
        finally
        {
            loadingState?.SetLoadingState(false);
        }

        return new RaHashResult(hash, tempExtractionPath, isExtractionSuccessful, extractionErrorMessage);
    }
}