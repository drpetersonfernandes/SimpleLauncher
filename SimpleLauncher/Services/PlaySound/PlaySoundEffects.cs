#nullable enable
using System.IO;
using NAudio.Wave;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.PlaySound;

public interface IPlaySoundEffects
{
    void PlayNotificationSound();
    void PlayShutterSound();
    void PlayTrashSound();
    void PlayConfiguredSound(string soundFileName);
}

public class PlaySoundEffects : IPlaySoundEffects, IDisposable
{
    private const string ClickSoundFile = "click.mp3";
    private const string ShutterSoundFile = "shutter.mp3";
    private const string TrashSoundFile = "trash.mp3";

    private static readonly Lock Lock = new();
    private readonly SettingsManager.SettingsManager _settingsManager;
    private readonly ILogErrors _logErrors;

    private WaveOutEvent? _waveOut;
    private Mp3FileReader? _reader;

    public PlaySoundEffects(SettingsManager.SettingsManager settings, ILogErrors logErrors)
    {
        _settingsManager = settings ?? throw new ArgumentNullException(nameof(settings));
        _logErrors = logErrors ?? throw new ArgumentNullException(nameof(logErrors));
    }

    public void PlayNotificationSound()
    {
        if (!_settingsManager.EnableNotificationSound)
        {
            return;
        }

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
            _logErrors.LogAndForget(
                new ArgumentNullException(nameof(soundFileName), @"PlayConfiguredSound called with null or empty soundFileName."),
                "Attempted to play sound with an empty filename.");
            return;
        }

        PlaySound(soundFileName);
    }

    private void PlaySound(string soundFileName)
    {
        if (string.IsNullOrWhiteSpace(soundFileName))
        {
            _logErrors.LogAndForget(
                new ArgumentNullException(nameof(soundFileName), @"Attempted to play sound with an empty filename."),
                "Attempted to play sound with an empty filename.");
            return;
        }

        var soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audio", soundFileName);
        if (!File.Exists(soundPath))
        {
            var contextMessageMissing = $"Sound file not found: {soundPath}";
            _logErrors.LogAndForget(
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
                _logErrors.LogAndForget(ex,
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
                DebugLogger.Log($"[PlaySoundEffects] Error stopping waveOut: {ex.Message}");
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
                DebugLogger.Log($"[PlaySoundEffects] Error disposing reader: {ex.Message}");
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
                DebugLogger.Log($"[PlaySoundEffects] Error disposing waveOut: {ex.Message}");
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

    public void Dispose()
    {
        lock (Lock)
        {
            StopCurrentPlayback();
        }

        GC.SuppressFinalize(this);
    }
}