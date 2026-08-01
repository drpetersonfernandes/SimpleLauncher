namespace SimpleLauncher.Interfaces;

/// <summary>
/// Plays sound effects for application events such as notifications, screenshots, and deletions.
/// </summary>
public interface IPlaySoundEffects
{
    /// <summary>
    /// Plays the configured notification sound if notifications are enabled.
    /// </summary>
    void PlayNotificationSound();

    /// <summary>
    /// Plays the shutter sound effect.
    /// </summary>
    void PlayShutterSound();

    /// <summary>
    /// Plays the trash/delete sound effect.
    /// </summary>
    void PlayTrashSound();

    /// <summary>
    /// Plays a sound file by name from the audio directory.
    /// </summary>
    /// <param name="soundFileName">The name of the sound file to play.</param>
    void PlayConfiguredSound(string soundFileName);
}
