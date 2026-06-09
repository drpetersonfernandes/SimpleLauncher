using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class SupportWindow : Window
{
    public SupportWindow()
    {
        InitializeComponent();
    }

    public SupportWindow(AvaloniaSupportViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += () => Close();
        viewModel.FormCleared += () =>
        {
            // Form fields are cleared via binding in the ViewModel
        };
    }
}
