using System.Diagnostics.CodeAnalysis;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.CheckIfDirectoryIsWritable;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="DirectoryValidationService"/> class.
/// </summary>
[SuppressMessage("ReSharper", "NullableWarningSuppressionIsUsed")]
public class DirectoryValidationServiceTests : IDisposable
{
    private readonly DirectoryValidationService _service;

    public DirectoryValidationServiceTests()
    {
        ServiceProviderMock.Install();
        _service = new DirectoryValidationService(new NoOpLogErrors());
    }

    public void Dispose()
    {
        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public void IsWritableDirectoryNullPathReturnsFalse()
    {
        var result = _service.IsWritableDirectory(null!);
        Assert.False(result);
    }

    [Fact]
    public void IsWritableDirectoryEmptyPathReturnsFalse()
    {
        var result = _service.IsWritableDirectory("");
        Assert.False(result);
    }

    [Fact]
    public void IsWritableDirectoryNonExistentDirectoryReturnsFalse()
    {
        var fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var result = _service.IsWritableDirectory(fakePath);
        Assert.False(result);
    }

    [Fact]
    public void IsWritableDirectoryWritableDirectoryReturnsTrue()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            var result = _service.IsWritableDirectory(tempDir);
            Assert.True(result);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void IsWritableDirectoryCleansUpTestFile()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);

        try
        {
            _service.IsWritableDirectory(tempDir);
            var tmpFiles = Directory.GetFiles(tempDir, "*.tmp");
            Assert.Empty(tmpFiles);
        }
        finally
        {
            Directory.Delete(tempDir, true);
        }
    }

    [Fact]
    public void IsWritableDirectoryFilePathReturnsFalse()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        File.WriteAllText(tempFile, "test");

        try
        {
            var result = _service.IsWritableDirectory(tempFile);
            Assert.False(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }
}
