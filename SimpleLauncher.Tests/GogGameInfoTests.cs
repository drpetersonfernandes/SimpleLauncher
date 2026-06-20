using System.Text.Json;
using SimpleLauncher.Services.GameScan.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="GogGameInfo"/> and <see cref="GogPlayTask"/> models
/// covering JSON deserialization, default values, and property assignment.
/// </summary>
public class GogGameInfoTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // GogGameInfo tests

    [Fact]
    public void GogGameInfoDefaultPropertiesAreNull()
    {
        var info = new GogGameInfo();
        Assert.Null(info.GameId);
        Assert.Null(info.RootGameId);
        Assert.Null(info.PlayTasks);
    }

    [Fact]
    public void GogGameInfoPropertiesCanBeSet()
    {
        var info = new GogGameInfo
        {
            GameId = "12345",
            RootGameId = "67890",
            PlayTasks = new List<GogPlayTask>()
        };

        Assert.Equal("12345", info.GameId);
        Assert.Equal("67890", info.RootGameId);
        Assert.NotNull(info.PlayTasks);
    }

    [Fact]
    public void GogGameInfoDeserializeFromJson()
    {
        const string json = """
                            {
                                "gameId": "12345",
                                "rootGameId": "67890",
                                "playTasks": []
                            }
                            """;

        var info = JsonSerializer.Deserialize<GogGameInfo>(json, JsonOptions);

        Assert.NotNull(info);
        Assert.Equal("12345", info.GameId);
        Assert.Equal("67890", info.RootGameId);
        Assert.NotNull(info.PlayTasks);
        Assert.Empty(info.PlayTasks);
    }

    [Fact]
    public void GogGameInfoDeserializeWithPlayTasks()
    {
        const string json = """
                            {
                                "gameId": "12345",
                                "rootGameId": "",
                                "playTasks": [
                                    {
                                        "isPrimary": true,
                                        "type": "FileTask",
                                        "path": "game.exe",
                                        "workingDir": ""
                                    }
                                ]
                            }
                            """;

        var info = JsonSerializer.Deserialize<GogGameInfo>(json, JsonOptions);

        Assert.NotNull(info);
        Assert.Single(info.PlayTasks);
        Assert.True(info.PlayTasks[0].IsPrimary);
        Assert.Equal("FileTask", info.PlayTasks[0].Type);
        Assert.Equal("game.exe", info.PlayTasks[0].Path);
    }

    [Fact]
    public void GogGameInfoDeserializeEmptyJson()
    {
        const string json = "{}";

        var info = JsonSerializer.Deserialize<GogGameInfo>(json, JsonOptions);

        Assert.NotNull(info);
        Assert.Null(info.GameId);
        Assert.Null(info.RootGameId);
        Assert.Null(info.PlayTasks);
    }

    [Fact]
    public void GogGameInfoDetectsDlcWhenRootGameIdDiffers()
    {
        var info = new GogGameInfo
        {
            GameId = "12345",
            RootGameId = "67890"
        };

        var isDlc = !string.IsNullOrEmpty(info.RootGameId) && info.RootGameId != info.GameId;
        Assert.True(isDlc);
    }

    [Fact]
    public void GogGameInfoNotDlcWhenRootGameIdMatches()
    {
        var info = new GogGameInfo
        {
            GameId = "12345",
            RootGameId = "12345"
        };

        var isDlc = !string.IsNullOrEmpty(info.RootGameId) && info.RootGameId != info.GameId;
        Assert.False(isDlc);
    }

    [Fact]
    public void GogGameInfoNotDlcWhenRootGameIdIsEmpty()
    {
        var info = new GogGameInfo
        {
            GameId = "12345",
            RootGameId = ""
        };

        var isDlc = !string.IsNullOrEmpty(info.RootGameId) && info.RootGameId != info.GameId;
        Assert.False(isDlc);
    }

    [Fact]
    public void GogGameInfoNotDlcWhenRootGameIdIsNull()
    {
        var info = new GogGameInfo
        {
            GameId = "12345",
            RootGameId = null
        };

        var isDlc = !string.IsNullOrEmpty(info.RootGameId) && info.RootGameId != info.GameId;
        Assert.False(isDlc);
    }

    // GogPlayTask tests

    [Fact]
    public void GogPlayTaskDefaultValues()
    {
        var task = new GogPlayTask();
        Assert.False(task.IsPrimary);
        Assert.Null(task.Type);
        Assert.Null(task.Path);
        Assert.Null(task.WorkingDir);
    }

    [Fact]
    public void GogPlayTaskPropertiesCanBeSet()
    {
        var task = new GogPlayTask
        {
            IsPrimary = true,
            Type = "FileTask",
            Path = "game.exe",
            WorkingDir = "bin"
        };

        Assert.True(task.IsPrimary);
        Assert.Equal("FileTask", task.Type);
        Assert.Equal("game.exe", task.Path);
        Assert.Equal("bin", task.WorkingDir);
    }

    [Fact]
    public void GogPlayTaskDeserializeFromJson()
    {
        const string json = """
                            {
                                "isPrimary": true,
                                "type": "URLTask",
                                "path": "https://example.com",
                                "workingDir": ""
                            }
                            """;

        var task = JsonSerializer.Deserialize<GogPlayTask>(json, JsonOptions);

        Assert.NotNull(task);
        Assert.True(task.IsPrimary);
        Assert.Equal("URLTask", task.Type);
        Assert.Equal("https://example.com", task.Path);
    }

    [Fact]
    public void GogPlayTaskDeserializeEmptyJson()
    {
        const string json = "{}";

        var task = JsonSerializer.Deserialize<GogPlayTask>(json, JsonOptions);

        Assert.NotNull(task);
        Assert.False(task.IsPrimary);
        Assert.Null(task.Type);
        Assert.Null(task.Path);
        Assert.Null(task.WorkingDir);
    }

    [Fact]
    public void GogGameInfoFindPrimaryFileTask()
    {
        var info = new GogGameInfo
        {
            GameId = "12345",
            PlayTasks =
            [
                new GogPlayTask { IsPrimary = false, Type = "URLTask", Path = "https://gog.com" },
                new GogPlayTask { IsPrimary = true, Type = "FileTask", Path = "game.exe" },
                new GogPlayTask { IsPrimary = false, Type = "FileTask", Path = "launcher.exe" }
            ]
        };

        var primaryTask = info.PlayTasks.FirstOrDefault(t => t.IsPrimary && t.Type == "FileTask");
        Assert.NotNull(primaryTask);
        Assert.Equal("game.exe", primaryTask.Path);
    }

    [Fact]
    public void GogGameInfoNoPrimaryFileTaskReturnsNull()
    {
        var info = new GogGameInfo
        {
            GameId = "12345",
            PlayTasks =
            [
                new GogPlayTask { IsPrimary = false, Type = "URLTask", Path = "https://gog.com" },
                new GogPlayTask { IsPrimary = true, Type = "URLTask", Path = "https://gog.com/launch" }
            ]
        };

        var primaryTask = info.PlayTasks.FirstOrDefault(t => t.IsPrimary && t.Type == "FileTask");
        Assert.Null(primaryTask);
    }

    [Fact]
    public void GogGameInfoDeserializeMultiplePlayTasks()
    {
        const string json = """
                            {
                                "gameId": "12345",
                                "playTasks": [
                                    { "isPrimary": false, "type": "URLTask", "path": "url1" },
                                    { "isPrimary": true, "type": "FileTask", "path": "main.exe" },
                                    { "isPrimary": false, "type": "FileTask", "path": "setup.exe" }
                                ]
                            }
                            """;

        var info = JsonSerializer.Deserialize<GogGameInfo>(json, JsonOptions);

        Assert.NotNull(info);
        Assert.Equal(3, info.PlayTasks.Count);
        Assert.Equal("main.exe", info.PlayTasks[1].Path);
    }
}
