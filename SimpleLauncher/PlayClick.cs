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
            await LogErrors.LogErrorAsync(ex, "Error sending bug report from Bug Report Window");
            MessageBox.Show($"An error occurred while sending the bug report: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}