using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Core.Services.DebugAndBugReport;

public class BugReportFormatterService : IBugReportFormatter
{
    private readonly IWindowsVersionService _windowsVersionService;

    public BugReportFormatterService(IWindowsVersionService windowsVersionService)
    {
        _windowsVersionService = windowsVersionService;
    }

    public string BuildReport(Exception ex, string contextMessage = null)
    {
        var message = new StringBuilder();

        message.AppendLine("=== Environment Details ===");
        message.AppendLine(CultureInfo.InvariantCulture, $"Date: {DateTime.Now}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Application Name: {GetApplicationName()}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Application Version: {GetApplicationVersion()}");
        message.AppendLine(CultureInfo.InvariantCulture, $"OS Version: {RuntimeInformation.OSDescription}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Architecture: {RuntimeInformation.OSArchitecture}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Bitness: {(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Windows Version: {_windowsVersionService.GetVersion()}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Processor Count: {Environment.ProcessorCount}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Base Directory: {AppContext.BaseDirectory}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Temp Path: {Path.GetTempPath()}");
        message.AppendLine();

        message.AppendLine("=== Error Details ===");
        message.AppendLine(CultureInfo.InvariantCulture, $"Error message: {GetErrorMessage(ex, contextMessage)}");
        message.AppendLine();

        message.AppendLine("=== Exception Details ===");
        if (ex == null)
        {
            message.AppendLine("Type: None");
            message.AppendLine("Message: None");
            message.AppendLine("Source: None");
            message.AppendLine("StackTrace: None");
            return message.ToString();
        }

        AppendException(message, ex);
        if (ex.InnerException != null)
        {
            message.AppendLine();
            message.AppendLine("--- Inner Exception ---");
            AppendException(message, ex.InnerException);
        }

        return message.ToString();
    }

    private static void AppendException(StringBuilder message, Exception exception)
    {
        message.AppendLine(CultureInfo.InvariantCulture, $"Type: {exception.GetType().FullName}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Message: {exception.Message}");
        message.AppendLine(CultureInfo.InvariantCulture, $"Source: {exception.Source}");
        message.AppendLine(CultureInfo.InvariantCulture, $"StackTrace: {exception.StackTrace}");
    }

    private static string GetErrorMessage(Exception ex, string contextMessage)
    {
        if (!string.IsNullOrWhiteSpace(contextMessage))
            return contextMessage;

        if (ex != null)
            return ex.Message;

        return "Unknown error";
    }

    private static string GetApplicationName()
    {
        return Assembly.GetExecutingAssembly().GetName().Name ?? "SimpleLauncher";
    }

    private static string GetApplicationVersion()
    {
        return Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
    }
}
