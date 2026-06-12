using SimpleLauncher.Services.InjectEmulatorConfig;
using Xunit;

namespace SimpleLauncher.Tests;

/// <summary>
/// Tests for the <see cref="InjectionErrorHandler"/> utility class.
/// </summary>
public class InjectionErrorHandlerTests
{
    /// <summary>
    /// Verifies that GetEmulatorName returns the filename without extension for a valid path.
    /// </summary>
    [Fact]
    public void GetEmulatorNameWithPathReturnsFileNameWithoutExtension()
    {
        var result = InjectionErrorHandler.GetEmulatorName("C:\\emulators\\mame.exe", typeof(object));
        Assert.Equal("mame", result);
    }

    /// <summary>
    /// Verifies that GetEmulatorName returns the filename when no extension is present.
    /// </summary>
    [Fact]
    public void GetEmulatorNameWithPathNoExtensionReturnsFileName()
    {
        var result = InjectionErrorHandler.GetEmulatorName("C:\\emulators\\mame", typeof(object));
        Assert.Equal("mame", result);
    }

    /// <summary>
    /// Verifies that GetEmulatorName falls back to window type name when path is null.
    /// </summary>
    [Fact]
    public void GetEmulatorNameWithNullPathFallsBackToWindowType()
    {
        var result = InjectionErrorHandler.GetEmulatorName(null, typeof(InjectDuckStationConfigWindow));
        Assert.Equal("DuckStation", result);
    }

    /// <summary>
    /// Verifies that GetEmulatorName falls back to window type name when path is empty.
    /// </summary>
    [Fact]
    public void GetEmulatorNameWithEmptyPathFallsBackToWindowType()
    {
        var result = InjectionErrorHandler.GetEmulatorName("", typeof(InjectRetroArchConfigWindow));
        Assert.Equal("RetroArch", result);
    }

    /// <summary>
    /// Verifies that GetEmulatorName returns the type name when path is null and type has no known prefix.
    /// </summary>
    [Fact]
    public void GetEmulatorNameWithNullPathAndGenericTypeNameReturnsTypeName()
    {
        var result = InjectionErrorHandler.GetEmulatorName(null, typeof(object));
        Assert.Equal("Object", result);
    }

    /// <summary>
    /// Verifies that GetEmulatorName strips the Inject prefix and ConfigWindow suffix.
    /// </summary>
    [Fact]
    public void GetEmulatorNameStripsInjectPrefixAndConfigWindowSuffix()
    {
        var result = InjectionErrorHandler.GetEmulatorName(null, typeof(InjectMameConfigWindow));
        Assert.Equal("Mame", result);
    }

    /// <summary>
    /// Verifies that GetEmulatorName handles paths containing spaces.
    /// </summary>
    [Fact]
    public void GetEmulatorNameWithPathContainingSpacesReturnsFileName()
    {
        var result = InjectionErrorHandler.GetEmulatorName("C:\\My Emulators\\Retro Arch.exe", typeof(object));
        Assert.Equal("Retro Arch", result);
    }

    // Dummy types to simulate injection config windows
    private class InjectDuckStationConfigWindow;

    private class InjectRetroArchConfigWindow;

    private class InjectMameConfigWindow;
}
