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
        var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "whatsnew.txt");

        try
        {
            WhatsNewTextBox.Text = File.Exists(filePath) ? File.ReadAllText(filePath) : "'whatsnew.txt' not found in the application folder.";
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "'whatsnew.txt' not found or could not be loaded.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }
}