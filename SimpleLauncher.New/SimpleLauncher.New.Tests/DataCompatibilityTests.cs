namespace SimpleLauncher.New.Tests;

/// <summary>
/// Verifies that SimpleLauncher.New can read data files from the existing SimpleLauncher.
/// </summary>
public class DataCompatibilityTests
{
    [Fact]
    public void FavoritesManager_CanLoadExistingFile()
    {
        // Check if the real favorites.dat exists and can be loaded
        var possiblePaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "SimpleLauncher", "bin", "Debug", "net10.0-windows"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SimpleLauncher")
        };

        foreach (var dir in possiblePaths)
        {
            var favPath = Path.Combine(dir, "favorites.dat");
            if (!File.Exists(favPath)) continue;

            // Just verify we can set up the data file location
            var tempDir = Path.Combine(Path.GetTempPath(), $"slnew_compat_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);
            try
            {
                File.Copy(favPath, Path.Combine(tempDir, "favorites.dat"), true);
                Environment.CurrentDirectory = tempDir;
                File.WriteAllText(Path.Combine(tempDir, "appsettings.json"), "{}");

                var manager = Services.Favorites.FavoritesManager.LoadFavorites();
                Assert.NotNull(manager);
                Assert.NotNull(manager.FavoriteList);
            }
            finally
            {
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch
                {
                    // ignored
                }
            }

            return; // Test passed
        }

        // No existing data file — test passes vacuously
    }

    [Fact]
    public void PlayHistoryManager_CanLoadExistingFile()
    {
        var possiblePaths = new[]
        {
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "SimpleLauncher", "bin", "Debug", "net10.0-windows"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "SimpleLauncher")
        };

        foreach (var dir in possiblePaths)
        {
            var histPath = Path.Combine(dir, "playhistory.dat");
            if (!File.Exists(histPath)) continue;

            var tempDir = Path.Combine(Path.GetTempPath(), $"slnew_compat_hist_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);
            try
            {
                File.Copy(histPath, Path.Combine(tempDir, "playhistory.dat"), true);
                Environment.CurrentDirectory = tempDir;
                File.WriteAllText(Path.Combine(tempDir, "appsettings.json"), "{}");

                var manager = Services.PlayHistory.PlayHistoryManager.LoadPlayHistory();
                Assert.NotNull(manager);
                Assert.NotNull(manager.PlayHistoryList);
            }
            finally
            {
                try
                {
                    Directory.Delete(tempDir, true);
                }
                catch
                {
                    // ignored
                }
            }

            return;
        }
    }

    [Fact]
    public void GameCardViewModel_SampleData_IsValid()
    {
        var games = ViewModels.GameCardViewModel.CreateSampleData(10);
        Assert.Equal(10, games.Count);
        Assert.All(games, g => Assert.False(string.IsNullOrEmpty(g.DisplayTitle)));
        Assert.All(games, g => Assert.False(string.IsNullOrEmpty(g.SystemName)));
        Assert.All(games, g => Assert.False(string.IsNullOrEmpty(g.FilePath)));
    }
}
