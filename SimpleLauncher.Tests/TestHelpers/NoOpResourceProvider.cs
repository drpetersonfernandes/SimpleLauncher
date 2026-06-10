using SimpleLauncher.Interfaces;

namespace SimpleLauncher.Tests.TestHelpers;

public class NoOpResourceProvider : IResourceProvider
{
    public string GetString(string key)
    {
        return key;
    }

    public string GetString(string key, string defaultValue)
    {
        return defaultValue;
    }
}
