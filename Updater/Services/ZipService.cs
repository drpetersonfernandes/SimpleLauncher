using System.Security;
using System.IO;
using SharpCompress.Archives.Zip;

namespace Updater.Services;

/// <summary>
/// Provides progress information for ZIP extraction operations.
/// </summary>
public class ExtractionProgressInfo
{
    /// <summary>
    /// The percentage of completion (0-100).
    /// </summary>
    public double Percentage { get; set; }

    /// <summary>
    /// The current file being extracted.
    /// </summary>
    public string? CurrentFile { get; set; }

    /// <summary>
    /// The number of files extracted so far.
    /// </summary>
    public int ExtractedCount { get; set; }

    /// <summary>
    /// The total number of files to extract.
    /// </summary>
    public int TotalEntries { get; set; }
}

/// <summary>
/// Service for extracting ZIP archives with security checks and progress reporting.
/// </summary>
public class ZipService
{
    private const int FileBufferSize = 81920; // 80KB buffer for efficient file I/O

    private readonly string _appDirectory;

    /// <summary>
    /// Event raised when extraction progress changes.
    /// </summary>
    public event Action<ExtractionProgressInfo>? ProgressChanged;

    /// <summary>
    /// Event raised when a log message needs to be displayed.
    /// </summary>
    public event Action<string>? LogMessage;

    /// <summary>
    /// Gets or sets the array of filenames to exclude from extraction.
    /// These are typically files that should not be overwritten (like the updater itself).
    /// </summary>
    public string[] IgnoredFiles { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Initializes a new instance of the ZipService class.
    /// </summary>
    /// <param name="appDirectory">The directory where files should be extracted.</param>
    public ZipService(string appDirectory)
    {
        _appDirectory = appDirectory;
    }

    /// <summary>
    /// Extracts a ZIP archive from a memory stream to the application directory.
    /// </summary>
    /// <param name="zipStream">The memory stream containing the ZIP archive.</param>
    /// <returns>The number of files extracted.</returns>
    /// <exception cref="SecurityException">Thrown when a ZIP entry attempts to escape the target directory.</exception>
    public async Task<int> ExtractFromStreamAsync(MemoryStream zipStream)
    {
        LogMessage?.Invoke("Extracting update files...");

        zipStream.Position = 0;
        using var archive = ZipArchive.OpenArchive(zipStream);
        var entries = archive.Entries.Where(static e => !string.IsNullOrEmpty(e.Key)).ToList();
        var totalEntries = entries.Count;
        var extractedCount = 0;

        // Report initial progress
        ProgressChanged?.Invoke(new ExtractionProgressInfo
        {
            Percentage = 0,
            CurrentFile = null,
            ExtractedCount = 0,
            TotalEntries = totalEntries
        });

        foreach (var entry in entries)
        {
            try
            {
                extractedCount++;
                var fileName = Path.GetFileName(entry.Key);
                if (!string.IsNullOrEmpty(fileName) && IgnoredFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                {
                    LogMessage?.Invoke($"Skipping self-update file: {entry.Key}");
                    continue;
                }

                // Validate and sanitize entry path to prevent path traversal attacks
                if (entry.Key != null)
                {
                    var safeEntryName = entry.Key.Replace("..", "").TrimStart('/', '\\');
                    var destinationPath = Path.GetFullPath(Path.Combine(_appDirectory, safeEntryName));
                    var appDirectoryFullPath = Path.GetFullPath(_appDirectory);

                    // Security check: ensure the destination path is within AppDirectory
                    if (!destinationPath.StartsWith(appDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new SecurityException($"Zip entry attempts to escape target directory: {entry.Key}");
                    }

                    var destinationDirectory = Path.GetDirectoryName(destinationPath);

                    if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                        Directory.CreateDirectory(destinationDirectory);

                    if (entry.IsDirectory) continue;

                    // Report extraction progress
                    var extractPercentage = totalEntries > 0 ? (double)extractedCount / totalEntries * 100 : 0;
                    ProgressChanged?.Invoke(new ExtractionProgressInfo
                    {
                        Percentage = extractPercentage,
                        CurrentFile = entry.Key,
                        ExtractedCount = extractedCount,
                        TotalEntries = totalEntries
                    });

                    await using var entryStream = entry.OpenEntryStream();
                    await using var destinationFileStream = new FileStream(
                        destinationPath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        FileBufferSize,
                        true);
                    await entryStream.CopyToAsync(destinationFileStream);
                }

                LogMessage?.Invoke($"Extracted: {entry.Key}");
            }
            catch (Exception ex)
            {
                await BugReportService.ReportBugAsync(ex, $"Error extracting file: {entry.Key}");
                throw;
            }
        }

        // Report completion
        ProgressChanged?.Invoke(new ExtractionProgressInfo
        {
            Percentage = 100,
            CurrentFile = null,
            ExtractedCount = extractedCount,
            TotalEntries = totalEntries
        });

        LogMessage?.Invoke("Extraction complete");
        return extractedCount;
    }
}
