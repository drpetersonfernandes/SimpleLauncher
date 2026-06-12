using System.IO.Compression;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.GameLauncher.MountFiles;
using Xunit;

namespace SimpleLauncher.Tests;

using Interfaces;

public class MountZipFilesTests
{
    private static void InvokeValidateZipForPathTraversal(string zipPath)
    {
        var method = typeof(MountZipFiles).GetMethod("ValidateZipForPathTraversal", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(method);
        var instance = CreateMountZipFilesInstance();
        method.Invoke(instance, [zipPath]);
    }

    private static MountZipFiles CreateMountZipFilesInstance()
    {
        var debugLogger = new NoOpDebugLogger();
        var configuration = new ConfigurationBuilder().Build();
        var constructor = typeof(MountZipFiles).GetConstructors(BindingFlags.Instance | BindingFlags.Public).First();
        return (MountZipFiles)constructor.Invoke([configuration, debugLogger]);
    }

    private static string CreateTestZip(string[] entryNames)
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
        using var archive = ZipFile.Open(tempFile, ZipArchiveMode.Create);
        foreach (var entryName in entryNames)
        {
            var entry = archive.CreateEntry(entryName);
            using var stream = entry.Open();
            using var writer = new StreamWriter(stream);
            writer.Write("test content");
        }

        return tempFile;
    }

    [Fact]
    public void ValidateZipForPathTraversalValidZipDoesNotThrow()
    {
        var zipPath = CreateTestZip(["roms/game.bin", "roms/game.cue"]);
        try
        {
            var ex = Record.Exception(() => InvokeValidateZipForPathTraversal(zipPath));
            Assert.Null(ex);
        }
        finally
        {
            File.Delete(zipPath);
        }
    }

    [Fact]
    public void ValidateZipForPathTraversalDotDotEntryThrowsInvalidOperationException()
    {
        var zipPath = CreateTestZip(["roms/../evil.exe"]);
        try
        {
            Assert.Throws<TargetInvocationException>(() => InvokeValidateZipForPathTraversal(zipPath));
        }
        finally
        {
            File.Delete(zipPath);
        }
    }

    [Fact]
    public void ValidateZipForPathTraversalRootedPathEntryThrowsInvalidOperationException()
    {
        var zipPath = CreateTestZip(["/Windows/System32/evil.exe"]);
        try
        {
            Assert.Throws<TargetInvocationException>(() => InvokeValidateZipForPathTraversal(zipPath));
        }
        finally
        {
            File.Delete(zipPath);
        }
    }

    [Fact]
    public void ValidateZipForPathTraversalLeadingSlashEntryThrowsInvalidOperationException()
    {
        var zipPath = CreateTestZip(["/evil.exe"]);
        try
        {
            Assert.Throws<TargetInvocationException>(() => InvokeValidateZipForPathTraversal(zipPath));
        }
        finally
        {
            File.Delete(zipPath);
        }
    }

    [Fact]
    public void ValidateZipForPathTraversalLeadingBackslashEntryThrowsInvalidOperationException()
    {
        var zipPath = CreateTestZip(["\\evil.exe"]);
        try
        {
            Assert.Throws<TargetInvocationException>(() => InvokeValidateZipForPathTraversal(zipPath));
        }
        finally
        {
            File.Delete(zipPath);
        }
    }

    [Fact]
    public void ValidateZipForPathTraversalNonExistentFileThrowsFileNotFoundException()
    {
        var fakePath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.zip");
        Assert.Throws<TargetInvocationException>(() => InvokeValidateZipForPathTraversal(fakePath));
    }

    [Fact]
    public void ValidateZipForPathTraversalEmptyZipDoesNotThrow()
    {
        var zipPath = CreateTestZip([]);
        try
        {
            var ex = Record.Exception(() => InvokeValidateZipForPathTraversal(zipPath));
            Assert.Null(ex);
        }
        finally
        {
            File.Delete(zipPath);
        }
    }

    [Fact]
    public void ValidateZipForPathTraversalDeepNestedValidPathsDoesNotThrow()
    {
        var zipPath = CreateTestZip([
            "level1/level2/level3/game.bin",
            "level1/level2/level3/game.cue"
        ]);
        try
        {
            var ex = Record.Exception(() => InvokeValidateZipForPathTraversal(zipPath));
            Assert.Null(ex);
        }
        finally
        {
            File.Delete(zipPath);
        }
    }

    private sealed class NoOpDebugLogger : IDebugLogger
    {
        public void Log(string message)
        {
        }

        public void LogException(Exception ex, string? contextMessage = null)
        {
        }
    }
}
