using SimpleLauncher.Services.CleanAndDeleteFiles;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the static <see cref="CleanSimpleLauncherFolder"/> utility class.
/// </summary>
public class CleanSimpleLauncherFolderTests
{
    [Fact]
    public void CleanupTrashDoesNotThrow()
    {
        var exception = Record.Exception(CleanSimpleLauncherFolder.CleanupTrash);
        Assert.Null(exception);
    }

    [Fact]
    public void CleanupTempFilesDoesNotThrow()
    {
        var exception = Record.Exception(CleanSimpleLauncherFolder.CleanupTempFiles);
        Assert.Null(exception);
    }

    [Fact]
    public void CleanupTrashCalledTwiceDoesNotThrow()
    {
        CleanSimpleLauncherFolder.CleanupTrash();
        var exception = Record.Exception(CleanSimpleLauncherFolder.CleanupTrash);
        Assert.Null(exception);
    }

    [Fact]
    public void CleanupTempFilesRemovesSimpleLauncherTempDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
        Directory.CreateDirectory(tempDir);
        var tempFile = Path.Combine(tempDir, "test.txt");
        File.WriteAllText(tempFile, "test content");

        Assert.True(Directory.Exists(tempDir));

        CleanSimpleLauncherFolder.CleanupTempFiles();

        Assert.False(Directory.Exists(tempDir));
    }

    [Fact]
    public void CleanupTempFilesRemovesSimpleZipDriveTempDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "SimpleZipDrive");
        Directory.CreateDirectory(tempDir);

        Assert.True(Directory.Exists(tempDir));

        CleanSimpleLauncherFolder.CleanupTempFiles();

        Assert.False(Directory.Exists(tempDir));
    }

    [Fact]
    public void CleanupTempFilesRemovesSimpleXisoDriveTempDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "SimpleXisoDrive");
        Directory.CreateDirectory(tempDir);

        Assert.True(Directory.Exists(tempDir));

        CleanSimpleLauncherFolder.CleanupTempFiles();

        Assert.False(Directory.Exists(tempDir));
    }

    [Fact]
    public void CleanupTempFilesDoesNotThrowWhenDirectoriesDoNotExist()
    {
        // Ensure cleanup works even when target dirs are absent
        var exception = Record.Exception(CleanSimpleLauncherFolder.CleanupTempFiles);
        Assert.Null(exception);
    }
}
