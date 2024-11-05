# List of Parameters to use in the "system.xml"

## Amstrad CPC

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch caprice32<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\cap32_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/caprice32/). Please read core documentation.

## Amstrad CPC GX4000

**System Folder:** [ROM Folder]<br>
**System Is MAME?** true<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** MAME gx4000<br>
**Emulator Location:** [MAME Folder]\mame.exe<br>
**Emulator Location (Example):** C:\emulators\mame\mame.exe<br>
**Emulator Parameters:** gx4000 -cart

## Arcade (MAME)

**System Folder:** [ROM Folder]<br>
**System Is MAME?** true<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** MAME<br>
**Emulator Location:** [MAME Folder]\mame.exe<br>
**Emulator Location (Example):** C:\emulators\mame\mame.exe<br>
**Emulator Parameters:** -rompath "[ROM Folder]"<br>
**Emulator Parameters (Example):** -rompath "c:\mame\roms"<br>

**Emulator Name:** Retroarch mame<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\mame_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/mame_2010/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

## Atari 2600

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch stella<br>
**Emulator Location:** [Retroarch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\stella_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/stella/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

**Emulator Name:** Stella<br>
**Emulator Location:** [Stella Folder]\Stella.exe<br>
**Emulator Location (Example):** C:\emulators\stella\Stella.exe<br>
**Emulator Parameters:** -fullscreen 1<br>

Command line documentation can be found at [Stella website](https://stella-emu.github.io/docs/index.html#CommandLine).

## Atari 5200

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Altirra<br>
**Emulator Location:** [Altirra Folder]\Altirra64.exe<br>
**Emulator Location (Example):** C:\emulators\altirra\Altirra64.exe<br>
**Emulator Parameters:** /f<br>

## Atari 7800

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch prosystem<br>
**Emulator Location:** [Retroarch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\prosystem_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/prosystem/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

## Atari 8-Bit

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Altirra<br>
**Emulator Location:** [Altirra Folder]\Altirra64.exe<br>
**Emulator Location (Example):** C:\emulators\altirra\Altirra64.exe<br>
**Emulator Parameters:** /f<br>

## Atari Jaguar

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** BigPEmu<br>
**Emulator Location:** [BigPEmu Folder]\BigPEmu.exe<br>
**Emulator Location (Example):** C:\emulators\bigpemu\BigPEmu.exe<br>
**Emulator Parameters:** <br>

## Atari Jaguar CD

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, cdi<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** cue, cdi<br>

**Emulator Name:** BigPEmu<br>
**Emulator Location:** [BigPEmu Folder]\BigPEmu.exe<br>
**Emulator Location (Example):** C:\emulators\bigpemu\BigPEmu.exe<br>
**Emulator Parameters:** <br>

## Atari Lynx

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** lnx, o<br>

**Emulator Name:** Retroarch mednafen_lynx<br>
**Emulator Location:** [Retroarch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\mednafen_lynx_libretro.dll" -f<br>

Core require a BIOS file to run.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/beetle_lynx/). Please read core documentation.

## Atari ST

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, msa, st, stx, dim, ipf<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch hatari<br>
**Emulator Location:** [Retroarch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\hatari_libretro.dll" -f<br>

Core requires BIOS file to run. Core documentation can be found at [Libretro website](https://docs.libretro.com/library/hatari/). Please read core documentation.

**Emulator Name:** Hatari<br>
**Emulator Location:** [Hatari Folder]\hatari.exe<br>
**Emulator Location (Example):** C:\emulators\hatari\hatari.exe<br>
**Emulator Parameters:** <br>

Emulator requires a BIOS file to run. Emulator documentation can be found at [GitHub website](https://github.com/hatari/hatari). Please read emulator documentation. 

## Bandai WonderSwan

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch mednafen_wswan<br>
**Emulator Location:** [Retroarch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\mednafen_wswan_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/beetle_cygne/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

**Emulator Name:** BizHawk<br>
**Emulator Location:** [BizHawk Folder]\EmuHawk.exe<br>
**Emulator Location (Example):** C:\emulators\emuhawk\EmuHawk.exe<br>
**Emulator Parameters:** <br>

## Bandai WonderSwan Color

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch mednafen_wswan<br>
**Emulator Location:** [Retroarch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\mednafen_wswan_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/beetle_cygne/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

**Emulator Name:** BizHawk<br>
**Emulator Location:** [BizHawk Folder]\EmuHawk.exe<br>
**Emulator Location (Example):** C:\emulators\emuhawk\EmuHawk.exe<br>
**Emulator Parameters:** <br>

## Casio PV-1000

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** true<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** MAME pv1000<br>
**Emulator Location:** [MAME Folder]\mame.exe<br>
**Emulator Location (Example):** C:\emulators\mame\mame.exe<br>
**Emulator Parameters:** pv1000 -cart<br>

## Colecovision

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** col, cv, bin, rom<br>

**Emulator Name:** Retroarch gearcoleco<br>
**Emulator Location:** [Retroarch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\gearcoleco_libretro.dll" -f<br>

Core requires a BIOS file to run.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/gearcoleco/).
Please read core documentation.

## Commodore 64

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch vice_x64<br>
**Emulator Location:** [Retroarch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\vice_x64_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/vice/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

## Commodore Amiga CD32

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** chd, cue, ccd, nrg, mds, iso<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch puae<br>
**Emulator Location:** [Retroarch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\puae_libretro.dll" -f<br>

Core requires a BIOS file to run.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/puae/) or at [GitHub Repository](https://github.com/libretro/libretro-uae).
Please read core documentation.

## LaserDisk

**System Folder:** [BAT Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** bat<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Daphne<br>
**Emulator Location:** <br>
**Emulator Parameters:** <br>

You need to create a BAT file that can directly launch the game.<br>
Try launching your batch file directly outside of Simple Launcher first.
If it works there, it will work inside Simple Launcher.

## Magnavox Odyssey 2

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** bin<br>

**Emulator Name:** Retroarch o2em<br>
**Emulator Location:** [Retroarch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\o2em_libretro.dll" -f<br>

Core requires a BIOS file to run. Core documentation can be found at [Libretro website](https://docs.libretro.com/library/o2em/). Please read core documentation.

## Mattel Aquarius

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** true<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** AquaLite<br>
**Emulator Location:** [AquaLite Folder]\AquaLite.exe<br>
**Emulator Location (Example):** C:\emulators\aqualite\AquaLite.exe<br>
**Emulator Parameters:** <br>

## Mattel Intellivision

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** int, rom, bin<br>

**Emulator Name:** Retroarch freeintv<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\freeintv_libretro.dll" -f<br>

Core requires a BIOS file to run.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/freeintv/).
Please read the documentation.

## Microsoft DOS

**System Folder:** [ZIP Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch dosbox_pure<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\dosbox_pure_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/dosbox_pure/).
Please read core documentation.<br>
Compress the game folder to a ZIP file, then put it inside the System Folder.

**Emulator Name:** Retroarch dosbox<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\dosbox_core_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/dosbox/).
Please read core documentation.<br>
Compress the game folder to a ZIP file, then put it inside the System Folder.

## Microsoft MSX

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** rom, ri, mx1, mx2, col, dsk, cas, sg, sc, m3u<br>

**Emulator Name:** OpenMSX<br>
**Emulator Location:** [OpenMSX Folder]\openmsx.exe<br>
**Emulator Location (Example):** C:\emulators\OpenMSX\openmsx.exe<br>
**Emulator Parameters:** <br>

You can find a list of all parameters available for this emulator at [OpenMSX Website](https://openmsx.org/manual/commands.html).

**Emulator Name:** Retroarch bluemsx<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\bluemsx_libretro.dll" -f<br>

Core requires BIOS or Other Files to run.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/bluemsx/).
Please read core documentation.

**Emulator Name:** Retroarch fmsx<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\fmsx_libretro.dll" -f<br>

Core requires BIOS or Other Files to run.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/fmsx/).
Please read core documentation.

## Microsoft MSX2

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** rom, ri, mx1, mx2, col, dsk, cas, sg, sc, m3u<br>

**Emulator Name:** OpenMSX<br>
**Emulator Location:** [OpenMSX Folder]\openmsx.exe<br>
**Emulator Location (Example):** C:\emulators\OpenMSX\openmsx.exe<br>
**Emulator Parameters:** <br>

You can find a list of all parameters available for this emulator at [OpenMSX Website](https://openmsx.org/manual/commands.html).

**Emulator Name:** Retroarch bluemsx<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\bluemsx_libretro.dll" -f<br>

Core requires BIOS or Other Files to run.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/bluemsx/).
Please read core documentation.

**Emulator Name:** Retroarch fmsx<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\fmsx_libretro.dll" -f<br>

Core requires BIOS or Other Files to run.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/fmsx/).
Please read core documentation.

## Microsoft Windows

**System Folder:** [BAT Folder] or [LNK Folder] or [EXE Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** lnk, bat, exe<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Direct Launch<br>
**Emulator Location:** <br>
**Emulator Parameters:** <br>

LNK files are shortcut links.
You can create a shortcut by right-clicking on the GameLauncher.exe and selecting 'Create Shortcut'.<br>

If you prefer to use BAT files, use the tool available in the Simple Launcher menu to generate BAT files for you.

## Microsoft Xbox

**System Folder:** [ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** iso<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Xemu<br>
**Emulator Location:** [Xemu Folder]\xemu.exe<br>
**Emulator Location (Example):** C:\emulators\xemu\xemu.exe<br>
**Emulator Parameters:** -full-screen -dvd_path<br>

This emulator requires BIOS and Special Files to run properly.
The list of required files can be found [here](https://xemu.app/docs/required-files/).<br>
The ISO file needs to be formated in XBOX format, as the original XBOX discs.
You can find information about that [here](https://xemu.app/docs/disc-images/).

## Microsoft Xbox 360

**System Folder:** [ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** iso<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Xenia<br>
**Emulator Location:** [Xenia Folder]\xenia.exe<br>
**Emulator Location (Example):** C:\emulators\xenia\xenia.exe<br>
**Emulator Parameters:** <br>

## Microsoft Xbox 360 XBLA

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, 7z, rar<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** bin<br>

**Emulator Name:** Xenia<br>
**Emulator Location:** [Xenia Folder]\xenia.exe<br>
**Emulator Location (Example):** C:\emulators\xenia\xenia.exe<br>
**Emulator Parameters:** <br>

## NEC PC Engine / TurboGrafx-16

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch mednafen_pce<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\mednafen_pce_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/beetle_pce_fast/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

## NEC PC Engine CD

**System Folder:** [ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** chd, pce, cue, ccd, iso, img, bin<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch mednafen_pce<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\mednafen_pce_libretro.dll" -f<br>

This core requires a BIOS file to run.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/beetle_pce_fast/).
Please read core documentation.

**Emulator Name:** ares<br>
**Emulator Location:** [ares Folder]\ares.exe<br>
**Emulator Location (Example):** C:\emulators\ares\ares-v138\ares.exe<br>
**Emulator Parameters:** --system "PC Engine CD"<br>

Command-line options can be found at [ares GitHub](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).

## NEC PC-FX

**System Folder:** [ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** chd, cue, ccd, toc<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch mednafen_pcfx<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\mednafen_pcfx_libretro.dll" -f<br>

This core requires a BIOS file.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/beetle_pc_fx/).
Please read core documentation.

**Emulator Name:** BizHawk<br>
**Emulator Location:** [BizHawk Folder]\EmuHawk.exe<br>
**Emulator Location (Example):** C:\emulators\bizhawk\EmuHawk.exe<br>
**Emulator Parameters:** <br>

## NEC SuperGrafx

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch mednafen_supergrafx<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\mednafen_supergrafx_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/beetle_sgx/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

## Nintendo 3DS

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** 3ds<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch citra<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\citra_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/citra/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

**Emulator Name:** Citra<br>
**Emulator Location:** [Citra Folder]\citra-qt.exe<br>
**Emulator Location (Example):** C:\emulators\citra\citra-qt.exe<br>
**Emulator Parameters:** <br>

## Nintendo 64

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** n64, v64, z64, bin, u1, ndd, gb<br>

**Emulator Name:** Retroarch mupen64plus_next<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\mupen64plus_next_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/mupen64plus/).
Please read core documentation.

## Nintendo 64DD

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** n64, v64, z64, bin, u1, ndd, gb<br>

**Emulator Name:** Retroarch mupen64plus_next<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\mupen64plus_next_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/mupen64plus/).
Please read core documentation.

## Nintendo DS

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** nds, bin<br>

**Emulator Name:** Retroarch melonds<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\melonds_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/melonds/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

**Emulator Name:** Retroarch desmume<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\desmume_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/desmume/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

## Nintendo Family Computer Disk System

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** nes, fds, unf, unif<br>

**Emulator Name:** Retroarch mesen<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\mesen_libretro.dll" -f<br>

This core requires a BIOS file.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/mesen/).
Please read core documentation.

**Emulator Name:** Retroarch nestopia<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\nestopia_libretro.dll" -f<br>

This core requires a BIOS file.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/nestopia_ue/).
Please read core documentation.

**Emulator Name:** Retroarch fceumm<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\fceumm_libretro.dll" -f<br>

This core requires a BIOS file.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/fceumm/).
Please read core documentation.

## Nintendo Game Boy

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch sameboy<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\sameboy_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/sameboy/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

## Nintendo Game Boy Advance

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch mgba<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\mgba_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/mgba/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

## Nintendo Game Boy Color

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch sameboy<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\sameboy_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/sameboy/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

## Nintendo GameCube

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** rvz<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Dolphin<br>
**Emulator Location:** [Dolphin Folder]\Dolphin.exe<br>
**Emulator Location (Example):** C:\emulators\dolphin\Dolphin.exe<br>
**Emulator Parameters:** <br>

**Emulator Name:** Retroarch dolphin<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\dolphin_libretro.dll" -f<br>

This core requires special files to work correctly.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/dolphin/).
Please read core documentation.

## Nintendo NES

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch mesen<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\mesen_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/mesen/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

**Emulator Name:** Retroarch nestopia<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\nestopia_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/nestopia_ue/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

**Emulator Name:** Retroarch fceumm<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\fceumm_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/fceumm/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

## Nintendo Satellaview

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** ares<br>
**Emulator Location:** [ares Folder]\ares.exe<br>
**Emulator Location (Example):** C:\emulators\ares\ares-v138\ares.exe<br>
**Emulator Parameters:** --system "Super Famicom"<br>

Command-line options can be found at [ares GitHub](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).

## Nintendo SNES

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** smc, sfc, swc, fig, bs, st<br>

**Emulator Name:** Retroarch snes9x<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\snes9x_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/snes9x/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

**Emulator Name:** Snes9x<br>
**Emulator Location:** [Snes9x Folder]\snes9x-x64.exe<br>
**Emulator Location (Example):** C:\emulators\snes9x\snes9x-x64.exe<br>
**Emulator Parameters:** -fullscreen<br>

## Nintendo SNES MSU1

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** smc, sfc, swc, fig, bs, st<br>

**Emulator Name:** Retroarch snes9x<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\snes9x_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/snes9x/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

**Emulator Name:** Snes9x<br>
**Emulator Location:** [Snes9x Folder]\snes9x-x64.exe<br>
**Emulator Location (Example):** C:\emulators\snes9x\snes9x-x64.exe<br>
**Emulator Parameters:** -fullscreen<br>

## Nintendo Switch

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** bsp, xci<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Ryujinx<br>
**Emulator Location:** [Ryujinx Folder]\Ryujinx.exe<br>
**Emulator Location (Example):** C:\emulators\ryujinx\Ryujinx.exe<br>
**Emulator Parameters:** <br>

**Emulator Name:** Yuzu<br>
**Emulator Location:** [Yuzu Folder]\yuzu.exe<br>
**Emulator Location (Example):** C:\emulators\yuzu\yuzu.exe<br>
**Emulator Location (Example):** C:\Users\HomePC\AppData\Local\yuzu\yuzu-windows-msvc\yuzu.exe<br>
**Emulator Parameters:** <br>

## Nintendo Wii

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** rvz<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Dolphin<br>
**Emulator Location:** [Dolphin Folder]\Dolphin.exe<br>
**Emulator Location (Example):** C:\emulators\dolphin\Dolphin.exe<br>
**Emulator Parameters:** <br>

**Emulator Name:** Retroarch dolphin<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\dolphin_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/dolphin/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

## Nintendo WiiU

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** wua<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Cemu<br>
**Emulator Location:** [Cemu Folder]\cemu.exe<br>
**Emulator Location (Example):** C:\emulators\cemu\cemu.exe<br>
**Emulator Parameters:** -f -g<br>

## Nintendo WiiWare

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** wad<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Dolphin<br>
**Emulator Location:** [Dolphin Folder]\Dolphin.exe<br>
**Emulator Location (Example):** C:\emulators\dolphin\Dolphin.exe<br>
**Emulator Parameters:** <br>

**Emulator Name:** Retroarch dolphin<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\dolphin_libretro.dll" -f<br>

This core requires special files to work correctly.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/dolphin/).
Please read core documentation.

## Panasonic 3DO

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** iso, bin, chd, cue<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch opera<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\opera_libretro.dll" -f<br>

This core requires BIOS or Other files to run properly.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/opera/).
Please read core documentation.

## Philips CD-i

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** chd, iso<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch same_cdi<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\same_cdi_libretro.dll" -f<br>

This core requires BIOS or Other files to run properly.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/same_cdi/).
Please read core documentation.

## ScummVM

**System Folder:** [BAT Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** bat<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Direct Launch<br>
**Emulator Location:** <br>
**Emulator Parameters:** <br>

Command line parameters can be found [here](https://scumm-thedocs.readthedocs.io/en/latest/advanced/command_line.html#command-line-interface).<br>
Use the tool available in the Simple Launcher menu to generate BAT files for you.

## Sega Dreamcast

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** chd, gdi, cue, bin, cdi<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Redream<br>
**Emulator Location:** [Redream Folder]\redream.exe<br>
**Emulator Location (Example):** C:\emulators\redream\redream.exe<br>
**Emulator Parameters:** <br>

**Emulator Name:** Retroarch flycast<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\flycast_libretro.dll" -f<br>

This core requires BIOS or Other files to run properly.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/flycast/).
Please read core documentation.

## Sega Game Gear

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch genesis_plus_gx<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\genesis_plus_gx_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/genesis_plus_gx/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

**Emulator Name:** MasterGear<br>
**Emulator Location:** [MasterGear Folder]\MG.exe<br>
**Emulator Location (Example):** C:\emulators\MasterGear\MG.exe<br>
**Emulator Parameters:** <br>

The list of commands available for this emulator can be found at [https://fms.komkon.org/MG/MG.html](https://fms.komkon.org/MG/MG.html).

## Sega Genesis

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip, 7z<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch picodrive<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\picodrive_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/picodrive/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

**Emulator Name:** Retroarch genesis_plus_gx<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\genesis_plus_gx_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/genesis_plus_gx/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

**Emulator Name:** Retroarch blastem<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\blastem_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/blastem/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

## Sega Genesis 32X

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch picodrive<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\picodrive_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/picodrive/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

## Sega Genesis CD

**System Folder:** [ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** chd, bin, cue, iso<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch picodrive<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\picodrive_libretro.dll" -f<br>

This core requires BIOS or Other files to run properly.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/picodrive/).
Please read core documentation.

**Emulator Name:** Retroarch genesis_plus_gx<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\genesis_plus_gx_libretro.dll" -f<br>

This core requires BIOS or Other files to run properly.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/genesis_plus_gx/).
Please read core documentation.

**Emulator Name:** ares<br>
**Emulator Location:** [ares Folder]\ares.exe<br>
**Emulator Location (Example):** C:\emulators\ares\ares-v138\ares.exe<br>
**Emulator Parameters:** --system "Mega CD"<br>

Command-line options can be found at [ares GitHub](https://github.com/ares-emulator/ares/blob/master/README.md#command-line-options).

## Sega Master System

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch genesis_plus_gx<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\genesis_plus_gx_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/genesis_plus_gx/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

**Emulator Name:** MasterGear<br>
**Emulator Location:** [MasterGear Folder]\MG.exe<br>
**Emulator Location (Example):** C:\emulators\MasterGear\MG.exe<br>
**Emulator Parameters:** <br>

The list of commands available for this emulator can be found at [https://fms.komkon.org/MG/MG.html](https://fms.komkon.org/MG/MG.html).

## Sega Model 3

**System Folder:** [BAT Folder]<br>
**System Is MAME?** true<br>
**Format To Search In System Folder:** bat<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Direct Launch<br>
**Emulator Location:** <br>
**Emulator Location (Example):** <br>
**Emulator Parameters:** <br>

BAT file example:
```bat
cd /d D:\Emulators\Supermodel
Supermodel.exe "D:\Sega Model3\bass.zip" -fullscreen -show-fps
```

Adjust the batch file as per your needs. You need a separate batch file for each game (or ROM) you own. Name each batch file with the same name as the game (or ROM).<br>
Before using Simple Launcher, first try running your batch file directly. If it works there, it will also work within Simple Launcher.

## Sega Saturn

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** chd, cue, toc, m3u, ccd<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch mednafen_saturn<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\mednafen_saturn_libretro.dll" -f<br>

This core requires BIOS or Other files to run properly.
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/beetle_saturn/).
Please read core documentation.

## Sega SC-3000

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** sms, gg, sg, sc, sf, dsk, cht<br>

**Emulator Name:** BizHawk<br>
**Emulator Location:** [BizHawk Folder]\EmuHawk.exe<br>
**Emulator Location (Example):** C:\emulators\bizhawk\EmuHawk.exe<br>
**Emulator Parameters:** <br>

**Emulator Name:** MasterGear<br>
**Emulator Location:** [MasterGear Folder]\MG.exe<br>
**Emulator Location (Example):** C:\emulators\MasterGear\MG.exe<br>
**Emulator Parameters:** <br>

The list of commands available for this emulator can be found at [https://fms.komkon.org/MG/MG.html](https://fms.komkon.org/MG/MG.html).

## Sega SG-1000

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** sms, gg, sg, sc, sf, dsk, cht<br>

**Emulator Name:** BizHawk<br>
**Emulator Location:** [BizHawk Folder]\EmuHawk.exe<br>
**Emulator Location (Example):** C:\emulators\bizhawk\EmuHawk.exe<br>
**Emulator Parameters:** <br>

**Emulator Name:** MasterGear<br>
**Emulator Location:** [MasterGear Folder]\MG.exe<br>
**Emulator Location (Example):** C:\emulators\MasterGear\MG.exe<br>
**Emulator Parameters:** <br>

The list of commands available for this emulator can be found at [https://fms.komkon.org/MG/MG.html](https://fms.komkon.org/MG/MG.html).

## Sinclair ZX Spectrum

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** true<br>
**Format To Launch After Extraction:** sms, gg, sg, sc, sf, dsk, cht<br>

**Emulator Name:** Retroarch fuse<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\fuse_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/fuse/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

## SNK Neo Geo CD

**System Folder:** [ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** chd, cue<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch neocd<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\neocd_libretro.dll" -f<br>

This core requires BIOS or other files to run properly.
Core documentation can be found at GitHub [here](https://github.com/libretro/neocd_libretro) and [here](https://github.com/libretro/libretro-core-info/blob/master/neocd_libretro.info).
Please read core documentation.

## SNK Neo Geo Pocket

**System Folder:** [ROM Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch mednafen_ngp<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\mednafen_ngp_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/beetle_neopop/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

## SNK Neo Geo Pocket Color

**System Folder:** [ROM or ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** zip<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** Retroarch mednafen_ngp<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\mednafen_ngp_libretro.dll" -f<br>

Core documentation can be found at [Libretro website](https://docs.libretro.com/library/beetle_neopop/).
Core may require BIOS files or special configurations to work properly.
Please read core documentation.

## Sony Playstation 1

**System Folder:** [ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** chd, cue, bin, img, mds, mdf, pbp<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** DuckStation<br>
**Emulator Location:** [DuckStation Folder]\duckstation-qt-x64-ReleaseLTCG.exe<br>
**Emulator Location (Example):** C:\emulators\duckstation\duckstation-qt-x64-ReleaseLTCG.exe<br>
**Emulator Parameters:** -fullscreen<br>

This emulator requires a BIOS file to run.<br>
Core documentation can be found at [GitHub](https://github.com/stenzek/duckstation).
Please read core documentation.

**Emulator Name:** Retroarch mednafen_psx<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\mednafen_psx_libretro.dll" -f<br>

This emulator requires a BIOS file to run.<br>
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/beetle_psx/).
Please read core documentation.

## Sony Playstation 2

**System Folder:** [ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** chd, bin, iso, mdf, cso, zso, gz<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** PCSX2<br>
**Emulator Location:** [PCSX2 Folder]\pcsx2-qt.exe<br>
**Emulator Location (Example):** C:\emulators\pcsx2\pcsx2-qt.exe<br>
**Emulator Parameters:** -fullscreen<br>

This emulator requires a BIOS file to run.<br>
Documentation can be found at [Emulator website](https://pcsx2.net/docs/).
Please read the documentation.

## Sony Playstation 3

**System Folder:** [BAT Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** bat<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** RPCS3<br>
**Emulator Location:** <br>
**Emulator Parameters:** <br>

Use the tool available in the Simple Launcher menu to generate BAT files for you.

## Sony PSP

**System Folder:** [ISO Folder]<br>
**System Is MAME?** false<br>
**Format To Search In System Folder:** cso, chd, iso, pbp, elf, prx<br>
**Extract File Before Launch?** false<br>
**Format To Launch After Extraction:** <br>

**Emulator Name:** PPSSPP<br>
**Emulator Location:** [PPSSPP Folder]\PPSSPPWindows64.exe<br>
**Emulator Location (Example):** C:\emulators\ppsspp\PPSSPPWindows64.exe<br>
**Emulator Parameters:** --fullscreen<br>

**Emulator Name:** Retroarch ppsspp<br>
**Emulator Location:** [RetroArch Folder]\retroarch.exe<br>
**Emulator Location (Example):** C:\emulators\retroarch\retroarch.exe<br>
**Emulator Parameters:** -L "[Retroarch Folder]\cores\ppsspp_libretro.dll" -f<br>

This core requires special files to work properly.<br>
Core documentation can be found at [Libretro website](https://docs.libretro.com/library/ppsspp/).
Please read core documentation.