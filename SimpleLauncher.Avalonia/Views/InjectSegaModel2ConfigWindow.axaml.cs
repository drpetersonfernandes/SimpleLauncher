using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class InjectSegaModel2ConfigWindow : Window
{
    public InjectSegaModel2ConfigWindow()
    {
        InitializeComponent();
    }

    public InjectSegaModel2ConfigWindow(AvaloniaInjectSegaModel2ConfigViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += Close;
    }
}
