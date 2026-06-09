using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.CheckPaths;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.LoadingInterface;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.Core.Services.SystemManager;

namespace SimpleLauncher.Avalonia.Services;

public class GameLauncherService
{
    private readonly ILogErrors _logErrors;
    private readonly IMessageDialogService _messageDialog;
    private readonly IConfiguration _configuration;
    private readonly IDispatcherService _dispatcher;

    public GameLauncherService(
        ILogErrors logErrors,
        IMessageDialogService messageDialog,
        IConfiguration configuration,
        IDispatcherService dispatcher)
    {
        _logErrors = logErrors;
        _messageDialog = messageDialog;
        _configuration = configuration;
        _dispatcher = dispatcher;
    }

    public async Task<TimeSpan> LaunchGameAsync(
        string filePath,
        string selectedEmulatorName,
        ISystemManager selectedSystemManager,
        SettingsManager settings)
    {
        var startTime = DateTime.Now;

        try
        {
            // 1. Resolve the file path
            var resolvedFilePath = PathHelper.ResolveRelativeToAppDirectory(filePath);
            if (string.IsNullOrEmpty(resolvedFilePath))
            {
                await _messageDialog.ShowErrorAsync("The game file path could not be resolved.", "Launch Error");
                return TimeSpan.Zero;
            }

            // 2. Validate file exists
            if (!File.Exists(resolvedFilePath) && !Directory.Exists(resolvedFilePath))
            {
                // Try Unicode normalization
                var normalizedPath = PathHelper.TryFindFileWithNormalizedPath(resolvedFilePath);
                if (!string.IsNullOrEmpty(normalizedPath))
                {
                    resolvedFilePath = normalizedPath;
                }
                else
                {
                    await _messageDialog.ShowErrorAsync($"Game file not found:\n{resolvedFilePath}", "Launch Error");
                    return TimeSpan.Zero;
                }
            }

            // 3. Find the emulator configuration
            var emulator = selectedSystemManager.Emulators
                .FirstOrDefault(e => e.EmulatorName.Equals(selectedEmulatorName, StringComparison.OrdinalIgnoreCase));

            if (emulator == null)
            {
                await _messageDialog.ShowErrorAsync($"Emulator '{selectedEmulatorName}' not found in system configuration.", "Launch Error");
                return TimeSpan.Zero;
            }

            // 4. Resolve emulator executable path
            var resolvedEmulatorExePath = PathHelper.ResolveRelativeToAppDirectory(emulator.EmulatorLocation);
            if (string.IsNullOrEmpty(resolvedEmulatorExePath) || !File.Exists(resolvedEmulatorExePath))
            {
                await _messageDialog.ShowErrorAsync($"Emulator executable not found:\n{emulator.EmulatorLocation}", "Launch Error");
                return TimeSpan.Zero;
            }

            // 5. Determine emulator folder
            var resolvedEmulatorFolderPath = Path.GetDirectoryName(resolvedEmulatorExePath);
            if (string.IsNullOrEmpty(resolvedEmulatorFolderPath))
            {
                await _messageDialog.ShowErrorAsync("Could not determine emulator folder path.", "Launch Error");
                return TimeSpan.Zero;
            }

            // 6. Resolve parameters
            var romSystemFolder = PathHelper.FindContainingSystemFolder(
                selectedSystemManager.SystemFolders,
                selectedSystemManager.PrimarySystemFolder,
                resolvedFilePath);

            var isDirectory = Directory.Exists(resolvedFilePath);
            var romName = isDirectory
                ? Path.GetFileName(resolvedFilePath)
                : Path.GetFileNameWithoutExtension(resolvedFilePath);

            var resolvedParameters = PathHelper.ResolveParameterString(
                emulator.EmulatorParameters,
                selectedSystemManager.SystemFolders,
                resolvedEmulatorFolderPath,
                resolvedFilePath,
                romSystemFolder,
                romName);

            // 7. Build arguments
            var rawParams = emulator.EmulatorParameters;
            var containsRomPlaceholder = rawParams.Contains("%ROM%", StringComparison.OrdinalIgnoreCase);
            var containsNamePlaceholder = rawParams.Contains("%NAME%", StringComparison.OrdinalIgnoreCase);

            string arguments;
            if (containsRomPlaceholder || containsNamePlaceholder || PathHelper.ContainsGameSpecificPlaceholder(resolvedParameters))
            {
                arguments = resolvedParameters;
            }
            else
            {
                var trimmedParameters = resolvedParameters.TrimEnd();
                var space = (string.IsNullOrWhiteSpace(trimmedParameters) || trimmedParameters.EndsWith('=')) ? "" : " ";

                // Detect MAME/Raine for stripped path call
                var isMame = selectedEmulatorName.Equals("MAME", StringComparison.OrdinalIgnoreCase) ||
                             emulator.EmulatorLocation.Contains("mame.exe", StringComparison.OrdinalIgnoreCase) ||
                             emulator.EmulatorLocation.Contains("mame64.exe", StringComparison.OrdinalIgnoreCase);

                var isRaine = selectedEmulatorName.Contains("Raine", StringComparison.OrdinalIgnoreCase) ||
                              emulator.EmulatorLocation.Contains("raine", StringComparison.OrdinalIgnoreCase);

                var ext = Path.GetExtension(resolvedFilePath).ToLowerInvariant();
                var isNeoGeoCd = ext is ".cue" or ".iso" or ".bin";

                if ((isMame || isRaine) && !isNeoGeoCd)
                {
                    arguments = $"{trimmedParameters}{space}\"{romName}\"";
                }
                else
                {
                    arguments = $"{trimmedParameters}{space}\"{resolvedFilePath}\"";
                }
            }

            // 8. Start the process
            var psi = new ProcessStartInfo
            {
                FileName = resolvedEmulatorExePath,
                Arguments = arguments,
                WorkingDirectory = resolvedEmulatorFolderPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using var process = new Process();
            process.StartInfo = psi;

            var output = new StringBuilder();
            var error = new StringBuilder();

            process.OutputDataReceived += (_, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                    output.AppendLine(args.Data);
            };

            process.ErrorDataReceived += (_, args) =>
            {
                if (!string.IsNullOrEmpty(args.Data))
                    error.AppendLine(args.Data);
            };

            var processStarted = process.Start();
            if (!processStarted)
            {
                await _messageDialog.ShowErrorAsync("Failed to start the emulator process.", "Launch Error");
                return TimeSpan.Zero;
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            // 9. Check for errors
            if (process.ExitCode != 0 && process.ExitCode != -1)
            {
                var errorMsg = $"Emulator exited with code {process.ExitCode}.";
                if (error.Length > 0)
                    errorMsg += $"\n\nError output:\n{error}";

                _logErrors?.LogAndForget(null, errorMsg);
            }

            return DateTime.Now - startTime;
        }
        catch (Exception ex)
        {
            _logErrors?.LogAndForget(ex, $"Error launching game: {filePath}");
            await _messageDialog.ShowErrorAsync($"Failed to launch game:\n{ex.Message}", "Launch Error");
            return DateTime.Now - startTime;
        }
    }
}
