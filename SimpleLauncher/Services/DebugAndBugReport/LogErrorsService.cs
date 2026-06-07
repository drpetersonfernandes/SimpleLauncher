using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Windows;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.CleanAndDeleteFiles;

namespace SimpleLauncher.Services.DebugAndBugReport;

public class LogErrorsService : ILogErrors
{
    private static readonly SemaphoreSlim LogFileLock = new(1, 1);
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public LogErrorsService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
    }

    public async Task LogErrorAsync(Exception ex, string contextMessage = null)
    {
        if (ex != null)
        {
            DebugLogger.LogException(ex, contextMessage);
        }
        else if (!string.IsNullOrWhiteSpace(contextMessage))
        {
            DebugLogger.Log(contextMessage);
        }

        var errorLogPath = CheckPaths.PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPathForAdmin") ?? "error.log");
        var userLogPath = CheckPaths.PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log");
        var errorMessage = BugReportFormatter.BuildReport(ex, contextMessage);

        await LogFileLock.WaitAsync();
        try
        {
            // Append the error message to the general log
            await File.AppendAllTextAsync(errorLogPath, errorMessage);

            // Append the error message to the user-specific log
            var userErrorMessage = errorMessage + "--------------------------------------------------------------------------------------------------------------\n\n\n";
            await File.AppendAllTextAsync(userLogPath, userErrorMessage);

            // Attempt to send the error log content to the API only if enabled
            if (await SendLogToApiAsync(ex, errorMessage))
            {
                // If the log was successfully sent, delete the general log file to clean up.
                if (File.Exists(errorLogPath))
                {
                    try
                    {
                        DeleteFiles.TryDeleteFile(errorLogPath);
                    }
                    catch (Exception ex2)
                    {
                        WriteLocalErrorLog(ex2, "Error deleting the ErrorLog.");
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            DebugLogger.LogException(ex2, "Error deleting the ErrorLog");
                        });
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
        // Check the API key
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

            // Use a CancellationToken with a 30-second timeout to prevent indefinite hangs
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var apiUrl = _configuration.GetValue<string>("BugReportApiUrl") ?? "https://www.purelogiccode.com/bugreport/api/send-bug-report";
            using var response = await httpClient.PostAsync(apiUrl, jsonContent, cts.Token);

            DebugLogger.Log($"The ErrorLog was successfully sent. API response: {response.StatusCode}");

            return response.IsSuccessStatusCode;
        }
        catch (OperationCanceledException)
        {
            // Request timed out - log locally and don't crash
            WriteLocalErrorLog(new TimeoutException("The request to the bug report API timed out after 30 seconds."), "Error sending the ErrorLog to the API: timeout.");
            DebugLogger.Log("The ErrorLog request timed out after 30 seconds.");

            return false;
        }
        catch (Exception sendEx)
        {
            WriteLocalErrorLog(sendEx, "Error sending the ErrorLog to the API.");
            DebugLogger.LogException(sendEx, "There was an error sending the ErrorLog");

            // If sending fails, don't disable logging, just return false
            return false;
        }
    }

    /// <summary>
    /// Builds a comprehensive stack trace including all inner exceptions.
    /// </summary>
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

    /// <summary>
    /// Gets user information (machine name as a basic identifier).
    /// </summary>
    private static string GetUserInfo()
    {
        try
        {
            return Environment.MachineName;
        }
        catch (Exception ex)
        {
            DebugLogger.LogException(ex, "Failed to get user info for bug report.");
            return null;
        }
    }

    /// <summary>
    /// Gets the environment name (Debug/Release).
    /// </summary>
    private static string GetEnvironmentName()
    {
#if DEBUG
        return "Debug";
#else
        return "Release";
#endif
    }

    /// <summary>
    /// Writes a critical error to a local log file when API logging is not available.
    /// </summary>
    private void WriteLocalErrorLog(Exception ex, string contextMessage)
    {
        var criticalLogPath = CheckPaths.PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPathCritical") ?? "critical_error.log");
        var errorMessage = BugReportFormatter.BuildReport(ex, contextMessage) +
                           "\n--------------------------------------------------------------------------------------------------------------\n\n\n";

        try
        {
            File.AppendAllText(criticalLogPath, errorMessage);
        }
        catch (Exception ex2)
        {
            DebugLogger.LogException(ex2, "There was an error writing the local ErrorLog");
        }
    }
}