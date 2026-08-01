using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.GamePad;
using SimpleLauncher.Services.PlaySound;

namespace SimpleLauncher.Services;

/// <summary>
/// Provides audio feedback and gamepad input control services to the application.
/// </summary>
public class AudioInputService : IAudioInputService, IDisposable
{
    private readonly PlaySoundEffects _playSoundEffects;
    private readonly GamePadController _gamePadController;

    /// <summary>
    /// Initializes a new instance of the <see cref="AudioInputService"/> class.
    /// </summary>
    /// <param name="playSoundEffects">The service used to play sound effects.</param>
    /// <param name="gamePadController">The gamepad input controller.</param>
    public AudioInputService(PlaySoundEffects playSoundEffects, GamePadController gamePadController)
    {
        _playSoundEffects = playSoundEffects;
        _gamePadController = gamePadController;
    }

    /// <summary>
    /// Plays the notification sound effect.
    /// </summary>
    public void PlayNotificationSound()
    {
        _playSoundEffects.PlayNotificationSound();
    }

    /// <summary>
    /// Plays the shutter sound effect.
    /// </summary>
    public void PlayShutterSound()
    {
        _playSoundEffects.PlayShutterSound();
    }

    /// <summary>
    /// Plays the trash sound effect.
    /// </summary>
    public void PlayTrashSound()
    {
        _playSoundEffects.PlayTrashSound();
    }

    /// <summary>
    /// Plays the sound effect configured for the given file name.
    /// </summary>
    /// <param name="soundFileName">The name of the configured sound file to play.</param>
    public void PlayConfiguredSound(string soundFileName)
    {
        _playSoundEffects.PlayConfiguredSound(soundFileName);
    }

    /// <summary>
    /// Gets a value indicating whether the gamepad controller is currently running.
    /// </summary>
    public bool IsGamepadRunning => _gamePadController.IsRunning;

    /// <summary>
    /// Starts the gamepad controller.
    /// </summary>
    public void StartGamepad()
    {
        _ = _gamePadController.StartAsync();
    }

    /// <summary>
    /// Stops the gamepad controller.
    /// </summary>
    public void StopGamepad()
    {
        _ = _gamePadController.StopAsync();
    }

    /// <summary>
    /// Sets the dead zone values for both gamepad stick axes.
    /// </summary>
    /// <param name="deadZoneX">The dead zone value for the X axis.</param>
    /// <param name="deadZoneY">The dead zone value for the Y axis.</param>
    public void SetGamepadDeadZone(float deadZoneX, float deadZoneY)
    {
        _gamePadController.DeadZoneX = deadZoneX;
        _gamePadController.DeadZoneY = deadZoneY;
    }

    /// <summary>
    /// Releases all resources used by the audio and gamepad services.
    /// </summary>
    public void Dispose()
    {
        _playSoundEffects.Dispose();
        _gamePadController.Dispose();
        GC.SuppressFinalize(this);
    }
}
