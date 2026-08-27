using System.Text.Json;
using SimpleLauncher.Core.Models;
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

    /// <summary>
    /// Verifies that a new GogGameInfo has all properties defaulting to null.
    /// </summary>
    [Fact]
    public void GogGameInfoDefaultPropertiesAreNull()
    {
        var info = new GogGameInfo();
        Assert.Null(info.GameId);
        Assert.Null(info.RootGameId);
        Assert.Null(info.PlayTasks);
    }

    /// <summary>
    /// Verifies that GogGameInfo properties can be set and retrieved correctly.
    /// </summary>
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

    /// <summary>
    /// Verifies that GogGameInfo can be deserialized from JSON with empty play tasks.
    /// </summary>
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

    /// <summary>
    /// Verifies that GogGameInfo can be deserialized from JSON with play tasks populated.
    /// </summary>
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

    /// <summary>
    /// Verifies that GogGameInfo can be deserialized from an empty JSON object.
    /// </summary>
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

    /// <summary>
    /// Verifies that GogGameInfo detects DLC when RootGameId differs from GameId.
    /// </summary>
    [Fact]
    public void GogGameInfoDetectsDlcWhenRootGameIdDiffers()
    {
        var info = new GogGameInfo
        {
            GameId = "12345",
            RootGameId = "67890"
        };

        var isDlc = !string.IsNullOrEmpty(info.RootGameId) &&
                    !string.Equals(info.RootGameId, info.GameId, StringComparison.Ordinal);
        Assert.True(isDlc);
    }

    /// <summary>
    /// Verifies that GogGameInfo does not detect DLC when RootGameId matches GameId.
    /// </summary>
    [Fact]
    public void GogGameInfoNotDlcWhenRootGameIdMatches()
    {
        var info = new GogGameInfo
        {
            GameId = "12345",
            RootGameId = "12345"
        };

        var isDlc = !string.IsNullOrEmpty(info.RootGameId) &&
                    !string.Equals(info.RootGameId, info.GameId, StringComparison.Ordinal);
        Assert.False(isDlc);
    }

    /// <summary>
    /// Verifies that GogGameInfo does not detect DLC when RootGameId is empty.
    /// </summary>
    [Fact]
    public void GogGameInfoNotDlcWhenRootGameIdIsEmpty()
    {
        var info = new GogGameInfo
        {
            GameId = "12345",
            RootGameId = ""
        };

        var isDlc = !string.IsNullOrEmpty(info.RootGameId) &&
                    !string.Equals(info.RootGameId, info.GameId, StringComparison.Ordinal);
        Assert.False(isDlc);
    }

    /// <summary>
    /// Verifies that GogGameInfo does not detect DLC when RootGameId is null.
    /// </summary>
    [Fact]
    public void GogGameInfoNotDlcWhenRootGameIdIsNull()
    {
        var info = new GogGameInfo
        {
            GameId = "12345",
            RootGameId = null!
        };

        var isDlc = !string.IsNullOrEmpty(info.RootGameId) &&
                    !string.Equals(info.RootGameId, info.GameId, StringComparison.Ordinal);
        Assert.False(isDlc);
    }

    // GogPlayTask tests

    /// <summary>
    /// Verifies that a new GogPlayTask has all properties defaulting to null or false.
    /// </summary>
    [Fact]
    public void GogPlayTaskDefaultValues()
    {
        var task = new GogPlayTask();
        Assert.False(task.IsPrimary);
        Assert.Null(task.Type);
        Assert.Null(task.Path);
        Assert.Null(task.WorkingDir);
    }

    /// <summary>
    /// Verifies that GogPlayTask properties can be set and retrieved correctly.
    /// </summary>
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

    /// <summary>
    /// Verifies that GogPlayTask can be deserialized from JSON with all properties populated.
    /// </summary>
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

    /// <summary>
    /// Verifies that GogPlayTask can be deserialized from an empty JSON object.
    /// </summary>
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

    /// <summary>
    /// Verifies that the primary FileTask can be found among multiple play tasks.
    /// </summary>
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

        var primaryTask = info.PlayTasks.FirstOrDefault(t => t is { IsPrimary: true, Type: "FileTask" });
        Assert.NotNull(primaryTask);
        Assert.Equal("game.exe", primaryTask.Path);
    }

    /// <summary>
    /// Verifies that null is returned when no primary FileTask exists.
    /// </summary>
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

        var primaryTask = info.PlayTasks.FirstOrDefault(t => t is { IsPrimary: true, Type: "FileTask" });
        Assert.Null(primaryTask);
    }

    /// <summary>
    /// Verifies that GogGameInfo can be deserialized from JSON with multiple play tasks.
    /// </summary>
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