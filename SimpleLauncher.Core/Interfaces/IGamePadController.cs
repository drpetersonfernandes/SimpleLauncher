#nullable enable

namespace SimpleLauncher.Core.Interfaces;

public interface IGamePadController : IDisposable
{
    bool IsRunning { get; }
    event Action<string>? ButtonPressed;
    Task Start();
    Task Stop();
    void SetDeadZone(float deadZoneX, float deadZoneY);
}
