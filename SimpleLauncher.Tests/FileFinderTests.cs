using Moq;
using SimpleLauncher.Core.Services.GameLauncher.MountFiles;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
///     Tests for the static file finder utilities: <see cref="FindBinFile" />,
///     <see cref="FindCueFile" />, <see cref="FindDefaultXbe" />, <see cref="FindDefaultXex" />,
///     <see cref="FindEbootBin" />, and <see cref="FindImageIso" />.
/// </summary>
public class FileFinderTests : IDisposable
{
    private readonly string _tempDir;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FileFinderTests" /> class, creating a temporary test directory.
    /// </summary>
    public FileFinderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), $"SimpleLauncherTests_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_tempDir);
    }

    /// <summary>
    ///     Cleans up the temporary test directory.
    /// </summary>
    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            try
            {
                Directory.Delete(_tempDir, true);
            }
            catch
            {
                /* best effort cleanup */
            }

        GC.SuppressFinalize(this);
    }

    private static Mock<ILogger> CreateLogErrorsMock()
    {
        return new Mock<ILogger>();
    }

    // --- FindBinFile Tests ---

    /// <summary>
    ///     Verifies that FindBinFile returns null when the path is null.
    /// </summary>
    [Fact]
    public void FindBinFileNullPathReturnsNull()
    {
        var result = FindBinFile.Find(null!, CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindBinFile returns null when the path is empty.
    /// </summary>
    [Fact]
    public void FindBinFileEmptyPathReturnsNull()
    {
        var result = FindBinFile.Find("", CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindBinFile returns null when the directory does not exist.
    /// </summary>
    [Fact]
    public void FindBinFileNonExistentDirectoryReturnsNull()
    {
        var result = FindBinFile.Find(@"C:\nonexistent_path_12345", CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindBinFile returns null when no .bin files exist in the directory.
    /// </summary>
    [Fact]
    public void FindBinFileNoBinFilesReturnsNull()
    {
        File.WriteAllText(Path.Combine(_tempDir, "game.iso"), "test");
        var result = FindBinFile.Find(_tempDir, CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindBinFile returns the correct file path when a .bin file exists.
    /// </summary>
    [Fact]
    public void FindBinFileFindsBinFile()
    {
        var binPath = Path.Combine(_tempDir, "game.bin");
        File.WriteAllText(binPath, "test");
        var result = FindBinFile.Find(_tempDir, CreateLogErrorsMock().Object);
        Assert.Equal(binPath, result);
    }

    /// <summary>
    ///     Verifies that FindBinFile returns the first .bin file found when multiple exist.
    /// </summary>
    [Fact]
    public void FindBinFileReturnsFirstBinFile()
    {
        var bin1 = Path.Combine(_tempDir, "a.bin");
        var bin2 = Path.Combine(_tempDir, "b.bin");
        File.WriteAllText(bin1, "test1");
        File.WriteAllText(bin2, "test2");
        var result = FindBinFile.Find(_tempDir, CreateLogErrorsMock().Object);
        Assert.NotNull(result);
        Assert.True(File.Exists(result));
    }

    // --- FindCueFile Tests ---

    /// <summary>
    ///     Verifies that FindCueFile returns null when the path is null.
    /// </summary>
    [Fact]
    public void FindCueFileNullPathReturnsNull()
    {
        var result = FindCueFile.Find(null!, CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindCueFile returns null when the path is empty.
    /// </summary>
    [Fact]
    public void FindCueFileEmptyPathReturnsNull()
    {
        var result = FindCueFile.Find("", CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindCueFile returns null when the directory does not exist.
    /// </summary>
    [Fact]
    public void FindCueFileNonExistentDirectoryReturnsNull()
    {
        var result = FindCueFile.Find(@"C:\nonexistent_path_12345", CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindCueFile returns null when no .cue files exist in the directory.
    /// </summary>
    [Fact]
    public void FindCueFileNoCueFilesReturnsNull()
    {
        File.WriteAllText(Path.Combine(_tempDir, "game.bin"), "test");
        var result = FindCueFile.Find(_tempDir, CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindCueFile returns the correct file path when a .cue file exists.
    /// </summary>
    [Fact]
    public void FindCueFileFindsCueFile()
    {
        var cuePath = Path.Combine(_tempDir, "game.cue");
        File.WriteAllText(cuePath, "test");
        var result = FindCueFile.Find(_tempDir, CreateLogErrorsMock().Object);
        Assert.Equal(cuePath, result);
    }

    /// <summary>
    ///     Verifies that FindCueFile returns the first .cue file found when multiple exist.
    /// </summary>
    [Fact]
    public void FindCueFileReturnsFirstCueFile()
    {
        var cue1 = Path.Combine(_tempDir, "a.cue");
        var cue2 = Path.Combine(_tempDir, "b.cue");
        File.WriteAllText(cue1, "test1");
        File.WriteAllText(cue2, "test2");
        var result = FindCueFile.Find(_tempDir, CreateLogErrorsMock().Object);
        Assert.NotNull(result);
        Assert.True(File.Exists(result));
    }

    // --- FindDefaultXbe Tests ---

    /// <summary>
    ///     Verifies that FindDefaultXbe returns null when the path is null.
    /// </summary>
    [Fact]
    public void FindDefaultXbeNullPathReturnsNull()
    {
        var result = FindDefaultXbe.Find(null!, CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindDefaultXbe returns null when the path is empty.
    /// </summary>
    [Fact]
    public void FindDefaultXbeEmptyPathReturnsNull()
    {
        var result = FindDefaultXbe.Find("", CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindDefaultXbe returns null when the directory does not exist.
    /// </summary>
    [Fact]
    public void FindDefaultXbeNonExistentDirectoryReturnsNull()
    {
        var result = FindDefaultXbe.Find(@"C:\nonexistent_path_12345", CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindDefaultXbe returns null when no default.xbe file exists.
    /// </summary>
    [Fact]
    public void FindDefaultXbeNoXbeFileReturnsNull()
    {
        File.WriteAllText(Path.Combine(_tempDir, "game.iso"), "test");
        var result = FindDefaultXbe.Find(_tempDir, CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindDefaultXbe returns the correct file path when default.xbe exists.
    /// </summary>
    [Fact]
    public void FindDefaultXbeFindsXbeFile()
    {
        var xbePath = Path.Combine(_tempDir, "default.xbe");
        File.WriteAllText(xbePath, "test");
        var result = FindDefaultXbe.Find(_tempDir, CreateLogErrorsMock().Object);
        Assert.Equal(xbePath, result);
    }

    // --- FindDefaultXex Tests ---

    /// <summary>
    ///     Verifies that FindDefaultXex returns null when the path is null.
    /// </summary>
    [Fact]
    public void FindDefaultXexNullPathReturnsNull()
    {
        var result = FindDefaultXex.Find(null!, CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindDefaultXex returns null when the path is empty.
    /// </summary>
    [Fact]
    public void FindDefaultXexEmptyPathReturnsNull()
    {
        var result = FindDefaultXex.Find("", CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindDefaultXex returns null when the directory does not exist.
    /// </summary>
    [Fact]
    public void FindDefaultXexNonExistentDirectoryReturnsNull()
    {
        var result = FindDefaultXex.Find(@"C:\nonexistent_path_12345", CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindDefaultXex returns null when no default.xex file exists.
    /// </summary>
    [Fact]
    public void FindDefaultXexNoXexFileReturnsNull()
    {
        File.WriteAllText(Path.Combine(_tempDir, "game.iso"), "test");
        var result = FindDefaultXex.Find(_tempDir, CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindDefaultXex returns the correct file path when default.xex exists.
    /// </summary>
    [Fact]
    public void FindDefaultXexFindsXexFile()
    {
        var xexPath = Path.Combine(_tempDir, "default.xex");
        File.WriteAllText(xexPath, "test");
        var result = FindDefaultXex.Find(_tempDir, CreateLogErrorsMock().Object);
        Assert.Equal(xexPath, result);
    }

    // --- FindImageIso Tests ---

    /// <summary>
    ///     Verifies that FindImageIso returns null when the path is null.
    /// </summary>
    [Fact]
    public void FindImageIsoNullPathReturnsNull()
    {
        var result = FindImageIso.Find(null!, CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindImageIso returns null when the path is empty.
    /// </summary>
    [Fact]
    public void FindImageIsoEmptyPathReturnsNull()
    {
        var result = FindImageIso.Find("", CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindImageIso returns null when the directory does not exist.
    /// </summary>
    [Fact]
    public void FindImageIsoNonExistentDirectoryReturnsNull()
    {
        var result = FindImageIso.Find(@"C:\nonexistent_path_12345", CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindImageIso returns null when no .iso or .img file exists.
    /// </summary>
    [Fact]
    public void FindImageIsoNoImageFileReturnsNull()
    {
        File.WriteAllText(Path.Combine(_tempDir, "game.bin"), "test");
        var result = FindImageIso.Find(_tempDir, CreateLogErrorsMock().Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindImageIso returns the correct file path when a .iso file exists.
    /// </summary>
    [Fact]
    public void FindImageIsoFindsImageIsoFile()
    {
        var isoPath = Path.Combine(_tempDir, "image.iso");
        File.WriteAllText(isoPath, "test");
        var result = FindImageIso.Find(_tempDir, CreateLogErrorsMock().Object);
        Assert.Equal(isoPath, result);
    }

    // --- FindEbootBin Tests ---

    /// <summary>
    ///     Verifies that FindEbootBin returns null when the path is null.
    /// </summary>
    [Fact]
    public void FindEbootBinNullPathReturnsNull()
    {
        var debugLoggerMock = new Mock<ILogger>();
        var result = FindEbootBin.FindEbootBinRecursive(null!, CreateLogErrorsMock().Object, debugLoggerMock.Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindEbootBin returns null when the path is empty.
    /// </summary>
    [Fact]
    public void FindEbootBinEmptyPathReturnsNull()
    {
        var debugLoggerMock = new Mock<ILogger>();
        var result = FindEbootBin.FindEbootBinRecursive("", CreateLogErrorsMock().Object, debugLoggerMock.Object);
        Assert.Null(result);
    }

    /// <summary>
    ///     Verifies that FindEbootBin finds EBOOT.BIN in the top-level directory.
    /// </summary>
    [Fact]
    public void FindEbootBinInTopDirectory()
    {
        var debugLoggerMock = new Mock<ILogger>();
        var ebootPath = Path.Combine(_tempDir, "EBOOT.BIN");
        File.WriteAllText(ebootPath, "test");
        var result = FindEbootBin.FindEbootBinRecursive(_tempDir, CreateLogErrorsMock().Object, debugLoggerMock.Object);
        Assert.Equal(ebootPath, result);
    }

    /// <summary>
    ///     Verifies that FindEbootBin finds EBOOT.BIN in the PS3_GAME/USRDIR directory structure.
    /// </summary>
    [Fact]
    public void FindEbootBinInPs3GameUsrDir()
    {
        var debugLoggerMock = new Mock<ILogger>();
        var ps3GameDir = Path.Combine(_tempDir, "PS3_GAME");
        var usrDir = Path.Combine(ps3GameDir, "USRDIR");
        Directory.CreateDirectory(usrDir);
        var ebootPath = Path.Combine(usrDir, "EBOOT.BIN");
        File.WriteAllText(ebootPath, "test");
        var result = FindEbootBin.FindEbootBinRecursive(_tempDir, CreateLogErrorsMock().Object, debugLoggerMock.Object);
        Assert.Equal(ebootPath, result);
    }

    /// <summary>
    ///     Verifies that FindEbootBin prefers the top-level directory over PS3_GAME/USRDIR.
    /// </summary>
    [Fact]
    public void FindEbootBinPrefersTopDirectoryOverPs3Game()
    {
        var debugLoggerMock = new Mock<ILogger>();
        var topEboot = Path.Combine(_tempDir, "EBOOT.BIN");
        File.WriteAllText(topEboot, "top");

        var ps3GameDir = Path.Combine(_tempDir, "PS3_GAME");
        var usrDir = Path.Combine(ps3GameDir, "USRDIR");
        Directory.CreateDirectory(usrDir);
        File.WriteAllText(Path.Combine(usrDir, "EBOOT.BIN"), "nested");

        var result = FindEbootBin.FindEbootBinRecursive(_tempDir, CreateLogErrorsMock().Object, debugLoggerMock.Object);
        Assert.Equal(topEboot, result);
    }

    /// <summary>
    ///     Verifies that FindEbootBin performs recursive search in nested directories.
    /// </summary>
    [Fact]
    public void FindEbootBinRecursiveSearch()
    {
        var debugLoggerMock = new Mock<ILogger>();
        var nestedDir = Path.Combine(_tempDir, "subdir1", "subdir2");
        Directory.CreateDirectory(nestedDir);
        var ebootPath = Path.Combine(nestedDir, "EBOOT.BIN");
        File.WriteAllText(ebootPath, "test");
        var result = FindEbootBin.FindEbootBinRecursive(_tempDir, CreateLogErrorsMock().Object, debugLoggerMock.Object);
        Assert.Equal(ebootPath, result);
    }

    /// <summary>
    ///     Verifies that FindEbootBin returns null when no EBOOT.BIN file exists.
    /// </summary>
    [Fact]
    public void FindEbootBinNotFoundReturnsNull()
    {
        var debugLoggerMock = new Mock<ILogger>();
        File.WriteAllText(Path.Combine(_tempDir, "game.iso"), "test");
        var result = FindEbootBin.FindEbootBinRecursive(_tempDir, CreateLogErrorsMock().Object, debugLoggerMock.Object);
        Assert.Null(result);
    }
}