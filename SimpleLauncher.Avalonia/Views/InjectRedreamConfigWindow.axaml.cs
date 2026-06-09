using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectRedreamConfigWindow : Window
{
    public InjectRedreamConfigWindow()
    {
        InitializeComponent();
    }

    public InjectRedreamConfigWindow(AvaloniaInjectRedreamConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
