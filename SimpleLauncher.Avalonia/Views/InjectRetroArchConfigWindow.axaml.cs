using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectRetroArchConfigWindow : Window
{
    public InjectRetroArchConfigWindow()
    {
        InitializeComponent();
    }

    public InjectRetroArchConfigWindow(AvaloniaInjectRetroArchConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
