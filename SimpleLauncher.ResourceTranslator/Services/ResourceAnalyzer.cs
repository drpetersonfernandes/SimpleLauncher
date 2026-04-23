using System.Text.RegularExpressions;
using SimpleLauncher.ResourceTranslator.Models;

namespace SimpleLauncher.ResourceTranslator.Services;

public static partial class ResourceAnalyzer
{
    private static readonly Regex EntryRegex = MyRegex();

    private static readonly Dictionary<string, string> LanguageNames = new(StringComparer.OrdinalIgnoreCase)
    {
        ["ar"] = "Arabic",
        ["bn"] = "Bengali",
        ["de"] = "German",
        ["es"] = "Spanish",
        ["fr"] = "French",
        ["hi"] = "Hindi",
        ["id"] = "Indonesian",
        ["it"] = "Italian",
        ["ja"] = "Japanese",
        ["ko"] = "Korean",
        ["nl"] = "Dutch",
        ["pt-br"] = "Brazilian Portuguese",
        ["ru"] = "Russian",
        ["tr"] = "Turkish",
        ["ur"] = "Urdu",
        ["vi"] = "Vietnamese",
        ["zh-hans"] = "Simplified Chinese"
    };

    public static Dictionary<string, string> ReadEnglishKeys(string englishFilePath)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var line in File.ReadAllLines(englishFilePath))
        {
            var match = EntryRegex.Match(line);
            if (match.Success)
            {
                result[match.Groups[1].Value] = match.Groups[2].Value;
            }
        }

        return result;
    }

    public static List<MissingKeyBatch> AnalyzeAllLanguages(string resourcesPath, Dictionary<string, string> englishKeys)
    {
        var batches = new List<MissingKeyBatch>();

        var otherFiles = Directory.EnumerateFiles(resourcesPath, "strings.*.xaml")
            .Where(static f => !f.EndsWith("strings.en.xaml", StringComparison.OrdinalIgnoreCase))
            .OrderBy(static f => f, StringComparer.OrdinalIgnoreCase);

        foreach (var file in otherFiles)
        {
            var fileName = Path.GetFileNameWithoutExtension(file);
            var langCode = fileName.Replace("strings.", "", StringComparison.OrdinalIgnoreCase);
            var langName = LanguageNames.GetValueOrDefault(langCode, langCode);

            var existingKeys = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var duplicateKeys = new List<string>();

            foreach (var line in File.ReadAllLines(file))
            {
                var match = EntryRegex.Match(line);
                if (match.Success)
                {
                    var key = match.Groups[1].Value;
                    if (existingKeys.ContainsKey(key))
                    {
                        duplicateKeys.Add(key);
                    }
                    else
                    {
                        existingKeys[key] = match.Groups[2].Value;
                    }
                }
            }

            var missing = englishKeys
                .Where(kvp => !existingKeys.ContainsKey(kvp.Key))
                .Select(static kvp => new KeyValuePair<string, string>(kvp.Key, kvp.Value))
                .ToList();

            if (missing.Count > 0 || duplicateKeys.Count > 0)
            {
                batches.Add(new MissingKeyBatch
                {
                    FilePath = file,
                    LanguageCode = langCode,
                    LanguageName = langName,
                    MissingKeys = missing,
                    DuplicateKeysRemoved = duplicateKeys.Distinct(StringComparer.OrdinalIgnoreCase).ToList()
                });
            }
        }

        return batches;
    }

    [GeneratedRegex("""^\s*<system:String x:Key="([^"]+)">(.*)</system:String>\s*$""", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
