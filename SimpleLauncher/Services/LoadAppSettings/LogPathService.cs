using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.LoadAppSettings;

public class LogPathService
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;

    public LogPathService(IConfiguration configuration, ILogErrors logErrors)
    {
        _configuration = configuration;
        _logErrors = logErrors;
    }

    public string GetPath()
    {
        try
        {
            var logPath = _configuration["LogPath"];

            if (string.IsNullOrEmpty(logPath))
                return string.Empty;

            var appPath = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(appPath, logPath);
        }
        catch (Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, "Failed to get the LogPath.");
            return string.Empty;
        }
    }
}