using System.Globalization;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.CheckPaths;

namespace SimpleLauncher.Services.DebugAndBugReport;

/// <summary>
/// Logs errors to local files and optionally sends them to a remote bug report API.
/// </summary>
public class LogErrorsService : ILogErrors
{
    private static readonly SemaphoreSlim LogFileLock = new(1, 1);
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly IDebugLogger _debugLogger;
    private readonly IDispatcherService _dispatcher;
    private readonly IDeleteFilesService _deleteFilesService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogErrorsService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients used to send error reports to the API.</param>
    /// <param name="configuration">Application configuration containing log paths and API settings.</param>
    /// <param name="debugLogger">Logger for writing debug and exception output.</param>
    /// <param name="dispatcher">Dispatcher service for invoking operations on the UI thread.</param>
    /// <param name="deleteFilesService">Service for deleting log files after successful API upload.</param>
    public LogErrorsService(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        IDebugLogger debugLogger,
        IDispatcherService dispatcher,
        IDeleteFilesService deleteFilesService)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _debugLogger = debugLogger;
        _dispatcher = dispatcher;
        _deleteFilesService = deleteFilesService;
    }

    /// <summary>
    /// Logs an error by writing to local log files and optionally sending the report to the remote bug report API.
    /// </summary>
    /// <param name="ex">The exception to log, or null if only a context message is provided.</param>
    /// <param name="contextMessage">An optional context message describing the error scenario.</param>
    public async Task LogErrorAsync(Exception ex, string contextMessage = null)
    {
        if (ex != null)
        {
            _debugLogger.LogException(ex, contextMessage);
        }
        else if (!string.IsNullOrWhiteSpace(contextMessage))
        {
            _debugLogger.Log(contextMessage);
        }

        var errorLogPath = PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPathForAdmin") ?? "error.log");
        var userLogPath = PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log");
        var errorMessage = BugReportFormatter.BuildReport(ex, contextMessage);

        await LogFileLock.WaitAsync();
        try
        {
            if (errorLogPath != null)
            {
                await File.AppendAllTextAsync(errorLogPath, errorMessage);

                var userErrorMessage = errorMessage + "--------------------------------------------------------------------------------------------------------------\n\n\n";
                if (userLogPath != null) await File.AppendAllTextAsync(userLogPath, userErrorMessage);

                if (await SendLogToApiAsync(ex, errorMessage))
                {
                    if (File.Exists(errorLogPath))
                    {
                        try
                        {
                            await _deleteFilesService.TryDeleteFileAsync(errorLogPath);
                        }
                        catch (Exception ex2)
                        {
                            WriteLocalErrorLog(ex2, "Error deleting the ErrorLog.");
                            await _dispatcher.InvokeAsync(() => _debugLogger.LogException(ex2, "Error deleting the ErrorLog"));
                        }
                    }
                }
            }
        }
        catch (Exception ex3)
        {
            WriteLocalErrorLog(ex3, "Error writing the ErrorLog.");
        }
        finally
        {
            LogFileLock.Release();
        }
    }

    private async Task<bool> SendLogToApiAsync(Exception ex, string logContent)
    {
        var apiKey = _configuration.GetValue<string>("ApiKey") ?? "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";
        if (string.IsNullOrEmpty(apiKey))
        {
            return false;
        }

        try
        {
            var httpClient = _httpClientFactory?.CreateClient("LogErrorsClient");
            if (httpClient == null)
            {
                return false;
            }

            httpClient.DefaultRequestHeaders.Add("X-API-KEY", apiKey);

            var assembly = Assembly.GetExecutingAssembly();
            var appName = assembly.GetName().Name ?? "SimpleLauncher";
            var version = assembly.GetName().Version?.ToString() ?? "Unknown";

            var payload = new
            {
                message = logContent,
                applicationName = appName,
                version,
                userInfo = GetUserInfo(),
                environment = GetEnvironmentName(),
                stackTrace = BuildStackTrace(ex)
            };

            var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var apiUrl = _configuration.GetValue<string>("BugReportApiUrl") ?? "https://www.purelogiccode.com/bugreport/api/send-bug-report";
            using var response = await httpClient.PostAsync(apiUrl, jsonContent, cts.Token);

            _debugLogger.Log($"The ErrorLog was successfully sent. API response: {response.StatusCode}");

            return response.IsSuccessStatusCode;
        }
        catch (OperationCanceledException)
        {
            WriteLocalErrorLog(new TimeoutException("The request to the bug report API timed out after 30 seconds."), "Error sending the ErrorLog to the API: timeout.");
            _debugLogger.Log("The ErrorLog request timed out after 30 seconds.");

            return false;
        }
        catch (Exception sendEx)
        {
            WriteLocalErrorLog(sendEx, "Error sending the ErrorLog to the API.");
            _debugLogger.LogException(sendEx, "There was an error sending the ErrorLog");

            return false;
        }
    }

    private static string BuildStackTrace(Exception exception)
    {
        if (exception == null)
        {
            return null;
        }

        var sb = new StringBuilder();
        var currentException = exception;
        var depth = 0;
        const int maxDepth = 10;

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
        }

        return sb.ToString();
    }

    private string GetUserInfo()
    {
        try
        {
            return Environment.MachineName;
        }
        catch (Exception ex)
        {
            _debugLogger.LogException(ex, "Failed to get user info for bug report.");
            return null;
        }
    }

    private static string GetEnvironmentName()
    {
#if DEBUG
        return "Debug";
#else
        return "Release";
#endif
    }

    private void WriteLocalErrorLog(Exception ex, string contextMessage)
    {
        var criticalLogPath = PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPathCritical") ?? "critical_error.log");
        var errorMessage = BugReportFormatter.BuildReport(ex, contextMessage) +
                           "\n--------------------------------------------------------------------------------------------------------------\n\n\n";

        try
        {
            if (criticalLogPath != null) File.AppendAllText(criticalLogPath, errorMessage);
        }
        catch (Exception ex2)
        {
            _debugLogger.LogException(ex2, "There was an error writing the local ErrorLog");
        }
    }
}
