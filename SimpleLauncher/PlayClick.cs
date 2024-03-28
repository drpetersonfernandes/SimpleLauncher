using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
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
            string contextMessage = $"Error playing the click sound.\n\nException details: {ex}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            MessageBox.Show(contextMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            logTask.Wait(TimeSpan.FromSeconds(2));
        }
    }
}