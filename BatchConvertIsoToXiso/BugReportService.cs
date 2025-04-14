using System.Net.Http;
using System.Net.Http.Json;

namespace BatchConvertIsoToXiso;

/// <inheritdoc />
/// <summary>
/// Service responsible for silently sending bug reports to the BugReport API
/// </summary>
public class BugReportService(string apiUrl, string apiKey, string applicationName) : IDisposable
{
    private readonly HttpClient _httpClient = new();
    private bool _disposed;
    private readonly string _apiUrl = apiUrl;
    private readonly string _apiKey = apiKey;
    private readonly string _applicationName = applicationName;

    /// <summary>
    /// Silently sends a bug report to the API
    /// </summary>
    /// <param name="message">The error message or bug report</param>
    /// <returns>A task representing the asynchronous operation</returns>
    public async Task<bool> SendBugReportAsync(string message)
    {
        try
        {
            // Add the API key to the headers
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("X-API-KEY", _apiKey);

            // Create the request payload
            var content = JsonContent.Create(new
            {
                message,
                applicationName = _applicationName
            });

            // Send the request
            var response = await _httpClient.PostAsync(_apiUrl, content);

            // Return true if successful
            return response.IsSuccessStatusCode;
        }
        catch
        {
            // Silently fail if there's an exception
            return false;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed) return;

        if (disposing)
        {
            _httpClient.Dispose();
        }

        _disposed = true;
    }
}