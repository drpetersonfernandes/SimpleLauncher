using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Updater.Services;

/// <summary>
/// Service for reporting bugs to the bug report API
/// </summary>
public static class BugReportService
{
    private const string ApiUrl = "https://www.purelogiccode.com/bugreport/api/send-bug-report";
    private const string ApiKey = "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";
    private static readonly string LogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bugreport_failures.log");

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    private static int _isReporting;

    /// <summary>
    /// Disposes the HttpClient instance. Should be called when the application is shutting down.
    /// </summary>
    public static void Dispose()
    {
        HttpClient.Dispose();
    }

    /// <summary>
    /// Reports an exception to the bug report API
    /// </summary>
    /// <param name="exception">The exception to report</param>
    /// <param name="additionalInfo">Additional context information</param>
    public static async Task ReportBugAsync(Exception exception, string? additionalInfo = null)
    {
        // Prevent recursive bug reporting using thread-safe Interlocked.CompareExchange
        if (Interlocked.CompareExchange(ref _isReporting, 1, 0) == 1) return;

        BugReportRequest? bugReport = null;

        try
        {
            bugReport = await CreateBugReportRequestAsync(exception, additionalInfo);
            var json = JsonSerializer.Serialize(bugReport);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl);
            request.Headers.Add("X-API-KEY", ApiKey);
            request.Content = content;

            var response = await HttpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                // Log failure to local file as fallback - don't throw to avoid infinite loops
                var responseContent = await response.Content.ReadAsStringAsync();
                var errorMessage = $"Failed to report bug: {response.StatusCode} - {responseContent}";
                System.Diagnostics.Debug.WriteLine(errorMessage);
                await LogToLocalFileAsync(errorMessage, bugReport);
            }
        }
        catch (Exception ex)
        {
            // Log failure to local file as fallback - don't throw to avoid infinite loops
            var errorMessage = $"Failed to report bug: {ex.Message}";
            System.Diagnostics.Debug.WriteLine(errorMessage);
            await LogToLocalFileAsync(errorMessage, bugReport);
        }
        finally
        {
            Interlocked.Exchange(ref _isReporting, 0);
        }
    }

    /// <summary>
    /// Reports an exception synchronously (for use in catch blocks where async is not possible)
    /// </summary>
    /// <param name="exception">The exception to report</param>
    /// <param name="additionalInfo">Additional context information</param>
    public static void ReportBug(Exception exception, string? additionalInfo = null)
    {
        // Fire and forget - don't block the calling thread
        _ = Task.Run(async () => await ReportBugAsync(exception, additionalInfo));
    }

    /// <summary>
    /// Logs bug report failure to a local file as fallback when API is unavailable.
    /// This ensures errors are not lost in production when Debug output is not visible.
    /// </summary>
    /// <param name="errorMessage">The error message to log</param>
    /// <param name="bugReport">The bug report data, if available</param>
    private static async Task LogToLocalFileAsync(string errorMessage, BugReportRequest? bugReport)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            var logEntry = new StringBuilder();
            logEntry.AppendLine(CultureInfo.InvariantCulture, $"[{timestamp}] {errorMessage}");

            if (bugReport != null)
            {
                logEntry.AppendLine("Bug Report Details:");
                logEntry.AppendLine(CultureInfo.InvariantCulture, $"  Application: {bugReport.ApplicationName}");
                logEntry.AppendLine(CultureInfo.InvariantCulture, $"  Version: {bugReport.Version}");
                logEntry.AppendLine(CultureInfo.InvariantCulture, $"  Message: {bugReport.Message?.Replace(Environment.NewLine, " ")}");
                logEntry.AppendLine(CultureInfo.InvariantCulture, $"  StackTrace: {bugReport.StackTrace?.Replace(Environment.NewLine, " ")}");
            }

            logEntry.AppendLine(new string('-', 80));

            // Append to log file with file locking to prevent concurrent access issues
            await Task.Run(() =>
            {
                try
                {
                    File.AppendAllText(LogFilePath, logEntry.ToString());
                }
                catch (IOException)
                {
                    // If file is locked, try with a unique filename
                    var fallbackPath = Path.Combine(
                        AppDomain.CurrentDomain.BaseDirectory,
                        $"bugreport_failures_{DateTime.Now:yyyyMMddHHmmssfff}.log");
                    File.AppendAllText(fallbackPath, logEntry.ToString());
                }
            });
        }
        catch
        {
            // Last resort: silently fail - we can't do anything more
        }
    }

    /// <summary>
    /// Creates a BugReportRequest with all environment and exception details
    /// </summary>
    /// <param name="exception">The exception to include in the report</param>
    /// <param name="additionalInfo">Additional context information</param>
    /// <returns>A BugReportRequest with all details populated</returns>
    private static async Task<BugReportRequest> CreateBugReportRequestAsync(Exception exception, string? additionalInfo)
    {
        var assembly = System.Reflection.Assembly.GetExecutingAssembly();
        var assemblyName = assembly.GetName().Name ?? "Updater";
        var version = assembly.GetName().Version?.ToString() ?? "Unknown";

        var environmentInfo = EnvironmentInfo.Collect();

        var message = BuildMessage(environmentInfo, exception, additionalInfo);
        var stackTrace = BuildFullStackTrace(exception);

        return new BugReportRequest
        {
            Message = message,
            ApplicationName = assemblyName,
            Version = version,
            UserInfo = GetUserInfo(),
            Environment = GetEnvironmentName(),
            StackTrace = stackTrace
        };
    }

    /// <summary>
    /// Builds the comprehensive bug report message with all environment details
    /// </summary>
    /// <param name="env">The environment information</param>
    /// <param name="exception">The exception to include</param>
    /// <param name="additionalInfo">Additional context information</param>
    /// <returns>A formatted message string with all details</returns>
    private static string BuildMessage(EnvironmentInfo env, Exception exception, string? additionalInfo)
    {
        var sb = new StringBuilder();

        // Add additional info if provided
        if (!string.IsNullOrEmpty(additionalInfo))
        {
            sb.AppendLine("=== Additional Context ===");
            sb.AppendLine(additionalInfo);
            sb.AppendLine();
        }

        // Environment Details
        sb.AppendLine("=== Environment Details ===");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Date: {env.Date}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Application Name: {env.ApplicationName}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Application Version: {env.ApplicationVersion}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"OS Version: {env.OsVersion}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Architecture: {env.Architecture}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Bitness: {env.Bitness}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Windows Version: {env.WindowsVersion}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Processor Count: {env.ProcessorCount}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Base Directory: {env.BaseDirectory}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Temp Path: {env.TempPath}");
        sb.AppendLine();

        // Error Details
        sb.AppendLine("=== Error Details ===");
        sb.AppendLine(exception.Message);
        sb.AppendLine();

        // Exception Details
        sb.AppendLine("=== Exception Details ===");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Type: {exception.GetType().FullName}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Message: {exception.Message}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Source: {exception.Source}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"StackTrace: {exception.StackTrace}");

        // Inner exception if present
        if (exception.InnerException != null)
        {
            sb.AppendLine();
            sb.AppendLine("=== Inner Exception ===");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Type: {exception.InnerException.GetType().FullName}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Message: {exception.InnerException.Message}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Source: {exception.InnerException.Source}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"StackTrace: {exception.InnerException.StackTrace}");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Builds a comprehensive stack trace including all inner exceptions
    /// </summary>
    /// <param name="exception">The exception to build the stack trace from</param>
    /// <param name="maxDepth">Maximum depth of inner exceptions to include</param>
    /// <returns>A formatted stack trace string</returns>
    private static string BuildFullStackTrace(Exception exception, int maxDepth = 10)
    {
        var sb = new StringBuilder();
        var currentException = exception;
        var depth = 0;

        while (currentException != null && depth < maxDepth)
        {
            if (depth > 0)
            {
                sb.AppendLine();
                sb.AppendLine("--- INNER EXCEPTION ---");
            }

            sb.AppendLine(CultureInfo.InvariantCulture, $"Exception Type: {currentException.GetType().FullName}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Message: {currentException.Message}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"Source: {currentException.Source}");
            sb.AppendLine(CultureInfo.InvariantCulture, $"StackTrace: {currentException.StackTrace}");

            currentException = currentException.InnerException;
            depth++;
        }

        if (currentException != null)
        {
            sb.AppendLine();
            sb.AppendLine("--- ADDITIONAL INNER EXCEPTIONS TRUNCATED ---");
            sb.AppendLine(CultureInfo.InvariantCulture, $"({depth} levels shown, more inner exceptions exist)");
        }

        return sb.ToString();
    }

    /// <summary>
    /// Gets user information (machine name as a basic identifier)
    /// </summary>
    /// <returns>The machine name, or null if it cannot be retrieved</returns>
    private static string? GetUserInfo()
    {
        try
        {
            return Environment.MachineName;
        }
        catch (Exception ex)
        {
            // Log failure but don't throw to avoid infinite loops
            System.Diagnostics.Debug.WriteLine($"Failed to get user info: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets the environment name (Debug/Release)
    /// </summary>
    /// <returns>The environment name</returns>
    private static string GetEnvironmentName()
    {
#if DEBUG
        return "Debug";
#else
        return "Release";
#endif
    }
}

/// <summary>
/// Data transfer object for bug report requests
/// </summary>
public class BugReportRequest
{
    /// <summary>
    /// Gets or sets the detailed error message and context information.
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// Gets or sets the name of the application that generated the bug report.
    /// </summary>
    [JsonPropertyName("applicationName")]
    public string? ApplicationName { get; set; }

    /// <summary>
    /// Gets or sets the version of the application that generated the bug report.
    /// </summary>
    [JsonPropertyName("version")]
    public string? Version { get; set; }

    /// <summary>
    /// Gets or sets information about the user or machine that generated the bug report.
    /// </summary>
    [JsonPropertyName("userInfo")]
    public string? UserInfo { get; set; }

    /// <summary>
    /// Gets or sets the environment name (e.g., Debug or Release) where the bug occurred.
    /// </summary>
    [JsonPropertyName("environment")]
    public string? Environment { get; set; }

    /// <summary>
    /// Gets or sets the complete stack trace of the exception.
    /// </summary>
    [JsonPropertyName("stackTrace")]
    public string? StackTrace { get; set; }
}
