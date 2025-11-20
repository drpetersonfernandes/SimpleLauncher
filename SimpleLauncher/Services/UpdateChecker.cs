using System;
using System.Collections.Generic;
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

namespace SimpleLauncher.Services;

public partial class UpdateChecker
{
    private const string RepoOwner = "drpetersonfernandes";
    private const string RepoName = "SimpleLauncher";
    private readonly IHttpClientFactory _httpClientFactory;

    private string CurrentRuntimeIdentifier
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

    public UpdateChecker(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
    }

    private string CurrentVersion
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
                _ = LogErrorsService.LogErrorAsync(ex, "Error getting CurrentVersion.");
                var unknown = (string)Application.Current.TryFindResource("Unknown") ?? "Unknown";

                return unknown;
            }
        }
    }

    private static readonly char[] Separator = { '.' };

    public async Task SilentCheckForUpdatesAsync(Window mainWindow)
    {
        try
        {
            if (_httpClientFactory == null)
            {
                throw new InvalidOperationException("HttpClientFactory is not initialized. Update check cannot proceed.");
            }

            var httpClient = _httpClientFactory?.CreateClient("UpdateCheckerClient");
            if (httpClient != null)
            {
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
                            await ShowUpdateWindowAsync(updaterZipAssetUrl, releasePackageUrlForFallback, CurrentVersion, latestVersion, mainWindow);
                        }
                        else
                        {
                            // Notify developer
                            var expectedUpdaterFileName = $"updater_{CurrentRuntimeIdentifier}.zip";
                            _ = LogErrorsService.LogErrorAsync(new FileNotFoundException($"'{expectedUpdaterFileName}' not found for version {latestVersion}. Automatic update of updater not possible.", expectedUpdaterFileName), "Update Check Info");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error checking for updates (silent).";
            _ = LogErrorsService.LogErrorAsync(ex, contextMessage);
        }
    }

    public async Task ManualCheckForUpdatesAsync(Window mainWindow)
    {
        try
        {
            if (_httpClientFactory == null)
            {
                throw new InvalidOperationException("HttpClientFactory is not initialized. Update check cannot proceed.");
            }

            var httpClient = _httpClientFactory?.CreateClient("UpdateCheckerClient");
            if (httpClient != null)
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "request");

                var response = await httpClient.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
                if (response.IsSuccessStatusCode)
                {
                    DebugLogger.Log("Check for Updates Success");

                    var content = await response.Content.ReadAsStringAsync();
                    var (latestVersion, releasePackageAssetUrl, updaterZipAssetUrl) = ParseVersionAndAssetUrlsFromResponse(content);

                    if (latestVersion == null)
                    {
                        _ = LogErrorsService.LogErrorAsync(new InvalidDataException("Could not determine latest version from API response."), "Update Check Error");
                        MessageBoxLibrary.ErrorCheckingForUpdatesMessageBox(mainWindow);
                        return;
                    }

                    if (IsNewVersionAvailable(CurrentVersion, latestVersion))
                    {
                        if (updaterZipAssetUrl != null)
                        {
                            await ShowUpdateWindowAsync(updaterZipAssetUrl, releasePackageAssetUrl, CurrentVersion, latestVersion, mainWindow);
                        }
                        else
                        {
                            var expectedUpdaterFileName = $"updater_{CurrentRuntimeIdentifier}.zip";
                            var message = $"A new version ({latestVersion}) is available, but the required '{expectedUpdaterFileName}' for automatic updater update was not found. ";
                            message += releasePackageAssetUrl != null
                                ? $"You can try to download the main package '{Path.GetFileName(releasePackageAssetUrl)}' manually from the releases page."
                                : "The main release package was also not found. Please check the GitHub releases page.";

                            // Notify developer
                            _ = LogErrorsService.LogErrorAsync(new FileNotFoundException(message, expectedUpdaterFileName), "Update Process Info");

                            // Notify user
                            MessageBoxLibrary.InstallUpdateManuallyMessageBox();
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
                    _ = LogErrorsService.LogErrorAsync(new HttpRequestException($"GitHub API request failed with status code {response.StatusCode}."), "Update Check Error");

                    // Notify user
                    MessageBoxLibrary.ErrorCheckingForUpdatesMessageBox(mainWindow);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error checking for updates (variant).";
            _ = LogErrorsService.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorCheckingForUpdatesMessageBox(mainWindow);
        }
    }

    public async Task<(string UpdaterZipUrl, string LatestVersion)> GetLatestUpdaterInfoAsync()
    {
        try
        {
            if (_httpClientFactory == null)
            {
                throw new InvalidOperationException("HttpClientFactory is not initialized. Update check cannot proceed.");
            }

            var httpClient = _httpClientFactory?.CreateClient("UpdateCheckerClient");
            if (httpClient != null)
            {
                httpClient.DefaultRequestHeaders.Add("User-Agent", "request");

                var response = await httpClient.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
                if (!response.IsSuccessStatusCode)
                {
                    // Notify developer
                    _ = LogErrorsService.LogErrorAsync(new HttpRequestException($"GitHub API request failed with status code {response.StatusCode}."), "Update Check Error");
                    return (null, null);
                }

                var content = await response.Content.ReadAsStringAsync();
                var (latestVersion, _, updaterZipAssetUrl) = ParseVersionAndAssetUrlsFromResponse(content);
                return (updaterZipAssetUrl, latestVersion);
            }

            return (null, null);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrorsService.LogErrorAsync(ex, "Error fetching latest updater info.");
            return (null, null);
        }
    }

    private async Task ShowUpdateWindowAsync(string updaterZipUrl, string releasePackageUrl, string currentVersion, string latestVersion, Window owner)
    {
        UpdateLogWindow logWindow = null;

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

            owner?.Hide();

            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var updaterExePath = Path.Combine(appDirectory, "Updater.exe");
            var updaterReady = false;

            logWindow.Log($"Attempting to download and update the updater from URL: {updaterZipUrl}");
            try
            {
                using var memoryStream = new MemoryStream();
                await DownloadUpdateFileToMemoryAsync(updaterZipUrl, memoryStream);

                logWindow.Log($"Extracting updater to '{appDirectory}'...");
                if (ExtractAllFromZip(memoryStream, appDirectory, logWindow))
                {
                    logWindow.Log("Updater files extracted successfully.");
                    updaterReady = true;
                }
                else
                {
                    logWindow.Log("Update of Updater.exe failed: Not all files from the updater package could be extracted.");
                }
            }
            catch (Exception ex)
            {
                // Notify developer
                _ = LogErrorsService.LogErrorAsync(ex, "Error processing updater package.");
                logWindow.Log($"Error downloading or extracting updater package: {ex.Message}");
            }

            // If the new updater is ready, use it.
            if (updaterReady && File.Exists(updaterExePath))
            {
                logWindow.Log("Launching newly updated Updater.exe...");
                await Task.Delay(500); // Brief delay for UI to update
                QuitApplication.ShutdownForUpdate(updaterExePath);
                return; // The application will be terminated by ShutdownForUpdate.
            }

            // If downloading/extracting the new updater failed, try to use the existing one.
            logWindow.Log("Failed to update Updater.exe from remote zip. Attempting to launch existing local Updater.exe...");
            if (File.Exists(updaterExePath))
            {
                logWindow.Log("Launching existing Updater.exe...");
                await Task.Delay(500);
                QuitApplication.ShutdownForUpdate(updaterExePath);
                return; // The application will be terminated by ShutdownForUpdate.
            }

            // If we reach here, all attempts to get and launch the updater have failed.
            logWindow.Log("All attempts to launch Updater.exe (new or existing) have failed.");
            if (!string.IsNullOrEmpty(releasePackageUrl))
            {
                logWindow.Log($"Please download the main update package manually from: {releasePackageUrl}");
            }
            else
            {
                logWindow.Log($"The main release package URL was not found. Please visit the GitHub releases page for {RepoOwner}/{RepoName}.");
            }

            MessageBoxLibrary.InstallUpdateManuallyMessageBox();
        }
        catch (Exception ex)
        {
            const string contextMessage = "There was an error preparing for the application update.";
            _ = LogErrorsService.LogErrorAsync(ex, contextMessage);
            logWindow?.Log($"An unexpected error occurred during the update process: {ex.Message}");
            MessageBoxLibrary.InstallUpdateManuallyMessageBox();
        }
        finally
        {
            // This finally block will now only be reached if the update process fails before shutdown.
            logWindow?.Close();
            owner?.Show();
        }
    }

    internal async Task DownloadUpdateFileToMemoryAsync(string url, MemoryStream memoryStream)
    {
        if (_httpClientFactory == null)
        {
            throw new InvalidOperationException("HttpClientFactory is not initialized. Cannot download update file.");
        }

        var httpClient = _httpClientFactory?.CreateClient("UpdateCheckerClient");
        if (httpClient != null)
        {
            using var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync();
            await stream.CopyToAsync(memoryStream);
        }

        memoryStream.Position = 0;
    }

    internal bool ExtractAllFromZip(MemoryStream zipStream, string destinationPath, UpdateLogWindow logWindow)
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
                    _ = LogErrorsService.LogErrorAsync(new SecurityException("Zip Slip vulnerability detected in update package."), errorMessage);
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
            _ = LogErrorsService.LogErrorAsync(ex, "Error processing the update ZIP archive.");
            logWindow?.Log($"Failed to process the update ZIP archive. Error: {ex.Message}");

            return false;
        }
    }

    private bool IsNewVersionAvailable(string currentVersion, string latestVersion)
    {
        try
        {
            if (string.IsNullOrEmpty(currentVersion) || string.IsNullOrEmpty(latestVersion))
            {
                // Notify developer
                _ = LogErrorsService.LogErrorAsync(new ArgumentException("Current or latest version string is null or empty."), "Invalid version string for comparison.");
                return false;
            }

            var currentNormalized = MyRegex1().Replace(currentVersion, "");
            var latestNormalized = MyRegex1().Replace(latestVersion, "");

            if (string.IsNullOrEmpty(currentNormalized) || string.IsNullOrEmpty(latestNormalized))
            {
                // Notify developer
                _ = LogErrorsService.LogErrorAsync(new ArgumentException("Normalized version string is null or empty after regex replace."), "Invalid version string after normalization.");
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
                _ = LogErrorsService.LogErrorAsync(ex, $"Invalid version number format after normalization. Current: '{currentVersion}' (Normalized: '{MyRegex1().Replace(currentVersion, "")}'), Latest: '{latestVersion}' (Normalized: '{MyRegex1().Replace(latestVersion, "")}').");
            }

            return false;
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrorsService.LogErrorAsync(ex, "Unexpected error in IsNewVersionAvailable.");
            return false;
        }
    }

    private (string version, string releasePackageUrl, string updaterZipUrl) ParseVersionAndAssetUrlsFromResponse(string jsonResponse)
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
                _ = LogErrorsService.LogErrorAsync(new KeyNotFoundException("'tag_name' not found in GitHub API response."), "GitHub API Response Error");
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
                _ = LogErrorsService.LogErrorAsync(new FormatException($"Could not extract or normalize a valid version from tag_name: '{versionTag}'."), "GitHub API Response Error");
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
                    _ = LogErrorsService.LogErrorAsync(new FileNotFoundException($"'{expectedUpdaterFileName}' asset not found in release '{versionTag}'.", expectedUpdaterFileName), "GitHub API Asset Info");
                }

                if (foundReleasePackageUrl == null)
                {
                    // Notify developer
                    _ = LogErrorsService.LogErrorAsync(new FileNotFoundException($"Expected release package '{expectedReleaseFileName}' not found in release '{versionTag}'.", expectedReleaseFileName), "GitHub API Asset Info");
                }

                return (extractedNormalizedVersion, foundReleasePackageUrl, foundUpdaterZipUrl);
            }
            else
            {
                // Notify developer
                _ = LogErrorsService.LogErrorAsync(new KeyNotFoundException("'assets' array not found or invalid in GitHub API response."), "GitHub API Response Error");
            }
        }
        catch (JsonException jsonEx)
        {
            // Notify developer
            _ = LogErrorsService.LogErrorAsync(jsonEx, "Failed to parse JSON response from GitHub API.");
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrorsService.LogErrorAsync(ex, "Unexpected error in ParseVersionAndAssetUrlsFromResponse.");
        }

        return (null, null, null);
    }

    private string NormalizeVersion(string version)
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

    [GeneratedRegex(@"(\d+(\.\d+){1,3})", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
    private static partial Regex MyRegex2();
}