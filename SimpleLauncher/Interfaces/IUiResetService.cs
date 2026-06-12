namespace SimpleLauncher.Interfaces;

public interface IUiResetService
{
    void Initialize(IUiResetHost host);
    Task ResetUiAsync();
}
