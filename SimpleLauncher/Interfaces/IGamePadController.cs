using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

public interface IGamePadController : IDisposable
{
    bool IsRunning { get; }
    event EventHandler<EventArgs<string>>? ButtonPressed;
    Task Start();
    Task Stop();
    void SetDeadZone(float deadZoneX, float deadZoneY);
}
