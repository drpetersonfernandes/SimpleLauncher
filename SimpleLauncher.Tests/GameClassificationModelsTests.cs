using SimpleLauncher.Core.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
///     Tests for <see cref="GameClassificationItem" /> and <see cref="GameClassificationResponse" /> models
///     covering default values, property assignment, and collection behavior.
/// </summary>
public class GameClassificationModelsTests
{
    // GameClassificationItem tests

    /// <summary>
    ///     Verifies that the default Name property of a new GameClassificationItem is an empty string.
    /// </summary>
    [Fact]
    public void GameClassificationItemDefaultNameIsEmpty()
    {
        var item = new GameClassificationItem();
        Assert.Equal("", item.Name);
    }

    /// <summary>
    ///     Verifies that the default properties of a new GameClassificationItem are null.
    /// </summary>
    [Fact]
    public void GameClassificationItemDefaultPropertiesAreNull()
    {
        var item = new GameClassificationItem();
        Assert.Null(item.AppId);
        Assert.Null(item.InstallLocation);
        Assert.Null(item.PackageFamilyName);
        Assert.Null(item.LogoRelativePath);
    }

    /// <summary>
    ///     Verifies that all GameClassificationItem properties can be set and retrieved correctly.
    /// </summary>
    [Fact]
    public void GameClassificationItemAllPropertiesCanBeSet()
    {
        var item = new GameClassificationItem
        {
            Name = "Minecraft",
            AppId = "9WDNCFHDXN7B",
            InstallLocation = @"C:\Program Files\Minecraft",
            PackageFamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe",
            LogoRelativePath = "Assets\\Logo.png"
        };

        Assert.Equal("Minecraft", item.Name);
        Assert.Equal("9WDNCFHDXN7B", item.AppId);
        Assert.Equal(@"C:\Program Files\Minecraft", item.InstallLocation);
        Assert.Equal("Microsoft.MinecraftUWP_8wekyb3d8bbwe", item.PackageFamilyName);
        Assert.Equal("Assets\\Logo.png", item.LogoRelativePath);
    }

    /// <summary>
    ///     Verifies that GameClassificationItem supports Unicode characters in the Name property.
    /// </summary>
    [Fact]
    public void GameClassificationItemSupportsUnicode()
    {
        var item = new GameClassificationItem { Name = "ポケモン" };
        Assert.Equal("ポケモン", item.Name);
    }

    /// <summary>
    ///     Verifies that GameClassificationItem supports long name strings.
    /// </summary>
    [Fact]
    public void GameClassificationItemSupportsLongName()
    {
        var longName = new string('A', 500);
        var item = new GameClassificationItem { Name = longName };
        Assert.Equal(longName, item.Name);
    }

    /// <summary>
    ///     Verifies that GameClassificationItem supports special characters in the Name property.
    /// </summary>
    [Fact]
    public void GameClassificationItemSupportsSpecialCharacters()
    {
        var item = new GameClassificationItem { Name = "Game (v1.0) [!] - Special Edition" };
        Assert.Equal("Game (v1.0) [!] - Special Edition", item.Name);
    }

    // GameClassificationResponse tests

    /// <summary>
    ///     Verifies that the default Games property of a new GameClassificationResponse is an empty list.
    /// </summary>
    [Fact]
    public void GameClassificationResponseDefaultGamesIsEmptyList()
    {
        var response = new GameClassificationResponse();
        Assert.NotNull(response.Games);
        Assert.Empty(response.Games);
    }

    /// <summary>
    ///     Verifies that GameClassificationResponse Games list can be populated with multiple items.
    /// </summary>
    [Fact]
    public void GameClassificationResponseGamesCanBePopulated()
    {
        var response = new GameClassificationResponse
        {
            Games =
            [
                new GameClassificationItem { Name = "Game1", AppId = "1" },
                new GameClassificationItem { Name = "Game2", AppId = "2" },
                new GameClassificationItem { Name = "Game3", AppId = "3" }
            ]
        };

        Assert.Equal(3, response.Games.Count);
        Assert.Equal("Game1", response.Games[0].Name);
        Assert.Equal("Game2", response.Games[1].Name);
        Assert.Equal("Game3", response.Games[2].Name);
    }

    /// <summary>
    ///     Verifies that games can be added dynamically to the GameClassificationResponse Games list.
    /// </summary>
    [Fact]
    public void GameClassificationResponseCanAddGamesDynamically()
    {
        var response = new GameClassificationResponse();
        response.Games.Add(new GameClassificationItem { Name = "NewGame" });

        Assert.Single(response.Games);
        Assert.Equal("NewGame", response.Games[0].Name);
    }

    /// <summary>
    ///     Verifies that the Games list can be cleared on a GameClassificationResponse.
    /// </summary>
    [Fact]
    public void GameClassificationResponseCanClearGames()
    {
        var response = new GameClassificationResponse
        {
            Games = [new GameClassificationItem { Name = "Game1" }]
        };

        response.Games.Clear();
        Assert.Empty(response.Games);
    }

    /// <summary>
    ///     Verifies that the Games list supports LINQ queries for filtering.
    /// </summary>
    [Fact]
    public void GameClassificationResponseGamesListSupportsLinq()
    {
        var response = new GameClassificationResponse
        {
            Games =
            [
                new GameClassificationItem { Name = "Action Game", AppId = "1" },
                new GameClassificationItem { Name = "RPG Game", AppId = "2" },
                new GameClassificationItem { Name = "Action RPG", AppId = "3" }
            ]
        };

        var actionGames = response.Games.Where(g => g.Name.Contains("Action", StringComparison.Ordinal)).ToList();
        Assert.Equal(2, actionGames.Count);
    }
}