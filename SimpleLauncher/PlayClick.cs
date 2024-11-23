using System;
using System.IO;
using System.Threading.Tasks;
using NAudio.Wave;

namespace SimpleLauncher
{
    public static class PlayClick
    {
        private static readonly string BaseDirectory = AppDomain.CurrentDomain.BaseDirectory;

        private static async Task PlaySoundAsync(string soundFileName)
        {
            string soundPath = Path.Combine(BaseDirectory, "audio", soundFileName);

            if (!File.Exists(soundPath))
            {
                throw new FileNotFoundException($"Sound file '{soundFileName}' not found.");
            }

            try
            {
                await using var audioFileReader = new AudioFileReader(soundPath);
                using var waveOut = new WaveOutEvent();
                
                waveOut.Init(audioFileReader);
                waveOut.Play();

                // Wait for the sound to finish playing
                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    await Task.Delay(100); // Check every 100ms
                }
            }
            catch (Exception ex)
            {
                string contextMessage = $"Error playing the sound '{soundFileName}'.\n\n" +
                                        $"Exception type: {ex.GetType().Name}\nException details: {ex.Message}";
                await LogErrors.LogErrorAsync(ex, contextMessage);
            }
        }

        public static Task PlayClickSound() => PlaySoundAsync("click.mp3");
        public static Task PlayShutterSound() => PlaySoundAsync("shutter.mp3");
        public static Task PlayTrashSound() => PlaySoundAsync("trash.mp3");
    }
}