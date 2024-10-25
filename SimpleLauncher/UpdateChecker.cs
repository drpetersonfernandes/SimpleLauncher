using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleLauncher
{
    public class UpdateChecker
    {
        private const string RepoOwner = "drpetersonfernandes";
        private const string RepoName = "SimpleLauncher";

        private static string CurrentVersion
        {
            get
            {
                try
                {
                    return NormalizeVersion(Assembly.GetExecutingAssembly().GetName().Version?.ToString());
                }
                catch
                {
                    return "Unknown";
                }
            }
        }

        public static async Task CheckForUpdatesAsync(Window mainWindow)
        {
            try
            {
                // Define the path to the update.zip file
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string updateZipPath = Path.Combine(appDirectory, "update.zip");

                // Delete the update.zip file if it exists
                if (File.Exists(updateZipPath))
                {
                    File.Delete(updateZipPath);
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "request");

                var response = await client.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var (latestVersion, assetUrl) = ParseVersionFromResponse(content);

                    if (IsNewVersionAvailable(CurrentVersion, latestVersion))
                    {
                        ShowUpdateDialog(assetUrl, CurrentVersion, latestVersion, mainWindow);
                    }
                }
            }
            catch (Exception ex)
            {
                string contextMessage = $"Error checking for updates.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, contextMessage);
            }
        }

        public static async Task CheckForUpdatesAsync2(Window mainWindow)
        {
            try
            {
                // Define the path to the update.zip file
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string updateZipPath = Path.Combine(appDirectory, "update.zip");

                // Delete the update.zip file if it exists
                if (File.Exists(updateZipPath))
                {
                    File.Delete(updateZipPath);
                }

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "request");

                var response = await client.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();

                    var (latestVersion, assetUrl) = ParseVersionFromResponse(content);

                    if (IsNewVersionAvailable(CurrentVersion, latestVersion))
                    {
                        ShowUpdateDialog(assetUrl, CurrentVersion, latestVersion, mainWindow);
                    }
                    else
                    {
                        // If no new version is available, show a message box with the current version
                        MessageBox.Show(mainWindow, $"There is no update available.\n\nThe current version is {CurrentVersion}", "No update available", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                string contextMessage = $"Error checking for updates.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, contextMessage);
                
                MessageBox.Show(mainWindow, $"There was an error checking for updates.\n\nMaybe there is a problem with your internet access or the GitHub server is offline.", "Error checking for updates", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            }
        }

        private static bool IsNewVersionAvailable(string currentVersion, string latestVersion)
        {
            Version current = new Version(Regex.Replace(currentVersion, @"[^\d\.]", ""));
            Version latest = new Version(Regex.Replace(latestVersion, @"[^\d\.]", ""));
            int versionComparison = latest.CompareTo(current);

            if (versionComparison > 0) return true;

            return false;
        }

        private static async void ShowUpdateDialog(string assetUrl, string currentVersion, string latestVersion, Window owner)
        {
            string message = $"There is a software update available.\n" +
                             $"The current version is {currentVersion}\n" +
                             $"The update version is {latestVersion}\n\n" +
                             "Do you want to download and install the latest version automatically?";

            MessageBoxResult result = MessageBox.Show(owner, message, "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                var logWindow = new UpdateLogWindow();
                logWindow.Show();
                logWindow.Log("Starting update process...");

                // Close the main window
                owner.Close();

                try
                {
                    string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string tempDirectory = Path.Combine(appDirectory, "temp2");
                    Directory.CreateDirectory(tempDirectory);

                    string tempFilePath = Path.Combine(tempDirectory, "update.zip");
                    logWindow.Log("Downloading update file...");

                    await Task.Run(async () =>
                    {
                        await DownloadUpdateFile(assetUrl, tempFilePath);
                        logWindow.Log("Extracting update file...");
                        ExtractUpdateFile(tempFilePath, tempDirectory);

                        var updaterFiles = new[]
                        {
                            "Updater.deps.json",
                            "Updater.dll",
                            "Updater.exe",
                            "Updater.pdb",
                            "Updater.runtimeconfig.json"
                        };
                        
                        logWindow.Log("Updating the updater app...");

                        foreach (var file in updaterFiles)
                        {
                            var sourceFile = Path.Combine(tempDirectory, file);
                            var destFile = Path.Combine(appDirectory, file);
                            if (File.Exists(sourceFile))
                            {
                                File.Copy(sourceFile, destFile, true);
                                logWindow.Log($"Copied {file}");
                            }
                        }

                        await Task.Delay(2000);

                        string appExePath = Assembly.GetExecutingAssembly().Location;
                        string updaterExePath = Path.Combine(appDirectory, "Updater.exe");

                        if (!File.Exists(updaterExePath))
                        {
                            logWindow.Log("Updater.exe not found in the application directory.\n\nPlease reinstall Simple Launcher manually.");
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show(logWindow, "Updater.exe not found in the application directory.\n\nPlease reinstall Simple Launcher manually.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                                logWindow.Close();
                            });
                            return;
                        }

                        logWindow.Log("Starting updater process...");
                        await Task.Delay(2000);
                        
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = updaterExePath,
                            Arguments = $"\"{appExePath}\" \"{tempDirectory}\" \"{tempFilePath}\"",
                            UseShellExecute = false
                        });

                        logWindow.Log("Closing application for update...");
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            foreach (Window window in Application.Current.Windows)
                            {
                                window.Close();  // Close each window manually
                            }
                            
                            GC.Collect();       // Force garbage collection
                            GC.WaitForPendingFinalizers();  // Wait for finalizers to complete

                            Application.Current.Shutdown();  // Shutdown the application
                            
                            // Forcefully kill the process to ensure all threads and handles are released
                            Process.GetCurrentProcess().Kill();
                        });
                    });
                }
                catch (Exception ex)
                {
                    string contextMessage = $"There was an error updating the application.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
                    await LogErrors.LogErrorAsync(ex, contextMessage);
                    
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Ask user if they want to be redirected to the download page
                        var messageBoxResult = MessageBox.Show(
                            "There was an error updating the application.\n\nWould you like to be redirected to the download page to update manually?",
                            "Update Error",
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Error);

                        if (messageBoxResult == MessageBoxResult.Yes)
                        {
                            // Redirect to the download page
                            string downloadPageUrl = $"https://github.com/{RepoOwner}/{RepoName}/releases/latest";
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = downloadPageUrl,
                                UseShellExecute = true // Open URL in default browser
                            });
                        }

                        logWindow.Log($"There was an error updating the application.\n\nPlease update it manually");
                        logWindow.Close();
                    });

                }
            }
        }

        private static async Task DownloadUpdateFile(string url, string destinationPath)
        {
            using var client = new HttpClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            await using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None);
            await response.Content.CopyToAsync(fileStream);
        }

        private static void ExtractUpdateFile(string zipFilePath, string destinationDirectory)
        {
            string sevenZipPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "7z.exe");
            if (!File.Exists(sevenZipPath))
            {
                string formattedException = $"7z.exe not found in the application directory.";
                Exception ex = new(formattedException);
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));
                
                MessageBox.Show("7z.exe not found in the application directory.\n\nPlease reinstall Simple Launcher.","7z.exe not found", MessageBoxButton.OK, MessageBoxImage.Error);
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
            process?.WaitForExit();

            if (process != null && process.ExitCode != 0)
            {
                string formattedException = $"7z.exe exited with code {process.ExitCode}.";
                Exception ex = new(formattedException);
                Task logTask = LogErrors.LogErrorAsync(ex, formattedException);
                logTask.Wait(TimeSpan.FromSeconds(2));

                MessageBox.Show("7z.exe could not extract the compressed file.\n\nMaybe the compressed file is corrupt.", "Error extracting the file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static (string version, string assetUrl) ParseVersionFromResponse(string jsonResponse)
        {
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("tag_name", out JsonElement tagNameElement) &&
                root.TryGetProperty("assets", out JsonElement assetsElement))
            {
                string versionTag = tagNameElement.GetString();
                string assetUrl = null;
                foreach (var asset in assetsElement.EnumerateArray())
                {
                    if (asset.TryGetProperty("browser_download_url", out JsonElement downloadUrlElement))
                    {
                        assetUrl = downloadUrlElement.GetString();
                        break;
                    }
                }

                var versionMatch = MyRegex().Match(versionTag ?? string.Empty);
                if (versionMatch.Success)
                {
                    return (NormalizeVersion(versionMatch.Value), assetUrl);
                }

                LogErrorAsync(
                    $"There was an error parsing the application version from the UpdateChecker class.\n\nVersion number was not found in the tag.");
            }
            else
            {
                LogErrorAsync(
                    $"There was an error parsing the application version from the UpdateChecker class.\n\nVersion information not found in the response.");
            }

            return (null, null);
        }

        private static void LogErrorAsync(string message)
        {
            Exception exception = new(message);
            _ = LogErrors.LogErrorAsync(exception, message);
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