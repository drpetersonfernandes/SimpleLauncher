using System.Windows;
using Updater.Services;

namespace Updater;

/// <summary>
/// Application class for the Updater application that handles startup, shutdown, and global exception handling.
/// </summary>
public partial class App
{
    /// <summary>
    /// Called when the application starts up. Sets up exception handling and creates the main window.
    /// </summary>
    /// <param name="e">The startup event arguments containing command line arguments.</param>
    protected override void OnStartup(StartupEventArgs e)
    {
        // Setup global exception handlers
        SetupExceptionHandling();

        // Call the base implementation
        base.OnStartup(e);

        // Send application launch statistics
        ApplicationStats.SendLaunchStats();

        // Manually create the window and pass the arguments
        var mainWindow = new MainWindow(e.Args);

        // Ensure it shows up
        mainWindow.Show();
    }

    /// <summary>
    /// Called when the application exits. Disposes HttpClient instances to prevent resource leaks.
    /// </summary>
    /// <param name="e">The exit event arguments.</param>
    protected override void OnExit(ExitEventArgs e)
    {
        // Dispose HttpClient instances to prevent socket exhaustion
        Updater.MainWindow.DisposeHttpClient();
        BugReportService.Dispose();

        base.OnExit(e);
    }

    /// <summary>
    /// Sets up global exception handling for the application
    /// </summary>
    private void SetupExceptionHandling()
    {
        // Handle exceptions from the main UI thread
        Current.DispatcherUnhandledException += (_, args) =>
        {
            BugReportService.ReportBug(args.Exception, "Unhandled UI thread exception");
            args.Handled = true;
            MessageBox.Show(
                $"An unexpected error occurred: {args.Exception.Message}\n\n" +
                "This error has been reported. The application will now close.",
                "Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown(1);
        };

        // Handle exceptions from non-UI threads
        AppDomain.CurrentDomain.UnhandledException += static (_, args) =>
        {
            if (args.ExceptionObject is Exception exception)
            {
                BugReportService.ReportBug(exception, "Unhandled AppDomain exception");
            }
        };

        // Handle unobserved task exceptions (TaskScheduler)
        TaskScheduler.UnobservedTaskException += static (_, args) =>
        {
            BugReportService.ReportBug(args.Exception, "Unobserved task exception");
            args.SetObserved();
        };
    }
}