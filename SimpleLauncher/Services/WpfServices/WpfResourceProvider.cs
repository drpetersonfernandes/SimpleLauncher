using System.Windows;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Services.WpfServices;

/// <summary>
/// WPF implementation of IResourceProvider, retrieving localized string resources from the application resource dictionary.
/// </summary>
public class WpfResourceProvider : IResourceProvider
{
    /// <summary>Gets a localized string resource by key, returning the key itself if not found.</summary>
    public string GetString(string key)
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
            return Application.Current.TryFindResource(key) as string ?? key;

        // Marshal to the UI thread synchronously. The dispatcher pumps queued messages while
        // the caller waits, so it cannot deadlock the UI; there is no timeout/fallback here that
        // could return stale data.
        return dispatcher.Invoke(() => Application.Current.TryFindResource(key) as string ?? key);
    }

    /// <summary>Gets a localized string resource by key, returning the specified default value if not found.</summary>
    public string GetString(string key, string defaultValue)
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
            return Application.Current.TryFindResource(key) as string ?? defaultValue;

        return dispatcher.Invoke(() => Application.Current.TryFindResource(key) as string ?? defaultValue);
    }
}