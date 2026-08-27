using SimpleLauncher.Core.Services.CheckForFileLock;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests the <see cref="CheckForFileLockService"/> utility for detecting locked files.
/// </summary>
public class CheckForFileLockTests
{
    /// <summary>
    /// Verifies that an empty file path returns false (not locked).
    /// </summary>
    [Theory]
    [InlineData("")]
    public void IsFileLockedEmptyReturnsFalse(string filePath)
    {
        var result = CheckForFileLockService.IsFileLocked(filePath);
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that a null file path returns false (not locked).
    /// </summary>
    [Fact]
    public void IsFileLockedNullReturnsFalse()
    {
        var result = CheckForFileLockService.IsFileLocked(null!);
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that a non-existent file path returns false (not locked).
    /// </summary>
    [Fact]
    public void IsFileLockedNonExistentFileReturnsFalse()
    {
        var fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "fake.txt");
        var result = CheckForFileLockService.IsFileLocked(fakePath);
        Assert.False(result);
    }

    /// <summary>
    /// Verifies that an existing unlocked file returns false (not locked).
    /// </summary>
    [Fact]
    public void IsFileLockedExistingUnlockedFileReturnsFalse()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var result = CheckForFileLockService.IsFileLocked(tempFile);
            Assert.False(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// Verifies that a file locked with an exclusive handle returns true.
    /// </summary>
    [Fact]
    public void IsFileLockedLockedFileReturnsTrue()
    {
        var tempFile = Path.GetTempFileName();
        using var stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.None);
        try
        {
            var result = CheckForFileLockService.IsFileLocked(tempFile);
            Assert.True(result);
        }
        finally
        {
            stream.Close();
            File.Delete(tempFile);
        }
    }
}