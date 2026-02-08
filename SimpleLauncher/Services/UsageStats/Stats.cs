using System;
using System.Globalization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.UsageStats;

public class Stats
{
    private string _apiKey;
    private string _statsApiUrl;
    private readonly IHttpClientFactory _httpClientFactory;
    private bool _isApiEnabled;

    public Stats(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        LoadConfiguration(configuration);
    }

    private void LoadConfiguration(IConfiguration configuration)
    {
        try
        {
            // Read ApiKey
            _apiKey = configuration.GetValue<string>("ApiKey") ?? "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";

            if (string.IsNullOrEmpty(_apiKey)) // ApiKey is missing or empty, disable API and log error locally
            {
                _isApiEnabled = false;

                // Notify developer
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new InvalidOperationException("API Key is missing or empty in the configuration file."), "Stats API Key missing.");

                return;
            }

            _statsApiUrl = configuration.GetValue<string>("StatsApiUrl") ?? "https://www.purelogiccode.com/simplelauncher/stats/stats/";

            if (string.IsNullOrEmpty(_statsApiUrl))
            {
                _isApiEnabled = false;

                // Notify developer
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new InvalidOperationException("Stats API URL is missing or empty in the configuration file."), "Stats API URL missing.");

                return;
            }

            // If we reached here, configuration is valid
            _isApiEnabled = true;
        }
        catch (Exception ex)
        {
            // Notify developer
            // Catch any other errors during loading (e.g., invalid JSON format)
            _isApiEnabled = false;
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Error loading Stats API configuration from appsettings.json.");
        }
    }

    /// <summary>
    /// Call the API.
    /// If an emulator name is provided, then it is assumed this is an emulator launch call.
    /// If no emulator name is provided, then it is a general usage call.
    /// </summary>
    /// <param name="emulatorName">The name of the emulator (if applicable); otherwise, null.</param>
    internal async Task CallApiAsync(string emulatorName = null)
    {
        // Check if API is enabled before proceeding
        if (!_isApiEnabled)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, "Stats API call skipped: API not enabled.");

            return;
        }

        // Determine which payload to send based on whether emulator info is provided.
        var callType = string.IsNullOrWhiteSpace(emulatorName) ? "usage" : "emulator";
        var payloadEmulatorName = callType == "emulator" ? NormalizeEmulatorName(emulatorName) : null;

        // Use the loaded API URL
        if (await TryApiAsync(callType, payloadEmulatorName))
        {
            // ReSharper disable once RedundantJumpStatement
            return; // Success.
        }
    }

    private async Task<bool> TryApiAsync(string callType, string emulatorName)
    {
        // Check if HttpClient is initialized (should be if _isApiEnabled is true)
        if (_httpClientFactory == null)
        {
            // Notify developer
            // This indicates a logic error if _isApiEnabled is true but _httpClient is null
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new InvalidOperationException("HttpClient is null when attempting Stats API call."), "Stats API call failed: HttpClient not initialized.");

            return false;
        }

        try
        {
            var httpClient = _httpClientFactory?.CreateClient("StatsClient");
            if (httpClient != null)
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);

                HttpContent jsonContent;
                if (callType == "emulator")
                {
                    // For emulator calls, send ONLY the emulatorName
                    var requestData = new { emulatorName };
                    var json = JsonSerializer.Serialize(requestData);
                    jsonContent = new StringContent(json, Encoding.UTF8, "application/json");
                }
                else
                {
                    // For general usage calls, send an empty body.
                    jsonContent = new StringContent(string.Empty, Encoding.UTF8, "application/json");
                }

                // Send the POST request.
                using var response = await httpClient.PostAsync(_statsApiUrl, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    DebugLogger.Log("The Stats was successfully sent. API response: OK");

                    return true; // Success.
                }

                // Notify developer
                // Log API response error
                var errorContent = await response.Content.ReadAsStringAsync();
                var contextMessage = $"Stats API responded with an error.\n" +
                                     $"Status Code: '{response.StatusCode}'.\n" +
                                     $"Response Body: '{errorContent}'\n" +
                                     $"CallType: {callType}" +
                                     (callType == "emulator" ? $", EmulatorName: {emulatorName}" : string.Empty);
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new HttpRequestException($"Stats API error: {response.StatusCode}"), contextMessage);
            }

            return false;
        }
        catch (HttpRequestException ex)
        {
            // Notify developer
            // Log network/HTTP request errors
            var contextMessage = $"Error communicating with the Stats API at '{_statsApiUrl}'.\n" +
                                 $"CallType: {callType}" +
                                 (callType == "emulator" ? $", EmulatorName: {emulatorName}" : string.Empty);
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            return false;
        }
        catch (Exception ex)
        {
            // Notify developer
            // Log any other unexpected errors
            var contextMessage = $"Unexpected error while using Stats API at '{_statsApiUrl}'.\n" +
                                 $"CallType: {callType}" +
                                 (callType == "emulator" ? $", EmulatorName: {emulatorName}" : string.Empty);
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

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