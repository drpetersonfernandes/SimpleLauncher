# 18 — Emulator Parameters

> Reference guide built from `SimpleLauncher\parameters.md` (the canonical, 5,831-line file shipped with the app and shown in the Edit System help pane).

## What `parameters.md` contains

One section per system (`## System Name`), each containing:

- The **six path placeholders** (see below).
- Suggested **System Folder** example, **extensions to search**, **Extract File Before Launch?**, **extensions to launch after extraction**, **Group Files by Folder?**.
- One block per emulator:
  - **Emulator Name** and **Emulator Path (Example)**
  - **Emulator Parameters** — examples using absolute paths, relative paths (`%EMULATORFOLDER%`), or per-file-type variants (MAME machines, WinUAE floppy/harddrive/cdrom, Vice `-autostart/-autoload`…)
  - **Fullscreen Parameter** (and sometimes a **Windowed Parameter**)
  - Availability (`Windows-x64`, `Windows-arm64`), BIOS requirements, documentation links, download links, RetroAchievements support notes.

## Path placeholders

| Placeholder | Meaning | Example |
|---|---|---|
| `%BASEFOLDER%` | Directory of `SimpleLauncher.exe` | `%BASEFOLDER%\emulators\RetroArch\retroarch.exe` |
| `%SYSTEMFOLDER%` | First `<SystemFolder>` of the current system | `%SYSTEMFOLDER%` |
| `%EMULATORFOLDER%` | Directory of the emulator executable | `-L "%EMULATORFOLDER%\cores\cap32_libretro.dll" -f` |
| `%ROM%` | Full ROM path (path + extension) | `%ROM%` |
| `%NAME%` | ROM name without path or extension | `dir="games/%NAME%"` |
| `%ROMSYSTEMFOLDER%` | System folder that contains the selected ROM | `%ROMSYSTEMFOLDER%` |

Resolution happens at launch time (`ResolveParameterString`, see [06 — Systems & Launch](06-systems-and-launch.md#regular-emulator-launch)); if no ROM placeholder is present the ROM path is auto-appended (MAME/Raine get the bare machine name).

## Common parameter patterns

- **RetroArch**: `-L "<core path>" -f` (core usually `%EMULATORFOLDER%\cores\<core>_libretro.dll`).
- **MAME**: `-rompath "<emulator>\roms;<emulator>\bios;<base>\roms\<System>;<systemfolder>" <machine> [-cart|-flop1|-cass|-cdrm]`; `-window` = windowed mode.
- **WinUAE**: `/config "%EMULATORFOLDER%\Config.uae" /nogui /run|/floppy0|/harddrive|/cdrom`; `/fullscreen` / `/windowed`.
- **Mednafen**: `-video.fs 1` for fullscreen.
- **Vice**: `-autostart` / `-autoload` / `-fullscreen`.
- **Commander Genius**: `dir="games/%NAME%"` (see the dedicated setup section in `parameters.md`).
- **Ares**: `--fullscreen --system "<System>"`.

## System → emulator index (84 systems, 352 emulator entries)

Generated from `parameters.md` at doc-build time:

| System | Emulators |
|---|---|
| Amstrad CPC | Retroarch caprice32<br>, CPCEC<br>, MAME<br> |
| Amstrad GX4000 | MAME<br> |
| Arcade | MAME<br>, Retroarch mame<br>, Raine<br> |
| Atari 2600 | Stella<br>, Ares<br>, Retroarch stella<br>, MAME<br> |
| Atari 5200 | Altirra<br>, Retroarch a5200<br>, MAME<br> |
| Atari 7800 | Retroarch prosystem<br>, ProSystem<br>, MAME<br> |
| Atari 8-Bits / Atari 800 | Altirra<br>, MAME<br>, Retroarch atari800_libretro<br> |
| Atari Jaguar | BigPEmu<br>, MAME<br> |
| Atari Jaguar CD | BigPEmu<br> |
| Atari Lynx | Mednafen<br>, Gearlynx<br>, Retroarch mednafen_lynx<br>, MAME<br> |
| Atari ST | Hatari<br>, Steem SSE<br>, Retroarch hatari<br>, MAME<br> |
| Atomiswave / Sammy Atomiswave | Flycast<br> |
| Bandai WonderSwan | Mednafen<br>, Ares<br>, BizHawk<br>, Retroarch mednafen_wswan<br>, MAME<br> |
| Bandai WonderSwan Color | Mednafen<br>, Ares<br>, BizHawk<br>, Retroarch mednafen_wswan<br>, MAME<br> |
| Casio PV-1000 | MAME<br> |
| Colecovision | Gearcoleco<br>, Ares<br>, Retroarch gearcoleco<br>, MAME<br> |
| Commander Genius | Commander Genius<br> |
| Commodore 64 | CCS64<br>, Vice<br>, Retroarch vice_x64_libretro<br>, Retroarch vice_x64sc_libretro<br>, MAME<br> |
| Commodore 128 | Vice<br>, MAME<br>, Retroarch vice_x128<br> |
| Commodore Amiga | WinUAE<br>, WinFellow<br>, Retroarch puae<br>, Retroarch puae2021<br>, MAME<br> |
| Commodore Amiga CD32 | Retroarch puae<br>, Retroarch puae2021<br>, MAME<br> |
| FM Towns / FM-Towns | Tsugaru<br> |
| LaserDisk | Daphne<br>, Hypseus Singe<br> |
| Magnavox Odyssey 2 | Retroarch o2em<br>, O2EM<br> |
| Mattel Aquarius | MAME<br> |
| Mattel Intellivision | Retroarch freeintv<br>, MAME<br> |
| Microsoft DOS | DOSBox<br>, DOSBox Staging<br>, DOSBox-X<br>, Retroarch dosbox_pure<br>, Retroarch dosbox<br> |
| Microsoft MSX | OpenMSX<br>, MSXEC<br>, Ares<br>, Retroarch bluemsx<br>, Retroarch fmsx<br>, fMSX<br>, MAME<br> |
| Microsoft MSX2 | OpenMSX<br>, MSXEC<br>, Ares<br>, Retroarch bluemsx<br>, Retroarch fmsx<br>, fMSX<br>, MAME<br> |
| Microsoft Windows | Direct Launch<br> |
| Microsoft Xbox | Xemu<br>
This emulator is available for Windows-x64 and Windows-arm64.<br>
This emulator requires BIOS and system files to work. The list of required files can be found on [Xemu Website](https://xemu.app/docs/required-files/).<br>
There are multiple ways to launch this emulator., Cxbx-Reloaded<br>
This emulator is available for Windows-x64.<br>
There are multiple ways to launch this emulator. |
| Microsoft Xbox 360 | Xenia<br>
This emulator is available for Windows-x64.<br>
There are multiple ways to launch this emulator. |
| Microsoft Xbox 360 XBLA | Xenia<br>
This emulator is available for Windows-x64.<br>
There are multiple ways to launch this emulator., Xenia<br>, Xenia<br> |
| NEC PC Engine / TurboGrafx 16 | Mednafen<br>, Ares<br>, Ootake<br>, Retroarch mednafen_pce<br>, MAME<br> |
| NEC PC Engine CD / TurboGrafx CD | Mednafen<br>, Geargrafx<br>, Ares<br>, Mesen<br>, BizHawk<br>, Retroarch mednafen_pce<br>, MAME<br> |
| NEC PC-FX / PC-FX / PCFX | Mednafen<br>, BizHawk<br>, Retroarch mednafen_pcfx<br>, MAME<br> |
| NEC SuperGrafx / SuperGrafx | Mednafen<br>, Ares<br>, Retroarch mednafen_supergrafx<br> |
| Nintendo 3DS | Azahar<br>, Borked3DS<br>, Citra<br>, Panda3DS<br>, Retroarch citra<br>, Retroarch panda3ds<br> |
| Nintendo 64 | Ares<br>, Simple64<br>, BizHawk<br>, Rosalie Mupen GUI - RMG<br>, Gopher64<br>, Project64<br>, Retroarch mupen64plus_next<br>, Retroarch parallel_n64_libretro<br>, MAME<br> |
| Nintendo 64DD | Ares<br>, Retroarch mupen64plus_next<br>, MAME<br> |
| Nintendo DS | melonDS<br>, DeSmuME<br>, NooDs<br>, Retroarch melonds<br>, Retroarch desmume<br> |
| Nintendo Family Computer Disk System / Famicom Disk System | Ares<br>, Mesen<br>, Mednafen<br>, Retroarch mesen<br>, Retroarch nestopia<br>, Retroarch fceumm<br> |
| Nintendo Game Boy | mGBA<br>, Ares<br>, JGenesis<br>, Gearboy<br>, Mednafen<br>, Sameboy<br>, Retroarch sameboy<br>, Retroarch gambatte<br>, Retroarch tgbdual<br>, Retroarch gearboy<br>, TGB-Dual-L<br>, MAME<br> |
| Nintendo Game Boy Advance | mGBA<br>, Ares<br>, Mednafen<br>, Hades<br>, VisualBoy Advance M<br>, Retroarch mgba<br>, MAME<br> |
| Nintendo Game Boy Color | mGBA<br>, Ares<br>, Mednafen<br>, JGenesis<br>, Gearboy<br>, Sameboy<br>, Retroarch sameboy<br>, Retroarch gambatte<br>, Retroarch tgbdual<br>, Retroarch gearboy<br>, TGB-Dual-L<br>, MAME<br> |
| Nintendo GameCube | Dolphin<br>, Retroarch dolphin<br> |
| Nintendo NES / Famicom | Ares<br>, puNES<br>, Mednafen<br>, JGenesis<br>, Mesen<br>, MyNes<br>, Retroarch mesen<br>, Retroarch nestopia<br>, Retroarch fceumm<br>, MAME<br> |
| Nintendo Satellaview | Ares<br> |
| Nintendo SNES / Super Nintendo / Super Famicom / Super Nes | Bsnes<br>, Snes9x<br>, JGenesis<br>, Ares<br>, Mednafen<br>, Retroarch snes9x<br>, Retroarch bsnes<br>, Retroarch bsnes-jg<br>, Retroarch mednafen_snes_libretro<br>, MAME<br> |
| Nintendo SNES MSU1 | Snes9x<br>, Ares<br>, Retroarch snes9x<br> |
| Nintendo Switch | Eden<br>, Citron<br>, Sudachi<br>, Yuzu<br>, Ryubing<br>, Kenji-NX<br>, Ryujinx<br> |
| Nintendo Virtual Boy | Mednafen<br>, Retroarch mednafen_vb_libretro<br>, MAME<br> |
| Nintendo Wii | Dolphin<br>, Retroarch dolphin<br> |
| Nintendo WiiU | Cemu<br> |
| Nintendo WiiWare | Dolphin<br>, Retroarch dolphin<br> |
| Panasonic 3DO | 4DO<br>, BizHawk<br>, Retroarch opera<br> |
| Philips CD-i | CDiEmu / CD-i Emulator<br>, Retroarch same_cdi<br> |
| ScummVM / Scumm-VM | ScummVM<br>
This emulator is available for Windows-x64 and Windows-arm64.<br>
There are multiple ways to use this program.<br>, ScummVM<br>, ScummVM<br> |
| Sega Dreamcast | Flycast<br>, Redream<br>, Demul<br>, Deecy<br>, Retroarch flycast<br>, MAME<br> |
| Sega Game Gear | MasterGear<br>, Kega Fusion<br>, JGenesis<br>, Mednafen<br>, Emulicious<br>, GearSystem<br>, Ares<br>, BizHawk<br>, Retroarch genesis_plus_gx<br>, MAME<br> |
| Sega Genesis / Mega Drive | Kega Fusion<br>, JGenesis<br>, Ares<br>, Blastem<br>, Mednafen<br>, ClownMDEmu<br>, Retroarch picodrive<br>, Retroarch genesis_plus_gx<br>, Retroarch blastem<br>, MAME<br> |
| Sega Genesis 32X / Mega Drive 32X | Kega Fusion<br>, JGenesis<br>, Ares<br>, Retroarch picodrive<br>, MAME<br> |
| Sega Genesis CD / Mega Drive CD / Sega CD / Mega CD | Ares<br>, JGenesis<br>, Gens<br>, Blastem<br>, Kega Fusion<br>, Retroarch picodrive<br>, Retroarch genesis_plus_gx<br>, MAME<br> |
| Sega Master System / Mark3 | MasterGear<br>, Kega Fusion<br>, JGenesis<br>, GearSystem<br>, Ares<br>, BizHawk<br>, Mednafen<br>, Retroarch genesis_plus_gx<br>, MAME<br> |
| Sega Naomi | Flycast<br> |
| Sega Naomi 2 | Flycast<br> |
| Sega Model 3 | Supermodel<br> |
| Sega Saturn | Mednafen<br>, Ymir<br>, SSF<br>, Kronos<br>, BizHawk<br>, Yaba Sanshiro<br>, Yabause<br>, Retroarch mednafen_saturn<br>, Retroarch kronos<br>, Retroarch yabasanshiro<br>, Retroarch yabause<br>, MAME<br> |
| Sega SC-3000 | BizHawk<br>, MasterGear<br>, Kega Fusion<br>, Ares<br>, MAME<br> |
| Sega SG-1000 | BizHawk<br>, MasterGear<br>, Kega Fusion<br>, GearSystem<br>, Ares<br>, MAME<br> |
| Sharp x68000 | XM6 Pro-68k<br>, Retroarch px68k<br>, MAME<br> |
| Sinclair ZX Spectrum | Spectral<br>, Speccy<br>, Ares<br>, Retroarch fuse<br>, MAME<br> |
| SNK Neo Geo | MAME<br>, Ares<br>, Raine<br>, Retroarch geolith<br> |
| SNK Neo Geo CD / NeoGeo CD | Ares<br>, Raine<br>, FinalBurn Neo<br>, FinalBurn Alpha<br>, Retroarch neocd<br>, MAME<br> |
| SNK Neo Geo Pocket | Mednafen<br>, Ares<br>, Retroarch mednafen_ngp<br>, Retroarch race<br>, MAME<br> |
| SNK Neo Geo Pocket Color | Mednafen<br>, Ares<br>, Retroarch mednafen_ngp<br>, Retroarch race<br>, MAME<br> |
| Sony PlayStation 1 / PSX 1 | DuckStation<br>, Mednafen<br>, BizHawk<br>, Ares<br>, PCSX-Redux<br>, ePSXe<br>, Retroarch mednafen_psx<br>, Retroarch swanstation<br>, Retroarch pcsx_rearmed<br>, MAME<br> |
| Sony PlayStation 2 / PSX2 / PSX 2 | PCSX2<br>, Play<br>, Retroarch pcsx2<br> |
| Sony PlayStation 3 / PSX 3 / PSX3 | RPCS3<br>
This emulator is available for Windows-x64.<br>
There are multiple ways to use this emulator.<br>, RPCS3<br>, RPCS3<br>, RPCS3<br>, RPCS3<br> |
| Sony PlayStation 4 / PSX4 / PSX 4 | shadPS4<br> |
| Sony PlayStation Vita | Vita3K<br> |
| Sony PSP / PlayStation Portable | PPSSPP<br>, Retroarch ppsspp<br> |
| Super ACan / Super-ACan / Super A'Can / Super-A'Can | MAME<br> |
| Zeebo | Infuse<br> |

## Keeping this doc in sync

The table above is generated from `SimpleLauncher\parameters.md` (`## ` headers + `**Emulator Name:**` entries). When `parameters.md` changes, regenerate the table with the same extraction and update this file. The canonical detail (per-emulator parameters, BIOS notes, download links) always lives in `parameters.md`.

## Related docs

- [05 — Configuration](05-configuration.md) (placeholders in `system.xml`)
- [06 — Systems & Launch](06-systems-and-launch.md)
- `SimpleLauncher\parameters.md` (canonical reference)
