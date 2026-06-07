using SimpleLauncher.Core.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class DownloadButtonStateTests
{
    [Fact]
    public void DownloadButtonStateEnumHasIdle()
    {
        Assert.Equal(0, (int)DownloadButtonState.Idle);
    }

    [Fact]
    public void DownloadButtonStateEnumHasDownloading()
    {
        Assert.Equal(1, (int)DownloadButtonState.Downloading);
    }

    [Fact]
    public void DownloadButtonStateEnumHasDownloaded()
    {
        Assert.Equal(2, (int)DownloadButtonState.Downloaded);
    }

    [Fact]
    public void DownloadButtonStateEnumHasFailed()
    {
        Assert.Equal(3, (int)DownloadButtonState.Failed);
    }

    [Fact]
    public void DownloadButtonStateEnumHasFourValues()
    {
        var values = Enum.GetValues<DownloadButtonState>();
        Assert.Equal(4, values.Length);
    }

    [Fact]
    public void DownloadButtonStateEnumAllValuesAreDefined()
    {
        Assert.True(Enum.IsDefined(DownloadButtonState.Idle));
        Assert.True(Enum.IsDefined(DownloadButtonState.Downloading));
        Assert.True(Enum.IsDefined(DownloadButtonState.Downloaded));
        Assert.True(Enum.IsDefined(DownloadButtonState.Failed));
    }

    [Fact]
    public void DownloadButtonStateParseFromString()
    {
        Assert.Equal(DownloadButtonState.Idle, Enum.Parse<DownloadButtonState>("Idle"));
        Assert.Equal(DownloadButtonState.Downloading, Enum.Parse<DownloadButtonState>("Downloading"));
        Assert.Equal(DownloadButtonState.Downloaded, Enum.Parse<DownloadButtonState>("Downloaded"));
        Assert.Equal(DownloadButtonState.Failed, Enum.Parse<DownloadButtonState>("Failed"));
    }
}
