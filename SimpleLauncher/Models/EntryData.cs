using MessagePack;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents a single ROM history entry containing software, systems, and descriptive text.
/// </summary>
[MessagePackObject]
public class EntryData
{
    /// <summary>
    /// Gets or sets the software metadata for this ROM history entry.
    /// </summary>
    [Key(0)]
    public SoftwareData? Software { get; set; }

    /// <summary>
    /// Gets or sets the systems metadata for this ROM history entry.
    /// </summary>
    [Key(1)]
    public SystemsData? Systems { get; set; }

    /// <summary>
    /// Gets or sets the descriptive text associated with this entry.
    /// </summary>
    [Key(2)]
    public string? Text { get; set; }
}
