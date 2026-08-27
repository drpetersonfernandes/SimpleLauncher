using SimpleLauncher.Avalonia.Services.GameLauncher.Handlers;

namespace SimpleLauncher.Avalonia.Tests;

/// <summary>
/// Verifies IsMatch name/path detection for all 21 emulator config handlers
/// (patterns taken verbatim from each handler's IsMatch implementation).
/// </summary>
public class ConfigHandlerIsMatchTests
{
    public static TheoryData<Type, string, string, string> HandlerCases => new()
    {
        { typeof(AresConfigHandler), "Ares", @"C:\Emus\ares.exe", "Nope" },
        { typeof(AzaharConfigHandler), "Azahar", @"C:\Emus\azahar.exe", "Nope" },
        { typeof(BlastemConfigHandler), "Blastem", @"C:\Emus\blastem.exe", "Nope" },
        { typeof(CemuConfigHandler), "Cemu 2.0", @"C:\Emus\Cemu.exe", "Nope" },
        { typeof(DaphneConfigHandler), "Daphne", @"C:\Emus\daphne.exe", "Nope" },
        { typeof(DolphinConfigHandler), "Dolphin (GameCube/Wii)", @"C:\Emus\Dolphin.exe", "Nope" },
        { typeof(DuckStationConfigHandler), "DuckStation", @"C:\Emus\duckstation-qt.exe", "Nope" },
        { typeof(FlycastConfigHandler), "Flycast", @"C:\Emus\flycast.exe", "Nope" },
        { typeof(MameConfigHandler), "MAME", @"C:\Emus\mame64.exe", "Nope" },
        { typeof(MednafenConfigHandler), "Mednafen", @"C:\Emus\mednafen.exe", "Nope" },
        { typeof(MesenConfigHandler), "Mesen", @"C:\Emus\Mesen.exe", "Nope" },
        { typeof(Pcsx2ConfigHandler), "PCSX2", @"C:\Emus\pcsx2-qt.exe", "Nope" },
        { typeof(RaineConfigHandler), "Raine", @"C:\Emus\raine64.exe", "Nope" },
        { typeof(RedreamConfigHandler), "Redream", @"C:\Emus\redream.exe", "Nope" },
        { typeof(RetroArchConfigHandler), "RetroArch", @"C:\Emus\retroarch.exe", "Nope" },
        { typeof(Rpcs3ConfigHandler), "RPCS3", @"C:\Emus\rpcs3.exe", "Nope" },
        { typeof(SegaModel2ConfigHandler), "SEGA Model 2", @"C:\Emus\emulator.exe", "Nope" },
        { typeof(StellaConfigHandler), "Stella", @"C:\Emus\stella.exe", "Nope" },
        { typeof(SupermodelConfigHandler), "Supermodel", @"C:\Emus\Supermodel.exe", "Nope" },
        { typeof(XeniaConfigHandler), "Xenia", @"C:\Emus\xenia_canary.exe", "Nope" },
        { typeof(YumirConfigHandler), "Yumir", @"C:\Emus\ymir.exe", "Nope" }
    };

    [Theory]
    [MemberData(nameof(HandlerCases))]
    public void IsMatch_MatchesEmulatorName(Type handlerType, string emulatorName, string _, string __)
    {
        var handler = HandlerFactory.CreateFromType(handlerType);

        Assert.True(handler.IsMatch(emulatorName, ""), $"{handlerType.Name} should match name '{emulatorName}'");
    }

    [Theory]
    [MemberData(nameof(HandlerCases))]
    public void IsMatch_MatchesEmulatorPath(Type handlerType, string _, string emulatorPath, string __)
    {
        var handler = HandlerFactory.CreateFromType(handlerType);

        Assert.True(handler.IsMatch("Some Emulator", emulatorPath),
            $"{handlerType.Name} should match path '{emulatorPath}'");
    }

    [Theory]
    [MemberData(nameof(HandlerCases))]
    public void IsMatch_DoesNotMatchUnrelated(Type handlerType, string _, string __, string unrelated)
    {
        var handler = HandlerFactory.CreateFromType(handlerType);

        Assert.False(handler.IsMatch(unrelated, "/usr/bin/some-other-emulator"),
            $"{handlerType.Name} should NOT match '{unrelated}'");
    }

    [Theory]
    [MemberData(nameof(HandlerCases))]
    public void IsMatch_IsCaseInsensitive(Type handlerType, string emulatorName, string emulatorPath, string _)
    {
        var handler = HandlerFactory.CreateFromType(handlerType);

        Assert.True(handler.IsMatch(emulatorName.ToUpperInvariant(), ""));
        Assert.True(handler.IsMatch(emulatorName.ToLowerInvariant(), ""));
        Assert.True(handler.IsMatch("", emulatorPath.ToLowerInvariant()));
    }
}