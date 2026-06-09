using Avalonia.Controls;

namespace SimpleLauncher.Avalonia.Views;

public partial class SettingsWindow : Window
{
    public SettingsWindow()
    {
        InitializeComponent();
        DataContext = App.ServiceProvider.GetService(typeof(SettingsViewModel));
    }
}
