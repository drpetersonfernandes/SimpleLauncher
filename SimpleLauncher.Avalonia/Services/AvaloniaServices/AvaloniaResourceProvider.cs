using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia.Services.AvaloniaServices;

/// <summary>
/// Implementation of IResourceProvider — delegates to LocalizationService.
/// </summary>
public class AvaloniaResourceProvider : IResourceProvider
{
    private readonly LocalizationService _localization;

    public AvaloniaResourceProvider(LocalizationService localization)
    {
        _localization = localization;
    }

    public string GetString(string key)
    {
        return _localization.GetString(key);
    }

    public string GetString(string key, string defaultValue)
    {
        var result = _localization.GetString(key);
        return result == key ? defaultValue : result;
    }
}
