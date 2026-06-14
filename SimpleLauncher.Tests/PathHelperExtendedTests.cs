using System.Diagnostics.CodeAnalysis;
using Xunit;
using PathHelper = SimpleLauncher.Services.CheckPaths.PathHelper;

namespace SimpleLauncher.Tests;

/// <summary>
/// Extended tests for <see cref="PathHelper"/> covering additional edge cases for
/// path resolution, placeholder handling, parameter string resolution, and file lookup.
/// </summary>
[SuppressMessage("ReSharper", "NullableWarningSuppressionIsUsed")]
public class PathHelperExtendedTests
{
    [Fact]
    public void ResolveRelativeToAppDirectoryNullReturnsNull()
    {
        var result = PathHelper.ResolveRelativeToAppDirectory(null);
        Assert.Null(result);
    }

    [Fact]
    public void ResolveRelativeToAppDirectoryEmptyReturnsNull()
    {
        var result = PathHelper.ResolveRelativeToAppDirectory("");
        Assert.Null(result);
    }

    [Fact]
    public void ResolveRelativeToAppDirectoryWhitespaceReturnsNull()
    {
        var result = PathHelper.ResolveRelativeToAppDirectory("   ");
        Assert.Null(result);
    }

    [Fact]
    public void ResolveRelativeToAppDirectoryVeryLongPathReturnsNull()
    {
        var longPath = new string('a', 5000);
        var result = PathHelper.ResolveRelativeToAppDirectory(longPath);
        Assert.Null(result);
    }

    [Fact]
    public void ResolveRelativeToAppDirectoryBaseFolderOnlyReturnsAppDirectory()
    {
        var result = PathHelper.ResolveRelativeToAppDirectory("%BASEFOLDER%");
        Assert.NotNull(result);
        Assert.True(Path.IsPathRooted(result));
    }

    [Fact]
    public void ResolveRelativeToAppDirectoryBaseFolderWithTrailingSeparator()
    {
        var result = PathHelper.ResolveRelativeToAppDirectory("%BASEFOLDER%\\");
        Assert.NotNull(result);
        Assert.True(Path.IsPathRooted(result));
    }

    [Fact]
    public void ResolveRelativeToAppDirectoryBaseFolderCaseInsensitive()
    {
        var result = PathHelper.ResolveRelativeToAppDirectory("%basefolder%\\test");
        Assert.NotNull(result);
        Assert.EndsWith("test", result);
    }

    [Fact]
    public void IsRelativePathWithoutBaseFolderNullReturnsFalse()
    {
        var result = PathHelper.IsRelativePathWithoutBaseFolder(null!);
        Assert.False(result);
    }

    [Fact]
    public void IsRelativePathWithoutBaseFolderWhitespaceReturnsFalse()
    {
        var result = PathHelper.IsRelativePathWithoutBaseFolder("   ");
        Assert.False(result);
    }

    [Theory]
    [InlineData("%GAME%", true)]
    [InlineData("%ROMNAME%", true)]
    [InlineData("%ROMFILE%", true)]
    [InlineData("$game$", true)]
    [InlineData("$romname$", true)]
    [InlineData("$romfile$", true)]
    [InlineData("{game}", true)]
    [InlineData("{romname}", true)]
    [InlineData("{romfile}", true)]
    [InlineData("%GAME%.zip", true)]
    [InlineData("path/%ROMNAME%/file", true)]
    [InlineData("normalpath", false)]
    [InlineData("%BASEFOLDER%", false)]
    public void ContainsGameSpecificPlaceholderAllVariants(string text, bool expected)
    {
        var result = PathHelper.ContainsGameSpecificPlaceholder(text);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetLongPathWhitespaceReturnsWhitespace()
    {
        var result = PathHelper.GetLongPath("   ");
        Assert.Equal("   ", result);
    }

    [Fact]
    public void GetLongPathAlreadyExtendedWithDotSlashReturnsUnchanged()
    {
        var result = PathHelper.GetLongPath(@"\\.\C:\path");
        Assert.Equal(@"\\.\C:\path", result);
    }

    [Fact]
    public void SanitizePathTokenNullReturnsEmpty()
    {
        var result = PathHelper.SanitizePathToken(null);
        Assert.Equal("", result);
    }

    [Fact]
    public void SanitizePathTokenOnlySeparatorsReturnsEmpty()
    {
        var result = PathHelper.SanitizePathToken("\\\\");
        Assert.Equal("", result);
    }

    [Fact]
    public void GetFileNameWithoutExtensionNullReturnsNull()
    {
        var result = PathHelper.GetFileNameWithoutExtension(null!);
        Assert.Null(result);
    }

    [Fact]
    public void GetFileNameNullReturnsNull()
    {
        var result = PathHelper.GetFileName(null!);
        Assert.Null(result);
    }

    [Fact]
    public void GetFileNameWithoutExtensionPathWithSpaces()
    {
        var result = PathHelper.GetFileNameWithoutExtension(@"C:\my games\super mario.zip");
        Assert.Equal("super mario", result);
    }

    [Fact]
    public void GetFileNamePathWithSpaces()
    {
        var result = PathHelper.GetFileName(@"C:\my games\super mario.zip");
        Assert.Equal("super mario.zip", result);
    }

    [Fact]
    public void ResolveParameterStringEmptyReturnsEmpty()
    {
        var result = PathHelper.ResolveParameterString("");
        Assert.Equal("", result);
    }

    [Fact]
    public void ResolveParameterStringWhitespaceOnlyReturnsEmpty()
    {
        var result = PathHelper.ResolveParameterString("   ");
        Assert.Equal("", result);
    }

    [Fact]
    public void ResolveParameterStringNoPlaceholdersReturnsUnchanged()
    {
        const string parameters = "-f --fullscreen -window";
        var result = PathHelper.ResolveParameterString(parameters);
        Assert.Equal(parameters, result);
    }

    [Fact]
    public void ResolveParameterStringEmptySystemFoldersListResolvesEmpty()
    {
        const string parameters = "-rompath %SYSTEMFOLDER%";
        var result = PathHelper.ResolveParameterString(parameters, []);
        Assert.Equal("-rompath ", result);
    }

    [Fact]
    public void ResolveParameterStringNullSystemFoldersResolvesEmpty()
    {
        const string parameters = "-rompath %SYSTEMFOLDER%";
        var result = PathHelper.ResolveParameterString(parameters, null);
        Assert.Equal("-rompath ", result);
    }

    [Fact]
    public void ResolveParameterStringRomPlaceholderWithSpacesAddsQuotes()
    {
        const string parameters = "-rom %ROM%";
        var result = PathHelper.ResolveParameterString(parameters, null, null, @"C:\my roms\game file.zip");
        Assert.Contains("C:\\my roms\\game file.zip", result);
    }

    [Fact]
    public void ResolveParameterStringNamePlaceholderPreservesQuotes()
    {
        const string parameters = "\"%NAME%\"";
        var result = PathHelper.ResolveParameterString(parameters, null, null, null, null, "test game");
        Assert.Equal("\"test game\"", result);
    }

    [Fact]
    public void FindFileInSystemFoldersNullListReturnsNull()
    {
        var result = PathHelper.FindFileInSystemFolders(null!, "game.zip");
        Assert.Null(result);
    }

    [Fact]
    public void FindFileInSystemFoldersEmptyListReturnsNull()
    {
        var result = PathHelper.FindFileInSystemFolders([], "game.zip");
        Assert.Null(result);
    }

    [Fact]
    public void FindFileInSystemFoldersNullFileNameReturnsNull()
    {
        var result = PathHelper.FindFileInSystemFolders(["C:\\roms"], null!);
        Assert.Null(result);
    }

    [Fact]
    public void FindFileInSystemFoldersEmptyFileNameReturnsNull()
    {
        var result = PathHelper.FindFileInSystemFolders(["C:\\roms"], "");
        Assert.Null(result);
    }

    [Fact]
    public void FindContainingSystemFolderEmptyFilePathReturnsPrimaryFolder()
    {
        var result = PathHelper.FindContainingSystemFolder(["C:\\roms"], "C:\\primary", "");
        Assert.Equal("C:\\primary", result);
    }

    [Fact]
    public void TryGetExistingDirectoryNullReturnsNull()
    {
        var result = PathHelper.TryGetExistingDirectory(null);
        Assert.Null(result);
    }

    [Fact]
    public void TryGetExistingDirectoryEmptyReturnsNull()
    {
        var result = PathHelper.TryGetExistingDirectory("");
        Assert.Null(result);
    }

    [Fact]
    public void TryGetExistingDirectoryWhitespaceReturnsNull()
    {
        var result = PathHelper.TryGetExistingDirectory("   ");
        Assert.Null(result);
    }

    [Fact]
    public void TryGetExistingDirectoryNonExistentReturnsNull()
    {
        var result = PathHelper.TryGetExistingDirectory("C:\\nonexistent_path_12345");
        Assert.Null(result);
    }

    [Fact]
    public void TryGetExistingDirectoryExistingReturnsPath()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        try
        {
            var result = PathHelper.TryGetExistingDirectory(tempDir);
            Assert.NotNull(result);
            Assert.True(Directory.Exists(result));
        }
        finally
        {
            Directory.Delete(tempDir);
        }
    }

    [Fact]
    public void ResolveRelativeToCurrentWorkingDirectoryRelativePathReturnsRooted()
    {
        var result = PathHelper.ResolveRelativeToCurrentWorkingDirectory("some\\relative\\path");
        Assert.True(Path.IsPathRooted(result));
        Assert.EndsWith("some\\relative\\path", result);
    }

    [Fact]
    public void ResolveRelativeToCurrentWorkingDirectoryAbsolutePathReturnsSame()
    {
        const string absPath = @"C:\Windows\System32";
        var result = PathHelper.ResolveRelativeToCurrentWorkingDirectory(absPath);
        Assert.Equal(absPath, result);
    }
}
