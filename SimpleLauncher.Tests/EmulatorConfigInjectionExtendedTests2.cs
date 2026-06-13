using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for emulator configuration injection services that are not covered by EmulatorConfigInjectionTests.
/// </summary>
public class EmulatorConfigInjectionExtendedTests2 : IDisposable
{
    private readonly string _testDirectory;
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors = new NoOpLogErrors();
    private readonly NoOpCredentialProtector _credentialProtector = new();

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }

    public EmulatorConfigInjectionExtendedTests2()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Urls:YouTubeSearch"] = "https://www.youtube.com/results?search_query=",
                ["Urls:IgdbSearch"] = "https://www.igdb.com/search?q="
            })
            .Build();

        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_EmuInjectionTest2_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
        }
        catch
        {
            // Best-effort cleanup
        }

        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    private SettingsManager CreateSettingsManager()
    {
        return new SettingsManager(_configuration, _logErrors, _credentialProtector);
    }

    private static string FakeEmulatorExePath(string emuDir)
    {
        return Path.Combine(emuDir, "emulator.exe");
    }

    // --- AresConfigurationService Tests ---

    [Fact]
    public void AresInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            AresConfigurationService.InjectSettings("", settings, _logErrors, new NoOpDebugLogger()));
    }

    // --- DolphinConfigurationService Tests ---

    [Fact]
    public void DolphinInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            DolphinConfigurationService.InjectSettings("", settings, _logErrors, new NoOpDebugLogger()));
    }

    // --- FlycastConfigurationService Tests ---

    [Fact]
    public void FlycastInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            FlycastConfigurationService.InjectSettings("", settings, _logErrors, new NoOpDebugLogger()));
    }

    // --- MameConfigurationService Tests ---

    [Fact]
    public void MameInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            MameConfigurationService.InjectSettings("", settings, _logErrors, new NoOpDebugLogger()));
    }

    // --- CemuConfigurationService Tests ---

    [Fact]
    public void CemuInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            CemuConfigurationService.InjectSettings("", settings, _logErrors, new NoOpDebugLogger()));
    }

    // --- AzaharConfigurationService Tests ---

    [Fact]
    public void AzaharInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            AzaharConfigurationService.InjectSettings("", settings, _logErrors, new NoOpDebugLogger()));
    }

    // --- RaineConfigurationService Tests ---

    [Fact]
    public void RaineInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            RaineConfigurationService.InjectSettings("", settings, _logErrors, new NoOpDebugLogger()));
    }

    // --- SupermodelConfigurationService Tests ---

    [Fact]
    public void SupermodelInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            SupermodelConfigurationService.InjectSettings("", settings, _logErrors, new NoOpDebugLogger()));
    }

    // --- SegaModel2ConfigurationService Tests ---

    [Fact]
    public void SegaModel2InjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            SegaModel2ConfigurationService.InjectSettings("", settings, _logErrors, new NoOpDebugLogger()));
    }

    // --- YumirConfigurationService Tests ---

    [Fact]
    public void YumirInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            YumirConfigurationService.InjectSettings("", settings, _logErrors, new NoOpDebugLogger()));
    }

    // --- EmulatorPathResolver Tests ---

    [Fact]
    public void EmulatorPathResolverNullHintReturnsNull()
    {
        var result = EmulatorPathResolver.TryFindEmulatorPath(null, _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void EmulatorPathResolverEmptyHintReturnsNull()
    {
        var result = EmulatorPathResolver.TryFindEmulatorPath("", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void EmulatorPathResolverWhitespaceHintReturnsNull()
    {
        var result = EmulatorPathResolver.TryFindEmulatorPath("   ", _logErrors);
        Assert.Null(result);
    }
}
