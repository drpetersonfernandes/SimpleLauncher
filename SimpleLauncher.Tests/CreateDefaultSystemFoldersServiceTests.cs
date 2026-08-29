using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
///     Tests for <see cref="CreateDefaultSystemFoldersService" /> using real temp directories,
///     a mocked <see cref="IMessageBoxLibraryService" /> and a mocked logger.
/// </summary>
public class CreateDefaultSystemFoldersServiceTests : IDisposable
{
    private readonly ILogger _logger = new NoOpLogger();
    private readonly Mock<IMessageBoxLibraryService> _messageBoxMock = new();
    private readonly string _testRoot;

    public CreateDefaultSystemFoldersServiceTests()
    {
        _testRoot = Path.Combine(Path.GetTempPath(), $"SL_CreateFolders_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testRoot);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testRoot)) Directory.Delete(_testRoot, true);
        }
        catch
        {
            // Best-effort cleanup
        }
    }

    private static IConfiguration ConfigWithAdditionalFolders(params string[] folders)
    {
        var dictionary = new Dictionary<string, string?>(StringComparer.Ordinal);
        for (var i = 0; i < folders.Length; i++) dictionary[$"AdditionalFolders:{i}"] = folders[i];

        return new ConfigurationBuilder().AddInMemoryCollection(dictionary).Build();
    }

    [Fact]
    public async Task CreateFoldersAsync_CreatesSystemAndImageFolders()
    {
        var systemFolder = Path.Combine(_testRoot, "system");
        var imageFolder = Path.Combine(_testRoot, "images");

        await CreateDefaultSystemFoldersService.CreateFoldersAsync(
            "Test System", systemFolder, imageFolder, ConfigWithAdditionalFolders(), _logger, _messageBoxMock.Object);

        Assert.True(Directory.Exists(systemFolder));
        Assert.True(Directory.Exists(imageFolder));
        _messageBoxMock.Verify(x => x.FolderCreationFailedMessageBoxAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateFoldersAsync_ExistingFolders_AreSkippedWithoutError()
    {
        var systemFolder = Path.Combine(_testRoot, "system");
        var imageFolder = Path.Combine(_testRoot, "images");
        Directory.CreateDirectory(systemFolder);
        Directory.CreateDirectory(imageFolder);

        await CreateDefaultSystemFoldersService.CreateFoldersAsync(
            "Test System", systemFolder, imageFolder, ConfigWithAdditionalFolders(), _logger, _messageBoxMock.Object);

        _messageBoxMock.Verify(x => x.FolderCreationFailedMessageBoxAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateFoldersAsync_NullOrEmptySystemFolder_IsSkippedSafely()
    {
        var imageFolder = Path.Combine(_testRoot, "images");

        await CreateDefaultSystemFoldersService.CreateFoldersAsync(
            "Test System", "", imageFolder, ConfigWithAdditionalFolders(), _logger, _messageBoxMock.Object);

        Assert.True(Directory.Exists(imageFolder));
        _messageBoxMock.Verify(x => x.FolderCreationFailedMessageBoxAsync(), Times.Never);
    }

    [Fact]
    public async Task CreateFoldersAsync_CreatesAdditionalFoldersUnderBaseDirectory()
    {
        var uniqueSystemName = $"SL_Test_{Guid.NewGuid():N}";
        try
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var romsFolder = Path.Combine(baseDirectory, "roms", uniqueSystemName);

            await CreateDefaultSystemFoldersService.CreateFoldersAsync(
                uniqueSystemName,
                Path.Combine(_testRoot, "system"),
                Path.Combine(_testRoot, "images"),
                ConfigWithAdditionalFolders("roms"),
                _logger,
                _messageBoxMock.Object);

            Assert.True(Directory.Exists(romsFolder));
        }
        finally
        {
            var romsRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "roms", uniqueSystemName);
            if (Directory.Exists(romsRoot)) Directory.Delete(romsRoot, true);
        }
    }

    [Fact]
    public async Task CreateFoldersAsync_SystemFolderPathBlockedByFile_ShowsErrorMessage()
    {
        // A file with the target name blocks directory creation
        var blockedPath = Path.Combine(_testRoot, "blocked");
        File.WriteAllText(blockedPath, "I am a file, not a folder");

        await CreateDefaultSystemFoldersService.CreateFoldersAsync(
            "Test System", blockedPath, Path.Combine(_testRoot, "images"), ConfigWithAdditionalFolders(), _logger,
            _messageBoxMock.Object);

        _messageBoxMock.Verify(x => x.FolderCreationFailedMessageBoxAsync(), Times.Once);
    }

    [Fact]
    public async Task CreateFoldersAsync_ImageFolderPathBlockedByFile_ShowsErrorMessage()
    {
        var blockedPath = Path.Combine(_testRoot, "blocked-image");
        File.WriteAllText(blockedPath, "I am a file, not a folder");

        await CreateDefaultSystemFoldersService.CreateFoldersAsync(
            "Test System", Path.Combine(_testRoot, "system"), blockedPath, ConfigWithAdditionalFolders(), _logger,
            _messageBoxMock.Object);

        _messageBoxMock.Verify(x => x.FolderCreationFailedMessageBoxAsync(), Times.Once);
    }
}