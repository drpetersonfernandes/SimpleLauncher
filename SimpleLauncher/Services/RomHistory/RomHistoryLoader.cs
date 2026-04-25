using System.Xml;
using System.Xml.Linq;

namespace SimpleLauncher.Services.RomHistory;

/// <summary>
/// Provides methods for loading and querying the local ROM history database (history.xml).
/// </summary>
public static class RomHistoryLoader
{
    /// <summary>
    /// Searches <paramref name="historyFilePath"/> for an entry matching <paramref name="romName"/>.
    /// </summary>
    /// <param name="historyFilePath">Full path to the history.xml file.</param>
    /// <param name="romName">The ROM name to search for.</param>
    /// <returns>The matching <see cref="XElement"/> entry, or <c>null</c> if not found.</returns>
    public static XElement FindEntry(string historyFilePath, string romName)
    {
        var settings = new XmlReaderSettings
        {
            DtdProcessing = DtdProcessing.Prohibit,
            XmlResolver = null
        };

        using var reader = XmlReader.Create(historyFilePath, settings);
        var doc = XDocument.Load(reader, LoadOptions.None);

        return doc.Descendants("entry")
                   .FirstOrDefault(e => e.Element("systems")?.Elements("system")
                       .Any(system => system.Attribute("name")?.Value == romName) == true)
               ?? doc.Descendants("entry")
                   .FirstOrDefault(e => e.Element("software")?.Elements("item")
                       .Any(item => item.Attribute("name")?.Value == romName) == true);
    }
}
