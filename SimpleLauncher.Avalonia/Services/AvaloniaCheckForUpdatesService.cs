using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using Avalonia.Controls;
using SimpleLauncher.Core.Interfaces;
using CoreMessageBoxResult = SimpleLauncher.Core.Models.MessageBoxResult;

namespace SimpleLauncher.Avalonia.Services;

/// <summary>
/// Checks for new application releases on GitHub, falling back to the secondary
/// server when GitHub is unreachable. Avalonia port of the WPF CheckForUpdatesService.
/// The automatic updater (Updater.exe) is not shipped with the Avalonia port yet, so
/// users are guided to the releases page when an update is available.
/// </summary>
public partial class AvaloniaCheckForUpdatesService
{
    private const string RepoName = "SimpleLauncher";
    private static readonly string[] RepoOwners = ["drpetersonfernandes", "purelogiccode"];
    private const string SecondaryServerBaseUrl = "https://assets.purelogiccode.com/Simple%20Launcher/Simple%20Launcher/";
    private readonly HttpClient _httpClient;
    private readonly ILogger _logger;
    private readonly IMessageBoxLibraryService _messageBoxLibrary;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaCheckForUpdatesService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">The factory used to create the HTTP client for GitHub API requests.</param>
    /// <param name="messageBoxLibrary">The message box service used to prompt the user about updates.</param>
    /// <param name="logger">The logger instance.</param>
    public AvaloniaCheckForUpdatesService(IHttpClientFactory httpClientFactory, IMessageBoxLibraryService messageBoxLibrary, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(httpClientFactory);
        _httpClient = httpClientFactory.CreateClient("UpdateCheckerClient");
        _messageBoxLibrary = messageBoxLibrary;
        _logger = logger;
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
                _logger.Error(ex, "Error getting CurrentVersion.");
                return "0.0.0.0";
            }
        }
    }

    /// <summary>
    /// Checks for updates manually and notifies the user whether an update is available or not.
    /// </summary>
    /// <param name="owner">The owner window for update dialogs (may be null).</param>
    /// <returns>A task representing the asynchronous update check operation.</returns>
    internal async Task ManualCheckForUpdatesAsync(Window? owner)
    {
        try
        {
            var (latestVersion, _, _, _) = await GetLatestReleaseInfoAsync();

            if (latestVersion == null)
            {
                // Expected condition (both sources unreachable / offline); the user is
                // already notified via the message box below — not a bug report.
                _logger.Information("Could not determine the latest version (GitHub and the secondary server are unreachable).");
                await _messageBoxLibrary.ErrorCheckingForUpdatesMessageBoxAsync();
                return;
            }

            if (IsNewVersionAvailable(CurrentVersion, latestVersion))
            {
                var result = await _messageBoxLibrary.DoYouWantToUpdateMessageBoxAsync(CurrentVersion, latestVersion);
                if (result == CoreMessageBoxResult.Yes)
                {
                    // The Avalonia port ships no Updater.exe yet — guide the user to the
                    // GitHub releases page for a manual download.
                    _logger.Information("Update to {LatestVersion} confirmed by user; manual download flow shown (no Updater.exe in the Avalonia port).", latestVersion);
                    await _messageBoxLibrary.InstallUpdateManuallyMessageBoxAsync();
                }
            }
            else
            {
                await _messageBoxLibrary.ThereIsNoUpdateAvailableMessageBoxAsync(CurrentVersion);
            }

            _ = owner;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error checking for updates (manual).");
            await _messageBoxLibrary.ErrorCheckingForUpdatesMessageBoxAsync();
        }
    }

    /// <summary>
    /// Gets the latest release info from the GitHub API (trying each repository in order),
    /// falling back to the secondary server (assets.purelogiccode.com) when GitHub is
    /// unreachable.
    /// </summary>
    /// <returns>A tuple with the latest version, release package URL, updater zip URL, and whether the fallback was used.</returns>
    private async Task<(string? latestVersion, string? releasePackageUrl, string? updaterZipAssetUrl, bool fromFallback)> GetLatestReleaseInfoAsync()
    {
        _httpClient.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "request");

        foreach (var repoOwner in RepoOwners)
        {
            try
            {
                var response = await _httpClient.GetAsync($"https://api.github.com/repos/{repoOwner}/{RepoName}/releases/latest");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.Debug($"[UpdateChecker] GitHub API check for '{repoOwner}/{RepoName}' failed with status {response.StatusCode}; trying the next source.");
                    continue;
                }

                _logger.Debug("Check for Updates Success");

                var content = await response.Content.ReadAsStringAsync();
                var (latestVersion, releasePackageUrl, updaterZipAssetUrl) = ParseVersionAndAssetUrlsFromResponse(content);
                return (latestVersion, releasePackageUrl, updaterZipAssetUrl, false);
            }
            catch (Exception ex)
            {
                _logger.Debug($"[UpdateChecker] GitHub API check for '{repoOwner}/{RepoName}' failed: {ex.Message}; trying the next source.");
            }
        }

        // Fallback: the secondary server hosts a version.txt file and the release packages.
        try
        {
            var versionResponse = await _httpClient.GetAsync(SecondaryServerBaseUrl + "version.txt");
            if (!versionResponse.IsSuccessStatusCode)
            {
                _logger.Debug($"[UpdateChecker] Secondary server check failed with status {versionResponse.StatusCode}.");
                return (null, null, null, false);
            }

            var versionText = (await versionResponse.Content.ReadAsStringAsync()).Trim();
            var versionMatch = VersionRegex().Match(versionText);
            if (!versionMatch.Success)
            {
                _logger.Debug($"[UpdateChecker] Secondary server version.txt has no valid version: '{versionText}'.");
                return (null, null, null, false);
            }

            var rawVersion = versionMatch.Value;
            var latestVersion = NormalizeVersion(rawVersion);
            var releasePackageUrl = SecondaryServerBaseUrl + $"release_{rawVersion}_win-x64.zip";
            var updaterZipAssetUrl = SecondaryServerBaseUrl + "updater_win-x64.zip";

            _logger.Information("GitHub API unavailable. Using the secondary server: version {LatestVersion}.", latestVersion);
            return (latestVersion, releasePackageUrl, updaterZipAssetUrl, true);
        }
        catch (Exception ex)
        {
            _logger.Debug($"[UpdateChecker] Secondary server check failed: {ex.Message}.");
            return (null, null, null, false);
        }
    }

    private bool IsNewVersionAvailable(string currentVersion, string latestVersion)
    {
        try
        {
            if (string.IsNullOrEmpty(currentVersion) || string.IsNullOrEmpty(latestVersion))
            {
                _logger.Error(new ArgumentException(@"Current or latest version string is null or empty.", nameof(currentVersion)), "Invalid version string for comparison.");
                return false;
            }

            var currentNormalized = NonNumericRegex().Replace(currentVersion, "");
            var latestNormalized = NonNumericRegex().Replace(latestVersion, "");

            if (string.IsNullOrEmpty(currentNormalized) || string.IsNullOrEmpty(latestNormalized))
            {
                _logger.Error(new ArgumentException(@"Normalized version string is null or empty after regex replace.", nameof(latestVersion)), "Invalid version string after normalization.");
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
                _logger.Error(ex, $"Invalid version number format after normalization. Current: '{currentVersion}', Latest: '{latestVersion}'.");
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error in IsNewVersionAvailable.");
            return false;
        }
    }

    private (string? version, string? releasePackageUrl, string? updaterZipUrl) ParseVersionAndAssetUrlsFromResponse(string jsonResponse)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            if (!root.TryGetProperty("tag_name", out var tagNameElement))
            {
                _logger.Error(new KeyNotFoundException("'tag_name' not found in GitHub API response."), "GitHub API Response Error");
                return (null, null, null);
            }

            var versionTag = tagNameElement.GetString();
            string? rawVersionStringFromTag = null;
            string? extractedNormalizedVersion = null;

            if (!string.IsNullOrEmpty(versionTag))
            {
                var versionMatch = VersionRegex().Match(versionTag);
                if (versionMatch.Success)
                {
                    rawVersionStringFromTag = versionMatch.Value;
                    extractedNormalizedVersion = NormalizeVersion(rawVersionStringFromTag);
                }
            }

            if (extractedNormalizedVersion == null)
            {
                _logger.Error(new FormatException($"Could not extract or normalize a valid version from tag_name: '{versionTag}'."), "GitHub API Response Error");
                return (null, null, null);
            }

            string? foundReleasePackageUrl = null;
            string? foundUpdaterZipUrl = null;

            const string expectedUpdaterFileName = "updater_win-x64.zip";
            var expectedReleaseFileName = $"release_{rawVersionStringFromTag}_win-x64.zip";

            _logger.Debug($"Searching for assets: '{expectedReleaseFileName}' and '{expectedUpdaterFileName}'");

            if (root.TryGetProperty("assets", out var assetsElement) && assetsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var asset in assetsElement.EnumerateArray())
                {
                    if (!asset.TryGetProperty("name", out var nameElement)) continue;

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

                    if (foundUpdaterZipUrl != null && foundReleasePackageUrl != null) break;
                }

                if (foundUpdaterZipUrl == null)
                {
                    _logger.Error(new FileNotFoundException($"'{expectedUpdaterFileName}' asset not found in release '{versionTag}'.", expectedUpdaterFileName), "GitHub API Asset Info");
                }

                if (foundReleasePackageUrl == null)
                {
                    _logger.Error(new FileNotFoundException($"Expected release package '{expectedReleaseFileName}' not found in release '{versionTag}'.", expectedReleaseFileName), "GitHub API Asset Info");
                }

                return (extractedNormalizedVersion, foundReleasePackageUrl, foundUpdaterZipUrl);
            }

            _logger.Error(new KeyNotFoundException("'assets' array not found or invalid in GitHub API response."), "GitHub API Response Error");
        }
        catch (JsonException jsonEx)
        {
            _logger.Error(jsonEx, "Failed to parse JSON response from GitHub API.");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Unexpected error in ParseVersionAndAssetUrlsFromResponse.");
        }

        return (null, null, null);
    }

    private static string NormalizeVersion(string? version)
    {
        if (string.IsNullOrEmpty(version)) return "0.0.0.0";

        var numericVersion = NonNumericRegex().Replace(version, "");
        numericVersion = MultiDotRegex().Replace(numericVersion, ".").Trim('.');
        if (string.IsNullOrEmpty(numericVersion)) return "0.0.0.0";

        var parts = new List<string>(numericVersion.Split('.', StringSplitOptions.RemoveEmptyEntries));

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

    [GeneratedRegex(@"[^\d\.]", RegexOptions.None, 1000)]
    private static partial Regex NonNumericRegex();

    [GeneratedRegex(@"(\d+(\.\d+){1,3})", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture, 1000)]
    private static partial Regex VersionRegex();

    [GeneratedRegex(@"\.{2,}", RegexOptions.None, 1000)]
    private static partial Regex MultiDotRegex();
}