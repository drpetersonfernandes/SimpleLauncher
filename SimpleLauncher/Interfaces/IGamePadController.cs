#nullable enable

namespace SimpleLauncher.Interfaces;

public interface IGamePadController : IDisposable
{
    bool IsRunning { get; }
    event Action<string>? ButtonPressed;
    Task Start();
    Task Stop();
    void SetDeadZone(float deadZoneX, float deadZoneY);
}
