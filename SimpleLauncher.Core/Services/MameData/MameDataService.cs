using SimpleLauncher.Core.Interfaces;

namespace SimpleLauncher.Core.Services.MameData;

/// <summary>
/// Provides access to MAME machine data loaded from the mame.dat file, including a list of machines and a name-to-description lookup dictionary.
/// </summary>
public class MameDataService : IMameDataService
{
    /// <summary>
    /// Gets the list of MAME machines loaded from the data file.
    /// </summary>
    public IReadOnlyList<MameManager.MameManagerService> Machines { get; }

    /// <summary>
    /// Gets a case-insensitive dictionary mapping machine names to their descriptions.
    /// </summary>
    public IDictionary<string, string> Lookup { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MameDataService"/> class by loading MAME machine data from the data file.
    /// </summary>
    /// <param name="logErrors">The logger instance for error logging.</param>
    /// <param name="messageBox">The message box service for displaying error dialogs if loading fails.</param>
    public MameDataService(ILogger logErrors, IMessageBoxLibraryService messageBox)
    {
        var machines = MameManager.MameManagerService.LoadFromDat(logErrors, messageBox: messageBox);
        Machines = machines.ToList();

        Lookup = machines
            .GroupBy(static m => m.MachineName, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(static g => g.Key, static g => g.First().Description, StringComparer.OrdinalIgnoreCase);
    }
}
