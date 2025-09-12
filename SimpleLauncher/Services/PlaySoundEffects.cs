#nullable enable
using System;
using System.IO;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection; // Add this using directive
using SimpleLauncher.Managers; // Add this using directive

namespace SimpleLauncher.Services;

public static class PlaySoundEffects
{
    private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

    private const string ClickSoundFile = "click.mp3";
    private const string ShutterSoundFile = "shutter.mp3";
    private const string TrashSoundFile = "trash.mp3";

    private static MediaPlayer? _currentMediaPlayer;

    public static void PlayClickSound()
    {
        PlaySound(ClickSoundFile);
    }

    public static void PlayNotificationSound()
    {
        // Retrieve SettingsManager from the service provider
        // This is acceptable for static utility classes that need singleton dependencies.
        var settings = App.ServiceProvider.GetRequiredService<SettingsManager>();
        if (!settings.EnableNotificationSound)
        {
            return;
        }

        // Use the custom sound file from settings.
        // If CustomNotificationSoundFile is empty or null, fall back to a default or do nothing.
        // For now, assume CustomNotificationSoundFile always has a valid default.
        PlaySound(settings.CustomNotificationSoundFile);
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
            _ = LogErrors.LogErrorAsync(null, "PlayConfiguredSound called with null or empty soundFileName.");

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
