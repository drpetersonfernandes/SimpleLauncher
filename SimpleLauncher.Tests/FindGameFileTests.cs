using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.GameLauncher.MountFiles;
using Xunit;

namespace SimpleLauncher.Tests;

public class FindGameFileTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly ILogErrors _logErrors = new NoOpLogErrors();
    private readonly IDebugLogger _debugLogger = new NoOpDebugLogger();

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class NoOpDebugLogger : IDebugLogger
    {
        public void Log(string message)
        {
        }

        public void LogException(Exception ex, string? contextMessage = null)
        {
        }
    }

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
    public void FindDefaultXexNullPathReturnsNull()
    {
        var result = FindDefaultXex.Find(null, _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindDefaultXexEmptyPathReturnsNull()
    {
        var result = FindDefaultXex.Find("", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindDefaultXexNonExistentDirectoryReturnsNull()
    {
        var result = FindDefaultXex.Find(@"C:\nonexistent\dir", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindDefaultXexFileExistsReturnsPath()
    {
        var xexPath = Path.Combine(_testDirectory, "default.xex");
        File.WriteAllText(xexPath, "fake");

        var result = FindDefaultXex.Find(_testDirectory, _logErrors);

        Assert.NotNull(result);
        Assert.Equal(xexPath, result);
    }

    [Fact]
    public void FindDefaultXexFileDoesNotExistReturnsNull()
    {
        var result = FindDefaultXex.Find(_testDirectory, _logErrors);
        Assert.Null(result);
    }

    // FindDefaultXbe tests
    [Fact]
    public void FindDefaultXbeNullPathReturnsNull()
    {
        var result = FindDefaultXbe.Find(null, _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindDefaultXbeEmptyPathReturnsNull()
    {
        var result = FindDefaultXbe.Find("", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindDefaultXbeNonExistentDirectoryReturnsNull()
    {
        var result = FindDefaultXbe.Find(@"C:\nonexistent\dir", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindDefaultXbeFileExistsReturnsPath()
    {
        var xbePath = Path.Combine(_testDirectory, "default.xbe");
        File.WriteAllText(xbePath, "fake");

        var result = FindDefaultXbe.Find(_testDirectory, _logErrors);

        Assert.NotNull(result);
        Assert.Equal(xbePath, result);
    }

    [Fact]
    public void FindDefaultXbeFileDoesNotExistReturnsNull()
    {
        var result = FindDefaultXbe.Find(_testDirectory, _logErrors);
        Assert.Null(result);
    }

    // FindCueFile tests
    [Fact]
    public void FindCueFileNullPathReturnsNull()
    {
        var result = FindCueFile.Find(null, _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindCueFileEmptyPathReturnsNull()
    {
        var result = FindCueFile.Find("", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindCueFileNonExistentDirectoryReturnsNull()
    {
        var result = FindCueFile.Find(@"C:\nonexistent\dir", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindCueFileFileExistsReturnsPath()
    {
        var cuePath = Path.Combine(_testDirectory, "game.cue");
        File.WriteAllText(cuePath, "fake");

        var result = FindCueFile.Find(_testDirectory, _logErrors);

        Assert.NotNull(result);
        Assert.Equal(cuePath, result);
    }

    [Fact]
    public void FindCueFileMultipleCueFilesReturnsFirst()
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
    public void FindCueFileNoCueFilesReturnsNull()
    {
        File.WriteAllText(Path.Combine(_testDirectory, "game.bin"), "fake");

        var result = FindCueFile.Find(_testDirectory, _logErrors);
        Assert.Null(result);
    }

    // FindBinFile tests
    [Fact]
    public void FindBinFileNullPathReturnsNull()
    {
        var result = FindBinFile.Find(null, _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindBinFileEmptyPathReturnsNull()
    {
        var result = FindBinFile.Find("", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindBinFileNonExistentDirectoryReturnsNull()
    {
        var result = FindBinFile.Find(@"C:\nonexistent\dir", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindBinFileFileExistsReturnsPath()
    {
        var binPath = Path.Combine(_testDirectory, "game.bin");
        File.WriteAllText(binPath, "fake");

        var result = FindBinFile.Find(_testDirectory, _logErrors);

        Assert.NotNull(result);
        Assert.Equal(binPath, result);
    }

    [Fact]
    public void FindBinFileNoBinFilesReturnsNull()
    {
        File.WriteAllText(Path.Combine(_testDirectory, "game.cue"), "fake");

        var result = FindBinFile.Find(_testDirectory, _logErrors);
        Assert.Null(result);
    }

    // FindImageIso tests
    [Fact]
    public void FindImageIsoNullPathReturnsNull()
    {
        var result = FindImageIso.Find(null, _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindImageIsoEmptyPathReturnsNull()
    {
        var result = FindImageIso.Find("", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindImageIsoNonExistentDirectoryReturnsNull()
    {
        var result = FindImageIso.Find(@"C:\nonexistent\dir", _logErrors);
        Assert.Null(result);
    }

    [Fact]
    public void FindImageIsoFileExistsReturnsPath()
    {
        var isoPath = Path.Combine(_testDirectory, "image.iso");
        File.WriteAllText(isoPath, "fake");

        var result = FindImageIso.Find(_testDirectory, _logErrors);

        Assert.NotNull(result);
        Assert.Equal(isoPath, result);
    }

    [Fact]
    public void FindImageIsoFileDoesNotExistReturnsNull()
    {
        var result = FindImageIso.Find(_testDirectory, _logErrors);
        Assert.Null(result);
    }

    // FindEbootBin tests
    [Fact]
    public void FindEbootBinNullPathReturnsNull()
    {
        var result = FindEbootBin.FindEbootBinRecursive(null, _logErrors, _debugLogger);
        Assert.Null(result);
    }

    [Fact]
    public void FindEbootBinEmptyPathReturnsNull()
    {
        var result = FindEbootBin.FindEbootBinRecursive("", _logErrors, _debugLogger);
        Assert.Null(result);
    }

    [Fact]
    public void FindEbootBinInTopDirectoryReturnsPath()
    {
        var ebootPath = Path.Combine(_testDirectory, "EBOOT.BIN");
        File.WriteAllText(ebootPath, "fake");

        var result = FindEbootBin.FindEbootBinRecursive(_testDirectory, _logErrors, _debugLogger);

        Assert.NotNull(result);
        Assert.Equal(ebootPath, result);
    }

    [Fact]
    public void FindEbootBinInPs3GameUsrDirReturnsPath()
    {
        var ps3GameDir = Path.Combine(_testDirectory, "PS3_GAME");
        var usrDir = Path.Combine(ps3GameDir, "USRDIR");
        Directory.CreateDirectory(usrDir);
        var ebootPath = Path.Combine(usrDir, "EBOOT.BIN");
        File.WriteAllText(ebootPath, "fake");

        var result = FindEbootBin.FindEbootBinRecursive(_testDirectory, _logErrors, _debugLogger);

        Assert.NotNull(result);
        Assert.Equal(ebootPath, result);
    }

    [Fact]
    public void FindEbootBinInNestedDirectoryReturnsPath()
    {
        var nestedDir = Path.Combine(_testDirectory, "subdir", "deep");
        Directory.CreateDirectory(nestedDir);
        var ebootPath = Path.Combine(nestedDir, "EBOOT.BIN");
        File.WriteAllText(ebootPath, "fake");

        var result = FindEbootBin.FindEbootBinRecursive(_testDirectory, _logErrors, _debugLogger);

        Assert.NotNull(result);
        Assert.Equal(ebootPath, result);
    }

    [Fact]
    public void FindEbootBinNotFoundReturnsNull()
    {
        var result = FindEbootBin.FindEbootBinRecursive(_testDirectory, _logErrors, _debugLogger);
        Assert.Null(result);
    }

    [Fact]
    public void FindEbootBinPrefersTopDirectoryOverNested()
    {
        var topEboot = Path.Combine(_testDirectory, "EBOOT.BIN");
        File.WriteAllText(topEboot, "top");

        var nestedDir = Path.Combine(_testDirectory, "PS3_GAME", "USRDIR");
        Directory.CreateDirectory(nestedDir);
        File.WriteAllText(Path.Combine(nestedDir, "EBOOT.BIN"), "nested");

        var result = FindEbootBin.FindEbootBinRecursive(_testDirectory, _logErrors, _debugLogger);

        Assert.NotNull(result);
        Assert.Equal(topEboot, result);
    }

    [Fact]
    public void FindEbootBinPrefersPs3StructureOverFullRecursive()
    {
        var ps3GameDir = Path.Combine(_testDirectory, "PS3_GAME");
        var usrDir = Path.Combine(ps3GameDir, "USRDIR");
        Directory.CreateDirectory(usrDir);
        var ps3Eboot = Path.Combine(usrDir, "EBOOT.BIN");
        File.WriteAllText(ps3Eboot, "ps3");

        var deepDir = Path.Combine(_testDirectory, "other", "deep");
        Directory.CreateDirectory(deepDir);
        File.WriteAllText(Path.Combine(deepDir, "EBOOT.BIN"), "deep");

        var result = FindEbootBin.FindEbootBinRecursive(_testDirectory, _logErrors, _debugLogger);

        Assert.NotNull(result);
        Assert.Equal(ps3Eboot, result);
    }
}
