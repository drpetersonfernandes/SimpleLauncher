using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectPcsx2ConfigWindow : Window
{
    public InjectPcsx2ConfigWindow()
    {
        InitializeComponent();
    }

    public InjectPcsx2ConfigWindow(AvaloniaInjectPcsx2ConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
