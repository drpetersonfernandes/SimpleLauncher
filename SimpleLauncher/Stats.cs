using System;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SimpleLauncher;

public static class Stats
{
    private static string _apiKey;
    private static readonly string ApiUrl = "https://purelogiccode.com/simplelauncher/stats.php";

    static Stats()
    {
        LoadConfiguration();
        ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
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
        int attempt = 0;
        Exception lastException = null;

        while (attempt < maxAttempts)
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(ApiUrl);
                response.EnsureSuccessStatusCode();
                return; // Success, exit method
            }
            catch (HttpRequestException ex)
            {
                lastException = ex;
                if (attempt < maxAttempts - 1)
                {
                    await Task.Delay(2000); // Wait 2 seconds before retrying
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                break; // Exit if it's not a HttpRequestException
            }

            attempt++;
        }

        if (lastException != null)
        {
            string errorMessage = lastException is HttpRequestException
                ? $"There was an error communicating with the stats API.\n\n" +
                  $"Exception type: {lastException.GetType().Name}\nException details: {lastException.Message}"
                : $"There was an unexpected error in CallApiAsync method.\n\n" +
                  $"Exception type: {lastException.GetType().Name}\nException details: {lastException.Message}";

            await LogErrors.LogErrorAsync(lastException, errorMessage);
        }
    }
}