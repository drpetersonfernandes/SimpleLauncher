namespace MAMEUtility;

public partial class LogWindow
{
    public LogWindow()
    {
        InitializeComponent();
    }

    public void AppendLog(string message)
    {
        Dispatcher.Invoke(() =>
        {
            LogTextBox.AppendText(message + "\n");
            LogTextBox.ScrollToEnd();
        });
    }
}