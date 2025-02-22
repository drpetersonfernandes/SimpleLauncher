#nullable enable
using System;
using System.IO;
using System.Windows.Media;

namespace SimpleLauncher;

public static class PlayClick
{
    private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private const string ClickSoundFile = "click.mp3";
    private const string ShutterSoundFile = "shutter.mp3";
    private const string TrashSoundFile = "trash.mp3";

    // Keep a reference to prevent garbage collection
    private static MediaPlayer? _mediaPlayer;

    public static void PlayClickSound() => PlaySound(ClickSoundFile);
    public static void PlayShutterSound() => PlaySound(ShutterSoundFile);
    public static void PlayTrashSound() => PlaySound(TrashSoundFile);

    private static void PlaySound(string soundFileName)
    {
        try
        {
            var soundPath = Path.Combine(BaseDirectory, "audio", soundFileName);
            _mediaPlayer = new MediaPlayer();
            _mediaPlayer.MediaEnded += (_, _) =>
            {
                _mediaPlayer.Close();
                _mediaPlayer = null;
            };
            _mediaPlayer?.Open(new Uri(soundPath, UriKind.RelativeOrAbsolute));
            _mediaPlayer?.Play();
        }
        catch (Exception ex)
        {
            // Notify developer
            var contextMessage =
                $"Error playing '{soundFileName}' sound.\n\n" +
                $"Exception type: {ex.GetType().Name}\n" +
                $"Exception details: {ex.Message}";
            LogErrors.LogErrorAsync(ex, contextMessage).Wait(TimeSpan.FromSeconds(2));
        }
    }
}