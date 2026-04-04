using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher.Services.MountFiles;

public static class MountChdFiles
{
    private const string ChdMounterRelativePath = @"tools\CHDMounter\CHDMounter.exe";

    /// <summary>
    /// Gets the current logical drives as a hash set for comparison.
    /// </summary>
    private static HashSet<char> GetCurrentDriveLetters()
    {
        return Environment.GetLogicalDrives()
            .Select(static d => char.ToUpper(d[0], CultureInfo.InvariantCulture))
            .ToHashSet();
    }

    /// <summary>
    /// Mounts a CHD file to an available drive letter using CHDMounter.exe.
    /// Uses /a flag to let CHDMounter auto-select the drive letter, then detects which drive was mounted.
    /// </summary>
    /// <param name="resolvedChdFilePath">The full path to the CHD file.</param>
    /// <param name="logPath">Path to the application's log file for error reporting.</param>
    /// <param name="consoleIndex">Optional console index for CHDMounter (1-17).</param>
    /// <returns>A disposable MountChdDrive object that manages the mount process.</returns>
    public static async Task<MountChdDrive> MountAsync(string resolvedChdFilePath, string logPath, int? consoleIndex = null)
    {
        DebugLogger.Log($"[MountChdFiles.MountAsync] Starting to mount CHD: {resolvedChdFilePath} (ConsoleIndex: {consoleIndex?.ToString(CultureInfo.InvariantCulture) ?? "default"})");

        var resolvedToolPath = PathHelper.ResolveRelativeToAppDirectory(ChdMounterRelativePath);

        DebugLogger.Log($"[MountChdFiles.MountAsync] Path to CHDMounter: {resolvedToolPath}");

        if (string.IsNullOrWhiteSpace(resolvedToolPath) || !File.Exists(resolvedToolPath))
        {
            const string errorMessage = $"CHDMounter.exe not found at {ChdMounterRelativePath}. Cannot mount CHD.";
            DebugLogger.Log($"[MountChdFiles.MountAsync] Error: {errorMessage}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, errorMessage);
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile();
            return new MountChdDrive();
        }

        // Capture existing drives before mounting
        var existingDrives = GetCurrentDriveLetters();
        DebugLogger.Log($"[MountChdFiles.MountAsync] Existing drives before mount: {string.Join(", ", existingDrives)}");

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

        DebugLogger.Log("[MountChdFiles.MountAsync] Using auto-select drive letter (/a)");
        DebugLogger.Log($"[MountChdFiles.MountAsync] Arguments: {psiMount.Arguments}");

        var mountProcess = new Process { StartInfo = psiMount, EnableRaisingEvents = true };

        try
        {
            if (!mountProcess.Start())
            {
                throw new InvalidOperationException("Failed to start the CHDMounter process.");
            }

            DebugLogger.Log($"[MountChdFiles.MountAsync] CHDMounter process started (ID: {mountProcess.Id}).");

            var (mountSuccessful, detectedDrive) = await WaitForDriveMountAndDetectAsync(existingDrives, mountProcess, mountProcess.Id);

            if (!mountSuccessful || detectedDrive == null)
            {
                if (!mountProcess.HasExited)
                {
                    mountProcess.Kill(true);
                }

                mountProcess.Dispose();
                MessageBoxLibrary.ThereWasAnErrorMountingTheFile();
                return new MountChdDrive();
            }

            var driveRoot = $"{detectedDrive.Value}:\\";
            DebugLogger.Log($"[MountChdFiles.MountAsync] CHD mounted successfully. Drive: {driveRoot}");
            return new MountChdDrive(mountProcess, driveRoot, detectedDrive.Value.ToString());
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[MountChdFiles.MountAsync] Exception during CHD mounting: {ex}");
            var contextMessage = $"Error during CHD mount process for {resolvedChdFilePath}.\nException: {ex.Message}";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

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

            MessageBoxLibrary.ThereWasAnErrorMountingTheFile();
            return new MountChdDrive();
        }
    }

    /// <summary>
    /// Mounts a CHD file and finds the executable file to load.
    /// </summary>
    /// <param name="resolvedChdFilePath">The full path to the CHD file.</param>
    /// <param name="selectedSystemName">The selected system name.</param>
    /// <param name="selectedEmulatorName">The selected emulator name.</param>
    /// <param name="selectedSystemManager">The system manager.</param>
    /// <param name="selectedEmulatorManager">The emulator manager.</param>
    /// <param name="rawEmulatorParameters">Raw emulator parameters.</param>
    /// <param name="mainWindow">The main window.</param>
    /// <param name="logPath">Path to the application's log file.</param>
    /// <param name="gameLauncher">The game launcher instance.</param>
    public static async Task MountChdFileAndLoadAsync(
        string resolvedChdFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        SystemManager.SystemManager selectedSystemManager,
        SystemManager.Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        MainWindow mainWindow,
        string logPath,
        GameLauncher.GameLauncher gameLauncher)
    {
        DebugLogger.Log($"[MountChdFiles] Starting to mount CHD for game loading: {resolvedChdFilePath}");
        DebugLogger.Log($"[MountChdFiles] System: {selectedSystemName}, Emulator: {selectedEmulatorName}");

        var resolvedToolPath = PathHelper.ResolveRelativeToAppDirectory(ChdMounterRelativePath);

        DebugLogger.Log($"[MountChdFiles] Path to CHDMounter: {resolvedToolPath}");

        if (string.IsNullOrWhiteSpace(resolvedToolPath) || !File.Exists(resolvedToolPath))
        {
            const string errorMessage = $"CHDMounter.exe not found at {ChdMounterRelativePath}. Cannot mount CHD.";
            DebugLogger.Log($"[MountChdFiles] Error: {errorMessage}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, errorMessage);
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile();
            return;
        }

        // Capture existing drives before mounting
        var existingDrives = GetCurrentDriveLetters();
        DebugLogger.Log($"[MountChdFiles] Existing drives before mount: {string.Join(", ", existingDrives)}");

        var psiMount = new ProcessStartInfo
        {
            FileName = resolvedToolPath,
            Arguments = $"/a \"{resolvedChdFilePath}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(resolvedToolPath) ?? AppDomain.CurrentDomain.BaseDirectory
        };

        DebugLogger.Log("[MountChdFiles] ProcessStartInfo:");
        DebugLogger.Log($"[MountChdFiles] FileName: {psiMount.FileName}");
        DebugLogger.Log($"[MountChdFiles] Arguments: {psiMount.Arguments}");
        DebugLogger.Log($"[MountChdFiles] WorkingDirectory: {psiMount.WorkingDirectory}");

        Process mountProcess = null;
        var mountProcessId = -1;
        string driveRoot = null;

        try
        {
            mountProcess = new Process { StartInfo = psiMount, EnableRaisingEvents = true };

            DebugLogger.Log("[MountChdFiles] Starting CHDMounter process...");
            if (!mountProcess.Start())
            {
                throw new InvalidOperationException("Failed to start the CHDMounter process.");
            }

            mountProcessId = mountProcess.Id;
            DebugLogger.Log($"[MountChdFiles] CHDMounter process started (ID: {mountProcessId}).");

            var (mountSuccessful, drive) = await WaitForDriveMountAndDetectAsync(existingDrives, mountProcess, mountProcessId);

            if (!mountSuccessful || drive == null)
            {
                if (!mountProcess.HasExited)
                {
                    mountProcess.Kill(true);
                }

                mountProcess.Dispose();
                MessageBoxLibrary.ThereWasAnErrorMountingTheFile();
                return;
            }

            driveRoot = $"{drive.Value}:\\";
            DebugLogger.Log($"[MountChdFiles] CHD mounted successfully on drive {drive.Value}:. Searching for game file...");

            var gameFilePath = FindGameFile(driveRoot);

            if (string.IsNullOrEmpty(gameFilePath))
            {
                DebugLogger.Log($"[MountChdFiles] No suitable game file found in {driveRoot}.");
                throw new FileNotFoundException($"Could not find a game file within the mounted CHD at {driveRoot}.");
            }

            DebugLogger.Log($"[MountChdFiles] Game file found at: {gameFilePath}. Proceeding to launch with {selectedEmulatorName}.");

            await gameLauncher.LaunchRegularEmulatorAsync(gameFilePath, selectedEmulatorName, selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters, mainWindow, mainWindow);

            DebugLogger.Log($"[MountChdFiles] Emulator for {gameFilePath} has exited.");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[MountChdFiles] Exception during CHD mounting or launching: {ex}");
            var exitCodeInfoInCatch = mountProcess is { HasExited: true } ? $"Exit Code: {mountProcess.ExitCode}" : "Process was still running or state unknown.";
            var contextMessage = $"Error during CHD mount/launch process for {resolvedChdFilePath}.\n" +
                                 $"Exception: {ex.Message}\n" +
                                 $"{exitCodeInfoInCatch}";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            MessageBoxLibrary.ThereWasAnErrorMountingTheFile();
        }
        finally
        {
            DebugLogger.Log($"[MountChdFiles] Entering finally block for {resolvedChdFilePath}. Mount Process ID: {mountProcessId}");
            if (mountProcess != null && mountProcessId != -1 && !mountProcess.HasExited)
            {
                DebugLogger.Log($"[MountChdFiles] Attempting to unmount by terminating CHDMounter (ID: {mountProcessId}).");
                try
                {
                    mountProcess.Kill(true);
                    DebugLogger.Log($"[MountChdFiles] Kill signal sent to CHDMounter (ID: {mountProcessId}). Waiting for process to exit (up to 10s).");
                    using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(10));
                    try
                    {
                        await mountProcess.WaitForExitAsync(cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        DebugLogger.Log($"[MountChdFiles] Timeout (10s) waiting for CHDMounter (ID: {mountProcessId}) to exit after Kill.");
                    }

                    if (mountProcess.HasExited)
                    {
                        DebugLogger.Log($"[MountChdFiles] CHDMounter (ID: {mountProcessId}) terminated. Exit code: {mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture)}.");
                    }
                    else
                    {
                        DebugLogger.Log($"[MountChdFiles] CHDMounter (ID: {mountProcessId}) did NOT terminate after Kill signal and 10s wait.");
                    }
                }
                catch (InvalidOperationException ioEx)
                {
                    if (ioEx.Message.Contains("process has already exited", StringComparison.OrdinalIgnoreCase) ||
                        ioEx.Message.Contains("No process is associated", StringComparison.OrdinalIgnoreCase))
                    {
                        DebugLogger.Log($"[MountChdFiles] CHDMounter (ID: {mountProcessId}) already exited: {ioEx.Message}");
                    }
                    else
                    {
                        DebugLogger.Log($"[MountChdFiles] InvalidOperationException while terminating CHDMounter (ID: {mountProcessId}): {ioEx}");
                        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ioEx, "Unexpected InvalidOperationException during CHDMounter termination.");
                    }
                }
                catch (Exception termEx)
                {
                    DebugLogger.Log($"[MountChdFiles] Exception while terminating CHDMounter (ID: {mountProcessId}): {termEx}");
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(termEx, $"Failed to terminate CHDMounter (ID: {mountProcessId}) for unmounting.");
                }
            }
            else if (mountProcessId != -1)
            {
                var exitCodeStr = mountProcess is { HasExited: true } ? mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A";
                DebugLogger.Log($"[MountChdFiles] CHDMounter (ID: {mountProcessId}) had already exited. Exit code likely {exitCodeStr}.");
            }
            else
            {
                DebugLogger.Log("[MountChdFiles] CHDMounter process was not started successfully. No termination needed.");
            }

            mountProcess?.Dispose();

            await Task.Delay(2000);
            if (Directory.Exists(driveRoot))
            {
                DebugLogger.Log($"[MountChdFiles] WARNING: Drive {driveRoot} still exists after attempting to unmount.");
            }
            else
            {
                DebugLogger.Log($"[MountChdFiles] Drive {driveRoot} successfully unmounted.");
            }
        }
    }

    /// <summary>
    /// Mounts a CHD file, finds the executable file, and launches it with the specified emulator.
    /// Supports custom system index for console type selection.
    /// </summary>
    /// <param name="resolvedChdFilePath">The full path to the CHD file.</param>
    /// <param name="selectedSystemName">The selected system name.</param>
    /// <param name="selectedEmulatorName">The selected emulator name.</param>
    /// <param name="selectedSystemManager">The system manager.</param>
    /// <param name="selectedEmulatorManager">The emulator manager.</param>
    /// <param name="rawEmulatorParameters">Raw emulator parameters.</param>
    /// <param name="mainWindow">The main window.</param>
    /// <param name="logPath">Path to the application's log file.</param>
    /// <param name="gameLauncher">The game launcher instance.</param>
    /// <param name="consoleIndex">Optional console index for CHDMounter (1-16). If null, uses /a for auto-detection.</param>
    public static async Task MountChdFileAndLoadWithConsoleIndexAsync(
        string resolvedChdFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        SystemManager.SystemManager selectedSystemManager,
        SystemManager.Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        MainWindow mainWindow,
        string logPath,
        GameLauncher.GameLauncher gameLauncher,
        int? consoleIndex = null)
    {
        DebugLogger.Log($"[MountChdFiles] Starting to mount CHD with console index for game loading: {resolvedChdFilePath}");
        DebugLogger.Log($"[MountChdFiles] System: {selectedSystemName}, Emulator: {selectedEmulatorName}, ConsoleIndex: {consoleIndex?.ToString(CultureInfo.InvariantCulture) ?? "auto"}");

        var resolvedToolPath = PathHelper.ResolveRelativeToAppDirectory(ChdMounterRelativePath);

        DebugLogger.Log($"[MountChdFiles] Path to CHDMounter: {resolvedToolPath}");

        if (string.IsNullOrWhiteSpace(resolvedToolPath) || !File.Exists(resolvedToolPath))
        {
            const string errorMessage = $"CHDMounter.exe not found at {ChdMounterRelativePath}. Cannot mount CHD.";
            DebugLogger.Log($"[MountChdFiles] Error: {errorMessage}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, errorMessage);
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile();
            return;
        }

        // Capture existing drives before mounting
        var existingDrives = GetCurrentDriveLetters();
        DebugLogger.Log($"[MountChdFiles] Existing drives before mount: {string.Join(", ", existingDrives)}");

        var arguments = consoleIndex.HasValue
            ? $"\"{resolvedChdFilePath}\" /a /s:{consoleIndex.Value}"
            : $"\"{resolvedChdFilePath}\" /a";

        var psiMount = new ProcessStartInfo
        {
            FileName = resolvedToolPath,
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(resolvedToolPath) ?? AppDomain.CurrentDomain.BaseDirectory
        };

        DebugLogger.Log("[MountChdFiles] ProcessStartInfo:");
        DebugLogger.Log($"[MountChdFiles] FileName: {psiMount.FileName}");
        DebugLogger.Log($"[MountChdFiles] Arguments: {psiMount.Arguments}");
        DebugLogger.Log($"[MountChdFiles] WorkingDirectory: {psiMount.WorkingDirectory}");

        Process mountProcess = null;
        var mountProcessId = -1;
        string driveRoot = null;

        try
        {
            mountProcess = new Process { StartInfo = psiMount, EnableRaisingEvents = true };

            DebugLogger.Log("[MountChdFiles] Starting CHDMounter process...");
            if (!mountProcess.Start())
            {
                throw new InvalidOperationException("Failed to start the CHDMounter process.");
            }

            mountProcessId = mountProcess.Id;
            DebugLogger.Log($"[MountChdFiles] CHDMounter process started (ID: {mountProcessId}).");

            var (mountSuccessful, drive) = await WaitForDriveMountAndDetectAsync(existingDrives, mountProcess, mountProcessId);

            if (!mountSuccessful || drive == null)
            {
                if (!mountProcess.HasExited)
                {
                    mountProcess.Kill(true);
                }

                mountProcess.Dispose();
                MessageBoxLibrary.ThereWasAnErrorMountingTheFile();
                return;
            }

            driveRoot = $"{drive.Value}:\\";
            DebugLogger.Log($"[MountChdFiles] CHD mounted successfully on drive {drive.Value}:. Searching for game file...");

            var gameFilePath = FindGameFile(driveRoot);

            if (string.IsNullOrEmpty(gameFilePath))
            {
                DebugLogger.Log($"[MountChdFiles] No suitable game file found in {driveRoot}.");
                throw new FileNotFoundException($"Could not find a game file within the mounted CHD at {driveRoot}.");
            }

            DebugLogger.Log($"[MountChdFiles] Game file found at: {gameFilePath}. Proceeding to launch with {selectedEmulatorName}.");

            await gameLauncher.LaunchRegularEmulatorAsync(gameFilePath, selectedEmulatorName, selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters, mainWindow, mainWindow);

            DebugLogger.Log($"[MountChdFiles] Emulator for {gameFilePath} has exited.");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[MountChdFiles] Exception during CHD mounting or launching: {ex}");
            var exitCodeInfoInCatch = mountProcess is { HasExited: true } ? $"Exit Code: {mountProcess.ExitCode}" : "Process was still running or state unknown.";
            var contextMessage = $"Error during CHD mount/launch process for {resolvedChdFilePath}.\n" +
                                 $"Exception: {ex.Message}\n" +
                                 $"{exitCodeInfoInCatch}";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            MessageBoxLibrary.ThereWasAnErrorMountingTheFile();
        }
        finally
        {
            DebugLogger.Log($"[MountChdFiles] Entering finally block for {resolvedChdFilePath}. Mount Process ID: {mountProcessId}");
            if (mountProcess != null && mountProcessId != -1 && !mountProcess.HasExited)
            {
                DebugLogger.Log($"[MountChdFiles] Attempting to unmount by terminating CHDMounter (ID: {mountProcessId}).");
                try
                {
                    mountProcess.Kill(true);
                    DebugLogger.Log($"[MountChdFiles] Kill signal sent to CHDMounter (ID: {mountProcessId}). Waiting for process to exit (up to 10s).");
                    using var cts = new System.Threading.CancellationTokenSource(TimeSpan.FromSeconds(10));
                    try
                    {
                        await mountProcess.WaitForExitAsync(cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        DebugLogger.Log($"[MountChdFiles] Timeout (10s) waiting for CHDMounter (ID: {mountProcessId}) to exit after Kill.");
                    }

                    if (mountProcess.HasExited)
                    {
                        DebugLogger.Log($"[MountChdFiles] CHDMounter (ID: {mountProcessId}) terminated. Exit code: {mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture)}.");
                    }
                    else
                    {
                        DebugLogger.Log($"[MountChdFiles] CHDMounter (ID: {mountProcessId}) did NOT terminate after Kill signal and 10s wait.");
                    }
                }
                catch (InvalidOperationException ioEx)
                {
                    if (ioEx.Message.Contains("process has already exited", StringComparison.OrdinalIgnoreCase) ||
                        ioEx.Message.Contains("No process is associated", StringComparison.OrdinalIgnoreCase))
                    {
                        DebugLogger.Log($"[MountChdFiles] CHDMounter (ID: {mountProcessId}) already exited: {ioEx.Message}");
                    }
                    else
                    {
                        DebugLogger.Log($"[MountChdFiles] InvalidOperationException while terminating CHDMounter (ID: {mountProcessId}): {ioEx}");
                        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ioEx, "Unexpected InvalidOperationException during CHDMounter termination.");
                    }
                }
                catch (Exception termEx)
                {
                    DebugLogger.Log($"[MountChdFiles] Exception while terminating CHDMounter (ID: {mountProcessId}): {termEx}");
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(termEx, $"Failed to terminate CHDMounter (ID: {mountProcessId}) for unmounting.");
                }
            }
            else if (mountProcessId != -1)
            {
                var exitCodeStr = mountProcess is { HasExited: true } ? mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A";
                DebugLogger.Log($"[MountChdFiles] CHDMounter (ID: {mountProcessId}) had already exited. Exit code likely {exitCodeStr}.");
            }
            else
            {
                DebugLogger.Log("[MountChdFiles] CHDMounter process was not started successfully. No termination needed.");
            }

            mountProcess?.Dispose();

            await Task.Delay(2000);
            if (Directory.Exists(driveRoot))
            {
                DebugLogger.Log($"[MountChdFiles] WARNING: Drive {driveRoot} still exists after attempting to unmount.");
            }
            else
            {
                DebugLogger.Log($"[MountChdFiles] Drive {driveRoot} successfully unmounted.");
            }
        }
    }

    /// <summary>
    /// Gets the console index for CHDMounter based on the system name and optionally the emulator name.
    /// </summary>
    /// <param name="systemName">The name of the system.</param>
    /// <param name="emulatorName">The name of the emulator (optional).</param>
    /// <returns>The console index, or null if not found.</returns>
    public static int? GetConsoleIndexFromSystemName(string systemName, string emulatorName = null)
    {
        if (string.IsNullOrEmpty(systemName)) return null;

        if ((systemName.Contains("AMIGA CD", StringComparison.OrdinalIgnoreCase) ||
             systemName.Contains("AMIGACD", StringComparison.OrdinalIgnoreCase)) &&
            !systemName.Contains("CD32", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        if (systemName.Contains("AMIGA CD32", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("AMIGACD32", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("CD32", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        if (systemName.Contains("CD-I", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("CDI", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("PHILIPS CDI", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("PHILIPSCDI", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        if (systemName.Contains("DREAMCAST", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("SEGA DREAMCAST", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        if (systemName.Contains("NEOGEO CD", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("NEO GEO CD", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        if (systemName.Contains("PCE-CD", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("PC ENGINE CD", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        if (systemName.Contains("PC-FX", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("PCFX", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        if ((systemName.Contains("PS1", StringComparison.OrdinalIgnoreCase) ||
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

        if (systemName.Contains("PS2", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("PSX2", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("PSX 2", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("PLAY 2", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("PLAYSTATION 2", StringComparison.OrdinalIgnoreCase))
        {
            return 9;
        }

        if (systemName.Contains("PS3", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("PSX3", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("PSX 3", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("PLAY 3", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("PLAYSTATION 3", StringComparison.OrdinalIgnoreCase))
        {
            return 10;
        }

        if (systemName.Contains("PSP", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("PLAYSTATION PORTABLE", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("SONY PSP", StringComparison.OrdinalIgnoreCase))
        {
            return 11;
        }

        if (systemName.Contains("SATURN", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("SEGA SATURN", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        if (systemName.Contains("GENESIS CD", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("SEGA CD", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("MEGA CD", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        if (systemName.Contains("3DO", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("PANASONIC 3DO", StringComparison.OrdinalIgnoreCase))
        {
            return 21;
        }

        if (systemName.Contains("XBOX", StringComparison.OrdinalIgnoreCase) &&
            !systemName.Contains("360"))
        {
            if (!string.IsNullOrEmpty(emulatorName) &&
                emulatorName.Contains("xemu", StringComparison.OrdinalIgnoreCase))
            {
                return 18;
            }

            return 17;
        }

        if (systemName.Contains("XBOX 360", StringComparison.OrdinalIgnoreCase) ||
            systemName.Contains("XBOX360", StringComparison.OrdinalIgnoreCase))
        {
            return 17;
        }

        if (!string.IsNullOrEmpty(emulatorName) &&
            emulatorName.Contains("RAINE", StringComparison.OrdinalIgnoreCase))
        {
            return 20;
        }

        return null;
    }

    /// <summary>
    /// Waits for a new drive to appear after mounting with /a flag (auto-select drive letter).
    /// Compares current drives against the pre-mount snapshot to detect which drive was assigned.
    /// </summary>
    private static async Task<(bool Success, char? DriveLetter)> WaitForDriveMountAndDetectAsync(HashSet<char> existingDrives, Process mountProcess, int processId)
    {
        const int maxRetries = 240; // 2 minutes with 500 ms intervals
        const int pollIntervalMs = 500;
        var retryCount = 0;

        DebugLogger.Log($"[MountChdFiles.WaitForDriveMountAndDetectAsync] Polling for new drive to appear (max {maxRetries * pollIntervalMs / 1000}s)...");

        while (retryCount < maxRetries)
        {
            // Check if process exited prematurely
            if (mountProcess.HasExited)
            {
                DebugLogger.Log($"[MountChdFiles.WaitForDriveMountAndDetectAsync] CHDMounter process (ID: {processId}) exited prematurely during polling. Exit Code: {mountProcess.ExitCode}.");
                var contextMessage = $"Failed to mount CHD. The CHDMounter tool exited prematurely with code {mountProcess.ExitCode}.";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
                return (false, null);
            }

            // Get current drives and find any new ones
            var currentDrives = GetCurrentDriveLetters();
            var newDrives = currentDrives.Except(existingDrives).ToList();

            if (newDrives.Count > 0)
            {
                var detectedDrive = newDrives[0];
                DebugLogger.Log($"[MountChdFiles.WaitForDriveMountAndDetectAsync] Found new drive '{detectedDrive}:' after {retryCount * pollIntervalMs / 1000.0:F1} seconds. Mount successful!");
                return (true, detectedDrive);
            }

            retryCount++;
            await Task.Delay(pollIntervalMs);
        }

        DebugLogger.Log($"[MountChdFiles.WaitForDriveMountAndDetectAsync] Timed out waiting for new drive after {maxRetries * pollIntervalMs / 1000} seconds.");
        const string timeoutContextMessage = "Timed out waiting for the CHD to mount. No new drive detected.";
        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, timeoutContextMessage);
        return (false, null);
    }

    /// <summary>
    /// Searches for a game file in the mounted drive.
    /// Looks for common disc image files and executable files.
    /// </summary>
    private static string FindGameFile(string driveRoot)
    {
        try
        {
            DebugLogger.Log($"[MountChdFiles.FindGameFile] Searching for game file in {driveRoot}...");

            if (!Directory.Exists(driveRoot))
            {
                DebugLogger.Log($"[MountChdFiles.FindGameFile] Drive {driveRoot} does not exist.");
                return null;
            }

            var allFiles = Directory.GetFiles(driveRoot, "*", SearchOption.AllDirectories);
            if (allFiles.Length == 0)
            {
                DebugLogger.Log($"[MountChdFiles.FindGameFile] No files found in {driveRoot}.");
                return null;
            }

            DebugLogger.Log($"[MountChdFiles.FindGameFile] Found {allFiles.Length} files in {driveRoot}.");

            // Log all found files for debugging
            foreach (var file in allFiles)
            {
                DebugLogger.Log($"[MountChdFiles.FindGameFile] Found: {file}");
            }

            return allFiles[0];
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[MountChdFiles.FindGameFile] Error searching for game file in {driveRoot}: {ex.Message}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Error in FindGameFile searching {driveRoot}");
            return null;
        }
    }
}
