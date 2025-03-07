using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Security.Authentication;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleLauncher;

public static partial class UpdateChecker
{
    private const string RepoOwner = "drpetersonfernandes";
    private const string RepoName = "SimpleLauncher";

    private static string CurrentVersion
    {
        get
        {
            try
            {
                return NormalizeVersion(Assembly.GetExecutingAssembly().GetName().Version?.ToString());
            }
            catch
            {
                var unknown2 = (string)Application.Current.TryFindResource("Unknown") ?? "Unknown";
                return unknown2;
            }
        }
    }

    private static readonly string[] Function =
    [
        "Updater.deps.json",
        "Updater.dll",
        "Updater.exe",
        "Updater.pdb",
        "Updater.runtimeconfig.json"
    ];

    public static async Task CheckForUpdatesAsync(Window mainWindow)
    {
        try
        {
            var handler = new HttpClientHandler();
            handler.SslProtocols = SslProtocols.Tls12;
            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "request");

            var response = await client.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var (latestVersion, assetUrl) = ParseVersionFromResponse(content);

                if (IsNewVersionAvailable(CurrentVersion, latestVersion))
                {
                    ShowUpdateWindow(assetUrl, CurrentVersion, latestVersion, mainWindow);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify user
            var contextMessage = $"Error checking for updates.\n\n" +
                                 $"Exception type: {ex.GetType().Name}\n" +
                                 $"Exception details: {ex.Message}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            // Ignore
        }
    }

    // Check for update from within About Window
    public static async Task CheckForUpdatesVariantAsync(Window mainWindow)
    {
        try
        {
            var handler = new HttpClientHandler();
            handler.SslProtocols = SslProtocols.Tls12;
            using var client = new HttpClient(handler);
            client.DefaultRequestHeaders.Add("User-Agent", "request");

            var response = await client.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var (latestVersion, assetUrl) = ParseVersionFromResponse(content);

                if (IsNewVersionAvailable(CurrentVersion, latestVersion))
                {
                    ShowUpdateWindow(assetUrl, CurrentVersion, latestVersion, mainWindow);
                }
                else
                {
                    // Notify user
                    ThereIsNoUpdateAvailableMessageBox();
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Error checking for updates.\n\n" +
                                 $"Exception type: {ex.GetType().Name}\n" +
                                 $"Exception details: {ex.Message}";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            ErrorCheckingForUpdatesMessageBox();
        }

        void ThereIsNoUpdateAvailableMessageBox()
        {
            var thereisnoupdateavailable2 = (string)Application.Current.TryFindResource("thereisnoupdateavailable") ?? "There is no update available.";
            var thecurrentversionis2 = (string)Application.Current.TryFindResource("Thecurrentversionis") ?? "The current version is";
            var noupdateavailable2 = (string)Application.Current.TryFindResource("Noupdateavailable") ?? "No update available";
            MessageBox.Show(mainWindow, $"{thereisnoupdateavailable2}\n\n" +
                                        $"{thecurrentversionis2} {CurrentVersion}",
                noupdateavailable2, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        void ErrorCheckingForUpdatesMessageBox()
        {
            var therewasanerrorcheckingforupdates2 = (string)Application.Current.TryFindResource("Therewasanerrorcheckingforupdates") ?? "There was an error checking for updates.";
            var maybethereisaproblemwithyourinternet2 = (string)Application.Current.TryFindResource("Maybethereisaproblemwithyourinternet") ?? "Maybe there is a problem with your internet access or the GitHub server is offline.";
            var error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show(mainWindow, $"{therewasanerrorcheckingforupdates2}\n\n" +
                                        $"{maybethereisaproblemwithyourinternet2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }

    private static async void ShowUpdateWindow(string assetUrl, string currentVersion, string latestVersion, Window owner)
    {
        try
        {
            // Notify user
            var result = DoYouWantToUpdateMessageBox();

            MessageBoxResult DoYouWantToUpdateMessageBox()
            {
                var thereisasoftwareupdateavailable2 = (string)Application.Current.TryFindResource("Thereisasoftwareupdateavailable") ?? "There is a software update available.";
                var thecurrentversionis2 = (string)Application.Current.TryFindResource("Thecurrentversionis") ?? "The current version is";
                var theupdateversionis2 = (string)Application.Current.TryFindResource("Theupdateversionis") ?? "The update version is";
                var doyouwanttodownloadandinstall2 = (string)Application.Current.TryFindResource("Doyouwanttodownloadandinstall") ?? "Do you want to download and install the latest version automatically?";
                var updateAvailable2 = (string)Application.Current.TryFindResource("UpdateAvailable") ?? "Update Available";
                var message = $"{thereisasoftwareupdateavailable2}\n" +
                              $"{thecurrentversionis2} {currentVersion}\n" +
                              $"{theupdateversionis2} {latestVersion}\n\n" +
                              $"{doyouwanttodownloadandinstall2}";
                var messageBoxResult1 = MessageBox.Show(owner, message,
                    updateAvailable2, MessageBoxButton.YesNo, MessageBoxImage.Information);
                return messageBoxResult1;
            }

            if (result != MessageBoxResult.Yes) return;

            var logWindow = new UpdateLogWindow();
            logWindow.Show();
            logWindow.Log("Starting update process...");

            // Close the main window
            owner.Close();

            try
            {
                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;

                logWindow.Log("Downloading update file...");

                await Task.Run(async () =>
                {
                    using var memoryStream = new MemoryStream();

                    // Download the update file to memory
                    await DownloadUpdateFileToMemory(assetUrl, memoryStream);

                    logWindow.Log("Extracting update file...");

                    // Files to be updated
                    var updaterFiles = Function;

                    // Extract directly from memory to the destination
                    ExtractFilesToDestination(memoryStream, appDirectory, updaterFiles, logWindow);

                    logWindow.Log("Update completed successfully.");

                    await Task.Delay(2000);

                    // Execute Updater
                    await ExecuteUpdater(logWindow);
                });
            }
            catch (Exception ex)
            {
                // Notify developer
                var contextMessage = $"There was an error updating the application.\n\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, contextMessage);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Notify user
                    MessageBoxLibrary.InstallUpdateManuallyMessageBox(RepoOwner, RepoName);

                    logWindow.Log("There was an error updating the application.");
                    logWindow.Log("Please update it manually.");
                    logWindow.Close();
                });
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"There was an error updating the application.\n\n" +
                                 $"Exception type: {ex.GetType().Name}\n" +
                                 $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            // Ignore
        }
    }

    private static async Task DownloadUpdateFileToMemory(string url, MemoryStream memoryStream)
    {
        var handler = new HttpClientHandler();
        handler.SslProtocols = SslProtocols.Tls12;
        using var client = new HttpClient(handler);
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        await stream.CopyToAsync(memoryStream);
    }

    private static void ExtractFilesToDestination(Stream zipStream, string destinationPath, string[] filesToExtract, UpdateLogWindow logWindow)
    {
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

        foreach (var fileName in filesToExtract)
        {
            var entry = archive.GetEntry(fileName);
            if (entry != null)
            {
                var destinationFile = Path.Combine(destinationPath, fileName);
                logWindow.Log($"Extracting {fileName} to {destinationFile}");

                using var entryStream = entry.Open();
                using var fileStream = new FileStream(destinationFile, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                entryStream.CopyTo(fileStream);
            }
            else
            {
                logWindow.Log($"File {fileName} not found in the archive.");
            }
        }
    }

    private static async Task ExecuteUpdater(UpdateLogWindow logWindow)
    {
        var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var appExePath = Assembly.GetExecutingAssembly().Location;
        var updaterExePath = Path.Combine(appDirectory, "Updater.exe");

        if (!File.Exists(updaterExePath))
        {
            logWindow.Log("Updater.exe not found in the application directory.");
            logWindow.Log("Please reinstall 'Simple Launcher' manually to fix the issue.");

            Application.Current.Dispatcher.Invoke(() =>
            {
                // Notify user
                MessageBoxLibrary.DownloadManuallyMessageBox(RepoOwner, RepoName);

                logWindow.Close();
            });
            return;
        }

        logWindow.Log("Starting updater process...");
        await Task.Delay(2000);

        Process.Start(new ProcessStartInfo
        {
            FileName = updaterExePath,
            Arguments = appExePath,
            UseShellExecute = false
        });

        logWindow.Log("Closing main application for update...");

        // Close Simple Launcher
        Application.Current.Dispatcher.Invoke(() =>
        {
            foreach (Window window in Application.Current.Windows)
            {
                window.Close(); // Close each window
            }

            GC.Collect(); // Force garbage collection
            GC.WaitForPendingFinalizers(); // Wait for finalizers to complete
            Application.Current.Shutdown(); // Shutdown the application
            Process.GetCurrentProcess().Kill(); // Forcefully kill the process
        });
    }

    private static bool IsNewVersionAvailable(string currentVersion, string latestVersion)
    {
        var current = new Version(MyRegex1().Replace(currentVersion, ""));
        var latest = new Version(MyRegex2().Replace(latestVersion, ""));
        var versionComparison = latest.CompareTo(current);
        return versionComparison > 0;
    }

    private static (string version, string assetUrl) ParseVersionFromResponse(string jsonResponse)
    {
        using var doc = JsonDocument.Parse(jsonResponse);
        var root = doc.RootElement;

        if (root.TryGetProperty("tag_name", out var tagNameElement) &&
            root.TryGetProperty("assets", out var assetsElement))
        {
            var versionTag = tagNameElement.GetString();
            string assetUrl = null;
            foreach (var asset in assetsElement.EnumerateArray())
            {
                if (!asset.TryGetProperty("browser_download_url", out var downloadUrlElement)) continue;
                assetUrl = downloadUrlElement.GetString();
                break;
            }

            var versionMatch = MyRegex().Match(versionTag ?? string.Empty);
            if (versionMatch.Success)
            {
                return (NormalizeVersion(versionMatch.Value), assetUrl);
            }

            LogErrorAsync("There was an error parsing the application version from the UpdateChecker class. Version number was not found in the tag.");
        }
        else
        {
            LogErrorAsync("There was an error parsing the application version from the UpdateChecker class. Version information not found in the response.");
        }

        return (null, null);
    }

    private static Regex MyRegex() => MyRegex3();

    private static string NormalizeVersion(string version)
    {
        if (string.IsNullOrEmpty(version)) return "0.0.0.0";

        var versionParts = version.Split('.');
        while (versionParts.Length < 4)
        {
            version += ".0";
            versionParts = version.Split('.');
        }

        // Remove any trailing dots (if any)
        return version.TrimEnd('.');
    }

    private static void LogErrorAsync(string message)
    {
        Exception exception = new(message);
        _ = LogErrors.LogErrorAsync(exception, message);
    }

    [GeneratedRegex(@"[^\d\.]")]
    private static partial Regex MyRegex1();

    [GeneratedRegex(@"[^\d\.]")]
    private static partial Regex MyRegex2();

    [GeneratedRegex(@"(?<=release(?:-[a-zA-Z0-9]+)?-?)\d+(\.\d+)*", RegexOptions.Compiled)]
    private static partial Regex MyRegex3();
}