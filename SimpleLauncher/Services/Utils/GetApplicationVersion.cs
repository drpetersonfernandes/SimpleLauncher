using System.Reflection;
using System.Windows;

namespace SimpleLauncher.Services.Utils;

public static class GetApplicationVersion
{
    public static string GetVersion
    {
        get
        {
            var version2 = (string)Application.Current.TryFindResource("Version") ?? "Version:";
            var unknown2 = (string)Application.Current.TryFindResource("Unknown") ?? "Unknown";
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version2} " + (version?.ToString() ?? unknown2);
        }
    }
}