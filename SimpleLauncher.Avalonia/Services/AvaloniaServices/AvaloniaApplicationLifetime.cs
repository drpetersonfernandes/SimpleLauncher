using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia.Services.AvaloniaServices;

/// <summary>
/// Avalonia implementation of IApplicationLifetime — controls app shutdown and restart.
/// </summary>
public class AvaloniaApplicationLifetime : SimpleLauncher.Core.Interfaces.IApplicationLifetime
{
    private static IClassicDesktopStyleApplicationLifetime? Lifetime =>
        Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;

    public void Shutdown()
    {
        Lifetime?.Shutdown();
    }

    public void Restart()
    {
        var exePath = Environment.ProcessPath;
        if (exePath is not null)
        {
            System.Diagnostics.Process.Start(exePath);
        }

        Lifetime?.Shutdown();
    }
}
