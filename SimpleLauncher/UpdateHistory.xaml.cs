using System;
using System.IO;

namespace SimpleLauncher;

public partial class UpdateHistory
{
    public UpdateHistory()
    {
        InitializeComponent();
        LoadWhatsNewContent();
    }

    private void LoadWhatsNewContent()
    {
        string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "whatsnew.txt");

        try
        {
            WhatsNewTextBox.Text = File.Exists(filePath) ? File.ReadAllText(filePath) : "'whatsnew.txt' not found in the application folder.";
        }
        catch (Exception ex)
        {
            // Notify developer
            string formattedException = "'whatsnew.txt' not found or could not be loaded.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\n" +
                                        $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, formattedException).Wait(TimeSpan.FromSeconds(2));
        }
    }
}