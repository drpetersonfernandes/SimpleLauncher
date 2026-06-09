using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectDaphneConfigWindow : Window
{
    public InjectDaphneConfigWindow()
    {
        InitializeComponent();
    }

    public InjectDaphneConfigWindow(AvaloniaInjectDaphneConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
