using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia.Services;

public class NoOpGamePadController : IGamePadController
{
    public bool IsRunning => false;
    public event Action<string>? ButtonPressed;

    public Task Start()
    {
        return Task.CompletedTask;
    }

    public Task Stop()
    {
        return Task.CompletedTask;
    }

    public void SetDeadZone(float deadZoneX, float deadZoneY)
    {
        // No-op
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
