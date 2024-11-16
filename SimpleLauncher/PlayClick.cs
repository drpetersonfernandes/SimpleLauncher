using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SimpleLauncher;

public static class PlayClick
{
    private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

    public static void PlayClickSound()
    {
        try
        {
            var soundPath = Path.Combine(BaseDirectory, "audio", "click.mp3");
            MediaPlayer mediaPlayer = new();
            mediaPlayer.Open(new Uri(soundPath, UriKind.RelativeOrAbsolute));
            mediaPlayer.Play();
        }
        catch (Exception ex)
        {
            string contextMessage = $"Error playing the click sound or the audio file could not be found or loaded.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
        }
    }
    
    public static void PlayShutterSound()
    {
        try
        {
            var soundPath = Path.Combine(BaseDirectory, "audio", "shutter.mp3");
            MediaPlayer mediaPlayer = new();
            mediaPlayer.Open(new Uri(soundPath, UriKind.RelativeOrAbsolute));
            mediaPlayer.Play();
        }
        catch (Exception ex)
        {
            string contextMessage = $"Error playing the shutter sound or the audio file could not be found or loaded.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
        }
    }
    
    public static void PlayTrashSound()
    {
        try
        {
            var soundPath = Path.Combine(BaseDirectory, "audio", "trash.mp3");
            MediaPlayer mediaPlayer = new();
            mediaPlayer.Open(new Uri(soundPath, UriKind.RelativeOrAbsolute));
            mediaPlayer.Play();
        }
        catch (Exception ex)
        {
            string contextMessage = $"Error playing the trash sound or the audio file could not be found or loaded.\n\nException type: {ex.GetType().Name}\nException details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
        }
    }
}