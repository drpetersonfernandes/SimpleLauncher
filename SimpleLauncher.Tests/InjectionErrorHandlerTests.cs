using SimpleLauncher.Services.InjectEmulatorConfig;
using Xunit;

namespace SimpleLauncher.Tests;

public class InjectionErrorHandlerTests
{
    [Fact]
    public void GetEmulatorNameWithPathReturnsFileNameWithoutExtension()
    {
        var result = InjectionErrorHandler.GetEmulatorName("C:\\emulators\\mame.exe", typeof(object));
        Assert.Equal("mame", result);
    }

    [Fact]
    public void GetEmulatorNameWithPathNoExtensionReturnsFileName()
    {
        var result = InjectionErrorHandler.GetEmulatorName("C:\\emulators\\mame", typeof(object));
        Assert.Equal("mame", result);
    }

    [Fact]
    public void GetEmulatorNameWithNullPathFallsBackToWindowType()
    {
        var result = InjectionErrorHandler.GetEmulatorName(null, typeof(InjectDuckStationConfigWindow));
        Assert.Equal("DuckStation", result);
    }

    [Fact]
    public void GetEmulatorNameWithEmptyPathFallsBackToWindowType()
    {
        var result = InjectionErrorHandler.GetEmulatorName("", typeof(InjectRetroArchConfigWindow));
        Assert.Equal("RetroArch", result);
    }

    [Fact]
    public void GetEmulatorNameWithNullPathAndGenericTypeNameReturnsTypeName()
    {
        var result = InjectionErrorHandler.GetEmulatorName(null, typeof(object));
        Assert.Equal("Object", result);
    }

    [Fact]
    public void GetEmulatorNameStripsInjectPrefixAndConfigWindowSuffix()
    {
        var result = InjectionErrorHandler.GetEmulatorName(null, typeof(InjectMameConfigWindow));
        Assert.Equal("Mame", result);
    }

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
