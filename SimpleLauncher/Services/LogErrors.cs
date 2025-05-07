using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SimpleLauncher.Services;

public static class LogErrors
{
    private static readonly HttpClient HttpClient = new();
    private static string ApiKey { get; set; }
    private static string BugReportApiUrl { get; set; } // Now a property
    private static bool _isApiLoggingEnabled; // Flag to control API logging

    static LogErrors()
    {
        LoadConfiguration();
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
                MessageBoxLibrary.HandleApiConfigErrorMessageBox("File 'appsettings.json' is missing.");
                return; // Stop loading configuration
            }

            var config = JObject.Parse(File.ReadAllText(configFile));

            // Read ApiKey
            ApiKey = config[nameof(ApiKey)]?.ToString();
            if (string.IsNullOrEmpty(ApiKey))
            {
                // ApiKey is missing or empty, disable API logging and notify the user
                _isApiLoggingEnabled = false;
                MessageBoxLibrary.HandleApiConfigErrorMessageBox("API Key is missing or empty in 'appsettings.json'.");
                return; // Stop loading configuration
            }

            // Read BugReportApiUrl
            BugReportApiUrl = config[nameof(BugReportApiUrl)]?.ToString();
            if (string.IsNullOrEmpty(BugReportApiUrl))
            {
                // BugReportApiUrl is missing or empty, disable API logging and notify the user
                _isApiLoggingEnabled = false;
                MessageBoxLibrary.HandleApiConfigErrorMessageBox("Bug Report API URL is missing or empty in 'appsettings.json'.");
                return; // Stop loading configuration
            }

            // If we reached here, the configuration is valid
            _isApiLoggingEnabled = true;

            // Set default request headers for the HttpClient
            HttpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", ApiKey);
        }
        catch (Exception ex)
        {
            // Catch any other errors during loading (e.g., invalid JSON format)
            _isApiLoggingEnabled = false;
            // Log this critical error locally, as API logging is disabled
            WriteLocalErrorLog(ex, "Error loading API configuration from appsettings.json.");
            MessageBoxLibrary.HandleApiConfigErrorMessageBox($"Error loading API configuration: {ex.Message}");
        }
    }

    public static async Task LogErrorAsync(Exception ex, string contextMessage = null)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var errorLogPath = Path.Combine(baseDirectory, "error.log");
        var userLogPath = Path.Combine(baseDirectory, "error_user.log");
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
                    catch (Exception)
                    {
                        // ignore
                    }
                }
            }
        }
        catch (Exception)
        {
            // Ignore any exceptions raised during logging to avoid interrupting the main flow
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
            // Create the content
            var payload = new
            {
                message = logContent,
                applicationName = "SimpleLauncher"
            };

            // Serialize to JSON
            var jsonContent = JsonSerializer.Serialize(payload);
            var stringContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Send the request
            // HttpClient headers are set in LoadConfiguration when _isApiLoggingEnabled is true
            using var response = await HttpClient.PostAsync(BugReportApiUrl, stringContent);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
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
        catch (Exception)
        {
            // If even local logging fails, there's not much else we can do here.
        }
    }
}