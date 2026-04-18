using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Net.Http;

namespace Updater.Services;

/// <summary>
/// Service for interacting with the GitHub API to fetch release information,
/// with fallback to a secondary server.
/// </summary>
public partial class GitHubService
{
    private const string RepoOwner = "drpetersonfernandes";
    private const string RepoName = "SimpleLauncher";
    private const string ApiUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
    private const string SecondaryServerBaseUrl = "https://assets.purelogiccode.com/Simple%20Launcher/Simple%20Launcher/";
    private const int GitHubTimeoutSeconds = 5;

    private readonly HttpClient _httpClient;

    /// <summary>
    /// Event raised when a log message needs to be displayed.
    /// </summary>
    public event Action<string>? LogMessage;

    /// <summary>
    /// Initializes a new instance of the GitHubService class.
    /// </summary>
    /// <param name="httpClient">The HTTP client to use for API requests.</param>
    public GitHubService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <summary>
    /// Gets the current runtime identifier based on the process architecture.
    /// </summary>
    public static string CurrentRuntimeIdentifier
    {
        get
        {
            var arch = RuntimeInformation.ProcessArchitecture;
            return arch switch
            {
                Architecture.Arm64 => "win-arm64",
                _ => "win-x64"
            };
        }
    }

    /// <summary>
    /// Fetches the latest release asset URL from GitHub with a timeout,
    /// falling back to the secondary server if GitHub is not available.
    /// </summary>
    /// <returns>A tuple containing the normalized version string and the asset download URL.</returns>
    /// <exception cref="HttpRequestException">Thrown when both GitHub and fallback requests fail.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the release data is invalid or the asset is not found.</exception>
    public async Task<(string version, string assetUrl)> GetLatestReleaseAssetUrlAsync()
    {
        // First, try to get release info from GitHub with a short timeout
        var gitHubResult = await TryGetGitHubReleaseAsync();

        if (gitHubResult != null)
        {
            return gitHubResult.Value;
        }

        // If GitHub failed, fall back to secondary server
        LogMessage?.Invoke($"GitHub not responding after {GitHubTimeoutSeconds} seconds. Using secondary server...");
        return await GetFallbackReleaseAsync();
    }

    /// <summary>
    /// Attempts to get the release from GitHub with a 5-second timeout.
    /// </summary>
    /// <returns>The version and asset URL if successful, null if timed out or failed.</returns>
    private async Task<(string version, string assetUrl)?> TryGetGitHubReleaseAsync()
    {
        try
        {
            LogMessage?.Invoke("Fetching the latest release from GitHub...");

            // Create a cancellation token that expires after 5 seconds
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(GitHubTimeoutSeconds));

            var response = await _httpClient.GetAsync(ApiUrl, cts.Token);

            if (!response.IsSuccessStatusCode)
            {
                LogMessage?.Invoke($"GitHub API returned status code: {response.StatusCode}");
                return null;
            }

            var jsonResponse = await response.Content.ReadAsStringAsync(cts.Token);
            using var jsonDoc = JsonDocument.Parse(jsonResponse);
            var root = jsonDoc.RootElement;

            var versionTag = root.GetProperty("tag_name").GetString() ?? string.Empty;

            // Validate version tag format
            if (string.IsNullOrWhiteSpace(versionTag))
            {
                LogMessage?.Invoke("Release tag_name is null or empty.");
                return null;
            }

            // Extract version from tag (handle "release5.3.1" format)
            var rawVersionString = ExtractVersionFromTag(versionTag);
            if (string.IsNullOrEmpty(rawVersionString))
            {
                LogMessage?.Invoke($"Could not extract version from tag: '{versionTag}'");
                return null;
            }

            // Validate that version has at least major.minor format
            var versionParts = rawVersionString.Split('.');
            if (versionParts.Length < 2)
            {
                LogMessage?.Invoke($"Invalid version format: '{rawVersionString}'. Version must have at least major.minor components.");
                return null;
            }

            var normalizedVersion = NormalizeVersion(rawVersionString);
            var expectedAssetName = $"release_{rawVersionString}_{CurrentRuntimeIdentifier}.zip";

            LogMessage?.Invoke($"Searching for asset: {expectedAssetName}");

            if (root.TryGetProperty("assets", out var assetsElement))
            {
                foreach (var asset in assetsElement.EnumerateArray())
                {
                    var assetName = asset.GetProperty("name").GetString();
                    if (assetName?.Equals(expectedAssetName, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        var assetUrl = asset.GetProperty("browser_download_url").GetString();
                        if (!string.IsNullOrEmpty(assetUrl))
                        {
                            LogMessage?.Invoke($"Latest version found: {normalizedVersion}");
                            LogMessage?.Invoke($"Release package URL: {assetUrl}");
                            return (normalizedVersion, assetUrl);
                        }
                    }
                }
            }

            LogMessage?.Invoke($"Could not find the required asset '{expectedAssetName}' in the latest release.");
            return null;
        }
        catch (OperationCanceledException)
        {
            LogMessage?.Invoke($"GitHub request timed out after {GitHubTimeoutSeconds} seconds.");
            return null;
        }
        catch (Exception ex)
        {
            LogMessage?.Invoke($"Error fetching from GitHub: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Gets the release from the secondary server when GitHub is unavailable.
    /// </summary>
    /// <returns>A tuple containing the normalized version string and the asset download URL.</returns>
    /// <exception cref="HttpRequestException">Thrown when the request fails.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the version file is invalid.</exception>
    private async Task<(string version, string assetUrl)> GetFallbackReleaseAsync()
    {
        try
        {
            LogMessage?.Invoke("Checking secondary server for latest version...");

            // The secondary server has a version.txt file with the current version
            const string versionUrl = SecondaryServerBaseUrl + "version.txt";

            var versionResponse = await _httpClient.GetAsync(versionUrl);
            if (!versionResponse.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"Failed to fetch version from secondary server. Status Code: {versionResponse.StatusCode}");
            }

            var versionText = (await versionResponse.Content.ReadAsStringAsync()).Trim();

            // Remove "release" prefix if present
            var rawVersionString = ExtractVersionFromTag(versionText);
            if (string.IsNullOrEmpty(rawVersionString))
            {
                throw new InvalidOperationException($"Invalid version format in version.txt: '{versionText}'");
            }

            // Validate that version has at least major.minor format
            var versionParts = rawVersionString.Split('.');
            if (versionParts.Length < 2)
            {
                throw new InvalidOperationException($"Invalid version format: '{rawVersionString}'. Version must have at least major.minor components.");
            }

            var normalizedVersion = NormalizeVersion(rawVersionString);
            var expectedAssetName = $"release_{rawVersionString}_{CurrentRuntimeIdentifier}.zip";
            var assetUrl = SecondaryServerBaseUrl + expectedAssetName;

            LogMessage?.Invoke($"Latest version found: {normalizedVersion}");
            LogMessage?.Invoke($"Release package URL: {assetUrl}");

            return (normalizedVersion, assetUrl);
        }
        catch (Exception ex)
        {
            await BugReportService.ReportBugAsync(ex, "Error in GetFallbackReleaseAsync - Secondary server fallback failed");
            throw;
        }
    }

    /// <summary>
    /// Extracts the version number from a tag or version string.
    /// Handles formats like "release5.3.1", "v5.3.1", or just "5.3.1".
    /// </summary>
    /// <param name="tag">The tag or version string.</param>
    /// <returns>The extracted version string, or null if extraction failed.</returns>
    private static string? ExtractVersionFromTag(string tag)
    {
        // Try to match version pattern (digits separated by dots)
        var match = VersionRegex().Match(tag);
        if (match.Success)
        {
            return match.Value;
        }

        // Fallback: if tag starts with "release" or "v", try to extract after that
        var normalizedTag = tag.Trim().ToLowerInvariant();
        if (normalizedTag.StartsWith("release", StringComparison.Ordinal))
        {
            var versionPart = tag[7..]; // Remove "release" prefix
            match = VersionRegex().Match(versionPart);
            if (match.Success)
            {
                return match.Value;
            }
        }
        else if (normalizedTag.StartsWith('v'))
        {
            var versionPart = tag[1..]; // Remove "v" prefix
            match = VersionRegex().Match(versionPart);
            if (match.Success)
            {
                return match.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the GitHub releases page URL for manual downloads.
    /// </summary>
    public static string GetReleasesPageUrl()
    {
        return $"https://github.com/{RepoOwner}/{RepoName}/releases/latest";
    }

    /// <summary>
    /// Normalizes a version string to ensure it has exactly 4 version components (major.minor.build.revision).
    /// </summary>
    /// <param name="version">The version string to normalize.</param>
    /// <returns>A normalized version string with 4 components, or "0.0.0.0" if the input is null or empty.</returns>
    private static string NormalizeVersion(string version)
    {
        if (string.IsNullOrEmpty(version)) return "0.0.0.0";

        var parts = new List<string>(version.Split('.'));
        while (parts.Count < 4)
        {
            parts.Add("0");
        }

        return string.Join(".", parts.Take(4));
    }

    [GeneratedRegex(@"(\d+(\.\d+){1,3})", RegexOptions.Compiled)]
    private static partial Regex VersionRegex();
}
