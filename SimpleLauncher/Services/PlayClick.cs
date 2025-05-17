#nullable enable
using System;
using System.IO;
using System.Windows.Media;

namespace SimpleLauncher.Services;

public static class PlayClick
{
    private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private const string ClickSoundFile = "click.mp3";
    private const string NotificationSoundFile = "notification.mp3";
    private const string ShutterSoundFile = "shutter.mp3";
    private const string TrashSoundFile = "trash.mp3";

    // This field will hold the currently playing MediaPlayer instance,
    // ensuring only one sound plays at a time and allowing us to manage its lifecycle.
    private static MediaPlayer? _currentMediaPlayer;

    public static void PlayClickSound()
    {
        PlaySound(ClickSoundFile);
    }

    public static void PlayNotificationSound()
    {
        PlaySound(NotificationSoundFile);
    }

    public static void PlayShutterSound()
    {
        PlaySound(ShutterSoundFile);
    }

    public static void PlayTrashSound()
    {
        PlaySound(TrashSoundFile);
    }

    private static void PlaySound(string soundFileName)
    {
        var soundPath = Path.Combine(BaseDirectory, "audio", soundFileName);
        if (!File.Exists(soundPath))
        {
            var contextMessageMissing = $"Sound file not found: {soundPath}";
            _ = LogErrors.LogErrorAsync(new FileNotFoundException(contextMessageMissing, soundPath), contextMessageMissing);
            return;
        }

        try
        {
            // If a sound is already playing, stop and release it.
            // This ensures only one sound plays at a time and cleans up the previous instance.
            if (_currentMediaPlayer != null)
            {
                _currentMediaPlayer.Stop();
                // Detach the event handler from the old player to prevent it from firing later.
                // Since the handler is a new lambda each time, we can't easily store and remove a specific one.
                // However, by setting _currentMediaPlayer to null after closing, the old handler,
                // if it fires, will see that its player is not the _currentMediaPlayer.
                _currentMediaPlayer.Close(); // Release resources of the old player
            }

            // Create a new MediaPlayer instance for this sound.
            var playerInstance = new MediaPlayer();

            // The event handler now correctly uses the 'sender' (which is the playerInstance that finished).
            playerInstance.MediaEnded += (sender, e) =>
            {
                if (sender is not MediaPlayer endedPlayer) return;

                endedPlayer.Close(); // Close the specific player that ended.

                // If the player that just ended is the one currently tracked by _currentMediaPlayer,
                // then we can clear _currentMediaPlayer. This is important to avoid issues if
                // another sound started playing immediately after this one was queued but before it ended.
                if (ReferenceEquals(_currentMediaPlayer, endedPlayer))
                {
                    _currentMediaPlayer = null;
                }
            };

            playerInstance.Open(new Uri(soundPath, UriKind.RelativeOrAbsolute));
            playerInstance.Play();

            // Update the static reference to point to the new player.
            _currentMediaPlayer = playerInstance;
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessageError = $"Error playing '{soundFileName}' sound.";
            _ = LogErrors.LogErrorAsync(ex, contextMessageError);
        }
    }
}
