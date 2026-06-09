using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectBlastemConfigWindow : Window
{
    public InjectBlastemConfigWindow()
    {
        InitializeComponent();
    }

    public InjectBlastemConfigWindow(AvaloniaInjectBlastemConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
