using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Core.Services.SettingsManager;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class DaphneConfigurationServiceTests : IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors = new NoOpLogErrors();
    private readonly NoOpCredentialProtector _credentialProtector = new();

    public DaphneConfigurationServiceTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Urls:YouTubeSearch"] = "https://www.youtube.com/results?search_query=",
                ["Urls:IgdbSearch"] = "https://www.igdb.com/search?q="
            })
            .Build();

        ServiceProviderMock.Install();
    }

    public void Dispose()
    {
        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    private SettingsManager CreateSettingsManager()
    {
        return new SettingsManager(_configuration, _logErrors, _credentialProtector);
    }

    [Fact]
    public void BuildArguments_NullSettings_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(static () => DaphneConfigurationService.BuildArguments(null));
    }

    [Fact]
    public void BuildArguments_DefaultSettings_ContainsUseOverlays()
    {
        var settings = CreateSettingsManager();
        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.Contains("-use_overlays", args);
    }

    [Fact]
    public void BuildArguments_FullscreenEnabled_ContainsFullscreenFlag()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.Fullscreen = true;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.Contains("-fullscreen", args);
    }

    [Fact]
    public void BuildArguments_FullscreenDisabled_OmitsFullscreenFlag()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.Fullscreen = false;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.DoesNotContain("-fullscreen", args);
    }

    [Fact]
    public void BuildArguments_ValidResolution_ContainsXYArgs()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.ResX = 1920;
        settings.Daphne.ResY = 1080;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.Contains("-x 1920", args);
        Assert.Contains("-y 1080", args);
    }

    [Fact]
    public void BuildArguments_ResolutionBelowMin_OmitsResolutionArgs()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.ResX = 100; // Below 640 minimum
        settings.Daphne.ResY = 100; // Below 480 minimum

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.DoesNotContain("-x", args);
        Assert.DoesNotContain("-y", args);
    }

    [Fact]
    public void BuildArguments_ResolutionAboveMax_OmitsResolutionArgs()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.ResX = 9999; // Above 7680 maximum
        settings.Daphne.ResY = 9999; // Above 4320 maximum

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.DoesNotContain("-x", args);
        Assert.DoesNotContain("-y", args);
    }

    [Fact]
    public void BuildArguments_CrosshairsDisabled_ContainsNoCrosshairsFlag()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.DisableCrosshairs = true;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.Contains("-nocrosshairs", args);
    }

    [Fact]
    public void BuildArguments_CrosshairsEnabled_OmitsNoCrosshairsFlag()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.DisableCrosshairs = false;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.DoesNotContain("-nocrosshairs", args);
    }

    [Fact]
    public void BuildArguments_BilinearDisabled_ContainsNoLinearScaleFlag()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.Bilinear = false;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.Contains("-nolinear_scale", args);
    }

    [Fact]
    public void BuildArguments_BilinearEnabled_OmitsNoLinearScaleFlag()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.Bilinear = true;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.DoesNotContain("-nolinear_scale", args);
    }

    [Fact]
    public void BuildArguments_SoundDisabled_ContainsNoSoundFlag()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.EnableSound = false;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.Contains("-nosound", args);
    }

    [Fact]
    public void BuildArguments_SoundEnabled_OmitsNoSoundFlag()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.EnableSound = true;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.DoesNotContain("-nosound", args);
    }

    [Fact]
    public void BuildArguments_OverlaysEnabled_ContainsUseOverlays1()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.UseOverlays = true;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.Contains("-use_overlays 1", args);
    }

    [Fact]
    public void BuildArguments_OverlaysDisabled_ContainsUseOverlays0()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.UseOverlays = false;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.Contains("-use_overlays 0", args);
    }

    [Fact]
    public void BuildArguments_AllOptionsEnabled_ContainsAllFlags()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.Fullscreen = true;
        settings.Daphne.ResX = 1920;
        settings.Daphne.ResY = 1080;
        settings.Daphne.DisableCrosshairs = true;
        settings.Daphne.Bilinear = false;
        settings.Daphne.EnableSound = false;
        settings.Daphne.UseOverlays = true;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.Contains("-fullscreen", args);
        Assert.Contains("-x 1920", args);
        Assert.Contains("-y 1080", args);
        Assert.Contains("-nocrosshairs", args);
        Assert.Contains("-nolinear_scale", args);
        Assert.Contains("-nosound", args);
        Assert.Contains("-use_overlays 1", args);
    }

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }
}
