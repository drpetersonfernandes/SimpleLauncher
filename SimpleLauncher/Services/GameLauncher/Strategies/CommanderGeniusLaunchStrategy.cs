using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.CleanAndDeleteFiles;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.ExtractFiles;
using SimpleLauncher.Services.GameLauncher.Models;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.TrayIcon;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher.Strategies;

public class CommanderGeniusLaunchStrategy : ILaunchStrategy
{
    private readonly IExtractionService _extractionService;
    private readonly IConfiguration _configuration;

    private static readonly string[] KeenDataExtensions =
    [
        ".CK1", ".CK2", ".CK3", ".CK4", ".CK5", ".CK6"
    ];

    public CommanderGeniusLaunchStrategy(IExtractionService extractionService, IConfiguration configuration)
    {
        _extractionService = extractionService;
        _configuration = configuration;
    }

    public int Priority => 20;

    public bool IsMatch(LaunchContext context)
    {
        if (string.IsNullOrEmpty(context.EmulatorName) ||
            string.IsNullOrEmpty(context.ResolvedFilePath))
            return false;

        if (!context.EmulatorName.Contains("Commander Genius", StringComparison.OrdinalIgnoreCase))
            return false;

        var ext = Path.GetExtension(context.ResolvedFilePath).ToUpperInvariant();
        return ext is ".ZIP" or ".7Z" or ".RAR";
    }

    public async Task ExecuteAsync(LaunchContext context, GameLauncher launcher)
    {
        string extractionDir = null;

        try
        {
            var cgDataPath = GetCommanderGeniusDataPath();
            if (string.IsNullOrEmpty(cgDataPath))
            {
                DebugLogger.Log("[CommanderGeniusLaunchStrategy] Could not resolve CG data path.");
                await LogErrorAsync(context, "Could not resolve Commander Genius data path.");
                return;
            }

            var zipName = Path.GetFileNameWithoutExtension(context.ResolvedFilePath);
            var gamesDir = Path.Combine(cgDataPath, "games");
            extractionDir = Path.Combine(gamesDir, zipName);

            if (Directory.Exists(extractionDir))
            {
                await CleanTempFolder.CleanupTempDirectoryAsync(extractionDir);
            }

            var extracted = await _extractionService.ExtractToFolderAsync(
                context.ResolvedFilePath, extractionDir);

            if (!extracted || !Directory.Exists(extractionDir))
            {
                DebugLogger.Log("[CommanderGeniusLaunchStrategy] Extraction failed or directory not found.");
                return;
            }

            FindAndFlattenGameData(extractionDir);

            var emulatorLocation = PathHelper.ResolveRelativeToAppDirectory(
                context.EmulatorManager.EmulatorLocation);

            if (string.IsNullOrEmpty(emulatorLocation) || !File.Exists(PathHelper.GetLongPath(emulatorLocation)))
            {
                DebugLogger.Log("[CommanderGeniusLaunchStrategy] Emulator executable not found.");
                await LogErrorAsync(context, $"Emulator executable not found: {emulatorLocation}");
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                return;
            }

            var arguments = $"dir=\"games/{zipName}\"";

            DebugLogger.Log($"CommanderGeniusLaunchStrategy:\n\n" +
                            $"Program Location: {emulatorLocation}\n" +
                            $"Arguments: {arguments}\n" +
                            $"Working Directory: {cgDataPath}\n" +
                            $"Zip: {context.ResolvedFilePath}");

            var launchedwith = (string)Application.Current.TryFindResource("launchedwith") ?? "launched with";
            var originalFileName = Path.GetFileNameWithoutExtension(context.FilePath);

            TrayIconManager.ShowTrayMessage($"{originalFileName} {launchedwith} {context.EmulatorName}");
            UpdateStatusBar.UpdateStatusBar.UpdateContent(
                $"{originalFileName} {launchedwith} {context.EmulatorName}", context.MainWindow);

            var psi = new ProcessStartInfo
            {
                FileName = emulatorLocation,
                Arguments = arguments,
                WorkingDirectory = cgDataPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            var output = new StringBuilder();
            var error = new StringBuilder();

            using var process = new Process();
            process.StartInfo = psi;

            process.OutputDataReceived += (_, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    output.AppendLine(args.Data);
                }
            };

            process.ErrorDataReceived += (_, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                {
                    error.AppendLine(args.Data);
                }
            };

            try
            {
                var processStarted = process.Start();
                if (!processStarted)
                {
                    throw new InvalidOperationException("Failed to start Commander Genius process.");
                }

                if (!process.HasExited)
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    await process.WaitForExitAsync();
                }
            }
            catch (Win32Exception ex)
            {
                var exitCodeInfo = SafeGetExitCode(process);
                var errorDetail = $"Commander Genius could not start.\n" +
                                  $"{exitCodeInfo}\n" +
                                  $"Emulator: {psi.FileName}\n" +
                                  $"Arguments: {psi.Arguments}\n" +
                                  $"Error: {ex.Message}";
                await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, errorDetail);

                if (context.EmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    await MessageBoxLibrary.CouldNotLaunchGameMessageBox(
                        PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                }
            }
            catch (Exception ex)
            {
                var exitCodeInfo = SafeGetExitCode(process);
                var errorDetail = $"Commander Genius error.\n" +
                                  $"{exitCodeInfo}\n" +
                                  $"Emulator: {psi.FileName}\n" +
                                  $"Arguments: {psi.Arguments}\n" +
                                  $"Output: {output}\n" +
                                  $"Error: {error}\n" +
                                  $"Exception: {ex.Message}";
                await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, errorDetail);

                if (context.EmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    await MessageBoxLibrary.CouldNotLaunchGameMessageBox(
                        PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                }
            }
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[CommanderGeniusLaunchStrategy] Unexpected error: {ex}");
            await LogErrorAsync(context, $"Unexpected error: {ex.Message}");
        }
        finally
        {
            if (extractionDir != null && Directory.Exists(extractionDir))
            {
                await CleanTempFolder.CleanupTempDirectoryAsync(extractionDir);
            }
        }
    }

    private static string GetCommanderGeniusDataPath()
    {
        var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        if (string.IsNullOrEmpty(documentsPath)) return null;

        var cgPath = Path.Combine(documentsPath, "Commander Genius");
        if (Directory.Exists(cgPath)) return cgPath;

        cgPath = Path.Combine(documentsPath, "My Documents", "Commander Genius");
        if (Directory.Exists(cgPath)) return cgPath;

        cgPath = Path.Combine(documentsPath, "Commander Genius");
        try
        {
            Directory.CreateDirectory(cgPath);
        }
        catch
        {
            return null;
        }

        return cgPath;
    }

    private static void FindAndFlattenGameData(string extractionDir)
    {
        var dirsToScore = new List<string> { extractionDir };
        try
        {
            dirsToScore.AddRange(Directory.EnumerateDirectories(
                extractionDir, "*", SearchOption.AllDirectories));
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[CommanderGenius] Error enumerating directories: {ex.Message}");
        }

        var dirScores = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var dir in dirsToScore)
        {
            try
            {
                var count = Directory.EnumerateFiles(dir)
                    .Count(static f => KeenDataExtensions.Contains(
                        Path.GetExtension(f).ToUpperInvariant()));
                if (count > 0)
                {
                    dirScores[dir] = count;
                }
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"[CommanderGenius] Error scanning directory '{dir}': {ex.Message}");
            }
        }

        string bestDir;
        if (dirScores.Count > 0)
        {
            bestDir = dirScores.OrderByDescending(static kvp => kvp.Value).First().Key;
            DebugLogger.Log($"[CommanderGenius] Game root identified by Keen files: {bestDir} (score: {dirScores[bestDir]})");
        }
        else
        {
            bestDir = ResolveSingleFolderChain(extractionDir);
            if (bestDir != extractionDir)
            {
                DebugLogger.Log($"[CommanderGenius] Game root identified by single-folder chain: {bestDir}");
            }
        }

        if (!string.IsNullOrEmpty(bestDir) &&
            !string.Equals(bestDir, extractionDir, StringComparison.OrdinalIgnoreCase))
        {
            MoveDirectoryContentsToRoot(bestDir, extractionDir);
        }

        CleanEmptySubdirectories(extractionDir);
    }

    private static string ResolveSingleFolderChain(string rootDir)
    {
        try
        {
            var subdirs = Directory.GetDirectories(rootDir);
            if (subdirs.Length != 1) return rootDir;

            var current = subdirs[0];
            while (true)
            {
                var nested = Directory.GetDirectories(current);
                if (nested.Length == 0) break;

                if (nested.Length > 1) return current;

                current = nested[0];
            }

            return current;
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[CommanderGenius] Error resolving folder chain: {ex.Message}");
            return rootDir;
        }
    }

    private static void MoveDirectoryContentsToRoot(string sourceDir, string targetRoot)
    {
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(targetRoot, Path.GetFileName(file));
            try
            {
                if (File.Exists(destFile)) File.Delete(destFile);
                File.Move(file, destFile);
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"[CommanderGenius] Failed to move file '{file}': {ex.Message}");
            }
        }

        foreach (var subdir in Directory.GetDirectories(sourceDir))
        {
            var destDir = Path.Combine(targetRoot, Path.GetFileName(subdir));
            try
            {
                if (Directory.Exists(destDir)) Directory.Delete(destDir, true);
                Directory.Move(subdir, destDir);
            }
            catch (Exception ex)
            {
                DebugLogger.Log($"[CommanderGenius] Failed to move directory '{subdir}': {ex.Message}");
            }
        }

        try
        {
            Directory.Delete(sourceDir, true);
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[CommanderGenius] Failed to delete source directory '{sourceDir}': {ex.Message}");
        }
    }

    private static void CleanEmptySubdirectories(string rootDir)
    {
        try
        {
            foreach (var subdir in Directory.GetDirectories(rootDir))
            {
                try
                {
                    Directory.Delete(subdir, true);
                }
                catch
                {
                    // ignored
                }
            }
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[CommanderGenius] Error cleaning subdirectories: {ex.Message}");
        }
    }

    private static string SafeGetExitCode(Process process)
    {
        try
        {
            return $"Exit code: {(process.HasExited ? process.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A")}";
        }
        catch
        {
            return "Exit code: N/A";
        }
    }

    private static Task LogErrorAsync(LaunchContext context, string message)
    {
        var fullMessage = $"[CommanderGeniusLaunchStrategy] {message}\nFile: {context?.FilePath}";
        return App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, fullMessage);
    }
}