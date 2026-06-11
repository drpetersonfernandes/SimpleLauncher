using SimpleLauncher.Services.GameScan;
using Xunit;

namespace SimpleLauncher.Tests;

public class GameScannerServiceTests : IDisposable
{
    private readonly string _testDirectory;

    public GameScannerServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_GameScanTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
        }
        catch
        {
            // Best-effort cleanup
        }

        GC.SuppressFinalize(this);
    }

    // IgnoredGameNames tests

    [Fact]
    public void IgnoredGameNamesContainsSteamworksCommonRedistributables()
    {
        Assert.Contains("Steamworks Common Redistributables", GameScannerService.IgnoredGameNames);
    }

    [Fact]
    public void IgnoredGameNamesContainsUnrealEngine()
    {
        Assert.Contains("Unreal Engine", GameScannerService.IgnoredGameNames);
    }

    [Fact]
    public void IgnoredGameNamesContainsDirectX()
    {
        Assert.Contains("DirectX", GameScannerService.IgnoredGameNames);
    }

    [Fact]
    public void IgnoredGameNamesContainsSpacewar()
    {
        Assert.Contains("Spacewar", GameScannerService.IgnoredGameNames);
    }

    [Fact]
    public void IgnoredGameNamesContainsBattleNet()
    {
        Assert.Contains("Battle.net", GameScannerService.IgnoredGameNames);
    }

    [Fact]
    public void IgnoredGameNamesContainsUbisoftConnect()
    {
        Assert.Contains("Ubisoft Connect", GameScannerService.IgnoredGameNames);
    }

    [Fact]
    public void IgnoredGameNamesContainsRockstarGamesLauncher()
    {
        Assert.Contains("Rockstar Games Launcher", GameScannerService.IgnoredGameNames);
    }

    [Fact]
    public void IgnoredGameNamesIsCaseInsensitive()
    {
        Assert.Contains("steamworks common redistributables", GameScannerService.IgnoredGameNames);
        Assert.Contains("STEAMWORKS COMMON REDISTRIBUTABLES", GameScannerService.IgnoredGameNames);
    }

    [Fact]
    public void IgnoredGameNamesDoesNotContainRandomString()
    {
        Assert.DoesNotContain("Some Random Game", GameScannerService.IgnoredGameNames);
    }

    [Fact]
    public void IgnoredGameNamesHasExpectedCount()
    {
        Assert.True(GameScannerService.IgnoredGameNames.Count >= 11, $"Expected at least 11 ignored names, got {GameScannerService.IgnoredGameNames.Count}");
    }

    // FindMainExecutable tests (via reflection since it's private static)

    [Fact]
    public void FindMainExecutableReturnsNullForNonExistentDirectory()
    {
        var method = typeof(GameScannerService).GetMethod("FindMainExecutable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);

        var result = method.Invoke(null, [Path.Combine(_testDirectory, "nonexistent"), "game", null]);
        Assert.Null(result);
    }

    [Fact]
    public void FindMainExecutableReturnsNullForEmptyDirectory()
    {
        var gameDir = Path.Combine(_testDirectory, "game");
        Directory.CreateDirectory(gameDir);

        var method = typeof(GameScannerService).GetMethod("FindMainExecutable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);

        var result = method.Invoke(null, [gameDir, "game", null]);
        Assert.Null(result);
    }

    [Fact]
    public void FindMainExecutableReturnsNameMatchExe()
    {
        var gameDir = Path.Combine(_testDirectory, "MyGame");
        Directory.CreateDirectory(gameDir);
        File.WriteAllText(Path.Combine(gameDir, "MyGame.exe"), "fake exe");
        File.WriteAllText(Path.Combine(gameDir, "other.exe"), "other exe");

        var method = typeof(GameScannerService).GetMethod("FindMainExecutable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);

        var result = method.Invoke(null, [gameDir, "MyGame", null]) as string;
        Assert.NotNull(result);
        Assert.Equal("MyGame.exe", Path.GetFileName(result));
    }

    [Fact]
    public void FindMainExecutableReturnsContainsMatchExe()
    {
        var gameDir = Path.Combine(_testDirectory, "GameDir");
        Directory.CreateDirectory(gameDir);
        File.WriteAllText(Path.Combine(gameDir, "SuperMyGameLauncher.exe"), "fake exe");
        File.WriteAllText(Path.Combine(gameDir, "other.exe"), "other exe");

        var method = typeof(GameScannerService).GetMethod("FindMainExecutable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);

        var result = method.Invoke(null, [gameDir, "MyGame", null]) as string;
        Assert.NotNull(result);
        Assert.Contains("MyGame", Path.GetFileName(result));
    }

    [Fact]
    public void FindMainExecutableReturnsSpecificExePathIfProvided()
    {
        var gameDir = Path.Combine(_testDirectory, "GameDir2");
        Directory.CreateDirectory(gameDir);
        var specificExe = Path.Combine(gameDir, "specific.exe");
        File.WriteAllText(specificExe, "specific exe");
        File.WriteAllText(Path.Combine(gameDir, "MyGame.exe"), "game exe");

        var method = typeof(GameScannerService).GetMethod("FindMainExecutable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);

        var result = method.Invoke(null, [gameDir, "MyGame", specificExe]) as string;
        Assert.NotNull(result);
        Assert.Equal("specific.exe", Path.GetFileName(result));
    }

    [Fact]
    public void FindMainExecutableExcludesUninstallerExe()
    {
        var gameDir = Path.Combine(_testDirectory, "GameDir3");
        Directory.CreateDirectory(gameDir);
        File.WriteAllText(Path.Combine(gameDir, "unins000.exe"), "uninstaller");
        File.WriteAllText(Path.Combine(gameDir, "game.exe"), new string('x', 1000));

        var method = typeof(GameScannerService).GetMethod("FindMainExecutable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);

        var result = method.Invoke(null, [gameDir, "MyGame", null]) as string;
        Assert.NotNull(result);
        Assert.Equal("game.exe", Path.GetFileName(result));
    }

    [Fact]
    public void FindMainExecutableExcludesSetupExe()
    {
        var gameDir = Path.Combine(_testDirectory, "GameDir4");
        Directory.CreateDirectory(gameDir);
        File.WriteAllText(Path.Combine(gameDir, "setup.exe"), "setup");
        File.WriteAllText(Path.Combine(gameDir, "game.exe"), new string('x', 1000));

        var method = typeof(GameScannerService).GetMethod("FindMainExecutable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);

        var result = method.Invoke(null, [gameDir, "MyGame", null]) as string;
        Assert.NotNull(result);
        Assert.Equal("game.exe", Path.GetFileName(result));
    }

    [Fact]
    public void FindMainExecutableExcludesCrashReporterExe()
    {
        var gameDir = Path.Combine(_testDirectory, "GameDir5");
        Directory.CreateDirectory(gameDir);
        File.WriteAllText(Path.Combine(gameDir, "CrashReporter.exe"), "crash reporter");
        File.WriteAllText(Path.Combine(gameDir, "game.exe"), new string('x', 1000));

        var method = typeof(GameScannerService).GetMethod("FindMainExecutable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);

        var result = method.Invoke(null, [gameDir, "MyGame", null]) as string;
        Assert.NotNull(result);
        Assert.Equal("game.exe", Path.GetFileName(result));
    }

    [Fact]
    public void FindMainExecutableExcludesLauncherExe()
    {
        var gameDir = Path.Combine(_testDirectory, "GameDir6");
        Directory.CreateDirectory(gameDir);
        File.WriteAllText(Path.Combine(gameDir, "GameLauncher.exe"), "launcher");
        File.WriteAllText(Path.Combine(gameDir, "game.exe"), new string('x', 1000));

        var method = typeof(GameScannerService).GetMethod("FindMainExecutable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);

        var result = method.Invoke(null, [gameDir, "MyGame", null]) as string;
        Assert.NotNull(result);
        Assert.Equal("game.exe", Path.GetFileName(result));
    }

    [Fact]
    public void FindMainExecutableLargestExeFallback()
    {
        var gameDir = Path.Combine(_testDirectory, "GameDir7");
        Directory.CreateDirectory(gameDir);
        File.WriteAllText(Path.Combine(gameDir, "small.exe"), "small");
        File.WriteAllText(Path.Combine(gameDir, "large.exe"), new string('x', 5000));

        var method = typeof(GameScannerService).GetMethod("FindMainExecutable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);

        var result = method.Invoke(null, [gameDir, "UnknownGame", null]) as string;
        Assert.NotNull(result);
        Assert.Equal("large.exe", Path.GetFileName(result));
    }

    [Fact]
    public void FindMainExecutableSpecificPathDoesNotExistFallsBackToHeuristics()
    {
        var gameDir = Path.Combine(_testDirectory, "GameDir8");
        Directory.CreateDirectory(gameDir);
        File.WriteAllText(Path.Combine(gameDir, "game.exe"), "game exe");

        var method = typeof(GameScannerService).GetMethod("FindMainExecutable", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.NotNull(method);

        var nonExistentPath = Path.Combine(gameDir, "nonexistent.exe");
        var result = method.Invoke(null, [gameDir, "game", nonExistentPath]) as string;
        Assert.NotNull(result);
        Assert.Equal("game.exe", Path.GetFileName(result));
    }
}
