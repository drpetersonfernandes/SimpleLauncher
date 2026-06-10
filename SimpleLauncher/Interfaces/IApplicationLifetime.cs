namespace SimpleLauncher.Interfaces;

public interface IApplicationLifetime
{
    void Shutdown();
    void Restart();
}
