namespace SimpleLauncher.Services.MameData;

public interface IMameDataService
{
    IReadOnlyList<MameManager.MameManager> Machines { get; }
    Dictionary<string, string> Lookup { get; }
}
