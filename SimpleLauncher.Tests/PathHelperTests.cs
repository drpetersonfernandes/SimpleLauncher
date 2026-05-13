using SimpleLauncher.Services.CheckPaths;
using Xunit;

namespace SimpleLauncher.Tests;

public class PathHelperTests
{
    [Theory]
    [InlineData("%BASEFOLDER%\\tools", false)]
    [InlineData("tools", true)]
    [InlineData(".", true)]
    [InlineData("..", true)]
    [InlineData("C:\\Windows", false)]
    [InlineData("%BASEFOLDER%", false)]
    [InlineData("", false)]
    public void IsRelativePathWithoutBaseFolderReturnsExpected(string path, bool expected)
    {
        var result = PathHelper.IsRelativePathWithoutBaseFolder(path);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("C:\\MyFolder\\", "C:\\MyFolder")]
    [InlineData("C:\\MyFolder//", "C:\\MyFolder")]
    [InlineData("C:\\MyFolder", "C:\\MyFolder")]
    [InlineData("", "")]
    public void SanitizePathTokenRemovesTrailingSeparators(string input, string expected)
    {
        var result = PathHelper.SanitizePathToken(input);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void SanitizePathTokenNullReturnsEmpty()
    {
        var result = PathHelper.SanitizePathToken(null);
        Assert.Equal("", result);
    }

    [Theory]
    [InlineData("C:\\test.txt", "test")]
    [InlineData("C:\\folder\\file.zip", "file")]
    [InlineData("file", "file")]
    public void GetFileNameWithoutExtensionReturnsExpected(string path, string expected)
    {
        var result = PathHelper.GetFileNameWithoutExtension(path);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("C:\\test.txt", "test.txt")]
    [InlineData("C:\\folder\\file.zip", "file.zip")]
    [InlineData("file", "file")]
    public void GetFileNameReturnsExpected(string path, string expected)
    {
        var result = PathHelper.GetFileName(path);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("%GAME%", true)]
    [InlineData("%ROMNAME%", true)]
    [InlineData("C:\\test.txt", false)]
    [InlineData("-f", false)]
    [InlineData("", false)]
    public void ContainsGameSpecificPlaceholderReturnsExpected(string text, bool expected)
    {
        var result = PathHelper.ContainsGameSpecificPlaceholder(text);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void ContainsGameSpecificPlaceholderNullReturnsFalse()
    {
        var result = PathHelper.ContainsGameSpecificPlaceholder(null);
        Assert.False(result);
    }

    [Theory]
    [InlineData("C:\\very\\long\\path", "\\\\?\\C:\\very\\long\\path")]
    [InlineData("\\\\server\\share", "\\\\?\\UNC\\server\\share")]
    [InlineData("\\\\?\\C:\\already\\extended", "\\\\?\\C:\\already\\extended")]
    [InlineData("", "")]
    public void GetLongPathReturnsExpected(string path, string expected)
    {
        var result = PathHelper.GetLongPath(path);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void GetLongPathNullReturnsNull()
    {
        var result = PathHelper.GetLongPath(null);
        Assert.Null(result);
    }

    [Fact]
    public void ResolveRelativeToCurrentWorkingDirectoryEmptyStringReturnsEmpty()
    {
        var result = PathHelper.ResolveRelativeToCurrentWorkingDirectory("");
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ResolveRelativeToCurrentWorkingDirectoryNullReturnsEmpty()
    {
        var result = PathHelper.ResolveRelativeToCurrentWorkingDirectory(null);
        Assert.Equal(string.Empty, result);
    }

    [Fact]
    public void ResolveRelativeToCurrentWorkingDirectoryRelativePathReturnsAbsolute()
    {
        var result = PathHelper.ResolveRelativeToCurrentWorkingDirectory(".");
        Assert.True(Path.IsPathRooted(result));
    }

    [Fact]
    public void FindContainingSystemFolderPrimaryFolderMatchReturnsPrimaryFolder()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var primaryFolder = Path.Combine(baseDir, "roms", "Arcade");
        var gameFile = Path.Combine(primaryFolder, "game.zip");

        try
        {
            Directory.CreateDirectory(primaryFolder);
            File.WriteAllText(gameFile, "dummy");

            var systemManager = new Services.SystemManager.SystemManager
            {
                SystemFolders = [primaryFolder]
            };

            var result = PathHelper.FindContainingSystemFolder(systemManager, gameFile);
            Assert.Equal(primaryFolder, result);
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, true);
        }
    }

    [Fact]
    public void FindContainingSystemFolderAdditionalFolderMatchReturnsAdditionalFolder()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var primaryFolder = Path.Combine(baseDir, "roms", "Arcade");
        var additionalFolder = Path.Combine(baseDir, "roms", "cps1");
        var gameFile = Path.Combine(additionalFolder, "sf2.zip");

        try
        {
            Directory.CreateDirectory(primaryFolder);
            Directory.CreateDirectory(additionalFolder);
            File.WriteAllText(gameFile, "dummy");

            var systemManager = new Services.SystemManager.SystemManager
            {
                SystemFolders = [primaryFolder, additionalFolder]
            };

            var result = PathHelper.FindContainingSystemFolder(systemManager, gameFile);
            Assert.Equal(additionalFolder, result);
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, true);
        }
    }

    [Fact]
    public void FindContainingSystemFolderNoMatchReturnsPrimaryFolder()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var primaryFolder = Path.Combine(baseDir, "roms", "Arcade");
        var otherFolder = Path.Combine(baseDir, "roms", "cps1");
        var gameFile = Path.Combine(baseDir, "unrelated", "game.zip");

        try
        {
            Directory.CreateDirectory(primaryFolder);
            Directory.CreateDirectory(otherFolder);
            Directory.CreateDirectory(Path.Combine(baseDir, "unrelated"));
            File.WriteAllText(gameFile, "dummy");

            var systemManager = new Services.SystemManager.SystemManager
            {
                SystemFolders = [primaryFolder, otherFolder]
            };

            var result = PathHelper.FindContainingSystemFolder(systemManager, gameFile);
            Assert.Equal(primaryFolder, result);
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, true);
        }
    }

    [Fact]
    public void FindContainingSystemFolderNestedSubfolderMatchReturnsParentFolder()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var primaryFolder = Path.Combine(baseDir, "roms", "Arcade");
        var nestedFolder = Path.Combine(primaryFolder, "subfolder");
        var gameFile = Path.Combine(nestedFolder, "game.zip");

        try
        {
            Directory.CreateDirectory(nestedFolder);
            File.WriteAllText(gameFile, "dummy");

            var systemManager = new Services.SystemManager.SystemManager
            {
                SystemFolders = [primaryFolder]
            };

            var result = PathHelper.FindContainingSystemFolder(systemManager, gameFile);
            Assert.Equal(primaryFolder, result);
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, true);
        }
    }

    [Fact]
    public void FindContainingSystemFolderNullSystemManagerReturnsNull()
    {
        var result = PathHelper.FindContainingSystemFolder(null, "C:\\game.zip");
        Assert.Null(result);
    }

    [Fact]
    public void FindContainingSystemFolderNullFilePathReturnsPrimaryFolder()
    {
        var systemManager = new Services.SystemManager.SystemManager
        {
            SystemFolders = ["C:\\roms\\Arcade"]
        };

        var result = PathHelper.FindContainingSystemFolder(systemManager, null);
        Assert.Equal("C:\\roms\\Arcade", result);
    }

    [Fact]
    public void ResolveRelativeToAppDirectoryBaseFolderPlaceholderResolvesCorrectly()
    {
        var result = PathHelper.ResolveRelativeToAppDirectory("%BASEFOLDER%\\roms\\test");
        Assert.NotNull(result);
        Assert.True(Path.IsPathRooted(result));
        Assert.EndsWith("roms\\test", result);
    }

    [Fact]
    public void ResolveRelativeToAppDirectoryAbsolutePathReturnsCanonicalPath()
    {
        var result = PathHelper.ResolveRelativeToAppDirectory("C:\\Windows\\System32");
        Assert.Equal("C:\\Windows\\System32", result);
    }

    [Fact]
    public void ResolveRelativeToAppDirectoryRelativePathResolvesToAppDirectory()
    {
        var result = PathHelper.ResolveRelativeToAppDirectory("roms\\test");
        Assert.NotNull(result);
        Assert.True(Path.IsPathRooted(result));
        Assert.EndsWith("roms\\test", result);
    }

    [Fact]
    public void ResolveParameterStringResolvesBaseFolderPlaceholder()
    {
        const string parameters = "-path %BASEFOLDER%\\roms";
        var result = PathHelper.ResolveParameterString(parameters);

        Assert.NotNull(result);
        Assert.DoesNotContain("%BASEFOLDER%", result);
        Assert.Contains("\\roms", result);
    }

    [Fact]
    public void ResolveParameterStringResolvesSystemFolderPlaceholder()
    {
        const string parameters = "-rompath %SYSTEMFOLDER%";
        var result = PathHelper.ResolveParameterString(parameters, ["C:\\roms\\Arcade"]);

        Assert.Equal("-rompath C:\\roms\\Arcade", result);
    }

    [Fact]
    public void ResolveParameterStringResolvesEmulatorFolderPlaceholder()
    {
        const string parameters = "-cfg %EMULATORFOLDER%\\config.ini";
        var result = PathHelper.ResolveParameterString(parameters, null, "C:\\emulators\\mame");

        Assert.Equal("-cfg C:\\emulators\\mame\\config.ini", result);
    }

    [Fact]
    public void ResolveParameterStringResolvesMultiplePlaceholders()
    {
        const string parameters = "-rompath %EMULATORFOLDER%\\roms;%SYSTEMFOLDER% -skip_gameinfo";
        var result = PathHelper.ResolveParameterString(parameters, ["C:\\roms\\Arcade"], "C:\\emulators\\mame");

        Assert.Contains("C:\\emulators\\mame\\roms", result);
        Assert.Contains("C:\\roms\\Arcade", result);
        Assert.DoesNotContain("%EMULATORFOLDER%", result);
        Assert.DoesNotContain("%SYSTEMFOLDER%", result);
    }

    [Fact]
    public void ResolveParameterStringKeepsUnknownPlaceholdersIntact()
    {
        const string parameters = "-path %UNKNOWN%\\test";
        var result = PathHelper.ResolveParameterString(parameters);

        Assert.Equal("-path %UNKNOWN%\\test", result);
    }

    [Fact]
    public void ResolveParameterStringKeepsGameSpecificPlaceholdersIntact()
    {
        const string parameters = "-rom %ROMNAME% -name %ROMFILE%";
        var result = PathHelper.ResolveParameterString(parameters, ["C:\\roms"], "C:\\emu");

        Assert.Equal("-rom %ROMNAME% -name %ROMFILE%", result);
    }

    [Fact]
    public void ResolveParameterStringResolvesPathTraversal()
    {
        const string parameters = "-path %BASEFOLDER%\\..\\..\\Windows";
        var result = PathHelper.ResolveParameterString(parameters);

        // No longer rejects path traversal; placeholder should still be resolved
        Assert.DoesNotContain("%BASEFOLDER%", result);
    }

    [Fact]
    public void ResolveParameterStringHandlesQuotedPaths()
    {
        const string parameters = "-rompath \"%SYSTEMFOLDER%\"";
        var result = PathHelper.ResolveParameterString(parameters, ["C:\\My Roms\\Arcade"]);

        Assert.Equal("-rompath \"C:\\My Roms\\Arcade\"", result);
    }

    [Fact]
    public void ResolveParameterStringExactMatchSystemFolderResolvesCorrectly()
    {
        const string parameters = "-rompath %SYSTEMFOLDER%";
        var result = PathHelper.ResolveParameterString(parameters, ["C:\\roms\\Arcade"]);

        Assert.Equal("-rompath C:\\roms\\Arcade", result);
    }

    [Fact]
    public void ResolveParameterStringKnownFlagsAreNotModified()
    {
        const string parameters = "-f -L core --fullscreen -rompath %SYSTEMFOLDER%";
        var result = PathHelper.ResolveParameterString(parameters, ["C:\\roms"]);

        Assert.Contains("-f", result);
        Assert.Contains("-L", result);
        Assert.Contains("--fullscreen", result);
        Assert.DoesNotContain("%SYSTEMFOLDER%", result);
    }

    [Fact]
    public void FindFileInSystemFoldersFileExistsReturnsPath()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var folder = Path.Combine(baseDir, "roms");
        const string fileName = "game.zip";
        var filePath = Path.Combine(folder, fileName);

        try
        {
            Directory.CreateDirectory(folder);
            File.WriteAllText(filePath, "dummy");

            var systemManager = new Services.SystemManager.SystemManager
            {
                SystemFolders = [folder]
            };

            var result = PathHelper.FindFileInSystemFolders(systemManager, fileName);
            Assert.Equal(filePath, result);
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, true);
        }
    }

    [Fact]
    public void FindFileInSystemFoldersFileNotFoundReturnsNull()
    {
        var systemManager = new Services.SystemManager.SystemManager
        {
            SystemFolders = ["C:\\nonexistent\\roms"]
        };

        var result = PathHelper.FindFileInSystemFolders(systemManager, "missing.zip");
        Assert.Null(result);
    }

    [Fact]
    public void ResolveParameterStringResolvesMultipleSystemFolders()
    {
        const string parameters = "-rompath %SYSTEMFOLDER%";
        var result = PathHelper.ResolveParameterString(parameters, ["C:\\roms\\Arcade", "C:\\roms\\CPS1"]);

        Assert.Equal("-rompath C:\\roms\\Arcade;C:\\roms\\CPS1", result);
    }

    [Fact]
    public void ResolveParameterStringResolvesRomPlaceholder()
    {
        const string parameters = "-rom %ROM%";
        var result = PathHelper.ResolveParameterString(parameters, null, null, "C:\\roms\\Arcade\\game.zip");

        Assert.Equal("-rom C:\\roms\\Arcade\\game.zip", result);
    }

    [Fact]
    public void FindContainingSystemFolderEmptySystemFoldersListReturnsNull()
    {
        var systemManager = new Services.SystemManager.SystemManager
        {
            SystemFolders = []
        };

        var result = PathHelper.FindContainingSystemFolder(systemManager, "C:\\game.zip");
        Assert.Null(result);
    }

    [Fact]
    public void ResolveParameterStringResolvesRomSystemFolderPlaceholder()
    {
        const string parameters = "-path %ROMSYSTEMFOLDER%";
        var result = PathHelper.ResolveParameterString(parameters, null, null, null, "C:\\roms\\Arcade");

        Assert.Equal("-path C:\\roms\\Arcade", result);
    }

    [Fact]
    public void TryFindFileWithNormalizedPathNullPathReturnsNull()
    {
        var result = PathHelper.TryFindFileWithNormalizedPath(null);
        Assert.Null(result);
    }

    [Fact]
    public void TryFindFileWithNormalizedPathEmptyPathReturnsNull()
    {
        var result = PathHelper.TryFindFileWithNormalizedPath("");
        Assert.Null(result);
    }

    [Fact]
    public void TryFindFileWithNormalizedPathWhitespacePathReturnsNull()
    {
        var result = PathHelper.TryFindFileWithNormalizedPath("   ");
        Assert.Null(result);
    }

    [Fact]
    public void TryFindFileWithNormalizedPathNonExistentDirectoryReturnsNull()
    {
        var result = PathHelper.TryFindFileWithNormalizedPath("C:\\NonExistentDir\\file.txt");
        Assert.Null(result);
    }

    [Fact]
    public void TryFindFileWithNormalizedPathFileExistsReturnsOriginalPath()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var filePath = Path.Combine(baseDir, "testfile.txt");

        try
        {
            Directory.CreateDirectory(baseDir);
            File.WriteAllText(filePath, "dummy");

            var result = PathHelper.TryFindFileWithNormalizedPath(filePath);

            Assert.NotNull(result);
            Assert.Equal(filePath, result);
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, true);
        }
    }

    [Fact]
    public void TryFindFileWithNormalizedPathDirectoryExistsReturnsDirectoryPath()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var subDir = Path.Combine(baseDir, "subdir");

        try
        {
            Directory.CreateDirectory(subDir);

            var result = PathHelper.TryFindFileWithNormalizedPath(subDir);

            Assert.NotNull(result);
            Assert.Equal(subDir, result);
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, true);
        }
    }

    [Fact]
    public void TryFindFileWithNormalizedPathCaseMismatchReturnsActualPath()
    {
        var baseDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var actualFilePath = Path.Combine(baseDir, "TestFile.txt");
        var searchFilePath = Path.Combine(baseDir, "testfile.txt");

        try
        {
            Directory.CreateDirectory(baseDir);
            File.WriteAllText(actualFilePath, "dummy");

            var result = PathHelper.TryFindFileWithNormalizedPath(searchFilePath);

            Assert.NotNull(result);
            Assert.Equal(actualFilePath, result);
        }
        finally
        {
            if (Directory.Exists(baseDir))
                Directory.Delete(baseDir, true);
        }
    }
}
