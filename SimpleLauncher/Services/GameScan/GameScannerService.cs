using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Services.CheckPaths;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.GameScan.Models;
using SimpleLauncher.Core.Services.SystemManager;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Services.GameScan;

public class GameScannerService
{
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBoxLibrary;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDebugLogger _debugLogger;
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

    private string _windowsRomsPath;
    private string _windowsImagesPath;

    internal bool WasNewSystemCreated { get; private set; }

    private bool _timeoutMessageShown;

    public GameScannerService(ILogErrors logErrors, IMessageBoxLibraryService messageBoxLibrary, IConfiguration configuration, IHttpClientFactory httpClientFactory, IDebugLogger debugLogger)
    {
        _logErrors = logErrors;
        _messageBoxLibrary = messageBoxLibrary;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _debugLogger = debugLogger;
    }

    internal async Task ScanForStoreGamesAsync()
    {
        try
        {
            // Initialize paths based on existing system configuration or create default
            var pathResult = await InitializeWindowsPathsAsync();
            _windowsRomsPath = pathResult.RomsPath;
            _windowsImagesPath = pathResult.ImagesPath;
            WasNewSystemCreated = pathResult.WasNewSystemCreated;

            var tasks = new List<Task>
            {
                ScanSteamGames.ScanSteamGamesAsync(this, _logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanEpicGames.ScanEpicGamesAsync(this, _logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanAmazonGames.ScanAmazonGamesAsync(this, _logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanBattleNetGames.ScanBattleNetGamesAsync(this, _logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanGogGames.ScanGogGamesAsync(this, _logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanHumbleGames.ScanHumbleGamesAsync(this, _logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanItchioGames.ScanItchioGamesAsync(this, _logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanRockstarGames.ScanRockstarGamesAsync(this, _logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanUplayGames.ScanUplayGamesAsync(this, _logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanEaGames.ScanEaGamesAsync(this, _logErrors, _windowsRomsPath, _windowsImagesPath, IgnoredGameNames),
                ScanMicrosoftStoreGames.ScanMicrosoftStoreGamesAsync(this, _logErrors, _windowsRomsPath, _windowsImagesPath)
            };

            await Task.WhenAll(tasks);

            _debugLogger.Log("[GameScannerService] All store game scans completed.");
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "An error occurred during the game scanning process.");
        }
    }

    private async Task<(string RomsPath, string ImagesPath, bool WasNewSystemCreated)> InitializeWindowsPathsAsync()
    {
        try
        {
            // Check if the system already exists
            var existingSystems = SystemManager.SystemManager.LoadSystemManagers(_configuration);
            var existingWindowsSystem = existingSystems.FirstOrDefault(static s =>
                s.SystemName.Equals(WindowsSystemName, StringComparison.OrdinalIgnoreCase));

            if (existingWindowsSystem != null)
            {
                // Use existing paths from the system configuration
                var existingRomsPath = existingWindowsSystem.PrimarySystemFolder;
                var existingImagesPath = existingWindowsSystem.SystemImageFolder;

                // Resolve the paths (handle %BASEFOLDER% placeholder)
                var resolvedRomsPath = PathHelper.ResolveRelativeToAppDirectory(existingRomsPath);
                var resolvedImagesPath = PathHelper.ResolveRelativeToAppDirectory(existingImagesPath);

                _debugLogger.Log($"[GameScannerService] Using existing '{WindowsSystemName}' system paths: ROMs='{resolvedRomsPath}', Images='{resolvedImagesPath}'");

                return (resolvedRomsPath, resolvedImagesPath, false);
            }

            // System doesn't exist, create it with default paths
            _debugLogger.Log($"[GameScannerService] '{WindowsSystemName}' system not found. Creating it now.");

            var defaultRomsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "roms", "Microsoft Windows");
            var defaultImagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "Microsoft Windows");

            var windowsSystem = new SystemManager.SystemManager
            {
                SystemName = WindowsSystemName,
                SystemFolders = ["%BASEFOLDER%\\roms\\Microsoft Windows"],
                SystemImageFolder = "%BASEFOLDER%\\images\\Microsoft Windows",
                FileFormatsToSearch = ["url", "lnk", "bat"],
                GroupByFolder = false,
                ExtractFileBeforeLaunch = false,
                FileFormatsToLaunch = [],
                Emulators =
                [
                    new Emulator
                    {
                        EmulatorName = "Direct Launch",
                        EmulatorLocation = "",
                        EmulatorParameters = "",
                        ReceiveANotificationOnEmulatorError = true
                    }
                ]
            };

            await SystemManager.SystemManager.SaveSystemConfigurationAsync(windowsSystem);

            // Create the necessary directories
            Directory.CreateDirectory(defaultRomsPath);
            Directory.CreateDirectory(defaultImagesPath);

            _debugLogger.Log($"[GameScannerService] Created new '{WindowsSystemName}' system with default paths: ROMs='{defaultRomsPath}', Images='{defaultImagesPath}'");

            return (defaultRomsPath, defaultImagesPath, true);
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, "Failed to initialize 'Microsoft Windows' system paths.");

            // Fall back to default paths even on error
            var fallbackRomsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "roms", "Microsoft Windows");
            var fallbackImagesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "images", "Microsoft Windows");

            return (fallbackRomsPath, fallbackImagesPath, false);
        }
    }

    internal async Task<bool> TryDownloadImageFromApiAsync(string gameName, string destinationPath, ILogErrors logErrors)
    {
        if (string.IsNullOrWhiteSpace(gameName)) return false;

        // Try up to 2 times (initial attempt + 1 retry after 5 seconds)
        for (var attempt = 0; attempt < 2; attempt++)
        {
            try
            {
                using var client = _httpClientFactory.CreateClient("GameImageClient");

                var encodedGameName = WebUtility.UrlEncode(gameName);
                var response = await client.GetAsync($"api/v1/games/search?name={encodedGameName}");

                if (!response.IsSuccessStatusCode)
                {
                    if (response.StatusCode != HttpStatusCode.NotFound)
                    {
                        _debugLogger.Log($"[GameScannerService] API query for '{gameName}' failed with status: {response.StatusCode}");
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
                    _debugLogger.Log($"[GameScannerService] Successfully downloaded image for '{gameName}' from API.");
                    return true;
                }
            }
            catch (OperationCanceledException) when (attempt == 0)
            {
                // Timeout on first attempt - wait and retry
                _debugLogger.Log($"[GameScannerService] Image download timeout for '{gameName}', retrying in 5 seconds...");
                await Task.Delay(5000);
            }
            catch (HttpRequestException ex) when (attempt == 0)
            {
                // Network error on first attempt - wait and retry
                var innerMessage = ex.InnerException?.Message ?? "none";
                _debugLogger.Log($"[GameScannerService] Image download network error for '{gameName}': {ex.Message}. Inner: {innerMessage}. Retrying in 5 seconds...");
                await Task.Delay(5000);
            }
            catch (Exception ex)
            {
                // On second attempt or unexpected errors, fail silently and let the caller fall back to icon extraction
                var errorType = ex switch
                {
                    OperationCanceledException => "timeout",
                    HttpRequestException => "network error",
                    _ => "error"
                };
                var innerDetails = GetInnerExceptionDetails(ex);
                var logMessage = $"[GameScannerService] Image download failed for '{gameName}' after retry ({errorType}: {ex.Message}).{innerDetails} Falling back to icon extraction.";
                _debugLogger.Log(logMessage);

                // Log persistent network errors to help identify API issues, but don't spam logs
                if (ex is HttpRequestException or OperationCanceledException)
                {
                    logErrors?.LogErrorAsync(ex, $"Failed to download image for '{gameName}' from API after retry.");

                    // Show message box for timeout/network errors on final attempt (attempt == 1)
                    if (attempt == 1 && !_timeoutMessageShown)
                    {
                        _timeoutMessageShown = true;
                        await _messageBoxLibrary.ShowImageDownloadTimeoutMessageBox();
                    }
                }
            }
        }

        return false;
    }

    internal async Task FindAndSaveGameImageAsync(ILogErrors logErrors, string originalGameName, string gameInstallPath, string sanitizedGameName, string windowsImagesPath, string specificExePath = null)
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
                IconExtractor.SaveIconFromExe(mainExe, imagePath, logErrors);
            }
        }
        catch (Exception ex)
        {
            await logErrors.LogErrorAsync(ex, $"Failed to find/save image for {sanitizedGameName} in {gameInstallPath}");
        }
    }

    // This is the final fallback for special cases like Steam/Microsoft Store
    internal async Task ExtractIconFromGameFolderAsync(ILogErrors logErrors, string gameInstallPath, string sanitizedGameName, string windowsImagesPath, string specificExePath = null)
    {
        try
        {
            var iconPath = Path.Combine(windowsImagesPath, $"{sanitizedGameName}.png");
            if (File.Exists(iconPath)) return;

            var mainExe = FindMainExecutable(gameInstallPath, sanitizedGameName, specificExePath);
            if (mainExe != null)
            {
                IconExtractor.SaveIconFromExe(mainExe, iconPath, logErrors);
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
                    return new FileInfo(PathHelper.GetLongPath(f)).Length;
                }
                catch
                {
                    return 0L;
                }
            })
            .FirstOrDefault();
    }

    private static string GetInnerExceptionDetails(Exception ex)
    {
        var inner = ex.InnerException;
        if (inner == null) return string.Empty;

        var details = " Inner exceptions:";
        var current = inner;
        var depth = 1;
        while (current != null && depth <= 3)
        {
            details += $" [{depth}] {current.GetType().Name}: {current.Message}";
            current = current.InnerException;
            depth++;
        }

        return details;
    }
}
