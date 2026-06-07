using SimpleLauncher.Services.PlaySound;

namespace SimpleLauncher.Services.AudioInput;

public interface IAudioInputService : IPlaySoundEffects
{
    bool IsGamepadRunning { get; }
    void StartGamepad();
    void StopGamepad();
    void SetGamepadDeadZone(float deadZoneX, float deadZoneY);
}
