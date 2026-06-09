using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectFlycastConfigWindow : Window
{
    public InjectFlycastConfigWindow()
    {
        InitializeComponent();
    }

    public InjectFlycastConfigWindow(AvaloniaInjectFlycastConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
