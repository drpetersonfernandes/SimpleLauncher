using System.Text;
using System.Windows.Threading;

namespace BatchConvertToCHD;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    // Bug Report API configuration
    private const string BugReportApiUrl = "https://www.purelogiccode.com/bugreport/api/send-bug-report";
    private const string BugReportApiKey = "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";
    private const string ApplicationName = "BatchConvertToCHD";

    private readonly BugReportService? _bugReportService;

    public App()
    {
        // Initialize the bug report service
        _bugReportService = new BugReportService(BugReportApiUrl, BugReportApiKey, ApplicationName);

        // Set up global exception handling
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            ReportException(exception, "AppDomain.UnhandledException");
        }
    }

    private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        ReportException(e.Exception, "Application.DispatcherUnhandledException");
        e.Handled = true;
    }

    private void TaskScheduler_UnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        ReportException(e.Exception, "TaskScheduler.UnobservedTaskException");
        e.SetObserved();
    }

    private async void ReportException(Exception exception, string source)
    {
        try
        {
            var message = BuildExceptionReport(exception, source);

            // Silently report the exception to our API
            if (_bugReportService != null)
            {
                await _bugReportService.SendBugReportAsync(message);
            }
        }
        catch
        {
            // Silently ignore any errors in the reporting process
        }
    }

    private string BuildExceptionReport(Exception exception, string source)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Error Source: {source}");
        sb.AppendLine($"Date and Time: {DateTime.Now}");
        sb.AppendLine($"OS Version: {Environment.OSVersion}");
        sb.AppendLine($".NET Version: {Environment.Version}");
        sb.AppendLine();

        // Add exception details
        sb.AppendLine("Exception Details:");
        AppendExceptionDetails(sb, exception);

        return sb.ToString();
    }

    private void AppendExceptionDetails(StringBuilder sb, Exception exception, int level = 0)
    {
        var indent = new string(' ', level * 2);

        sb.AppendLine($"{indent}Type: {exception.GetType().FullName}");
        sb.AppendLine($"{indent}Message: {exception.Message}");
        sb.AppendLine($"{indent}Source: {exception.Source}");
        sb.AppendLine($"{indent}StackTrace:");
        sb.AppendLine($"{indent}{exception.StackTrace}");

        // If there's an inner exception, include it too
        if (exception.InnerException != null)
        {
            sb.AppendLine($"{indent}Inner Exception:");
            AppendExceptionDetails(sb, exception.InnerException, level + 1);
        }
    }
}