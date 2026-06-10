using System.Windows;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.WpfServices;

public class WpfApplicationLifetime : IApplicationLifetime
{
    public void Shutdown()
    {
        Application.Current.Shutdown();
    }

    public void Restart()
    {
        System.Diagnostics.Process.Start(Application.ResourceAssembly.Location);
        Application.Current.Shutdown();
    }
}
