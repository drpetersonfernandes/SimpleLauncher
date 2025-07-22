using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Managers;

namespace SimpleLauncher.Services;

public static class MountZipFiles
{
    private static string _configuredMountDriveLetterOnly = "Z";
    private static string _zipMountExecutableName = "SimpleZipDrive.exe";
    private static string _zipMountExecutableRelativePath = @"tools\SimpleZipDrive\SimpleZipDrive.exe";

    // Public property to get the letter ("Z")
    private static string ConfiguredMountDriveLetter => _configuredMountDriveLetterOnly;

    // Public property to get the root path ("Z:\") for directory checks
    public static string ConfiguredMountDriveRoot => _configuredMountDriveLetterOnly + ":\\";

    public static void Configure(IConfiguration configuration)
    {
        var mountPathFromConfig = configuration.GetValue("ZipMountOptions:MountDriveLetter", "Z:");
        _zipMountExecutableName = configuration.GetValue("ZipMountOptions:ZipMountExecutableName", "SimpleZipDrive.exe");
        _zipMountExecutableRelativePath = configuration.GetValue("ZipMountOptions:ZipMountExecutableRelativePath", @"tools\SimpleZipDrive\SimpleZipDrive.exe");

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

        DebugLogger.Log($"[MountZipFiles] Configured MountDriveLetter (for {_zipMountExecutableName}): {_configuredMountDriveLetterOnly}");
        DebugLogger.Log($"[MountZipFiles] Configured MountDriveRoot (for checks): {ConfiguredMountDriveRoot}");
        DebugLogger.Log($"[MountZipFiles] Configured ZipMountExecutableName: {_zipMountExecutableName}");
        DebugLogger.Log($"[MountZipFiles] Configured ZipMountExecutableRelativePath: {_zipMountExecutableRelativePath}");
    }

    public static async Task MountZipFileAndLoadEbootBin(
        string resolvedZipFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        SystemManager selectedSystemManager,
        SystemManager.Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        MainWindow mainWindow,
        string logPath)
    {
        DebugLogger.Log($"[MountZipFiles] Starting to mount ZIP for EBOOT.BIN: {resolvedZipFilePath}");
        DebugLogger.Log($"[MountZipFiles] System: {selectedSystemName}, Emulator: {selectedEmulatorName}");

        var resolvedZipMountExePath = PathHelper.ResolveRelativeToAppDirectory(_zipMountExecutableRelativePath);

        DebugLogger.Log($"[MountZipFiles] Path to {_zipMountExecutableName}: {resolvedZipMountExePath}");

        if (string.IsNullOrWhiteSpace(resolvedZipMountExePath) || !File.Exists(resolvedZipMountExePath))
        {
            // Notify developer
            var errorMessage = $"{_zipMountExecutableName} not found at {_zipMountExecutableRelativePath}. Cannot mount ZIP.";
            DebugLogger.Log($"[MountZipFiles] Error: {errorMessage}");
            _ = LogErrors.LogErrorAsync(null, errorMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);

            return;
        }

        // Use the letter ONLY for the mount point argument
        var mountPathArgument = ConfiguredMountDriveLetter; // This will be "Z"
        var mountDriveRootForChecks = ConfiguredMountDriveRoot; // This will be "Z:\" for Directory.Exists

        var psiMount = new ProcessStartInfo
        {
            FileName = resolvedZipMountExePath,
            // SimpleZipDrive uses positional arguments: "<PathToZipFile>" "<MountPoint>"
            Arguments = $"\"{resolvedZipFilePath}\" \"{mountPathArgument}\"",
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = false,
            WorkingDirectory = Path.GetDirectoryName(resolvedZipMountExePath) ?? AppDomain.CurrentDomain.BaseDirectory
        };

        DebugLogger.Log($"[MountZipFiles] ProcessStartInfo for {_zipMountExecutableName}:");
        DebugLogger.Log($"[MountZipFiles] FileName: {psiMount.FileName}");
        DebugLogger.Log($"[MountZipFiles] Arguments: {psiMount.Arguments}");
        DebugLogger.Log($"[MountZipFiles] WorkingDirectory: {psiMount.WorkingDirectory}");

        Process mountProcess = null;
        var mountProcessId = -1;

        try
        {
            mountProcess = new Process();
            mountProcess.StartInfo = psiMount;
            mountProcess.EnableRaisingEvents = true;

            DebugLogger.Log($"[MountZipFiles] Starting {_zipMountExecutableName} process...");
            var processStarted = mountProcess.Start();
            if (!processStarted)
            {
                throw new InvalidOperationException($"Failed to start the {_zipMountExecutableName} process.");
            }

            mountProcessId = mountProcess.Id;
            DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} process started (ID: {mountProcessId}).");

            // Polling mechanism to wait for the mount to complete.
            var mountSuccessful = false;
            var timeout = TimeSpan.FromMinutes(1); // 1-minute timeout for zip mounting
            var pollInterval = TimeSpan.FromMilliseconds(500);
            var stopwatch = Stopwatch.StartNew();

            DebugLogger.Log($"[MountZipFiles] Polling for drive '{mountDriveRootForChecks}' to appear (timeout: {timeout.TotalSeconds}s)...");

            while (stopwatch.Elapsed < timeout)
            {
                if (Directory.Exists(mountDriveRootForChecks))
                {
                    mountSuccessful = true;
                    DebugLogger.Log($"[MountZipFiles] Found drive '{mountDriveRootForChecks}' after {stopwatch.Elapsed.TotalSeconds:F1} seconds.");
                    break;
                }

                if (mountProcess.HasExited)
                {
                    DebugLogger.Log($"[MountZipFiles] Mount process {_zipMountExecutableName} (ID: {mountProcessId}) exited prematurely during polling. Exit Code: {mountProcess.ExitCode}.");
                    break;
                }

                await Task.Delay(pollInterval);
            }

            stopwatch.Stop();

            if (!mountSuccessful)
            {
                var exitCodeInfoOnFailure = mountProcess.HasExited ? $"The process exited with code {mountProcess.ExitCode}." : "The process was still running.";
                DebugLogger.Log($"[MountZipFiles] Mount check failed. Drive {mountDriveRootForChecks} not found. {exitCodeInfoOnFailure} Check the console window of {_zipMountExecutableName} for details.");
                throw new Exception($"Failed to mount ZIP. Drive {mountDriveRootForChecks} not found after timeout.");
            }

            DebugLogger.Log($"[MountZipFiles] Drive {mountDriveRootForChecks} detected. Searching for EBOOT.BIN...");

            // Find EBOOT.BIN
            var ebootBinPath = FindEbootBin.FindEbootBinRecursive(mountDriveRootForChecks);

            if (string.IsNullOrEmpty(ebootBinPath))
            {
                DebugLogger.Log($"[MountZipFiles] EBOOT.BIN not found in {mountDriveRootForChecks}.");
                throw new FileNotFoundException($"EBOOT.BIN not found within the mounted ZIP file at {mountDriveRootForChecks}.");
            }

            DebugLogger.Log($"[MountZipFiles] EBOOT.BIN found at: {ebootBinPath}. Proceeding to launch with {selectedEmulatorName}.");
            await GameLauncher.LaunchRegularEmulator(ebootBinPath, selectedSystemName, selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters, mainWindow);
            DebugLogger.Log($"[MountZipFiles] Emulator for {ebootBinPath} has exited.");
        }
        catch (Exception ex)
        {
            // Notify developer
            DebugLogger.Log($"[MountZipFiles] Exception during ZIP mounting or launching: {ex}");
            var exitCodeInfoInCatch = mountProcess != null && mountProcess.HasExited ? $"Exit Code: {mountProcess.ExitCode}" : "Process was still running or state unknown.";
            var contextMessage = $"Error during ZIP mount/launch process for {resolvedZipFilePath}.\n" +
                                 $"Exception: {ex.Message}\n" +
                                 $"The tool's output was not redirected. {exitCodeInfoInCatch}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);

            return;
        }
        finally
        {
            DebugLogger.Log($"[MountZipFiles] Entering finally block for {resolvedZipFilePath}. Mount Process ID: {mountProcessId}");
            if (mountProcess != null && mountProcessId != -1 && !mountProcess.HasExited)
            {
                DebugLogger.Log($"[MountZipFiles] Attempting to unmount by terminating {_zipMountExecutableName} (ID: {mountProcessId}).");
                try
                {
                    mountProcess.Kill(true);
                    DebugLogger.Log($"[MountZipFiles] Kill signal sent to {_zipMountExecutableName} (ID: {mountProcessId}). Waiting for process to exit (up to 10s).");
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    try
                    {
                        await mountProcess.WaitForExitAsync(cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        DebugLogger.Log($"[MountZipFiles] Timeout (10s) waiting for {_zipMountExecutableName} (ID: {mountProcessId}) to exit after Kill.");
                    }

                    if (mountProcess.HasExited)
                    {
                        DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} (ID: {mountProcessId}) terminated. Exit code: {mountProcess.ExitCode}.");
                    }
                    else
                    {
                        DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} (ID: {mountProcessId}) did NOT terminate after Kill signal and 10s wait.");
                    }
                }
                catch (InvalidOperationException ioEx)
                {
                    if (ioEx.Message.Contains("process has already exited", StringComparison.OrdinalIgnoreCase) ||
                        ioEx.Message.Contains("No process is associated", StringComparison.OrdinalIgnoreCase))
                    {
                        DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} (ID: {mountProcessId}) already exited or no process associated: {ioEx.Message}");
                    }
                    else
                    {
                        DebugLogger.Log($"[MountZipFiles] InvalidOperationException while terminating {_zipMountExecutableName} (ID: {mountProcessId}): {ioEx}");

                        // Notify developer
                        _ = LogErrors.LogErrorAsync(ioEx, $"Unexpected InvalidOperationException during {_zipMountExecutableName} termination.");
                    }
                }
                catch (Exception termEx)
                {
                    DebugLogger.Log($"[MountZipFiles] Exception while terminating {_zipMountExecutableName} (ID: {mountProcessId}): {termEx}");

                    // Notify developer
                    _ = LogErrors.LogErrorAsync(termEx, $"Failed to terminate {_zipMountExecutableName} (ID: {mountProcessId}) for unmounting.");
                }
            }
            else if (mountProcessId != -1)
            {
                DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} (ID: {mountProcessId}) had already exited or was not running when finally cleanup was attempted. Exit code likely {(mountProcess != null && mountProcess.HasExited ? mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A")}.");
            }
            else
            {
                DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} process was not started successfully (ID: {mountProcessId}). No termination needed.");
            }

            mountProcess?.Dispose();

            await Task.Delay(2000);
            // Use mountDriveRootForChecks for Directory.Exists
            if (Directory.Exists(mountDriveRootForChecks))
            {
                DebugLogger.Log($"[MountZipFiles] WARNING: Drive {mountDriveRootForChecks} still exists after attempting to unmount. {_zipMountExecutableName} might not have unmounted correctly or is still running.");
            }
            else
            {
                DebugLogger.Log($"[MountZipFiles] Drive {mountDriveRootForChecks} successfully unmounted (or was not detected).");
            }
        }
    }

    public static async Task MountZipFileAndSearchForFileToLoad(
        string resolvedZipFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        SystemManager selectedSystemManager,
        SystemManager.Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        MainWindow mainWindow,
        string logPath)
    {
        DebugLogger.Log($"[MountZipFiles] Starting to mount ZIP for nested file search: {resolvedZipFilePath}");
        DebugLogger.Log($"[MountZipFiles] System: {selectedSystemName}, Emulator: {selectedEmulatorName}");

        var resolvedZipMountExePath = PathHelper.ResolveRelativeToAppDirectory(_zipMountExecutableRelativePath);

        DebugLogger.Log($"[MountZipFiles] Path to {_zipMountExecutableName}: {resolvedZipMountExePath}");

        if (string.IsNullOrWhiteSpace(resolvedZipMountExePath) || !File.Exists(resolvedZipMountExePath))
        {
            // Notify developer
            var errorMessage = $"{_zipMountExecutableName} not found at {_zipMountExecutableRelativePath}. Cannot mount ZIP.";
            DebugLogger.Log($"[MountZipFiles] Error: {errorMessage}");
            _ = LogErrors.LogErrorAsync(null, errorMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);

            return;
        }

        var mountPathArgument = ConfiguredMountDriveLetter;
        var mountDriveRootForChecks = ConfiguredMountDriveRoot;

        var psiMount = new ProcessStartInfo
        {
            FileName = resolvedZipMountExePath,
            Arguments = $"\"{resolvedZipFilePath}\" \"{mountPathArgument}\"",
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = false,
            WorkingDirectory = Path.GetDirectoryName(resolvedZipMountExePath) ?? AppDomain.CurrentDomain.BaseDirectory
        };

        DebugLogger.Log($"[MountZipFiles] ProcessStartInfo for {_zipMountExecutableName}:");
        DebugLogger.Log($"[MountZipFiles] FileName: {psiMount.FileName}");
        DebugLogger.Log($"[MountZipFiles] Arguments: {psiMount.Arguments}");
        DebugLogger.Log($"[MountZipFiles] WorkingDirectory: {psiMount.WorkingDirectory}");

        Process mountProcess = null;
        var mountProcessId = -1;

        try
        {
            mountProcess = new Process { StartInfo = psiMount, EnableRaisingEvents = true };

            DebugLogger.Log($"[MountZipFiles] Starting {_zipMountExecutableName} process...");
            if (!mountProcess.Start())
            {
                throw new InvalidOperationException($"Failed to start the {_zipMountExecutableName} process.");
            }

            mountProcessId = mountProcess.Id;
            DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} process started (ID: {mountProcessId}).");

            // Polling mechanism to wait for the mount to complete.
            var mountSuccessful = false;
            var timeout = TimeSpan.FromMinutes(1);
            var pollInterval = TimeSpan.FromMilliseconds(500);
            var stopwatch = Stopwatch.StartNew();

            DebugLogger.Log($"[MountZipFiles] Polling for drive '{mountDriveRootForChecks}' to appear (timeout: {timeout.TotalSeconds}s)...");

            while (stopwatch.Elapsed < timeout)
            {
                if (Directory.Exists(mountDriveRootForChecks))
                {
                    mountSuccessful = true;
                    DebugLogger.Log($"[MountZipFiles] Found drive '{mountDriveRootForChecks}' after {stopwatch.Elapsed.TotalSeconds:F1} seconds.");
                    break;
                }

                if (mountProcess.HasExited)
                {
                    DebugLogger.Log($"[MountZipFiles] Mount process {_zipMountExecutableName} (ID: {mountProcessId}) exited prematurely during polling. Exit Code: {mountProcess.ExitCode}.");
                    break;
                }

                await Task.Delay(pollInterval);
            }

            stopwatch.Stop();

            if (!mountSuccessful)
            {
                var exitCodeInfoOnFailure = mountProcess.HasExited ? $"The process exited with code {mountProcess.ExitCode}." : "The process was still running.";
                DebugLogger.Log($"[MountZipFiles] Mount check failed. Drive {mountDriveRootForChecks} not found. {exitCodeInfoOnFailure} Check the console window of {_zipMountExecutableName} for details.");
                throw new Exception($"Failed to mount ZIP. Drive {mountDriveRootForChecks} not found after timeout.");
            }

            DebugLogger.Log($"[MountZipFiles] Drive {mountDriveRootForChecks} detected. Searching for nested file...");
            var fileToLoad = FindNestedFile(mountDriveRootForChecks);

            if (string.IsNullOrEmpty(fileToLoad))
            {
                DebugLogger.Log($"[MountZipFiles] No suitable file found in nested directory structure in {mountDriveRootForChecks}.");
                throw new FileNotFoundException($"Could not find a file to launch within the expected nested directory structure of the mounted ZIP at {mountDriveRootForChecks}.");
            }

            DebugLogger.Log($"[MountZipFiles] Nested file found at: {fileToLoad}. Proceeding to launch with {selectedEmulatorName}.");
            await GameLauncher.LaunchRegularEmulator(fileToLoad, selectedSystemName, selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters, mainWindow);
            DebugLogger.Log($"[MountZipFiles] Emulator for {fileToLoad} has exited.");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[MountZipFiles] Exception during ZIP mounting or launching: {ex}");

            // Notify developer
            var exitCodeInfoInCatch = mountProcess != null && mountProcess.HasExited ? $"Exit Code: {mountProcess.ExitCode}" : "Process was still running or state unknown.";
            var contextMessage = $"Error during ZIP mount/launch process for {resolvedZipFilePath}.\n" +
                                 $"Exception: {ex.Message}\n" +
                                 $"The tool's output was not redirected. {exitCodeInfoInCatch}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);
        }
        finally
        {
            DebugLogger.Log($"[MountZipFiles] Entering finally block for {resolvedZipFilePath}. Mount Process ID: {mountProcessId}");
            if (mountProcess != null && mountProcessId != -1 && !mountProcess.HasExited)
            {
                DebugLogger.Log($"[MountZipFiles] Attempting to unmount by terminating {_zipMountExecutableName} (ID: {mountProcessId}).");
                try
                {
                    mountProcess.Kill(true);
                    DebugLogger.Log($"[MountZipFiles] Kill signal sent to {_zipMountExecutableName} (ID: {mountProcessId}). Waiting for process to exit (up to 10s).");
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    try
                    {
                        await mountProcess.WaitForExitAsync(cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        DebugLogger.Log($"[MountZipFiles] Timeout (10s) waiting for {_zipMountExecutableName} (ID: {mountProcessId}) to exit after Kill.");
                    }

                    if (mountProcess.HasExited)
                    {
                        DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} (ID: {mountProcessId}) terminated. Exit code: {mountProcess.ExitCode}.");
                    }
                    else
                    {
                        DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} (ID: {mountProcessId}) did NOT terminate after Kill signal and 10s wait.");
                    }
                }
                catch (Exception termEx)
                {
                    DebugLogger.Log($"[MountZipFiles] Exception while terminating {_zipMountExecutableName} (ID: {mountProcessId}): {termEx}");

                    // Notify developer
                    _ = LogErrors.LogErrorAsync(termEx, $"Failed to terminate {_zipMountExecutableName} (ID: {mountProcessId}) for unmounting.");
                }
            }
            else if (mountProcessId != -1)
            {
                DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} (ID: {mountProcessId}) had already exited or was not running when finally cleanup was attempted.");
            }
            else
            {
                DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} process was not started successfully (ID: {mountProcessId}). No termination needed.");
            }

            mountProcess?.Dispose();

            await Task.Delay(2000);
            if (Directory.Exists(mountDriveRootForChecks))
            {
                DebugLogger.Log($"[MountZipFiles] WARNING: Drive {mountDriveRootForChecks} still exists after attempting to unmount.");
            }
            else
            {
                DebugLogger.Log($"[MountZipFiles] Drive {mountDriveRootForChecks} successfully unmounted.");
            }
        }
    }

    private static string FindNestedFile(string directoryPath)
    {
        const string targetFolderName = "000D0000";
        try
        {
            DebugLogger.Log($"[FindNestedFile] Searching for directory '{targetFolderName}' in {directoryPath}...");
            var targetDirs = Directory.GetDirectories(directoryPath, targetFolderName, SearchOption.AllDirectories);

            if (targetDirs.Length > 0)
            {
                var nestedDirPath = targetDirs[0];
                DebugLogger.Log($"[FindNestedFile] Found directory at: {nestedDirPath}. Searching for first file inside...");

                var filesInNestedDir = Directory.GetFiles(nestedDirPath, "*", SearchOption.TopDirectoryOnly);
                if (filesInNestedDir.Length > 0)
                {
                    var fileToLaunch = filesInNestedDir[0];
                    DebugLogger.Log($"[FindNestedFile] Found file to launch in nested directory: {fileToLaunch}");
                    return fileToLaunch;
                }

                DebugLogger.Log(
                    $"[FindNestedFile] Directory '{nestedDirPath}' was found but is empty. Will check other folders.");
            }
            else
            {
                DebugLogger.Log(
                    $"[FindNestedFile] Directory '{targetFolderName}' not found in {directoryPath}. Will check other folders.");
            }

            // Check other folders if the nested folder doesn't exist or is empty
            var filesInRootDir = Directory.GetFiles(directoryPath, "*", SearchOption.AllDirectories);
            if (filesInRootDir.Length > 0)
            {
                var fileToLaunch = filesInRootDir[0];
                DebugLogger.Log($"[FindNestedFile] Found file to launch: {fileToLaunch}");
                return fileToLaunch;
            }

            DebugLogger.Log(
                $"[FindNestedFile] No files found in nested directory '{targetFolderName}' or inside other folders.");
            return null;
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[FindNestedFile] Error searching for nested file in {directoryPath}: {ex.Message}");

            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, $"Error in FindNestedFile searching {directoryPath}");

            return null;
        }
    }

    public static async Task MountZipFileAndLoadWithScummVm(
        string resolvedZipFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        SystemManager selectedSystemManager,
        SystemManager.Emulator selectedEmulatorManager,
        string selectedEmulatorParameters,
        string logPath)
    {
        DebugLogger.Log($"[MountZipFiles] Starting to mount ZIP for ScummVM: {resolvedZipFilePath}");
        DebugLogger.Log($"[MountZipFiles] System: {selectedSystemName}, Emulator: {selectedEmulatorName}");

        var resolvedZipMountExePath = PathHelper.ResolveRelativeToAppDirectory(_zipMountExecutableRelativePath);

        DebugLogger.Log($"[MountZipFiles] Path to {_zipMountExecutableName}: {resolvedZipMountExePath}");

        if (string.IsNullOrWhiteSpace(resolvedZipMountExePath) || !File.Exists(resolvedZipMountExePath))
        {
            // Notify developer
            var errorMessage = $"{_zipMountExecutableName} not found at {_zipMountExecutableRelativePath}. Cannot mount ZIP.";
            DebugLogger.Log($"[MountZipFiles] Error: {errorMessage}");
            _ = LogErrors.LogErrorAsync(null, errorMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);

            return;
        }

        var mountPathArgument = ConfiguredMountDriveLetter;
        var mountDriveRootForChecks = ConfiguredMountDriveRoot;

        var psiMount = new ProcessStartInfo
        {
            FileName = resolvedZipMountExePath,
            Arguments = $"\"{resolvedZipFilePath}\" \"{mountPathArgument}\"",
            UseShellExecute = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = false,
            WorkingDirectory = Path.GetDirectoryName(resolvedZipMountExePath) ?? AppDomain.CurrentDomain.BaseDirectory
        };

        DebugLogger.Log($"[MountZipFiles] ProcessStartInfo for {_zipMountExecutableName}:");
        DebugLogger.Log($"[MountZipFiles] FileName: {psiMount.FileName}");
        DebugLogger.Log($"[MountZipFiles] Arguments: {psiMount.Arguments}");
        DebugLogger.Log($"[MountZipFiles] WorkingDirectory: {psiMount.WorkingDirectory}");

        Process mountProcess = null;
        var mountProcessId = -1;

        try
        {
            mountProcess = new Process { StartInfo = psiMount, EnableRaisingEvents = true };

            DebugLogger.Log($"[MountZipFiles] Starting {_zipMountExecutableName} process...");
            if (!mountProcess.Start())
            {
                throw new InvalidOperationException($"Failed to start the {_zipMountExecutableName} process.");
            }

            mountProcessId = mountProcess.Id;
            DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} process started (ID: {mountProcessId}).");

            // Polling mechanism to wait for the mount to complete.
            var mountSuccessful = false;
            var timeout = TimeSpan.FromMinutes(1);
            var pollInterval = TimeSpan.FromMilliseconds(500);
            var stopwatch = Stopwatch.StartNew();

            DebugLogger.Log($"[MountZipFiles] Polling for drive '{mountDriveRootForChecks}' to appear (timeout: {timeout.TotalSeconds}s)...");

            while (stopwatch.Elapsed < timeout)
            {
                if (Directory.Exists(mountDriveRootForChecks))
                {
                    mountSuccessful = true;
                    DebugLogger.Log($"[MountZipFiles] Found drive '{mountDriveRootForChecks}' after {stopwatch.Elapsed.TotalSeconds:F1} seconds.");
                    break;
                }

                if (mountProcess.HasExited)
                {
                    DebugLogger.Log($"[MountZipFiles] Mount process {_zipMountExecutableName} (ID: {mountProcessId}) exited prematurely during polling. Exit Code: {mountProcess.ExitCode}.");
                    break;
                }

                await Task.Delay(pollInterval);
            }

            stopwatch.Stop();

            if (!mountSuccessful)
            {
                var exitCodeInfoOnFailure = mountProcess.HasExited ? $"The process exited with code {mountProcess.ExitCode}." : "The process was still running.";
                DebugLogger.Log($"[MountZipFiles] Mount check failed. Drive {mountDriveRootForChecks} not found. {exitCodeInfoOnFailure} Check the console window of {_zipMountExecutableName} for details.");
                throw new Exception($"Failed to mount ZIP. Drive {mountDriveRootForChecks} not found after timeout.");
            }

            DebugLogger.Log($"[MountZipFiles] Drive {mountDriveRootForChecks} detected. Proceeding to launch with {selectedEmulatorName}.");

            // --- Custom ScummVM Launch Logic ---

            // 1. Resolve Emulator Path
            var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(selectedEmulatorManager.EmulatorLocation);
            if (string.IsNullOrEmpty(resolvedEmulatorExePath) || !File.Exists(resolvedEmulatorExePath))
            {
                throw new FileNotFoundException($"Emulator executable not found: {selectedEmulatorManager.EmulatorLocation}");
            }

            var resolvedEmulatorFolderPath = Path.GetDirectoryName(resolvedEmulatorExePath);
            if (string.IsNullOrEmpty(resolvedEmulatorFolderPath))
            {
                throw new FileNotFoundException("Emulator executable folder could not be determined");
            }

            var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(selectedSystemManager.PrimarySystemFolder);

            // 2. Resolve Parameters
            var resolvedParameters = ParameterValidator.ResolveParameterString(
                selectedEmulatorParameters,
                resolvedSystemFolderPath,
                resolvedEmulatorFolderPath
            );

            var fixedMountDriveRootForChecks = mountDriveRootForChecks.TrimEnd('\\'); // Remove '\'
            var arguments = $"-p \"{fixedMountDriveRootForChecks}\" {resolvedParameters} ";

            var psiEmulator = new ProcessStartInfo
            {
                FileName = resolvedEmulatorExePath,
                Arguments = arguments,
                WorkingDirectory = resolvedEmulatorFolderPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            DebugLogger.Log($"[MountZipFiles] Launching ScummVM with mounted ZIP:\n\n" +
                            $"Program Location: {psiEmulator.FileName}\n" +
                            $"Arguments: {psiEmulator.Arguments}\n" +
                            $"Working Directory: {psiEmulator.WorkingDirectory}");

            // 3. Launch Emulator
            using (var emulatorProcess = new Process())
            {
                emulatorProcess.StartInfo = psiEmulator;
                emulatorProcess.Start();
                await emulatorProcess.WaitForExitAsync();
                DebugLogger.Log($"[MountZipFiles] ScummVM process has exited with code: {emulatorProcess.ExitCode}.");
            }

            DebugLogger.Log($"[MountZipFiles] Emulator for {mountDriveRootForChecks} has exited.");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[MountZipFiles] Exception during ScummVM ZIP mounting or launching: {ex}");

            // Notify developer
            var exitCodeInfoInCatch = mountProcess != null && mountProcess.HasExited ? $"Exit Code: {mountProcess.ExitCode}" : "Process was still running or state unknown.";
            var contextMessage = $"Error during ScummVM ZIP mount/launch process for {resolvedZipFilePath}.\n" +
                                 $"Exception: {ex.Message}\n" +
                                 $"The tool's output was not redirected. {exitCodeInfoInCatch}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);
        }
        finally
        {
            DebugLogger.Log($"[MountZipFiles] Entering finally block for {resolvedZipFilePath}. Mount Process ID: {mountProcessId}");
            if (mountProcess != null && mountProcessId != -1 && !mountProcess.HasExited)
            {
                DebugLogger.Log($"[MountZipFiles] Attempting to unmount by terminating {_zipMountExecutableName} (ID: {mountProcessId}).");
                try
                {
                    mountProcess.Kill(true);
                    DebugLogger.Log($"[MountZipFiles] Kill signal sent to {_zipMountExecutableName} (ID: {mountProcessId}). Waiting for process to exit (up to 10s).");
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                    try
                    {
                        await mountProcess.WaitForExitAsync(cts.Token);
                    }
                    catch (TaskCanceledException)
                    {
                        DebugLogger.Log($"[MountZipFiles] Timeout (10s) waiting for {_zipMountExecutableName} (ID: {mountProcessId}) to exit after Kill.");
                    }

                    if (mountProcess.HasExited)
                    {
                        DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} (ID: {mountProcessId}) terminated. Exit code: {mountProcess.ExitCode}.");
                    }
                    else
                    {
                        DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} (ID: {mountProcessId}) did NOT terminate after Kill signal and 10s wait.");
                    }
                }
                catch (Exception termEx)
                {
                    DebugLogger.Log($"[MountZipFiles] Exception while terminating {_zipMountExecutableName} (ID: {mountProcessId}): {termEx}");

                    // Notify developer
                    _ = LogErrors.LogErrorAsync(termEx, $"Failed to terminate {_zipMountExecutableName} (ID: {mountProcessId}) for unmounting.");
                }
            }
            else if (mountProcessId != -1)
            {
                DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} (ID: {mountProcessId}) had already exited or was not running when finally cleanup was attempted.");
            }
            else
            {
                DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} process was not started successfully (ID: {mountProcessId}). No termination needed.");
            }

            mountProcess?.Dispose();

            await Task.Delay(2000);
            if (Directory.Exists(mountDriveRootForChecks))
            {
                DebugLogger.Log($"[MountZipFiles] WARNING: Drive {mountDriveRootForChecks} still exists after attempting to unmount.");
            }
            else
            {
                DebugLogger.Log($"[MountZipFiles] Drive {mountDriveRootForChecks} successfully unmounted.");
            }
        }
    }
}
