using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests the <see cref="DownloadButtonState"/> enum values, count, and parsing behavior.
/// </summary>
public class DownloadButtonStateTests
{
    /// <summary>
    /// Verifies that the Idle enum value has the expected integer value of 0.
    /// </summary>
    [Fact]
    public void DownloadButtonStateEnumHasIdle()
    {
        Assert.Equal(0, (int)DownloadButtonState.Idle);
    }

    /// <summary>
    /// Verifies that the Downloading enum value has the expected integer value of 1.
    /// </summary>
    [Fact]
    public void DownloadButtonStateEnumHasDownloading()
    {
        Assert.Equal(1, (int)DownloadButtonState.Downloading);
    }

    /// <summary>
    /// Verifies that the Downloaded enum value has the expected integer value of 2.
    /// </summary>
    [Fact]
    public void DownloadButtonStateEnumHasDownloaded()
    {
        Assert.Equal(2, (int)DownloadButtonState.Downloaded);
    }

    /// <summary>
    /// Verifies that the Failed enum value has the expected integer value of 3.
    /// </summary>
    [Fact]
    public void DownloadButtonStateEnumHasFailed()
    {
        Assert.Equal(3, (int)DownloadButtonState.Failed);
    }

    /// <summary>
    /// Verifies that the DownloadButtonState enum has exactly four defined values.
    /// </summary>
    [Fact]
    public void DownloadButtonStateEnumHasFourValues()
    {
        var values = Enum.GetValues<DownloadButtonState>();
        Assert.Equal(4, values.Length);
    }

    /// <summary>
    /// Verifies that all DownloadButtonState enum values are defined.
    /// </summary>
    [Fact]
    public void DownloadButtonStateEnumAllValuesAreDefined()
    {
        Assert.True(Enum.IsDefined(DownloadButtonState.Idle));
        Assert.True(Enum.IsDefined(DownloadButtonState.Downloading));
        Assert.True(Enum.IsDefined(DownloadButtonState.Downloaded));
        Assert.True(Enum.IsDefined(DownloadButtonState.Failed));
    }

    /// <summary>
    /// Verifies that DownloadButtonState enum values can be parsed from their string names.
    /// </summary>
    [Fact]
    public void DownloadButtonStateParseFromString()
    {
        Assert.Equal(DownloadButtonState.Idle, Enum.Parse<DownloadButtonState>("Idle"));
        Assert.Equal(DownloadButtonState.Downloading, Enum.Parse<DownloadButtonState>("Downloading"));
        Assert.Equal(DownloadButtonState.Downloaded, Enum.Parse<DownloadButtonState>("Downloaded"));
        Assert.Equal(DownloadButtonState.Failed, Enum.Parse<DownloadButtonState>("Failed"));
    }
}
