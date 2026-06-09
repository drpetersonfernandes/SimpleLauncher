using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectStellaConfigWindow : Window
{
    public InjectStellaConfigWindow()
    {
        InitializeComponent();
    }

    public InjectStellaConfigWindow(AvaloniaInjectStellaConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
