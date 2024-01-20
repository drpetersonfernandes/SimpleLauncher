using System;
using System.IO;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;


namespace SimpleLauncher
{
    public partial class UpdateChecker
    {
        private const string RepoOwner = "drpetersonfernandes";
        private const string RepoName = "SimpleLauncher";

        public static string CurrentVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

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
                        ShowUpdateDialog(releaseUrl, mainWindow);
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

        private static void ShowUpdateDialog(string releaseUrl, Window owner)
        {
            MessageBoxResult result = MessageBox.Show(owner, "There is a software update available. Do you want to download the latest version?",
                                                      "Update Available",
                                                      MessageBoxButton.YesNo,
                                                      MessageBoxImage.Information);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = releaseUrl,
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                }
                catch (Exception ex)
                {
                    LogToFile($"Failed to open the browser: {ex.Message}");
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
                var versionMatch = MyRegex().Match(versionTag);
                if (versionMatch.Success)
                {
                    return (versionMatch.Value, releaseUrl);
                }
                else
                {
                    throw new InvalidOperationException("Version number not found in tag.");
                }
            }
            else
            {
                throw new InvalidOperationException("Version information not found in the response.");
            }
        }

        private static void LogToFile(string message)
        {
            string logFilePath = "UpdateCheckerLog.txt"; // Specify your log file path here

            // Ensure that the message ends with a newline
            if (!message.EndsWith(Environment.NewLine))
            {
                message += Environment.NewLine;
            }

            File.AppendAllText(logFilePath, $"{DateTime.Now}: {message}");
        }

        [System.Text.RegularExpressions.GeneratedRegex(@"(?<=\D*)\d+(\.\d+)*")]
        private static partial System.Text.RegularExpressions.Regex MyRegex();
    }
}
