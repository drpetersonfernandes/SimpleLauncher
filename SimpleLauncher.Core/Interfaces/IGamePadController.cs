using SimpleLauncher.Core.Models;

namespace SimpleLauncher.Core.Interfaces;

/// <summary>
/// Provides methods to manage gamepad input, including starting/stopping polling and handling button press events.
/// </summary>
public interface IGamePadController : IDisposable
{
    /// <summary>
    /// Gets a value indicating whether the gamepad polling loop is currently active.
    /// </summary>
    bool IsRunning { get; }

    /// <summary>
    /// Occurs when a gamepad button is pressed.
    /// </summary>
    event EventHandler<EventArgs<string>>? ButtonPressed;

    /// <summary>
    /// Starts the gamepad polling loop for processing controller input.
    /// </summary>
    Task Start();

    /// <summary>
    /// Stops the gamepad polling loop.
    /// </summary>
    Task Stop();

    /// <summary>
    /// Sets the dead zone thresholds for the gamepad analog sticks.
    /// </summary>
    /// <param name="deadZoneX">The dead zone value for the X-axis.</param>
    /// <param name="deadZoneY">The dead zone value for the Y-axis.</param>
    void SetDeadZone(float deadZoneX, float deadZoneY);
}
