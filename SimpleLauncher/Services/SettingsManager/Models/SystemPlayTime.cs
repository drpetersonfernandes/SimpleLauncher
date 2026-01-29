using MessagePack;

namespace SimpleLauncher.Services.SettingsManager.Models;

[MessagePackObject]
public class SystemPlayTime
{
    [Key(0)]
    public string SystemName { get; init; }

    [Key(1)]
    public string PlayTime { get; set; }
}
