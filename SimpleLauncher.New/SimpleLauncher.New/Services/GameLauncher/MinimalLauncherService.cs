using System.Diagnostics;
using System.IO.Compression;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;

namespace SimpleLauncher.New.Services.GameLauncher;

/// <summary>
/// ILauncherService implementation with file-type detection and strategy dispatch.
/// Handles: direct launch (.exe/.bat/.lnk), ZIP extraction, ISO mount (PowerShell).
/// Phase 6+: File-type aware launch pipeline.
/// </summary>
public class MinimalLauncherService : ILauncherService
{
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IEnumerable<IEmulatorConfigHandler> _configHandlers;
    private readonly ChdMountService _chdMount;
    private string? _chdMountPath; // Track mounted CHD path for cleanup

    public MinimalLauncherService(IMessageBoxLibraryService messageBox, IEnumerable<IEmulatorConfigHandler> configHandlers, ChdMountService chdMount)
    {
        _messageBox = messageBox;
        _configHandlers = configHandlers;
        _chdMount = chdMount;
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
        loadingStateProvider?.SetLoadingState(true, "Preparing...");

        var ext = Path.GetExtension(resolvedFilePath).ToUpperInvariant();
        var actualFilePath = resolvedFilePath;
        string? cleanupPath = null; // temp dir to clean up after launch

        try
        {
            // ── File-type dispatch ──
            switch (ext)
            {
                case ".ZIP":
                case ".7Z":
                case ".RAR":
                    loadingStateProvider?.SetLoadingState(true, "Extracting...");
                    actualFilePath = await ExtractAndFindLaunchableAsync(resolvedFilePath);
                    cleanupPath = Path.GetDirectoryName(actualFilePath); // clean up temp dir
                    break;

                case ".ISO":
                    loadingStateProvider?.SetLoadingState(true, "Mounting ISO...");
                    actualFilePath = await MountIsoAsync(resolvedFilePath);
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

                case ".XISO":
                    loadingStateProvider?.SetLoadingState(true, "");
                    await _messageBox.CustomErrorMessageBoxAsync(
                        "XISO files require Xbox emulators (Xemu/Cxbx-Reloaded) with specific configuration.",
                        "XISO Not Supported");
                    loadingStateProvider?.SetLoadingState(false);
                    return;
            }

            // ── Run matching emulator config handlers ──
            var emulatorName = selectedEmulatorManager.EmulatorName;
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
                    try { await handler.HandleConfigurationAsync(launchContext); }
                    catch (Exception ex) { Log.Error(ex, "Emulator config injection failed for {Emulator}", emulatorName); }
                }
            }

            // ── Launch ──
            loadingStateProvider?.SetLoadingState(true, "Launching...");

            if (string.IsNullOrWhiteSpace(emulatorPath))
            {
                await _messageBox.ErrorLaunchingGameMessageBoxAsync("No emulator path configured.");
                return;
            }

            var parameters = rawEmulatorParameters
                .Replace("%ROM%", $"\"{actualFilePath}\"")
                .Replace("%BASEFOLDER%", Path.GetDirectoryName(actualFilePath) ?? "")
                .Replace("%EMULATORFOLDER%", Path.GetDirectoryName(emulatorPath) ?? "");

            await Task.Run(() =>
            {
                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = emulatorPath,
                            Arguments = parameters,
                            UseShellExecute = false,
                            WorkingDirectory = Path.GetDirectoryName(emulatorPath) ?? ""
                        }
                    };

                    process.Start();
                    process.WaitForExit();
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to launch emulator process {EmulatorPath}", emulatorPath);
                    _ = _messageBox.ErrorLaunchingGameMessageBoxAsync(ex.Message);
                }
            });

            loadingStateProvider?.SetLoadingState(false, "Done");
        }
        finally
        {
            // Clean up temp extraction directory
            if (cleanupPath is not null)
            {
                try { Directory.Delete(cleanupPath, true); } catch (Exception ex) { Log.Debug(ex, "Failed to delete temp extraction dir {Path}", cleanupPath); }
            }
            // Unmount CHD if we mounted one
            if (_chdMountPath is not null)
            {
                try { _chdMount.Unmount(_chdMountPath); } catch (Exception ex) { Log.Debug(ex, "Failed to unmount CHD drive"); }
                _chdMountPath = null;
            }
        }
    }

    #region ZIP Extraction

    /// <summary>
    /// Extracts a ZIP/7z/RAR archive to a temp directory and finds a launchable file.
    /// </summary>
    private static async Task<string> ExtractAndFindLaunchableAsync(string archivePath)
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "SimpleLauncher_New", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        await Task.Run(() =>
        {
            try
            {
                // Use System.IO.Compression for ZIP; sharpcompress for 7z/rar
                var ext = Path.GetExtension(archivePath).ToUpperInvariant();
                if (ext == ".ZIP")
                {
                    ZipFile.ExtractToDirectory(archivePath, tempDir);
                }
                else
                {
                    // For 7z/RAR, try SharpCompress (registered in DI)
                    try
                    {
                        SharpCompress.Archives.ArchiveFactory.WriteToDirectory(archivePath, tempDir);
                    }
                    catch (Exception ex)
                    {
                        // Fallback: just return the original path, let the emulator handle it
                        // (some emulators like RetroArch can read archives directly)
                        Log.Warning(ex, "SharpCompress extraction failed for {ArchivePath}", archivePath);
                    }
                }
            }
            catch (Exception ex)
            {
                // If extraction fails, the original path is returned as-is
                Log.Warning(ex, "Extraction failed for {ArchivePath}", archivePath);
            }
        });

        // Find a launchable file in the temp dir
        var launchable = FindLaunchableFile(tempDir);
        return launchable ?? archivePath; // Fallback to original path
    }

    /// <summary>
    /// Finds a launchable file in an extracted directory.
    /// Priority: .cue > .iso > .bin > .exe > first found file
    /// </summary>
    private static string? FindLaunchableFile(string directory)
    {
        try
        {
            var allFiles = Directory.GetFiles(directory, "*.*", SearchOption.AllDirectories);

            // Priority order for launchable files
            foreach (var ext in new[] { ".cue", ".iso", ".bin", ".gdi", ".ccd", ".mds", ".exe", ".bat" })
            {
                var match = allFiles.FirstOrDefault(f =>
                    Path.GetExtension(f).Equals(ext, StringComparison.OrdinalIgnoreCase));
                if (match is not null) return match;
            }

            // Fallback: first file
            return allFiles.FirstOrDefault();
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "Failed to enumerate files in {Directory}", directory);
            return null;
        }
    }

    #endregion

    #region ISO Mount (PowerShell)

    /// <summary>
    /// Mounts an ISO file using PowerShell and returns the mount point path.
    /// </summary>
    private static async Task<string> MountIsoAsync(string isoPath)
    {
        // Try PowerShell mount
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -Command \"$d=Mount-DiskImage -ImagePath '{isoPath}' -PassThru; $v=$d | Get-Volume; Write-Output $v.DriveLetter\"",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);
            if (process is null) return isoPath;

            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            var driveLetter = output.Trim().Replace(":", "");
            if (!string.IsNullOrEmpty(driveLetter))
            {
                var mountPath = $"{driveLetter}:\\";
                if (Directory.Exists(mountPath))
                    return mountPath;
            }
        }
        catch (Exception ex) { Log.Debug(ex, "ISO mount via PowerShell failed for {IsoPath}", isoPath); }

        // Fallback: return original path (some emulators handle ISO directly)
        return isoPath;
    }

    #endregion

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
                process.WaitForExit();
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
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = resolvedFilePath,
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
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to launch {Path}", resolvedFilePath);
                _ = _messageBox.ErrorLaunchingGameMessageBoxAsync(ex.Message);
            }
        });
    }

    #endregion
}
