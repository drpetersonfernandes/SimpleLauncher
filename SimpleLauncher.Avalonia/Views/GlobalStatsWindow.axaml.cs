using Avalonia.Controls;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class GlobalStatsWindow : Window
{
    public GlobalStatsWindow()
    {
        InitializeComponent();
    }

    public GlobalStatsWindow(AvaloniaGlobalStatsViewModel viewModel) : this()
    {
        DataContext = viewModel;
        viewModel.CloseRequested += () => Close();
    }
}
