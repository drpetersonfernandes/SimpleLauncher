using System;
using System.IO;
using System.Threading.Tasks;

namespace SimpleLauncher
{
    public class LogErrors
    {
        private static readonly object _lockObject = new object();

        public async Task LogErrorAsync(Exception ex, string contextMessage = null)
        {
            await Task.Run(() =>
            {
                string errorLogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "error.log");
                string errorMessage = $"Date: {DateTime.Now}\nContext: {contextMessage}\nException Details:\n{ex}\n\n";

                lock (_lockObject)
                {
                    File.AppendAllText(errorLogPath, errorMessage);
                }
            });
        }
    }
}
