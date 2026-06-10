using Avalonia.Controls;

namespace SimpleLauncher.Avalonia.Views;

public partial class EasyModeWindow : Window
{
    public EasyModeWindow()
    {
        InitializeComponent();

        // Load systems when window opens
        Loaded += async (_, _) =>
        {
            if (DataContext is EasyModeViewModel vm)
            {
                await vm.LoadSystemNamesAsync();
            }
        };
    }
}
