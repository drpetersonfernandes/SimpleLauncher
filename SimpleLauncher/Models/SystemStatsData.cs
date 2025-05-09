namespace SimpleLauncher.Models;

// Class for binding data to DataGrid
public class SystemStatsData
{
    public string SystemName { get; init; }
    public int NumberOfFiles { get; init; }
    public int NumberOfImages { get; init; }
    public long TotalDiskSize { get; init; }

    public bool AreFilesAndImagesEqual => NumberOfFiles == NumberOfImages; // This property checks if the number of files and images are equal
}