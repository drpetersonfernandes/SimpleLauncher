using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;

namespace BatchConvertToCHD;

public partial class AboutWindow
{
    public AboutWindow()
    {
        InitializeComponent();

        // Set version information
        AppVersionTextBlock.Text = $"Version: {GetApplicationVersion()}";
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });

            // Dispose the Process object if non-null
            process?.Dispose();
        }
        catch (Exception ex)
        {
            // Log error silently using the bug report service
            var bugReportService = new BugReportService(
                "https://www.purelogiccode.com/bugreport/api/send-bug-report",
                "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e",
                "BatchConvertToCHD");

            _ = bugReportService.SendBugReportAsync($"Error opening URL: {e.Uri.AbsoluteUri}. Exception: {ex.Message}");

            // Show error to user
            MessageBox.Show($"Unable to open link: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        // Mark the event as handled
        e.Handled = true;
    }

    private string GetApplicationVersion()
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version;
        return version?.ToString() ?? "Unknown";
    }
}