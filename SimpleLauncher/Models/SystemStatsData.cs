namespace SimpleLauncher.Models;

public class SystemStatsData
{
    public string SystemName { get; init; }
    public int NumberOfFiles { get; init; }
    public int NumberOfImages { get; init; }
    public long TotalDiskSize { get; init; }

    public bool AreFilesAndImagesEqual => NumberOfFiles == NumberOfImages;
}