using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.CleanAndDeleteFiles;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="CleanSimpleLauncherFolderService"/> class.
/// </summary>
public class CleanSimpleLauncherFolderServiceTests
{
    [Fact]
    public void ConstructorDoesNotThrow()
    {
        var service = new CleanSimpleLauncherFolderService(new NoOpDeleteFilesService());
        Assert.NotNull(service);
    }

    [Fact]
    public void ImplementsICleanSimpleLauncherFolderService()
    {
        var service = new CleanSimpleLauncherFolderService(new NoOpDeleteFilesService());
        Assert.IsAssignableFrom<ICleanSimpleLauncherFolderService>(service);
    }

    [Fact]
    public void CleanupTrashDoesNotThrow()
    {
        var service = new CleanSimpleLauncherFolderService(new NoOpDeleteFilesService());
        var exception = Record.Exception(service.CleanupTrash);
        Assert.Null(exception);
    }

    [Fact]
    public void CleanupTempFilesDoesNotThrow()
    {
        var service = new CleanSimpleLauncherFolderService(new NoOpDeleteFilesService());
        var exception = Record.Exception(service.CleanupTempFiles);
        Assert.Null(exception);
    }

    [Fact]
    public void CleanupTrashCalledTwiceDoesNotThrow()
    {
        var service = new CleanSimpleLauncherFolderService(new NoOpDeleteFilesService());
        service.CleanupTrash();
        var exception = Record.Exception(service.CleanupTrash);
        Assert.Null(exception);
    }

    [Fact]
    public void CleanupTempFilesCalledTwiceDoesNotThrow()
    {
        var service = new CleanSimpleLauncherFolderService(new NoOpDeleteFilesService());
        service.CleanupTempFiles();
        var exception = Record.Exception(service.CleanupTempFiles);
        Assert.Null(exception);
    }

    [Fact]
    public void CleanupTrashCallsDeleteForTempDirectory()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), "SimpleLauncher");
        Directory.CreateDirectory(tempDir);
        var tempFile = Path.Combine(tempDir, "test.txt");
        File.WriteAllText(tempFile, "test");

        try
        {
            var deleteService = new TrackingDeleteFilesService();
            var service = new CleanSimpleLauncherFolderService(deleteService);

            // CleanupTempFiles should delete the temp directory
            service.CleanupTempFiles();

            // The directory should be gone (if it was accessible)
            Assert.False(Directory.Exists(tempDir));
            Assert.NotEmpty(deleteService.DeletedFiles);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    private sealed class NoOpDeleteFilesService : IDeleteFilesService
    {
        public void TryDeleteFile(string filePath) { }
        public Task TryDeleteFileAsync(string filePath)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class TrackingDeleteFilesService : IDeleteFilesService
    {
        public List<string> DeletedFiles { get; } = [];

        public void TryDeleteFile(string filePath)
        {
            DeletedFiles.Add(filePath);
            try
            {
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch
            {
                // ignore
            }
        }

        public Task TryDeleteFileAsync(string filePath)
        {
            TryDeleteFile(filePath);
            return Task.CompletedTask;
        }
    }
}
