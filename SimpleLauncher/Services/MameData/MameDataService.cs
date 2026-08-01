using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Services.MameData;

public class MameDataService : IMameDataService
{
    public IReadOnlyList<MameManager.MameManagerService> Machines { get; }
    public IDictionary<string, string> Lookup { get; }

    public MameDataService(ILogger logErrors, IMessageBoxLibraryService messageBox)
    {
        var machines = MameManager.MameManagerService.LoadFromDat(logErrors, messageBox: messageBox);
        Machines = machines.ToList();

        Lookup = machines
            .GroupBy(static m => m.MachineName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(static g => g.Key, static g => g.First().Description, StringComparer.OrdinalIgnoreCase);
    }
}
