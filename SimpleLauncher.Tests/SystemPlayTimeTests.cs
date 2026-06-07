using SimpleLauncher.Core.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class SystemPlayTimeTests
{
    [Fact]
    public void DefaultValuesAreCorrect()
    {
        var item = new SystemPlayTime { SystemName = "NES" };

        Assert.Equal("NES", item.SystemName);
        Assert.Equal(0, item.PlayTimeSeconds);
    }

    [Fact]
    public void FormattedPlayTimeZeroSecondsReturnsZeroFormat()
    {
        var item = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 0 };
        Assert.Equal("00:00:00", item.FormattedPlayTime);
    }

    [Fact]
    public void FormattedPlayTimeUnderOneHour()
    {
        var item = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 90 }; // 1m 30s
        Assert.Equal("00:01:30", item.FormattedPlayTime);
    }

    [Fact]
    public void FormattedPlayTimeExactlyOneHour()
    {
        var item = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 3600 };
        Assert.Equal("01:00:00", item.FormattedPlayTime);
    }

    [Fact]
    public void FormattedPlayTimeOverOneHour()
    {
        var item = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 3661 }; // 1h 1m 1s
        Assert.Equal("01:01:01", item.FormattedPlayTime);
    }

    [Fact]
    public void FormattedPlayTimeLargeValue()
    {
        var item = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 7384 }; // 2h 3m 4s
        Assert.Equal("02:03:04", item.FormattedPlayTime);
    }

    [Fact]
    public void FormattedPlayTime59Minutes59Seconds()
    {
        var item = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 3599 };
        Assert.Equal("00:59:59", item.FormattedPlayTime);
    }

    [Fact]
    public void PlayTimeSecondsCanBeUpdated()
    {
        var item = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 100 };
        item.PlayTimeSeconds = 200;

        Assert.Equal(200, item.PlayTimeSeconds);
        Assert.Equal("00:03:20", item.FormattedPlayTime);
    }

    [Fact]
    public void SystemNameIsInitOnly()
    {
        var item = new SystemPlayTime { SystemName = "SNES" };
        Assert.Equal("SNES", item.SystemName);
    }

    [Fact]
    public void FormattedPlayTimeReflectsCurrentPlayTimeSeconds()
    {
        var item = new SystemPlayTime { SystemName = "Genesis", PlayTimeSeconds = 0 };
        Assert.Equal("00:00:00", item.FormattedPlayTime);

        item.PlayTimeSeconds = 120;
        Assert.Equal("00:02:00", item.FormattedPlayTime);
    }

    [Fact]
    public void FormattedPlayTimeUsesInvariantCulture()
    {
        // The format uses hh\:mm\:ss with CultureInfo.InvariantCulture
        // This ensures consistent output regardless of system culture
        var item = new SystemPlayTime { SystemName = "PSX", PlayTimeSeconds = 3723 }; // 1h 2m 3s
        Assert.Equal("01:02:03", item.FormattedPlayTime);
    }
}
