using MessagePack;

namespace Mame.DatCreator.Models;

/// <summary>
///     Represents a MAME machine entry with name and description.
/// </summary>
[MessagePackObject]
public class MachineInfo
{
    /// <summary>
    ///     Gets or sets the machine name identifier.
    /// </summary>
    [Key(0)]
    public string MachineName { get; set; } = "";

    /// <summary>
    ///     Gets or sets the machine description.
    /// </summary>
    [Key(1)]
    public string Description { get; set; } = "";
}