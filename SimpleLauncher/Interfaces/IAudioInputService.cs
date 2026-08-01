namespace SimpleLauncher.Interfaces;

/// <summary>
/// Extends sound effect playback with gamepad input support for audio-driven interactions.
/// </summary>
public interface IAudioInputService : IPlaySoundEffects
{
    /// <summary>
    /// Gets a value indicating whether the gamepad polling loop is currently active.
    /// </summary>
    bool IsGamepadRunning { get; }

    /// <summary>
    /// Starts the gamepad polling loop for processing controller input.
    /// </summary>
    void StartGamepad();

    /// <summary>
    /// Stops the gamepad polling loop.
    /// </summary>
    void StopGamepad();

    /// <summary>
    /// Sets the dead zone thresholds for the gamepad analog sticks.
    /// </summary>
    /// <param name="deadZoneX">The dead zone value for the X-axis.</param>
    /// <param name="deadZoneY">The dead zone value for the Y-axis.</param>
    void SetGamepadDeadZone(float deadZoneX, float deadZoneY);
}
