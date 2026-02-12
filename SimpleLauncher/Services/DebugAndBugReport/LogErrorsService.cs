using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
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
        if (ex == null)
        {
            ex = new ArgumentNullException(nameof(ex), @"Exception parameter is null.");
        }

        DebugLogger.LogException(ex, contextMessage);

        var errorLogPath = CheckPaths.PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPathForAdmin") ?? "error.log");
        var userLogPath = CheckPaths.PathHelper.ResolveRelativeToAppDirectory(_configuration.GetValue<string>("LogPath") ?? "error_user.log");
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";

        // Gather additional environment info
        var osVersion = RuntimeInformation.OSDescription;
        var architecture = RuntimeInformation.OSArchitecture.ToString();
        var is64Bit = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
        var windowsVersion = GetMicrosoftWindowsVersion.GetVersion();

        // Write error Message
        var errorMessage =
            $"Date: {DateTime.Now}\n" +
            $"Simple Launcher Version: {version}\n" +
            $"OS Version: {osVersion}\n" +
            $"Architecture: {architecture}\n" +
            $"Bitness: {is64Bit}\n" +
            $"Windows Version: {windowsVersion}\n\n" +
            $"Exception type: {ex.GetType().Name}\n" +
            $"Exception details: {ex.Message}\n\n" +
            $"{contextMessage}\n\n";

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
                        DebugLogger.LogException(ex2, "Error deleting the ErrorLog");
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
                using var response = await httpClient.PostAsync(_configuration.GetValue<string>("BugReportApiUrl") ?? "https://www.purelogiccode.com/bugreport/api/send-bug-report/", jsonContent);

                DebugLogger.Log(@"The ErrorLog was successfully sent. API response: " + response.StatusCode);

                return response.IsSuccessStatusCode;
            }

            // If httpClient is null, return false
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
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
        var osVersion = RuntimeInformation.OSDescription;
        var architecture = RuntimeInformation.OSArchitecture.ToString();
        var is64Bit = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
        var windowsVersion = GetMicrosoftWindowsVersion.GetVersion();

        var errorMessage =
            $"Date: {DateTime.Now}\n" +
            $"Simple Launcher Version: {version}\n" +
            $"OS Version: {osVersion}\n" +
            $"Architecture: {architecture}\n" +
            $"Bitness: {is64Bit}\n" +
            $"Windows Version: {windowsVersion}\n\n" +
            $"Exception type: {ex.GetType().Name}\n" +
            $"Exception details: {ex.Message}\n\n" +
            $"{contextMessage}\n\n" +
            "--------------------------------------------------------------------------------------------------------------\n\n\n";

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