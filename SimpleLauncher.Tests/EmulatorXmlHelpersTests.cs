using System.Globalization;
using System.Xml.Linq;
using SimpleLauncher.Core.Services.SettingsManager;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="EmulatorXmlHelpers"/> typed XML reading with fallback locations and defaults.
/// </summary>
public class EmulatorXmlHelpersTests
{
    private static readonly XElement Root = new("Config",
        new XElement("SystemConfig",
            new XElement("Volume", "0.5"),
            new XElement("Fullscreen", "true"),
            new XElement("MaxPlayers", "4"),
            new XElement("RomPath", "C:\\roms")),
        // Flattened fallback location used when the section does not contain the property
        new XElement("SystemConfigVolume", "0.75"),
        new XElement("SystemConfigFullscreen", "false"),
        new XElement("SystemConfigMaxPlayers", "2"),
        new XElement("SystemConfigRomPath", "D:\\fallback"));

    private static XElement Section => Root.Element("SystemConfig")!;

    [Fact]
    public void ReadBool_SectionValueWins()
    {
        var result = EmulatorXmlHelpers.ReadBool(Section, "SystemConfig", Root, "Fullscreen", fallback: false);
        Assert.True(result);
    }

    [Fact]
    public void ReadBool_FallsBackToFlattenedRootElement()
    {
        var sectionWithoutProperty = new XElement("SystemConfig");
        var result = EmulatorXmlHelpers.ReadBool(sectionWithoutProperty, "SystemConfig", Root, "Fullscreen", fallback: false);
        Assert.False(result);
    }

    [Fact]
    public void ReadBool_InvalidValueUsesFallback()
    {
        var root = new XElement("Config", new XElement("SystemConfigFullscreen", "not-a-bool"));
        var result = EmulatorXmlHelpers.ReadBool(null, "SystemConfig", root, "Fullscreen", fallback: true);
        Assert.True(result);
    }

    [Fact]
    public void ReadBool_MissingEverywhereUsesFallback()
    {
        var root = new XElement("Config");
        var result = EmulatorXmlHelpers.ReadBool(null, "SystemConfig", root, "Fullscreen", fallback: true);
        Assert.True(result);
    }

    [Fact]
    public void ReadInt_SectionValueWins()
    {
        var result = EmulatorXmlHelpers.ReadInt(Section, "SystemConfig", Root, "MaxPlayers", fallback: 0);
        Assert.Equal(4, result);
    }

    [Fact]
    public void ReadInt_FallsBackToFlattenedRootElement()
    {
        var result = EmulatorXmlHelpers.ReadInt(null, "SystemConfig", Root, "MaxPlayers", fallback: 0);
        Assert.Equal(2, result);
    }

    [Fact]
    public void ReadInt_InvalidValueUsesFallback()
    {
        var root = new XElement("Config", new XElement("SystemConfigMaxPlayers", "many"));
        var result = EmulatorXmlHelpers.ReadInt(null, "SystemConfig", root, "MaxPlayers", fallback: 8);
        Assert.Equal(8, result);
    }

    [Fact]
    public void ReadDouble_SectionValueParsedWithInvariantCulture()
    {
        var section = new XElement("SystemConfig", new XElement("Volume", "0.5"));
        var result = EmulatorXmlHelpers.ReadDouble(section, "SystemConfig", Root, "Volume", fallback: 0);
        Assert.Equal(0.5, result, precision: 10);
    }

    [Fact]
    public void ReadDouble_InvalidValueUsesFallback()
    {
        var section = new XElement("SystemConfig", new XElement("Volume", "loud"));
        var result = EmulatorXmlHelpers.ReadDouble(section, "SystemConfig", Root, "Volume", fallback: 1.0);
        Assert.Equal(1.0, result);
    }

    [Fact]
    public void ReadString_SectionValueWins()
    {
        var result = EmulatorXmlHelpers.ReadString(Section, "SystemConfig", Root, "RomPath", fallback: "");
        Assert.Equal("C:\\roms", result);
    }

    [Fact]
    public void ReadString_FallsBackToFlattenedRootElement()
    {
        var result = EmulatorXmlHelpers.ReadString(null, "SystemConfig", Root, "RomPath", fallback: "");
        Assert.Equal("D:\\fallback", result);
    }

    [Fact]
    public void ReadString_MissingEverywhereUsesFallback()
    {
        var result = EmulatorXmlHelpers.ReadString(null, "SystemConfig", Root, "DoesNotExist", fallback: "default");
        Assert.Equal("default", result);
    }

    [Fact]
    public void ReadString_NullSectionAndMissingRootElementUsesFallback()
    {
        var root = new XElement("Config");
        var result = EmulatorXmlHelpers.ReadString(null, "SystemConfig", root, "RomPath", fallback: "fallback-path");
        Assert.Equal("fallback-path", result);
    }

    [Fact]
    public void ReadInt_InvariantCultureParsing_DoesNotDependOnCurrentCulture()
    {
        var originalCulture = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            var section = new XElement("SystemConfig", new XElement("MaxPlayers", "4"));
            var result = EmulatorXmlHelpers.ReadInt(section, "SystemConfig", Root, "MaxPlayers", fallback: 0);
            Assert.Equal(4, result);
        }
        finally
        {
            CultureInfo.CurrentCulture = originalCulture;
        }
    }
}
