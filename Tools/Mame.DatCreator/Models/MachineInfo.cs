using MessagePack;

namespace Mame.DatCreator.Models;

[MessagePackObject]
public class MachineInfo
{
    [Key(0)]
    public string MachineName { get; set; } = string.Empty;

    [Key(1)]
    public string Description { get; set; } = string.Empty;
}