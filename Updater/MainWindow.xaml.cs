using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using ICSharpCode.SharpZipLib.Zip;

namespace Updater;

public partial class MainWindow
{
    private const string RepoOwner = "drpetersonfernandes";
    private const string RepoName = "SimpleLauncher";
    private static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;

    private readonly string[] _args;
    private static readonly HttpClient HttpClient = new();

    private static string CurrentRuntimeIdentifier
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

    public MainWindow(string[] args)
    {
        InitializeComponent();
        _args = args;

        // Force UI to show on top and focused
        Loaded += (_, _) =>
        {
            Activate();
            Focus();
            Topmost = false; // Release topmost after initial show so user can switch away if needed
        };

        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("SimpleLauncher-Updater");

        var applicationVersion = GetApplicationVersion();
        Log($"Updater version: {applicationVersion}\n\n");

        // Start update process async when window is loaded
        Loaded += async (_, _) => await ExecuteUpdateAsync();
    }

    private static string GetApplicationVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version?.ToString() ?? "Version not available";
    }

    private async Task ExecuteUpdateAsync()
    {
        try
        {
            await UpdateProcess();
        }
        catch (Exception ex)
        {
            Log($"An error occurred during update process: {ex.Message}");
            Log("Please update manually.");
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
            await WaitForMainAppToExit();

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
                "ICSharpCode.SharpZipLib.dll",
                "Updater.pdb" // In debug builds
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

                    await using var destinationFileStream = new FileStream(
                        destinationPath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None,
                        81920,
                        true);

                    await zipInputStream.CopyToAsync(destinationFileStream);

                    Log($"Extracted: {entry.Name}");
                }
            }

            Log("Update installed successfully.");
            MessageBox.Show("Update installed successfully.", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

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

    private async Task WaitForMainAppToExit()
    {
        if (_args.Length > 0 && int.TryParse(_args[0], out var pid))
        {
            try
            {
                var mainAppProcess = Process.GetProcessById(pid);
                Log($"Waiting for Simple Launcher (PID: {pid}) to exit...");

                // Use Task.Run to prevent UI freeze during the synchronous WaitForExit
                await Task.Run(() => mainAppProcess.WaitForExit(10000));

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
            Log("No PID provided by Simple Launcher. Waiting for 3 seconds (this is unreliable)...");
            await Task.Delay(3000); // Added await
        }
    }

    private static async Task<MemoryStream> DownloadUpdateFileToMemoryAsync(string url)
    {
        var response = await HttpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

        if (!response.IsSuccessStatusCode)
        {
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
            MessageBox.Show("Update complete, but failed to restart SimpleLauncher automatically. Please start it manually.",
                "Restart Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void RedirectToDownloadPage(string message)
    {
        if (!IsLoaded) return;

        var result = MessageBox.Show(message, "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
        if (result == MessageBoxResult.Yes)
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
        if (!Dispatcher.CheckAccess())
        {
            Dispatcher.BeginInvoke(new Action(() => Log(message)));
            return;
        }

        if (IsLoaded)
        {
            LogTextBox.AppendText($"{DateTime.Now:HH:mm:ss} - {message}{Environment.NewLine}");
            LogTextBox.ScrollToEnd();
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