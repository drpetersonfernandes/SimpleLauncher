using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SimpleLauncher;

public static class LogErrors
{
    private static readonly HttpClient HttpClient = new();
    private static string ApiKey { get; set; }

    static LogErrors()
    {
        LoadConfiguration();
    }

    private static void LoadConfiguration()
    {
        var configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        if (!File.Exists(configFile)) return;

        var config = JObject.Parse(File.ReadAllText(configFile));
        ApiKey = config[nameof(ApiKey)]?.ToString();
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
        var windowsVersion = GetWindowsVersion();

        // Write error Message
        var errorMessage =
            $"Date: {DateTime.Now}\n" +
            $"Simple Launcher Version: {version}\n" +
            $"OS Version: {osVersion}\n" +
            $"Architecture: {architecture}\n" +
            $"Bitness: {is64Bit}\n" +
            $"Windows Version: {windowsVersion}\n\n" +
            $"Exception type: {ex.GetType().Name}\n" +
            $"Exception details: {ex.Message}\n\n";

        try
        {
            // Append the error message to the general log
            await File.AppendAllTextAsync(errorLogPath, errorMessage);

            // Append the error message to the user-specific log
            var userErrorMessage = errorMessage + "--------------------------------------------------------------------------------------------------------------\n\n\n";
            await File.AppendAllTextAsync(userLogPath, userErrorMessage);

            // Attempt to send the error log content to the API.
            if (await SendLogToApiAsync(errorMessage))
            {
                // If the log was successfully sent, delete the general log file to clean up.
                if (File.Exists(errorLogPath))
                {
                    try
                    {
                        File.Delete(errorLogPath);
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
        if (string.IsNullOrEmpty(ApiKey))
        {
            // Log or handle the missing API key appropriately
            return false;
        }

        // Prepare the content to be sent via HTTP POST.
        var formData = new MultipartFormDataContent
        {
            { new StringContent("contact@purelogiccode.com"), "recipient" },
            { new StringContent("Error Log from SimpleLauncher"), "subject" },
            { new StringContent("SimpleLauncher User"), "name" },
            { new StringContent(logContent), "message" }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://www.purelogiccode.com/simplelauncher/send_email.php")
        {
            Content = formData
        };
        request.Headers.Add("X-API-KEY", ApiKey);

        try
        {
            var response = await HttpClient.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception)
        {
            return false;
        }
    }

    private static string GetWindowsVersion()
    {
        var version = Environment.OSVersion.Version;
        return version switch
        {
            { Major: 10, Minor: 0 } => "Windows 10 or Windows 11",
            { Major: 6, Minor: 3 } => "Windows 8.1",
            { Major: 6, Minor: 2 } => "Windows 8",
            { Major: 6, Minor: 1 } => "Windows 7",
            _ => $"Unknown Windows Version ({version})"
        };
    }
}