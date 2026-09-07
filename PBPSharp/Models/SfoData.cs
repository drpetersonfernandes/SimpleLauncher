namespace PBPSharp.Models;

/// <summary>
///     Represents the SFO (System File Object) metadata header.
/// </summary>
public sealed class SfoData
{
    /// <summary>
    ///     The SFO magic identifier (0x46535000).
    /// </summary>
    public uint Magic { get; internal set; }

    /// <summary>
    ///     The SFO format version.
    /// </summary>
    public uint Version { get; internal set; }

    /// <summary>
    ///     Offset to the key table within the SFO.
    /// </summary>
    public uint KeyTableOffset { get; internal set; }

    /// <summary>
    ///     Offset to the data table within the SFO.
    /// </summary>
    public uint DataTableOffset { get; internal set; }

    /// <summary>
    ///     The parsed SFO entries (key-value pairs).
    /// </summary>
    public IReadOnlyList<SfoEntry> Entries { get; internal set; } = [];

    /// <summary>
    ///     Total size of the SFO data in bytes.
    /// </summary>
    public uint Size { get; internal set; }

    /// <summary>
    ///     Gets the value for the specified key, or null if not found.
    /// </summary>
    public string? GetString(string key)
    {
        return Entries
            .FirstOrDefault(e => string.Equals(e.Key, key, StringComparison.Ordinal))
            ?.Value as string;
    }

    /// <summary>
    ///     Gets the uint value for the specified key, or null if not found.
    /// </summary>
    public uint? GetUInt32(string key)
    {
        return Entries
            .FirstOrDefault(e => string.Equals(e.Key, key, StringComparison.Ordinal))
            ?.Value as uint?;
    }

    /// <summary>
    ///     Well-known SFO keys for PlayStation PBP files.
    /// </summary>
    public static class Keys
    {
        /// <summary>Indicates whether the disc is bootable.</summary>
        public const string Bootable = "BOOTABLE";

        /// <summary>The game category code (e.g., "ME" for PS1 EBOOT).</summary>
        public const string Category = "CATEGORY";

        /// <summary>The disc identifier (e.g., "SCUS94163").</summary>
        public const string DiscId = "DISC_ID";

        /// <summary>The disc version string.</summary>
        public const string DiscVersion = "DISC_VERSION";

        /// <summary>The license identifier.</summary>
        public const string License = "LICENSE";

        /// <summary>The parental restriction level.</summary>
        public const string ParentalLevel = "PARENTAL_LEVEL";

        /// <summary>The minimum PSP system software version required.</summary>
        public const string PspSystemVer = "PSP_SYSTEM_VER";

        /// <summary>The region code of the disc.</summary>
        public const string Region = "REGION";

        /// <summary>The display title of the game.</summary>
        public const string Title = "TITLE";
    }
}