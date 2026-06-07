namespace SimpleLauncher.Core.Interfaces;

public interface IResourceProvider
{
    string GetString(string key);
    string GetString(string key, string defaultValue);
}
