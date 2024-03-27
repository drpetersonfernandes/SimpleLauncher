using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace SimpleLauncher;

public static class PlayClick
{
    private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

    public static async void PlayClickSound()
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
            string errorMessage = $"Error playing the click sound.";
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            await LogErrors.LogErrorAsync(ex, $"{errorMessage}\n\nException details: {ex}");
        }
    }
}