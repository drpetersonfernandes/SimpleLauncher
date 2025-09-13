#nullable enable
using System;
using System.IO;
using System.Windows.Media;
using SimpleLauncher.Managers;
 
namespace SimpleLauncher.Services;

public static class PlaySoundEffects
{
    private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

    private const string ClickSoundFile = "click.mp3";
    private const string ShutterSoundFile = "shutter.mp3";
    private const string TrashSoundFile = "trash.mp3";

    private static MediaPlayer? _currentMediaPlayer;
    private static SettingsManager? _settingsManager;

    /// <summary>
    /// Initializes the PlaySoundEffects service with the necessary dependencies.
    /// This method should be called once during application startup.
    /// </summary>
    /// <param name="settings">The application's settings manager.</param>
    public static void Initialize(SettingsManager settings)
    {
        _settingsManager = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public static void PlayClickSound()
    {
        PlaySound(ClickSoundFile);
    }

    public static void PlayNotificationSound()
    {
        if (_settingsManager == null)
        {
            // This indicates a setup issue: Initialize was not called or settingsManager was null.
            // Notify developer
            _ = LogErrors.LogErrorAsync(new InvalidOperationException("PlaySoundEffects not initialized with SettingsManager."), "Attempted to play notification sound before PlaySoundEffects was initialized.");

            return;
        }

        if (!_settingsManager.EnableNotificationSound)
        {
            return;
        }

        // Use the custom sound file from settings.
        // If CustomNotificationSoundFile is empty or null, fall back to a default or do nothing.
        // For now, assume CustomNotificationSoundFile always has a valid default.
        PlaySound(_settingsManager.CustomNotificationSoundFile);
    }

    public static void PlayShutterSound()
    {
        PlaySound(ShutterSoundFile);
    }

    public static void PlayTrashSound()
    {
        PlaySound(TrashSoundFile);
    }

    public static void PlayConfiguredSound(string soundFileName)
    {
        if (string.IsNullOrWhiteSpace(soundFileName))
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(new ArgumentNullException(nameof(soundFileName), @"PlayConfiguredSound called with null or empty soundFileName."), "Attempted to play sound with an empty filename.");

            return;
        }

        PlaySound(soundFileName);
    }

    private static void PlaySound(string soundFileName)
    {
        if (string.IsNullOrWhiteSpace(soundFileName))
        {
            const string contextMessageEmpty = "Attempted to play sound with an empty filename.";

            // Notify developer
            _ = LogErrors.LogErrorAsync(new ArgumentNullException(nameof(soundFileName), contextMessageEmpty), contextMessageEmpty);

            return;
        }

        var soundPath = Path.Combine(BaseDirectory, "audio", soundFileName);
        if (!File.Exists(soundPath))
        {
            // Notify developer
            var contextMessageMissing = $"Sound file not found: {soundPath}";
            _ = LogErrors.LogErrorAsync(new FileNotFoundException(contextMessageMissing, soundPath), contextMessageMissing);

            return;
        }

        try
        {
            if (_currentMediaPlayer != null)
            {
                _currentMediaPlayer.Stop();
                _currentMediaPlayer.Close();
            }

            var playerInstance = new MediaPlayer();
            playerInstance.MediaEnded += (sender, e) =>
            {
                if (sender is not MediaPlayer endedPlayer) return;

                endedPlayer.Close();
                if (ReferenceEquals(_currentMediaPlayer, endedPlayer))
                {
                    _currentMediaPlayer = null;
                }
            };

            playerInstance.Open(new Uri(soundPath, UriKind.RelativeOrAbsolute));
            playerInstance.Play();
            _currentMediaPlayer = playerInstance;
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessageError = $"Error playing '{soundFileName}' sound from path '{soundPath}'.";
            _ = LogErrors.LogErrorAsync(ex, contextMessageError);
        }
    }
}
