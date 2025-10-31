# List of Parameters to use in the "system.xml"

## Amstrad CPC

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Amstrad CPC<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch caprice32<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\cap32_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\cap32_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/caprice32/).<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/caprice32/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example for floppy disk images - .dsk):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" cpc6128 -flop1<br>
**Emulator Parameters (Example for floppy disk images - .dsk):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" cpc464 -flop1<br>
**Emulator Parameters (Example for tape images - .cdt, .wav):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" cpc464 -cass<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Amstrad GX4000

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Amstrad CPC GX4000<br>
**System Is MAME?** true<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\Amstrad CPC GX4000" gx4000 -cart<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" gx4000 -cart<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Arcade

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\emulators\mame\roms<br>
**System Is MAME?** true<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\emulators\mame\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "c:\emulators\mame\roms;c:\emulators\mame\bios"<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios"<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>

.

**Emulator Name:** Retroarch mame<br>
**Emulator Path (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "C:\emulators\retroarch\cores\mame_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\mame_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/mame_2010/).<br>
Core may require BIOS files or system files to work properly.

## Atari 2600

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Atari 2600<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch stella<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\stella_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\stella_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/stella/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Stella<br>
**Emulator Path (Example):** C:\emulators\stella\Stella.exe<br>
**Emulator Parameters (Example):** -fullscreen 1<br>
**Fullscreen Parameter:** -fullscreen 1<br>

This emulator is available for Windows-x64.<br>
Command line documentation can be found on [Stella Website](https://stella-emu.github.io/docs/index.html#CommandLine).

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Atari 2600"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Command-line options can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;C:\ROMs\Atari 2600" a2600<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" a2600<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Atari 5200

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Atari 5200<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** ATR, ATX, ATZ, DCM, XFD, PRO, ARC, BAS, ROM, BIN, A52, CAS, SAP<br>

.

**Emulator Name:** Altirra<br>
**Emulator Path (Example):** c:\emulators\altirra\Altirra64.exe<br>
**Emulator Parameters (Example):** /f<br>
**Fullscreen Parameter:** /f<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>

.

**Emulator Name:** Retroarch a5200<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\a5200_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\a5200_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/atari800/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;C:\ROMs\Atari 5200" a5200<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" a5200<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Atari 7800

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Atari 7800<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch prosystem<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\prosystem_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\prosystem_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/prosystem/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;C:\ROMs\Atari 7800" a7800<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" a7800<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Atari 8-Bit / 800

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Atari 8-Bit<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** ATR, ATX, ATZ, DCM, XFD, PRO, ARC, BAS, ROM, BIN, A52, CAS, SAP<br>

.

**Emulator Name:** Altirra<br>
**Emulator Path (Example):** c:\emulators\altirra\Altirra64.exe<br>
**Emulator Parameters:** /f<br>
**Fullscreen Parameter:** /f<br>

This emulator is available for Windows-x64 and Windows-arm64.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;C:\ROMs\Atari 800" a800<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" a800<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

.

**Emulator Name:** Retroarch atari800_libretro<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\atari800_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\atari800_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/atari800/).<br>
Core may require BIOS files or system files to work properly.

## Atari Jaguar

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Atari Jaguar<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** BigPEmu<br>
**Emulator Path (Example):** c:\emulators\bigpemu\BigPEmu.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64 and Windows-arm64.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;C:\ROMs\Atari Jaguar" jaguar<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" jaguar<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Atari Jaguar CD

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Atari Jaguar CD<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** cue, cdi<br>

.

**Emulator Name:** BigPEmu<br>
**Emulator Path (Example):** c:\emulators\bigpemu\BigPEmu.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64 and Windows-arm64.

## Atari Lynx

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Atari Lynx<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** lnx, o<br>

.

**Emulator Name:** Retroarch mednafen_lynx<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\mednafen_lynx_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\mednafen_lynx_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/beetle_lynx/).<br>
Core requires lynxboot.img BIOS file to work.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\MAME\roms;C:\MAME\bios;C:\Atari Lynx" lynx<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" lynx<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Atari ST

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Atari ST<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** msa, st, stx, dim, ipf<br>

.

**Emulator Name:** Retroarch hatari<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\hatari_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\hatari_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/hatari/).<br>
Core requires tos.img BIOS file to work.

.

**Emulator Name:** Hatari<br>
**Emulator Path (Example):** C:\emulators\hatari\hatari.exe<br>
**Emulator Parameters:** <br>
**Fullscreen Parameter:** -f or --fullscreen<br>
**Windowed Parameter:** -w or --window<br>

This emulator is available for Windows-x64.<br>
You can find a list of command-line args on the [Hatari Website](https://www.hatari-emu.org/doc/manual.html#Command_line_options_and_arguments).<br>
Emulator may require BIOS or system files to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\MAME\roms;C:\MAME\bios;C:\Atari ST" st<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" st<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Bandai WonderSwan

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Bandai WonderSwan<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch mednafen_wswan<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\mednafen_wswan_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\mednafen_wswan_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/beetle_cygne/).<br>
Core may require BIOS or system files to work properly.

.

**Emulator Name:** BizHawk<br>
**Emulator Path (Example):** c:\emulators\BizHawk\EmuHawk.exe<br>
**Emulator Parameters:** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>

.

**Emulator Name:** Mednafen<br>
**Emulator Path (Example):** c:\emulators\mednafen\mednafen.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
Emulator documentation can be found on [Mednafen Website](https://mednafen.github.io/documentation/).

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "WonderSwan"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Command-line options can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;C:\ROMs\WonderSwan" wswan<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" wswan<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Bandai WonderSwan Color

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Bandai WonderSwan Color<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch mednafen_wswan<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\mednafen_wswan_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\mednafen_wswan_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/beetle_cygne/).<br>
Core may require BIOS or system files to work properly.

.

**Emulator Name:** BizHawk<br>
**Emulator Path (Example):** c:\emulators\BizHawk\EmuHawk.exe<br>
**Emulator Parameters:** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>

.

**Emulator Name:** Mednafen<br>
**Emulator Path (Example):** c:\emulators\mednafen\mednafen.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
Emulator documentation can be found on [Mednafen Website](https://mednafen.github.io/documentation/).

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "WonderSwan Color"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Command-line options can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;C:\ROMs\WonderSwan Color" wscolor<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" wscolor<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Casio PV-1000

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Casio PV-1000<br>
**System Is MAME?** true<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** MAME pv1000<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios\;C:\Casio PV-1000" pv1000 -cart<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios\;%SYSTEMFOLDER%" pv1000 -cart<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Colecovision

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Colecovision<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** col, cv, bin, rom<br>

.

**Emulator Name:** Retroarch gearcoleco<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\gearcoleco_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\gearcoleco_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/gearcoleco/).<br>
Core requires colecovision.rom BIOS file to work.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "ColecoVision"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Command-line options can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios\;C:\Colecovision" coleco<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios\;%SYSTEMFOLDER%" coleco<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Commodore 64

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Commodore 64<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** arc, d64, d71, d81, g64, lnx, nbz, nib, prg, sda, sfx, t64, tap, 80, bin, crt<br>

.

**Emulator Name:** Retroarch vice_x64<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\vice_x64_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\vice_x64_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/vice/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** CCS64<br>
**Emulator Path (Example):** c:\emulators\CCS64\CCS64.exe<br>
**Emulator Parameters (Example):**<br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
Emulator documentation can be found on [CCS64 Website](https://www.ccs64.com/ccs64.html).

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** c:\emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" c64<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" c64 -flop<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" c64 -cass<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" c64 -cart<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

.

**Emulator Name:** Vice<br>
**Emulator Path (Example):** C:\Emulators\Vice\bin\x64sc.exe<br>
**Emulator Parameters (Autostart):** -autostart<br>
**Emulator Parameters (Attach and autoload tape/disk image):** -autoload<br>
**Fullscreen Parameter:** -fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
You can find the list of command-line args on the [Vice Website](https://vice-emu.sourceforge.io/vice_toc.html).

## Commodore 128

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Commodore 128<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** arc, d64, d71, d81, g64, lnx, nbz, nib, prg, sda, sfx, t64, tap, 80, bin, crt<br>

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** c:\emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" c128<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" c128 -flop<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" c128 -cass<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" c128 -cart<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

.

**Emulator Name:** Vice<br>
**Emulator Path (Example):** C:\Emulators\Vice\bin\x128.exe<br>
**Emulator Parameters (Autostart):** -autostart<br>
**Emulator Parameters (Attach and autoload tape/disk image):** -autoload<br>
**Fullscreen Parameter:** -fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
You can find the list of command-line args on the [Vice Website](https://vice-emu.sourceforge.io/vice_toc.html).

## Commodore Amiga

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Commodore Amiga<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** adf, adz, dms, fdi, ipf, raw, hdf, hdz, directory, lha, slave, info, cur, ccd, chd, nrg, mds, iso, uae, m3u<br>

.

**Emulator Name:** Retroarch puae<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\puae_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\puae_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/puae/) or [GitHub Repository](https://github.com/libretro/libretro-uae).<br>
Core requires BIOS files or system files to work.

.

**Emulator Name:** WinUAE<br>
**Emulator Path (Example):** "C:\Emulators\WinUAE\WinUAE.exe"<br>
**Emulator Parameters (Example using relative paths):** /config "%EMULATORFOLDER%\Config.uae" /nogui /run<br>
**Emulator Parameters (Example using relative paths):** /config "%EMULATORFOLDER%\Config.uae" /nogui /floppy0<br>
**Emulator Parameters (Example using relative paths):** /config "%EMULATORFOLDER%\Config.uae" /nogui /harddrive<br>
**Emulator Parameters (Example using relative paths):** /config "%EMULATORFOLDER%\Config.uae" /nogui /cdrom<br>
**Fullscreen Parameter:** /fullscreen<br>
**Fullscreen Parameter:** /windowed<br>

This emulator is available for Windows-x64.

.

**Emulator Name:** WinFellow<br>
**Emulator Path (Example):** <br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64 and Windows-arm64.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;C:\ROMs\Commodore Amiga" a1200n<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" a1200n<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" a1000n<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" a600n<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" a500n<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" a3000<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" a2000<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" a1200<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" a1000<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" a600<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" a500<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Commodore Amiga CD32

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Commodore Amiga CD32<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** chd, cue, ccd, nrg, mds, iso<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch puae<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\puae_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\puae_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/puae/) or [GitHub Repository](https://github.com/libretro/libretro-uae).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;C:\ROMs\Commodore Amiga CD32" cd32<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" cd32<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## LaserDisk

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\LaserDisk<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** bat<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Daphne<br>
**Emulator Path:** <br>
**Emulator Parameters:** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
You need to create BAT files to launch the games.

.

**Emulator Name:** Hypseus Singe<br>
**Emulator Path:** <br>
**Emulator Parameters:** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
You need to create BAT files to launch the games.

## Magnavox Odyssey 2

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Magnavox Odyssey 2<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** bin<br>

.

**Emulator Name:** Retroarch o2em<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\o2em_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\o2em_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/o2em/).<br>
Core require BIOS files to work.

.

**Emulator Name:** O2EM<br>
**Emulator Path (Example):** "C:\Emulators\O2EM\o2em.exe"<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

## Mattel Aquarius

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Mattel Aquarius<br>
**System Is MAME?** true<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;C:\Mattel Aquarius" aquarius<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" aquarius<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Mattel Intellivision

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Mattel Intellivision<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** int, rom, bin<br>

.

**Emulator Name:** Retroarch freeintv<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\freeintv_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\freeintv_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/freeintv/).<br>
Core requires BIOS files to work.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios\;C:\Intellivision" intv<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios\;%SYSTEMFOLDER%" intv<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Microsoft DOS

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Microsoft DOS<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, 7z, rar<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** bat<br>

.

**Emulator Name:** DOSBox<br>
**Emulator Path (Example):** c:\emulators\DOSBox\DOSBox.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** DOSBox Staging<br>
**Emulator Path (Example):** c:\emulators\DOSBox Staging\dosbox.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** Retroarch dosbox_pure<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\dosbox_pure_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\dosbox_pure_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/dosbox_pure/).

.

**Emulator Name:** Retroarch dosbox<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\dosbox_core_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\dosbox_core_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/dosbox/).

.

**Emulator Name:** DOSBox-X<br>
**Emulator Path (Example):** c:\emulators\DOSBox-X\dosbox-x.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64 and Windows-arm64.<br>

## Microsoft MSX

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Microsoft MSX<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** rom, ri, mx1, mx2, col, dsk, cas, sg, sc, m3u<br>

.

**Emulator Name:** OpenMSX<br>
**Emulator Path (Example):** c:\emulators\openmsx\openmsx.exe<br>
**Emulator Parameters:** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
You can find a list of parameters for this emulator on [OpenMSX Website](https://openmsx.org/manual/commands.html).

.

**Emulator Name:** Retroarch bluemsx<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\bluemsx_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\bluemsx_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/bluemsx/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch fmsx<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\fmsx_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\fmsx_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/fmsx/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** MSXEC<br>
**Emulator Path (Example):** c:\emulators\msxex\MSXEC.exe<br>
**Emulator Parameters:** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "MSX"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Command-line options can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;C:\ROMs\Microsoft MSX" msx1<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" msx1<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Microsoft MSX2

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** rom, ri, mx1, mx2, col, dsk, cas, sg, sc, m3u<br>

.

**Emulator Name:** OpenMSX<br>
**Emulator Path (Example):** c:\emulators\openmsx\openmsx.exe<br>
**Emulator Parameters:** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
You can find a list of parameters for this emulator on [OpenMSX Website](https://openmsx.org/manual/commands.html).

.

**Emulator Name:** Retroarch bluemsx<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\bluemsx_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\bluemsx_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/bluemsx/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch fmsx<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\fmsx_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\fmsx_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/fmsx/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** MSXEC<br>
**Emulator Path (Example):** c:\emulators\msxex\MSXEC.exe<br>
**Emulator Parameters:** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "MSX2"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Command-line options can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;C:\ROMs\Microsoft MSX2" msx2<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" msx2<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Microsoft Windows

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Windows Games<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** lnk, bat, exe<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Direct Launch<br>
**Emulator Path:** <br>
**Emulator Parameters:** <br>

LNK files are shortcut files.
You can create a shortcut by right-clicking on the Game.exe and selecting 'Create Shortcut'.<br>
If you prefer to use BAT files, use the tool available in the 'Simple Launcher' menu to generate BAT files for you.

## Microsoft Xbox

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Microsoft Xbox<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** iso<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Xemu<br>
**Emulator Path (Example):** c:\emulators\xemu\xemu.exe<br>
**Emulator Parameters (Example):** -full-screen -dvd_path<br>
**Fullscreen Parameter:** -full-screen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
This emulator requires BIOS and system files to work properly.<br>
The list of required files can be found on [Xemu Website](https://xemu.app/docs/required-files/).<br>

The ISO file needs to be formated in XISO format, as the original XBOX discs.<br>
You can find a tool in the 'Simple Launcher' tools menu that can convert ISO to XISO format.

.

**Emulator Name:** Cxbx-Reloaded<br>
**Emulator Path (Example):** c:\emulators\Cxbx-Reloaded\cxbx.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
For this emulator to work in 'Simple Launcher', you need to install Dokan from [GitHub](https://github.com/dokan-dev/dokany).<br>
'Simple Launcher' has logic to mount XISO files into a virtual drive on Window and then load the file default.exe.<br>
For the logic to work; the 'Emulator Name' needs to have the word 'Cxbx' in it.<br>

The ISO file needs to be formated in XISO format, as the original XBOX discs.<br>
You can find a tool in the 'Simple Launcher' tools menu that can convert ISO to XISO format.

## Microsoft Xbox 360

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Microsoft Xbox 360<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** iso<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Xenia<br>
**Emulator Path (Example):** C:\Emulators\Xenia\xenia.exe<br>
**Emulator Parameters:** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
I recommend that you convert the original Redump ISO file to an optimized XISO file, which is smaller.<br>
You can find a tool in the 'Simple Launcher' tools menu that can optimize original Xbox ISO files.

## Microsoft Xbox 360 XBLA

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

.

**Emulator Name:** Xenia<br>
This emulator is available for Windows-x64.<br>
There are multiple ways to use this emulator. You can use [Game Folders] or ZIP files.<br>

**Option 1 - Use [Game Folders]**

**System Folder (Example):** c:\Microsoft Xbox 360 XBLA<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** bat<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Xenia<br>
**Emulator Path (Example):** <br>
**Emulator Parameters:** <br>
**Fullscreen Parameter:** <br>

Use the tool 'Create Batch Files For Xbox360 XBLA Games' in the 'Simple Launcher' tools menu.
This tool will generate BAT files for you.

**Option 2 - Use ZIP files (Recommended)**

**System Folder (Example):** c:\Microsoft Xbox 360 XBLA<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Xenia<br>
**Emulator Path (Example):** c:\emulators\Xenia\xenia_canary.exe<br>
**Emulator Parameters:** <br>
**Fullscreen Parameter:** <br>

You need to install Dokan from [GitHub](https://github.com/dokan-dev/dokany) to mount files on Windows.<br>
'Simple Launcher' will mount the ZIP file into a virtual drive, then load the game inside a nested '000D0000' folder.<br>
For the logic to work you need to add the word 'xbla' or 'xbox live' or 'live arcade' into the 'System Name'.

## NEC PC Engine / TurboGrafx 16

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\NEC PC Engine<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch mednafen_pce<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\mednafen_pce_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\mednafen_pce_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/beetle_pce_fast/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Mednafen<br>
**Emulator Path (Example):** c:\emulators\mednafen\mednafen.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
Emulator documentation can be found on [Mednafen Website](https://mednafen.github.io/documentation/).

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "PC Engine"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Command-line options can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\NEC PC Engine" pce<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" pce<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\NEC TurboGrafx 16" tg16<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" tg16<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## NEC PC Engine CD

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder:** c:\NEC PC Engine CD<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** chd, pce, cue, ccd, iso, img, bin<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch mednafen_pce<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\mednafen_pce_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\mednafen_pce_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/beetle_pce_fast/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "PC Engine CD"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Command-line options can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\NEC PC Engine CD" pcecd<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" pcecd<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## NEC PC-FX

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\NEC PC-FX<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** chd, cue, ccd, toc<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch mednafen_pcfx<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\mednafen_pcfx_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\mednafen_pcfx_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/beetle_pc_fx/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\NEC PC Engine CD" pcfx<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" pcfx<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## NEC SuperGrafx

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\NEC SuperGrafx<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch mednafen_supergrafx<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\mednafen_supergrafx_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\mednafen_supergrafx_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/beetle_sgx/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Mednafen<br>
**Emulator Path (Example):** c:\emulators\mednafen\mednafen.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
Emulator documentation can be found on [Mednafen Website](https://mednafen.github.io/documentation/).

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "SuperGrafx"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Command-line options can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).<br>
This emulator requires a BIOS file to work properly.

## Nintendo 3DS

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Nintendo 3DS<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** 3ds, cci, 3dsx, elf, axf, cxi, app<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Azahar<br>
**Emulator Path (Example):** c:\emulators\Azahar\azahar.exe<br>
**Emulator Parameters (Example):** -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** Borked3DS<br>
**Emulator Path (Example):** c:\emulators\Borked3DS\borked3ds.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** Citra<br>
**Emulator Path (Example):** c:\emulators\citra\citra-qt.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** Panda3DS<br>
**Emulator Path (Example):** c:\emulators\panda3ds\Alber.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
Documentation can be found on [GitHub](https://github.com/wheremyfoodat/Panda3DS).

.

**Emulator Name:** Retroarch citra<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\citra_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\citra_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/citra/). <br>
Core may require BIOS or system files to work properly.

.

**Emulator Name:** Retroarch panda3ds<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\panda3ds_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\panda3ds_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Documentation can be found on [GitHub](https://github.com/jonian/libretro-panda3ds) or [GitLab](https://git.libretro.com/libretro/Panda3DS).

## Nintendo 64

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Nintendo 64<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** n64, v64, z64, bin, u1, ndd, gb<br>

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Nintendo 64"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>

.

**Emulator Name:** Simple64<br>
**Emulator Path (Example):** c:\emulators\Simple64\simple64-gui.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** Retroarch mupen64plus_next<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\mupen64plus_next_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\mupen64plus_next_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/mupen64plus/).

.

**Emulator Name:** Retroarch parallel_n64_libretro<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\parallel_n64_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\parallel_n64_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>

.

**Emulator Name:** Project64<br>
**Emulator Path (Example):** c:\emulators\project64\Project64.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** BizHawk<br>
**Emulator Path (Example):** c:\emulators\BizHawk\EmuHawk.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>

.

**Emulator Name:** Rosalie Mupen GUI<br>
**Emulator Path (Example):** c:\emulators\Rosalie Mupen GUI\RMG.exe<br>
**Emulator Parameters (Example):** --fullscreen<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** Gopher64<br>
**Emulator Path (Example):** c:\emulators\Gopher64\gopher64-windows-x86_64.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;C:\ROMs\Nintendo 64" n64<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" n64<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Nintendo 64DD

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Nintendo 64DD<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** n64, v64, z64, bin, u1, ndd, gb<br>

.

**Emulator Name:** Retroarch mupen64plus_next<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\mupen64plus_next_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\mupen64plus_next_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/mupen64plus/).

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Nintendo 64DD"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;C:\ROMs\Nintendo 64DD" n64dd<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" n64dd<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Nintendo DS

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Nintendo DS<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** nds<br>

.

**Emulator Name:** Retroarch melonds<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\melonds_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\melonds_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/melonds/).

.

**Emulator Name:** Retroarch desmume<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\desmume_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\desmume_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/desmume/).

.

**Emulator Name:** melonDS<br>
**Emulator Path (Example):** c:\emulators\melonDS\melonDS.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Emulator repository available on [GitHub](https://github.com/melonDS-emu/melonDS).

.

**Emulator Name:** DeSmuME<br>
**Emulator Path (Example):** c:\emulators\DeSmuME\DeSmuME.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
Emulator repository available on [GitHub](https://github.com/TASEmulators/desmume).

## Nintendo Family Computer Disk System

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Nintendo Family Computer Disk System<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** nes, fds, unf, unif, qd<br>

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Famicom Disk System"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** Retroarch mesen<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\mesen_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\mesen_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/mesen/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch nestopia<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\nestopia_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\nestopia_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/nestopia_ue/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Mesen<br>
**Emulator Path (Example):** c:\emulators\mesen\Mesen.exe<br>
**Emulator Parameters (Example):** --fullscreen<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64.<br>
This emulator requires a BIOS file to run this system.

## Nintendo Game Boy

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Nintendo Game Boy<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** c:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Game Boy"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** Retroarch sameboy<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\sameboy_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\sameboy_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/sameboy/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch gambatte<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\gambatte_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\gambatte_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/gambatte/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch tgbdual<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\tgbdual_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\tgbdual_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/tgb_dual/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch gearboy<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\gearboy_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\gearboy_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/gearboy/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** JGenesis<br>
**Emulator Path (Example):** c:\emulators\JGenesis\jgenesis-cli.exe<br>
**Emulator Parameters (Example):** --file-path<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios\;C:\Game Boy" gameboy<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios\;%SYSTEMFOLDER%" gameboy<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Nintendo Game Boy Advance

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Nintendo Game Boy Advance<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch mgba<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\mgba_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\mgba_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/mgba/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Hades<br>
**Emulator Path (Example):** c:\emulators\Hades\Hades.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
Emulator documentation can be found on [GitHub](https://github.com/hades-emu/hades).<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** VisualBoy Advance M<br>
**Emulator Path (Example):** c:\emulators\VisualBoy Advance M\visualboyadvance-m.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64 and Windows-arm64.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** c:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Game Boy Advance"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\Nintendo Game Boy Advance" gba<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" gba<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Nintendo Game Boy Color

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Nintendo Game Boy Color<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch sameboy<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\sameboy_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\sameboy_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/sameboy/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch gambatte<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\gambatte_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\gambatte_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/gambatte/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch tgbdual<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\tgbdual_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\tgbdual_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/tgb_dual/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch gearboy<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\gearboy_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\gearboy_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/gearboy/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** JGenesis<br>
**Emulator Path (Example):** c:\emulators\JGenesis\jgenesis-cli.exe<br>
**Emulator Parameters (Example):** --file-path<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** c:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Game Boy Color"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios\;C:\Game Boy Color" gbcolor<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios\;%SYSTEMFOLDER%" gbcolor<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Nintendo GameCube

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Nintendo GameCube<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** rvz<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Dolphin<br>
**Emulator Path (Example):** c:\emulators\dolphin\Dolphin.exe<br>
**Emulator Parameters:** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
It supports RetroAchievements.<br>

.

**Emulator Name:** Retroarch dolphin<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\dolphin_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\dolphin_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/dolphin/).<br>
Core may require BIOS files or system files to work properly.

## Nintendo NES

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Nintendo NES<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch mesen<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\mesen_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\mesen_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/mesen/).

.

**Emulator Name:** Retroarch nestopia<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\nestopia_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\nestopia_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/nestopia_ue/).

.

**Emulator Name:** Mednafen<br>
**Emulator Path (Example):** c:\emulators\mednafen\mednafen.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
Emulator documentation can be found on [Mednafen Website](https://mednafen.github.io/documentation/).

.

**Emulator Name:** JGenesis<br>
**Emulator Path (Example):** c:\emulators\JGenesis\jgenesis-cli.exe<br>
**Emulator Parameters (Example):** --file-path<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** Mesen<br>
**Emulator Path (Example):** c:\emulators\mesen\Mesen.exe<br>
**Emulator Parameters (Example):** --fullscreen<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** c:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Famicom"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\Nintendo NES" nes<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" nes<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Nintendo Satellaview

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Nintendo Satellaview<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Super Famicom"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Command-line options can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).

## Nintendo SNES / Super Nintendo / Super Famicom

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Nintendo SNES<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** smc, sfc, swc, fig, bs, st<br>

.

**Emulator Name:** Retroarch snes9x<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\snes9x_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\snes9x_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/snes9x/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch bsnes<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\bsnes_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\bsnes_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/beetle_bsnes/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Snes9x<br>
**Emulator Path (Example):** c:\emulators\snes9x\snes9x-x64.exe<br>
**Emulator Parameters (Example):** -fullscreen<br>
**Fullscreen Parameter:** -fullscreen<br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** JGenesis<br>
**Emulator Path (Example):** c:\emulators\JGenesis\jgenesis-cli.exe<br>
**Emulator Parameters (Example):** --file-path<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Super Famicom"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Command-line options can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\Nintendo SNES" snes<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" snes<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Nintendo SNES MSU1

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder:** c:\Nintendo SNES MSU1<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** smc, sfc, swc, fig, bs, st<br>

.

**Emulator Name:** Retroarch snes9x<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\snes9x_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\snes9x_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/snes9x/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Snes9x<br>
**Emulator Path (Example):** c:\emulators\snes9x\snes9x-x64.exe<br>
**Emulator Parameters (Example):** -fullscreen<br>
**Fullscreen Parameter:** -fullscreen<br>

This emulator is available for Windows-x64.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Super Famicom"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Command-line options can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).

## Nintendo Switch

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Nintendo Switch<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** nsp, xci, nca, nro, nso<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Yuzu Original<br>
**Emulator Path (Example):** C:\Users\HomePC\AppData\Local\yuzu\yuzu-windows-msvc\yuzu.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** Yuzu Fork Citron<br>
**Emulator Path (Example):** c:\emulators\Yuzu Fork Citron\citron.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** Yuzu Fork Eden<br>
**Emulator Path (Example):** c:\emulators\Yuzu Fork Eden\eden.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** Yuzu Fork Sudachi<br>
**Emulator Path (Example):** c:\emulators\Yuzu Fork Sudachi\sudachi.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** Ryujinx Original<br>
**Emulator Path (Example):** c:\emulators\Ryujinx\Ryujinx.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** Ryujinx Fork Ryubing<br>
**Emulator Path (Example):** c:\emulators\Ryujinx Fork Ryubing\Ryujinx.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** Ryujinx Fork Kenji-NX<br>
**Emulator Path (Example):** c:\emulators\Ryujinx Fork Kenji-Nx\Ryujinx.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.

## Nintendo Virtual Boy

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Nintendo Virtual Boy<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\Nintendo Virtual Boy" vboy<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" vboy<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Nintendo Wii

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Nintendo Wii<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** rvz, elf, iso, gcm, dol, tgc, wbfs, ciso, gcz, wad<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Dolphin<br>
**Emulator Path (Example):** c:\emulators\dolphin\Dolphin.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
It supports RetroAchievements.<br>

.

**Emulator Name:** Retroarch dolphin<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\dolphin_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\dolphin_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/dolphin/).<br>
Core may require BIOS files or system files to work properly.

## Nintendo WiiU

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Nintendo WiiU<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** wua<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Cemu<br>
**Emulator Path (Example):** c:\emulators\cemu\cemu.exe<br>
**Emulator Parameters (Example):** -f -g<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.

## Nintendo WiiWare

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Nintendo WiiWare<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** wad<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Dolphin<br>
**Emulator Path (Example):** c:\emulators\dolphin\Dolphin.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
It supports RetroAchievements.<br>

.

**Emulator Name:** Retroarch dolphin<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\dolphin_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\dolphin_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/dolphin/).<br>
Core may require BIOS files or system files to work properly.

## Panasonic 3DO

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Panasonic 3DO<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** iso, bin, chd, cue<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch opera<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\opera_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\opera_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/opera/).<br>
Core may require BIOS files or system files to work properly.

## Philips CD-i

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Philips CD-i<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** chd, iso<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch same_cdi<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\same_cdi_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\same_cdi_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/same_cdi/).<br>
Core may require BIOS files or system files to work properly.

## ScummVM

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

.

**Emulator Name:** ScummVM<br>
This emulator is available for Windows-x64 and Windows-arm64.<br>
There are multiple ways to use this program. You can use [Game Folders] or ZIP files.<br>

**Option 1 - Use [Game Folders]**

**System Folder (Example):** c:\ScummVM<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** bat<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** ScummVM<br>
**Emulator Path:** <br>
**Emulator Parameters:** <br>
**Fullscreen Parameter:** <br>

Use the tool available in the 'Simple Launcher' tools menu to generate BAT files for you.

**Option 2 - Use ZIP files**

**System Folder (Example):** c:\ScummVM<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** ScummVM<br>
**Emulator Path:** c:\Emulators\ScummVM\scummvm.exe<br>
**Emulator Parameters:** --auto-detect --fullscreen<br>
**Fullscreen Parameter:** --fullscreen<br>

You need to install Dokan from [GitHub](https://github.com/dokan-dev/dokany) to mount files on Windows.<br>
'Simple Launcher' will mount the ZIP file into a virtual drive, then load the game using a custom logic.<br>
For the logic to work you need to add the word 'ScummVM' or 'Scumm-VM' or 'Scumm' into the 'System Name'

Command line parameters can be found on [ScummVM Website](https://scumm-thedocs.readthedocs.io/en/latest/advanced/command_line.html#command-line-interface).

## Sega Dreamcast

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Sega Dreamcast<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** chd, gdi, cue, bin, cdi<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Redream<br>
**Emulator Path (Example):** c:\emulators\redream\redream.exe<br>
**Emulator Parameters:** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** Retroarch flycast<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\flycast_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\flycast_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/flycast/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Flycast<br>
**Emulator Path (Example):** c:\emulators\Flycast\flycast.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.
It supports RetroAchievements.<br>

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\Sega Dreamcast" dc<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" dc<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Sega Game Gear

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Sega Game Gear<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch genesis_plus_gx<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\genesis_plus_gx_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\genesis_plus_gx_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/genesis_plus_gx/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** MasterGear<br>
**Emulator Path (Example):** c:\emulators\mastergear\MG.exe<br>
**Emulator Parameters:** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
The list of commands available for this emulator can be found on [MasterGear Website](https://fms.komkon.org/MG/MG.html).

.

**Emulator Name:** Kega Fusion<br>
**Emulator Path (Example):** c:\emulators\Kega Fusion\Fusion.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** JGenesis<br>
**Emulator Path (Example):** c:\emulators\JGenesis\jgenesis-cli.exe<br>
**Emulator Parameters (Example):** --file-path<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** Emulicious<br>
**Emulator Path (Example):** c:\emulators\Emulicious\Emulicious.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** GearSystem<br>
**Emulator Path (Example):** c:\emulators\GearSystem\Gearsystem.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64 and Windows-arm64.<br>

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Game Gear"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
The list of command-line arguments available for this emulator can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\Sega Game Gear" gamegear<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" gamegear<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Sega Genesis / Mega Drive

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Sega Genesis<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch picodrive<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\picodrive_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\picodrive_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/picodrive/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch genesis_plus_gx<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\genesis_plus_gx_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\genesis_plus_gx_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/genesis_plus_gx/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch blastem<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\blastem_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\blastem_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/blastem/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Kega Fusion<br>
**Emulator Path (Example):** c:\emulators\Kega Fusion\Fusion.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** JGenesis<br>
**Emulator Path (Example):** c:\emulators\JGenesis\jgenesis-cli.exe<br>
**Emulator Parameters (Example):** --file-path<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** c:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Mega Drive"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
The list of command-line arguments available for this emulator can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\Sega Genesis" megadriv<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" megadriv<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Sega Genesis 32X / Mega Drive 32X

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Sega Genesis 32X<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch picodrive<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\picodrive_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\picodrive_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/picodrive/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Kega Fusion<br>
**Emulator Path (Example):** c:\emulators\Kega Fusion\Fusion.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** JGenesis<br>
**Emulator Path (Example):** c:\emulators\JGenesis\jgenesis-cli.exe<br>
**Emulator Parameters (Example):** --file-path<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** c:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Mega 32X"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
The list of command-line arguments available for this emulator can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\Sega Genesis 32x" 32x<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" 32x<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Sega Genesis CD / Mega Drive CD

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Sega Genesis CD<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** chd, bin, cue, iso<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch picodrive<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\picodrive_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\picodrive_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/picodrive/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch genesis_plus_gx<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\genesis_plus_gx_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\genesis_plus_gx_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/genesis_plus_gx/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** JGenesis<br>
**Emulator Path (Example):** c:\emulators\JGenesis\jgenesis-cli.exe<br>
**Emulator Parameters (Example):** --file-path<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** c:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Mega CD"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
The list of command-line arguments available for this emulator can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\Sega Genesis CD" megacd<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" megacd<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Sega Master System / Mark3

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Sega Master System<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch genesis_plus_gx<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\genesis_plus_gx_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\genesis_plus_gx_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/genesis_plus_gx/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** MasterGear<br>
**Emulator Path (Example):** c:\emulators\mastergear\MG.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
The list of command-line arguments available for this emulator can be found on [MasterGear Website](https://fms.komkon.org/MG/MG.html).

.

**Emulator Name:** Kega Fusion<br>
**Emulator Path (Example):** c:\emulators\Kega Fusion\Fusion.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** JGenesis<br>
**Emulator Path (Example):** c:\emulators\JGenesis\jgenesis-cli.exe<br>
**Emulator Parameters (Example):** --file-path<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** GearSystem<br>
**Emulator Path (Example):** c:\emulators\GearSystem\Gearsystem.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64 and Windows-arm64.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** c:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Master System"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
The list of command-line arguments available for this emulator can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\Sega Master System" sms<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" sms<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Sega Model 3

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Sega Model 3<br>
**System Is MAME?** true<br>
**Format To Search In System Folder:** bat<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Supermodel<br>
**Emulator Path (Example):** <br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
You can find a tool in the 'Simple Launcher' tools menu that can generate BAT files for you.

## Sega Saturn

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Sega Saturn<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** chd, cue, toc, m3u, ccd<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch mednafen_saturn<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\mednafen_saturn_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\mednafen_saturn_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/beetle_saturn/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch kronos<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\kronos_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\kronos_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/kronos/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch yabasanshiro<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\yabasanshiro_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\yabasanshiro_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/yabasanshiro/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch yabause<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\yabause_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\yabause_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/yabause/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Mednafen<br>
**Emulator Path (Example):** c:\emulators\mednafen\mednafen.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
Emulator documentation can be found on [Mednafen Website](https://mednafen.github.io/documentation/).

.

**Emulator Name:** SSF<br>
**Emulator Path (Example):** c:\emulators\SSF\SSF64.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
Emulator documentation can be found on [Emulation General Wiki](https://emulation.gametechwiki.com/index.php/SSF).

.

**Emulator Name:** Ymir<br>
**Emulator Path (Example):** c:\emulators\Ymir\ymir-sdl3.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64 and Windows-arm64.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\Sega Saturn" saturn<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" saturn<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Sega SC-3000

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Sega SC-3000<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** sms, gg, sg, sc, sf, dsk, cht<br>

.

**Emulator Name:** BizHawk<br>
**Emulator Path (Example):** c:\emulators\BizHawk\EmuHawk.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>

.

**Emulator Name:** MasterGear<br>
**Emulator Path (Example):** c:\emulators\MasterGear\MG.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
The list of command-line arguments available for this emulator can be found on [MasterGear Website](https://fms.komkon.org/MG/MG.html).

.

**Emulator Name:** Kega Fusion<br>
**Emulator Path (Example):** c:\emulators\Kega Fusion\Fusion.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** c:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "SC-3000"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
The list of command-line arguments available for this emulator can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\Sega SC-3000" sc3000<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" sc3000<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Sega SG-1000

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Sega SG-1000<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** sms, gg, sg, sc, sf, dsk, cht<br>

.

**Emulator Name:** BizHawk<br>
**Emulator Path (Example):** c:\emulators\BizHawk\EmuHawk.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>

.

**Emulator Name:** MasterGear<br>
**Emulator Path (Example):** c:\emulators\MasterGear\MG.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
The list of command-line arguments available for this emulator can be found on [MasterGear Website](https://fms.komkon.org/MG/MG.html).

.

**Emulator Name:** Kega Fusion<br>
**Emulator Path (Example):** c:\emulators\Kega Fusion\Fusion.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>

.

**Emulator Name:** GearSystem<br>
**Emulator Path (Example):** c:\emulators\GearSystem\Gearsystem.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64 and Windows-arm64.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** c:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "SG-1000"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
The list of command-line arguments available for this emulator can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\Sega SG-1000" sg1000<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" sg1000<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Sharp x68000

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Sharp x68000<br>
**System Is MAME?** true<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch px68k<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\px68k_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\px68k_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [libretro Website](https://docs.libretro.com/library/px68k/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** c:\emulators\mame\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "c:\emulators\mame\roms;c:\emulators\mame\bios;c:\Sharp X68000" x68000 -flop1<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" x68000 -flop1<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Sinclair ZX Spectrum

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Sinclair ZX Spectrum<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** tzx, tap, z80, rzx, scl, trd, ipf<br>

.

**Emulator Name:** Retroarch fuse<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\fuse_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\fuse_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/fuse/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Speccy<br>
**Emulator Path (Example):** c:\emulators\Speccy\Speccy.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:**

This emulator is available for Windows-x64.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** c:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "ZX Spectrum"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
The list of command-line arguments available for this emulator can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** c:\emulators\mame\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\emulators\mame\roms;C:\emulators\mame\bios;C:\ZX Spectrum" spectrum<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" spectrum<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## SNK Neo Geo

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\SNK Neo Geo<br>
**System Is MAME?** true<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\Roms;C:\Emulators\MAME\Bios;c:\ROMs\NeoGeo\"<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%"<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** c:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Neo Geo AES"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
The list of command-line arguments available for this emulator can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).

## SNK Neo Geo CD

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\SNK Neo Geo CD<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** chd, cue<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch neocd<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\neocd_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\neocd_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [NeoCD Repository](https://github.com/libretro/neocd_libretro).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\SNK Neo Geo CD" neocd<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" neocd<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## SNK Neo Geo Pocket

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\SNK Neo Geo Pocket<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch mednafen_ngp<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\mednafen_ngp_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\mednafen_ngp_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/beetle_neopop/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch race<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\race_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\race_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/race/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Mednafen<br>
**Emulator Path (Example):** c:\emulators\mednafen\mednafen.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
Emulator documentation can be found on [Mednafen Website](https://mednafen.github.io/documentation/).

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Neo Geo Pocket"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Command-line options can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\SNK Neo Geo Pocket" ngp<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" ngp<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## SNK Neo Geo Pocket Color

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\SNK Neo Geo Pocket Color<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Retroarch mednafen_ngp<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\mednafen_ngp_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\mednafen_ngp_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/beetle_neopop/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch race<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\race_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\race_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/race/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Mednafen<br>
**Emulator Path (Example):** c:\emulators\mednafen\mednafen.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.<br>
Emulator documentation can be found on [Mednafen Website](https://mednafen.github.io/documentation/).

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "Neo Geo Pocket Color"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Command-line options can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\SNK Neo Geo Pocket Color" ngpc<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" ngpc<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Sony PlayStation 1

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Sony PlayStation 1<br>
**System Is MAME?** false (may be true if you are using a MAME compatible ROM set)<br>
**Format To Search In System Folder:** chd, cue, bin, img, mds, mdf, pbp<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** DuckStation<br>
**Emulator Path (Example):** C:\emulators\duckstation\duckstation-qt-x64-ReleaseLTCG.exe<br>
**Emulator Parameters (Example):** -fullscreen<br>
**Fullscreen Parameter:** -fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
It supports RetroAchievements.<br>
Emulator documentation can be found on [DuckStation Repository](https://github.com/stenzek/duckstation).<br>
Emulator may need BIOS or system files to work properly.

.

**Emulator Name:** Retroarch mednafen_psx<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\mednafen_psx_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\mednafen_psx_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/beetle_psx/).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Retroarch swanstation<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\swanstation_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\swanstation_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [GitHub](https://github.com/libretro/swanstation).<br>
Core may require BIOS files or system files to work properly.

.

**Emulator Name:** Ares<br>
**Emulator Path (Example):** C:\emulators\ares\ares-v146\ares.exe<br>
**Emulator Parameters (Example):** --fullscreen --system "PlayStation"<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Command-line options can be found on [ares Repository](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).<br>
This emulator requires a BIOS file to work properly.

.

**Emulator Name:** MAME<br>
**Emulator Path (Example):** C:\Emulators\MAME\mame.exe<br>
**Emulator Parameters (Example using absolute paths):** -rompath "C:\Emulators\MAME\roms;C:\Emulators\MAME\bios;c:\ROMs\Sony PlayStation 1" psx<br>
**Emulator Parameters (Example using relative paths):** -rompath "%EMULATORFOLDER%\roms;%EMULATORFOLDER%\bios;%SYSTEMFOLDER%" psx<br>
**Fullscreen Parameter:** -window (will load in windowed mode)<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
Please use the exact Emulator Name provided above.<br>
To use this emulator you need the use a MAME compatible set of ROMs, with the right filenames.<br>
You can find the right set of ROMs on the [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html) website. I am not affiliated with PleasureDome in any way.

## Sony PlayStation 2

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Sony PlayStation 2<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** chd, bin, iso, mdf, cso, zso, gz<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** PCSX2<br>
**Emulator Path (Example):** c:\emulators\pcsx2\pcsx2-qt.exe<br>
**Emulator Parameters (Example):** -fullscreen<br>
**Fullscreen Parameter:** -fullscreen<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Documentation can be found on [PCSX2 Website](https://pcsx2.net/docs/).<br>
This emulator may require BIOS or system files to work properly.

.

**Emulator Name:** Retroarch pcsx2<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example using absolute paths):** -L "c:\emulators\retroarch\cores\pcsx2_libretro.dll" -f<br>
**Emulator Parameters (Example using relative paths):** -L "%EMULATORFOLDER%\cores\pcsx2_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/pcsx2/).<br>
Core may require BIOS files or system files to work properly.

## Sony PlayStation 3

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)<br>

.

**Emulator Name:** RPCS3<br>
This emulator is available for Windows-x64.<br>
There are multiple ways to use this emulator. You can use [Game Folders], ZIP files or ISO files.<br>

**Option 1 - Use [Game Folders]**

**System Folder (Example):** c:\Sony PlayStation 3<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** bat<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** RPCS3<br>
**Emulator Path (Example):** <br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

Use the tool available in the 'Simple Launcher' Tools menu to generate BAT files for you.<br>

**Option 2 - Use ZIP files**

**System Folder (Example):** c:\Sony PlayStation 3<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** RPCS3<br>
**Emulator Path (Example):** C:\Emulators\RPCS3\rpcs3.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

You need to install Dokan from [GitHub](https://github.com/dokan-dev/dokany) to mount files on Windows.<br>
'Simple Launcher' will mount the ZIP file into a virtual drive, then load the game using a custom logic.<br>
For the logic to work, you need to add the word 'RPCS3' into the 'Emulator Name'.

**Option 3 — Use decrypted ISO files**

**System Folder (Example):** c:\Sony PlayStation 3<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** iso<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** RPCS3<br>
**Emulator Path (Example):** C:\Emulators\RPCS3\rpcs3.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

'Simple Launcher' will mount the ISO file using native Microsoft Windows capabilities, then load the game using a custom logic.<br>
For the logic to work, you need to add the word 'RPCS3' into the 'Emulator Name'.

## Sony PlayStation 4

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Sony PlayStation 4<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** bin, elf, oelf<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** shadPS4<br>
**Emulator Path (Example):** c:\emulators\shadPS4\shadPS4.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.

## Sony PlayStation Vita

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Sony PlayStation Vita<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** iso<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** Vita3K<br>
**Emulator Path (Example):** c:\emulators\Vita3K\Vita3K.exe<br>
**Emulator Parameters (Example):** <br>
**Fullscreen Parameter:** <br>

This emulator is available for Windows-x64.

## Sony PSP

**Double-check file and folder paths when entering them in Simple Launcher.**<br>

**If you want to use relative paths, use the following placeholders:**<br>
**%BASEFOLDER%** - Represents the Simple Launcher folder<br>
**%SYSTEMFOLDER%** - Represents the System Folder (where the ROMs or ISOs are)<br>
**%EMULATORFOLDER%** - Represents the Emulator Folder (where the emulator .exe is)

**System Folder (Example):** c:\Sony PSP<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** cso, chd, iso, pbp, elf, prx<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

.

**Emulator Name:** PPSSPP<br>
**Emulator Path (Example):** c:\emulators\ppsspp\PPSSPPWindows64.exe<br>
**Emulator Parameters (Example):** --fullscreen<br>
**Fullscreen Parameter:** --fullscreen<br>

This emulator is available for Windows-x64 and Windows-arm64.<br>
It supports RetroAchievements.<br>

.

**Emulator Name:** Retroarch ppsspp<br>
**Emulator Path (Example):** c:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters (Example):** -L "c:\emulators\retroarch\cores\ppsspp_libretro.dll" -f<br>
**Fullscreen Parameter:** -f<br>

This emulator is available for Windows-x64.<br>
It supports RetroAchievements.<br>
Core documentation can be found on [Libretro Website](https://docs.libretro.com/library/ppsspp/).<br>
Core may require BIOS files or system files to work properly.