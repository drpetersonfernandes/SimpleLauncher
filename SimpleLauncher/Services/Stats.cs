using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleLauncher.Services;

public static class Stats
{
    private static string _apiKey;
    private static string _statsApiUrl;
    private static readonly IHttpClientFactory HttpClientFactory;
    private static bool _isApiEnabled;

    static Stats()
    {
        HttpClientFactory = App.ServiceProvider?.GetService<IHttpClientFactory>();
        LoadConfiguration();
    }

    private static void LoadConfiguration()
    {
        var configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        try
        {
            if (!File.Exists(configFile))
            {
                // File is missing, disable API and log error locally
                _isApiEnabled = false;

                // Notify developer
                _ = LogErrors.LogErrorAsync(new FileNotFoundException($"Configuration file not found: '{configFile}'"),
                    "Stats API configuration file missing.");

                return;
            }

            var jsonString = File.ReadAllText(configFile);
            using var document = JsonDocument.Parse(jsonString);
            var root = document.RootElement;

            // Read ApiKey
            if (root.TryGetProperty("ApiKey", out var apiKeyElement) && apiKeyElement.ValueKind == JsonValueKind.String)
            {
                _apiKey = apiKeyElement.GetString();
            }

            if (string.IsNullOrEmpty(_apiKey)) // ApiKey is missing or empty, disable API and log error locally
            {
                _isApiEnabled = false;

                // Notify developer
                _ = LogErrors.LogErrorAsync(new InvalidOperationException("API Key is missing or empty in the configuration file."),
                    "Stats API Key missing.");

                return;
            }

            // Read StatsApiUrl
            if (root.TryGetProperty("StatsApiUrl", out var statsApiUrlElement) && statsApiUrlElement.ValueKind == JsonValueKind.String)
            {
                _statsApiUrl = statsApiUrlElement.GetString();
            }

            if (string.IsNullOrEmpty(_statsApiUrl)) // StatsApiUrl is missing or empty, disable API and log error locally
            {
                _isApiEnabled = false;

                // Notify developer
                Exception ex = new InvalidOperationException("Stats API URL is missing or empty in the configuration file.");
                _ = LogErrors.LogErrorAsync(ex, "Stats API URL missing.");

                return;
            }

            // If we reached here, configuration is valid
            _isApiEnabled = true;
        }
        catch (Exception ex)
        {
            // Catch any other errors during loading (e.g., invalid JSON format)
            _isApiEnabled = false;
            _ = LogErrors.LogErrorAsync(ex, "Error loading Stats API configuration from appsettings.json.");
        }
    }

    /// <summary>
    /// Call the API.
    /// If an emulator name is provided, then it is assumed this is an emulator launch call.
    /// If no emulator name is provided, then it is a general usage call.
    /// </summary>
    /// <param name="emulatorName">The name of the emulator (if applicable); otherwise, null.</param>
    public static async Task CallApiAsync(string emulatorName = null)
    {
        // Check if API is enabled before proceeding
        if (!_isApiEnabled)
        {
            _ = LogErrors.LogErrorAsync(null, "Stats API call skipped: API not enabled.");

            return;
        }

        // Determine which payload to send based on whether emulator info is provided.
        var callType = string.IsNullOrWhiteSpace(emulatorName) ? "usage" : "emulator";
        var payloadEmulatorName = callType == "emulator" ? NormalizeEmulatorName(emulatorName) : null;

        // Use the loaded API URL
        if (await TryApiAsync(_statsApiUrl, callType, payloadEmulatorName))
        {
            return; // Success.
        }
    }

    private static async Task<bool> TryApiAsync(string apiUrl, string callType, string emulatorName)
    {
        // Check if HttpClient is initialized (should be if _isApiEnabled is true)
        if (HttpClientFactory == null)
        {
            // This indicates a logic error if _isApiEnabled is true but _httpClient is null
            _ = LogErrors.LogErrorAsync(new InvalidOperationException("HttpClient is null when attempting Stats API call."), "Stats API call failed: HttpClient not initialized.");

            Debug.WriteLine(@"Stats API return failure.");
            return false;
        }

        try
        {
            var httpClient = HttpClientFactory.CreateClient("StatsClient");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

            // Build the payload depending on the call type.
            object requestData = callType == "emulator"
                ? new { callType, emulatorName }
                : new { callType }; // For a general usage call, we simply send the callType.

            var json = JsonSerializer.Serialize(requestData);
            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");

            // Send the POST request.
            using var response = await httpClient.PostAsync(apiUrl, jsonContent);

            if (response.IsSuccessStatusCode)
            {
                Debug.WriteLine(@"Stats API return success.");
                return true; // Success.
            }

            // Log API response error
            var errorContent = await response.Content.ReadAsStringAsync();
            var contextMessage = $"Stats API responded with an error.\n" +
                                 $"Status Code: '{response.StatusCode}'.\n" +
                                 $"Response Body: '{errorContent}'\n" +
                                 $"CallType: {callType}" +
                                 (callType == "emulator" ? $", EmulatorName: {emulatorName}" : string.Empty);
            _ = LogErrors.LogErrorAsync(new HttpRequestException($"Stats API error: {response.StatusCode}"), contextMessage);

            Debug.WriteLine(@"Stats API return failure.");
            return false;
        }
        catch (HttpRequestException ex)
        {
            // Log network/HTTP request errors
            var contextMessage = $"Error communicating with the Stats API at '{apiUrl}'.\n" +
                                 $"CallType: {callType}" +
                                 (callType == "emulator" ? $", EmulatorName: {emulatorName}" : string.Empty);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            Debug.WriteLine(@"Stats API return failure.");
            return false;
        }
        catch (Exception ex)
        {
            // Log any other unexpected errors
            var contextMessage = $"Unexpected error while using Stats API at '{apiUrl}'.\n" +
                                 $"CallType: {callType}" +
                                 (callType == "emulator" ? $", EmulatorName: {emulatorName}" : string.Empty);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            Debug.WriteLine(@"Stats API return failure.");
            return false;
        }
    }

    /// <summary>
    /// Converts the input string to a title case (first letter of each word capitalized).
    /// </summary>
    /// <param name="input">The input string.</param>
    /// <returns>The normalized string.</returns>
    private static string NormalizeEmulatorName(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;
        // Use CultureInfo.InvariantCulture to ensure consistent title casing regardless of user's locale
        return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(input.ToLowerInvariant());
    }
}