using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleLauncher.Services;

public static class LogErrors
{
    private static readonly IHttpClientFactory HttpClientFactory;
    private static string ApiKey { get; set; }
    private static string BugReportApiUrl { get; set; }
    private static bool _isApiLoggingEnabled;

    static LogErrors()
    {
        LoadConfiguration();
        HttpClientFactory = App.ServiceProvider?.GetRequiredService<IHttpClientFactory>();
    }

    private static void LoadConfiguration()
    {
        var configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        try
        {
            if (!File.Exists(configFile))
            {
                // File is missing, disable API logging and notify the user
                _isApiLoggingEnabled = false;

                // Notify user
                MessageBoxLibrary.HandleApiConfigErrorMessageBox("File 'appsettings.json' is missing.");

                return;
            }

            var jsonString = File.ReadAllText(configFile);
            using var document = JsonDocument.Parse(jsonString);
            var root = document.RootElement;

            // Read ApiKey
            if (root.TryGetProperty(nameof(ApiKey), out var apiKeyElement) && apiKeyElement.ValueKind == JsonValueKind.String)
            {
                ApiKey = apiKeyElement.GetString();
            }

            if (string.IsNullOrEmpty(ApiKey))
            {
                // ApiKey is missing or empty, disable API logging and notify the user
                _isApiLoggingEnabled = false;

                // Notify user
                MessageBoxLibrary.HandleApiConfigErrorMessageBox("API Key is missing or empty in 'appsettings.json'.");

                return;
            }

            // Read BugReportApiUrl
            if (root.TryGetProperty(nameof(BugReportApiUrl), out var urlElement) && urlElement.ValueKind == JsonValueKind.String)
            {
                BugReportApiUrl = urlElement.GetString();
            }

            // BugReportApiUrl is missing or empty, disable API logging and notify the user
            if (string.IsNullOrEmpty(BugReportApiUrl))
            {
                _isApiLoggingEnabled = false;

                // Notify user
                MessageBoxLibrary.HandleApiConfigErrorMessageBox("Bug Report API URL is missing or empty in 'appsettings.json'.");

                return;
            }

            // If we reached here, the configuration is valid
            _isApiLoggingEnabled = true;
        }
        catch (Exception ex)
        {
            // Catch any other errors during loading (e.g., invalid JSON format)
            _isApiLoggingEnabled = false;

            // Log this critical error locally, as API logging is disabled
            WriteLocalErrorLog(ex, "Error loading API configuration from appsettings.json.");
            DebugLogger.LogException(ex, "Error loading API configuration from appsettings.json.");

            // Notify user
            MessageBoxLibrary.HandleApiConfigErrorMessageBox($"Error loading API configuration: {ex.Message}");
        }
    }

    public static async Task LogErrorAsync(Exception ex, string contextMessage = null)
    {
        if (ex == null)
        {
            ex = new Exception("Exception is null.");
        }

        DebugLogger.LogException(ex, contextMessage);

        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var errorLogPath = Path.Combine(baseDirectory, "error.log");
        var userLogPath = GetLogPath.Path();
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        version ??= "Unknown";

        // Gather additional environment info
        var osVersion = RuntimeInformation.OSDescription;
        var architecture = RuntimeInformation.OSArchitecture.ToString();
        var is64Bit = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
        var windowsVersion = GetWindowsVersion.GetVersion();

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

        try
        {
            // Append the error message to the general log
            await File.AppendAllTextAsync(errorLogPath, errorMessage);

            // Append the error message to the user-specific log
            var userErrorMessage = errorMessage + "--------------------------------------------------------------------------------------------------------------\n\n\n";
            await File.AppendAllTextAsync(userLogPath, userErrorMessage);

            // Attempt to send the error log content to the API only if enabled
            if (_isApiLoggingEnabled && await SendLogToApiAsync(errorMessage))
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
            DebugLogger.LogException(ex3, "Error writing the ErrorLog");
        }
    }

    private static async Task<bool> SendLogToApiAsync(string logContent)
    {
        // Check the flag again, just in case
        if (!_isApiLoggingEnabled)
        {
            return false;
        }

        try
        {
            var httpClient = HttpClientFactory?.CreateClient("LogErrorsClient");
            if (httpClient != null)
            {
                httpClient.DefaultRequestHeaders.Add("X-API-KEY", ApiKey);

                var payload = new
                {
                    message = logContent,
                    applicationName = "SimpleLauncher"
                };

                var jsonContent = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
                using var response = await httpClient.PostAsync(BugReportApiUrl, jsonContent);

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
    private static void WriteLocalErrorLog(Exception ex, string contextMessage)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var criticalLogPath = Path.Combine(baseDirectory, "critical_error.log");
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
        var osVersion = RuntimeInformation.OSDescription;
        var architecture = RuntimeInformation.OSArchitecture.ToString();
        var is64Bit = Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit";
        var windowsVersion = GetWindowsVersion.GetVersion();

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