using SimpleLauncher.Core.Services.MameManager;

namespace SimpleLauncher.Core.Interfaces;

/// <summary>
///     Provides access to MAME machine data loaded from the MAME data source.
/// </summary>
public interface IMameDataService
{
    /// <summary>
    ///     Gets the list of MAME machines available in the data source.
    /// </summary>
    IReadOnlyList<MameManagerService> Machines { get; }

    /// <summary>
    ///     Gets the lookup dictionary mapping machine names to their descriptions.
    /// </summary>
    IDictionary<string, string> Lookup { get; }
}