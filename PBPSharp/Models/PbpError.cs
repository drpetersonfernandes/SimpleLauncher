namespace PBPSharp.Models;

/// <summary>
///     Error codes returned by PBP file operations.
/// </summary>
public enum PbpError
{
    /// <summary>
    ///     No error. Operation completed successfully.
    /// </summary>
    None,

    /// <summary>
    ///     The file does not contain a valid PBP magic header.
    /// </summary>
    InvalidHeader,

    /// <summary>
    ///     The file could not be found or opened.
    /// </summary>
    FileNotFound,

    /// <summary>
    ///     An I/O error occurred while reading the file.
    /// </summary>
    IoError,

    /// <summary>
    ///     The PBP file is corrupted or has invalid internal structure.
    /// </summary>
    CorruptFile,

    /// <summary>
    ///     The PSAR section header is invalid or unrecognized.
    /// </summary>
    InvalidPsarHeader,

    /// <summary>
    ///     The specified disc index is out of range.
    /// </summary>
    DiscOutOfRange,

    /// <summary>
    ///     The specified resource type is not available in this PBP.
    /// </summary>
    ResourceNotFound,

    /// <summary>
    ///     A decompression error occurred while reading ISO data.
    /// </summary>
    DecompressionError,

    /// <summary>
    ///     The PSAR section identifies itself as a PlayStation disc image but contains no ISO index
    ///     entries. This almost always means the file is truncated or otherwise incomplete.
    /// </summary>
    TruncatedPsar,

    /// <summary>
    ///     The PARAM.SFO region inside the PBP does not carry the expected "\0PSF" signature, so the
    ///     header offsets do not point at a valid SFO.
    /// </summary>
    InvalidSfo
}