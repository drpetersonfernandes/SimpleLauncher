namespace SimpleLauncher.Core.Models;

/// <summary>
/// Contains statistical data about a system's ROM files and images.
/// </summary>
public class SystemStatsData
{
    /// <summary>
    /// The name of the system.
    /// </summary>
    public string SystemName { get; init; } = null!;

    /// <summary>
    /// The number of ROM files in the system.
    /// </summary>
    public int NumberOfFiles { get; init; }

    /// <summary>
    /// The number of cover images available for the system.
    /// </summary>
    public int NumberOfImages { get; init; }

    /// <summary>
    /// The total disk size in bytes occupied by the system's files.
    /// </summary>
    public long TotalDiskSize { get; init; }

    /// <summary>
    /// Gets a value indicating whether the number of files matches the number of images.
    /// </summary>
    public bool AreFilesAndImagesEqual => NumberOfFiles == NumberOfImages;
}
