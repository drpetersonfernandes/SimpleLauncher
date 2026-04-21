using SimpleLauncher.Services.CheckIfDirectoryIsWritable;
using Xunit;

namespace SimpleLauncher.Tests;

public class CheckIfDirectoryIsWritableTests
{
    [Fact]
    public void IsWritableDirectory_NonExistent_ReturnsFalse()
    {
        var fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "nonexistent");
        var result = CheckIfDirectoryIsWritable.IsWritableDirectory(fakePath);
        Assert.False(result);
    }

    [Fact]
    public void IsWritableDirectory_TempDirectory_ReturnsTrue()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var result = CheckIfDirectoryIsWritable.IsWritableDirectory(tempDir);
            Assert.True(result);
        }
        finally
        {
            Directory.Delete(tempDir);
        }
    }

    [Fact]
    public void IsWritableDirectory_LeavesNoTempFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            _ = CheckIfDirectoryIsWritable.IsWritableDirectory(tempDir);
            var files = Directory.GetFiles(tempDir, "*.tmp");
            Assert.Empty(files);
        }
        finally
        {
            Directory.Delete(tempDir);
        }
    }
}
