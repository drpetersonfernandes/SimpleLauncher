using System.Text;
using SimpleLauncher.ResourceTranslator.Models;
using SimpleLauncher.ResourceTranslator.Services;

namespace SimpleLauncher.ResourceTranslator;

public class Program
{
    private const int BatchSize = 40;

    public static async Task Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        var resourcesPath = FindResourcesPath();
        if (resourcesPath == null)
        {
            Console.WriteLine("ERROR: Could not locate SimpleLauncher/resources directory.");
            Environment.Exit(1);
        }

        var englishFile = Path.Combine(resourcesPath, "strings.en.xaml");
        if (!File.Exists(englishFile))
        {
            Console.WriteLine($"ERROR: English resource file not found: {englishFile}");
            Environment.Exit(1);
        }

        Console.WriteLine("Simple Launcher Resource Translator");
        Console.WriteLine("===================================");
        Console.WriteLine();

        var englishKeys = ResourceAnalyzer.ReadEnglishKeys(englishFile);
        Console.WriteLine($"English base file loaded: {englishKeys.Count} keys");

        var batches = ResourceAnalyzer.AnalyzeAllLanguages(resourcesPath, englishKeys);

        if (batches.Count == 0)
        {
            Console.WriteLine();
            Console.WriteLine("All language files are fully synchronized with English. No action needed.");
            return;
        }

        var totalMissing = batches.Sum(static b => b.MissingKeys.Count);
        var totalDuplicates = batches.Sum(static b => b.DuplicateKeysRemoved.Count);

        Console.WriteLine();
        Console.WriteLine("Analysis Results:");
        Console.WriteLine($"  Languages needing updates: {batches.Count}");
        Console.WriteLine($"  Total missing keys: {totalMissing}");
        if (totalDuplicates > 0)
            Console.WriteLine($"  Total duplicate keys to remove: {totalDuplicates}");
        Console.WriteLine();

        foreach (var batch in batches)
        {
            Console.WriteLine($"  [{batch.LanguageCode}] {batch.LanguageName}: {batch.MissingKeys.Count} missing, {batch.DuplicateKeysRemoved.Count} duplicates");
        }

        Console.WriteLine();
        Console.WriteLine("Press any key to proceed with translation, or Ctrl+C to cancel...");
        Console.ReadKey(true);
        Console.WriteLine();

        // Prompt for API key (not stored)
        Console.Write("Enter your Google Gemini API key: ");
        var apiKey = Console.ReadLine()?.Trim();
        if (string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("ERROR: API key is required.");
            Environment.Exit(1);
        }

        Console.WriteLine();

        // Model selection
        var models = GeminiTranslationService.GetAvailableModels();
        Console.WriteLine("Available Gemini models:");
        for (var i = 0; i < models.Count; i++)
        {
            var marker = models[i].Id == "gemini-2.5-flash" ? " (default)" : "";
            Console.WriteLine($"  {i + 1}. {models[i].Name} - {models[i].Description}{marker}");
        }

        Console.WriteLine();
        Console.Write("Select model number (press Enter for default): ");
        var modelInput = Console.ReadLine()?.Trim();

        GeminiModelInfo selectedModel;
        if (string.IsNullOrEmpty(modelInput) || !int.TryParse(modelInput, out var modelIndex) || modelIndex < 1 || modelIndex > models.Count)
        {
            selectedModel = models.First(static m => m.Id == "gemini-2.5-flash");
            Console.WriteLine($"Using default model: {selectedModel.Name}");
        }
        else
        {
            selectedModel = models[modelIndex - 1];
            Console.WriteLine($"Selected model: {selectedModel.Name}");
        }

        Console.WriteLine();

        var translator = new GeminiTranslationService(apiKey, selectedModel.Id, selectedModel.ApiVersion);
        var overallStopwatch = System.Diagnostics.Stopwatch.StartNew();

        foreach (var batch in batches)
        {
            Console.WriteLine($"Processing [{batch.LanguageCode}] {batch.LanguageName}...");
            var languageStopwatch = System.Diagnostics.Stopwatch.StartNew();

            var allTranslations = new Dictionary<string, string>(StringComparer.Ordinal);
            var missingList = batch.MissingKeys;
            var totalBatches = (int)Math.Ceiling(missingList.Count / (double)BatchSize);

            for (var i = 0; i < missingList.Count; i += BatchSize)
            {
                var currentBatch = missingList.Skip(i).Take(BatchSize).ToList();
                var batchNumber = (i / BatchSize) + 1;

                Console.Write($"  Batch {batchNumber}/{totalBatches} ({currentBatch.Count} keys)... ");
                var sw = System.Diagnostics.Stopwatch.StartNew();

                try
                {
                    var translations = await translator.TranslateBatchAsync(batch.LanguageName, currentBatch);
                    foreach (var kvp in translations)
                    {
                        allTranslations[kvp.Key] = kvp.Value;
                    }

                    Console.WriteLine($"done in {sw.ElapsedMilliseconds}ms");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"FAILED: {ex.Message}");
                    Console.WriteLine("  This batch was skipped and will not be written to the resource file.");
                }

                // Small delay to avoid rate limits
                if (i + BatchSize < missingList.Count)
                {
                    await Task.Delay(500);
                }
            }

            // Write back to XAML
            XamlResourceWriter.UpdateResourceFile(batch.FilePath, allTranslations, batch.DuplicateKeysRemoved);

            languageStopwatch.Stop();
            Console.WriteLine($"  Written {allTranslations.Count} entries to {Path.GetFileName(batch.FilePath)} in {languageStopwatch.ElapsedMilliseconds}ms");
            Console.WriteLine();
        }

        overallStopwatch.Stop();
        Console.WriteLine("===================================");
        Console.WriteLine("Translation complete!");
        Console.WriteLine($"Total time: {overallStopwatch.Elapsed.Minutes:D2}:{overallStopwatch.Elapsed.Seconds:D2}");
        Console.WriteLine($"Languages updated: {batches.Count}");
        Console.WriteLine($"Total keys translated: {totalMissing}");
        if (totalDuplicates > 0)
            Console.WriteLine($"Total duplicates removed: {totalDuplicates}");
    }

    private static string? FindResourcesPath()
    {
        // If running from the project directory (development)
        var devPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "SimpleLauncher", "resources");
        if (Directory.Exists(devPath))
        {
            var fullPath = Path.GetFullPath(devPath);
            if (File.Exists(Path.Combine(fullPath, "strings.en.xaml")))
                return fullPath;
        }

        // If running from output near SimpleLauncher project
        var nearProject = Path.Combine(AppContext.BaseDirectory, "..", "..", "SimpleLauncher", "resources");
        if (Directory.Exists(nearProject))
        {
            var fullPath = Path.GetFullPath(nearProject);
            if (File.Exists(Path.Combine(fullPath, "strings.en.xaml")))
                return fullPath;
        }

        // If running from the same folder as SimpleLauncher
        var siblingPath = Path.Combine(AppContext.BaseDirectory, "..", "SimpleLauncher", "resources");
        if (Directory.Exists(siblingPath))
        {
            var fullPath = Path.GetFullPath(siblingPath);
            if (File.Exists(Path.Combine(fullPath, "strings.en.xaml")))
                return fullPath;
        }

        // Search upward for SimpleLauncher folder
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, "SimpleLauncher", "resources");
            if (Directory.Exists(candidate) && File.Exists(Path.Combine(candidate, "strings.en.xaml")))
                return candidate;

            dir = dir.Parent;
        }

        return null;
    }
}
