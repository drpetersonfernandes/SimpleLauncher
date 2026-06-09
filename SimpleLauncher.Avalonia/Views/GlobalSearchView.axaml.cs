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
}
