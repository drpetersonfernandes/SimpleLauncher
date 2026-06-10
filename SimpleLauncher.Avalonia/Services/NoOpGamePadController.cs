using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Avalonia.Services;

public class NoOpGamePadController : IGamePadController
{
    public bool IsRunning => false;
#pragma warning disable CS0067 // Event is never used
    public event Action<string>? ButtonPressed;
#pragma warning restore CS0067

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
