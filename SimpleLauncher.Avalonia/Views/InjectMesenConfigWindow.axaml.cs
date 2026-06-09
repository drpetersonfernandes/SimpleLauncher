using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectMesenConfigWindow : Window
{
    public InjectMesenConfigWindow()
    {
        InitializeComponent();
    }

    public InjectMesenConfigWindow(AvaloniaInjectMesenConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
