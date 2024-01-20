using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace SimpleLauncher
{
    public class LogErrors
    {
        private static readonly object _lockObject = new();

        public static async Task LogErrorAsync(Exception ex, string contextMessage = null)
        {
            await Task.Run(async () => // Note the use of async lambda here
            {
                string errorLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                string errorMessage = $"Date: {DateTime.Now}\nContext: {contextMessage}\nException Details:\n{ex}\n\n";

                lock (_lockObject)
                {
                    File.AppendAllText(errorLogPath, errorMessage);
                }

                // After writing to the log, send the log to the PHP API
                await SendLogToApiAsync();
            });
        }

        public static async Task SendLogToApiAsync()
        {
            string logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
            if (!File.Exists(logFilePath))
            {
                return;
            }

            string logContent = File.ReadAllText(logFilePath);

            using var client = new HttpClient();
            var formData = new Dictionary<string, string>
            {
                { "name", "Simple Launcher Error Logger" },
                { "email", "logreport@purelogiccode.com" },
                { "phone", "" },
                { "message", logContent }
            };

            var content = new FormUrlEncodedContent(formData);
            var response = await client.PostAsync("https://purelogiccode.com/sendmail.php", content);

            if (!response.IsSuccessStatusCode)
            {
                return;
            }
        }
    }
}