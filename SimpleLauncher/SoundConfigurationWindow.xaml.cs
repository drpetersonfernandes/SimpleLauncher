#nullable enable
using System;
using System.IO;
using System.Windows;
using Microsoft.Win32;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.MessageBox;
using SimpleLauncher.Services.PlaySound;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Services.UpdateStatusBar;

namespace SimpleLauncher;

public partial class SoundConfigurationWindow
{
    private readonly SettingsManager _settings;
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly ILogErrors _logErrors;
    private const string DefaultNotificationSound = "click.mp3";
    private static readonly string AudioFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audio");

    public SoundConfigurationWindow(SettingsManager settings, PlaySoundEffects playSoundEffects, ILogErrors logErrors)
    {
        InitializeComponent();
        App.ApplyThemeToWindow(this);

        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
        _playSoundEffects = playSoundEffects ?? throw new ArgumentNullException(nameof(playSoundEffects));
        _logErrors = logErrors ?? throw new ArgumentNullException(nameof(logErrors));

        Owner = Application.Current.MainWindow;

        LoadSettings();
    }

    private void LoadSettings()
    {
        EnableNotificationSoundCheckBox.IsChecked = _settings.EnableNotificationSound;
        NotificationSoundFileTextBox.Text = _settings.CustomNotificationSoundFile;
        UpdateControlsState();
    }

    private void EnableNotificationSoundCheckBox_Click(object sender, RoutedEventArgs e)
    {
        UpdateControlsState();
    }

    private void UpdateControlsState()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var isEnabled = EnableNotificationSoundCheckBox.IsChecked == true;
            NotificationSoundFileTextBox.IsEnabled = isEnabled;
            ChooseSoundFileButton.IsEnabled = isEnabled;
            PlayCurrentSoundButton.IsEnabled = isEnabled;
            ResetToDefaultButton.IsEnabled = isEnabled;
        });
    }

    private void ChooseSoundFileButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "MP3 files (*.mp3)|*.mp3|All files (*.*)|*.*",
            Title = (string)Application.Current.TryFindResource("SelectNotificationSoundFile") ?? "Select Notification Sound File"
        };

        if (openFileDialog.ShowDialog() != true) return;

        try
        {
            var sourceFilePath = openFileDialog.FileName;
            var chosenFileName = Path.GetFileName(sourceFilePath);

            Directory.CreateDirectory(AudioFolderPath); // Ensure audio directory exists

            var destinationFilePath = Path.Combine(AudioFolderPath, chosenFileName);

            // Copy the file if it's not already the one in the audio folder
            // or if it's a different file with the same name from another location.
            if (!string.Equals(Path.GetFullPath(sourceFilePath), Path.GetFullPath(destinationFilePath), StringComparison.OrdinalIgnoreCase))
            {
                File.Copy(sourceFilePath, destinationFilePath, true); // Overwrite if exists
            }

            NotificationSoundFileTextBox.Text = chosenFileName;
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = _logErrors.LogErrorAsync(ex, "Error choosing or copying sound file.");

            // Notify user
            MessageBoxLibrary.ErrorSettingSoundFile();
        }
    }

    private void PlayCurrentSoundButton_Click(object sender, RoutedEventArgs e)
    {
        switch (EnableNotificationSoundCheckBox.IsChecked)
        {
            case true when !string.IsNullOrWhiteSpace(NotificationSoundFileTextBox.Text):
                _playSoundEffects.PlayConfiguredSound(NotificationSoundFileTextBox.Text);
                break;
            case false:
                MessageBoxLibrary.NotificationSoundIsDisable();
                break;
            default:
                MessageBoxLibrary.NoSoundFileIsSelected();
                break;
        }
    }

    private void ResetToDefaultButton_Click(object sender, RoutedEventArgs e)
    {
        EnableNotificationSoundCheckBox.IsChecked = true;
        NotificationSoundFileTextBox.Text = DefaultNotificationSound;
        UpdateControlsState();
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        _settings.EnableNotificationSound = EnableNotificationSoundCheckBox.IsChecked == true;
        _settings.CustomNotificationSoundFile = NotificationSoundFileTextBox.Text;
        _settings.Save();

        UpdateStatusBar.UpdateContent((string)Application.Current.TryFindResource("SavingSoundSettings") ?? "Saving sound settings...", Application.Current.MainWindow as MainWindow);
        MessageBoxLibrary.SettingsSavedSuccessfully();

        DialogResult = true;
        Close();
    }
}