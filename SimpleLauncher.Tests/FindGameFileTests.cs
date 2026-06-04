using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.GameLauncher.MountFiles;
using Xunit;

namespace SimpleLauncher.Tests;

public class FindGameFileTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ILogErrors _logErrors = new NoOpLogErrors();

    public FindGameFileTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_FindGame_{Guid.NewGuid():N}");
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

    // FindDefaultXex tests
    [Fact]
    public void FindDefaultXex_NullPath_ReturnsNull()
    {
        var result = FindDefaultXex.Find(null, _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindDefaultXex_EmptyPath_ReturnsNull()
    {
        var result = FindDefaultXex.Find("", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindDefaultXex_NonExistentDirectory_ReturnsNull()
    {
        var result = FindDefaultXex.Find(@"C:\nonexistent\dir", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindDefaultXex_FileExists_ReturnsPath()
    {
        var xexPath = Path.Combine(_testDirectory, "default.xex");
        File.WriteAllText(xexPath, "fake");

        var result = FindDefaultXex.Find(_testDirectory, _logErrors);

        Assert.NotNull(result);
        Assert.Equal(xexPath, result);
    }

    [Fact]
    public void FindDefaultXex_FileDoesNotExist_ReturnsNull()
    {
        var result = FindDefaultXex.Find(_testDirectory, _logErrors);
        Assert.Null(result);
    }

    // FindDefaultXbe tests
    [Fact]
    public void FindDefaultXbe_NullPath_ReturnsNull()
    {
        var result = FindDefaultXbe.Find(null, _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindDefaultXbe_EmptyPath_ReturnsNull()
    {
        var result = FindDefaultXbe.Find("", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindDefaultXbe_NonExistentDirectory_ReturnsNull()
    {
        var result = FindDefaultXbe.Find(@"C:\nonexistent\dir", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindDefaultXbe_FileExists_ReturnsPath()
    {
        var xbePath = Path.Combine(_testDirectory, "default.xbe");
        File.WriteAllText(xbePath, "fake");

        var result = FindDefaultXbe.Find(_testDirectory, _logErrors);

        Assert.NotNull(result);
        Assert.Equal(xbePath, result);
    }

    [Fact]
    public void FindDefaultXbe_FileDoesNotExist_ReturnsNull()
    {
        var result = FindDefaultXbe.Find(_testDirectory, _logErrors);
        Assert.Null(result);
    }

    // FindCueFile tests
    [Fact]
    public void FindCueFile_NullPath_ReturnsNull()
    {
        var result = FindCueFile.Find(null, _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindCueFile_EmptyPath_ReturnsNull()
    {
        var result = FindCueFile.Find("", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindCueFile_NonExistentDirectory_ReturnsNull()
    {
        var result = FindCueFile.Find(@"C:\nonexistent\dir", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindCueFile_FileExists_ReturnsPath()
    {
        var cuePath = Path.Combine(_testDirectory, "game.cue");
        File.WriteAllText(cuePath, "fake");

        var result = FindCueFile.Find(_testDirectory, _logErrors);

        Assert.NotNull(result);
        Assert.Equal(cuePath, result);
    }

    [Fact]
    public void FindCueFile_MultipleCueFiles_ReturnsFirst()
    {
        var cue1 = Path.Combine(_testDirectory, "disc1.cue");
        var cue2 = Path.Combine(_testDirectory, "disc2.cue");
        File.WriteAllText(cue1, "fake");
        File.WriteAllText(cue2, "fake");

        var result = FindCueFile.Find(_testDirectory, _logErrors);

        Assert.NotNull(result);
        Assert.True(result == cue1 || result == cue2);
    }

    [Fact]
    public void FindCueFile_NoCueFiles_ReturnsNull()
    {
        File.WriteAllText(Path.Combine(_testDirectory, "game.bin"), "fake");

        var result = FindCueFile.Find(_testDirectory, _logErrors);
        Assert.Null(result);
    }

    // FindBinFile tests
    [Fact]
    public void FindBinFile_NullPath_ReturnsNull()
    {
        var result = FindBinFile.Find(null, _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindBinFile_EmptyPath_ReturnsNull()
    {
        var result = FindBinFile.Find("", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindBinFile_NonExistentDirectory_ReturnsNull()
    {
        var result = FindBinFile.Find(@"C:\nonexistent\dir", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindBinFile_FileExists_ReturnsPath()
    {
        var binPath = Path.Combine(_testDirectory, "game.bin");
        File.WriteAllText(binPath, "fake");

        var result = FindBinFile.Find(_testDirectory, _logErrors);

        Assert.NotNull(result);
        Assert.Equal(binPath, result);
    }

    [Fact]
    public void FindBinFile_NoBinFiles_ReturnsNull()
    {
        File.WriteAllText(Path.Combine(_testDirectory, "game.cue"), "fake");

        var result = FindBinFile.Find(_testDirectory, _logErrors);
        Assert.Null(result);
    }

    // FindImageIso tests
    [Fact]
    public void FindImageIso_NullPath_ReturnsNull()
    {
        var result = FindImageIso.Find(null, _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindImageIso_EmptyPath_ReturnsNull()
    {
        var result = FindImageIso.Find("", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindImageIso_NonExistentDirectory_ReturnsNull()
    {
        var result = FindImageIso.Find(@"C:\nonexistent\dir", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindImageIso_FileExists_ReturnsPath()
    {
        var isoPath = Path.Combine(_testDirectory, "image.iso");
        File.WriteAllText(isoPath, "fake");

        var result = FindImageIso.Find(_testDirectory, _logErrors);

        Assert.NotNull(result);
        Assert.Equal(isoPath, result);
    }

    [Fact]
    public void FindImageIso_FileDoesNotExist_ReturnsNull()
    {
        var result = FindImageIso.Find(_testDirectory, _logErrors);
        Assert.Null(result);
    }

    // FindEbootBin tests
    [Fact]
    public void FindEbootBin_NullPath_ReturnsNull()
    {
        var result = FindEbootBin.FindEbootBinRecursive(null, _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindEbootBin_EmptyPath_ReturnsNull()
    {
        var result = FindEbootBin.FindEbootBinRecursive("", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindEbootBin_InTopDirectory_ReturnsPath()
    {
        var ebootPath = Path.Combine(_testDirectory, "EBOOT.BIN");
        File.WriteAllText(ebootPath, "fake");

        var result = FindEbootBin.FindEbootBinRecursive(_testDirectory, _logErrors);

        Assert.NotNull(result);
        Assert.Equal(ebootPath, result);
    }

    [Fact]
    public void FindEbootBin_InPS3GameUsrDir_ReturnsPath()
    {
        var ps3GameDir = Path.Combine(_testDirectory, "PS3_GAME");
        var usrDir = Path.Combine(ps3GameDir, "USRDIR");
        Directory.CreateDirectory(usrDir);
        var ebootPath = Path.Combine(usrDir, "EBOOT.BIN");
        File.WriteAllText(ebootPath, "fake");

        var result = FindEbootBin.FindEbootBinRecursive(_testDirectory, _logErrors);

        Assert.NotNull(result);
        Assert.Equal(ebootPath, result);
    }

    [Fact]
    public void FindEbootBin_InNestedDirectory_ReturnsPath()
    {
        var nestedDir = Path.Combine(_testDirectory, "subdir", "deep");
        Directory.CreateDirectory(nestedDir);
        var ebootPath = Path.Combine(nestedDir, "EBOOT.BIN");
        File.WriteAllText(ebootPath, "fake");

        var result = FindEbootBin.FindEbootBinRecursive(_testDirectory, _logErrors);

        Assert.NotNull(result);
        Assert.Equal(ebootPath, result);
    }

    [Fact]
    public void FindEbootBin_NotFound_ReturnsNull()
    {
        var result = FindEbootBin.FindEbootBinRecursive(_testDirectory, _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindEbootBin_PrefersTopDirectoryOverNested()
    {
        var topEboot = Path.Combine(_testDirectory, "EBOOT.BIN");
        File.WriteAllText(topEboot, "top");

        var nestedDir = Path.Combine(_testDirectory, "PS3_GAME", "USRDIR");
        Directory.CreateDirectory(nestedDir);
        File.WriteAllText(Path.Combine(nestedDir, "EBOOT.BIN"), "nested");

        var result = FindEbootBin.FindEbootBinRecursive(_testDirectory, _logErrors);

        Assert.NotNull(result);
        Assert.Equal(topEboot, result);
    }

    [Fact]
    public void FindEbootBin_PrefersPS3StructureOverFullRecursive()
    {
        var ps3GameDir = Path.Combine(_testDirectory, "PS3_GAME");
        var usrDir = Path.Combine(ps3GameDir, "USRDIR");
        Directory.CreateDirectory(usrDir);
        var ps3Eboot = Path.Combine(usrDir, "EBOOT.BIN");
        File.WriteAllText(ps3Eboot, "ps3");

        var deepDir = Path.Combine(_testDirectory, "other", "deep");
        Directory.CreateDirectory(deepDir);
        File.WriteAllText(Path.Combine(deepDir, "EBOOT.BIN"), "deep");

        var result = FindEbootBin.FindEbootBinRecursive(_testDirectory, _logErrors);

        Assert.NotNull(result);
        Assert.Equal(ps3Eboot, result);
    }

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }
}
