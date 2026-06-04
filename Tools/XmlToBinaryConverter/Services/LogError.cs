using System;
using System.IO;
using System.Threading.Tasks;

namespace XmlToBinaryConverter.Services;

public class LogError
{
    private const string LogFileName = "error_log.txt";

    private static string LogFilePath => Path.Combine(AppContext.BaseDirectory, LogFileName);

    public async Task LogAsync(Exception ex)
    {
        var errorMessage = $"[{DateTime.Now}] Error: {ex.Message}\nStackTrace: {ex.StackTrace}\n\n";

        try
        {
            await File.AppendAllTextAsync(LogFilePath, errorMessage);
        }
        catch (Exception)
        {
            // If the first attempt fails (e.g., directory doesn't exist), try creating the directory
            try
            {
                var logDirectory = Path.GetDirectoryName(LogFilePath);
                if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // Try writing again
                await File.AppendAllTextAsync(LogFilePath, errorMessage);
            }
            catch (Exception innerEx)
            {
                // If even directory creation and second write fail, there's not much else we can do.
                // Log to console as a last resort.
                Console.WriteLine($"FATAL ERROR: Could not write to log file at {LogFilePath}.");
                Console.WriteLine($"Original Error: {ex.Message}");
                Console.WriteLine($"Logging Error: {innerEx.Message}");
            }
        }
    }

    public async Task<string> ReadLogAsync()
    {
        if (File.Exists(LogFilePath))
        {
            try
            {
                return await File.ReadAllTextAsync(LogFilePath);
            }
            catch (Exception ex)
            {
                // Handle potential read errors
                return $"Error reading log file: {ex.Message}";
            }
        }

        return "No error log found.";
    }

    public void ClearLog()
    {
        if (File.Exists(LogFilePath))
        {
            try
            {
                File.Delete(LogFilePath);
            }
            catch (Exception ex)
            {
                 // Handle potential delete errors
                 Console.WriteLine($"Error clearing log file: {ex.Message}");
            }
        }
    }
}
