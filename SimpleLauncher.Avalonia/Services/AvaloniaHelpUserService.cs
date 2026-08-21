using SimpleLauncher.Core.Interfaces;
using SimpleLauncher.Core.Services.HelpUser;

namespace SimpleLauncher.Avalonia.Services;

/// <summary>
/// Provides emulator parameter help text for systems, sourced from parameters.md
/// (loaded by the Core <see cref="HelpUserManager"/>). Avalonia port of the WPF
/// <c>HelpUserService</c> — the markdown is rendered by Markdown.Avalonia viewers
/// (no RichTextBox dependency).
/// </summary>
public class AvaloniaHelpUserService
{
    private readonly HelpUserManager _manager;
    private readonly ILogger _logger;
    private readonly LocalizationService? _localization;

    /// <summary>
    /// Maps system-name aliases (user-typed names) to the canonical names used by parameters.md.
    /// Same alias set as the WPF HelpUserService.
    /// </summary>
    private static readonly Dictionary<string, string> AliasToCanonicalName = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Amstrad CPC", "Amstrad CPC" },
        { "CPC", "Amstrad CPC" },
        { "Amstrad CPC GX4000", "Amstrad GX4000" },
        { "Amstrad GX4000", "Amstrad GX4000" },
        { "CPC GX4000", "Amstrad GX4000" },
        { "GX4000", "Amstrad GX4000" },
        { "Arcade", "Arcade" },
        { "Mame", "Arcade" },
        { "Raine", "Arcade" },
        { "Atari 2600", "Atari 2600" },
        { "Atari2600", "Atari 2600" },
        { "Atari 5200", "Atari 5200" },
        { "Atari5200", "Atari 5200" },
        { "Atari 7800", "Atari 7800" },
        { "Atari7800", "Atari 7800" },
        { "Atari 8-Bit", "Atari 8-Bit" },
        { "Atari 8-Bits", "Atari 8-Bit" },
        { "Atari 8 bits", "Atari 8-Bit" },
        { "Atari 8bits", "Atari 8-Bit" },
        { "Atari 800", "Atari 8-Bit" },
        { "Atari Jaguar", "Atari Jaguar" },
        { "Jaguar", "Atari Jaguar" },
        { "Atari Jaguar CD", "Atari Jaguar CD" },
        { "Jaguar CD", "Atari Jaguar CD" },
        { "Atari Lynx", "Atari Lynx" },
        { "Lynx", "Atari Lynx" },
        { "Atari ST", "Atari ST" },
        { "AtariST", "Atari ST" },
        { "Atomiswave", "Atomiswave" },
        { "Bandai WonderSwan", "Bandai WonderSwan" },
        { "Bandai Wonder Swan", "Bandai WonderSwan" },
        { "WonderSwan", "Bandai WonderSwan" },
        { "Wonder Swan", "Bandai WonderSwan" },
        { "Bandai WonderSwan Color", "Bandai WonderSwan Color" },
        { "Bandai Wonder Swan Color", "Bandai WonderSwan Color" },
        { "WonderSwan Color", "Bandai WonderSwan Color" },
        { "Wonder Swan Color", "Bandai WonderSwan Color" },
        { "Casio PV-1000", "Casio PV-1000" },
        { "Casio PV1000", "Casio PV-1000" },
        { "Casio PV 1000", "Casio PV-1000" },
        { "PV-1000", "Casio PV-1000" },
        { "PV1000", "Casio PV-1000" },
        { "PV 1000", "Casio PV-1000" },
        { "Colecovision", "Colecovision" },
        { "Commander Genius", "Commander Genius" },
        { "Comander Genius", "Commander Genius" },
        { "Commodore 64", "Commodore 64" },
        { "Commodore64", "Commodore 64" },
        { "Commodore 128", "Commodore 128" },
        { "Commodore128", "Commodore 128" },
        { "Amiga", "Commodore Amiga" },
        { "Commodore Amiga", "Commodore Amiga" },
        { "Commodore Amiga CD32", "Commodore Amiga CD32" },
        { "Commodore Amiga CD", "Commodore Amiga CD32" },
        { "Amiga CD", "Commodore Amiga CD32" },
        { "Amiga CD32", "Commodore Amiga CD32" },
        { "AmigaCD", "Commodore Amiga CD32" },
        { "AmigaCD32", "Commodore Amiga CD32" },
        { "FMTowns", "FM Towns" },
        { "FM Towns", "FM Towns" },
        { "LaserDisk", "LaserDisk" },
        { "Laser Disk", "LaserDisk" },
        { "Daphne", "LaserDisk" },
        { "Magnavox Odyssey 2", "Magnavox Odyssey 2" },
        { "Odyssey", "Magnavox Odyssey 2" },
        { "Mattel Aquarius", "Mattel Aquarius" },
        { "Aquarius", "Mattel Aquarius" },
        { "Mattel Intellivision", "Mattel Intellivision" },
        { "Intellivision", "Mattel Intellivision" },
        { "Microsoft DOS", "Microsoft DOS" },
        { "DOS", "Microsoft DOS" },
        { "Microsoft MSX", "Microsoft MSX" },
        { "MSX", "Microsoft MSX" },
        { "MSX1", "Microsoft MSX" },
        { "Microsoft MSX2", "Microsoft MSX2" },
        { "MSX2", "Microsoft MSX2" },
        { "MSX 2", "Microsoft MSX2" },
        { "Microsoft Windows", "Microsoft Windows" },
        { "Windows", "Microsoft Windows" },
        { "PC", "Microsoft Windows" },
        { "Microsoft Xbox", "Microsoft Xbox" },
        { "Xbox", "Microsoft Xbox" },
        { "Xbox Original", "Microsoft Xbox" },
        { "Microsoft Xbox 360", "Microsoft Xbox 360" },
        { "Xbox 360", "Microsoft Xbox 360" },
        { "Xbox360", "Microsoft Xbox 360" },
        { "Microsoft Xbox 360 XBLA", "Microsoft Xbox 360 XBLA" },
        { "Xbox 360 XBLA", "Microsoft Xbox 360 XBLA" },
        { "Xbox360 XBLA", "Microsoft Xbox 360 XBLA" },
        { "XBLA", "Microsoft Xbox 360 XBLA" },
        { "NEC PC Engine", "NEC PC Engine" },
        { "PC Engine", "NEC PC Engine" },
        { "PCEngine", "NEC PC Engine" },
        { "NEC PC Engine CD", "NEC PC Engine CD" },
        { "PC Engine CD", "NEC PC Engine CD" },
        { "PCEngine CD", "NEC PC Engine CD" },
        { "PCEngineCD", "NEC PC Engine CD" },
        { "NEC PC-FX", "NEC PC-FX" },
        { "PC-FX", "NEC PC-FX" },
        { "PCFX", "NEC PC-FX" },
        { "NEC SuperGrafx", "NEC SuperGrafx" },
        { "SuperGrafx", "NEC SuperGrafx" },
        { "NEC TurboGrafx-16", "NEC TurboGrafx-16" },
        { "NEC TurboGrafx 16", "NEC TurboGrafx-16" },
        { "NEC TurboGrafx", "NEC TurboGrafx-16" },
        { "TurboGrafx", "NEC TurboGrafx-16" },
        { "TurboGrafx16", "NEC TurboGrafx-16" },
        { "Nintendo 3DS", "Nintendo 3DS" },
        { "Nintendo3DS", "Nintendo 3DS" },
        { "3DS", "Nintendo 3DS" },
        { "Nintendo 64", "Nintendo 64" },
        { "Nintendo64", "Nintendo 64" },
        { "N64", "Nintendo 64" },
        { "Nintendo 64DD", "Nintendo 64DD" },
        { "Nintendo64DD", "Nintendo 64DD" },
        { "N64DD", "Nintendo 64DD" },
        { "Nintendo DS", "Nintendo DS" },
        { "NintendoDS", "Nintendo DS" },
        { "DS", "Nintendo DS" },
        { "Nintendo Family Computer Disk System", "Nintendo Family Computer Disk System" },
        { "Family Computer Disk System", "Nintendo Family Computer Disk System" },
        { "Nintendo Game Boy", "Nintendo Game Boy" },
        { "Game Boy", "Nintendo Game Boy" },
        { "GameBoy", "Nintendo Game Boy" },
        { "Nintendo Game Boy Advance", "Nintendo Game Boy Advance" },
        { "Game Boy Advance", "Nintendo Game Boy Advance" },
        { "GameBoy Advance", "Nintendo Game Boy Advance" },
        { "Nintendo Game Boy Color", "Nintendo Game Boy Color" },
        { "Game Boy Color", "Nintendo Game Boy Color" },
        { "GameBoy Color", "Nintendo Game Boy Color" },
        { "Nintendo GameCube", "Nintendo GameCube" },
        { "GameCube", "Nintendo GameCube" },
        { "Nintendo NES", "Nintendo NES" },
        { "NES", "Nintendo NES" },
        { "Nintendo Entertainment System", "Nintendo NES" },
        { "Nintendo Famicom", "Nintendo NES" },
        { "Famicom", "Nintendo NES" },
        { "Nintendo Satellaview", "Nintendo Satellaview" },
        { "Satellaview", "Nintendo Satellaview" },
        { "Nintendo SNES", "Nintendo SNES" },
        { "SNES", "Nintendo SNES" },
        { "Super Nintendo", "Nintendo SNES" },
        { "Super NES", "Nintendo SNES" },
        { "Nintendo Super Famicom", "Nintendo SNES" },
        { "Super Famicom", "Nintendo SNES" },
        { "Nintendo SNES MSU1", "Nintendo SNES MSU1" },
        { "Nintendo Super NES MSU1", "Nintendo SNES MSU1" },
        { "SNES MSU1", "Nintendo SNES MSU1" },
        { "MSU1", "Nintendo SNES MSU1" },
        { "Super NES MSU1", "Nintendo SNES MSU1" },
        { "Nintendo Switch", "Nintendo Switch" },
        { "Switch", "Nintendo Switch" },
        { "Nintendo Virtual Boy", "Nintendo Virtual Boy" },
        { "Nintendo VirtualBoy", "Nintendo Virtual Boy" },
        { "Virtual Boy", "Nintendo Virtual Boy" },
        { "VirtualBoy", "Nintendo Virtual Boy" },
        { "Virtual-Boy", "Nintendo Virtual Boy" },
        { "V-Boy", "Nintendo Virtual Boy" },
        { "VBoy", "Nintendo Virtual Boy" },
        { "Nintendo Wii", "Nintendo Wii" },
        { "Wii", "Nintendo Wii" },
        { "Nintendo WiiU", "Nintendo WiiU" },
        { "WiiU", "Nintendo WiiU" },
        { "Nintendo WiiWare", "Nintendo WiiWare" },
        { "WiiWare", "Nintendo WiiWare" },
        { "Panasonic 3DO", "Panasonic 3DO" },
        { "Panasonic3DO", "Panasonic 3DO" },
        { "3DO", "Panasonic 3DO" },
        { "Philips CD-i", "Philips CD-i" },
        { "Philips CDi", "Philips CD-i" },
        { "CD-i", "Philips CD-i" },
        { "CDi", "Philips CD-i" },
        { "ScummVM", "ScummVM" },
        { "Scumm-VM", "ScummVM" },
        { "Sega Dreamcast", "Sega Dreamcast" },
        { "Dreamcast", "Sega Dreamcast" },
        { "Sega Game Gear", "Sega Game Gear" },
        { "Game Gear", "Sega Game Gear" },
        { "GameGear", "Sega Game Gear" },
        { "Sega Genesis", "Sega Genesis" },
        { "Genesis", "Sega Genesis" },
        { "Mega Drive", "Sega Genesis" },
        { "MegaDrive", "Sega Genesis" },
        { "Sega Genesis 32X", "Sega Genesis 32X" },
        { "Genesis 32X", "Sega Genesis 32X" },
        { "Genesis32X", "Sega Genesis 32X" },
        { "Sega 32X", "Sega Genesis 32X" },
        { "Sega32X", "Sega Genesis 32X" },
        { "Sega Genesis CD", "Sega Genesis CD" },
        { "Genesis CD", "Sega Genesis CD" },
        { "GenesisCD", "Sega Genesis CD" },
        { "Sega Master System", "Sega Master System" },
        { "MasterSystem", "Sega Master System" },
        { "Master System", "Sega Master System" },
        { "Sega Mark3", "Sega Master System" },
        { "Mark3", "Sega Master System" },
        { "Sega MarkIII", "Sega Master System" },
        { "MarkIII", "Sega Master System" },
        { "Mark III", "Sega Master System" },
        { "Sega Model 3", "Sega Model 3" },
        { "Model 3", "Sega Model 3" },
        { "Model3", "Sega Model 3" },
        { "Sega Naomi", "Sega Naomi" },
        { "SegaNaomi", "Sega Naomi" },
        { "Naomi", "Sega Naomi" },
        { "Sega Naomi2", "Sega Naomi 2" },
        { "SegaNaomi2", "Sega Naomi 2" },
        { "Naomi2", "Sega Naomi 2" },
        { "Sega Saturn", "Sega Saturn" },
        { "Saturn", "Sega Saturn" },
        { "Sega SC-3000", "Sega SC-3000" },
        { "Sega SC3000", "Sega SC-3000" },
        { "SC-3000", "Sega SC-3000" },
        { "SC3000", "Sega SC-3000" },
        { "Sega SG-1000", "Sega SG-1000" },
        { "Sega SG1000", "Sega SG-1000" },
        { "SG-1000", "Sega SG-1000" },
        { "SG1000", "Sega SG-1000" },
        { "Sharp x68000", "Sharp x68000" },
        { "Sharp x-68000", "Sharp x68000" },
        { "x68000", "Sharp x68000" },
        { "x-68000", "Sharp x68000" },
        { "Sinclair ZX Spectrum", "Sinclair ZX Spectrum" },
        { "ZX Spectrum", "Sinclair ZX Spectrum" },
        { "ZX-Spectrum", "Sinclair ZX Spectrum" },
        { "Spectrum", "Sinclair ZX Spectrum" },
        { "SNK Neo Geo", "SNK Neo Geo" },
        { "SNK NeoGeo", "SNK Neo Geo" },
        { "Neo Geo", "SNK Neo Geo" },
        { "NeoGeo", "SNK Neo Geo" },
        { "SNK Neo Geo CD", "SNK Neo Geo CD" },
        { "SNK NeoGeo CD", "SNK Neo Geo CD" },
        { "SNK NeoGeoCD", "SNK Neo Geo CD" },
        { "Neo Geo CD", "SNK Neo Geo CD" },
        { "NeoGeo CD", "SNK Neo Geo CD" },
        { "NeoGeoCD", "SNK Neo Geo CD" },
        { "SNK Neo Geo Pocket", "SNK Neo Geo Pocket" },
        { "SNK NeoGeo Pocket", "SNK Neo Geo Pocket" },
        { "NeoGeo Pocket", "SNK Neo Geo Pocket" },
        { "SNK Neo Geo Pocket Color", "SNK Neo Geo Pocket Color" },
        { "SNK NeoGeo Pocket Color", "SNK Neo Geo Pocket Color" },
        { "Neo Geo Pocket Color", "SNK Neo Geo Pocket Color" },
        { "NeoGeo Pocket Color", "SNK Neo Geo Pocket Color" },
        { "Sony PlayStation 1", "Sony PlayStation 1" },
        { "PlayStation 1", "Sony PlayStation 1" },
        { "PlayStation", "Sony PlayStation 1" },
        { "PSX", "Sony PlayStation 1" },
        { "PSX1", "Sony PlayStation 1" },
        { "PSX 1", "Sony PlayStation 1" },
        { "Sony PlayStation 2", "Sony PlayStation 2" },
        { "PlayStation 2", "Sony PlayStation 2" },
        { "PSX2", "Sony PlayStation 2" },
        { "PSX 2", "Sony PlayStation 2" },
        { "Sony PlayStation 3", "Sony PlayStation 3" },
        { "PlayStation 3", "Sony PlayStation 3" },
        { "PSX3", "Sony PlayStation 3" },
        { "PSX 3", "Sony PlayStation 3" },
        { "Sony PlayStation 4", "Sony PlayStation 4" },
        { "PlayStation 4", "Sony PlayStation 4" },
        { "PSX4", "Sony PlayStation 4" },
        { "PSX 4", "Sony PlayStation 4" },
        { "Sony PlayStation Vita", "Sony PlayStation Vita" },
        { "PlayStation Vita", "Sony PlayStation Vita" },
        { "Vita", "Sony PlayStation Vita" },
        { "Sony PSP", "Sony PSP" },
        { "PlayStation Portable", "Sony PSP" },
        { "PSP", "Sony PSP" },
        { "Super A'Can", "Super Acan" },
        { "Super ACan", "Super Acan" },
        { "Super-A'Can", "Super Acan" },
        { "Super-ACan", "Super Acan" },
        { "SuperA'Can", "Super Acan" },
        { "SuperACan", "Super Acan" },
        { "A'Can", "Super Acan" },
        { "ACan", "Super Acan" },
        { "Zeebo", "Zeebo" }
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaHelpUserService"/> class
    /// and starts loading the parameters file.
    /// </summary>
    /// <param name="logger">The Serilog logger.</param>
    /// <param name="messageBoxLibrary">The message box service for user notifications.</param>
    /// <param name="localization">Optional localization service for fallback messages
    /// (WPF DynamicResource Nosystemnameprovided / Nodetailsavailablefor parity).</param>
    public AvaloniaHelpUserService(
        ILogger logger,
        IMessageBoxLibraryService messageBoxLibrary,
        LocalizationService? localization = null)
    {
        _logger = logger;
        _localization = localization;
        _manager = new HelpUserManager(logger, messageBoxLibrary);
        try
        {
            _ = _manager.LoadAsync(); // Load parameters.md
        }
        catch (Exception ex)
        {
            // Notify developer
            _logger.Error(ex, "Failed to load parameters.md.");
        }
    }

    /// <summary>
    /// Gets the help text for the given system name (alias-aware), falling back to
    /// a message when no information is available for the system.
    /// </summary>
    /// <param name="systemName">The name of the system to get help for.</param>
    /// <returns>The help text for the system.</returns>
    public string GetHelpText(string systemName)
    {
        if (string.IsNullOrEmpty(systemName))
        {
            return _localization?.GetString("Nosystemnameprovided") ?? "No system name provided.";
        }

        var canonicalName = AliasToCanonicalName.GetValueOrDefault(systemName, systemName);
        // parameters.md uses <br> tags; strip them like the WPF renderer does
        return GetSystemDetails(canonicalName).Replace("<br>", string.Empty);
    }

    /// <summary>
    /// Determines whether the help file has been loaded and contains the system.
    /// </summary>
    /// <param name="systemName">The name of the system to check.</param>
    public bool HasSystemDetails(string systemName)
    {
        if (string.IsNullOrEmpty(systemName)) return false;

        var canonicalName = AliasToCanonicalName.GetValueOrDefault(systemName, systemName);
        return _manager.Systems.Any(s => s.SystemName.Contains(canonicalName, StringComparison.OrdinalIgnoreCase));
    }

    private string GetSystemDetails(string systemName)
    {
        // Fetch the system details from the configuration
        var system = _manager.Systems.FirstOrDefault(s => s.SystemName.Contains(systemName, StringComparison.OrdinalIgnoreCase));

        var fallback = _localization?.GetString("Nodetailsavailablefor") ?? "No details available for";
        return system?.SystemHelperText ?? $"{fallback} '{systemName}'.";
    }
}