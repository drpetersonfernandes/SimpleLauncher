namespace PBPSharp;

/// <summary>
///     Thrown when a PSAR section that identifies itself as a PlayStation disc image
///     ("PSISOIMG0000" or "PSTITLEIMG000000") carries no ISO index entries. This is distinct from
///     generic corruption: the container header parsed correctly, so the file most likely ends
///     before its data area - typically a truncated or incomplete download. Callers can catch this
///     type to report that specific cause instead of a generic corrupt-file error.
/// </summary>
public sealed class NoIsoIndexException : Exception
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="NoIsoIndexException" /> class.
    /// </summary>
    public NoIsoIndexException()
        : base("No ISO index was found.")
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NoIsoIndexException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NoIsoIndexException(string message)
        : base(message)
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="NoIsoIndexException" /> class.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public NoIsoIndexException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}