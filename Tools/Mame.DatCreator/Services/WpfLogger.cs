using System.Windows.Controls;
using System.Windows.Media;

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
        AppendLog($"[INFO] {message}", Brushes.LightGreen);
    }

    public void Warning(string message)
    {
        AppendLog($"[WARN] {message}", Brushes.Yellow);
    }

    public void Error(string message, Exception? ex = null)
    {
        AppendLog($"[ERROR] {message}", Brushes.Red);
        if (ex != null)
        {
            AppendLog(ex.ToString(), Brushes.Red);
        }
    }

    private void AppendLog(string message, Brush color)
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
        if (_scrollViewer != null)
        {
            _scrollViewer.ScrollToEnd();
        }
    }
}