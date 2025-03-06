using System;
using System.Windows;
using System.Diagnostics;
using System.Windows.Navigation;
using System.Reflection;

namespace SimpleLauncher;

public partial class AboutWindow
{
    public AboutWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        DataContext = this;

        AppVersionTextBlock.Text = ApplicationVersion;
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
            // Notify developer
            var formattedException = $"Error in the Hyperlink_RequestNavigate method.\n\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.UnableTgoOpenLinkMessageBox();
        }
        finally
        {
            // Mark the event as handled, regardless of success or failure
            e.Handled = true;
        }
    }

    private async void CheckForUpdateAsync_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await UpdateChecker.CheckForUpdatesVariantAsync(this);
        }
        catch (Exception ex)
        {
            // Notify developer
            var formattedException = $"Error in the CheckForUpdateAsync_Click method.\n\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            await LogErrors.LogErrorAsync(ex, formattedException);

            // Notify user
            MessageBoxLibrary.ErrorCheckingForUpdatesMessageBox();
        }
    }

    private static string ApplicationVersion
    {
        get
        {
            var version2 = (string)Application.Current.TryFindResource("Version") ?? "Version:";
            var unknown2 = (string)Application.Current.TryFindResource("Unknown") ?? "Unknown";
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            return $"{version2} " + (version?.ToString() ?? unknown2);
        }
    }

    private void UpdateHistory_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var updateHistoryWindow = new UpdateHistoryWindow();
            updateHistoryWindow.ShowDialog();
        }
        catch (Exception ex)
        {
            // Notify developer
            var formattedException = $"Error in the UpdateHistory_Click method.\n\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.ErrorOpeningTheUpdateHistoryWindowMessageBox();
        }
    }
}