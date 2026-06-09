using Avalonia.Styling;
using Avalonia.Threading;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia.Services;

public class AvaloniaResourceProvider : IResourceProvider
{
    public string GetString(string key)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            return FindResource(key) ?? key;
        }

        return Dispatcher.UIThread.Invoke(() => FindResource(key) ?? key);
    }

    public string GetString(string key, string defaultValue)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            return FindResource(key) ?? defaultValue;
        }

        return Dispatcher.UIThread.Invoke(() => FindResource(key) ?? defaultValue);
    }

    private static string? FindResource(string key)
    {
        try
        {
            if (global::Avalonia.Application.Current?.Resources.TryGetResource(key, ThemeVariant.Default, out var value) == true)
            {
                return value as string;
            }

            return null;
        }
        catch
        {
            return null;
        }
    }
}
