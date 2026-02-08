using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.LoadAppSettings;

public class ImageExtensionsService
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;

    public ImageExtensionsService(IConfiguration configuration, ILogErrors logErrors)
    {
        _configuration = configuration;
        _logErrors = logErrors;
    }

    public string[] GetExtensions()
    {
        try
        {
            var extensions = _configuration.GetSection("ImageExtensions").Get<string[]>();
            return extensions ?? [];
        }
        catch (System.Exception ex)
        {
            _ = _logErrors.LogErrorAsync(ex, "Failed to get image extensions.");
            return System.Array.Empty<string>();
        }
    }
}