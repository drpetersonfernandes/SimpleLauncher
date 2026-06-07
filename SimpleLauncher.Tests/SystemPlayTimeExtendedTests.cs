using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

public class SystemPlayTimeExtendedTests
{
    [Fact]
    public void SystemPlayTimeDefaultPlayTimeSecondsIsZero()
    {
        var spt = new SystemPlayTime { SystemName = "NES" };
        Assert.Equal(0, spt.PlayTimeSeconds);
    }

    [Fact]
    public void SystemPlayTimeFormattedPlayTimeZeroSeconds()
    {
        var spt = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 0 };
        Assert.Equal("00:00:00", spt.FormattedPlayTime);
    }

    [Fact]
    public void SystemPlayTimeFormattedPlayTimeOneHour()
    {
        var spt = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 3600 };
        Assert.Equal("01:00:00", spt.FormattedPlayTime);
    }

    [Fact]
    public void SystemPlayTimeFormattedPlayTimeComplex()
    {
        var spt = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 3723 };
        Assert.Equal("01:02:03", spt.FormattedPlayTime);
    }

    [Fact]
    public void SystemPlayTimeFormattedPlayTimeMinutesOnly()
    {
        var spt = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 120 };
        Assert.Equal("00:02:00", spt.FormattedPlayTime);
    }

    [Fact]
    public void SystemPlayTimeFormattedPlayTimeSecondsOnly()
    {
        var spt = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 45 };
        Assert.Equal("00:00:45", spt.FormattedPlayTime);
    }

    [Fact]
    public void SystemPlayTimeFormattedPlayTimeLargeValue()
    {
        var spt = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 86400 };
        Assert.Equal("00:00:00", spt.FormattedPlayTime);
    }

    [Fact]
    public void SystemPlayTimeSystemNameIsInitOnly()
    {
        var spt = new SystemPlayTime { SystemName = "SNES" };
        Assert.Equal("SNES", spt.SystemName);
    }

    [Fact]
    public void SystemPlayTimePlayTimeSecondsCanBeSet()
    {
        var spt = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 100 };
        Assert.Equal(100, spt.PlayTimeSeconds);

        spt.PlayTimeSeconds = 200;
        Assert.Equal(200, spt.PlayTimeSeconds);
    }

    [Fact]
    public void SystemPlayTimeFormattedPlayTimeUpdatesWithPlayTimeSeconds()
    {
        var spt = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 0 };
        Assert.Equal("00:00:00", spt.FormattedPlayTime);

        spt.PlayTimeSeconds = 3661;
        Assert.Equal("01:01:01", spt.FormattedPlayTime);
    }

    [Fact]
    public void SystemPlayTimeMultipleSystems()
    {
        var times = new List<SystemPlayTime>
        {
            new() { SystemName = "NES", PlayTimeSeconds = 3600 },
            new() { SystemName = "SNES", PlayTimeSeconds = 7200 },
            new() { SystemName = "Genesis", PlayTimeSeconds = 1800 }
        };

        Assert.Equal(3, times.Count);
        Assert.Equal("01:00:00", times[0].FormattedPlayTime);
        Assert.Equal("02:00:00", times[1].FormattedPlayTime);
        Assert.Equal("00:30:00", times[2].FormattedPlayTime);
    }
}
