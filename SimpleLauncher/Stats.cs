using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Authentication;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SimpleLauncher;

public static class Stats
{
    private static string _apiKey;
    private const string ApiUrl = "https://www.purelogiccode.com/simplelauncher/stats/stats";
    private static HttpClient _httpClient;

    static Stats()
    {
        LoadConfiguration();
        InitializeHttpClient();
    }

    private static void LoadConfiguration()
    {
        var configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        if (File.Exists(configFile))
        {
            var config = JObject.Parse(File.ReadAllText(configFile));
            _apiKey = config["ApiKey"]?.ToString();
            if (string.IsNullOrEmpty(_apiKey))
            {
                throw new InvalidOperationException("API Key is missing or empty in the configuration file.");
            }
        }
        else
        {
            throw new FileNotFoundException($"Configuration file not found: '{configFile}'");
        }
    }

    private static void InitializeHttpClient()
    {
        var handler = new HttpClientHandler();
        handler.SslProtocols = SslProtocols.Tls12;
        _httpClient = new HttpClient(handler);

        if (!string.IsNullOrEmpty(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _apiKey);
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

        if (await TryApiAsync(ApiUrl, callType, payloadEmulatorName))
        {
            return; // Success.
        }

        // Notify the developer
        const string contextMessage = "API request failed.";
        var ex = new HttpRequestException(contextMessage);
        _ = LogErrors.LogErrorAsync(ex, contextMessage);
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
        try
        {
            // Build the payload depending on the call type.
            object requestData = callType == "emulator"
                ? new { callType, emulatorName }
                : new { callType }; // For a general usage call, we simply send the callType.

            var json = JsonSerializer.Serialize(requestData);
            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");

            // Send the POST request.
            var response = await _httpClient.PostAsync(apiUrl, jsonContent);

            if (response.IsSuccessStatusCode) return true; // Success.

            // Notify the developer
            var contextMessage = $"API responded with an error.\n" +
                                 $"Status Code: '{response.StatusCode}'.\n" +
                                 $"CallType: {callType}" +
                                 (callType == "emulator" ? $", EmulatorName: {emulatorName}" : string.Empty);
            var ex = new HttpRequestException(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            return false;
        }
        catch (HttpRequestException ex)
        {
            // Notify developer.
            var contextMessage = $"Error communicating with the API at '{apiUrl}'.\n" +
                                 $"CallType: {callType}" +
                                 (callType == "emulator" ? $", EmulatorName: {emulatorName}" : string.Empty);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
        catch (Exception ex)
        {
            // Notify developer.
            var contextMessage = $"Unexpected error while using '{apiUrl}'.\n" +
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
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLowerInvariant());
    }
}