using SimpleLauncher.Core.Services.CleanAndDeleteFiles;
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

    /// <summary>
    /// Initializes a new instance of <see cref="DeleteFilesServiceTests"/> with a temporary test directory.
    /// </summary>
    public DeleteFilesServiceTests()
    {
        _service = new DeleteFilesService(Log.Logger);
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_DeleteFilesTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    /// <summary>
    /// Cleans up the temporary test directory.
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    /// Verifies that TryDeleteFile with a null path does not throw.
    /// </summary>
    [Fact]
    public void TryDeleteFileNullPathDoesNotThrow()
    {
        var ex = Record.Exception(() => _service.TryDeleteFile(null!));
        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that TryDeleteFile with an empty path does not throw.
    /// </summary>
    [Fact]
    public void TryDeleteFileEmptyPathDoesNotThrow()
    {
        var ex = Record.Exception(() => _service.TryDeleteFile(""));
        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that TryDeleteFile with a non-existent file path does not throw.
    /// </summary>
    [Fact]
    public void TryDeleteFileNonExistentFileDoesNotThrow()
    {
        var fakePath = Path.Combine(_testDirectory, "nonexistent.txt");
        var ex = Record.Exception(() => _service.TryDeleteFile(fakePath));
        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that TryDeleteFile deletes an existing file.
    /// </summary>
    [Fact]
    public void TryDeleteFileExistingFileDeletesFile()
    {
        var filePath = Path.Combine(_testDirectory, "test.txt");
        File.WriteAllText(filePath, "content");

        Assert.True(File.Exists(filePath));

        _service.TryDeleteFile(filePath);

        Assert.False(File.Exists(filePath));
    }

    /// <summary>
    /// Verifies that TryDeleteFile deletes a read-only file by removing the read-only attribute.
    /// </summary>
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

    /// <summary>
    /// Verifies that TryDeleteFileAsync with a null path does not throw.
    /// </summary>
    [Fact]
    public async Task TryDeleteFileAsyncNullPathDoesNotThrow()
    {
        var ex = await Record.ExceptionAsync(() => _service.TryDeleteFileAsync(null!));
        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that TryDeleteFileAsync with an empty path does not throw.
    /// </summary>
    [Fact]
    public async Task TryDeleteFileAsyncEmptyPathDoesNotThrow()
    {
        var ex = await Record.ExceptionAsync(() => _service.TryDeleteFileAsync(""));
        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that TryDeleteFileAsync with a non-existent file path does not throw.
    /// </summary>
    [Fact]
    public async Task TryDeleteFileAsyncNonExistentFileDoesNotThrow()
    {
        var fakePath = Path.Combine(_testDirectory, "nonexistent.txt");
        var ex = await Record.ExceptionAsync(() => _service.TryDeleteFileAsync(fakePath));
        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that TryDeleteFileAsync deletes an existing file.
    /// </summary>
    [Fact]
    public async Task TryDeleteFileAsyncExistingFileDeletesFile()
    {
        var filePath = Path.Combine(_testDirectory, "test_async.txt");
        await File.WriteAllTextAsync(filePath, "content");

        Assert.True(File.Exists(filePath));

        await _service.TryDeleteFileAsync(filePath);

        Assert.False(File.Exists(filePath));
    }

    /// <summary>
    /// Verifies that TryDeleteFileAsync deletes a read-only file.
    /// </summary>
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

    /// <summary>
    /// Verifies that TryDeleteDirectory with a null path does not throw.
    /// </summary>
    [Fact]
    public void TryDeleteDirectoryNullPathDoesNotThrow()
    {
        var ex = Record.Exception(() => _service.TryDeleteDirectory(null!));
        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that TryDeleteDirectory with an empty path does not throw.
    /// </summary>
    [Fact]
    public void TryDeleteDirectoryEmptyPathDoesNotThrow()
    {
        var ex = Record.Exception(() => _service.TryDeleteDirectory(""));
        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that TryDeleteDirectory with a non-existent directory path does not throw.
    /// </summary>
    [Fact]
    public void TryDeleteDirectoryNonExistentDirectoryDoesNotThrow()
    {
        var fakePath = Path.Combine(_testDirectory, "nonexistent_dir");
        var ex = Record.Exception(() => _service.TryDeleteDirectory(fakePath));
        Assert.Null(ex);
    }

    /// <summary>
    /// Verifies that TryDeleteDirectory deletes an existing directory and its contents.
    /// </summary>
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

    /// <summary>
    /// Verifies that TryDeleteDirectory deletes a directory with deeply nested subdirectories.
    /// </summary>
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

    /// <summary>
    /// Verifies that TryDeleteFile handles file paths containing spaces.
    /// </summary>
    [Fact]
    public void TryDeleteFileWithSpacesInPathDeletesFile()
    {
        var filePath = Path.Combine(_testDirectory, "file with spaces.txt");
        File.WriteAllText(filePath, "content");

        _service.TryDeleteFile(filePath);

        Assert.False(File.Exists(filePath));
    }

    /// <summary>
    /// Verifies that TryDeleteFile handles files with long names.
    /// </summary>
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