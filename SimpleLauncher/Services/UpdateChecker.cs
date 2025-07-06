using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
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
    private const string UpdaterZipFileName = "updater.zip";
    private const string ReleasePackageNamePrefix = "release"; // "release" in "release1.2.3.4.zip"
    private const string ReleasePackageNameSuffix = ".zip";

    private static readonly IHttpClientFactory HttpClientFactory;

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

    public static async Task CheckForUpdatesAsync(Window mainWindow) // Silent check
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
                var (latestVersion, _, updaterZipAssetUrl) = ParseVersionAndAssetUrlsFromResponse(content); // releasePackageUrl not strictly needed for silent decision

                if (latestVersion == null) return;

                if (IsNewVersionAvailable(CurrentVersion, latestVersion))
                {
                    if (updaterZipAssetUrl != null) // Only proceed if updater.zip is available for automatic update
                    {
                        // For silent check, we might decide to show update window directly or queue it
                        // For now, let's assume it behaves like variant if updater.zip is found
                        // The releasePackageUrl isn't strictly necessary here for the ShowUpdateWindow call if updater.zip is primary
                        // but ShowUpdateWindow expects it for its own fallback.
                        var (_, releasePackageUrlForFallback, _) = ParseVersionAndAssetUrlsFromResponse(content); // Re-parse or pass from above
                        await ShowUpdateWindow(updaterZipAssetUrl, releasePackageUrlForFallback, CurrentVersion, latestVersion, mainWindow);
                    }
                    else
                    {
                        // Notify developer
                        _ = LogErrors.LogErrorAsync(new FileNotFoundException($"'{UpdaterZipFileName}' not found for version {latestVersion}. Automatic update of updater not possible.", UpdaterZipFileName), "Update Check Info");
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

    public static async Task CheckForUpdatesVariantAsync(Window mainWindow) // User-initiated check
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
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(new InvalidDataException("Could not determine latest version from API response."), "Update Check Error");

                    // Notify user
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
                        var message = $"A new version ({latestVersion}) is available, but the required '{UpdaterZipFileName}' for automatic updater update was not found. ";
                        message += releasePackageAssetUrl != null
                            ? $"You can try to download the main package '{Path.GetFileName(releasePackageAssetUrl)}' manually from the releases page."
                            : "The main release package was also not found. Please check the GitHub releases page.";

                        // Notify developer
                        _ = LogErrors.LogErrorAsync(new FileNotFoundException(message, UpdaterZipFileName), "Update Process Info");

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

    private static async Task ShowUpdateWindow(string updaterZipUrl, string releasePackageUrl, string currentVersion, string latestVersion, Window owner)
    {
        UpdateLogWindow logWindow = null;
        try
        {
            var result = MessageBoxLibrary.DoYouWantToUpdateMessageBox(currentVersion, latestVersion, owner);
            if (result != MessageBoxResult.Yes) return;

            logWindow = new UpdateLogWindow();
            logWindow.Show();
            logWindow.Log("Starting update process...");

            if (owner == null)
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(new ArgumentNullException(nameof(owner), @"Owner window was null when trying to close it during update."), "Update Process Warning");

                // Notify user
                logWindow.Log("Main window reference is null; cannot close it explicitly. Attempting global shutdown later.");
            }
            else
            {
                owner.Close();
            }

            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var updaterLaunchedSuccessfully = false;

            // Attempt 1: Download, extract, and run new Updater.exe
            logWindow.Log($"Attempting to download and update Updater.exe using '{UpdaterZipFileName}' from URL: {updaterZipUrl}");
            try
            {
                await Task.Run(async () => // Offload download and extraction to a background thread
                {
                    using var memoryStream = new MemoryStream();
                    await DownloadUpdateFileToMemory(updaterZipUrl, memoryStream);

                    logWindow.Log($"Extracting contents of '{UpdaterZipFileName}' to '{appDirectory}'...");
                    var allFilesExtracted = ExtractAllFromZip(memoryStream, appDirectory, logWindow);

                    if (allFilesExtracted)
                    {
                        logWindow.Log("Updater files from ZIP extracted successfully.");
                        await Task.Delay(1000); // Brief pause
                        updaterLaunchedSuccessfully = await TryExecuteUpdater(logWindow, appDirectory, true);
                    }
                    else
                    {
                        logWindow.Log($"Update of Updater.exe failed: Not all files from '{UpdaterZipFileName}' could be extracted.");
                        // updaterLaunchedSuccessfully remains false, will fall through to next attempt
                    }
                });
            }
            catch (Exception ex) // Catches exceptions from downloading/extracting updater.zip
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(ex, $"Error processing '{UpdaterZipFileName}'.");

                // Notify user
                logWindow.Log($"Error downloading or extracting '{UpdaterZipFileName}': {ex.Message}");
                // updaterLaunchedSuccessfully remains false
            }

            // Attempt 2: Run existing Updater.exe if the first attempt failed
            if (!updaterLaunchedSuccessfully)
            {
                // Notify user
                logWindow.Log("Failed to update and launch Updater.exe from remote zip. Attempting to launch existing local Updater.exe...");
                updaterLaunchedSuccessfully = await TryExecuteUpdater(logWindow, appDirectory, false);
            }

            // Fallback: Manual download if all attempts to launch Updater.exe failed
            if (!updaterLaunchedSuccessfully)
            {
                // Notify user
                logWindow.Log("All attempts to launch Updater.exe (new or existing) have failed.");
                if (!string.IsNullOrEmpty(releasePackageUrl))
                {
                    // Notify user
                    logWindow.Log($"Please download the main update package manually from: {releasePackageUrl}");
                }
                else
                {
                    // Notify user
                    logWindow.Log($"The main release package URL was not found. Please visit the GitHub releases page for {RepoOwner}/{RepoName}.");
                }

                // Notify user
                MessageBoxLibrary.InstallUpdateManuallyMessageBox(RepoOwner, RepoName); // Consider if this needs the releasePackageUrl
            }
            // If updaterLaunchedSuccessfully is true, the application would have been quit by TryExecuteUpdater.
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "There was an error preparing for the application update.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            if (logWindow != null)
            {
                logWindow.Log($"Outer catch block error during update preparation: {ex.Message}");
            }

            // Notify user
            MessageBoxLibrary.InstallUpdateManuallyMessageBox(RepoOwner, RepoName);
        }
    }

    private static async Task<bool> TryExecuteUpdater(UpdateLogWindow logWindow, string appDirectory, bool isNewlyUpdated)
    {
        var updaterExePath = Path.Combine(appDirectory, "Updater.exe");
        var context = isNewlyUpdated ? "newly extracted" : "existing local";

        if (!File.Exists(updaterExePath))
        {
            logWindow.Log($"Updater application ('Updater.exe') ({context}) not found in '{appDirectory}'.");
            return false;
        }

        logWindow.Log($"Attempting to start {context} Updater.exe from: {updaterExePath}");
        await Task.Delay(500); // Short delay for log visibility

        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = updaterExePath,
                UseShellExecute = true
            });

            logWindow.Log($"Updater.exe ({context}) launched successfully. Closing Simple Launcher...");
            await Task.Delay(1000); // Give log a moment to be seen and updater to initialize

            Application.Current.Dispatcher.Invoke(static () =>
            {
                QuitApplication.ForcefullyQuitApplication();
            });
            return true; // Successfully launched and initiated application quit
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, $"Failed to start {context} Updater.exe from '{updaterExePath}'.");
            logWindow.Log($"Failed to start {context} Updater.exe. Error: {ex.Message}");

            return false;
        }
    }

    private static async Task DownloadUpdateFileToMemory(string url, MemoryStream memoryStream)
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

    private static bool ExtractAllFromZip(MemoryStream zipStream, string destinationPath, UpdateLogWindow logWindow)
    {
        try
        {
            // First, validate all entry paths to prevent Zip Slip
            using (var zipInputStream = new ZipInputStream(zipStream))
            {
                zipInputStream.IsStreamOwner = false; // We will reset and reuse the stream
                var hasEntries = false;
                var fullDestinationPath = Path.GetFullPath(destinationPath);

                while (zipInputStream.GetNextEntry() is { } entry)
                {
                    hasEntries = true;
                    var destinationFileFullPath = Path.GetFullPath(Path.Combine(destinationPath, entry.Name));
                    if (destinationFileFullPath.StartsWith(fullDestinationPath, StringComparison.OrdinalIgnoreCase))
                        continue;

                    var errorMessage = $"Security Warning: Path traversal attempt detected for entry '{entry.Name}'. Aborting update.";
                    logWindow.Log(errorMessage);

                    // Notify developer
                    _ = LogErrors.LogErrorAsync(new SecurityException("Zip Slip vulnerability detected in update package."), errorMessage);

                    return false; // Abort on security risk
                }

                if (!hasEntries)
                {
                    logWindow.Log("Warning: The downloaded ZIP archive is empty or corrupted.");
                    return false;
                }
            }

            // If validation passes, reset stream and extract
            zipStream.Position = 0;

            var fastZip = new FastZip();
            fastZip.ExtractZip(zipStream, destinationPath, FastZip.Overwrite.Always, null, null, null, true, false);

            logWindow.Log($"All files from '{UpdaterZipFileName}' extracted successfully.");
            return true;
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Error processing the update ZIP archive.");
            logWindow.Log($"Failed to process the update ZIP archive. Error: {ex.Message}");

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

            string rawVersionStringFromTag = null; // Will store version like "4.0.1" from tag "release4.0.1"
            string extractedNormalizedVersion = null; // Will store normalized version like "4.0.1.0"

            if (!string.IsNullOrEmpty(versionTag))
            {
                var versionMatch = MyRegex2().Match(versionTag); // MyRegex2 extracts version like "X.Y.Z.W"
                if (versionMatch.Success)
                {
                    rawVersionStringFromTag = versionMatch.Value; // Capture the version as it is in the tag (e.g., "4.0.1")
                    extractedNormalizedVersion = NormalizeVersion(rawVersionStringFromTag); // Normalize for comparison and return (e.g., "4.0.1.0")
                }
            }

            if (extractedNormalizedVersion == null) // Check if a version was successfully extracted and normalized
            {
                // Notify developer
                _ = LogErrors.LogErrorAsync(new FormatException($"Could not extract or normalize a valid version from tag_name: '{versionTag}'."), "GitHub API Response Error");

                return (null, null, null);
            }

            string foundReleasePackageUrl = null;
            string foundUpdaterZipUrl = null;

            // Construct the expected release file name using the RAW version string from the tag, not the normalized one.
            var expectedReleaseFileName = $"{ReleasePackageNamePrefix}{rawVersionStringFromTag}{ReleasePackageNameSuffix}";

            if (root.TryGetProperty("assets", out var assetsElement) && assetsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var asset in assetsElement.EnumerateArray())
                {
                    if (asset.TryGetProperty("name", out var nameElement))
                    {
                        var assetName = nameElement.GetString();
                        if (assetName?.Equals(UpdaterZipFileName, StringComparison.OrdinalIgnoreCase) == true)
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

                    // if both found, no need to iterate further
                    if (foundUpdaterZipUrl != null && foundReleasePackageUrl != null) break;
                }

                if (foundUpdaterZipUrl == null)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(new FileNotFoundException($"'{UpdaterZipFileName}' asset not found in release '{versionTag}'.", UpdaterZipFileName), "GitHub API Asset Info");
                }

                if (foundReleasePackageUrl == null)
                {
                    // Notify developer
                    _ = LogErrors.LogErrorAsync(new FileNotFoundException($"Expected release package '{expectedReleaseFileName}' not found in release '{versionTag}'.", expectedReleaseFileName), "GitHub API Asset Info");
                }

                // Return the 4-part normalized version for consistency in comparisons and display
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

        // Ensure exactly 4 parts, truncating if more
        if (parts.Count > 4)
        {
            parts = parts.GetRange(0, 4);
        }

        return string.Join(".", parts);
    }

    [GeneratedRegex(@"[^\d\.]")] // Removes anything not a digit or a dot
    private static partial Regex MyRegex1();

    // Regex to extract version like "1.2.3" or "1.2.3.4" from a string like "release-1.2.3.4" or "v1.2.3"
    // This regex assumes the version number is what we want to normalize.
    // (?<=release(?:-[a-zA-Z0-9]+)?-?) : Positive lookbehind for "release", optional alphanumeric part, optional hyphen
    // \d+(\.\d+)* : Matches one or more digits, followed by zero or more groups of (a dot and one or more digits)
    [GeneratedRegex(@"(?<=release(?:-[a-zA-Z0-9]+)?-?)\d+(\.\d+){0,3}", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex MyRegex2(); // Capture typical release tag versions up to 4 parts
}