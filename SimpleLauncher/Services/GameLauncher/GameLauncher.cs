using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Services.CheckApplicationControlPolicy;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.ExtractFiles;
using SimpleLauncher.Core.Services.GameLauncher;
using SimpleLauncher.Core.Services.LoadingInterface;
using SimpleLauncher.Core.Services.SystemManager;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameLauncher.Models;
using SimpleLauncher.Services.GameLauncher.MountFiles;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.TrayIcon;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.UpdateStatusBar;
using SimpleLauncher.Services.UsageStats;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Services.GameLauncher;

public partial class GameLauncher
{
    private readonly IEnumerable<IEmulatorConfigHandler> _configHandlers;
    private readonly IEnumerable<ILaunchStrategy> _launchStrategies;
    private readonly IConfiguration _configuration;
    private readonly IExtractionService _extractionService;
    private readonly Stats _stats;
    private readonly ILogErrors _logErrors;
    private readonly IUpdateStatusBar _updateStatusBar;
    private const int MemoryAccessViolation = -1073741819;
    private const int DepViolation = -1073740791;

    public GameLauncher(
        IEnumerable<IEmulatorConfigHandler> configHandlers,
        IEnumerable<ILaunchStrategy> launchStrategies,
        IExtractionService extraction,
        Stats stats,
        IConfiguration configuration,
        ILogErrors logErrors,
        IUpdateStatusBar updateStatusBar)
    {
        _configHandlers = configHandlers;
        _launchStrategies = launchStrategies.OrderBy(static s => s.Priority);
        _extractionService = extraction;
        _stats = stats;
        _configuration = configuration;
        _logErrors = logErrors;
        _updateStatusBar = updateStatusBar;
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
            // 2. Validate SystemManager and Emulators before resolving
            if (context.SystemManager == null)
            {
                var contextMessage = $"SystemManager is null when attempting to launch.\n" +
                                     $"SystemName: '{context.SystemName}', EmulatorName: '{context.EmulatorName}', FilePath: '{context.FilePath}'";
                _logErrors.LogAndForget(null, contextMessage);
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                return;
            }

            if (context.SystemManager.Emulators == null || context.SystemManager.Emulators.Count == 0)
            {
                var contextMessage = $"SystemManager.Emulators is null or empty for system '{context.SystemName}'.\n" +
                                     $"EmulatorName: '{context.EmulatorName}', FilePath: '{context.FilePath}'";
                _logErrors.LogAndForget(null, contextMessage);
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                return;
            }

            // 3. Resolve Emulator Manager
            context.EmulatorManager = context.SystemManager.Emulators.FirstOrDefault(e => e.EmulatorName.Equals(context.EmulatorName, StringComparison.OrdinalIgnoreCase));
            if (context.EmulatorManager == null)
            {
                MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                _logErrors.LogAndForget(null, $"Could not find EmulatorManager for emulator '{context.EmulatorName}' in system '{context.SystemName}'.");
                return;
            }

            // 4. Perform Validation
            if (!await ValidateContextAsync(context))
            {
                return;
            }

            // 5. Set Parameters
            context.Parameters = context.EmulatorManager.EmulatorParameters;

            // 6. Run Configuration Handlers (Interceptors)
            var handler = _configHandlers.FirstOrDefault(h => h.IsMatch(context.EmulatorName, context.EmulatorManager.EmulatorLocation));
            if (handler != null)
            {
                if (!await handler.HandleConfigurationAsync(context)) return;
            }

            // 7. Pre-launch UI/State
            var wasGamePadRunning = gamePadController.IsRunning;
            if (wasGamePadRunning) gamePadController.Stop();

            var startTime = DateTime.Now;
            context.LoadingState.SetLoadingState(true);

            try
            {
                // 8. Execute Strategy
                var strategy = _launchStrategies.FirstOrDefault(s => s.IsMatch(context));
                if (strategy == null)
                {
                    var errorMessage = $"No launch strategy found for the context: SystemName='{context.SystemName}', EmulatorName='{context.EmulatorName}', FilePath='{context.FilePath}'";
                    _logErrors.LogAndForget(null, errorMessage);
                    MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                    return;
                }

                await strategy.ExecuteAsync(context, this);
            }
            finally
            {
                // 9. Post-launch Cleanup & Stats
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
            var detailedMessage = $"Launch Pipeline Failed.\n" +
                                  $"Exception Type: {ex.GetType().FullName}\n" +
                                  $"SystemName: '{context.SystemName ?? "null"}'\n" +
                                  $"EmulatorName: '{context.EmulatorName ?? "null"}'\n" +
                                  $"FilePath: '{context.FilePath ?? "null"}'\n" +
                                  $"ResolvedFilePath: '{context.ResolvedFilePath ?? "null"}'\n" +
                                  $"SystemManager is null: {context.SystemManager == null}\n" +
                                  $"EmulatorManager is null: {context.EmulatorManager == null}\n" +
                                  $"SystemManager.Emulators is null: {context.SystemManager?.Emulators == null}\n" +
                                  $"Stack Trace: {ex.StackTrace}";
            _logErrors.LogAndForget(ex, detailedMessage);
            await MessageBoxLibrary.CouldNotLaunchGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
        }
    }

    private async Task<bool> ValidateContextAsync(LaunchContext context)
    {
        if (string.IsNullOrWhiteSpace(context.ResolvedFilePath))
        {
            _logErrors.LogAndForget(null, "Resolved file path is empty");
            MessageBoxLibrary.FilePathIsInvalidMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
            return false;
        }

        var standardPath = context.ResolvedFilePath;
        var longPath = PathHelper.GetLongPath(standardPath);

        // Check both standard and long path formats for maximum compatibility
        var standardFileExists = File.Exists(standardPath);
        var longFileExists = File.Exists(longPath);
        var standardDirExists = Directory.Exists(standardPath);
        var longDirExists = Directory.Exists(longPath);

        var fileExists = standardFileExists || longFileExists;
        var directoryExists = standardDirExists || longDirExists;

        // If file doesn't exist, try Unicode normalization variations
        // This handles cases where filenames have different normalization forms (NFC vs NFD)
        // commonly occurring when files are created on different operating systems (macOS vs Windows)
        string normalizedPath = null;
        if (!fileExists && !directoryExists)
        {
            normalizedPath = PathHelper.TryFindFileWithNormalizedPath(standardPath);
            if (!string.IsNullOrEmpty(normalizedPath))
            {
                fileExists = true;
                // Update the resolved path to use the normalized version that actually exists
                context.ResolvedFilePath = normalizedPath;
                DebugLogger.Log($"[ValidateContextAsync] Found file using Unicode normalization: {normalizedPath}");
            }
        }

        if (!fileExists && !directoryExists)
        {
            var msg = $"File not found: {context.ResolvedFilePath}";

            // Check if the path is in a OneDrive folder and provide specific guidance
            if (!string.IsNullOrEmpty(context.ResolvedFilePath) &&
                context.ResolvedFilePath.Contains("OneDrive", StringComparison.OrdinalIgnoreCase))
            {
                var parentDir = Path.GetDirectoryName(context.ResolvedFilePath);
                var oneDriveFolderExists = !string.IsNullOrEmpty(parentDir) && Directory.Exists(parentDir);
                if (!oneDriveFolderExists)
                {
                    msg += "\nThe parent OneDrive folder does not exist or is not accessible. " +
                           "Ensure OneDrive is signed in and synced, and that the folder is available on this device.";
                }
                else
                {
                    msg += "\nThe file is in a OneDrive folder but could not be found. " +
                           "Ensure the file is synced and downloaded to your device. " +
                           "Right-click the file in File Explorer and select 'Always keep on this device'.";
                }
            }

            _logErrors.LogAndForget(new FileNotFoundException(msg), msg);
            MessageBoxLibrary.FilePathIsInvalidMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
            return false;
        }

        // Detect path format mismatch (exists in one format but not the other)
        // This helps identify Unicode normalization or path handling issues
        var hasFileMismatch = standardFileExists != longFileExists;
        var hasDirMismatch = standardDirExists != longDirExists;

        if (hasFileMismatch || hasDirMismatch)
        {
            var mismatchDetails = $"Path validation mismatch detected:\n" +
                                  $"  Original Path: {context.FilePath}\n" +
                                  $"  Resolved Path: {standardPath}\n" +
                                  $"  Long Path: {longPath}\n" +
                                  $"  Normalized Path Found: {normalizedPath ?? "N/A"}\n" +
                                  $"  Standard File.Exists: {standardFileExists}\n" +
                                  $"  Long Path File.Exists: {longFileExists}\n" +
                                  $"  Standard Directory.Exists: {standardDirExists}\n" +
                                  $"  Long Path Directory.Exists: {longDirExists}\n" +
                                  $"  This may indicate a Unicode normalization or path handling issue.";

            DebugLogger.Log(mismatchDetails);

            // Send to developer for investigation but don't block the launch
            _logErrors.LogAndForget(new InvalidOperationException("Path validation mismatch"), mismatchDetails);
        }

        if (string.IsNullOrWhiteSpace(context.EmulatorName))
        {
            _logErrors.LogAndForget(null, "Emulator name is empty");
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

            var isDosBox = emulatorName.Contains("DOSBox", StringComparison.OrdinalIgnoreCase) ||
                           emulatorLocation.Contains("dosbox", StringComparison.OrdinalIgnoreCase);

            if (!isMame && !isDosBox)
            {
                MessageBoxLibrary.GroupByFolderOnlyForMameAndDosBoxMessageBox();
                return false;
            }
        }

        return true;
    }

    private void UpdateStatsAndPlayCountAsync(TimeSpan playTime, LaunchContext context)
    {
        context.Settings.UpdateSystemPlayTime(context.SystemName, playTime);
        context.Settings.SaveAsync();

        var playTimeFormatted = playTime.ToString(@"h\:mm\:ss", CultureInfo.InvariantCulture);
        var playTimeLabel = (string)Application.Current.TryFindResource("Playtime") ?? "Playtime";

        TrayIconManager.ShowTrayMessage($"{playTimeLabel}: {playTimeFormatted}");
        _updateStatusBar.UpdateContent("");

        try
        {
            // Use FilePath (original archive path) instead of ResolvedFilePath (temp extracted path)
            // to ensure play history stores the persistent archive location, not the temp file
            context.MainWindow.PlayHistoryManager.AddOrUpdatePlayHistoryItem(context.FilePath, context.SystemName, playTime);

            var systemPlayTime = context.Settings.SystemPlayTimes.FirstOrDefault(s => s.SystemName.Equals(context.SystemName, StringComparison.OrdinalIgnoreCase));
            if (systemPlayTime != null)
            {
                context.MainWindow.PlayTime = systemPlayTime.FormattedPlayTime;
            }

            context.MainWindow.RefreshGameListAfterPlay(context.FilePath, context.SystemName);
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error updating play history");
        }

        _ = _stats.CallApiAsync(context.EmulatorName);
    }

    internal async Task RunBatchFileAsync(string resolvedFilePath, Emulator selectedEmulatorManager, MainWindow mainWindow)
    {
        var invalidPaths = ValidateBatchFile.FindInvalidQuotedPathsSimple(resolvedFilePath);
        if (invalidPaths.Count > 0)
        {
            var shouldContinue = MessageBoxLibrary.BatchFilePathsMissingMessageBox(invalidPaths);
            if (!shouldContinue) return;
        }

        var psi = new ProcessStartInfo
        {
            FileName = resolvedFilePath,
            UseShellExecute = true
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
            _logErrors.LogAndForget(ex, $"Could not get workingDirectory for batch file: '{resolvedFilePath}'. Using default.");
            psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }

        if (!string.IsNullOrEmpty(psi.WorkingDirectory) && !Directory.Exists(psi.WorkingDirectory))
        {
            var logPath = PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log");
            var msg = $"The working directory for the batch file does not exist: {psi.WorkingDirectory}";
            var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
            _logErrors.LogAndForget(new DirectoryNotFoundException(msg), $"{msg}\n{userNotified}");
            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                MessageBoxLibrary.BatchFileFailedMessageBox(resolvedFilePath, $"Working directory not found: {psi.WorkingDirectory}", logPath);
            }

            return;
        }

        DebugLogger.Log("RunBatchFileAsync:\n\n");
        DebugLogger.Log($"Command: {psi.FileName}");
        DebugLogger.Log($"Working Directory: {psi.WorkingDirectory}\n");

        try
        {
            using var process = Process.Start(psi);
            if (process == null)
            {
                throw new InvalidOperationException("Failed to start the batch file process.");
            }

            await process.WaitForExitAsync();

            var batchShortName = Path.GetFileName(resolvedFilePath);
            if (process.ExitCode != 0)
            {
                var logPath = PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log");
                var errorDetail = $"Batch file exited with code {process.ExitCode}.\nBatch file: {resolvedFilePath}";
                var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                _logErrors.LogAndForget(new InvalidOperationException($"Batch file exited with code {process.ExitCode}"), $"{errorDetail}\n{userNotified}");

                LogBatchFileContentsOnError(resolvedFilePath);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    MessageBoxLibrary.BatchFileFailedMessageBox(resolvedFilePath, $"Exit code: {process.ExitCode}", logPath, process.ExitCode);
                }

                TrayIconManager.ShowTrayMessage($"Error: {batchShortName} failed");
                _updateStatusBar.UpdateContent($"Error: {batchShortName} failed");
            }
            else
            {
                var launched = (string)Application.Current.TryFindResource("Launched") ?? "launched";
                TrayIconManager.ShowTrayMessage($"{batchShortName} {launched}");
                _updateStatusBar.UpdateContent($"{batchShortName} {launched}");
            }
        }
        catch (Win32Exception ex)
        {
            if (CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex))
            {
                MessageBoxLibrary.ApplicationControlPolicyBlockedMessageBox();
                _logErrors.LogAndForget(ex, "Application control policy blocked launching batch file.");
                _updateStatusBar.UpdateContent($"Error: {Path.GetFileName(resolvedFilePath)} failed");
            }
            else if (CheckApplicationControlPolicy.IsElevationRequired(ex))
            {
                MessageBoxLibrary.ElevationRequiredMessageBox();
                _logErrors.LogAndForget(ex, "Elevation required to launch batch file.");
                _updateStatusBar.UpdateContent($"Error: {Path.GetFileName(resolvedFilePath)} failed");
            }
            else if (CheckApplicationControlPolicy.IsOperationCanceledByUser(ex))
            {
                // User canceled the operation (e.g., clicked Cancel on UAC prompt) - do nothing, don't log
            }
            else
            {
                var errorDetail = $"Exception running the batch process.\nBatch file: {resolvedFilePath}\nException: {ex.Message}";
                var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                _logErrors.LogAndForget(ex, $"{errorDetail}\n{userNotified}");

                LogBatchFileContentsOnError(resolvedFilePath);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    var logPath = PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log");
                    MessageBoxLibrary.BatchFileFailedMessageBox(resolvedFilePath, ex.Message, logPath);
                }

                _updateStatusBar.UpdateContent($"Error: {Path.GetFileName(resolvedFilePath)} failed");
            }
        }
        catch (Exception ex)
        {
            var errorDetail = $"Exception running the batch process.\nBatch file: {resolvedFilePath}\nException: {ex.Message}";
            var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
            _logErrors.LogAndForget(ex, $"{errorDetail}\n{userNotified}");

            LogBatchFileContentsOnError(resolvedFilePath);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                var logPath = PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log");
                MessageBoxLibrary.BatchFileFailedMessageBox(resolvedFilePath, ex.Message, logPath);
            }

            _updateStatusBar.UpdateContent($"Error: {Path.GetFileName(resolvedFilePath)} failed");
        }
    }

    private static void LogBatchFileContentsOnError(string batchFilePath)
    {
        try
        {
            if (File.Exists(batchFilePath))
            {
                var contents = File.ReadAllText(batchFilePath);
                DebugLogger.Log($"Batch file contents for '{batchFilePath}':\n{contents}");
                DebugLogger.Log("End of batch file contents.");
            }
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"Could not read batch file contents for logging: {ex.Message}");
        }
    }

    internal async Task LaunchShortcutFileAsync(string resolvedFilePath, Emulator selectedEmulatorManager, MainWindow mainWindow)
    {
        // Common UI updates.
        var launched = (string)Application.Current.TryFindResource("Launched") ?? "launched";
        var fileName = Path.GetFileName(resolvedFilePath);
        TrayIconManager.ShowTrayMessage($"{fileName} {launched}");
        _updateStatusBar.UpdateContent($"{fileName} {launched}");

        try
        {
            var extension = Path.GetExtension(resolvedFilePath).ToUpperInvariant();

            // Validate file exists first
            if (!File.Exists(PathHelper.GetLongPath(resolvedFilePath)))
            {
                throw new FileNotFoundException($"Shortcut file not found: {resolvedFilePath}");
            }

            if (extension == ".URL")
            {
                // Read and validate the .url file content
                var urlContent = await File.ReadAllTextAsync(resolvedFilePath);
                var urlMatch = MyRegex().Match(urlContent);

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
                        MessageBoxLibrary.ProtocolHandlerNotRegisteredMessageBox(protocol);
                        return;
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
            if (CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex))
            {
                MessageBoxLibrary.ApplicationControlPolicyBlockedMessageBox();
                _logErrors.LogAndForget(ex, "Application control policy blocked launching shortcut file.");
            }
            else if (CheckApplicationControlPolicy.IsElevationRequired(ex))
            {
                MessageBoxLibrary.ElevationRequiredMessageBox();
                _logErrors.LogAndForget(ex, "Elevation required to launch shortcut file.");
            }
            else if (CheckApplicationControlPolicy.IsOperationCanceledByUser(ex))
            {
                // User canceled the operation (e.g., clicked Cancel on UAC prompt) - do nothing, don't log
            }
            else
            {
                // Existing error handling for other Win32Exceptions
                var fileContent = File.Exists(PathHelper.GetLongPath(resolvedFilePath))
                    ? $"\nFile Content:\n{await File.ReadAllTextAsync(PathHelper.GetLongPath(resolvedFilePath))}"
                    : "\nFile does not exist.";
                var errorDetail = $"Exception launching the shortcut file.\n" +
                                  $"Shortcut file: {resolvedFilePath}\n" +
                                  $"Exception: {ex.Message}" +
                                  fileContent;
                var userNotified = selectedEmulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
                var contextMessage = $"{errorDetail}\n{userNotified}";
                _logErrors.LogAndForget(ex, contextMessage);

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
            _logErrors.LogAndForget(ex, contextMessage);

            if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
            {
                var launchErrorTitle = (string)Application.Current.TryFindResource("LaunchErrorTitle") ?? "Launch Error";
                var couldNotLaunchShortcut = (string)Application.Current.TryFindResource("CouldNotLaunchShortcut") ?? "Could not launch the game shortcut. The protocol handler may not be installed. Please ensure the game launcher (Steam, GOG Galaxy, etc.) is installed.";
                var userMessage = ex switch
                {
                    FileNotFoundException => $"Shortcut file not found: {Path.GetFileName(resolvedFilePath)}.",
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
            _logErrors.LogAndForget(ex, $"Could not get workingDirectory for executable file: '{resolvedFilePath}'. Using default.");
            psi.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
        }

        var launched = (string)Application.Current.TryFindResource("Launched") ?? "launched";
        DebugLogger.Log("LaunchExecutableAsync:\n\n");
        DebugLogger.Log($"Executable File: {psi.FileName}");
        DebugLogger.Log($"Working Directory: {psi.WorkingDirectory}\n");

        TrayIconManager.ShowTrayMessage($"{Path.GetFileName(psi.FileName)} {launched}");
        _updateStatusBar.UpdateContent($"{Path.GetFileName(psi.FileName)} {launched}");

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
                _logErrors.LogAndForget(null, contextMessage);

                if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                {
                    MessageBoxLibrary.ThereWasAnErrorLaunchingThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                }
            }
        }
        catch (Win32Exception ex)
        {
            if (CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex))
            {
                MessageBoxLibrary.ApplicationControlPolicyBlockedMessageBox();
                _logErrors.LogAndForget(ex, "Application control policy blocked launching executable.");
            }
            else if (CheckApplicationControlPolicy.IsElevationRequired(ex))
            {
                MessageBoxLibrary.ElevationRequiredMessageBox();
                _logErrors.LogAndForget(ex, "Elevation required to launch executable.");
            }
            else if (CheckApplicationControlPolicy.IsOperationCanceledByUser(ex))
            {
                // User canceled the operation (e.g., clicked Cancel on UAC prompt) - do nothing, don't log
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
                _logErrors.LogAndForget(ex, contextMessage);

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
            _logErrors.LogAndForget(ex, contextMessage);

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
        ILoadingState loadingStateProvider,
        string originalFilePathForDisplay = null)
    {
        // Use the original file path for display if provided (e.g., for mounted files),
        // otherwise use the resolved file path.
        // This ensures we show the original archive name, not the temp extracted or mounted file.
        var displayFilePath = originalFilePathForDisplay ?? resolvedFilePath;
        var originalFileName = Path.GetFileNameWithoutExtension(displayFilePath);

        var isDirectory = Directory.Exists(resolvedFilePath);

        if (string.IsNullOrEmpty(selectedEmulatorName))
        {
            // Notify developer
            const string contextMessage = "[LaunchRegularEmulatorAsync] selectedEmulatorName is null or empty.";
            _logErrors.LogAndForget(null, contextMessage);
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

        var isChd = Path.GetExtension(resolvedFilePath).Equals(".chd", StringComparison.OrdinalIgnoreCase);
        var isCue = Path.GetExtension(resolvedFilePath).Equals(".cue", StringComparison.OrdinalIgnoreCase);
        var isBin = Path.GetExtension(resolvedFilePath).Equals(".bin", StringComparison.OrdinalIgnoreCase);
        var isIso = Path.GetExtension(resolvedFilePath).Equals(".iso", StringComparison.OrdinalIgnoreCase);
        var isZip = Path.GetExtension(resolvedFilePath).Equals(".zip", StringComparison.OrdinalIgnoreCase);
        var is7Z = Path.GetExtension(resolvedFilePath).Equals(".7z", StringComparison.OrdinalIgnoreCase);
        var isRar = Path.GetExtension(resolvedFilePath).Equals(".rar", StringComparison.OrdinalIgnoreCase);

        var isAzahar = selectedEmulatorManager?.EmulatorLocation != null && (selectedEmulatorName.Contains("Azahar", StringComparison.OrdinalIgnoreCase) ||
                                                                             selectedEmulatorManager.EmulatorLocation.Contains("azahar.exe", StringComparison.OrdinalIgnoreCase));

        var isCitra = selectedEmulatorManager?.EmulatorLocation != null && (selectedEmulatorName.Contains("Citra", StringComparison.OrdinalIgnoreCase) ||
                                                                            selectedEmulatorManager.EmulatorLocation.Contains("citra.exe", StringComparison.OrdinalIgnoreCase) ||
                                                                            selectedEmulatorManager.EmulatorLocation.Contains("citra-qt.exe", StringComparison.OrdinalIgnoreCase));

        var isDuckstation = selectedEmulatorManager?.EmulatorLocation != null && (selectedEmulatorName.Contains("duckstation", StringComparison.OrdinalIgnoreCase) ||
                                                                                  selectedEmulatorManager.EmulatorLocation.Contains("duckstation", StringComparison.OrdinalIgnoreCase));

        var isGeolith = selectedEmulatorManager?.EmulatorLocation != null && (selectedEmulatorManager.EmulatorParameters.Contains("geolith_libretro", StringComparison.OrdinalIgnoreCase) ||
                                                                              selectedEmulatorManager.EmulatorParameters.Contains("geolith_libretro.dll", StringComparison.OrdinalIgnoreCase));

        var isMame = selectedEmulatorManager?.EmulatorLocation != null && (selectedEmulatorName.Equals("MAME", StringComparison.OrdinalIgnoreCase) ||
                                                                           selectedEmulatorName.Equals("M.A.M.E.", StringComparison.OrdinalIgnoreCase) ||
                                                                           selectedEmulatorManager.EmulatorLocation.Contains("mame.exe", StringComparison.OrdinalIgnoreCase) ||
                                                                           selectedEmulatorManager.EmulatorLocation.Contains("mame64.exe", StringComparison.OrdinalIgnoreCase));

        var isOotake = selectedEmulatorManager?.EmulatorLocation != null && (selectedEmulatorName.Contains("Ootake", StringComparison.OrdinalIgnoreCase) ||
                                                                             selectedEmulatorManager.EmulatorLocation.Contains("ootake.exe", StringComparison.OrdinalIgnoreCase));

        var isRaine = selectedEmulatorManager?.EmulatorLocation != null && (selectedEmulatorName.Contains("Raine", StringComparison.OrdinalIgnoreCase) ||
                                                                            selectedEmulatorManager.EmulatorLocation.Contains("raine", StringComparison.OrdinalIgnoreCase));

        var isRetroArch = selectedEmulatorManager?.EmulatorLocation != null && (selectedEmulatorManager.EmulatorName.Contains("retroarch", StringComparison.OrdinalIgnoreCase) ||
                                                                                selectedEmulatorManager.EmulatorLocation.Contains("retroarch", StringComparison.OrdinalIgnoreCase));

        var isSameboy = selectedEmulatorManager?.EmulatorLocation != null && (selectedEmulatorName.Contains("Sameboy", StringComparison.OrdinalIgnoreCase) ||
                                                                              selectedEmulatorManager.EmulatorLocation.Contains("sameboy.exe", StringComparison.OrdinalIgnoreCase));

        var isXemu = selectedEmulatorManager?.EmulatorLocation != null && (selectedEmulatorName.Contains("Xemu", StringComparison.OrdinalIgnoreCase) ||
                                                                           selectedEmulatorManager.EmulatorLocation.Contains("xemu", StringComparison.OrdinalIgnoreCase));

        // Declare tempExtractionPath here to be accessible in the finally block
        string tempExtractionPath = null;

        var fileExtension = Path.GetExtension(resolvedFilePath).ToLowerInvariant();

        if (isRetroArch && (selectedEmulatorManager != null) && (!selectedEmulatorManager.EmulatorParameters.Contains("-L", StringComparison.OrdinalIgnoreCase)))
        {
            var errorMessage = $"[LaunchRegularEmulatorAsync] RetroArch parameter should contain -L. Parameter field: {selectedEmulatorManager.EmulatorParameters}";
            DebugLogger.Log(errorMessage);
            _logErrors.LogAndForget(null, errorMessage);

            MessageBoxLibrary.RetroArchParameterShouldContainLMessageBox();

            return;
        }

        if (isXemu && (selectedEmulatorManager != null) && (!selectedEmulatorManager.EmulatorParameters.Contains("-dvd_path", StringComparison.OrdinalIgnoreCase)))
        {
            var errorMessage = $"[LaunchRegularEmulatorAsync] Xemu parameter should contain '-dvd_path'. Parameter field: {selectedEmulatorManager.EmulatorParameters}";
            DebugLogger.Log(errorMessage);
            _logErrors.LogAndForget(null, errorMessage);

            MessageBoxLibrary.XemuParameterShouldContainDvdPathMessageBox();

            return;
        }

        if ((selectedSystemManager.ExtractFileBeforeLaunch || isAzahar || isCitra || isDuckstation || isOotake || isSameboy) && !isDirectory && !isMountedXbe && !isMountedZip && !isTempConvertedFile)
        {
            if (fileExtension is ".zip" or ".rar" or ".7z")
            {
                var extractingMsg = (string)Application.Current.TryFindResource("ExtractingEllipsis") ?? "Extracting file... Please wait.";
                loadingStateProvider.SetLoadingState(true, extractingMsg);
                _updateStatusBar.UpdateContent(extractingMsg);

                try
                {
                    var (extractedGameFilePath, extractedTempDirPath) = await _extractionService.ExtractToTempAndGetLaunchFileAsync(resolvedFilePath, selectedSystemManager.FileFormatsToLaunch);

                    if (!string.IsNullOrEmpty(extractedGameFilePath))
                    {
                        resolvedFilePath = extractedGameFilePath;
                    }

                    // Always store the temp directory path for cleanup, even if no game file was found within it
                    tempExtractionPath = extractedTempDirPath;
                }
                finally
                {
                    // End extraction loading state before starting launch state
                    loadingStateProvider.SetLoadingState(false);
                }

                // Update message for launching without incrementing count (caller already has loading state active)
                var launchingMsg = (string)Application.Current.TryFindResource("Launching") ?? "Launching...";
                _updateStatusBar.UpdateContent(launchingMsg);
            }
        }

        if (isOotake && (isChd || isBin || isCue || isIso))
        {
            MessageBoxLibrary.OotakeDoesNotSupportImageFilesMessageBox();
            return;
        }

        if (isGeolith && (isZip || is7Z || isRar))
        {
            MessageBoxLibrary.GeolithDoesNotSupportCompressedFilesMessageBox();
            return;
        }

        if (string.IsNullOrEmpty(resolvedFilePath))
        {
            // Notify developer
            const string contextMessage = "resolvedFilePath is null or empty after extraction attempt (or for mounted files).";
            _logErrors.LogAndForget(null, contextMessage);
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));

            // The finally block will handle cleanup of tempExtractionPath if it was set.
            return;
        }

        // For mounted files, ensure it still exists before proceeding
        if ((isMountedXbe || isMountedZip) && !File.Exists(PathHelper.GetLongPath(resolvedFilePath)))
        {
            // Notify developer
            var contextMessage = $"Mounted file {resolvedFilePath} not found when trying to launch with emulator.";
            _logErrors.LogAndForget(null, contextMessage);
            DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

            // Notify user
            MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));

            return;
        }

        // Resolve the Emulator Path (executable)
        if (selectedEmulatorManager != null)
        {
            // Check if emulator location is empty or null
            if (string.IsNullOrWhiteSpace(selectedEmulatorManager.EmulatorLocation))
            {
                // Notify developer
                var contextMessage = $"EmulatorLocation is null or empty for emulator '{selectedEmulatorName}'. " +
                                     $"This typically means the system was configured to run directly executable files (.bat, .exe, .lnk) " +
                                     $"but the user is trying to launch a non-executable file that requires an emulator.";
                _logErrors.LogAndForget(null, contextMessage);
                DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

                // Notify user with a helpful message
                MessageBoxLibrary.EmulatorPathNotConfiguredMessageBox();

                return;
            }

            var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(selectedEmulatorManager.EmulatorLocation);
            if (string.IsNullOrEmpty(resolvedEmulatorExePath) || !File.Exists(PathHelper.GetLongPath(resolvedEmulatorExePath)))
            {
                // Notify developer
                var contextMessage = $"Emulator executable path is null, empty, or does not exist after resolving: '{selectedEmulatorManager.EmulatorLocation}' -> '{resolvedEmulatorExePath}'";
                _logErrors.LogAndForget(new FileNotFoundException(contextMessage), "Emulator configuration error.");
                DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));

                return;
            }

            // Determine the emulator's directory, which is the base for %EMULATORFOLDER%
            var resolvedEmulatorFolderPath = Path.GetDirectoryName(resolvedEmulatorExePath);
            if (string.IsNullOrEmpty(resolvedEmulatorFolderPath) || !Directory.Exists(PathHelper.GetLongPath(resolvedEmulatorFolderPath))) // Should exist if exe exists
            {
                // Notify developer
                var contextMessage = $"Could not determine emulator folder path from executable path: '{resolvedEmulatorExePath}'";
                _logErrors.LogAndForget(null, contextMessage);
                DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

                // Notify user
                MessageBoxLibrary.CouldNotLaunchThisGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));

                return;
            }

            // Resolve the system folder that contains this specific ROM
            var romSystemFolder = PathHelper.FindContainingSystemFolder(selectedSystemManager, resolvedFilePath);

            // Compute the ROM name (filename without extension, or folder name for directories)
            // before ResolveParameterString so %NAME% can be resolved
            var romName = isDirectory ? Path.GetFileName(resolvedFilePath) : Path.GetFileNameWithoutExtension(resolvedFilePath);

            // Resolve Emulator Parameters using the PathHelper.ResolveParameterString
            var resolvedParameters = PathHelper.ResolveParameterString(
                rawEmulatorParameters, // The raw parameter string from config
                selectedSystemManager.SystemFolders, // All configured system folders
                resolvedEmulatorFolderPath, // The fully resolved emulator directory path
                resolvedFilePath, // The ROM path for %ROM%
                romSystemFolder, // The system folder containing this ROM for %ROMSYSTEMFOLDER%
                romName // The ROM name (filename without extension) for %NAME%
            );

            string arguments;

            // Detect NeoGeo CD based on extension
            var ext = Path.GetExtension(resolvedFilePath).ToLowerInvariant();
            var isNeoGeoCd = ext is ".cue" or ".iso" or ".bin";

            // If the original parameters contained %ROM%, it was already resolved by ResolveParameterString
            // and we should not append the file path again.
            var containsRomPlaceholder = rawEmulatorParameters.Contains("%ROM%", StringComparison.OrdinalIgnoreCase);
            var containsNamePlaceholder = rawEmulatorParameters.Contains("%NAME%", StringComparison.OrdinalIgnoreCase);

            if (containsRomPlaceholder || containsNamePlaceholder || PathHelper.ContainsGameSpecificPlaceholder(resolvedParameters))
            {
                arguments = resolvedParameters;
            }
            else
            {
                // Trim trailing spaces and check if it ends with '=' to avoid adding an extra space
                var trimmedParameters = resolvedParameters?.TrimEnd() ?? string.Empty;
                var space = (string.IsNullOrWhiteSpace(trimmedParameters) || trimmedParameters.EndsWith('=')) ? "" : " ";

                // Will load the filename without the extension
                if ((isMame || isRaine) && !isNeoGeoCd)
                {
                    DebugLogger.Log($"Stripped path call detected. Launching: {romName}");
                    arguments = $"{trimmedParameters}{space}\"{romName}\"";
                }
                else
                {
                    // General call or Raine NeoGeo CD - Provide full filepath
                    arguments = $"{trimmedParameters}{space}\"{resolvedFilePath}\"";
                }
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
                _logErrors.LogAndForget(ex, $"Could not get workingDirectory for emulator: '{resolvedEmulatorFolderPath}'. Using default.");
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
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            DebugLogger.Log($"LaunchRegularEmulatorAsync:\n\n" +
                            $"Program Location: {resolvedEmulatorExePath}\n" +
                            $"Arguments: {arguments}\n" +
                            $"Working Directory: {psi.WorkingDirectory}\n" +
                            $"File to launch: {resolvedFilePath}");

            var launchedwith = (string)Application.Current.TryFindResource("launchedwith") ?? "launched with";

            TrayIconManager.ShowTrayMessage($"{originalFileName} {launchedwith} {selectedEmulatorName}");
            _updateStatusBar.UpdateContent($"{originalFileName} {launchedwith} {selectedEmulatorName}");

            StringBuilder output = new();
            StringBuilder error = new();

            using (var process = new Process())
            {
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
                    _logErrors.LogAndForget(ex, contextMessage);
                    DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error: {contextMessage}");

                    if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                    {
                        // Notify user
                        await MessageBoxLibrary.InvalidOperationExceptionMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                        // SupportFromTheDeveloper.DoYouWantToReceiveSupportFromTheDeveloper(_configuration, _httpClientFactory, _logErrors, ex, contextMessage, _playSoundEffects);
                    }
                }
                catch (Win32Exception ex) // Catch Win32Exception specifically
                {
                    if (CheckApplicationControlPolicy.IsApplicationControlPolicyBlocked(ex))
                    {
                        MessageBoxLibrary.ApplicationControlPolicyBlockedMessageBox();
                        _logErrors.LogAndForget(ex, "Application control policy blocked launching emulator.");
                    }
                    else if (CheckApplicationControlPolicy.IsElevationRequired(ex))
                    {
                        MessageBoxLibrary.ElevationRequiredMessageBox();
                        _logErrors.LogAndForget(ex, "Elevation required to launch emulator.");
                    }
                    else if (CheckApplicationControlPolicy.IsOperationCanceledByUser(ex))
                    {
                        // User canceled the operation (e.g., clicked Cancel on UAC prompt) - do nothing, don't log
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
                        _logErrors.LogAndForget(ex, contextMessage);

                        if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                        {
                            // Notify user
                            await MessageBoxLibrary.CouldNotLaunchGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                            // SupportFromTheDeveloper.DoYouWantToReceiveSupportFromTheDeveloper(_configuration, _httpClientFactory, _logErrors, ex, contextMessage, _playSoundEffects);
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
                    _logErrors.LogAndForget(ex, contextMessage);

                    if (selectedEmulatorManager.ReceiveANotificationOnEmulatorError)
                    {
                        // Notify user
                        await MessageBoxLibrary.CouldNotLaunchGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
                        // SupportFromTheDeveloper.DoYouWantToReceiveSupportFromTheDeveloper(_configuration, _httpClientFactory, _logErrors, ex, contextMessage, _playSoundEffects);
                    }
                }
            }

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
                    // Log the error but don't prevent other cleanup actions
                    _logErrors.LogAndForget(ex, $"Failed to delete temporary extraction directory: {tempExtractionPath}");
                    DebugLogger.Log($"[LaunchRegularEmulatorAsync] Error deleting temporary extraction directory {tempExtractionPath}: {ex.Message}");
                }
            }
        }
    }

    private Task CheckForExitCodeWithErrorAnyAsync(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, Emulator emulatorManager)
    {
        var userNotified = emulatorManager.ReceiveANotificationOnEmulatorError ? "User was notified." : "User was not notified.";
        var contextMessage = $"The emulator could not open the game with the provided parameters.\n" +
                             $"{userNotified}\n\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Calling parameters: {psi.Arguments}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n";

        // Ignore MemoryAccessViolation and DepViolation
        if (!process.HasExited || process.ExitCode == 0 || process.ExitCode == MemoryAccessViolation || process.ExitCode == DepViolation)
        {
            return Task.CompletedTask;
        }

        // Handle common RetroArch error that should be ignored
        if (output.ToString().Contains("File open/read error", StringComparison.OrdinalIgnoreCase))
        {
            DebugLogger.Log($"[CheckForExitCodeWithErrorAnyAsync] Ignored exit code {process.ExitCode} due to 'File open/read error' in output.");
            return Task.CompletedTask;
        }

        // Handle RetroArch failing due to special characters in path (mkdir permission denied)
        if ((emulatorManager.EmulatorName.Contains("retroarch", StringComparison.OrdinalIgnoreCase) ||
             emulatorManager.EmulatorLocation.Contains("retroarch", StringComparison.OrdinalIgnoreCase)) &&
            output.ToString().Contains("mkdir(", StringComparison.OrdinalIgnoreCase) &&
            output.ToString().Contains("Permission denied", StringComparison.OrdinalIgnoreCase))
        {
            DebugLogger.Log("[CheckForExitCodeWithErrorAnyAsync] RetroArch mkdir permission denied due to special characters in path.");
            _logErrors.LogAndForget(null, contextMessage);

            if (emulatorManager.ReceiveANotificationOnEmulatorError)
            {
                MessageBoxLibrary.RetroArchSpecialCharactersInPathMessageBox();
                MessageBoxLibrary.WouldYouLikeToOpenTheLogMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
            }

            return Task.CompletedTask;
        }

        // Handle RetroArch parameter issues
        if (emulatorManager.EmulatorName.Contains("retroarch", StringComparison.OrdinalIgnoreCase) ||
            emulatorManager.EmulatorLocation.Contains("retroarch", StringComparison.OrdinalIgnoreCase))
        {
            DebugLogger.Log("[CheckForExitCodeWithErrorAnyAsync] RetroArch parameter issues.");
            _logErrors.LogAndForget(null, contextMessage);

            if (emulatorManager.ReceiveANotificationOnEmulatorError)
            {
                MessageBoxLibrary.RetroArchParameterIssueMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
            }

            return Task.CompletedTask;
        }

        // Handle MAME Not Found error
        if ((emulatorManager.EmulatorName.Contains("MAME", StringComparison.OrdinalIgnoreCase) ||
             emulatorManager.EmulatorLocation.Contains("mame", StringComparison.OrdinalIgnoreCase) ||
             emulatorManager.EmulatorLocation.Contains("mame64", StringComparison.OrdinalIgnoreCase) ||
             emulatorManager.EmulatorLocation.Contains("retroarch", StringComparison.OrdinalIgnoreCase)) &&
            (output.ToString().Contains("Not Found", StringComparison.OrdinalIgnoreCase) ||
             output.ToString().Contains("WRONG LENGTH", StringComparison.OrdinalIgnoreCase) ||
             output.ToString().Contains("Required files are missing", StringComparison.OrdinalIgnoreCase)))
        {
            DebugLogger.Log("[CheckForExitCodeWithErrorAnyAsync] MAME ROM set error.");
            _logErrors.LogAndForget(null, contextMessage);

            if (emulatorManager.ReceiveANotificationOnEmulatorError)
            {
                MessageBoxLibrary.MameRomSetErrorMessageBox();
                MessageBoxLibrary.WouldYouLikeToOpenTheLogMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
            }

            return Task.CompletedTask;
        }

        // Handle MAME Unknown system error
        if ((emulatorManager.EmulatorName.Contains("MAME", StringComparison.OrdinalIgnoreCase) ||
             emulatorManager.EmulatorName.Contains("retroarch", StringComparison.OrdinalIgnoreCase) ||
             emulatorManager.EmulatorLocation.Contains("mame", StringComparison.OrdinalIgnoreCase) ||
             emulatorManager.EmulatorLocation.Contains("retroarch", StringComparison.OrdinalIgnoreCase)) &&
            (output.ToString().Contains("Unknown system", StringComparison.OrdinalIgnoreCase) ||
             output.ToString().Contains("approximately matches the following", StringComparison.OrdinalIgnoreCase)))
        {
            DebugLogger.Log("[CheckForExitCodeWithErrorAnyAsync] MAME Unknown system error.");
            _logErrors.LogAndForget(null, contextMessage);

            if (emulatorManager.ReceiveANotificationOnEmulatorError)
            {
                MessageBoxLibrary.MameUnknownSystemErrorMessageBox();
                MessageBoxLibrary.WouldYouLikeToOpenTheLogMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
            }

            return Task.CompletedTask;
        }

        // Handle MAME Unable to load image
        if ((emulatorManager.EmulatorName.Contains("MAME", StringComparison.OrdinalIgnoreCase) ||
             emulatorManager.EmulatorLocation.Contains("mame", StringComparison.OrdinalIgnoreCase)) &&
            (output.ToString().Contains("Unable to load image", StringComparison.OrdinalIgnoreCase) ||
             output.ToString().Contains("No such file or directory", StringComparison.OrdinalIgnoreCase)))
        {
            DebugLogger.Log("[CheckForExitCodeWithErrorAnyAsync] MAME Unable to load image error.");
            _logErrors.LogAndForget(null, contextMessage);

            if (emulatorManager.ReceiveANotificationOnEmulatorError)
            {
                MessageBoxLibrary.MameUnableToLoadImageMessageBox();
                MessageBoxLibrary.WouldYouLikeToOpenTheLogMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
            }

            return Task.CompletedTask;
        }

        // Handle MAME corrupted INI (unknown option warnings)
        if ((emulatorManager.EmulatorName.Contains("MAME", StringComparison.OrdinalIgnoreCase) ||
             emulatorManager.EmulatorLocation.Contains("mame", StringComparison.OrdinalIgnoreCase) ||
             emulatorManager.EmulatorLocation.Contains("mame64", StringComparison.OrdinalIgnoreCase)) &&
            error.ToString().Contains("Warning: unknown option in INI", StringComparison.OrdinalIgnoreCase))
        {
            DebugLogger.Log("[CheckForExitCodeWithErrorAnyAsync] MAME unknown option in INI detected. Restoring mame.ini from sample.");
            var restored = MameConfigurationService.RestoreMameIniFromSample(psi.FileName, _logErrors);
            if (restored)
            {
                DebugLogger.Log("[CheckForExitCodeWithErrorAnyAsync] mame.ini restored successfully. User should retry.");
            }
            else
            {
                DebugLogger.Log("[CheckForExitCodeWithErrorAnyAsync] Failed to restore mame.ini from sample.");
            }

            return Task.CompletedTask;
        }

        DebugLogger.Log($"[CheckForExitCodeWithErrorAnyAsync] Exit code {process.ExitCode} detected.");
        _logErrors.LogAndForget(null, contextMessage);

        // Generic error handler
        if (emulatorManager.ReceiveANotificationOnEmulatorError)
        {
            return MessageBoxLibrary.CouldNotLaunchGameMessageBox(PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log"));
            // SupportFromTheDeveloper.DoYouWantToReceiveSupportFromTheDeveloper(_configuration, _httpClientFactory, _logErrors, null, contextMessage, _playSoundEffects);
        }

        return Task.CompletedTask;
    }

    private Task CheckForMemoryAccessViolationAsync(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error)
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
        _logErrors.LogAndForget(null, contextMessage);

        return Task.CompletedTask;
    }

    // ReSharper disable once UnusedParameter.Local
    private Task CheckForDepViolationAsync(Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error, Emulator emulatorManager)
    {
        if (process.HasExited && process.ExitCode != DepViolation)
        {
            return Task.CompletedTask;
        }

        // Notify developer
        var contextMessage = $"Data Execution Prevention (DEP) violation error occurred while running the emulator.\n" +
                             $"User was not notified.\n" +
                             $"Exit code: {process.ExitCode}\n" +
                             $"Emulator: {psi.FileName}\n" +
                             $"Calling parameters: {psi.Arguments}\n" +
                             $"Emulator output: {output}\n" +
                             $"Emulator error: {error}\n";
        _logErrors.LogAndForget(null, contextMessage);

        // if (emulatorManager.ReceiveANotificationOnEmulatorError)
        // {
        //     // Notify user
        //     return MessageBoxLibrary.CouldNotLaunchGameDueToDepViolationMessageBox();
        //     // SupportFromTheDeveloper.DoYouWantToReceiveSupportFromTheDeveloper(_configuration, _httpClientFactory, _logErrors, null, contextMessage, _playSoundEffects);
        // }

        return Task.CompletedTask;
    }

    private bool DoNotCheckErrorsOnSpecificEmulators(string selectedEmulatorName, string resolvedEmulatorExePath, Process process, ProcessStartInfo psi, StringBuilder output, StringBuilder error)
    {
        var emulatorsToSkipErrorChecking = _configuration.GetValue<string[]>("EmulatorsToSkipErrorChecking") ??
        [
            "Kega Fusion", "KegaFusion", "Kega", "Fusion", "Fusion.exe", "Project64", "Project 64", "Project64.exe", "Emulicious", "Emulicious.exe", "Speccy", "Speccy.exe", "ProSystem.exe", "ProSystem"
        ];

        foreach (var emulatorToSkip in emulatorsToSkipErrorChecking)
        {
            if (selectedEmulatorName.Contains(emulatorToSkip, StringComparison.OrdinalIgnoreCase) ||
                resolvedEmulatorExePath.Contains(emulatorToSkip, StringComparison.OrdinalIgnoreCase))
            {
                var contextMessage = $"User just ran {selectedEmulatorName}.\n" +
                                     $"'Simple Launcher' do not track error codes for this emulator.\n\n" +
                                     $"Exit code: {process.ExitCode}\n" +
                                     $"Emulator: {psi.FileName}\n" +
                                     $"Calling parameters: {psi.Arguments}\n" +
                                     $"Emulator output: {output}\n" +
                                     $"Emulator error: {error}\n";
                _logErrors.LogAndForget(null, contextMessage);

                return true;
            }
        }

        return false;
    }

    private bool IsProtocolRegistered(string protocol)
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
            _logErrors.LogAndForget(ex, $"Error checking if protocol '{protocol}' is registered.");
            DebugLogger.Log($"[IsProtocolRegistered] Error checking protocol '{protocol}': {ex.Message}");
            return false;
        }
    }

    [GeneratedRegex(@"URL=(.+)", RegexOptions.IgnoreCase, "pt-BR")]
    private static partial Regex MyRegex();
}