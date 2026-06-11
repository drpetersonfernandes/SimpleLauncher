using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.UsageStats;

public class ApplicationStats
{
    private static readonly IDebugLogger DebugLogger = App.ServiceProvider.GetRequiredService<IDebugLogger>();

    public static async Task CallApplicationStatsAsync(IConfiguration configuration, ILogErrors logErrors)
    {
        try
        {
            var apiKey = configuration.GetValue<string>("ApiKey") ?? "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";
            var statsUrl = configuration.GetValue<string>("StatsApiUrl2") ?? "https://www.purelogiccode.com/ApplicationStats/stats";
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";

            var httpClientFactory = App.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            using var client = httpClientFactory.CreateClient("StatsClient");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            var payload = new { applicationId = "simplelauncher", version };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Use a CancellationToken with a 20-second timeout to prevent indefinite hangs
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(20));

            using var response = await client.PostAsync(statsUrl, content, cts.Token);
            if (!response.IsSuccessStatusCode)
            {
                DebugLogger.Log($"ApplicationStats API returned: {response.StatusCode}");

                var ex = new HttpRequestException($"ApplicationStats API returned: {response.StatusCode}");
                if (response.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    // ignore
                }
                else
                {
                    logErrors.LogAndForget(ex, $"ApplicationStats API returned: {response.StatusCode}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Request timed out - log but don't crash
            const string message = "ApplicationStats API call timed out after 15 seconds.";
            DebugLogger.Log(message);
            logErrors.LogAndForget(null, message);
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"ApplicationStats API call failed: {ex.Message}");
            logErrors.LogAndForget(ex, $"ApplicationStats API call failed: {ex.Message}");
        }
    }
}