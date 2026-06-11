using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.Services.InjectEmulatorConfig;
using SimpleLauncher.Services.SettingsManager;
using SimpleLauncher.Tests.TestHelpers;
using Xunit;

namespace SimpleLauncher.Tests;

public class StellaConfigInjectionTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly IConfiguration _configuration;
    private readonly ILogErrors _logErrors = new NoOpLogErrors();
    private readonly NoOpCredentialProtector _credentialProtector = new();

    public StellaConfigInjectionTests()
    {
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Urls:YouTubeSearch"] = "https://www.youtube.com/results?search_query=",
                ["Urls:IgdbSearch"] = "https://www.igdb.com/search?q="
            })
            .Build();

        ServiceProviderMock.Install();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"SL_StellaTest_{Guid.NewGuid():N}");
        Directory.CreateDirectory(_testDirectory);
    }

    public void Dispose()
    {
        try
        {
            if (Directory.Exists(_testDirectory))
                Directory.Delete(_testDirectory, true);
        }
        catch
        {
            // Best-effort cleanup
        }

        ServiceProviderMock.Restore();
        GC.SuppressFinalize(this);
    }

    private void CopySampleToEmuDir(string emulatorDirName, string sampleSubDir, string configFileName)
    {
        var emuDir = Path.Combine(_testDirectory, emulatorDirName);
        Directory.CreateDirectory(emuDir);

        var samplePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "samples", sampleSubDir, configFileName);
        var destPath = Path.Combine(emuDir, configFileName);
        File.Copy(samplePath, destPath);
    }

    private static string FakeEmulatorExePath(string emuDir)
    {
        return Path.Combine(emuDir, "stella.exe");
    }

    private SettingsManager CreateSettingsManager()
    {
        return new SettingsManager(_configuration, _logErrors, _credentialProtector);
    }

    private static Dictionary<string, string> ReadSqliteSettings(string dbPath)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var connectionString = new SqliteConnectionStringBuilder { DataSource = dbPath }.ToString();
        using var connection = new SqliteConnection(connectionString);
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT setting, value FROM settings";
        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            result[reader.GetString(0)] = reader.GetString(1);
        }

        return result;
    }

    [Fact]
    public void StellaInjectsSettingsCorrectly()
    {
        CopySampleToEmuDir("Stella", "Stella", "stella.sqlite3");

        var settings = CreateSettingsManager();
        settings.Stella.Fullscreen = true;
        settings.Stella.Vsync = false;
        settings.Stella.VideoDriver = "opengl";
        settings.Stella.CorrectAspect = true;
        settings.Stella.TvFilter = 3;
        settings.Stella.Scanlines = 50;
        settings.Stella.AudioEnabled = false;
        settings.Stella.AudioVolume = 75;
        settings.Stella.TimeMachine = true;
        settings.Stella.ConfirmExit = false;

        var emuDir = Path.Combine(_testDirectory, "Stella");
        StellaConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors, new NoOpDebugLogger());

        var configPath = Path.Combine(emuDir, "stella.sqlite3");
        Assert.True(File.Exists(configPath));

        var values = ReadSqliteSettings(configPath);

        Assert.Equal("1", values["fullscreen"]);
        Assert.Equal("false", values["vsync"]);
        Assert.Equal("opengl", values["video"]);
        Assert.Equal("true", values["tia.correct_aspect"]);
        Assert.Equal("3", values["tv.filter"]);
        Assert.Equal("50", values["tv.scanlines"]);
        Assert.Equal("0", values["audio.enabled"]);
        Assert.Equal("75", values["audio.volume"]);
        Assert.Equal("1", values["dev.timemachine"]);
        Assert.Equal("0", values["confirmexit"]);
    }

    [Fact]
    public void StellaDisabledOptionsUsesZeroValues()
    {
        CopySampleToEmuDir("Stella", "Stella", "stella.sqlite3");

        var settings = CreateSettingsManager();
        settings.Stella.Fullscreen = false;
        settings.Stella.Vsync = true;
        settings.Stella.CorrectAspect = false;
        settings.Stella.AudioEnabled = true;
        settings.Stella.TimeMachine = false;
        settings.Stella.ConfirmExit = true;

        var emuDir = Path.Combine(_testDirectory, "Stella");
        StellaConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors, new NoOpDebugLogger());

        var configPath = Path.Combine(emuDir, "stella.sqlite3");
        var values = ReadSqliteSettings(configPath);

        Assert.Equal("0", values["fullscreen"]);
        Assert.Equal("true", values["vsync"]);
        Assert.Equal("false", values["tia.correct_aspect"]);
        Assert.Equal("1", values["audio.enabled"]);
        Assert.Equal("0", values["dev.timemachine"]);
        Assert.Equal("1", values["confirmexit"]);
    }

    [Fact]
    public void StellaCreatesConfigFromSampleIfMissing()
    {
        var emuDir = Path.Combine(_testDirectory, "Stella");
        Directory.CreateDirectory(emuDir);

        var settings = CreateSettingsManager();
        settings.Stella.Fullscreen = true;
        settings.Stella.VideoDriver = "sdl";
        settings.Stella.AudioVolume = 50;

        StellaConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors, new NoOpDebugLogger());

        var configPath = Path.Combine(emuDir, "stella.sqlite3");
        Assert.True(File.Exists(configPath));

        var values = ReadSqliteSettings(configPath);
        Assert.Equal("1", values["fullscreen"]);
        Assert.Equal("sdl", values["video"]);
        Assert.Equal("50", values["audio.volume"]);
    }

    [Fact]
    public void StellaUpsertOverwritesExistingValues()
    {
        CopySampleToEmuDir("Stella", "Stella", "stella.sqlite3");

        var settings1 = CreateSettingsManager();
        settings1.Stella.Fullscreen = false;
        settings1.Stella.AudioVolume = 25;

        var emuDir = Path.Combine(_testDirectory, "Stella");
        StellaConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings1, _logErrors, new NoOpDebugLogger());

        var configPath = Path.Combine(emuDir, "stella.sqlite3");
        var values1 = ReadSqliteSettings(configPath);
        Assert.Equal("0", values1["fullscreen"]);
        Assert.Equal("25", values1["audio.volume"]);

        // Inject again with different values
        var settings2 = CreateSettingsManager();
        settings2.Stella.Fullscreen = true;
        settings2.Stella.AudioVolume = 100;

        StellaConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings2, _logErrors, new NoOpDebugLogger());

        var values2 = ReadSqliteSettings(configPath);
        Assert.Equal("1", values2["fullscreen"]);
        Assert.Equal("100", values2["audio.volume"]);
    }

    [Fact]
    public void StellaBooleanEncodingIsInconsistent()
    {
        CopySampleToEmuDir("Stella", "Stella", "stella.sqlite3");

        var settings = CreateSettingsManager();
        settings.Stella.Fullscreen = true;
        settings.Stella.Vsync = true;
        settings.Stella.CorrectAspect = true;
        settings.Stella.AudioEnabled = true;
        settings.Stella.TimeMachine = true;
        settings.Stella.ConfirmExit = true;

        var emuDir = Path.Combine(_testDirectory, "Stella");
        StellaConfigurationService.InjectSettings(FakeEmulatorExePath(emuDir), settings, _logErrors, new NoOpDebugLogger());

        var configPath = Path.Combine(emuDir, "stella.sqlite3");
        var values = ReadSqliteSettings(configPath);

        // fullscreen, audio.enabled, dev.timemachine, confirmexit use "1"/"0"
        Assert.Equal("1", values["fullscreen"]);
        Assert.Equal("1", values["audio.enabled"]);
        Assert.Equal("1", values["dev.timemachine"]);
        Assert.Equal("1", values["confirmexit"]);

        // vsync, tia.correct_aspect use "true"/"false"
        Assert.Equal("true", values["vsync"]);
        Assert.Equal("true", values["tia.correct_aspect"]);
    }

    private sealed class NoOpLogErrors : ILogErrors
    {
        public Task LogErrorAsync(Exception? ex, string? contextMessage = null)
        {
            return Task.CompletedTask;
        }
    }
}
