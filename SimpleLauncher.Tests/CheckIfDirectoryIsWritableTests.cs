using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.CheckIfDirectoryIsWritable;
using Xunit;

namespace SimpleLauncher.Tests;

public class CheckIfDirectoryIsWritableTests
{
    private static readonly ILogErrors NullLogErrors = new NullLogErrorsImpl();

    [Fact]
    public void IsWritableDirectoryNonExistentReturnsFalse()
    {
        var fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "nonexistent");
        var result = CheckIfDirectoryIsWritable.IsWritableDirectory(fakePath, NullLogErrors);
        Assert.False(result);
    }

    [Fact]
    public void IsWritableDirectoryTempDirectoryReturnsTrue()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var result = CheckIfDirectoryIsWritable.IsWritableDirectory(tempDir, NullLogErrors);
            Assert.True(result);
        }
        finally
        {
            Directory.Delete(tempDir);
        }
    }

    [Fact]
    public void IsWritableDirectoryLeavesNoTempFiles()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            _ = CheckIfDirectoryIsWritable.IsWritableDirectory(tempDir, NullLogErrors);
            var files = Directory.GetFiles(tempDir, "*.tmp");
            Assert.Empty(files);
        }
        finally
        {
            Directory.Delete(tempDir);
        }
    }

    private sealed class NullLogErrorsImpl : ILogErrors
    {
        public Task LogErrorAsync(Exception ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }
}
