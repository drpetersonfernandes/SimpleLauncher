using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SimpleLauncher.Tests;

public class ApiConnectivityTests
{
    private static readonly HttpClient HttpClient = new(new SocketsHttpHandler
    {
        AllowAutoRedirect = true,
        PooledConnectionLifetime = TimeSpan.FromMinutes(2)
    })
    {
        Timeout = TimeSpan.FromSeconds(30),
        DefaultRequestHeaders = { { "User-Agent", "SimpleLauncherTests/1.0" } }
    };

    private static string GetProjectFilePath(string relativePath)
    {
        var assemblyLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        if (assemblyLocation == null)
        {
            throw new InvalidOperationException("Could not determine executing assembly location.");
        }

        var path = Path.Combine(assemblyLocation, "..", "..", "..", "..", relativePath);
        return Path.GetFullPath(path);
    }

    private static async Task<JsonDocument> LoadAppSettingsAsync()
    {
        var settingsPath = GetProjectFilePath(Path.Combine("SimpleLauncher", "appsettings.json"));
        Assert.True(File.Exists(settingsPath), $"appsettings.json not found at {settingsPath}");
        var json = await File.ReadAllTextAsync(settingsPath);
        return JsonDocument.Parse(json);
    }

    [Fact]
    public async Task BugReportApiCanSendReport()
    {
        using var settings = await LoadAppSettingsAsync();
        var apiKey = settings.RootElement.GetProperty("ApiKey").GetString()
                     ?? "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";
        var apiUrl = settings.RootElement.GetProperty("BugReportApiUrl").GetString()
                     ?? "https://www.purelogiccode.com/bugreport/api/send-bug-report/";

        var payload = new
        {
            message = "Test bug report from SimpleLauncher.Tests",
            applicationName = "SimpleLauncher.Tests",
            version = "1.0.0",
            userInfo = "TestRunner",
            environment = "Test",
            stackTrace = "Test stack trace"
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, apiUrl);
        request.Headers.Add("X-API-KEY", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        using var response = await HttpClient.SendAsync(request);
        Assert.True(
            response.IsSuccessStatusCode,
            $"Bug report API returned {(int)response.StatusCode} ({response.StatusCode}). Expected a success status code.");
    }

    [Theory]
    [InlineData("https://www.purelogiccode.com/simplelauncher/stats/stats/")]
    [InlineData("https://www.purelogiccode.com/ApplicationStats/stats")]
    public async Task StatsApiIsReachable(string url)
    {
        using var settings = await LoadAppSettingsAsync();
        var apiKey = settings.RootElement.GetProperty("ApiKey").GetString()
                     ?? "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e";

        HttpContent content;
        if (url.Contains("simplelauncher/stats"))
        {
            // Usage stats call (empty body, matching Stats.CallApiAsync behavior)
            content = new StringContent(string.Empty, Encoding.UTF8, "application/json");
        }
        else
        {
            // ApplicationStats call
            var payload = new { applicationId = "simplelauncher", version = "1.0.0" };
            content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = content;

        using var response = await HttpClient.SendAsync(request);

        var isReachable = response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.TooManyRequests;
        Assert.True(
            isReachable,
            $"Stats API {url} returned {(int)response.StatusCode} ({response.StatusCode}). Could not connect to the stats API.");
    }
}
