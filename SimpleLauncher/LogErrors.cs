using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Reflection;

namespace SimpleLauncher
{
    public class LogErrors
    {
        private static readonly object _lockObject = new();
        private static readonly HttpClient _httpClient = new();

        public static async Task LogErrorAsync(Exception ex, string contextMessage = null)
        {
            string errorLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");

            // Get application version
            var version = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            string errorMessage = $"Date: {DateTime.Now}\nVersion: {version}\nContext: {contextMessage}\nException Details:\n{ex}\n\n";

            lock (_lockObject)
            {
                File.AppendAllText(errorLogPath, errorMessage);
            }

            await SendLogToApiAsync();
        }

        public static async Task SendLogToApiAsync()
        {
            string errorLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            if (!File.Exists(errorLogPath))
            {
                //LogInternalError("Error log file not found.");
                return;
            }

            string logContent = File.ReadAllText(errorLogPath);

            // Prepare the POST data
            var formData = new MultipartFormDataContent
            {
                { new StringContent("contact@purelogiccode.com"), "recipient" },
                { new StringContent("Error Log"), "subject" },
                { new StringContent("SimpleLauncher"), "name" },
                { new StringContent(logContent), "message" }
            };

            // Set the API Key
            if (!_httpClient.DefaultRequestHeaders.Contains("X-API-KEY"))
            {
                _httpClient.DefaultRequestHeaders.Add("X-API-KEY", "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e");
            }

            try
            {
                // Send the POST request
                HttpResponseMessage response = await _httpClient.PostAsync("https://purelogiccode.com/simplelauncher/send_email.php", formData);

                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("An error occurred while launching the game. The error has been logged and sent to the developer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else if (!response.IsSuccessStatusCode)
                {
                    //LogInternalError($"Failed to send log to the developer. Status code: {response.StatusCode}");
                    return;
                }
            }
            catch
            {
                //LogInternalError($"Exception occurred while sending log to the developer: {ex.Message}");
                return;
            }
        }

        //private static void LogInternalError(string message)
        //{
        //    string internalLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "internal_error.log");
        //    string logMessage = $"Date: {DateTime.Now}\n{message}\n\n";
        //    lock (_lockObject)
        //    {
        //        File.AppendAllText(internalLogPath, logMessage);
        //    }
        //}
    }
}