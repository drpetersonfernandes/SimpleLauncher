using System.Diagnostics;
using System.IO;

namespace Updater.Services;

/// <summary>
/// Service for managing process operations like waiting for process exit and restarting applications.
/// </summary>
public class ProcessService
{
    private const int ProcessExitTimeoutMs = 10000; // 10 seconds timeout for main app to exit
    private const int FallbackWaitDelayMs = 3000; // 3 seconds fallback when no PID provided

    /// <summary>
    /// Event raised when a log message needs to be displayed.
    /// </summary>
    public event Action<string>? LogMessage;

    /// <summary>
    /// Waits for the main application process to exit.
    /// </summary>
    /// <param name="processId">The process ID of the main application, or null if not available.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task WaitForProcessExitAsync(int? processId)
    {
        if (processId.HasValue)
        {
            try
            {
                var mainAppProcess = Process.GetProcessById(processId.Value);
                LogMessage?.Invoke($"Waiting for Simple Launcher (PID: {processId}) to exit...");

                // Use Task.Run to prevent UI freeze during the synchronous WaitForExit
                await Task.Run(() => mainAppProcess.WaitForExit(ProcessExitTimeoutMs));

                if (!mainAppProcess.HasExited)
                {
                    LogMessage?.Invoke("Warning: Simple Launcher did not exit in time. Update may fail.");
                }
                else
                {
                    LogMessage?.Invoke("Simple Launcher has exited.");
                }
            }
            catch (ArgumentException)
            {
                LogMessage?.Invoke("Simple Launcher process not found. Assuming it has already exited.");
            }
        }
        else
        {
            LogMessage?.Invoke("No PID provided by Simple Launcher. Waiting for 3 seconds (this is unreliable)...");
            await Task.Delay(FallbackWaitDelayMs);
        }
    }

    /// <summary>
    /// Restarts the main application after an update.
    /// </summary>
    /// <param name="appDirectory">The directory containing the application executable.</param>
    /// <param name="executableName">The name of the executable to start (without .exe extension).</param>
    /// <param name="arguments">Command line arguments to pass to the executable.</param>
    /// <returns>True if the process was started successfully, false otherwise.</returns>
    public bool RestartApplication(string appDirectory, string executableName, string arguments)
    {
        try
        {
            var exePath = Path.Combine(appDirectory, $"{executableName}.exe");

            // Check if the executable exists before attempting to start it
            if (!File.Exists(exePath))
            {
                LogMessage?.Invoke($"{executableName}.exe not found. Cannot restart automatically.");
                return false;
            }

            var startInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = arguments,
                UseShellExecute = true,
                WorkingDirectory = appDirectory
            };
            Process.Start(startInfo);
            return true;
        }
        catch (Exception ex)
        {
            // Fire-and-forget async bug report with exception logging
            _ = ReportBugFireAndForgetAsync(ex, $"Failed to restart application: {executableName}");

            LogMessage?.Invoke($"Failed to restart the main application: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Opens a URL in the default web browser.
    /// </summary>
    /// <param name="url">The URL to open.</param>
    public void OpenUrl(string url)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = url,
            UseShellExecute = true
        });
    }

    /// <summary>
    /// Fire-and-forget helper for reporting bugs from synchronous contexts.
    /// Logs exceptions to Debug output if the bug report itself fails.
    /// </summary>
    private static async Task ReportBugFireAndForgetAsync(Exception exception, string context)
    {
        try
        {
            await BugReportService.ReportBugAsync(exception, context);
        }
        catch (Exception ex)
        {
            // If bug reporting fails, log to debug output - don't throw
            Debug.WriteLine($"Failed to report bug: {ex.Message}");
            Debug.WriteLine($"Original exception: {exception.Message}");
        }
    }
}
