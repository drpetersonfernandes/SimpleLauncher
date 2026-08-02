using SimpleLauncher.New.Services.PlayHistory;

namespace SimpleLauncher.New.Tests;

[Collection("Sequential")]
public class PlayHistoryManagerTests : IDisposable
{
    private readonly string _histPath;

    public PlayHistoryManagerTests()
    {
        // DataFileLocation uses AppDomain.CurrentDomain.BaseDirectory, not Environment.CurrentDirectory.
        _histPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "playhistory.dat");
        try
        {
            File.Delete(_histPath);
        }
        catch
        {
            // ignored
        }
    }

    [Fact]
    public async Task RecordPlay_NewGame_IncrementsCount()
    {
        var manager = PlayHistoryManager.LoadPlayHistory();
        await manager.RecordPlayAsync(@"C:\roms\NES\mario.zip", "NES");

        var count = manager.GetPlayCount(@"C:\roms\NES\mario.zip");
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task RecordPlay_ExistingGame_IncrementsCount()
    {
        var manager = PlayHistoryManager.LoadPlayHistory();
        await manager.RecordPlayAsync(@"C:\roms\NES\mario.zip", "NES");
        await manager.RecordPlayAsync(@"C:\roms\NES\mario.zip", "NES");
        await manager.RecordPlayAsync(@"C:\roms\NES\mario.zip", "NES");

        Assert.Equal(3, manager.GetPlayCount(@"C:\roms\NES\mario.zip"));
    }

    [Fact]
    public async Task RecordPlay_SetsLastPlayedDate()
    {
        var manager = PlayHistoryManager.LoadPlayHistory();
        await manager.RecordPlayAsync(@"C:\roms\SNES\zelda.sfc", "SNES");

        var recent = manager.GetRecentHistory(10);
        Assert.Contains(recent, h => h.FileName == @"C:\roms\SNES\zelda.sfc");
        Assert.Equal("SNES", recent.First(h => h.FileName == @"C:\roms\SNES\zelda.sfc").SystemName);
    }

    [Fact]
    public async Task GetRecentHistory_OrdersByMostRecent()
    {
        var manager = PlayHistoryManager.LoadPlayHistory();
        await manager.RecordPlayAsync(@"C:\roms\A.zip", "SYS");
        await Task.Delay(1100); // Ensure different second for LastPlayTime
        await manager.RecordPlayAsync(@"C:\roms\B.zip", "SYS");

        var recent = manager.GetRecentHistory(10);
        Assert.Equal(@"C:\roms\B.zip", recent[0].FileName);
        Assert.Equal(@"C:\roms\A.zip", recent[1].FileName);
    }

    [Fact]
    public void GetPlayCount_UnknownGame_ReturnsZero()
    {
        var manager = PlayHistoryManager.LoadPlayHistory();
        Assert.Equal(0, manager.GetPlayCount(@"C:\nonexistent.zip"));
    }

    [Fact]
    public async Task GetHistoryLookup_ReturnsDictionary()
    {
        var manager = PlayHistoryManager.LoadPlayHistory();
        await manager.RecordPlayAsync(@"C:\roms\NES\game.zip", "NES");

        var lookup = manager.GetHistoryLookup();
        Assert.Contains(@"C:\roms\NES\game.zip", lookup.Keys);
    }

    [Fact]
    public async Task LoadPlayHistory_PersistsAcrossInstances()
    {
        var manager1 = PlayHistoryManager.LoadPlayHistory();
        await manager1.RecordPlayAsync(@"C:\roms\NES\persist.zip", "NES");

        var manager2 = PlayHistoryManager.LoadPlayHistory();
        Assert.Equal(1, manager2.GetPlayCount(@"C:\roms\NES\persist.zip"));
    }

    public void Dispose()
    {
        try
        {
            File.Delete(_histPath);
        }
        catch
        {
            // ignored
        }
    }
}
