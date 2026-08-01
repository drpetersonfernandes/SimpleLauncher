namespace SimpleLauncher.Interfaces;

public interface ISteamVdfParser
{
    IDictionary<string, object> Parse(string filePath, ILogger? logErrors = null, ILogger? logger = null);
}
