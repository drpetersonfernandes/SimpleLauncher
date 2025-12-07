using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Interfaces;
using SimpleLauncher.Managers;

namespace SimpleLauncher.UiHelpers;

public static partial class HelpUser
{
    private static readonly HelpUserManager Manager = new();

    static HelpUser()
    {
        try
        {
            Manager.Load(); // Load helpuser.xml
        }
        catch (Exception ex)
        {
            // Notify developer
            const string contextMessage = "Failed to load helpuser.xml.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
        }
    }

    // Renamed for clarity: Matches **bold text**
    [GeneratedRegex(@"\*\*(.*?)\*\*", RegexOptions.Compiled)]
    private static partial Regex BoldRegex();

    // Renamed for clarity: Matches ## headings
    [GeneratedRegex(@"^##\s*(.*?)$", RegexOptions.Multiline)]
    private static partial Regex HeadingRegex();

    // New regex for Markdown links: [text](url)
    [GeneratedRegex(@"\[(?<text>[^\]]+?)\]\((?<url>https?://\S+?)\)", RegexOptions.Compiled)]
    private static partial Regex MarkdownLinkRegex();

    // Renamed for clarity: Matches raw URLs like http://example.com or www.example.com
    [GeneratedRegex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled)]
    private static partial Regex RawUrlRegex();

    /// <summary>
    /// Updates the content of a RichTextBox with formatted text, including bold, headings,
    /// Markdown links ([text](url)), and raw URLs.
    /// </summary>
    /// <param name="helpUserRichTextBox">The RichTextBox to update.</param>
    /// <param name="systemNameTextBox">The TextBox containing the system name.</param>
    public static void UpdateHelpUserTextBlock(RichTextBox helpUserRichTextBox, TextBox systemNameTextBox)
    {
        var systemName = systemNameTextBox?.Text.Trim() ?? string.Empty;

        // Clear the RichTextBox's content
        helpUserRichTextBox.Document.Blocks.Clear();

        if (string.IsNullOrEmpty(systemName))
        {
            var nosystemnameprovided2 = (string)Application.Current.TryFindResource("Nosystemnameprovided") ?? "No system name provided.";

            // Add a default message to the RichTextBox
            helpUserRichTextBox.Document.Blocks.Add(new Paragraph(new Run(nosystemnameprovided2)));

            return;
        }

        // Define the emulator configurations based on system names
        var responses = new Dictionary<string, Func<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "Amstrad CPC", AmstradCpcDetails },
            { "Amiga", CommodoreAmigaDetails },
            { "CPC", AmstradCpcDetails },
            { "Amstrad CPC GX4000", AmstradCpcgx4000Details },
            { "CPC GX4000", AmstradCpcgx4000Details },
            { "GX4000", AmstradCpcgx4000Details },
            { "Arcade", ArcadeDetails },
            { "Mame", ArcadeDetails },
            { "Atari 2600", Atari2600Details },
            { "Atari2600", Atari2600Details },
            { "Atari 5200", Atari5200Details },
            { "Atari5200", Atari5200Details },
            { "Atari 7800", Atari7800Details },
            { "Atari7800", Atari7800Details },
            { "Atari 8-Bit", Atari8BitDetails },
            { "Atari 8-Bits", Atari8BitDetails },
            { "Atari 8 bits", Atari8BitDetails },
            { "Atari 8bits", Atari8BitDetails },
            { "Atari 800", Atari8BitDetails },
            { "Atari Jaguar", AtariJaguarDetails },
            { "Jaguar", AtariJaguarDetails },
            { "Atari Jaguar CD", AtariJaguarCdDetails },
            { "Jaguar CD", AtariJaguarCdDetails },
            { "Atari Lynx", AtariLynxDetails },
            { "Lynx", AtariLynxDetails },
            { "Atari ST", AtariStDetails },
            { "AtariST", AtariStDetails },
            { "Bandai WonderSwan", BandaiWonderSwanDetails },
            { "Bandai Wonder Swan", BandaiWonderSwanDetails },
            { "WonderSwan", BandaiWonderSwanDetails },
            { "Wonder Swan", BandaiWonderSwanDetails },
            { "Bandai WonderSwan Color", BandaiWonderSwanColorDetails },
            { "Bandai Wonder Swan Color", BandaiWonderSwanColorDetails },
            { "WonderSwan Color", BandaiWonderSwanColorDetails },
            { "Wonder Swan Color", BandaiWonderSwanColorDetails },
            { "Casio PV-1000", CasioPv1000Details },
            { "Casio PV1000", CasioPv1000Details },
            { "Casio PV 1000", CasioPv1000Details },
            { "PV-1000", CasioPv1000Details },
            { "PV1000", CasioPv1000Details },
            { "PV 1000", CasioPv1000Details },
            { "Colecovision", ColecovisionDetails },
            { "Commodore 64", Commodore64Details },
            { "Commodore64", Commodore64Details },
            { "Commodore 128", Commodore128Details },
            { "Commodore128", Commodore128Details },
            { "Commodore Amiga", CommodoreAmigaDetails },
            { "Commodore Amiga CD32", CommodoreAmigaCd32Details },
            { "Commodore Amiga CD", CommodoreAmigaCd32Details },
            { "Amiga CD", CommodoreAmigaCd32Details },
            { "Amiga CD32", CommodoreAmigaCd32Details },
            { "AmigaCD", CommodoreAmigaCd32Details },
            { "AmigaCD32", CommodoreAmigaCd32Details },
            { "LaserDisk", LaserDiskDetails },
            { "Laser Disk", LaserDiskDetails },
            { "Daphne", LaserDiskDetails },
            { "Magnavox Odyssey 2", MagnavoxOdyssey2Details },
            { "Odyssey", MagnavoxOdyssey2Details },
            { "Mattel Aquarius", MattelAquariusDetails },
            { "Aquarius", MattelAquariusDetails },
            { "Mattel Intellivision", MattelIntellivisionDetails },
            { "Intellivision", MattelIntellivisionDetails },
            { "Microsoft DOS", MicrosoftDosDetails },
            { "DOS", MicrosoftDosDetails },
            { "Microsoft MSX", MicrosoftMsxDetails },
            { "MSX", MicrosoftMsxDetails },
            { "MSX1", MicrosoftMsxDetails },
            { "Microsoft MSX2", MicrosoftMsx2Details },
            { "MSX2", MicrosoftMsx2Details },
            { "MSX 2", MicrosoftMsx2Details },
            { "Microsoft Windows", MicrosoftWindowsDetails },
            { "Windows", MicrosoftWindowsDetails },
            { "PC", MicrosoftWindowsDetails },
            { "Microsoft Xbox", MicrosoftXboxDetails },
            { "Xbox", MicrosoftXboxDetails },
            { "Xbox Original", MicrosoftXboxDetails },
            { "Microsoft Xbox 360", MicrosoftXbox360Details },
            { "Xbox 360", MicrosoftXbox360Details },
            { "Xbox360", MicrosoftXbox360Details },
            { "Microsoft Xbox 360 XBLA", MicrosoftXbox360XblaDetails },
            { "Xbox 360 XBLA", MicrosoftXbox360XblaDetails },
            { "Xbox360 XBLA", MicrosoftXbox360XblaDetails },
            { "XBLA", MicrosoftXbox360XblaDetails },
            { "NEC PC Engine", NecpcEngineDetails },
            { "PC Engine", NecpcEngineDetails },
            { "PCEngine", NecpcEngineDetails },
            { "NEC TurboGrafx-16", NecTurboGrafx16Details },
            { "NEC TurboGrafx 16", NecTurboGrafx16Details },
            { "NEC TurboGrafx", NecTurboGrafx16Details },
            { "TurboGrafx", NecTurboGrafx16Details },
            { "TurboGrafx16", NecTurboGrafx16Details },
            { "NEC PC Engine CD", NecpcEngineCdDetails },
            { "PC Engine CD", NecpcEngineCdDetails },
            { "PCEngine CD", NecpcEngineCdDetails },
            { "PCEngineCD", NecpcEngineCdDetails },
            { "NEC PC-FX", NecpcfxDetails },
            { "PC-FX", NecpcfxDetails },
            { "PCFX", NecpcfxDetails },
            { "NEC SuperGrafx", NecSuperGrafxDetails },
            { "SuperGrafx", NecSuperGrafxDetails },
            { "Nintendo 3DS", Nintendo3DsDetails },
            { "Nintendo3DS", Nintendo3DsDetails },
            { "3DS", Nintendo3DsDetails },
            { "Nintendo 64", Nintendo64Details },
            { "Nintendo64", Nintendo64Details },
            { "N64", Nintendo64Details },
            { "Nintendo 64DD", Nintendo64DdDetails },
            { "Nintendo64DD", Nintendo64DdDetails },
            { "N64DD", Nintendo64DdDetails },
            { "Nintendo DS", NintendoDsDetails },
            { "NintendoDS", NintendoDsDetails },
            { "DS", NintendoDsDetails },
            { "Nintendo Family Computer Disk System", NintendoFamilyComputerDiskSystemDetails },
            { "Family Computer Disk System", NintendoFamilyComputerDiskSystemDetails },
            { "Nintendo Game Boy", NintendoGameBoyDetails },
            { "Game Boy", NintendoGameBoyDetails },
            { "GameBoy", NintendoGameBoyDetails },
            { "Nintendo Game Boy Advance", NintendoGameBoyAdvanceDetails },
            { "Game Boy Advance", NintendoGameBoyAdvanceDetails },
            { "GameBoy Advance", NintendoGameBoyAdvanceDetails },
            { "Nintendo Game Boy Color", NintendoGameBoyColorDetails },
            { "Game Boy Color", NintendoGameBoyColorDetails },
            { "GameBoy Color", NintendoGameBoyColorDetails },
            { "Nintendo GameCube", NintendoGameCubeDetails },
            { "GameCube", NintendoGameCubeDetails },
            { "Nintendo NES", NintendoNesDetails },
            { "NES", NintendoNesDetails },
            { "Nintendo Entertainment System", NintendoNesDetails },
            { "Nintendo Famicom", NintendoNesDetails },
            { "Famicom", NintendoNesDetails },
            { "Nintendo Satellaview", NintendoSatellaviewDetails },
            { "Satellaview", NintendoSatellaviewDetails },
            { "Nintendo SNES", NintendoSnesDetails },
            { "SNES", NintendoSnesDetails },
            { "Super Nintendo", NintendoSnesDetails },
            { "Super NES", NintendoSnesDetails },
            { "Nintendo Super Famicom", NintendoSnesDetails },
            { "Super Famicom", NintendoSnesDetails },
            { "Nintendo SNES MSU1", NintendoSnesmsu1Details },
            { "Nintendo Super NES MSU1", NintendoSnesmsu1Details },
            { "SNES MSU1", NintendoSnesmsu1Details },
            { "MSU1", NintendoSnesmsu1Details },
            { "Super NES MSU1", NintendoSnesmsu1Details },
            { "Nintendo Switch", NintendoSwitchDetails },
            { "Switch", NintendoSwitchDetails },
            { "Nintendo Wii", NintendoWiiDetails },
            { "Wii", NintendoWiiDetails },
            { "Nintendo WiiU", NintendoWiiUDetails },
            { "WiiU", NintendoWiiUDetails },
            { "Nintendo WiiWare", NintendoWiiWareDetails },
            { "WiiWare", NintendoWiiWareDetails },

            { "Nintendo Virtual Boy", NintendoVirtualBoyDetails },
            { "Nintendo VirtualBoy", NintendoVirtualBoyDetails },
            { "Virtual Boy", NintendoVirtualBoyDetails },
            { "VirtualBoy", NintendoVirtualBoyDetails },

            { "Panasonic 3DO", Panasonic3DoDetails },
            { "Panasonic3DO", Panasonic3DoDetails },
            { "3DO", Panasonic3DoDetails },
            { "Philips CD-i", PhilipsCDiDetails },
            { "Philips CDi", PhilipsCDiDetails },
            { "CD-i", PhilipsCDiDetails },
            { "CDi", PhilipsCDiDetails },
            { "ScummVM", ScummVmDetails },
            { "Scumm-VM", ScummVmDetails },
            { "Sega Dreamcast", SegaDreamcastDetails },
            { "Dreamcast", SegaDreamcastDetails },
            { "Sega Game Gear", SegaGameGearDetails },
            { "Game Gear", SegaGameGearDetails },
            { "GameGear", SegaGameGearDetails },
            { "Sega Genesis", SegaGenesisDetails },
            { "Genesis", SegaGenesisDetails },
            { "Mega Drive", SegaGenesisDetails },
            { "MegaDrive", SegaGenesisDetails },
            { "Sega Genesis 32X", SegaGenesis32XDetails },
            { "Genesis 32X", SegaGenesis32XDetails },
            { "Genesis32X", SegaGenesis32XDetails },
            { "Sega 32X", SegaGenesis32XDetails },
            { "Sega32X", SegaGenesis32XDetails },
            { "Sega Genesis CD", SegaGenesisCdDetails },
            { "Genesis CD", SegaGenesisCdDetails },
            { "GenesisCD", SegaGenesisCdDetails },
            { "Sega Master System", SegaMasterSystemDetails },
            { "MasterSystem", SegaMasterSystemDetails },
            { "Master System", SegaMasterSystemDetails },
            { "Sega Mark3", SegaMasterSystemDetails },
            { "Mark3", SegaMasterSystemDetails },
            { "Sega MarkIII", SegaMasterSystemDetails },
            { "MarkIII", SegaMasterSystemDetails },
            { "Mark III", SegaMasterSystemDetails },
            { "Sega Model 3", SegaModel3Details },
            { "Model 3", SegaModel3Details },
            { "Model3", SegaModel3Details },
            { "Sega Saturn", SegaSaturnDetails },
            { "Saturn", SegaSaturnDetails },
            { "Sega SC-3000", SegaSc3000Details },
            { "Sega SC3000", SegaSc3000Details },
            { "SC-3000", SegaSc3000Details },
            { "SC3000", SegaSc3000Details },
            { "Sega SG-1000", SegaSg1000Details },
            { "Sega SG1000", SegaSg1000Details },
            { "SG-1000", SegaSg1000Details },
            { "SG1000", SegaSg1000Details },
            { "Sharp x68000", Sharpx68000Details },
            { "Sharp x-68000", Sharpx68000Details },
            { "x68000", Sharpx68000Details },
            { "x-68000", Sharpx68000Details },
            { "Sinclair ZX Spectrum", SinclairZxSpectrumDetails },
            { "ZX Spectrum", SinclairZxSpectrumDetails },
            { "ZX-Spectrum", SinclairZxSpectrumDetails },
            { "Spectrum", SinclairZxSpectrumDetails },
            { "SNK Neo Geo", SnkNeoGeoDetails },
            { "SNK NeoGeo", SnkNeoGeoDetails },
            { "Neo Geo", SnkNeoGeoDetails },
            { "NeoGeo", SnkNeoGeoDetails },
            { "SNK Neo Geo CD", SnkNeoGeoCdDetails },
            { "SNK NeoGeo CD", SnkNeoGeoCdDetails },
            { "SNK NeoGeoCD", SnkNeoGeoCdDetails },
            { "Neo Geo CD", SnkNeoGeoCdDetails },
            { "NeoGeo CD", SnkNeoGeoCdDetails },
            { "NeoGeoCD", SnkNeoGeoCdDetails },
            { "SNK Neo Geo Pocket", SnkNeoGeoPocketDetails },
            { "SNK NeoGeo Pocket", SnkNeoGeoPocketDetails },
            { "NeoGeo Pocket", SnkNeoGeoPocketDetails },
            { "SNK Neo Geo Pocket Color", SnkNeoGeoPocketColorDetails },
            { "SNK NeoGeo Pocket Color", SnkNeoGeoPocketColorDetails },
            { "Neo Geo Pocket Color", SnkNeoGeoPocketColorDetails },
            { "NeoGeo Pocket Color", SnkNeoGeoPocketColorDetails },
            { "Sony PlayStation 1", SonyPlayStation1Details },
            { "PlayStation 1", SonyPlayStation1Details },
            { "PlayStation", SonyPlayStation1Details },
            { "PSX", SonyPlayStation1Details },
            { "PSX1", SonyPlayStation1Details },
            { "PSX 1", SonyPlayStation1Details },
            { "Sony PlayStation 2", SonyPlayStation2Details },
            { "PlayStation 2", SonyPlayStation2Details },
            { "PSX2", SonyPlayStation2Details },
            { "PSX 2", SonyPlayStation2Details },
            { "Sony PlayStation 3", SonyPlayStation3Details },
            { "PlayStation 3", SonyPlayStation3Details },
            { "PSX3", SonyPlayStation3Details },
            { "PSX 3", SonyPlayStation3Details },
            { "Sony PlayStation 4", SonyPlayStation4Details },
            { "PlayStation 4", SonyPlayStation4Details },
            { "PSX4", SonyPlayStation4Details },
            { "PSX 4", SonyPlayStation4Details },
            { "Sony PlayStation Vita", SonyPlayStationVitaDetails },
            { "PlayStation Vita", SonyPlayStationVitaDetails },
            { "Vita", SonyPlayStationVitaDetails },
            { "Sony PSP", SonyPspDetails },
            { "PlayStation Portable", SonyPspDetails },
            { "PSP", SonyPspDetails }
        };

        // Check if a response exists for the given system name
        if (responses.TryGetValue(systemName, out var responseGenerator))
        {
            var text = responseGenerator();
            SetTextWithMarkdownInternal(helpUserRichTextBox, text); // Call the internal parsing method
        }
        else
        {
            // Display a message if the system name is not recognized
            var noinformationavailableforsystem2 = (string)Application.Current.TryFindResource("Noinformationavailableforsystem") ?? "No information available for system:";
            helpUserRichTextBox.Document.Blocks.Add(new Paragraph(new Run($"{noinformationavailableforsystem2} {systemName}")));
        }
    }

    private static string AmstradCpcDetails()
    {
        return GetSystemDetails("Amstrad CPC");
    }

    private static string AmstradCpcgx4000Details()
    {
        return GetSystemDetails("Amstrad CPC GX4000");
    }

    private static string ArcadeDetails()
    {
        return GetSystemDetails("Arcade");
    }

    private static string Atari2600Details()
    {
        return GetSystemDetails("Atari 2600");
    }

    private static string Atari5200Details()
    {
        return GetSystemDetails("Atari 5200");
    }

    private static string Atari7800Details()
    {
        return GetSystemDetails("Atari 7800");
    }

    private static string Atari8BitDetails()
    {
        return GetSystemDetails("Atari 8-Bit");
    }

    private static string AtariJaguarDetails()
    {
        return GetSystemDetails("Atari Jaguar");
    }

    private static string AtariJaguarCdDetails()
    {
        return GetSystemDetails("Atari Jaguar CD");
    }

    private static string AtariLynxDetails()
    {
        return GetSystemDetails("Atari Lynx");
    }

    private static string AtariStDetails()
    {
        return GetSystemDetails("Atari ST");
    }

    private static string BandaiWonderSwanDetails()
    {
        return GetSystemDetails("Bandai WonderSwan");
    }

    private static string BandaiWonderSwanColorDetails()
    {
        return GetSystemDetails("Bandai WonderSwan Color");
    }

    private static string CasioPv1000Details()
    {
        return GetSystemDetails("Casio PV-1000");
    }

    private static string ColecovisionDetails()
    {
        return GetSystemDetails("Colecovision");
    }

    private static string Commodore64Details()
    {
        return GetSystemDetails("Commodore 64");
    }

    private static string Commodore128Details()
    {
        return GetSystemDetails("Commodore 128");
    }

    private static string CommodoreAmigaDetails()
    {
        return GetSystemDetails("Commodore Amiga");
    }

    private static string CommodoreAmigaCd32Details()
    {
        return GetSystemDetails("Commodore Amiga CD32");
    }

    private static string LaserDiskDetails()
    {
        return GetSystemDetails("LaserDisk");
    }

    private static string MagnavoxOdyssey2Details()
    {
        return GetSystemDetails("Magnavox Odyssey 2");
    }

    private static string MattelAquariusDetails()
    {
        return GetSystemDetails("Mattel Aquarius");
    }

    private static string MattelIntellivisionDetails()
    {
        return GetSystemDetails("Mattel Intellivision");
    }

    private static string MicrosoftDosDetails()
    {
        return GetSystemDetails("Microsoft DOS");
    }

    private static string MicrosoftMsxDetails()
    {
        return GetSystemDetails("Microsoft MSX");
    }

    private static string MicrosoftMsx2Details()
    {
        return GetSystemDetails("Microsoft MSX2");
    }

    private static string MicrosoftWindowsDetails()
    {
        return GetSystemDetails("Microsoft Windows");
    }

    private static string MicrosoftXboxDetails()
    {
        return GetSystemDetails("Microsoft Xbox");
    }

    private static string MicrosoftXbox360Details()
    {
        return GetSystemDetails("Microsoft Xbox 360");
    }

    private static string MicrosoftXbox360XblaDetails()
    {
        return GetSystemDetails("Microsoft Xbox 360 XBLA");
    }

    private static string NecpcEngineDetails()
    {
        return GetSystemDetails("NEC PC Engine");
    }

    private static string NecTurboGrafx16Details()
    {
        return GetSystemDetails("NEC TurboGrafx-16");
    }

    private static string NecpcEngineCdDetails()
    {
        return GetSystemDetails("NEC PC Engine CD");
    }

    private static string NecpcfxDetails()
    {
        return GetSystemDetails("NEC PC-FX");
    }

    private static string NecSuperGrafxDetails()
    {
        return GetSystemDetails("NEC SuperGrafx");
    }

    private static string Nintendo3DsDetails()
    {
        return GetSystemDetails("Nintendo 3DS");
    }

    private static string Nintendo64Details()
    {
        return GetSystemDetails("Nintendo 64");
    }

    private static string Nintendo64DdDetails()
    {
        return GetSystemDetails("Nintendo 64DD");
    }

    private static string NintendoDsDetails()
    {
        return GetSystemDetails("Nintendo DS");
    }

    private static string NintendoFamilyComputerDiskSystemDetails()
    {
        return GetSystemDetails("Nintendo Family Computer Disk System");
    }

    private static string NintendoGameBoyDetails()
    {
        return GetSystemDetails("Nintendo Game Boy");
    }

    private static string NintendoGameBoyAdvanceDetails()
    {
        return GetSystemDetails("Nintendo Game Boy Advance");
    }

    private static string NintendoGameBoyColorDetails()
    {
        return GetSystemDetails("Nintendo Game Boy Color");
    }

    private static string NintendoGameCubeDetails()
    {
        return GetSystemDetails("Nintendo GameCube");
    }

    private static string NintendoNesDetails()
    {
        return GetSystemDetails("Nintendo NES");
    }

    private static string NintendoSatellaviewDetails()
    {
        return GetSystemDetails("Nintendo Satellaview");
    }

    private static string NintendoSnesDetails()
    {
        return GetSystemDetails("Nintendo SNES");
    }

    private static string NintendoSnesmsu1Details()
    {
        return GetSystemDetails("Nintendo SNES MSU1");
    }

    private static string NintendoSwitchDetails()
    {
        return GetSystemDetails("Nintendo Switch");
    }

    private static string NintendoWiiDetails()
    {
        return GetSystemDetails("Nintendo Wii");
    }

    private static string NintendoWiiUDetails()
    {
        return GetSystemDetails("Nintendo WiiU");
    }

    private static string NintendoWiiWareDetails()
    {
        return GetSystemDetails("Nintendo WiiWare");
    }

    private static string NintendoVirtualBoyDetails()
    {
        return GetSystemDetails("Nintendo Virtual Boy");
    }

    private static string Panasonic3DoDetails()
    {
        return GetSystemDetails("Panasonic 3DO");
    }

    private static string PhilipsCDiDetails()
    {
        return GetSystemDetails("Philips CD-i");
    }

    private static string ScummVmDetails()
    {
        return GetSystemDetails("ScummVM");
    }

    private static string SegaDreamcastDetails()
    {
        return GetSystemDetails("Sega Dreamcast");
    }

    private static string SegaGameGearDetails()
    {
        return GetSystemDetails("Sega Game Gear");
    }

    private static string SegaGenesisDetails()
    {
        return GetSystemDetails("Sega Genesis");
    }

    private static string SegaGenesis32XDetails()
    {
        return GetSystemDetails("Sega Genesis 32X");
    }

    private static string SegaGenesisCdDetails()
    {
        return GetSystemDetails("Sega Genesis CD");
    }

    private static string SegaMasterSystemDetails()
    {
        return GetSystemDetails("Sega Master System");
    }

    private static string SegaModel3Details()
    {
        return GetSystemDetails("Sega Model 3");
    }

    private static string SegaSaturnDetails()
    {
        return GetSystemDetails("Sega Saturn");
    }

    private static string SegaSc3000Details()
    {
        return GetSystemDetails("Sega SC-3000");
    }

    private static string SegaSg1000Details()
    {
        return GetSystemDetails("Sega SG-1000");
    }

    private static string Sharpx68000Details()
    {
        return GetSystemDetails("Sharp x68000");
    }

    private static string SinclairZxSpectrumDetails()
    {
        return GetSystemDetails("Sinclair ZX Spectrum");
    }

    private static string SnkNeoGeoDetails()
    {
        return GetSystemDetails("SNK Neo Geo");
    }

    private static string SnkNeoGeoCdDetails()
    {
        return GetSystemDetails("SNK Neo Geo CD");
    }

    private static string SnkNeoGeoPocketDetails()
    {
        return GetSystemDetails("SNK Neo Geo Pocket");
    }

    private static string SnkNeoGeoPocketColorDetails()
    {
        return GetSystemDetails("SNK Neo Geo Pocket Color");
    }

    private static string SonyPlayStation1Details()
    {
        return GetSystemDetails("Sony PlayStation 1");
    }

    private static string SonyPlayStation2Details()
    {
        return GetSystemDetails("Sony PlayStation 2");
    }

    private static string SonyPlayStation3Details()
    {
        return GetSystemDetails("Sony PlayStation 3");
    }

    private static string SonyPlayStation4Details()
    {
        return GetSystemDetails("Sony PlayStation 4");
    }

    private static string SonyPlayStationVitaDetails()
    {
        return GetSystemDetails("Sony PlayStation Vita");
    }

    private static string SonyPspDetails()
    {
        return GetSystemDetails("Sony PSP");
    }

    private static string GetSystemDetails(string systemName)
    {
        // Fetch the system details from the configuration
        var system = Manager.Systems.FirstOrDefault(s => s.SystemName.Contains(systemName, StringComparison.OrdinalIgnoreCase));

        var nodetailsavailablefor2 = (string)Application.Current.TryFindResource("Nodetailsavailablefor") ?? "No details available for";
        return system?.SystemHelperText ?? $"{nodetailsavailablefor2} '{systemName}'.";
    }

    /// <summary>
    /// Helper method to add plain text to a paragraph, processing raw URLs within it.
    /// </summary>
    /// <param name="paragraph">The paragraph to add inlines to.</param>
    /// <param name="text">The plain text segment to process.</param>
    private static void AddRawUrlsToParagraph(Paragraph paragraph, string text)
    {
        if (string.IsNullOrEmpty(text)) return;

        var parts = RawUrlRegex().Split(text);
        var matches = RawUrlRegex().Matches(text);

        var matchIndex = 0;
        foreach (var part in parts)
        {
            if (!string.IsNullOrEmpty(part))
            {
                paragraph.Inlines.Add(new Run(part));
            }

            if (matchIndex < matches.Count)
            {
                var rawUrl = matches[matchIndex].Value;
                var rawHyperlink = new Hyperlink(new Run(rawUrl))
                {
                    NavigateUri = new Uri(rawUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                        ? rawUrl
                        : "http://" + rawUrl)
                };
                rawHyperlink.RequestNavigate += static (_, e) =>
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = e.Uri.AbsoluteUri,
                        UseShellExecute = true
                    });
                };
                paragraph.Inlines.Add(rawHyperlink);
                matchIndex++;
            }
        }
    }

    /// <summary>
    /// Parses the input text for Markdown formatting (bold, headings, Markdown links, raw URLs)
    /// and displays it in the provided RichTextBox.
    /// </summary>
    /// <param name="richTextBox">The RichTextBox to display the formatted text in.</param>
    /// <param name="text">The text containing Markdown to parse.</param>
    private static void SetTextWithMarkdownInternal(RichTextBox richTextBox, string text)
    {
        var flowDocument = new FlowDocument();
        var paragraph = new Paragraph();

        // Remove <br> tags
        text = text.Replace("<br>", "");

        // 1. Process headings first, converting them to bold syntax for subsequent bold processing
        text = HeadingRegex().Replace(text, static match => $"**{match.Groups[1].Value.Trim()}**");

        var matches = new List<(Match Match, string Type)>();

        // Find all bold matches
        foreach (Match match in BoldRegex().Matches(text))
        {
            matches.Add((match, "bold"));
        }

        // Find all markdown link matches
        foreach (Match match in MarkdownLinkRegex().Matches(text))
        {
            matches.Add((match, "markdownLink"));
        }

        // Sort matches by their starting index
        // This is crucial for correct processing order and handling overlaps (e.g., a markdown link
        // will take precedence over a raw URL that might be part of its URL component).
        matches.Sort((a, b) => a.Match.Index.CompareTo(b.Match.Index));

        var lastIndex = 0;

        foreach (var (match, type) in matches)
        {
            // Add plain text (and any raw URLs within it) before the current match
            if (match.Index > lastIndex)
            {
                var plainTextSegment = text.Substring(lastIndex, match.Index - lastIndex);
                AddRawUrlsToParagraph(paragraph, plainTextSegment);
            }

            // Add the formatted text based on type
            switch (type)
            {
                case "bold":
                    paragraph.Inlines.Add(new Bold(new Run(match.Groups[1].Value)));
                    break;
                case "markdownLink":
                    var linkText = match.Groups["text"].Value;
                    var url = match.Groups["url"].Value;
                    var hyperlink = new Hyperlink(new Run(linkText))
                    {
                        NavigateUri = new Uri(url)
                    };
                    hyperlink.RequestNavigate += static (_, e) =>
                    {
                        Process.Start(new ProcessStartInfo
                        {
                            FileName = e.Uri.AbsoluteUri,
                            UseShellExecute = true
                        });
                    };
                    paragraph.Inlines.Add(hyperlink);
                    break;
            }

            lastIndex = match.Index + match.Length;
        }

        // Add any remaining plain text (and raw URLs within it) after the last match
        if (lastIndex < text.Length)
        {
            var remainingText = text.Substring(lastIndex);
            AddRawUrlsToParagraph(paragraph, remainingText);
        }

        flowDocument.Blocks.Add(paragraph);
        richTextBox.Document = flowDocument;
    }
}
