using System;
using System.IO;
using System.Windows.Media;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Managers;
 
namespace SimpleLauncher.Services;

public class PlaySoundEffects
{
    private readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

    private const string ClickSoundFile = "click.mp3";
    private const string ShutterSoundFile = "shutter.mp3";
    private const string TrashSoundFile = "trash.mp3";

    private static MediaPlayer _currentMediaPlayer;
    private readonly SettingsManager _settingsManager;

    /// <summary>
    /// Initializes the PlaySoundEffects service with the necessary dependencies.
    /// </summary>
    /// <param name="settings">The application's settings manager.</param>
    public PlaySoundEffects(SettingsManager settings)
    {
        _settingsManager = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    public void PlayNotificationSound()
    {
        if (_settingsManager == null)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new InvalidOperationException("PlaySoundEffects not initialized with SettingsManager."), "Attempted to play notification sound before PlaySoundEffects was initialized.");

            return;
        }

        if (!_settingsManager.EnableNotificationSound)
        {
            return;
        }

        // Use the custom sound file from settings.
        PlaySound(_settingsManager.CustomNotificationSoundFile ?? ClickSoundFile);
    }

    public void PlayShutterSound()
    {
        PlaySound(ShutterSoundFile);
    }

    public void PlayTrashSound()
    {
        PlaySound(TrashSoundFile);
    }

    public void PlayConfiguredSound(string soundFileName)
    {
        if (string.IsNullOrWhiteSpace(soundFileName))
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new ArgumentNullException(nameof(soundFileName), @"PlayConfiguredSound called with null or empty soundFileName."), "Attempted to play sound with an empty filename.");

            return;
        }

        PlaySound(soundFileName);
    }

    private void PlaySound(string soundFileName)
    {
        if (string.IsNullOrWhiteSpace(soundFileName))
        {
            const string contextMessageEmpty = "Attempted to play sound with an empty filename.";

            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new ArgumentNullException(nameof(soundFileName), contextMessageEmpty), contextMessageEmpty);

            return;
        }

        var soundPath = Path.Combine(_baseDirectory, "audio", soundFileName);
        if (!File.Exists(soundPath))
        {
            // Notify developer
            var contextMessageMissing = $"Sound file not found: {soundPath}";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(new FileNotFoundException(contextMessageMissing, soundPath), contextMessageMissing);

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
            playerInstance.MediaEnded += static (sender, _) =>
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
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessageError);
        }
    }
}