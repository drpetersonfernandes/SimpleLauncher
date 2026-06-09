using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;

namespace SimpleLauncher.Core.ViewModels;

public class UpdateHistoryViewModel : ObservableObject
{
    private readonly ILogErrors _logErrors;
    private readonly IResourceProvider _resourceProvider;
    private string _markdownContent;

    public UpdateHistoryViewModel(ILogErrors logErrors, IResourceProvider resourceProvider)
    {
        _logErrors = logErrors;
        _resourceProvider = resourceProvider;
        LoadWhatsNewContent();
    }

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
            var defaultContent = _resourceProvider.GetString("WhatsNewFileNotFound", "# 'whatsnew.md' not found. The update history file could not be found.");
            MarkdownContent = File.Exists(filePath) ? File.ReadAllText(filePath) : defaultContent;
        }
        catch (Exception ex)
        {
            const string contextMessage = "Failed to load 'whatsnew.md'.";
            _logErrors.LogAndForget(ex, contextMessage);

            MarkdownContent = _resourceProvider.GetString("UpdateHistoryLoadError", "Error. Could not load the update history. The error has been logged.");
        }
    }

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
