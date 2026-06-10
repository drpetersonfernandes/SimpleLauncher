namespace SimpleLauncher.Services.PlaySound;

public interface IPlaySoundEffects
{
    void PlayNotificationSound();
    void PlayShutterSound();
    void PlayTrashSound();
    void PlayConfiguredSound(string soundFileName);
}
