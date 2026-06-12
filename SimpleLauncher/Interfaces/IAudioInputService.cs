namespace SimpleLauncher.Interfaces;

public interface IAudioInputService : IPlaySoundEffects
{
    bool IsGamepadRunning { get; }
    void StartGamepad();
    void StopGamepad();
    void SetGamepadDeadZone(float deadZoneX, float deadZoneY);
}
