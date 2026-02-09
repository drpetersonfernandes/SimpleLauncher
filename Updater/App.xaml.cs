using System.Windows;

namespace Updater;

public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // Call the base implementation
        base.OnStartup(e);

        // Manually create the window and pass the arguments
        var mainWindow = new MainWindow(e.Args);

        // Ensure it shows up
        mainWindow.Show();
    }
}