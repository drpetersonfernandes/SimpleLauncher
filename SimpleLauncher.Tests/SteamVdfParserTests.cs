using SimpleLauncher.Tests.TestHelpers;
using Xunit;
using SteamVdfParser = SimpleLauncher.Services.GameScan.SteamVdfParser;

namespace SimpleLauncher.Tests;

public class SteamVdfParserTests : IDisposable
{
    private readonly string _testDirectory;

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

        var result = SteamVdfParser.Parse(filePath);

        Assert.NotNull(result);
        Assert.True(result.ContainsKey("AppState"));

        var appState = (Dictionary<string, object>)result["AppState"];
        Assert.Equal("228980", appState["appid"]);
        Assert.Equal("Steamworks Common Redistributables", appState["name"]);
        Assert.Equal("Steamworks Shared", appState["installdir"]);
    }

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

        var result = SteamVdfParser.Parse(filePath);

        var root = (Dictionary<string, object>)result["root"];
        var level1 = (Dictionary<string, object>)root["level1"];
        Assert.Equal("value", level1["key"]);
    }

    [Fact]
    public void ParseEscapedQuotesInValuesUnescapesCorrectly()
    {
        var filePath = CreateVdfFile("""
            "key"		"some \"quoted\" value"
            """);

        var result = SteamVdfParser.Parse(filePath);

        Assert.Equal("some \"quoted\" value", result["key"].ToString());
    }

    [Fact]
    public void ParseEscapedBackslashesUnescapesCorrectly()
    {
        // Use a path without \t to avoid the tab replacement in UnescapeVdfValue
        var filePath = CreateVdfFile("""
            "key"		"path\\of\\file"
            """);

        var result = SteamVdfParser.Parse(filePath);

        Assert.Equal("path\\of\\file", result["key"].ToString());
    }

    [Fact]
    public void ParseWindowsBackslashPathsPreservesBackslashes()
    {
        var filePath = CreateVdfFile("""
            "installdir"		"C:\\games\\Steam"
            """);

        var result = SteamVdfParser.Parse(filePath);

        Assert.Equal("C:\\games\\Steam", result["installdir"].ToString());
    }

    [Fact]
    public void ParseEmptyFileReturnsEmptyDictionary()
    {
        var filePath = Path.Combine(_testDirectory, "empty.vdf");
        File.WriteAllText(filePath, "");

        var result = SteamVdfParser.Parse(filePath);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseNonExistentFileReturnsEmptyDictionary()
    {
        var filePath = Path.Combine(_testDirectory, "nonexistent.vdf");

        var result = SteamVdfParser.Parse(filePath);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public void ParseCommentsAreIgnored()
    {
        var filePath = CreateVdfFile("""
            // This is a comment
            "key1"		"value1"
            // Another comment
            "key2"		"value2"
            """);

        var result = SteamVdfParser.Parse(filePath);

        Assert.Equal("value1", result["key1"].ToString());
        Assert.Equal("value2", result["key2"].ToString());
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void ParseIsCaseInsensitiveForKeys()
    {
        var filePath = CreateVdfFile("""
            "AppState"
            {
                "AppID"		"12345"
            }
            """);

        var result = SteamVdfParser.Parse(filePath);

        var appState = (Dictionary<string, object>)result["AppState"];
        Assert.True(appState.ContainsKey("AppID"));
        Assert.True(appState.ContainsKey("appid"));
        Assert.True(appState.ContainsKey("APPID"));
    }

    [Fact]
    public void ParseNewlineEscapesInValuesUnescapesCorrectly()
    {
        var filePath = CreateVdfFile("""
            "key"		"line1\\nline2"
            """);

        var result = SteamVdfParser.Parse(filePath);

        Assert.Equal("line1\nline2", result["key"].ToString());
    }

    [Fact]
    public void ParseTabEscapesInValuesUnescapesCorrectly()
    {
        var filePath = CreateVdfFile("""
            "key"		"col1\\tcol2"
            """);

        var result = SteamVdfParser.Parse(filePath);

        Assert.Equal("col1\tcol2", result["key"].ToString());
    }

    private string CreateVdfFile(string content)
    {
        var filePath = Path.Combine(_testDirectory, $"{Guid.NewGuid():N}.vdf");
        File.WriteAllText(filePath, content);
        return filePath;
    }
}
