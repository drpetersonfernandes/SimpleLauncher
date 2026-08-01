using Microsoft.Extensions.Configuration;
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
    private readonly ILogger _logErrors = new NoOpLogger();
    private readonly NoOpCredentialProtector _credentialProtector = new();


    /// <summary>
    /// Initializes a new instance of <see cref="EmulatorConfigInjectionExtendedTests2"/> with in-memory configuration and a temporary test directory.
    /// </summary>
    public EmulatorConfigInjectionExtendedTests2()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["Urls:YouTubeSearch"] = "https://www.youtube.com/results?search_query=",
                ["Urls:IgdbSearch"] = "https://www.igdb.com/search?q="
            })
            .Build();

        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_EmuInjectionTest2_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    /// Cleans up the test directory and restores the service provider mock.
    /// </summary>
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

    private SettingsManagerService CreateSettingsManager()
    {
        return new SettingsManagerService(_configuration, _logErrors, _credentialProtector);
    }

    // --- AresConfigurationService Tests ---

    /// <summary>
    /// Verifies that Ares InjectSettings throws when given an invalid path.
    /// </summary>
    [Fact]
    public void AresInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            AresConfigurationService.InjectSettings("", settings, Log.Logger));
    }

    // --- DolphinConfigurationService Tests ---

    /// <summary>
    /// Verifies that Dolphin InjectSettings throws when given an invalid path.
    /// </summary>
    [Fact]
    public void DolphinInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            DolphinConfigurationService.InjectSettings("", settings, Log.Logger));
    }

    // --- FlycastConfigurationService Tests ---

    /// <summary>
    /// Verifies that Flycast InjectSettings throws when given an invalid path.
    /// </summary>
    [Fact]
    public void FlycastInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            FlycastConfigurationService.InjectSettings("", settings, Log.Logger));
    }

    // --- MameConfigurationService Tests ---

    /// <summary>
    /// Verifies that MAME InjectSettings throws when given an invalid path.
    /// </summary>
    [Fact]
    public void MameInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            MameConfigurationService.InjectSettings("", settings, Log.Logger));
    }

    // --- CemuConfigurationService Tests ---

    /// <summary>
    /// Verifies that Cemu InjectSettings throws when given an invalid path.
    /// </summary>
    [Fact]
    public void CemuInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            CemuConfigurationService.InjectSettings("", settings, Log.Logger));
    }

    // --- AzaharConfigurationService Tests ---

    /// <summary>
    /// Verifies that Azahar InjectSettings throws when given an invalid path.
    /// </summary>
    [Fact]
    public void AzaharInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            AzaharConfigurationService.InjectSettings("", settings, Log.Logger));
    }

    // --- RaineConfigurationService Tests ---

    /// <summary>
    /// Verifies that Raine InjectSettings throws when given an invalid path.
    /// </summary>
    [Fact]
    public void RaineInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            RaineConfigurationService.InjectSettings("", settings, Log.Logger));
    }

    // --- SupermodelConfigurationService Tests ---

    /// <summary>
    /// Verifies that Supermodel InjectSettings throws when given an invalid path.
    /// </summary>
    [Fact]
    public void SupermodelInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            SupermodelConfigurationService.InjectSettings("", settings, Log.Logger));
    }

    // --- SegaModel2ConfigurationService Tests ---

    /// <summary>
    /// Verifies that SegaModel2 InjectSettings throws when given an invalid path.
    /// </summary>
    [Fact]
    public void SegaModel2InjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            SegaModel2ConfigurationService.InjectSettings("", settings, Log.Logger));
    }

    // --- YumirConfigurationService Tests ---

    /// <summary>
    /// Verifies that Yumir InjectSettings throws when given an invalid path.
    /// </summary>
    [Fact]
    public void YumirInjectSettingsInvalidPathThrowsException()
    {
        var settings = CreateSettingsManager();
        Assert.ThrowsAny<Exception>(() =>
            YumirConfigurationService.InjectSettings("", settings, Log.Logger));
    }

    // --- EmulatorPathResolver Tests ---

    /// <summary>
    /// Verifies that EmulatorPathResolver returns null for a null hint.
    /// </summary>
    [Fact]
    public void EmulatorPathResolverNullHintReturnsNull()
    {
        var result = EmulatorPathResolver.TryFindEmulatorPath(null!, _logErrors);
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that EmulatorPathResolver returns null for an empty hint.
    /// </summary>
    [Fact]
    public void EmulatorPathResolverEmptyHintReturnsNull()
    {
        var result = EmulatorPathResolver.TryFindEmulatorPath("", _logErrors);
        Assert.Null(result);
    }

    /// <summary>
    /// Verifies that EmulatorPathResolver returns null for a whitespace-only hint.
    /// </summary>
    [Fact]
    public void EmulatorPathResolverWhitespaceHintReturnsNull()
    {
        var result = EmulatorPathResolver.TryFindEmulatorPath("   ", _logErrors);
        Assert.Null(result);
    }
}
