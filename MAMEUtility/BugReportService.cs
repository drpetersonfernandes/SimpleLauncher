using System.Globalization;
using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace MAMEUtility;

public class BugReportService(string apiUrl, string apiKey, string applicationName = "MAME Utility")
    : IDisposable
{
    private readonly string _apiUrl = apiUrl;
    private readonly string _apiKey = apiKey;
    private readonly string _applicationName = applicationName;
    private readonly HttpClient _httpClient = new();

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
        sb.AppendLine(CultureInfo.InvariantCulture, $"Exception: {exception.GetType().Name}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Message: {exception.Message}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Stack Trace: {exception.StackTrace}");

        // Add version info
        sb.AppendLine(CultureInfo.InvariantCulture, $"App Version: {AboutWindow.ApplicationVersion}");

        // Add OS info
        sb.AppendLine(CultureInfo.InvariantCulture, $"OS: {Environment.OSVersion}");
        sb.AppendLine(CultureInfo.InvariantCulture, $".NET Version: {Environment.Version}");

        // Add additional information about inner exceptions if present
        var innerException = exception.InnerException;
        if (innerException == null) return sb.ToString();

        sb.AppendLine("\nInner Exception:");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Type: {innerException.GetType().Name}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Message: {innerException.Message}");
        sb.AppendLine(CultureInfo.InvariantCulture, $"Stack Trace: {innerException.StackTrace}");

        return sb.ToString();
    }

    public void Dispose()
    {
        // Dispose of the HttpClient to release resources
        _httpClient?.Dispose();

        // Suppress finalization
        GC.SuppressFinalize(this);
    }
}