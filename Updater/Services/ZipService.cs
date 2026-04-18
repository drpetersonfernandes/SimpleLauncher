using System.Security;
using System.IO;
using SharpCompress.Readers;
using SharpCompress.Readers.Zip;

namespace Updater.Services;

/// <summary>
/// Provides progress information for ZIP extraction operations.
/// </summary>
public class ExtractionProgressInfo
{
    /// <summary>
    /// The current file being extracted.
    /// </summary>
    public string? CurrentFile { get; set; }

    /// <summary>
    /// The number of files extracted so far.
    /// </summary>
    public int ExtractedCount { get; set; }
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
    /// Uses streaming extraction without upfront indexing for faster start.
    /// </summary>
    /// <param name="zipStream">The memory stream containing the ZIP archive.</param>
    /// <returns>The number of files extracted.</returns>
    /// <exception cref="SecurityException">Thrown when a ZIP entry attempts to escape the target directory.</exception>
    public async Task<int> ExtractFromStreamAsync(MemoryStream zipStream)
    {
        LogMessage?.Invoke("Extracting update files...");

        zipStream.Position = 0;
        var extractedCount = 0;

        // Use ZipReader for streaming extraction - no upfront indexing needed
        using var reader = ZipReader.OpenReader(zipStream, new ReaderOptions { LeaveStreamOpen = true });

        while (reader.MoveToNextEntry())
        {
            var entryKey = reader.Entry.Key;

            try
            {
                // Skip directory entries
                if (reader.Entry.IsDirectory)
                    continue;

                // Skip entries without keys
                if (string.IsNullOrEmpty(entryKey))
                    continue;

                var fileName = Path.GetFileName(entryKey);
                if (!string.IsNullOrEmpty(fileName) && IgnoredFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                {
                    LogMessage?.Invoke($"Skipping self-update file: {entryKey}");
                    continue;
                }

                // Validate and sanitize entry path to prevent path traversal attacks
                var safeEntryName = entryKey.Replace("..", "").TrimStart('/', '\\');
                var destinationPath = Path.GetFullPath(Path.Combine(_appDirectory, safeEntryName));
                var appDirectoryFullPath = Path.GetFullPath(_appDirectory);

                // Security check: ensure the destination path is within AppDirectory
                if (!destinationPath.StartsWith(appDirectoryFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    throw new SecurityException($"Zip entry attempts to escape target directory: {entryKey}");
                }

                var destinationDirectory = Path.GetDirectoryName(destinationPath);

                if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                    Directory.CreateDirectory(destinationDirectory);

                extractedCount++;

                // Report extraction progress (current file only, no percentage)
                ProgressChanged?.Invoke(new ExtractionProgressInfo
                {
                    CurrentFile = entryKey,
                    ExtractedCount = extractedCount
                });

                await using var destinationFileStream = new FileStream(
                    destinationPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    FileBufferSize,
                    true);
                await using var entryStream = reader.OpenEntryStream();
                await entryStream.CopyToAsync(destinationFileStream);

                LogMessage?.Invoke($"Extracted: {entryKey}");
            }
            catch (Exception ex)
            {
                await BugReportService.ReportBugAsync(ex, $"Error extracting file: {entryKey}");
                throw;
            }
        }

        // Report completion
        ProgressChanged?.Invoke(new ExtractionProgressInfo
        {
            CurrentFile = null,
            ExtractedCount = extractedCount
        });

        LogMessage?.Invoke($"Extraction complete ({extractedCount} files extracted)");
        return extractedCount;
    }
}
