using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Core.Services.PlaySound;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.ViewModels;

public partial class AvaloniaSoundConfigurationViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly IPlaySoundEffects _playSoundEffects;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IResourceProvider _resourceProvider;
    private readonly IFilePickerService _filePicker;

    private const string DefaultNotificationSound = "click.mp3";
    private static readonly string AudioFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audio");

    [ObservableProperty] private bool _enableNotificationSound;
    [ObservableProperty] private string _notificationSoundFile = string.Empty;
    [ObservableProperty] private bool _isSoundControlsEnabled;

    public AvaloniaSoundConfigurationViewModel(
        SettingsManager settings,
        IPlaySoundEffects playSoundEffects,
        ILogErrors logErrors,
        IMessageBoxLibraryService messageBox,
        IResourceProvider resourceProvider,
        IFilePickerService filePicker)
    {
        _settings = settings;
        _playSoundEffects = playSoundEffects;
        _logErrors = logErrors;
        _messageBox = messageBox;
        _resourceProvider = resourceProvider;
        _filePicker = filePicker;

        _enableNotificationSound = _settings.EnableNotificationSound;
        _notificationSoundFile = _settings.CustomNotificationSoundFile;
        _isSoundControlsEnabled = _enableNotificationSound;
    }

    public event Action? SaveCompleted;
    public event Action? CloseRequested;

    partial void OnEnableNotificationSoundChanged(bool value)
    {
        IsSoundControlsEnabled = value;
    }

    [RelayCommand]
    private async Task ChooseSoundFileAsync()
    {
        var sourceFilePath = await _filePicker.OpenFileAsync("Select Notification Sound File", "MP3 files|*.mp3|All files|*.*");
        if (string.IsNullOrEmpty(sourceFilePath)) return;

        try
        {
            var chosenFileName = Path.GetFileName(sourceFilePath);
            Directory.CreateDirectory(AudioFolderPath);

            var destinationFilePath = Path.Combine(AudioFolderPath, chosenFileName);
            if (!string.Equals(Path.GetFullPath(sourceFilePath), Path.GetFullPath(destinationFilePath), StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(sourceFilePath, destinationFilePath, true);
            }

            NotificationSoundFile = chosenFileName;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "Error choosing or copying sound file.");
            await _messageBox.ErrorSettingSoundFileMessageBox();
        }
    }

    [RelayCommand]
    private Task PlayCurrentSoundAsync()
    {
        switch (EnableNotificationSound)
        {
            case true when !string.IsNullOrWhiteSpace(NotificationSoundFile):
                _playSoundEffects.PlayConfiguredSound(NotificationSoundFile);
                break;
            case false:
                return _messageBox.NotificationSoundIsDisableMessageBox();
            default:
                return _messageBox.NoSoundFileIsSelectedMessageBox();
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private void ResetToDefault()
    {
        EnableNotificationSound = true;
        NotificationSoundFile = DefaultNotificationSound;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        _settings.EnableNotificationSound = EnableNotificationSound;
        _settings.CustomNotificationSoundFile = NotificationSoundFile;
        await _settings.SaveAsync();

        await _messageBox.SettingsSavedSuccessfullyMessageBox();
        SaveCompleted?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke();
    }
}
