using System.Windows;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.WpfServices;

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

        var task = dispatcher.InvokeAsync(() =>
            Application.Current.TryFindResource(key) as string ?? key);
        return task.Task.Wait(TimeSpan.FromSeconds(5)) ? task.Task.Result : key;
    }

    /// <summary>Gets a localized string resource by key, returning the specified default value if not found.</summary>
    public string GetString(string key, string defaultValue)
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
            return Application.Current.TryFindResource(key) as string ?? defaultValue;

        var task = dispatcher.InvokeAsync(() =>
            Application.Current.TryFindResource(key) as string ?? defaultValue);
        return task.Task.Wait(TimeSpan.FromSeconds(5)) ? task.Task.Result : defaultValue;
    }
}
