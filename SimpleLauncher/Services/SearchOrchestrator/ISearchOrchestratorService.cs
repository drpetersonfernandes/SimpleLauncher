namespace SimpleLauncher.Services.SearchOrchestrator;

public interface ISearchOrchestratorService
{
    Task<SearchValidationResult> ValidateAndPrepareAsync(string searchQuery, string selectedSystem, CancellationToken cancellationToken);
}
