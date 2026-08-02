using SimpleLauncher.Core.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="ParameterResolverRequest"/> model covering default values,
/// property assignment, and collection initialization.
/// </summary>
public class ParameterResolverRequestTests
{
    /// <summary>
    /// Verifies that the default SystemName is an empty string.
    /// </summary>
    [Fact]
    public void DefaultSystemNameIsEmpty()
    {
        var request = new ParameterResolverRequest();
        Assert.Equal("", request.SystemName);
    }

    /// <summary>
    /// Verifies that the default SystemFolder is an empty string.
    /// </summary>
    [Fact]
    public void DefaultSystemFolderIsEmpty()
    {
        var request = new ParameterResolverRequest();
        Assert.Equal("", request.SystemFolder);
    }

    /// <summary>
    /// Verifies that the default EmulatorName is an empty string.
    /// </summary>
    [Fact]
    public void DefaultEmulatorNameIsEmpty()
    {
        var request = new ParameterResolverRequest();
        Assert.Equal("", request.EmulatorName);
    }

    /// <summary>
    /// Verifies that the default EmulatorPath is an empty string.
    /// </summary>
    [Fact]
    public void DefaultEmulatorPathIsEmpty()
    {
        var request = new ParameterResolverRequest();
        Assert.Equal("", request.EmulatorPath);
    }

    /// <summary>
    /// Verifies that the default CurrentParameters is an empty string.
    /// </summary>
    [Fact]
    public void DefaultCurrentParametersIsEmpty()
    {
        var request = new ParameterResolverRequest();
        Assert.Equal("", request.CurrentParameters);
    }

    /// <summary>
    /// Verifies that the default FileFormatsToSearch is an empty list.
    /// </summary>
    [Fact]
    public void DefaultFileFormatsToSearchIsEmptyList()
    {
        var request = new ParameterResolverRequest();
        Assert.NotNull(request.FileFormatsToSearch);
        Assert.Empty(request.FileFormatsToSearch);
    }

    /// <summary>
    /// Verifies that the default FileFormatsToLaunch is an empty list.
    /// </summary>
    [Fact]
    public void DefaultFileFormatsToLaunchIsEmptyList()
    {
        var request = new ParameterResolverRequest();
        Assert.NotNull(request.FileFormatsToLaunch);
        Assert.Empty(request.FileFormatsToLaunch);
    }

    /// <summary>
    /// Verifies that the default ExtractFileBeforeLaunch is false.
    /// </summary>
    [Fact]
    public void DefaultExtractFileBeforeLaunchIsFalse()
    {
        var request = new ParameterResolverRequest();
        Assert.False(request.ExtractFileBeforeLaunch);
    }

    /// <summary>
    /// Verifies that the default GroupByFolder is false.
    /// </summary>
    [Fact]
    public void DefaultGroupByFolderIsFalse()
    {
        var request = new ParameterResolverRequest();
        Assert.False(request.GroupByFolder);
    }

    /// <summary>
    /// Verifies that the default DisableRecursiveSearch is false.
    /// </summary>
    [Fact]
    public void DefaultDisableRecursiveSearchIsFalse()
    {
        var request = new ParameterResolverRequest();
        Assert.False(request.DisableRecursiveSearch);
    }

    /// <summary>
    /// Verifies that all properties can be set and retrieved correctly via object initializer.
    /// </summary>
    [Fact]
    public void AllPropertiesCanBeSet()
    {
        var request = new ParameterResolverRequest
        {
            SystemName = "NES",
            SystemFolder = @"C:\roms\NES",
            FileFormatsToSearch = ["zip", "nes"],
            ExtractFileBeforeLaunch = true,
            FileFormatsToLaunch = ["nes"],
            GroupByFolder = true,
            DisableRecursiveSearch = true,
            EmulatorName = "RetroArch",
            EmulatorPath = @"C:\emulators\retroarch.exe",
            CurrentParameters = "-L core.dll"
        };

        Assert.Equal("NES", request.SystemName);
        Assert.Equal(@"C:\roms\NES", request.SystemFolder);
        Assert.Equal(["zip", "nes"], request.FileFormatsToSearch);
        Assert.True(request.ExtractFileBeforeLaunch);
        Assert.Equal(["nes"], request.FileFormatsToLaunch);
        Assert.True(request.GroupByFolder);
        Assert.True(request.DisableRecursiveSearch);
        Assert.Equal("RetroArch", request.EmulatorName);
        Assert.Equal(@"C:\emulators\retroarch.exe", request.EmulatorPath);
        Assert.Equal("-L core.dll", request.CurrentParameters);
    }

    /// <summary>
    /// Verifies that items can be added to the FileFormatsToSearch collection.
    /// </summary>
    [Fact]
    public void FileFormatsToSearchCanAddItems()
    {
        var request = new ParameterResolverRequest();
        request.FileFormatsToSearch.Add("zip");
        request.FileFormatsToSearch.Add("nes");

        Assert.Equal(2, request.FileFormatsToSearch.Count);
        Assert.Contains("zip", request.FileFormatsToSearch, StringComparer.Ordinal);
        Assert.Contains("nes", request.FileFormatsToSearch, StringComparer.Ordinal);
    }

    /// <summary>
    /// Verifies that items can be added to the FileFormatsToLaunch collection.
    /// </summary>
    [Fact]
    public void FileFormatsToLaunchCanAddItems()
    {
        var request = new ParameterResolverRequest();
        request.FileFormatsToLaunch.Add("iso");
        request.FileFormatsToLaunch.Add("cue");

        Assert.Equal(2, request.FileFormatsToLaunch.Count);
        Assert.Contains("iso", request.FileFormatsToLaunch, StringComparer.Ordinal);
        Assert.Contains("cue", request.FileFormatsToLaunch, StringComparer.Ordinal);
    }

    /// <summary>
    /// Verifies that SystemName supports Unicode characters.
    /// </summary>
    [Fact]
    public void SystemNameSupportsUnicode()
    {
        var request = new ParameterResolverRequest { SystemName = "ポケモン" };
        Assert.Equal("ポケモン", request.SystemName);
    }

    /// <summary>
    /// Verifies that SystemName supports special characters such as parentheses and brackets.
    /// </summary>
    [Fact]
    public void SystemNameSupportsSpecialCharacters()
    {
        var request = new ParameterResolverRequest { SystemName = "Game (v1.0) [!]" };
        Assert.Equal("Game (v1.0) [!]", request.SystemName);
    }

    /// <summary>
    /// Verifies that CurrentParameters supports very long strings.
    /// </summary>
    [Fact]
    public void CurrentParametersSupportsLongString()
    {
        var longParam = new string('-', 10000);
        var request = new ParameterResolverRequest { CurrentParameters = longParam };
        Assert.Equal(longParam, request.CurrentParameters);
    }

    /// <summary>
    /// Verifies that multiple instances maintain independent property values.
    /// </summary>
    [Fact]
    public void MultipleInstancesAreIndependent()
    {
        var r1 = new ParameterResolverRequest { SystemName = "NES" };
        var r2 = new ParameterResolverRequest { SystemName = "SNES" };

        Assert.NotEqual(r1.SystemName, r2.SystemName, StringComparer.Ordinal);
    }

    /// <summary>
    /// Verifies that properties can be modified after the object is created.
    /// </summary>
    [Fact]
    public void PropertiesCanBeChangedAfterCreation()
    {
        var request = new ParameterResolverRequest { SystemName = "NES" };
        request.SystemName = "SNES";
        Assert.Equal("SNES", request.SystemName);
    }
}
