#nullable enable

using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Models;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MameManager;
using SimpleLauncher.Services.PlayHistory;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SettingsManager;
using SystemManager = SimpleLauncher.Services.SystemManager.SystemManager;
using CoreMessageBoxResult = SimpleLauncher.Interfaces.MessageBoxResult;

namespace SimpleLauncher.ViewModels;

[SuppressMessage("ReSharper", "NotAccessedField.Local")]
public partial class PlayHistoryViewModel : ObservableObject, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors;
    private readonly PlayHistoryManager _playHistoryManager;
    private readonly SettingsManager _settings;
    private readonly List<SystemManager> _systemManagers;
    private readonly List<MameManager> _machines;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly IFindCoverImageService _findCoverImage;
    private readonly IImageLoader _imageLoader;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IResourceProvider _resourceProvider;

    private const string TimeFormat = "HH:mm:ss";
    private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

    [ObservableProperty] private ObservableCollection<PlayHistoryItem> _playHistoryList = [];

    [ObservableProperty] private PlayHistoryItem? _selectedItem;

    [ObservableProperty] private Stream? _previewImageSource;

    partial void OnPreviewImageSourceChanging(Stream? value)
    {
        value?.Dispose();
    }

    [ObservableProperty] private bool _isLoading;

    [ObservableProperty] private string _loadingMessage = "";

    public PlayHistoryViewModel(
        IConfiguration configuration,
        ILogErrors logErrors,
        PlayHistoryManager playHistoryManager,
        SettingsManager settings,
        List<SystemManager> systemManagers,
        List<MameManager> machines,
        PlaySoundEffects playSoundEffects,
        IFindCoverImageService findCoverImage,
        IImageLoader imageLoader,
        IMessageBoxLibraryService messageBox,
        IResourceProvider resourceProvider)
    {
        _configuration = configuration;
        _logErrors = logErrors;
        _playHistoryManager = playHistoryManager;
        _settings = settings;
        _systemManagers = systemManagers;
        _machines = machines;
        _playSoundEffects = playSoundEffects;
        _findCoverImage = findCoverImage;
        _imageLoader = imageLoader;
        _messageBox = messageBox;
        _resourceProvider = resourceProvider;
    }

    public async Task LoadHistoryAsync()
    {
        try
        {
            IsLoading = true;
            LoadingMessage = _resourceProvider.GetString("LoadingHistory", "Loading history...");

            await Task.Yield();

            var processedHistory = await Task.Run(() =>
            {
                var processedList = new List<PlayHistoryItem>();
                foreach (var historyItemConfig in _playHistoryManager.PlayHistoryList)
                {
                    var machine = _machines.FirstOrDefault(m =>
                        m.MachineName.Equals(Path.GetFileNameWithoutExtension(historyItemConfig.FileName), StringComparison.OrdinalIgnoreCase));
                    var machineDescription = machine?.Description ?? "";
                    var systemManager = _systemManagers.FirstOrDefault(config =>
                        config.SystemName.Equals(historyItemConfig.SystemName, StringComparison.OrdinalIgnoreCase));
                    var defaultEmulator = systemManager?.Emulators.FirstOrDefault()?.EmulatorName
                                          ?? _resourceProvider.GetString("UnknownString", "Unknown");
                    var coverImagePath = GetCoverImagePath(historyItemConfig.SystemName, historyItemConfig.FileName);

                    processedList.Add(new PlayHistoryItem
                    {
                        FileName = historyItemConfig.FileName,
                        SystemName = historyItemConfig.SystemName,
                        TotalPlayTime = historyItemConfig.TotalPlayTime,
                        TimesPlayed = historyItemConfig.TimesPlayed,
                        LastPlayDate = historyItemConfig.LastPlayDate,
                        LastPlayTime = historyItemConfig.LastPlayTime,
                        MachineDescription = machineDescription,
                        DefaultEmulator = defaultEmulator,
                        CoverImage = coverImagePath
                    });
                }

                return processedList;
            });

            PlayHistoryList = new ObservableCollection<PlayHistoryItem>(processedHistory);
            SortByDate();
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error loading play history data.");
            await _messageBox.ErrorLoadingRomHistoryMessageBox();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public void SortByDate()
    {
        var sorted = new ObservableCollection<PlayHistoryItem>(
            PlayHistoryList.OrderByDescending(item => TryParseDateTime(item.LastPlayDate, item.LastPlayTime))
        );
        PlayHistoryList = sorted;
    }

    public void SortByTotalPlayTime()
    {
        var sorted = new ObservableCollection<PlayHistoryItem>(
            PlayHistoryList.OrderByDescending(static item => item.TotalPlayTime)
        );
        PlayHistoryList = sorted;
    }

    public void SortByTimesPlayed()
    {
        var sorted = new ObservableCollection<PlayHistoryItem>(
            PlayHistoryList.OrderByDescending(static item => item.TimesPlayed)
        );
        PlayHistoryList = sorted;
    }

    public void RemoveItem(PlayHistoryItem item)
    {
        PlayHistoryList.Remove(item);
        _playHistoryManager.PlayHistoryList = PlayHistoryList;
        _ = _playHistoryManager.SavePlayHistoryAsync();
        PreviewImageSource = null;
    }

    public void RemoveItems(IEnumerable<PlayHistoryItem> items)
    {
        foreach (var item in items)
        {
            PlayHistoryList.Remove(item);
        }

        _playHistoryManager.PlayHistoryList = PlayHistoryList;
        _ = _playHistoryManager.SavePlayHistoryAsync();
        PreviewImageSource = null;
    }

    [RelayCommand]
    private async Task RemoveAllAsync()
    {
        try
        {
            var result = await _messageBox.ReallyWantToRemoveAllPlayHistoryMessageBox();
            if (result == CoreMessageBoxResult.Yes)
            {
                PlayHistoryList.Clear();
                _playHistoryManager.PlayHistoryList = PlayHistoryList;
                await _playHistoryManager.SavePlayHistoryAsync();

                _playSoundEffects.PlayTrashSound();
                PreviewImageSource = null;
            }
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error in RemoveAllAsync.");
        }
    }

    [RelayCommand]
    private async Task LaunchGameAsync()
    {
        if (SelectedItem == null)
        {
            await _messageBox.SelectAGameToLaunchMessageBox();
            return;
        }

        _playSoundEffects.PlayNotificationSound();
        // Game launching is handled by the caller (code-behind) since it needs WPF Window context
    }

    public async Task UpdatePreviewImageAsync(string? imagePath)
    {
        try
        {
            if (string.IsNullOrEmpty(imagePath))
            {
                PreviewImageSource = null;
                return;
            }

            var (imageStream, _) = await _imageLoader.LoadImageAsync(imagePath);
            PreviewImageSource = imageStream;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error loading preview image.");
            PreviewImageSource = null;
        }
    }

    public SystemManager? GetSystemManager(string systemName)
    {
        return _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
    }

    public void RefreshAfterGameLaunch()
    {
        var newList = new ObservableCollection<PlayHistoryItem>();
        foreach (var historyItemConfig in _playHistoryManager.PlayHistoryList)
        {
            var machine = _machines.FirstOrDefault(m =>
                m.MachineName.Equals(Path.GetFileNameWithoutExtension(historyItemConfig.FileName), StringComparison.OrdinalIgnoreCase));
            var machineDescription = machine?.Description ?? "";
            var systemManager = _systemManagers.FirstOrDefault(manager =>
                manager.SystemName.Equals(historyItemConfig.SystemName, StringComparison.OrdinalIgnoreCase));
            var defaultEmulator = systemManager?.Emulators.FirstOrDefault()?.EmulatorName
                                  ?? _resourceProvider.GetString("UnknownString", "Unknown");
            var coverImagePath = GetCoverImagePath(historyItemConfig.SystemName, historyItemConfig.FileName);

            newList.Add(new PlayHistoryItem
            {
                FileName = historyItemConfig.FileName,
                SystemName = historyItemConfig.SystemName,
                TotalPlayTime = historyItemConfig.TotalPlayTime,
                TimesPlayed = historyItemConfig.TimesPlayed,
                LastPlayDate = historyItemConfig.LastPlayDate,
                LastPlayTime = historyItemConfig.LastPlayTime,
                MachineDescription = machineDescription,
                DefaultEmulator = defaultEmulator,
                CoverImage = coverImagePath
            });
        }

        PlayHistoryList = newList;
        SortByDate();
    }

    private DateTime TryParseDateTime(string dateStr, string timeStr)
    {
        try
        {
            if (DateTime.TryParseExact($"{dateStr} {timeStr}", "yyyy-MM-dd HH:mm:ss",
                    InvariantCulture, DateTimeStyles.None, out var result))
            {
                return result;
            }

            string[] dateFormats =
            [
                "yyyy/MM/dd", "yyyy.MM.dd", "dd.MM.yyyy",
                "MM/dd/yyyy", "dd/MM/yyyy", "dd-MM-yyyy",
                "d", "D"
            ];
            foreach (var df in dateFormats)
            {
                if (DateTime.TryParseExact($"{dateStr} {timeStr}",
                        $"{df} {TimeFormat}", InvariantCulture, DateTimeStyles.None, out result))
                {
                    return result;
                }
            }

            if (DateTime.TryParse($"{dateStr} {timeStr}", InvariantCulture, DateTimeStyles.None, out result))
            {
                return result;
            }

            return DateTime.MinValue;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, $"Error parsing date: {dateStr} {timeStr}");
            return DateTime.MinValue;
        }
    }

    private string GetCoverImagePath(string systemName, string fileName)
    {
        var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        var systemManager = _systemManagers.FirstOrDefault(manager => manager.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
        var defaultCoverImagePath = Path.Combine(baseDirectory, "images", "default.png");

        if (systemManager == null)
        {
            return defaultCoverImagePath;
        }

        return _findCoverImage.FindCoverImagePath(fileNameWithoutExtension, systemName, systemManager.SystemImageFolder);
    }

    public void Dispose()
    {
        PreviewImageSource?.Dispose();
        PreviewImageSource = null;
        GC.SuppressFinalize(this);
    }
}