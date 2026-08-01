using SimpleLauncher.Services.MameManager;

namespace SimpleLauncher.Interfaces;

public interface IMameDataService
{
    IReadOnlyList<MameManagerService> Machines { get; }
    IDictionary<string, string> Lookup { get; }
}
