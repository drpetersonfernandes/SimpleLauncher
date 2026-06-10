using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using SharpCompress.Archives.Zip;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.QuitOrReinstall;
using CoreMessageBoxResult = SimpleLauncher.Interfaces.MessageBoxResult;

namespace SimpleLauncher.Services.CheckForUpdates;

public partial class UpdateChecker
{
    private const string RepoOwner = "drpetersonfernandes";
    private const string RepoName = "SimpleLauncher";
    private readonly HttpClient _httpClient;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBoxLibrary;
    private readonly IResourceProvider _resourceProvider;
    private readonly IDebugLogger _debugLogger;
    private readonly QuitSimpleLauncher _quitSimpleLauncher;
    private readonly IServiceProvider _serviceProvider;

    private static string CurrentRuntimeIdentifier
    {
        get
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
            return arch switch
            {
                Architecture.Arm64 => "win-arm64",
                Architecture.X64 => "win-x64",
                _ => throw new NotSupportedException($"Unsupported runtime architecture '{arch}'. Only win-x64 and win-arm64 are supported.")
            };
        }
    }

    public UpdateChecker(IHttpClientFactory httpClientFactory, ILogErrors logErrors, IMessageBoxLibraryService messageBoxLibrary, IResourceProvider resourceProvider, IDebugLogger debugLogger, QuitSimpleLauncher quitSimpleLauncher, IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        _httpClient = httpClientFactory.CreateClient("UpdateCheckerClient");
        _logErrors = logErrors;
        _messageBoxLibrary = messageBoxLibrary;
        _resourceProvider = resourceProvider;
        _debugLogger = debugLogger;
        _quitSimpleLauncher = quitSimpleLauncher;
        _serviceProvider = serviceProvider;
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
                _logErrors.LogAndForget(ex, "Error getting CurrentVersion.");

                return _resourceProvider.GetString("UnknownString", "Unknown");
            }
        }
    }

    private static readonly char[] Separator = ['.'];

    internal async Task SilentCheckForUpdatesAsync(Window mainWindow)
    {
        try
        {
            if (_httpClient == null)
            {
                throw new InvalidOperationException("HttpClientFactory is not initialized. Update check cannot proceed.");
            }

            // Use the pre-initialized HttpClient instance
            if (_httpClient != null)
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "request");

                var response = await _httpClient.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
                if (response.IsSuccessStatusCode)
                {
                    _debugLogger.Log("Check for Updates Success");

                    var content = await response.Content.ReadAsStringAsync();
                    var (latestVersion, _, updaterZipAssetUrl) = ParseVersionAndAssetUrlsFromResponse(content);

                    if (latestVersion == null) return;

                    if (IsNewVersionAvailable(CurrentVersion, latestVersion))
                    {
                        if (updaterZipAssetUrl != null)
                        {
                            var (_, releasePackageUrlForFallback, _) = ParseVersionAndAssetUrlsFromResponse(content);
                            await ShowUpdateWindowAsync(releasePackageUrlForFallback, CurrentVersion, latestVersion, mainWindow);
                        }
                        else
                        {
                            // Notify developer
                            var expectedUpdaterFileName = $"updater_{CurrentRuntimeIdentifier}.zip";
                            _logErrors.LogAndForget(new FileNotFoundException($"'{expectedUpdaterFileName}' not found for version {latestVersion}. Automatic update of updater not possible.", expectedUpdaterFileName), "Update Check Info");
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error checking for updates (silent).";
            _logErrors.LogAndForget(ex, contextMessage);
        }
    }

    internal async Task ManualCheckForUpdatesAsync(Window mainWindow)
    {
        try
        {
            if (_httpClient == null)
            {
                throw new InvalidOperationException("HttpClientFactory is not initialized. Update check cannot proceed.");
            }

            // Use the pre-initialized HttpClient instance
            if (_httpClient != null)
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "request");

                var response = await _httpClient.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
                if (response.IsSuccessStatusCode)
                {
                    _debugLogger.Log("Check for Updates Success");

                    var content = await response.Content.ReadAsStringAsync();
                    var (latestVersion, releasePackageAssetUrl, updaterZipAssetUrl) = ParseVersionAndAssetUrlsFromResponse(content);

                    if (latestVersion == null)
                    {
                        _logErrors.LogAndForget(new InvalidDataException("Could not determine latest version from API response."), "Update Check Error");
                        await _messageBoxLibrary.ErrorCheckingForUpdatesMessageBox();
                        return;
                    }

                    if (IsNewVersionAvailable(CurrentVersion, latestVersion))
                    {
                        if (updaterZipAssetUrl != null)
                        {
                            await ShowUpdateWindowAsync(releasePackageAssetUrl, CurrentVersion, latestVersion, mainWindow);
                        }
                        else
                        {
                            var expectedUpdaterFileName = $"updater_{CurrentRuntimeIdentifier}.zip";
                            var message = $"A new version ({latestVersion}) is available, but the required '{expectedUpdaterFileName}' for automatic updater update was not found. ";
                            message += releasePackageAssetUrl != null
                                ? $"You can try to download the main package '{Path.GetFileName(releasePackageAssetUrl)}' manually from the releases page."
                                : "The main release package was also not found. Please check the GitHub releases page.";

                            // Notify developer
                            _logErrors.LogAndForget(new FileNotFoundException(message, expectedUpdaterFileName), "Update Process Info");

                            // Notify user
                            await _messageBoxLibrary.InstallUpdateManuallyMessageBox();
                        }
                    }
                    else
                    {
                        // Notify user
                        await _messageBoxLibrary.ThereIsNoUpdateAvailableMessageBox(CurrentVersion);
                    }
                }
                else
                {
                    // Notify developer
                    _logErrors.LogAndForget(new HttpRequestException($"GitHub API request failed with status code {response.StatusCode}."), "Update Check Error");

                    // Notify user
                    await _messageBoxLibrary.ErrorCheckingForUpdatesMessageBox();
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error checking for updates (variant).";
            _logErrors.LogAndForget(ex, contextMessage);

            // Notify user
            await _messageBoxLibrary.ErrorCheckingForUpdatesMessageBox();
        }
    }

    internal async Task<(string UpdaterZipUrl, string LatestVersion)> GetLatestUpdaterInfoAsync()
    {
        try
        {
            if (_httpClient == null)
            {
                throw new InvalidOperationException("HttpClientFactory is not initialized. Update check cannot proceed.");
            }

            // Use the pre-initialized HttpClient instance
            if (_httpClient != null)
            {
                _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "request");

                var response = await _httpClient.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
                if (!response.IsSuccessStatusCode)
                {
                    // Notify developer
                    _logErrors.LogAndForget(new HttpRequestException($"GitHub API request failed with status code {response.StatusCode}."), "Update Check Error");
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
            _logErrors.LogAndForget(ex, "Error fetching latest updater info.");
            return (null, null);
        }
    }

    private async Task ShowUpdateWindowAsync(string releasePackageUrl, string currentVersion, string latestVersion, Window owner)
    {
        UpdateLogWindow logWindow = null;

        try
        {
            var result = await _messageBoxLibrary.DoYouWantToUpdateMessageBox(currentVersion, latestVersion);
            if (result != CoreMessageBoxResult.Yes)
            {
                return;
            }

            logWindow = _serviceProvider.GetRequiredService<UpdateLogWindow>();
            logWindow.Show();
            logWindow.Log("Starting update process...");

            owner?.Hide();

            var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var updaterExePath = Path.Combine(appDirectory, "Updater.exe");

            logWindow.Log("Launching Updater.exe (auto-downloads from GitHub if needed)...");
            await Task.Delay(500);
            await _quitSimpleLauncher.ShutdownForUpdateAsync(updaterExePath, _messageBoxLibrary);
            // If we reach here, ShutdownForUpdateAsync returned without killing the process
            // (the update failed — an error was already shown to the user)
            logWindow.Log("Updater.exe launch failed.");
            if (!string.IsNullOrEmpty(releasePackageUrl))
            {
                logWindow.Log($"Please download the update package manually from: {releasePackageUrl}");
            }
            else
            {
                logWindow.Log($"The update package URL was not found. Please visit the GitHub releases page for {RepoOwner}/{RepoName}.");
            }
        }
        catch (Exception ex)
        {
            const string contextMessage = "There was an error preparing for the application update.";
            _logErrors.LogAndForget(ex, contextMessage);
            logWindow?.Log($"An unexpected error occurred during the update process: {ex.Message}");
            await _messageBoxLibrary.InstallUpdateManuallyMessageBox();
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
        if (_httpClient == null)
        {
            throw new InvalidOperationException("HttpClientFactory is not initialized. Cannot download update file.");
        }

        // Use the pre-initialized HttpClient instance
        if (_httpClient != null)
        {
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            await using var stream = await response.Content.ReadAsStreamAsync();
            await stream.CopyToAsync(memoryStream);
        }

        memoryStream.Position = 0;
    }

    internal static bool ExtractAllFromZip(MemoryStream zipStream, string destinationPath, UpdateLogWindow logWindow, ILogErrors logErrors)
    {
        try
        {
            zipStream.Position = 0;

            // Ensure destination directory exists
            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            var fullDestinationPath = Path.GetFullPath(destinationPath);
            if (!fullDestinationPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
            {
                fullDestinationPath += Path.DirectorySeparatorChar;
            }

            using var archive = ZipArchive.OpenArchive(zipStream);
            var hasEntries = false;

            foreach (var entry in archive.Entries)
            {
                hasEntries = true;

                if (entry.IsDirectory)
                {
                    continue;
                }

                // Security check: prevent path traversal attacks (zip slip)
                if (entry.Key != null)
                {
                    var destinationFileFullPath = Path.GetFullPath(Path.Combine(fullDestinationPath, entry.Key));
                    if (!destinationFileFullPath.StartsWith(fullDestinationPath, StringComparison.OrdinalIgnoreCase))
                    {
                        var errorMessage = $"Security Warning: Path traversal attempt detected for entry '{entry.Key}'. Aborting update.";
                        logWindow?.Log(errorMessage);

                        // Notify developer
                        logErrors.LogAndForget(new SecurityException("Zip Slip vulnerability detected in update package."), errorMessage);
                        return false;
                    }

                    // Ensure the directory exists
                    var entryDirectory = Path.GetDirectoryName(destinationFileFullPath);
                    if (!string.IsNullOrEmpty(entryDirectory) && !Directory.Exists(entryDirectory))
                    {
                        Directory.CreateDirectory(entryDirectory);
                    }

                    // Extract the entry
                    using (var entryStream = entry.OpenEntryStream())
                    using (var fileStream = File.Create(destinationFileFullPath))
                    {
                        entryStream.CopyTo(fileStream);
                    }

                    // Preserve file time if available
                    if (entry.LastModifiedTime.HasValue)
                    {
                        File.SetLastWriteTime(destinationFileFullPath, entry.LastModifiedTime.Value);
                    }
                }
            }

            if (!hasEntries)
            {
                logWindow?.Log("Warning: The downloaded ZIP archive is empty or corrupted.");
                return false;
            }

            logWindow?.Log("All files from the updater package extracted successfully.");
            return true;
        }
        catch (Exception ex)
        {
            // Notify developer
            logErrors.LogAndForget(ex, "Error processing the update ZIP archive.");
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
                _logErrors.LogAndForget(new ArgumentException("Current or latest version string is null or empty."), "Invalid version string for comparison.");
                return false;
            }

            var currentNormalized = MyRegex1().Replace(currentVersion, "");
            var latestNormalized = MyRegex1().Replace(latestVersion, "");

            if (string.IsNullOrEmpty(currentNormalized) || string.IsNullOrEmpty(latestNormalized))
            {
                // Notify developer
                _logErrors.LogAndForget(new ArgumentException("Normalized version string is null or empty after regex replace."), "Invalid version string after normalization.");
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
                _logErrors.LogAndForget(ex, $"Invalid version number format after normalization. Current: '{currentVersion}' (Normalized: '{MyRegex1().Replace(currentVersion, "")}'), Latest: '{latestVersion}' (Normalized: '{MyRegex1().Replace(latestVersion, "")}').");
            }

            return false;
        }
        catch (Exception ex)
        {
            // Notify developer
            _logErrors.LogAndForget(ex, "Unexpected error in IsNewVersionAvailable.");
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
                _logErrors.LogAndForget(new KeyNotFoundException("'tag_name' not found in GitHub API response."), "GitHub API Response Error");
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
                _logErrors.LogAndForget(new FormatException($"Could not extract or normalize a valid version from tag_name: '{versionTag}'."), "GitHub API Response Error");
                return (null, null, null);
            }

            string foundReleasePackageUrl = null;
            string foundUpdaterZipUrl = null;

            var runtimeIdentifier = CurrentRuntimeIdentifier;
            var expectedReleaseFileName = $"release_{rawVersionStringFromTag}_{runtimeIdentifier}.zip";
            var expectedUpdaterFileName = $"updater_{runtimeIdentifier}.zip";

            _debugLogger.Log($"Searching for assets: '{expectedReleaseFileName}' and '{expectedUpdaterFileName}'");

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
                    _logErrors.LogAndForget(new FileNotFoundException($"'{expectedUpdaterFileName}' asset not found in release '{versionTag}'.", expectedUpdaterFileName), "GitHub API Asset Info");
                }

                if (foundReleasePackageUrl == null)
                {
                    // Notify developer
                    _logErrors.LogAndForget(new FileNotFoundException($"Expected release package '{expectedReleaseFileName}' not found in release '{versionTag}'.", expectedReleaseFileName), "GitHub API Asset Info");
                }

                return (extractedNormalizedVersion, foundReleasePackageUrl, foundUpdaterZipUrl);
            }
            else
            {
                // Notify developer
                _logErrors.LogAndForget(new KeyNotFoundException("'assets' array not found or invalid in GitHub API response."), "GitHub API Response Error");
            }
        }
        catch (JsonException jsonEx)
        {
            // Notify developer
            _logErrors.LogAndForget(jsonEx, "Failed to parse JSON response from GitHub API.");
        }
        catch (Exception ex)
        {
            // Notify developer
            _logErrors.LogAndForget(ex, "Unexpected error in ParseVersionAndAssetUrlsFromResponse.");
        }

        return (null, null, null);
    }

    private static string NormalizeVersion(string version)
    {
        if (string.IsNullOrEmpty(version)) return "0.0.0.0";

        var numericVersion = MyRegex1().Replace(version, "");
        numericVersion = MyRegex().Replace(numericVersion, ".").Trim('.');
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

    [GeneratedRegex(@"\.{2,}")]
    private static partial Regex MyRegex();
}