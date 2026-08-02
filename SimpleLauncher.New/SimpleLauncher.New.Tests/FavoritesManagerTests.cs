using SimpleLauncher.New.Services.Favorites;

namespace SimpleLauncher.New.Tests;

[Collection("Sequential")]
public class FavoritesManagerTests : IDisposable
{
    private readonly string _favPath;

    public FavoritesManagerTests()
    {
        // DataFileLocation uses AppDomain.CurrentDomain.BaseDirectory, not Environment.CurrentDirectory.
        // We write directly to the test output directory to control the file.
        _favPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "favorites.dat");
        // Clean slate for each test
        try
        {
            File.Delete(_favPath);
        }
        catch
        {
            // ignored
        }
    }

    [Fact]
    public async Task AddFavorite_NewGame_ReturnsTrue()
    {
        var manager = FavoritesManager.LoadFavorites();
        var result = await manager.AddFavoriteAsync(@"C:\roms\NES\mario.zip", "NES");

        Assert.True(result);
        Assert.True(manager.IsFavorite(@"C:\roms\NES\mario.zip"));
    }

    [Fact]
    public async Task AddFavorite_Duplicate_ReturnsFalse()
    {
        var manager = FavoritesManager.LoadFavorites();
        await manager.AddFavoriteAsync(@"C:\roms\NES\mario.zip", "NES");
        var result = await manager.AddFavoriteAsync(@"C:\roms\NES\mario.zip", "NES");

        Assert.False(result);
    }

    [Fact]
    public async Task Toggle_AddsThenRemoves()
    {
        var manager = FavoritesManager.LoadFavorites();

        var added = await manager.ToggleAsync(@"C:\roms\SNES\zelda.sfc", "SNES");
        Assert.True(added);
        Assert.True(manager.IsFavorite(@"C:\roms\SNES\zelda.sfc"));

        var removed = await manager.ToggleAsync(@"C:\roms\SNES\zelda.sfc", "SNES");
        Assert.False(removed);
        Assert.False(manager.IsFavorite(@"C:\roms\SNES\zelda.sfc"));
    }

    [Fact]
    public async Task GetFavoritePaths_ReturnsAllPaths()
    {
        var manager = FavoritesManager.LoadFavorites();
        await manager.AddFavoriteAsync(@"C:\roms\NES\a.zip", "NES");
        await manager.AddFavoriteAsync(@"C:\roms\SNES\b.sfc", "SNES");

        var paths = manager.GetFavoritePaths();
        Assert.Contains(@"C:\roms\NES\a.zip", paths);
        Assert.Contains(@"C:\roms\SNES\b.sfc", paths);
    }

    [Fact]
    public async Task IsFavorite_CaseInsensitive()
    {
        var manager = FavoritesManager.LoadFavorites();
        await manager.AddFavoriteAsync(@"C:\roms\NES\GAME.ZIP", "NES");

        Assert.True(manager.IsFavorite(@"c:\roms\nes\game.zip"));
    }

    [Fact]
    public async Task LoadFavorites_PersistsAcrossInstances()
    {
        var manager1 = FavoritesManager.LoadFavorites();
        await manager1.AddFavoriteAsync(@"C:\roms\NES\persist.zip", "NES");

        var manager2 = FavoritesManager.LoadFavorites();
        Assert.True(manager2.IsFavorite(@"C:\roms\NES\persist.zip"));
    }

    public void Dispose()
    {
        try
        {
            File.Delete(_favPath);
        }
        catch
        {
            // ignored
        }
    }
}
