using System.Runtime.InteropServices;
using NLayer;

namespace SimpleLauncher.Core.Services.PlaySound;

/// <summary>
/// Plays PCM audio on Linux through the ALSA API (libasound.so.2), with MP3 decoding
/// done in managed code via NLayer. Falls back silently when no audio device exists
/// (e.g. containers, WSL2 without audio, headless CI) — the caller must not crash.
/// </summary>
/// <remarks>
/// Only used at runtime on non-Windows platforms; the P/Invoke declarations compile
/// on all TFMs. ALSA's "default" device routes to PulseAudio/PipeWire on desktop distros.
/// </remarks>
public sealed class AlsaSoundPlayer : IDisposable
{
    private readonly ILogger _logger;
    private readonly Lock _lock = new();
    private CancellationTokenSource? _playbackCts;
    private IntPtr _pcm;

    /// <summary>
    /// Initializes a new instance of the <see cref="AlsaSoundPlayer"/> class.
    /// </summary>
    public AlsaSoundPlayer(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Decodes an MP3 file to interleaved float samples using NLayer (managed, cross-platform).
    /// </summary>
    /// <param name="mp3Path">Path to the MP3 file.</param>
    /// <returns>(samples, sampleRate, channels), or null when the file cannot be decoded.</returns>
    internal static (float[] Samples, int SampleRate, int Channels)? DecodeMp3(string mp3Path)
    {
        try
        {
            using var mpeg = new MpegFile(mp3Path);
            var sampleRate = mpeg.SampleRate;
            var channels = mpeg.Channels;
            if (sampleRate <= 0 || channels <= 0)
            {
                return null;
            }

            var samples = new List<float>(sampleRate * channels * 2);
            var buffer = new float[8192];
            int read;
            while ((read = mpeg.ReadSamples(buffer, 0, buffer.Length)) > 0)
            {
                for (var i = 0; i < read; i++)
                {
                    samples.Add(buffer[i]);
                }
            }

            if (samples.Count == 0)
            {
                return null;
            }

            return (samples.ToArray(), sampleRate, channels);
        }
        catch (Exception ex)
        {
            Log.Debug(ex, "[AlsaSoundPlayer] Failed to decode MP3: {Path}", mp3Path);
            return null;
        }
    }

    /// <summary>
    /// Decodes and plays an MP3 file asynchronously. Any failure (missing libasound,
    /// no audio device, invalid file) is logged and silently ignored.
    /// </summary>
    /// <param name="mp3Path">Path to the MP3 file.</param>
    public void Play(string mp3Path)
    {
        lock (_lock)
        {
            _playbackCts?.Cancel();
            _playbackCts?.Dispose();
            _playbackCts = new CancellationTokenSource();
        }

        var token = _playbackCts.Token;
        _ = Task.Run(() => PlayCoreAsync(mp3Path, token));
    }

    /// <summary>
    /// Stops any in-progress playback.
    /// </summary>
    public void Stop()
    {
        lock (_lock)
        {
            _playbackCts?.Cancel();
        }
    }

    private void PlayCoreAsync(string mp3Path, CancellationToken token)
    {
        try
        {
            var decoded = DecodeMp3(mp3Path);
            if (decoded is not { } audio)
            {
                return;
            }

            lock (_lock)
            {
                ClosePcm();
                if (AlsaNative.snd_pcm_open(out _pcm, "default", AlsaNative.SndPcmStreamPlayback, 0) != 0)
                {
                    _logger.Debug("[AlsaSoundPlayer] No ALSA audio device available; sound skipped.");
                    ClosePcm();
                    return;
                }

                var result = AlsaNative.snd_pcm_set_params(
                    _pcm, AlsaNative.SndPcmFormatS16Le, AlsaNative.SndPcmAccessRwInterleaved,
                    audio.Channels, audio.SampleRate, 1, 500000);
                if (result != 0)
                {
                    _logger.Debug($"[AlsaSoundPlayer] snd_pcm_set_params failed ({result}); sound skipped.");
                    ClosePcm();
                    return;
                }
            }

            // float (-1..1) interleaved -> S16LE bytes
            var frames = audio.Samples.Length / audio.Channels;
            var buffer = new byte[4096 * audio.Channels * 2]; // 4096 frames per write
            var frameOffset = 0;

            while (frameOffset < frames && !token.IsCancellationRequested)
            {
                var framesThisChunk = Math.Min(4096, frames - frameOffset);
                var bytes = framesThisChunk * audio.Channels * 2;
                for (var i = 0; i < framesThisChunk * audio.Channels; i++)
                {
                    var sample = audio.Samples[(frameOffset * audio.Channels) + i];
                    var clamped = Math.Clamp(sample, -1f, 1f);
                    var s16 = (short)(clamped * short.MaxValue);
                    buffer[i * 2] = (byte)(s16 & 0xFF);
                    buffer[i * 2 + 1] = (byte)((s16 >> 8) & 0xFF);
                }

                long written;
                lock (_lock)
                {
                    if (_pcm == IntPtr.Zero)
                    {
                        return;
                    }

                    written = AlsaNative.snd_pcm_writei(_pcm, buffer, framesThisChunk);
                }

                if (written < 0)
                {
                    // -EPIPE = underrun: recover by preparing the device and retrying once
                    lock (_lock)
                    {
                        AlsaNative.snd_pcm_prepare(_pcm);
                    }

                    if (written == -32)
                    {
                        continue;
                    }

                    _logger.Debug($"[AlsaSoundPlayer] snd_pcm_writei failed ({written}); sound truncated.");
                    return;
                }

                frameOffset += (int)written;
            }

            lock (_lock)
            {
                if (_pcm != IntPtr.Zero)
                {
                    AlsaNative.snd_pcm_drain(_pcm);
                }
            }
        }
        catch (DllNotFoundException)
        {
            _logger.Debug("[AlsaSoundPlayer] libasound not present; sound skipped.");
        }
        catch (Exception ex)
        {
            _logger.Debug(ex, "[AlsaSoundPlayer] Playback failed");
        }
        finally
        {
            lock (_lock)
            {
                ClosePcm();
            }
        }
    }

    private void ClosePcm()
    {
        if (_pcm != IntPtr.Zero)
        {
            AlsaNative.snd_pcm_close(_pcm);
            _pcm = IntPtr.Zero;
        }
    }

    /// <summary>
    /// Stops playback and releases the ALSA handle.
    /// </summary>
    public void Dispose()
    {
        lock (_lock)
        {
            _playbackCts?.Cancel();
            _playbackCts?.Dispose();
            _playbackCts = null;
            ClosePcm();
        }

        GC.SuppressFinalize(this);
    }
}

/// <summary>
/// Minimal ALSA (libasound.so.2) P/Invoke surface used for PCM playback.
/// </summary>
internal static class AlsaNative
{
    internal const int SndPcmStreamPlayback = 0;
    internal const int SndPcmFormatS16Le = 2;
    internal const int SndPcmAccessRwInterleaved = 3;

    [DllImport("libasound.so.2")]
    internal static extern int snd_pcm_open(out IntPtr pcm, [MarshalAs(UnmanagedType.LPUTF8Str)] string name, int stream, int mode);

    [DllImport("libasound.so.2")]
    internal static extern int snd_pcm_set_params(IntPtr pcm, int format, int access, int channels, int rate, int softResample, int latency);

    [DllImport("libasound.so.2")]
    internal static extern long snd_pcm_writei(IntPtr pcm, byte[] buffer, long size);

    [DllImport("libasound.so.2")]
    internal static extern int snd_pcm_prepare(IntPtr pcm);

    [DllImport("libasound.so.2")]
    internal static extern int snd_pcm_drain(IntPtr pcm);

    [DllImport("libasound.so.2")]
    internal static extern int snd_pcm_close(IntPtr pcm);
}
