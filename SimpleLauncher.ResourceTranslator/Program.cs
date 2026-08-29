using System.Diagnostics;
using System.Globalization;
using System.Text;
using Serilog.Events;
using SimpleLauncher.ResourceTranslator.Models;
using SimpleLauncher.ResourceTranslator.Services;
using SimpleLauncher.ResourceTranslator.Services.DebugAndBugReport;

namespace SimpleLauncher.ResourceTranslator;

/// <summary>
///     Entry point for the SimpleLauncher Resource Translator tool.
/// </summary>
public class Program
{
    private const int BatchSize = 40;

    /// <summary>
    ///     Asynchronous entry point for the application.
    /// </summary>
    private static async Task MainAsync()
    {
        Console.OutputEncoding = Encoding.UTF8;

        var appDataLogFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        Directory.CreateDirectory(appDataLogFolder);

        var bugReportSink = new BugReportApiSink(appDataLogFolder);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.Console(outputTemplate: "[{Level:u3}] {Timestamp:HH:mm:ss} {Message}{NewLine}{Exception}")
            .WriteTo.Async(a => a.File(
                Path.Combine(appDataLogFolder, "error_user.log"),
                LogEventLevel.Warning,
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 7,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}"))
            .WriteTo.Sink(bugReportSink)
            .CreateLogger();

        try
        {
            var resourcesPath = FindResourcesPath();
            if (resourcesPath == null)
            {
                Log.Error("Could not locate SimpleLauncher/resources directory.");
                Environment.Exit(1);
            }

            var englishFile = Path.Combine(resourcesPath, "strings.en.xaml");
            if (!File.Exists(englishFile))
            {
                Log.Error("English resource file not found: {FilePath}", englishFile);
                Environment.Exit(1);
            }

            Log.Information("Simple Launcher Resource Translator");
            Log.Information("===================================");
            Console.WriteLine();

            var englishKeys = ResourceAnalyzer.ReadEnglishKeys(englishFile);
            Log.Information("English base file loaded: {KeyCount} keys", englishKeys.Count);

            var batches = ResourceAnalyzer.AnalyzeAllLanguages(resourcesPath, englishKeys);

            if (batches.Count == 0)
            {
                Console.WriteLine();
                Log.Information("All language files are fully synchronized with English. No action needed.");
                return;
            }

            var totalMissing = batches.Sum(static b => b.MissingKeys.Count);
            var totalDuplicates = batches.Sum(static b => b.DuplicateKeysRemoved.Count);

            Console.WriteLine();
            Log.Information("Analysis Results:");
            Log.Information("  Languages needing updates: {BatchCount}", batches.Count);
            Log.Information("  Total missing keys: {TotalMissing}", totalMissing);
            if (totalDuplicates > 0)
                Log.Information("  Total duplicate keys to remove: {TotalDuplicates}", totalDuplicates);
            Console.WriteLine();

            foreach (var batch in batches)
                Log.Information(
                    "  [{LanguageCode}] {LanguageName}: {MissingCount} missing, {DuplicateCount} duplicates",
                    batch.LanguageCode, batch.LanguageName, batch.MissingKeys.Count, batch.DuplicateKeysRemoved.Count);

            Console.WriteLine();
            Console.WriteLine("Press any key to proceed with translation, or Ctrl+C to cancel...");
            Console.ReadKey(true);
            Console.WriteLine();

            // Prompt for API key (not stored)
            Console.Write("Enter your Google Gemini API key: ");
            var apiKey = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(apiKey))
            {
                Log.Error("API key is required.");
                Environment.Exit(1);
            }

            Console.WriteLine();

            // Model selection
            var models = GeminiTranslationService.GetAvailableModels();
            Log.Information("Available Gemini models:");
            for (var i = 0; i < models.Count; i++)
            {
                var marker = string.Equals(models[i].Id, "gemini-2.5-flash", StringComparison.Ordinal)
                    ? " (default)"
                    : "";
                Console.WriteLine($"  {i + 1}. {models[i].Name} - {models[i].Description}{marker}");
            }

            Console.WriteLine();
            Console.Write("Select model number (press Enter for default): ");
            var modelInput = Console.ReadLine()?.Trim();

            GeminiModelInfo selectedModel;
            if (string.IsNullOrEmpty(modelInput) ||
                !int.TryParse(modelInput, CultureInfo.InvariantCulture, out var modelIndex) ||
                modelIndex < 1 || modelIndex > models.Count)
            {
                selectedModel = models.First(static m =>
                    string.Equals(m.Id, "gemini-2.5-flash", StringComparison.Ordinal));
                Log.Information("Using default model: {ModelName}", selectedModel.Name);
            }
            else
            {
                selectedModel = models[modelIndex - 1];
                Log.Information("Selected model: {ModelName}", selectedModel.Name);
            }

            Console.WriteLine();

            var translator = new GeminiTranslationService(apiKey, selectedModel.Id, selectedModel.ApiVersion);
            var overallStopwatch = Stopwatch.StartNew();

            foreach (var batch in batches)
            {
                Log.Information("Processing [{LanguageCode}] {LanguageName}...", batch.LanguageCode,
                    batch.LanguageName);
                var languageStopwatch = Stopwatch.StartNew();

                var allTranslations = new Dictionary<string, string>(StringComparer.Ordinal);
                var missingList = batch.MissingKeys;
                var totalBatches = (int)Math.Ceiling(missingList.Count / (double)BatchSize);

                for (var i = 0; i < missingList.Count; i += BatchSize)
                {
                    var currentBatch = missingList.Skip(i).Take(BatchSize).ToList();
                    var batchNumber = i / BatchSize + 1;

                    Console.Write($"  Batch {batchNumber}/{totalBatches} ({currentBatch.Count} keys)... ");
                    var sw = Stopwatch.StartNew();

                    try
                    {
                        var translations = await translator.TranslateBatchAsync(batch.LanguageName, currentBatch);
                        foreach (var kvp in translations) allTranslations[kvp.Key] = kvp.Value;

                        Console.WriteLine($"done in {sw.ElapsedMilliseconds}ms");
                        Log.Debug("Batch {BatchNumber} completed in {ElapsedMs}ms", batchNumber,
                            sw.ElapsedMilliseconds);
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Batch {BatchNumber} failed for {LanguageName}", batchNumber,
                            batch.LanguageName);
                        Console.WriteLine($"FAILED: {ex.Message}");
                        Console.WriteLine("  This batch was skipped and will not be written to the resource file.");
                    }

                    // Small delay to avoid rate limits
                    if (i + BatchSize < missingList.Count) await Task.Delay(500);
                }

                // Write back to XAML
                XamlResourceWriter.UpdateResourceFile(batch.FilePath, allTranslations, batch.DuplicateKeysRemoved);

                languageStopwatch.Stop();
                Log.Information("Written {TranslationCount} entries to {FileName} in {ElapsedMs}ms",
                    allTranslations.Count, Path.GetFileName(batch.FilePath), languageStopwatch.ElapsedMilliseconds);
                Console.WriteLine();
            }

            overallStopwatch.Stop();
            Log.Information("===================================");
            Log.Information("Translation complete!");
            Log.Information("Total time: {Minutes:D2}:{Seconds:D2}", overallStopwatch.Elapsed.Minutes,
                overallStopwatch.Elapsed.Seconds);
            Log.Information("Languages updated: {BatchCount}", batches.Count);
            Log.Information("Total keys translated: {TotalMissing}", totalMissing);
            if (totalDuplicates > 0)
                Log.Information("Total duplicates removed: {TotalDuplicates}", totalDuplicates);
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "An unhandled error occurred during translation");
            throw;
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    /// <summary>
    ///     Synchronous entry point that invokes the asynchronous main method.
    /// </summary>
    /// <param name="args">Command-line arguments.</param>
    public static void Main(string[] args)
    {
        MainAsync().GetAwaiter().GetResult();
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