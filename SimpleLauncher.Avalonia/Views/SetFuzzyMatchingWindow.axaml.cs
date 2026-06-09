using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SimpleLauncher.Avalonia.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class SetFuzzyMatchingWindow : Window
{
    public SetFuzzyMatchingWindow(AvaloniaSetFuzzyMatchingViewModel viewModel)
    {
        InitializeComponent();

        viewModel.SaveCompleted += () => Close(true);
        viewModel.CancelRequested += () => Close(false);

        DataContext = viewModel;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
