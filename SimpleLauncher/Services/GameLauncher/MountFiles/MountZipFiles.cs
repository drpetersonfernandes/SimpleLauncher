using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using SharpCompress.Archives;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SystemManager;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

internal static class MountZipFiles
{
    private static string _preferredMountDriveLetterOnly = "Z";
    private static string _zipMountExecutableName;
    private static string _zipMountExecutableRelativePath;
    internal static string ConfiguredMountDriveRoot => _preferredMountDriveLetterOnly + ":\\";


    private static void ValidateZipForPathTraversal(string archivePath)
    {
        if (!File.Exists(archivePath))
        {
            throw new FileNotFoundException($"Compressed file not found: {archivePath}");
        }

        try
        {
            using var archive = ArchiveFactory.OpenArchive(archivePath);

            foreach (var entry in archive.Entries)
            {
                // Skip directory entries
                if (entry.IsDirectory)
                    continue;

                var entryName = entry.Key;

                if (string.IsNullOrEmpty(entryName))
                    continue;

                // Check for common path traversal indicators
                if (entryName.Contains("..") ||
                    Path.IsPathRooted(entryName) ||
                    entryName.StartsWith('/') ||
                    entryName.StartsWith('\\'))
                {
                    throw new InvalidOperationException($"Archive contains path traversal entry: '{entryName}'");
                }

                // Additional thorough check: simulate extraction path normalization
                var normalizedEntryName = entryName.Replace('/', Path.DirectorySeparatorChar);
                var simulatedFullPath = Path.GetFullPath(Path.Combine("D:\\MOCKROOT", normalizedEntryName));
                if (!simulatedFullPath.StartsWith("D:\\MOCKROOT", StringComparison.Ordinal))
                {
                    throw new InvalidOperationException($"Archive entry escapes simulated root: '{entryName}' -> '{simulatedFullPath}'");
                }
            }
        }
        catch (Exception ex) when (ex is InvalidOperationException)
        {
            // Re-throw our own exceptions (path traversal errors)
            throw;
        }
        catch (Exception ex)
        {
            // Wrap other exceptions (corrupted archive, etc.) as InvalidOperationException
            throw new InvalidOperationException($"Invalid or corrupted archive file: {ex.Message}", ex);
        }
    }


    internal static void Configure(IConfiguration configuration, ILogErrors logErrors)
    {
        var mountPathFromConfig = configuration.GetValue("ZipMountOptions:MountDriveLetter", "Z:");

        // Determine the correct executable based on architecture
        _zipMountExecutableName = GetArchitectureSpecificExecutableName();
        _zipMountExecutableRelativePath = Path.Combine("tools", "SimpleZipDrive", _zipMountExecutableName);

        if (string.IsNullOrEmpty(mountPathFromConfig))
        {
            mountPathFromConfig = "Z:"; // Fallback
        }

        // Extract just the drive letter
        if (mountPathFromConfig.EndsWith(":\\", StringComparison.Ordinal))
        {
            _preferredMountDriveLetterOnly = mountPathFromConfig.Substring(0, mountPathFromConfig.Length - 2);
        }
        else if (mountPathFromConfig.EndsWith(':'))
        {
            _preferredMountDriveLetterOnly = mountPathFromConfig.Substring(0, mountPathFromConfig.Length - 1);
        }
        else // Assume it's just the letter or an invalid format, try to take the first char if it's a letter
        {
            _preferredMountDriveLetterOnly = mountPathFromConfig.Length > 0 && char.IsLetter(mountPathFromConfig[0])
                ? mountPathFromConfig[0].ToString().ToUpperInvariant()
                : "Z";
        }

        DebugLogger.Log($"[MountZipFiles] Preferred MountDriveLetter (for {_zipMountExecutableName}): {_preferredMountDriveLetterOnly}");
        DebugLogger.Log($"[MountZipFiles] Configured ZipMountExecutableName: {_zipMountExecutableName}");
        DebugLogger.Log($"[MountZipFiles] Configured ZipMountExecutableRelativePath: {_zipMountExecutableRelativePath}");
    }

    private static string GetArchitectureSpecificExecutableName()
    {
        var arch = RuntimeInformation.ProcessArchitecture;

        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        return arch switch
        {
            Architecture.X64 => "SimpleZipDrive.exe",
            Architecture.Arm64 => "SimpleZipDrive_arm64.exe",
            _ => throw new PlatformNotSupportedException($"Architecture {arch} is not supported by SimpleZipDrive.")
        };
    }

    private static string GetExitCodeReason(int exitCode)
    {
        return exitCode switch
        {
            -1073741515 => "STATUS_DLL_NOT_FOUND (Dokan library is not installed)",
            -1073741510 => "STATUS_ORDINAL_NOT_FOUND (Dokan library version mismatch — the installed version may be incompatible)",
            _ => "unknown error"
        };
    }

    /// <summary>
    /// Finds an available drive letter, preferring the configured letter, then searching from Z: down to D:.
    /// </summary>
    /// <returns>An available character for a drive letter, or null if none are available.</returns>
    private static char? GetAvailableDriveLetter(ILogErrors logErrors)
    {
        try
        {
            // Use Environment.GetLogicalDrives() to avoid hanging on disconnected network drives
            var existingDrives = Environment.GetLogicalDrives()
                .Select(static d => char.ToUpper(d[0], CultureInfo.InvariantCulture))
                .ToHashSet();

            // First, try the preferred drive letter from configuration
            var preferredLetter = char.ToUpper(_preferredMountDriveLetterOnly[0], CultureInfo.InvariantCulture);
            if (!existingDrives.Contains(preferredLetter))
            {
                DebugLogger.Log($"[MountZipFiles.GetAvailableDriveLetter] Preferred drive letter {preferredLetter}: is available.");
                return preferredLetter;
            }

            DebugLogger.Log($"[MountZipFiles.GetAvailableDriveLetter] Preferred drive letter {preferredLetter}: is already in use. Searching for alternative...");

            // If preferred is not available, search from Z: down to D:
            for (var letter = 'Z'; letter >= 'D'; letter--)
            {
                if (!existingDrives.Contains(letter))
                {
                    DebugLogger.Log($"[MountZipFiles.GetAvailableDriveLetter] Found available drive letter: {letter}:");
                    return letter;
                }
            }

            DebugLogger.Log("[MountZipFiles.GetAvailableDriveLetter] No available drive letters found between D: and Z:.");
            return null;
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[MountZipFiles.GetAvailableDriveLetter] Error enumerating drives: {ex.Message}");
            _ = logErrors.LogErrorAsync(ex, "Error enumerating available drive letters for ZIP mounting.");
            return null;
        }
    }

    private static void KillAllSimpleZipDriveProcesses(ILogErrors logErrors)
    {
        try
        {
            var processNames = new[] { "SimpleZipDrive", "SimpleZipDrive_arm64" };
            foreach (var processName in processNames)
            {
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length == 0) continue;

                DebugLogger.Log($"[MountZipFiles.KillAllSimpleZipDriveProcesses] Found {processes.Length} {processName} process(es) to kill.");

                foreach (var process in processes)
                {
                    try
                    {
                        if (!process.HasExited)
                        {
                            DebugLogger.Log($"[MountZipFiles.KillAllSimpleZipDriveProcesses] Killing {processName} (ID: {process.Id}).");
                            process.Kill(true);
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugLogger.Log($"[MountZipFiles.KillAllSimpleZipDriveProcesses] Error killing process {process.Id}: {ex.Message}");
                        logErrors.LogErrorAsync(ex, $"[MountZipFiles.KillAllSimpleZipDriveProcesses] Error killing process {process.Id}: {ex.Message}");
                    }
                    finally
                    {
                        process.Dispose();
                    }
                }
            }
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[MountZipFiles.KillAllSimpleZipDriveProcesses] Error enumerating processes: {ex.Message}");
        }
    }

    internal static async Task MountZipFileAndLoadEbootBinAsync(
        string resolvedZipFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        SystemManager.SystemManager selectedSystemManager,
        Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        MainWindow mainWindow,
        string logPath,
        GameLauncher gameLauncher,
        ILogErrors logErrors)
    {
        DebugLogger.Log($"[MountZipFiles] Starting to mount ZIP for EBOOT.BIN: {resolvedZipFilePath}");
        DebugLogger.Log($"[MountZipFiles] System: {selectedSystemName}, Emulator: {selectedEmulatorName}");

        ValidateZipForPathTraversal(resolvedZipFilePath);

        var resolvedZipMountExePath = PathHelper.ResolveRelativeToAppDirectory(_zipMountExecutableRelativePath);

        DebugLogger.Log($"[MountZipFiles] Path to {_zipMountExecutableName}: {resolvedZipMountExePath}");

        if (string.IsNullOrWhiteSpace(resolvedZipMountExePath) || !File.Exists(resolvedZipMountExePath))
        {
            // Notify developer
            var errorMessage = $"{_zipMountExecutableName} not found at {_zipMountExecutableRelativePath}. Cannot mount ZIP.";
            DebugLogger.Log($"[MountZipFiles] Error: {errorMessage}");
            _ = logErrors.LogErrorAsync(null, errorMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFileMessageBox();

            return;
        }

        if (!DokanValidation.IsDokanInstalled())
        {
            const string errorMessage = "Dokan driver not found. Cannot mount ZIP.";
            DebugLogger.Log($"[MountZipFiles] Error: {errorMessage}");
            _ = logErrors.LogErrorAsync(null, errorMessage);
            MessageBoxLibrary.DokanDriverNotInstalledMessageBox();
            return;
        }

        // Get an available drive letter dynamically
        var driveLetter = GetAvailableDriveLetter(logErrors);
        if (driveLetter == null)
        {
            const string errorMessage = "No available drive letters found to mount the ZIP.";
            DebugLogger.Log($"[MountZipFiles] Error: {errorMessage}");
            _ = logErrors.LogErrorAsync(null, errorMessage);
            MessageBoxLibrary.ThereWasAnErrorMountingTheFileMessageBox();
            return;
        }

        var mountPathArgument = driveLetter.Value.ToString().ToLowerInvariant(); // SimpleZipDrive expects lowercase letter
        var mountDriveRootForChecks = $"{driveLetter.Value}:\\"; // For Directory.Exists checks

        DebugLogger.Log($"[MountZipFiles] Selected drive letter for mounting: {driveLetter.Value}:");

        var psiMount = new ProcessStartInfo
        {
            FileName = resolvedZipMountExePath,
            // SimpleZipDrive uses positional arguments: "<PathToZipFile>" "<MountPoint>"
            Arguments = $"\"{resolvedZipFilePath}\" \"{mountPathArgument}\"",
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
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
                    var exitCode = mountProcess.ExitCode;
                    DebugLogger.Log($"[MountZipFiles] Mount process {_zipMountExecutableName} (ID: {mountProcessId}) exited prematurely during polling. Exit Code: {exitCode}.");
                    break;
                }

                await Task.Delay(pollInterval);
            }

            stopwatch.Stop();

            if (!mountSuccessful)
            {
                if (mountProcess.HasExited)
                {
                    var exitCode = mountProcess.ExitCode;
                    var reason = GetExitCodeReason(exitCode);
                    DebugLogger.Log($"[MountZipFiles] Mount check failed. Drive {mountDriveRootForChecks} not found. The process exited with code {exitCode} ({reason}).");
                    throw new InvalidOperationException($"Failed to mount ZIP. {_zipMountExecutableName} exited with code {exitCode} ({reason}).");
                }
                else
                {
                    DebugLogger.Log($"[MountZipFiles] Mount check failed. Drive {mountDriveRootForChecks} not found. The process was still running after timeout. Check the console window of {_zipMountExecutableName} for details.");
                    throw new TimeoutException($"Failed to mount ZIP. Drive {mountDriveRootForChecks} not found after timeout.");
                }
            }

            DebugLogger.Log($"[MountZipFiles] Drive {mountDriveRootForChecks} detected. Searching for EBOOT.BIN...");

            // Find EBOOT.BIN
            var ebootBinPath = FindEbootBin.FindEbootBinRecursive(mountDriveRootForChecks, logErrors);

            if (string.IsNullOrEmpty(ebootBinPath))
            {
                DebugLogger.Log($"[MountZipFiles] EBOOT.BIN not found in {mountDriveRootForChecks}.");
                throw new FileNotFoundException($"EBOOT.BIN not found within the mounted ZIP file at {mountDriveRootForChecks}.");
            }

            DebugLogger.Log($"[MountZipFiles] EBOOT.BIN found at: {ebootBinPath}. Proceeding to launch with {selectedEmulatorName}.");

            // Pass the original ZIP file path for display in notifications
            await gameLauncher.LaunchRegularEmulatorAsync(ebootBinPath, selectedEmulatorName, selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters, mainWindow, mainWindow, resolvedZipFilePath);

            DebugLogger.Log($"[MountZipFiles] Emulator for {ebootBinPath} has exited.");
        }
        catch (Exception ex)
        {
            // Notify developer
            DebugLogger.Log($"[MountZipFiles] Exception during ZIP mounting or launching: {ex}");
            var exitCodeInfoInCatch = mountProcess is { HasExited: true } ? $"Exit Code: {mountProcess.ExitCode}" : "Process was still running or state unknown.";
            var contextMessage = $"Error during ZIP mount/launch process for {resolvedZipFilePath}.\n" +
                                 $"Exception: {ex.Message}\n" +
                                 $"The tool's output was not redirected. {exitCodeInfoInCatch}";
            _ = logErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFileMessageBox(mountProcess is { HasExited: true } ? mountProcess.ExitCode : null);
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
                    DebugLogger.Log($"[MountZipFiles] Kill signal sent to {_zipMountExecutableName} (ID: {mountProcessId}). Waiting for process to exit (up to 20s).");
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
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
                        DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} (ID: {mountProcessId}) terminated. Exit code: {mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture)}.");
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
                        _ = logErrors.LogErrorAsync(ioEx, $"Unexpected InvalidOperationException during {_zipMountExecutableName} termination.");
                    }
                }
                catch (Exception termEx)
                {
                    DebugLogger.Log($"[MountZipFiles] Exception while terminating {_zipMountExecutableName} (ID: {mountProcessId}): {termEx}");

                    // Notify developer
                    _ = logErrors.LogErrorAsync(termEx, $"Failed to terminate {_zipMountExecutableName} (ID: {mountProcessId}) for unmounting.");
                }
            }
            else if (mountProcessId != -1)
            {
                var exitCodeStr = mountProcess is { HasExited: true } ? mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A";
                DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} (ID: {mountProcessId}) had already exited or was not running when finally cleanup was attempted. Exit code likely {exitCodeStr}.");
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

            // Safety net: ensure all SimpleZipDrive processes are killed
            KillAllSimpleZipDriveProcesses(logErrors);
        }
    }

    public static async Task MountZipFileAndSearchForFileToLoadAsync(
        string resolvedZipFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        SystemManager.SystemManager selectedSystemManager,
        Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        MainWindow mainWindow,
        string logPath,
        GameLauncher gameLauncher,
        ILogErrors logErrors)
    {
        DebugLogger.Log($"[MountZipFiles] Starting to mount ZIP for nested file search: {resolvedZipFilePath}");
        DebugLogger.Log($"[MountZipFiles] System: {selectedSystemName}, Emulator: {selectedEmulatorName}");

        ValidateZipForPathTraversal(resolvedZipFilePath);

        var resolvedZipMountExePath = PathHelper.ResolveRelativeToAppDirectory(_zipMountExecutableRelativePath);

        DebugLogger.Log($"[MountZipFiles] Path to {_zipMountExecutableName}: {resolvedZipMountExePath}");

        if (string.IsNullOrWhiteSpace(resolvedZipMountExePath) || !File.Exists(resolvedZipMountExePath))
        {
            // Notify developer
            var errorMessage = $"{_zipMountExecutableName} not found at {_zipMountExecutableRelativePath}. Cannot mount ZIP.";
            DebugLogger.Log($"[MountZipFiles] Error: {errorMessage}");
            _ = logErrors.LogErrorAsync(null, errorMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFileMessageBox();

            return;
        }

        if (!DokanValidation.IsDokanInstalled())
        {
            const string errorMessage = "Dokan driver not found. Cannot mount ZIP.";
            DebugLogger.Log($"[MountZipFiles] Error: {errorMessage}");
            _ = logErrors.LogErrorAsync(null, errorMessage);
            MessageBoxLibrary.DokanDriverNotInstalledMessageBox();
            return;
        }

        // Get an available drive letter dynamically
        var driveLetter = GetAvailableDriveLetter(logErrors);
        if (driveLetter == null)
        {
            const string errorMessage = "No available drive letters found to mount the ZIP.";
            DebugLogger.Log($"[MountZipFiles] Error: {errorMessage}");
            _ = logErrors.LogErrorAsync(null, errorMessage);
            MessageBoxLibrary.ThereWasAnErrorMountingTheFileMessageBox();
            return;
        }

        var mountPathArgument = driveLetter.Value.ToString().ToLowerInvariant();
        var mountDriveRootForChecks = $"{driveLetter.Value}:\\";

        DebugLogger.Log($"[MountZipFiles] Selected drive letter for mounting: {driveLetter.Value}:");

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
                    var exitCode = mountProcess.ExitCode;
                    DebugLogger.Log($"[MountZipFiles] Mount process {_zipMountExecutableName} (ID: {mountProcessId}) exited prematurely during polling. Exit Code: {exitCode}.");
                    break;
                }

                await Task.Delay(pollInterval);
            }

            stopwatch.Stop();

            if (!mountSuccessful)
            {
                if (mountProcess.HasExited)
                {
                    var exitCode = mountProcess.ExitCode;
                    var reason = GetExitCodeReason(exitCode);
                    DebugLogger.Log($"[MountZipFiles] Mount check failed. Drive {mountDriveRootForChecks} not found. The process exited with code {exitCode} ({reason}).");
                    throw new InvalidOperationException($"Failed to mount ZIP. {_zipMountExecutableName} exited with code {exitCode} ({reason}).");
                }
                else
                {
                    DebugLogger.Log($"[MountZipFiles] Mount check failed. Drive {mountDriveRootForChecks} not found. The process was still running after timeout. Check the console window of {_zipMountExecutableName} for details.");
                    throw new TimeoutException($"Failed to mount ZIP. Drive {mountDriveRootForChecks} not found after timeout.");
                }
            }

            DebugLogger.Log($"[MountZipFiles] Drive {mountDriveRootForChecks} detected. Searching for nested file...");
            var fileToLoad = FindNestedFile(mountDriveRootForChecks, logErrors);

            if (string.IsNullOrEmpty(fileToLoad))
            {
                DebugLogger.Log($"[MountZipFiles] No suitable file found in nested directory structure in {mountDriveRootForChecks}.");
                throw new FileNotFoundException($"Could not find a file to launch within the expected nested directory structure of the mounted ZIP at {mountDriveRootForChecks}.");
            }

            DebugLogger.Log($"[MountZipFiles] Nested file found at: {fileToLoad}. Proceeding to launch with {selectedEmulatorName}.");

            // Pass the original ZIP file path for display in notifications
            await gameLauncher.LaunchRegularEmulatorAsync(fileToLoad, selectedEmulatorName, selectedSystemManager, selectedEmulatorManager, rawEmulatorParameters, mainWindow, mainWindow, resolvedZipFilePath);

            DebugLogger.Log($"[MountZipFiles] Emulator for {fileToLoad} has exited.");
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[MountZipFiles] Exception during ZIP mounting or launching: {ex}");

            // Notify developer
            var exitCodeInfoInCatch = mountProcess is { HasExited: true } ? $"Exit Code: {mountProcess.ExitCode}" : "Process was still running or state unknown.";
            var contextMessage = $"Error during ZIP mount/launch process for {resolvedZipFilePath}.\n" +
                                 $"Exception: {ex.Message}\n" +
                                 $"The tool's output was not redirected. {exitCodeInfoInCatch}";
            _ = logErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFileMessageBox(mountProcess is { HasExited: true } ? mountProcess.ExitCode : null);
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
                    DebugLogger.Log($"[MountZipFiles] Kill signal sent to {_zipMountExecutableName} (ID: {mountProcessId}). Waiting for process to exit (up to 20s).");
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
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
                        DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} (ID: {mountProcessId}) terminated. Exit code: {mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture)}.");
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
                    _ = logErrors.LogErrorAsync(termEx, $"Failed to terminate {_zipMountExecutableName} (ID: {mountProcessId}) for unmounting.");
                }
            }
            else if (mountProcessId != -1)
            {
                var exitCodeStr = mountProcess is { HasExited: true } ? mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A";
                DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} (ID: {mountProcessId}) had already exited or was not running when finally cleanup was attempted. Exit code likely {exitCodeStr}.");
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

            // Safety net: ensure all SimpleZipDrive processes are killed
            KillAllSimpleZipDriveProcesses(logErrors);
        }
    }

    private static string FindNestedFile(string directoryPath, ILogErrors logErrors)
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
            _ = logErrors.LogErrorAsync(ex, $"Error in FindNestedFile searching {directoryPath}");

            return null;
        }
    }

    private static string FindScummVmGamePath(string mountDriveRootForChecks, ILogErrors logErrors)
    {
        try
        {
            DebugLogger.Log($"[FindScummVmGamePath] Searching for game files in {mountDriveRootForChecks}...");

            var currentPath = mountDriveRootForChecks;

            while (true)
            {
                var directories = Directory.GetDirectories(currentPath);
                var files = Directory.GetFiles(currentPath);

                if (files.Length > 0)
                {
                    DebugLogger.Log($"[FindScummVmGamePath] Found files in: {currentPath}");
                    return currentPath;
                }

                switch (directories.Length)
                {
                    case 1:
                        DebugLogger.Log($"[FindScummVmGamePath] Single folder found, navigating into: {directories[0]}");
                        currentPath = directories[0];
                        continue;
                    case > 1:
                        DebugLogger.Log($"[FindScummVmGamePath] Multiple folders found, using current path: {currentPath}");
                        return currentPath;
                    default:
                        DebugLogger.Log($"[FindScummVmGamePath] Empty directory, returning current path: {currentPath}");
                        return currentPath;
                }
            }
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[FindScummVmGamePath] Error: {ex.Message}");

            // Notify developer
            _ = logErrors.LogErrorAsync(ex, "Error in FindScummVmGamePath");

            return mountDriveRootForChecks;
        }
    }

    public static async Task MountZipFileAndLoadWithScummVmAsync(
        string resolvedZipFilePath,
        string selectedSystemName,
        string selectedEmulatorName,
        SystemManager.SystemManager selectedSystemManager,
        Emulator selectedEmulatorManager,
        string selectedEmulatorParameters,
        string logPath,
        ILogErrors logErrors)
    {
        DebugLogger.Log($"[MountZipFiles] Starting to mount ZIP for ScummVM: {resolvedZipFilePath}");
        DebugLogger.Log($"[MountZipFiles] System: {selectedSystemName}, Emulator: {selectedEmulatorName}");

        ValidateZipForPathTraversal(resolvedZipFilePath);

        var resolvedZipMountExePath = PathHelper.ResolveRelativeToAppDirectory(_zipMountExecutableRelativePath);

        DebugLogger.Log($"[MountZipFiles] Path to {_zipMountExecutableName}: {resolvedZipMountExePath}");

        if (string.IsNullOrWhiteSpace(resolvedZipMountExePath) || !File.Exists(resolvedZipMountExePath))
        {
            // Notify developer
            var errorMessage = $"{_zipMountExecutableName} not found at {_zipMountExecutableRelativePath}. Cannot mount ZIP.";
            DebugLogger.Log($"[MountZipFiles] Error: {errorMessage}");
            _ = logErrors.LogErrorAsync(null, errorMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFileMessageBox();

            return;
        }

        if (!DokanValidation.IsDokanInstalled())
        {
            const string errorMessage = "Dokan driver not found. Cannot mount ZIP.";
            DebugLogger.Log($"[MountZipFiles] Error: {errorMessage}");
            _ = logErrors.LogErrorAsync(null, errorMessage);
            MessageBoxLibrary.DokanDriverNotInstalledMessageBox();
            return;
        }

        // Get an available drive letter dynamically
        var driveLetter = GetAvailableDriveLetter(logErrors);
        if (driveLetter == null)
        {
            const string errorMessage = "No available drive letters found to mount the ZIP.";
            DebugLogger.Log($"[MountZipFiles] Error: {errorMessage}");
            _ = logErrors.LogErrorAsync(null, errorMessage);
            MessageBoxLibrary.ThereWasAnErrorMountingTheFileMessageBox();
            return;
        }

        var mountPathArgument = driveLetter.Value.ToString().ToLowerInvariant();
        var mountDriveRootForChecks = $"{driveLetter.Value}:\\";

        DebugLogger.Log($"[MountZipFiles] Selected drive letter for mounting: {driveLetter.Value}:");

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
                    var exitCode = mountProcess.ExitCode;
                    DebugLogger.Log($"[MountZipFiles] Mount process {_zipMountExecutableName} (ID: {mountProcessId}) exited prematurely during polling. Exit Code: {exitCode}.");
                    break;
                }

                await Task.Delay(pollInterval);
            }

            stopwatch.Stop();

            if (!mountSuccessful)
            {
                if (mountProcess.HasExited)
                {
                    var exitCode = mountProcess.ExitCode;
                    var reason = GetExitCodeReason(exitCode);
                    DebugLogger.Log($"[MountZipFiles] Mount check failed. Drive {mountDriveRootForChecks} not found. The process exited with code {exitCode} ({reason}).");
                    throw new InvalidOperationException($"Failed to mount ZIP. {_zipMountExecutableName} exited with code {exitCode} ({reason}).");
                }
                else
                {
                    DebugLogger.Log($"[MountZipFiles] Mount check failed. Drive {mountDriveRootForChecks} not found. The process was still running after timeout. Check the console window of {_zipMountExecutableName} for details.");
                    throw new TimeoutException($"Failed to mount ZIP. Drive {mountDriveRootForChecks} not found after timeout.");
                }
            }

            DebugLogger.Log($"[MountZipFiles] Drive {mountDriveRootForChecks} detected. Proceeding to launch with {selectedEmulatorName}.");

            // --- Custom ScummVM Launch Logic ---

            // 1. Resolve Emulator Path
            if (string.IsNullOrWhiteSpace(selectedEmulatorManager.EmulatorLocation))
            {
                throw new FileNotFoundException($"Emulator executable path is not configured for '{selectedEmulatorName}'. " +
                                                "Please edit the system configuration and provide a valid emulator path.");
            }

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

            // 2. Resolve Parameters
            var romSystemFolder = selectedSystemManager != null
                ? PathHelper.FindContainingSystemFolder(selectedSystemManager, resolvedZipFilePath)
                : null;
            var resolvedParameters = PathHelper.ResolveParameterString(
                selectedEmulatorParameters,
                selectedSystemManager?.SystemFolders,
                resolvedEmulatorFolderPath,
                resolvedZipFilePath,
                romSystemFolder
            );

            // Navigate into nested single-folder directories to find the actual game files location
            var gamePath = FindScummVmGamePath(mountDriveRootForChecks, logErrors);
            // ScummVM -p expects just the drive letter (e.g. "Y:") for root paths, not "Y:\"
            var scummVmPath = gamePath == mountDriveRootForChecks ? mountDriveRootForChecks.TrimEnd('\\') : gamePath;
            var arguments = $"-p \"{scummVmPath}\" {resolvedParameters} ";

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
            var exitCodeInfoInCatch = mountProcess is { HasExited: true } ? $"Exit Code: {mountProcess.ExitCode}" : "Process was still running or state unknown.";
            var contextMessage = $"Error during ScummVM ZIP mount/launch process for {resolvedZipFilePath}.\n" +
                                 $"Exception: {ex.Message}\n" +
                                 $"The tool's output was not redirected. {exitCodeInfoInCatch}";
            _ = logErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ThereWasAnErrorMountingTheFileMessageBox(mountProcess is { HasExited: true } ? mountProcess.ExitCode : null);
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
                    DebugLogger.Log($"[MountZipFiles] Kill signal sent to {_zipMountExecutableName} (ID: {mountProcessId}). Waiting for process to exit (up to 20s).");
                    using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
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
                        DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} (ID: {mountProcessId}) terminated. Exit code: {mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture)}.");
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
                    _ = logErrors.LogErrorAsync(termEx, $"Failed to terminate {_zipMountExecutableName} (ID: {mountProcessId}) for unmounting.");
                }
            }
            else if (mountProcessId != -1)
            {
                var exitCodeStr = mountProcess is { HasExited: true } ? mountProcess.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A";
                DebugLogger.Log($"[MountZipFiles] {_zipMountExecutableName} (ID: {mountProcessId}) had already exited or was not running when finally cleanup was attempted. Exit code likely {exitCodeStr}.");
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

            // Safety net: ensure all SimpleZipDrive processes are killed
            KillAllSimpleZipDriveProcesses(logErrors);
        }
    }
}