namespace SimpleLauncher.Services.UIReset;

public interface IUiResetService
{
    void Initialize(IUiResetHost host);
    Task ResetUiAsync();
}
