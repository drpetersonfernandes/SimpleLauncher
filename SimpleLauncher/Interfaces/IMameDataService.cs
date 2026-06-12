using SimpleLauncher.Services.MameManager;

namespace SimpleLauncher.Interfaces;

public interface IMameDataService
{
    IReadOnlyList<MameManager> Machines { get; }
    Dictionary<string, string> Lookup { get; }
}
