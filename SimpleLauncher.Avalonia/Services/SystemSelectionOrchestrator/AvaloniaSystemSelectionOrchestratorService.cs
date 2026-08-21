using SimpleLauncher.Avalonia.Interfaces;
using SimpleLauncher.Avalonia.Services.SystemManager;

namespace SimpleLauncher.Avalonia.Services.SystemSelectionOrchestrator;

/// <summary>
/// Orchestrates system selection UI: loading system.xml into the top System ComboBox,
/// coordinating system selection with the Emulator ComboBox, and refreshing the whole
/// shell after system.xml changes (port of the WPF SystemSelectionOrchestratorService
/// onto the Avalonia sidebar shell).
/// </summary>
public class AvaloniaSystemSelectionOrchestratorService
{
    private readonly SystemManagerService _systemManager;
    private readonly AvaloniaGameFileLoadingOrchestrator _loadingOrchestrator;
    private readonly ILogger _logger;
    private ISystemSelectionHost _host = null!;

    /// <summary>
    /// Initializes a new instance of the SystemSelectionOrchestratorService with the specified dependencies.
    /// </summary>
    public AvaloniaSystemSelectionOrchestratorService(
        SystemManagerService systemManager,
        AvaloniaGameFileLoadingOrchestrator loadingOrchestrator,
        ILogger logger)
    {
        _systemManager = systemManager;
        _loadingOrchestrator = loadingOrchestrator;
        _logger = logger;
    }

    /// <summary>Initializes the orchestrator with the specified UI host.</summary>
    public void Initialize(ISystemSelectionHost host)
    {
        _host = host;
    }

    /// <summary>Loads or reloads system manager configurations and updates the System ComboBox source.</summary>
    public void LoadOrReloadSystemManager()
    {
        try
        {
            var sortedSystemNames = _systemManager.LoadSystems()
                .Select(static manager => manager.SystemName)
                .OrderBy(static name => name, StringComparer.Ordinal)
                .ToList();
            _host.SetSystemComboBoxItems(sortedSystemNames);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to load or reload the system manager list");
        }
    }

    /// <summary>
    /// Handles a top System ComboBox selection change: navigates the game browser to
    /// the selected system and refreshes the Emulator ComboBox for that system.
    /// </summary>
    public void HandleSystemSelectionChanged()
    {
        try
        {
            var systemName = _host.GetSelectedSystem();
            if (string.IsNullOrEmpty(systemName)) return;

            _host.NavigateToSystem(systemName);

            var emulatorNames = _systemManager.GetSystem(systemName)?.Emulators
                                    .Select(static e => e.EmulatorName).ToList()
                                ?? [];
            _host.SetEmulatorComboBoxItems(emulatorNames);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error in the method HandleSystemSelectionChanged");
        }
    }

    /// <summary>
    /// Refreshes everything that depends on system.xml after a configuration change
    /// (system added/edited/deleted, image pack downloaded, store scan): invalidates
    /// cached data, reloads the System ComboBox, rebuilds the sidebar, restarts the
    /// ROM folder watcher, and re-syncs the Emulator ComboBox with the selection.
    /// </summary>
    public Task ReloadAfterConfigurationChangeAsync()
    {
        try
        {
            // Per-system file lists are stale once system.xml changed.
            _loadingOrchestrator.InvalidateAll();

            LoadOrReloadSystemManager();
            _host.RefreshSidebar();
            _host.RestartFileWatcher();

            // Keep the Emulator ComboBox in sync with the current selection; clear it
            // when the selected system no longer exists (renamed or deleted).
            var selectedSystem = _host.GetSelectedSystem();
            var emulatorNames = string.IsNullOrEmpty(selectedSystem)
                ? []
                : _systemManager.GetSystem(selectedSystem)?.Emulators
                      .Select(static e => e.EmulatorName).ToList()
                  ?? [];
            _host.SetEmulatorComboBoxItems(emulatorNames);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error reloading the UI after a system configuration change");
        }

        return Task.CompletedTask;
    }
}
