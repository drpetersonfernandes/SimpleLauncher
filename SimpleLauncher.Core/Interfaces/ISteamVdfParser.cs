namespace SimpleLauncher.Core.Interfaces;

/// <summary>
///     Parses Valve KeyValue (VDF) files into nested dictionaries of key/value pairs.
/// </summary>
public interface ISteamVdfParser
{
    /// <summary>
    ///     Parses a Valve KeyValue (VDF) file into a nested dictionary of key/value pairs.
    /// </summary>
    /// <param name="filePath">The path to the VDF file to parse.</param>
    /// <param name="logErrors">The error logger used when parsing fails.</param>
    /// <param name="logger">The fallback logger used when logErrors is not provided.</param>
    /// <returns>A dictionary representing the parsed VDF content.</returns>
    IDictionary<string, object> Parse(string filePath, ILogger? logErrors = null, ILogger? logger = null);
}