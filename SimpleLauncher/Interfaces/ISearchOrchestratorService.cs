using SimpleLauncher.Services.SearchOrchestrator;
using SimpleLauncher.Models;

namespace SimpleLauncher.Interfaces;

public interface ISearchOrchestratorService
{
    Task<SearchValidationResult> ValidateAndPrepareAsync(string searchQuery, string? selectedSystem, CancellationToken cancellationToken);
}
