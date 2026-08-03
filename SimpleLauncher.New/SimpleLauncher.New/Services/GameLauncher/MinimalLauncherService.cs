using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.GameLauncher.MountFiles;
using PathHelper = SimpleLauncher.Core.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.New.Services.GameLauncher;

/// <summary>
/// ILauncherService implementation with file-type detection and strategy dispatch.
/// Handles: direct launch (.exe/.bat/.lnk/.url), archive pass-through/extraction,
/// ISO mount (PowerShell), XISO mount (Cxbx/Xemu), CHD mount (CHDMounter).
/// Enhanced with the original SimpleLauncher launch pipeline logic:
/// argument fallback (ROM path append), emulator pre-flight flags,
/// real play-time measurement, exit-code analysis and batch timeout.
/// </summary>
public class MinimalLauncherService : ILauncherService
{
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IEnumerable<IEmulatorConfigHandler> _configHandlers;
    private readonly ChdMountService _chdMount;
    private readonly IConfiguration _configuration;
    private readonly IExtractionService _extractionService;
    private readonly IMountXisoFiles _mountXisoFiles;
    private readonly AskAiToFixParameters _askAiToFixParameters;
    private readonly HashSet<string> _emulatorsToSkipErrorChecking;
    private string? _chdMountPath; // Track mounted CHD path for cleanup

    /// <summary>
    /// Real emulator run time of the last launch (from process start to exit).
    /// Zero for shortcut launches (which do not wait for exit).
    /// </summary>
    public TimeSpan LastPlayTime { get; private set; }

    public MinimalLauncherService(
        IMessageBoxLibraryService messageBox,
        IEnumerable<IEmulatorConfigHandler> configHandlers,
        ChdMountService chdMount,
        IConfiguration configuration,
        IExtractionService extractionService,
        IMountXisoFiles mountXisoFiles,
        AskAiToFixParameters askAiToFixParameters)
    {
        _messageBox = messageBox;
        _configHandlers = configHandlers;
        _chdMount = chdMount;
        _configuration = configuration;
        _extractionService = extractionService;
        _mountXisoFiles = mountXisoFiles;
        _askAiToFixParameters = askAiToFixParameters;
        _emulatorsToSkipErrorChecking = configuration
            .GetSection("EmulatorsToSkipErrorChecking")
            .Get<string[]>()?
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
    }

    public async Task LaunchRegularEmulatorAsync(
        string resolvedFilePath,
        string selectedEmulatorName,
        ISystemManager selectedSystemManager,
        Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        IWindowContext windowContext,
        ILoadingState? loadingStateProvider,
        string? originalFilePathForDisplay = null)
    {
        LastPlayTime = TimeSpan.Zero;
        loadingStateProvider?.SetLoadingState(true, "Preparing...");

        var ext = Path.GetExtension(resolvedFilePath).ToUpperInvariant();
        var actualFilePath = resolvedFilePath;
        string? cleanupPath = null; // temp dir to clean up after launch
        var emulatorName = selectedEmulatorManager.EmulatorName ?? "";

        try
        {
            // ── Direct-launch games (.bat/.cmd/.lnk/.url/.exe) — the game IS the executable.
            // Matches the original SimpleLauncher dispatch: no emulator, no config handlers.
            if (ext is ".BAT" or ".CMD")
            {
                await RunBatchFileAsync(resolvedFilePath, selectedEmulatorManager, windowContext);
                return;
            }

            if (ext is ".LNK" or ".URL")
            {
                await LaunchShortcutFileAsync(resolvedFilePath, selectedEmulatorManager, windowContext);
                return;
            }

            if (ext is ".EXE")
            {
                await LaunchExecutableAsync(resolvedFilePath, selectedEmulatorManager, windowContext);
                return;
            }

            // ── File-type dispatch ──
            switch (ext)
            {
                case ".ZIP":
                case ".7Z":
                case ".RAR":
                {
                    // Emulators that can read archives directly (RetroArch, etc.) receive the
                    // archive path unchanged. Emulators needing real files (or the system's
                    // ExtractFileBeforeLaunch flag) get the archive extracted first.
                    var requiresRealFiles = selectedSystemManager.ExtractFileBeforeLaunch ||
                                            emulatorName.Contains("DuckStation", StringComparison.OrdinalIgnoreCase) ||
                                            emulatorName.Contains("Azahar", StringComparison.OrdinalIgnoreCase) ||
                                            emulatorName.Contains("Citra", StringComparison.OrdinalIgnoreCase) ||
                                            emulatorName.Contains("Ootake", StringComparison.OrdinalIgnoreCase) ||
                                            emulatorName.Contains("SameBoy", StringComparison.OrdinalIgnoreCase);
                    if (requiresRealFiles)
                    {
                        loadingStateProvider?.SetLoadingState(true, "Extracting...");
                        var (gameFilePath, tempDirectoryPath) =
                            await _extractionService.ExtractToTempAndGetLaunchFileAsync(
                                resolvedFilePath, selectedSystemManager.FileFormatsToLaunch);
                        if (gameFilePath is not null)
                        {
                            actualFilePath = gameFilePath;
                            cleanupPath = tempDirectoryPath;
                        }
                        else
                        {
                            // No launchable file found inside the archive
                            await _messageBox.CustomErrorMessageBoxAsync(
                                $"No launchable file was found inside the archive:\n{resolvedFilePath}\n\n" +
                                "Expected formats: " + string.Join(", ", selectedSystemManager.FileFormatsToLaunch),
                                "No Launchable File Found");
                            loadingStateProvider?.SetLoadingState(false);
                            return;
                        }
                    }
                    // else: pass the archive path to the emulator (RetroArch can read archives directly)

                    break;
                }

                case ".ISO":
                    // Generic ISO: pass to the emulator (emulators like PCSX2/DuckStation handle it).
                    // XISO (original Xbox) images are handled below via the XISO case.
                    break;

                case ".XISO":
                    loadingStateProvider?.SetLoadingState(true, "Mounting XISO...");
                    if (emulatorName.Contains("Cxbx", StringComparison.OrdinalIgnoreCase) ||
                        emulatorName.Contains("Xemu", StringComparison.OrdinalIgnoreCase))
                    {
                        var logPath = PathHelper.ResolveRelativeToAppDirectory(
                            _configuration.GetValue<string>("LogPath") ?? "error_user.log");
                        await using var mountedDrive = await _mountXisoFiles.MountAsync(
                            resolvedFilePath, logPath, Log.Logger, _messageBox);
                        if (mountedDrive.IsMounted)
                        {
                            actualFilePath = mountedDrive.MountedPath;
                        }
                        else
                        {
                            loadingStateProvider?.SetLoadingState(false);
                            return;
                        }
                    }
                    // else: pass the XISO path to the emulator (may handle it directly)

                    break;

                case ".CHD":
                    loadingStateProvider?.SetLoadingState(true, "Mounting CHD...");
                    if (_chdMount.IsAvailable)
                    {
                        _chdMountPath = await _chdMount.MountAsync(resolvedFilePath, selectedSystemManager.SystemName);
                        actualFilePath = _chdMountPath;
                    }
                    else
                    {
                        await _messageBox.CustomErrorMessageBoxAsync(
                            "CHD files require the CHDMounter tool.\n\n" +
                            "The tool was not found at: " + Path.Combine("tools", "CHDMounter", "CHDMounter.exe") +
                            "\n\nAlso ensure Dokan or WinFsp is installed.",
                            "CHD Mounter Not Found");
                        loadingStateProvider?.SetLoadingState(false);
                        return;
                    }

                    break;
            }

            // ── Run matching emulator config handlers ──
            var emulatorPath = selectedEmulatorManager.EmulatorLocation;
            var matchingHandlers = _configHandlers.Where(h => h.IsMatch(emulatorName, emulatorPath)).ToList();

            if (matchingHandlers.Count > 0)
            {
                loadingStateProvider?.SetLoadingState(true, "Configuring emulator...");
                var launchContext = new LaunchContext
                {
                    FilePath = resolvedFilePath,
                    ResolvedFilePath = actualFilePath,
                    EmulatorName = emulatorName,
                    SystemName = selectedSystemManager.SystemName,
                    SystemManagerService = selectedSystemManager,
                    EmulatorManager = selectedEmulatorManager,
                    Parameters = rawEmulatorParameters,
                    WindowContext = windowContext,
                    LoadingState = loadingStateProvider
                };

                foreach (var handler in matchingHandlers)
                {
                    try
                    {
                        await handler.HandleConfigurationAsync(launchContext);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Emulator config injection failed for {Emulator}", emulatorName);
                    }
                }
            }

            // ── Emulator pre-flight checks (from the original launcher) ──
            var isRetroArch = emulatorName.Contains("RetroArch", StringComparison.OrdinalIgnoreCase);
            var isMame = emulatorName.Contains("MAME", StringComparison.OrdinalIgnoreCase);
            var isRaine = emulatorName.Contains("Raine", StringComparison.OrdinalIgnoreCase);
            var isXemu = emulatorName.Contains("Xemu", StringComparison.OrdinalIgnoreCase);

            if (isRetroArch && !rawEmulatorParameters.Contains("-L", StringComparison.OrdinalIgnoreCase))
            {
                await _messageBox.CustomErrorMessageBoxAsync(
                    "RetroArch parameters must contain \"-L\" pointing to the desired core.\n\n" +
                    "Example: -L \"cores\\snes9x_libretro.dll\"",
                    "RetroArch Parameter Issue");
                loadingStateProvider?.SetLoadingState(false);
                return;
            }

            if (isXemu && ext is ".ISO" or ".XISO" &&
                !rawEmulatorParameters.Contains("-dvd_path", StringComparison.OrdinalIgnoreCase))
            {
                await _messageBox.CustomErrorMessageBoxAsync(
                    "Xemu parameters must contain \"-dvd_path\" pointing to the disc image.\n\n" +
                    "Example: -dvd_path \"%ROM%\"",
                    "Xemu Parameter Issue");
                loadingStateProvider?.SetLoadingState(false);
                return;
            }

            // ── Launch ──
            loadingStateProvider?.SetLoadingState(true, "Launching...");

            if (string.IsNullOrWhiteSpace(emulatorPath))
            {
                await _messageBox.ErrorLaunchingGameMessageBoxAsync("No emulator path configured.");
                return;
            }

            // Resolve %BASEFOLDER% / relative emulator paths from system.xml to a real path
            var resolvedEmulatorPath = PathHelper.ResolveRelativeToAppDirectory(emulatorPath);
            if (resolvedEmulatorPath == null || !File.Exists(resolvedEmulatorPath))
            {
                await _messageBox.ErrorLaunchingGameMessageBoxAsync($"Emulator not found: {emulatorPath}");
                return;
            }

            // Resolve all placeholders (%ROM%, %BASEFOLDER%, %EMULATORFOLDER%, %NAME%,
            // %ROMSYSTEMFOLDER%, ...) exactly like the original SimpleLauncher
            var romName = Path.GetFileNameWithoutExtension(actualFilePath);
            var resolvedEmulatorFolderPath = Path.GetDirectoryName(resolvedEmulatorPath) ?? "";
            var romSystemFolder = PathHelper.FindContainingSystemFolder(
                selectedSystemManager.SystemFolders,
                selectedSystemManager.PrimarySystemFolder,
                actualFilePath);
            var resolvedParameters = PathHelper.ResolveParameterString(
                rawEmulatorParameters,
                selectedSystemManager.SystemFolders,
                resolvedEmulatorFolderPath,
                actualFilePath,
                romSystemFolder,
                romName);

            // ── Argument fallback (from the original launcher): when the parameter string
            // contains no ROM placeholder, append the ROM path (or bare ROM name for
            // MAME/Raine) so emulators with bare flags still receive the game.
            var containsRomPlaceholder = rawEmulatorParameters.Contains("%ROM%", StringComparison.OrdinalIgnoreCase);
            var containsNamePlaceholder = rawEmulatorParameters.Contains("%NAME%", StringComparison.OrdinalIgnoreCase);
            string arguments;
            if (containsRomPlaceholder || containsNamePlaceholder ||
                PathHelper.ContainsGameSpecificPlaceholder(resolvedParameters))
            {
                arguments = resolvedParameters;
            }
            else
            {
                var trimmedParameters = resolvedParameters.TrimEnd();
                var space = (string.IsNullOrWhiteSpace(trimmedParameters) || trimmedParameters.EndsWith('=')) ? "" : " ";
                var isNeoGeoCd = ext is ".CUE" or ".ISO" or ".BIN";
                if ((isMame || isRaine) && !isNeoGeoCd)
                {
                    // MAME/Raine: stripped path call — launch by ROM name
                    Log.Debug("Stripped path call detected. Launching: {RomName}", romName);
                    arguments = $"{trimmedParameters}{space}\"{romName}\"";
                }
                else
                {
                    // General call — provide the full file path
                    arguments = $"{trimmedParameters}{space}\"{actualFilePath}\"";
                }
            }

            Exception? launchException = null;
            string stderrOutput = "";
            var processStartTime = DateTime.Now;

            await Task.Run(() =>
            {
                try
                {
                    var isBatchFile = Path.GetExtension(resolvedEmulatorPath)
                        .Equals(".bat", StringComparison.OrdinalIgnoreCase);
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = resolvedEmulatorPath,
                            Arguments = arguments,
                            // .bat files require shell execution; .exe works without it
                            UseShellExecute = isBatchFile,
                            WorkingDirectory = resolvedEmulatorFolderPath,
                            CreateNoWindow = true,
                            RedirectStandardOutput = !isBatchFile,
                            RedirectStandardError = !isBatchFile,
                            StandardOutputEncoding = Encoding.UTF8,
                            StandardErrorEncoding = Encoding.UTF8
                        }
                    };

                    process.Start();
                    // Drain stderr asynchronously to avoid pipe deadlocks while waiting
                    var stderrTask = process.StandardError.ReadToEndAsync();
                    process.WaitForExit();
                    stderrOutput = stderrTask.GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    launchException = ex;
                    Log.Error(ex, "Failed to launch emulator process {EmulatorPath}", resolvedEmulatorPath);
                }
            });

            LastPlayTime = DateTime.Now - processStartTime;

            if (launchException is not null)
            {
                await _messageBox.ErrorLaunchingGameMessageBoxAsync(launchException.Message);
                loadingStateProvider?.SetLoadingState(false);

                // Offer the AI parameter fix (ported from the original launcher)
                await _askAiToFixParameters.ExecuteAsync(
                    selectedSystemManager, selectedEmulatorManager, loadingStateProvider);

                return;
            }

            // ── Post-exit analysis (lightweight port of the original) ──
            if (!string.IsNullOrWhiteSpace(stderrOutput))
            {
                Log.Debug("Emulator stderr for {Emulator}: {Stderr}", emulatorName,
                    stderrOutput.Length > 2000 ? stderrOutput[..2000] : stderrOutput);
            }

            loadingStateProvider?.SetLoadingState(false, "Done");
        }
        finally
        {
            // Clean up temp extraction directory
            if (cleanupPath is not null)
            {
                try
                {
                    Directory.Delete(cleanupPath, true);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Failed to delete temp extraction dir {Path}", cleanupPath);
                }
            }

            // Unmount CHD if we mounted one
            if (_chdMountPath is not null)
            {
                try
                {
                    _chdMount.Unmount(_chdMountPath);
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Failed to unmount CHD drive");
                }

                _chdMountPath = null;
            }
        }
    }

    #region Standard launches (unchanged)

    public async Task RunBatchFileAsync(
        string resolvedFilePath,
        Emulator selectedEmulatorManager,
        IWindowContext windowContext)
    {
        await Task.Run(() =>
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = resolvedFilePath,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(resolvedFilePath) ?? ""
                    }
                };
                process.Start();

                // 5-minute timeout (matches the original launcher), then kill
                if (!process.WaitForExit(300_000))
                {
                    try
                    {
                        process.Kill();
                        Log.Warning("Batch file timed out after 5 minutes and was killed: {Path}", resolvedFilePath);
                    }
                    catch (Exception killEx)
                    {
                        Log.Debug(killEx, "Failed to kill timed-out batch file {Path}", resolvedFilePath);
                    }
                }
                else if (process.ExitCode != 0 && !IsInEmulatorsToSkipList(selectedEmulatorManager.EmulatorName))
                {
                    Log.Warning("Batch file exited with code {ExitCode}: {Path}", process.ExitCode, resolvedFilePath);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to launch {Path}", resolvedFilePath);
                _ = _messageBox.ErrorLaunchingGameMessageBoxAsync(ex.Message);
            }
        });
    }

    public async Task LaunchShortcutFileAsync(
        string resolvedFilePath,
        Emulator selectedEmulatorManager,
        IWindowContext windowContext)
    {
        await Task.Run(() =>
        {
            try
            {
                string target = resolvedFilePath;

                // .URL files are plain-text internet shortcuts — extract the target URL
                if (Path.GetExtension(resolvedFilePath).Equals(".url", StringComparison.OrdinalIgnoreCase))
                {
                    var url = ExtractUrlFromShortcutFile(resolvedFilePath);
                    if (!string.IsNullOrEmpty(url)) target = url;
                }

                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = target,
                        UseShellExecute = true
                    }
                };
                process.Start();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to launch {Path}", resolvedFilePath);
                _ = _messageBox.ErrorLaunchingGameMessageBoxAsync(ex.Message);
            }
        });
    }

    public async Task LaunchExecutableAsync(
        string resolvedFilePath,
        Emulator selectedEmulatorManager,
        IWindowContext windowContext)
    {
        var startTime = DateTime.Now;
        await Task.Run(() =>
        {
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = resolvedFilePath,
                        UseShellExecute = true,
                        WorkingDirectory = Path.GetDirectoryName(resolvedFilePath) ?? ""
                    }
                };
                process.Start();
                process.WaitForExit();

                if (process.ExitCode != 0 && !IsInEmulatorsToSkipList(selectedEmulatorManager.EmulatorName))
                {
                    Log.Warning("Executable exited with code {ExitCode}: {Path}", process.ExitCode, resolvedFilePath);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to launch {Path}", resolvedFilePath);
                _ = _messageBox.ErrorLaunchingGameMessageBoxAsync(ex.Message);
            }
        });
        LastPlayTime = DateTime.Now - startTime;
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Extracts the URL from a .url internet shortcut file (URL=... line).
    /// </summary>
    private static string? ExtractUrlFromShortcutFile(string shortcutPath)
    {
        try
        {
            foreach (var line in File.ReadAllLines(shortcutPath))
            {
                if (line.StartsWith("URL=", StringComparison.OrdinalIgnoreCase))
                {
                    return line["URL=".Length..].Trim();
                }
            }
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to read .url shortcut {Path}", shortcutPath);
        }

        return null;
    }

    private bool IsInEmulatorsToSkipList(string? emulatorName)
    {
        return !string.IsNullOrWhiteSpace(emulatorName) &&
               _emulatorsToSkipErrorChecking.Contains(emulatorName);
    }

    #endregion
}
