using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia.Services;

public class AvaloniaResourceProvider : IResourceProvider
{
    public string GetString(string key)
    {
        // TODO: Implement with Avalonia resource lookup
        return string.Empty;
    }

    public string GetString(string key, string defaultValue)
    {
        // TODO: Implement with Avalonia resource lookup
        return defaultValue;
    }
}
