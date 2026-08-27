using System.Runtime.InteropServices;

namespace SimpleLauncher.Core.Services.PlaySound;

/// <summary>
/// Minimal ALSA (libasound.so.2) P/Invoke surface used for PCM playback.
/// </summary>
internal static class AlsaNative
{
    internal const int SndPcmStreamPlayback = 0;
    internal const int SndPcmFormatS16Le = 2;
    internal const int SndPcmAccessRwInterleaved = 3;

    [DllImport("libasound.so.2")]
    internal static extern int snd_pcm_open(out IntPtr pcm, [MarshalAs(UnmanagedType.LPUTF8Str)] string name,
        int stream, int mode);

    [DllImport("libasound.so.2")]
    internal static extern int snd_pcm_set_params(IntPtr pcm, int format, int access, int channels, int rate,
        int softResample, int latency);

    [DllImport("libasound.so.2")]
    internal static extern long snd_pcm_writei(IntPtr pcm, byte[] buffer, long size);

    [DllImport("libasound.so.2")]
    internal static extern int snd_pcm_prepare(IntPtr pcm);

    [DllImport("libasound.so.2")]
    internal static extern int snd_pcm_drain(IntPtr pcm);

    [DllImport("libasound.so.2")]
    internal static extern int snd_pcm_close(IntPtr pcm);
}