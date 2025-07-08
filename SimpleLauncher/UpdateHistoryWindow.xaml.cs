using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class UpdateHistoryWindow
{
    public UpdateHistoryWindow()
    {
        InitializeComponent();
        LoadWhatsNewContent();
    }

    private void LoadWhatsNewContent()
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "whatsnew.md");

        try
        {
            MarkdownViewer.Markdown = File.Exists(filePath)
                ? File.ReadAllText(filePath)
                : "# 'whatsnew.md' not found\n\nThe update history file could not be found in the application folder.";
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "'whatsnew.md' not found or could not be loaded.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
            MarkdownViewer.Markdown = "# Error\n\nCould not load the update history. The error has been logged.";
        }
    }

    // This method handles the command from the hyperlink click
    private void Hyperlink_Executed(object sender, ExecutedRoutedEventArgs e)
    {
        // The URL is passed as a string command parameter.
        if (e.Parameter is not string url || string.IsNullOrWhiteSpace(url)) return;

        try
        {
            // Use ShellExecute to open the URL in the user's default browser
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            e.Handled = true;
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Error opening hyperlink from UpdateHistoryWindow.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.UnableToOpenLinkMessageBox();
        }
    }
}