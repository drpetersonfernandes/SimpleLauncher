using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Navigation;
using SimpleLauncher.Services;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.Utils;

namespace SimpleLauncher;

public partial class AboutWindow
{
    public AboutWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);
        DataContext = this;
        AppVersionTextBlock.Text = GetApplicationVersion.GetVersion;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri)
            {
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the Hyperlink_RequestNavigate method.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.UnableToOpenLinkMessageBox();
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
            var updateChecker = App.ServiceProvider.GetRequiredService<UpdateChecker>();
            await updateChecker.ManualCheckForUpdatesAsync(this);
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error in the CheckForUpdateAsync_Click method.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorCheckingForUpdatesMessageBox();
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
            const string contextMessage = "Error in the UpdateHistory_Click method.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorOpeningTheUpdateHistoryWindowMessageBox();
        }
    }
}