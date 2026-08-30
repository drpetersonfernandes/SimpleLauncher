using Microsoft.Extensions.Configuration;
using SimpleLauncher.Avalonia.Services.SystemImageResolver;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.SettingsManager;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
///     Tests for the Avalonia SystemImageResolverService: exact file-name match,
///     multi-extension support, annotation-stripped match, and Jaro-Winkler fuzzy
///     matching with a configurable similarity threshold.
/// </summary>
public class SystemImageResolverServiceTests : IDisposable
{
    private readonly string _defaultImage;
    private readonly SystemImageResolverService _service;
    private readonly SettingsManagerService _settings;
    private readonly string _systemsFolder;
    private readonly string _tempRoot = Path.Combine(Path.GetTempPath(), $"SL_ImageResolver_{Guid.NewGuid():N}");

    public SystemImageResolverServiceTests()
    {
        _systemsFolder = Path.Combine(_tempRoot, "images", "systems");
        Directory.CreateDirectory(_systemsFolder);

        // Global default in images/ (one level above systems/)
        var imagesFolder = Path.Combine(_tempRoot, "images");
        _defaultImage = Path.Combine(imagesFolder, "default.png");
        File.WriteAllText(_defaultImage, "global-default");

        // Build a configuration pointing at the temp images folder.
        // The service reads ImageExtensions from config; let it use the default [".png", ".jpg", ".jpeg"].
        var config = new ConfigurationBuilder().Build();
        _settings = TestDependencies.Settings(config);

        // Enable fuzzy + annotation stripping by default (test them)
        _settings.EnableFuzzyMatching = true;
        _settings.FuzzyMatchingThreshold = 0.85;
        _settings.EnableAnnotationStripping = true;

        // The service resolves images relative to AppDomain.CurrentDomain.BaseDirectory.
        // We cannot change that at runtime, so instead we create the expected directory
        // structure under the test output directory.  When running under dotnet test the
        // BaseDirectory IS the test output dir, so this works for local runs.
        var baseDir = AppContext.BaseDirectory;
        var baseSystems = Path.Combine(baseDir, "images", "systems");
        if (!Directory.Exists(baseSystems))
            Directory.CreateDirectory(baseSystems);

        // Plant a known test image so exact-match tests always have something to find.
        PlantFile(baseSystems, "NES.png", "nes-icon");
        PlantFile(baseSystems, "Sega Genesis.jpg", "genesis-icon");

        _service = new SystemImageResolverService(config, _settings);
    }

    public void Dispose()
    {
        try
        {
            // Only clean up the temp directory; leave BaseDirectory test files alone
            // (they are gitignored and harmless).
            if (Directory.Exists(_tempRoot))
                Directory.Delete(_tempRoot, true);
        }
        catch
        {
            // best effort
        }

        GC.SuppressFinalize(this);
    }

    private static void PlantFile(string dir, string name, string content)
    {
        var path = Path.Combine(dir, name);
        if (!File.Exists(path))
            File.WriteAllText(path, content);
    }

    // ── Exact match ──────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveSystemIcon_ExactPngMatch_ReturnsPath()
    {
        var config = SystemConfig("NES");
        var result = await _service.ResolveDisplayImageAsync(config);

        Assert.EndsWith("NES.png", result, StringComparison.OrdinalIgnoreCase);
        Assert.True(File.Exists(result));
    }

    [Fact]
    public async Task ResolveSystemIcon_ExactJpgMatch_ReturnsPath()
    {
        // Use a unique name so the .jpg is the only match (no .png counterpart)
        PlantFile(Path.Combine(AppContext.BaseDirectory, "images", "systems"), "UniqueJpgOnly_System.jpg",
            "unique-jpg-icon");

        var config = SystemConfig("UniqueJpgOnly_System");
        var result = await _service.ResolveDisplayImageAsync(config);

        Assert.EndsWith("UniqueJpgOnly_System.jpg", result, StringComparison.OrdinalIgnoreCase);
        Assert.True(File.Exists(result));
    }

    [Fact]
    public async Task ResolveSystemIcon_NoMatch_ReturnsDefaultPath()
    {
        var config = SystemConfig("Nonexistent System 12345");
        var result = await _service.ResolveDisplayImageAsync(config);

        Assert.EndsWith("default.png", result, StringComparison.OrdinalIgnoreCase);
    }

    // ── Annotation-stripped match ─────────────────────────────────────────

    [Fact]
    public async Task ResolveSystemIcon_AnnotationStrippedExact_ReturnsMatch()
    {
        // Seed: "Atari 2600.png" exists; query: "Atari 2600 (US)"
        var baseDir = AppContext.BaseDirectory;
        PlantFile(Path.Combine(baseDir, "images", "systems"), "Atari 2600.png", "atari-icon");

        var config = SystemConfig("Atari 2600 (US)");
        var result = await _service.ResolveDisplayImageAsync(config);

        Assert.EndsWith("Atari 2600.png", result, StringComparison.OrdinalIgnoreCase);
    }

    // ── ResolveSystemIconAsync (sidebar path — returns null on no match) ─

    [Fact]
    public async Task ResolveSystemIcon_NoMatch_ReturnsNull()
    {
        var result = await _service.ResolveSystemIconAsync("Nonexistent System 12345");

        Assert.Null(result);
    }

    [Fact]
    public async Task ResolveSystemIcon_ExactMatch_ReturnsPath()
    {
        var result = await _service.ResolveSystemIconAsync("NES");

        Assert.NotNull(result);
        Assert.EndsWith("NES.png", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ResolveSystemIcon_NullOrEmpty_ReturnsNull()
    {
        Assert.Null(await _service.ResolveSystemIconAsync(null!));
        Assert.Null(await _service.ResolveSystemIconAsync(""));
        Assert.Null(await _service.ResolveSystemIconAsync("   "));
    }

    // ── Fuzzy matching ────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveSystemIcon_FuzzyMatch_SimilarName_ReturnsMatch()
    {
        // Seed: "Nintendo Entertainment System.png"; query: "Nintendo Entertainmnt System" (typo)
        var baseDir = AppContext.BaseDirectory;
        PlantFile(Path.Combine(baseDir, "images", "systems"), "Nintendo Entertainment System.png", "nes-fuzzy");

        var result = await _service.ResolveSystemIconAsync("Nintendo Entertainment Systm");

        Assert.NotNull(result);
        Assert.Contains("Nintendo Entertainment System", result, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ResolveSystemIcon_FuzzyMatch_BelowThreshold_ReturnsNull()
    {
        _settings.FuzzyMatchingThreshold = 0.99; // effectively impossible to match

        var result = await _service.ResolveSystemIconAsync("something completely different from NES");

        Assert.Null(result);
    }

    // ── Fuzzy disabled ────────────────────────────────────────────────────

    [Fact]
    public async Task ResolveSystemIcon_FuzzyDisabled_NoMatch_ReturnsNull()
    {
        _settings.EnableFuzzyMatching = false;

        var result = await _service.ResolveSystemIconAsync("Nintendo Entertainmnt System");

        Assert.Null(result);
    }

    // ── Helpers ──────────────────────────────────────────────────────────

    private static SystemManagerConfig SystemConfig(string name)
    {
        return new SystemManagerConfig
        {
            SystemName = name,
            SystemFolders = [],
            SystemImageFolder = "",
            FileFormatsToSearch = [],
            FileFormatsToLaunch = []
        };
    }
}