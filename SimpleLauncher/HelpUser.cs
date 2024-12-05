using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Documents;

namespace SimpleLauncher;

public static class HelpUser
{
    public static void UpdateHelpUserTextBlock(TextBlock helpUserTextBlock, TextBox systemNameTextBox)
    {
        // Retrieve the system name from the TextBox
        string systemName = systemNameTextBox?.Text.Trim() ?? string.Empty;

        if (string.IsNullOrEmpty(systemName))
        {
            // Clear the TextBlock and display a default message if no system name is provided
            helpUserTextBlock.Inlines.Clear();
            helpUserTextBlock.Inlines.Add(new Run("No system name provided."));
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
            { "Bandai WonderSwan", BandaiWonderSwanDetails }
        };
        
        helpUserTextBlock.Inlines.Clear();

        // Check if a response exists for the given system name
        if (responses.TryGetValue(systemName, out var responseGenerator))
        {
            var text = responseGenerator();
            SetTextWithLinks(helpUserTextBlock, text);
        }
        else
        {
            // Display a message if the system name is not recognized
            helpUserTextBlock.Inlines.Add(new Run($"No information available for system: {systemName}"));
        }
    }
    
    private static void SetTextWithLinks(TextBlock textBlock, string text)
    {
        var regex = new Regex(@"\b(?:https?://|www\.)\S+\b", RegexOptions.Compiled);
        var parts = regex.Split(text);
        var matches = regex.Matches(text);

        int index = 0;
        foreach (var part in parts)
        {
            textBlock.Inlines.Add(new Run(part));

            if (index < matches.Count)
            {
                var hyperlink = new Hyperlink(new Run(matches[index].Value))
                {
                    NavigateUri = new Uri(matches[index].Value.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                        ? matches[index].Value
                        : "http://" + matches[index].Value)
                };
                hyperlink.RequestNavigate += (_, e) =>
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
    }

    private static string AmstradCpcDetails()
    {
        return
            @"Amstrad CPC

System Folder (Example): c:\Amstrad CPC
System Is MAME? false
Format To Search In System Folder: zip
Extract File Before Launch? false
Format To Launch After Extraction:

Emulator Name: Retroarch caprice32
Emulator Location (Example): c:\emulators\retroarch\retroarch.exe
Emulator Parameters (Example): -L ""c:\emulators\retroarch\cores\cap32_libretro.dll"" -f
Fullscreen Parameter: -f

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/caprice32/). 
Core may require BIOS files or system files to work properly.";
    }

    private static string AmstradCpcgx4000Details()
    {
        return
            @"Amstrad CPC GX4000

System Folder (Example): c:\Amstrad CPC GX4000
System Is MAME? true
Format To Search In System Folder: zip
Extract File Before Launch? false
Format To Launch After Extraction:

Emulator Name: MAME Amstrad CPC GX4000
Emulator Location (Example): c:\emulators\mame\mame.exe
Emulator Parameters (Example): -rompath ""c:\emulators\mame\roms;c:\emulators\mame\bios;c:\Amstrad CPC GX4000"" gx4000 -cart
Fullscreen Parameter: -window";
    }

    private static string ArcadeDetails()
    {
        return
            @"Arcade

System Folder (Example): c:\emulators\mame\roms
System Is MAME? true
Format To Search In System Folder: zip
Extract File Before Launch? false
Format To Launch After Extraction:

Emulator Name: MAME
Emulator Location (Example): C:\emulators\mame\mame.exe
Emulator Parameters (Example): -rompath ""c:\emulators\mame\roms;c:\emulators\mame\bios""
Fullscreen Parameter: -window

Emulator Name: Retroarch mame
Emulator Location (Example): C:\emulators\retroarch\retroarch.exe
Emulator Parameters (Example): -L ""C:\emulators\retroarch\cores\mame_libretro.dll"" -f
Fullscreen Parameter: -f

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/mame_2010/).
Core may require BIOS files or system files to work properly.";
    }
    
    private static string Atari2600Details()
    {
        return
            @"Atari 2600

System Folder (Example): c:\Atari 2600
System Is MAME? false
Format To Search In System Folder: zip
Extract File Before Launch? false
Format To Launch After Extraction: 

Emulator Name: Retroarch stella
Emulator Location (Example): c:\emulators\retroarch\retroarch.exe
Emulator Parameters (Example): -L ""c:\emulators\retroarch\cores\stella_libretro.dll"" -f
Fullscreen Parameter: -f

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/stella/).
Core may require BIOS files or system files to work properly.

Emulator Name: Stella
Emulator Location (Example): C:\emulators\stella\Stella.exe
Emulator Parameters (Example): -fullscreen 1
Fullscreen Parameter: -fullscreen 1

Command line documentation can be found at [Stella website](https://stella-emu.github.io/docs/index.html#CommandLine).";
    }
    
    private static string Atari5200Details()
    {
        return
            @"Atari 5200

System Folder (Example): c:\Atari 5200
System Is MAME? false
Format To Search In System Folder: zip
Extract File Before Launch? false
Format To Launch After Extraction: 

Emulator Name: Altirra
Emulator Location (Example): c:\emulators\altirra\Altirra64.exe
Emulator Parameters (Example): /f
Fullscreen Parameter: /f";
    }
    
    private static string Atari7800Details()
    {
        return
            @"Atari 7800

System Folder (Example): c:\Atari 7800
System Is MAME? false
Format To Search In System Folder: zip
Extract File Before Launch? false
Format To Launch After Extraction: 

Emulator Name: Retroarch prosystem
Emulator Location (Example): c:\emulators\retroarch\retroarch.exe
Emulator Parameters (Example): -L ""c:\emulators\retroarch\cores\prosystem_libretro.dll"" -f
Fullscreen Parameter: -f

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/prosystem/).
Core may require BIOS files or system files to work properly.";
    }
    
    private static string Atari8BitDetails()
    {
        return
            @"Atari 8-Bit

System Folder (Example): c:\Atari 8-Bit
System Is MAME? false
Format To Search In System Folder: zip
Extract File Before Launch? false
Format To Launch After Extraction: 

Emulator Name: Altirra
Emulator Location (Example): c:\emulators\altirra\Altirra64.exe
Emulator Parameters: /f
Fullscreen Parameter: /f";
    }
    
    private static string AtariJaguarDetails()
    {
        return
            @"Atari Jaguar

System Folder (Example): c:\Atari Jaguar
System Is MAME? false
Format To Search In System Folder: zip
Extract File Before Launch? false
Format To Launch After Extraction: 

Emulator Name: BigPEmu
Emulator Location (Example): c:\emulators\bigpemu\BigPEmu.exe
Emulator Parameters (Example): 
Fullscreen Parameter: ";
    }
    
    private static string AtariJaguarCdDetails()
    {
        return
            @"Atari Jaguar CD

System Folder (Example): c:\Atari Jaguar CD
System Is MAME? false
Format To Search In System Folder: zip, 7z
Extract File Before Launch? true
Format To Launch After Extraction: cue, cdi

Emulator Name: BigPEmu
Emulator Location (Example): c:\emulators\bigpemu\BigPEmu.exe
Emulator Parameters (Example): 
Fullscreen Parameter: ";
    }
    
    private static string AtariLynxDetails()
    {
        return
            @"Atari Lynx

System Folder (Example): c:\Atari Lynx
System Is MAME? false
Format To Search In System Folder: zip, 7z
Extract File Before Launch? false
Format To Launch After Extraction: lnx, o

Emulator Name: Retroarch mednafen_lynx
Emulator Location (Example): c:\emulators\retroarch\retroarch.exe
Emulator Parameters (Example): -L ""c:\emulators\retroarch\cores\mednafen_lynx_libretro.dll"" -f
Fullscreen Parameter: -f

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/beetle_lynx/).
Core may require BIOS or system files to run properly.

Emulator Name: Mednafen
Emulator Location (Example): c:\emulators\mednafen\mednafen.exe
Emulator Parameters: 
Fullscreen Parameter: ";
    }
    
    private static string AtariStDetails()
    {
        return
            @"Atari ST

System Folder (Example): c:\Atari ST
System Is MAME? false
Format To Search In System Folder: zip, msa, st, stx, dim, ipf
Extract File Before Launch? false
Format To Launch After Extraction: 

Emulator Name: Retroarch hatari
Emulator Location (Example): c:\emulators\retroarch\retroarch.exe
Emulator Parameters (Example): -L ""c:\emulators\retroarch\cores\hatari_libretro.dll"" -f
Fullscreen Parameter: -f

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/hatari/).
Core may require BIOS or system files to run properly.

Emulator Name: Hatari
Emulator Location (Example): C:\emulators\hatari\hatari.exe
Emulator Parameters: 
Fullscreen Parameter: 

Emulator documentation can be found at [GitHub website](https://github.com/hatari/hatari).
Emulator may require BIOS or system files to run properly.";
    }
    
    private static string BandaiWonderSwanDetails()
    {
        return
            @"Bandai WonderSwan

System Folder (Example): c:\Bandai WonderSwan
System Is MAME? false
Format To Search In System Folder: zip
Extract File Before Launch? false
Format To Launch After Extraction: 

Emulator Name: Retroarch mednafen_wswan
Emulator Location (Example): c:\emulators\retroarch\retroarch.exe
Emulator Parameters (Example): -L ""c:\emulators\retroarch\cores\mednafen_wswan_libretro.dll"" -f
Fullscreen Parameter: -f

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/beetle_cygne/).
Core may require BIOS or system files to work properly.

Emulator Name: BizHawk
Emulator Location (Example): c:\emulators\emuhawk\EmuHawk.exe
Emulator Parameters: 
Fullscreen Parameter: 

Emulator Name: Mednafen
Emulator Location (Example): c:\emulators\mednafen\mednafen.exe
Emulator Parameters: 
Fullscreen Parameter: ";
    }
}