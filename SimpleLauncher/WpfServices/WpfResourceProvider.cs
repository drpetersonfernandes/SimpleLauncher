using System.Windows;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.WpfServices;

public class WpfResourceProvider : IResourceProvider
{
    public string GetString(string key)
    {
        return Application.Current.Dispatcher.Invoke(() =>
            Application.Current.TryFindResource(key) as string ?? key);
    }

    public string GetString(string key, string defaultValue)
    {
        return Application.Current.Dispatcher.Invoke(() =>
            Application.Current.TryFindResource(key) as string ?? defaultValue);
    }
}
