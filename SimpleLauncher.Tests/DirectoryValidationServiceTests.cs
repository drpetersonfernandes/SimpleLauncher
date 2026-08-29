using System.Diagnostics.CodeAnalysis;
using SimpleLauncher.Core.Services.CheckIfDirectoryIsWritable;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
///     Tests for the <see cref="DirectoryValidationService" /> class.
/// </summary>
[SuppressMessage("ReSharper", "NullableWarningSuppressionIsUsed")]
public class DirectoryValidationServiceTests : IDisposable
{
    private readonly DirectoryValidationService _service;

    /// <summary>
    ///     Initializes a new instance of <see cref="DirectoryValidationServiceTests" /> with a mock service provider.
    /// </summary>
    public DirectoryValidationServiceTests()
    {
        ServiceProviderMock.Install();
        _service = new DirectoryValidationService(new NoOpLogger());
    }

    /// <summary>
    ///     Restores the service provider mock state.
    /// </summary>
    public void Dispose()
    {
    }

    /// <summary>
    ///     Verifies that IsWritableDirectory returns false for a null path.
    /// </summary>
    [Fact]
    public void IsWritableDirectoryNullPathReturnsFalse()
    {
        var result = _service.IsWritableDirectory(null!);
        Assert.False(result);
    }

    /// <summary>
    ///     Verifies that IsWritableDirectory returns false for an empty path.
    /// </summary>
    [Fact]
    public void IsWritableDirectoryEmptyPathReturnsFalse()
    {
        var result = _service.IsWritableDirectory("");
        Assert.False(result);
    }

    /// <summary>
    ///     Verifies that IsWritableDirectory returns false for a non-existent directory.
    /// </summary>
    [Fact]
    public void IsWritableDirectoryNonExistentDirectoryReturnsFalse()
    {
        var fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var result = _service.IsWritableDirectory(fakePath);
        Assert.False(result);
    }

    /// <summary>
    ///     Verifies that IsWritableDirectory returns true for a writable directory.
    /// </summary>
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

    /// <summary>
    ///     Verifies that IsWritableDirectory cleans up the temporary test file it creates.
    /// </summary>
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

    /// <summary>
    ///     Verifies that IsWritableDirectory returns false when given a file path instead of a directory.
    /// </summary>
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
}