namespace SimpleLauncher.Core.Interfaces;

public interface IApplicationLifetime
{
    void Shutdown();
    void Restart();
}
