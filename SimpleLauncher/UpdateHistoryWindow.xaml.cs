using System;
using System.IO;

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
            var contextMessage = "'whatsnew.txt' not found or could not be loaded.\n\n" +
                                     $"Exception type: {ex.GetType().Name}\n" +
                                     $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));
        }
    }
}