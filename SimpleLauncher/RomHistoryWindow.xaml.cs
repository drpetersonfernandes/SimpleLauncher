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
    private readonly SystemConfig _systemConfig;

    public RomHistoryWindow(string romName, string systemName, string searchTerm, SystemConfig systemConfig)
    {
        InitializeComponent();
        _romName = romName;
        _systemName = systemName;
        _searchTerm = searchTerm;
        _systemConfig = systemConfig;
        
        RomNameTextBox.Text = _romName;
        RomDescriptionTextBox.Text = _searchTerm;
        RomDescriptionTextBox.Visibility = Visibility.Collapsed;

        // Load history synchronously after window initialization
        Loaded += (_, _) => LoadRomHistory();
    }

    private void LoadRomHistory()
    {
        try
        {
            string historyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.xml");

            if (!File.Exists(historyFilePath))
            {
                HistoryTextBlock.Text = "No history.xml file found in the application folder.";
                return;
            }

            XDocument doc = XDocument.Load(historyFilePath);

            var entry = doc.Descendants("entry")
                            .FirstOrDefault(e => e.Element("systems")?.Elements("system")
                                .Any(system => system.Attribute("name")?.Value == _romName) == true)
                        ?? doc.Descendants("entry")
                            .FirstOrDefault(e => e.Element("software")?.Elements("item")
                                .Any(item => item.Attribute("name")?.Value == _romName) == true);

            RomNameTextBox.Text = _romName;

            // Only show _searchTerm in RomDescriptionTextBox if SystemIsMame is true
            if (_systemConfig.SystemIsMame)
            {
                RomNameTextBox.Text = _romName;
                RomDescriptionTextBox.Text = _searchTerm;
                RomDescriptionTextBox.Visibility = Visibility.Visible;
            }
            else
            {
                RomDescriptionTextBox.Visibility = Visibility.Collapsed;
            }

            if (entry != null)
            {
                string historyText = entry.Element("text")?.Value ?? "No text available.";
                HistoryTextBlock.Text = historyText;
            }
            else
            {
                PromptForOnlineSearch();
            }
        }
        catch (Exception ex)
        {
            string contextMessage = $"An error occurred while loading ROM history.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            MessageBox.Show("An error occurred while loading ROM history." +
                            " The error was reported to the developer.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PromptForOnlineSearch()
    {
        var result = MessageBox.Show(
            "Simple Launcher did not find a ROM history in the local database for the selected file.\n\nDo you want to search online for the ROM history?",
            "ROM History Not Found", MessageBoxButton.YesNo, MessageBoxImage.Question);

        RomNameTextBox.Text = _romName;

        if (_systemConfig.SystemIsMame)
        {
            RomNameTextBox.Text = _romName;
            RomDescriptionTextBox.Text = _searchTerm;
            RomDescriptionTextBox.Visibility = Visibility.Visible;
        }
        else
        {
            RomDescriptionTextBox.Visibility = Visibility.Collapsed;
        }

        HistoryTextBlock.Text = "No ROM history found in the local database for the selected file.";

        if (result == MessageBoxResult.Yes)
        {
            OpenGoogleSearch();
        }
    }

    private void OpenGoogleSearch()
    {
        var query = !string.IsNullOrEmpty(_searchTerm) ? $"\"{_systemName}\" \"{_searchTerm}\" history" : $"\"{_systemName}\" \"{_romName}\" history";
        string googleSearchUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(query)}";

        try
        {
            Process.Start(new ProcessStartInfo { FileName = googleSearchUrl, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            string contextMessage = $"An error occurred while opening the browser.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            MessageBox.Show("An error occurred while opening the browser.\n\nThe error was reported to the developer that will try to fix the issue.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}