using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

using Interfaces;

/// <summary>
/// Tests the <see cref="DaphneConfigurationService"/> command-line argument builder for the Daphne emulator.
/// </summary>
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

    /// <summary>
    /// Verifies that passing null settings to BuildArguments throws ArgumentNullException.
    /// </summary>
    [Fact]
    public void BuildArgumentsNullSettingsThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(static () => DaphneConfigurationService.BuildArguments(null));
    }

    /// <summary>
    /// Verifies that default settings produce arguments containing the -use_overlays flag.
    /// </summary>
    [Fact]
    public void BuildArgumentsDefaultSettingsContainsUseOverlays()
    {
        var settings = CreateSettingsManager();
        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.Contains("-use_overlays", args);
    }

    /// <summary>
    /// Verifies that enabling fullscreen produces arguments containing the -fullscreen flag.
    /// </summary>
    [Fact]
    public void BuildArgumentsFullscreenEnabledContainsFullscreenFlag()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.Fullscreen = true;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.Contains("-fullscreen", args);
    }

    /// <summary>
    /// Verifies that disabling fullscreen omits the -fullscreen flag from arguments.
    /// </summary>
    [Fact]
    public void BuildArgumentsFullscreenDisabledOmitsFullscreenFlag()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.Fullscreen = false;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.DoesNotContain("-fullscreen", args);
    }

    /// <summary>
    /// Verifies that valid resolution values produce -x and -y arguments.
    /// </summary>
    [Fact]
    public void BuildArgumentsValidResolutionContainsXyArgs()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.ResX = 1920;
        settings.Daphne.ResY = 1080;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.Contains("-x 1920", args);
        Assert.Contains("-y 1080", args);
    }

    /// <summary>
    /// Verifies that resolution values below the minimum threshold omit -x and -y arguments.
    /// </summary>
    [Fact]
    public void BuildArgumentsResolutionBelowMinOmitsResolutionArgs()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.ResX = 100; // Below 640 minimum
        settings.Daphne.ResY = 100; // Below 480 minimum

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.DoesNotContain("-x", args);
        Assert.DoesNotContain("-y", args);
    }

    /// <summary>
    /// Verifies that resolution values above the maximum threshold omit -x and -y arguments.
    /// </summary>
    [Fact]
    public void BuildArgumentsResolutionAboveMaxOmitsResolutionArgs()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.ResX = 9999; // Above 7680 maximum
        settings.Daphne.ResY = 9999; // Above 4320 maximum

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.DoesNotContain("-x", args);
        Assert.DoesNotContain("-y", args);
    }

    [Fact]
    public void BuildArgumentsCrosshairsDisabledContainsNoCrosshairsFlag()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.DisableCrosshairs = true;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.Contains("-nocrosshairs", args);
    }

    /// <summary>
    /// Verifies that enabling crosshairs omits the -nocrosshairs flag from arguments.
    /// </summary>
    [Fact]
    public void BuildArgumentsCrosshairsEnabledOmitsNoCrosshairsFlag()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.DisableCrosshairs = false;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.DoesNotContain("-nocrosshairs", args);
    }

    /// <summary>
    /// Verifies that disabling bilinear filtering produces arguments containing -nolinear_scale.
    /// </summary>
    [Fact]
    public void BuildArgumentsBilinearDisabledContainsNoLinearScaleFlag()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.Bilinear = false;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.Contains("-nolinear_scale", args);
    }

    /// <summary>
    /// Verifies that enabling bilinear filtering omits the -nolinear_scale flag from arguments.
    /// </summary>
    [Fact]
    public void BuildArgumentsBilinearEnabledOmitsNoLinearScaleFlag()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.Bilinear = true;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.DoesNotContain("-nolinear_scale", args);
    }

    /// <summary>
    /// Verifies that disabling sound produces arguments containing -nosound.
    /// </summary>
    [Fact]
    public void BuildArgumentsSoundDisabledContainsNoSoundFlag()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.EnableSound = false;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.Contains("-nosound", args);
    }

    /// <summary>
    /// Verifies that enabling sound omits the -nosound flag from arguments.
    /// </summary>
    [Fact]
    public void BuildArgumentsSoundEnabledOmitsNoSoundFlag()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.EnableSound = true;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.DoesNotContain("-nosound", args);
    }

    /// <summary>
    /// Verifies that enabling overlays produces arguments containing -use_overlays 1.
    /// </summary>
    [Fact]
    public void BuildArgumentsOverlaysEnabledContainsUseOverlays1()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.UseOverlays = true;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.Contains("-use_overlays 1", args);
    }

    /// <summary>
    /// Verifies that disabling overlays produces arguments containing -use_overlays 0.
    /// </summary>
    [Fact]
    public void BuildArgumentsOverlaysDisabledContainsUseOverlays0()
    {
        var settings = CreateSettingsManager();
        settings.Daphne.UseOverlays = false;

        var args = DaphneConfigurationService.BuildArguments(settings);

        Assert.Contains("-use_overlays 0", args);
    }

    /// <summary>
    /// Verifies that all options enabled produce arguments containing every expected flag.
    /// </summary>
    [Fact]
    public void BuildArgumentsAllOptionsEnabledContainsAllFlags()
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
