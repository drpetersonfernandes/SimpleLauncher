using System.Text.Json;
using SimpleLauncher.Services.GameScan.Models;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for game scan model DTOs: <see cref="StoreAppInfo"/>, <see cref="EpicInstalledApp"/>,
/// <see cref="EpicInstalledAppList"/>, <see cref="BNetAppDef"/>, <see cref="RockstarGameDef"/>,
/// <see cref="GameImageApiResponse"/>.
/// </summary>
public class GameScanModelsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // StoreAppInfo tests

    [Fact]
    public void StoreAppInfoDefaultValues()
    {
        var info = new StoreAppInfo();
        Assert.Null(info.Name);
        Assert.Null(info.AppId);
        Assert.Null(info.InstallLocation);
        Assert.Null(info.PackageFamilyName);
        Assert.Null(info.LogoRelativePath);
    }

    [Fact]
    public void StoreAppInfoPropertiesCanBeSet()
    {
        var info = new StoreAppInfo
        {
            Name = "Minecraft",
            AppId = "9WDNCFHDXN7B",
            InstallLocation = @"C:\Program Files\WindowsApps\Minecraft",
            PackageFamilyName = "Microsoft.MinecraftUWP_8wekyb3d8bbwe",
            LogoRelativePath = "Assets\\Logo.png"
        };

        Assert.Equal("Minecraft", info.Name);
        Assert.Equal("9WDNCFHDXN7B", info.AppId);
        Assert.Equal(@"C:\Program Files\WindowsApps\Minecraft", info.InstallLocation);
        Assert.Equal("Microsoft.MinecraftUWP_8wekyb3d8bbwe", info.PackageFamilyName);
        Assert.Equal("Assets\\Logo.png", info.LogoRelativePath);
    }

    [Fact]
    public void StoreAppInfoSupportsUnicodeName()
    {
        var info = new StoreAppInfo { Name = "ポケモン" };
        Assert.Equal("ポケモン", info.Name);
    }

    // EpicInstalledApp tests

    [Fact]
    public void EpicInstalledAppDefaultValues()
    {
        var app = new EpicInstalledApp();
        Assert.Null(app.InstallLocation);
        Assert.Null(app.AppName);
        Assert.Null(app.AppVersion);
    }

    [Fact]
    public void EpicInstalledAppPropertiesCanBeSet()
    {
        var app = new EpicInstalledApp
        {
            InstallLocation = @"C:\Games\Fortnite",
            AppName = "Fortnite",
            AppVersion = "1.0.0"
        };

        Assert.Equal(@"C:\Games\Fortnite", app.InstallLocation);
        Assert.Equal("Fortnite", app.AppName);
        Assert.Equal("1.0.0", app.AppVersion);
    }

    [Fact]
    public void EpicInstalledAppDeserializeFromJson()
    {
        const string json = """
                            {
                                "InstallLocation": "C:\\Games\\Fortnite",
                                "AppName": "Fortnite",
                                "AppVersion": "25.10"
                            }
                            """;

        var app = JsonSerializer.Deserialize<EpicInstalledApp>(json, JsonOptions);

        Assert.NotNull(app);
        Assert.Equal(@"C:\Games\Fortnite", app.InstallLocation);
        Assert.Equal("Fortnite", app.AppName);
        Assert.Equal("25.10", app.AppVersion);
    }

    [Fact]
    public void EpicInstalledAppDeserializeEmptyJson()
    {
        const string json = "{}";
        var app = JsonSerializer.Deserialize<EpicInstalledApp>(json, JsonOptions);

        Assert.NotNull(app);
        Assert.Null(app.InstallLocation);
        Assert.Null(app.AppName);
        Assert.Null(app.AppVersion);
    }

    // EpicInstalledAppList tests

    [Fact]
    public void EpicInstalledAppListDefaultInstallationListIsNull()
    {
        var list = new EpicInstalledAppList();
        Assert.Null(list.InstallationList);
    }

    [Fact]
    public void EpicInstalledAppListDeserializeFromJson()
    {
        const string json = """
                            {
                                "InstallationList": [
                                    { "InstallLocation": "C:\\Games\\Game1", "AppName": "Game1", "AppVersion": "1.0" },
                                    { "InstallLocation": "C:\\Games\\Game2", "AppName": "Game2", "AppVersion": "2.0" }
                                ]
                            }
                            """;

        var list = JsonSerializer.Deserialize<EpicInstalledAppList>(json, JsonOptions);

        Assert.NotNull(list);
        Assert.Equal(2, list.InstallationList.Count);
        Assert.Equal("Game1", list.InstallationList[0].AppName);
        Assert.Equal("Game2", list.InstallationList[1].AppName);
    }

    [Fact]
    public void EpicInstalledAppListDeserializeEmptyList()
    {
        const string json = """{"InstallationList": []}""";
        var list = JsonSerializer.Deserialize<EpicInstalledAppList>(json, JsonOptions);

        Assert.NotNull(list);
        Assert.Empty(list.InstallationList);
    }

    // BNetAppDef tests

    [Fact]
    public void BNetAppDefDefaultValues()
    {
        var app = new BNetAppDef();
        Assert.Null(app.InternalId);
        Assert.Null(app.Name);
        Assert.False(app.IsClassic);
        Assert.Null(app.Exe);
        Assert.Null(app.ProductId);
    }

    [Fact]
    public void BNetAppDefPropertiesCanBeSet()
    {
        var app = new BNetAppDef
        {
            InternalId = "wow",
            Name = "World of Warcraft",
            IsClassic = false,
            Exe = "Wow.exe",
            ProductId = "WoW"
        };

        Assert.Equal("wow", app.InternalId);
        Assert.Equal("World of Warcraft", app.Name);
        Assert.False(app.IsClassic);
        Assert.Equal("Wow.exe", app.Exe);
        Assert.Equal("WoW", app.ProductId);
    }

    [Fact]
    public void BNetAppDefIsClassicCanBeSetToTrue()
    {
        var app = new BNetAppDef { IsClassic = true };
        Assert.True(app.IsClassic);
    }

    // RockstarGameDef tests

    [Fact]
    public void RockstarGameDefDefaultValues()
    {
        var game = new RockstarGameDef();
        Assert.Null(game.TitleId);
        Assert.Null(game.Name);
        Assert.Null(game.Exe);
    }

    [Fact]
    public void RockstarGameDefPropertiesCanBeSet()
    {
        var game = new RockstarGameDef
        {
            TitleId = "gta5",
            Name = "Grand Theft Auto V",
            Exe = "GTA5.exe"
        };

        Assert.Equal("gta5", game.TitleId);
        Assert.Equal("Grand Theft Auto V", game.Name);
        Assert.Equal("GTA5.exe", game.Exe);
    }

    // GameImageApiResponse tests

    [Fact]
    public void GameImageApiResponseDefaultValues()
    {
        var response = new GameImageApiResponse();
        Assert.False(response.Success);
        Assert.Null(response.ImageUrl);
    }

    [Fact]
    public void GameImageApiResponsePropertiesCanBeSet()
    {
        var response = new GameImageApiResponse
        {
            Success = true,
            ImageUrl = "https://example.com/image.png"
        };

        Assert.True(response.Success);
        Assert.Equal("https://example.com/image.png", response.ImageUrl);
    }

    [Fact]
    public void GameImageApiResponseDeserializeFromJson()
    {
        const string json = """
                            {
                                "success": true,
                                "imageUrl": "https://example.com/cover.png"
                            }
                            """;

        var response = JsonSerializer.Deserialize<GameImageApiResponse>(json, JsonOptions);

        Assert.NotNull(response);
        Assert.True(response.Success);
        Assert.Equal("https://example.com/cover.png", response.ImageUrl);
    }

    [Fact]
    public void GameImageApiResponseDeserializeFailedResponse()
    {
        const string json = """
                            {
                                "success": false,
                                "imageUrl": null
                            }
                            """;

        var response = JsonSerializer.Deserialize<GameImageApiResponse>(json, JsonOptions);

        Assert.NotNull(response);
        Assert.False(response.Success);
        Assert.Null(response.ImageUrl);
    }
}
