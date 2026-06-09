using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.CheckPaths;
using SimpleLauncher.Core.Services.CleanAndDeleteFiles;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.ExtractFiles;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Services.GameLauncher.MountFiles;


namespace SimpleLauncher.Services.GameLauncher.Strategies;

public class DosBoxLaunchStrategy : ILaunchStrategy
{
    private readonly IExtractionService _extractionService;
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IMountChdFiles _mountChdFiles;
    private readonly IMountIsoFiles _mountIsoFiles;

    private static readonly string[] PriorityGameFormats = [".conf", ".bat", ".exe", ".com"];
    private static readonly List<string> ExtractionFormats = ["conf", "bat", "exe", "com"];

    public DosBoxLaunchStrategy(IExtractionService extractionService, IConfiguration configuration, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IMountChdFiles mountChdFiles, IMountIsoFiles mountIsoFiles)
    {
        _extractionService = extractionService;
        _configuration = configuration;
        _logErrors = logErrors;
        _messageBox = messageBox;
        _mountChdFiles = mountChdFiles;
        _mountIsoFiles = mountIsoFiles;
    }

    public int Priority => 25;

    public bool IsMatch(LaunchContext context)
    {
        if (string.IsNullOrEmpty(context.EmulatorName) ||
            string.IsNullOrEmpty(context.ResolvedFilePath))
            return false;

        if (!IsDosBoxEmulator(context))
            return false;

        if (Directory.Exists(context.ResolvedFilePath))
            return true;

        var ext = Path.GetExtension(context.ResolvedFilePath).ToUpperInvariant();
        return ext is ".ZIP" or ".7Z" or ".RAR" or ".ISO" or ".CHD";
    }

    public async Task ExecuteAsync(LaunchContext context, ILauncherService launcher)
    {
        var ext = Path.GetExtension(context.ResolvedFilePath).ToUpperInvariant();
        switch (ext)
        {
            case ".ISO":
                await ExecuteIsoAsync(context, launcher);
                return;
            case ".CHD":
                await ExecuteChdAsync(context, launcher);
                return;
            default:
            {
                string tempDir = null;

                try
                {
                    string workingDir;
                    if (Directory.Exists(context.ResolvedFilePath))
                    {
                        workingDir = context.ResolvedFilePath;
                    }
                    else
                    {
                        var (_, extractedDir) = await _extractionService.ExtractToTempAndGetLaunchFileAsync(
                            context.ResolvedFilePath, ExtractionFormats);

                        if (string.IsNullOrEmpty(extractedDir) || !Directory.Exists(extractedDir))
                        {
                            DebugLogger.Log("[DosBoxLaunchStrategy] Extraction failed or temp directory not created.");
                            return;
                        }

                        tempDir = extractedDir;
                        workingDir = extractedDir;
                    }

                    var gameFiles = FindAllGameFiles(workingDir);
                    if (gameFiles.Count == 0)
                    {
                        DebugLogger.Log($"[DosBoxLaunchStrategy] No game file (conf/bat/exe/com) found in {workingDir}");
                        await _logErrors.LogErrorAsync(null, $"No DOS game executable found in: {context.ResolvedFilePath}");
                        await _messageBox.CouldNotFindAFileMessageBox();
                        return;
                    }

                    string selectedFile;
                    if (gameFiles.Count == 1)
                    {
                        selectedFile = gameFiles[0];
                        DebugLogger.Log($"[DosBoxLaunchStrategy] Single game file found, auto-selecting: {selectedFile}");
                    }
                    else
                    {
                        var dialog = App.ServiceProvider.GetRequiredService<DosBoxFileSelectionWindow>();
                        dialog.Initialize(gameFiles, workingDir);
                        var result = dialog.ShowDialog();

                        if (result != true || string.IsNullOrEmpty(dialog.SelectedFilePath))
                        {
                            DebugLogger.Log("[DosBoxLaunchStrategy] User cancelled file selection.");
                            return;
                        }

                        selectedFile = dialog.SelectedFilePath;
                        DebugLogger.Log($"[DosBoxLaunchStrategy] User selected file: {selectedFile}");
                    }

                    string confPath;
                    if (Path.GetExtension(selectedFile).Equals(".conf", StringComparison.OrdinalIgnoreCase))
                    {
                        confPath = selectedFile;
                    }
                    else
                    {
                        confPath = GenerateTempConf(workingDir, selectedFile);
                    }

                    var launchParameters = BuildLaunchParameters(context.Parameters);

                    await launcher.LaunchRegularEmulatorAsync(
                        confPath,
                        context.EmulatorName,
                        context.SystemManager,
                        context.EmulatorManager,
                        launchParameters,
                        context.WindowContext,
                        context.LoadingState,
                        context.ResolvedFilePath);
                }
                catch (Exception ex)
                {
                    await _logErrors.LogErrorAsync(ex, $"[DosBoxLaunchStrategy] Error launching DOS game: {context.ResolvedFilePath}");
                    await _messageBox.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                }
                finally
                {
                    if (tempDir != null)
                    {
                        await CleanTempFolder.CleanupTempDirectoryAsync(tempDir);
                    }
                }

                break;
            }
        }
    }

    internal static bool IsDosBoxEmulator(LaunchContext context)
    {
        var name = context.EmulatorName;
        var path = context.EmulatorManager?.EmulatorLocation ?? string.Empty;

        return name.Contains("DOSBox", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("DOSBox-X", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("DOSBox Staging", StringComparison.OrdinalIgnoreCase) ||
               name.Contains("dosbox_pure", StringComparison.OrdinalIgnoreCase) ||
               path.Contains("dosbox", StringComparison.OrdinalIgnoreCase);
    }

    private static List<string> FindAllGameFiles(string directory)
    {
        var foundFiles = new List<string>();

        foreach (var format in PriorityGameFormats)
        {
            try
            {
                var files = Directory.GetFiles(directory, $"*{format}", SearchOption.AllDirectories);
                foundFiles.AddRange(files);
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"[DosBoxLaunchStrategy] Error searching for *{format}: {ex.Message}");
            }
        }

        DebugLogger.Log($"[DosBoxLaunchStrategy] Found {foundFiles.Count} game file(s) in {directory}");
        return foundFiles;
    }

    private static string GenerateTempConf(string gameDir, string executablePath)
    {
        var executableName = Path.GetFileName(executablePath);
        var executableDir = Path.GetDirectoryName(executablePath);
        var confPath = Path.Combine(gameDir, "_simplelauncher_dosbox.conf");

        string confContent;
        if (!string.IsNullOrEmpty(executableDir) &&
            !executableDir.Equals(gameDir, StringComparison.OrdinalIgnoreCase))
        {
            var relativeDir = executableDir.Replace(gameDir, "").TrimStart('\\', '/');
            confContent = string.Join("\r\n",
                "[dosbox]",
                "",
                "[autoexec]",
                "@echo off",
                $"mount c \"{gameDir}\"",
                "c:",
                $"cd {relativeDir}",
                executableName,
                "exit",
                "");
        }
        else
        {
            confContent = string.Join("\r\n",
                "[dosbox]",
                "",
                "[autoexec]",
                "@echo off",
                $"mount c \"{gameDir}\"",
                "c:",
                executableName,
                "exit",
                "");
        }

        File.WriteAllText(confPath, confContent, Encoding.ASCII);
        DebugLogger.Log($"[DosBoxLaunchStrategy] Generated conf file: {confPath}");

        return confPath;
    }

    private async Task ExecuteIsoAsync(LaunchContext context, ILauncherService launcher)
    {
        string mountPath;
        string selectedFile;

        try
        {
            // 1. Mount ISO via PowerShell to scan for executables
            var driveLetter = await _mountIsoFiles.ExecutePowerShellMountCommandAsync(context.ResolvedFilePath, _logErrors, _messageBox);
            if (string.IsNullOrEmpty(driveLetter))
            {
                DebugLogger.Log("[DosBoxLaunchStrategy] Failed to mount ISO via PowerShell.");
                await _messageBox.ThereWasAnErrorMountingTheFileMessageBox();
                return;
            }

            mountPath = $"{driveLetter}:\\";
            DebugLogger.Log($"[DosBoxLaunchStrategy] ISO mounted to {mountPath} for scanning");

            if (!await _mountIsoFiles.WaitForDirectoryToExistAsync(mountPath, 10000, 200, _logErrors))
            {
                DebugLogger.Log($"[DosBoxLaunchStrategy] Mount path {mountPath} did not become available.");
                await _messageBox.ThereWasAnErrorMountingTheFileMessageBox();
                return;
            }

            var gameFiles = FindAllGameFiles(mountPath);
            switch (gameFiles.Count)
            {
                case 0:
                    DebugLogger.Log($"[DosBoxLaunchStrategy] No game file (conf/bat/exe/com) found on mounted ISO at {mountPath}");
                    await _logErrors.LogErrorAsync(null, $"No DOS game executable found in ISO: {context.ResolvedFilePath}");
                    await _messageBox.CouldNotFindAFileMessageBox();
                    return;
                case 1:
                    selectedFile = gameFiles[0];
                    DebugLogger.Log($"[DosBoxLaunchStrategy] Single game file found on ISO, auto-selecting: {selectedFile}");
                    break;
                default:
                {
                    var dialog = App.ServiceProvider.GetRequiredService<DosBoxFileSelectionWindow>();
                    dialog.Initialize(gameFiles, mountPath);
                    var result = dialog.ShowDialog();

                    if (result != true || string.IsNullOrEmpty(dialog.SelectedFilePath))
                    {
                        DebugLogger.Log("[DosBoxLaunchStrategy] User cancelled file selection for ISO.");
                        return;
                    }

                    selectedFile = dialog.SelectedFilePath;
                    DebugLogger.Log($"[DosBoxLaunchStrategy] User selected file from ISO: {selectedFile}");
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, $"[DosBoxLaunchStrategy] Error scanning ISO: {context.ResolvedFilePath}");
            await _messageBox.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
            return;
        }
        finally
        {
            // 2. Dismount PowerShell ISO — no longer needed after scanning
            if (!string.IsNullOrEmpty(context.ResolvedFilePath))
            {
                DebugLogger.Log($"[DosBoxLaunchStrategy] Dismounting PowerShell ISO mount after scanning: {context.ResolvedFilePath}");
                await _mountIsoFiles.ExecutePowerShellDismountCommandAsync(context.ResolvedFilePath, _logErrors, _messageBox);
            }
        }

        // 3. Generate conf that lets DOSBox mount the ISO natively via imgmount
        var confPath = GenerateIsoConf(mountPath, selectedFile, context.ResolvedFilePath);
        var launchParameters = BuildLaunchParameters(context.Parameters);

        await launcher.LaunchRegularEmulatorAsync(
            confPath,
            context.EmulatorName,
            context.SystemManager,
            context.EmulatorManager,
            launchParameters,
            context.WindowContext,
            context.LoadingState,
            context.ResolvedFilePath);
    }

    private static string GenerateIsoConf(string mountPath, string executablePath, string isoFilePath)
    {
        var executableName = Path.GetFileName(executablePath);
        var executableDir = Path.GetDirectoryName(executablePath);
        var tempDir = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
        Directory.CreateDirectory(tempDir);
        var confPath = Path.Combine(tempDir, $"{Guid.NewGuid():N}_dosbox_iso.conf");

        string confContent;
        if (!string.IsNullOrEmpty(executableDir) &&
            !executableDir.TrimEnd('\\').Equals(mountPath.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
        {
            var relativeDir = executableDir.Replace(mountPath, "").TrimStart('\\', '/');
            confContent = string.Join("\r\n",
                "[dosbox]",
                "",
                "[autoexec]",
                "@echo off",
                $"imgmount d \"{isoFilePath}\" -t iso",
                "d:",
                $"cd {relativeDir}",
                executableName,
                "exit",
                "");
        }
        else
        {
            confContent = string.Join("\r\n",
                "[dosbox]",
                "",
                "[autoexec]",
                "@echo off",
                $"imgmount d \"{isoFilePath}\" -t iso",
                "d:",
                executableName,
                "exit",
                "");
        }

        File.WriteAllText(confPath, confContent, Encoding.ASCII);
        DebugLogger.Log($"[DosBoxLaunchStrategy] Generated ISO conf file: {confPath}");

        return confPath;
    }

    private static string BuildLaunchParameters(string parameters)
    {
        var launchParameters = parameters ?? string.Empty;
        if (!launchParameters.Contains("-conf", StringComparison.OrdinalIgnoreCase))
        {
            launchParameters = string.IsNullOrWhiteSpace(launchParameters)
                ? "-conf %ROM%"
                : launchParameters.Contains("%ROM%", StringComparison.OrdinalIgnoreCase)
                    ? launchParameters.Replace("%ROM%", "-conf %ROM%", StringComparison.OrdinalIgnoreCase)
                    : $"-conf %ROM% {launchParameters}";
        }

        return launchParameters;
    }

    private async Task ExecuteChdAsync(LaunchContext context, ILauncherService launcher)
    {
        try
        {
            await using var mountedDrive = await _mountChdFiles.MountAsync(context.ResolvedFilePath, 19, _logErrors, _messageBox);

            if (!mountedDrive.IsMounted)
            {
                DebugLogger.Log("[DosBoxLaunchStrategy] Failed to mount CHD via CHDMounter.");
                return;
            }

            var mountPath = mountedDrive.MountedPath;
            DebugLogger.Log($"[DosBoxLaunchStrategy] CHD mounted at {mountPath} via CHDMounter");

            var gameFiles = FindAllGameFiles(mountPath);
            if (gameFiles.Count == 0)
            {
                DebugLogger.Log($"[DosBoxLaunchStrategy] No game file (conf/bat/exe/com) found on mounted CHD at {mountPath}");
                await _logErrors.LogErrorAsync(null, $"No DOS game executable found in CHD: {context.ResolvedFilePath}");
                await _messageBox.CouldNotFindAFileMessageBox();
                return;
            }

            string selectedFile;
            if (gameFiles.Count == 1)
            {
                selectedFile = gameFiles[0];
                DebugLogger.Log($"[DosBoxLaunchStrategy] Single game file found on CHD, auto-selecting: {selectedFile}");
            }
            else
            {
                var dialog = App.ServiceProvider.GetRequiredService<DosBoxFileSelectionWindow>();
                dialog.Initialize(gameFiles, mountPath);
                var result = dialog.ShowDialog();

                if (result != true || string.IsNullOrEmpty(dialog.SelectedFilePath))
                {
                    DebugLogger.Log("[DosBoxLaunchStrategy] User cancelled file selection for CHD.");
                    return;
                }

                selectedFile = dialog.SelectedFilePath;
                DebugLogger.Log($"[DosBoxLaunchStrategy] User selected file from CHD: {selectedFile}");
            }

            var confPath = GenerateChdConf(mountPath, selectedFile);
            var launchParameters = BuildLaunchParameters(context.Parameters);

            await launcher.LaunchRegularEmulatorAsync(
                confPath,
                context.EmulatorName,
                context.SystemManager,
                context.EmulatorManager,
                launchParameters,
                context.WindowContext,
                context.LoadingState,
                context.ResolvedFilePath);
        }
        catch (Exception ex)
        {
            await _logErrors.LogErrorAsync(ex, $"[DosBoxLaunchStrategy] Error launching CHD: {context.ResolvedFilePath}");
            await _messageBox.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
        }
    }

    private static string GenerateChdConf(string mountPath, string executablePath)
    {
        var executableName = Path.GetFileName(executablePath);
        var executableDir = Path.GetDirectoryName(executablePath);
        var driveLetter = mountPath[0];
        var tempDir = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
        Directory.CreateDirectory(tempDir);
        var confPath = Path.Combine(tempDir, $"{Guid.NewGuid():N}_dosbox_chd.conf");

        string confContent;
        if (!string.IsNullOrEmpty(executableDir) &&
            !executableDir.TrimEnd('\\').Equals(mountPath.TrimEnd('\\'), StringComparison.OrdinalIgnoreCase))
        {
            var relativeDir = executableDir.Replace(mountPath, "").TrimStart('\\', '/');
            confContent = string.Join("\r\n",
                "[dosbox]",
                "",
                "[autoexec]",
                "@echo off",
                $"mount {char.ToLowerInvariant(driveLetter)} \"{mountPath}\" -t cdrom",
                $"{char.ToLowerInvariant(driveLetter)}:",
                $"cd {relativeDir}",
                executableName,
                "exit",
                "");
        }
        else
        {
            confContent = string.Join("\r\n",
                "[dosbox]",
                "",
                "[autoexec]",
                "@echo off",
                $"mount {char.ToLowerInvariant(driveLetter)} \"{mountPath}\" -t cdrom",
                $"{char.ToLowerInvariant(driveLetter)}:",
                executableName,
                "exit",
                "");
        }

        File.WriteAllText(confPath, confContent, Encoding.ASCII);
        DebugLogger.Log($"[DosBoxLaunchStrategy] Generated CHD conf file: {confPath}");

        return confPath;
    }
}