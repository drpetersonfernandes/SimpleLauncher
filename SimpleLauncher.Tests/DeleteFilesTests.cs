using SimpleLauncher.Services.CleanAndDeleteFiles;
using Xunit;

namespace SimpleLauncher.Tests;

public class DeleteFilesTests
{
    [Fact]
    public void TryDeleteFileNullPathDoesNotThrow()
    {
        var ex = Record.Exception(static () => DeleteFiles.TryDeleteFile(null));
        Assert.Null(ex);
    }

    [Fact]
    public void TryDeleteFileEmptyPathDoesNotThrow()
    {
        var ex = Record.Exception(static () => DeleteFiles.TryDeleteFile(""));
        Assert.Null(ex);
    }

    [Fact]
    public void TryDeleteFileNonExistentFileDoesNotThrow()
    {
        var fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "fake.txt");
        var ex = Record.Exception(() => DeleteFiles.TryDeleteFile(fakePath));
        Assert.Null(ex);
    }

    [Fact]
    public void TryDeleteFileExistingFileDeletesFile()
    {
        var tempFile = Path.GetTempFileName();
        Assert.True(File.Exists(tempFile));

        DeleteFiles.TryDeleteFile(tempFile);

        Assert.False(File.Exists(tempFile));
    }

    [Fact]
    public void TryDeleteFileReadOnlyFileDeletesFile()
    {
        var tempFile = Path.GetTempFileName();
        File.SetAttributes(tempFile, FileAttributes.ReadOnly);
        Assert.True(File.Exists(tempFile));

        DeleteFiles.TryDeleteFile(tempFile);

        Assert.False(File.Exists(tempFile));
    }

    [Fact]
    public async Task TryDeleteFileAsyncNullPathDoesNotThrow()
    {
        var ex = await Record.ExceptionAsync(static () => DeleteFiles.TryDeleteFileAsync(null));
        Assert.Null(ex);
    }

    [Fact]
    public async Task TryDeleteFileAsyncEmptyPathDoesNotThrow()
    {
        var ex = await Record.ExceptionAsync(static () => DeleteFiles.TryDeleteFileAsync(""));
        Assert.Null(ex);
    }

    [Fact]
    public async Task TryDeleteFileAsyncNonExistentFileDoesNotThrow()
    {
        var fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "fake.txt");
        var ex = await Record.ExceptionAsync(() => DeleteFiles.TryDeleteFileAsync(fakePath));
        Assert.Null(ex);
    }

    [Fact]
    public async Task TryDeleteFileAsyncExistingFileDeletesFile()
    {
        var tempFile = Path.GetTempFileName();
        Assert.True(File.Exists(tempFile));

        await DeleteFiles.TryDeleteFileAsync(tempFile);

        Assert.False(File.Exists(tempFile));
    }
}
