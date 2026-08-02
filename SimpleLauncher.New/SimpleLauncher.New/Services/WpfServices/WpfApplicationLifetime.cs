using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.New.Services.WpfServices;

/// <summary>
/// WPF implementation of IApplicationLifetime — controls app shutdown and restart.
/// </summary>
public class WpfApplicationLifetime : IApplicationLifetime
{
    public void Shutdown()
    {
        System.Windows.Application.Current?.Shutdown();
    }

    public void Restart()
    {
        var exePath = Environment.ProcessPath;
        if (exePath is not null)
        {
            System.Diagnostics.Process.Start(exePath);
        }

        System.Windows.Application.Current?.Shutdown();
    }
}
