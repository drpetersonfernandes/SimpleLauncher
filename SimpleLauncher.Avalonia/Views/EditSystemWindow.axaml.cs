using Avalonia.Controls;

namespace SimpleLauncher.Avalonia.Views;

public partial class EditSystemWindow : Window
{
    public EditSystemWindow()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetService(typeof(EditSystemViewModel));
    }

    public EditSystemWindow(string systemName) : this()
    {
        if (DataContext is EditSystemViewModel vm)
        {
            _ = vm.LoadSystemAsync(systemName);
        }
    }
}
