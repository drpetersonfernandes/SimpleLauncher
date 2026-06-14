using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;

namespace SimpleLauncher.Services.ParameterResolver;

public class ParameterResolverService : IParameterResolverService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ParameterResolverService(IHttpClientFactory httpClientFactory, IConfiguration configuration, ILogErrors logErrors)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logErrors = logErrors;
    }

    public async Task<ParameterResolverResult> ResolveParametersAsync(ParameterResolverRequest request)
    {
        var apiKey = _configuration["ApiKey"];
        var client = _httpClientFactory.CreateClient("ParameterResolverClient");

        var jsonContent = JsonSerializer.Serialize(request, JsonOptions);
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "api/ParameterResolver/resolve");
        httpRequest.Headers.Add("X-Api-Key", apiKey);
        httpRequest.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await client.SendAsync(httpRequest);
        var responseBody = await response.Content.ReadAsStringAsync();

        if (response.IsSuccessStatusCode)
        {
            return JsonSerializer.Deserialize<ParameterResolverResult>(responseBody, JsonOptions);
        }

        var apiException = new InvalidOperationException($"ParameterResolver API returned {(int)response.StatusCode}: {responseBody}");
        _logErrors.LogAndForget(apiException, "ParameterResolver API error");
        return null;
    }
}
