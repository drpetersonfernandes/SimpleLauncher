using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;

namespace Updater;

public partial class UpdateForm : Form
{
    private delegate void LogDelegate(string message);

    // Configuration
    private const string RepoOwner = "drpetersonfernandes";
    private const string RepoName = "SimpleLauncher";
    private static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;

    // Arguments passed from the main application (e.g., PID)
    private readonly string[] _args;

    // Use a single, static HttpClient instance for performance and resource management
    private static readonly HttpClient HttpClient = new();

    // Gets the runtime identifier for the current process architecture (e.g., "win-x64", "win-arm64")
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

    public UpdateForm(string[] args)
    {
        InitializeComponent();
        _args = args;

        // Set a user-agent for GitHub API requests
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SimpleLauncher-Updater");

        var applicationVersion = GetApplicationVersion();
        Log($"Updater version: {applicationVersion}\n\n");
    }

    private static string GetApplicationVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version != null ? version.ToString() : "Version not available";
    }

    protected override async void OnLoad(EventArgs e)
    {
        try
        {
            base.OnLoad(e);

            // Ensure the form is visible and in the foreground
            WindowState = FormWindowState.Normal;
            Activate();

            // Start the update process directly on the UI thread context
            await UpdateProcess();
        }
        catch (Exception ex)
        {
            Log($"An error occurred during update process: {ex.Message}");
            Log("Please updated manually.");
        }
    }

    private async Task UpdateProcess()
    {
        if (string.IsNullOrEmpty(AppDirectory))
        {
            Log("Could not determine the application directory.");
            RedirectToDownloadPage("Could not determine the application directory.\n\n" +
                                   "The automatic update won't work.\n\n" +
                                   "Would you like to update manually?");
            return;
        }

        try
        {
            // Wait for the main application to exit using its Process ID
            WaitForMainAppToExit();

            // Fetch the latest release from GitHub
            Log("Fetching the latest release from GitHub...");
            var (latestVersion, assetUrl) = await GetLatestReleaseAssetUrlAsync();
            Log($"Latest version found: {latestVersion}");
            Log($"Release package URL: {assetUrl}");

            // Download the update file to memory
            Log("Downloading the update file...");
            await using var updateFileStream = await DownloadUpdateFileToMemoryAsync(assetUrl);

            // Files to exclude during extraction to prevent self-destruction
            var ignoredFiles = new[]
            {
                "Updater.exe",
                "Updater.pdb",
                "Updater.dll",
                "Updater.deps.json",
                "Updater.runtimeconfig.json",
                "ICSharpCode.SharpZipLib.dll" // Using SharpZipLib now
            };

            // Extract the ZIP file
            Log("Extracting update files...");
            await using (var zipInputStream = new ZipInputStream(updateFileStream))
            {
                while (zipInputStream.GetNextEntry() is { } entry)
                {
                    if (string.IsNullOrEmpty(entry.Name)) continue;

                    var fileName = Path.GetFileName(entry.Name);
                    if (ignoredFiles.Contains(fileName, StringComparer.OrdinalIgnoreCase))
                    {
                        Log($"Skipping self-update file: {entry.Name}");
                        continue;
                    }

                    var destinationPath = Path.Combine(AppDirectory, entry.Name);
                    var destinationDirectory = Path.GetDirectoryName(destinationPath);

                    if (!string.IsNullOrEmpty(destinationDirectory) && !Directory.Exists(destinationDirectory))
                        Directory.CreateDirectory(destinationDirectory);

                    if (entry.IsDirectory) continue;

                    await using var destinationFileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
                    await zipInputStream.CopyToAsync(destinationFileStream);

                    Log($"Extracted: {entry.Name}");
                }
            }

            Log("Update installed successfully.");
            MessageBox.Show("Update installed successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            RestartMainApplication();
            Close();
        }
        catch (Exception ex)
        {
            Log($"Automatic update failed: {ex.Message}");
            RedirectToDownloadPage("Automatic update failed.\n\n" +
                                   "Would you like to update manually?");
        }
    }

    private void WaitForMainAppToExit()
    {
        if (_args.Length > 0 && int.TryParse(_args[0], out var pid))
        {
            try
            {
                var mainAppProcess = Process.GetProcessById(pid);
                Log($"Waiting for Simple Launcher (PID: {pid}) to exit...");
                mainAppProcess.WaitForExit(10000); // Wait for up to 10 seconds
                if (!mainAppProcess.HasExited)
                {
                    Log("Warning: Simple Launcher did not exit in time. Update may fail.");
                }
                else
                {
                    Log("Simple Launcher has exited.");
                }
            }
            catch (ArgumentException)
            {
                Log("Simple Launcher process not found. Assuming it has already exited.");
            }
        }
        else
        {
            // This is now a less likely fallback.
            Log("No PID provided by Simple Launcher. Waiting for 3 seconds (this is unreliable)...");
            Thread.Sleep(3000);
        }
    }

    private async Task<MemoryStream> DownloadUpdateFileToMemoryAsync(string url)
    {
        var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
            // Throw an exception to be handled by the central catch block
            throw new HttpRequestException($"Failed to download the update file. Status Code: {response.StatusCode}");
        }

        var memoryStream = new MemoryStream();
        await response.Content.CopyToAsync(memoryStream);
        memoryStream.Position = 0;
        return memoryStream;
    }

    private async Task<(string version, string assetUrl)> GetLatestReleaseAssetUrlAsync()
    {
        const string apiUrl = $"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest";
        var response = await HttpClient.GetAsync(apiUrl);

        if (!response.IsSuccessStatusCode)
        {
            throw new HttpRequestException($"Failed to fetch release info. Status Code: {response.StatusCode}");
        }

        var jsonResponse = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(jsonResponse);
        var root = jsonDoc.RootElement;

        var versionTag = root.GetProperty("tag_name").GetString() ?? string.Empty;
        var versionMatch = MyRegex().Match(versionTag);
        var rawVersionString = versionMatch.Success ? versionMatch.Value : "0.0.0.0";
        var normalizedVersion = NormalizeVersion(rawVersionString);

        var expectedAssetName = $"release_{rawVersionString}_{CurrentRuntimeIdentifier}.zip";
        Log($"Searching for asset: {expectedAssetName}");

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
                        return (normalizedVersion, assetUrl);
                    }
                }
            }
        }

        throw new InvalidOperationException($"Could not find the required asset '{expectedAssetName}' in the latest release.");
    }

    private void RestartMainApplication()
    {
        try
        {
            var simpleLauncherExePath = Path.Combine(AppDirectory, "SimpleLauncher.exe");
            var startInfo = new ProcessStartInfo
            {
                FileName = simpleLauncherExePath,
                Arguments = "-whatsnew",
                UseShellExecute = true,
                WorkingDirectory = AppDirectory
            };
            Process.Start(startInfo);
        }
        catch (Exception ex)
        {
            Log($"Failed to restart the main application: {ex.Message}");
            MessageBox.Show("Update complete, but failed to restart SimpleLauncher automatically. Please start it manually.", "Restart Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void RedirectToDownloadPage(string message)
    {
        if (IsDisposed) return;

        var result = MessageBox.Show(message, "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);
        if (result == DialogResult.Yes)
        {
            const string downloadPageUrl = $"https://github.com/{RepoOwner}/{RepoName}/releases/latest";
            Process.Start(new ProcessStartInfo
            {
                FileName = downloadPageUrl,
                UseShellExecute = true
            });
        }

        Close();
    }

    private void Log(string message)
    {
        if (InvokeRequired)
        {
            Invoke(new LogDelegate(Log), message);
            return;
        }

        if (!IsDisposed && logTextBox.IsHandleCreated)
        {
            logTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
        }
    }

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
    private static partial Regex MyRegex();
}
