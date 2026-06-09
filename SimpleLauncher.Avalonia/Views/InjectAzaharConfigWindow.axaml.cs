using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectAzaharConfigWindow : Window
{
    public InjectAzaharConfigWindow()
    {
        InitializeComponent();
    }

    public InjectAzaharConfigWindow(AvaloniaInjectAzaharConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
