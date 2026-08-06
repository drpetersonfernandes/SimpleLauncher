using System.Net;
using System.Text;
using System.Text.Json;
using Moq;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.ParameterResolver;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="ParameterResolverService"/> using a fake <see cref="HttpMessageHandler"/>
/// so no real network access is required.
/// </summary>
public class ParameterResolverServiceTests
{
    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public FakeHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }

    private sealed class FakeHttpClientFactory(HttpMessageHandler handler) : IHttpClientFactory
    {
        // The real app registers the client with a BaseAddress; the service sends relative URIs
        public HttpClient CreateClient(string name) => new(handler) { BaseAddress = new Uri("http://localhost") };
    }

    private static ParameterResolverRequest CreateRequest() => new()
    {
        SystemName = "NES",
        SystemFolder = "C:\\roms",
        FileFormatsToSearch = ["zip"],
        ExtractFileBeforeLaunch = true,
        FileFormatsToLaunch = ["nes"],
        GroupByFolder = false
    };

    private static ParameterResolverService CreateService(HttpMessageHandler handler) =>
        new(new FakeHttpClientFactory(handler), new NoOpLogger());

    [Fact]
    public async Task ResolveParametersAsync_Success_ReturnsDeserializedResult()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"suggestedParameter":"-fullscreen","explanation":"Fullscreen is recommended"}""", Encoding.UTF8, "application/json")
        });
        var service = CreateService(handler);

        var result = await service.ResolveParametersAsync(CreateRequest());

        Assert.NotNull(result);
        Assert.Equal("-fullscreen", result!.SuggestedParameter);
        Assert.Equal("Fullscreen is recommended", result.Explanation);
    }

    [Fact]
    public async Task ResolveParametersAsync_NonSuccessStatus_ReturnsNull()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent("server exploded")
        });
        var service = CreateService(handler);

        var result = await service.ResolveParametersAsync(CreateRequest());

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveParametersAsync_MalformedJson_ReturnsNull()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("this is not json")
        });
        var service = CreateService(handler);

        var result = await service.ResolveParametersAsync(CreateRequest());

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveParametersAsync_SendsExpectedRequest()
    {
        HttpRequestMessage? capturedRequest = null;
        string? requestBody = null;
        var handler = new FakeHttpMessageHandler(request =>
        {
            capturedRequest = request;
            requestBody = request.Content!.ReadAsStringAsync().GetAwaiter().GetResult();
            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"suggestedParameter":"-x","explanation":""}""", Encoding.UTF8, "application/json")
            };
        });
        var service = CreateService(handler);

        await service.ResolveParametersAsync(CreateRequest());

        Assert.NotNull(capturedRequest);
        Assert.Equal(HttpMethod.Post, capturedRequest!.Method);
        Assert.Equal("/api/ParameterResolver/resolve", capturedRequest.RequestUri!.AbsolutePath);
        Assert.True(capturedRequest.Headers.Contains("X-Api-Key"));

        var json = JsonDocument.Parse(requestBody!);
        Assert.Equal("NES", json.RootElement.GetProperty("systemName").GetString());
        Assert.Equal("C:\\roms", json.RootElement.GetProperty("systemFolder").GetString());
        Assert.Equal("zip", json.RootElement.GetProperty("fileFormatsToSearch")[0].GetString());
        Assert.True(json.RootElement.GetProperty("extractFileBeforeLaunch").GetBoolean());
    }

    [Fact]
    public async Task ResolveParametersAsync_HttpRequestException_PropagatesToCaller()
    {
        var handler = new FakeHttpMessageHandler(_ => throw new HttpRequestException("connection refused"));
        var service = CreateService(handler);

        await Assert.ThrowsAsync<HttpRequestException>(() => service.ResolveParametersAsync(CreateRequest()));
    }

    [Fact]
    public async Task ResolveParametersAsync_NullLogger_DoesNotThrowOnSuccess()
    {
        var handler = new FakeHttpMessageHandler(_ => new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"suggestedParameter":"-x","explanation":""}""", Encoding.UTF8, "application/json")
        });
        var service = new ParameterResolverService(new FakeHttpClientFactory(handler), null!);

        var result = await service.ResolveParametersAsync(CreateRequest());

        Assert.NotNull(result);
    }
}
