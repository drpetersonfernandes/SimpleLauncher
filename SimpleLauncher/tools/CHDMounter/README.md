[![Platform: Windows](https://img.shields.io/badge/Platform-Windows%20x64%20%7C%20ARM64-0078d7.svg)](https://www.microsoft.com/windows)
[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512bd4.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE.txt)
[![GitHub release](https://img.shields.io/github/v/release/drpetersonfernandes/CHDMounter)](https://github.com/drpetersonfernandes/CHDMounter/releases)

# CHDMounter

Mount CHD (Compressed Hunks of Data) CD/DVD images as virtual read-only drives on Windows. Supports 31 console types and 8 filesystem formats across consoles and PC optical media.

![](screenshot.png)

---

## Features

- **Mount CHD files as virtual drives** using [Dokan](https://github.com/dokan-dev/dokany) or [WinFsp](https://winfsp.dev/)
- **31 console types** — ISO 9660, UDF, XDVDFS (Xbox), OperaFS (3DO), CD-i Green Book, HFS/HFS+ (Pippin), and more
- **Virtual CUE/BIN/ISO/WAV export** — 5 disc image export modes for emulators and burning tools
- **SingleFile ISO passthrough** — single `image.iso` file for emulators that expect raw ISO (xemu, RPCS3)
- **Read-only** — never modifies your CHD files
- **Automatic filesystem detection by track header** — no manual setup needed for most discs
- **Command-line interface** for scripting and frontend integration
- **WPF dark theme UI** with console type dropdown, real-time log output, settings dialog
- **Settings with DPAPI encryption** — persisted across sessions
- **Update checker** — polls GitHub releases for new versions
- **Serilog** structured logging to file and debug output
- **Single-file self-contained publish** — distribute as one `.exe`
- **x64 and ARM64** support

---

## Supported Filesystems

| Console / Format         | CLI Alias                         | Filesystem               | Notes                                            |
|--------------------------|-----------------------------------|--------------------------|--------------------------------------------------|
| PS1                      | `ps1`, `playstation`, `psx`       | ISO 9660                 | Track-level parsing, aggressive PVD scan         |
| PS2                      | `ps2`                             | ISO 9660                 | CD/DVD auto-detection                            |
| PS3                      | `ps3`                             | UDF → ISO 9660 fallback  | Multi-extent large file support                  |
| PS3 (ISO Raw)            | `isoraw`                          | Virtual ISO passthrough  | Single `image.iso` for RPCS3                     |
| PSP                      | `psp`                             | ISO 9660                 | UMD image support                                |
| PlayStation (Auto)       | `psauto`, `psdetect`              | ISO 9660                 | Auto-detect PS1/PS2/PS3/PSP                      |
| Xbox                     | `xbox`                            | XDVDFS                   | Binary tree directory structure                  |
| Xbox 360                 | `xbox360`, `x360`                 | XDVDFS                   | XGD2/XGD3 offset detection                       |
| Xbox (ISO Raw)           | `isoraw`                          | Virtual ISO passthrough  | Single `image.iso` for xemu                      |
| Dreamcast                | `dreamcast`, `dc`                 | ISO 9660                 | GD-ROM offset search (-45000, -150, 0, etc.)     |
| 3DO                      | `3do`                             | OperaFS                  | Block-based directory chain, ISO 9660 fallback   |
| CD-i                     | `cdi`, `cd-i`                     | Green Book               | Interleaved stream support, Path Table           |
| Saturn                   | `saturn`                          | ISO 9660                 |                                                  |
| NeoGeo CD                | `neogeo`, `ngcd`                  | ISO 9660                 |                                                  |
| PC Engine CD             | `pcengine`, `pce`, `tgcd`         | ISO 9660                 | Boot signature scan, fallback raw track files    |
| Sega Genesis / Mega CD   | `segagenesis`, `megacd`, `segacd` | ISO 9660                 |                                                  |
| PC-FX                    | `pcfx`                            | PC-FX ISO                | Dedicated byte-offset VD scanner                 |
| Amiga CD32               | `amigacd32`, `cd32`               | ISO 9660                 |                                                  |
| Amiga CD                 | `amigacd`, `amiga`                | ISO 9660                 | Also handles Amiga CDTV                          |
| FM Towns                 | `fmtowns`, `fmt`                  | ISO 9660                 |                                                  |
| X68000                   | `x68000`, `x68k`                  | ISO 9660 → UDF fallback  |                                                  |
| PC-98                    | `pc98`, `pc-98`, `nec98`          | ISO 9660                 |                                                  |
| Nuon                     | `nuon`                            | UDF → ISO 9660 fallback  | VM Labs Nuon DVD                                 |
| Pico                     | `pico`                            | ISO 9660                 | Sega Pico                                        |
| Pippin                   | `pippin`                          | HFS → HFS+ → UDF → ISO   | Apple Bandai Pippin                              |
| Generic ISO 9660         | `iso9660`, `generic`, `iso`       | ISO 9660 / High Sierra   | Joliet/UTF-16BE, SUSP/Rock Ridge                 |
| Generic ISO Raw          | `isoraw`                          | Raw sectors              | Entire CHD as single `image.iso`                 |
| CUE/BIN (Raw)            | `cuebin`, `cue`                   | Virtual                  | Raw 2352-byte sectors                            |
| CUE/BIN (Cooked)         | `cuebin2048`, `cue2048`           | Virtual                  | 2048-byte sectors                                |
| CUE/ISO                  | `cueiso`                          | Virtual                  | CUE sheet + ISO at 2048 bytes/sector             |
| CUE/BIN/WAV              | `cuebinwav`, `cuewav`             | Virtual                  | CUE + BIN + WAV audio tracks                     |
| CUE/ISO/WAV              | `cueisowav`                       | Virtual                  | CUE + ISO + WAV audio tracks                     |

---

## Requirements

- **Windows 10 or later** (x64 or ARM64)
- **.NET 10.0 Desktop Runtime** (or self-contained build ships its own)
- **Dokan** Use latest version — [install Dokany](https://github.com/dokan-dev/dokany/releases)  
  or
- **WinFsp** Use latest version — [download WinFsp](https://github.com/winfsp/winfsp/releases)

> Only one driver is needed. The Dokan executable uses Dokan; the WinFsp executable uses WinFsp. Both ship side-by-side.

---

## Quick Start

### Download

Grab the latest self-contained executable from [Releases](https://github.com/drpetersonfernandes/CHDMounter/releases):

- `CHDMounter.exe`

No installation required. Just download and run.

### GUI Mode

```
CHDMounter.exe
```

Opens the main window. Click **Browse** to select a CHD file, pick a filesystem type from the dropdown, and click **Mount**. The drive appears in Explorer.

### Command Line

```
CHDMounter.exe [/l] [/a] [/s:<index>] <chd_file> [mount_point]
```

| Argument | Description |
|----------|-------------|
| `/l` | Launch Explorer after mount |
| `/a` | Auto-select drive letter |
| `/s:<index>` | Select console system by numeric index or name |
| `<chd_file>` | Path to the .chd file (required) |
| `[mount_point]` | Drive letter/path for mount (optional, auto-picks if omitted) |

**Notes:**
- Arguments can be in any order
- Flags are prefixed with `/`
- A bare numeric argument (not prefixed with `/`) is accepted as a console index shorthand
- If only `<chd_file>` is provided without flags, a dialog appears asking you to choose

### Console Type Reference

You can specify the console type using either a **numeric index** or a **string alias** (case-insensitive).

#### Numeric Indexes (1–31)

| Index | Console Type                 | CLI String Aliases |
|-------|------------------------------|-------------------|
| 1 | Amiga CD                     | `amigacd`, `amiga` |
| 2 | Amiga CD32                   | `amigacd32`, `cd32` |
| 3 | CD-i                         | `cdi`, `cd-i` |
| 4 | Dreamcast                    | `dreamcast`, `dc` |
| 5 | Neo Geo CD                   | `neogeo`, `ngcd` |
| 6 | PC Engine CD                 | `pcengine`, `pce`, `tgcd` |
| 7 | PC-FX                        | `pcfx` |
| 8 | PS1                          | `ps1`, `playstation`, `psx` |
| 9 | PS2                          | `ps2` |
| 10 | PS3                          | `ps3` |
| 11 | PS3 (ISO Raw)                | `isoraw` |
| 12 | PSP                          | `psp` |
| 13 | Saturn                       | `saturn` |
| 14 | Sega Genesis / Mega CD       | `segagenesis`, `megacd`, `segacd` |
| 15 | 3DO                          | `3do` |
| 16 | Xbox                         | `xbox` |
| 17 | Xbox 360                     | `xbox360`, `x360` |
| 18 | Generic ISO Raw              | `isoraw` |
| 19 | Generic ISO 9660             | `iso9660`, `generic`, `iso` |
| 20 | CUE/BIN (Raw)                | `cuebin`, `cue` |
| 21 | CUE/BIN (Cooked)             | `cuebin2048`, `cue2048` |
| 22 | CUE/ISO                      | `cueiso` |
| 23 | CUE/BIN/WAV                  | `cuebinwav`, `cuewav` |
| 24 | CUE/ISO/WAV                  | `cueisowav` |
| 25 | FM Towns                     | `fmtowns`, `fmt` |
| 26 | PlayStation (Auto)           | `psauto`, `psdetect` |
| 27 | X68000                       | `x68000`, `x68k` |
| 28 | Pico                         | `pico` |
| 29 | PC-98                        | `pc98`, `pc-98`, `nec98` |
| 30 | Nuon                         | `nuon` |
| 31 | Pippin                       | `pippin` |

**Examples:**
```bash
# Mount a PS2 game as drive M:
CHDMounter.exe /s:9 game.chd M:

# Mount an Xbox 360 game (auto-select drive letter)
CHDMounter.exe 17 game.chd

# Mount a Dreamcast game and launch Explorer
CHDMounter.exe /l /s:4 game.chd

# Mount as virtual CUE/BIN
CHDMounter.exe /s:20 disc.chd

# Mount with generic ISO 9660 parser
CHDMounter.exe /s:19 data.chd N:

# Mount with console name alias
CHDMounter.exe game.chd ps3
```

If no console type is specified, a dialog appears asking you to choose.

## Build from Source

### Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Windows 10+ (x64 or ARM64)

---

## How It Works

### CHD Reading

CHD (Compressed Hunks of Data) is MAME's lossless compression format for CD/DVD/HDD images. CHDMounter uses [CHDSharp](https://github.com/drpetersonfernandes/CHDSharp) to read all five CHD versions (V1-V5) and all compression codecs:

- **General**: zlib (deflate), LZMA, FLAC (headerless, 16-bit stereo), dynamic Huffman, Zstd
- **CD-sector**: CDZL (zlib), CDLZ (LZMA), CDFL (FLAC), CDZS (Zstd) — with ECC regeneration
- **AV**: AVHuff — audio+video Huffman for laserdisc/arcade captures

### Sector Reading

The `SectorReader` maps logical block addresses (LBAs) to byte offsets within CHD hunks. It:

1. Parses track metadata (`CHT2`/`CHTR`/`CHGD` tags) for multi-track CD images
2. Maps LBA to CHD frame number using track offsets (handles GD-ROM 45000-LBA shift)
3. Reads and caches the compressed hunk via `ReadHunk()`
4. Detects sector mode (Mode 1 = 16-byte header, Mode 2 = 24-byte header) by scanning for the CD sync pattern `00 FF×10 00`
5. Strips headers to deliver 2048 bytes of user data per sector

### File System Parsing

CHDMounter uses the [VideoGameFileSystemParser](https://github.com/drpetersonfernandes/VideoGameFileSystemParser) library (v1.0.0) to parse various console file systems from raw 2048-byte sectors and reconstruct directory trees.

Supported file systems include ISO 9660 (with Joliet/Rock Ridge), XDVDFS (Xbox), OperaFS (3DO), CD-i Green Book, UDF, and HFS/HFS+.

### VFS Operations

The `ChdContainer` bridges the parsed `FsNode` tree to the Dokan/WinFsp VFS layer:

- **Dokan**: `ChdFs` implements `IDokanOperations` — `CreateFile` resolves paths, `ReadFile` maps file offsets to sector reads, `FindFiles` lists directories, `GetVolumeInformation` reports "CHDFS" read-only volume.
- **WinFsp**: `ChdFs` extends `FileSystemBase` — `Open` resolves paths and returns `FileEntry` as both file node and descriptor, `Read` copies sector data into native memory via `Marshal.Copy`, `ReadDirectoryEntry` enumerates children with `.` and `..`.

For CUE/BIN/ISO/WAV modes, the container generates virtual entries dynamically:
- The `.cue` file contains standard `FILE`, `TRACK`, and `INDEX` descriptors built from CHD track metadata.
- The `.bin`/`.iso` file maps reads to raw sector data (2352 or 2048 bytes per sector).
- `.wav` files are generated per audio track with standard WAV headers.

For SingleFile mode, the container serves the entire decompressed CHD image as a single `image.iso` file via `CHDFile.Read()`.

---

## Differences between Dokan and WinFsp Variants

| Aspect | Dokan | WinFsp |
|--------|-------|--------|
| Driver | Kernel-mode (Dokan.sys) | User-mode (WinFsp) |
| NuGet | `DokanNet` 2.3.0.3 | `winfsp.net` 2.2.26194 |
| FileSystem base | `IDokanOperations` | `FileSystemBase` |
| Admin mount | Standard | Cross-integrity folder mounts with permissive DACL |

Both variants share the same core functionality and UI. Choose based on your driver preference.

---

## Tester Application (CHDMounter_Tester)

WPF desktop application for **batch testing and benchmarking** CHD disc image parsing. Scans folders of `.chd` files, parses each one with a selected console file system parser, and generates summary reports with PDF export.

### Features

- Select a folder of CHD files and a target console type
- Batch-parse every `.chd` file in the folder
- Report success/failure, file count, directory count, volume size, and timing per file
- View aggregated summary statistics (fastest, slowest, average, total throughput)
- Export results to **PDF** with QuestPDF

### Usage

1. Launch `CHDMounter_Tester.exe`
2. Select a folder containing `.chd` files
3. Choose a console type from the dropdown
4. Click **Run Tests** to begin batch parsing
5. Click **Export PDF** to save results

---

## Dependencies

### NuGet Packages

| Package              | Version   | Purpose                                   |
|----------------------|-----------|-------------------------------------------|
| `CHDSharp`           | 1.2.0     | MAME CHD format reader                    |
| `coverlet.collector` | 10.0.1    | Code coverage (Tests only)                |
| `DokanNet`           | 2.3.0.3   | Dokan virtual filesystem driver bindings  |
| `Microsoft.NET.Test.Sdk` | 18.8.1    | Test SDK (Tests only)                     |
| `QuestPDF`           | 2026.7.1  | PDF report generation (Tester only)       |
| `Serilog`            | 4.4.0     | Structured logging                        |
| `Serilog.Sinks.Debug`| 3.0.0     | Visual Studio debug output logging        |
| `Serilog.Sinks.File` | 7.0.0     | File-based log output                     |
| `VideoGameFileSystemParser` | 1.0.0     | Filesystem parser                         |
| `winfsp.net`         | 2.2.26194 | WinFsp virtual filesystem driver bindings |
| `WPF-UI`             | 4.3.0     | Modern WPF theming (Fluent/Win11 style)   |
| `xUnit`              | 2.9.3     | Test framework (Tests only)               |
| `xunit.runner.visualstudio` | 3.1.5     | VS test adapter (Tests only)              |

---

## Acknowledgments

- **CHDSharp** — https://github.com/drpetersonfernandes/CHDSharp
- **VideoGameFileSystemParser** — https://github.com/drpetersonfernandes/VideoGameFileSystemParser
- **MAME** — https://github.com/mamedev/mame

---

## License

This project is licensed under the GPLv3 License – see the [LICENSE](LICENSE.txt) file for details.