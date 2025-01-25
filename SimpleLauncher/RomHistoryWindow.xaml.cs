using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;
using System.Text.RegularExpressions;
using System.Windows.Documents;

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
                SetHistoryTextWithLinks(historyText);
            }
            else
            {
                PromptForOnlineSearch();
            }
        }
        catch (Exception ex)
        {
            string contextMessage = $"An error occurred while loading ROM history.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            MessageBox.Show("An error occurred while loading ROM history." +
                            " The error was reported to the developer.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void PromptForOnlineSearch()
    {
        string didnotfindaRoMhistory2 = (string)Application.Current.TryFindResource("didnotfindaROMhistory") ?? "did not find a ROM history in the local database for the selected file.";
        string doyouwanttosearchonline2 = (string)Application.Current.TryFindResource("Doyouwanttosearchonline") ?? "Do you want to search online for the ROM history?";
        string rOmHistoryNotFound2 = (string)Application.Current.TryFindResource("ROMHistorynotfound") ?? "ROM History not found";
        var result = MessageBox.Show(
            $"'Simple Launcher' {didnotfindaRoMhistory2}\n\n{doyouwanttosearchonline2}",
            rOmHistoryNotFound2, MessageBoxButton.YesNo, MessageBoxImage.Question);

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
        string noRoMhistoryfoundinthelocal2 = (string)Application.Current.TryFindResource("NoROMhistoryfoundinthelocal") ?? "No ROM history found in the local database for the selected file.";
        HistoryTextBlock.Text = noRoMhistoryfoundinthelocal2;

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
            string contextMessage = $"An error occurred while opening the browser.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
            
            MessageBox.Show("An error occurred while opening the browser.\n\n" +
                            "The error was reported to the developer that will try to fix the issue.",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    private void SetHistoryTextWithLinks(string historyText)
    {
        HistoryTextBlock.Inlines.Clear();

        var regexLink = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled);
        var regexBoldLine = new Regex(@"- .* -", RegexOptions.Compiled);

        var parts = regexLink.Split(historyText);
        var matches = regexLink.Matches(historyText);

        int index = 0;
        foreach (var part in parts)
        {
            // Check if the part contains a bold line pattern
            if (regexBoldLine.IsMatch(part))
            {
                var boldMatches = regexBoldLine.Matches(part);
                int boldIndex = 0;

                foreach (var subPart in regexBoldLine.Split(part))
                {
                    HistoryTextBlock.Inlines.Add(new Run(subPart));

                    // If there's a match for the bold pattern, make it bold
                    if (boldIndex < boldMatches.Count)
                    {
                        HistoryTextBlock.Inlines.Add(new Bold(new Run(boldMatches[boldIndex].Value)));
                        boldIndex++;
                    }
                }
            }
            else
            {
                HistoryTextBlock.Inlines.Add(new Run(part));
            }

            // If there's a link, make it bold and clickable
            if (index < matches.Count)
            {
                var hyperlink = new Hyperlink(new Bold(new Run(matches[index].Value)))
                {
                    NavigateUri = new Uri(matches[index].Value.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                        ? matches[index].Value
                        : "http://" + matches[index].Value)
                };
                hyperlink.RequestNavigate += (_, e) =>
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = e.Uri.AbsoluteUri,
                        UseShellExecute = true
                    });
                };
                HistoryTextBlock.Inlines.Add(hyperlink);
                index++;
            }
        }
    }

}