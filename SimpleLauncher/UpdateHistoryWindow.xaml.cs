using System;
using System.IO;
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
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "WhatsNew.md");

        try
        {
            MarkdownViewer.Markdown = File.Exists(filePath)
                ? File.ReadAllText(filePath)
                : "# 'WhatsNew.md' not found\n\nThe update history file could not be found in the application folder.";
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "'WhatsNew.md' not found or could not be loaded.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
            MarkdownViewer.Markdown = "# Error\n\nCould not load the update history. The error has been logged.";
        }
    }
}