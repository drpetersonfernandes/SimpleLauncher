using Avalonia.Controls;
using Avalonia.Input;

namespace SimpleLauncher.Avalonia.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void OnGameGridDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not MainWindowViewModel viewModel) return;
        if (sender is not DataGrid dataGrid) return;
        if (dataGrid.SelectedItem is not GameItemViewModel game) return;

        _ = viewModel.LaunchGameCommand.ExecuteAsync(game);
    }
}
