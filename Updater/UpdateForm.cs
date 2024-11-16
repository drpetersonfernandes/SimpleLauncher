using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Reflection;
using System.IO.Compression;

namespace Updater
{
    public partial class UpdateForm : Form
    {
        private delegate void LogDelegate(string message);
        private const string RepoOwner = "drpetersonfernandes";
        private const string RepoName = "SimpleLauncher";
        static readonly string AppDirectory = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string TempDirectory = Path.Combine(AppDirectory, "temp2");
        private static readonly string UpdateFile = Path.Combine(AppDirectory, "update.zip");

        public UpdateForm()
        {
            InitializeComponent();
            EnsureDirectories();

            string applicationVersion = GetApplicationVersion();
            Log($"Updater version: {applicationVersion}\n\n");
        }

        private static string GetApplicationVersion()
        {
            // Retrieve the version from the executing assembly
            Version? version = Assembly.GetExecutingAssembly().GetName().Version;
        
            // Check if the version is null, and format it as needed
            return version != null ? version.ToString() : "Version not available";
        }

        private static void EnsureDirectories()
        {
            if (!Directory.Exists(TempDirectory))
            {
                Directory.CreateDirectory(TempDirectory);
            }
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
            if (string.IsNullOrEmpty(AppDirectory))
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
                Log("Fetch the latest release from GitHub.");
                var (latestVersion, assetUrl) = await GetLatestReleaseAssetUrlAsync();
                Log($"Latest version: {latestVersion}");

                if (string.IsNullOrEmpty(assetUrl))
                {
                    Log("Failed to retrieve download URL for the latest release.");

                    // Ask user if they want to be redirected to the download page
                    var result = MessageBox.Show(
                        "Failed to retrieve the latest release. Would you like to be redirected to the download page to update manually?",
                        "Error", MessageBoxButtons.YesNo, MessageBoxIcon.Error);

                    if (result == DialogResult.Yes)
                    {
                        // Redirect to the download page
                        string downloadPageUrl = $"https://github.com/{RepoOwner}/{RepoName}/releases/latest";
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = downloadPageUrl,
                            UseShellExecute = true // Open URL in default browser
                        });
                    }
                    return;
                }

                await DownloadUpdateFile(assetUrl, UpdateFile);

                ExtractUpdateFile(UpdateFile, TempDirectory);

                // Ensure the TempDirectory exists after extraction
                if (!Directory.Exists(TempDirectory))
                {
                    Log("Failed to extract update files. Update process aborted.");
    
                    // Ask user if they want to be redirected to the download page
                    var result = MessageBox.Show(
                        "Failed to extract update files.\n\n" +
                        "Would you like to be redirected to the download page to update manually?",
                        "Error",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error);

                    if (result == DialogResult.Yes)
                    {
                        // Redirect to the download page
                        string downloadPageUrl = $"https://github.com/{RepoOwner}/{RepoName}/releases/latest";
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = downloadPageUrl,
                            UseShellExecute = true // Open URL in the default browser
                        });
                    }
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
                foreach (var file in Directory.GetFiles(TempDirectory))
                {
                    Log("Copy new files to the application directory.");
                    
                    var fileName = Path.GetFileName(file);
                    if (!ignoredFiles.Contains(fileName))
                    {
                        var destFile = Path.Combine(AppDirectory, fileName);
                        Log($"Copying {fileName}...");
                        File.Copy(file, destFile, true);
                    }
                }
                
                // Copy directories recursively
                foreach (var directory in Directory.GetDirectories(TempDirectory, "*", SearchOption.AllDirectories))
                {
                    // Calculate the relative path of the directory to retain the folder structure
                    var relativePath = Path.GetRelativePath(TempDirectory, directory);
                    var destDir = Path.Combine(AppDirectory, relativePath);

                    // Create the destination directory if it doesn't exist
                    if (!Directory.Exists(destDir))
                    {
                        Directory.CreateDirectory(destDir);
                        Log($"Created directory {destDir}");
                    }

                    // Copy all files in the current directory
                    foreach (var file in Directory.GetFiles(directory))
                    {
                        var fileName = Path.GetFileName(file);
                        if (!ignoredFiles.Contains(fileName))
                        {
                            var destFile = Path.Combine(destDir, fileName);
                            Log($"Copying {fileName} to {destDir}...");
                            File.Copy(file, destFile, true);
                        }
                    }
                }

                // Delete the temporary files and folders
                Log("Deleting temporary update files...");

                if (File.Exists(UpdateFile))
                {
                    File.Delete(UpdateFile);
                    Log($"Deleted file: {UpdateFile}");
                }

                if (Directory.Exists(TempDirectory))
                {
                    Directory.Delete(TempDirectory, true);
                    Log($"Deleted folder: {TempDirectory}");
                }

                // Notify the user of a successful update
                Log("Update installed successfully. The application will now restart.");
                MessageBox.Show("Update installed successfully.\n\nThe application will now restart.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

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

                // Close the update Window
                Close();
            }
            catch (Exception ex)
            {
                Log($"Automatic update failed: {ex.Message}");
    
                // Prompt the user to be redirected to the download page
                var result = MessageBox.Show(
                    "Automatic update failed.\n\nDo you want to be redirected to the download page to update manually?",
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

                Log("Error parsing the version from the release tag.");
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
            if (!File.Exists(zipFilePath))
            {
                Log("Zip file not found.");

                // Ask user if they want to be redirected to the download page
                var result = MessageBox.Show(
                    "The specified zip file was not found in the provided path.\n\n" +
                    "'Simple Launcher' won't be able to automatically update to the new version.\n\n" +
                    "Would you like to be redirected to the download page to download it manually?",
                    "Update Failure",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);
                
                if (result == DialogResult.Yes)
                {
                    // Redirect to the download page
                    string downloadPageUrl = $"https://github.com/{RepoOwner}/{RepoName}/releases/latest";
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadPageUrl,
                        UseShellExecute = true
                    });
                }
                return;
            }

            try
            {
                Log("Extracting the update file...");

                // Ensure the destination directory exists
                if (!Directory.Exists(destinationDirectory))
                {
                    Directory.CreateDirectory(destinationDirectory);
                }

                // Extract the zip file
                ZipFile.ExtractToDirectory(zipFilePath, destinationDirectory, overwriteFiles: true);

                Log("Extraction completed successfully.");
            }
            catch (InvalidDataException)
            {
                Log("Invalid or corrupted zip file.");

                // Ask user if they want to be redirected to the download page
                var result = MessageBox.Show(
                    "The update file appears to be corrupted or invalid.\n\n" +
                    "'Simple Launcher' won't be able to automatically update to the new version.\n\n" +
                    "Would you like to be redirected to the download page to download it manually?",
                    "Update Failure",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);
                
                if (result == DialogResult.Yes)
                {
                    // Redirect to the download page
                    string downloadPageUrl = $"https://github.com/{RepoOwner}/{RepoName}/releases/latest";
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadPageUrl,
                        UseShellExecute = true
                    });
                }
            }
            catch (IOException ioEx)
            {
                Log($"Error during extraction: {ioEx.Message}");

                // Ask user if they want to be redirected to the download page
                var result = MessageBox.Show(
                    "An error occurred during the extraction process.\n\n" +
                    "'Simple Launcher' won't be able to automatically update to the new version.\n\n" +
                    "Would you like to be redirected to the download page to download it manually?",
                    "Update Failure",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);
                
                if (result == DialogResult.Yes)
                {
                    // Redirect to the download page
                    string downloadPageUrl = $"https://github.com/{RepoOwner}/{RepoName}/releases/latest";
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadPageUrl,
                        UseShellExecute = true
                    });
                }
            }
            catch (UnauthorizedAccessException)
            {
                Log("Permission error: Unauthorized access to the destination folder.");

                // Ask user if they want to be redirected to the download page
                var result = MessageBox.Show(
                    "Permission error: Unable to access the destination folder.\n\n" +
                    "You can try to run with administrative privileges.\n\n" +
                    "'Simple Launcher' won't be able to automatically update to the new version.\n\n" +
                    "Would you like to be redirected to the download page to download it manually?",
                    "Update Failure",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);
                
                if (result == DialogResult.Yes)
                {
                    // Redirect to the download page
                    string downloadPageUrl = $"https://github.com/{RepoOwner}/{RepoName}/releases/latest";
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadPageUrl,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                Log($"Unexpected error: {ex.Message}");

                // Ask user if they want to be redirected to the download page
                var result = MessageBox.Show(
                    "An unexpected error occurred.\n\n" +
                    "'Simple Launcher' won't be able to automatically update to the new version.\n\n" +
                    "Would you like to be redirected to the download page to download it manually?",
                    "Update Failure",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);
                
                if (result == DialogResult.Yes)
                {
                    // Redirect to the download page
                    string downloadPageUrl = $"https://github.com/{RepoOwner}/{RepoName}/releases/latest";
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadPageUrl,
                        UseShellExecute = true
                    });
                }
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
    
                // Ask user if they want to be redirected to the download page
                var result = MessageBox.Show(
                    "Failed to download update.zip.\n\nPlease check your internet connection and try again.\n\nWould you like to be redirected to the download page to download it manually?",
                    "Download Failed",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Error);

                if (result == DialogResult.Yes)
                {
                    // Redirect to the download page
                    string downloadPageUrl = $"https://github.com/{RepoOwner}/{RepoName}/releases/latest";
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadPageUrl,
                        UseShellExecute = true // Open URL in the default browser
                    });
                }
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