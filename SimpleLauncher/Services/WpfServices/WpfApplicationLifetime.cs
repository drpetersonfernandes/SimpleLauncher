using System.Diagnostics;
using System.Windows;
using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.WpfServices;

/// <summary>
/// WPF implementation of IApplicationLifetime, providing application shutdown and restart functionality.
/// </summary>
public class WpfApplicationLifetime(ILogErrors logErrors, IMessageBoxLibraryService messageBoxLibraryService) : IApplicationLifetime
{
    private readonly ILogErrors _logErrors = logErrors;
    private readonly IMessageBoxLibraryService _messageBoxLibraryService = messageBoxLibraryService;

    /// <summary>Shuts down the WPF application.</summary>
    public void Shutdown()
    {
        Application.Current.Shutdown();
    }

    /// <summary>Restarts the application by launching a new process and shutting down the current one.</summary>
    public void Restart()
    {
        try
        {
            if (Environment.ProcessPath != null)
            {
                _ = Process.Start(Environment.ProcessPath);
            }

            Application.Current.Shutdown();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Restart failed: Process.Start threw an exception.");
            _ = _messageBoxLibraryService.FailedToRestartMessageBoxAsync();
        }
    }
}
