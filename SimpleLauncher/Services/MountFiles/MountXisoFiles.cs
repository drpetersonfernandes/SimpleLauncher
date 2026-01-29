using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.CheckPaths;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher.Services.MountFiles;

public static class MountXisoFiles
{
    private static string GetToolPath()
    {
        var exeName = RuntimeInformation.OSArchitecture == Architecture.Arm64
            ? "SimpleXisoDrive_arm64.exe"
            : "SimpleXisoDrive.exe";

        return Path.Combine("tools", "SimpleXisoDrive", exeName);
    }

    /// <summary>
    /// Finds an available drive letter from Z: down to D:.
    /// </summary>
    /// <returns>An available character for a drive letter, or null if none are available.</returns>
    private static char? GetAvailableDriveLetter()
    {
        try
        {
            // Use Environment.GetLogicalDrives() to avoid hanging on disconnected network drives
            var existingDrives = Environment.GetLogicalDrives()
                .Select(static d => char.ToUpper(d[0], CultureInfo.InvariantCulture))
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
        catch (Exception ex)
        {
            DebugLogger.Log($"[MountXisoFiles.GetAvailableDriveLetter] Error enumerating drives: {ex.Message}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error enumerating available drive letters.");
            return null;
        }
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

        var toolRelativePath = GetToolPath();
        var resolvedToolPath = PathHelper.ResolveRelativeToAppDirectory(toolRelativePath);

        DebugLogger.Log($"[MountXisoFiles.MountAsync] Path to tool: {resolvedToolPath}");

        if (string.IsNullOrWhiteSpace(resolvedToolPath) || !File.Exists(resolvedToolPath))
        {
            var errorMessage = $"{Path.GetFileName(toolRelativePath)} not found. Cannot mount ISO.";
            DebugLogger.Log($"[MountXisoFiles.MountAsync] Error: {errorMessage}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, errorMessage);
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);
            return new MountXisoDrive(); // Return failed state
        }

        var driveLetter = GetAvailableDriveLetter();
        if (driveLetter == null)
        {
            const string errorMessage = "No available drive letters found to mount the ISO.";
            DebugLogger.Log($"[MountXisoFiles.MountAsync] Error: {errorMessage}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, errorMessage);
            MessageBoxLibrary.ThereWasAnErrorMountingTheFile(logPath);
            return new MountXisoDrive(); // Return failed state
        }

        var driveLetterOnly = $"{driveLetter.Value}:";
        var defaultXbePath = $"{driveLetter.Value}:\\default.xbe";
        var driveRoot = $"{driveLetter.Value}:\\";

        var psiMount = new ProcessStartInfo
        {
            FileName = resolvedToolPath,
            Arguments = $"\"{resolvedIsoFilePath}\" \"{driveLetterOnly}\"",
            UseShellExecute = true,
            CreateNoWindow = false,
            WindowStyle = ProcessWindowStyle.Normal,
            WorkingDirectory = Path.GetDirectoryName(resolvedToolPath) ?? AppDomain.CurrentDomain.BaseDirectory
        };

        DebugLogger.Log($"[MountXisoFiles.MountAsync] Attempting to mount on drive {driveLetter.Value}:");
        DebugLogger.Log($"[MountXisoFiles.MountAsync] Arguments: {psiMount.Arguments}");

        var mountProcess = new Process { StartInfo = psiMount, EnableRaisingEvents = true };
        var toolName = Path.GetFileName(toolRelativePath);

        try
        {
            if (!mountProcess.Start())
            {
                throw new InvalidOperationException($"Failed to start the {toolName} process.");
            }

            DebugLogger.Log($"[MountXisoFiles.MountAsync] {toolName} process started (ID: {mountProcess.Id}).");

            var mountSuccessful = await WaitForDriveMountAsync(defaultXbePath, driveRoot, mountProcess, toolName, mountProcess.Id);

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
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
                return false;
            }

            retryCount++;
            await Task.Delay(pollIntervalMs);
        }

        DebugLogger.Log($"[MountXisoFiles.WaitForDriveMountAsync] Timed out waiting for '{defaultXbePath}' after {maxRetries * pollIntervalMs / 1000} seconds.");
        var timeoutContextMessage = $"Timed out waiting for the ISO to mount to '{driveRoot}'.";
        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, timeoutContextMessage);
        return false;
    }
}