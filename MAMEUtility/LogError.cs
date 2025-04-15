using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using MessageBox = System.Windows.MessageBox;

namespace MAMEUtility;

/// <summary>
/// Provides error logging functionality for the application.
/// Logs errors to a file and sends them to the bug reporting service.
/// </summary>
public class LogError
{
    private readonly BugReportService _bugReportService;
    private readonly string _logFilePath;
    private static LogError? _instance;

    /// <summary>
    /// Initializes a new instance of the LogError class.
    /// </summary>
    /// <param name="bugReportService">The bug report service to use for sending error reports.</param>
    public LogError(BugReportService bugReportService)
    {
        _bugReportService = bugReportService;
        _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ErrorLog.txt");
    }

    /// <summary>
    /// Initializes the static instance of the LogError class.
    /// </summary>
    /// <param name="bugReportService">The bug report service to use for sending error reports.</param>
    public static void Initialize(BugReportService bugReportService)
    {
        _instance = new LogError(bugReportService);
    }

    /// <summary>
    /// Logs an exception using the static instance.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="additionalInfo">Optional additional information about the error context.</param>
    public static async Task LogAsync(Exception exception, string additionalInfo = "")
    {
        if (_instance == null)
        {
            // Fallback if not initialized, just log to file
            try
            {
                var logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ErrorLog.txt");
                await File.AppendAllTextAsync(logFilePath,
                    $"Date/Time: {DateTime.Now}\n" +
                    $"Error: {exception.Message}\n" +
                    $"Stack Trace: {exception.StackTrace}\n" +
                    $"Additional Info: {additionalInfo}\n" +
                    new string('-', 80) + "\n");
            }
            catch
            {
                // Unable to log error - we tried our best
            }

            return;
        }

        await _instance.LogExceptionAsync(exception, additionalInfo);
    }

    /// <summary>
    /// Logs a message using the static instance.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="level">The log level (Info, Warning, Error).</param>
    public static async Task LogMessageAsync(string message, LogLevel level = LogLevel.Info)
    {
        if (_instance == null)
        {
            // Fallback if not initialized, just log to file
            try
            {
                var logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ErrorLog.txt");
                await File.AppendAllTextAsync(logFilePath,
                    $"Date/Time: {DateTime.Now}\n" +
                    $"Level: {level}\n" +
                    $"Message: {message}\n" +
                    new string('-', 80) + "\n");
            }
            catch
            {
                // Unable to log message - we tried our best
            }

            return;
        }

        await _instance.LogMessageToFileAsync(message, level);
    }

    /// <summary>
    /// Logs an exception to a file and sends it to the bug report service.
    /// </summary>
    /// <param name="exception">The exception to log.</param>
    /// <param name="additionalInfo">Optional additional information about the error context.</param>
    public async Task LogExceptionAsync(Exception exception, string additionalInfo = "")
    {
        try
        {
            // Format the error message
            var errorMessage = FormatExceptionMessage(exception, additionalInfo);

            // Log to file
            await LogToFileAsync(errorMessage);

            // Send it to API
            await _bugReportService.SendExceptionReportAsync(exception);

            // Ask user if they want to open the log file
            AskUserToOpenLogFile();
        }
        catch (Exception ex)
        {
            // If an error occurs during logging, at least try to write it to the file
            try
            {
                await File.AppendAllTextAsync(_logFilePath, $"Error in logging: {ex.Message}\n");
            }
            catch
            {
                // We tried our best, nothing more we can do here
            }
        }
    }

    /// <summary>
    /// Formats an exception message for logging.
    /// </summary>
    private static string FormatExceptionMessage(Exception exception, string additionalInfo)
    {
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"Date/Time: {DateTime.Now}");
        if (!string.IsNullOrEmpty(additionalInfo))
        {
            sb.AppendLine(CultureInfo.InvariantCulture, $"Additional Info: {additionalInfo}");
        }

        sb.AppendLine(CultureInfo.InvariantCulture, $"Exception: {exception.GetType().Name}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Message: {exception.Message}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Stack Trace: {exception.StackTrace}");

        // Add version info
        sb.AppendLine(CultureInfo.InvariantCulture, $"App Version: {AboutWindow.ApplicationVersion}");

        // Add OS info
        sb.AppendLine(CultureInfo.InvariantCulture, $"OS: {Environment.OSVersion}");
        sb.AppendLine(CultureInfo.InvariantCulture, $".NET Version: {Environment.Version}");

        // Add inner exception info if present
        if (exception.InnerException != null)
        {
            sb.AppendLine("\nInner Exception:");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Type: {exception.InnerException.GetType().Name}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Message: {exception.InnerException.Message}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Stack Trace: {exception.InnerException.StackTrace}");
        }

        sb.AppendLine(new string('-', 80)); // Separator line
        return sb.ToString();
    }

    /// <summary>
    /// Logs a message to the error log file.
    /// </summary>
    private Task LogToFileAsync(string errorMessage)
    {
        // Ensure directory exists
        var directory = Path.GetDirectoryName(_logFilePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Append the error to the log file
        return File.AppendAllTextAsync(_logFilePath, errorMessage);
    }

    /// <summary>
    /// Logs a message to the log file.
    /// </summary>
    /// <param name="message">The message to log.</param>
    /// <param name="level">The log level.</param>
    private async Task LogMessageToFileAsync(string message, LogLevel level)
    {
        try
        {
            var formattedMessage = FormatLogMessage(message, level);
            await LogToFileAsync(formattedMessage);

            // Only show message box for warnings and errors
            if (level is LogLevel.Warning or LogLevel.Error)
            {
                var messageType = level == LogLevel.Warning ? MessageBoxImage.Warning : MessageBoxImage.Error;
                AskUserToOpenLogFile(messageType);
            }
        }
        catch (Exception ex)
        {
            // If an error occurs during logging, at least try to write it to the file
            try
            {
                await File.AppendAllTextAsync(_logFilePath, $"Error in logging: {ex.Message}\n");
            }
            catch
            {
                // We tried our best, nothing more we can do here
            }
        }
    }

    /// <summary>
    /// Formats a log message.
    /// </summary>
    private string FormatLogMessage(string message, LogLevel level)
    {
        var sb = new StringBuilder();
        sb.AppendLine(CultureInfo.InvariantCulture, $"Date/Time: {DateTime.Now}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Level: {level}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Message: {message}");

        // Add version info
        sb.AppendLine(CultureInfo.InvariantCulture, $"App Version: {AboutWindow.ApplicationVersion}");

        sb.AppendLine(new string('-', 80)); // Separator line
        return sb.ToString();
    }

    /// <summary>
    /// Asks the user if they want to open the error log file.
    /// </summary>
    private void AskUserToOpenLogFile(MessageBoxImage messageBoxImage = MessageBoxImage.Error)
    {
        var title = messageBoxImage == MessageBoxImage.Warning ? "Warning Occurred" : "Error Occurred";
        var message = $"A {(messageBoxImage == MessageBoxImage.Warning ? "warning" : "error")} has occurred and has been logged. Would you like to open the log file?";

        var result = MessageBox.Show(
            message,
            title,
            MessageBoxButton.YesNo,
            messageBoxImage);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            // Open the log file with the default application
            Process.Start(new ProcessStartInfo
            {
                FileName = _logFilePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open log file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}