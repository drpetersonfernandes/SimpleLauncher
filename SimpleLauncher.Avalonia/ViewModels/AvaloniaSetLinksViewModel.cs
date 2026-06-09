using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaSetLinksViewModel : ObservableObject
{
    private readonly SettingsManager _settingsManager;
    private readonly IConfiguration _configuration;
    private readonly IMessageBoxLibraryService _messageBox;

    [ObservableProperty] private string _videoUrl;
    [ObservableProperty] private string _infoUrl;

    public AvaloniaSetLinksViewModel(SettingsManager settingsManager, IConfiguration configuration, IMessageBoxLibraryService messageBox)
    {
        _settingsManager = settingsManager;
        _configuration = configuration;
        _messageBox = messageBox;

        _videoUrl = _settingsManager.VideoUrl;
        _infoUrl = _settingsManager.InfoUrl;
    }

    public event Action? SaveCompleted;
    public event Action? CloseRequested;

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
        await _messageBox.LinksRevertedMessageBox();
        CloseRequested?.Invoke();
    }
}
