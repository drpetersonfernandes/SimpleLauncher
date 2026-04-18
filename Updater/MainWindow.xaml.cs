using System.Diagnostics;
using System.Net.Http;
using System.Reflection;
using System.Windows;
using Updater.Services;

namespace Updater;

/// <summary>
/// Exception marker to track exceptions that have already been reported to avoid duplicate bug reports.
/// </summary>
internal class AlreadyReportedException : Exception
{
    public AlreadyReportedException(Exception innerException) : base("Exception already reported", innerException)
    {
    }
}

/// <summary>
/// Main window for the Updater application that manages the update process for SimpleLauncher.
/// </summary>
public partial class MainWindow
{
    private static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private static readonly HttpClient HttpClient = new() { Timeout = TimeSpan.FromMinutes(5) };

    private readonly UpdateService _updateService;
    private readonly string[] _args;

    // Files to exclude during extraction to prevent self-destruction
    private static readonly string[] IgnoredFiles =
    [
        "Updater.exe",
        "Updater.pdb",
        "Updater.dll",
        "Updater.deps.json",
        "Updater.runtimeconfig.json"
    ];

    static MainWindow()
    {
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SimpleLauncher-Updater");
    }

    /// <summary>
    /// Disposes the HttpClient instance. Should be called when the application is shutting down.
    /// </summary>
    public static void DisposeHttpClient()
    {
        HttpClient.Dispose();
    }

    /// <summary>
    /// Initializes a new instance of the MainWindow class.
    /// </summary>
    /// <param name="args">Command line arguments, typically containing the process ID of the main application.</param>
    public MainWindow(string[] args)
    {
        InitializeComponent();
        _args = args;

        // Initialize services
        _updateService = CreateUpdateService();
        WireUpServiceEvents();

        // Force UI to show on top and focused
        Loaded += (_, _) =>
        {
            Activate();
            Focus();
            Topmost = false; // Release topmost after initial show so user can switch away if needed
        };

        var applicationVersion = GetApplicationVersion();
        Log($"Updater version: {applicationVersion}\n\n");

        // Start update process async when window is loaded
        Loaded += async (_, _) => await ExecuteUpdateAsync();
    }

    /// <summary>
    /// Creates and configures the UpdateService with all required dependencies.
    /// </summary>
    private static UpdateService CreateUpdateService()
    {
        var gitHubService = new GitHubService(HttpClient);
        var downloadService = new DownloadService(HttpClient);
        var zipService = new ZipService(AppDirectory);
        var processService = new ProcessService();

        return new UpdateService(gitHubService, downloadService, zipService, processService, AppDirectory);
    }

    /// <summary>
    /// Wires up event handlers for the UpdateService events.
    /// </summary>
    private void WireUpServiceEvents()
    {
        _updateService.LogMessage += Log;
        _updateService.DownloadProgressChanged += (_, e) =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                DownloadProgressBar.Value = e.Percentage;
                ProgressStatusText.Text = e.StatusText;
            });
        };
        _updateService.DownloadProgressReset += (_, _) =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                DownloadProgressBar.Value = 0;
                ProgressStatusText.Text = "Download complete";
            });
        };
        _updateService.ExtractionStarted += (_, _) =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                DownloadProgressBar.Maximum = 100;
                DownloadProgressBar.Value = 0;
                ProgressStatusText.Text = "Extracting files...";
            });
        };
        _updateService.ExtractionProgressChanged += (_, e) =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                DownloadProgressBar.Value = e.Percentage;
                ProgressStatusText.Text = e.StatusText;
            });
        };
        _updateService.ExtractionCompleted += (_, _) =>
        {
            Dispatcher.BeginInvoke(() =>
            {
                DownloadProgressBar.Value = 0;
                ProgressStatusText.Text = "Extraction complete";
            });
        };
    }

    /// <summary>
    /// Gets the version of the currently executing assembly.
    /// </summary>
    /// <returns>The application version string, or "Version not available" if the version cannot be determined.</returns>
    private static string GetApplicationVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version?.ToString() ?? "Version not available";
    }

    /// <summary>
    /// Executes the update process asynchronously with error handling and bug reporting.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task ExecuteUpdateAsync()
    {
        try
        {
            // Parse process ID from command line arguments
            int? processId = null;
            if (_args.Length > 0 && int.TryParse(_args[0], out var pid))
            {
                processId = pid;
            }

            // Execute the update through the service
            var result = await _updateService.ExecuteUpdateAsync(processId, IgnoredFiles);

            if (result.Success)
            {
                MessageBox.Show("Update installed successfully.", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                _updateService.RestartMainApplication();
                Close();
            }
            else if (result.RequiresManualUpdate)
            {
                RedirectToDownloadPage(result.ErrorMessage ?? "Automatic update failed.\n\nWould you like to update manually?");
            }
        }
        catch (Exception ex)
        {
            // Report bug to the bug report API
            await BugReportService.ReportBugAsync(ex, "Error during main update execution");

            Log($"An error occurred during update process: {ex.Message}");
            Log("Please update manually.");
        }
    }

    private void RedirectToDownloadPage(string message)
    {
        if (!IsLoaded) return;

        var result = MessageBox.Show(message, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
        {
            _updateService.OpenManualDownloadPage();
        }

        Close();
    }

    private void Log(string message)
    {
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(() => Log(message));
            return;
        }

        if (IsLoaded)
        {
            try
            {
                LogTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
                LogTextBox.ScrollToEnd();
            }
            catch (InvalidOperationException ex)
            {
                // Window may have been closed, ignore logging but report bug (fire-and-forget)
                _ = ReportBugFireAndForgetAsync(ex, "Error logging message to UI");
            }
        }
    }

    /// <summary>
    /// Fire-and-forget helper for reporting bugs from synchronous contexts.
    /// Logs exceptions to Debug output if the bug report itself fails.
    /// </summary>
    private static async Task ReportBugFireAndForgetAsync(Exception exception, string context)
    {
        try
        {
            await BugReportService.ReportBugAsync(exception, context);
        }
        catch (Exception ex)
        {
            // If bug reporting fails, log to debug output - don't throw
            Debug.WriteLine($"Failed to report bug: {ex.Message}");
            Debug.WriteLine($"Original exception: {exception.Message}");
        }
    }
}