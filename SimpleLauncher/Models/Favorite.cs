#nullable enable
using MessagePack;
using SimpleLauncher.Services;

namespace SimpleLauncher.Models;

// New MessagePack format
[MessagePackObject]
public class Favorite
{
    [Key(0)]
    public required string FileName { get; init; }

    [Key(1)]
    public required string SystemName { get; init; }

    [IgnoreMember]
    public string? MachineDescription { get; init; }

    [IgnoreMember]
    public string? CoverImage { get; init; }

    [IgnoreMember]
    public string? DefaultEmulator { get; set; }

    [IgnoreMember]
    public long FileSizeBytes { get; set; }

    // Add property to format file size using the helper (ignored for serialization)
    [IgnoreMember]
    public string FormattedFileSize => FormatFileSize.Format(FileSizeBytes);
}