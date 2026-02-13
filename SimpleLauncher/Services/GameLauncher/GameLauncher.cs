using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.ExtractFiles;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.LoadingInterface;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.MountFiles;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SystemManager;
using SimpleLauncher.Services.TrayIcon;
using SimpleLauncher.Services.UsageStats;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher;

public class GameLauncher
{
    private readonly IEnumerable<IEmulatorConfigHandler> _configHandlers;
    private readonly IEnumerable<ILaunchStrategy> _launchStrategies;
    private readonly IConfiguration _configuration;
    private static IHttpClientFactory _httpClientFactory;
    private readonly ILogErrors _logErrors;
    private readonly IExtractionService _extractionService;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly Stats _stats;

    private const int MemoryAccessViolation = -1073741819;
    private const int DepViolation = -1073740791;

    public GameLauncher(
        IEnumerable<IEmulatorConfigHandler> configHandlers,
        IEnumerable<ILaunchStrategy> launchStrategies,
        IExtractionService extraction,
        PlaySoundEffects sounds,
        Stats stats,
        IConfiguration configuration,
        IHttpClientFactory httpClientFactory,
        ILogErrors logErrors)
    {
        _configHandlers = configHandlers;
        _launchStrategies = launchStrategies.OrderBy(static s => s.Priority);
        _extractionService = extraction;
        _playSoundEffects = sounds;
        _stats = stats;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _logErrors = logErrors;
    }

    internal async Task HandleButtonClickAsync(string filePath,
        string selectedEmulatorName,
        string selectedSystemName,
        SystemManager.SystemManager selectedSystemManager,
        SettingsManager.SettingsManager settings,
        MainWindow mainWindow,
        GamePadController gamePadController,
        ILoadingState loadingStateProvider = null)
    {
        loadingStateProvider ??= mainWindow;

        // 1. Create Context
        var context = new LaunchContext
        {
            FilePath = filePath,
            ResolvedFilePath = PathHelper.ResolveRelativeToAppDirectory(filePath),
            EmulatorName = selectedEmulatorName,
            SystemName = selectedSystemName,
            SystemManager = selectedSystemManager,
            Settings = settings,
            MainWindow = mainWindow,
            LoadingState = loadingStateProvider
        };

        try
        {
            // 2. Resolve Emulator Manager
            context.EmulatorManager = context.SystemManager.Emulators.FirstOrDefault(e => e.EmulatorName.Equals(context.EmulatorName, StringComparison.OrdinalIgnoreCase));
            if (context.EmulatorManager == null)
            {
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                // Add logging here for developer context
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, $"Could not find EmulatorManager for emulator '{context.EmulatorName}' in system '{context.SystemName}'.");
                return;
            }

            // 3. Perform Validation
            if (!await ValidateContextAsync(context)) return;

            // 4. Set Parameters
            context.Parameters = context.EmulatorManager.EmulatorParameters;

            // 5. Run Configuration Handlers (Interceptors)
            var handler = _configHandlers.FirstOrDefault(h => h.IsMatch(context.EmulatorName, context.EmulatorManager.EmulatorLocation));
            if (handler != null)
            {
                if (!await handler.HandleConfigurationAsync(context)) return;
            }

            // 6. Pre-launch UI/State
            var wasGamePadRunning = gamePadController.IsRunning;
            if (wasGamePadRunning) gamePadController.Stop();

            var startTime = DateTime.Now;
            context.LoadingState.SetLoadingState(true);

            try
            {
                // 7. Execute Strategy
                var strategy = _launchStrategies.First(s => s.IsMatch(context));
                await strategy.ExecuteAsync(context, this);
            }
            finally
            {
                // 8. Post-launch Cleanup & Stats
                context.LoadingState.SetLoadingState(false);
                if (wasGamePadRunning) gamePadController.Start();

                var playTime = DateTime.Now - startTime;
                if (playTime.TotalSeconds > 5)
                {
                    UpdateStatsAndPlayCountAsync(playTime, context);
                }
            }
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Launch Pipeline Failed");
            await MessageBoxLibrary.CouldNotLaunchGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
        }
    }


    private async Task<bool> ValidateContextAsync(LaunchContext context)
    {
        if (string.IsNullOrWhiteSpace(context.ResolvedFilePath) ||
            (!File.Exists(context.ResolvedFilePath) && !Directory.Exists(context.ResolvedFilePath)))
        {
            var msg = $"File not found: {context.ResolvedFilePath}";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new FileNotFoundException(msg), msg);
            MessageBoxLibrary.FilePathIsInvalid(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
            return false;
        }

        if (string.IsNullOrWhiteSpace(context.EmulatorName))
        {
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, "Emulator name is empty");
            await MessageBoxLibrary.CouldNotLaunchGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
            return false;
        }

        // Add the GroupByFolder check
        if (context.SystemManager.GroupByFolder)
        {
            var emulatorName = context.EmulatorName ?? string.Empty;
            var emulatorLocation = context.EmulatorManager?.EmulatorLocation ?? string.Empty;

            var isMame = emulatorName.Contains("MAME", StringComparison.OrdinalIgnoreCase) ||
                         emulatorLocation.Contains("mame.exe", StringComparison.OrdinalIgnoreCase) ||
                         emulatorLocation.Contains("mame64.exe", StringComparison.OrdinalIgnoreCase);

            if (!isMame)
            {
                MessageBoxLibrary.GroupByFolderOnlyForMameMessageBox();
                return false;
            }
        }

        return true;
    }

    private void UpdateStatsAndPlayCountAsync(TimeSpan playTime, LaunchContext context)
    {
        context.Settings.UpdateSystemPlayTime(context.SystemName, playTime);
        context.Settings.Save();

        var playTimeFormatted = playTime.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture);
        var playTimeLabel = (string)Application.Current.TryFindResource("Playtime") ?? "Playtime";

        TrayIconManager.ShowTrayMessage($"{playTimeLabel}: {playTimeFormatted}");
        UpdateStatusBar.UpdateStatusBar.UpdateContent("", context.MainWindow);

        try
        {
            context.MainWindow.PlayHistoryManager.AddOrUpdatePlayHistoryItem(context.ResolvedFilePath, context.SystemName, playTime);

            var systemPlayTime = context.Settings.SystemPlayTimes.FirstOrDefault(s => s.SystemName == context.SystemName);
            if (systemPlayTime != null)
            {
                context.MainWindow.PlayTime = systemPlayTime.PlayTime;
            }

            context.MainWindow.RefreshGameListAfterPlay(context.ResolvedFilePath, context.SystemName);
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error updating play history");
        }

        _ = _stats.CallApiAsync(context.EmulatorName);
    }

    internal async Task RunBatchFileAsync(string resolvedFilePath, Emulator selectedEmulatorManager, MainWindow mainWindow)
    {
        // On Windows, .bat files are not direct executables.
        // To redirect output (UseShellExecute = false), we must run cmd.exe /c "path_to_script.bat"
        var psi = new ProcessStartInfo
        {
            FileName = "cmd.exe",
            Arguments = $"/c \"{resolvedFilePath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            // CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        // Set the working directory to the directory of the batch file
        try
        {
            var workingDirectory = Path.GetDirectoryName(resolvedFilePath);
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                psi.WorkingDirectory = workingDirectory;
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Could not get workingDirectory for batch file: '{resolvedFilePath}'. Using default.");

            psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory; // Fallback
        }

        var launched = (string)Application.Current.TryFindResource("Launched") ?? "launched";
        DebugLogger.Log("RunBatchFileAsync:\n\n");
        DebugLogger.Log($"Command: {psi.FileName} {psi.Arguments}");
        DebugLogger.Log($"Working Directory: {psi.WorkingDirectory}\n");

        TrayIconManager.ShowTrayMessage($"{Path.GetFileName(resolvedFilePath)} {launched}");
        UpdateStatusBar.UpdateStatusBar.UpdateContent($"{Path.GetFileName(resolvedFilePath)} {launched}", mainWindow);

        using var process = new Process();
        process.StartInfo = psi;

        StringBuilder output = new();
        StringBuilder error = new();

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
                throw new InvalidOperationException("Failed to start the cmd.exe process for the batch file.");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                // Notify developer
                var errorDetail = $"There was an issue running the batch process.\n" +
                                  $"Batch file: {resolvedFilePath}\n" +
                                  $"Exit code {process.ExitCode}\n" +
                                  $"Output: {output}\n" +
                                  $"Error: {error}";
                var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                var contextMessage = $"{errorDetail}\n{userNotified}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    // Notify user
                    MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                }
            }
        }
        catch (Win32Exception ex)
        {
            if (CheckApplicationControlPolicy.CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex))
            {
                MessageBoxLibrary.ApplicationControlPolicyBlockedMessageBox();
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Application control policy blocked launching batch file.");
            }
            else
            {
                string exitCodeInfo;
                try
                {
                    exitCodeInfo = $"Exit code: {(process.HasExited ? process.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A")}";
                }
                catch (InvalidOperationException)
                {
                    exitCodeInfo = "Exit code: N/A (Process not associated)";
                }

                var errorDetail = $"Exception running the batch process.\n" +
                                  $"Batch file: {resolvedFilePath}\n" +
                                  $"{exitCodeInfo}\n" +
                                  $"Exception: {ex.Message}\n" +
                                  $"Output: {output}\n" +
                                  $"Error: {error}";
                var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                var contextMessage = $"{errorDetail}\n{userNotified}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string exitCodeInfo;
            try
            {
                exitCodeInfo = $"Exit code: {(process.HasExited ? process.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A")}";
            }
            catch (InvalidOperationException)
            {
                exitCodeInfo = "Exit code: N/A (Process not associated)";
            }

            var errorDetail = $"Exception running the batch process.\n" +
                              $"Batch file: {resolvedFilePath}\n" +
                              $"{exitCodeInfo}\n" +
                              $"Exception: {ex.Message}\n" +
                              $"Output: {output}\n" +
                              $"Error: {error}";
            var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
            var contextMessage = $"{errorDetail}\n{userNotified}";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
            }
        }
    }

    internal async Task LaunchShortcutFileAsync(string resolvedFilePath, Emulator selectedEmulatorManager, MainWindow mainWindow)
    {
        // Common UI updates.
        var launched = (string)Application.Current.TryFindResource("Launched") ?? "launched";
        var fileName = Path.GetFileName(resolvedFilePath);
        TrayIconManager.ShowTrayMessage($"{fileName} {launched}");
        UpdateStatusBar.UpdateStatusBar.UpdateContent($"{fileName} {launched}", mainWindow);

        try
        {
            var extension = Path.GetExtension(resolvedFilePath).ToUpperInvariant();

            // Validate file exists first
            if (!File.Exists(@"\\?\" + resolvedFilePath))
            {
                throw new FileNotFoundException($"Shortcut file not found: {resolvedFilePath}");
            }

            if (extension == ".URL")
            {
                // Read and validate the .url file content
                var urlContent = await File.ReadAllTextAsync(resolvedFilePath);
#pragma warning disable SYSLIB1045
                var urlMatch = System.Text.RegularExpressions.Regex.Match(urlContent, @"URL=(.+)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
#pragma warning restore SYSLIB1045

                if (!urlMatch.Success || string.IsNullOrWhiteSpace(urlMatch.Groups[1].Value))
                {
                    throw new InvalidOperationException($"Invalid .url file format or missing URL in: {resolvedFilePath}");
                }

                var targetUrl = urlMatch.Groups[1].Value.Trim();
                DebugLogger.Log($"LaunchShortcutFileAsync (.URL):\n\nShortcut File: {resolvedFilePath}\nTarget URL: {targetUrl}\n");

                // Verify protocol handler is registered ONLY if it's a real URI (contains ://)
                // This prevents treating drive letters (C:\) as protocols.
                var protocolIndex = targetUrl.IndexOf("://", StringComparison.Ordinal);
                if (protocolIndex > 0)
                {
                    var protocol = targetUrl[..protocolIndex];
                    if (!IsProtocolRegistered(protocol))
                    {
                        throw new InvalidOperationException($"Protocol handler for '{protocol}://' is not registered. Please ensure the associated application is installed.");
                    }
                }

                // Use shell execution with explicit working directory set to null
                var psi = new ProcessStartInfo
                {
                    FileName = targetUrl, // Launch the URL directly, not the .url file
                    UseShellExecute = true,
                    WorkingDirectory = null // Explicitly no working directory
                };

                using var process = new Process();
                process.StartInfo = psi;

                // For shell execution, Start() might return false if a process was reused.
                // Win32Exception will be thrown if it actually fails.
                process.Start();
            }
            else // .LNK files
            {
                var psi = new ProcessStartInfo
                {
                    FileName = resolvedFilePath,
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(resolvedFilePath) ?? AppDomain.CurrentDomain.BaseDirectory
                };

                DebugLogger.Log($"LaunchShortcutFileAsync (.LNK):\n\nShortcut File: {psi.FileName}\nWorking Directory: {psi.WorkingDirectory}\n");

                using var process = new Process();
                process.StartInfo = psi;

                // For shell execution, Start() might return false if a process was reused.
                process.Start();
            }
        }
        catch (Win32Exception ex) // Catch Win32Exception specifically
        {
            if (CheckApplicationControlPolicy.CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex))
            {
                MessageBoxLibrary.ApplicationControlPolicyBlockedMessageBox();
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Application control policy blocked launching shortcut file.");
            }
            else
            {
                // Existing error handling for other Win32Exceptions
                var fileContent = File.Exists(@"\\?\" + resolvedFilePath)
                    ? $"\nFile Content:\n{await File.ReadAllTextAsync(resolvedFilePath)}"
                    : "\nFile does not exist.";
                var errorDetail = $"Exception launching the shortcut file.\n" +
                                  $"Shortcut file: {resolvedFilePath}\n" +
                                  $"Exception: {ex.Message}" +
                                  fileContent;
                var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                var contextMessage = $"{errorDetail}\n{userNotified}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    var launchErrorTitle = (string)Application.Current.TryFindResource("LaunchErrorTitle") ?? "Launch Error";
                    MessageBoxLibrary.ShowCustomMessageBox("There was a Win32Exception.", launchErrorTitle, PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                }
            }
        }
        catch (Exception ex)
        {
            // Only attempt to read file content if it's a text-based .URL file
            var isUrlFile = Path.GetExtension(resolvedFilePath).Equals(".url", StringComparison.OrdinalIgnoreCase);
            var fileContent = isUrlFile && File.Exists(resolvedFilePath)
                ? $"\nFile Content:\n{await File.ReadAllTextAsync(resolvedFilePath)}"
                : "\nFile content not displayed (binary .LNK or missing file).";

            var errorDetail = $"Exception launching the shortcut file.\n" +
                              $"Shortcut file: {resolvedFilePath}\n" +
                              $"Exception: {ex.Message}" +
                              fileContent;
            var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
            var contextMessage = $"{errorDetail}\n{userNotified}";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                var launchErrorTitle = (string)Application.Current.TryFindResource("LaunchErrorTitle") ?? "Launch Error";
                var couldNotLaunchShortcut = (string)Application.Current.TryFindResource("CouldNotLaunchShortcut") ?? "Could not launch the game shortcut. The protocol handler may not be installed. Please ensure the game launcher (Steam, GOG Galaxy, etc.) is installed.";
                var userMessage = ex switch
                {
                    FileNotFoundException => $"Shortcut file not found: {Path.GetFileName(resolvedFilePath)}.",
                    InvalidOperationException when ex.Message.Contains("Protocol handler for", StringComparison.OrdinalIgnoreCase) => ex.Message,
                    _ => couldNotLaunchShortcut
                };

                MessageBoxLibrary.ShowCustomMessageBox(userMessage, launchErrorTitle, PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
            }
        }
    }

    internal async Task LaunchExecutableAsync(string resolvedFilePath, Emulator selectedEmulatorManager, MainWindow mainWindow)
    {
        var psi = new ProcessStartInfo
        {
            FileName = resolvedFilePath,
            Arguments = "",
            UseShellExecute = false, // Keep false to be able to read ExitCode
            CreateNoWindow = false // Let the app show its own window
        };

        try
        {
            var workingDirectory = Path.GetDirectoryName(resolvedFilePath);
            if (!string.IsNullOrEmpty(workingDirectory))
            {
                psi.WorkingDirectory = workingDirectory;
            }
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Could not get workingDirectory for executable file: '{resolvedFilePath}'. Using default.");
            psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }

        var launched = (string)Application.Current.TryFindResource("Launched") ?? "launched";
        DebugLogger.Log("LaunchExecutableAsync:\n\n");
        DebugLogger.Log($"Executable File: {psi.FileName}");
        DebugLogger.Log($"Working Directory: {psi.WorkingDirectory}\n");

        TrayIconManager.ShowTrayMessage($"{Path.GetFileName(psi.FileName)} {launched}");
        UpdateStatusBar.UpdateStatusBar.UpdateContent($"{Path.GetFileName(psi.FileName)} {launched}", mainWindow);

        using var process = new Process();
        process.StartInfo = psi;

        try
        {
            var processStarted = process.Start();
            if (!processStarted)
            {
                throw new InvalidOperationException("Failed to start the executable process.");
            }

            await process.WaitForExitAsync();

            // Only check for critical failures (crashes, not normal exit codes like 1)
            if (process.ExitCode < 0) // Negative exit codes typically indicate system-level failures
            {
                var errorDetail = $"Executable process exited with system error code.\n" +
                                  $"Executable file: {psi.FileName}\n" +
                                  $"Exit code: {process.ExitCode}";
                var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                var contextMessage = $"{errorDetail}\n{userNotified}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                }
            }
        }
        catch (Win32Exception ex)
        {
            if (CheckApplicationControlPolicy.CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex))
            {
                MessageBoxLibrary.ApplicationControlPolicyBlockedMessageBox();
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Application control policy blocked launching executable.");
            }
            else
            {
                string exitCodeInfo;
                try
                {
                    exitCodeInfo = $"Exit code: {(process.HasExited ? process.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A")}";
                }
                catch (InvalidOperationException)
                {
                    exitCodeInfo = "Exit code: N/A (Process not associated)";
                }

                var errorDetail = $"Exception launching the executable file.\n" +
                                  $"Executable file: {psi.FileName}\n" +
                                  $"{exitCodeInfo}\n" +
                                  $"Exception: {ex.Message}";
                var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                var contextMessage = $"{errorDetail}\n\n{userNotified}";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                }
            }
        }
        catch (Exception ex)
        {
            string exitCodeInfo;
            try
            {
                exitCodeInfo = $"Exit code: {(process.HasExited ? process.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A")}";
            }
            catch (InvalidOperationException)
            {
                exitCodeInfo = "Exit code: N/A (Process not associated)";
            }

            var errorDetail = $"Exception launching the executable file.\n" +
                              $"Executable file: {psi.FileName}\n" +
                              $"{exitCodeInfo}\n" +
                              $"Exception: {ex.Message}";
            var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
            var contextMessage = $"{errorDetail}\n\n{userNotified}";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
            }
        }
    }

    internal async Task LaunchRegularEmulatorAsync(
        string resolvedFilePath,
        string selectedEmulatorName,
        SystemManager.SystemManager selectedSystemManager,
        Emulator selectedEmulatorManager,
        string rawEmulatorParameters,
        MainWindow mainWindow,
        ILoadingState loadingStateProvider)
    {
        var isDirectory = Directory.Exists(resolvedFilePath);

        if (string.IsNullOrEmpty(selectedEmulatorName))
        {
            // Notify developer
            const string contextMessage = "[LaunchRegularEmulatorAsync] selectedEmulatorName is null or empty.";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));

            return;
        }

        // A simple and effective way to identify a mounted XBE path from our tool
        // is by its characteristic filename. This avoids hardcoding drive letters.
        var isMountedXbe = Path.GetFileName(resolvedFilePath).Equals("default.xbe", StringComparison.OrdinalIgnoreCase);

        // Check if the file to launch is a mounted ZIP file, which will not be extracted
        var isMountedZip = resolvedFilePath.StartsWith(MountZipFiles.ConfiguredMountDriveRoot, StringComparison.OrdinalIgnoreCase);

        // Check if it's a file we just converted to temp
        var isTempConvertedFile = resolvedFilePath.Contains(Path.Combine(Path.GetTempPath(), "SimpleLauncher"), StringComparison.OrdinalIgnoreCase);

        // Check if it is the Ootake emulator
        var isOotake = selectedEmulatorName.Contains("Ootake", StringComparison.OrdinalIgnoreCase) ||
                       (selectedEmulatorManager?.EmulatorLocation?.Contains("ootake.exe", StringComparison.OrdinalIgnoreCase) ?? false);

        // Check if it is the Sameboy emulator
        var isSameboy = selectedEmulatorName.Contains("Sameboy", StringComparison.OrdinalIgnoreCase) ||
                        (selectedEmulatorManager?.EmulatorLocation?.Contains("sameboy.exe", StringComparison.OrdinalIgnoreCase) ?? false);

        // Declare tempExtractionPath here to be accessible in the finally block
        string tempExtractionPath = null;
        string tempConvertedPath = null;

        var fileExtension = Path.GetExtension(resolvedFilePath).ToLowerInvariant();

        if ((selectedSystemManager.ExtractFileBeforeLaunch || isOotake || isSameboy) && !isDirectory && !isMountedXbe && !isMountedZip && !isTempConvertedFile)
        {
            if (selectedSystemManager.FileFormatsToLaunch == null || selectedSystemManager.FileFormatsToLaunch.Count == 0)
            {
                // Notify developer
                const string contextMessage = "FileFormatsToLaunch is null or empty, but ExtractFileBeforeLaunch is true for game launching. Cannot determine which file to launch after extraction.";
                await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
                DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

                // Notify user
                MessageBoxLibrary.NullFileExtensionMessageBox();
                return; // Abort
            }

            if (fileExtension is ".zip" or ".rar" or ".7z")
            {
                var extractingMsg = (string)Application.Current.TryFindResource("ExtractingEllipsis") ?? "Extracting file... Please wait.";
                loadingStateProvider.SetLoadingState(true, extractingMsg);
                UpdateStatusBar.UpdateStatusBar.UpdateContent(extractingMsg, mainWindow);

                // Use the extraction service from the DI container
                var (extractedGameFilePath, extractedTempDirPath) = await _extractionService.ExtractToTempAndGetLaunchFileAsync(resolvedFilePath, selectedSystemManager.FileFormatsToLaunch);

                if (!string.IsNullOrEmpty(extractedGameFilePath))
                {
                    resolvedFilePath = extractedGameFilePath;
                }

                // Always store the temp directory path for cleanup, even if no game file was found within it
                tempExtractionPath = extractedTempDirPath;

                var launchingMsg = (string)Application.Current.TryFindResource("Launching") ?? "Launching...";
                loadingStateProvider.SetLoadingState(true, launchingMsg);
            }
        }

        // CHD Handling: If the file is a CHD (either provided directly or extracted from a zip), convert it to CUE/BIN on the fly.
        var isChd = Path.GetExtension(resolvedFilePath).Equals(".chd", StringComparison.OrdinalIgnoreCase);
        var isRaine = selectedEmulatorManager is { EmulatorLocation: not null } && (selectedEmulatorName.Contains("Raine", StringComparison.OrdinalIgnoreCase) ||
                                                                                    selectedEmulatorManager.EmulatorLocation.Contains("raine", StringComparison.OrdinalIgnoreCase));
        var is4Do = selectedEmulatorManager is { EmulatorLocation: not null } && (selectedEmulatorName.Contains("4do", StringComparison.OrdinalIgnoreCase) ||
                                                                                  selectedEmulatorManager.EmulatorLocation.Contains("4do", StringComparison.OrdinalIgnoreCase));
        var isMednafen = selectedEmulatorManager is { EmulatorLocation: not null } && (selectedEmulatorName.Contains("Mednafen", StringComparison.OrdinalIgnoreCase) ||
                                                                                       selectedEmulatorManager.EmulatorLocation.Contains("mednafen", StringComparison.OrdinalIgnoreCase));
        var isXemu = selectedEmulatorManager is { EmulatorLocation: not null } && (selectedEmulatorName.Contains("Xemu", StringComparison.OrdinalIgnoreCase) ||
                                                                                   selectedEmulatorManager.EmulatorLocation.Contains("xemu", StringComparison.OrdinalIgnoreCase));
        var isXenia = selectedEmulatorManager is { EmulatorLocation: not null } && (selectedEmulatorName.Contains("Xenia", StringComparison.OrdinalIgnoreCase) ||
                                                                                    selectedEmulatorManager.EmulatorLocation.Contains("xenia", StringComparison.OrdinalIgnoreCase));

        if (isChd && (isRaine || is4Do || isMednafen))
        {
            var convertingMsg = (string)Application.Current.TryFindResource("ConvertingChdToCue") ?? "Converting CHD...";
            loadingStateProvider.SetLoadingState(true, convertingMsg);

            tempConvertedPath = await Converters.ConvertChdToCueBin.ConvertChdToCueBinAsync(resolvedFilePath);
            if (tempConvertedPath != null)
            {
                resolvedFilePath = tempConvertedPath;
            }
            else
            {
                loadingStateProvider.SetLoadingState(false);
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                return;
            }
        }

        if (isChd && (isXemu || isXenia))
        {
            var convertingMsg = (string)Application.Current.TryFindResource("ConvertingChdToIso") ?? "Converting CHD...";
            loadingStateProvider.SetLoadingState(true, convertingMsg);

            tempConvertedPath = await Converters.ConvertChdToIso.ConvertChdToIsoAsync(resolvedFilePath);
            if (tempConvertedPath != null)
            {
                resolvedFilePath = tempConvertedPath;
            }
            else
            {
                loadingStateProvider.SetLoadingState(false);
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                return;
            }
        }

        if (string.IsNullOrEmpty(resolvedFilePath))
        {
            // Notify developer
            const string contextMessage = "resolvedFilePath is null or empty after extraction attempt (or for mounted files).";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));

            // The finally block will handle cleanup of tempExtractionPath if it was set.
            return;
        }

        // For mounted files, ensure it still exists before proceeding
        if ((isMountedXbe || isMountedZip) && !File.Exists(@"\\?\" + resolvedFilePath))
        {
            // Notify developer
            var contextMessage = $"Mounted file {resolvedFilePath} not found when trying to launch with emulator.";
            await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));

            return;
        }

        // Resolve the Emulator Path (executable)
        if (selectedEmulatorManager != null)
        {
            var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(selectedEmulatorManager.EmulatorLocation);
            if (string.IsNullOrEmpty(resolvedEmulatorExePath) || !File.Exists(@"\\?\" + resolvedEmulatorExePath))
            {
                // Notify developer
                var contextMessage = $"Emulator executable path is null, empty, or does not exist after resolving: '{selectedEmulatorManager.EmulatorLocation}' -> '{resolvedEmulatorExePath}'";
                await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new FileNotFoundException(contextMessage), "Emulator configuration error.");
                DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));

                return;
            }

            // Determine the emulator's directory, which is the base for %EMULATORFOLDER%
            var resolvedEmulatorFolderPath = Path.GetDirectoryName(resolvedEmulatorExePath);
            if (string.IsNullOrEmpty(resolvedEmulatorFolderPath) || !Directory.Exists(@"\\?\" + resolvedEmulatorFolderPath)) // Should exist if exe exists
            {
                // Notify developer
                var contextMessage = $"Could not determine emulator folder path from executable path: '{resolvedEmulatorExePath}'";
                await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
                DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));

                return;
            }

            // Resolve System Folder Path, which is the base for %SYSTEMFOLDER%
            var resolvedSystemFolderPath = PathHelper.ResolveRelativeToAppDirectory(selectedSystemManager.PrimarySystemFolder);
            // Note: SystemFolder might not be strictly required to exist for all emulators/parameters,
            // but if %SYSTEMFOLDER% is used in parameters, this path needs to be valid.

            // Resolve Emulator Parameters using the PathHelper.ResolveParameterString
            var resolvedParameters = PathHelper.ResolveParameterString(
                rawEmulatorParameters, // The raw parameter string from config
                resolvedSystemFolderPath, // The fully resolved system folder path
                resolvedEmulatorFolderPath // The fully resolved emulator directory path
            );

            string arguments;

            // Handling MAME and Raine Arcade Related Games
            // Will load the filename without the extension
            var isMame = selectedEmulatorManager.EmulatorLocation != null && (selectedEmulatorName.Equals("MAME", StringComparison.OrdinalIgnoreCase) ||
                                                                              selectedEmulatorName.Equals("M.A.M.E.", StringComparison.OrdinalIgnoreCase) ||
                                                                              selectedEmulatorManager.EmulatorLocation.Contains("mame.exe", StringComparison.OrdinalIgnoreCase) ||
                                                                              selectedEmulatorManager.EmulatorLocation.Contains("mame64.exe", StringComparison.OrdinalIgnoreCase));

            // Detect NeoGeo CD based on extension
            var ext = Path.GetExtension(resolvedFilePath).ToLowerInvariant();
            var isNeoGeoCd = ext is ".cue" or ".iso" or ".bin";

            if (isMame || (isRaine && !isNeoGeoCd))
            {
                var romName = isDirectory ? Path.GetFileName(resolvedFilePath) : Path.GetFileNameWithoutExtension(resolvedFilePath);
                DebugLogger.Log($"Stripped path call detected (MAME/Raine Arcade). Launching: {romName}");
                arguments = $"{resolvedParameters} \"{romName}\"";
            }
            else
            {
                // General call or Raine NeoGeo CD - Provide full filepath
                arguments = $"{resolvedParameters} \"{resolvedFilePath}\"";
            }

            string workingDirectory;
            try
            {
                // Set the working directory to the directory of the emulator executable
                workingDirectory = resolvedEmulatorFolderPath;
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Could not get workingDirectory for emulator: '{resolvedEmulatorFolderPath}'. Using default.");
                DebugLogger.Log($"Could not get workingDirectory for emulator: '{resolvedEmulatorFolderPath}'. Using default.");

                workingDirectory = AppDomain.CurrentDomain.BaseDirectory; // fallback
            }

            var psi = new ProcessStartInfo
            {
                FileName = resolvedEmulatorExePath,
                Arguments = arguments,
                WorkingDirectory = workingDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                // CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            DebugLogger.Log($"LaunchRegularEmulatorAsync:\n\n" +
                            $"Program Location: {resolvedEmulatorExePath}\n" +
                            $"Arguments: {arguments}\n" +
                            $"Working Directory: {psi.WorkingDirectory}\n" +
                            $"File to launch: {resolvedFilePath}");

            var fileName = Path.GetFileNameWithoutExtension(resolvedFilePath);
            var launchedwith = (string)Application.Current.TryFindResource("launchedwith") ?? "launched with";
            TrayIconManager.ShowTrayMessage($"{fileName} {launchedwith} {selectedEmulatorName}");
            UpdateStatusBar.UpdateStatusBar.UpdateContent($"{fileName} {launchedwith} {selectedEmulatorName}", mainWindow);

            using var process = new Process();
            process.StartInfo = psi;
            StringBuilder output = new();
            StringBuilder error = new();

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
                    throw new InvalidOperationException("Failed to start the process.");
                }

                if (!process.HasExited)
                {
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();
                    await process.WaitForExitAsync();
                }

                if (process.HasExited)
                {
                    if (DoNotCheckErrorsOnSpecificEmulators(selectedEmulatorName, resolvedEmulatorExePath, process, psi, output, error)) return;

                    await CheckForMemoryAccessViolationAsync(process, psi, output, error);
                    await CheckForDepViolationAsync(process, psi, output, error, selectedEmulatorManager);
                    await CheckForExitCodeWithErrorAnyAsync(process, psi, output, error, selectedEmulatorManager);
                }
            }
            catch (InvalidOperationException ex)
            {
                // Notify developer
                const string contextMessage = "InvalidOperationException while launching emulator.";
                await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
                DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    // Notify user
                    await MessageBoxLibrary.InvalidOperationExceptionMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));

                    SupportFromTheDeveloper.DoYouWantToReceiveSupportFromTheDeveloper(_configuration, _httpClientFactory, _logErrors, ex, contextMessage, _playSoundEffects);
                }
            }
            catch (Win32Exception ex) // Catch Win32Exception specifically
            {
                if (CheckApplicationControlPolicy.CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex))
                {
                    MessageBoxLibrary.ApplicationControlPolicyBlockedMessageBox();
                    _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Application control policy blocked launching emulator.");
                }
                else
                {
                    // Existing error handling for other Win32Exceptions
                    // Notify developer
                    // Safely check if the process ever started before trying to access its properties.
                    // A simple way is to check if an ID was ever assigned.
                    string exitCodeInfo;
                    try
                    {
                        // This check is safe even if the process didn't start.
                        _ = process.Id;
                        exitCodeInfo = $"Exit code: {(process.HasExited ? process.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A (Still Running or Failed to get code)")}";
                    }
                    catch (InvalidOperationException)
                    {
                        exitCodeInfo = "Exit code: N/A (Process failed to start)";
                    }

                    var errorDetail = $"{exitCodeInfo}\n" +
                                      $"Emulator: {psi.FileName}\n" +
                                      $"Calling parameters: {psi.Arguments}\n" +
                                      $"Emulator output: {output}\n" +
                                      $"Emulator error: {error}\n";
                    var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                    var contextMessage = $"The emulator could not open the game with the provided parameters. {userNotified}\n\n{errorDetail}";
                    await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                    if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                    {
                        // Notify user
                        await MessageBoxLibrary.CouldNotLaunchGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                        SupportFromTheDeveloper.DoYouWantToReceiveSupportFromTheDeveloper(_configuration, _httpClientFactory, _logErrors, ex, contextMessage, _playSoundEffects);
                    }
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                // Safely check if the process ever started before trying to access its properties.
                // A simple way is to check if an ID was ever assigned.
                string exitCodeInfo;
                try
                {
                    // This check is safe even if the process didn't start.
                    _ = process.Id;
                    exitCodeInfo = $"Exit code: {(process.HasExited ? process.ExitCode.ToString(CultureInfo.InvariantCulture) : "N/A (Still Running or Failed to get code)")}";
                }
                catch (InvalidOperationException)
                {
                    exitCodeInfo = "Exit code: N/A (Process failed to start)";
                }

                var errorDetail = $"{exitCodeInfo}\n" +
                                  $"Emulator: {psi.FileName}\n" +
                                  $"Calling parameters: {psi.Arguments}\n" +
                                  $"Emulator output: {output}\n" +
                                  $"Emulator error: {error}\n";
                var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                var contextMessage = $"The emulator could not open the game with the provided parameters. {userNotified}\n\n{errorDetail}";
                await App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    // Notify user
                    await MessageBoxLibrary.CouldNotLaunchGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                    SupportFromTheDeveloper.DoYouWantToReceiveSupportFromTheDeveloper(_configuration, _httpClientFactory, _logErrors, ex, contextMessage, _playSoundEffects);
                }
            }
            finally
            {
                // Only attempt to delete if a temporary extraction path was actually set
                if (!string.IsNullOrEmpty(tempExtractionPath) && Directory.Exists(tempExtractionPath))
                {
                    try
                    {
                        DebugLogger.Log($"[LaunchRegularEmulatorAsync] Attempting to delete temporary extraction directory: {tempExtractionPath}");
                        Directory.Delete(tempExtractionPath, true); // Use Directory.Delete with recursive=true
                        DebugLogger.Log($"[LaunchRegularEmulatorAsync] Successfully deleted temporary extraction directory: {tempExtractionPath}");
                    }
                    catch (Exception ex)
                    {
                        // Log the error but don't prevent other finally block actions
                        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Failed to delete temporary extraction directory: {tempExtractionPath}");
                        DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error deleting temporary extraction directory {tempExtractionPath}: {ex.Message}");
                    }
                }

                // Cleanup temporary CHD conversion files (.cue, .bin or .iso)
                if (!string.IsNullOrEmpty(tempConvertedPath))
                {
                    try
                    {
                        var binPath = Path.ChangeExtension(tempConvertedPath, ".bin");
                        if (File.Exists(tempConvertedPath))
                        {
                            File.Delete(tempConvertedPath);
                        }

                        if (File.Exists(binPath))
                        {
                            File.Delete(binPath);
                        }

                        DebugLogger.Log($"Cleaned up temporary CHD conversion files: {tempConvertedPath}");
                    }
                    catch (Exception ex)
                    {
                        DebugLogger.Log($"Failed to cleanup CHD temp files: {ex.Message}");
                    }
                }
            }
        }
    }

    private async Task CheckForExitCodeWithErrorAnyAsync(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, Emulator emulatorManager)
    {
        string contextMessage;

        // Ignore MemoryAccessViolation and DepViolation
        if (!process.HasExited || process.ExitCode == 0 || process.ExitCode == MemoryAccessViolation || process.ExitCode == DepViolation)
        {
            return;
        }

        // Check if the output contains "File open/read error" and ignore it,
        // Common RetroArch error that should be ignored
        if (output.ToString().Contains("File open/read error", StringComparison.OrdinalIgnoreCase))
        {
            DebugLogger.Log($"[CheckForExitCodeWithErrorAnyAsync] Ignored exit code {process.ExitCode} due to 'File open/read error' in output.");
            return;
        }

        if (emulatorManager.ReceiveANotificationOnEmulatorError)
        {
            // Notify developer
            contextMessage = $"The emulator could not open the game with the provided parameters.\n" +
                             $"User was notified.\n\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Calling parameters: {psi.Arguments}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
        }
        else
        {
            // Notify developer
            contextMessage = $"The emulator could not open the game with the provided parameters.\n" +
                             $"User was not notified.\n\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Calling parameters: {psi.Arguments}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);
        }

        if (emulatorManager.ReceiveANotificationOnEmulatorError)
        {
            // Notify user
            await MessageBoxLibrary.CouldNotLaunchGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
            SupportFromTheDeveloper.DoYouWantToReceiveSupportFromTheDeveloper(_configuration, _httpClientFactory, _logErrors, null, contextMessage, _playSoundEffects);
        }
    }

    private static Task CheckForMemoryAccessViolationAsync(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error)
    {
        if (process.HasExited && process.ExitCode != MemoryAccessViolation)
        {
            return Task.CompletedTask;
        }

        // Notify developer
        var contextMessage = $"There was a memory access violation error running the emulator.\n" +
                             $"User was not notified.\n\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Calling parameters: {psi.Arguments}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n";
        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

        return Task.CompletedTask;
    }

    private async Task CheckForDepViolationAsync(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, Emulator emulatorManager)
    {
        if (process.HasExited && process.ExitCode != DepViolation)
        {
            return;
        }

        // Notify developer
        var contextMessage = $"Data Execution Prevention (DEP) violation error occurred while running the emulator.\n" +
                             $"User was not notified.\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Calling parameters: {psi.Arguments}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n";
        _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

        if (emulatorManager.ReceiveANotificationOnEmulatorError)
        {
            // Notify user
            await MessageBoxLibrary.CouldNotLaunchGameDueToDepViolation();
            SupportFromTheDeveloper.DoYouWantToReceiveSupportFromTheDeveloper(_configuration, _httpClientFactory, _logErrors, null, contextMessage, _playSoundEffects);
        }
    }

    private bool DoNotCheckErrorsOnSpecificEmulators(string selectedEmulatorName, string resolvedEmulatorExePath, Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error)
    {
        var emulatorsToSkipErrorChecking = _configuration.GetValue<string[]>("EmulatorsToSkipErrorChecking") ??
        [
            "Kega Fusion", "KegaFusion", "Kega", "Fusion", "Fusion.exe", "Project64", "Project 64", "Project64.exe", "Emulicious", "Emulicious.exe", "Speccy", "Speccy.exe"
        ];

        // Check if the emulator name or executable path matches any entry in the skip list
        foreach (var emulatorToSkip in emulatorsToSkipErrorChecking)
        {
            if (selectedEmulatorName.Contains(emulatorToSkip, StringComparison.OrdinalIgnoreCase) ||
                resolvedEmulatorExePath.Contains(emulatorToSkip, StringComparison.OrdinalIgnoreCase))
            {
                // Notify developer
                var contextMessage = $"User just ran {selectedEmulatorName}.\n" +
                                     $"'Simple Launcher' do not track error codes for this emulator.\n\n" +
                                     $"Exit code: {process.ExitCode}\n" +
                                     $"Emulator: {psi.FileName}\n" +
                                     $"Calling parameters: {psi.Arguments}\n" +
                                     $"Emulator output: {output}\n" +
                                     $"Emulator error: {error}\n";
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, contextMessage);

                return true;
            }
        }

        return false;
    }

    private static bool IsProtocolRegistered(string protocol)
    {
        if (string.IsNullOrEmpty(protocol)) return false;

        try
        {
            // Protocol names are case-insensitive in registry, but typically stored lowercase.
            // Ensure we check for the existence of the protocol key itself.
            using var protocolKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(protocol.ToLowerInvariant());
            if (protocolKey == null)
            {
                DebugLogger.Log($"[IsProtocolRegistered] Protocol key '{protocol.ToLowerInvariant()}' not found in HKEY_CLASSES_ROOT.");
                return false;
            }

            // A protocol is considered "registered" if it has a command handler defined.
            // This is typically found under shell\open\command.
            using var shellOpenCommandKey = protocolKey.OpenSubKey(@"shell\open\command");
            if (shellOpenCommandKey == null)
            {
                DebugLogger.Log($"[IsProtocolRegistered] 'shell\\open\\command' subkey not found for protocol '{protocol.ToLowerInvariant()}'.");
                return false;
            }

            var command = shellOpenCommandKey.GetValue(null) as string; // Default value
            if (string.IsNullOrWhiteSpace(command))
            {
                DebugLogger.Log($"[IsProtocolRegistered] Command handler is empty for protocol '{protocol.ToLowerInvariant()}'.");
                return false;
            }

            DebugLogger.Log($"[IsProtocolRegistered] Protocol '{protocol.ToLowerInvariant()}' is registered with command: '{command}'.");
            return true;
        }
        catch (Exception ex)
        {
            // Log any exceptions during registry access.
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, $"Error checking if protocol '{protocol}' is registered.");
            DebugLogger.Log($"[IsProtocolRegistered] Error checking protocol '{protocol}': {ex.Message}");
            return false;
        }
    }
}