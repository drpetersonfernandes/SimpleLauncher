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
            LogTextBox.AppendText($"{message}\n");
            LogTextBox.ScrollToEnd();
        });
    }
}