using SimpleLauncher.Core.Services.CleanAndDeleteFiles;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the static <see cref="CleanSimpleLauncherFolder"/> utility class.
/// </summary>
public class CleanSimpleLauncherFolderTests
{
    /// <summary>
    /// Verifies that CleanupTrash does not throw.
    /// </summary>
    [Fact]
    public void CleanupTrashDoesNotThrow()
    {
        var exception = Record.Exception(CleanSimpleLauncherFolder.CleanupTrash);
        Assert.Null(exception);
    }

    /// <summary>
    /// Verifies that CleanupTempFiles does not throw.
    /// </summary>
    [Fact]
    public void CleanupTempFilesDoesNotThrow()
    {
        var exception = Record.Exception(CleanSimpleLauncherFolder.CleanupTempFiles);
        Assert.Null(exception);
    }

    /// <summary>
    /// Verifies that calling CleanupTrash twice does not throw.
    /// </summary>
    [Fact]
    public void CleanupTrashCalledTwiceDoesNotThrow()
    {
        CleanSimpleLauncherFolder.CleanupTrash();
        var exception = Record.Exception(CleanSimpleLauncherFolder.CleanupTrash);
        Assert.Null(exception);
    }

    /// <summary>
    /// Verifies that CleanupTempFiles removes the SimpleLauncher temp directory and its contents.
    /// </summary>
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

    /// <summary>
    /// Verifies that CleanupTempFiles removes the SimpleZipDrive temp directory.
    /// </summary>
    [Fact]
    public void CleanupTempFilesRemovesSimpleZipDriveTempDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "SimpleZipDrive");
        Directory.CreateDirectory(tempDir);

        Assert.True(Directory.Exists(tempDir));

        CleanSimpleLauncherFolder.CleanupTempFiles();

        Assert.False(Directory.Exists(tempDir));
    }

    /// <summary>
    /// Verifies that CleanupTempFiles removes the SimpleXisoDrive temp directory.
    /// </summary>
    [Fact]
    public void CleanupTempFilesRemovesSimpleXisoDriveTempDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "SimpleXisoDrive");
        Directory.CreateDirectory(tempDir);

        Assert.True(Directory.Exists(tempDir));

        CleanSimpleLauncherFolder.CleanupTempFiles();

        Assert.False(Directory.Exists(tempDir));
    }

    /// <summary>
    /// Verifies that CleanupTempFiles does not throw when target directories do not exist.
    /// </summary>
    [Fact]
    public void CleanupTempFilesDoesNotThrowWhenDirectoriesDoNotExist()
    {
        // Ensure cleanup works even when target dirs are absent
        var exception = Record.Exception(CleanSimpleLauncherFolder.CleanupTempFiles);
        Assert.Null(exception);
    }
}
