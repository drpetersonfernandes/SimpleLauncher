using System.Diagnostics;
using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Avalonia.Services.PlayHistory;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.GameLauncher.MountFiles;
using SimpleLauncher.Core.Services.GamePad;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.Core.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.UsageStats;
using PathHelper = SimpleLauncher.Core.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Avalonia.Services.GameLauncher;

/// <summary>
/// ILauncherService implementation with file-type detection and strategy dispatch.
/// Handles: direct launch (.exe/.bat/.lnk/.url), archive pass-through/extraction,
/// ZIP mounting (RPCS3/ScummVM/XBLA), XISO mount (Cxbx), CHD mount (CHDMounter via Core).
/// Mirrors the original SimpleLauncher launch pipeline: argument fallback (ROM path append),
/// emulator pre-flight flags, real play-time measurement and emulator config injection.
/// </summary>
public class MinimalLauncherService : ILauncherService
{
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IEnumerable<IEmulatorConfigHandler> _configHandlers;
    private readonly IConfiguration _configuration;
    private readonly IExtractionService _extractionService;
    private readonly IMountXisoFiles _mountXisoFiles;
    private readonly IMountChdFiles _mountChdFiles;
    private readonly IMountZipFiles _mountZipFiles;
    private readonly AskAiToFixParameters _askAiToFixParameters;
    private readonly SettingsManagerService _settings;
    private readonly IEnumerable<ILaunchStrategy> _launchStrategies;
    private readonly HashSet<string> _emulatorsToSkipErrorChecking;
    private readonly PlayHistoryManager _playHistoryManager;
    private readonly Stats _stats;
    private readonly GamePadController _gamePadController;

    /// <summary>
    /// Real emulator run time of the last launch (from process start to exit).
    /// Zero for shortcut launches (which do not wait for exit).
    /// </summary>
    public TimeSpan LastPlayTime { get; private set; }

    /// <summary>
    /// Raised after a game has finished playing with a play time above the 5-second threshold.
    /// </summary>
    public event EventHandler<GamePlayedEventArgs>? GamePlayed;

    public MinimalLauncherService(
        IMessageBoxLibraryService messageBox,
        IEnumerable<IEmulatorConfigHandler> configHandlers,
        IConfiguration configuration,
        IExtractionService extractionService,
        IMountXisoFiles mountXisoFiles,
        IMountChdFiles mountChdFiles,
        IMountZipFiles mountZipFiles,
        AskAiToFixParameters askAiToFixParameters,
        SettingsManagerService settings,
        IEnumerable<ILaunchStrategy> launchStrategies,
        PlayHistoryManager playHistoryManager,
        Stats stats,
        GamePadController gamePadController)
    {
        _messageBox = messageBox;
        _configHandlers = configHandlers;
        _configuration = configuration;
        _extractionService = extractionService;
        _mountXisoFiles = mountXisoFiles;
        _mountChdFiles = mountChdFiles;
        _mountZipFiles = mountZipFiles;
        _askAiToFixParameters = askAiToFixParameters;
        _settings = settings;
        _launchStrategies = launchStrategies.OrderBy(static s => s.Priority).ToList();
        _emulatorsToSkipErrorChecking = configuration
            .GetSection("EmulatorsToSkipErrorChecking")
            .Get<string[]>()?
            .ToHashSet(StringComparer.OrdinalIgnoreCase) ?? [];
        _playHistoryManager = playHistoryManager;
        _stats = stats;
        _gamePadController = gamePadController;
    }

    /// <summary>
    /// Game launch pipeline entry point (port of the WPF GameLauncherService.HandleButtonClickAsync):
    /// resolves the path, builds a <see cref="LaunchContext"/>, runs light validation, and dispatches
    /// to the first matching <see cref="ILaunchStrategy"/> (ascending priority, Default last).
    /// Play-time tracking stays with <see cref="LastPlayTime"/>.
    /// </summary>
    public async Task HandleButtonClickAsync(
        string filePath,
        string selectedEmulatorName,
        string selectedSystemName,
        ISystemManager selectedSystemManager,
        Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        IWindowContext windowContext,
        ILoadingState? loadingStateProvider)
    {
        // 1. Create Context
        var context = new LaunchContext
        {
            FilePath = filePath,
            ResolvedFilePath = PathHelper.ResolveRelativeToAppDirectory(filePath) ?? filePath,
            EmulatorName = selectedEmulatorName,
            SystemName = selectedSystemName,
            SystemManagerService = selectedSystemManager,
            EmulatorManager = selectedEmulatorManager,
            Parameters = rawEmulatorParameters,
            Settings = _settings,
            WindowContext = windowContext,
            LoadingState = loadingStateProvider
        };

        // Pause gamepad input while the emulator is running to prevent
        // mouse/scroll leaking to the desktop (mirrors the WPF app behavior).
        var wasGamePadRunning = _gamePadController.IsRunning;
        if (wasGamePadRunning) await _gamePadController.StopAsync();

        try
        {
            // 2. Validate SystemManagerService and Emulators before resolving
            if (context.SystemManagerService?.Emulators == null ||
                context.SystemManagerService.Emulators.Count == 0 ||
                context.EmulatorManager == null)
            {
                Log.Warning("SystemManagerService or Emulators is null/empty when attempting to launch.");
                await _messageBox.ThereWasAnErrorLaunchingThisGameMessageBoxAsync(LogFilePath());
                return;
            }

            // 3. Perform Validation (file exists — expected user condition, Information level)
            if (!await ValidateContextAsync(context))
            {
                return;
            }

            // 4. Select and execute the first matching strategy (ascending priority).
            // The inline dispatch inside LaunchRegularEmulatorAsync mirrors the WPF
            // Default/ZIP/XISO/CHD-mount strategies, so only strategies that previously
            // had no Avalonia handling (PBP conversion, DOSBox, Commander Genius,
            // CHD-to-CUE) can match before the Default fallback runs.
            var strategy = _launchStrategies.FirstOrDefault(s => s.IsMatch(context));
            if (strategy == null)
            {
                Log.Warning("No launch strategy found for the context: SystemName='{System}', EmulatorName='{Emulator}', FilePath='{Path}'",
                    context.SystemName, context.EmulatorName, context.FilePath);
                await _messageBox.ThereWasAnErrorLaunchingThisGameMessageBoxAsync(LogFilePath());
                return;
            }

            await strategy.ExecuteAsync(context, this);
        }
        catch (Exception ex)
        {
            var detailedMessage = $"Launch Pipeline Failed.\n" +
                                  $"Exception Type: {ex.GetType().FullName}\n" +
                                  $"SystemName: '{context.SystemName ?? "null"}'\n" +
                                  $"EmulatorName: '{context.EmulatorName ?? "null"}'\n" +
                                  $"FilePath: '{context.FilePath ?? "null"}'\n" +
                                  $"ResolvedFilePath: '{context.ResolvedFilePath ?? "null"}'";
            Log.Error(ex, detailedMessage);
            await _messageBox.CouldNotLaunchGameMessageBoxAsync(LogFilePath());
        }
        finally
        {
            if (wasGamePadRunning) await _gamePadController.StartAsync();
        }
    }

    private async Task<bool> ValidateContextAsync(LaunchContext context)
    {
        if (string.IsNullOrWhiteSpace(context.ResolvedFilePath))
        {
            Log.Warning("Resolved file path is empty");
            await _messageBox.FilePathIsInvalidMessageBoxAsync(LogFilePath());
            return false;
        }

        var fileExists = File.Exists(context.ResolvedFilePath);
        var directoryExists = Directory.Exists(context.ResolvedFilePath);

        if (!fileExists && !directoryExists)
        {
            // Expected condition: the game entry is stale (file deleted/moved since the list
            // was loaded) and the user is already notified via the message box — not a bug report.
            Log.Information("File not found: {Path}", context.ResolvedFilePath);
            await _messageBox.FilePathIsInvalidMessageBoxAsync(LogFilePath());
            return false;
        }

        if (string.IsNullOrWhiteSpace(context.EmulatorName))
        {
            Log.Warning("Emulator name is empty");
            await _messageBox.CouldNotLaunchGameMessageBoxAsync(LogFilePath());
            return false;
        }

        // Add the GroupByFolder check
        if (context.SystemManagerService is { GroupByFolder: true })
        {
            var emulatorName = context.EmulatorName;
            var emulatorLocation = context.EmulatorManager?.EmulatorLocation ?? "";

            var isMame = emulatorName.Contains("MAME", StringComparison.OrdinalIgnoreCase) ||
                         emulatorLocation.Contains("mame.exe", StringComparison.OrdinalIgnoreCase) ||
                         emulatorLocation.Contains("mame64.exe", StringComparison.OrdinalIgnoreCase);

            var isDosBox = emulatorName.Contains("DOSBox", StringComparison.OrdinalIgnoreCase) ||
                           emulatorLocation.Contains("dosbox", StringComparison.OrdinalIgnoreCase);

            if (!isMame && !isDosBox)
            {
                await _messageBox.GroupByFolderOnlyForMameAndDosBoxMessageBoxAsync();
                return false;
            }
        }

        return true;
    }

    private string LogFilePath()
    {
        return PathHelper.ResolveLogFilePath(_configuration.GetValue<string>("LogPath") ?? "error_user.log");
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
        var emulatorLocation = selectedEmulatorManager.EmulatorLocation ?? "";

        // Mount handles live for the whole launch (drive must stay mounted while the
        // emulator runs — mirroring the WPF strategies which dispose after launch).
        MountChdDrive? mountedChd = null;
        MountXisoDrive? mountedXiso = null;

        try
        {
            // ── Direct-launch games (.bat/.cmd/.lnk/.url/.exe) — the game IS the executable.
            // Matches the original SimpleLauncher dispatch: no emulator, no config handlers.
            switch (ext)
            {
                case ".BAT" or ".CMD":
                    await RunBatchFileAsync(resolvedFilePath, selectedEmulatorManager, windowContext);
                    return;
                case ".LNK" or ".URL":
                    await LaunchShortcutFileAsync(resolvedFilePath, selectedEmulatorManager, windowContext);
                    return;
                case ".EXE":
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
                    // ZIP mounting strategies (mirror of the WPF ZipMountStrategy): RPCS3
                    // games are mounted and EBOOT.BIN is located, ScummVM games get the
                    // archive extracted to its data folder, XBLA games are searched for a
                    // launchable file. These paths handle the whole launch themselves.
                    var isRpcs3 = emulatorName.Contains("RPCS3", StringComparison.OrdinalIgnoreCase);
                    var isScummVm = selectedSystemManager.SystemName.Contains("Scumm", StringComparison.OrdinalIgnoreCase);
                    var isXbla = selectedSystemManager.SystemName.Contains("xbla", StringComparison.OrdinalIgnoreCase);

                    if (isRpcs3 || isScummVm || isXbla)
                    {
                        var logPath = PathHelper.ResolveRelativeToAppDirectory(
                            _configuration.GetValue<string>("LogPath") ?? "error_user.log");
                        loadingStateProvider?.SetLoadingState(true, "Mounting archive...");

                        if (isRpcs3)
                        {
                            await _mountZipFiles.MountZipFileAndLoadEbootBinAsync(
                                resolvedFilePath, selectedSystemManager.SystemName, emulatorName,
                                selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters,
                                windowContext, logPath, this, Log.Logger, _messageBox);
                        }
                        else if (isScummVm)
                        {
                            await _mountZipFiles.MountZipFileAndLoadWithScummVmAsync(
                                resolvedFilePath, selectedSystemManager.SystemName, emulatorName,
                                selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters,
                                logPath, Log.Logger, _messageBox);
                        }
                        else
                        {
                            await _mountZipFiles.MountZipFileAndSearchForFileToLoadAsync(
                                resolvedFilePath, selectedSystemManager.SystemName, emulatorName,
                                selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters,
                                windowContext, logPath, this, Log.Logger, _messageBox);
                        }

                        loadingStateProvider?.SetLoadingState(false);
                        return;
                    }

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
                case ".XISO":
                    // Original Xbox images: Cxbx cannot read a raw ISO, so mount it as a
                    // virtual drive (drive stays mounted until the emulator exits, mirroring
                    // the WPF XisoMountStrategy). Emulators like Xemu read the ISO natively.
                    if (emulatorName.Contains("Cxbx", StringComparison.OrdinalIgnoreCase))
                    {
                        loadingStateProvider?.SetLoadingState(true, "Mounting XISO...");
                        var logPath = PathHelper.ResolveRelativeToAppDirectory(
                            _configuration.GetValue<string>("LogPath") ?? "error_user.log");
                        mountedXiso = await _mountXisoFiles.MountAsync(
                            resolvedFilePath, logPath, Log.Logger, _messageBox);
                        if (mountedXiso.IsMounted)
                        {
                            actualFilePath = mountedXiso.MountedPath;
                        }
                        else
                        {
                            loadingStateProvider?.SetLoadingState(false);
                            return;
                        }
                    }
                    // else: pass the ISO/XISO path to the emulator (may handle it directly)

                    break;

                case ".CHD":
                {
                    // Mirror of the WPF ChdMountStrategy: only emulators that cannot read
                    // .chd natively get the image mounted; RetroArch and the rest receive
                    // the raw .chd path.
                    var chdKind = GetChdGameFileKind(emulatorName, emulatorLocation);
                    if (chdKind != ChdGameFileKind.None)
                    {
                        loadingStateProvider?.SetLoadingState(true, "Mounting CHD...");
                        var consoleAlias = _mountChdFiles.GetConsoleAliasFromSystemName(
                            selectedSystemManager.SystemName, emulatorName, emulatorLocation, Log.Logger);
                        mountedChd = await _mountChdFiles.MountAsync(
                            resolvedFilePath, consoleAlias, Log.Logger, _messageBox);

                        if (!mountedChd.IsMounted)
                        {
                            // Error message already shown by MountChdFiles
                            loadingStateProvider?.SetLoadingState(false);
                            return;
                        }

                        var gameFilePath = FindGameFileInMountedChd(mountedChd.MountedPath, chdKind);
                        if (string.IsNullOrEmpty(gameFilePath))
                        {
                            Log.Warning("No game file found in mounted CHD for emulator '{Emulator}'", emulatorName);
                            await _messageBox.CustomErrorMessageBoxAsync(
                                "No suitable game file was found inside the mounted CHD image.",
                                "No Game File Found");
                            loadingStateProvider?.SetLoadingState(false);
                            return;
                        }

                        actualFilePath = gameFilePath;
                    }
                    // else: the emulator reads .chd natively — pass the path directly

                    break;
                }
            }

            // ── Run matching emulator config handlers ──
            var emulatorPath = selectedEmulatorManager.EmulatorLocation ?? "";
            var matchingHandlers = _configHandlers.Where(h => h.IsMatch(emulatorName, emulatorPath)).ToList();
            var launchParameters = rawEmulatorParameters;

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
                    Settings = _settings,
                    WindowContext = windowContext,
                    LoadingState = loadingStateProvider
                };

                foreach (var handler in matchingHandlers)
                {
                    try
                    {
                        // A handler returning false means the user cancelled (or injection
                        // failed fatally) — abort the launch, same as the WPF launcher.
                        if (!await handler.HandleConfigurationAsync(launchContext))
                        {
                            Log.Information("Emulator config handler {Handler} aborted launch for {Emulator}",
                                handler.GetType().Name, emulatorName);
                            loadingStateProvider?.SetLoadingState(false);
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Emulator config injection failed for {Emulator}", emulatorName);
                    }
                }

                // Handlers may modify Parameters (e.g. Daphne appends -framefile/-script) —
                // the modified string is what must be used for the launch.
                launchParameters = launchContext.Parameters;
            }

            // ── Emulator pre-flight checks (from the original launcher) ──
            var isRetroArch = emulatorName.Contains("RetroArch", StringComparison.OrdinalIgnoreCase);
            var isMame = emulatorName.Contains("MAME", StringComparison.OrdinalIgnoreCase);
            var isRaine = emulatorName.Contains("Raine", StringComparison.OrdinalIgnoreCase);
            var isXemu = emulatorName.Contains("Xemu", StringComparison.OrdinalIgnoreCase);

            if (isRetroArch && !launchParameters.Contains("-L", StringComparison.OrdinalIgnoreCase))
            {
                await _messageBox.CustomErrorMessageBoxAsync(
                    "RetroArch parameters must contain \"-L\" pointing to the desired core.\n\n" +
                    "Example: -L \"cores\\snes9x_libretro.dll\"",
                    "RetroArch Parameter Issue");
                loadingStateProvider?.SetLoadingState(false);
                return;
            }

            if (isXemu && ext is ".ISO" or ".XISO" or ".CHD" &&
                !launchParameters.Contains("-dvd_path", StringComparison.OrdinalIgnoreCase))
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
                launchParameters,
                selectedSystemManager.SystemFolders,
                resolvedEmulatorFolderPath,
                actualFilePath,
                romSystemFolder,
                romName);

            // ── Argument fallback (from the original launcher): when the parameter string
            // contains no ROM placeholder, append the ROM path (or bare ROM name for
            // MAME/Raine) so emulators with bare flags still receive the game.
            var containsRomPlaceholder = launchParameters.Contains("%ROM%", StringComparison.OrdinalIgnoreCase);
            var containsNamePlaceholder = launchParameters.Contains("%NAME%", StringComparison.OrdinalIgnoreCase);
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
            var stderrOutput = "";
            var stdoutOutput = "";
            var processExitCode = 0;
            var processStartTime = DateTime.Now;

            await Task.Run(async () =>
            {
                try
                {
                    var isBatchFile = Path.GetExtension(resolvedEmulatorPath)
                        .Equals(".bat", StringComparison.OrdinalIgnoreCase);
                    var stderrBuffer = new StringBuilder();
                    var stdoutBuffer = new StringBuilder();

                    using var process = new Process();
                    process.StartInfo = new ProcessStartInfo
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
                    };

                    if (!isBatchFile)
                    {
                        // Drain BOTH streams asynchronously: a chatty emulator (MAME, RetroArch
                        // with log verbosity, CLI tools) would otherwise fill the pipe buffer
                        // and block forever, deadlocking WaitForExit (mirrors the WPF launcher).
                        process.OutputDataReceived += (_, e) =>
                        {
                            if (e.Data is not null)
                            {
                                stdoutBuffer.AppendLine(e.Data);
                            }
                        };
                        process.ErrorDataReceived += (_, e) =>
                        {
                            if (e.Data is not null)
                            {
                                stderrBuffer.AppendLine(e.Data);
                            }
                        };
                    }

                    process.Start();

                    if (!isBatchFile)
                    {
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                    }

                    // WaitForExitAsync also waits for the async output reads to complete.
                    await process.WaitForExitAsync();
                    stderrOutput = stderrBuffer.ToString();
                    stdoutOutput = stdoutBuffer.ToString();
                    processExitCode = process.ExitCode;
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

            // ── Post-exit error analysis (ported from the WPF GameLauncherService) ──
            if (!string.IsNullOrWhiteSpace(stderrOutput))
            {
                Log.Debug("Emulator stderr for {Emulator}: {Stderr}", emulatorName,
                    stderrOutput.Length > 2000 ? stderrOutput[..2000] : stderrOutput);
            }

            await AnalyzeProcessExitAsync(
                processExitCode, stdoutOutput, stderrOutput,
                resolvedEmulatorPath, arguments,
                emulatorName, emulatorLocation,
                selectedEmulatorManager, selectedSystemManager,
                loadingStateProvider);

            // ── Play history & stats integration (ported from WPF) ──
            if (LastPlayTime.TotalSeconds > 5)
            {
                await UpdateStatsAndPlayCountAsync(
                    LastPlayTime, resolvedFilePath, selectedSystemManager.SystemName,
                    emulatorName, selectedEmulatorManager);
                GamePlayed?.Invoke(this, new GamePlayedEventArgs(resolvedFilePath, selectedSystemManager.SystemName));
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

            // Unmount the drives AFTER the emulator exited (kills the mount processes)
            if (mountedChd is not null)
            {
                try
                {
                    await mountedChd.DisposeAsync();
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Failed to unmount CHD drive");
                }
            }

            if (mountedXiso is not null)
            {
                try
                {
                    await mountedXiso.DisposeAsync();
                }
                catch (Exception ex)
                {
                    Log.Debug(ex, "Failed to unmount XISO drive");
                }
            }
        }
    }

    #region Standard launches

    public async Task RunBatchFileAsync(
        string resolvedFilePath,
        Emulator selectedEmulatorManager,
        IWindowContext windowContext)
    {
        Exception? error = null;
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
                error = ex;
            }
        });

        // Show the error on the UI thread (continuation of the awaited Task.Run)
        if (error is not null)
        {
            await _messageBox.ErrorLaunchingGameMessageBoxAsync(error.Message);
        }
    }

    public async Task LaunchShortcutFileAsync(
        string resolvedFilePath,
        Emulator selectedEmulatorManager,
        IWindowContext windowContext)
    {
        Exception? error = null;
        await Task.Run(() =>
        {
            try
            {
                var target = resolvedFilePath;

                // .URL files are plain-text internet shortcuts — extract the target URL
                if (Path.GetExtension(resolvedFilePath).Equals(".url", StringComparison.OrdinalIgnoreCase))
                {
                    var url = ExtractUrlFromShortcutFile(resolvedFilePath);
                    if (!string.IsNullOrEmpty(url))
                    {
                        target = url;
                    }
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
                error = ex;
            }
        });

        // Show the error on the UI thread (continuation of the awaited Task.Run)
        if (error is not null)
        {
            await _messageBox.ErrorLaunchingGameMessageBoxAsync(error.Message);
        }
    }

    public async Task LaunchExecutableAsync(
        string resolvedFilePath,
        Emulator selectedEmulatorManager,
        IWindowContext windowContext)
    {
        var startTime = DateTime.Now;
        Exception? error = null;
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
                error = ex;
            }
        });
        LastPlayTime = DateTime.Now - startTime;

        // Show the error on the UI thread (continuation of the awaited Task.Run)
        if (error is not null)
        {
            await _messageBox.ErrorLaunchingGameMessageBoxAsync(error.Message);
        }
    }

    #endregion

    #region Helpers

    /// <summary>
    /// Updates play time statistics, records play history, and reports to the stats API.
    /// Port of the WPF GameLauncherService.UpdateStatsAndPlayCountAsync.
    /// </summary>
    private async Task UpdateStatsAndPlayCountAsync(
        TimeSpan playTime, string filePath, string systemName,
        string emulatorName, Emulator selectedEmulatorManager)
    {
        // Update per-system play time in settings
        try
        {
            _settings.UpdateSystemPlayTime(systemName, playTime);
            await _settings.SaveAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating system play time in settings");
        }

        // Record play history
        try
        {
            await _playHistoryManager.RecordPlayAsync(filePath, systemName, (long)playTime.TotalSeconds);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error updating play history");
        }

        // Report to stats API (fire-and-forget)
        _ = _stats.CallApiAsync(emulatorName);
    }

    // ── Post-exit error analysis constants ──
    private const int MemoryAccessViolation = -1073741819;
    private const int DepViolation = -1073740791;

    /// <summary>
    /// Post-exit error analysis — port of the WPF GameLauncherService's
    /// DoNotCheckErrorsOnSpecificEmulators + CheckForMemoryAccessViolationAsync +
    /// CheckForDepViolationAsync + CheckForExitCodeWithErrorAnyAsync pipeline.
    /// </summary>
    private async Task AnalyzeProcessExitAsync(
        int exitCode,
        string stdout,
        string stderr,
        string emulatorPath,
        string arguments,
        string emulatorName,
        string? emulatorLocation,
        Emulator selectedEmulatorManager,
        ISystemManager selectedSystemManager,
        ILoadingState? loadingStateProvider)
    {
        // Skip error checking for known problematic emulators
        if (IsInEmulatorsToSkipList(emulatorName) || IsInEmulatorsToSkipList(Path.GetFileName(emulatorPath)))
        {
            Log.Information(
                "User just ran {Emulator}. Simple Launcher does not track error codes for this emulator.\n" +
                "Exit code: {ExitCode}, Emulator: {EmulatorPath}, Parameters: {Arguments}",
                emulatorName, exitCode, emulatorPath, arguments);
            return;
        }

        // Success — nothing to analyze
        if (exitCode == 0)
        {
            return;
        }

        // Memory access violation — log only, no user notification
        if (exitCode == MemoryAccessViolation)
        {
            Log.Warning(
                "Memory access violation error running the emulator.\n" +
                "Exit code: {ExitCode}, Emulator: {EmulatorPath}, Parameters: {Arguments}\n" +
                "Stdout: {Stdout}\nStderr: {Stderr}",
                exitCode, emulatorPath, arguments, stdout, stderr);
            return;
        }

        // DEP violation — log only, no user notification
        if (exitCode == DepViolation)
        {
            Log.Warning(
                "Data Execution Prevention (DEP) violation error running the emulator.\n" +
                "Exit code: {ExitCode}, Emulator: {EmulatorPath}, Parameters: {Arguments}\n" +
                "Stdout: {Stdout}\nStderr: {Stderr}",
                exitCode, emulatorPath, arguments, stdout, stderr);
            return;
        }

        var combinedOutput = stdout + "\n" + stderr;

        // Ignore RetroArch "File open/read error" — not actionable
        if (combinedOutput.Contains("File open/read error", StringComparison.OrdinalIgnoreCase))
        {
            Log.Debug("Ignored exit code {ExitCode} due to 'File open/read error' in output.", exitCode);
            return;
        }

        var isRetroArch = emulatorName.Contains("retroarch", StringComparison.OrdinalIgnoreCase) ||
                          (emulatorLocation ?? "").Contains("retroarch", StringComparison.OrdinalIgnoreCase);
        var isMame = emulatorName.Contains("MAME", StringComparison.OrdinalIgnoreCase) ||
                     (emulatorLocation ?? "").Contains("mame", StringComparison.OrdinalIgnoreCase) ||
                     (emulatorLocation ?? "").Contains("mame64", StringComparison.OrdinalIgnoreCase);

        // RetroArch mkdir permission denied (special characters in path)
        if (isRetroArch &&
            combinedOutput.Contains("mkdir(", StringComparison.OrdinalIgnoreCase) &&
            combinedOutput.Contains("Permission denied", StringComparison.OrdinalIgnoreCase))
        {
            Log.Debug("RetroArch mkdir permission denied due to special characters in path.");
            Log.Warning(
                "RetroArch special characters error.\n" +
                "Exit code: {ExitCode}, Emulator: {EmulatorPath}, Parameters: {Arguments}\n" +
                "Stdout: {Stdout}\nStderr: {Stderr}",
                exitCode, emulatorPath, arguments, stdout, stderr);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                await _messageBox.RetroArchSpecialCharactersInPathMessageBoxAsync();
                await _messageBox.WouldYouLikeToOpenTheLogMessageBoxAsync(LogFilePath());
                await _askAiToFixParameters.ExecuteAsync(
                    selectedSystemManager, selectedEmulatorManager, loadingStateProvider);
            }
            return;
        }

        // RetroArch generic parameter issues
        if (isRetroArch)
        {
            Log.Debug("RetroArch parameter issues detected.");
            Log.Warning(
                "RetroArch parameter issue.\n" +
                "Exit code: {ExitCode}, Emulator: {EmulatorPath}, Parameters: {Arguments}\n" +
                "Stdout: {Stdout}\nStderr: {Stderr}",
                exitCode, emulatorPath, arguments, stdout, stderr);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                await _messageBox.RetroArchParameterIssueMessageBoxAsync(LogFilePath());
                await _askAiToFixParameters.ExecuteAsync(
                    selectedSystemManager, selectedEmulatorManager, loadingStateProvider);
            }
            return;
        }

        // MAME ROM set error
        if ((isMame || isRetroArch) &&
            (combinedOutput.Contains("Not Found", StringComparison.OrdinalIgnoreCase) ||
             combinedOutput.Contains("WRONG LENGTH", StringComparison.OrdinalIgnoreCase) ||
             combinedOutput.Contains("Required files are missing", StringComparison.OrdinalIgnoreCase)))
        {
            Log.Debug("MAME ROM set error detected.");
            Log.Warning(
                "MAME ROM set error.\n" +
                "Exit code: {ExitCode}, Emulator: {EmulatorPath}, Parameters: {Arguments}\n" +
                "Stdout: {Stdout}\nStderr: {Stderr}",
                exitCode, emulatorPath, arguments, stdout, stderr);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                await _messageBox.MameRomSetErrorMessageBoxAsync();
                await _messageBox.WouldYouLikeToOpenTheLogMessageBoxAsync(LogFilePath());
                await _askAiToFixParameters.ExecuteAsync(
                    selectedSystemManager, selectedEmulatorManager, loadingStateProvider);
            }
            return;
        }

        // MAME Unknown system
        if ((isMame || isRetroArch) &&
            (combinedOutput.Contains("Unknown system", StringComparison.OrdinalIgnoreCase) ||
             combinedOutput.Contains("approximately matches the following", StringComparison.OrdinalIgnoreCase)))
        {
            Log.Debug("MAME Unknown system error detected.");
            Log.Warning(
                "MAME Unknown system error.\n" +
                "Exit code: {ExitCode}, Emulator: {EmulatorPath}, Parameters: {Arguments}\n" +
                "Stdout: {Stdout}\nStderr: {Stderr}",
                exitCode, emulatorPath, arguments, stdout, stderr);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                await _messageBox.MameUnknownSystemErrorMessageBoxAsync();
                await _messageBox.WouldYouLikeToOpenTheLogMessageBoxAsync(LogFilePath());
                await _askAiToFixParameters.ExecuteAsync(
                    selectedSystemManager, selectedEmulatorManager, loadingStateProvider);
            }
            return;
        }

        // MAME Unable to load image
        if (isMame &&
            (combinedOutput.Contains("Unable to load image", StringComparison.OrdinalIgnoreCase) ||
             combinedOutput.Contains("No such file or directory", StringComparison.OrdinalIgnoreCase)))
        {
            Log.Debug("MAME Unable to load image error detected.");
            Log.Warning(
                "MAME Unable to load image error.\n" +
                "Exit code: {ExitCode}, Emulator: {EmulatorPath}, Parameters: {Arguments}\n" +
                "Stdout: {Stdout}\nStderr: {Stderr}",
                exitCode, emulatorPath, arguments, stdout, stderr);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                await _messageBox.MameUnableToLoadImageMessageBoxAsync();
                await _messageBox.WouldYouLikeToOpenTheLogMessageBoxAsync(LogFilePath());
                await _askAiToFixParameters.ExecuteAsync(
                    selectedSystemManager, selectedEmulatorManager, loadingStateProvider);
            }
            return;
        }

        // MAME corrupted INI (auto-restore from sample)
        if (isMame &&
            stderr.Contains("Warning: unknown option in INI", StringComparison.OrdinalIgnoreCase))
        {
            Log.Debug("MAME unknown option in INI detected. Attempting to restore mame.ini from sample.");
            var restored = MameConfigurationService.RestoreMameIniFromSample(emulatorPath, Log.Logger);
            if (restored)
            {
                Log.Debug("mame.ini restored successfully. User should retry.");
            }
            else
            {
                Log.Debug("Failed to restore mame.ini from sample.");
            }
            return;
        }

        // Generic fallback — any other non-zero exit code
        Log.Debug("Exit code {ExitCode} detected for {Emulator}.", exitCode, emulatorName);
        Log.Warning(
            "Emulator exited with error.\n" +
            "Exit code: {ExitCode}, Emulator: {EmulatorPath}, Parameters: {Arguments}\n" +
            "Stdout: {Stdout}\nStderr: {Stderr}",
            exitCode, emulatorPath, arguments, stdout, stderr);

        if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
        {
            await _messageBox.CouldNotLaunchGameMessageBoxAsync(LogFilePath());
            await _askAiToFixParameters.ExecuteAsync(
                selectedSystemManager, selectedEmulatorManager, loadingStateProvider);
        }
    }

    /// <summary>
    /// Determines which launchable file to look for inside a mounted CHD image for the
    /// given emulator, and whether the image should be mounted at all. Emulators that
    /// read .chd natively (RetroArch, DuckStation, PCSX2, ...) return <see cref="ChdGameFileKind.None"/>.
    /// Mirrors the emulator gate of the WPF ChdMountStrategy.
    /// </summary>
    private static ChdGameFileKind GetChdGameFileKind(string emulatorName, string? emulatorLocation)
    {
        var loc = emulatorLocation ?? string.Empty;

        if (emulatorName.Contains("RetroArch", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("retroarch.exe", StringComparison.OrdinalIgnoreCase))
        {
            return ChdGameFileKind.None;
        }

        if (emulatorName.Contains("RPCS3", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("rpcs3", StringComparison.OrdinalIgnoreCase))
        {
            return ChdGameFileKind.EbootBin;
        }

        if (emulatorName.Contains("Xenia", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("xenia", StringComparison.OrdinalIgnoreCase))
        {
            return ChdGameFileKind.DefaultXex;
        }

        if (emulatorName.Contains("Xemu", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("xemu", StringComparison.OrdinalIgnoreCase))
        {
            return ChdGameFileKind.ImageIso;
        }

        if (emulatorName.Contains("Cxbx", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("cxbx", StringComparison.OrdinalIgnoreCase))
        {
            return ChdGameFileKind.DefaultXbe;
        }

        if (emulatorName.Contains("Gens", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("gens.exe", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("CDi", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("CD-i", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("wcdiemu", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("Kega Fusion", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("Fusion", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("fusion.exe", StringComparison.OrdinalIgnoreCase))
        {
            return ChdGameFileKind.BinFile;
        }

        if (emulatorName.Contains("Genesis Plus GX", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("gen_sdl.exe", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("4do", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("4do.exe", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("blastem", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("blastem.exe", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("FBAlpha", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("FB Alpha", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("FinalBurnAlpha", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("Final Burn Alpha", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("fba64.exe", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("FBNeo", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("FB Neo", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("FinalBurnNeo", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("Final Burn Neo", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("fbneo64.exe", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("Mednafen", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("mednafen", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("Mesen", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("Mesen.exe", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("Nebula", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("nebula.exe", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("PCSX-Redux", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("PCSX Redux", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("pcsx-redux", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("PicoDrive", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("Pico Drive", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("PicoDrive.exe", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("raine", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("raine.exe", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("Tsugaru", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("Tsugaru_CUI.exe", StringComparison.OrdinalIgnoreCase) ||
            emulatorName.Contains("Yabause", StringComparison.OrdinalIgnoreCase) ||
            loc.Contains("yabause.exe", StringComparison.OrdinalIgnoreCase))
        {
            return ChdGameFileKind.CueFile;
        }

        return ChdGameFileKind.None;
    }

    /// <summary>
    /// Locates the launchable file inside a mounted CHD drive for the given emulator
    /// (mirror of the WPF ChdMountStrategy file selection).
    /// </summary>
    private static string? FindGameFileInMountedChd(string mountedPath, ChdGameFileKind kind)
    {
        return kind switch
        {
            ChdGameFileKind.EbootBin => FindEbootBin.FindEbootBinRecursive(mountedPath, Log.Logger, Log.Logger),
            ChdGameFileKind.DefaultXex => FindDefaultXex.Find(mountedPath, Log.Logger),
            ChdGameFileKind.ImageIso => FindImageIso.Find(mountedPath, Log.Logger),
            ChdGameFileKind.DefaultXbe => FindDefaultXbe.Find(mountedPath, Log.Logger),
            ChdGameFileKind.BinFile => FindBinFile.Find(mountedPath, Log.Logger),
            ChdGameFileKind.CueFile => FindCueFile.Find(mountedPath, Log.Logger),
            _ => null
        };
    }

    /// <summary>
    /// Kinds of launchable files that can be located inside a mounted CHD image.
    /// </summary>
    private enum ChdGameFileKind
    {
        None,
        EbootBin,
        DefaultXex,
        ImageIso,
        DefaultXbe,
        BinFile,
        CueFile
    }

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