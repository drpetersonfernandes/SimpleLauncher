using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SettingsManager;
using Application = System.Windows.Application;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the video and info link configuration window.
/// </summary>
public partial class SetLinksViewModel : ObservableObject
{
    private readonly SettingsManager _settingsManager;
    private readonly IConfiguration _configuration;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IResourceProvider _resourceProvider;

    [ObservableProperty] private string _videoUrl;
    [ObservableProperty] private string _infoUrl;

    public SetLinksViewModel(SettingsManager settingsManager, IConfiguration configuration, IMessageBoxLibraryService messageBox, IResourceProvider resourceProvider)
    {
        _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
        _configuration = configuration;
        _messageBox = messageBox;
        _resourceProvider = resourceProvider;

        _videoUrl = _settingsManager.VideoUrl;
        _infoUrl = _settingsManager.InfoUrl;
    }

    /// <summary>Event raised when settings have been saved.</summary>
    public event Action SaveCompleted;

    /// <summary>Event raised when the window should be closed.</summary>
    public event Action CloseRequested;

    [RelayCommand]
    private async Task SaveAsync()
    {
        _settingsManager.VideoUrl = string.IsNullOrWhiteSpace(VideoUrl)
            ? "https://www.youtube.com/results?search_query="
            : VideoUrl;

        _settingsManager.InfoUrl = string.IsNullOrWhiteSpace(InfoUrl)
            ? "https://www.igdb.com/search?q="
            : InfoUrl;

        await _settingsManager.SaveAsync();

        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent(
            _resourceProvider.GetString("SavingLinkSettings", "Saving link settings..."));

        await _messageBox.LinksSavedMessageBox();

        SaveCompleted?.Invoke();
    }

    [RelayCommand]
    private async Task RevertAsync()
    {
        _settingsManager.VideoUrl = _configuration.GetValue<string>("Urls:YouTubeSearch") ?? "https://www.youtube.com/results?search_query=";
        _settingsManager.InfoUrl = _configuration.GetValue<string>("Urls:IgdbSearch") ?? "https://www.igdb.com/search?q=";

        VideoUrl = _settingsManager.VideoUrl;
        InfoUrl = _settingsManager.InfoUrl;

        await _settingsManager.SaveAsync();

        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent(
            _resourceProvider.GetString("RevertingLinkSettings", "Reverting link settings..."));

        await _messageBox.LinksRevertedMessageBox();

        CloseRequested?.Invoke();
    }
}
