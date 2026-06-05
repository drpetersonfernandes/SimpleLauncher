using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SettingsManager;
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

    private const string DefaultNotificationSound = "click.mp3";
    private static readonly string AudioFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audio");

    [ObservableProperty] private bool _enableNotificationSound;
    [ObservableProperty] private string _notificationSoundFile;
    [ObservableProperty] private bool _isSoundControlsEnabled;

    public SoundConfigurationViewModel(SettingsManager settings, PlaySoundEffects playSoundEffects, ILogErrors logErrors)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _playSoundEffects = playSoundEffects ?? throw new ArgumentNullException(nameof(playSoundEffects));
        _logErrors = logErrors ?? throw new ArgumentNullException(nameof(logErrors));

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
    private void ChooseSoundFile()
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
            MessageBoxLibrary.ErrorSettingSoundFileMessageBox();
        }
    }

    [RelayCommand]
    private void PlayCurrentSound()
    {
        switch (EnableNotificationSound)
        {
            case true when !string.IsNullOrWhiteSpace(NotificationSoundFile):
                _playSoundEffects.PlayConfiguredSound(NotificationSoundFile);
                break;
            case false:
                MessageBoxLibrary.NotificationSoundIsDisableMessageBox();
                break;
            default:
                MessageBoxLibrary.NoSoundFileIsSelectedMessageBox();
                break;
        }
    }

    [RelayCommand]
    private void ResetToDefault()
    {
        EnableNotificationSound = true;
        NotificationSoundFile = DefaultNotificationSound;
    }

    [RelayCommand]
    private void Save()
    {
        _settings.EnableNotificationSound = EnableNotificationSound;
        _settings.CustomNotificationSoundFile = NotificationSoundFile;
        _settings.Save();

        (Application.Current.MainWindow as MainWindow)?.UpdateStatusBarService.UpdateContent(
            (string)Application.Current.TryFindResource("SavingSoundSettings") ?? "Saving sound settings...",
            Application.Current.MainWindow as MainWindow);

        MessageBoxLibrary.SettingsSavedSuccessfullyMessageBox();

        SaveCompleted?.Invoke();
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseRequested?.Invoke();
    }
}
