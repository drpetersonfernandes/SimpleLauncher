using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectXeniaConfigWindow : Window
{
    public InjectXeniaConfigWindow()
    {
        InitializeComponent();
    }

    public InjectXeniaConfigWindow(AvaloniaInjectXeniaConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
