using Moq;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.PlaySound;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Tests for the Linux sound backend (managed NLayer MP3 decode + ALSA output).
/// The decode path is pure managed and verified on all platforms; actual device
/// playback can only be attempted on a machine with an audio device, so the
/// end-to-end test only asserts graceful (non-throwing) behavior.
/// </summary>
public class AlsaSoundPlayerTests
{
    private static string NotificationMp3 => Path.Combine(AppContext.BaseDirectory, "audio", "notification.mp3");

    [Fact]
    public void DecodeMp3_DecodesBundledNotificationSound()
    {
        if (!File.Exists(NotificationMp3))
        {
            return; // audio files not shipped in this context
        }

        var decoded = AlsaSoundPlayer.DecodeMp3(NotificationMp3);

        Assert.NotNull(decoded);
        Assert.True(decoded!.Value.Samples.Length > 0, "MP3 should decode to PCM samples");
        Assert.InRange(decoded.Value.SampleRate, 8000, 48000);
        Assert.InRange(decoded.Value.Channels, 1, 2);
    }

    [Fact]
    public void DecodeMp3_EmptyFile_ReturnsNull()
    {
        var temp = Path.Combine(Path.GetTempPath(), "SLTest_empty_" + Guid.NewGuid().ToString("N") + ".mp3");
        File.WriteAllBytes(temp, []); // zero bytes — no frames to decode

        try
        {
            Assert.Null(AlsaSoundPlayer.DecodeMp3(temp));
        }
        finally
        {
            try
            {
                File.Delete(temp); // best effort — NLayer may briefly hold the handle
            }
            catch
            {
                // OS temp cleaner will reclaim it
            }
        }
    }

    [Fact]
    public async Task PlaySoundEffects_OnLinux_DoesNotThrow()
    {
        if (OperatingSystem.IsWindows())
        {
            return; // Windows path (NAudio) is exercised by the desktop app, not CI
        }

        var settings = new SettingsManagerService(
            new Microsoft.Extensions.Configuration.ConfigurationBuilder().Build(),
            new Mock<ILogger>().Object,
            new Mock<ICredentialProtector>().Object);
        settings.EnableNotificationSound = true;

        var player = new PlaySoundEffects(settings, new Mock<ILogger>().Object);

        try
        {
            // Must not throw whether or not an ALSA device exists (headless CI, WSL2).
            player.PlayNotificationSound();
            player.PlayShutterSound();
            player.PlayTrashSound();

            await Task.Delay(500); // allow background playback attempts to settle
        }
        finally
        {
            player.Dispose();
        }
    }
}
