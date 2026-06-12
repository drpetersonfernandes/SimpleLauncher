using System.Diagnostics;
using System.Globalization;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.SystemManager;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

/// <summary>
/// Mounts CHD (Compressed Hunks of Data) disc images using CHDMounter.exe and the Dokan filesystem driver.
/// </summary>
public class MountChdFiles : IMountChdFiles
{
    private const string ChdMounterRelativePath = @"tools\CHDMounter\CHDMounter.exe";

    private readonly IDebugLogger _debugLogger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MountChdFiles"/> class.
    /// </summary>
    public MountChdFiles(IDebugLogger debugLogger)
    {
        _debugLogger = debugLogger;
    }

    private static HashSet<char> GetCurrentDriveLetters()
    {
        return Environment.GetLogicalDrives()
            .Select(static d => char.ToUpper(d[0], CultureInfo.InvariantCulture))
            .ToHashSet();
    }

    /// <summary>
    /// Mounts a CHD file and returns a disposable drive handle with the mounted path and drive letter.
    /// </summary>
    public async Task<MountChdDrive> MountAsync(string resolvedChdFilePath, int? consoleIndex, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
    {
        _debugLogger.Log($"[MountChdFiles.MountAsync] Starting to mount CHD: {resolvedChdFilePath} (ConsoleIndex: {consoleIndex?.ToString(CultureInfo.InvariantCulture) ?? "default"})");

        var resolvedToolPath = PathHelper.ResolveRelativeToAppDirectory(ChdMounterRelativePath);

        _debugLogger.Log($"[MountChdFiles.MountAsync] Path to CHDMounter: {resolvedToolPath}");

        if (string.IsNullOrWhiteSpace(resolvedToolPath) || !File.Exists(resolvedToolPath))
        {
            const string errorMessage = $"CHDMounter.exe not found at {ChdMounterRelativePath}. Cannot mount CHD.";
            _debugLogger.Log($"[MountChdFiles.MountAsync] Error: {errorMessage}");
            logErrors.LogAndForget(null, errorMessage);
            await messageBox.ThereWasAnErrorMountingTheFileMessageBox();
            return new MountChdDrive(logErrors, _debugLogger);
        }

        if (!DokanValidation.IsDokanInstalled())
        {
            const string errorMessage = "Dokan driver not found. Cannot mount CHD.";
            _debugLogger.Log($"[MountChdFiles.MountAsync] Error: {errorMessage}");
            logErrors.LogAndForget(null, errorMessage);
            await messageBox.DokanDriverNotInstalledMessageBox();
            return new MountChdDrive(logErrors, _debugLogger);
        }

        var existingDrives = GetCurrentDriveLetters();
        _debugLogger.Log($"[MountChdFiles.MountAsync] Existing drives before mount: {string.Join(", ", existingDrives)}");

        var arguments = consoleIndex.HasValue
            ? $"/a \"{resolvedChdFilePath}\" /s:{consoleIndex.Value}"
            : $"/a \"{resolvedChdFilePath}\"";

        var psiMount = new ProcessStartInfo
        {
            FileName = resolvedToolPath,
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(resolvedToolPath) ?? AppDomain.CurrentDomain.BaseDirectory
        };

        _debugLogger.Log("[MountChdFiles.MountAsync] Using auto-select drive letter (/a)");
        _debugLogger.Log($"[MountChdFiles.MountAsync] Arguments: {psiMount.Arguments}");

        var mountProcess = new Process { StartInfo = psiMount, EnableRaisingEvents = true };

        try
        {
            if (!mountProcess.Start())
            {
                throw new InvalidOperationException("Failed to start the CHDMounter process.");
            }

            _debugLogger.Log($"[MountChdFiles.MountAsync] CHDMounter process started (ID: {mountProcess.Id}).");

            var (mountSuccessful, detectedDrive, exitCode) = await WaitForDriveMountAndDetectAsync(existingDrives, mountProcess, mountProcess.Id, logErrors);

            if (!mountSuccessful || detectedDrive == null)
            {
                if (!mountProcess.HasExited)
                {
                    mountProcess.Kill(true);
                }

                mountProcess.Dispose();
                await messageBox.ThereWasAnErrorMountingTheFileMessageBox(exitCode);
                return new MountChdDrive(logErrors, _debugLogger);
            }

            var driveRoot = $"{detectedDrive.Value}:\\";
            _debugLogger.Log($"[MountChdFiles.MountAsync] CHD mounted successfully. Drive: {driveRoot}");
            return new MountChdDrive(mountProcess, driveRoot, detectedDrive.Value.ToString(), logErrors, _debugLogger);
        }
        catch (Exception ex)
        {
            _debugLogger.Log($"[MountChdFiles.MountAsync] Exception during CHD mounting: {ex}");
            var contextMessage = $"Error during CHD mount process for {resolvedChdFilePath}.\nException: {ex.Message}";
            logErrors.LogAndForget(ex, contextMessage);

            if (!mountProcess.HasExited)
            {
                try
                {
                    mountProcess.Kill(true);
                }
                catch
                {
                    /* ignore */
                }
            }

            mountProcess.Dispose();

            await messageBox.ThereWasAnErrorMountingTheFileMessageBox();
            return new MountChdDrive(logErrors, _debugLogger);
        }
    }

    /// <summary>
    /// Mounts a CHD file, locates a game file within the mounted drive, launches the emulator, and unmounts on exit.
    /// </summary>
    public async Task MountChdFileAndLoadAsync(
        string resolvedChdFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        ISystemManager selectedSystemManager,
        Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        IWindowContext windowContext,
        ILauncherService gameLauncher,
        ILogErrors logErrors,
        IMessageBoxLibraryService messageBox)
    {
        _debugLogger.Log($"[MountChdFiles] Starting to mount CHD for game loading: {resolvedChdFilePath}");
        _debugLogger.Log($"[MountChdFiles] System: {selectedSystemName}, Emulator: {selectedEmulatorName}");

        var resolvedToolPath = PathHelper.ResolveRelativeToAppDirectory(ChdMounterRelativePath);

        _debugLogger.Log($"[MountChdFiles] Path to CHDMounter: {resolvedToolPath}");

        if (string.IsNullOrWhiteSpace(resolvedToolPath) || !File.Exists(resolvedToolPath))
        {
            const string errorMessage = $"CHDMounter.exe not found at {ChdMounterRelativePath}. Cannot mount CHD.";
            _debugLogger.Log($"[MountChdFiles] Error: {errorMessage}");
            logErrors.LogAndForget(null, errorMessage);
            await messageBox.ThereWasAnErrorMountingTheFileMessageBox();
            return;
        }

        if (!DokanValidation.IsDokanInstalled())
        {
            const string errorMessage = "Dokan driver not found. Cannot mount CHD.";
            _debugLogger.Log($"[MountChdFiles] Error: {errorMessage}");
            logErrors.LogAndForget(null, errorMessage);
            await messageBox.DokanDriverNotInstalledMessageBox();
            return;
        }

        var existingDrives = GetCurrentDriveLetters();
        _debugLogger.Log($"[MountChdFiles] Existing drives before mount: {string.Join(", ", existingDrives)}");

        var psiMount = new ProcessStartInfo
        {
            FileName = resolvedToolPath,
            Arguments = $"/a \"{resolvedChdFilePath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(resolvedToolPath) ?? AppDomain.CurrentDomain.BaseDirectory
        };

        _debugLogger.Log("[MountChdFiles] ProcessStartInfo:");
        _debugLogger.Log($"[MountChdFiles] FileName: {psiMount.FileName}");
        _debugLogger.Log($"[MountChdFiles] Arguments: {psiMount.Arguments}");
        _debugLogger.Log($"[MountChdFiles] WorkingDirectory: {psiMount.WorkingDirectory}");

        Process mountProcess = null;
        var mountProcessId = -1;
        string driveRoot = null;

        try
        {
            mountProcess = new Process { StartInfo = psiMount, EnableRaisingEvents = true };

            _debugLogger.Log("[MountChdFiles] Starting CHDMounter process...");
            if (!mountProcess.Start())
            {
                throw new InvalidOperationException("Failed to start the CHDMounter process.");
            }

            mountProcessId = mountProcess.Id;
            _debugLogger.Log($"[MountChdFiles] CHDMounter process started (ID: {mountProcessId}).");

            var (mountSuccessful, drive, exitCode) = await WaitForDriveMountAndDetectAsync(existingDrives, mountProcess, mountProcessId, logErrors);

            if (!mountSuccessful || drive == null)
            {
                if (!mountProcess.HasExited)
                {
                    mountProcess.Kill(true);
                }

                mountProcess.Dispose();
                await messageBox.ThereWasAnErrorMountingTheFileMessageBox(exitCode);
                return;
            }

            driveRoot = $"{drive.Value}:\\";
            _debugLogger.Log($"[MountChdFiles] CHD mounted successfully on drive {drive.Value}:. Searching for game file...");

            var gameFilePath = FindGameFile(driveRoot, logErrors);

            if (string.IsNullOrEmpty(gameFilePath))
            {
                _debugLogger.Log($"[MountChdFiles] No suitable game file found in {driveRoot}.");
                throw new FileNotFoundException($"Could not find a game file within the mounted CHD at {driveRoot}.");
            }

            _debugLogger.Log($"[MountChdFiles] Game file found at: {gameFilePath}. Proceeding to launch with {selectedEmulatorName}.");

            await gameLauncher.LaunchRegularEmulatorAsync(gameFilePath, selectedEmulatorName, selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters, windowContext, null, resolvedChdFilePath);

            _debugLogger.Log($"[MountChdFiles] Emulator for {gameFilePath} has exited.");
        }
        catch (Exception ex)
        {
            _debugLogger.Log($"[MountChdFiles] Exception during CHD mounting or launching: {ex}");
            var exitCodeInfoInCatch = mountProcess is { HasExited: true } ? $"Exit Code: {mountProcess.ExitCode}" : "Process was still running or state unknown.";
            var contextMessage = $"Error during CHD mount/launch process for {resolvedChdFilePath}.\n" +
                                 $"Exception: {ex.Message}\n" +
                                 $"{exitCodeInfoInCatch}";
            logErrors.LogAndForget(ex, contextMessage);

            await messageBox.ThereWasAnErrorMountingTheFileMessageBox();
        }
        finally
        {
            _debugLogger.Log($"[MountChdFiles] Entering finally block for {resolvedChdFilePath}. Mount Process ID: {mountProcessId}");
            if (mountProcess != null && mountProcessId != -1 && !mountProcess.HasExited)
            {
                _debugLogger.Log($"[MountChdFiles] Attempting to unmount by terminating CHDMounter (ID: {mountProcessId}).");
                try
                {
                    mountProcess.Kill(true);
                    _debugLogger.Log($"[MountChdFiles] Kill signal sent to CHDMounter (ID: {mountProcessId}). Waiting for process to exit (up to 20s).");
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                    try
                    {
                        await mountProcess.WaitForExitAsync(cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        _debugLogger.Log($"[MountChdFiles] Timeout (10s) waiting for CHDMounter (ID: {mountProcessId}) to exit after Kill.");
                    }

                    if (mountProcess.HasExited)
                    {
                        _debugLogger.Log($"[MountChdFiles] CHDMounter (ID: {mountProcessId}) terminated. Exit code: {mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture)}.");
                    }
                    else
                    {
                        _debugLogger.Log($"[MountChdFiles] CHDMounter (ID: {mountProcessId}) did NOT terminate after Kill signal and 10s wait.");
                    }
                }
                catch (InvalidOperationException ioEx)
                {
                    if (ioEx.Message.Contains("process has already exited", StringComparison.OrdinalIgnoreCase) ||
                        ioEx.Message.Contains("No process is associated", StringComparison.OrdinalIgnoreCase))
                    {
                        _debugLogger.Log($"[MountChdFiles] CHDMounter (ID: {mountProcessId}) already exited: {ioEx.Message}");
                    }
                    else
                    {
                        _debugLogger.Log($"[MountChdFiles] InvalidOperationException while terminating CHDMounter (ID: {mountProcessId}): {ioEx}");
                        logErrors.LogAndForget(ioEx, "Unexpected InvalidOperationException during CHDMounter termination.");
                    }
                }
                catch (Exception termEx)
                {
                    _debugLogger.Log($"[MountChdFiles] Exception while terminating CHDMounter (ID: {mountProcessId}): {termEx}");
                    logErrors.LogAndForget(termEx, $"Failed to terminate CHDMounter (ID: {mountProcessId}) for unmounting.");
                }
            }
            else if (mountProcessId != -1)
            {
                var exitCodeStr = mountProcess is { HasExited: true } ? mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A";
                _debugLogger.Log($"[MountChdFiles] CHDMounter (ID: {mountProcessId}) had already exited. Exit code likely {exitCodeStr}.");
            }
            else
            {
                _debugLogger.Log("[MountChdFiles] CHDMounter process was not started successfully. No termination needed.");
            }

            mountProcess?.Dispose();

            await Task.Delay(2000);
            if (Directory.Exists(driveRoot))
            {
                _debugLogger.Log($"[MountChdFiles] WARNING: Drive {driveRoot} still exists after attempting to unmount.");
            }
            else
            {
                _debugLogger.Log($"[MountChdFiles] Drive {driveRoot} successfully unmounted.");
            }
        }
    }

    /// <summary>
    /// Mounts a CHD file with an explicit console index, locates a game file, launches the emulator, and unmounts on exit.
    /// </summary>
    public async Task MountChdFileAndLoadWithConsoleIndexAsync(
        string resolvedChdFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        ISystemManager selectedSystemManager,
        Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        IWindowContext windowContext,
        ILauncherService gameLauncher,
        int? consoleIndex,
        ILogErrors logErrors,
        IMessageBoxLibraryService messageBox)
    {
        _debugLogger.Log($"[MountChdFiles] Starting to mount CHD with console index for game loading: {resolvedChdFilePath}");
        _debugLogger.Log($"[MountChdFiles] System: {selectedSystemName}, Emulator: {selectedEmulatorName}, ConsoleIndex: {consoleIndex?.ToString(CultureInfo.InvariantCulture) ?? "auto"}");

        var resolvedToolPath = PathHelper.ResolveRelativeToAppDirectory(ChdMounterRelativePath);

        _debugLogger.Log($"[MountChdFiles] Path to CHDMounter: {resolvedToolPath}");

        if (string.IsNullOrWhiteSpace(resolvedToolPath) || !File.Exists(resolvedToolPath))
        {
            const string errorMessage = $"CHDMounter.exe not found at {ChdMounterRelativePath}. Cannot mount CHD.";
            _debugLogger.Log($"[MountChdFiles] Error: {errorMessage}");
            logErrors.LogAndForget(null, errorMessage);
            await messageBox.ThereWasAnErrorMountingTheFileMessageBox();
            return;
        }

        if (!DokanValidation.IsDokanInstalled())
        {
            const string errorMessage = "Dokan driver not found. Cannot mount CHD.";
            _debugLogger.Log($"[MountChdFiles] Error: {errorMessage}");
            logErrors.LogAndForget(null, errorMessage);
            await messageBox.DokanDriverNotInstalledMessageBox();
            return;
        }

        var existingDrives = GetCurrentDriveLetters();
        _debugLogger.Log($"[MountChdFiles] Existing drives before mount: {string.Join(", ", existingDrives)}");

        var arguments = consoleIndex.HasValue
            ? $"\"{resolvedChdFilePath}\" /a /s:{consoleIndex.Value}"
            : $"\"{resolvedChdFilePath}\" /a";

        var psiMount = new ProcessStartInfo
        {
            FileName = resolvedToolPath,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            WorkingDirectory = Path.GetDirectoryName(resolvedToolPath) ?? AppDomain.CurrentDomain.BaseDirectory
        };

        _debugLogger.Log("[MountChdFiles] ProcessStartInfo:");
        _debugLogger.Log($"[MountChdFiles] FileName: {psiMount.FileName}");
        _debugLogger.Log($"[MountChdFiles] Arguments: {psiMount.Arguments}");
        _debugLogger.Log($"[MountChdFiles] WorkingDirectory: {psiMount.WorkingDirectory}");

        Process mountProcess = null;
        var mountProcessId = -1;
        string driveRoot = null;

        try
        {
            mountProcess = new Process { StartInfo = psiMount, EnableRaisingEvents = true };

            _debugLogger.Log("[MountChdFiles] Starting CHDMounter process...");
            if (!mountProcess.Start())
            {
                throw new InvalidOperationException("Failed to start the CHDMounter process.");
            }

            mountProcessId = mountProcess.Id;
            _debugLogger.Log($"[MountChdFiles] CHDMounter process started (ID: {mountProcessId}).");

            var (mountSuccessful, drive, exitCode) = await WaitForDriveMountAndDetectAsync(existingDrives, mountProcess, mountProcessId, logErrors);

            if (!mountSuccessful || drive == null)
            {
                if (!mountProcess.HasExited)
                {
                    mountProcess.Kill(true);
                }

                mountProcess.Dispose();
                await messageBox.ThereWasAnErrorMountingTheFileMessageBox(exitCode);
                return;
            }

            driveRoot = $"{drive.Value}:\\";
            _debugLogger.Log($"[MountChdFiles] CHD mounted successfully on drive {drive.Value}:. Searching for game file...");

            var gameFilePath = FindGameFile(driveRoot, logErrors);

            if (string.IsNullOrEmpty(gameFilePath))
            {
                _debugLogger.Log($"[MountChdFiles] No suitable game file found in {driveRoot}.");
                throw new FileNotFoundException($"Could not find a game file within the mounted CHD at {driveRoot}.");
            }

            _debugLogger.Log($"[MountChdFiles] Game file found at: {gameFilePath}. Proceeding to launch with {selectedEmulatorName}.");

            await gameLauncher.LaunchRegularEmulatorAsync(gameFilePath, selectedEmulatorName, selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters, windowContext, null, resolvedChdFilePath);

            _debugLogger.Log($"[MountChdFiles] Emulator for {gameFilePath} has exited.");
        }
        catch (Exception ex)
        {
            _debugLogger.Log($"[MountChdFiles] Exception during CHD mounting or launching: {ex}");
            var exitCodeInfoInCatch = mountProcess is { HasExited: true } ? $"Exit Code: {mountProcess.ExitCode}" : "Process was still running or state unknown.";
            var contextMessage = $"Error during CHD mount/launch process for {resolvedChdFilePath}.\n" +
                                 $"Exception: {ex.Message}\n" +
                                 $"{exitCodeInfoInCatch}";
            logErrors.LogAndForget(ex, contextMessage);

            await messageBox.ThereWasAnErrorMountingTheFileMessageBox();
        }
        finally
        {
            _debugLogger.Log($"[MountChdFiles] Entering finally block for {resolvedChdFilePath}. Mount Process ID: {mountProcessId}");
            if (mountProcess != null && mountProcessId != -1 && !mountProcess.HasExited)
            {
                _debugLogger.Log($"[MountChdFiles] Attempting to unmount by terminating CHDMounter (ID: {mountProcessId}).");
                try
                {
                    mountProcess.Kill(true);
                    _debugLogger.Log($"[MountChdFiles] Kill signal sent to CHDMounter (ID: {mountProcessId}). Waiting for process to exit (up to 20s).");
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
                    try
                    {
                        await mountProcess.WaitForExitAsync(cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        _debugLogger.Log($"[MountChdFiles] Timeout (10s) waiting for CHDMounter (ID: {mountProcessId}) to exit after Kill.");
                    }

                    if (mountProcess.HasExited)
                    {
                        _debugLogger.Log($"[MountChdFiles] CHDMounter (ID: {mountProcessId}) terminated. Exit code: {mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture)}.");
                    }
                    else
                    {
                        _debugLogger.Log($"[MountChdFiles] CHDMounter (ID: {mountProcessId}) did NOT terminate after Kill signal and 10s wait.");
                    }
                }
                catch (InvalidOperationException ioEx)
                {
                    if (ioEx.Message.Contains("process has already exited", StringComparison.OrdinalIgnoreCase) ||
                        ioEx.Message.Contains("No process is associated", StringComparison.OrdinalIgnoreCase))
                    {
                        _debugLogger.Log($"[MountChdFiles] CHDMounter (ID: {mountProcessId}) already exited: {ioEx.Message}");
                    }
                    else
                    {
                        _debugLogger.Log($"[MountChdFiles] InvalidOperationException while terminating CHDMounter (ID: {mountProcessId}): {ioEx}");
                        logErrors.LogAndForget(ioEx, "Unexpected InvalidOperationException during CHDMounter termination.");
                    }
                }
                catch (Exception termEx)
                {
                    _debugLogger.Log($"[MountChdFiles] Exception while terminating CHDMounter (ID: {mountProcessId}): {termEx}");
                    logErrors.LogAndForget(termEx, $"Failed to terminate CHDMounter (ID: {mountProcessId}) for unmounting.");
                }
            }
            else if (mountProcessId != -1)
            {
                var exitCodeStr = mountProcess is { HasExited: true } ? mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A";
                _debugLogger.Log($"[MountChdFiles] CHDMounter (ID: {mountProcessId}) had already exited. Exit code likely {exitCodeStr}.");
            }
            else
            {
                _debugLogger.Log("[MountChdFiles] CHDMounter process was not started successfully. No termination needed.");
            }

            mountProcess?.Dispose();

            await Task.Delay(2000);
            if (Directory.Exists(driveRoot))
            {
                _debugLogger.Log($"[MountChdFiles] WARNING: Drive {driveRoot} still exists after attempting to unmount.");
            }
            else
            {
                _debugLogger.Log($"[MountChdFiles] Drive {driveRoot} successfully unmounted.");
            }
        }
    }

    /// <summary>
    /// Determines the CHDMounter console index for a given system name and emulator name.
    /// </summary>
    public int? GetConsoleIndexFromSystemName(string systemName, string emulatorName, ILogErrors logErrors)
    {
        if (string.IsNullOrEmpty(systemName))
        {
            return null;
        }

        if ((systemName.Contains("AMIGA CD", StringComparison.OrdinalIgnoreCase) ||
             systemName.Contains("AMIGACD", StringComparison.OrdinalIgnoreCase)) &&
            !systemName.Contains("CD32", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        else if (systemName.Contains("AMIGA CD32", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("AMIGACD32", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("CD32", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        else if (systemName.Contains("CD-I", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("CDI", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("PHILIPS CDI", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("PHILIPSCDI", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        else if (systemName.Contains("DREAMCAST", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("SEGA DREAMCAST", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        else if (systemName.Contains("FM Towns", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("FMTowns", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        else if (systemName.Contains("NEOGEO CD", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("NEO GEO CD", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        else if (systemName.Contains("PCE-CD", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("PC ENGINE CD", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        else if (systemName.Contains("PC-FX", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("PCFX", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        else if ((systemName.Contains("PS1", StringComparison.OrdinalIgnoreCase) ||
                  systemName.Contains("PSX1", StringComparison.OrdinalIgnoreCase) ||
                  systemName.Contains("PSX 1", StringComparison.OrdinalIgnoreCase) ||
                  systemName.Contains("PLAY 1", StringComparison.OrdinalIgnoreCase) ||
                  systemName.Contains("PLAYSTATION 1", StringComparison.OrdinalIgnoreCase) ||
                  systemName.Contains("PLAYSTATION", StringComparison.OrdinalIgnoreCase)) &&
                 !systemName.Contains('2') &&
                 !systemName.Contains('3'))
        {
            return 20;
        }

        else if (systemName.Contains("PS2", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("PSX2", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("PSX 2", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("PLAY 2", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("PLAYSTATION 2", StringComparison.OrdinalIgnoreCase))
        {
            return 9;
        }

        else if (systemName.Contains("PS3", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("PSX3", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("PSX 3", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("PLAY 3", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("PLAYSTATION 3", StringComparison.OrdinalIgnoreCase))
        {
            return 10;
        }

        else if (systemName.Contains("PSP", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("PLAYSTATION PORTABLE", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("SONY PSP", StringComparison.OrdinalIgnoreCase))
        {
            return 11;
        }

        else if (systemName.Contains("SATURN", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("SEGA SATURN", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        else if (systemName.Contains("GENESIS CD", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("SEGA CD", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("MEGA CD", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        else if (systemName.Contains("3DO", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("PANASONIC 3DO", StringComparison.OrdinalIgnoreCase))
        {
            return 21;
        }

        else if (systemName.Contains("XBOX", StringComparison.OrdinalIgnoreCase) &&
                 !systemName.Contains("360"))
        {
            if (!string.IsNullOrEmpty(emulatorName) &&
                emulatorName.Contains("xemu", StringComparison.OrdinalIgnoreCase))
            {
                return 18;
            }

            return 17;
        }

        else if (systemName.Contains("XBOX 360", StringComparison.OrdinalIgnoreCase) ||
                 systemName.Contains("XBOX360", StringComparison.OrdinalIgnoreCase))
        {
            return 17;
        }

        else if (!string.IsNullOrEmpty(emulatorName) &&
                 emulatorName.Contains("RAINE", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        else
        {
            return 20;
        }
    }

    private async Task<(bool Success, char? DriveLetter, int? ExitCode)> WaitForDriveMountAndDetectAsync(HashSet<char> existingDrives, Process mountProcess, int processId, ILogErrors logErrors)
    {
        const int maxRetries = 240;
        const int pollIntervalMs = 500;
        var retryCount = 0;

        _debugLogger.Log($"[MountChdFiles.WaitForDriveMountAndDetectAsync] Polling for new drive to appear (max {maxRetries * pollIntervalMs / 1000}s)...");

        while (retryCount < maxRetries)
        {
            if (mountProcess.HasExited)
            {
                var exitCode = mountProcess.ExitCode;
                var contextMessage = exitCode == -1073741515
                    ? $"Failed to mount CHD. The CHDMounter tool exited prematurely with code {exitCode} (STATUS_DLL_NOT_FOUND). This indicates that the Dokan library is not installed."
                    : $"Failed to mount CHD. The CHDMounter tool exited prematurely with code {exitCode}.";

                _debugLogger.Log($"[MountChdFiles.WaitForDriveMountAndDetectAsync] CHDMounter process (ID: {processId}) exited prematurely during polling. {contextMessage}");
                logErrors.LogAndForget(null, contextMessage);
                return (false, null, exitCode);
            }

            var currentDrives = GetCurrentDriveLetters();
            var newDrives = currentDrives.Except(existingDrives).ToList();

            if (newDrives.Count > 0)
            {
                var detectedDrive = newDrives[0];
                _debugLogger.Log($"[MountChdFiles.WaitForDriveMountAndDetectAsync] Found new drive '{detectedDrive}:' after {retryCount * pollIntervalMs / 1000.0:F1} seconds. Mount successful!");
                return (true, detectedDrive, null);
            }

            retryCount++;
            await Task.Delay(pollIntervalMs);
        }

        _debugLogger.Log($"[MountChdFiles.WaitForDriveMountAndDetectAsync] Timed out waiting for new drive after {maxRetries * pollIntervalMs / 1000} seconds.");
        const string timeoutContextMessage = "Timed out waiting for the CHD to mount. No new drive detected.";
        logErrors.LogAndForget(null, timeoutContextMessage);
        return (false, null, null);
    }

    private string FindGameFile(string driveRoot, ILogErrors logErrors)
    {
        try
        {
            _debugLogger.Log($"[MountChdFiles.FindGameFile] Searching for game file in {driveRoot}...");

            if (!Directory.Exists(driveRoot))
            {
                _debugLogger.Log($"[MountChdFiles.FindGameFile] Drive {driveRoot} does not exist.");
                return null;
            }

            var allFiles = Directory.GetFiles(driveRoot, "*", SearchOption.AllDirectories);
            if (allFiles.Length == 0)
            {
                _debugLogger.Log($"[MountChdFiles.FindGameFile] No files found in {driveRoot}.");
                return null;
            }

            _debugLogger.Log($"[MountChdFiles.FindGameFile] Found {allFiles.Length} files in {driveRoot}.");

            foreach (var file in allFiles)
            {
                _debugLogger.Log($"[MountChdFiles.FindGameFile] Found: {file}");
            }

            return allFiles[0];
        }
        catch (Exception ex)
        {
            _debugLogger.Log($"[MountChdFiles.FindGameFile] Error searching for game file in {driveRoot}: {ex.Message}");
            logErrors.LogAndForget(ex, $"Error in FindGameFile searching {driveRoot}");
            return null;
        }
    }

    /// <summary>
    /// Terminates all running CHDMounter processes to ensure clean unmounting.
    /// </summary>
    public void KillAllChdMounterProcesses(ILogErrors logErrors)
    {
        try
        {
            var processes = Process.GetProcessesByName("CHDMounter");
            if (processes.Length == 0)
            {
                _debugLogger.Log("[MountChdFiles.KillAllChdMounterProcesses] No CHDMounter processes found.");
                return;
            }

            _debugLogger.Log($"[MountChdFiles.KillAllChdMounterProcesses] Found {processes.Length} CHDMounter process(es) to kill.");

            foreach (var process in processes)
            {
                try
                {
                    if (!process.HasExited)
                    {
                        _debugLogger.Log($"[MountChdFiles.KillAllChdMounterProcesses] Killing CHDMounter (ID: {process.Id}).");
                        process.Kill(true);
                        process.WaitForExit(5000);
                        _debugLogger.Log($"[MountChdFiles.KillAllChdMounterProcesses] CHDMounter (ID: {process.Id}) terminated. Exit code: {process.ExitCode}.");
                    }
                }
                catch (Exception ex)
                {
                    _debugLogger.Log($"[MountChdFiles.KillAllChdMounterProcesses] Failed to kill CHDMounter (ID: {process.Id}): {ex.Message}");
                }
                finally
                {
                    process.Dispose();
                }
            }
        }
        catch (Exception ex)
        {
            _debugLogger.Log($"[MountChdFiles.KillAllChdMounterProcesses] Error enumerating CHDMounter processes: {ex.Message}");
        }
    }
}
