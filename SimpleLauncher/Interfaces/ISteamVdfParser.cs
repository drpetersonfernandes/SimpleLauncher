namespace SimpleLauncher.Interfaces;

public interface ISteamVdfParser
{
    Dictionary<string, object> Parse(string filePath, ILogErrors logErrors = null, ILogger logger = null);
}
