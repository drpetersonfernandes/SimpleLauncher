using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Xml.Linq;
using SimpleLauncher.Services;

namespace SimpleLauncher;

public partial class RomHistoryWindow
{
    private readonly string _romName;
    private readonly string _systemName;
    private readonly string _searchTerm;
    private readonly SystemManager _systemManager;

    public RomHistoryWindow(string romName, string systemName, string searchTerm, SystemManager systemManager)
    {
        InitializeComponent();

        _romName = romName;
        _systemName = systemName;
        _searchTerm = searchTerm;
        _systemManager = systemManager;

        RomNameTextBox.Text = _romName;
        RomDescriptionTextBox.Text = _searchTerm;
        RomDescriptionTextBox.Visibility = Visibility.Collapsed;

        // Load history asynchronously after window initialization
        Loaded += async (_, _) => await LoadRomHistoryAsync();
    }

    private async Task LoadRomHistoryAsync()
    {
        try
        {
            var historyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.xml");

            // Check file existence asynchronously
            if (!await Task.Run(() => File.Exists(historyFilePath)))
            {
                // Notify developer
                const string contextMessage = "'history.xml' is missing.";
                var ex = new Exception(contextMessage);
                _ = LogErrors.LogErrorAsync(ex, contextMessage);

                // Update UI on the UI thread
                await Dispatcher.InvokeAsync(() =>
                {
                    var nohistoryxmlfilefound2 = (string)Application.Current.TryFindResource("Nohistoryxmlfilefound") ?? "No 'history.xml' file found in the application folder.";
                    HistoryTextBlock.Text = nohistoryxmlfilefound2;

                    MessageBoxLibrary.NoHistoryXmlFoundMessageBox();
                });

                return;
            }

            // Load and parse XML in a background thread
            var entry = await Task.Run(() =>
            {
                var doc = XDocument.Load(historyFilePath);

                return doc.Descendants("entry")
                           .FirstOrDefault(e => e.Element("systems")?.Elements("system")
                               .Any(system => system.Attribute("name")?.Value == _romName) == true)
                       ?? doc.Descendants("entry")
                           .FirstOrDefault(e => e.Element("software")?.Elements("item")
                               .Any(item => item.Attribute("name")?.Value == _romName) == true);
            });

            // Update UI on the UI thread
            await Dispatcher.InvokeAsync(() =>
            {
                RomNameTextBox.Text = _romName;

                // Only show _searchTerm in RomDescriptionTextBox if SystemIsMame is true
                if (_systemManager.SystemIsMame)
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
                    var notextavailable2 = (string)Application.Current.TryFindResource("Notextavailable") ?? "No text available.";
                    var historyText = entry.Element("text")?.Value ?? notextavailable2;
                    SetHistoryTextWithLinks(historyText);
                }
                else
                {
                    PromptForOnlineSearch();
                }
            });
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while loading ROM history.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user on the UI thread
            await Dispatcher.InvokeAsync(MessageBoxLibrary.ErrorLoadingRomHistoryMessageBox);
        }
    }

    private void PromptForOnlineSearch()
    {
        RomNameTextBox.Text = _romName;

        if (_systemManager.SystemIsMame)
        {
            RomNameTextBox.Text = _romName;
            RomDescriptionTextBox.Text = _searchTerm;
            RomDescriptionTextBox.Visibility = Visibility.Visible;
        }
        else
        {
            RomDescriptionTextBox.Visibility = Visibility.Collapsed;
        }

        var noRoMhistoryfoundinthelocal2 = (string)Application.Current.TryFindResource("NoROMhistoryfoundinthelocal") ?? "No ROM history found in the local database for the selected file.";
        HistoryTextBlock.Text = noRoMhistoryfoundinthelocal2;

        // Notify user
        DidNotFindRomHistoryMessageBox();
        return;

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
        var googleSearchUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(query)}";

        try
        {
            Process.Start(new ProcessStartInfo { FileName = googleSearchUrl, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "An error occurred while opening the browser.";
            _ = LogErrors.LogErrorAsync(ex, contextMessage);

            // Notify user
            MessageBoxLibrary.ErrorOpeningBrowserMessageBox();
        }
    }

    private void SetHistoryTextWithLinks(string historyText)
    {
        HistoryTextBlock.Inlines.Clear();

        var regexLink = MyRegex();
        var regexBoldLine = MyRegex1();

        var parts = regexLink.Split(historyText);
        var matches = regexLink.Matches(historyText);

        var index = 0;
        foreach (var part in parts)
        {
            // Check if the part contains a bold line pattern
            if (regexBoldLine.IsMatch(part))
            {
                var boldMatches = regexBoldLine.Matches(part);
                var boldIndex = 0;

                foreach (var subPart in regexBoldLine.Split(part))
                {
                    HistoryTextBlock.Inlines.Add(new Run(subPart));

                    // If there's a match for the bold pattern, make it bold
                    if (boldIndex >= boldMatches.Count) continue;

                    HistoryTextBlock.Inlines.Add(new Bold(new Run(boldMatches[boldIndex].Value)));
                    boldIndex++;
                }
            }
            else
            {
                HistoryTextBlock.Inlines.Add(new Run(part));
            }

            // If there's a link, make it bold and clickable
            if (index >= matches.Count) continue;

            var hyperlink = new Hyperlink(new Bold(new Run(matches[index].Value)))
            {
                NavigateUri = new Uri(matches[index].Value.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? matches[index].Value
                    : "http://" + matches[index].Value)
            };
            hyperlink.RequestNavigate += static (_, e) =>
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

    [GeneratedRegex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled)]
    private static partial Regex MyRegex();

    [GeneratedRegex(@"- .* -", RegexOptions.Compiled)]
    private static partial Regex MyRegex1();
}