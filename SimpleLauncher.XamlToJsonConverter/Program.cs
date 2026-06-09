using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SimpleLauncher.XamlToJsonConverter;

public static partial class Program
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    [GeneratedRegex("""^\s*<system:String x:Key="([^"]+)">(.*)</system:String>\s*$""")]
    private static partial Regex EntryRegex();

    public static void Main(string[] args)
    {
        Console.WriteLine("=== XAML to JSON Resource Converter ===");
        Console.WriteLine();

        // Find paths
        var baseDir = AppContext.BaseDirectory;
        var solutionDir = FindSolutionDirectory(baseDir);

        if (solutionDir == null)
        {
            Console.Error.WriteLine("ERROR: Could not find solution directory.");
            return;
        }

        var sourceDir = Path.Combine(solutionDir, "SimpleLauncher", "resources");
        var outputDir = Path.Combine(solutionDir, "SimpleLauncher.Core", "Resources");

        if (!Directory.Exists(sourceDir))
        {
            Console.Error.WriteLine($"ERROR: Source directory not found: {sourceDir}");
            return;
        }

        // Create output directory
        Directory.CreateDirectory(outputDir);

        // Find all XAML resource files
        var xamlFiles = Directory.GetFiles(sourceDir, "strings.*.xaml")
            .OrderBy(static f => f)
            .ToArray();

        if (xamlFiles.Length == 0)
        {
            Console.Error.WriteLine("ERROR: No strings.*.xaml files found in source directory.");
            return;
        }

        Console.WriteLine($"Source: {sourceDir}");
        Console.WriteLine($"Output: {outputDir}");
        Console.WriteLine($"Found {xamlFiles.Length} XAML files to convert.");
        Console.WriteLine();

        var totalKeys = 0;
        var totalFiles = 0;

        foreach (var xamlFile in xamlFiles)
        {
            var fileName = Path.GetFileName(xamlFile);
            var languageCode = ExtractLanguageCode(fileName);

            if (languageCode == null)
            {
                Console.WriteLine($"  SKIP: {fileName} (could not extract language code)");
                continue;
            }

            var jsonFileName = $"strings.{languageCode}.json";
            var jsonFilePath = Path.Combine(outputDir, jsonFileName);

            try
            {
                var entries = ParseXamlFile(xamlFile);

                if (entries.Count == 0)
                {
                    Console.WriteLine($"  SKIP: {fileName} (no valid entries found)");
                    continue;
                }

                WriteJsonFile(jsonFilePath, entries);

                Console.WriteLine($"  OK: {jsonFileName} ({entries.Count} keys)");
                totalKeys += entries.Count;
                totalFiles++;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"  ERROR: {fileName} - {ex.Message}");
            }
        }

        Console.WriteLine();
        Console.WriteLine($"Done! Converted {totalFiles} files with {totalKeys} total keys.");
        Console.WriteLine($"Output: {outputDir}");
    }

    private static string? FindSolutionDirectory(string startDir)
    {
        var dir = new DirectoryInfo(startDir);

        while (dir != null)
        {
            if (Directory.GetFiles(dir.FullName, "*.sln").Length > 0)
                return dir.FullName;

            dir = dir.Parent;
        }

        return null;
    }

    private static string? ExtractLanguageCode(string fileName)
    {
        // Pattern: strings.{lang}.xaml
        var match = MyRegex().Match(fileName);
        return match.Success ? match.Groups[1].Value : null;
    }

    private static Dictionary<string, string> ParseXamlFile(string filePath)
    {
        var entries = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var lines = File.ReadAllLines(filePath, Encoding.UTF8);

        foreach (var line in lines)
        {
            var match = EntryRegex().Match(line);

            if (!match.Success)
                continue;

            var key = match.Groups[1].Value;
            var value = UnescapeXml(match.Groups[2].Value);

            // Skip empty values
            if (string.IsNullOrWhiteSpace(value))
                continue;

            // Skip duplicates (keep first occurrence)
            entries.TryAdd(key, value);
        }

        return entries;
    }

    private static void WriteJsonFile(string filePath, Dictionary<string, string> entries)
    {
        // Sort by key and write
        var sorted = new SortedDictionary<string, string>(entries, StringComparer.OrdinalIgnoreCase);
        var json = JsonSerializer.Serialize(sorted, JsonOptions);
        File.WriteAllText(filePath, json, new UTF8Encoding(true));
    }

    private static string UnescapeXml(string value)
    {
        return value
            .Replace("&amp;", "&")
            .Replace("&lt;", "<")
            .Replace("&gt;", ">")
            .Replace("&quot;", "\"")
            .Replace("&apos;", "'");
    }

    [GeneratedRegex(@"^strings\.(.+)\.xaml$")]
    private static partial Regex MyRegex();
}
