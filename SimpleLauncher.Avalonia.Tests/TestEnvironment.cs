using System.Text;
using Microsoft.Extensions.Configuration;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
///     Filesystem-level test helpers (portable settings.xml in the test output dir,
///     in-memory configuration).
/// </summary>
internal static class TestEnvironment
{
    private static readonly Lock Sync = new();
    private static bool _portableSettingsReady;

    /// <summary>
    ///     Ensures a settings.xml exists next to the test assembly so SettingsManagerService
    ///     resolves to portable mode and never writes to the real %LOCALAPPDATA%\SimpleLauncher.
    /// </summary>
    public static void EnsurePortableSettings()
    {
        lock (Sync)
        {
            if (_portableSettingsReady) return;
        }

        lock (Sync)
        {
            if (_portableSettingsReady) return;

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.xml");
            if (!File.Exists(path)) File.WriteAllText(path, "<Settings />");

            _portableSettingsReady = true;
        }
    }

    /// <summary>
    ///     Builds an IConfiguration from a JSON string (AddInMemoryCollection is not
    ///     referenced by this solution, so JSON streams are used instead).
    /// </summary>
    public static IConfiguration ConfigurationFromJson(string json)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        return new ConfigurationBuilder().AddJsonStream(stream).Build();
    }
}