using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectYumirConfigWindow : Window
{
    public InjectYumirConfigWindow()
    {
        InitializeComponent();
    }

    public InjectYumirConfigWindow(AvaloniaInjectYumirConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
