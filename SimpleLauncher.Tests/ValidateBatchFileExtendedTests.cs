using SimpleLauncher.Services.GameLauncher;
using Xunit;

namespace SimpleLauncher.Tests;

public class ValidateBatchFileExtendedTests
{
    [Fact]
    public void ValidateBatchFileContentsNonExistentFileReturnsEmpty()
    {
        var result = ValidateBatchFile.ValidateBatchFileContents(@"C:\nonexistent_batch_file_test_12345.bat");
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateBatchFileContentsEmptyFileReturnsEmpty()
    {
        var tempFile = Path.GetTempFileName();
        File.Move(tempFile, Path.ChangeExtension(tempFile, ".bat"));
        var batFile = Path.ChangeExtension(tempFile, ".bat");
        try
        {
            File.WriteAllText(batFile, "");
            var result = ValidateBatchFile.ValidateBatchFileContents(batFile);
            Assert.Empty(result);
        }
        finally
        {
            if (File.Exists(batFile)) File.Delete(batFile);
        }
    }

    [Fact]
    public void ValidateBatchFileContentsCommentsAreSkipped()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.bat");
        File.WriteAllText(tempFile, """
                                   @echo off
                                   rem "C:\nonexistent\path.exe"
                                   :: "C:\another\fake.exe"
                                   # "C:\fake\app.exe"
                                   """);
        try
        {
            var result = ValidateBatchFile.ValidateBatchFileContents(tempFile);
            Assert.Empty(result);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void ValidateBatchFileContentsDetectsMissingPaths()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.bat");
        const string nonexistentPath = @"C:\nonexistent_folder_12345\fake.exe";
        File.WriteAllText(tempFile, $"@echo off{Environment.NewLine}start \"{nonexistentPath}\"");
        try
        {
            var result = ValidateBatchFile.ValidateBatchFileContents(tempFile);
            Assert.NotEmpty(result);
            var expectedExpanded = Environment.ExpandEnvironmentVariables(nonexistentPath);
            Assert.Contains(result, p => string.Equals(p, expectedExpanded, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void ValidateBatchFileContentsExistingFileNotReported()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.bat");
        try
        {
            var testExe = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.exe");
            File.WriteAllText(testExe, "test");
            File.WriteAllText(tempFile, $"start \"{testExe}\"");
            try
            {
                var result = ValidateBatchFile.ValidateBatchFileContents(tempFile);
                Assert.Empty(result);
            }
            finally
            {
                if (File.Exists(testExe)) File.Delete(testExe);
            }
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void FindInvalidQuotedPathsSimpleNonExistentFileReturnsEmpty()
    {
        var result = ValidateBatchFile.FindInvalidQuotedPathsSimple(@"C:\nonexistent_batch_12345.bat");
        Assert.Empty(result);
    }

    [Fact]
    public void FindInvalidQuotedPathsSimpleDetectsMissingPaths()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.bat");
        const string nonexistentPath = @"C:\nonexistent_folder_67890\tool.exe";
        File.WriteAllText(tempFile, $"\"{nonexistentPath}\"");
        try
        {
            var result = ValidateBatchFile.FindInvalidQuotedPathsSimple(tempFile);
            Assert.NotEmpty(result);
            var expectedExpanded = Environment.ExpandEnvironmentVariables(nonexistentPath);
            Assert.Contains(result, p => string.Equals(p, expectedExpanded, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void FindInvalidQuotedPathsSimpleSkipNonPaths()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.bat");
        File.WriteAllText(tempFile, """
                                   @echo off
                                   set "NAME=hello"
                                   echo "this is not a path"
                                   """);
        try
        {
            var result = ValidateBatchFile.FindInvalidQuotedPathsSimple(tempFile);
            Assert.Empty(result);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }

    [Fact]
    public void FindInvalidQuotedPathsSimpleEmptyFileReturnsEmpty()
    {
        var tempFile = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.bat");
        File.WriteAllText(tempFile, "");
        try
        {
            var result = ValidateBatchFile.FindInvalidQuotedPathsSimple(tempFile);
            Assert.Empty(result);
        }
        finally
        {
            if (File.Exists(tempFile)) File.Delete(tempFile);
        }
    }
}
