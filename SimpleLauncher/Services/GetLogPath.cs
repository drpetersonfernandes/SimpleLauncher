using System;
using System.IO;
using System.Text.Json;

namespace SimpleLauncher.Services;

public static class GetLogPath
{
    public static string Path()
    {
        try
        {
            var jsonText = File.ReadAllText("appsettings.json");
            var jsonDocument = JsonDocument.Parse(jsonText);
            var logPath = string.Empty;

            if (jsonDocument.RootElement.TryGetProperty("LogPath", out var logPathElement) &&
                logPathElement.ValueKind == JsonValueKind.String)
            {
                logPath = logPathElement.GetString() ?? string.Empty;
            }

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