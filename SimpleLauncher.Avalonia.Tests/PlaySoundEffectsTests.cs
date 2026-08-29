using Microsoft.Extensions.Configuration;
using Moq;
using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.PlaySound;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
///     Tests for <see cref="PlaySoundEffects" /> (NAudio 3 playback: Media Foundation +
///     WaveOut on Windows, libsndfile + ALSA on Linux). Actual device playback can only
///     be attempted on a machine with an audio device, so the test asserts graceful
///     (non-throwing) behavior — headless CI and WSL2 have no audio device.
/// </summary>
public class PlaySoundEffectsTests
{
    [Fact]
    public async Task PlaySoundEffects_OnLinux_DoesNotThrow()
    {
        if (OperatingSystem.IsWindows()) return; // Windows path is exercised by the desktop app, not CI

        var settings = new SettingsManagerService(
            new ConfigurationBuilder().Build(),
            new Mock<ILogger>().Object,
            new Mock<ICredentialProtector>().Object);
        settings.EnableNotificationSound = true;

        var player = new PlaySoundEffects(settings, new Mock<ILogger>().Object);

        try
        {
            // Must not throw whether or not an ALSA device exists (headless CI, WSL2),
            // and whether or not the sound file exists.
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