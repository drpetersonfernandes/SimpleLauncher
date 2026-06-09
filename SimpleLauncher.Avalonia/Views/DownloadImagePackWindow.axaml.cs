using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class DownloadImagePackWindow : Window
{
    public DownloadImagePackWindow()
    {
        InitializeComponent();
    }

    public DownloadImagePackWindow(AvaloniaDownloadImagePackViewModel viewModel) : this()
    {
        DataContext = viewModel;
    }

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        if (DataContext is AvaloniaDownloadImagePackViewModel vm)
        {
            vm.Dispose();
        }

        base.OnClosing(e);
    }
}
