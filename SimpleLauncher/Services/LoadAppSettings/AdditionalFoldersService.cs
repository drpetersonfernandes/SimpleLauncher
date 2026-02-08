using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.LoadAppSettings;

public class AdditionalFoldersService
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;

    public AdditionalFoldersService(IConfiguration configuration, ILogErrors logErrors)
    {
        _configuration = configuration;
        _logErrors = logErrors;
    }

    public IEnumerable<string> GetFolders()
    {
        try
        {
            var folders = _configuration.GetSection("AdditionalFolders").Get<IEnumerable<string>>();
            return folders ?? Enumerable.Empty<string>();
        }
        catch (System.Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, "Failed to get additional folders.");
            return System.Array.Empty<string>();
        }
    }
}