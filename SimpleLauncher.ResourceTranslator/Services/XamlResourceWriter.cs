using System.Text;
using System.Text.RegularExpressions;

namespace SimpleLauncher.ResourceTranslator.Services;

public static partial class XamlResourceWriter
{
    private static readonly Regex EntryRegex = MyRegex();

    public static void UpdateResourceFile(
        string filePath,
        Dictionary<string, string> newTranslations,
        List<string> duplicatesToRemove)
    {
        var lines = File.ReadAllLines(filePath).ToList();

        // Extract header (before first entry)
        var firstEntryIndex = -1;
        for (var i = 0; i < lines.Count; i++)
        {
            if (EntryRegex.IsMatch(lines[i]))
            {
                firstEntryIndex = i;
                break;
            }
        }

        if (firstEntryIndex == -1)
        {
            // No existing entries - find the closing tag
            for (var i = 0; i < lines.Count; i++)
            {
                if (lines[i].Trim() == "</ResourceDictionary>")
                {
                    firstEntryIndex = i;
                    break;
                }
            }
        }

        if (firstEntryIndex == -1)
            throw new InvalidOperationException($"Could not parse XAML structure in {filePath}");

        var header = lines.Take(firstEntryIndex).ToList();

        // Parse existing entries, skipping duplicates
        var existingEntries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var duplicatesRemoved = new HashSet<string>(duplicatesToRemove, StringComparer.OrdinalIgnoreCase);

        foreach (var line in lines)
        {
            var match = EntryRegex.Match(line);
            if (match.Success)
            {
                var key = match.Groups[1].Value;
                if (!duplicatesRemoved.Contains(key) && !existingEntries.ContainsKey(key))
                {
                    existingEntries[key] = match.Groups[2].Value;
                }
            }
        }

        // Merge new translations
        foreach (var kvp in newTranslations)
        {
            existingEntries[kvp.Key] = kvp.Value;
        }

        // Sort by key and build lines
        var sortedEntries = existingEntries
            .OrderBy(static e => e.Key, StringComparer.OrdinalIgnoreCase)
            .Select(static e =>
            {
                var escapedValue = EscapeXml(e.Value);
                return $"    <system:String x:Key=\"{e.Key}\">{escapedValue}</system:String>";
            })
            .ToList();

        var footer = new List<string> { "</ResourceDictionary>" };

        var encoding = new UTF8Encoding(true);
        File.WriteAllLines(filePath, header.Concat(sortedEntries).Concat(footer), encoding);
    }

    private static string EscapeXml(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return text
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;");
    }

    [GeneratedRegex("""^\s*<system:String x:Key="([^"]+)">(.*)</system:String>\s*$""", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
