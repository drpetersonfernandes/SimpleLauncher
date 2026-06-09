using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectMednafenConfigWindow : Window
{
    public InjectMednafenConfigWindow()
    {
        InitializeComponent();
    }

    public InjectMednafenConfigWindow(AvaloniaInjectMednafenConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
