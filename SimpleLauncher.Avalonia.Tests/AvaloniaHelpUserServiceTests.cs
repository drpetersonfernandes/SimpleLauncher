using Moq;
using SimpleLauncher.Avalonia.Services;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Tests for <see cref="AvaloniaHelpUserService"/> (Phase 3). The alias-to-canonical
/// mapping is tested via text equality: an alias must resolve to the same details
/// text as its canonical system name, regardless of whether parameters.md is
/// present in the test output (both fall back identically when the file is absent).
/// </summary>
public class AvaloniaHelpUserServiceTests
{
    private static AvaloniaHelpUserService CreateService()
    {
        return new AvaloniaHelpUserService(
            new Mock<ILogger>().Object,
            TestDependencies.MessageBox().Object);
    }

    [Fact]
    public void GetHelpText_NullOrEmpty_ReturnsNoSystemNameProvided()
    {
        var service = CreateService();

        Assert.Equal("No system name provided.", service.GetHelpText(""));
        Assert.Equal("No system name provided.", service.GetHelpText(null!));
    }

    [Fact]
    public void GetHelpText_AliasResolvesToSameTextAsCanonicalName()
    {
        var service = CreateService();

        Assert.Equal(service.GetHelpText("Sony PlayStation 1"), service.GetHelpText("PSX"));
        Assert.Equal(service.GetHelpText("Sony PlayStation 1"), service.GetHelpText("PlayStation 1"));
        Assert.Equal(service.GetHelpText("Nintendo SNES"), service.GetHelpText("Super Nintendo"));
        Assert.Equal(service.GetHelpText("SNK Neo Geo"), service.GetHelpText("NeoGeo"));
        Assert.Equal(service.GetHelpText("Sega Genesis"), service.GetHelpText("Mega Drive"));
        Assert.Equal(service.GetHelpText("Microsoft Windows"), service.GetHelpText("PC"));
    }

    [Fact]
    public void GetHelpText_UnknownSystem_ReturnsNoDetailsFallback()
    {
        var service = CreateService();

        Assert.Equal("No details available for 'Unicorn 2000'.", service.GetHelpText("Unicorn 2000"));
    }

    [Fact]
    public void GetHelpText_IsCaseInsensitive()
    {
        var service = CreateService();

        Assert.Equal(service.GetHelpText("NES"), service.GetHelpText("nes"));
        Assert.Equal(service.GetHelpText("SNES"), service.GetHelpText("Snes"));
    }

    [Fact]
    public void HasSystemDetails_ReturnsFalseForUnknownSystems()
    {
        var service = CreateService();

        Assert.False(service.HasSystemDetails("Unicorn 2000"));
        Assert.False(service.HasSystemDetails(""));
        Assert.False(service.HasSystemDetails(null!));
    }

    [Fact]
    public void GetHelpText_WithLocalization_ReturnsLocalizedNoSystemNameFallback()
    {
        var localization = new SimpleLauncher.Avalonia.Services.LocalizationService();
        var service = new AvaloniaHelpUserService(
            new Mock<ILogger>().Object,
            TestDependencies.MessageBox().Object,
            localization);

        Assert.Equal("No system name provided.", service.GetHelpText(""));
    }

    [Fact]
    public void AllLocaleFiles_ContainSystemHelpKeys()
    {
        // The System Help panel (EditSystemWindow right column) needs these keys
        // in every locale; GetString falls back to English but the parity contract
        // is that translations exist for all 18 locales.
        var keys = new[]
        {
            "DeveloperSuggestion", "TooltipDeveloperSuggestionLabel",
            "Nodetailsavailablefor", "Noinformationavailableforsystem", "Nosystemnameprovided"
        };

        var resourcesDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
        var files = Directory.EnumerateFiles(resourcesDir, "strings.*.json").ToList();
        Assert.Equal(18, files.Count);

        foreach (var file in files)
        {
            var json = File.ReadAllText(file);
            foreach (var key in keys)
            {
                Assert.True(json.Contains($"\"{key}\"", StringComparison.Ordinal),
                    $"{Path.GetFileName(file)} is missing key '{key}'");
            }
        }
    }
}