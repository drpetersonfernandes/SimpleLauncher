using Avalonia.Controls;
using Avalonia.Input;

namespace SimpleLauncher.Avalonia.Views;

public partial class PlayHistoryView : UserControl
{
    public PlayHistoryView()
    {
        InitializeComponent();
    }

    private void OnHistoryDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not PlayHistoryViewModel viewModel) return;
        if (sender is not ListBox listBox) return;
        if (listBox.SelectedItem is not Core.Models.PlayHistoryItem) return;

        _ = viewModel.LaunchGameCommand.ExecuteAsync(null);
    }
}
