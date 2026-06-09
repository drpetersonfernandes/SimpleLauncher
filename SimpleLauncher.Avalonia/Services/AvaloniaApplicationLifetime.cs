using System.Diagnostics;
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
        var executablePath = Environment.ProcessPath;
        if (!string.IsNullOrEmpty(executablePath))
        {
            Process.Start(executablePath);
        }

        Shutdown();
    }
}
