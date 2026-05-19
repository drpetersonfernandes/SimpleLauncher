using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.CleanAndDeleteFiles;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.ExtractFiles;
using SimpleLauncher.Services.GameLauncher.Models;
using SimpleLauncher.Services.MessageBox;

namespace SimpleLauncher.Services.GameLauncher.Strategies;

public class DosBoxLaunchStrategy : ILaunchStrategy
{
    private readonly IExtractionService _extractionService;
    private readonly IConfiguration _configuration;

    private static readonly string[] PriorityGameFormats = [".conf", ".bat", ".exe", ".com"];
    private static readonly List<string> ExtractionFormats = ["conf", "bat", "exe", "com"];

    public DosBoxLaunchStrategy(IExtractionService extractionService, IConfiguration configuration)
    {
        _extractionService = extractionService;
        _configuration = configuration;
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
        return ext is ".ZIP" or ".7Z" or ".RAR";
    }

    public async Task ExecuteAsync(LaunchContext context, GameLauncher launcher)
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
                await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"No DOS game executable found in: {context.ResolvedFilePath}");
                MessageBoxLibrary.CouldNotFindAFileMessageBox();
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
                var dialog = new DosBoxFileSelectionWindow(gameFiles, workingDir);
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

            var launchParameters = context.Parameters ?? string.Empty;
            if (!launchParameters.Contains("-conf", StringComparison.OrdinalIgnoreCase))
            {
                launchParameters = string.IsNullOrWhiteSpace(launchParameters)
                    ? "-conf %ROM%"
                    : launchParameters.Contains("%ROM%", StringComparison.OrdinalIgnoreCase)
                        ? launchParameters.Replace("%ROM%", "-conf %ROM%", StringComparison.OrdinalIgnoreCase)
                        : $"-conf %ROM% {launchParameters}";
            }

            await launcher.LaunchRegularEmulatorAsync(
                confPath,
                context.EmulatorName,
                context.SystemManager,
                context.EmulatorManager,
                launchParameters,
                context.MainWindow,
                context.LoadingState,
                context.ResolvedFilePath);
        }
        catch (Exception ex)
        {
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"[DosBoxLaunchStrategy] Error launching DOS game: {context.ResolvedFilePath}");
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(CheckPaths.PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
        }
        finally
        {
            if (tempDir != null)
            {
                await CleanTempFolder.CleanupTempDirectoryAsync(tempDir);
            }
        }
    }

    private static bool IsDosBoxEmulator(LaunchContext context)
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
}