using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace SimpleLauncher;

public partial class RomHistoryWindow
{
    private readonly string _romName;
    private readonly string _systemName;
    private readonly string _searchTerm;
    
    public RomHistoryWindow(string romName, string systemName, string searchTerm = "")
    {
        InitializeComponent();
        _romName = romName;
        _systemName = systemName;
        _searchTerm = searchTerm;
        
        // Attach Loaded event to start loading history after the window is fully initialized
        Loaded += async (_, _) => await LoadRomHistoryAsync();
    }

    private async Task LoadRomHistoryAsync()
    {
        try
        {
            string historyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.xml");

            if (!File.Exists(historyFilePath))
            {
                HistoryTextBlock.Text = "No history.xml file found in the application folder.";
                return;
            }

            XDocument doc = await Task.Run(() => XDocument.Load(historyFilePath));

            var entry = doc.Descendants("entry")
                            .FirstOrDefault(e => e.Element("systems")?.Elements("system")
                                .Any(system => system.Attribute("name")?.Value == _romName) == true)
                        ?? doc.Descendants("entry")
                            .FirstOrDefault(e => e.Element("software")?.Elements("item")
                                .Any(item => item.Attribute("name")?.Value == _romName) == true);

            RomNameTextBox.Text = _romName;
            RomDescriptionTextBox.Text = _searchTerm;

            if (entry != null)
            {
                string historyText = entry.Element("text")?.Value ?? "No text available.";
                HistoryTextBlock.Text = historyText;
            }
            else
            {
                await PromptForOnlineSearchAsync();
            }
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex, "An error occurred while loading ROM history.");
            MessageBox.Show("An error occurred while loading ROM history."
                            +
                            " The error was reported to the developer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }


    private async Task PromptForOnlineSearchAsync()
    {
        var result = MessageBox.Show(
            "Simple Launcher did not find a ROM history in the local database for the selected file.\n\nDo you want to search online for the ROM history?",
            "ROM History Not Found", MessageBoxButton.YesNo, MessageBoxImage.Question);

        RomNameTextBox.Text = _romName;
        RomDescriptionTextBox.Text = _searchTerm;
        HistoryTextBlock.Text = "No ROM history found in the local database for the selected file.";

        if (result == MessageBoxResult.Yes)
        {
            await OpenGoogleSearchAsync();
        }
    }
        
    private async Task OpenGoogleSearchAsync()
    {
        var query = !string.IsNullOrEmpty(_searchTerm) ? $"\"{_systemName}\" \"{_searchTerm}\" history" : $"\"{_systemName}\" \"{_romName}\" history";
        string googleSearchUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(query)}";

        try
        {
            await Task.Run(() => Process.Start(new ProcessStartInfo { FileName = googleSearchUrl, UseShellExecute = true }));
        }
        catch (Exception ex)
        {
            await LogErrorAsync(ex, "An error occurred while opening the browser.");
            MessageBox.Show("An error occurred while opening the browser.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private async Task LogErrorAsync(Exception ex, string contextMessage)
    {
        // Assuming LogErrors.LogErrorAsync logs error and returns Task
        await LogErrors.LogErrorAsync(ex, contextMessage);
    }
    
    
}