using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.DebugAndBugReport;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.MountFiles;

public class MountXisoFiles : IMountXisoFiles
{
    private readonly IDebugLogger _debugLogger;

    public MountXisoFiles(IDebugLogger debugLogger)
    {
        _debugLogger = debugLogger;
    }

    private static string GetToolPath()
    {
        var exeName = RuntimeInformation.OSArchitecture == Architecture.Arm64
            ? "SimpleXisoDrive_arm64.exe"
            : "SimpleXisoDrive.exe";

        return Path.Combine("tools", "SimpleXisoDrive", exeName);
    }

    private char? GetAvailableDriveLetter(ILogErrors logErrors)
    {
        try
        {
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
            _debugLogger.Log($"[MountXisoFiles.GetAvailableDriveLetter] Error enumerating drives: {ex.Message}");
            logErrors.LogAndForget(ex, "Error enumerating available drive letters.");
            return null;
        }
    }

    public async Task<MountXisoDrive> MountAsync(string resolvedIsoFilePath, string logPath, ILogErrors logErrors, IMessageBoxLibraryService messageBox)
    {
        _debugLogger.Log($"[MountXisoFiles.MountAsync] Starting to mount ISO: {resolvedIsoFilePath}");

        var toolRelativePath = GetToolPath();
        var resolvedToolPath = PathHelper.ResolveRelativeToAppDirectory(toolRelativePath);

        _debugLogger.Log($"[MountXisoFiles.MountAsync] Path to tool: {resolvedToolPath}");

        if (string.IsNullOrWhiteSpace(resolvedToolPath) || !File.Exists(resolvedToolPath))
        {
            var errorMessage = $"{Path.GetFileName(toolRelativePath)} not found. Cannot mount ISO.";
            _debugLogger.Log($"[MountXisoFiles.MountAsync] Error: {errorMessage}");
            logErrors.LogAndForget(null, errorMessage);
            await messageBox.ThereWasAnErrorMountingTheFileMessageBox();
            return new MountXisoDrive(logErrors, _debugLogger);
        }

        if (!DokanValidation.IsDokanInstalled())
        {
            const string errorMessage = "Dokan driver not found. Cannot mount ISO.";
            _debugLogger.Log($"[MountXisoFiles.MountAsync] Error: {errorMessage}");
            logErrors.LogAndForget(null, errorMessage);
            await messageBox.DokanDriverNotInstalledMessageBox();
            return new MountXisoDrive(logErrors, _debugLogger);
        }

        var driveLetter = GetAvailableDriveLetter(logErrors);
        if (driveLetter == null)
        {
            const string errorMessage = "No available drive letters found to mount the ISO.";
            _debugLogger.Log($"[MountXisoFiles.MountAsync] Error: {errorMessage}");
            logErrors.LogAndForget(null, errorMessage);
            await messageBox.ThereWasAnErrorMountingTheFileMessageBox();
            return new MountXisoDrive(logErrors, _debugLogger);
        }

        var driveLetterOnly = $"{driveLetter.Value}:";
        var defaultXbePath = $"{driveLetter.Value}:\\default.xbe";
        var driveRoot = $"{driveLetter.Value}:\\";

        var psiMount = new ProcessStartInfo
        {
            FileName = resolvedToolPath,
            Arguments = $"\"{resolvedIsoFilePath}\" \"{driveLetterOnly}\"",
            WindowStyle = ProcessWindowStyle.Normal,
            UseShellExecute = false,
            CreateNoWindow = false,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            WorkingDirectory = Path.GetDirectoryName(resolvedToolPath) ?? AppDomain.CurrentDomain.BaseDirectory
        };

        _debugLogger.Log($"[MountXisoFiles.MountAsync] Attempting to mount on drive {driveLetter.Value}:");
        _debugLogger.Log($"[MountXisoFiles.MountAsync] Arguments: {psiMount.Arguments}");

        var mountProcess = new Process { StartInfo = psiMount, EnableRaisingEvents = true };
        var toolName = Path.GetFileName(toolRelativePath);

        try
        {
            if (!mountProcess.Start())
            {
                throw new InvalidOperationException($"Failed to start the {toolName} process.");
            }

            _debugLogger.Log($"[MountXisoFiles.MountAsync] {toolName} process started (ID: {mountProcess.Id}).");

            var mountSuccessful = await WaitForDriveMountAsync(defaultXbePath, driveRoot, mountProcess, toolName, mountProcess.Id, logErrors);

            if (!mountSuccessful)
            {
                if (!mountProcess.HasExited)
                {
                    mountProcess.Kill(true);
                }

                mountProcess.Dispose();
                await messageBox.ThereWasAnErrorMountingTheFileMessageBox();
                return new MountXisoDrive(logErrors, _debugLogger);
            }

            _debugLogger.Log($"[MountXisoFiles.MountAsync] ISO mounted successfully. Path: {defaultXbePath}");
            return new MountXisoDrive(mountProcess, defaultXbePath, logErrors, _debugLogger);
        }
        catch (Exception ex)
        {
            _debugLogger.Log($"[MountXisoFiles.MountAsync] Exception during mounting: {ex}");
            var contextMessage = $"Error during ISO mount process for {resolvedIsoFilePath}.\nException: {ex.Message}";
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
            return new MountXisoDrive(logErrors, _debugLogger);
        }
    }

    private async Task<bool> WaitForDriveMountAsync(string defaultXbePath, string driveRoot, Process mountProcess, string toolName, int processId, ILogErrors logErrors)
    {
        const int maxRetries = 240;
        const int pollIntervalMs = 500;
        var retryCount = 0;

        _debugLogger.Log($"[MountXisoFiles.WaitForDriveMountAsync] Polling for '{defaultXbePath}' to appear (max {maxRetries * pollIntervalMs / 1000}s)...");

        while (retryCount < maxRetries)
        {
            if (File.Exists(defaultXbePath))
            {
                _debugLogger.Log($"[MountXisoFiles.WaitForDriveMountAsync] Found '{defaultXbePath}' after {retryCount * pollIntervalMs / 1000.0:F1} seconds. Mount successful!");
                return true;
            }

            if (Directory.Exists(driveRoot))
            {
                _debugLogger.Log($"[MountXisoFiles.WaitForDriveMountAsync] {driveRoot} drive exists after {retryCount * pollIntervalMs / 1000.0:F1} seconds, but '{defaultXbePath}' not found. Continuing to poll...");
            }

            if (mountProcess.HasExited)
            {
                _debugLogger.Log($"[MountXisoFiles.WaitForDriveMountAsync] Mount process {toolName} (ID: {processId}) exited prematurely during polling. Exit Code: {mountProcess.ExitCode}.");
                var contextMessage = $"Failed to mount ISO. The mounting tool '{toolName}' exited prematurely with code {mountProcess.ExitCode}.";
                logErrors.LogAndForget(null, contextMessage);
                return false;
            }

            retryCount++;
            await Task.Delay(pollIntervalMs);
        }

        _debugLogger.Log($"[MountXisoFiles.WaitForDriveMountAsync] Timed out waiting for '{defaultXbePath}' after {maxRetries * pollIntervalMs / 1000} seconds.");
        var timeoutContextMessage = $"Timed out waiting for the ISO to mount to '{driveRoot}'.";
        logErrors.LogAndForget(null, timeoutContextMessage);
        return false;
    }
}
