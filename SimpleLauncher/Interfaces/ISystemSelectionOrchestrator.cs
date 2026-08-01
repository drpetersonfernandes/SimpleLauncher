namespace SimpleLauncher.Interfaces;

/// <summary>
/// Orchestrates system selection screen display and system selection changes.
/// </summary>
public interface ISystemSelectionOrchestrator
{
    /// <summary>
    /// Initializes the orchestrator with the specified UI host.
    /// </summary>
    /// <param name="host">The host providing system selection UI access.</param>
    void Initialize(ISystemSelectionHost host);

    /// <summary>
    /// Displays the system selection screen with clickable system buttons.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task DisplaySystemSelectionScreenAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles system combo box selection changes, loading the selected system's games and metadata.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SystemComboBoxSelectionChangedAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Handles a system button click, loading the selected system's games.
    /// </summary>
    /// <param name="systemName">The name of the system to load.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SystemButtonClickAsync(string systemName, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a system configuration after user confirmation.
    /// </summary>
    /// <param name="systemName">The name of the system to delete.</param>
    void DeleteSystemFromContextMenuAsync(string systemName);

    /// <summary>
    /// Opens the edit system dialog for the specified system.
    /// </summary>
    /// <param name="systemName">The name of the system to edit.</param>
    void EditSystemFromContextMenu(string systemName);

    /// <summary>
    /// Loads or reloads system manager configurations and updates the combo box source.
    /// </summary>
    void LoadOrReloadSystemManager();
}
