using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectRpcs3ConfigWindow : Window
{
    public InjectRpcs3ConfigWindow()
    {
        InitializeComponent();
    }

    public InjectRpcs3ConfigWindow(AvaloniaInjectRpcs3ConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
