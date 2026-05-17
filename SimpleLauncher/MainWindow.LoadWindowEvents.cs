using System.Windows;

namespace SimpleLauncher;

public partial class MainWindow
{
    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        _startupInitializationService.Initialize(this);
    }
}
