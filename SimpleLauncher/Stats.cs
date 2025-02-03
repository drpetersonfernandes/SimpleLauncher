using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SimpleLauncher;

public static class Stats
{
    private static string _apiKey;
    private static readonly string ApiUrl = "https://www.purelogiccode.com/simplelauncher/stats/stats";
    private static HttpClient _httpClient;

    static Stats()
    {
        LoadConfiguration();
        InitializeHttpClient();
    }

    private static void LoadConfiguration()
    {
        string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
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
            throw new FileNotFoundException($"Configuration file not found: {configFile}");
        }
    }

    private static void InitializeHttpClient()
    {
        _httpClient = new HttpClient();
        if (!string.IsNullOrEmpty(_apiKey))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _apiKey);
        }
    }

    public static async Task CallApiAsync(string emulatorName = null)
    {
        if (await TryApiAsync(ApiUrl, emulatorName))
        {
            return; // Success
        }

        // Notify developer
        string finalErrorMessage = "API request failed.";
        Exception ex = new HttpRequestException(finalErrorMessage);
        await LogErrors.LogErrorAsync(ex, finalErrorMessage);
    }

    private static async Task<bool> TryApiAsync(string apiUrl, string emulatorName)
    {
        try
        {
            // Prepare the request content
            var requestData = new
            {
                emulatorName = emulatorName ?? "Unknown" // Default to "Unknown" if null
            };
            var jsonContent = new StringContent(
                JsonSerializer.Serialize(requestData),
                Encoding.UTF8,
                "application/json"
            );

            // Send the POST request
            HttpResponseMessage response = await _httpClient.PostAsync(apiUrl, jsonContent);

            if (!response.IsSuccessStatusCode)
            {
                // Notify developer
                string errorMessage = $"API responded with an error. Status Code: {response.StatusCode}. " +
                                      $"EmulatorName: {emulatorName ?? "Unknown"}.";
                await LogErrors.LogErrorAsync(new HttpRequestException(errorMessage), errorMessage);
             
                return false;
            }

            return true; // Success
        }
        catch (HttpRequestException ex)
        {
            // Notify developer
            string errorMessage = $"Error communicating with the API at {apiUrl}. " +
                                  $"EmulatorName: {emulatorName ?? "Unknown"}. " +
                                  $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);
        }
        catch (Exception ex)
        {
            // Notify developer
            string errorMessage = $"Unexpected error while using {apiUrl}. " +
                                  $"EmulatorName: {emulatorName ?? "Unknown"}. " +
                                  $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, errorMessage);
        }

        return false; // Failed after exception
    }
}