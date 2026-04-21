using SimpleLauncher.Services.CheckForFileLock;
using Xunit;

namespace SimpleLauncher.Tests;

public class CheckForFileLockTests
{
    [Theory]
    [InlineData("")]
    public void IsFileLocked_Empty_ReturnsFalse(string filePath)
    {
        var result = CheckForFileLock.IsFileLocked(filePath);
        Assert.False(result);
    }

    [Fact]
    public void IsFileLocked_Null_ReturnsFalse()
    {
        var result = CheckForFileLock.IsFileLocked(null);
        Assert.False(result);
    }

    [Fact]
    public void IsFileLocked_NonExistentFile_ReturnsFalse()
    {
        var fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "fake.txt");
        var result = CheckForFileLock.IsFileLocked(fakePath);
        Assert.False(result);
    }

    [Fact]
    public void IsFileLocked_ExistingUnlockedFile_ReturnsFalse()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var result = CheckForFileLock.IsFileLocked(tempFile);
            Assert.False(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void IsFileLocked_LockedFile_ReturnsTrue()
    {
        var tempFile = Path.GetTempFileName();
        using var stream = new FileStream(tempFile, FileMode.Open, FileAccess.Read, FileShare.None);
        try
        {
            var result = CheckForFileLock.IsFileLocked(tempFile);
            Assert.True(result);
        }
        finally
        {
            stream.Close();
            File.Delete(tempFile);
        }
    }
}
