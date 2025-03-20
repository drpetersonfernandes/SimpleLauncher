using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace MAMEUtility;

public class BugReportService
{
    private readonly string _apiUrl;
    private readonly string _apiKey;
    private readonly string _applicationName;
    private readonly HttpClient _httpClient;

    public BugReportService(string apiUrl, string apiKey, string applicationName = "MAME Utility")
    {
        _apiUrl = apiUrl;
        _apiKey = apiKey;
        _applicationName = applicationName;
        _httpClient = new HttpClient();
    }

    public async Task SendExceptionReportAsync(Exception exception)
    {
        try
        {
            var message = FormatExceptionMessage(exception);
            await SendReportAsync(message);
        }
        catch
        {
            // Silently fail if we can't send the bug report,
            // We don't want errors in the error reporting to cause more issues
        }
    }

    private async Task SendReportAsync(string message)
    {
        try
        {
            var content = new
            {
                message,
                applicationName = _applicationName
            };

            var json = JsonSerializer.Serialize(content);
            var stringContent = new StringContent(json, Encoding.UTF8, "application/json");

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);

            var response = await _httpClient.PostAsync(_apiUrl, stringContent);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            // Silently fail if sending fails
        }
    }

    private static string FormatExceptionMessage(Exception exception)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Exception: {exception.GetType().Name}");
        sb.AppendLine($"Message: {exception.Message}");
        sb.AppendLine($"Stack Trace: {exception.StackTrace}");

        // Add version info
        sb.AppendLine($"App Version: {AboutWindow.ApplicationVersion}");

        // Add OS info
        sb.AppendLine($"OS: {Environment.OSVersion}");
        sb.AppendLine($".NET Version: {Environment.Version}");

        // Add additional information about inner exceptions if present
        var innerException = exception.InnerException;
        if (innerException != null)
        {
            sb.AppendLine("\nInner Exception:");
            sb.AppendLine($"Type: {innerException.GetType().Name}");
            sb.AppendLine($"Message: {innerException.Message}");
            sb.AppendLine($"Stack Trace: {innerException.StackTrace}");
        }

        return sb.ToString();
    }
}