using System;
using System.Diagnostics;
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

        private static string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version!.ToString();

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

                    var (latestVersion, releaseUrl) = ParseVersionFromResponse(content);

                    if (IsNewVersionAvailable(CurrentVersion, latestVersion))
                    {
                        ShowUpdateDialog(releaseUrl, CurrentVersion, latestVersion, mainWindow);
                    }
                }
            }
            catch
            {
                // Silent fail
            }
        }

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

                    var (latestVersion, releaseUrl) = ParseVersionFromResponse(content);

                    if (IsNewVersionAvailable(CurrentVersion, latestVersion))
                    {
                        ShowUpdateDialog(releaseUrl, CurrentVersion, latestVersion, mainWindow);
                    }
                    else
                    {
                        // If no new version is available, show a message box with the current version
                        MessageBox.Show(mainWindow, $"There is no update available.\nThe current version is {CurrentVersion}", "No Update Available", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch
            {
                // Silent fail
            }
        }

        private static bool IsNewVersionAvailable(string currentVersion, string latestVersion)
        {
            return new Version(latestVersion).CompareTo(new Version(currentVersion)) > 0;
        }

        private static void ShowUpdateDialog(string releaseUrl, string currentVersion, string latestVersion, Window owner)
        {
            string message = $"There is a software update available.\n" +
                             $"The current version is {currentVersion}.\n" +
                             $"The update version is {latestVersion}.\n" +
                             "Do you want to download the latest version?";

            MessageBoxResult result = MessageBox.Show(owner, message, "Update Available", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var psi = new ProcessStartInfo
                    {
                        FileName = releaseUrl,
                        UseShellExecute = true
                    };
                    Process.Start(psi);
                }
                catch
                {
                    // Silent fail
                }
            }
        }

        private static (string version, string url) ParseVersionFromResponse(string jsonResponse)
        {
            using JsonDocument doc = JsonDocument.Parse(jsonResponse);
            JsonElement root = doc.RootElement;

            if (root.TryGetProperty("tag_name", out JsonElement tagNameElement) &&
                root.TryGetProperty("html_url", out JsonElement htmlUrlElement))
            {
                string versionTag = tagNameElement.GetString();
                string releaseUrl = htmlUrlElement.GetString();

                // This regex matches a sequence of numbers (and periods), optionally prefixed by non-digit characters
                var versionMatch = MyRegex().Match(versionTag!);
                if (versionMatch.Success)
                {
                    return (versionMatch.Value, releaseUrl);
                }

                throw new InvalidOperationException("Version number not found in tag.");
            }

            throw new InvalidOperationException("Version information not found in the response.");
        }

        private static Regex MyRegex() => new Regex(@"(?<=\D*)\d+(\.\d+)*", RegexOptions.Compiled);

    }
}
