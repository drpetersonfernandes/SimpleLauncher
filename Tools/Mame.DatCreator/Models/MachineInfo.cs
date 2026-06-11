using MessagePack;

namespace Mame.DatCreator.Models;

[MessagePackObject]
public class MachineInfo
{
    [Key(0)]
    public string MachineName { get; set; } = "";

    [Key(1)]
    public string Description { get; set; } = "";
}