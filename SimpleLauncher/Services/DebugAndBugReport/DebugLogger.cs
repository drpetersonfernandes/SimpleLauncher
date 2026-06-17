using System.Diagnostics;
using System.Globalization;

namespace SimpleLauncher.Services.DebugAndBugReport;

using Interfaces;

/// <summary>
/// Provides debug logging functionality with optional output to a debug window.
/// </summary>
public class DebugLogger : IDebugLogger
{
    private readonly bool _isDebugMode;
    private readonly object _initLock = new();
    private readonly object _bufferLock = new();
    private readonly List<string> _messageBuffer = [];
    private DebugWindow _logWindowInstance;
    private bool _windowInitialized;

    /// <summary>
    /// Initializes a new instance of the <see cref="DebugLogger"/> class.
    /// </summary>
    /// <param name="isDebugModeEnabled">If true, enables debug window output for log messages.</param>
    public DebugLogger(bool isDebugModeEnabled)
    {
        _isDebugMode = isDebugModeEnabled;
    }

    /// <summary>
    /// Logs a debug message to the debug output and optionally to the debug window.
    /// Messages are always buffered so they can be displayed when the debug window is opened later.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Log(string message)
    {
        Debug.WriteLine($"[DEBUG] {DateTime.Now:HH:mm:ss.fff} - {message}");

        var timestampedMessage = $"{DateTime.Now:HH:mm:ss} - {message}";

        lock (_bufferLock)
        {
            _messageBuffer.Add(timestampedMessage);
        }

        if (_windowInitialized)
        {
            _logWindowInstance?.AppendLogMessage(message);
            return;
        }

        if (!_isDebugMode) return;

        EnsureWindowInitialized();
        _logWindowInstance?.AppendLogMessage(message);
    }

    private void EnsureWindowInitialized()
    {
        lock (_initLock)
        {
            if (_windowInitialized) return;
        }

        lock (_initLock)
        {
            if (_windowInitialized) return;

            _windowInitialized = true;
            var dispatcher = System.Windows.Application.Current?.Dispatcher;
            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.Invoke(() =>
                {
                    DebugWindow.Initialize();
                    _logWindowInstance = DebugWindow.Instance;
                });
            }
            else
            {
                DebugWindow.Initialize();
                _logWindowInstance = DebugWindow.Instance;
            }
        }
    }

    /// <summary>
    /// Opens the debug window, connecting the logger to it and loading any buffered messages.
    /// Can be called from any thread.
    /// </summary>
    public void OpenDebugWindow()
    {
        var dispatcher = System.Windows.Application.Current?.Dispatcher;
        if (dispatcher != null && !dispatcher.CheckAccess())
        {
            dispatcher.Invoke(OpenDebugWindow);
            return;
        }

        DebugWindow.Initialize();

        lock (_initLock)
        {
            if (!_windowInitialized)
            {
                _windowInitialized = true;
                _logWindowInstance = DebugWindow.Instance;

                if (_logWindowInstance != null)
                {
                    List<string> bufferedCopy;
                    lock (_bufferLock)
                    {
                        bufferedCopy = [.. _messageBuffer];
                    }

                    _logWindowInstance.LoadBufferedMessages(bufferedCopy);
                }
            }
        }

        if (DebugWindow.Instance == null) return;

        DebugWindow.Instance.Show();
        DebugWindow.Instance.WindowState = System.Windows.WindowState.Normal;
        DebugWindow.Instance.Activate();
    }

    /// <summary>
    /// Logs exception details including type, message, source, stack trace, and any inner exception.
    /// </summary>
    /// <param name="ex">The exception to log.</param>
    /// <param name="contextMessage">An optional context message describing the error scenario.</param>
    public void LogException(Exception ex, string contextMessage = null)
    {
        if (ex == null)
        {
            ex = new ArgumentNullException(nameof(ex), @"Exception is null.");
        }

        var message = new System.Text.StringBuilder();
        if (!string.IsNullOrEmpty(contextMessage))
        {
            message.AppendLine(CultureInfo.InvariantCulture, $"Context: {contextMessage}");
        }

        message.AppendLine(CultureInfo.InvariantCulture, $"Exception Type: {ex.GetType().FullName}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Message: {ex.Message}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Source: {ex.Source}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Stack Trace: {ex.StackTrace}");

        if (ex.InnerException != null)
        {
            message.AppendLine("--- Inner Exception ---");
            message.AppendLine(CultureInfo.InvariantCulture, $"Exception Type: {ex.InnerException.GetType().FullName}");
            message.AppendLine(CultureInfo.InvariantCulture, $"Message: {ex.InnerException.Message}");
            message.AppendLine(CultureInfo.InvariantCulture, $"Source: {ex.InnerException.Source}");
            message.AppendLine(CultureInfo.InvariantCulture, $"Stack Trace: {ex.InnerException.StackTrace}");
            message.AppendLine("-----------------------");
        }

        Log($"EXCEPTION: {message}");
    }
}
