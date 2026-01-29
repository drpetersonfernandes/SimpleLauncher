using System.Windows.Controls;

namespace Mame.DatCreator.Services;

public class WpfLogger
{
    private readonly TextBox _logTextBox;
    private readonly ScrollViewer? _scrollViewer;

    public WpfLogger(TextBox logTextBox, ScrollViewer? scrollViewer = null)
    {
        _logTextBox = logTextBox;
        _scrollViewer = scrollViewer;
    }

    public void Info(string message)
    {
        AppendLog($"[INFO] {message}");
    }

    public void Warning(string message)
    {
        AppendLog($"[WARN] {message}");
    }

    public void Error(string message, Exception? ex = null)
    {
        AppendLog($"[ERROR] {message}");
        if (ex != null)
        {
            AppendLog(ex.ToString());
        }
    }

    private void AppendLog(string message)
    {
        if (_logTextBox.Dispatcher.CheckAccess())
        {
            AppendLogInternal(message);
        }
        else
        {
            _logTextBox.Dispatcher.Invoke(() => AppendLogInternal(message));
        }
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