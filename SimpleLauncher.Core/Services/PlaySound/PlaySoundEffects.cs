using NAudio.Wave;
using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Core.Services.PlaySound;

/// <summary>
/// Plays UI sound effects such as click, shutter, and trash sounds using NAudio.
/// </summary>
public class PlaySoundEffects : IPlaySoundEffects, IDisposable
{
    private const string ClickSoundFile = "click.mp3";
    private const string ShutterSoundFile = "shutter.mp3";
    private const string TrashSoundFile = "trash.mp3";

    private static readonly Lock Lock = new();
    private readonly SettingsManager.SettingsManagerService _settingsManager;
    private readonly ILogger _logger;

    private WaveOutEvent? _waveOut;
    private Mp3FileReader? _reader;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaySoundEffects"/> class.
    /// </summary>
    public PlaySoundEffects(SettingsManager.SettingsManagerService settings, ILogger logger)
    {
        _settingsManager = settings ?? throw new ArgumentNullException(nameof(settings));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Plays the configured notification sound if notifications are enabled.
    /// </summary>
    public void PlayNotificationSound()
    {
        if (!_settingsManager.EnableNotificationSound)
        {
            return;
        }

        PlaySound(_settingsManager.CustomNotificationSoundFile ?? ClickSoundFile);
    }

    /// <summary>
    /// Plays the shutter sound effect.
    /// </summary>
    public void PlayShutterSound()
    {
        PlaySound(ShutterSoundFile);
    }

    /// <summary>
    /// Plays the trash/delete sound effect.
    /// </summary>
    public void PlayTrashSound()
    {
        PlaySound(TrashSoundFile);
    }

    /// <summary>
    /// Plays a sound file by name from the audio directory.
    /// </summary>
    public void PlayConfiguredSound(string soundFileName)
    {
        if (string.IsNullOrWhiteSpace(soundFileName))
        {
            lock (Lock)
            {
                _logger.Error(
                    new ArgumentNullException(nameof(soundFileName), "PlayConfiguredSound called with null or empty soundFileName."),
                    "Attempted to play sound with an empty filename.");
            }

            return;
        }

        PlaySound(soundFileName);
    }

    private void PlaySound(string soundFileName)
    {
        if (string.IsNullOrWhiteSpace(soundFileName))
        {
            _logger.Error(
                new ArgumentNullException(nameof(soundFileName), "Attempted to play sound with an empty filename."),
                "Attempted to play sound with an empty filename.");
            return;
        }

        var soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audio", soundFileName);
        if (!File.Exists(soundPath))
        {
            var contextMessageMissing = $"Sound file not found: {soundPath}";
            _logger.Error(
                new FileNotFoundException(contextMessageMissing, soundPath),
                contextMessageMissing);
            return;
        }

        lock (Lock)
        {
            StopCurrentPlayback();

            try
            {
                _reader = new Mp3FileReader(soundPath);
                _waveOut = new WaveOutEvent();
                _waveOut.PlaybackStopped += OnPlaybackStopped;
                _waveOut.Init(_reader);
                _waveOut.Play();
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    $"Failed to play sound: {soundPath}");
                StopCurrentPlayback();
            }
        }
    }

    private void StopCurrentPlayback()
    {
        var waveOut = _waveOut;
        if (waveOut != null)
        {
            _waveOut = null;
            waveOut.PlaybackStopped -= OnPlaybackStopped;
            try
            {
                waveOut.Stop();
            }
            catch (Exception ex)
            {
                _logger.Debug($"[PlaySoundEffects] Error stopping waveOut: {ex.Message}");
            }
        }

        var reader = _reader;
        if (reader != null)
        {
            _reader = null;
            try
            {
                reader.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Debug($"[PlaySoundEffects] Error disposing reader: {ex.Message}");
            }
        }

        if (waveOut != null)
        {
            try
            {
                waveOut.Dispose();
            }
            catch (Exception ex)
            {
                _logger.Debug($"[PlaySoundEffects] Error disposing waveOut: {ex.Message}");
            }
        }
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        lock (Lock)
        {
            if (_waveOut == sender)
            {
                StopCurrentPlayback();
            }
        }
    }

    /// <summary>
    /// Stops any current playback and releases audio resources.
    /// </summary>
    public void Dispose()
    {
        lock (Lock)
        {
            StopCurrentPlayback();
        }

        GC.SuppressFinalize(this);
    }
}
