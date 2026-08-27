namespace PBPSharp.Models;

/// <summary>
/// Represents a single entry in the SFO (PARAM.SFO) metadata.
/// </summary>
public sealed class SfoEntry
{
    /// <summary>
    /// The key name (e.g., "TITLE", "DISC_ID", "CATEGORY").
    /// </summary>
    public string Key { get; internal set; } = string.Empty;

    /// <summary>
    /// The data format (0x0204 = UTF-8 string, 0x0404 = uint32).
    /// </summary>
    public ushort Format { get; internal set; }

    /// <summary>
    /// The length of the data.
    /// </summary>
    public uint Length { get; internal set; }

    /// <summary>
    /// The maximum length of the data.
    /// </summary>
    public uint MaxLength { get; internal set; }

    /// <summary>
    /// The value (string for UTF-8 entries, uint for integer entries).
    /// </summary>
    public object? Value { get; internal set; }
}