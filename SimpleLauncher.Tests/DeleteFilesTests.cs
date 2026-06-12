using SimpleLauncher.Services.CleanAndDeleteFiles;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests the <see cref="DeleteFiles"/> utility for synchronous and asynchronous file deletion.
/// </summary>
public class DeleteFilesTests
{
    /// <summary>
    /// Verifies that TryDeleteFile with a null path does not throw an exception.
    /// </summary>
    [Fact]
    public void TryDeleteFileNullPathDoesNotThrow()
    {
        var ex = Record.Exception(static () => DeleteFiles.TryDeleteFile(null));
        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that TryDeleteFile with an empty path does not throw an exception.
    /// </summary>
    [Fact]
    public void TryDeleteFileEmptyPathDoesNotThrow()
    {
        var ex = Record.Exception(static () => DeleteFiles.TryDeleteFile(""));
        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that TryDeleteFile with a non-existent file path does not throw an exception.
    /// </summary>
    [Fact]
    public void TryDeleteFileNonExistentFileDoesNotThrow()
    {
        var fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "fake.txt");
        var ex = Record.Exception(() => DeleteFiles.TryDeleteFile(fakePath));
        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that TryDeleteFile deletes an existing file.
    /// </summary>
    [Fact]
    public void TryDeleteFileExistingFileDeletesFile()
    {
        var tempFile = Path.GetTempFileName();
        Assert.True(File.Exists(tempFile));

        DeleteFiles.TryDeleteFile(tempFile);

        Assert.False(File.Exists(tempFile));
    }

    /// <summary>
    /// Verifies that TryDeleteFile deletes a read-only file.
    /// </summary>
    [Fact]
    public void TryDeleteFileReadOnlyFileDeletesFile()
    {
        var tempFile = Path.GetTempFileName();
        File.SetAttributes(tempFile, FileAttributes.ReadOnly);
        Assert.True(File.Exists(tempFile));

        DeleteFiles.TryDeleteFile(tempFile);

        Assert.False(File.Exists(tempFile));
    }

    /// <summary>
    /// Verifies that TryDeleteFileAsync with a null path does not throw an exception.
    /// </summary>
    [Fact]
    public async Task TryDeleteFileAsyncNullPathDoesNotThrow()
    {
        var ex = await Record.ExceptionAsync(static () => DeleteFiles.TryDeleteFileAsync(null));
        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that TryDeleteFileAsync with an empty path does not throw an exception.
    /// </summary>
    [Fact]
    public async Task TryDeleteFileAsyncEmptyPathDoesNotThrow()
    {
        var ex = await Record.ExceptionAsync(static () => DeleteFiles.TryDeleteFileAsync(""));
        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that TryDeleteFileAsync with a non-existent file path does not throw an exception.
    /// </summary>
    [Fact]
    public async Task TryDeleteFileAsyncNonExistentFileDoesNotThrow()
    {
        var fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "fake.txt");
        var ex = await Record.ExceptionAsync(() => DeleteFiles.TryDeleteFileAsync(fakePath));
        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that TryDeleteFileAsync deletes an existing file.
    /// </summary>
    [Fact]
    public async Task TryDeleteFileAsyncExistingFileDeletesFile()
    {
        var tempFile = Path.GetTempFileName();
        Assert.True(File.Exists(tempFile));

        await DeleteFiles.TryDeleteFileAsync(tempFile);

        Assert.False(File.Exists(tempFile));
    }
}
