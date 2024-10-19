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

        try
        {
            HttpResponseMessage response = await client.GetAsync(ApiUrl);
            response.EnsureSuccessStatusCode();
        }
        catch (HttpRequestException ex)
        {
            await LogErrors.LogErrorAsync(ex, $"There was an error communicating with the stats API.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}");
        }
        catch (Exception ex)
        {
            await LogErrors.LogErrorAsync(ex, $"There was an unexpected error in CallApiAsync method.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}");
        }
    }
}