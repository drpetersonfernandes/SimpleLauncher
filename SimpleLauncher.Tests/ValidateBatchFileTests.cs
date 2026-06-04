using SimpleLauncher.Services.GameLauncher;
using Xunit;

namespace SimpleLauncher.Tests;

public class ValidateBatchFileTests : IDisposable
{
    private readonly string _testDirectory;

    public ValidateBatchFileTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_BatchTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
        }
        catch
        {
            // Best-effort cleanup
        }

        GC.SuppressFinalize(this);
    }

    [Fact]
    public void ValidateBatchFileContents_NonExistentFile_ReturnsEmptyList()
    {
        var result = ValidateBatchFile.ValidateBatchFileContents(@"C:\nonexistent\file.bat");
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateBatchFileContents_EmptyFile_ReturnsEmptyList()
    {
        var batchFile = Path.Combine(_testDirectory, "empty.bat");
        File.WriteAllText(batchFile, "");

        var result = ValidateBatchFile.ValidateBatchFileContents(batchFile);
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateBatchFileContents_ValidPaths_ReturnsEmptyList()
    {
        var batchFile = Path.Combine(_testDirectory, "valid.bat");
        File.WriteAllText(batchFile, "@echo off\necho hello\n");

        var result = ValidateBatchFile.ValidateBatchFileContents(batchFile);
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateBatchFileContents_MissingPaths_ReturnsMissingPaths()
    {
        var batchFile = Path.Combine(_testDirectory, "missing.bat");
        File.WriteAllText(batchFile, "@echo off\n\"C:\\nonexistent\\game.exe\" --arg\n");

        var result = ValidateBatchFile.ValidateBatchFileContents(batchFile);
        Assert.Contains(result, static p => p.Contains("game.exe"));
    }

    [Fact]
    public void ValidateBatchFileContents_RemLinesAreSkipped()
    {
        var batchFile = Path.Combine(_testDirectory, "rem.bat");
        File.WriteAllText(batchFile, "rem \"C:\\nonexistent\\file.exe\"\n:: also a comment\n");

        var result = ValidateBatchFile.ValidateBatchFileContents(batchFile);
        Assert.Empty(result);
    }

    [Fact]
    public void ValidateBatchFileContents_CommentsWithHashAreSkipped()
    {
        var batchFile = Path.Combine(_testDirectory, "comment.bat");
        File.WriteAllText(batchFile, "# \"C:\\nonexistent\\file.exe\"\n");

        var result = ValidateBatchFile.ValidateBatchFileContents(batchFile);
        Assert.Empty(result);
    }

    [Fact]
    public void FindInvalidQuotedPathsSimple_NonExistentFile_ReturnsEmptyList()
    {
        var result = ValidateBatchFile.FindInvalidQuotedPathsSimple(@"C:\nonexistent\file.bat");
        Assert.Empty(result);
    }

    [Fact]
    public void FindInvalidQuotedPathsSimple_EmptyFile_ReturnsEmptyList()
    {
        var batchFile = Path.Combine(_testDirectory, "empty2.bat");
        File.WriteAllText(batchFile, "");

        var result = ValidateBatchFile.FindInvalidQuotedPathsSimple(batchFile);
        Assert.Empty(result);
    }

    [Fact]
    public void FindInvalidQuotedPathsSimple_MissingDrivePath_ReturnsPath()
    {
        var batchFile = Path.Combine(_testDirectory, "drive.bat");
        File.WriteAllText(batchFile, "\"C:\\nonexistent\\path\\file.exe\"\n");

        var result = ValidateBatchFile.FindInvalidQuotedPathsSimple(batchFile);
        Assert.Single(result);
        Assert.Contains("file.exe", result[0]);
    }

    [Fact]
    public void FindInvalidQuotedPathsSimple_ValidExistingPath_ReturnsEmpty()
    {
        var existingDir = Path.Combine(_testDirectory, "existing");
        Directory.CreateDirectory(existingDir);

        var batchFile = Path.Combine(_testDirectory, "valid.bat");
        File.WriteAllText(batchFile, $"\"{existingDir}\"\n");

        var result = ValidateBatchFile.FindInvalidQuotedPathsSimple(batchFile);
        Assert.Empty(result);
    }

    [Fact]
    public void FindInvalidQuotedPathsSimple_UncPath_IsDetectedAsPath()
    {
        var batchFile = Path.Combine(_testDirectory, "unc.bat");
        File.WriteAllText(batchFile, "\"\\\\server\\share\\file.exe\"\n");

        var result = ValidateBatchFile.FindInvalidQuotedPathsSimple(batchFile);
        Assert.Single(result);
    }

    [Fact]
    public void FindInvalidQuotedPathsSimple_NonPathText_IsIgnored()
    {
        var batchFile = Path.Combine(_testDirectory, "nonpath.bat");
        File.WriteAllText(batchFile, "\"just some text\"\n");

        var result = ValidateBatchFile.FindInvalidQuotedPathsSimple(batchFile);
        Assert.Empty(result);
    }

    [Fact]
    public void FindInvalidQuotedPathsSimple_MultipleQuotedPaths_FindsAll()
    {
        var batchFile = Path.Combine(_testDirectory, "multi.bat");
        File.WriteAllText(batchFile, "\"C:\\missing1\\a.exe\" \"D:\\missing2\\b.exe\"\n");

        var result = ValidateBatchFile.FindInvalidQuotedPathsSimple(batchFile);
        Assert.Equal(2, result.Count);
    }
}
