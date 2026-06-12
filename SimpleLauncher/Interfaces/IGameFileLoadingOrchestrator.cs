namespace SimpleLauncher.Interfaces;

public interface IGameFileLoadingOrchestrator
{
    void Initialize(IGameFileLoadingHost host);
    Task LoadGameFilesAsync(string startLetter = null, string searchQuery = null, CancellationToken cancellationToken = default);
    Task InvalidateGameFileCachesAsync(CancellationToken cancellationToken = default);
    void OnGameFilesChanged(string systemName);
}
