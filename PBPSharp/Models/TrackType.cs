namespace PBPSharp.Models;

/// <summary>
///     The type of a track in the disc's Table of Contents.
/// </summary>
public enum TrackType
{
    /// <summary>
    ///     Data track (MODE2/2352).
    /// </summary>
    Data = 0x41,

    /// <summary>
    ///     Audio track (AUDIO).
    /// </summary>
    Audio = 0x01
}