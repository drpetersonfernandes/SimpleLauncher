namespace SimpleLauncher.Services.GlobalStats.Models;

public sealed class GlobalStatsData
{
    public int TotalSystems { get; init; }
    public int TotalEmulators { get; init; }
    public int TotalGames { get; init; }
    public int TotalImages { get; init; }
    public long TotalDiskSize { get; init; }
    public int TotalSystemsWithMissingImages { get; init; }
}