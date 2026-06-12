using SimpleLauncher.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Provides extended test coverage for the <see cref="SystemPlayTime"/> model, verifying formatted output
/// and property behavior across various scenarios.
/// </summary>
public class SystemPlayTimeExtendedTests
{
    /// <summary>
    /// Verifies that <see cref="SystemPlayTime.PlayTimeSeconds"/> defaults to zero.
    /// </summary>
    [Fact]
    public void SystemPlayTimeDefaultPlayTimeSecondsIsZero()
    {
        var spt = new SystemPlayTime { SystemName = "NES" };
        Assert.Equal(0, spt.PlayTimeSeconds);
    }

    /// <summary>
    /// Verifies that zero seconds produces 0:00:00.
    /// </summary>
    [Fact]
    public void SystemPlayTimeFormattedPlayTimeZeroSeconds()
    {
        var spt = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 0 };
        Assert.Equal("0:00:00", spt.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that 3600 seconds produces 1:00:00.
    /// </summary>
    [Fact]
    public void SystemPlayTimeFormattedPlayTimeOneHour()
    {
        var spt = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 3600 };
        Assert.Equal("1:00:00", spt.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that 3723 seconds produces 1:02:03.
    /// </summary>
    [Fact]
    public void SystemPlayTimeFormattedPlayTimeComplex()
    {
        var spt = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 3723 };
        Assert.Equal("1:02:03", spt.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that 120 seconds produces 0:02:00.
    /// </summary>
    [Fact]
    public void SystemPlayTimeFormattedPlayTimeMinutesOnly()
    {
        var spt = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 120 };
        Assert.Equal("0:02:00", spt.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that 45 seconds produces 0:00:45.
    /// </summary>
    [Fact]
    public void SystemPlayTimeFormattedPlayTimeSecondsOnly()
    {
        var spt = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 45 };
        Assert.Equal("0:00:45", spt.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that 86400 seconds (24 hours) produces 24:00:00.
    /// </summary>
    [Fact]
    public void SystemPlayTimeFormattedPlayTimeLargeValue()
    {
        var spt = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 86400 };
        Assert.Equal("24:00:00", spt.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that <see cref="SystemPlayTime.SystemName"/> is set correctly during object initialization.
    /// </summary>
    [Fact]
    public void SystemPlayTimeSystemNameIsInitOnly()
    {
        var spt = new SystemPlayTime { SystemName = "SNES" };
        Assert.Equal("SNES", spt.SystemName);
    }

    /// <summary>
    /// Verifies that <see cref="SystemPlayTime.PlayTimeSeconds"/> can be set and updated after construction.
    /// </summary>
    [Fact]
    public void SystemPlayTimePlayTimeSecondsCanBeSet()
    {
        var spt = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 100 };
        Assert.Equal(100, spt.PlayTimeSeconds);

        spt.PlayTimeSeconds = 200;
        Assert.Equal(200, spt.PlayTimeSeconds);
    }

    /// <summary>
    /// Verifies that <see cref="SystemPlayTime.FormattedPlayTime"/> reflects the current <see cref="SystemPlayTime.PlayTimeSeconds"/> value after updates.
    /// </summary>
    [Fact]
    public void SystemPlayTimeFormattedPlayTimeUpdatesWithPlayTimeSeconds()
    {
        var spt = new SystemPlayTime { SystemName = "NES", PlayTimeSeconds = 0 };
        Assert.Equal("0:00:00", spt.FormattedPlayTime);

        spt.PlayTimeSeconds = 3661;
        Assert.Equal("1:01:01", spt.FormattedPlayTime);
    }

    /// <summary>
    /// Verifies that multiple <see cref="SystemPlayTime"/> instances maintain independent formatted outputs.
    /// </summary>
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
        Assert.Equal("1:00:00", times[0].FormattedPlayTime);
        Assert.Equal("2:00:00", times[1].FormattedPlayTime);
        Assert.Equal("0:30:00", times[2].FormattedPlayTime);
    }
}
