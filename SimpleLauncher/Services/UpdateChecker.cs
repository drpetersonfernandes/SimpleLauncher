using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleLauncher.Services;

public static partial class UpdateChecker
{
    private const string RepoOwner = "drpetersonfernandes";
    private const string RepoName = "SimpleLauncher";

    private static readonly IHttpClientFactory HttpClientFactory;

    private static string CurrentRuntimeIdentifier
    {
        get
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            return arch switch
            {
                Architecture.Arm64 => "win-arm64",
                Architecture.X64 => "win-x64",
                _ => "win-x64" // Fallback to x64 for any other architecture
            };
        }
    }

    static UpdateChecker()
    {
        HttpClientFactory = App.ServiceProvider?.GetService<IHttpClientFactory>();
    }

    private static string CurrentVersion
    {
        get
        {
            try
            {
                return NormalizeVersion(Assembly.GetExecutingAssembly().GetName().Version?.ToString());
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Error getting CurrentVersion.");
                var unknown = (string)Application.Current.TryFindResource("Unknown") ?? "Unknown";

                return unknown;
            }
        }
    }

    private static readonly char[] Separator = new[] { '.' };

    public static async Task SilentCheckForUpdatesAsync(Window mainWindow)
    {
        try
        {
            if (HttpClientFactory == null)
            {
                throw new InvalidOperationException("HttpClientFactory is not initialized. Update check cannot proceed.");
            }

            var httpClient = HttpClientFactory.CreateClient("UpdateCheckerClient");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "request");

            var response = await httpClient.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
            if (response.IsSuccessStatusCode)
            {
                DebugLogger.Log("Check for Updates Success");

                var content = await response.Content.ReadAsStringAsync();
                var (latestVersion, _, updaterZipAssetUrl) = ParseVersionAndAssetUrlsFromResponse(content);

                if (latestVersion == null) return;

                if (IsNewVersionAvailable(CurrentVersion, latestVersion))
                {
                    if (updaterZipAssetUrl != null)
                    {
                        var (_, releasePackageUrlForFallback, _) = ParseVersionAndAssetUrlsFromResponse(content);
                        await ShowUpdateWindow(updaterZipAssetUrl, releasePackageUrlForFallback, CurrentVersion, latestVersion, mainWindow);
                    }
                    else
                    {
                        // Notify developer
                        var expectedUpdaterFileName = $"updater_{CurrentRuntimeIdentifier}.zip";
                        _ = LogErrors.LogErrorAsync(new FileNotFoundException($"'{expectedUpdaterFileName}' not found for version {latestVersion}. Automatic update of updater not possible.", expectedUpdaterFileName), "Update Check Info");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error checking for updates (silent).";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    public static async Task ManualCheckForUpdatesAsync(Window mainWindow)
    {
        try
        {
            if (HttpClientFactory == null)
            {
                throw new InvalidOperationException("HttpClientFactory is not initialized. Update check cannot proceed.");
            }

            var httpClient = HttpClientFactory.CreateClient("UpdateCheckerClient");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "request");

            var response = await httpClient.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
            if (response.IsSuccessStatusCode)
            {
                DebugLogger.Log("Check for Updates Success");

                var content = await response.Content.ReadAsStringAsync();
                var (latestVersion, releasePackageAssetUrl, updaterZipAssetUrl) = ParseVersionAndAssetUrlsFromResponse(content);

                if (latestVersion == null)
                {
                    _ = LogErrors.LogErrorAsync(new InvalidDataException("Could not determine latest version from API response."), "Update Check Error");
                    MessageBoxLibrary.ErrorCheckingForUpdatesMessageBox(mainWindow);
                    return;
                }

                if (IsNewVersionAvailable(CurrentVersion, latestVersion))
                {
                    if (updaterZipAssetUrl != null)
                    {
                        await ShowUpdateWindow(updaterZipAssetUrl, releasePackageAssetUrl, CurrentVersion, latestVersion, mainWindow);
                    }
                    else
                    {
                        var expectedUpdaterFileName = $"updater_{CurrentRuntimeIdentifier}.zip";
                        var message = $"A new version ({latestVersion}) is available, but the required '{expectedUpdaterFileName}' for automatic updater update was not found. ";
                        message += releasePackageAssetUrl != null
                            ? $"You can try to download the main package '{Path.GetFileName(releasePackageAssetUrl)}' manually from the releases page."
                            : "The main release package was also not found. Please check the GitHub releases page.";

                        // Notify developer
                        _ = LogErrors.LogErrorAsync(new FileNotFoundException(message, expectedUpdaterFileName), "Update Process Info");

                        // Notify user
                        MessageBoxLibrary.InstallUpdateManuallyMessageBox(RepoOwner, RepoName);
                    }
                }
                else
                {
                    // Notify user
                    MessageBoxLibrary.ThereIsNoUpdateAvailableMessageBox(mainWindow, CurrentVersion);
                }
            }
            else
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(new HttpRequestException($"GitHub API request failed with status code {response.StatusCode}."), "Update Check Error");

                // Notify user
                MessageBoxLibrary.ErrorCheckingForUpdatesMessageBox(mainWindow);
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error checking for updates (variant).";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorCheckingForUpdatesMessageBox(mainWindow);
        }
    }

    public static async Task<(string UpdaterZipUrl, string LatestVersion)> GetLatestUpdaterInfoAsync()
    {
        try
        {
            if (HttpClientFactory == null)
            {
                throw new InvalidOperationException("HttpClientFactory is not initialized. Update check cannot proceed.");
            }

            var httpClient = HttpClientFactory.CreateClient("UpdateCheckerClient");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "request");

            var response = await httpClient.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
            if (!response.IsSuccessStatusCode)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(new HttpRequestException($"GitHub API request failed with status code {response.StatusCode}."), "Update Check Error");
                return (null, null);
            }

            var content = await response.Content.ReadAsStringAsync();
            var (latestVersion, _, updaterZipAssetUrl) = ParseVersionAndAssetUrlsFromResponse(content);
            return (updaterZipAssetUrl, latestVersion);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error fetching latest updater info.");
            return (null, null);
        }
    }

    private static async Task ShowUpdateWindow(string updaterZipUrl, string releasePackageUrl, string currentVersion, string latestVersion, Window owner)
    {
        UpdateLogWindow logWindow = null;
        var updaterLaunchedSuccessfully = false;

        try
        {
            var result = MessageBoxLibrary.DoYouWantToUpdateMessageBox(currentVersion, latestVersion, owner);
            if (result != MessageBoxResult.Yes)
            {
                return;
            }

            logWindow = new UpdateLogWindow();
            logWindow.Show();
            logWindow.Log("Starting update process...");

            if (owner != null)
            {
                owner.Hide();
            }
            else
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(new ArgumentNullException(nameof(owner), @"Owner window was null when trying to hide it during update."), "Update Process Warning");
                logWindow.Log("Main window reference is null; cannot hide it explicitly. Update will proceed.");
            }

            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;

            logWindow.Log($"Attempting to download and update the updater from URL: {updaterZipUrl}");
            try
            {
                using var memoryStream = new MemoryStream();
                await DownloadUpdateFileToMemory(updaterZipUrl, memoryStream);

                logWindow.Log($"Extracting updater to '{appDirectory}'...");
                var allFilesExtracted = ExtractAllFromZip(memoryStream, appDirectory, logWindow);

                if (allFilesExtracted)
                {
                    logWindow.Log("Updater files extracted successfully.");
                    updaterLaunchedSuccessfully = await TryExecuteUpdater(logWindow, appDirectory, true);
                }
                else
                {
                    logWindow.Log("Update of Updater.exe failed: Not all files from the updater package could be extracted.");
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, "Error processing updater package.");
                logWindow.Log($"Error downloading or extracting updater package: {ex.Message}");
            }

            if (!updaterLaunchedSuccessfully)
            {
                logWindow.Log("Failed to update and launch Updater.exe from remote zip. Attempting to launch existing local Updater.exe...");
                updaterLaunchedSuccessfully = await TryExecuteUpdater(logWindow, appDirectory, false);
            }

            if (updaterLaunchedSuccessfully)
            {
                Application.Current.Dispatcher.Invoke(QuitApplication.ForcefullyQuitApplication);
                return;
            }

            logWindow.Log("All attempts to launch Updater.exe (new or existing) have failed.");
            if (!string.IsNullOrEmpty(releasePackageUrl))
            {
                logWindow.Log($"Please download the main update package manually from: {releasePackageUrl}");
            }
            else
            {
                logWindow.Log($"The main release package URL was not found. Please visit the GitHub releases page for {RepoOwner}/{RepoName}.");
            }

            // Notify user
            MessageBoxLibrary.InstallUpdateManuallyMessageBox(RepoOwner, RepoName);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was an error preparing for the application update.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            logWindow?.Log($"Outer catch block error during update preparation: {ex.Message}");

            // Notify user
            MessageBoxLibrary.InstallUpdateManuallyMessageBox(RepoOwner, RepoName);
        }
        finally
        {
            if (!updaterLaunchedSuccessfully)
            {
                logWindow?.Close();
                owner?.Show();
            }
        }
    }

    internal static async Task<bool> TryExecuteUpdater(UpdateLogWindow logWindow, string appDirectory, bool isNewlyUpdated)
    {
        var updaterExePath = Path.Combine(appDirectory, "Updater.exe");
        var context = isNewlyUpdated ? "newly extracted" : "existing local";

        if (!File.Exists(updaterExePath))
        {
            logWindow?.Log($"Updater application ('Updater.exe') ({context}) not found in '{appDirectory}'.");
            return false;
        }

        logWindow?.Log($"Attempting to start {context} Updater.exe from: {updaterExePath}");
        await Task.Delay(500);

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = updaterExePath,
                UseShellExecute = true
            });

            logWindow?.Log($"Updater.exe ({context}) launched successfully. Simple Launcher will now exit.");
            await Task.Delay(1000);
            return true;
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, $"Failed to start {context} Updater.exe from '{updaterExePath}'.");
            logWindow?.Log($"Failed to start {context} Updater.exe. Error: {ex.Message}");
            return false;
        }
    }

    internal static async Task DownloadUpdateFileToMemory(string url, MemoryStream memoryStream)
    {
        if (HttpClientFactory == null)
        {
            throw new InvalidOperationException("HttpClientFactory is not initialized. Cannot download update file.");
        }

        var httpClient = HttpClientFactory.CreateClient("UpdateCheckerClient");
        using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        await stream.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
    }

    internal static bool ExtractAllFromZip(MemoryStream zipStream, string destinationPath, UpdateLogWindow logWindow)
    {
        try
        {
            using (var zipInputStream = new ZipInputStream(zipStream))
            {
                zipInputStream.IsStreamOwner = false;
                var hasEntries = false;
                var fullDestinationPath = Path.GetFullPath(destinationPath);

                while (zipInputStream.GetNextEntry() is { } entry)
                {
                    hasEntries = true;
                    var destinationFileFullPath = Path.GetFullPath(Path.Combine(destinationPath, entry.Name));
                    if (destinationFileFullPath.StartsWith(fullDestinationPath, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var errorMessage = $"Security Warning: Path traversal attempt detected for entry '{entry.Name}'. Aborting update.";
                    logWindow?.Log(errorMessage);

                    // Notify developer
                    _ = LogErrors.LogErrorAsync(new SecurityException("Zip Slip vulnerability detected in update package."), errorMessage);
                    return false;
                }

                if (!hasEntries)
                {
                    logWindow?.Log("Warning: The downloaded ZIP archive is empty or corrupted.");
                    return false;
                }
            }

            zipStream.Position = 0;

            var fastZip = new FastZip();
            fastZip.ExtractZip(zipStream, destinationPath, FastZip.Overwrite.Always, null, null, null, true, false);

            logWindow?.Log("All files from the updater package extracted successfully.");
            return true;
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error processing the update ZIP archive.");
            logWindow?.Log($"Failed to process the update ZIP archive. Error: {ex.Message}");

            return false;
        }
    }

    private static bool IsNewVersionAvailable(string currentVersion, string latestVersion)
    {
        try
        {
            if (string.IsNullOrEmpty(currentVersion) || string.IsNullOrEmpty(latestVersion))
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(new ArgumentException("Current or latest version string is null or empty."), "Invalid version string for comparison.");
                return false;
            }

            var currentNormalized = MyRegex1().Replace(currentVersion, "");
            var latestNormalized = MyRegex1().Replace(latestVersion, "");

            if (string.IsNullOrEmpty(currentNormalized) || string.IsNullOrEmpty(latestNormalized))
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(new ArgumentException("Normalized version string is null or empty after regex replace."), "Invalid version string after normalization.");
                return false;
            }

            var current = new Version(currentNormalized);
            var latest = new Version(latestNormalized);
            return latest.CompareTo(current) > 0;
        }
        catch (ArgumentException ex)
        {
            if (currentVersion == null) return false;

            if (latestVersion != null)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, $"Invalid version number format after normalization. Current: '{currentVersion}' (Normalized: '{MyRegex1().Replace(currentVersion, "")}'), Latest: '{latestVersion}' (Normalized: '{MyRegex1().Replace(latestVersion, "")}').");
            }

            return false;
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Unexpected error in IsNewVersionAvailable.");
            return false;
        }
    }

    private static (string version, string releasePackageUrl, string updaterZipUrl) ParseVersionAndAssetUrlsFromResponse(string jsonResponse)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            string versionTag;
            if (root.TryGetProperty("tag_name", out var tagNameElement))
            {
                versionTag = tagNameElement.GetString();
            }
            else
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(new KeyNotFoundException("'tag_name' not found in GitHub API response."), "GitHub API Response Error");
                return (null, null, null);
            }

            string rawVersionStringFromTag = null;
            string extractedNormalizedVersion = null;

            if (!string.IsNullOrEmpty(versionTag))
            {
                var versionMatch = MyRegex2().Match(versionTag);
                if (versionMatch.Success)
                {
                    rawVersionStringFromTag = versionMatch.Value;
                    extractedNormalizedVersion = NormalizeVersion(rawVersionStringFromTag);
                }
            }

            if (extractedNormalizedVersion == null)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(new FormatException($"Could not extract or normalize a valid version from tag_name: '{versionTag}'."), "GitHub API Response Error");
                return (null, null, null);
            }

            string foundReleasePackageUrl = null;
            string foundUpdaterZipUrl = null;

            var runtimeIdentifier = CurrentRuntimeIdentifier;
            var expectedReleaseFileName = $"release_{rawVersionStringFromTag}_{runtimeIdentifier}.zip";
            var expectedUpdaterFileName = $"updater_{runtimeIdentifier}.zip";

            DebugLogger.Log($"Searching for assets: '{expectedReleaseFileName}' and '{expectedUpdaterFileName}'");

            if (root.TryGetProperty("assets", out var assetsElement) && assetsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var asset in assetsElement.EnumerateArray())
                {
                    if (asset.TryGetProperty("name", out var nameElement))
                    {
                        var assetName = nameElement.GetString();
                        if (assetName?.Equals(expectedUpdaterFileName, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            if (asset.TryGetProperty("browser_download_url", out var downloadUrlElement))
                            {
                                foundUpdaterZipUrl = downloadUrlElement.GetString();
                            }
                        }
                        else if (assetName?.Equals(expectedReleaseFileName, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            if (asset.TryGetProperty("browser_download_url", out var downloadUrlElement))
                            {
                                foundReleasePackageUrl = downloadUrlElement.GetString();
                            }
                        }
                    }

                    if (foundUpdaterZipUrl != null && foundReleasePackageUrl != null) break;
                }

                if (foundUpdaterZipUrl == null)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(new FileNotFoundException($"'{expectedUpdaterFileName}' asset not found in release '{versionTag}'.", expectedUpdaterFileName), "GitHub API Asset Info");
                }

                if (foundReleasePackageUrl == null)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(new FileNotFoundException($"Expected release package '{expectedReleaseFileName}' not found in release '{versionTag}'.", expectedReleaseFileName), "GitHub API Asset Info");
                }

                return (extractedNormalizedVersion, foundReleasePackageUrl, foundUpdaterZipUrl);
            }
            else
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(new KeyNotFoundException("'assets' array not found or invalid in GitHub API response."), "GitHub API Response Error");
            }
        }
        catch (JsonException jsonEx)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(jsonEx, "Failed to parse JSON response from GitHub API.");
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Unexpected error in ParseVersionAndAssetUrlsFromResponse.");
        }

        return (null, null, null);
    }

    private static string NormalizeVersion(string version)
    {
        if (string.IsNullOrEmpty(version)) return "0.0.0.0";

        var numericVersion = MyRegex1().Replace(version, "");
        numericVersion = Regex.Replace(numericVersion, @"\.{2,}", ".").Trim('.');
        if (string.IsNullOrEmpty(numericVersion)) return "0.0.0.0";

        var parts = new List<string>(numericVersion.Split(Separator, StringSplitOptions.RemoveEmptyEntries));

        while (parts.Count < 4)
        {
            parts.Add("0");
        }

        if (parts.Count > 4)
        {
            parts = parts.GetRange(0, 4);
        }

        return string.Join(".", parts);
    }

    [GeneratedRegex(@"[^\d\.]")]
    private static partial Regex MyRegex1();

    [GeneratedRegex(@"(\d+\.\d+\.\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex MyRegex2();
}