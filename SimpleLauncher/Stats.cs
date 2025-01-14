using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SimpleLauncher;

public static class Stats
{
    private static string _apiKey;
    private static readonly string PrimaryApiUrl = "https://www.purelogiccode.com/simplelauncher/stats/stats";
    private static readonly string BackupApiUrl = "https://www.purelogiccode.com/simplelauncher/stats.php";

    static Stats()
    {
        LoadConfiguration();
    }

    private static void LoadConfiguration()
    {
        string configFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");
        if (File.Exists(configFile))
        {
            var config = JObject.Parse(File.ReadAllText(configFile));
            _apiKey = config["ApiKey"]?.ToString();
        }
    }

    public static async Task CallApiAsync()
    {
        using var client = new HttpClient();
        if (!string.IsNullOrEmpty(_apiKey))
        {
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }
        else
        {
            string errorMessage = "API Key is missing in the CallApiAsync method.";
            Exception ex = new Exception(errorMessage);
            await LogErrors.LogErrorAsync(ex, errorMessage);
            return;
        }

        int maxAttempts = 2;

        // Try the primary API first
        if (await TryApiAsync(client, PrimaryApiUrl, maxAttempts))
        {
            return; // Success
        }

        // Fallback to the backup API if the primary API fails
        if (await TryApiAsync(client, BackupApiUrl, maxAttempts))
        {
            return; // Success
        }

        string finalErrorMessage = "Both primary and backup APIs failed after multiple attempts.";
        await LogErrors.LogErrorAsync(new Exception(finalErrorMessage), finalErrorMessage);
    }

    private static async Task<bool> TryApiAsync(HttpClient client, string apiUrl, int maxAttempts)
    {
        int attempt = 0;
        while (attempt < maxAttempts)
        {
            try
            {
                HttpResponseMessage response = await client.PostAsync(apiUrl, null);
                response.EnsureSuccessStatusCode();
                return true; // Success
            }
            catch (HttpRequestException ex)
            {
                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(2000); // Wait 2 seconds before retrying
                }
                else
                {
                    string errorMessage = $"There was an error communicating with the stats API at {apiUrl}." +
                                          $"Exception type: {ex.GetType().Name}" +
                                          $"Exception details: {ex.Message}";
                    await LogErrors.LogErrorAsync(ex, errorMessage);
                }
            }
            catch (Exception ex)
            {
                string errorMessage = $"There was an unexpected error in CallApiAsync method while using {apiUrl}." +
                                      $"Exception type: {ex.GetType().Name}" +
                                      $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, errorMessage);
                break; // Exit if it's not a HttpRequestException
            }
            attempt++;
        }
        return false; // Failed after max attempts
    }
}