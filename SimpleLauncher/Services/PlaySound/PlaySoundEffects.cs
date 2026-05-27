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

    private WaveOutEvent? _waveOut;
    private Mp3FileReader? _reader;

    public PlaySoundEffects(SettingsManager.SettingsManager settings)
    {
        _settingsManager = settings ?? throw new ArgumentNullException(nameof(settings));
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
            App.LogErrorAsync(
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
            App.LogErrorAsync(
                new ArgumentNullException(nameof(soundFileName), @"Attempted to play sound with an empty filename."),
                "Attempted to play sound with an empty filename.");
            return;
        }

        var soundPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "audio", soundFileName);
        if (!File.Exists(soundPath))
        {
            var contextMessageMissing = $"Sound file not found: {soundPath}";
            App.LogErrorAsync(
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
                App.LogErrorAsync(ex,
                    $"Failed to play sound: {soundPath}");
                StopCurrentPlayback();
            }
        }
    }

    private void StopCurrentPlayback()
    {
        if (_waveOut != null)
        {
            _waveOut.PlaybackStopped -= OnPlaybackStopped;
            _waveOut.Stop();
            _waveOut.Dispose();
            _waveOut = null;
        }

        try
        {
            _reader?.Dispose();
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"[PlaySoundEffects] Error disposing reader: {ex.Message}");
        }

        _reader = null;
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