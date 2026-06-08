using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.RomHistory;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the ROM history information window.
/// </summary>
public partial class RomHistoryViewModel : ObservableObject
{
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;

    private string _romName;
    private string _systemName;
    private string _searchTerm;

    [ObservableProperty] private string _romNameText;
    [ObservableProperty] private string _romDescriptionText;
    [ObservableProperty] private string _historyMarkdown;
    [ObservableProperty] private Visibility _descriptionVisibility = Visibility.Collapsed;

    public RomHistoryViewModel(ILogErrors logErrors)
    {
        _logErrors = logErrors;
        _messageBox = App.ServiceProvider.GetRequiredService<IMessageBoxLibraryService>();
    }

    /// <summary>
    /// Initializes the ViewModel with ROM information for history lookup.
    /// </summary>
    public void Initialize(string romName, string systemName, string searchTerm)
    {
        _romName = romName;
        _systemName = systemName;
        _searchTerm = searchTerm;

        RomNameText = _romName;
        RomDescriptionText = _searchTerm;
        DescriptionVisibility = Visibility.Collapsed;
    }

    /// <summary>
    /// Loads ROM history from the local history.dat/history.xml database.
    /// </summary>
    public async Task LoadRomHistoryAsync()
    {
        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent(
            (string)Application.Current.TryFindResource("LoadingROMHistory") ?? "Loading ROM history...");

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

                var nohistoryxmlfilefound2 = (string)Application.Current.TryFindResource("Nohistoryxmlfilefound2") ?? "No 'history.dat' or 'history.xml' file found in the application folder.";
                HistoryMarkdown = nohistoryxmlfilefound2;

                await _messageBox.NoHistoryXmlOrDatFoundMessageBox();
                return;
            }

            var entry = await Task.Run(() =>
                RomHistoryLoader.FindEntry(historyXmlFilePath, _romName));

            RomNameText = _romName;
            RomDescriptionText = _searchTerm;
            DescriptionVisibility = Visibility.Visible;

            if (entry != null)
            {
                var notextavailable2 = (string)Application.Current.TryFindResource("Notextavailable") ?? "No text available.";
                var historyText = entry.Element("text")?.Value ?? notextavailable2;
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
        DescriptionVisibility = Visibility.Visible;

        var noRoMhistoryfoundinthelocal2 = (string)Application.Current.TryFindResource("NoROMhistoryfoundinthelocal") ?? "No ROM history found in the local database for the selected file.";
        HistoryMarkdown = noRoMhistoryfoundinthelocal2;

        var result = await _messageBox.SearchOnlineForRomHistoryMessageBox();
        if (result == Core.Interfaces.MessageBoxResult.Yes)
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
