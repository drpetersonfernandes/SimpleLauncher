using MessagePack;

namespace SimpleLauncher.Models;

// New MessagePack format
[MessagePackObject]
public class Favorite
{
    [Key(0)]
    public string FileName { get; init; }

    [Key(1)]
    public string SystemName { get; init; }

    [IgnoreMember]
    public string MachineDescription { get; init; }

    [IgnoreMember]
    public string CoverImage { get; init; }

    [IgnoreMember]
    public string DefaultEmulator { get; set; }
}