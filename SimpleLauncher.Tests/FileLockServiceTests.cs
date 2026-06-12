using System.Diagnostics.CodeAnalysis;
using SimpleLauncher.Services.CheckForFileLock;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="FileLockService"/> class.
/// </summary>
[SuppressMessage("ReSharper", "NullableWarningSuppressionIsUsed")]
public class FileLockServiceTests
{
    private readonly FileLockService _service = new();

    [Fact]
    public void IsFileLockedNullPathReturnsFalse()
    {
        var result = _service.IsFileLocked(null!);
        Assert.False(result);
    }

    [Fact]
    public void IsFileLockedEmptyPathReturnsFalse()
    {
        var result = _service.IsFileLocked("");
        Assert.False(result);
    }

    [Fact]
    public void IsFileLockedNonExistentFileReturnsFalse()
    {
        var fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "nonexistent.txt");
        var result = _service.IsFileLocked(fakePath);
        Assert.False(result);
    }

    [Fact]
    public void IsFileLockedUnlockedFileReturnsFalse()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        File.WriteAllText(tempFile, "test content");

        try
        {
            var result = _service.IsFileLocked(tempFile);
            Assert.False(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void IsFileLockedLockedFileReturnsTrue()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".txt");
        File.WriteAllText(tempFile, "test content");

        try
        {
            using var stream = new FileStream(tempFile, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            var result = _service.IsFileLocked(tempFile);
            Assert.True(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void IsFileLockedWhitespacePathReturnsFalse()
    {
        var result = _service.IsFileLocked("   ");
        Assert.False(result);
    }
}
