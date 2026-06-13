using Moq;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.GameLauncher.MountFiles;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="FileFinderService"/> class.
/// </summary>
public class FileFinderServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FileFinderService _service;

    public FileFinderServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"SimpleLauncherTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _service = new FileFinderService(new Mock<ILogErrors>().Object);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            try { Directory.Delete(_tempDir, true); }
            catch { /* best effort cleanup */ }
        }

        GC.SuppressFinalize(this);
    }

    // --- FindDefaultXex Tests ---

    [Fact]
    public void FindDefaultXexNullPathReturnsNull()
    {
        Assert.Null(_service.FindDefaultXex(null));
    }

    [Fact]
    public void FindDefaultXexEmptyPathReturnsNull()
    {
        Assert.Null(_service.FindDefaultXex(""));
    }

    [Fact]
    public void FindDefaultXexNonExistentDirectoryReturnsNull()
    {
        Assert.Null(_service.FindDefaultXex(@"C:\nonexistent_path_12345"));
    }

    [Fact]
    public void FindDefaultXexNoFileReturnsNull()
    {
        File.WriteAllText(Path.Combine(_tempDir, "game.iso"), "test");
        Assert.Null(_service.FindDefaultXex(_tempDir));
    }

    [Fact]
    public void FindDefaultXexFindsFile()
    {
        var xexPath = Path.Combine(_tempDir, "default.xex");
        File.WriteAllText(xexPath, "test");
        Assert.Equal(xexPath, _service.FindDefaultXex(_tempDir));
    }

    // --- FindDefaultXbe Tests ---

    [Fact]
    public void FindDefaultXbeNullPathReturnsNull()
    {
        Assert.Null(_service.FindDefaultXbe(null));
    }

    [Fact]
    public void FindDefaultXbeEmptyPathReturnsNull()
    {
        Assert.Null(_service.FindDefaultXbe(""));
    }

    [Fact]
    public void FindDefaultXbeNonExistentDirectoryReturnsNull()
    {
        Assert.Null(_service.FindDefaultXbe(@"C:\nonexistent_path_12345"));
    }

    [Fact]
    public void FindDefaultXbeNoFileReturnsNull()
    {
        File.WriteAllText(Path.Combine(_tempDir, "game.iso"), "test");
        Assert.Null(_service.FindDefaultXbe(_tempDir));
    }

    [Fact]
    public void FindDefaultXbeFindsFile()
    {
        var xbePath = Path.Combine(_tempDir, "default.xbe");
        File.WriteAllText(xbePath, "test");
        Assert.Equal(xbePath, _service.FindDefaultXbe(_tempDir));
    }

    // --- FindCueFile Tests ---

    [Fact]
    public void FindCueFileNullPathReturnsNull()
    {
        Assert.Null(_service.FindCueFile(null));
    }

    [Fact]
    public void FindCueFileEmptyPathReturnsNull()
    {
        Assert.Null(_service.FindCueFile(""));
    }

    [Fact]
    public void FindCueFileNonExistentDirectoryReturnsNull()
    {
        Assert.Null(_service.FindCueFile(@"C:\nonexistent_path_12345"));
    }

    [Fact]
    public void FindCueFileNoFileReturnsNull()
    {
        File.WriteAllText(Path.Combine(_tempDir, "game.bin"), "test");
        Assert.Null(_service.FindCueFile(_tempDir));
    }

    [Fact]
    public void FindCueFileFindsFile()
    {
        var cuePath = Path.Combine(_tempDir, "game.cue");
        File.WriteAllText(cuePath, "test");
        Assert.Equal(cuePath, _service.FindCueFile(_tempDir));
    }

    // --- FindBinFile Tests ---

    [Fact]
    public void FindBinFileNullPathReturnsNull()
    {
        Assert.Null(_service.FindBinFile(null));
    }

    [Fact]
    public void FindBinFileEmptyPathReturnsNull()
    {
        Assert.Null(_service.FindBinFile(""));
    }

    [Fact]
    public void FindBinFileNonExistentDirectoryReturnsNull()
    {
        Assert.Null(_service.FindBinFile(@"C:\nonexistent_path_12345"));
    }

    [Fact]
    public void FindBinFileNoFileReturnsNull()
    {
        File.WriteAllText(Path.Combine(_tempDir, "game.cue"), "test");
        Assert.Null(_service.FindBinFile(_tempDir));
    }

    [Fact]
    public void FindBinFileFindsFile()
    {
        var binPath = Path.Combine(_tempDir, "game.bin");
        File.WriteAllText(binPath, "test");
        Assert.Equal(binPath, _service.FindBinFile(_tempDir));
    }

    // --- FindEbootBin Tests ---

    [Fact]
    public void FindEbootBinNullPathReturnsNull()
    {
        Assert.Null(_service.FindEbootBin(null));
    }

    [Fact]
    public void FindEbootBinEmptyPathReturnsNull()
    {
        Assert.Null(_service.FindEbootBin(""));
    }

    [Fact]
    public void FindEbootBinNonExistentDirectoryReturnsNull()
    {
        Assert.Null(_service.FindEbootBin(@"C:\nonexistent_path_12345"));
    }

    [Fact]
    public void FindEbootBinInTopDirectory()
    {
        var ebootPath = Path.Combine(_tempDir, "EBOOT.BIN");
        File.WriteAllText(ebootPath, "test");
        Assert.Equal(ebootPath, _service.FindEbootBin(_tempDir));
    }

    [Fact]
    public void FindEbootBinInSubdirectory()
    {
        var subDir = Path.Combine(_tempDir, "subdir");
        Directory.CreateDirectory(subDir);
        var ebootPath = Path.Combine(subDir, "EBOOT.BIN");
        File.WriteAllText(ebootPath, "test");
        Assert.Equal(ebootPath, _service.FindEbootBin(_tempDir));
    }

    [Fact]
    public void FindEbootBinPrefersTopDirectory()
    {
        var topEboot = Path.Combine(_tempDir, "EBOOT.BIN");
        File.WriteAllText(topEboot, "top");

        var subDir = Path.Combine(_tempDir, "subdir");
        Directory.CreateDirectory(subDir);
        File.WriteAllText(Path.Combine(subDir, "EBOOT.BIN"), "nested");

        Assert.Equal(topEboot, _service.FindEbootBin(_tempDir));
    }

    [Fact]
    public void FindEbootBinNotFoundReturnsNull()
    {
        File.WriteAllText(Path.Combine(_tempDir, "game.iso"), "test");
        Assert.Null(_service.FindEbootBin(_tempDir));
    }

    // --- FindImageIso Tests ---

    [Fact]
    public void FindImageIsoNullPathReturnsNull()
    {
        Assert.Null(_service.FindImageIso(null));
    }

    [Fact]
    public void FindImageIsoEmptyPathReturnsNull()
    {
        Assert.Null(_service.FindImageIso(""));
    }

    [Fact]
    public void FindImageIsoNonExistentDirectoryReturnsNull()
    {
        Assert.Null(_service.FindImageIso(@"C:\nonexistent_path_12345"));
    }

    [Fact]
    public void FindImageIsoNoFileReturnsNull()
    {
        File.WriteAllText(Path.Combine(_tempDir, "game.bin"), "test");
        Assert.Null(_service.FindImageIso(_tempDir));
    }

    [Fact]
    public void FindImageIsoFindsIsoFile()
    {
        var isoPath = Path.Combine(_tempDir, "game.iso");
        File.WriteAllText(isoPath, "test");
        Assert.Equal(isoPath, _service.FindImageIso(_tempDir));
    }

    [Fact]
    public void FindImageIsoPrefersIsoOverImg()
    {
        var isoPath = Path.Combine(_tempDir, "game.iso");
        var imgPath = Path.Combine(_tempDir, "game.img");
        File.WriteAllText(isoPath, "iso");
        File.WriteAllText(imgPath, "img");
        Assert.Equal(isoPath, _service.FindImageIso(_tempDir));
    }

    [Fact]
    public void FindImageIsoFindsImgWhenNoIso()
    {
        var imgPath = Path.Combine(_tempDir, "game.img");
        File.WriteAllText(imgPath, "test");
        Assert.Equal(imgPath, _service.FindImageIso(_tempDir));
    }
}
