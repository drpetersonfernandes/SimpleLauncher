using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Windows;
using Microsoft.Extensions.Configuration;
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
            if (true) await File.AppendAllTextAsync(userLogPath, userErrorMessage);

            // Attempt to send the error log content to the API only if enabled
            if (await SendLogToApiAsync(errorMessage))
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

    private async Task<bool> SendLogToApiAsync(string logContent)
    {
        // Check the flag again, just in case
        if (string.IsNullOrEmpty(_configuration.GetValue<string>("ApiKey") ?? "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e"))
        {
            return false;
        }

        try
        {
            var httpClient = _httpClientFactory?.CreateClient("LogErrorsClient");
            if (httpClient != null)
            {
                httpClient.DefaultRequestHeaders.Add("X-API-KEY", _configuration.GetValue<string>("ApiKey") ?? "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e");

                var payload = new
                {
                    message = logContent,
                    applicationName = "SimpleLauncher"
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                // Use a CancellationToken with a 15-second timeout to prevent indefinite hangs
                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
                using var response = await httpClient.PostAsync(_configuration.GetValue<string>("BugReportApiUrl") ?? "https://www.purelogiccode.com/bugreport/api/send-bug-report/", jsonContent, cts.Token);

                DebugLogger.Log(@"The ErrorLog was successfully sent. API response: " + response.StatusCode);

                return response.IsSuccessStatusCode;
            }

            // If httpClient is null, return false
            return false;
        }
        catch (OperationCanceledException)
        {
            // Request timed out - log locally and don't crash
            WriteLocalErrorLog(new TimeoutException("The request to the bug report API timed out after 15 seconds."), "Error sending the ErrorLog to the API: timeout.");
            DebugLogger.Log("The ErrorLog request timed out after 15 seconds.");

            return false;
        }
        catch (Exception ex)
        {
            WriteLocalErrorLog(ex, "Error sending the ErrorLog to the API.");
            DebugLogger.LogException(ex, "There was an error sending the ErrorLog");

            // If sending fails, don't disable logging, just return false
            return false;
        }
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