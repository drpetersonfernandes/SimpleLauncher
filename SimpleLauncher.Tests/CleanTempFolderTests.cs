using SimpleLauncher.Core.Services.CleanAndDeleteFiles;
using Xunit;

namespace SimpleLauncher.Tests;

public class CleanTempFolderTests
{
    [Fact]
    public async Task CleanupTempDirectoryAsyncNullPathDoesNotThrow()
    {
        var ex = await Record.ExceptionAsync(static () => CleanTempFolder.CleanupTempDirectoryAsync(null));
        Assert.Null(ex);
    }

    [Fact]
    public async Task CleanupTempDirectoryAsyncEmptyPathDoesNotThrow()
    {
        var ex = await Record.ExceptionAsync(static () => CleanTempFolder.CleanupTempDirectoryAsync(""));
        Assert.Null(ex);
    }

    [Fact]
    public async Task CleanupTempDirectoryAsyncNonExistentPathDoesNotThrow()
    {
        var fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "nonexistent");
        var ex = await Record.ExceptionAsync(() => CleanTempFolder.CleanupTempDirectoryAsync(fakePath));
        Assert.Null(ex);
    }

    [Fact]
    public async Task CleanupTempDirectoryAsyncExistingDirectoryDeletesDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var nestedFile = Path.Combine(tempDir, "test.txt");
        await File.WriteAllTextAsync(nestedFile, "test");

        Assert.True(Directory.Exists(tempDir));

        await CleanTempFolder.CleanupTempDirectoryAsync(tempDir);

        Assert.False(Directory.Exists(tempDir));
    }

    [Fact]
    public async Task CleanupPartialExtractionAsyncNullPathDoesNotThrow()
    {
        var ex = await Record.ExceptionAsync(static () => CleanTempFolder.CleanupPartialExtractionAsync(null));
        Assert.Null(ex);
    }

    [Fact]
    public async Task CleanupPartialExtractionAsyncEmptyPathDoesNotThrow()
    {
        var ex = await Record.ExceptionAsync(static () => CleanTempFolder.CleanupPartialExtractionAsync(""));
        Assert.Null(ex);
    }

    [Fact]
    public async Task CleanupPartialExtractionAsyncNonExistentPathDoesNotThrow()
    {
        var fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "nonexistent");
        var ex = await Record.ExceptionAsync(() => CleanTempFolder.CleanupPartialExtractionAsync(fakePath));
        Assert.Null(ex);
    }

    [Fact]
    public async Task CleanupPartialExtractionAsyncDeletesTrackingFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var trackingFile = Path.Combine(tempDir, ".extraction_in_progress");
        await File.WriteAllTextAsync(trackingFile, "in progress");

        Assert.True(File.Exists(trackingFile));

        await CleanTempFolder.CleanupPartialExtractionAsync(tempDir);

        Assert.False(File.Exists(trackingFile));
        Assert.True(Directory.Exists(tempDir)); // Directory itself remains
    }

    [Fact]
    public async Task CleanupPartialExtractionAsyncDeletesFilesAndSubdirectories()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var subDir = Path.Combine(tempDir, "subdir");
        Directory.CreateDirectory(subDir);

        var file1 = Path.Combine(tempDir, "file1.txt");
        var file2 = Path.Combine(subDir, "file2.txt");
        await File.WriteAllTextAsync(file1, "content1");
        await File.WriteAllTextAsync(file2, "content2");

        Assert.True(File.Exists(file1));
        Assert.True(File.Exists(file2));
        Assert.True(Directory.Exists(subDir));

        await CleanTempFolder.CleanupPartialExtractionAsync(tempDir);

        // Files should be deleted, subdirectories too, but the root dir stays
        Assert.False(File.Exists(file1));
        Assert.False(File.Exists(file2));
        Assert.False(Directory.Exists(subDir));
        Assert.True(Directory.Exists(tempDir));
    }

    [Fact]
    public async Task CleanupPartialExtractionAsyncReadOnlyFilesDeletesSuccessfully()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var readOnlyFile = Path.Combine(tempDir, "readonly.txt");
        await File.WriteAllTextAsync(readOnlyFile, "content");
        File.SetAttributes(readOnlyFile, FileAttributes.ReadOnly);

        Assert.True(File.Exists(readOnlyFile));

        await CleanTempFolder.CleanupPartialExtractionAsync(tempDir);

        Assert.False(File.Exists(readOnlyFile), "Read-only file should be deleted after cleanup.");
    }
}
