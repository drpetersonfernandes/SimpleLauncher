using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.SettingsManager;
using Application = System.Windows.Application;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the video and info link configuration window.
/// </summary>
public partial class SetLinksViewModel : ObservableObject
{
    private readonly SettingsManager _settingsManager;
    private readonly IConfiguration _configuration;

    [ObservableProperty] private string _videoUrl;
    [ObservableProperty] private string _infoUrl;

    public SetLinksViewModel(SettingsManager settingsManager, IConfiguration configuration)
    {
        _settingsManager = settingsManager ?? throw new ArgumentNullException(nameof(settingsManager));
        _configuration = configuration;

        _videoUrl = _settingsManager.VideoUrl;
        _infoUrl = _settingsManager.InfoUrl;
    }

    /// <summary>Event raised when settings have been saved.</summary>
    public event Action SaveCompleted;

    /// <summary>Event raised when the window should be closed.</summary>
    public event Action CloseRequested;

    [RelayCommand]
    private void Save()
    {
        _settingsManager.VideoUrl = string.IsNullOrWhiteSpace(VideoUrl)
            ? "https://www.youtube.com/results?search_query="
            : VideoUrl;

        _settingsManager.InfoUrl = string.IsNullOrWhiteSpace(InfoUrl)
            ? "https://www.igdb.com/search?q="
            : InfoUrl;

        _settingsManager.SaveAsync();

        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent(
            (string)Application.Current.TryFindResource("SavingLinkSettings") ?? "Saving link settings...",
            Application.Current.MainWindow as MainWindow);

        MessageBoxLibrary.LinksSavedMessageBox();

        SaveCompleted?.Invoke();
    }

    [RelayCommand]
    private void Revert()
    {
        _settingsManager.VideoUrl = _configuration.GetValue<string>("Urls:YouTubeSearch") ?? "https://www.youtube.com/results?search_query=";
        _settingsManager.InfoUrl = _configuration.GetValue<string>("Urls:IgdbSearch") ?? "https://www.igdb.com/search?q=";

        VideoUrl = _settingsManager.VideoUrl;
        InfoUrl = _settingsManager.InfoUrl;

        _settingsManager.SaveAsync();

        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent(
            (string)Application.Current.TryFindResource("RevertingLinkSettings") ?? "Reverting link settings...",
            Application.Current.MainWindow as MainWindow);

        MessageBoxLibrary.LinksRevertedMessageBox();

        CloseRequested?.Invoke();
    }
}
