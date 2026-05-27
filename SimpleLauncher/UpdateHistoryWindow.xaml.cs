using System.Diagnostics;
using System.IO;
using System.Windows.Documents;
using System.Windows.Navigation;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using Application = System.Windows.Application;

namespace SimpleLauncher;

public partial class UpdateHistoryWindow
{
    private readonly ILogErrors _logErrors;

    public UpdateHistoryWindow()
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _logErrors = App.ServiceProvider.GetRequiredService<ILogErrors>();

        HistoryMarkdownViewer.AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler(OnHyperlinkRequestNavigate));

        LoadWhatsNewContent();
    }

    private void LoadWhatsNewContent()
    {
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "whatsnew.md");

        try
        {
            var defaultContent = (string)Application.Current.TryFindResource("WhatsNewFileNotFound") ?? "# 'whatsnew.md' not found. The update history file could not be found.";
            var markdownText = File.Exists(filePath) ? File.ReadAllText(filePath) : defaultContent;

            HistoryMarkdownViewer.Markdown = markdownText;
        }
        catch (Exception ex)
        {
            const string contextMessage = "Failed to load 'whatsnew.md'.";
            _logErrors.LogAndForget(ex, contextMessage);

            HistoryMarkdownViewer.Markdown = (string)Application.Current.TryFindResource("UpdateHistoryLoadError") ?? "Error. Could not load the update history. The error has been logged.";
        }
    }

    private static void OnHyperlinkRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.AbsoluteUri,
                UseShellExecute = true
            });
        }
        catch
        {
            // Ignore errors when opening the browser
        }

        e.Handled = true;
    }
}
