using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectMameConfigWindow : Window
{
    public InjectMameConfigWindow()
    {
        InitializeComponent();
    }

    public InjectMameConfigWindow(AvaloniaInjectMameConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
