using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleLauncher.Services;

public static class MountXisoFiles
{
    /// <summary>
    /// Finds an available drive letter from Z: down to D:.
    /// </summary>
    /// <returns>An available character for a drive letter, or null if none are available.</returns>
    private static char? GetAvailableDriveLetter()
    {
        var existingDrives = DriveInfo.GetDrives()
            .Select(static d => char.ToUpper(d.Name[0], CultureInfo.InvariantCulture))
            .ToHashSet();

        for (var letter = 'Z'; letter >= 'D'; letter--)
        {
            if (!existingDrives.Contains(letter))
            {
                return letter;
            }
        }

        return null;
    }

    /// <summary>
    /// Mounts an Xbox ISO file to an available drive letter.
    /// </summary>
    /// <param name="resolvedIsoFilePath">The full path to the ISO file.</param>
    /// <param name="logPath">Path to the application's log file for error reporting.</param>
    /// <returns>A disposable MountXisoDrive object that manages the mount process.</returns>
    public static async Task<MountXisoDrive> MountAsync(string resolvedIsoFilePath, string logPath)
    {
        DebugLogger.Log($"[MountXisoFiles.MountAsync] Starting to mount ISO: {resolvedIsoFilePath}");

        const string xboxIsoVfsExe = @"tools\xbox-iso-vfs\xbox-iso-vfs.exe";
        var resolvedXboxIsoVfsPath = PathHelper.ResolveRelativeToAppDirectory(xboxIsoVfsExe);

        DebugLogger.Log($"[MountXisoFiles.MountAsync] Path to {xboxIsoVfsExe}: {resolvedXboxIsoVfsPath}");

        if (string.IsNullOrWhiteSpace(resolvedXboxIsoVfsPath) || !File.Exists(resolvedXboxIsoVfsPath))
        {
            const string errorMessage = "xbox-iso-vfs.exe not found. Cannot mount ISO.";
            DebugLogger.Log($"[MountXisoFiles.MountAsync] Error: {errorMessage}");
            _ = LogErrors.LogErrorAsync(null, errorMessage);
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);
            return new MountXisoDrive(); // Return failed state
        }

        var driveLetter = GetAvailableDriveLetter();
        if (driveLetter == null)
        {
            const string errorMessage = "No available drive letters found to mount the ISO.";
            DebugLogger.Log($"[MountXisoFiles.MountAsync] Error: {errorMessage}");
            _ = LogErrors.LogErrorAsync(null, errorMessage);
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);
            return new MountXisoDrive(); // Return failed state
        }

        var driveLetterChar = driveLetter.Value.ToString().ToLowerInvariant();
        var defaultXbePath = $"{driveLetter.Value}:\\default.xbe";
        var driveRoot = $"{driveLetter.Value}:\\";

        var psiMount = new ProcessStartInfo
        {
            FileName = resolvedXboxIsoVfsPath,
            Arguments = $"/l \"{resolvedIsoFilePath}\" {driveLetterChar}",
            UseShellExecute = false,
            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Normal,
            WorkingDirectory = Path.GetDirectoryName(resolvedXboxIsoVfsPath) ?? AppDomain.CurrentDomain.BaseDirectory
        };

        DebugLogger.Log($"[MountXisoFiles.MountAsync] Attempting to mount on drive {driveLetter.Value}:");
        DebugLogger.Log($"[MountXisoFiles.MountAsync] Arguments: {psiMount.Arguments}");

        var mountProcess = new Process { StartInfo = psiMount, EnableRaisingEvents = true };

        try
        {
            if (!mountProcess.Start())
            {
                throw new InvalidOperationException($"Failed to start the {xboxIsoVfsExe} process.");
            }

            DebugLogger.Log($"[MountXisoFiles.MountAsync] {xboxIsoVfsExe} process started (ID: {mountProcess.Id}).");

            var mountSuccessful = await WaitForDriveMountAsync(defaultXbePath, driveRoot, mountProcess, xboxIsoVfsExe, mountProcess.Id);

            if (!mountSuccessful)
            {
                if (!mountProcess.HasExited)
                {
                    mountProcess.Kill(true);
                }

                mountProcess.Dispose();
                MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);
                return new MountXisoDrive(); // Return failed state
            }

            DebugLogger.Log($"[MountXisoFiles.MountAsync] ISO mounted successfully. Path: {defaultXbePath}");
            // Success: return the disposable object which owns the process
            return new MountXisoDrive(mountProcess, defaultXbePath);
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[MountXisoFiles.MountAsync] Exception during mounting: {ex}");
            var contextMessage = $"Error during ISO mount process for {resolvedIsoFilePath}.\nException: {ex.Message}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            if (mountProcess != null && !mountProcess.HasExited)
            {
                mountProcess.Kill(true);
            }

            mountProcess?.Dispose();

            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);
            return new MountXisoDrive(); // Return failed state
        }
    }

    private static async Task<bool> WaitForDriveMountAsync(string defaultXbePath, string driveRoot, Process mountProcess, string toolName, int processId)
    {
        const int maxRetries = 240; // 2 minutes with 500 ms intervals
        const int pollIntervalMs = 500;
        var retryCount = 0;

        DebugLogger.Log($"[MountXisoFiles.WaitForDriveMountAsync] Polling for '{defaultXbePath}' to appear (max {maxRetries * pollIntervalMs / 1000}s)...");

        while (retryCount < maxRetries)
        {
            if (File.Exists(defaultXbePath))
            {
                DebugLogger.Log($"[MountXisoFiles.WaitForDriveMountAsync] Found '{defaultXbePath}' after {retryCount * pollIntervalMs / 1000.0:F1} seconds. Mount successful!");
                return true;
            }

            if (Directory.Exists(driveRoot))
            {
                DebugLogger.Log($"[MountXisoFiles.WaitForDriveMountAsync] {driveRoot} drive exists after {retryCount * pollIntervalMs / 1000.0:F1} seconds, but '{defaultXbePath}' not found. Continuing to poll...");
            }

            if (mountProcess.HasExited)
            {
                DebugLogger.Log($"[MountXisoFiles.WaitForDriveMountAsync] Mount process {toolName} (ID: {processId}) exited prematurely during polling. Exit Code: {mountProcess.ExitCode}.");
                var contextMessage = $"Failed to mount ISO. The mounting tool '{toolName}' exited prematurely with code {mountProcess.ExitCode}.";
                _ = LogErrors.LogErrorAsync(null, contextMessage);
                return false;
            }

            retryCount++;
            await Task.Delay(pollIntervalMs);
        }

        DebugLogger.Log($"[MountXisoFiles.WaitForDriveMountAsync] Timed out waiting for '{defaultXbePath}' after {maxRetries * pollIntervalMs / 1000} seconds.");
        var timeoutContextMessage = $"Timed out waiting for the ISO to mount to '{driveRoot}'.";
        _ = LogErrors.LogErrorAsync(null, timeoutContextMessage);
        return false;
    }
}
