using System.Diagnostics;
using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the UpdateHistoryWindow.
/// </summary>
public class UpdateHistoryViewModel : ObservableObject
{
    private readonly ILogErrors _logErrors;
    private string _markdownContent;

    public UpdateHistoryViewModel(ILogErrors logErrors)
    {
        _logErrors = logErrors;
        LoadWhatsNewContent();
    }

    /// <summary>
    /// Gets or sets the markdown content to display.
    /// </summary>
    public string MarkdownContent
    {
        get => _markdownContent;
        private set => SetProperty(ref _markdownContent, value);
    }

    private void LoadWhatsNewContent()
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "whatsnew.md");

        try
        {
            var defaultContent = (string)Application.Current.TryFindResource("WhatsNewFileNotFound") ?? "# 'whatsnew.md' not found. The update history file could not be found.";
            MarkdownContent = File.Exists(filePath) ? File.ReadAllText(filePath) : defaultContent;
        }
        catch (Exception ex)
        {
            const string contextMessage = "Failed to load 'whatsnew.md'.";
            _logErrors.LogAndForget(ex, contextMessage);

            MarkdownContent = (string)Application.Current.TryFindResource("UpdateHistoryLoadError") ?? "Error. Could not load the update history. The error has been logged.";
        }
    }

    /// <summary>
    /// Handles hyperlink navigation requests.
    /// </summary>
    public void OnHyperlinkRequestNavigate(Uri uri)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = uri.AbsoluteUri,
                UseShellExecute = true
            });
        }
        catch
        {
            // Ignore errors when opening the browser
        }
    }
}