using System;
using System.IO;
using Newtonsoft.Json.Linq;

namespace SimpleLauncher.Services;

public static class GetLogPath
{
    public static string Path()
    {
        try
        {
            var jsonText = File.ReadAllText("appsettings.json");
            var jObject = JObject.Parse(jsonText);
            var logPath = jObject["LogPath"]?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(logPath))
                return string.Empty;

            // Get the application's base directory and combine with the log path
            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            return System.IO.Path.Combine(appPath, logPath);
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Failed to get the LogPath.");
            return string.Empty;
        }
    }
}