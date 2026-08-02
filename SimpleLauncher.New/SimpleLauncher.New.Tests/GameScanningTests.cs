using SimpleLauncher.Core.Models;
using SimpleLauncher.New.ViewModels;

namespace SimpleLauncher.New.Tests;

/// <summary>
/// Verifies that games are enumerated strictly from the system's configured folder path,
/// filtered by the system's file formats (resolves %BASEFOLDER% / relative paths).
/// </summary>
public class GameScanningTests
{
    [Fact]
    public void EnumerateSystemFiles_ReturnsOnlyMatchingFilesFromSystemFolder()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"sln_scan_{Guid.NewGuid():N}");
        var romFolder = Path.Combine(baseDir, "roms", "Atari 2600");
        Directory.CreateDirectory(romFolder);
        try
        {
            // Games for Atari 2600
            File.WriteAllText(Path.Combine(romFolder, "Combat.a26"), "");
            File.WriteAllText(Path.Combine(romFolder, "Pac-Man.a26"), "");

            // Non-matching files in the same folder (must be ignored)
            File.WriteAllText(Path.Combine(romFolder, "readme.txt"), "");

            // Games for a different system in a sibling folder (must NOT leak in)
            var otherFolder = Path.Combine(baseDir, "roms", "NES");
            Directory.CreateDirectory(otherFolder);
            File.WriteAllText(Path.Combine(otherFolder, "SuperMario.nes"), "");

            var system = new SystemManagerConfig
            {
                SystemName = "Atari 2600",
                SystemFolders = [romFolder],
                SystemImageFolder = "",
                FileFormatsToSearch = ["a26"],
                FileFormatsToLaunch = ["a26"],
                Emulators = []
            };

            var files = MainViewModel.EnumerateSystemFiles(system).ToList();

            Assert.Equal(2, files.Count);
            Assert.All(files, f => Assert.StartsWith(romFolder, f, StringComparison.OrdinalIgnoreCase));
            Assert.All(files, f => Assert.EndsWith(".a26", f, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            try
            {
                Directory.Delete(baseDir, true);
            }
            catch
            {
                /* best effort */
            }
        }
    }

    [Fact]
    public void EnumerateSystemFiles_RecursiveAndTopOnly_RespectDisableRecursiveSearch()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), $"sln_scan_{Guid.NewGuid():N}");
        var romFolder = Path.Combine(baseDir, "roms", "NES");
        var subFolder = Path.Combine(romFolder, "Subdir");
        Directory.CreateDirectory(subFolder);
        try
        {
            File.WriteAllText(Path.Combine(romFolder, "Top.nes"), "");
            File.WriteAllText(Path.Combine(subFolder, "Nested.nes"), "");

            var recursive = new SystemManagerConfig
            {
                SystemName = "NES",
                SystemFolders = [romFolder],
                SystemImageFolder = "",
                FileFormatsToSearch = ["nes"],
                FileFormatsToLaunch = ["nes"],
                DisableRecursiveSearch = false,
                Emulators = []
            };
            Assert.Equal(2, MainViewModel.EnumerateSystemFiles(recursive).Count());

            var topOnly = new SystemManagerConfig
            {
                SystemName = "NES",
                SystemFolders = [romFolder],
                SystemImageFolder = "",
                FileFormatsToSearch = ["nes"],
                FileFormatsToLaunch = ["nes"],
                DisableRecursiveSearch = true,
                Emulators = []
            };
            Assert.Single(MainViewModel.EnumerateSystemFiles(topOnly));
        }
        finally
        {
            try
            {
                Directory.Delete(baseDir, true);
            }
            catch
            {
                /* best effort */
            }
        }
    }

    [Fact]
    public void EnumerateSystemFiles_MissingFolder_YieldsNothing()
    {
        var missing = Path.Combine(Path.GetTempPath(), $"sln_missing_{Guid.NewGuid():N}");

        var system = new SystemManagerConfig
        {
            SystemName = "Ghost",
            SystemFolders = [missing],
            SystemImageFolder = "",
            FileFormatsToSearch = ["zip"],
            FileFormatsToLaunch = ["zip"],
            Emulators = []
        };

        Assert.Empty(MainViewModel.EnumerateSystemFiles(system));
    }
}
