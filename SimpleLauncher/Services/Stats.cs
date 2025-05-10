using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SimpleLauncher.Services;

public static class Stats
{
    private static string _apiKey;
    private static string _statsApiUrl;
    private static HttpClient _httpClient;
    private static bool _isApiEnabled; // Flag to control API calls

    static Stats()
    {
        LoadConfiguration();

        // Initialize HttpClient only if API is enabled
        if (_isApiEnabled)
        {
            InitializeHttpClient();
        }
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

                // Use LogErrors directly for local logging if config loading fails
                _ = LogErrors.LogErrorAsync(new FileNotFoundException($"Configuration file not found: '{configFile}'"),
                    "Stats API configuration file missing.");

                return; // Stop loading configuration
            }

            var config = JObject.Parse(File.ReadAllText(configFile));

            // Read ApiKey
            _apiKey = config["ApiKey"]?.ToString();
            if (string.IsNullOrEmpty(_apiKey))
            {
                // ApiKey is missing or empty, disable API and log error locally
                _isApiEnabled = false;

                _ = LogErrors.LogErrorAsync(new InvalidOperationException("API Key is missing or empty in the configuration file."), "Stats API Key missing.");

                return; // Stop loading configuration
            }

            // Read StatsApiUrl
            _statsApiUrl = config["StatsApiUrl"]?.ToString();
            if (string.IsNullOrEmpty(_statsApiUrl))
            {
                // StatsApiUrl is missing or empty, disable API and log error locally
                _isApiEnabled = false;

                // Notify developer
                Exception ex = new InvalidOperationException("Stats API URL is missing or empty in the configuration file.");
                _ = LogErrors.LogErrorAsync(ex, "Stats API URL missing.");

                return; // Stop loading configuration
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

    private static void InitializeHttpClient()
    {
        // Only initialize if API is enabled
        if (!_isApiEnabled) return;

        var handler = new HttpClientHandler();
        _httpClient = new HttpClient(handler);

        // ApiKey is guaranteed to be not null/empty if _isApiEnabled is true
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

        // Add a User-Agent header as required by some APIs (like GitHub, though this API might not need it, it's good practice)
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("SimpleLauncher", "1.0")); // Replace 1.0 with actual version if available
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
        string callType;
        string payloadEmulatorName = null;
        if (string.IsNullOrWhiteSpace(emulatorName))
        {
            // General usage call – no emulator information.
            callType = "usage";
        }
        else
        {
            // Normalize the emulator name to Title Case (first letter of each word capitalized).
            payloadEmulatorName = NormalizeEmulatorName(emulatorName);
            callType = "emulator";
        }

        // Use the loaded API URL
        if (await TryApiAsync(_statsApiUrl, callType, payloadEmulatorName))
        {
            // ReSharper disable once RedundantJumpStatement
            return; // Success.
        }

        // If TryApiAsync returns false, it means the request failed after handling exceptions internally.
        // An error has already been logged by TryApiAsync via LogErrors.LogErrorAsync.
        // No need for further logging here.
    }

    /// <summary>
    /// Attempts to send a POST to the API.
    /// </summary>
    /// <param name="apiUrl">The API URL.</param>
    /// <param name="callType">Type of call ("usage" or "emulator").</param>
    /// <param name="emulatorName">Normalized emulator name (if callType is "emulator"); otherwise, null.</param>
    /// <returns>True if the request succeeds; otherwise, false.</returns>
    private static async Task<bool> TryApiAsync(string apiUrl, string callType, string emulatorName)
    {
        // Check if HttpClient is initialized (should be if _isApiEnabled is true)
        if (_httpClient == null)
        {
            // This indicates a logic error if _isApiEnabled is true but _httpClient is null
            _ = LogErrors.LogErrorAsync(new InvalidOperationException("HttpClient is null when attempting Stats API call."), "Stats API call failed: HttpClient not initialized.");

            return false;
        }

        try
        {
            // Build the payload depending on the call type.
            object requestData = callType == "emulator"
                ? new { callType, emulatorName }
                : new { callType }; // For a general usage call, we simply send the callType.

            var json = JsonSerializer.Serialize(requestData);
            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");

            // Send the POST request.
            using var response = await _httpClient.PostAsync(apiUrl, jsonContent);

            if (response.IsSuccessStatusCode) return true; // Success.

            // Log API response error
            var errorContent = await response.Content.ReadAsStringAsync();
            var contextMessage = $"Stats API responded with an error.\n" +
                                 $"Status Code: '{response.StatusCode}'.\n" +
                                 $"Response Body: '{errorContent}'\n" +
                                 $"CallType: {callType}" +
                                 (callType == "emulator" ? $", EmulatorName: {emulatorName}" : string.Empty);
            _ = LogErrors.LogErrorAsync(new HttpRequestException($"Stats API error: {response.StatusCode}"), contextMessage);

            return false;
        }
        catch (HttpRequestException ex)
        {
            // Log network/HTTP request errors
            var contextMessage = $"Error communicating with the Stats API at '{apiUrl}'.\n" +
                                 $"CallType: {callType}" +
                                 (callType == "emulator" ? $", EmulatorName: {emulatorName}" : string.Empty);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
        catch (Exception ex)
        {
            // Log any other unexpected errors
            var contextMessage = $"Unexpected error while using Stats API at '{apiUrl}'.\n" +
                                 $"CallType: {callType}" +
                                 (callType == "emulator" ? $", EmulatorName: {emulatorName}" : string.Empty);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }

        return false; // Failed after exception.
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

    public static void DisposeHttpClient()
    {
        _httpClient?.Dispose();
        _httpClient = null;
    }
}