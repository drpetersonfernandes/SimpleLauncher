using Avalonia.Controls.ApplicationLifetimes;
using IApplicationLifetime = SimpleLauncher.Core.Interfaces.IApplicationLifetime;

namespace SimpleLauncher.Avalonia.Services;

public class AvaloniaApplicationLifetime : IApplicationLifetime
{
    public void Shutdown()
    {
        if (global::Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime lifetime)
        {
            lifetime.Shutdown();
        }
    }

    public void Restart()
    {
        // TODO: Implement restart logic
        Shutdown();
    }
}
