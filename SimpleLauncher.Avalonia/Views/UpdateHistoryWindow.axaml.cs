using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using SimpleLauncher.Core.ViewModels;

namespace SimpleLauncher.Avalonia.Views;

public partial class UpdateHistoryWindow : Window
{
    public UpdateHistoryWindow(UpdateHistoryViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
