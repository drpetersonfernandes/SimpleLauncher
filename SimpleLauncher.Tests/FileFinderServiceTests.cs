using Moq;
using SimpleLauncher.Core.Services.GameLauncher.MountFiles;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="FileFinderService"/> class.
/// </summary>
public class FileFinderServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FileFinderService _service;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileFinderServiceTests"/> class, creating a temporary directory and service instance.
    /// </summary>
    public FileFinderServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"SimpleLauncherTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
        _service = new FileFinderService(new Mock<ILogger>().Object);
    }

    /// <summary>
    /// Cleans up the temporary test directory.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            try
            {
                Directory.Delete(_tempDir, true);
            }
            catch
            {
                /* best effort cleanup */
            }
        }

        GC.SuppressFinalize(this);
    }

    // --- FindDefaultXex Tests ---

    /// <summary>
    /// Verifies that FindDefaultXex returns null when the path is null.
    /// </summary>
    [Fact]
    public void FindDefaultXexNullPathReturnsNull()
    {
        Assert.Null(_service.FindDefaultXex(null!));
    }

    /// <summary>
    /// Verifies that FindDefaultXex returns null when the path is empty.
    /// </summary>
    [Fact]
    public void FindDefaultXexEmptyPathReturnsNull()
    {
        Assert.Null(_service.FindDefaultXex(""));
    }

    /// <summary>
    /// Verifies that FindDefaultXex returns null when the directory does not exist.
    /// </summary>
    [Fact]
    public void FindDefaultXexNonExistentDirectoryReturnsNull()
    {
        Assert.Null(_service.FindDefaultXex(@"C:\nonexistent_path_12345"));
    }

    /// <summary>
    /// Verifies that FindDefaultXex returns null when no default.xex file exists.
    /// </summary>
    [Fact]
    public void FindDefaultXexNoFileReturnsNull()
    {
        File.WriteAllText(Path.Combine(_tempDir, "game.iso"), "test");
        Assert.Null(_service.FindDefaultXex(_tempDir));
    }

    /// <summary>
    /// Verifies that FindDefaultXex returns the correct file path when default.xex exists.
    /// </summary>
    [Fact]
    public void FindDefaultXexFindsFile()
    {
        var xexPath = Path.Combine(_tempDir, "default.xex");
        File.WriteAllText(xexPath, "test");
        Assert.Equal(xexPath, _service.FindDefaultXex(_tempDir));
    }

    // --- FindDefaultXbe Tests ---

    /// <summary>
    /// Verifies that FindDefaultXbe returns null when the path is null.
    /// </summary>
    [Fact]
    public void FindDefaultXbeNullPathReturnsNull()
    {
        Assert.Null(_service.FindDefaultXbe(null!));
    }

    /// <summary>
    /// Verifies that FindDefaultXbe returns null when the path is empty.
    /// </summary>
    [Fact]
    public void FindDefaultXbeEmptyPathReturnsNull()
    {
        Assert.Null(_service.FindDefaultXbe(""));
    }

    /// <summary>
    /// Verifies that FindDefaultXbe returns null when the directory does not exist.
    /// </summary>
    [Fact]
    public void FindDefaultXbeNonExistentDirectoryReturnsNull()
    {
        Assert.Null(_service.FindDefaultXbe(@"C:\nonexistent_path_12345"));
    }

    /// <summary>
    /// Verifies that FindDefaultXbe returns null when no default.xbe file exists.
    /// </summary>
    [Fact]
    public void FindDefaultXbeNoFileReturnsNull()
    {
        File.WriteAllText(Path.Combine(_tempDir, "game.iso"), "test");
        Assert.Null(_service.FindDefaultXbe(_tempDir));
    }

    /// <summary>
    /// Verifies that FindDefaultXbe returns the correct file path when default.xbe exists.
    /// </summary>
    [Fact]
    public void FindDefaultXbeFindsFile()
    {
        var xbePath = Path.Combine(_tempDir, "default.xbe");
        File.WriteAllText(xbePath, "test");
        Assert.Equal(xbePath, _service.FindDefaultXbe(_tempDir));
    }

    // --- FindCueFile Tests ---

    /// <summary>
    /// Verifies that FindCueFile returns null when the path is null.
    /// </summary>
    [Fact]
    public void FindCueFileNullPathReturnsNull()
    {
        Assert.Null(_service.FindCueFile(null!));
    }

    /// <summary>
    /// Verifies that FindCueFile returns null when the path is empty.
    /// </summary>
    [Fact]
    public void FindCueFileEmptyPathReturnsNull()
    {
        Assert.Null(_service.FindCueFile(""));
    }

    /// <summary>
    /// Verifies that FindCueFile returns null when the directory does not exist.
    /// </summary>
    [Fact]
    public void FindCueFileNonExistentDirectoryReturnsNull()
    {
        Assert.Null(_service.FindCueFile(@"C:\nonexistent_path_12345"));
    }

    /// <summary>
    /// Verifies that FindCueFile returns null when no .cue file exists.
    /// </summary>
    [Fact]
    public void FindCueFileNoFileReturnsNull()
    {
        File.WriteAllText(Path.Combine(_tempDir, "game.bin"), "test");
        Assert.Null(_service.FindCueFile(_tempDir));
    }

    /// <summary>
    /// Verifies that FindCueFile returns the correct file path when a .cue file exists.
    /// </summary>
    [Fact]
    public void FindCueFileFindsFile()
    {
        var cuePath = Path.Combine(_tempDir, "game.cue");
        File.WriteAllText(cuePath, "test");
        Assert.Equal(cuePath, _service.FindCueFile(_tempDir));
    }

    // --- FindBinFile Tests ---

    /// <summary>
    /// Verifies that FindBinFile returns null when the path is null.
    /// </summary>
    [Fact]
    public void FindBinFileNullPathReturnsNull()
    {
        Assert.Null(_service.FindBinFile(null!));
    }

    /// <summary>
    /// Verifies that FindBinFile returns null when the path is empty.
    /// </summary>
    [Fact]
    public void FindBinFileEmptyPathReturnsNull()
    {
        Assert.Null(_service.FindBinFile(""));
    }

    /// <summary>
    /// Verifies that FindBinFile returns null when the directory does not exist.
    /// </summary>
    [Fact]
    public void FindBinFileNonExistentDirectoryReturnsNull()
    {
        Assert.Null(_service.FindBinFile(@"C:\nonexistent_path_12345"));
    }

    /// <summary>
    /// Verifies that FindBinFile returns null when no .bin file exists.
    /// </summary>
    [Fact]
    public void FindBinFileNoFileReturnsNull()
    {
        File.WriteAllText(Path.Combine(_tempDir, "game.cue"), "test");
        Assert.Null(_service.FindBinFile(_tempDir));
    }

    /// <summary>
    /// Verifies that FindBinFile returns the correct file path when a .bin file exists.
    /// </summary>
    [Fact]
    public void FindBinFileFindsFile()
    {
        var binPath = Path.Combine(_tempDir, "game.bin");
        File.WriteAllText(binPath, "test");
        Assert.Equal(binPath, _service.FindBinFile(_tempDir));
    }

    // --- FindEbootBin Tests ---

    /// <summary>
    /// Verifies that FindEbootBin returns null when the path is null.
    /// </summary>
    [Fact]
    public void FindEbootBinNullPathReturnsNull()
    {
        Assert.Null(_service.FindEbootBin(null!));
    }

    /// <summary>
    /// Verifies that FindEbootBin returns null when the path is empty.
    /// </summary>
    [Fact]
    public void FindEbootBinEmptyPathReturnsNull()
    {
        Assert.Null(_service.FindEbootBin(""));
    }

    /// <summary>
    /// Verifies that FindEbootBin returns null when the directory does not exist.
    /// </summary>
    [Fact]
    public void FindEbootBinNonExistentDirectoryReturnsNull()
    {
        Assert.Null(_service.FindEbootBin(@"C:\nonexistent_path_12345"));
    }

    /// <summary>
    /// Verifies that FindEbootBin finds EBOOT.BIN in the top-level directory.
    /// </summary>
    [Fact]
    public void FindEbootBinInTopDirectory()
    {
        var ebootPath = Path.Combine(_tempDir, "EBOOT.BIN");
        File.WriteAllText(ebootPath, "test");
        Assert.Equal(ebootPath, _service.FindEbootBin(_tempDir));
    }

    /// <summary>
    /// Verifies that FindEbootBin finds EBOOT.BIN in a subdirectory.
    /// </summary>
    [Fact]
    public void FindEbootBinInSubdirectory()
    {
        var subDir = Path.Combine(_tempDir, "subdir");
        Directory.CreateDirectory(subDir);
        var ebootPath = Path.Combine(subDir, "EBOOT.BIN");
        File.WriteAllText(ebootPath, "test");
        Assert.Equal(ebootPath, _service.FindEbootBin(_tempDir));
    }

    /// <summary>
    /// Verifies that FindEbootBin prefers the top-level directory over subdirectories.
    /// </summary>
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

    /// <summary>
    /// Verifies that FindEbootBin returns null when no EBOOT.BIN file exists.
    /// </summary>
    [Fact]
    public void FindEbootBinNotFoundReturnsNull()
    {
        File.WriteAllText(Path.Combine(_tempDir, "game.iso"), "test");
        Assert.Null(_service.FindEbootBin(_tempDir));
    }

    // --- FindImageIso Tests ---

    /// <summary>
    /// Verifies that FindImageIso returns null when the path is null.
    /// </summary>
    [Fact]
    public void FindImageIsoNullPathReturnsNull()
    {
        Assert.Null(_service.FindImageIso(null!));
    }

    /// <summary>
    /// Verifies that FindImageIso returns null when the path is empty.
    /// </summary>
    [Fact]
    public void FindImageIsoEmptyPathReturnsNull()
    {
        Assert.Null(_service.FindImageIso(""));
    }

    /// <summary>
    /// Verifies that FindImageIso returns null when the directory does not exist.
    /// </summary>
    [Fact]
    public void FindImageIsoNonExistentDirectoryReturnsNull()
    {
        Assert.Null(_service.FindImageIso(@"C:\nonexistent_path_12345"));
    }

    /// <summary>
    /// Verifies that FindImageIso returns null when no .iso or .img file exists.
    /// </summary>
    [Fact]
    public void FindImageIsoNoFileReturnsNull()
    {
        File.WriteAllText(Path.Combine(_tempDir, "game.bin"), "test");
        Assert.Null(_service.FindImageIso(_tempDir));
    }

    /// <summary>
    /// Verifies that FindImageIso returns the .iso file path when one exists.
    /// </summary>
    [Fact]
    public void FindImageIsoFindsIsoFile()
    {
        var isoPath = Path.Combine(_tempDir, "game.iso");
        File.WriteAllText(isoPath, "test");
        Assert.Equal(isoPath, _service.FindImageIso(_tempDir));
    }

    /// <summary>
    /// Verifies that FindImageIso prefers .iso over .img when both exist.
    /// </summary>
    [Fact]
    public void FindImageIsoPrefersIsoOverImg()
    {
        var isoPath = Path.Combine(_tempDir, "game.iso");
        var imgPath = Path.Combine(_tempDir, "game.img");
        File.WriteAllText(isoPath, "iso");
        File.WriteAllText(imgPath, "img");
        Assert.Equal(isoPath, _service.FindImageIso(_tempDir));
    }

    /// <summary>
    /// Verifies that FindImageIso returns the .img file path when no .iso exists.
    /// </summary>
    [Fact]
    public void FindImageIsoFindsImgWhenNoIso()
    {
        var imgPath = Path.Combine(_tempDir, "game.img");
        File.WriteAllText(imgPath, "test");
        Assert.Equal(imgPath, _service.FindImageIso(_tempDir));
    }
}