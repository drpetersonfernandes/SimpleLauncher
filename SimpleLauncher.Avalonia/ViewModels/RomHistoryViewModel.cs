using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.RomHistory;
using MessageBoxResult = SimpleLauncher.Core.Models.MessageBoxResult;

namespace SimpleLauncher.Avalonia.ViewModels;

/// <summary>
///     ViewModel for the ROM history information window.
/// </summary>
public partial class RomHistoryViewModel : ObservableObject
{
    private readonly ILogger _logger;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IResourceProvider _resourceProvider;
    [ObservableProperty] private string _historyText = null!;
    [ObservableProperty] private bool _isDescriptionVisible;
    [ObservableProperty] private string _romDescriptionText = null!;

    private string _romName = null!;

    [ObservableProperty] private string _romNameText = null!;
    private string _searchTerm = null!;
    private string _systemName = null!;

    /// <summary>Initializes a new instance of the <see cref="RomHistoryViewModel" />.</summary>
    /// <param name="logErrors">The logger instance.</param>
    /// <param name="messageBox">The message box service.</param>
    /// <param name="resourceProvider">The resource provider for localized strings.</param>
    public RomHistoryViewModel(ILogger logErrors, IMessageBoxLibraryService messageBox,
        IResourceProvider resourceProvider)
    {
        _logger = logErrors;
        _messageBox = messageBox;
        _resourceProvider = resourceProvider;
    }

    /// <summary>
    ///     Initializes the ViewModel with ROM information for history lookup.
    /// </summary>
    public void Initialize(string romName, string systemName, string searchTerm)
    {
        _romName = romName;
        _systemName = systemName;
        _searchTerm = searchTerm;

        RomNameText = _romName;
        RomDescriptionText = _searchTerm;
        IsDescriptionVisible = false;
    }

    /// <summary>
    ///     Loads ROM history from the local history.dat/history.xml database.
    /// </summary>
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
                _logger.Warning(contextMessage);

                var nohistoryxmlfilefound2 = _resourceProvider.GetString("Nohistoryxmlfilefound2",
                    "No 'history.dat' or 'history.xml' file found in the application folder.");
                HistoryText = nohistoryxmlfilefound2;

                await _messageBox.NoHistoryXmlOrDatFoundMessageBoxAsync();
                return;
            }

            var entry = await Task.Run(() =>
                RomHistoryLoader.FindEntry(historyXmlFilePath, _romName));

            RomNameText = _romName;
            RomDescriptionText = _searchTerm;
            IsDescriptionVisible = true;

            if (entry != null)
            {
                var notextavailable2 = _resourceProvider.GetString("Notextavailable", "No text available.");
                var historyText = entry.Element("text")?.Value ?? notextavailable2;
                HistoryText = historyText;
            }
            else
            {
                await PromptForOnlineSearchAsync();
            }
        }
        catch (Exception ex)
        {
            const string contextMessage = "An error occurred while loading ROM history.";
            _logger.Error(ex, contextMessage);
            await _messageBox.ErrorLoadingRomHistoryMessageBoxAsync();
        }
    }

    private async Task PromptForOnlineSearchAsync()
    {
        RomNameText = _romName;
        RomDescriptionText = _searchTerm;
        IsDescriptionVisible = true;

        var noRoMhistoryfoundinthelocal2 = _resourceProvider.GetString("NoROMhistoryfoundinthelocal",
            "No ROM history found in the local database for the selected file.");
        HistoryText = noRoMhistoryfoundinthelocal2;

        var result = await _messageBox.SearchOnlineForRomHistoryMessageBoxAsync();
        if (result == MessageBoxResult.Yes) await OpenGoogleSearchAsync();
    }

    [RelayCommand]
    private async Task OpenGoogleSearchAsync()
    {
        var query = !string.IsNullOrEmpty(_searchTerm)
            ? $"\"{_systemName}\" \"{_searchTerm}\" history"
            : $"\"{_systemName}\" \"{_romName}\" history";
        var googleSearchUrl = $"https://www.google.com/search?q={Uri.EscapeDataString(query)}";

        try
        {
            Process.Start(new ProcessStartInfo { FileName = googleSearchUrl, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            const string contextMessage = "An error occurred while opening the browser.";
            _logger.Error(ex, contextMessage);
            await _messageBox.ErrorOpeningBrowserMessageBoxAsync();
        }
    }
}