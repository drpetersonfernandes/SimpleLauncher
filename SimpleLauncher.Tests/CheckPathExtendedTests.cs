using SimpleLauncher.Services.CheckPaths;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for <see cref="CheckPath"/> covering additional edge cases for
/// path validation and emulator executable detection.
/// </summary>
public class CheckPathExtendedTests
{
    [Fact]
    public void IsValidPathDirectoryWithSpacesReturnsTrue()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"dir with spaces {Guid.NewGuid()}");
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
    public void IsValidPathFileWithSpacesReturnsTrue()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"file with spaces {Guid.NewGuid()}.txt");
        File.WriteAllText(tempFile, "test");
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
    public void IsValidPathNestedNonExistentPathReturnsFalse()
    {
        var result = CheckPath.IsValidPath(@"C:\nonexistent\a\b\c\d\file.txt");
        Assert.False(result);
    }

    [Fact]
    public void IsValidEmulatorExecutablePathDirectoryReturnsFalse()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var result = CheckPath.IsValidEmulatorExecutablePath(tempDir);
            Assert.False(result);
        }
        finally
        {
            Directory.Delete(tempDir);
        }
    }

    [Theory]
    [InlineData(".exe", true)]
    [InlineData(".bat", true)]
    [InlineData(".lnk", true)]
    [InlineData(".txt", false)]
    [InlineData(".zip", false)]
    [InlineData(".dll", false)]
    [InlineData(".cmd", false)]
    public void IsValidEmulatorExecutablePathByExtension(string extension, bool expected)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{extension}");
        File.WriteAllText(tempFile, "fake content");
        try
        {
            var result = CheckPath.IsValidEmulatorExecutablePath(tempFile);
            Assert.Equal(expected, result);
        }
        finally
        {
            File.Delete(tempFile);
        }
    }

    [Fact]
    public void IsValidEmulatorExecutablePathExeUpperCaseReturnsTrue()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.EXE");
        File.WriteAllText(tempFile, "fake");
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

    [Fact]
    public void IsValidEmulatorExecutablePathBatMixedCaseReturnsTrue()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.Bat");
        File.WriteAllText(tempFile, "@echo off");
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

    [Fact]
    public void IsValidPathWithBaseFolderPlaceholderResolvesToAppDirectory()
    {
        var result = CheckPath.IsValidPath("%BASEFOLDER%");
        Assert.True(result); // App directory should exist
    }

    [Fact]
    public void IsValidEmulatorExecutablePathNonExistentFileReturnsFalse()
    {
        var result = CheckPath.IsValidEmulatorExecutablePath(@"C:\nonexistent\emulator.exe");
        Assert.False(result);
    }

    [Fact]
    public void IsValidPathVeryLongPathReturnsFalse()
    {
        var longPath = @"C:\" + new string('a', 3000) + @"\file.txt";
        var result = CheckPath.IsValidPath(longPath);
        Assert.False(result);
    }
}
