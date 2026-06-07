using System.Xml;
using SimpleLauncher.Core.Services.RomHistory;
using Xunit;

namespace SimpleLauncher.Tests;

public class RomHistoryLoaderTests : IDisposable
{
    private readonly string _tempDir;

    public RomHistoryLoaderTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_tempDir);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
        {
            Directory.Delete(_tempDir, true);
        }

        GC.SuppressFinalize(this);
    }

    private string CreateHistoryXml(string content)
    {
        var path = Path.Combine(_tempDir, "history.xml");
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public void FindEntryValidXmlWithSystemMatchReturnsEntry()
    {
        const string xml = """
                           <?xml version="1.0" encoding="UTF-8"?>
                           <database>
                             <entry>
                               <systems>
                                 <system name="pacman" />
                               </systems>
                               <text>Pac-Man history text.</text>
                             </entry>
                           </database>
                           """;

        var path = CreateHistoryXml(xml);
        var result = RomHistoryLoader.FindEntry(path, "pacman");

        Assert.NotNull(result);
        Assert.Equal("Pac-Man history text.", result.Element("text")?.Value);
    }

    [Fact]
    public void FindEntryValidXmlWithSoftwareMatchReturnsEntry()
    {
        const string xml = """
                           <?xml version="1.0" encoding="UTF-8"?>
                           <database>
                             <entry>
                               <software>
                                 <item name="galaga" />
                               </software>
                               <text>Galaga history text.</text>
                             </entry>
                           </database>
                           """;

        var path = CreateHistoryXml(xml);
        var result = RomHistoryLoader.FindEntry(path, "galaga");

        Assert.NotNull(result);
        Assert.Equal("Galaga history text.", result.Element("text")?.Value);
    }

    [Fact]
    public void FindEntryValidXmlWithoutMatchReturnsNull()
    {
        const string xml = """
                           <?xml version="1.0" encoding="UTF-8"?>
                           <database>
                             <entry>
                               <systems>
                                 <system name="pacman" />
                               </systems>
                               <text>Pac-Man history text.</text>
                             </entry>
                           </database>
                           """;

        var path = CreateHistoryXml(xml);
        var result = RomHistoryLoader.FindEntry(path, "nonexistent");

        Assert.Null(result);
    }

    [Fact]
    public void FindEntryCorruptedXmlThrowsXmlException()
    {
        const string xml = "<database><entry><unclosed>";
        var path = CreateHistoryXml(xml);

        Assert.Throws<XmlException>(() => RomHistoryLoader.FindEntry(path, "anything"));
    }

    [Fact]
    public void FindEntryMissingFileThrowsFileNotFoundException()
    {
        var missingPath = Path.Combine(_tempDir, "does_not_exist.xml");

        Assert.Throws<FileNotFoundException>(() => RomHistoryLoader.FindEntry(missingPath, "anything"));
    }
}
