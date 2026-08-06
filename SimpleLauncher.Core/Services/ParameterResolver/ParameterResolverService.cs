using System.Text;
using System.Text.Json;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Core.Services.ParameterResolver;

/// <summary>
/// Resolves emulator launch parameters by calling a remote API endpoint with the system and emulator context.
/// </summary>
public class ParameterResolverService : IParameterResolverService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="ParameterResolverService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The HTTP client factory for making API requests.</param>
    /// <param name="logErrors">The logger instance for error logging.</param>
    public ParameterResolverService(IHttpClientFactory httpClientFactory, ILogger logErrors)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logErrors;
    }

    /// <summary>
    /// Sends a parameter resolution request to the remote API and returns the resolved parameters.
    /// </summary>
    /// <param name="request">The request containing system name, emulator name, and ROM file information.</param>
    /// <returns>The resolved parameters, or null if the API call fails.</returns>
    public async Task<ParameterResolverResult?> ResolveParametersAsync(ParameterResolverRequest request)
    {
        var apiKey = AppConstants.GetApiKey();
        var client = _httpClientFactory.CreateClient("ParameterResolverClient");

        var jsonContent = JsonSerializer.Serialize(request, JsonOptions);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/ParameterResolver/resolve");
        httpRequest.Headers.Add("X-Api-Key", apiKey);
        httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await client.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            try
            {
                return JsonSerializer.Deserialize<ParameterResolverResult>(responseBody, JsonOptions);
            }
            catch (JsonException ex)
            {
                // The API returned a 200 with an unparseable body; treat it as a failed resolution
                _logger.Error(ex, "ParameterResolver API returned malformed JSON");
                return null;
            }
        }

        var apiException = new InvalidOperationException($"ParameterResolver API returned {(int)response.StatusCode}: {responseBody}");
        _logger.Error(apiException, "ParameterResolver API error");
        return null;
    }
}
