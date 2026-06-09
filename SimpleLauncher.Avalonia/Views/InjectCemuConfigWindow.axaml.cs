using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectCemuConfigWindow : Window
{
    public InjectCemuConfigWindow()
    {
        InitializeComponent();
    }

    public InjectCemuConfigWindow(AvaloniaInjectCemuConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
