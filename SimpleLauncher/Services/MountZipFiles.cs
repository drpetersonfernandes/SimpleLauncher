using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Managers;

namespace SimpleLauncher.Services;

public static class MountZipFiles
{
    private static string _configuredMountDriveLetterOnly = "Z";
    private static string _zipToVdExecutableName = "zip2vd.cli.exe";

    // Public property to get the letter ("Z")
    private static string ConfiguredMountDriveLetter => _configuredMountDriveLetterOnly;

    // Public property to get the root path ("Z:\") for directory checks
    public static string ConfiguredMountDriveRoot => _configuredMountDriveLetterOnly + ":\\";

    public static void Configure(IConfiguration configuration)
    {
        var mountPathFromConfig = configuration.GetValue("ZipMountOptions:MountDriveLetter", "Z:");
        _zipToVdExecutableName = configuration.GetValue("ZipMountOptions:ZipToVdExecutableName", "zip2vd.cli.exe");

        if (string.IsNullOrEmpty(mountPathFromConfig))
        {
            mountPathFromConfig = "Z:"; // Fallback
        }

        // Extract just the drive letter
        if (mountPathFromConfig.EndsWith(":\\", StringComparison.Ordinal))
        {
            _configuredMountDriveLetterOnly = mountPathFromConfig.Substring(0, mountPathFromConfig.Length - 2);
        }
        else if (mountPathFromConfig.EndsWith(':'))
        {
            _configuredMountDriveLetterOnly = mountPathFromConfig.Substring(0, mountPathFromConfig.Length - 1);
        }
        else // Assume it's just the letter or an invalid format, try to take the first char if it's a letter
        {
            _configuredMountDriveLetterOnly = mountPathFromConfig.Length > 0 && char.IsLetter(mountPathFromConfig[0])
                ? mountPathFromConfig[0].ToString().ToUpperInvariant()
                : "Z";
        }

        DebugLogger.Log($"[MountZipFiles] Configured MountDriveLetter (for zip2vd): {_configuredMountDriveLetterOnly}");
        DebugLogger.Log($"[MountZipFiles] Configured MountDriveRoot (for checks): {ConfiguredMountDriveRoot}");
        DebugLogger.Log($"[MountZipFiles] Configured ZipToVdExecutableName: {_zipToVdExecutableName}");
    }

    public static async Task MountZipFile(
        string resolvedZipFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        SystemManager selectedSystemManager,
        SystemManager.Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        MainWindow mainWindow,
        string logPath)
    {
        DebugLogger.Log($"[MountZipFiles] Starting to mount ZIP: {resolvedZipFilePath}");
        DebugLogger.Log($"[MountZipFiles] System: {selectedSystemName}, Emulator: {selectedEmulatorName}");

        var resolvedZipToVdPath = PathHelper.ResolveRelativeToAppDirectory(_zipToVdExecutableName);

        DebugLogger.Log($"[MountZipFiles] Path to {_zipToVdExecutableName}: {resolvedZipToVdPath}");

        if (string.IsNullOrWhiteSpace(resolvedZipToVdPath) || !File.Exists(resolvedZipToVdPath))
        {
            // Notify developer
            var errorMessage = $"{_zipToVdExecutableName} not found in application directory. Cannot mount ZIP.";
            DebugLogger.Log($"[MountZipFiles] Error: {errorMessage}");
            _ = LogErrors.LogErrorAsync(null, errorMessage);

            // Notify user
            MessageBoxLibrary.GeneralErrorOccurred(logPath, $"{_zipToVdExecutableName} not found.");

            return;
        }

        // Use the letter ONLY for the --MountPath argument
        var mountPathArgument = ConfiguredMountDriveLetter; // This will be "Z"
        var mountDriveRootForChecks = ConfiguredMountDriveRoot; // This will be "Z:\" for Directory.Exists

        var psiMount = new ProcessStartInfo
        {
            FileName = resolvedZipToVdPath,
            Arguments = $"--FilePath \"{resolvedZipFilePath}\" --MountPath \"{mountPathArgument}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetDirectoryName(resolvedZipToVdPath) ?? AppDomain.CurrentDomain.BaseDirectory
        };

        DebugLogger.Log($"[MountZipFiles] ProcessStartInfo for {_zipToVdExecutableName}:");
        DebugLogger.Log($"[MountZipFiles] FileName: {psiMount.FileName}");
        DebugLogger.Log($"[MountZipFiles] Arguments: {psiMount.Arguments}");
        DebugLogger.Log($"[MountZipFiles] WorkingDirectory: {psiMount.WorkingDirectory}");

        Process mountProcess = null;
        var mountProcessId = -1;
        var mountOutput = new StringBuilder();
        var mountError = new StringBuilder();

        try
        {
            mountProcess = new Process();
            mountProcess.StartInfo = psiMount;
            mountProcess.EnableRaisingEvents = true;

            mountProcess.OutputDataReceived += (_, args) =>
            {
                if (args.Data == null) return;

                mountOutput.AppendLine(args.Data);
                DebugLogger.Log($"[MountZipFiles] {_zipToVdExecutableName} STDOUT: {args.Data}");
            };
            mountProcess.ErrorDataReceived += (_, args) =>
            {
                if (args.Data == null) return;

                mountError.AppendLine(args.Data);
                DebugLogger.Log($"[MountZipFiles] {_zipToVdExecutableName} STDERR: {args.Data}");
            };

            DebugLogger.Log($"[MountZipFiles] Starting {_zipToVdExecutableName} process...");
            var processStarted = mountProcess.Start();
            if (!processStarted)
            {
                throw new InvalidOperationException($"Failed to start the {_zipToVdExecutableName} process.");
            }

            mountProcessId = mountProcess.Id;
            DebugLogger.Log($"[MountZipFiles] {_zipToVdExecutableName} process started (ID: {mountProcessId}).");

            mountProcess.BeginOutputReadLine();
            mountProcess.BeginErrorReadLine();

            DebugLogger.Log($"[MountZipFiles] Waiting a few seconds for ZIP to mount to {mountDriveRootForChecks}...");
            await Task.Delay(5000);

            // Use mountDriveRootForChecks for Directory.Exists
            if (!Directory.Exists(mountDriveRootForChecks))
            {
                DebugLogger.Log($"[MountZipFiles] Mount check failed. Drive {mountDriveRootForChecks} not found.");
                DebugLogger.Log($"[MountZipFiles] {_zipToVdExecutableName} Output:\n{mountOutput}");
                DebugLogger.Log($"[MountZipFiles] {_zipToVdExecutableName} Error:\n{mountError}");
                throw new Exception($"Failed to mount ZIP. Drive {mountDriveRootForChecks} not found after timeout.");
            }

            DebugLogger.Log($"[MountZipFiles] Drive {mountDriveRootForChecks} detected. Searching for EBOOT.BIN...");
            var ebootBinPath = FindEbootBinRecursive(mountDriveRootForChecks);

            if (string.IsNullOrEmpty(ebootBinPath))
            {
                DebugLogger.Log($"[MountZipFiles] EBOOT.BIN not found in {mountDriveRootForChecks}.");
                throw new FileNotFoundException($"EBOOT.BIN not found within the mounted ZIP file at {mountDriveRootForChecks}.");
            }

            DebugLogger.Log($"[MountZipFiles] EBOOT.BIN found at: {ebootBinPath}. Proceeding to launch with {selectedEmulatorName}.");
            await GameLauncher.LaunchRegularEmulator(ebootBinPath, selectedEmulatorName, selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters, mainWindow);
            DebugLogger.Log($"[MountZipFiles] Emulator for {ebootBinPath} has exited.");
        }
        catch (Exception ex)
        {
            // Notify developer
            DebugLogger.Log($"[MountZipFiles] Exception during ZIP mounting or launching: {ex}");
            var contextMessage = $"Error during ZIP mount/launch process for {resolvedZipFilePath}.\n" +
                                 $"Exception: {ex.Message}\n" +
                                 $"{_zipToVdExecutableName} Output: {mountOutput}\n" +
                                 $"{_zipToVdExecutableName} Error: {mountError}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.GeneralErrorOccurred(logPath, $"Error mounting or launching ZIP file: {ex.Message}");

            return;
        }
        finally
        {
            DebugLogger.Log($"[MountZipFiles] Entering finally block for {resolvedZipFilePath}. Mount Process ID: {mountProcessId}");
            if (mountProcess != null && mountProcessId != -1 && !mountProcess.HasExited)
            {
                DebugLogger.Log($"[MountZipFiles] Attempting to unmount by terminating {_zipToVdExecutableName} (ID: {mountProcessId}).");
                try
                {
                    mountProcess.Kill(true);
                    DebugLogger.Log($"[MountZipFiles] Kill signal sent to {_zipToVdExecutableName} (ID: {mountProcessId}). Waiting for process to exit (up to 10s).");
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    try
                    {
                        await mountProcess.WaitForExitAsync(cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        DebugLogger.Log($"[MountZipFiles] Timeout (10s) waiting for {_zipToVdExecutableName} (ID: {mountProcessId}) to exit after Kill.");
                    }

                    if (mountProcess.HasExited)
                    {
                        DebugLogger.Log($"[MountZipFiles] {_zipToVdExecutableName} (ID: {mountProcessId}) terminated. Exit code: {mountProcess.ExitCode}.");
                    }
                    else
                    {
                        DebugLogger.Log($"[MountZipFiles] {_zipToVdExecutableName} (ID: {mountProcessId}) did NOT terminate after Kill signal and 10s wait.");
                    }
                }
                catch (InvalidOperationException ioEx)
                {
                    if (ioEx.Message.Contains("process has already exited", StringComparison.OrdinalIgnoreCase) ||
                        ioEx.Message.Contains("No process is associated", StringComparison.OrdinalIgnoreCase))
                    {
                        DebugLogger.Log($"[MountZipFiles] {_zipToVdExecutableName} (ID: {mountProcessId}) already exited or no process associated: {ioEx.Message}");
                    }
                    else
                    {
                        DebugLogger.Log($"[MountZipFiles] InvalidOperationException while terminating {_zipToVdExecutableName} (ID: {mountProcessId}): {ioEx}");

                        // Notify developer
                        _ = LogErrors.LogErrorAsync(ioEx, $"Unexpected InvalidOperationException during {_zipToVdExecutableName} termination.");
                    }
                }
                catch (Exception termEx)
                {
                    DebugLogger.Log($"[MountZipFiles] Exception while terminating {_zipToVdExecutableName} (ID: {mountProcessId}): {termEx}");

                    // Notify developer
                    _ = LogErrors.LogErrorAsync(termEx, $"Failed to terminate {_zipToVdExecutableName} (ID: {mountProcessId}) for unmounting.");
                }
            }
            else if (mountProcessId != -1)
            {
                DebugLogger.Log($"[MountZipFiles] {_zipToVdExecutableName} (ID: {mountProcessId}) had already exited or was not running when finally cleanup was attempted. Exit code likely {(mountProcess != null && mountProcess.HasExited ? mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A")}.");
            }
            else
            {
                DebugLogger.Log($"[MountZipFiles] {_zipToVdExecutableName} process was not started successfully (ID: {mountProcessId}). No termination needed.");
            }

            mountProcess?.Dispose();

            await Task.Delay(2000);
            // Use mountDriveRootForChecks for Directory.Exists
            if (Directory.Exists(mountDriveRootForChecks))
            {
                DebugLogger.Log($"[MountZipFiles] WARNING: Drive {mountDriveRootForChecks} still exists after attempting to unmount. {_zipToVdExecutableName} might not have unmounted correctly or is still running.");
            }
            else
            {
                DebugLogger.Log($"[MountZipFiles] Drive {mountDriveRootForChecks} successfully unmounted (or was not detected).");
            }
        }
    }

    private static string FindEbootBinRecursive(string directoryPath)
    {
        const string targetFileName = "EBOOT.BIN";
        try
        {
            var files = Directory.GetFiles(directoryPath, targetFileName, SearchOption.TopDirectoryOnly);
            if (files.Length != 0)
            {
                return files[0];
            }

            var ps3GameDirs = Directory.GetDirectories(directoryPath, "PS3_GAME", SearchOption.TopDirectoryOnly);
            foreach (var ps3GameDir in ps3GameDirs)
            {
                var usrDir = Path.Combine(ps3GameDir, "USRDIR");
                if (!Directory.Exists(usrDir)) continue;

                files = Directory.GetFiles(usrDir, targetFileName, SearchOption.TopDirectoryOnly);
                if (files.Length != 0)
                {
                    return files[0];
                }
            }

            DebugLogger.Log($"[FindEbootBinRecursive] EBOOT.BIN not found in typical PS3_GAME/USRDIR structure in {directoryPath}. Starting full recursive search...");
            files = Directory.GetFiles(directoryPath, targetFileName, SearchOption.AllDirectories);
            if (files.Length != 0)
            {
                DebugLogger.Log($"[FindEbootBinRecursive] Found EBOOT.BIN via full recursive search: {files[0]}");
                return files[0];
            }
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[FindEbootBinRecursive] Error searching for EBOOT.BIN in {directoryPath}: {ex.Message}");
        }

        return null;
    }
}