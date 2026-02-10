using System;

namespace SimpleLauncher;

public partial class UpdateLogWindow
{
    public UpdateLogWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
    }

    public void Log(string message)
    {
        Dispatcher.Invoke(() =>
        {
            LogTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
            LogTextBox.ScrollToEnd();
        });
    }
}