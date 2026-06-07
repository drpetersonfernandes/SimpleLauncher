namespace SimpleLauncher.Services.SystemSelectionOrchestrator;

public interface ISystemSelectionOrchestrator
{
    void Initialize(ISystemSelectionHost host);
    Task DisplaySystemSelectionScreenAsync(CancellationToken cancellationToken = default);
    Task SystemComboBoxSelectionChangedAsync(CancellationToken cancellationToken = default);
    Task SystemButtonClickAsync(string systemName, CancellationToken cancellationToken);
    void DeleteSystemFromContextMenu(string systemName);
    void EditSystemFromContextMenu(string systemName);
    void LoadOrReloadSystemManager();
}
