using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq; // Added for OfType<UpdateLogWindow>()
using System.Net.Http;
using System.Reflection;
using System.Security; // Added for SecurityException
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace SimpleLauncher.Services;

public static partial class UpdateChecker
{
    private const string RepoOwner = "drpetersonfernandes";
    private const string RepoName = "SimpleLauncher";
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
                _ = LogErrors.LogErrorAsync(ex, "Error getting CurrentVersion.");
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
            if (HttpClientFactory == null)
            {
                throw new InvalidOperationException("HttpClientFactory is not initialized. Update check cannot proceed.");
            }

            var httpClient = HttpClientFactory.CreateClient("UpdateCheckerClient");
            httpClient.DefaultRequestHeaders.Add("User-Agent", "request");

            var response = await httpClient.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var (latestVersion, assetUrl) = ParseVersionFromResponse(content);

                if (latestVersion == null) return;

                if (IsNewVersionAvailable(CurrentVersion, latestVersion))
                {
                    if (assetUrl != null) await ShowUpdateWindow(assetUrl, CurrentVersion, latestVersion, mainWindow);
                }
            }
        }
        catch (Exception ex)
        {
            const string contextMessage = "Error checking for updates.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    public static async Task CheckForUpdatesVariantAsync(Window mainWindow)
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
                var content = await response.Content.ReadAsStringAsync();
                var (latestVersion, assetUrl) = ParseVersionFromResponse(content);

                if (latestVersion == null) return;

                if (IsNewVersionAvailable(CurrentVersion, latestVersion))
                {
                    if (assetUrl != null) await ShowUpdateWindow(assetUrl, CurrentVersion, latestVersion, mainWindow);
                }
                else
                {
                    MessageBoxLibrary.ThereIsNoUpdateAvailableMessageBox(mainWindow, CurrentVersion);
                }
            }
        }
        catch (Exception ex)
        {
            const string contextMessage = "Error checking for updates.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
            MessageBoxLibrary.ErrorCheckingForUpdatesMessageBox(mainWindow);
        }
    }

    private static async Task ShowUpdateWindow(string assetUrl, string currentVersion, string latestVersion, Window owner)
    {
        UpdateLogWindow logWindow = null; // Initialize to null
        try
        {
            var result = MessageBoxLibrary.DoYouWantToUpdateMessageBox(currentVersion, latestVersion, owner);
            if (result != MessageBoxResult.Yes) return;

            logWindow = new UpdateLogWindow();
            logWindow.Show();
            logWindow.Log("Starting update process...");

            if (owner == null)
            {
                _ = LogErrors.LogErrorAsync(new ArgumentNullException(nameof(owner), @"Owner window was null when trying to close it during update."), "Update Process Warning");
                logWindow.Log("Main window reference is null; cannot close it explicitly. Attempting global shutdown later.");
            }
            else
            {
                owner.Close();
            }

            try
            {
                var appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                logWindow.Log("Downloading update file...");

                await Task.Run(async () =>
                {
                    using var memoryStream = new MemoryStream();
                    await DownloadUpdateFileToMemory(assetUrl, memoryStream);

                    logWindow.Log("Extracting update file...");
                    var updaterFiles = Function;

                    var allFilesExtracted = ExtractFilesToDestination(memoryStream, appDirectory, updaterFiles, logWindow);

                    if (allFilesExtracted)
                    {
                        logWindow.Log("Update files extracted successfully.");
                        await Task.Delay(2000);
                        await ExecuteUpdater(logWindow);
                    }
                    else
                    {
                        logWindow.Log("Update failed: Not all required files could be extracted due to security checks or other errors.");
                        // This exception will be caught by the outer catch block, leading to manual update instructions.
                        throw new IOException("Failed to extract one or more update files. Please check the update log for details.");
                    }
                });
            }
            catch (Exception ex)
            {
                const string contextMessage = "There was an error updating the application.";
                _ = LogErrors.LogErrorAsync(ex, contextMessage);
                MessageBoxLibrary.InstallUpdateManuallyMessageBox(RepoOwner, RepoName);
                if (logWindow != null)
                {
                    logWindow.Log($"Error during update: {ex.Message}");
                    logWindow.Log("Please update it manually.");
                    // logWindow.Close(); // Consider closing after a delay or when user acknowledges
                }
            }
        }
        catch (Exception ex)
        {
            const string contextMessage = "There was an error updating the application.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
            if (logWindow != null) // Check if logWindow was initialized
            {
                logWindow.Log($"Outer catch block error: {ex.Message}");
                // logWindow.Close();
            }
            else // Fallback if logWindow itself failed or wasn't created
            {
                // Attempt to find any existing log window, though less likely to be the correct one here.
                var existingLogWindow = Application.Current?.Windows.OfType<UpdateLogWindow>().FirstOrDefault();
                existingLogWindow?.Log($"Outer catch block error (logWindow was null): {ex.Message}");
            }

            MessageBoxLibrary.InstallUpdateManuallyMessageBox(RepoOwner, RepoName);
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
    }

    private static bool ExtractFilesToDestination(Stream zipStream, string destinationPath, string[] filesToExtract, UpdateLogWindow logWindow)
    {
        var successfullyExtractedCount = 0;
        using var archive = new ZipArchive(zipStream, ZipArchiveMode.Read);

        // Normalize the destination root directory path.
        // Path.GetFullPath will resolve to an absolute path and normalize it.
        // For a directory, it's good practice to ensure it ends with a directory separator for StartsWith comparisons.
        var destinationRootFullPath = Path.GetFullPath(destinationPath);
        if (!destinationRootFullPath.EndsWith(Path.DirectorySeparatorChar.ToString(), StringComparison.Ordinal))
        {
            destinationRootFullPath += Path.DirectorySeparatorChar;
        }

        foreach (var fileNameInArray in filesToExtract)
        {
            // It's crucial that fileNameInArray matches the entry name in the zip or is just the base name if entries are at root.
            // Assuming filesToExtract contains exact entry names as they are in the zip root.
            var entry = archive.GetEntry(fileNameInArray);
            if (entry != null)
            {
                // Calculate the full path for the destination file.
                var destinationFileFullPath = Path.GetFullPath(Path.Combine(destinationPath, entry.FullName)); // Use entry.FullName for robustness

                // Security Check (Zip Slip)
                // Ensure the normalized destination file path starts with the normalized root directory path.
                if (!destinationFileFullPath.StartsWith(destinationRootFullPath, StringComparison.OrdinalIgnoreCase))
                {
                    logWindow.Log($"Skipping extraction of '{entry.FullName}' to '{destinationFileFullPath}' due to security violation (Zip Slip detected). Expected base: '{destinationRootFullPath}'");
                    _ = LogErrors.LogErrorAsync(new SecurityException($"Zip Slip detected for file {entry.FullName} in archive. Attempted path: {destinationFileFullPath}"), "Zip Slip Vulnerability Mitigated");
                    MessageBoxLibrary.PotentialPathManipulationDetectedMessageBox(entry.FullName);
                    continue; // Skip this file
                }

                var directory = Path.GetDirectoryName(destinationFileFullPath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    try
                    {
                        Directory.CreateDirectory(directory);
                    }
                    catch (Exception ex)
                    {
                        _ = LogErrors.LogErrorAsync(ex, $"Error creating directory '{directory}'.");
                        logWindow.Log($"Failed to create directory: {directory}. Error: {ex.Message}. Skipping file '{entry.FullName}'.");
                        continue; // Skip this file if directory creation fails
                    }
                }

                logWindow.Log($"Extracting {entry.FullName} to {destinationFileFullPath}");
                try
                {
                    using var entryStream = entry.Open();
                    using var fileStream = new FileStream(destinationFileFullPath, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite);
                    entryStream.CopyTo(fileStream);
                    successfullyExtractedCount++;
                }
                catch (Exception ex)
                {
                    _ = LogErrors.LogErrorAsync(ex, $"Error extracting file '{entry.FullName}' to '{destinationFileFullPath}'.");
                    logWindow.Log($"Failed to extract file: {entry.FullName}. Error: {ex.Message}");
                    // Decide if we should continue with other files or fail the whole update
                }
            }
            else
            {
                logWindow.Log($"File '{fileNameInArray}' not found in the archive.");
            }
        }

        return successfullyExtractedCount == filesToExtract.Length;
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
            MessageBoxLibrary.DownloadManuallyMessageBox(RepoOwner, RepoName);
            // logWindow.Close(); // Consider closing after a delay or user acknowledgement
            return;
        }

        logWindow.Log("Starting updater process...");
        await Task.Delay(1000); // Reduced delay slightly

        Process.Start(new ProcessStartInfo
        {
            FileName = updaterExePath,
            Arguments = $"\"{appExePath}\"", // Ensure appExePath is quoted if it contains spaces
            UseShellExecute = false
        });

        logWindow.Log("Closing main application for update...");
        await Task.Delay(1000); // Give log a moment to be seen

        if (Application.Current?.Dispatcher != null)
        {
            Application.Current.Dispatcher.Invoke(static () =>
            {
                QuitApplication.ForcefullyQuitApplication();
            });
        }
        else
        {
            _ = LogErrors.LogErrorAsync(new InvalidOperationException("Application.Current or Dispatcher is null in ExecuteUpdater."), "Updater: Failed to dispatch application quit.");
            logWindow.Log("Error: Could not dispatch application quit. Manual close might be required.");
        }
    }

    private static bool IsNewVersionAvailable(string currentVersion, string latestVersion)
    {
        try
        {
            if (string.IsNullOrEmpty(currentVersion) || string.IsNullOrEmpty(latestVersion))
            {
                _ = LogErrors.LogErrorAsync(new ArgumentException("Current or latest version string is null or empty."), "Invalid version string for comparison.");
                return false;
            }

            var currentNormalized = MyRegex1().Replace(currentVersion, "");
            var latestNormalized = MyRegex1().Replace(latestVersion, "");

            if (string.IsNullOrEmpty(currentNormalized) || string.IsNullOrEmpty(latestNormalized))
            {
                _ = LogErrors.LogErrorAsync(new ArgumentException("Normalized version string is null or empty after regex replace."), "Invalid version string after normalization.");
                return false;
            }

            var current = new Version(currentNormalized);
            var latest = new Version(latestNormalized);
            return latest.CompareTo(current) > 0;
        }
        catch (ArgumentException ex)
        {
            _ = LogErrors.LogErrorAsync(ex, $"Invalid version number format. Current: '{currentVersion}', Latest: '{latestVersion}'.");
            return false;
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Unexpected error in IsNewVersionAvailable.");
            return false;
        }
    }

    private static (string version, string assetUrl) ParseVersionFromResponse(string jsonResponse)
    {
        try
        {
            using var doc = JsonDocument.Parse(jsonResponse);
            var root = doc.RootElement;

            if (root.TryGetProperty("tag_name", out var tagNameElement) &&
                root.TryGetProperty("assets", out var assetsElement))
            {
                var versionTag = tagNameElement.GetString();
                string assetUrl = null;
                if (assetsElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var asset in assetsElement.EnumerateArray())
                    {
                        if (!asset.TryGetProperty("browser_download_url", out var downloadUrlElement)) continue;

                        assetUrl = downloadUrlElement.GetString();
                        if (!string.IsNullOrEmpty(assetUrl)) break;
                    }
                }

                var versionMatch = MyRegex2().Match(versionTag ?? string.Empty);
                if (versionMatch.Success)
                {
                    return (NormalizeVersion(versionMatch.Value), assetUrl);
                }

                const string noVersionInTagMessage = "Version number was not found in the tag using regex.";
                _ = LogErrors.LogErrorAsync(new FormatException(noVersionInTagMessage + $" Tag: '{versionTag}'"), "Error parsing application version from GitHub API response.");
            }
            else
            {
                const string missingPropertiesMessage = "Version information ('tag_name' or 'assets') not found in the GitHub API response.";
                _ = LogErrors.LogErrorAsync(new KeyNotFoundException(missingPropertiesMessage), "Error parsing application version from GitHub API response.");
            }
        }
        catch (JsonException jsonEx)
        {
            _ = LogErrors.LogErrorAsync(jsonEx, "Failed to parse JSON response from GitHub API.");
        }
        catch (Exception ex)
        {
            _ = LogErrors.LogErrorAsync(ex, "Unexpected error in ParseVersionFromResponse.");
        }

        return (null, null);
    }

    private static string NormalizeVersion(string version)
    {
        if (string.IsNullOrEmpty(version)) return "0.0.0.0";

        var numericVersion = Regex.Replace(version, @"[^\d.]", "");
        numericVersion = Regex.Replace(numericVersion, @"\.{2,}", ".").Trim('.');
        if (string.IsNullOrEmpty(numericVersion)) return "0.0.0.0";

        var parts = numericVersion.Split('.');
        while (parts.Length < 4)
        {
            numericVersion += ".0";
            parts = numericVersion.Split('.');
        }

        return numericVersion;
    }

    [GeneratedRegex(@"[^\d\.]")]
    private static partial Regex MyRegex1();

    [GeneratedRegex(@"(?<=release(?:-[a-zA-Z0-9]+)?-?)\d+(\.\d+)*", RegexOptions.Compiled)]
    private static partial Regex MyRegex2();
}