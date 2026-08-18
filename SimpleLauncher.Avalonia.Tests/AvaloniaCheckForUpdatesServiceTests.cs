using System.Text;
using System.Text.Json;
using Moq;
using SimpleLauncher.Avalonia.Services;
using SimpleLauncher.Core.Interfaces;
using CoreMessageBoxResult = SimpleLauncher.Core.Models.MessageBoxResult;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Tests for <see cref="AvaloniaCheckForUpdatesService"/> (Phase 3 service).
/// All HTTP traffic is served by a fake HttpMessageHandler, so no live endpoints are hit.
/// </summary>
public class AvaloniaCheckForUpdatesServiceTests
{
    private static string GitHubReleaseJson(string versionTag, string assetName)
    {
        var assetUrl = $"https://github.com/drpetersonfernandes/SimpleLauncher/releases/download/{versionTag}/{assetName}";
        return JsonSerializer.Serialize(new
        {
            tag_name = versionTag,
            assets = new[] { new { name = assetName, browser_download_url = assetUrl } }
        });
    }

    private static (AvaloniaCheckForUpdatesService Service, Mock<IMessageBoxLibraryService> MessageBox, Mock<ILogger> Logger)
        CreateService(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        var handler = new FakeMessageHandler(responder);
        var httpClient = new HttpClient(handler);

        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var messageBox = new Mock<IMessageBoxLibraryService>();
        var logger = new Mock<ILogger>();

        var service = new AvaloniaCheckForUpdatesService(httpClientFactory.Object, messageBox.Object, logger.Object);
        return (service, messageBox, logger);
    }

    private static HttpResponseMessage Json(string content, int status = 200)
    {
        return new HttpResponseMessage((System.Net.HttpStatusCode)status)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        };
    }

    private static HttpResponseMessage Text(string content, int status = 200)
    {
        return new HttpResponseMessage((System.Net.HttpStatusCode)status)
        {
            Content = new StringContent(content, Encoding.UTF8, "text/plain")
        };
    }

    [Fact]
    public async Task ManualCheck_NewerVersionOnGitHub_UserAccepts_GuidesToManualDownload()
    {
        var (service, messageBox, _) = CreateService(request =>
            request.RequestUri!.Host.StartsWith("api.github.com", StringComparison.Ordinal)
                ? Json(GitHubReleaseJson("v9.9.9", "release_9.9.9_win-x64.zip"))
                : throw new InvalidOperationException("Unexpected request: " + request.RequestUri));

        messageBox.Setup(m => m.DoYouWantToUpdateMessageBoxAsync(It.IsAny<string>(), "9.9.9.0"))
            .ReturnsAsync(CoreMessageBoxResult.Yes);

        await service.ManualCheckForUpdatesAsync(owner: null);

        messageBox.Verify(m => m.DoYouWantToUpdateMessageBoxAsync(It.IsAny<string>(), "9.9.9.0"), Times.Once);
        messageBox.Verify(m => m.InstallUpdateManuallyMessageBoxAsync(), Times.Once);
        messageBox.Verify(m => m.ThereIsNoUpdateAvailableMessageBoxAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ManualCheck_NewerVersionOnGitHub_UserDeclines_NoManualDownload()
    {
        var (service, messageBox, _) = CreateService(request =>
            Json(GitHubReleaseJson("v9.9.9", "release_9.9.9_win-x64.zip")));

        messageBox.Setup(m => m.DoYouWantToUpdateMessageBoxAsync(It.IsAny<string>(), "9.9.9.0"))
            .ReturnsAsync(CoreMessageBoxResult.No);

        await service.ManualCheckForUpdatesAsync(owner: null);

        messageBox.Verify(m => m.DoYouWantToUpdateMessageBoxAsync(It.IsAny<string>(), "9.9.9.0"), Times.Once);
        messageBox.Verify(m => m.InstallUpdateManuallyMessageBoxAsync(), Times.Never);
    }

    [Fact]
    public async Task ManualCheck_CurrentVersionUpToDate_ShowsNoUpdateAvailable()
    {
        var (service, messageBox, _) = CreateService(request =>
            Json(GitHubReleaseJson("v5.6.1", "release_5.6.1_win-x64.zip")));

        await service.ManualCheckForUpdatesAsync(owner: null);

        messageBox.Verify(m => m.ThereIsNoUpdateAvailableMessageBoxAsync(It.IsAny<string>()), Times.Once);
        messageBox.Verify(m => m.DoYouWantToUpdateMessageBoxAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ManualCheck_GitHubUnavailable_FallsBackToSecondaryServer()
    {
        var (service, messageBox, _) = CreateService(request =>
        {
            var uri = request.RequestUri!;
            if (uri.Host.StartsWith("api.github.com", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);
            }

            if (uri.Host.StartsWith("assets.purelogiccode.com", StringComparison.Ordinal))
            {
                return Text("Version 9.9.9");
            }

            throw new InvalidOperationException("Unexpected request: " + uri);
        });

        messageBox.Setup(m => m.DoYouWantToUpdateMessageBoxAsync(It.IsAny<string>(), "9.9.9.0"))
            .ReturnsAsync(CoreMessageBoxResult.Yes);

        await service.ManualCheckForUpdatesAsync(owner: null);

        messageBox.Verify(m => m.DoYouWantToUpdateMessageBoxAsync(It.IsAny<string>(), "9.9.9.0"), Times.Once);
        messageBox.Verify(m => m.ErrorCheckingForUpdatesMessageBoxAsync(), Times.Never);
    }

    [Fact]
    public async Task ManualCheck_GitHubThrows_FallsBackToSecondaryServer()
    {
        var (service, messageBox, _) = CreateService(request =>
        {
            var uri = request.RequestUri!;
            if (uri.Host.StartsWith("api.github.com", StringComparison.Ordinal))
            {
                throw new HttpRequestException("network down");
            }

            if (uri.Host.StartsWith("assets.purelogiccode.com", StringComparison.Ordinal))
            {
                return Text("Version 9.9.9");
            }

            throw new InvalidOperationException("Unexpected request: " + uri);
        });

        messageBox.Setup(m => m.DoYouWantToUpdateMessageBoxAsync(It.IsAny<string>(), "9.9.9.0"))
            .ReturnsAsync(CoreMessageBoxResult.Yes);

        await service.ManualCheckForUpdatesAsync(owner: null);

        messageBox.Verify(m => m.DoYouWantToUpdateMessageBoxAsync(It.IsAny<string>(), "9.9.9.0"), Times.Once);
    }

    [Fact]
    public async Task ManualCheck_AllSourcesUnreachable_ShowsError()
    {
        var (service, messageBox, _) = CreateService(_ =>
            new HttpResponseMessage(System.Net.HttpStatusCode.NotFound));

        await service.ManualCheckForUpdatesAsync(owner: null);

        messageBox.Verify(m => m.ErrorCheckingForUpdatesMessageBoxAsync(), Times.Once);
        messageBox.Verify(m => m.DoYouWantToUpdateMessageBoxAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task ManualCheck_InvalidVersionOnSecondaryServer_ShowsError()
    {
        var (service, messageBox, _) = CreateService(request =>
            request.RequestUri!.Host.StartsWith("api.github.com", StringComparison.Ordinal)
                ? new HttpResponseMessage(System.Net.HttpStatusCode.NotFound)
                : Text("no version information here"));

        await service.ManualCheckForUpdatesAsync(owner: null);

        messageBox.Verify(m => m.ErrorCheckingForUpdatesMessageBoxAsync(), Times.Once);
    }

    [Fact]
    public async Task ManualCheck_GitHubResponseMissingVersion_ShowsError()
    {
        var (service, messageBox, _) = CreateService(_ =>
            Json("""{"message": "no tag_name here"}"""));

        await service.ManualCheckForUpdatesAsync(owner: null);

        messageBox.Verify(m => m.ErrorCheckingForUpdatesMessageBoxAsync(), Times.Once);
    }

    private sealed class FakeMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _responder;

        public FakeMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> responder)
        {
            _responder = responder;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_responder(request));
        }
    }
}