namespace SimpleLauncher.Interfaces;

public interface ISteamVdfParser
{
    Dictionary<string, object> Parse(string filePath, ILogErrors logErrors = null, IDebugLogger debugLogger = null);
}
