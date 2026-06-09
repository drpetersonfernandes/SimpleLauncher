using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class SetLinksWindow : Window
{
    public SetLinksWindow()
    {
        InitializeComponent();
    }

    public SetLinksWindow(AvaloniaSetLinksViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.SaveCompleted += () => Close();
        viewModel.CloseRequested += () => Close();
    }
}
