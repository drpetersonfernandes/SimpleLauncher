using System.Windows;
using System.Windows.Threading;

namespace MAMEUtility;

/// <inheritdoc cref="System.Windows.Application" />
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : IDisposable
{
    private BugReportService? _bugReportService;
    private LogError? _logError;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Initialize bug report service using configuration
        var config = AppConfig.Instance;
        _bugReportService = new BugReportService(
            config.BugReportApiUrl,
            config.BugReportApiKey
        );

        // Initialize error logging
        _logError = new LogError(_bugReportService);
        LogError.Initialize(_bugReportService);

        // Set up global exception handling
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnUnhandledException(object? sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            LogAndReportException(exception);
        }
    }

    private void OnDispatcherUnhandledException(object? sender, DispatcherUnhandledExceptionEventArgs e)
    {
        LogAndReportException(e.Exception);

        // Mark as handled to prevent application from crashing
        e.Handled = true;
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        LogAndReportException(e.Exception);
        e.SetObserved();
    }

    private void LogAndReportException(Exception exception)
    {
        try
        {
            // Log exception to console for debugging purposes
            Console.WriteLine($"Error: {exception.Message}");

            // Use our LogError class to handle the exception
            if (_logError != null)
            {
                _ = _logError.LogExceptionAsync(exception);
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