using System.Reflection;
using System.Text;
using System.Text.Json;
using SimpleLauncher.AdminAPI.Models;

namespace SimpleLauncher.AdminAPI.Services;

public interface IBugReportService
{
    Task SendBugReportAsync(Exception exception, string? userInfo = null);
}

public class BugReportService : IBugReportService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<BugReportService> _logger;

    public BugReportService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogger<BugReportService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendBugReportAsync(Exception exception, string? userInfo = null)
    {
        var serviceUrl = _configuration["BugReportService:Url"];
        var apiKey = _configuration["BugReportService:ApiKey"];

        if (string.IsNullOrEmpty(serviceUrl) || string.IsNullOrEmpty(apiKey))
        {
            // Original: _logger.LogWarning("Bug Report Service URL or API Key is not configured. Skipping bug report.");
            Log.BugReportServiceNotConfigured(_logger);
            return;
        }

        try
        {
            var payload = new BugReportPayload
            {
                ApplicationName = Assembly.GetEntryAssembly()?.GetName().Name ?? "SimpleLauncher.AdminAPI",
                Version = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(),
                Message = exception.Message,
                StackTrace = exception.ToString(), // Send the full exception details
                UserInfo = userInfo ?? "N/A"
            };

            var client = _httpClientFactory.CreateClient("BugReportServiceClient");
            var request = new HttpRequestMessage(HttpMethod.Post, serviceUrl);
            request.Headers.Add("X-API-KEY", apiKey);

            var jsonPayload = JsonSerializer.Serialize(payload);
            request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            var response = await client.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                // Original: _logger.LogInformation("Successfully sent bug report. Response: {Response}", responseBody);
                Log.BugReportSentSuccess(_logger, responseBody);
            }
            else
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                // Original: _logger.LogError(
                //     "Failed to send bug report. Status Code: {StatusCode}. Response: {ErrorBody}",
                //     response.StatusCode,
                //     errorBody);
                Log.BugReportSentFailure(_logger, response.StatusCode, errorBody);
            }
        }
        catch (Exception ex)
        {
            // Original: _logger.LogError(ex, "An exception occurred while trying to send a bug report.");
            Log.BugReportSendException(_logger, ex);
        }
    }
}
