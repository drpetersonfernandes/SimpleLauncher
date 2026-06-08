using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.DebugAndBugReport;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Core.Services.SettingsManager;
using Application = System.Windows.Application;

namespace SimpleLauncher.ViewModels;

/// <summary>
/// ViewModel for the sound configuration window.
/// </summary>
public partial class SoundConfigurationViewModel : ObservableObject
{
    private readonly SettingsManager _settings;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly ILogErrors _logErrors;
    private readonly IMessageBoxLibraryService _messageBox;
    private readonly IResourceProvider _resourceProvider;

    private const string DefaultNotificationSound = "click.mp3";
    private static readonly string AudioFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audio");

    [ObservableProperty] private bool _enableNotificationSound;
    [ObservableProperty] private string _notificationSoundFile;
    [ObservableProperty] private bool _isSoundControlsEnabled;

    public SoundConfigurationViewModel(SettingsManager settings, PlaySoundEffects playSoundEffects, ILogErrors logErrors, IMessageBoxLibraryService messageBox, IResourceProvider resourceProvider)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _playSoundEffects = playSoundEffects ?? throw new ArgumentNullException(nameof(playSoundEffects));
        _logErrors = logErrors ?? throw new ArgumentNullException(nameof(logErrors));
        _messageBox = messageBox;
        _resourceProvider = resourceProvider;

        _enableNotificationSound = _settings.EnableNotificationSound;
        _notificationSoundFile = _settings.CustomNotificationSoundFile;
        _isSoundControlsEnabled = _enableNotificationSound;
    }

    /// <summary>Event raised when settings have been saved.</summary>
    public event Action SaveCompleted;

    /// <summary>Event raised when the window should be closed.</summary>
    public event Action CloseRequested;

    /// <summary>Event raised to request a sound file path from the view.</summary>
    public event Func<string> RequestSoundFilePath;

    partial void OnEnableNotificationSoundChanged(bool value)
    {
        IsSoundControlsEnabled = value;
    }

    [RelayCommand]
    private async Task ChooseSoundFile()
    {
        var sourceFilePath = RequestSoundFilePath?.Invoke();
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
    private Task PlayCurrentSound()
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
    private async Task Save()
    {
        _settings.EnableNotificationSound = EnableNotificationSound;
        _settings.CustomNotificationSoundFile = NotificationSoundFile;
        await _settings.SaveAsync();

        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent(
            _resourceProvider.GetString("SavingSoundSettings", "Saving sound settings..."));

        await _messageBox.SettingsSavedSuccessfullyMessageBox();

        SaveCompleted?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke();
    }
}
