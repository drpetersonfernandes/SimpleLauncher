using System.Diagnostics;
using System.IO.Compression;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Updater;

public partial class UpdateForm : Form
{
    private delegate void LogDelegate(string message);

    private const string RepoOwner = "drpetersonfernandes";
    private const string RepoName = "SimpleLauncher";
    private static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;

    public UpdateForm()
    {
        InitializeComponent();

        var applicationVersion = GetApplicationVersion();
        Log($"Updater version: {applicationVersion}\n\n");
    }

    private static string GetApplicationVersion()
    {
        // Retrieve the version from the executing assembly
        var version = Assembly.GetExecutingAssembly().GetName().Version;

        // Check if the version is null, and format it as needed
        return version != null ? version.ToString() : "Version not available";
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);
        var updateThread = new Thread(RunUpdateProcess)
        {
            IsBackground = true
        };
        updateThread.Start();
    }

    private void RunUpdateProcess()
    {
        UpdateProcess().Wait();
    }

    private async Task UpdateProcess()
    {
        if (string.IsNullOrEmpty(AppDirectory))
        {
            Log("Could not determine the application directory.");

            RedirectToDownloadPage("Could not determine the application directory.\n\n" +
                                   "The automatic update wont work.\n\n" +
                                   "Would you like to update manually?");
            Close();
            return;
        }

        try
        {
            // Wait for the main application to exit
            Log("Waiting for the main application to exit...");
            Thread.Sleep(3000);

            // Fetch the latest release from GitHub
            Log("Fetch the latest release from GitHub.");
            var (latestVersion, assetUrl) = await GetLatestReleaseAssetUrlAsync();
            Log($"Latest version: {latestVersion}");

            if (string.IsNullOrEmpty(assetUrl))
            {
                Log("Failed to retrieve download URL for the latest release.");
                RedirectToDownloadPage("Failed to retrieve the latest release.\n\n" +
                                       "Would you like to update manually?");
                Close();
                return;
            }

            // Download the update file to memory
            Log("Downloading the update file...");
            using var updateFileStream = await DownloadUpdateFileToMemoryAsync(assetUrl);

            // Extract the ZIP file directly in memory
            Log("Extracting update files...");
            using var archive = new ZipArchive(updateFileStream);

            // Files to exclude during extraction
            var ignoredFiles = new[]
            {
                "Updater.deps.json",
                "Updater.dll",
                "Updater.exe",
                "Updater.pdb",
                "Updater.runtimeconfig.json"
            };

            foreach (var entry in archive.Entries)
            {
                // Skip ignored files and directories
                if (string.IsNullOrEmpty(entry.Name) || ignoredFiles.Contains(entry.Name))
                    continue;

                // Construct the destination path
                var destinationPath = Path.Combine(AppDirectory, entry.FullName);

                // Ensure the destination directory exists
                var destinationDirectory = Path.GetDirectoryName(destinationPath);
                if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                    Directory.CreateDirectory(destinationDirectory);

                // Extract the file
                await using var entryStream = entry.Open();
                await using var destinationFileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                await entryStream.CopyToAsync(destinationFileStream);

                Log($"Extracted: {entry.FullName}");
            }

            // Notify the user of a successful update
            Log("Update installed successfully.");
            Log("The application will now restart.");
            MessageBox.Show("Update installed successfully.\n\n" +
                            "The application will now restart.",
                "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // Restart the main application with the "whatsnew" parameter
            var simpleLauncherExePath = Path.Combine(AppDirectory, "SimpleLauncher.exe");
            var startInfo = new ProcessStartInfo
            {
                FileName = simpleLauncherExePath,
                Arguments = "whatsnew",
                UseShellExecute = false,
                WorkingDirectory = AppDirectory
            };

            Process.Start(startInfo);

            // Automatically close the update Window
            Close();
        }
        catch (Exception ex)
        {
            Log($"Automatic update failed: {ex.Message}");

            RedirectToDownloadPage("Automatic update failed.\n\n" +
                                   "Would you like to update manually?");
        }
    }

    private async Task<MemoryStream> DownloadUpdateFileToMemoryAsync(string url)
    {
        using var httpClient = new HttpClient();
        var response = await httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
            Log($"Failed to download the update file: {response.StatusCode}");

            RedirectToDownloadPage("Failed to download the update file.\n\n" +
                                   "Would you like to update manually?");

            Close();
        }

        var memoryStream = new MemoryStream();
        await response.Content.CopyToAsync(memoryStream);
        memoryStream.Position = 0; // Reset the stream position for reading
        return memoryStream;
    }

    private void RedirectToDownloadPage(string message)
    {
        var result = MessageBox.Show(message, "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
        if (result == DialogResult.Yes)
        {
            const string downloadPageUrl = $"https://github.com/{RepoOwner}/{RepoName}/releases/latest";
            Process.Start(new ProcessStartInfo
            {
                FileName = downloadPageUrl,
                UseShellExecute = true // Open URL in default browser
            });
        }

        Close();
    }

    private async Task<(string version, string assetUrl)> GetLatestReleaseAssetUrlAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "request");

            var response = await client.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
            if (!response.IsSuccessStatusCode)
            {
                Log($"Failed to fetch the latest release information. Status Code: {response.StatusCode}");
                return (string.Empty, string.Empty); // Return empty strings if the request fails
            }

            var jsonResponse = await response.Content.ReadAsStringAsync();
            using var jsonDoc = JsonDocument.Parse(jsonResponse);
            var root = jsonDoc.RootElement;

            // Get the version from the "tag_name" field
            var versionTag = root.TryGetProperty("tag_name", out var tagNameElement)
                ? tagNameElement.GetString() ?? string.Empty // Handle potential null here
                : string.Empty; // Default to empty if tag_name is not found

            // Initialize assetUrl to an empty string to avoid nullability issues
            var assetUrl = string.Empty;

            if (root.TryGetProperty("assets", out var assetsElement))
            {
                foreach (var asset in assetsElement.EnumerateArray())
                {
                    if (!asset.TryGetProperty("browser_download_url", out var downloadUrlElement)) continue;

                    assetUrl = downloadUrlElement.GetString() ?? string.Empty; // Handle potential null here
                    break;
                }
            }

            // Match the version using the regex and return the version and asset URL
            var versionMatch = MyRegex().Match(versionTag);
            if (versionMatch.Success)
            {
                return (NormalizeVersion(versionMatch.Value), assetUrl);
            }

            Log("Error parsing the version from the release tag.");
            return (string.Empty, assetUrl); // Return assetUrl even if version parsing fails
        }
        catch (Exception ex)
        {
            Log($"Failed to fetch the latest release asset URL: {ex.Message}");
            return (string.Empty, string.Empty); // Return empty strings in case of an exception
        }
    }

    private void Log(string message)
    {
        if (InvokeRequired)
        {
            Invoke(new LogDelegate(Log), message);
            return;
        }

        logTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
    }

    private static Regex MyRegex()
    {
        return MyRegex1();
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

    [GeneratedRegex(@"(?<=release(?:-[a-zA-Z0-9]+)?-?)\d+(\.\d+)*", RegexOptions.Compiled)]
    private static partial Regex MyRegex1();
}