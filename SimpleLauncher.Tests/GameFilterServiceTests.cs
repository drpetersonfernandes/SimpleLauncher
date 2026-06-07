using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameFilter;
using Xunit;

namespace SimpleLauncher.Tests;

public class GameFilterServiceTests
{
    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }

    private static GameFilterService CreateService(string showGames = "ShowAll", bool enableFuzzy = false)
    {
        var configuration = new ConfigurationBuilder().Build();
        var findCoverImage = new FindCoverImageNoOp();
        var settings = new Services.SettingsManager.SettingsManager(configuration, new NoOpLogErrors())
        {
            ShowGames = showGames,
            EnableFuzzyMatching = enableFuzzy
        };
        return new GameFilterService(findCoverImage, settings);
    }

    [Fact]
    public async Task FilterByShowGamesSettingAsyncShowAllReturnsAllFiles()
    {
        var service = CreateService();
        var files = new List<string> { "game1.zip", "game2.nes", "game3.smc" };
        var config = new Services.SystemManager.SystemManager { SystemName = "NES" };

        var result = await service.FilterByShowGamesSettingAsync(files, "NES", config);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task FilterByShowGamesSettingAsyncEmptyListReturnsEmpty()
    {
        var service = CreateService("ShowWithCover");
        var config = new Services.SystemManager.SystemManager { SystemName = "NES" };

        var result = await service.FilterByShowGamesSettingAsync([], "NES", config);
        Assert.Empty(result);
    }

    [Fact]
    public async Task FilterByLetterAsyncNullLetterReturnsAllFiles()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\game1.zip", @"C:\roms\game2.nes" };

        var result = await service.FilterByLetterAsync(files, null);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task FilterByLetterAsyncEmptyLetterReturnsAllFiles()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\game1.zip", @"C:\roms\game2.nes" };

        var result = await service.FilterByLetterAsync(files, "");
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task FilterByLetterAsyncHashReturnsDigitFiles()
    {
        var service = CreateService();
        var files = new List<string>
        {
            @"C:\roms\1942.zip",
            @"C:\roms\mario.zip",
            @"C:\roms\2048.zip",
            @"C:\roms\zelda.zip"
        };

        var result = await service.FilterByLetterAsync(files, "#");
        Assert.Equal(2, result.Count);
        Assert.All(result, static f => Assert.True(char.IsDigit(Path.GetFileName(f)[0])));
    }

    [Fact]
    public async Task FilterByLetterAsyncLetterReturnsMatchingFiles()
    {
        var service = CreateService();
        var files = new List<string>
        {
            @"C:\roms\mario.zip",
            @"C:\roms\mega man.zip",
            @"C:\roms\zelda.zip",
            @"C:\roms\Metroid.zip"
        };

        var result = await service.FilterByLetterAsync(files, "M");
        Assert.Equal(3, result.Count); // mario, mega man, Metroid (case-insensitive)
    }

    [Fact]
    public async Task FilterByLetterAsyncNoMatchReturnsEmpty()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\mario.zip", @"C:\roms\zelda.zip" };

        var result = await service.FilterByLetterAsync(files, "X");
        Assert.Empty(result);
    }

    [Fact]
    public void SortByMameDescriptionMachineDescriptionSortsByDescription()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\game1.zip", @"C:\roms\game2.zip" };
        var mameLookup = new Dictionary<string, string>
        {
            ["game1"] = "Zelda",
            ["game2"] = "Mario"
        };

        var result = service.SortByMameDescription(files, "MachineDescription", mameLookup);
        Assert.Equal("game2.zip", Path.GetFileName(result[0]));
    }

    [Fact]
    public void SortByMameDescriptionMachineDescriptionFallsBackToFileName()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\beta.zip", @"C:\roms\alpha.zip" };
        var mameLookup = new Dictionary<string, string>();

        var result = service.SortByMameDescription(files, "MachineDescription", mameLookup);
        Assert.Equal("alpha.zip", Path.GetFileName(result[0]));
    }

    [Fact]
    public void SortByMameDescriptionFileNameSortsByFileName()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\zzz.zip", @"C:\roms\aaa.zip" };

        var result = service.SortByMameDescription(files, "FileName", new Dictionary<string, string>());
        Assert.Equal("aaa.zip", Path.GetFileName(result[0]));
    }

    [Fact]
    public async Task FilterBySearchQueryAsyncMatchesFileName()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\super mario.zip", @"C:\roms\zelda.zip" };

        var result = await service.FilterBySearchQueryAsync(files, "mario", new Dictionary<string, string>());
        Assert.Single(result);
        Assert.Contains("mario", Path.GetFileNameWithoutExtension(result[0]));
    }

    [Fact]
    public async Task FilterBySearchQueryAsyncMatchesMameDescription()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\game1.zip", @"C:\roms\game2.zip" };
        var mameLookup = new Dictionary<string, string>
        {
            ["game1"] = "Pac-Man",
            ["game2"] = "Donkey Kong"
        };

        var result = await service.FilterBySearchQueryAsync(files, "pac-man", mameLookup);
        Assert.Single(result);
    }

    [Fact]
    public async Task FilterBySearchQueryAsyncCaseInsensitive()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\Mario.zip" };

        var result = await service.FilterBySearchQueryAsync(files, "MARIO", new Dictionary<string, string>());
        Assert.Single(result);
    }

    [Fact]
    public async Task FilterBySearchQueryAsyncNoMatchReturnsEmpty()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\mario.zip" };

        var result = await service.FilterBySearchQueryAsync(files, "zelda", new Dictionary<string, string>());
        Assert.Empty(result);
    }

    [Fact]
    public async Task FilterBySearchQueryAsyncWithNullMameLookup()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\mario.zip" };

        var result = await service.FilterBySearchQueryAsync(files, "mario", null);
        Assert.Single(result);
    }

    private class FindCoverImageNoOp : Services.FindCoverImage.IFindCoverImage
    {
        public string FindCoverImagePath(string fileNameWithoutExtension, string systemName, Services.SystemManager.SystemManager systemManager, Services.SettingsManager.SettingsManager settings)
        {
            return "default.png";
        }

        public double CalculateJaroWinklerSimilarity(string s1, string s2)
        {
            return 0.0;
        }
    }
}
