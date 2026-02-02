# Simple Launcher Wiki

Welcome to the official documentation for **Simple Launcher**, a powerful, flexible, open-source frontend for managing and launching emulators, retro games, and modern PC titles. Despite its name, Simple Launcher offers advanced features for both beginners (via **Easy Mode**) and power users (via **Expert Mode** manual configuration).

This wiki provides **comprehensive, step-by-step guidance** to get you up and running, from installation to advanced customization. Whether you're emulating retro consoles or launching Steam/Epic games, Simple Launcher streamlines your library.

## Table of Contents
1. [Introduction](#1-introduction)
2. [Getting Started](#2-getting-started)
   * [System Requirements](#system-requirements)
   * [Required Dependencies](#required-dependencies)
   * [Installation](#installation)
   * [First Launch](#first-launch)
3. [Core Concepts](#3-core-concepts)
   * [Systems](#systems)
   * [Game Files (ROMs/ISOs)](#game-files-romsisos)
   * [Emulators](#emulators)
   * [Media and Folders](#media-and-folders)
4. [Configuring Systems](#4-configuring-systems)
   * [Easy Mode: Guided Setup](#easy-mode-guided-setup)
     * [Step-by-Step Walkthrough](#step-by-step-walkthrough)
   * [Expert Mode: Manual Configuration (`system.xml`)](#expert-mode-manual-configuration-systemxml)
     * [Full `system.xml` Reference](#full-systemxml-reference)
   * [Path Placeholders](#path-placeholders)
5. [Main Window and UI](#5-main-window-and-ui)
   * [System Selection Screen](#system-selection-screen)
   * [Game Browsing](#game-browsing)
   * [Navigation Panel](#navigation-panel)
   * [View Modes (Grid vs. List)](#view-modes-grid-vs-list)
   * [Context Menu](#context-menu)
6. [Key Features](#6-key-features)
   * [Game Launching & File Mounting](#game-launching--file-mounting)
   * [Group Files by Folder](#group-files-by-folder)
   * [Favorites Manager](#favorites-manager)
   * [Play History](#play-history)
   * [Global Search](#global-search)
   * [Global Statistics](#global-statistics)
   * [Image Pack Downloader](#image-pack-downloader)
   * [Controller Support](#controller-support)
   * [Fuzzy Image Matching](#fuzzy-image-matching)
   * [Sound Configuration](#sound-configuration)
   * [RetroAchievements Integration](#retroachievements-integration)
     * [Setup](#setup)
     * [Features](#features)
   * [Automatic Updates](#automatic-updates)
7. [Bundled Tools](#7-bundled-tools)
   * [Batch Converters](#batch-converters)
   * [Batch File Creators](#batch-file-creators)
   * [Metadata Tools](#metadata-tools)
8. [Advanced Topics](#8-advanced-topics)
   * [Custom `system.xml` Editing](#custom-systemxml-editing)
   * [Troubleshooting Common Issues](#troubleshooting-common-issues)
9. [Dependencies & Credits](#9-dependencies--credits)

---

## 1. Introduction

**Simple Launcher** is a multi-platform frontend designed for **retro emulation** and **modern PC gaming**. It excels at organizing large libraries, launching games with precise emulator parameters, and handling complex setups like file mounting and RetroAchievements tracking.

### Key Strengths
- **Dual Configuration Modes**: **Easy Mode** downloads and configures emulators automatically; **Expert Mode** offers full XML control.
- **On-the-Fly Mounting**: Launch from `.zip`, `.iso`, `.xiso` without extraction (requires **Dokan**).
- **Rich Media**: Supports covers, snapshots, videos, manuals via dedicated folders.
- **Modern Features**: RetroAchievements integration, play history, global search, favorites, statistics.
- **PC Game Scanning**: Auto-detects Steam, Epic, GOG, Battle.net, Microsoft Store games.
- **Controller Navigation**: Full Xbox/PS support with deadzone tuning.
- **Themes & Customization**: 20+ accent colors, light/dark modes, grid/list views, zoom.

**Target Users**:
- Beginners: Easy Mode handles everything.
- Experts: Granular control, scripting, multi-emulator per system.

---

## 2. Getting Started

### System Requirements
| Requirement | Details |
|-------------|---------|
| **OS** | Windows 7+ (tested on 10/11) |
| **.NET** | .NET 10 Runtime (installer bundles it) |
| **RAM** | 4GB+ (8GB recommended for large libraries) |
| **Disk** | Enough space for ROMs/emulators (tools need ~5GB temp) |
| **GPU** | Modern GPU for shaders (OpenGL/Vulkan/DirectX) |

### Required Dependencies
- **Dokan** (mandatory for ZIP/XISO mounting):
  1. Download from [Dokan GitHub Releases](https://github.com/dokan-dev/dokany/releases).
  2. Install **Dokan Library** (not driver-only).
  3. Restart PC after install.
- **RetroAchievements** (optional): Username/API key for achievements.

### Installation (Step-by-Step)
1. **Download**:
   - Visit [GitHub Releases](https://github.com/drpetersonfernandes/SimpleLauncher/releases).
   - Choose `release_x64.zip` (Intel/AMD) or `release_arm64.zip` (ARM like Snapdragon X).

2. **Extract**:
   - Unzip **entire folder** to writable location: `C:\Users\[YourName]\Documents\SimpleLauncher` or `D:\Games\SimpleLauncher`.
   - **Avoid**: `C:\Program Files` (permission issues).

3. **Verify Dependencies**:
   - Install Dokan if using mounting.
   - Run `SimpleLauncher.exe`.

4. **First Launch**:
   - App auto-creates `system.xml`, scans for Windows games.
   - **Easy Mode prompt**: Add first system (downloads emulator/core/images).

### First Launch Walkthrough
1. **No Systems**: "Add via Easy Mode?" → Yes → Select console (e.g., SNES) → Download emulator/image pack → Add.
2. **System Selected**: Browse games, launch.
3. **Tray Icon**: Minimize to tray; double-click restore.

---

## 3. Core Concepts

### Systems
A **System** = console/platform (e.g., "Nintendo SNES"). Configure paths, emulators, media.

**Add Systems**:
- **Easy Mode**: Guided (download emulators/cores/images).
- **Expert Mode**: Fully edit `system.xml`.

### Game Files (ROMs/ISOs)
Supported: `.zip`/`.7z`/`.rar` (auto-extract), `.iso`/`.chd`/`.rvz`, `.exe`/`.bat`/`.lnk`.

**Launching**:
- Direct: `.exe`/`.bat`/`.lnk`.
- Extract: Compressed → temp folder → launch inner file.
- Mount: ZIP/ISO/XISO → virtual drive (Dokan required).

### Emulators
External programs (provide your own). Configure multiple per system.

**Parameters Example** (RetroArch SNES):
```
-L "%EMULATORFOLDER%\cores\snes9x_libretro.dll" -f "%1"
```
- `%1` = game path.
- Placeholders: `%BASEFOLDER%`, `%SYSTEMFOLDER%`, `%EMULATORFOLDER%`.

### Media and Folders
**Required Structure** (auto-created):
```
SimpleLauncher/
├── audio/                # Sounds
├── emulators/            # Emulator exes
├── images/
│   ├── systems/          # Logos (system select)
│   └── [SystemName]/     # Covers
├── roms/
│   └── [SystemName]/     # Games (multi-folder OK)
├── title_snapshots/      # Title screens
├── gameplay_snapshots/   # In-game snaps
├── videos/               # Videos
├── manuals/              # PDFs
├── walkthrough/          # Guides
├── cabinets/             # Cabinet art
├── carts/                # Cartridge art
├── flyers/               # Flyers
├── pcbs/                 # PCBs
├── tools/                # Bundled tools
└── SimpleLauncher.exe
```

---

## 4. Configuring Systems

### Easy Mode: Guided Setup
**Recommended for beginners**.

1. **Menu**: `Edit System → Easy Mode`.
2. **Select System**: Dropdown (pre-configured: emulators/cores/images).
3. **ROM Folder** (optional): Browse or use default `roms/[System]`.
4. **Install**:
   - **Emulator**: Download/install.
   - **Core** (RetroArch): Download if needed.
   - **Image Pack**: Covers/snapshots.
5. **Add System**: Saves to `system.xml`.

### Expert Mode: Full Configuration (`system.xml`)
**Full control**.

1. **Menu**: `Edit System → Expert Mode`.
2. Edit fields; save → auto-creates folders.

#### Full `system.xml` Reference
```xml
<SystemConfigs>
  <SystemConfig>
    <SystemName>Nintendo SNES</SystemName> <!-- Unique display name -->
    <SystemFolders> <!-- Multiple OK -->
      <SystemFolder>.\roms\SNES</SystemFolder> <!-- Or absolute: D:\Games\SNES -->
      <SystemFolder>%BASEFOLDER%\extra_roms\SNES</SystemFolder>
    </SystemFolders>
    <SystemImageFolder>.\images\Nintendo SNES</SystemImageFolder>
    <SystemIsMAME>true</SystemIsMAME> <!-- Enables MAME desc/sort/grouping -->
    <FileFormatsToSearch>zip,smc,sfc</FileFormatsToSearch> <!-- No dots -->
    <ExtractFileBeforeLaunch>true</ExtractFileBeforeLaunch>
    <FileFormatsToLaunch>smc,sfc</FileFormatsToLaunch> <!-- Inner archive files -->
    <GroupByFolder>false</GroupByFolder> <!-- MAME multi-file sets -->
    <Emulators>
      <Emulator>
        <EmulatorName>RetroArch Snes9x</EmulatorName>
        <EmulatorLocation>.\emulators\RetroArch\retroarch.exe</EmulatorLocation>
        <EmulatorParameters>-L "%EMULATORFOLDER%\cores\snes9x_libretro.dll" "%1" -f</EmulatorParameters>
        <ReceiveANotificationOnEmulatorError>true</ReceiveANotificationOnEmulatorError>
      </Emulator>
    </Emulators>
  </SystemConfig>
</SystemConfigs>
```

**Validation**: Red text for invalid paths; fix before save.

### Path Placeholders
Portable configs:
- `%BASEFOLDER%`: App dir (`SimpleLauncher/`).
- `%SYSTEMFOLDER%`: **First** `<SystemFolder>`.
- `%EMULATORFOLDER%`: Emulator `.exe` dir.

**Example**:
```
<EmulatorParameters>-L "%EMULATORFOLDER%\cores\snes9x.dll" "%1"</EmulatorParameters>
```

---

## 5. Main Window and UI

### System Selection Screen
Grid of system logos. Click → loads games.

**No Systems**: "Configure via Edit System".

### Game Browsing
- **Letter Filter**: Jump to A-Z/0-9.
- **System/Emulator Dropdowns**: Switch.
- **Search**: `AND`/`OR` (e.g., "mario OR luigi").

**Pagination**: Next/Prev for large lists.

**MAME Toggle**: Filename ↔ description sort.

### Navigation Panel (Left Sidebar)
| Button | Action |
|--------|--------|
| 🔄 Restart | Reload systems |
| 🔍 Global Search | Cross-system search |
| ❤️ Favorites | Favorites window |
| ⏰ Play History | Play tracking |
| ⚙️ Expert Mode | Edit `system.xml` |
| 📱 Toggle View | Grid ↔ List |
| 📐 Aspect Ratio | Cycle ratios |
| ➕ Zoom In/Out | Thumbnail size (+Ctrl+Wheel) |

### View Modes (Grid vs. List)
- **Grid**: Visual covers; overlays (RA/Video/Info).
- **List**: Table (filename/desc/folder/size/plays/time); preview pane.

### Context Menu (Right-Click Game)
- Launch / Favorites / Achievements
- Video/Info Links / ROM History
- Media Viewers (cover/snap/manual/etc.)
- **Take Screenshot**: Launch → select window → capture cover.
- Delete Game/Cover.

---

## 6. Key Features

### Game Launching & File Mounting
**Seamless**:
1. **Direct**: `.exe`/`.bat`/`.lnk`.
2. **Pass-Through**: ROMs/ISOs to emulator.
3. **Extract**: ZIP/7Z/RAR → temp → launch inner.
4. **Mount** (Dokan req.):

   | Emulator | Supported |
      |----------|-----------|
   | RPCS3 (PS3) | ISO/ZIP → EBOOT.BIN |
   | Cxbx (Xbox) | XISO → default.xbe |
   | ScummVM | ZIP → auto-detect |
   | XBLA (360) | ZIP → nested exe |

**Steps**: Right-click → Launch (auto-handles).

### Group Files by Folder
**MAME-only**: Treats subfolders as single games.

**Enable**: Expert Mode → `<GroupByFolder>true</GroupByFolder>`.
- Folders → single UI entry.
- Launch → passes folder path.

**Warning**: Non-MAME = launch fail.

### Favorites Manager
1. Right-click → Add/Remove.
2. `Favorites` window: Launch/manage.
- Stored: `favorites.dat` (MessagePack).

### Play History
Auto-tracks: plays/time/last played.
- `Play History` window.
- Stored: `playhistory.dat` (MessagePack).

### Global Search
`Global Search`: Cross-system; `AND`/`OR`.

### Global Statistics
`Tools → Global Stats`: Totals (systems/emus/games/images/space).

### Image Pack Downloader
`Edit System → Download Image Pack`: Bulk covers.

### Controller Support
1. `Options → Gamepad → Enable`.
2. Deadzone: `Set Gamepad Dead Zone`.
- Full UI navigation (Xbox/PS).

### Fuzzy Image Matching
Matches covers despite filename diffs.
- Toggle: `Options → Fuzzy → Enable`.
- Threshold: `Set Threshold` (0.7-0.95).

### Sound Configuration
`Options → Sound`:
- Enable/disable notifications.
- Custom MP3/test/reset.

### RetroAchievements Integration
**Setup**:
1. `Options → RA Settings`: Username/API key.
2. Auto-injects to PCSX2/DuckStation/etc.

**Features**:
- Game progress/leaderboards.
- Overlay buttons.
- Hashing for complex files (PS1/Saturn/DC).

### Automatic Updates
Startup check; auto-download/install via `Updater.exe`.

---

## 7. Bundled Tools
**Tools Menu**:

| Tool | Purpose |
|------|---------|
| **Batch Iso → XISO** | Xbox ISO → XISO (+verify) |
| **Batch → CHD** | Discs → CHD (+verify) |
| **Batch → Compressed** | Files/folders → ZIP/7Z (+verify) |
| **Batch → RVZ** | GC/Wii → RVZ (+test) |
| **Batch PS3/ScummVM/Model3/Windows/XBLA** | Auto `.bat` scripts |
| **FindRomCover** | Organize covers (MAME desc) |
| **RomValidator** | No-Intro DAT validation |

---

## 8. Advanced Topics

### Troubleshooting Common Issues
| Issue | Fix |
|-------|-----|
| **No Launch** | Check paths/params; `Expert Mode` validate. BIOS? |
| **Mount Fails** | Install Dokan; admin? |
| **No Covers** | Image packs; fuzzy match. |
| **RA No Track** | Hash match; inject creds. |
| **Crashes** | `error_user.log`; writable folder. |

**Logs**: `error_user.log`; debug: `SimpleLauncher.exe -debug`.

---

## 9. Dependencies & Credits
- **Dokan**: Mounting.
- **NuGet**: MahApps.Metro (UI), MessagePack (serialize), SharpDX (input).
- **Bundled**: 7z.dll, chdman.exe, DolphinTool.exe, RAHasher.exe.

**License**: GPLv3. Source: [GitHub](https://github.com/drpetersonfernandes/SimpleLauncher).

**Help**: [Issues](https://github.com/drpetersonfernandes/SimpleLauncher/issues).

```