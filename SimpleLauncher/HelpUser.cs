using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using SimpleLauncher.Services;

namespace SimpleLauncher;

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
            _ = LogErrors.LogErrorAsync(ex, contextMessage);
        }
    }

    public static void UpdateHelpUserTextBlock(TextBlock helpUserTextBlock, TextBox systemNameTextBox)
    {
        var systemName = systemNameTextBox?.Text.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(systemName))
        {
            var nosystemnameprovided2 = (string)Application.Current.TryFindResource("Nosystemnameprovided") ?? "No system name provided.";

            // Clear the TextBlock and display a default message if no system name is provided
            helpUserTextBlock.Inlines.Clear();
            helpUserTextBlock.Inlines.Add(new Run(nosystemnameprovided2));

            return;
        }

        // Define the emulator configurations based on system names
        var responses = new Dictionary<string, Func<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "Amstrad CPC", AmstradCpcDetails },
            { "Amstrad CPC GX4000", AmstradCpcgx4000Details },
            { "Arcade", ArcadeDetails },
            { "Atari 2600", Atari2600Details },
            { "Atari 5200", Atari5200Details },
            { "Atari 7800", Atari7800Details },
            { "Atari 8-Bit", Atari8BitDetails },
            { "Atari Jaguar", AtariJaguarDetails },
            { "Atari Jaguar CD", AtariJaguarCdDetails },
            { "Atari Lynx", AtariLynxDetails },
            { "Atari ST", AtariStDetails },
            { "Bandai WonderSwan", BandaiWonderSwanDetails },
            { "Bandai WonderSwan Color", BandaiWonderSwanColorDetails },
            { "Casio PV-1000", CasioPv1000Details },
            { "Colecovision", ColecovisionDetails },
            { "Commodore 64", Commodore64Details },
            { "Commodore Amiga CD32", CommodoreAmigaCd32Details },
            { "LaserDisk", LaserDiskDetails },
            { "Magnavox Odyssey 2", MagnavoxOdyssey2Details },
            { "Mattel Aquarius", MattelAquariusDetails },
            { "Mattel Intellivision", MattelIntellivisionDetails },
            { "Microsoft DOS", MicrosoftDosDetails },
            { "Microsoft MSX", MicrosoftMsxDetails },
            { "Microsoft MSX2", MicrosoftMsx2Details },
            { "Microsoft Windows", MicrosoftWindowsDetails },
            { "Microsoft Xbox", MicrosoftXboxDetails },
            { "Microsoft Xbox 360", MicrosoftXbox360Details },
            { "Microsoft Xbox 360 XBLA", MicrosoftXbox360XblaDetails },
            { "Microsoft Xbox 360 XBLA Using Compressed Folders", MicrosoftXbox360XblaUsingCompressedFoldersDetails },
            { "Microsoft Xbox 360 XBLA Using BAT files", MicrosoftXbox360XblaUsingBaTfilesDetails },
            { "NEC PC Engine", NecpcEngineDetails },
            { "NEC TurboGrafx-16", NecTurboGrafx16Details },
            { "NEC PC Engine CD", NecpcEngineCdDetails },
            { "NEC PC-FX", NecpcfxDetails },
            { "NEC SuperGrafx", NecSuperGrafxDetails },
            { "Nintendo 3DS", Nintendo3DsDetails },
            { "Nintendo 64", Nintendo64Details },
            { "Nintendo 64DD", Nintendo64DdDetails },
            { "Nintendo DS", NintendoDsDetails },
            { "Nintendo Family Computer Disk System", NintendoFamilyComputerDiskSystemDetails },
            { "Nintendo Game Boy", NintendoGameBoyDetails },
            { "Nintendo Game Boy Advance", NintendoGameBoyAdvanceDetails },
            { "Nintendo Game Boy Color", NintendoGameBoyColorDetails },
            { "Nintendo GameCube", NintendoGameCubeDetails },
            { "Nintendo NES", NintendoNesDetails },
            { "Nintendo Satellaview", NintendoSatellaviewDetails },
            { "Nintendo SNES", NintendoSnesDetails },
            { "Nintendo SNES MSU1", NintendoSnesmsu1Details },
            { "Nintendo Switch", NintendoSwitchDetails },
            { "Nintendo Wii", NintendoWiiDetails },
            { "Nintendo WiiU", NintendoWiiUDetails },
            { "Nintendo WiiWare", NintendoWiiWareDetails },
            { "Panasonic 3DO", Panasonic3DoDetails },
            { "Philips CD-i", PhilipsCDiDetails },
            { "ScummVM", ScummVmDetails },
            { "Sega Dreamcast", SegaDreamcastDetails },
            { "Sega Game Gear", SegaGameGearDetails },
            { "Sega Genesis", SegaGenesisDetails },
            { "Sega Genesis 32X", SegaGenesis32XDetails },
            { "Sega Genesis CD", SegaGenesisCdDetails },
            { "Sega Master System", SegaMasterSystemDetails },
            { "Sega Model 3", SegaModel3Details },
            { "Sega Saturn", SegaSaturnDetails },
            { "Sega SC-3000", SegaSc3000Details },
            { "Sega SG-1000", SegaSg1000Details },
            { "Sharp x68000", Sharpx68000Details },
            { "Sinclair ZX Spectrum", SinclairZxSpectrumDetails },
            { "SNK Neo Geo CD", SnkNeoGeoCdDetails },
            { "SNK Neo Geo Pocket", SnkNeoGeoPocketDetails },
            { "SNK Neo Geo Pocket Color", SnkNeoGeoPocketColorDetails },
            { "Sony PlayStation 1", SonyPlayStation1Details },
            { "Sony PlayStation 2", SonyPlayStation2Details },
            { "Sony PlayStation 3", SonyPlayStation3Details },
            { "Sony PlayStation 4", SonyPlayStation4Details },
            { "Sony PlayStation Vita", SonyPlayStationVitaDetails },
            { "Sony PSP", SonyPspDetails }
        };

        helpUserTextBlock.Inlines.Clear();

        // Check if a response exists for the given system name
        if (responses.TryGetValue(systemName, out var responseGenerator))
        {
            var text = responseGenerator();
            SetTextWithMarkdown(helpUserTextBlock, text);
        }
        else
        {
            // Display a message if the system name is not recognized
            var noinformationavailableforsystem2 = (string)Application.Current.TryFindResource("Noinformationavailableforsystem") ?? "No information available for system:";
            helpUserTextBlock.Inlines.Add(new Run($"{noinformationavailableforsystem2} {systemName}"));
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

    private static string MicrosoftXbox360XblaUsingCompressedFoldersDetails()
    {
        return GetSystemDetails("Microsoft Xbox 360 XBLA Using Compressed Folders");
    }

    private static string MicrosoftXbox360XblaUsingBaTfilesDetails()
    {
        return GetSystemDetails("Microsoft Xbox 360 XBLA Using BAT files");
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
        var system = Manager.Systems.FirstOrDefault(s => s.SystemName.Equals(systemName, StringComparison.OrdinalIgnoreCase));

        var nodetailsavailablefor2 = (string)Application.Current.TryFindResource("Nodetailsavailablefor") ?? "No details available for";
        return system?.SystemHelperText ?? $"{nodetailsavailablefor2} '{systemName}'.";
    }

    private static void SetTextWithMarkdown(TextBlock textBlock, string text)
    {
        textBlock.Inlines.Clear();

        // Remove <br> tags
        text = text.Replace("<br>", "");

        // Regular expressions for bold and headings (excluding underscore italics)
        var markdownRegex = MyRegex(); // Match bold (**text**)
        var headingRegex = MyRegex1(); // Match lines starting with ##
        var linkRegex = MyRegex2(); // Match URLs

        // Process lines for headings (##)
        text = headingRegex.Replace(text, static match =>
        {
            var boldText = match.Groups[1].Value.Trim();
            return $"**{boldText}**"; // Convert headings to bold syntax
        });

        var lastIndex = 0;

        foreach (Match match in markdownRegex.Matches(text))
        {
            // Add plain text before the match
            if (match.Index > lastIndex)
            {
                var plainText = text.Substring(lastIndex, match.Index - lastIndex);
                AddTextWithLinks(textBlock, plainText, linkRegex);
            }

            // Add formatted text (bold only)
            if (match.Groups[1].Success) // Bold
            {
                textBlock.Inlines.Add(new Bold(new Run(match.Groups[1].Value)));
            }

            lastIndex = match.Index + match.Length;
        }

        // Add the remaining text after the last match
        if (lastIndex >= text.Length) return;

        var remainingText = text.Substring(lastIndex);
        AddTextWithLinks(textBlock, remainingText, linkRegex);
    }

    private static void AddTextWithLinks(TextBlock textBlock, string text, Regex linkRegex)
    {
        var parts = linkRegex.Split(text);
        var matches = linkRegex.Matches(text);

        var index = 0;
        foreach (var part in parts)
        {
            // Add plain text
            textBlock.Inlines.Add(new Run(part));

            // Add hyperlink in bold
            if (index >= matches.Count) continue;

            var hyperlink = new Hyperlink(new Bold(new Run(matches[index].Value)))
            {
                NavigateUri = new Uri(matches[index].Value.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                    ? matches[index].Value
                    : "http://" + matches[index].Value)
            };
            hyperlink.RequestNavigate += static (_, e) =>
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
            };
            textBlock.Inlines.Add(hyperlink);
            index++;
        }
    }

    [GeneratedRegex(@"\*\*(.*?)\*\*", RegexOptions.Compiled)]
    private static partial Regex MyRegex();

    [GeneratedRegex(@"^##\s*(.*?)$", RegexOptions.Multiline)]
    private static partial Regex MyRegex1();

    [GeneratedRegex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled)]
    private static partial Regex MyRegex2();
}