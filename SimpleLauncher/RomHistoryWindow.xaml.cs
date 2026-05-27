using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.UpdateStatusBar;

namespace SimpleLauncher;

public partial class RomHistoryWindow
{
    private readonly string _romName;
    private readonly string _systemName;
    private readonly string _searchTerm;
    private readonly ILogErrors _logErrors;

    public RomHistoryWindow(string romName, string systemName, string searchTerm, ILogErrors logErrors)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _romName = romName;
        _systemName = systemName;
        _searchTerm = searchTerm;
        _logErrors = logErrors;

        RomNameTextBox.Text = _romName;
        RomDescriptionTextBox.Text = _searchTerm;
        RomDescriptionTextBox.Visibility = Visibility.Collapsed;

        Loaded += (_, _) =>
        {
            // Attach a bubbling RequestNavigate handler to catch hyperlink clicks
            HistoryMarkdownViewer.AddHandler(Hyperlink.RequestNavigateEvent, new RequestNavigateEventHandler(OnHyperlinkRequestNavigate));
        };

        Loaded += async (_, _) =>
        {
            try
            {
                await LoadRomHistoryAsync();
            }
            catch (Exception ex)
            {
                _logErrors.LogAndForget(ex, "Error loading ROM history.");
                DebugLogger.Log($"Error loading ROM history: {ex.Message}");
            }
        };
    }

    private async Task LoadRomHistoryAsync()
    {
        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("LoadingROMHistory") ?? "Loading ROM history...", Application.Current.MainWindow as MainWindow);
        try
        {
            var historyFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.xml");

            if (!await Task.Run(() => File.Exists(historyFilePath)))
            {
                const string contextMessage = "'history.xml' is missing.";
                _logErrors.LogAndForget(null, contextMessage);

                await Dispatcher.InvokeAsync(() =>
                {
                    var nohistoryxmlfilefound2 = (string)Application.Current.TryFindResource("Nohistoryxmlfilefound") ?? "No 'history.xml' file found in the application folder.";
                    HistoryMarkdownViewer.Markdown = nohistoryxmlfilefound2;
                });

                MessageBoxLibrary.NoHistoryXmlFoundMessageBox();
                return;
            }

            var entry = await Task.Run(() =>
                Services.RomHistory.RomHistoryLoader.FindEntry(historyFilePath, _romName));

            await Dispatcher.InvokeAsync(() =>
            {
                RomNameTextBox.Text = _romName;
                RomDescriptionTextBox.Text = _searchTerm;
                RomDescriptionTextBox.Visibility = Visibility.Visible;

                if (entry != null)
                {
                    var notextavailable2 = (string)Application.Current.TryFindResource("Notextavailable") ?? "No text available.";
                    var historyText = entry.Element("text")?.Value ?? notextavailable2;
                    HistoryMarkdownViewer.Markdown = ConvertUrlsToMarkdown(historyText);
                }
                else
                {
                    PromptForOnlineSearch();
                }
            });
        }
        catch (Exception ex)
        {
            const string contextMessage = "An error occurred while loading ROM history.";
            _logErrors.LogAndForget(ex, contextMessage);
            MessageBoxLibrary.ErrorLoadingRomHistoryMessageBox();
        }
    }

    private void PromptForOnlineSearch()
    {
        RomNameTextBox.Text = _romName;
        RomDescriptionTextBox.Text = _searchTerm;
        RomDescriptionTextBox.Visibility = Visibility.Visible;

        var noRoMhistoryfoundinthelocal2 = (string)Application.Current.TryFindResource("NoROMhistoryfoundinthelocal") ?? "No ROM history found in the local database for the selected file.";
        HistoryMarkdownViewer.Markdown = noRoMhistoryfoundinthelocal2;

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
            const string contextMessage = "An error occurred while opening the browser.";
            _logErrors.LogAndForget(ex, contextMessage);
            MessageBoxLibrary.ErrorOpeningBrowserMessageBox();
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
        catch (Exception ex)
        {
            DebugLogger.Log($"Failed to open link: {e.Uri} - {ex.Message}");
        }

        e.Handled = true;
    }

    private static string ConvertUrlsToMarkdown(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var regex = MyRegex();

        return regex.Replace(text, static match =>
        {
            var url = match.Value;
            var fullUrl = url.StartsWith("www.", StringComparison.OrdinalIgnoreCase)
                ? "https://" + url
                : url;
            return $"[{url}]({fullUrl})";
        });
    }

    [GeneratedRegex(@"https?://[^\s""\]>)]+", RegexOptions.Compiled)]
    private static partial Regex MyRegex();
}
