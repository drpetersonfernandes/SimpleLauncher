using SimpleLauncher.Services.CleanAndDeleteFiles;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for <see cref="DeleteFilesService"/> covering file deletion, directory deletion,
/// read-only file handling, and null/empty path handling.
/// </summary>
public class DeleteFilesServiceTests : IDisposable
{
    private readonly DeleteFilesService _service;
    private readonly string _testDirectory;

    public DeleteFilesServiceTests()
    {
        _service = new DeleteFilesService(new NoOpDebugLogger());
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_DeleteFilesTest_{Guid.NewGuid():N}");
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

    [Fact]
    public void TryDeleteFileNullPathDoesNotThrow()
    {
        var ex = Record.Exception(() => _service.TryDeleteFile(null));
        Assert.Null(ex);
    }

    [Fact]
    public void TryDeleteFileEmptyPathDoesNotThrow()
    {
        var ex = Record.Exception(() => _service.TryDeleteFile(""));
        Assert.Null(ex);
    }

    [Fact]
    public void TryDeleteFileNonExistentFileDoesNotThrow()
    {
        var fakePath = Path.Combine(_testDirectory, "nonexistent.txt");
        var ex = Record.Exception(() => _service.TryDeleteFile(fakePath));
        Assert.Null(ex);
    }

    [Fact]
    public void TryDeleteFileExistingFileDeletesFile()
    {
        var filePath = Path.Combine(_testDirectory, "test.txt");
        File.WriteAllText(filePath, "content");

        Assert.True(File.Exists(filePath));

        _service.TryDeleteFile(filePath);

        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void TryDeleteFileReadOnlyFileDeletesFile()
    {
        var filePath = Path.Combine(_testDirectory, "readonly.txt");
        File.WriteAllText(filePath, "content");
        File.SetAttributes(filePath, FileAttributes.ReadOnly);

        Assert.True(File.Exists(filePath));

        _service.TryDeleteFile(filePath);

        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public async Task TryDeleteFileAsyncNullPathDoesNotThrow()
    {
        var ex = await Record.ExceptionAsync(() => _service.TryDeleteFileAsync(null));
        Assert.Null(ex);
    }

    [Fact]
    public async Task TryDeleteFileAsyncEmptyPathDoesNotThrow()
    {
        var ex = await Record.ExceptionAsync(() => _service.TryDeleteFileAsync(""));
        Assert.Null(ex);
    }

    [Fact]
    public async Task TryDeleteFileAsyncNonExistentFileDoesNotThrow()
    {
        var fakePath = Path.Combine(_testDirectory, "nonexistent.txt");
        var ex = await Record.ExceptionAsync(() => _service.TryDeleteFileAsync(fakePath));
        Assert.Null(ex);
    }

    [Fact]
    public async Task TryDeleteFileAsyncExistingFileDeletesFile()
    {
        var filePath = Path.Combine(_testDirectory, "test_async.txt");
        await File.WriteAllTextAsync(filePath, "content");

        Assert.True(File.Exists(filePath));

        await _service.TryDeleteFileAsync(filePath);

        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public async Task TryDeleteFileAsyncReadOnlyFileDeletesFile()
    {
        var filePath = Path.Combine(_testDirectory, "readonly_async.txt");
        await File.WriteAllTextAsync(filePath, "content");
        File.SetAttributes(filePath, FileAttributes.ReadOnly);

        Assert.True(File.Exists(filePath));

        await _service.TryDeleteFileAsync(filePath);

        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void TryDeleteDirectoryNullPathDoesNotThrow()
    {
        var ex = Record.Exception(() => _service.TryDeleteDirectory(null));
        Assert.Null(ex);
    }

    [Fact]
    public void TryDeleteDirectoryEmptyPathDoesNotThrow()
    {
        var ex = Record.Exception(() => _service.TryDeleteDirectory(""));
        Assert.Null(ex);
    }

    [Fact]
    public void TryDeleteDirectoryNonExistentDirectoryDoesNotThrow()
    {
        var fakePath = Path.Combine(_testDirectory, "nonexistent_dir");
        var ex = Record.Exception(() => _service.TryDeleteDirectory(fakePath));
        Assert.Null(ex);
    }

    [Fact]
    public void TryDeleteDirectoryExistingDirectoryDeletesDirectory()
    {
        var dirPath = Path.Combine(_testDirectory, "subdir");
        Directory.CreateDirectory(dirPath);
        File.WriteAllText(Path.Combine(dirPath, "file.txt"), "content");

        Assert.True(Directory.Exists(dirPath));

        _service.TryDeleteDirectory(dirPath);

        Assert.False(Directory.Exists(dirPath));
    }

    [Fact]
    public void TryDeleteDirectoryWithNestedDirectoriesDeletesAll()
    {
        var dirPath = Path.Combine(_testDirectory, "nested");
        var nestedPath = Path.Combine(dirPath, "level1", "level2");
        Directory.CreateDirectory(nestedPath);
        File.WriteAllText(Path.Combine(nestedPath, "deep.txt"), "content");

        _service.TryDeleteDirectory(dirPath);

        Assert.False(Directory.Exists(dirPath));
    }

    [Fact]
    public void TryDeleteFileWithSpacesInPathDeletesFile()
    {
        var filePath = Path.Combine(_testDirectory, "file with spaces.txt");
        File.WriteAllText(filePath, "content");

        _service.TryDeleteFile(filePath);

        Assert.False(File.Exists(filePath));
    }

    [Fact]
    public void TryDeleteFileWithLongFileNameDeletesFile()
    {
        var longName = new string('a', 200) + ".txt";
        var filePath = Path.Combine(_testDirectory, longName);
        File.WriteAllText(filePath, "content");

        _service.TryDeleteFile(filePath);

        Assert.False(File.Exists(filePath));
    }
}
