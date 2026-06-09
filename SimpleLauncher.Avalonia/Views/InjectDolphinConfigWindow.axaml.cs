using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectDolphinConfigWindow : Window
{
    public InjectDolphinConfigWindow()
    {
        InitializeComponent();
    }

    public InjectDolphinConfigWindow(AvaloniaInjectDolphinConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
