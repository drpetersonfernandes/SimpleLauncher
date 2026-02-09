using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameScan.Models;

namespace SimpleLauncher.Services.GameScan;

public class GameScannerService
{
    private readonly ILogErrors _logErrors;
    private readonly IConfiguration _configuration;
    private const string WindowsSystemName = "Microsoft Windows";

    internal static readonly HashSet<string> IgnoredGameNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "Steamworks Common Redistributables",
        "Unreal Engine",
        "Fab UE Plugin",
        "Quixel Bridge",
        "DirectX",
        "Google Earth VR",
        "Spacewar",
        "PC Health Check",
        "Rockstar Games Launcher",
        "Battle.net",
        "Ubisoft Connect"
    };

    private readonly string _windowsRomsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "roms", "Microsoft Windows");
    private readonly string _windowsImagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "Microsoft Windows");

    internal bool WasNewSystemCreated { get; private set; }

    public GameScannerService(ILogErrors logErrors)
    {
        _logErrors = logErrors;
        _configuration = App.ServiceProvider.GetRequiredService<IConfiguration>();
    }

    internal async Task ScanForStoreGamesAsync()
    {
        try
        {
            WasNewSystemCreated = await EnsureWindowsSystemExistsAsync();

            var tasks = new List<Task>
            {
                ScanSteamGames.ScanSteamGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanEpicGames.ScanEpicGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanAmazonGames.ScanAmazonGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanBattleNetGames.ScanBattleNetGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanGogGames.ScanGogGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanHumbleGames.ScanHumbleGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanItchioGames.ScanItchioGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanRockstarGames.ScanRockstarGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanUplayGames.ScanUplayGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanEaGames.ScanEaGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanMicrosoftStoreGames.ScanMicrosoftStoreGamesAsync(_logErrors, _windowsRomsPath, _windowsImagesPath)
            };

            await Task.WhenAll(tasks);

            DebugLogger.Log("[GameScannerService] All store game scans completed.");
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "An error occurred during the game scanning process.");
        }
    }

    private async Task<bool> EnsureWindowsSystemExistsAsync()
    {
        try
        {
            // Fix 1: Use SystemManager.SystemManager for the static method
            if (SystemManager.SystemManager.SystemExists(WindowsSystemName, _configuration))
            {
                return false;
            }

            DebugLogger.Log($"[GameScannerService] '{WindowsSystemName}' system not found. Creating it now.");

            var windowsSystem = new SystemManager.SystemManager
            {
                SystemName = WindowsSystemName,
                SystemFolders = ["%BASEFOLDER%\\roms\\Microsoft Windows"],
                SystemImageFolder = "%BASEFOLDER%\\images\\Microsoft Windows",
                SystemIsMame = false,
                FileFormatsToSearch = ["url", "lnk", "bat"],
                GroupByFolder = false,
                ExtractFileBeforeLaunch = false,
                FileFormatsToLaunch = [],
                Emulators =
                [
                    // Fix 2: Use SystemManager.SystemManager.Emulator for the nested class
                    new SystemManager.SystemManager.Emulator
                    {
                        EmulatorName = "Direct Launch",
                        EmulatorLocation = "",
                        EmulatorParameters = "",
                        ReceiveANotificationOnEmulatorError = true
                    }
                ]
            };

            // Fix 3: Use SystemManager.SystemManager for the static method
            await SystemManager.SystemManager.SaveSystemConfigurationAsync(windowsSystem);

            // Create the necessary directories
            Directory.CreateDirectory(_windowsRomsPath);
            Directory.CreateDirectory(_windowsImagesPath);

            return true;
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "Failed to create 'Microsoft Windows' system in system.xml.");
            return false;
        }
    }

    internal static async Task<bool> TryDownloadImageFromApiAsync(string gameName, string destinationPath, ILogErrors logErrors)
    {
        if (string.IsNullOrWhiteSpace(gameName)) return false;

        try
        {
            var httpClientFactory = App.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            using var client = httpClientFactory.CreateClient("GameImageClient");

            var encodedGameName = System.Net.WebUtility.UrlEncode(gameName);
            var response = await client.GetAsync($"api/v1/games/search?name={encodedGameName}");

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    DebugLogger.Log($"[GameScannerService] API query for '{gameName}' failed with status: {response.StatusCode}");
                }

                return false;
            }

            await using var jsonStream = await response.Content.ReadAsStreamAsync();
            var apiResponse = await JsonSerializer.DeserializeAsync<GameImageApiResponse>(jsonStream);

            if (apiResponse is { Success: true, ImageUrl: not null } && Uri.IsWellFormedUriString(apiResponse.ImageUrl, UriKind.Absolute))
            {
                // HttpClient supports absolute URLs directly, even when BaseAddress is configured
                var imageBytes = await client.GetByteArrayAsync(apiResponse.ImageUrl);
                await File.WriteAllBytesAsync(destinationPath, imageBytes);
                DebugLogger.Log($"[GameScannerService] Successfully downloaded image for '{gameName}' from API.");
                return true;
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, $"Failed to fetch or download image for '{gameName}' from API.");
        }

        return false;
    }

    internal static async Task FindAndSaveGameImageAsync(ILogErrors logErrors, string originalGameName, string gameInstallPath, string sanitizedGameName, string windowsImagesPath, string specificExePath = null)
    {
        try
        {
            var imagePath = Path.Combine(windowsImagesPath, $"{sanitizedGameName}.png");
            if (File.Exists(imagePath)) return;

            // 1. Try to download from API
            if (await TryDownloadImageFromApiAsync(originalGameName, imagePath, logErrors))
            {
                return;
            }

            // 2. Fallback to extracting icon from EXE
            var mainExe = FindMainExecutable(gameInstallPath, sanitizedGameName, specificExePath);
            if (mainExe != null)
            {
                IconExtractor.SaveIconFromExe(mainExe, imagePath);
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, $"Failed to find/save image for {sanitizedGameName} in {gameInstallPath}");
        }
    }

    // This is the final fallback for special cases like Steam/Microsoft Store
    internal static async Task ExtractIconFromGameFolder(ILogErrors logErrors, string gameInstallPath, string sanitizedGameName, string windowsImagesPath, string specificExePath = null)
    {
        try
        {
            var iconPath = Path.Combine(windowsImagesPath, $"{sanitizedGameName}.png");
            if (File.Exists(iconPath)) return;

            var mainExe = FindMainExecutable(gameInstallPath, sanitizedGameName, specificExePath);
            if (mainExe != null)
            {
                IconExtractor.SaveIconFromExe(mainExe, iconPath);
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, $"Failed to extract icon for {sanitizedGameName} in {gameInstallPath}");
        }
    }

    private static string FindMainExecutable(string gameInstallPath, string sanitizedGameName, string specificExePath = null)
    {
        if (!Directory.Exists(gameInstallPath)) return null;

        // 1. Use the specific path if provided and it exists.
        if (!string.IsNullOrEmpty(specificExePath) && File.Exists(specificExePath))
        {
            return specificExePath;
        }

        // 2. Heuristics to find the main EXE
        var exeFiles = Directory.GetFiles(gameInstallPath, "*.exe", SearchOption.TopDirectoryOnly);
        if (exeFiles.Length == 0) return null;

        // 2a. Name match
        var mainExe = exeFiles.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Equals(sanitizedGameName, StringComparison.OrdinalIgnoreCase));
        if (mainExe != null) return mainExe;

        // 2b. Contains name
        mainExe = exeFiles.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f).Contains(sanitizedGameName, StringComparison.OrdinalIgnoreCase));
        if (mainExe != null) return mainExe;

        // 2c. Largest EXE (ignoring common non-game executables)
        return exeFiles
            .Where(static f => !f.Contains("unins", StringComparison.OrdinalIgnoreCase) &&
                               !f.Contains("setup", StringComparison.OrdinalIgnoreCase) &&
                               !f.Contains("crash", StringComparison.OrdinalIgnoreCase) &&
                               !f.Contains("redist", StringComparison.OrdinalIgnoreCase) &&
                               !f.Contains("dxsetup", StringComparison.OrdinalIgnoreCase) &&
                               !f.Contains("update", StringComparison.OrdinalIgnoreCase) &&
                               !f.Contains("unity", StringComparison.OrdinalIgnoreCase) &&
                               !f.Contains("launcher", StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(static f =>
            {
                try
                {
                    return new FileInfo(@"\\?\" + f).Length;
                }
                catch
                {
                    return 0L;
                }
            })
            .FirstOrDefault();
    }
}