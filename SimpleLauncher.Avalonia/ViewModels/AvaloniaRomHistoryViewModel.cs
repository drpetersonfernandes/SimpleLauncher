using System.Diagnostics;
using System.Text.RegularExpressions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.RomHistory;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaRomHistoryViewModel : ObservableObject
{
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;

    private string _romName = string.Empty;
    private string _systemName = string.Empty;
    private string _searchTerm = string.Empty;

    [ObservableProperty] private string _romNameText = string.Empty;
    [ObservableProperty] private string _romDescriptionText = string.Empty;
    [ObservableProperty] private string _historyMarkdown = string.Empty;
    [ObservableProperty] private bool _isDescriptionVisible;

    public AvaloniaRomHistoryViewModel(ILogErrors logErrors, IMessageBoxLibraryService messageBox)
    {
        _logErrors = logErrors;
        _messageBox = messageBox;
    }

    public void Initialize(string romName, string systemName, string searchTerm)
    {
        _romName = romName;
        _systemName = systemName;
        _searchTerm = searchTerm;

        RomNameText = _romName;
        RomDescriptionText = _searchTerm;
        IsDescriptionVisible = false;
    }

    public async Task LoadRomHistoryAsync()
    {
        try
        {
            var historyDatFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.dat");
            var historyXmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "history.xml");

            var datExists = await Task.Run(() => File.Exists(historyDatFilePath));
            var xmlExists = await Task.Run(() => File.Exists(historyXmlFilePath));

            if (!datExists && !xmlExists)
            {
                const string contextMessage = "'history.dat' and 'history.xml' are both missing.";
                _logErrors.LogAndForget(null, contextMessage);

                HistoryMarkdown = "No 'history.dat' or 'history.xml' file found in the application folder.";
                await _messageBox.NoHistoryXmlOrDatFoundMessageBox();
                return;
            }

            var entry = await Task.Run(() =>
                RomHistoryLoader.FindEntry(historyXmlFilePath, _romName));

            RomNameText = _romName;
            RomDescriptionText = _searchTerm;
            IsDescriptionVisible = true;

            if (entry != null)
            {
                var historyText = entry.Element("text")?.Value ?? "No text available.";
                HistoryMarkdown = ConvertUrlsToMarkdown(historyText);
            }
            else
            {
                await PromptForOnlineSearch();
            }
        }
        catch (Exception ex)
        {
            const string contextMessage = "An error occurred while loading ROM history.";
            _logErrors.LogAndForget(ex, contextMessage);
            await _messageBox.ErrorLoadingRomHistoryMessageBox();
        }
    }

    private async Task PromptForOnlineSearch()
    {
        RomNameText = _romName;
        RomDescriptionText = _searchTerm;
        IsDescriptionVisible = true;

        HistoryMarkdown = "No ROM history found in the local database for the selected file.";

        var result = await _messageBox.SearchOnlineForRomHistoryMessageBox();
        if (result == MessageBoxResult.Yes)
        {
            await OpenGoogleSearch();
        }
    }

    [RelayCommand]
    private async Task OpenGoogleSearch()
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
            await _messageBox.ErrorOpeningBrowserMessageBox();
        }
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
