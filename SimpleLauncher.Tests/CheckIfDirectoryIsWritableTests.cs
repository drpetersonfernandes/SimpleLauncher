using SimpleLauncher.Core.Services.CheckIfDirectoryIsWritable;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests the <see cref="CheckIfDirectoryIsWritableService"/> utility for verifying directory write access.
/// </summary>
public class CheckIfDirectoryIsWritableTests
{
    private static readonly ILogger NullLogErrors = new NoOpLogger();

    /// <summary>
    /// Verifies that a non-existent directory path returns false (not writable).
    /// </summary>
    [Fact]
    public void IsWritableDirectoryNonExistentReturnsFalse()
    {
        var fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "nonexistent");
        var result = CheckIfDirectoryIsWritableService.IsWritableDirectory(fakePath, NullLogErrors);
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that a writable temporary directory returns true.
    /// </summary>
    [Fact]
    public void IsWritableDirectoryTempDirectoryReturnsTrue()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var result = CheckIfDirectoryIsWritableService.IsWritableDirectory(tempDir, NullLogErrors);
            Assert.True(result);
        }
        finally
        {
            Directory.Delete(tempDir);
        }
    }

    /// <summary>
    /// Verifies that the writable directory check leaves no temporary files behind.
    /// </summary>
    [Fact]
    public void IsWritableDirectoryLeavesNoTempFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            _ = CheckIfDirectoryIsWritableService.IsWritableDirectory(tempDir, NullLogErrors);
            var files = Directory.GetFiles(tempDir, "*.tmp");
            Assert.Empty(files);
        }
        finally
        {
            Directory.Delete(tempDir);
        }
    }
}
