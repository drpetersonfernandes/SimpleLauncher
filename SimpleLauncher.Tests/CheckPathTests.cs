using SimpleLauncher.Services.CheckPaths;
using Xunit;

namespace SimpleLauncher.Tests;

public class CheckPathTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidPathInvalidInputReturnsFalse(string path)
    {
        var result = CheckPath.IsValidPath(path);
        Assert.False(result);
    }

    [Fact]
    public void IsValidPathNullReturnsFalse()
    {
        var result = CheckPath.IsValidPath(null);
        Assert.False(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void IsValidEmulatorExecutablePathInvalidInputReturnsFalse(string path)
    {
        var result = CheckPath.IsValidEmulatorExecutablePath(path);
        Assert.False(result);
    }

    [Fact]
    public void IsValidEmulatorExecutablePathNullReturnsFalse()
    {
        var result = CheckPath.IsValidEmulatorExecutablePath(null);
        Assert.False(result);
    }

    [Fact]
    public void IsValidPathExistingFileReturnsTrue()
    {
        var tempFile = Path.GetTempFileName();
        try
        {
            var result = CheckPath.IsValidPath(tempFile);
            Assert.True(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void IsValidPathExistingDirectoryReturnsTrue()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var result = CheckPath.IsValidPath(tempDir);
            Assert.True(result);
        }
        finally
        {
            Directory.Delete(tempDir);
        }
    }

    [Fact]
    public void IsValidPathNonExistentPathReturnsFalse()
    {
        var fakePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString(), "fake.txt");
        var result = CheckPath.IsValidPath(fakePath);
        Assert.False(result);
    }

    [Fact]
    public void IsValidEmulatorExecutablePathNonExeFileReturnsFalse()
    {
        var tempFile = Path.GetTempFileName(); // .tmp extension
        try
        {
            var result = CheckPath.IsValidEmulatorExecutablePath(tempFile);
            Assert.False(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void IsValidEmulatorExecutablePathExeFileReturnsTrue()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.exe");
        File.WriteAllText(tempFile, "fake exe");
        try
        {
            var result = CheckPath.IsValidEmulatorExecutablePath(tempFile);
            Assert.True(result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }
}
