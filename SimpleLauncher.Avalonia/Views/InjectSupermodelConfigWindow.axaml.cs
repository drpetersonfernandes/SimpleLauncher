using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectSupermodelConfigWindow : Window
{
    public InjectSupermodelConfigWindow()
    {
        InitializeComponent();
    }

    public InjectSupermodelConfigWindow(AvaloniaInjectSupermodelConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
