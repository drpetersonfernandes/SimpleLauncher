using SimpleLauncher.Services.SearchOrchestrator;

namespace SimpleLauncher.Interfaces;

public interface ISearchOrchestratorService
{
    Task<SearchValidationResult> ValidateAndPrepareAsync(string searchQuery, string selectedSystem, CancellationToken cancellationToken);
}
