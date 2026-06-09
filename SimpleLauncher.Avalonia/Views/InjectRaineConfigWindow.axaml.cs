using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectRaineConfigWindow : Window
{
    public InjectRaineConfigWindow()
    {
        InitializeComponent();
    }

    public InjectRaineConfigWindow(AvaloniaInjectRaineConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
