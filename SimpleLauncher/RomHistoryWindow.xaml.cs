using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
                // Notify developer
                string contextMessage = "'history.xml' is missing.";
                Exception ex = new Exception(contextMessage);
                LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));
                
                // Notify user
                string nohistoryxmlfilefound2 = (string)Application.Current.TryFindResource("Nohistoryxmlfilefound") ?? "No 'history.xml' file found in the application folder.";
                HistoryTextBlock.Text = nohistoryxmlfilefound2;

                MessageBoxLibrary.NoHistoryXmlFoundMessageBox();

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
                string notextavailable2 = (string)Application.Current.TryFindResource("Notextavailable") ?? "No text available.";
                string historyText = entry.Element("text")?.Value ?? notextavailable2;
                SetHistoryTextWithLinks(historyText);
            }
            else
            {
                PromptForOnlineSearch();
            }
        }
        catch (Exception ex)
        {
            // Notify developer
            string contextMessage = $"An error occurred while loading ROM history.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));

            // Notify user
            MessageBoxLibrary.ErrorLoadingRomHistoryMessageBox();
        }
    }

    private void PromptForOnlineSearch()
    {
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
        
        // Notify user
        DidNotFindRomHistoryMessageBox();
        void DidNotFindRomHistoryMessageBox()
        {
            var result = MessageBoxLibrary.SearchOnlineForRomHistoryMessageBox();
            if (result == MessageBoxResult.Yes)
            {
                OpenGoogleSearch();
            }
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
            // Notify developer
            string contextMessage = $"An error occurred while opening the browser.\n\n" +
                                    $"Exception type: {ex.GetType().Name}\n" +
                                    $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));
            
            // Notify user
            MessageBoxLibrary.ErrorOpeningBrowserMessageBox();
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