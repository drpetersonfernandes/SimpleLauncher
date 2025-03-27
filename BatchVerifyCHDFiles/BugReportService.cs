using System.Net.Http;
using System.Net.Http.Json;

namespace BatchVerifyCHDFiles;

/// <summary>
/// Service responsible for silently sending bug reports to the BugReport API
/// </summary>
public class BugReportService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiUrl;
    private readonly string _apiKey;
    private readonly string _applicationName;

    public BugReportService(string apiUrl, string apiKey, string applicationName)
    {
        _httpClient = new HttpClient();
        _apiUrl = apiUrl;
        _apiKey = apiKey;
        _applicationName = applicationName;
    }

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
}