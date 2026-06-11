using System.Windows;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.WpfServices;

public class WpfResourceProvider : IResourceProvider
{
    public string GetString(string key)
    {
        var dispatcher = Application.Current.Dispatcher;
        if (dispatcher.CheckAccess())
            return Application.Current.TryFindResource(key) as string ?? key;

        var task = dispatcher.InvokeAsync(() =>
            Application.Current.TryFindResource(key) as string ?? key);
        return task.Task.Wait(TimeSpan.FromSeconds(5)) ? task.Task.Result : key;
    }

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
