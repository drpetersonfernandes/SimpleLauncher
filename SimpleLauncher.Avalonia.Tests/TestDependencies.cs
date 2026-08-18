using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.PlaySound;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Common mock/builders for ViewModel tests. All dependencies are isolated mocks
/// (no real I/O), except SettingsManagerService / PlaySoundEffects which are real
/// classes constructed with mocked collaborators.
/// </summary>
internal static class TestDependencies
{
    public static Mock<ILogger> Logger()
    {
        return new Mock<ILogger>();
    }

    public static Mock<IMessageBoxLibraryService> MessageBox()
    {
        return new Mock<IMessageBoxLibraryService>();
    }

    public static Mock<ICredentialProtector> CredentialProtector()
    {
        return new Mock<ICredentialProtector>();
    }

    /// <summary>
    /// IResourceProvider mock that returns the supplied fallback (default-value) string,
    /// mirroring the app behavior when a key is missing.
    /// </summary>
    public static Mock<IResourceProvider> ResourceProvider()
    {
        var mock = new Mock<IResourceProvider>();
        mock.Setup(r => r.GetString(It.IsAny<string>(), It.IsAny<string>()))
            .Returns<string, string>((_, fallback) => fallback);
        return mock;
    }

    public static SettingsManagerService Settings(IConfiguration? configuration = null, Mock<IMessageBoxLibraryService>? messageBox = null)
    {
        TestEnvironment.EnsurePortableSettings();
        return new SettingsManagerService(
            configuration ?? new ConfigurationBuilder().Build(),
            Logger().Object,
            CredentialProtector().Object,
            (messageBox ?? MessageBox()).Object);
    }

    public static PlaySoundEffects PlaySound(SettingsManagerService settings)
    {
        return new PlaySoundEffects(settings, Logger().Object);
    }

    public static HttpClient HttpClientWith(Func<HttpRequestMessage, HttpResponseMessage> responder)
    {
        return new HttpClient(new FakeMessageHandler(responder));
    }

    public static Mock<IHttpClientFactory> HttpFactory(HttpClient client)
    {
        var factory = new Mock<IHttpClientFactory>();
        factory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(client);
        return factory;
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