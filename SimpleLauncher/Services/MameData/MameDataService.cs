using SimpleLauncher.Interfaces;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.MameData;

public class MameDataService : IMameDataService
{
    public IReadOnlyList<MameManager.MameManager> Machines { get; }
    public Dictionary<string, string> Lookup { get; }

    public MameDataService(ILogErrors logErrors, IMessageBoxLibraryService messageBox)
    {
        var machines = MameManager.MameManager.LoadFromDat(logErrors, messageBox: messageBox);
        Machines = machines;

        Lookup = machines
            .GroupBy(static m => m.MachineName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(static g => g.Key, static g => g.First().Description, StringComparer.OrdinalIgnoreCase);
    }
}
