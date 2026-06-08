using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SimpleLauncher.Avalonia.Views;

public partial class MessageDialog : Window
{
    public string Message { get; set; } = string.Empty;
    public bool ShowCancel { get; set; }
    public bool Result { get; private set; }

    public MessageDialog()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Result = true;
        Close();
    }

    private void OnCancelClick(object? sender, RoutedEventArgs e)
    {
        Result = false;
        Close();
    }
}
