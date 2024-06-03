using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
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
            catch (Exception exception)
            {
                string contextMessage = $"Error checking for updates.\n\nException details: {exception}";
                Task logTask = LogErrors.LogErrorAsync(exception, contextMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
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
                        MessageBox.Show(mainWindow, $"There is no update available.\n\nThe current version is {CurrentVersion}", "No Update Available", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception exception)
            {
                string contextMessage = $"Error checking for updates.\n\nException details: {exception}";
                Task logTask = LogErrors.LogErrorAsync(exception, contextMessage);
                logTask.Wait(TimeSpan.FromSeconds(2));
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

        // private static async void ShowUpdateDialog(string assetUrl, string currentVersion, string latestVersion, Window owner)
        // {
        //     string message = $"There is a software update available.\n" +
        //                      $"The current version is {currentVersion}\n" +
        //                      $"The update version is {latestVersion}\n\n" +
        //                      "Do you want to download and install the latest version automatically?";
        //
        //     MessageBoxResult result = MessageBox.Show(owner, message, "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);
        //
        //     if (result == MessageBoxResult.Yes)
        //     {
        //         try
        //         {
        //             string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        //             string tempDirectory = Path.Combine(appDirectory, "temp");
        //             Directory.CreateDirectory(tempDirectory);
        //
        //             string tempFilePath = Path.Combine(tempDirectory, "update.zip");
        //             await DownloadUpdateFile(assetUrl, tempFilePath);
        //             ExtractUpdateFile(tempFilePath, tempDirectory);
        //
        //             string appExePath = Assembly.GetExecutingAssembly().Location;
        //             string updaterExePath = Path.Combine(appDirectory, "Updater.exe");
        //
        //             if (!File.Exists(updaterExePath))
        //             {
        //                 MessageBox.Show(owner, "Updater.exe not found in the application directory.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //                 return;
        //             }
        //
        //             // Start the updater process
        //             Process.Start(new ProcessStartInfo
        //             {
        //                 FileName = updaterExePath,
        //                 Arguments = $"\"{appExePath}\" \"{tempDirectory}\" \"{tempFilePath}\" \"{Environment.CommandLine}\"",
        //                 UseShellExecute = false
        //             });
        //
        //             // Close the main application
        //             Application.Current.Shutdown();
        //         }
        //         catch (Exception exception)
        //         {
        //             string contextMessage = $"There was an error updating the application.\n\nException details: {exception}";
        //             Task logTask = LogErrors.LogErrorAsync(exception, contextMessage);
        //             MessageBox.Show(contextMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //             logTask.Wait(TimeSpan.FromSeconds(2));
        //         }
        //     }
        // }
        
        private static async void ShowUpdateDialog(string assetUrl, string currentVersion, string latestVersion, Window owner)
        {
            string message = $"There is a software update available.\n" +
                             $"The current version is {currentVersion}\n" +
                             $"The update version is {latestVersion}\n\n" +
                             "Do you want to download and install the latest version automatically?";

            MessageBoxResult result = MessageBox.Show(owner, message, "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string tempDirectory = Path.Combine(appDirectory, "temp");
                    Directory.CreateDirectory(tempDirectory);

                    string tempFilePath = Path.Combine(tempDirectory, "update.zip");
                    await DownloadUpdateFile(assetUrl, tempFilePath);
                    ExtractUpdateFile(tempFilePath, tempDirectory);

                    // Copy updater files to the application directory
                    var updaterFiles = new[]
                    {
                        "Updater.deps.json",
                        "Updater.dll",
                        "Updater.exe",
                        "Updater.pdb",
                        "Updater.runtimeconfig.json"
                    };

                    foreach (var file in updaterFiles)
                    {
                        var sourceFile = Path.Combine(tempDirectory, file);
                        var destFile = Path.Combine(appDirectory, file);
                        if (File.Exists(sourceFile))
                        {
                            File.Copy(sourceFile, destFile, true);
                        }
                    }
            
                    Thread.Sleep(2000); // Ensure time for file operations to complete

                    string appExePath = Assembly.GetExecutingAssembly().Location;
                    string updaterExePath = Path.Combine(appDirectory, "Updater.exe");

                    if (!File.Exists(updaterExePath))
                    {
                        MessageBox.Show(owner, "Updater.exe not found in the application directory.\nPlease update manually.", "Update Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    // Start the updater process
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = updaterExePath,
                        Arguments = $"\"{appExePath}\" \"{tempDirectory}\" \"{tempFilePath}\" \"{Environment.CommandLine}\"",
                        UseShellExecute = false
                    });

                    // Close the main application
                    Application.Current.Shutdown();
                }
                catch (Exception exception)
                {
                    string contextMessage = $"There was an error updating the application.\n\nPlease update manually.\n\nException details: {exception}";
                    Task logTask = LogErrors.LogErrorAsync(exception, contextMessage);
                    MessageBox.Show(contextMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    logTask.Wait(TimeSpan.FromSeconds(2));
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
                throw new FileNotFoundException("7z.exe not found in the application directory.");
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
                throw new InvalidOperationException($"7z.exe exited with code {process.ExitCode}");
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

                throw new InvalidOperationException("Version number not found in tag.");
            }

            throw new InvalidOperationException("Version information not found in the response.");
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