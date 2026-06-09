using System.Diagnostics;
using System.Reflection;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace SimpleLauncher.Avalonia.Views;

public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();

        // Set version text
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
        VersionText.Text = $"Version {version}";
    }

    private void OnOkClick(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void OnGitHubClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/SimpleLauncher/SimpleLauncher",
                UseShellExecute = true
            });
        }
        catch
        {
            // Ignore errors
        }
    }

    private void OnReportBugClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/SimpleLauncher/SimpleLauncher/issues",
                UseShellExecute = true
            });
        }
        catch
        {
            // Ignore errors
        }
    }
}
