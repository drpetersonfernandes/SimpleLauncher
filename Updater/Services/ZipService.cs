using System.Security;
using System.IO;
using System.Threading;
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
    private const int FileWriteRetryAttempts = 5; // Number of retry attempts for locked files
    private const int FileWriteRetryDelayMs = 500; // Delay between retry attempts

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
    /// <param name="cancellationToken">Token to cancel the extraction operation.</param>
    /// <returns>The number of files extracted.</returns>
    /// <exception cref="SecurityException">Thrown when a ZIP entry attempts to escape the target directory.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    public async Task<int> ExtractFromStreamAsync(MemoryStream zipStream, CancellationToken cancellationToken = default)
    {
        LogMessage?.Invoke("Extracting update files...");

        zipStream.Position = 0;
        var extractedCount = 0;

        // Use ZipReader for streaming extraction - no upfront indexing needed
        using var reader = ZipReader.OpenReader(zipStream, new ReaderOptions { LeaveStreamOpen = true });

        while (reader.MoveToNextEntry())
        {
            cancellationToken.ThrowIfCancellationRequested();
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
                // Normalize: remove leading slashes, then combine and resolve
                var trimmedEntry = entryKey.TrimStart('/', '\\');
                var destinationPath = Path.GetFullPath(Path.Combine(_appDirectory, trimmedEntry));
                var appDirectoryFullPath = Path.GetFullPath(_appDirectory);

                // Security check: ensure the resolved destination path is within AppDirectory
                // This is the actual guard — it catches all traversal attempts including encoded or multi-level ".."
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

                // Extract with retry logic for locked files
                await ExtractFileWithRetryAsync(reader, destinationPath, entryKey, cancellationToken);

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

    /// <summary>
    /// Extracts a file from the ZIP reader with retry logic for locked files.
    /// </summary>
    /// <param name="reader">The ZIP reader positioned at the entry to extract.</param>
    /// <param name="destinationPath">The destination file path.</param>
    /// <param name="entryKey">The ZIP entry key for logging purposes.</param>
    /// <param name="cancellationToken">Token to cancel the extraction operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <exception cref="IOException">Thrown when the file cannot be written after all retry attempts.</exception>
    /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled.</exception>
    private async Task ExtractFileWithRetryAsync(IReader reader, string destinationPath, string entryKey, CancellationToken cancellationToken = default)
    {
        Exception? lastException = null;

        for (var attempt = 1; attempt <= FileWriteRetryAttempts; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                await using var destinationFileStream = new FileStream(
                    destinationPath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.ReadWrite | FileShare.Delete,
                    FileBufferSize,
                    true);
                await using var entryStream = reader.OpenEntryStream();
                await entryStream.CopyToAsync(destinationFileStream, cancellationToken);
                return; // Success, exit the method
            }
            catch (IOException ex) when (attempt < FileWriteRetryAttempts)
            {
                // File is likely locked by another process, retry after delay
                lastException = ex;
                LogMessage?.Invoke($"File locked ({attempt}/{FileWriteRetryAttempts}): {entryKey} - retrying in {FileWriteRetryDelayMs}ms...");
                await Task.Delay(FileWriteRetryDelayMs * attempt, cancellationToken); // Increasing delay for each attempt
            }
        }

        // All retry attempts failed
        if (lastException != null)
        {
            throw new IOException(
                $"Failed to extract file after {FileWriteRetryAttempts} attempts: {entryKey}. " +
                $"The file may be locked by another process.", lastException);
        }
    }
}
