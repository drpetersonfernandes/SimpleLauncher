namespace SimpleLauncher.Interfaces;

public interface ISteamVdfParser
{
    Dictionary<string, object> Parse(string filePath, ILogger? logErrors = null, ILogger? logger = null);
}
