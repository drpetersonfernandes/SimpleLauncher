using System.IO;

namespace Updater.Services;

/// <summary>
/// Event arguments for download progress updates.
/// </summary>
public class DownloadProgressEventArgs : EventArgs
{
    public double Percentage { get; set; }
    public long BytesRead { get; set; }
    public long TotalBytes { get; set; }
    public string StatusText { get; set; } = "";
}

/// <summary>
/// Event arguments for extraction progress updates.
/// </summary>
public class ExtractionProgressEventArgs : EventArgs
{
    public string? CurrentFile { get; set; }
    public int ExtractedCount { get; set; }
    public string StatusText { get; set; } = "";
}

/// <summary>
/// Represents the result of an update operation.
/// </summary>
public class UpdateResult
{
    public bool Success { get; set; }
    public string? Version { get; set; }
    public string? ErrorMessage { get; set; }
    public bool RequiresManualUpdate { get; set; }
}

/// <summary>
/// Service that orchestrates the entire update process using other specialized services.
/// </summary>
public class UpdateService
{
    private readonly GitHubService _gitHubService;
    private readonly DownloadService _downloadService;
    private readonly ZipService _zipService;
    private readonly ProcessService _processService;
    private readonly DokanService _dokanService;
    private readonly string _appDirectory;

    /// <summary>
    /// Event raised when a log message should be displayed.
    /// </summary>
    public event Action<string>? LogMessage;

    /// <summary>
    /// Event raised when download progress changes.
    /// </summary>
    public event EventHandler<DownloadProgressEventArgs>? DownloadProgressChanged;

    /// <summary>
    /// Event raised when download progress should be reset.
    /// </summary>
    public event EventHandler? DownloadProgressReset;

    /// <summary>
    /// Event raised when extraction progress changes.
    /// </summary>
    public event EventHandler<ExtractionProgressEventArgs>? ExtractionProgressChanged;

    /// <summary>
    /// Event raised when extraction starts (to configure progress bar).
    /// </summary>
    public event EventHandler? ExtractionStarted;

    /// <summary>
    /// Event raised when extraction completes.
    /// </summary>
    public event EventHandler? ExtractionCompleted;

    /// <summary>
    /// Event raised when Dokan is not installed and the user should be prompted.
    /// Returns true if the user wants to install Dokan, false otherwise.
    /// </summary>
    public event Func<Task<bool>>? DokanInstallationPrompt;

    /// <summary>
    /// Initializes a new instance of the UpdateService class.
    /// </summary>
    public UpdateService(
        GitHubService gitHubService,
        DownloadService downloadService,
        ZipService zipService,
        ProcessService processService,
        DokanService dokanService,
        string appDirectory)
    {
        _gitHubService = gitHubService;
        _downloadService = downloadService;
        _zipService = zipService;
        _processService = processService;
        _dokanService = dokanService;
        _appDirectory = appDirectory;

        // Wire up service events
        _gitHubService.LogMessage += msg => LogMessage?.Invoke(msg);
        _downloadService.LogMessage += msg => LogMessage?.Invoke(msg);
        _downloadService.ProgressChanged += OnDownloadProgressChanged;
        _zipService.LogMessage += msg => LogMessage?.Invoke(msg);
        _zipService.ProgressChanged += OnExtractionProgressChanged;
        _processService.LogMessage += msg => LogMessage?.Invoke(msg);
        _dokanService.LogMessage += msg => LogMessage?.Invoke(msg);
        _dokanService.ProgressChanged += OnDownloadProgressChanged;
    }

    /// <summary>
    /// Executes the complete update process.
    /// </summary>
    /// <param name="processId">The process ID of the main application to wait for, or null.</param>
    /// <param name="ignoredFiles">Files to exclude during extraction (typically updater files).</param>
    /// <returns>The result of the update operation.</returns>
    public async Task<UpdateResult> ExecuteUpdateAsync(int? processId, string[] ignoredFiles)
    {
        if (string.IsNullOrEmpty(_appDirectory))
        {
            return new UpdateResult
            {
                Success = false,
                ErrorMessage = "Could not determine the application directory.",
                RequiresManualUpdate = true
            };
        }

        try
        {
            // Wait for the main application to exit
            try
            {
                await _processService.WaitForProcessExitAsync(processId);
            }
            catch (AlreadyReportedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await BugReportService.ReportBugAsync(ex, "Error waiting for main application to exit");
                throw new AlreadyReportedException(ex);
            }

            // Fetch the latest release from GitHub
            string assetUrl;
            string latestVersion;
            try
            {
                (latestVersion, assetUrl) = await _gitHubService.GetLatestReleaseAssetUrlAsync();
            }
            catch (AlreadyReportedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await BugReportService.ReportBugAsync(ex, "Error fetching latest release from GitHub");
                throw new AlreadyReportedException(ex);
            }

            // Download the update file to memory
            MemoryStream updateFileStream;
            try
            {
                DownloadProgressReset?.Invoke(this, EventArgs.Empty);
                updateFileStream = await _downloadService.DownloadToMemoryAsync(assetUrl);
                DownloadProgressReset?.Invoke(this, EventArgs.Empty);
            }
            catch (AlreadyReportedException)
            {
                throw;
            }
            catch (Exception ex)
            {
                await BugReportService.ReportBugAsync(ex, $"Error downloading update file from: {assetUrl}");
                throw new AlreadyReportedException(ex);
            }

            // Configure ignored files for ZIP extraction
            _zipService.IgnoredFiles = ignoredFiles;

            // Extract the ZIP file
            try
            {
                ExtractionStarted?.Invoke(this, EventArgs.Empty);
                await _zipService.ExtractFromStreamAsync(updateFileStream);
                ExtractionCompleted?.Invoke(this, EventArgs.Empty);
            }
            catch (AlreadyReportedException)
            {
                throw;
            }
            // InvalidOperationException is excluded here because it typically indicates the UI (Dispatcher)
            // has been shut down (e.g., window closed), which is a normal application lifecycle event
            // during update and not a bug that needs reporting. Other exceptions during extraction
            // are genuine errors that should be reported.
            catch (Exception ex) when (ex is not InvalidOperationException)
            {
                await BugReportService.ReportBugAsync(ex, "Error extracting ZIP archive");
                throw new AlreadyReportedException(ex);
            }
            finally
            {
                await updateFileStream.DisposeAsync();
            }

            LogMessage?.Invoke("Update installed successfully.");

            return new UpdateResult
            {
                Success = true,
                Version = latestVersion
            };
        }
        catch (AlreadyReportedException)
        {
            LogMessage?.Invoke("Automatic update failed due to an already reported error.");
            return new UpdateResult
            {
                Success = false,
                ErrorMessage = "Automatic update failed.",
                RequiresManualUpdate = true
            };
        }
        catch (Exception ex)
        {
            // Report bug to the bug report API
            await BugReportService.ReportBugAsync(ex, "Error during update process (UpdateService)");

            LogMessage?.Invoke($"Automatic update failed: {ex.Message}");
            return new UpdateResult
            {
                Success = false,
                ErrorMessage = "Automatic update failed.",
                RequiresManualUpdate = true
            };
        }
    }

    /// <summary>
    /// Restarts the main application after a successful update.
    /// </summary>
    /// <returns>True if the restart was successful, false otherwise.</returns>
    public bool RestartMainApplication()
    {
        return _processService.RestartApplication(_appDirectory, "SimpleLauncher", "-whatsnew");
    }

    /// <summary>
    /// Opens the manual download page for the user to download the update manually.
    /// </summary>
    public void OpenManualDownloadPage()
    {
        _processService.OpenUrl(GitHubService.GetReleasesPageUrl());
    }

    /// <summary>
    /// Checks if Dokan is installed and offers to install it if missing.
    /// </summary>
    public async Task CheckAndInstallDokanAsync()
    {
        try
        {
            if (_dokanService.IsDokanInstalled())
            {
                LogMessage?.Invoke("Dokan is already installed. No action needed.");
                return;
            }

            // Dokan is not installed - ask the user
            LogMessage?.Invoke("Dokan library is not installed.");

            if (DokanInstallationPrompt != null)
            {
                var shouldInstall = await DokanInstallationPrompt.Invoke();
                if (!shouldInstall)
                {
                    LogMessage?.Invoke("Skipping Dokan installation.");
                    return;
                }
            }
            else
            {
                LogMessage?.Invoke("No prompt handler configured. Skipping Dokan installation.");
                return;
            }

            // User chose to install Dokan
            LogMessage?.Invoke("Starting Dokan download and installation...");
            await _dokanService.DownloadAndInstallDokanAsync(_appDirectory);
            LogMessage?.Invoke("Dokan installer has been launched.");
        }
        catch (Exception ex)
        {
            await BugReportService.ReportBugAsync(ex, "Error during Dokan installation check");
            LogMessage?.Invoke($"Error during Dokan check/installation: {ex.Message}");
        }
    }

    private void OnDownloadProgressChanged(DownloadProgressInfo info)
    {
        string statusText;
        if (info.Percentage >= 0)
        {
            var speedText = DownloadService.FormatBytes(info.BytesPerSecond) + "/s";
            statusText = $"{DownloadService.FormatBytes(info.BytesRead)} / {DownloadService.FormatBytes(info.TotalBytes)} ({info.Percentage:F1}%) - {speedText}";
        }
        else
        {
            statusText = $"Downloaded: {DownloadService.FormatBytes(info.BytesRead)}";
        }

        DownloadProgressChanged?.Invoke(this, new DownloadProgressEventArgs
        {
            Percentage = info.Percentage,
            BytesRead = info.BytesRead,
            TotalBytes = info.TotalBytes,
            StatusText = statusText
        });
    }

    private void OnExtractionProgressChanged(ExtractionProgressInfo info)
    {
        string statusText;
        if (info.CurrentFile != null)
        {
            statusText = $"Extracting ({info.ExtractedCount}): {info.CurrentFile}";
        }
        else
        {
            statusText = "Extraction complete";
        }

        ExtractionProgressChanged?.Invoke(this, new ExtractionProgressEventArgs
        {
            CurrentFile = info.CurrentFile,
            ExtractedCount = info.ExtractedCount,
            StatusText = statusText
        });
    }
}
