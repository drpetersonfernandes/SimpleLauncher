using System.Diagnostics;
using System.IO;

namespace Updater.Services;

/// <summary>
/// Service for managing process operations like waiting for process exit and restarting applications.
/// </summary>
public class ProcessService
{
    private const int ProcessExitTimeoutMs = 30000; // 30 seconds timeout for main app to exit
    private const int ProcessExitPollIntervalMs = 500; // Poll every 500ms to check if process exited
    private const int FallbackWaitDelayMs = 5000; // 5 seconds fallback when no PID provided

    /// <summary>
    /// Event raised when a log message needs to be displayed.
    /// </summary>
    public event Action<string>? LogMessage;

    /// <summary>
    /// Waits for the main application process to exit.
    /// </summary>
    /// <param name="processId">The process ID of the main application, or null if not available.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="TimeoutException">Thrown when the process does not exit within the timeout period.</exception>
    public async Task WaitForProcessExitAsync(int? processId)
    {
        if (processId.HasValue)
        {
            try
            {
                using var mainAppProcess = Process.GetProcessById(processId.Value);
                LogMessage?.Invoke($"Waiting for Simple Launcher (PID: {processId}) to exit...");

                var stopwatch = Stopwatch.StartNew();
                while (!mainAppProcess.HasExited && stopwatch.ElapsedMilliseconds < ProcessExitTimeoutMs)
                {
                    await Task.Delay(ProcessExitPollIntervalMs);
                    mainAppProcess.Refresh();
                }

                if (!mainAppProcess.HasExited)
                {
                    throw new TimeoutException(
                        $"Simple Launcher (PID: {processId}) did not exit within {ProcessExitTimeoutMs / 1000} seconds. " +
                        "The process may be unresponsive or still shutting down.");
                }

                // Add a small delay to ensure file handles are released
                await Task.Delay(500);
                LogMessage?.Invoke("Simple Launcher has exited.");
            }
            catch (ArgumentException)
            {
                LogMessage?.Invoke("Simple Launcher process not found. Assuming it has already exited.");
            }
        }
        else
        {
            LogMessage?.Invoke("No PID provided by Simple Launcher. Waiting for 5 seconds (this is unreliable)...");
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
            Process.Start(startInfo)?.Dispose();
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
        })?.Dispose();
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
