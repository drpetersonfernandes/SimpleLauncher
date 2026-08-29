namespace PBPSharp.Models;

/// <summary>
///     Represents a single entry in the disc's Table of Contents (TOC).
/// </summary>
public sealed class TocEntry
{
    /// <summary>
    ///     The track type (Data or Audio).
    /// </summary>
    public TrackType TrackType { get; internal set; }

    /// <summary>
    ///     The track number (1-based).
    /// </summary>
    public int TrackNo { get; internal set; }

    /// <summary>
    ///     Minutes component of the track's MSF (Minutes:Seconds:Frames) address.
    /// </summary>
    public int Minutes { get; internal set; }

    /// <summary>
    ///     Seconds component of the track's MSF address.
    /// </summary>
    public int Seconds { get; internal set; }

    /// <summary>
    ///     Frames component of the track's MSF address.
    /// </summary>
    public int Frames { get; internal set; }
}