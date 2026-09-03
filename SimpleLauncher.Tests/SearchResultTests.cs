using System.ComponentModel;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
///     Tests for <see cref="SearchResult" /> score change notification and default emulator fallback.
/// </summary>
public class SearchResultTests
{
    [Fact]
    public void Score_Setter_RaisesPropertyChanged()
    {
        var result = new SearchResult();
        var changedProperties = new List<string?>();
        result.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        result.Score = 42;

        Assert.Equal(["Score"], changedProperties);
        Assert.Equal(42, result.Score);
    }

    [Fact]
    public void Score_Setter_SameValue_DoesNotRaisePropertyChanged()
    {
        var result = new SearchResult { Score = 7 };
        var changedProperties = new List<string?>();
        result.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        result.Score = 7;

        Assert.Empty(changedProperties);
    }

    [Fact]
    public void Score_ImplementsINotifyPropertyChanged()
    {
        Assert.IsType<INotifyPropertyChanged>(new SearchResult(), exactMatch: false);
    }

    [Fact]
    public void DefaultEmulator_WithConfiguredEmulator_ReturnsEmulatorName()
    {
        var result = new SearchResult { EmulatorManager = new Emulator { EmulatorName = "RetroArch" } };

        Assert.Equal("RetroArch", result.DefaultEmulator);
    }

    [Fact]
    public void DefaultEmulator_WithoutEmulator_ReturnsFallbackMessage()
    {
        var result = new SearchResult();

        // Application.Current resource lookup falls back to the hardcoded message
        Assert.Equal("No Default Emulator", result.DefaultEmulator);
    }

    [Fact]
    public void InitOnlyProperties_AreSettableViaObjectInitializer()
    {
        var result = new SearchResult
        {
            FileName = "Super Mario",
            FileNameWithExtension = "Super Mario.zip",
            MachineName = "smb",
            FolderName = "Nintendo NES",
            FilePath = "C:\\roms\\Super Mario.zip",
            SystemName = "NES",
            CoverImage = "C:\\images\\smb.png"
        };

        Assert.Equal("Super Mario", result.FileName);
        Assert.Equal("smb", result.MachineName);
        Assert.Equal("NES", result.SystemName);
    }
}