using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace SimpleLauncher.Updater.Services;

/// <summary>
/// Service for reporting application usage statistics to the ApplicationStats API
/// </summary>
internal static class ApplicationStats
{
    private const string StatsApiUrl = "https://www.purelogiccode.com/ApplicationStats/stats";

    private const string ApiKeyEncoded =
        "YUdwb04zbDFOblExTm5SNWNqVTBNRzg1ZFRnM05qYzJOelp5TlRZM05EVXpORFExTXpJek5USTJOR00zTldJMmREZG5aMmRvWjJjM05uUnlaalUyTkdVPQ==";

    private static readonly string ApiKey = DecodeApiKey();

    private static string DecodeApiKey()
    {
        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(ApiKeyEncoded));
        return Encoding.UTF8.GetString(Convert.FromBase64String(decoded));
    }

    /// <summary>
    /// Sends application launch statistics to the ApplicationStats API.
    /// This is a fire-and-forget operation that will not block or throw.
    /// </summary>
    public static void SendLaunchStats()
    {
        _ = Task.Run(static async () => await SendLaunchStatsAsync());
    }

    /// <summary>
    /// Sends application launch statistics to the ApplicationStats API asynchronously.
    /// </summary>
    private static async Task SendLaunchStatsAsync()
    {
        try
        {
            var assembly = Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version?.ToString() ?? "0.0.0";

            var httpClient = MainWindow.HttpClient;
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ApiKey);

            var payload = new { applicationId = "simplelauncher-updater", version };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));
            using var response = await httpClient.PostAsync(StatsApiUrl, content, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                Log.Warning("ApplicationStats API returned non-success status: {StatusCode}", response.StatusCode);
            }
        }
        catch (OperationCanceledException)
        {
            Log.Warning("ApplicationStats API call timed out.");
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "ApplicationStats API call failed");
        }
    }
}
