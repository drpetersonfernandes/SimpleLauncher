namespace SimpleLauncher;

public partial class PleaseWaitWindow
{
    // Modified constructor to accept a message
    public PleaseWaitWindow(string message)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        // Set the text of the TextBlock
        MessageTextBlock.Text = message;
    }
}