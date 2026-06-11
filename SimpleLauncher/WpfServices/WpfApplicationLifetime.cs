using System.Diagnostics;
using System.Windows;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.WpfServices;

public class WpfApplicationLifetime(ILogErrors logErrors) : IApplicationLifetime
{
    private readonly ILogErrors _logErrors = logErrors;

    public void Shutdown()
    {
        Application.Current.Shutdown();
    }

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
            MessageBox.Show($"Failed to restart the application: {ex.Message}\nPlease restart manually.", "Restart Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}
