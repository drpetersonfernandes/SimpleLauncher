using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace Updater
{
    public partial class UpdateForm : Form
    {
        private readonly string[] _args;
        private delegate void LogDelegate(string message);
        private const string RepoOwner = "drpetersonfernandes";
        private const string RepoName = "SimpleLauncher";

        public UpdateForm(string[] args)
        {
            InitializeComponent();
            _args = args ?? throw new ArgumentNullException(nameof(args));
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
            UpdateProcess().Wait(); // Run the async method synchronously in the thread
        }

        private async Task UpdateProcess()
        {
            if (_args.Length < 3) // Expecting 3 arguments: appExePath, updateSourcePath, updateZipPath
            {
                Log("Invalid arguments. Usage: Updater <appExePath> <updateSourcePath> <updateZipPath>");
                return;
            }

            var appExePath = _args[0];
            var updateSourcePath = _args[1];
            var updateZipPath = _args[2];

            if (string.IsNullOrEmpty(appExePath) || string.IsNullOrEmpty(updateSourcePath) || string.IsNullOrEmpty(updateZipPath))
            {
                Log("Invalid file paths provided.");
                return;
            }

            var appDirectory = Path.GetDirectoryName(appExePath) ?? string.Empty;
            if (string.IsNullOrEmpty(appDirectory))
            {
                Log("Could not determine the application directory.");
                return;
            }

            try
            {
                // Wait for the main application to exit
                Log("Waiting for the main application to exit...");
                Thread.Sleep(3000);

                // Fetch the latest release from GitHub
                var (latestVersion, assetUrl) = await GetLatestReleaseAssetUrlAsync();
        
                // Log the latest version (if needed)
                Log($"Latest version: {latestVersion}");

                if (string.IsNullOrEmpty(assetUrl))
                {
                    Log("Failed to retrieve download URL for the latest release.");
                    MessageBox.Show("Failed to retrieve the latest release. Please update manually.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Check if update.zip exists. If not, download it.
                if (!File.Exists(updateZipPath))
                {
                    Log("update.zip not found. Downloading...");
                    await DownloadUpdateFile(assetUrl, updateZipPath);
                }

                // Check if the updateSourcePath exists. If not, extract updateZipPath
                if (!Directory.Exists(updateSourcePath))
                {
                    Log("updateSourcePath not found. Extracting updateZipPath...");
                    ExtractUpdateFile(updateZipPath, updateSourcePath);
                }

                // Ensure the updateSourcePath exists after extraction
                if (!Directory.Exists(updateSourcePath))
                {
                    Log("Failed to extract update files. Update process aborted.");
                    MessageBox.Show("Failed to extract update files. Please update manually.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Files to be ignored during the update
                var ignoredFiles = new[]
                {
                    "Updater.deps.json",
                    "Updater.dll",
                    "Updater.exe",
                    "Updater.pdb",
                    "Updater.runtimeconfig.json"
                };

                // Copy new files to the application directory
                foreach (var file in Directory.GetFiles(updateSourcePath))
                {
                    Log("Copy new files to the application directory.");
                    
                    var fileName = Path.GetFileName(file);
                    if (!ignoredFiles.Contains(fileName))
                    {
                        var destFile = Path.Combine(appDirectory, fileName);
                        Log($"Copying {fileName}...");
                        File.Copy(file, destFile, true);
                    }
                }

                // Delete the temporary update files and the update.zip file
                Log("Deleting temporary update files...");
                Directory.Delete(updateSourcePath, true);
                File.Delete(updateZipPath);

                // Notify the user of a successful update
                Log("Update installed successfully. The application will now restart.");
                MessageBox.Show("Update installed successfully. The application will now restart.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Restart the main application
                var simpleLauncherExePath = Path.Combine(appDirectory, "SimpleLauncher.exe");
                var startInfo = new ProcessStartInfo
                {
                    FileName = simpleLauncherExePath,
                    UseShellExecute = false,
                    WorkingDirectory = appDirectory
                };

                Process.Start(startInfo);

                // Close the update Window
                Close();
            }
            catch (Exception ex)
            {
                Log($"Automatic update failed: {ex.Message}");
    
                // Prompt the user to be redirected to the download page
                var result = MessageBox.Show(
                    "Automatic update failed.\nDo you want to be redirected to the download page to update manually?",
                    "Update Failed",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);

                // If the user selects Yes, open the download page in the default browser
                if (result == DialogResult.Yes)
                {
                    // URL to the releases' page of the GitHub repository
                    string downloadPageUrl = $"https://github.com/{RepoOwner}/{RepoName}/releases/latest";
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadPageUrl,
                        UseShellExecute = true // UseShellExecute is required to open the URL in the default browser
                    });
                }

                // Close the update window regardless of the user's choice
                Close();
            }
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
                using JsonDocument jsonDoc = JsonDocument.Parse(jsonResponse);
                var root = jsonDoc.RootElement;

                // Get the version from the "tag_name" field
                string versionTag = root.TryGetProperty("tag_name", out JsonElement tagNameElement)
                    ? tagNameElement.GetString() ?? string.Empty  // Handle potential null here
                    : string.Empty; // Default to empty if tag_name is not found

                // Initialize assetUrl to an empty string to avoid nullability issues
                string assetUrl = string.Empty;

                if (root.TryGetProperty("assets", out JsonElement assetsElement))
                {
                    foreach (var asset in assetsElement.EnumerateArray())
                    {
                        if (asset.TryGetProperty("browser_download_url", out JsonElement downloadUrlElement))
                        {
                            assetUrl = downloadUrlElement.GetString() ?? string.Empty; // Handle potential null here
                            break;
                        }
                    }
                }

                // Match the version using the regex and return the version and asset URL
                var versionMatch = MyRegex().Match(versionTag);
                if (versionMatch.Success)
                {
                    return (NormalizeVersion(versionMatch.Value), assetUrl);
                }

                Log($"Error parsing the version from the release tag.");
                return (string.Empty, assetUrl); // Return assetUrl even if version parsing fails
            }
            catch (Exception ex)
            {
                Log($"Failed to fetch the latest release asset URL: {ex.Message}");
                return (string.Empty, string.Empty); // Return empty strings in case of an exception
            }
        }

        private void ExtractUpdateFile(string zipFilePath, string destinationDirectory)
        {
            string sevenZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "7z.exe");
            if (!File.Exists(sevenZipPath))
            {
                Log("7z.exe not found in the application directory.");
                MessageBox.Show("7z.exe not found in the application directory.\n\nPlease reinstall Simple Launcher.", "7z.exe not found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var psi = new ProcessStartInfo
            {
                FileName = sevenZipPath,
                Arguments = $"x \"{zipFilePath}\" -o\"{destinationDirectory}\" -y",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi);

            Log("Extracting the update file...");
            
            process?.WaitForExit();

            if (process != null && process.ExitCode != 0)
            {
                Log($"7z.exe exited with code {process.ExitCode}. Extraction failed.");
                MessageBox.Show("7z.exe could not extract the compressed file.\n\nMaybe the compressed file is corrupt.", "Error extracting the file", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async Task DownloadUpdateFile(string url, string destinationPath)
        {
            Log("Trying to download the latest release...");
            try
            {
                using var client = new HttpClient();
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();

                await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fileStream);

                Log("update.zip downloaded successfully.");
            }
            catch (Exception ex)
            {
                Log($"Failed to download update.zip: {ex.Message}");
                MessageBox.Show("Failed to download update.zip. Please check your internet connection and try again.", "Download Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private static Regex MyRegex() => new Regex(@"(?<=\D*)\d+(\.\d+)*", RegexOptions.Compiled);

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
    }
}