using System.Windows.Controls;

namespace Mame.DatCreator.Services;

/// <summary>
///     Provides logging functionality that writes to a WPF TextBox control.
/// </summary>
public class WpfLogger
{
    private readonly TextBox _logTextBox;
    private readonly ScrollViewer? _scrollViewer;

    /// <summary>
    ///     Initializes a new instance of the <see cref="WpfLogger" /> class.
    /// </summary>
    /// <param name="logTextBox">The TextBox to write log messages to.</param>
    /// <param name="scrollViewer">Optional ScrollViewer for auto-scrolling.</param>
    public WpfLogger(TextBox logTextBox, ScrollViewer? scrollViewer = null)
    {
        _logTextBox = logTextBox;
        _scrollViewer = scrollViewer;
    }

    /// <summary>
    ///     Logs an informational message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Info(string message)
    {
        Log.Information(message);
        AppendLog($"[INFO] {message}");
    }

    /// <summary>
    ///     Logs a warning message.
    /// </summary>
    /// <param name="message">The message to log.</param>
    public void Warning(string message)
    {
        Log.Warning(message);
        AppendLog($"[WARN] {message}");
    }

    /// <summary>
    ///     Logs an error message with an optional exception.
    /// </summary>
    /// <param name="message">The error message to log.</param>
    /// <param name="ex">The optional exception to log.</param>
    public void Error(string message, Exception? ex = null)
    {
        if (ex != null)
            Log.Error(ex, message);
        else
            Log.Error(message);
        AppendLog($"[ERROR] {message}");
        if (ex != null) AppendLog(ex.ToString());
    }

    private void AppendLog(string message)
    {
        if (_logTextBox.Dispatcher.CheckAccess())
            AppendLogInternal(message);
        else
            _logTextBox.Dispatcher.Invoke(() => AppendLogInternal(message));
    }

    private void AppendLogInternal(string message)
    {
        _logTextBox.AppendText($"{message}\n");

        // Auto-scroll using TextBox's built-in method
        _logTextBox.CaretIndex = _logTextBox.Text.Length;
        _logTextBox.ScrollToEnd();

        // Also scroll the parent ScrollViewer if available
        _scrollViewer?.ScrollToEnd();
    }
}