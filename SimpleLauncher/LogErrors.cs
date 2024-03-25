using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Reflection;

namespace SimpleLauncher
{
    public class LogErrors
    {
        private static readonly HttpClient HttpClient = new();

        public static async Task LogErrorAsync(Exception ex, string contextMessage = null)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            string errorLogPath = Path.Combine(baseDirectory, "error.log");
            string userLogPath = Path.Combine(baseDirectory, "error_user.log");
            var version = Assembly.GetExecutingAssembly().GetName().Version!.ToString();
            // string errorMessage = $"Date: {DateTime.Now}\nVersion: {version}\nContext: {contextMessage}\n\nException Details:\n{ex}\n\n";
            string errorMessage = $"Date: {DateTime.Now}\nVersion: {version}\n\n{contextMessage}\n";

            try
            {
                // Append the error message to both the general and user-specific logs.
                await File.AppendAllTextAsync(errorLogPath, errorMessage);
                await File.AppendAllTextAsync(userLogPath, errorMessage);

                // Attempt to send the error log content to the API.
                if (await SendLogToApiAsync(errorMessage))
                {
                    // If the log was successfully sent, delete the general log file to clean up.
                    File.Delete(errorLogPath);
                }
            }
            catch (Exception)
            {
                // Ignore
            }
        }
        
        private static async Task<bool> SendLogToApiAsync(string logContent)
        {
            // Prepare the content to be sent via HTTP POST.
            var formData = new MultipartFormDataContent
            {
                { new StringContent("contact@purelogiccode.com"), "recipient" },
                { new StringContent("Error Log from SimpleLauncher"), "subject" },
                { new StringContent("SimpleLauncher User"), "name" },
                { new StringContent(logContent), "message" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://purelogiccode.com/simplelauncher/send_email.php")
            {
                Content = formData
            };
            request.Headers.Add("X-API-KEY", "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e");

            try
            {
                HttpResponseMessage response = await HttpClient.SendAsync(request);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }
        
    }
}