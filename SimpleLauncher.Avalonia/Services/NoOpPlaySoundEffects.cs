using SimpleLauncher.Core.Services.PlaySound;

namespace SimpleLauncher.Avalonia.Services;

public class NoOpPlaySoundEffects : IPlaySoundEffects
{
    public void PlayNotificationSound()
    {
    }

    public void PlayShutterSound()
    {
    }

    public void PlayTrashSound()
    {
    }

    public void PlayConfiguredSound(string soundFileName)
    {
    }
}
