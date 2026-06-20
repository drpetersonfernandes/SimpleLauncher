using SimpleLauncher.Services.GameScan.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="GameClassificationItem"/> and <see cref="GameClassificationResponse"/> models
/// covering default values, property assignment, and collection behavior.
/// </summary>
public class GameClassificationModelsTests
{
    // GameClassificationItem tests

    [Fact]
    public void GameClassificationItemDefaultNameIsEmpty()
    {
        var item = new GameClassificationItem();
        Assert.Equal("", item.Name);
    }

    [Fact]
    public void GameClassificationItemDefaultPropertiesAreNull()
    {
        var item = new GameClassificationItem();
        Assert.Null(item.AppId);
        Assert.Null(item.InstallLocation);
        Assert.Null(item.PackageFamilyName);
        Assert.Null(item.LogoRelativePath);
    }

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

    [Fact]
    public void GameClassificationItemSupportsUnicode()
    {
        var item = new GameClassificationItem { Name = "ポケモン" };
        Assert.Equal("ポケモン", item.Name);
    }

    [Fact]
    public void GameClassificationItemSupportsLongName()
    {
        var longName = new string('A', 500);
        var item = new GameClassificationItem { Name = longName };
        Assert.Equal(longName, item.Name);
    }

    [Fact]
    public void GameClassificationItemSupportsSpecialCharacters()
    {
        var item = new GameClassificationItem { Name = "Game (v1.0) [!] - Special Edition" };
        Assert.Equal("Game (v1.0) [!] - Special Edition", item.Name);
    }

    // GameClassificationResponse tests

    [Fact]
    public void GameClassificationResponseDefaultGamesIsEmptyList()
    {
        var response = new GameClassificationResponse();
        Assert.NotNull(response.Games);
        Assert.Empty(response.Games);
    }

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

    [Fact]
    public void GameClassificationResponseCanAddGamesDynamically()
    {
        var response = new GameClassificationResponse();
        response.Games.Add(new GameClassificationItem { Name = "NewGame" });

        Assert.Single(response.Games);
        Assert.Equal("NewGame", response.Games[0].Name);
    }

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

        var actionGames = response.Games.Where(g => g.Name.Contains("Action")).ToList();
        Assert.Equal(2, actionGames.Count);
    }
}
