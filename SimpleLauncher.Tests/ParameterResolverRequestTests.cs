using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="ParameterResolverRequest"/> model covering default values,
/// property assignment, and collection initialization.
/// </summary>
public class ParameterResolverRequestTests
{
    [Fact]
    public void DefaultSystemNameIsEmpty()
    {
        var request = new ParameterResolverRequest();
        Assert.Equal("", request.SystemName);
    }

    [Fact]
    public void DefaultSystemFolderIsEmpty()
    {
        var request = new ParameterResolverRequest();
        Assert.Equal("", request.SystemFolder);
    }

    [Fact]
    public void DefaultEmulatorNameIsEmpty()
    {
        var request = new ParameterResolverRequest();
        Assert.Equal("", request.EmulatorName);
    }

    [Fact]
    public void DefaultEmulatorPathIsEmpty()
    {
        var request = new ParameterResolverRequest();
        Assert.Equal("", request.EmulatorPath);
    }

    [Fact]
    public void DefaultCurrentParametersIsEmpty()
    {
        var request = new ParameterResolverRequest();
        Assert.Equal("", request.CurrentParameters);
    }

    [Fact]
    public void DefaultFileFormatsToSearchIsEmptyList()
    {
        var request = new ParameterResolverRequest();
        Assert.NotNull(request.FileFormatsToSearch);
        Assert.Empty(request.FileFormatsToSearch);
    }

    [Fact]
    public void DefaultFileFormatsToLaunchIsEmptyList()
    {
        var request = new ParameterResolverRequest();
        Assert.NotNull(request.FileFormatsToLaunch);
        Assert.Empty(request.FileFormatsToLaunch);
    }

    [Fact]
    public void DefaultExtractFileBeforeLaunchIsFalse()
    {
        var request = new ParameterResolverRequest();
        Assert.False(request.ExtractFileBeforeLaunch);
    }

    [Fact]
    public void DefaultGroupByFolderIsFalse()
    {
        var request = new ParameterResolverRequest();
        Assert.False(request.GroupByFolder);
    }

    [Fact]
    public void DefaultDisableRecursiveSearchIsFalse()
    {
        var request = new ParameterResolverRequest();
        Assert.False(request.DisableRecursiveSearch);
    }

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

    [Fact]
    public void FileFormatsToSearchCanAddItems()
    {
        var request = new ParameterResolverRequest();
        request.FileFormatsToSearch.Add("zip");
        request.FileFormatsToSearch.Add("nes");

        Assert.Equal(2, request.FileFormatsToSearch.Count);
        Assert.Contains("zip", request.FileFormatsToSearch);
        Assert.Contains("nes", request.FileFormatsToSearch);
    }

    [Fact]
    public void FileFormatsToLaunchCanAddItems()
    {
        var request = new ParameterResolverRequest();
        request.FileFormatsToLaunch.Add("iso");
        request.FileFormatsToLaunch.Add("cue");

        Assert.Equal(2, request.FileFormatsToLaunch.Count);
        Assert.Contains("iso", request.FileFormatsToLaunch);
        Assert.Contains("cue", request.FileFormatsToLaunch);
    }

    [Fact]
    public void SystemNameSupportsUnicode()
    {
        var request = new ParameterResolverRequest { SystemName = "ポケモン" };
        Assert.Equal("ポケモン", request.SystemName);
    }

    [Fact]
    public void SystemNameSupportsSpecialCharacters()
    {
        var request = new ParameterResolverRequest { SystemName = "Game (v1.0) [!]" };
        Assert.Equal("Game (v1.0) [!]", request.SystemName);
    }

    [Fact]
    public void CurrentParametersSupportsLongString()
    {
        var longParam = new string('-', 10000);
        var request = new ParameterResolverRequest { CurrentParameters = longParam };
        Assert.Equal(longParam, request.CurrentParameters);
    }

    [Fact]
    public void MultipleInstancesAreIndependent()
    {
        var r1 = new ParameterResolverRequest { SystemName = "NES" };
        var r2 = new ParameterResolverRequest { SystemName = "SNES" };

        Assert.NotEqual(r1.SystemName, r2.SystemName);
    }

    [Fact]
    public void PropertiesCanBeChangedAfterCreation()
    {
        var request = new ParameterResolverRequest { SystemName = "NES" };
        request.SystemName = "SNES";
        Assert.Equal("SNES", request.SystemName);
    }
}
