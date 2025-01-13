using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;

namespace SimpleLauncher;

public static class PlayClick
{
    private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;
    private const string ClickSoundFile = "click.mp3";
    private const string ShutterSoundFile = "shutter.mp3";
    private const string TrashSoundFile = "trash.mp3";

    public static void PlayClickSound() => PlaySound(ClickSoundFile);
    public static void PlayShutterSound() => PlaySound(ShutterSoundFile);
    public static void PlayTrashSound() => PlaySound(TrashSoundFile);

    private static void PlaySound(string soundFileName)
    {
        try
        {
            var soundPath = Path.Combine(BaseDirectory, "audio", soundFileName);
            MediaPlayer mediaPlayer = new();
            mediaPlayer.Open(new Uri(soundPath, UriKind.RelativeOrAbsolute));
            mediaPlayer.Play();
        }
        catch (Exception ex)
        {
            string contextMessage =
                $"Error playing the {soundFileName} sound or the audio file could not be found or loaded.\n\n" +
                $"Exception type: {ex.GetType().Name}\n" +
                $"Exception details: {ex.Message}";
            Task logTask = LogErrors.LogErrorAsync(ex, contextMessage);
            logTask.Wait(TimeSpan.FromSeconds(2));
        }
    }
}