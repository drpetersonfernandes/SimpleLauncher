using Avalonia.Controls;
using Avalonia.Input;

namespace SimpleLauncher.Avalonia.Views;

public partial class FavoritesView : UserControl
{
    public FavoritesView()
    {
        InitializeComponent();
    }

    private void OnFavoriteDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not FavoritesViewModel viewModel) return;
        if (sender is not ListBox listBox) return;
        if (listBox.SelectedItem is not Core.Models.Favorite) return;

        _ = viewModel.LaunchGameCommand.ExecuteAsync(null);
    }
}
