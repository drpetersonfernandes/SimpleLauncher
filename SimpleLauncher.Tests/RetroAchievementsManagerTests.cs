using MessagePack;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.RetroAchievements.Models;
using SimpleLauncher.Services.RetroAchievements;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class RetroAchievementsManagerTests : IDisposable
{
    private readonly ILogErrors _logErrors = new NoOpLogErrors();
    private readonly string _datFilePath;

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }

    public RetroAchievementsManagerTests()
    {
        ServiceProviderMock.Install();
        _datFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "RetroAchievements.dat");
    }

    public void Dispose()
    {
        // Clean up the dat file if it was created during a test
        if (File.Exists(_datFilePath))
        {
            File.Delete(_datFilePath);
        }

        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void LoadRetroAchievementValidDatFileReturnsManagerWithGames()
    {
        var games = new List<RaGameInfo>
        {
            new()
            {
                Id = 1,
                Title = "Super Mario Bros.",
                ConsoleId = 7,
                ConsoleName = "NES",
                ImageIcon = "image.png",
                NumAchievements = 10,
                Points = 100,
                DateModified = "2024-01-01",
                Hashes = ["abc123"]
            }
        };

        var bytes = MessagePackSerializer.Serialize(games);
        File.WriteAllBytes(_datFilePath, bytes);

        var manager = RetroAchievementsManager.LoadRetroAchievement(_logErrors);

        Assert.NotNull(manager);
        Assert.Single(manager.AllGames);
        Assert.Equal("Super Mario Bros.", manager.AllGames[0].Title);
    }

    [Fact]
    public void LoadRetroAchievementCorruptedMessagePackReturnsEmptyManager()
    {
        File.WriteAllBytes(_datFilePath, [0xFF, 0xFF, 0xFF, 0xFF, 0xFF]);

        var manager = RetroAchievementsManager.LoadRetroAchievement(_logErrors);

        Assert.NotNull(manager);
        Assert.Empty(manager.AllGames);
    }

    [Fact]
    public void LoadRetroAchievementXmlContentReturnsEmptyManager()
    {
        File.WriteAllText(_datFilePath, "<xml><item>test</item></xml>");

        var manager = RetroAchievementsManager.LoadRetroAchievement(_logErrors);

        Assert.NotNull(manager);
        Assert.Empty(manager.AllGames);
    }

    [Fact]
    public void LoadRetroAchievementEmptyFileReturnsEmptyManager()
    {
        File.WriteAllBytes(_datFilePath, []);

        var manager = RetroAchievementsManager.LoadRetroAchievement(_logErrors);

        Assert.NotNull(manager);
        Assert.Empty(manager.AllGames);
    }

    [Fact]
    public void LoadRetroAchievementMissingFileReturnsEmptyManager()
    {
        // Ensure the file does not exist
        if (File.Exists(_datFilePath))
        {
            File.Delete(_datFilePath);
        }

        var manager = RetroAchievementsManager.LoadRetroAchievement(_logErrors);

        Assert.NotNull(manager);
        Assert.Empty(manager.AllGames);
    }

    [Fact]
    public void GetGameInfoByHashKnownHashReturnsGameInfo()
    {
        var games = new List<RaGameInfo>
        {
            new()
            {
                Id = 1,
                Title = "Super Mario Bros.",
                ConsoleId = 7,
                ConsoleName = "NES",
                ImageIcon = "image.png",
                NumAchievements = 10,
                Points = 100,
                DateModified = "2024-01-01",
                Hashes = ["abc123", "def456"]
            }
        };

        var bytes = MessagePackSerializer.Serialize(games);
        File.WriteAllBytes(_datFilePath, bytes);

        var manager = RetroAchievementsManager.LoadRetroAchievement(_logErrors);
        var result = manager.GetGameInfoByHash("abc123");

        Assert.NotNull(result);
        Assert.Equal("Super Mario Bros.", result.Title);
    }

    [Fact]
    public void GetGameInfoByHashUnknownHashReturnsNull()
    {
        var games = new List<RaGameInfo>
        {
            new()
            {
                Id = 1,
                Title = "Super Mario Bros.",
                ConsoleId = 7,
                ConsoleName = "NES",
                ImageIcon = "image.png",
                NumAchievements = 10,
                Points = 100,
                DateModified = "2024-01-01",
                Hashes = ["abc123"]
            }
        };

        var bytes = MessagePackSerializer.Serialize(games);
        File.WriteAllBytes(_datFilePath, bytes);

        var manager = RetroAchievementsManager.LoadRetroAchievement(_logErrors);
        var result = manager.GetGameInfoByHash("unknown_hash");

        Assert.Null(result);
    }

    [Fact]
    public void GetGameInfoByHashEmptyStringReturnsNull()
    {
        var games = new List<RaGameInfo>
        {
            new()
            {
                Id = 1,
                Title = "Super Mario Bros.",
                ConsoleId = 7,
                ConsoleName = "NES",
                ImageIcon = "image.png",
                NumAchievements = 10,
                Points = 100,
                DateModified = "2024-01-01",
                Hashes = ["abc123"]
            }
        };

        var bytes = MessagePackSerializer.Serialize(games);
        File.WriteAllBytes(_datFilePath, bytes);

        var manager = RetroAchievementsManager.LoadRetroAchievement(_logErrors);
        var result = manager.GetGameInfoByHash("");

        Assert.Null(result);
    }

    [Fact]
    public void GetGameInfoByHashOverlappingHashesReturnsFirstMatch()
    {
        var games = new List<RaGameInfo>
        {
            new()
            {
                Id = 1,
                Title = "First Game",
                ConsoleId = 7,
                ConsoleName = "NES",
                ImageIcon = "image1.png",
                NumAchievements = 5,
                Points = 50,
                DateModified = "2024-01-01",
                Hashes = ["shared_hash", "unique1"]
            },
            new()
            {
                Id = 2,
                Title = "Second Game",
                ConsoleId = 7,
                ConsoleName = "NES",
                ImageIcon = "image2.png",
                NumAchievements = 10,
                Points = 100,
                DateModified = "2024-02-01",
                Hashes = ["shared_hash", "unique2"]
            }
        };

        var bytes = MessagePackSerializer.Serialize(games);
        File.WriteAllBytes(_datFilePath, bytes);

        var manager = RetroAchievementsManager.LoadRetroAchievement(_logErrors);
        var result = manager.GetGameInfoByHash("shared_hash");

        Assert.NotNull(result);
        Assert.Equal("First Game", result.Title);
    }
}
