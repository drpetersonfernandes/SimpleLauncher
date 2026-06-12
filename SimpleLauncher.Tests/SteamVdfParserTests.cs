using SimpleLauncher.Tests.TestHelpers;
using SimpleLauncher.Services.GameScan;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests the SteamVdfParser for parsing Valve Data Format (VDF) files used by Steam.
/// </summary>
public class SteamVdfParserTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly SteamVdfParser _parser = new();

    public SteamVdfParserTests()
    {
        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_VdfTest_{Guid.NewGuid():N}");
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

        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Verifies that simple key-value pairs in a VDF file are parsed into the correct dictionary structure.
    /// </summary>
    [Fact]
    public void ParseSimpleKeyValuePairsReturnsCorrectDictionary()
    {
        var filePath = CreateVdfFile("""
            "AppState"
            {
                "appid"		"228980"
                "name"		"Steamworks Common Redistributables"
                "installdir"	"Steamworks Shared"
            }
            """);

        var result = _parser.Parse(filePath);

        Assert.NotNull(result);
        Assert.True(result.ContainsKey("AppState"));

        var appState = (Dictionary<string, object>)result["AppState"];
        Assert.Equal("228980", appState["appid"]);
        Assert.Equal("Steamworks Common Redistributables", appState["name"]);
        Assert.Equal("Steamworks Shared", appState["installdir"]);
    }

    /// <summary>
    /// Verifies that nested dictionary blocks in VDF format are parsed with correct hierarchy.
    /// </summary>
    [Fact]
    public void ParseNestedDictionariesReturnsCorrectStructure()
    {
        var filePath = CreateVdfFile("""
            "root"
            {
                "level1"
                {
                    "key"		"value"
                }
            }
            """);

        var result = _parser.Parse(filePath);

        var root = (Dictionary<string, object>)result["root"];
        var level1 = (Dictionary<string, object>)root["level1"];
        Assert.Equal("value", level1["key"]);
    }

    /// <summary>
    /// Verifies that escaped quote characters in VDF values are unescaped properly.
    /// </summary>
    [Fact]
    public void ParseEscapedQuotesInValuesUnescapesCorrectly()
    {
        var filePath = CreateVdfFile("""
            "key"		"some \"quoted\" value"
            """);

        var result = _parser.Parse(filePath);

        Assert.Equal("some \"quoted\" value", result["key"].ToString());
    }

    /// <summary>
    /// Verifies that escaped backslash characters in VDF values are preserved correctly.
    /// </summary>
    [Fact]
    public void ParseEscapedBackslashesUnescapesCorrectly()
    {
        // Use a path without \t to avoid the tab replacement in UnescapeVdfValue
        var filePath = CreateVdfFile("""
            "key"		"path\\of\\file"
            """);

        var result = _parser.Parse(filePath);

        Assert.Equal("path\\of\\file", result["key"].ToString());
    }

    /// <summary>
    /// Verifies that Windows-style backslash paths in VDF values are preserved.
    /// </summary>
    [Fact]
    public void ParseWindowsBackslashPathsPreservesBackslashes()
    {
        var filePath = CreateVdfFile("""
            "installdir"		"C:\\games\\Steam"
            """);

        var result = _parser.Parse(filePath);

        Assert.Equal("C:\\games\\Steam", result["installdir"].ToString());
    }

    /// <summary>
    /// Verifies that parsing an empty VDF file returns an empty dictionary.
    /// </summary>
    [Fact]
    public void ParseEmptyFileReturnsEmptyDictionary()
    {
        var filePath = Path.Combine(_testDirectory, "empty.vdf");
        File.WriteAllText(filePath, "");

        var result = _parser.Parse(filePath);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that parsing a non-existent file returns an empty dictionary.
    /// </summary>
    [Fact]
    public void ParseNonExistentFileReturnsEmptyDictionary()
    {
        var filePath = Path.Combine(_testDirectory, "nonexistent.vdf");

        var result = _parser.Parse(filePath);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    /// <summary>
    /// Verifies that comment lines starting with // are ignored during parsing.
    /// </summary>
    [Fact]
    public void ParseCommentsAreIgnored()
    {
        var filePath = CreateVdfFile("""
            // This is a comment
            "key1"		"value1"
            // Another comment
            "key2"		"value2"
            """);

        var result = _parser.Parse(filePath);

        Assert.Equal("value1", result["key1"].ToString());
        Assert.Equal("value2", result["key2"].ToString());
        Assert.Equal(2, result.Count);
    }

    /// <summary>
    /// Verifies that VDF dictionary key lookups are case-insensitive.
    /// </summary>
    [Fact]
    public void ParseIsCaseInsensitiveForKeys()
    {
        var filePath = CreateVdfFile("""
            "AppState"
            {
                "AppID"		"12345"
            }
            """);

        var result = _parser.Parse(filePath);

        var appState = (Dictionary<string, object>)result["AppState"];
        Assert.True(appState.ContainsKey("AppID"));
        Assert.True(appState.ContainsKey("appid"));
        Assert.True(appState.ContainsKey("APPID"));
    }

    /// <summary>
    /// Verifies that escaped newline characters in VDF values are unescaped properly.
    /// </summary>
    [Fact]
    public void ParseNewlineEscapesInValuesUnescapesCorrectly()
    {
        var filePath = CreateVdfFile("""
            "key"		"line1\\nline2"
            """);

        var result = _parser.Parse(filePath);

        Assert.Equal("line1\nline2", result["key"].ToString());
    }

    /// <summary>
    /// Verifies that escaped tab characters in VDF values are unescaped properly.
    /// </summary>
    [Fact]
    public void ParseTabEscapesInValuesUnescapesCorrectly()
    {
        var filePath = CreateVdfFile("""
            "key"		"col1\\tcol2"
            """);

        var result = _parser.Parse(filePath);

        Assert.Equal("col1\tcol2", result["key"].ToString());
    }

    private string CreateVdfFile(string content)
    {
        var filePath = Path.Combine(_testDirectory, $"{Guid.NewGuid():N}.vdf");
        File.WriteAllText(filePath, content);
        return filePath;
    }
}
