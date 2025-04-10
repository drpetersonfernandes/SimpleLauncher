using System.Windows;
using System.Windows.Threading;

namespace MAMEUtility;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : IDisposable
{
    private BugReportService? _bugReportService;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize bug report service using configuration
        var config = AppConfig.Instance;
        _bugReportService = new BugReportService(
            config.BugReportApiUrl,
            config.BugReportApiKey
        );

        // Set up global exception handling
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            ReportException(exception);
        }
    }

    private void OnDispatcherUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
    {
        ReportException(e.Exception);

        // Mark as handled to prevent application from crashing
        e.Handled = true;
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        ReportException(e.Exception);
        e.SetObserved();
    }

    private void ReportException(Exception exception)
    {
        try
        {
            // Log exception to console for debugging purposes
            Console.WriteLine($"Error: {exception.Message}");

            // Asynchronously send to bug report API
            if (_bugReportService != null)
            {
                _ = _bugReportService.SendExceptionReportAsync(exception);
            }
        }
        catch
        {
            // Ignore exceptions in the exception handler
        }
    }

    public void Dispose()
    {
        // Unregister from event handlers to prevent memory leaks
        AppDomain.CurrentDomain.UnhandledException -= OnUnhandledException;
        DispatcherUnhandledException -= OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException -= OnUnobservedTaskException;

        // Dispose the bug report service if it exists
        if (_bugReportService != null)
        {
            _bugReportService.Dispose();
            _bugReportService = null;
        }

        // Suppress finalization
        GC.SuppressFinalize(this);
    }
}