using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
///     Tests for <see cref="CheckForRequiredFilesService" /> using the test bin directory
///     (the app base directory) and a mocked <see cref="IMessageBoxLibraryService" />.
///     Note: the service reads the required-files list via <c>GetValue&lt;string[]&gt;</c>, which
///     does not bind array config keys, so the hardcoded default list is used in practice.
/// </summary>
public class CheckForRequiredFilesServiceTests
{
    private readonly Mock<IMessageBoxLibraryService> _messageBoxMock = new();
    private readonly CheckForRequiredFilesService _service;

    public CheckForRequiredFilesServiceTests()
    {
        _service = new CheckForRequiredFilesService(_messageBoxMock.Object);
    }

    private static IConfiguration EmptyConfiguration()
    {
        return new ConfigurationBuilder().AddInMemoryCollection().Build();
    }

    [Fact]
    public async Task CheckFilesAsync_AllDefaultFilesPresent_DoesNotShowMessageBox()
    {
        // The SimpleLauncher project copies all default files to the test output,
        // so the default list must pass without any dialog.
        await _service.CheckFilesAsync(EmptyConfiguration(), new NoOpLogger());

        _messageBoxMock.Verify(x => x.HandleMissingRequiredFilesMessageBoxAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task CheckFilesAsync_MissingDefaultFile_ShowsMessageBoxWithFullPath()
    {
        // Temporarily remove one of the shipped default files so the check reports it,
        // then restore it. No other test depends on this file existing on disk.
        var clickSound = Path.Combine(AppContext.BaseDirectory, "audio", "click.mp3");
        var backup = clickSound + ".bak";
        var wasPresent = File.Exists(clickSound);

        try
        {
            if (wasPresent) File.Move(clickSound, backup);

            string? capturedList = null;
            _messageBoxMock
                .Setup(x => x.HandleMissingRequiredFilesMessageBoxAsync(It.IsAny<string>()))
                .Callback<string>(list => { capturedList = list; })
                .Returns(Task.CompletedTask);

            await _service.CheckFilesAsync(EmptyConfiguration(), new NoOpLogger());

            Assert.NotNull(capturedList);
            Assert.Contains(clickSound, capturedList, StringComparison.Ordinal);
        }
        finally
        {
            if (wasPresent && File.Exists(backup)) File.Move(backup, clickSound);
        }
    }

    [Fact]
    public async Task CheckFilesAsync_MissingFileException_IsLoggedAndDoesNotThrow()
    {
        var messageBoxMock = new Mock<IMessageBoxLibraryService>();
        messageBoxMock
            .Setup(x => x.HandleMissingRequiredFilesMessageBoxAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("MessageBox failure"));
        var service = new CheckForRequiredFilesService(messageBoxMock.Object);
        var logger = new NoOpLogger();

        var clickSound = Path.Combine(AppContext.BaseDirectory, "audio", "click.mp3");
        var backup = clickSound + ".bak";
        var wasPresent = File.Exists(clickSound);

        try
        {
            if (wasPresent) File.Move(clickSound, backup);

            await service.CheckFilesAsync(EmptyConfiguration(), logger);
            // No exception expected; the error is logged
        }
        finally
        {
            if (wasPresent && File.Exists(backup)) File.Move(backup, clickSound);
        }
    }
}