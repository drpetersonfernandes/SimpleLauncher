using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.PlaySound;

namespace SimpleLauncher.Services.AudioInput;

using Interfaces;

public class AudioInputService : IAudioInputService, IDisposable
{
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly GamePadController _gamePadController;

    public AudioInputService(PlaySoundEffects playSoundEffects, GamePadController gamePadController)
    {
        _playSoundEffects = playSoundEffects;
        _gamePadController = gamePadController;
    }

    public void PlayNotificationSound()
    {
        _playSoundEffects.PlayNotificationSound();
    }

    public void PlayShutterSound()
    {
        _playSoundEffects.PlayShutterSound();
    }

    public void PlayTrashSound()
    {
        _playSoundEffects.PlayTrashSound();
    }

    public void PlayConfiguredSound(string soundFileName)
    {
        _playSoundEffects.PlayConfiguredSound(soundFileName);
    }

    public bool IsGamepadRunning => _gamePadController.IsRunning;

    public void StartGamepad()
    {
        _ = _gamePadController.Start();
    }

    public void StopGamepad()
    {
        _ = _gamePadController.Stop();
    }

    public void SetGamepadDeadZone(float deadZoneX, float deadZoneY)
    {
        _gamePadController.DeadZoneX = deadZoneX;
        _gamePadController.DeadZoneY = deadZoneY;
    }

    public void Dispose()
    {
        _playSoundEffects.Dispose();
        _gamePadController.Dispose();
        GC.SuppressFinalize(this);
    }
}
