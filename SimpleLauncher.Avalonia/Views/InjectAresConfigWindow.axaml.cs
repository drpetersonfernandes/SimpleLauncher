using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectAresConfigWindow : Window
{
    public InjectAresConfigWindow()
    {
        InitializeComponent();
    }

    public InjectAresConfigWindow(AvaloniaInjectAresConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
