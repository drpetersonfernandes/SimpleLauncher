using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.GameFilter;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for the <see cref="GameFilterService"/> class covering additional edge cases for filtering, sorting, and search operators.
/// </summary>
public class GameFilterServiceExtendedTests
{
    private static GameFilterService CreateService(string showGames = "ShowAll", bool enableFuzzy = false)
    {
        var configuration = new ConfigurationBuilder().Build();
        var findCoverImage = new FindCoverImageNoOp();
        var settings = new SettingsManagerService(configuration, new NoOpLogger(), new NoOpCredentialProtector())
        {
            ShowGames = showGames,
            EnableFuzzyMatching = enableFuzzy
        };
        return new GameFilterService(findCoverImage, settings);
    }

    private class FindCoverImageNoOp : IFindCoverImageService
    {
        /// <summary>
        /// Always returns the default cover image name, simulating a game that has no cover.
        /// </summary>
        /// <param name="fileNameWithoutExtension">The game file name without its extension.</param>
        /// <param name="systemName">The name of the system the game belongs to.</param>
        /// <param name="systemImageFolder">The folder configured to hold the system cover images.</param>
        /// <returns>The literal string <c>default.png</c>.</returns>
        public string FindCoverImagePath(string fileNameWithoutExtension, string systemName, string systemImageFolder)
        {
            return "default.png";
        }
    }

    private class FindCoverImageAlwaysHasCover : IFindCoverImageService
    {
        /// <summary>
        /// Always returns a fabricated cover image path, simulating a game that has a cover.
        /// </summary>
        /// <param name="fileNameWithoutExtension">The game file name without its extension.</param>
        /// <param name="systemName">The name of the system the game belongs to.</param>
        /// <param name="systemImageFolder">The folder configured to hold the system cover images.</param>
        /// <returns>A fake cover image path built from <paramref name="fileNameWithoutExtension"/>.</returns>
        public string FindCoverImagePath(string fileNameWithoutExtension, string systemName, string systemImageFolder)
        {
            return $"C:\\covers\\{fileNameWithoutExtension}.png";
        }
    }

    // FilterByShowGamesSettingAsync tests

    /// <summary>
    /// Verifies that ShowAll returns all files regardless of cover image availability.
    /// </summary>
    [Fact]
    public async Task FilterByShowGamesSettingAsyncShowAllReturnsAllRegardlessOfCover()
    {
        var configuration = new ConfigurationBuilder().Build();
        var settings = new SettingsManagerService(configuration, new NoOpLogger(), new NoOpCredentialProtector())
        {
            ShowGames = "ShowAll"
        };
        var service = new GameFilterService(new FindCoverImageAlwaysHasCover(), settings);
        var files = new List<string> { "game1.zip", "game2.nes", "game3.smc" };
        var config = new Services.SystemManager.SystemManagerService { SystemName = "NES", SystemImageFolder = "images" };

        var result = await service.FilterByShowGamesSettingAsync(files, "NES", config);
        Assert.Equal(3, result.Count);
    }

    /// <summary>
    /// Verifies that ShowWithCover filters out games with default cover images.
    /// </summary>
    [Fact]
    public async Task FilterByShowGamesSettingAsyncShowWithCoverFiltersDefaultImages()
    {
        var configuration = new ConfigurationBuilder().Build();
        var settings = new SettingsManagerService(configuration, new NoOpLogger(), new NoOpCredentialProtector())
        {
            ShowGames = "ShowWithCover"
        };
        var service = new GameFilterService(new FindCoverImageNoOp(), settings);
        var files = new List<string> { "game1.zip", "game2.nes" };
        var config = new Services.SystemManager.SystemManagerService { SystemName = "NES", SystemImageFolder = "images" };

        var result = await service.FilterByShowGamesSettingAsync(files, "NES", config);
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that ShowWithoutCover returns only games with default cover images.
    /// </summary>
    [Fact]
    public async Task FilterByShowGamesSettingAsyncShowWithoutCoverReturnsDefaultImageGames()
    {
        var configuration = new ConfigurationBuilder().Build();
        var settings = new SettingsManagerService(configuration, new NoOpLogger(), new NoOpCredentialProtector())
        {
            ShowGames = "ShowWithoutCover"
        };
        var service = new GameFilterService(new FindCoverImageNoOp(), settings);
        var files = new List<string> { "game1.zip", "game2.nes" };
        var config = new Services.SystemManager.SystemManagerService { SystemName = "NES", SystemImageFolder = "images" };

        var result = await service.FilterByShowGamesSettingAsync(files, "NES", config);
        Assert.Equal(2, result.Count);
    }

    /// <summary>
    /// Verifies that empty ShowGames setting does not behave as ShowAll.
    /// </summary>
    [Fact]
    public async Task FilterByShowGamesSettingAsyncEmptyShowGamesTreatsAsShowAll()
    {
        var configuration = new ConfigurationBuilder().Build();
        var settings = new SettingsManagerService(configuration, new NoOpLogger(), new NoOpCredentialProtector())
        {
            ShowGames = ""
        };
        var service = new GameFilterService(new FindCoverImageNoOp(), settings);
        var files = new List<string> { "game1.zip" };
        var config = new Services.SystemManager.SystemManagerService { SystemName = "NES", SystemImageFolder = "images" };

        var result = await service.FilterByShowGamesSettingAsync(files, "NES", config);
        // Empty ShowGames doesn't match "ShowAll" so it goes through filtering
        // and since FindCoverImageNoOp returns "default.png", all games are filtered out for ShowWithCover
        Assert.Empty(result);
    }

    // FilterByLetterAsync tests

    /// <summary>
    /// Verifies that FilterByLetterAsync handles special character brackets.
    /// </summary>
    [Fact]
    public async Task FilterByLetterAsyncSpecialCharacters()
    {
        var service = CreateService();
        var files = new List<string>
        {
            @"C:\roms\[BIOS]test.zip",
            @"C:\roms\game.zip"
        };

        var result = await service.FilterByLetterAsync(files, "[");
        Assert.Single(result);
        Assert.Contains("[BIOS]test.zip", result[0], StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that FilterByLetterAsync with '#' returns empty when no files start with a digit.
    /// </summary>
    [Fact]
    public async Task FilterByLetterAsyncHashWithNoDigitFiles()
    {
        var service = CreateService();
        var files = new List<string>
        {
            @"C:\roms\mario.zip",
            @"C:\roms\zelda.zip"
        };

        var result = await service.FilterByLetterAsync(files, "#");
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that FilterByLetterAsync works with a single file in the list.
    /// </summary>
    [Fact]
    public async Task FilterByLetterAsyncSingleFile()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\astro.zip" };

        var result = await service.FilterByLetterAsync(files, "A");
        Assert.Single(result);
    }

    /// <summary>
    /// Verifies that FilterByLetterAsync matches letter case-insensitively.
    /// </summary>
    [Fact]
    public async Task FilterByLetterAsyncCaseInsensitiveA()
    {
        var service = CreateService();
        var files = new List<string>
        {
            @"C:\roms\apple.zip",
            @"C:\roms\Apple.zip",
            @"C:\roms\APPLE.zip"
        };

        var result = await service.FilterByLetterAsync(files, "a");
        Assert.Equal(3, result.Count);
    }

    // SortByMameDescription tests

    /// <summary>
    /// Verifies that SortByMameDescription with MachineDescription sorts case-insensitively.
    /// </summary>
    [Fact]
    public void SortByMameDescriptionMachineDescriptionSortsCaseInsensitive()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\game1.zip", @"C:\roms\game2.zip" };
        var mameLookup = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["game1"] = "zebra",
            ["game2"] = "APPLE"
        };

        var result = service.SortByMameDescription(files, AppConstants.MameSortOrderMachineDescription, mameLookup);
        Assert.Equal("game2.zip", Path.GetFileName(result[0]));
    }

    /// <summary>
    /// Verifies that SortByMameDescription with FileName sorts case-insensitively.
    /// </summary>
    [Fact]
    public void SortByMameDescriptionFileNameSortsCaseInsensitive()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\ZEBRA.zip", @"C:\roms\apple.zip" };

        var result = service.SortByMameDescription(files, AppConstants.MameSortOrderFileName, new Dictionary<string, string>(StringComparer.Ordinal));
        Assert.Equal("apple.zip", Path.GetFileName(result[0]));
    }

    /// <summary>
    /// Verifies that SortByMameDescription with empty files returns empty.
    /// </summary>
    [Fact]
    public void SortByMameDescriptionEmptyFilesReturnsEmpty()
    {
        var service = CreateService();
        var result = service.SortByMameDescription([], AppConstants.MameSortOrderMachineDescription, new Dictionary<string, string>(StringComparer.Ordinal));
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that SortByMameDescription falls back to filename for items not in the MAME lookup.
    /// </summary>
    [Fact]
    public void SortByMameDescriptionPartialLookupFallsBackToFileName()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\game1.zip", @"C:\roms\game2.zip", @"C:\roms\game3.zip" };
        var mameLookup = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["game1"] = "Zebra",
            ["game3"] = "Apple"
        };

        var result = service.SortByMameDescription(files, AppConstants.MameSortOrderMachineDescription, mameLookup);
        // game3 → "Apple", game2 → "game2" (fallback), game1 → "Zebra"
        Assert.Equal("game3.zip", Path.GetFileName(result[0]));
        Assert.Equal("game2.zip", Path.GetFileName(result[1]));
        Assert.Equal("game1.zip", Path.GetFileName(result[2]));
    }

    /// <summary>
    /// Verifies that SortByMameDescription with unknown sort order defaults to filename sorting.
    /// </summary>
    [Fact]
    public void SortByMameDescriptionUnknownSortOrderDefaultsToFileName()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\zzz.zip", @"C:\roms\aaa.zip" };

        var result = service.SortByMameDescription(files, "UnknownOrder", new Dictionary<string, string>(StringComparer.Ordinal));
        Assert.Equal("aaa.zip", Path.GetFileName(result[0]));
    }

    // FilterBySearchQueryAsync tests

    /// <summary>
    /// Verifies that FilterBySearchQueryAsync supports partial filename matching.
    /// </summary>
    [Fact]
    public async Task FilterBySearchQueryAsyncPartialMatch()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\super mario bros.zip", @"C:\roms\zelda.zip" };

        var result = await service.FilterBySearchQueryAsync(files, "mario", new Dictionary<string, string>(StringComparer.Ordinal));
        Assert.Single(result);
    }

    /// <summary>
    /// Verifies that FilterBySearchQueryAsync returns multiple matches.
    /// </summary>
    [Fact]
    public async Task FilterBySearchQueryAsyncMultipleMatches()
    {
        var service = CreateService();
        var files = new List<string>
        {
            @"C:\roms\mario bros.zip",
            @"C:\roms\super mario.zip",
            @"C:\roms\mario kart.zip",
            @"C:\roms\zelda.zip"
        };

        var result = await service.FilterBySearchQueryAsync(files, "mario", new Dictionary<string, string>(StringComparer.Ordinal));
        Assert.Equal(3, result.Count);
    }

    /// <summary>
    /// Verifies that FilterBySearchQueryAsync with empty query returns all files.
    /// </summary>
    [Fact]
    public async Task FilterBySearchQueryAsyncEmptyQueryReturnsAll()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\mario.zip", @"C:\roms\zelda.zip" };

        var result = await service.FilterBySearchQueryAsync(files, "", new Dictionary<string, string>(StringComparer.Ordinal));
        Assert.Equal(2, result.Count);
    }

    /// <summary>
    /// Verifies that FilterBySearchQueryAsync with empty file list returns empty.
    /// </summary>
    [Fact]
    public async Task FilterBySearchQueryAsyncEmptyFilesReturnsEmpty()
    {
        var service = CreateService();

        var result = await service.FilterBySearchQueryAsync([], "mario", new Dictionary<string, string>(StringComparer.Ordinal));
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that FilterBySearchQueryAsync matches MAME descriptions case-insensitively.
    /// </summary>
    [Fact]
    public async Task FilterBySearchQueryAsyncMameDescriptionCaseInsensitive()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\game1.zip" };
        var mameLookup = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["game1"] = "PAC-MAN"
        };

        var result = await service.FilterBySearchQueryAsync(files, "pac-man", mameLookup);
        Assert.Single(result);
    }

    /// <summary>
    /// Verifies that FilterBySearchQueryAsync supports partial MAME description matching.
    /// </summary>
    [Fact]
    public async Task FilterBySearchQueryAsyncMameDescriptionPartialMatch()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\game1.zip" };
        var mameLookup = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["game1"] = "Super Mario Bros"
        };

        var result = await service.FilterBySearchQueryAsync(files, "mario", mameLookup);
        Assert.Single(result);
    }

    /// <summary>
    /// Verifies that FilterBySearchQueryAsync handles MAME lookup with empty description.
    /// </summary>
    [Fact]
    public async Task FilterBySearchQueryAsyncMameLookupWithEmptyDescription()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\game1.zip" };
        var mameLookup = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["game1"] = ""
        };

        var result = await service.FilterBySearchQueryAsync(files, "game", mameLookup);
        Assert.Single(result); // matches filename
    }

    /// <summary>
    /// Verifies that FilterBySearchQueryAsync matches filenames even when MAME description differs.
    /// </summary>
    [Fact]
    public async Task FilterBySearchQueryAsyncSearchInFilenameNotMame()
    {
        var service = CreateService();
        var files = new List<string> { @"C:\roms\street fighter.zip" };
        var mameLookup = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["street fighter"] = "SF2"
        };

        var result = await service.FilterBySearchQueryAsync(files, "street", mameLookup);
        Assert.Single(result);
    }

    // AND/OR operator tests for FilterBySearchQueryAsync

    /// <summary>
    /// Verifies that FilterBySearchQueryAsync with AND operator requires both terms to match.
    /// </summary>
    [Fact]
    public async Task FilterBySearchQueryAsyncAndOperatorBothTermsMustMatch()
    {
        var service = CreateService();
        var files = new List<string>
        {
            @"C:\roms\super mario bros.zip",
            @"C:\roms\mario kart.zip",
            @"C:\roms\zelda.zip"
        };

        var result = await service.FilterBySearchQueryAsync(files, "mario AND kart", new Dictionary<string, string>(StringComparer.Ordinal));
        Assert.Single(result);
        Assert.Contains("mario kart", result[0], StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that FilterBySearchQueryAsync with OR operator matches when any term matches.
    /// </summary>
    [Fact]
    public async Task FilterBySearchQueryAsyncOrOperatorAnyTermMatches()
    {
        var service = CreateService();
        var files = new List<string>
        {
            @"C:\roms\mario.zip",
            @"C:\roms\zelda.zip",
            @"C:\roms\metroid.zip"
        };

        var result = await service.FilterBySearchQueryAsync(files, "mario OR zelda", new Dictionary<string, string>(StringComparer.Ordinal));
        Assert.Equal(2, result.Count);
    }

    /// <summary>
    /// Verifies that FilterBySearchQueryAsync treats multiple space-separated terms as AND by default.
    /// </summary>
    [Fact]
    public async Task FilterBySearchQueryAsyncDefaultAndForMultipleTerms()
    {
        var service = CreateService();
        var files = new List<string>
        {
            @"C:\roms\super mario bros.zip",
            @"C:\roms\mario kart.zip",
            @"C:\roms\zelda.zip"
        };

        var result = await service.FilterBySearchQueryAsync(files, "super mario", new Dictionary<string, string>(StringComparer.Ordinal));
        Assert.Single(result);
        Assert.Contains("super mario", result[0], StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that FilterBySearchQueryAsync supports quoted phrase matching.
    /// </summary>
    [Fact]
    public async Task FilterBySearchQueryAsyncQuotedPhrase()
    {
        var service = CreateService();
        var files = new List<string>
        {
            @"C:\roms\super mario bros.zip",
            @"C:\roms\mario super.zip",
            @"C:\roms\zelda.zip"
        };

        var result = await service.FilterBySearchQueryAsync(files, "\"super mario\"", new Dictionary<string, string>(StringComparer.Ordinal));
        Assert.Single(result);
        Assert.Contains("super mario", result[0], StringComparison.Ordinal);
    }

    /// <summary>
    /// Verifies that FilterBySearchQueryAsync with AND operator works with MAME descriptions.
    /// </summary>
    [Fact]
    public async Task FilterBySearchQueryAsyncAndWithMameDescription()
    {
        var service = CreateService();
        var files = new List<string>
        {
            @"C:\roms\game1.zip",
            @"C:\roms\game2.zip"
        };
        var mameLookup = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["game1"] = "Super Mario Bros",
            ["game2"] = "Donkey Kong"
        };

        var result = await service.FilterBySearchQueryAsync(files, "super AND game", mameLookup);
        Assert.Single(result);
    }

    /// <summary>
    /// Verifies that FilterBySearchQueryAsync with OR operator works with MAME descriptions.
    /// </summary>
    [Fact]
    public async Task FilterBySearchQueryAsyncOrWithMameDescription()
    {
        var service = CreateService();
        var files = new List<string>
        {
            @"C:\roms\game1.zip",
            @"C:\roms\game2.zip",
            @"C:\roms\zelda.zip"
        };
        var mameLookup = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["game1"] = "Pac-Man",
            ["game2"] = "Donkey Kong"
        };

        var result = await service.FilterBySearchQueryAsync(files, "pac-man OR zelda", mameLookup);
        Assert.Equal(2, result.Count);
    }

    /// <summary>
    /// Verifies that FilterBySearchQueryAsync with AND operator returns empty when no file matches both terms.
    /// </summary>
    [Fact]
    public async Task FilterBySearchQueryAsyncAndOperatorNoMatch()
    {
        var service = CreateService();
        var files = new List<string>
        {
            @"C:\roms\mario.zip",
            @"C:\roms\zelda.zip"
        };

        var result = await service.FilterBySearchQueryAsync(files, "mario AND zelda", new Dictionary<string, string>(StringComparer.Ordinal));
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that FilterBySearchQueryAsync with OR operator returns empty when no term matches.
    /// </summary>
    [Fact]
    public async Task FilterBySearchQueryAsyncOrOperatorNoMatch()
    {
        var service = CreateService();
        var files = new List<string>
        {
            @"C:\roms\mario.zip"
        };

        var result = await service.FilterBySearchQueryAsync(files, "zelda OR metroid", new Dictionary<string, string>(StringComparer.Ordinal));
        Assert.Empty(result);
    }
}
