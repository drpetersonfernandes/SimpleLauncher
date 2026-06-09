using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectDuckStationConfigWindow : Window
{
    public InjectDuckStationConfigWindow()
    {
        InitializeComponent();
    }

    public InjectDuckStationConfigWindow(AvaloniaInjectDuckStationConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
