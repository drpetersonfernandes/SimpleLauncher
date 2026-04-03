# CHD File Explorer & Mounter

A high-performance C++20 toolkit for inspecting, extracting, and mounting **Compressed Hunks of Data (CHD)** disc images. Designed for retro-computing, gaming preservation, and emulator frontend integration. Provides deep inspection of filesystem structures across 19 console systems.

## Table of Contents

- [Overview](#overview)
- [Key Features](#key-features)
- [Supported Systems](#supported-systems)
- [How It Works](#how-it-works)
  - [CHD Format](#chd-format)
  - [Mounting Architecture](#mounting-architecture)
  - [Sector Reading Pipeline](#sector-reading-pipeline)
  - [Virtual CUE/BIN Generation](#virtual-cuebin-generation)
  - [Interleaved File Handling (CD-i)](#interleaved-file-handling-cd-i)
- [Building](#building)
- [Usage](#usage)
  - [GUI Explorer](#gui-explorer)
  - [CLI Explorer](#cli-explorer)
  - [CHD Mounter](#chd-mounter)
- [Emulator Frontend Integration](#emulator-frontend-integration)
  - [Quick Integration Guide](#quick-integration-guide)
  - [xemu (Xbox)](#xemu-xbox)
  - [RPCS3 (PS3)](#rpcs3-ps3)
  - [RetroArch / Mednafen / Any Emulator](#retroarch--mednafen--any-emulator)
  - [Custom Frontend Integration (Developer Guide)](#custom-frontend-integration-developer-guide)
  - [Command-Line Reference for Integration](#command-line-reference-for-integration)
  - [Exit Codes](#exit-codes)
- [Dependencies](#dependencies)
- [License](#license)

## Overview

CHD File Explorer & Mounter is a Windows-native toolkit that lets you work with CHD (Compressed Hunks of Data) disc images — the standard compressed format used by MAME and many emulators. It consists of three executables:

| Executable | Purpose |
|:-----------|:--------|
| `CHDFileExplorer_GUI.exe` | Graphical explorer with file tree, hex viewer, integrity checker |
| `CHDFileExplorer.exe` | Command-line tool for quick CHD inspection |
| `CHDMounter.exe` | Mounts CHD files as virtual drives on Windows |

The **CHD Mounter** is the primary tool for emulator frontend integration. It uses [Dokany v2](https://github.com/dokan-dev/dokany) to create a virtual read-only drive that exposes the CHD's filesystem as a standard Windows drive letter. Emulators can then access the disc image content without needing to manually extract or convert anything.

## Key Features

- **Virtual Drive Mounting** — Mount any CHD as a read-only local drive using Dokany v2. Appears as a standard removable drive in Windows Explorer.
- **Virtual CUE/BIN** — Present CD-based CHDs as standard `.cue`/`.bin` file pairs in the mounted drive. Selected from the interactive system menu. Compatible with any emulator or tool that reads CUE/BIN.
- **Single File Mode** — Presents entire CHD as a single `image.iso` for emulators like xemu and RPCS3 that expect raw disc images.
- **Console-Aware Parsing** — Dedicated parsers for each system's quirks: GD-ROM LBA offsets (Dreamcast), Mode 2 sectors (PS1), XDVDFS tree (Xbox), OperaFS (3DO), interleaved streams (CD-i), UDF (PS3/PSP).
- **Multi-Architecture** — Native builds for x64 and ARM64 (Windows on Snapdragon).
- **19 Supported Systems** — PlayStation, Xbox, Dreamcast, Saturn, PSP, CD-i, 3DO, PC Engine, PC-FX, Neo Geo CD, Amiga, and more.
- **Audio Extraction** — Convert CDDA audio tracks from mixed-mode CHDs directly to WAV.
- **Graphical Explorer** — Dear ImGui interface with file tree browsing, hex viewer, CHD integrity verifier, metadata inspector, and real-time memory monitoring.
- **High Performance** — Efficient sector reading with hunk caching and multi-threaded reader pool.

## Supported Systems

| # | System | Filesystem | Parser Mode | Notes |
|:-:|:-------|:-----------|:------------|:------|
| 1 | Amiga CD | ISO9660 | Full | Standard ISO9660 |
| 2 | Amiga CD32 | ISO9660 | Full | Standard ISO9660 |
| 3 | Philips CD-i | CD-RTOS / Green Book | Full | Interleaved stream support |
| 4 | Sega Dreamcast | GD-ROM / ISO9660 | Full | LBA 45000 high-density offset |
| 5 | SNK Neo Geo CD | ISO9660 | Full | Standard ISO9660 |
| 6 | NEC PC Engine CD | Proprietary / ISO9660 | Full | System Card boot sectors |
| 7 | NEC PC-FX | ISO9660 | Full | Standard ISO9660 |
| 8 | Sony PlayStation 1 | ISO9660 / Joliet | Full | Mode 2 Form 1/2 support |
| 9 | Sony PlayStation 2 | ISO9660 | Full | Auto DVD/CD detection |
| 10 | Sony PlayStation 3 | UDF / ISO9660 | Full | Multi-extent large files |
| 11 | PS3 (Single File) | Raw ISO (Virtual) | Virtual | Presents CHD as `image.iso` for RPCS3 |
| 12 | Sony PSP | ISO9660 | Full | UMD ISO format |
| 13 | Sega Saturn | ISO9660 | Full | Standard ISO9660 |
| 14 | Sega Genesis CD | ISO9660 | Full | Mega CD format |
| 15 | 3DO Interactive Multiplayer | OperaFS | Full | Proprietary 3DO filesystem |
| 16 | Microsoft Xbox | XDVDFS (XISO) | Full | All magic offsets supported |
| 17 | Microsoft Xbox 360 | XDVDFS (XISO) | Full | All magic offsets supported |
| 18 | Xbox (Single File) | Raw ISO (Virtual) | Virtual | Presents CHD as `image.iso` for xemu |
| 19 | Generic ISO 9660 | ISO9660 | Full | Reference parser using lib9660 |
| 20 | Generic CUE/BIN (Raw CD Image) | CD-ROM (Virtual) | Virtual | Works with all CD-based CHDs |

## How It Works

### CHD Format

CHD (Compressed Hunks of Data) is a file format originally developed for MAME to store compressed disc images. It supports:

- **CHDv1 through v5** format versions
- **Multiple compression codecs**: Huffman, FLAC, zlib, LZMA
- **CD-ROM** (2352-byte raw sectors), **DVD** (2048-byte sectors), and **hard drive** images
- **Track metadata** stored in header tags: `CHT2`/`CHTR` (standard CD tracks), `CHGD`/`CHGT` (GD-ROM tracks with PAD field)
- **Sector formats**: Mode 1, Mode 2 Form 1/2, raw 2352-byte, CDDA audio

The CHD file stores data in compressed "hunks" (blocks). A hunk is the minimum unit of decompression — when any sector within a hunk is requested, the entire hunk is decompressed and cached.

### Mounting Architecture

The CHD Mounter uses a layered architecture to present a CHD file as a standard Windows drive:

```
┌─────────────────────────────────────────────────┐
│                   Windows                        │
│  Explorer / Emulator / Any Application           │
├─────────────────────────────────────────────────┤
│              Dokany v2 Filesystem Driver         │
│         (User-mode virtual filesystem)           │
├─────────────────────────────────────────────────┤
│         ChdVfsOperations (Dokan Callbacks)       │
│   ReadFile, FindFiles, GetFileInfo, etc.         │
├─────────────────────────────────────────────────┤
│          ChdVfsContainer (VFS Container)         │
│   File tree mapping, reader pool, CUE/BIN gen    │
├─────────────────────────────────────────────────┤
│          Console Parser (e.g., PS2Parser)        │
│   System-specific filesystem → FsNode tree       │
├─────────────────────────────────────────────────┤
│              Filesystem Parsers                  │
│   ISO9660, XDVDFS, UDF, OperaFS, CD-i, etc.     │
├─────────────────────────────────────────────────┤
│           ChdSectorReader (Sector I/O)           │
│   LBA mapping, hunk caching, sync offset detect   │
├─────────────────────────────────────────────────┤
│              ChdFile (CHD Access)                │
│   Wraps libchdr C API, reads hunks, metadata     │
├─────────────────────────────────────────────────┤
│                libchdr (BSD-3)                   │
│   CHD decompression: Huffman, FLAC, zlib, LZMA   │
└─────────────────────────────────────────────────┘
```

**Mounting process (step by step):**

1. **Open CHD** — `ChdFile::open()` calls `chd_open()` from libchdr to open and validate the CHD header.

2. **Console Selection** — The user selects a console type (via interactive menu or `/s:<index>` flag). The interactive menu includes both filesystem modes and "(CUE/BIN)" variants for each CD-based console. `ParserFactory::createParser()` instantiates the appropriate parser.

3. **Track Detection** — `ChdFile::getTracksWithLBA()` reads track metadata from CHD header tags. GD-ROM images are detected by `CHGD`/`CHGT` tags, PAD fields, or frame count heuristics (~549,000 frames).

4. **Filesystem Parsing** — The console parser reads the disc's filesystem and builds a tree of `FsNode` structures. Each node contains the file's LBA (sector address), size, name, and system-specific metadata (e.g., CD-i interleaved file numbers, multi-extent chains).

5. **VFS Container Setup** — `Container::setup()` flattens the `FsNode` tree into a `FileEntry` map indexed by lowercase path keys for fast case-insensitive lookup. In CUE/BIN mode, the file tree is skipped entirely — only the virtual `.cue` and `.bin` files are exposed.

6. **Reader Pool Creation** — N `PooledReader` instances are created (N = hardware concurrency, 2–16). Each opens its own `ChdFile` handle because `chd_read()` is **not** thread-safe on the same handle.

7. **Dokany Initialization** — `DokanMain()` is called with `DOKAN_OPTION_WRITE_PROTECT` (read-only) and `DOKAN_OPTION_REMOVABLE` (appears as removable drive). The `GlobalContext` pointer references the `Container`.

8. **File Reads** — When any application reads a file from the mounted drive:
   - Dokan invokes `vfsReadFile()` → `Container::readFile()`
   - `Container::readFile()` acquires a reader from the pool
   - `ChdSectorReader::readSector()` maps the logical LBA to a CHD frame, decompresses the hunk, detects and strips subchannel prefixes and CD sector headers, and returns 2048 bytes of user data

### Sector Reading Pipeline

The `ChdSectorReader` handles the complexity of reading logical sectors from a CHD:

```
Logical Sector Request (LBA)
        │
        ▼
Track Lookup (CHT2/CHGD metadata)
        │
        ▼
CHD Frame Calculation (track-relative or absolute)
        │
        ▼
Hunk Number + Offset Within Hunk
        │
        ▼
chd_read() → Decompress Hunk (cached)
        │
        ▼
Subchannel Detection & Sync Offset
  (skip 96-byte subchannel prefix if present,
   locate CD sync pattern 00 FF FF FF FF FF FF 00)
        │
        ▼
CD Sector Header Detection
        │
        ├── Mode 1: strip 16-byte header → 2048 bytes user data
        ├── Mode 2 Form 1: strip 24-byte header → 2048 bytes user data
        ├── Mode 2 Form 2: strip 24-byte header → 2324 bytes user data
        └── Audio: byte-swap for endianness correction
        │
        ▼
Return Logical Sector Data
```

Key behaviors:
- **Hunk caching**: Decompressed hunks are cached in memory. Repeated reads to the same hunk skip decompression entirely.
- **Sync offset detection**: The reader detects subchannel prefixes (e.g., 96 bytes) before the CD sync pattern and caches the offset per track. This ensures correct sector data extraction from CHDs that store raw subchannel data.
- **Per-track offset caching**: After the first sector of each track is read, the header offset is cached. Subsequent reads from the same track use the cached offset.
- **GD-ROM handling**: For Dreamcast GD-ROM images, the reader accounts for the LBA 45000 high-density area offset and PAD fields between tracks.

### Virtual CUE/BIN Generation

When a "(CUE/BIN)" entry is selected from the interactive system menu, the mounter generates virtual `.cue` and `.bin` files instead of the parsed directory tree. Track selection is skipped — all tracks are included automatically.

1. **CUE Sheet Generation** — Track metadata (`CHT2`, `CHTR`, `CHGD` tags) is read to determine track types, frame counts, and pregap information. A standard CUE sheet is generated with `TRACK`, `FILE`, `INDEX`, and `PREGAP` directives. INDEX 01 positions use cumulative frame offsets within the virtual BIN (not physical disc LBA). The standard 2-second pregap (00:02:00) is applied to Track 1.

2. **Virtual BIN Streaming** — The `.bin` file streams raw 2352-byte sectors on demand. If the CHD stores subchannel data (unitbytes > 2352), it is stripped to produce standard 2352-byte sectors. When a read request arrives:
   - The byte offset is resolved to a track number and sector offset
   - `ChdSectorReader::readRawSector()` reads the raw unit from the CHD
   - The sync offset is used to skip any subchannel prefix and locate the actual CD sector data
   - Reads seamlessly cross track boundaries — the BIN appears as one continuous file

**Track type mapping:**

| CHD Track Type | CUE Mode |
|:---------------|:---------|
| `MODE1` | `MODE1/2352` |
| `MODE2` | `MODE2/2352` |
| `CDI_2352` | `MODE2/2352` |
| `AUDIO` | `AUDIO` |

### Interleaved File Handling (CD-i)

CD-i (Green Book) discs use interleaved sectors where multiple files share the same track in an interleaved pattern. The CD-i parser handles this by:

1. Reading the raw sector subheader at byte offset 16 to identify the file number
2. Skipping sectors belonging to other files
3. Using a per-file scan cache to avoid O(N^2) rescanning on repeated reads

This ensures correct file content extraction even from complex CD-i titles with many interleaved streams.

## Building

### Prerequisites

- **MinGW-w64/Clang** compiler with C++20 support
- **CMake 3.15+**
- **[Dokany v2.1.0+](https://github.com/dokan-dev/dokany)** (required for the Mounter only; the CLI and GUI do not need Dokany)

### Build Commands

```bash
mkdir build && cd build
cmake .. -G "Ninja" -DCMAKE_BUILD_TYPE=Release
ninja
```

### Build Targets

| CMake Target | Output | Description |
|:-------------|:-------|:------------|
| `CPP_CHDFileExplorer` | `CHDFileExplorer.exe` | Command-line CHD inspector |
| `CPP_CHDFileExplorer_GUI` | `CHDFileExplorer_GUI.exe` | Graphical explorer (Win32 + DirectX11 + ImGui) |
| `CHDMounter` | `CHDMounter.exe` | Virtual drive mounter (requires Dokany) |
| `CPP_CHDFileExplorer_Test` | `CPP_CHDFileExplorer_Test.exe` | Parser validation test suite |

### Cross-Compilation (ARM64)

ARM64 builds for Windows on Snapdragon use the provided toolchain file:

```bash
cmake .. -G "Ninja" -DCMAKE_BUILD_TYPE=Release -DCMAKE_TOOLCHAIN_FILE=cmake/toolchain-arm64.cmake
ninja
```

## Usage

### GUI Explorer

1. Run `CHDFileExplorer_GUI.exe`
2. Select the **Parser Mode** from the dropdown (e.g., PlayStation 2, Dreamcast, Xbox)
3. Go to **File > Open CHD** to load a disc image
4. Browse the file tree in the left panel
5. **Double-click** a file to extract it to a temp location and open it with the default application
6. **Right-click** a file for "Save As" to extract to a specific location
7. Use the **Hex View** tab to inspect raw sector data with hunk navigation
8. Use **Test Integrity** to verify the CHD by decompressing every hunk
9. The **Metadata** tab shows CHD header info and track metadata

### CLI Explorer

```cmd
CHDFileExplorer.exe <path_to_chd> <console_type>
```

Console types: `ps1`, `ps2`, `ps3`, `xbox`, `xbox360`, `dreamcast`, `psp`, `saturn`, `segacd`, `cdi`, `3do`, `amiga`, `amigacd32`, `pce`, `pcfx`, `neogeo`, `iso9660`

Example:
```cmd
CHDFileExplorer.exe "C:\Games\game.chd" ps2
```

### CHD Mounter

#### Basic Usage

Mount a CHD to a specific drive letter:
```cmd
CHDMounter.exe "C:\Games\game.chd" X:
```

Mount a CHD and auto-select a drive letter:
```cmd
CHDMounter.exe /a "C:\Games\game.chd"
```

Mount and automatically open Windows Explorer:
```cmd
CHDMounter.exe /l "C:\Games\game.chd" X:
```

#### Command-Line Flags

| Flag | Description |
|:-----|:------------|
| `/d` | Enable Dokany debug output (shows filesystem operations in console) |
| `/l` | Launch Windows Explorer after mounting |
| `/a` | Auto-select the first available drive letter |
| `/s:<index>` | Select console system by index (skips interactive menu) |

> **Note:** Virtual CUE/BIN mode is now selected from the interactive system menu (see [Interactive Mode](#interactive-mode)). A single "Generic CUE/BIN" option works for all CD-based images. When using `/s:`, only filesystem or single-file modes are available.

#### Console System Indices

When using `/s:<index>`, the following indices are available:

| Index | System |
|:------|:-------|
| 1 | Amiga CD |
| 2 | Amiga CD32 |
| 3 | Philips CD-i |
| 4 | Sega Dreamcast |
| 5 | SNK Neo Geo CD |
| 6 | NEC PC Engine CD |
| 7 | NEC PC-FX |
| 8 | PlayStation 1 |
| 9 | PlayStation 2 |
| 10 | PlayStation 3 |
| 11 | PS3 (Single File) |
| 12 | Sony PSP |
| 13 | Sega Saturn |
| 14 | Sega Genesis CD |
| 15 | 3DO Interactive Multiplayer |
| 16 | Microsoft Xbox |
| 17 | Microsoft Xbox 360 |
| 18 | Xbox (Single File) |
| 19 | Generic ISO 9660 |

#### Interactive Mode

If no console type is specified via `/s:`, the mounter displays an interactive menu:

```
SELECT CONSOLE SYSTEM

  [1] Amiga CD
  [2] Amiga CD32
  [3] Philips CD-i
  [4] Sega Dreamcast
  [5] SNK Neo Geo CD
  [6] NEC PC Engine CD
  [7] NEC PC-FX
  [8] PlayStation 1
  [9] PlayStation 2
  [10] PlayStation 3
  [11] PS3 (Single File)
  [12] Sony PSP
  [13] Sega Saturn
  [14] Sega Genesis CD
  [15] 3DO Interactive Multiplayer
  [16] Microsoft Xbox
  [17] Microsoft Xbox 360
  [18] Xbox (Single File)
  [19] Generic ISO 9660
  [20] Generic CUE/BIN (Raw CD Image)
Enter choice: _
```

Selecting "Generic CUE/BIN (Raw CD Image)" mounts the CHD with virtual `.cue`/`.bin` files instead of the disc's directory tree. This single option works for all CD-based consoles. All tracks are included automatically — no track selection is needed.

If the CHD contains multiple tracks and a filesystem mode is chosen, a second menu appears:
```
CHD TRACK LIST

  [1] Track 1 - MODE1 [DATA] - 300000 sectors
  [2] Track 2 - AUDIO [AUDIO] - 5000 sectors

Select track to mount: _
```

#### Virtual CUE/BIN Mode

Select "Generic CUE/BIN (Raw CD Image)" from the interactive system menu to mount any CD-based CHD with virtual `.cue` and `.bin` files. No track selection is needed — all tracks are included automatically.

For example, to mount a PCE CD game as CUE/BIN, run without `/s:` to get the interactive menu and select "Generic CUE/BIN (Raw CD Image)":

```cmd
CHDMounter.exe /a "Ys Book I & II (Japan).chd" X:
```

The mounted drive will contain only the virtual CUE/BIN pair (the disc's directory tree is not exposed):
```
X:\
  Ys Book I & II (Japan).cue   (1 KB — CUE sheet)
  Ys Book I & II (Japan).bin   (650 MB — raw disc image)
```

If the CHD stores subchannel data (unitbytes > 2352), the BIN output strips it automatically to produce standard 2352-byte raw sectors (sync + header + data + EDC/ECC), ensuring compatibility with emulators and burning tools.

These files work with any emulator, burning tool, or software that reads standard CUE/BIN pairs.

#### Unmounting

Press `Ctrl+C` in the console window where `CHDMounter.exe` is running. The drive will be cleanly unmounted.

## Emulator Frontend Integration

### Quick Integration Guide

For frontend developers who need to mount CHDs programmatically:

```cmd
REM 1. Mount the CHD (returns immediately once mounted)
CHDMounter.exe /l /a /s:9 "C:\Games\game.chd"

REM 2. The game is now accessible at the auto-selected drive letter
REM    e.g., X:\ contains the disc filesystem

REM 3. Point your emulator to the mounted drive
REM    For PS2: emulator.exe --disc "X:"

REM 4. When done, kill the CHDMounter process
taskkill /IM CHDMounter.exe /F
```

**Recommended approach for frontends:** Use `/a` (auto-drive) and `/l` (launch explorer) or skip `/l` if you don't need Explorer. Use `/s:<index>` to avoid the interactive menu. Parse the console output or check mounted drives to determine the assigned drive letter.

### xemu (Xbox)

xemu expects a raw disc image. Use the **Xbox (Single File)** parser mode (index 18):

```cmd
CHDMounter.exe /s:18 /a "C:\Games\game.chd"
```

The mounted drive will contain a single file `image.iso`. Point xemu to it:
```
Settings > Hard Disk Image: X:\image.iso
```

The VFS serves raw CHD sector data sequentially, making `image.iso` functionally equivalent to a raw ISO rip.

### RPCS3 (PS3)

RPCS3 can load from a mounted disc. Use the **PS3 (Single File)** parser mode (index 11):

```cmd
CHDMounter.exe /s:11 /a "C:\Games\game.chd"
```

Point RPCS3 to the mounted drive:
```
File > Boot Game > X:\
```

Or use the full filesystem mode (index 10) to expose the UDF directory tree directly:
```cmd
CHDMounter.exe /s:10 /a "C:\Games\game.chd"
```

### RetroArch / Mednafen / Any Emulator

For emulators that read CUE/BIN or expect a mounted disc filesystem:

**Option A — Virtual CUE/BIN (for CD-based emulators):**

Run without `/s:` to access the interactive menu, then select "Generic CUE/BIN (Raw CD Image)":
```cmd
CHDMounter.exe /a "C:\Games\game.chd"
```
Then point the emulator to `X:\game.cue`.

**Option B — Filesystem Mount (for emulators that browse disc contents):**
```cmd
CHDMounter.exe /s:9 /a "C:\Games\game.chd"
```
Then point the emulator to the mounted drive letter.

### Custom Frontend Integration (Developer Guide)

#### Architecture for Integration

A typical emulator frontend integration follows this flow:

```
Frontend Application
        │
        ├── Launch CHDMounter.exe with appropriate flags
        │         │
        │         ▼
        │   CHD mounted as virtual drive (e.g., X:\)
        │
        ├── Detect mount completion (poll for drive letter or parse output)
        │
        ├── Launch emulator with path to mounted drive
        │         │
        │         ▼
        │   Emulator reads disc image from virtual drive
        │
        └── On emulator exit, kill CHDMounter.exe process
```

#### Step-by-Step Integration

**1. Determine Console Type**

Map your frontend's system detection to CHDMounter's console indices. If your frontend already identifies the console system, pass it via `/s:<index>`. If not, you can either:
- Let the user select the system (omit `/s:` and handle the interactive menu)
- Implement your own detection logic and map to the index table above

**2. Mount the CHD**

Launch `CHDMounter.exe` as a subprocess:

```cpp
// C++ example (Windows)
STARTUPINFOW si = { sizeof(si) };
PROCESS_INFORMATION pi = {};
std::wstring cmd = L"CHDMounter.exe /s:9 /a \"C:\\Games\\game.chd\"";
CreateProcessW(nullptr, cmd.data(), nullptr, nullptr, FALSE, 0, nullptr, nullptr, &si, &pi);
```

```python
# Python example
import subprocess
proc = subprocess.Popen(
    ["CHDMounter.exe", "/s:9", "/a", r"C:\Games\game.chd"],
    stdout=subprocess.PIPE, stderr=subprocess.PIPE
)
```

**3. Detect the Mounted Drive Letter**

When using `/a`, the mounter auto-selects an available drive letter. To detect it:

- **Method A — Poll logical drives**: Before mounting, snapshot `GetLogicalDrives()`. After mounting, compare to find the new drive.
- **Method B — Parse mounter output**: The mounter prints status to stdout. Capture and parse it.
- **Method C — Fixed drive letter**: Specify a drive letter explicitly instead of using `/a`: `CHDMounter.exe /s:9 "game.chd" Z:`

**4. Launch the Emulator**

Point the emulator to the mounted drive:

```cpp
// Launch emulator with mounted drive
std::wstring emuCmd = L"emulator.exe --disc \"" + driveLetter + L":\"";
CreateProcessW(nullptr, emuCmd.data(), nullptr, nullptr, FALSE, 0, nullptr, nullptr, &si, &pi);
```

**5. Cleanup on Exit**

When the emulator closes, terminate the mounter process:

```cpp
TerminateProcess(pi.hProcess, 0);
WaitForSingleObject(pi.hProcess, 5000); // Wait up to 5 seconds
CloseHandle(pi.hProcess);
CloseHandle(pi.hThread);
```

Or use `taskkill`:
```cmd
taskkill /IM CHDMounter.exe /F
```

#### Batch Processing

To mount multiple CHDs (e.g., for multi-disc games), use different drive letters:

```cmd
CHDMounter.exe /s:9 "disc1.chd" Z:
CHDMounter.exe /s:9 "disc2.chd" Y:
```

#### Error Handling

- If `CHDMounter.exe` returns exit code `1`, the mount failed (invalid CHD, unsupported format, or no drive letter available).
- If the drive letter doesn't appear within 5 seconds, the Dokany driver may not be installed.
- The mounter requires **administrator privileges** on some systems if Dokany is not already installed for the current user.

### Command-Line Reference for Integration

```cmd
REM Mount with system selection menu (includes CUE/BIN options)
CHDMounter.exe "game.chd" X:

REM Mount PS2 game, auto-drive, open Explorer
CHDMounter.exe /s:9 /a /l "game.chd"

REM Mount Xbox game as single file for xemu
CHDMounter.exe /s:18 /a "game.chd"

REM Mount PS3 game as single file for RPCS3
CHDMounter.exe /s:11 /a "game.chd"

REM Mount Dreamcast game as virtual CUE/BIN (use interactive menu)
CHDMounter.exe /a "game.chd"

REM Debug mode (shows Dokany filesystem operations)
CHDMounter.exe /d /s:9 /a "game.chd"
```

### Exit Codes

| Code | Meaning |
|:-----|:--------|
| `0` | Success (mount completed; process runs until Ctrl+C) |
| `1` | Failure (invalid arguments, CHD open error, mount error) |

## Dependencies

| Library | Purpose | License | Location |
|:--------|:--------|:--------|:---------|
| [libchdr](https://github.com/rtissera/libchdr) | CHD decompression (CHDv1–5, Huffman/FLAC/zlib/LZMA) | BSD-3 | `thirdparty/libchdr/` |
| [Dear ImGui](https://github.com/ocornut/imgui) | Immediate-mode GUI (Win32 + DirectX11 backend) | MIT | `thirdparty/imgui/` |
| [Dokany v2](https://github.com/dokan-dev/dokany) | User-mode filesystem driver for virtual drives | LGPL/MIT | `thirdparty/Dokan/` |
| [lib9660](https://github.com/Lichtso/lib9660) | Reference ISO9660 parser for test validation | — | `thirdparty/lib9660/` |

### System Requirements

- **Windows 10/11** (x64 or ARM64)
- **Dokany v2.1.0+** driver installed (required for the Mounter; download from [dokan-dev/dokany](https://github.com/dokan-dev/dokany/releases))
- No runtime dependencies — all libraries are statically linked

## License

This project is licensed under the **GNU General Public License v3.0** (GPL-3.0). See [LICENSE](LICENSE) for the full license text.

```
Copyright (C) 2026 PureLogicCode

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
```

### Third-Party License Compatibility

All bundled third-party libraries are compatible with GPL-3.0:
- **libchdr** — BSD-3-Clause (GPL-compatible)
- **Dear ImGui** — MIT (GPL-compatible)
- **Dokany v2** — LGPL/MIT (dynamically linked; GPL-compatible)
- **lib9660** — Used only in tests
