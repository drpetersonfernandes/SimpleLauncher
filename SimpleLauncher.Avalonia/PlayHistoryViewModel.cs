using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class PlayHistoryViewModel : ObservableObject, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly IImageLoader _imageLoader;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IMessageDialogService _messageDialog;
    private readonly SettingsManager _settings;

    public PlayHistoryViewModel(
        IConfiguration configuration,
        IImageLoader imageLoader,
        IMessageBoxLibraryService messageBox,
        IMessageDialogService messageDialog,
        SettingsManager settings)
    {
        _configuration = configuration;
        _imageLoader = imageLoader;
        _messageBox = messageBox;
        _messageDialog = messageDialog;
        _settings = settings;
    }

    // ── Collections ─────────────────────────────────────────────

    [ObservableProperty] private ObservableCollection<PlayHistoryItem> _playHistoryList = [];

    [ObservableProperty] private PlayHistoryItem? _selectedItem;

    // ── Preview Image ───────────────────────────────────────────

    [ObservableProperty] private Stream? _previewImageSource;

    // ── Loading State ───────────────────────────────────────────

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private string _loadingMessage = string.Empty;

    // ── Empty State ─────────────────────────────────────────────

    [ObservableProperty] private bool _isEmpty = true;

    // ── Sort Mode ───────────────────────────────────────────────

    [ObservableProperty] private string _currentSortMode = "Date";

    // ── Commands ────────────────────────────────────────────────

    [RelayCommand]
    private async Task RemoveAllAsync()
    {
        if (PlayHistoryList.Count == 0) return;

        var result = await _messageDialog.ShowYesNoAsync(
            "Are you sure you want to remove all play history?",
            "Clear History");

        if (result)
        {
            PlayHistoryList.Clear();
            IsEmpty = true;
            await SavePlayHistoryAsync();
        }
    }

    [RelayCommand]
    private async Task RemoveSelectedAsync()
    {
        if (SelectedItem == null)
        {
            await _messageBox.SelectAHistoryItemToRemoveMessageBox();
            return;
        }

        PlayHistoryList.Remove(SelectedItem);
        IsEmpty = PlayHistoryList.Count == 0;
        SelectedItem = null;
        PreviewImageSource = null;
        await SavePlayHistoryAsync();
    }

    [RelayCommand]
    private async Task LaunchGameAsync()
    {
        if (SelectedItem == null)
        {
            await _messageDialog.ShowInfoAsync("Please select a game to launch.", "Launch Game");
            return;
        }

        // TODO: Implement game launching
        await _messageDialog.ShowInfoAsync("Game launching will be implemented soon.", "Launch Game");
    }

    [RelayCommand]
    private void SortByDate()
    {
        var sorted = PlayHistoryList.OrderByDescending(static x => x.LastPlayDate).ThenByDescending(static x => x.LastPlayTime).ToList();
        PlayHistoryList = new ObservableCollection<PlayHistoryItem>(sorted);
        CurrentSortMode = "Date";
    }

    [RelayCommand]
    private void SortByPlayTime()
    {
        var sorted = PlayHistoryList.OrderByDescending(static x => x.TotalPlayTime).ToList();
        PlayHistoryList = new ObservableCollection<PlayHistoryItem>(sorted);
        CurrentSortMode = "Play Time";
    }

    [RelayCommand]
    private void SortByTimesPlayed()
    {
        var sorted = PlayHistoryList.OrderByDescending(static x => x.TimesPlayed).ToList();
        PlayHistoryList = new ObservableCollection<PlayHistoryItem>(sorted);
        CurrentSortMode = "Times Played";
    }

    // ── Public Methods ──────────────────────────────────────────

    public async Task LoadHistoryAsync()
    {
        IsLoading = true;
        LoadingMessage = "Loading play history...";

        try
        {
            var historyPath = GetHistoryPath();
            if (!File.Exists(historyPath))
            {
                PlayHistoryList = [];
                IsEmpty = true;
                return;
            }

            var history = await LoadHistoryFromFileAsync(historyPath);
            PlayHistoryList = new ObservableCollection<PlayHistoryItem>(history);
            IsEmpty = PlayHistoryList.Count == 0;
            SortByDate();
        }
        catch
        {
            PlayHistoryList = [];
            IsEmpty = true;
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task UpdatePreviewImageAsync(string? imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
        {
            PreviewImageSource = null;
            return;
        }

        try
        {
            var (stream, _) = await _imageLoader.LoadImageAsync(imagePath);
            PreviewImageSource = stream;
        }
        catch
        {
            PreviewImageSource = null;
        }
    }

    public async Task RefreshAfterGameLaunch()
    {
        await LoadHistoryAsync();
        SortByDate();
    }

    // ── Private Methods ─────────────────────────────────────────

    private string GetHistoryPath()
    {
        var basePath = _configuration.GetValue<string>("PlayHistoryPath") ?? "history";
        return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, basePath, "playhistory.bin");
    }

    private static async Task<List<PlayHistoryItem>> LoadHistoryFromFileAsync(string path)
    {
        try
        {
            var bytes = await File.ReadAllBytesAsync(path);
            return MessagePack.MessagePackSerializer.Deserialize<List<PlayHistoryItem>>(bytes);
        }
        catch
        {
            return [];
        }
    }

    private async Task SavePlayHistoryAsync()
    {
        try
        {
            var path = GetHistoryPath();
            var directory = Path.GetDirectoryName(path);
            if (directory != null && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var bytes = MessagePack.MessagePackSerializer.Serialize(PlayHistoryList.ToList());
            await File.WriteAllBytesAsync(path, bytes);
        }
        catch
        {
            // Log error
        }
    }

    // ── IDisposable ─────────────────────────────────────────────

    public void Dispose()
    {
        PreviewImageSource?.Dispose();
        GC.SuppressFinalize(this);
    }
}
