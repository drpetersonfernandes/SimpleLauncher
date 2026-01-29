using MessagePack;

namespace SimpleLauncher.SharedModels;

[MessagePackObject]
public class SystemPlayTime
{
    [Key(0)]
    public string SystemName { get; init; }

    [Key(1)]
    public string PlayTime { get; set; }
}
