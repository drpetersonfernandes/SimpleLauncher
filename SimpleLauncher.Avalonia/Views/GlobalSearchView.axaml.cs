using Avalonia.Controls;
using Avalonia.Input;

namespace SimpleLauncher.Avalonia.Views;

public partial class GlobalSearchView : UserControl
{
    public GlobalSearchView()
    {
        InitializeComponent();
    }

    private void OnSearchKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && DataContext is GlobalSearchViewModel vm)
        {
            vm.SearchCommand.Execute(null);
        }
    }

    private void OnSearchResultDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is not GlobalSearchViewModel viewModel) return;
        if (sender is not ListBox listBox) return;
        if (listBox.SelectedItem is not SearchResultItem) return;

        _ = viewModel.LaunchGameCommand.ExecuteAsync(null);
    }
}
