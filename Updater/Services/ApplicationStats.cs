using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Updater.Services;

/// <summary>
/// Service for reporting application usage statistics to the ApplicationStats API
/// </summary>
public static class ApplicationStats
{
    private const string StatsApiUrl = "https://www.purelogiccode.com/ApplicationStats/stats";
    private const string ApiKey = "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";

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
                System.Diagnostics.Debug.WriteLine($"ApplicationStats API returned: {response.StatusCode}");
            }
        }
        catch (OperationCanceledException)
        {
            System.Diagnostics.Debug.WriteLine("ApplicationStats API call timed out.");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"ApplicationStats API call failed: {ex.Message}");
        }
    }
}
