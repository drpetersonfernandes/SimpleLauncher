using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;

namespace SimpleLauncher
{
    public class LogErrors
    {
        private static readonly object LockObject = new();
        private static readonly HttpClient HttpClient = new();

        public static async Task LogErrorAsync(Exception ex, string contextMessage = null)
        {
            string errorLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");

            // Get application version
            var version = Assembly.GetExecutingAssembly().GetName().Version!.ToString();

            string errorMessage = $"Date: {DateTime.Now}\nVersion: {version}\nContext: {contextMessage}\nException Details:\n{ex}\n\n";

            // Attempt to write to the log file, ignoring exceptions if the file cannot be accessed
            try
            {
                lock (LockObject)
                {
                    File.AppendAllText(errorLogPath, errorMessage);
                }

                // Check the result of SendLogToApiAsync and delete the log file if successful
                if (await SendLogToApiAsync())
                {
                    File.Delete(errorLogPath);
                }
            }
            catch
            {
                // If an exception occurs while accessing the log file, simply ignore it and return.
            }
        }

        private static async Task<bool> SendLogToApiAsync()
        {
            string errorLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            if (!File.Exists(errorLogPath))
            {
                //LogInternalError("Error log file not found.");
                return false;
            }

            string logContent = await File.ReadAllTextAsync(errorLogPath);

            // Prepare the POST data
            var formData = new MultipartFormDataContent
            {
                { new StringContent("contact@purelogiccode.com"), "recipient" },
                { new StringContent("Error Log from SimpleLauncher"), "subject" },
                { new StringContent("SimpleLauncher User"), "name" },
                { new StringContent(logContent), "message" }
            };

            // Set the API Key
            if (!HttpClient.DefaultRequestHeaders.Contains("X-API-KEY"))
            {
                HttpClient.DefaultRequestHeaders.Add("X-API-KEY", "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e");
            }

            try
            {
                // Send the POST request
                HttpResponseMessage response = await HttpClient.PostAsync("https://purelogiccode.com/simplelauncher/send_email.php", formData);

                return response.IsSuccessStatusCode;
            }
            catch
            {
                //LogInternalError($"Exception occurred while sending log to the developer: {ex.Message}");
                return false;
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