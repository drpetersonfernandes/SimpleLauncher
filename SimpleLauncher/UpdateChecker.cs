using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SimpleLauncher;

public static class UpdateChecker
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
                var unknown2 = (string)Application.Current.TryFindResource("Unknown") ?? "Unknown";
                return unknown2;
            }
        }
    }

    // Regular CheckForUpdates
    public static async Task CheckForUpdatesAsync(Window mainWindow)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "request");

            var response = await client.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var (latestVersion, assetUrl) = ParseVersionFromResponse(content);

                if (IsNewVersionAvailable(CurrentVersion, latestVersion))
                {
                    ShowUpdateWindow(assetUrl, CurrentVersion, latestVersion, mainWindow);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify user
            var contextMessage = $"Error checking for updates.\n\n" +
                                 $"Exception type: {ex.GetType().Name}\n" +
                                 $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, contextMessage);
            
            // Notify user
            // Ignore
        }
    }

    // Check for update from within About Window
    public static async Task CheckForUpdatesAsync2(Window mainWindow)
    {
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "request");

            var response = await client.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

                var (latestVersion, assetUrl) = ParseVersionFromResponse(content);

                if (IsNewVersionAvailable(CurrentVersion, latestVersion))
                {
                    ShowUpdateWindow(assetUrl, CurrentVersion, latestVersion, mainWindow);
                }
                else
                {
                    // Notify user
                    ThereIsNoUpdateAvailableMessageBox();
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage = $"Error checking for updates.\n\n" +
                                 $"Exception type: {ex.GetType().Name}\n" +
                                 $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            ErrorCheckingForUpdatesMessageBox();
        }

        void ThereIsNoUpdateAvailableMessageBox()
        {
            string thereisnoupdateavailable2 = (string)Application.Current.TryFindResource("thereisnoupdateavailable") ?? "There is no update available.";
            string thecurrentversionis2 = (string)Application.Current.TryFindResource("Thecurrentversionis") ?? "The current version is";
            string noupdateavailable2 = (string)Application.Current.TryFindResource("Noupdateavailable") ?? "No update available";
            MessageBox.Show(mainWindow, $"{thereisnoupdateavailable2}\n\n" +
                                        $"{thecurrentversionis2} {CurrentVersion}",
                noupdateavailable2, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        void ErrorCheckingForUpdatesMessageBox()
        {
            string therewasanerrorcheckingforupdates2 = (string)Application.Current.TryFindResource("Therewasanerrorcheckingforupdates") ?? "There was an error checking for updates.";
            string maybethereisaproblemwithyourinternet2 = (string)Application.Current.TryFindResource("Maybethereisaproblemwithyourinternet") ?? "Maybe there is a problem with your internet access or the GitHub server is offline.";
            string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show(mainWindow, $"{therewasanerrorcheckingforupdates2}\n\n" +
                                        $"{maybethereisaproblemwithyourinternet2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }

    public static async Task ReinstallSimpleLauncherAsync(Window mainWindow)
    {
        // Set BaseVersion
        string baseVersion = "0.0.0.0";
            
        try
        {
            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent", "request");

            var response = await client.GetAsync($"https://api.github.com/repos/{RepoOwner}/{RepoName}/releases/latest");
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var (latestVersion, assetUrl) = ParseVersionFromResponse(content);

                if (IsNewVersionAvailable(baseVersion, latestVersion))
                {
                    ShowReinstallWindow(assetUrl, mainWindow);
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string contextMessage = $"Error reinstalling 'Simple Launcher'.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            ErrorReinstallingSimpleLauncherMessageBox();
        }
        void ErrorReinstallingSimpleLauncherMessageBox()
        {
            string therewasanerrorreinstallingSimpleLauncher2 = (string)Application.Current.TryFindResource("TherewasanerrorreinstallingSimpleLauncher") ?? "There was an error reinstalling 'Simple Launcher'.";
            string theerrorwasreportedtothedeveloper2 = (string)Application.Current.TryFindResource("Theerrorwasreportedtothedeveloper") ?? "The error was reported to the developer who will try to fix the issue.";
            string error2 = (string)Application.Current.TryFindResource("Error") ?? "Error";
            MessageBox.Show(mainWindow, $"{therewasanerrorreinstallingSimpleLauncher2}\n\n" +
                                        $"{theerrorwasreportedtothedeveloper2}",
                error2, MessageBoxButton.OK, MessageBoxImage.Exclamation);
        }
    }

    // Regular call of the Update Window
    private static async void ShowUpdateWindow(string assetUrl, string currentVersion, string latestVersion, Window owner)
    {
        try
        {
            // Notify user
            var result = DoYouWantToUpdateMessageBox();
            MessageBoxResult DoYouWantToUpdateMessageBox()
            {
                string thereisasoftwareupdateavailable2 = (string)Application.Current.TryFindResource("Thereisasoftwareupdateavailable") ?? "There is a software update available.";
                string thecurrentversionis2 = (string)Application.Current.TryFindResource("Thecurrentversionis") ?? "The current version is";
                string theupdateversionis2 = (string)Application.Current.TryFindResource("Theupdateversionis") ?? "The update version is";
                string doyouwanttodownloadandinstall2 = (string)Application.Current.TryFindResource("Doyouwanttodownloadandinstall") ?? "Do you want to download and install the latest version automatically?";
                string updateAvailable2 = (string)Application.Current.TryFindResource("UpdateAvailable") ?? "Update Available";
                string message = $"{thereisasoftwareupdateavailable2}\n" +
                                 $"{thecurrentversionis2} {currentVersion}\n" +
                                 $"{theupdateversionis2} {latestVersion}\n\n" +
                                 $"{doyouwanttodownloadandinstall2}";
                MessageBoxResult messageBoxResult1 = MessageBox.Show(owner, message,
                    updateAvailable2, MessageBoxButton.YesNo, MessageBoxImage.Information);
                return messageBoxResult1;
            }

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
                    
                    logWindow.Log("Downloading update file...");

                    await Task.Run(async () =>
                    {
                        using var memoryStream = new MemoryStream();
                        
                        // Download the update file to memory
                        await DownloadUpdateFileToMemory(assetUrl, memoryStream);
                        
                        logWindow.Log("Extracting update file...");
                        
                        // Files to be updated
                        var updaterFiles = new[]
                        {
                            "Updater.deps.json",
                            "Updater.dll",
                            "Updater.exe",
                            "Updater.pdb",
                            "Updater.runtimeconfig.json"
                        };

                        // Extract directly from memory to the destination
                        ExtractFilesToDestination(memoryStream, appDirectory, updaterFiles, logWindow);

                        logWindow.Log("Update completed successfully.");

                        await Task.Delay(2000);
                        
                        // Execute Updater
                        await ExecuteUpdater(logWindow);
                    });
                }
                catch (Exception ex)
                {
                    // Notify developer
                    string contextMessage = $"There was an error updating the application.\n\n" +
                                            $"Exception type: {ex.GetType().Name}\n" +
                                            $"Exception details: {ex.Message}";
                    await LogErrors.LogErrorAsync(ex, contextMessage);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        // Notify user
                        MessageBoxLibrary.InstallUpdateManuallyMessageBox(RepoOwner, RepoName);

                        logWindow.Log("There was an error updating the application.");
                        logWindow.Log("Please update it manually.");
                        logWindow.Close();
                    });
                }
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string contextMessage = $"There was an error updating the application.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, contextMessage);
            
            // Notify user
            // Ignore
        }
    }

    private static async void ShowReinstallWindow(string assetUrl, Window owner)
    {
        try
        {
            var logWindow = new UpdateLogWindow();
            logWindow.Show();
            logWindow.Log("Starting the installation ...");

            // Close the main window
            owner.Close();

            try
            {
                string appDirectory = AppDomain.CurrentDomain.BaseDirectory;

                logWindow.Log("Downloading installation file...");
                    
                await Task.Run(async () =>
                {
                    // Download the update file to memory
                    using var memoryStream = new MemoryStream();
                    await DownloadUpdateFileToMemory(assetUrl, memoryStream);
                        
                    logWindow.Log("Extracting installation file...");

                    // Files to be updated
                    var updaterFiles = new[]
                    {
                        "Updater.deps.json",
                        "Updater.dll",
                        "Updater.exe",
                        "Updater.pdb",
                        "Updater.runtimeconfig.json"
                    };
                        
                    // Extract directly from memory to the destination
                    ExtractFilesToDestination(memoryStream, appDirectory, updaterFiles, logWindow);
                    
                    logWindow.Log("Installation completed successfully.");

                    await Task.Delay(2000);

                    // Execute Updater
                    await ExecuteUpdater(logWindow);
                });
            }
            catch (Exception ex)
            {
                // Notify developer
                string contextMessage = $"There was an error installing the application.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, contextMessage);
                    
                Application.Current.Dispatcher.Invoke(() =>
                {
                    // Notify user
                    MessageBoxLibrary.InstallUpdateManuallyMessageBox(RepoOwner, RepoName);

                    logWindow.Log("There was an error updating the application.");
                    logWindow.Log("Please update it manually.");
                });
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string contextMessage = $"There was an error installing the application.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, contextMessage);
            
            // Notify user
            // Ignore
        }
    }
    
    private static async Task DownloadUpdateFileToMemory(string url, MemoryStream memoryStream)
    {
        using var client = new HttpClient();
        using var response = await client.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync();
        await stream.CopyToAsync(memoryStream);
    }

    private static void ExtractFilesToDestination(Stream zipStream, string destinationPath, string[] filesToExtract, UpdateLogWindow logWindow)
    {
        using var archive = new System.IO.Compression.ZipArchive(zipStream, System.IO.Compression.ZipArchiveMode.Read);

        foreach (var fileName in filesToExtract)
        {
            var entry = archive.GetEntry(fileName);
            if (entry != null)
            {
                string destinationFile = Path.Combine(destinationPath, fileName);
                logWindow.Log($"Extracting {fileName} to {destinationFile}");

                using var entryStream = entry.Open();
                using var fileStream = new FileStream(destinationFile, FileMode.Create, FileAccess.Write);
                entryStream.CopyTo(fileStream);
            }
            else
            {
                logWindow.Log($"File {fileName} not found in the archive.");
            }
        }
    }
    
    private static async Task ExecuteUpdater(UpdateLogWindow logWindow)
    {
        string appDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string appExePath = Assembly.GetExecutingAssembly().Location;
        string updaterExePath = Path.Combine(appDirectory, "Updater.exe");

        if (!File.Exists(updaterExePath))
        {
            logWindow.Log("Updater.exe not found in the application directory.");
            logWindow.Log("Please reinstall 'Simple Launcher' manually to fix the issue.");
    
            Application.Current.Dispatcher.Invoke(() =>
            {
                // Notify user
                MessageBoxLibrary.DownloadManuallyMessageBox(RepoOwner, RepoName);

                logWindow.Close();
            });
            return;
        }

        logWindow.Log("Starting updater process...");
        await Task.Delay(2000);
                        
        Process.Start(new ProcessStartInfo
        {
            FileName = updaterExePath,
            Arguments = appExePath,
            UseShellExecute = false
        });

        logWindow.Log("Closing main application for update...");

        // Close Simple Launcher
        Application.Current.Dispatcher.Invoke(() =>
        {
            foreach (Window window in Application.Current.Windows)
            {
                window.Close();  // Close each window
            }
                            
            GC.Collect(); // Force garbage collection
            GC.WaitForPendingFinalizers(); // Wait for finalizers to complete
            Application.Current.Shutdown(); // Shutdown the application
            Process.GetCurrentProcess().Kill(); // Forcefully kill the process
        });
    }
  
    private static bool IsNewVersionAvailable(string currentVersion, string latestVersion)
    {
        Version current = new Version(Regex.Replace(currentVersion, @"[^\d\.]", ""));
        Version latest = new Version(Regex.Replace(latestVersion, @"[^\d\.]", ""));
        int versionComparison = latest.CompareTo(current);

        if (versionComparison > 0) return true;

        return false;
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

            LogErrorAsync("There was an error parsing the application version from the UpdateChecker class. Version number was not found in the tag.");
        }
        else
        {
            LogErrorAsync("There was an error parsing the application version from the UpdateChecker class. Version information not found in the response.");
        }

        return (null, null);
    }
    
    private static Regex MyRegex() => new Regex(@"(?<=release(?:-[a-zA-Z0-9]+)?-?)\d+(\.\d+)*", RegexOptions.Compiled);
    
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
    
    private static void LogErrorAsync(string message)
    {
        Exception exception = new(message);
        _ = LogErrors.LogErrorAsync(exception, message);
    }
}