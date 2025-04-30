using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public static partial class UpdateChecker
{
    private const string RepoOwner = "drpetersonfernandes";
    private const string RepoName = "SimpleLauncher";
    private static HttpClient _httpClient;

    static UpdateChecker()
    {
        InitializeHttpClient();
    }

    private static void InitializeHttpClient()
    {
        var handler = new HttpClientHandler();
        _httpClient = new HttpClient(handler);
    }

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
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");

            var response = await _httpClient.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
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
            const string contextMessage = "Error checking for updates.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    // Check for update from within About Window
    public static async Task CheckForUpdatesVariantAsync(Window mainWindow)
    {
        try
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "request");

            var response = await _httpClient.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
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
                    MessageBoxLibrary.ThereIsNoUpdateAvailableMessageBox(mainWindow, CurrentVersion);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error checking for updates.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorCheckingForUpdatesMessageBox(mainWindow);
        }
    }

    private static async void ShowUpdateWindow(string assetUrl, string currentVersion, string latestVersion, Window owner)
    {
        try
        {
            // Notify user
            var result = MessageBoxLibrary.DoYouWantToUpdateMessageBox(currentVersion, latestVersion, owner);

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
                const string contextMessage = "There was an error updating the application.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                    // Notify user
                    MessageBoxLibrary.InstallUpdateManuallyMessageBox(RepoOwner, RepoName);

                    logWindow.Log("There was an error updating the application.");
                    logWindow.Log("Please update it manually.");
                    logWindow.Close();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was an error updating the application.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    private static async Task DownloadUpdateFileToMemory(string url, MemoryStream memoryStream)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
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

                // Notify user
                MessageBoxLibrary.DownloadManuallyMessageBox(RepoOwner, RepoName);

                logWindow.Close();

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
        Application.Current.Dispatcher.Invoke(static () =>
        {
            QuitApplication.ForcefullyQuitApplication();
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

            var versionMatch = MyRegex3().Match(versionTag ?? string.Empty);
            if (versionMatch.Success)
            {
                return (NormalizeVersion(versionMatch.Value), assetUrl);
            }

            // Notify developer
            const string contextMessage = "There was an error parsing the application version from the UpdateChecker class.\n" +
                                          "Version number was not found in the tag.";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
        else
        {
            // Notify developer
            const string contextMessage = "There was an error parsing the application version from the UpdateChecker class.\n" +
                                          "Version information not found in the response.";
            var ex = new Exception(contextMessage);
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }

        return (null, null);
    }

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

    public static void DisposeHttpClient()
    {
        _httpClient?.Dispose();
        _httpClient = null;
    }

    [GeneratedRegex(@"[^\d\.]")]
    private static partial Regex MyRegex1();

    [GeneratedRegex(@"[^\d\.]")]
    private static partial Regex MyRegex2();

    [GeneratedRegex(@"(?<=release(?:-[a-zA-Z0-9]+)?-?)\d+(\.\d+)*", RegexOptions.Compiled)]
    private static partial Regex MyRegex3();
}