# Simple Launcher Wiki

Welcome to the official documentation for Simple Launcher, a powerful and flexible open-source frontend for emulators.

## Table of Contents
1.  [Introduction](#1-introduction)
2.  [Getting Started](#2-getting-started)
  *   [System Requirements](#system-requirements)
  *   [Required Dependencies](#required-dependencies)
  *   [Installation](#installation)
3.  [Core Concepts](#3-core-concepts)
  *   [Systems](#systems)
  *   [Game Files (ROMs/ISOs)](#game-files-roms-isos)
  *   [Emulators](#emulators)
  *   [Media and Folders](#media-and-folders)
4.  [Configuring Systems](#4-configuring-systems)
  *   [Easy Mode: Guided Setup](#easy-mode-guided-setup)
  *   [Expert Mode: Manual Configuration (`system.xml`)](#expert-mode-manual-configuration-systemxml)
  *   [Path Placeholders](#path-placeholders)
5.  [Main Window and UI](#5-main-window-and-ui)
  *   [System Selection Screen](#system-selection-screen)
  *   [Game Browsing](#game-browsing)
  *   [Navigation Panel](#navigation-panel)
  *   [View Modes (Grid vs. List)](#view-modes-grid-vs-list)
  *   [Context Menu](#context-menu)
6.  [Key Features](#6-key-features)
  *   [Game Launching & File Mounting](#game-launching--file-mounting)
  *   [Favorites Manager](#favorites-manager)
  *   [Play History](#play-history)
  *   [Global Search](#global-search)
  *   [Global Statistics](#global-statistics)
  *   [Image Pack Downloader](#image-pack-downloader)
  *   [Controller Support](#controller-support)
  *   [Automatic Updates](#automatic-updates)
7.  [Bundled Tools](#7-bundled-tools)
8.  [Troubleshooting](#8-troubleshooting)
9.  [Dependencies & Credits](#9-dependencies--credits)

---

## 1. Introduction

**Simple Launcher** is a feature-rich, open-source frontend for managing and launching your retro game collection. While its name implies simplicity, it packs powerful features for both beginners and advanced users.

**Key Features:**
- **Easy and Expert Configuration:** Use a guided wizard to add systems or dive deep into manual XML configuration.
- **On-the-Fly File Mounting:** Launch games directly from `.zip`, `.iso`, and `.xiso` files without manual extraction.
- **Extensive Customization:** Control themes, languages, button sizes, aspect ratios, and more.
- **Rich Media Support:** Display covers, snapshots, videos, manuals, and other media for your games.
- **Powerful Game Management:** Keep track of your favorites, view your play history, and use a global search to find any game across all your systems.
- **Bundled Conversion Tools:** A suite of utilities to help you convert, verify, and manage your game files.
- **Modern UI:** A clean, responsive interface built with WPF and MahApps.Metro.

## 2. Getting Started

### System Requirements
- **Operating System:** Windows 7 or newer.
- **.NET Framework:** .NET 9 (The installer will handle this).

### Required Dependencies
For full functionality, especially the on-the-fly file mounting, you must install **Dokan**.
- **Dokan:** A user-mode file system for Windows that allows Simple Launcher to mount archive files as virtual drives.
  - **Download Link:** [https://github.com/dokan-dev/dokany/releases](https://github.com/dokan-dev/dokany/releases)
  - Please install Dokan before using features that rely on mounting ZIP or ISO files.

### Installation
1.  Download the latest release from the [GitHub Releases page](https://github.com/drpetersonfernandes/SimpleLauncher/releases).
2.  Unzip the entire application folder to a **writable location** on your computer, such as `C:\Users\YourName\Documents\SimpleLauncher` or `D:\Emulation\SimpleLauncher`.
3.  **Do not** place it in restricted folders like `C:\Program Files` or `C:\Program Files (x86)`, as Simple Launcher needs to write configuration files (`system.xml`, `settings.xml`, etc.) and logs in its own directory.
4.  Run `SimpleLauncher.exe`.

## 3. Core Concepts

### Systems
A "System" represents a gaming console or platform you want to emulate (e.g., "Nintendo SNES", "Sega Genesis", "Arcade"). Each system has its own configuration, including its game folder, emulators, and media paths.

### Game Files (ROMs/ISOs)
These are the actual files for your games. Simple Launcher supports a wide variety of formats, from single ROM files (`.smc`, `.nes`) to disc images (`.iso`, `.chd`, `.rvz`) and compressed archives (`.zip`, `.7z`, `.rar`).

### Emulators
These are the external programs that run your games. Simple Launcher is a frontend, meaning it organizes your games and tells the emulator which game to launch. You must provide your own emulators.

### Media and Folders
Simple Launcher is designed to be a highly visual frontend. It expects a specific folder structure within its main directory to automatically find and display media for your games.

The default folder structure is:
```
SimpleLauncher/
├── audio/
├── emulators/
├── images/
│   ├── systems/
│   └── [SystemName]/
├── roms/
│   └── [SystemName]/
├── title_snapshots/
│   └── [SystemName]/
├── gameplay_snapshots/
│   └── [SystemName]/
├── videos/
│   └── [SystemName]/
├── manuals/
│   └── [SystemName]/
├── walkthrough/
│   └── [SystemName]/
├── cabinets/
│   └── [SystemName]/
├── carts/
│   └── [SystemName]/
├── flyers/
│   └── [SystemName]/
├── pcbs/
│   └── [SystemName]/
├── tools/
└── SimpleLauncher.exe
```
- **`roms/[SystemName]`**: The default location for your game files for a specific system.
- **`images/[SystemName]`**: The default location for your game cover art.
- **`images/systems`**: The location for system logos displayed on the selection screen.
- Other folders (`videos`, `manuals`, etc.) are for additional media accessible via the context menu.

## 4. Configuring Systems

You can add and edit systems in two ways: **Easy Mode** for a guided setup and **Expert Mode** for detailed manual control.

### Easy Mode: Guided Setup
This is the recommended method for new users.
1.  Go to **Edit System -> Easy Mode**.
2.  **Choose a System:** Select a pre-configured system from the dropdown list. These presets contain recommended settings and download links.
3.  **Choose a ROMs Folder (Optional):** You can specify a folder for your games. If you leave it blank, a default folder will be created for you inside `SimpleLauncher\roms\[SystemName]`.
4.  **Install Components:**
  - **Download Emulator:** Downloads the recommended emulator for the system.
  - **Download Core (for RetroArch):** Downloads the necessary core if the emulator is RetroArch.
  - **Download Image Pack:** Downloads a pre-packaged set of cover art for the system.
5.  **Add System:** Once the required components (Emulator and Core, if applicable) are downloaded, click this button to save the configuration to `system.xml`.

### Expert Mode: Manual Configuration (`system.xml`)
This mode gives you full control over the `system.xml` file, which stores all system configurations. Go to **Edit System -> Expert Mode**.

Here's a breakdown of the `system.xml` structure and fields:

| Field | Description | Example |
| :--- | :--- | :--- |
| **SystemName** | The display name of the system (e.g., "Nintendo SNES"). This must be unique. | `Nintendo SNES` |
| **SystemFolder** | The path to the folder containing the game files for this system. Can be absolute or relative. | `.\roms\SNES` or `D:\Games\SNES` |
| **SystemImageFolder** | The path to the folder containing the cover art for this system. | `.\images\Nintendo SNES` |
| **SystemIsMAME** | `true` or `false`. If `true`, the launcher will use the MAME description for display and search instead of just the filename. | `true` |
| **FileFormatsToSearch** | A comma-separated list of file extensions to look for in the `SystemFolder`. Do not include the dot. | `zip, 7z, smc, sfc` |
| **ExtractFileBeforeLaunch** | `true` or `false`. If `true`, the launcher will extract compressed files (`.zip`, `.7z`, `.rar`) to a temporary folder before launching. | `true` |
| **FileFormatsToLaunch** | Required if `ExtractFileBeforeLaunch` is `true`. A comma-separated list of extensions to look for *inside* the extracted archive. The first one found will be launched. | `smc, sfc` |
| **EmulatorName** | A unique name for this emulator configuration (e.g., "RetroArch Snes9x", "ZSNES"). | `RetroArch Snes9x` |
| **EmulatorLocation** | The path to the emulator's executable file. | `.\emulators\RetroArch\retroarch.exe` |
| **EmulatorParameters** | The command-line arguments to pass to the emulator. Use `%ROM%` as a placeholder for the game file path. | `-L "cores\snes9x_libretro.dll" "%ROM%"` |
| **ReceiveANotification...** | `true` or `false`. If `true`, Simple Launcher will show a message if the emulator exits with an error code. | `true` |

### Path Placeholders
To make your configuration portable, you can use special placeholders in path fields:
- **`%BASEFOLDER%`**: Represents the root directory where `SimpleLauncher.exe` is located.
- **`%SYSTEMFOLDER%`**: Represents the path defined in the `<SystemFolder>` tag for the current system.
- **`%EMULATORFOLDER%`**: Represents the directory where the emulator's executable is located.

**Example Usage in `system.xml`:**
```xml
<EmulatorLocation>%BASEFOLDER%\emulators\RetroArch\retroarch.exe</EmulatorLocation>
<EmulatorParameters>-L "%EMULATORFOLDER%\cores\snes9x_libretro.dll" -f "%ROM%"</EmulatorParameters>
```

## 5. Main Window and UI

### System Selection Screen
When you first start Simple Launcher, or when no system is selected, you'll see a grid of all configured systems. Click a system's logo to load its game library.

### Game Browsing
- **Top Bar:** Use the letter/number filter bar to quickly jump to games starting with that character.
- **System & Emulator Dropdowns:** Select the system you want to browse and the emulator you want to use.
- **Search Bar:** Search for games within the currently selected system.
- **Pagination:** Use the "Next" and "Previous" buttons at the bottom to navigate through large game lists.

### Navigation Panel
The vertical panel on the left provides quick access to key features:
- **Restart:** Reloads the UI to the system selection screen.
- **Global Search:** Opens the Global Search window.
- **Favorites:** Opens the Favorites window.
- **Play History:** Opens the Play History window.
- **Expert Mode:** Opens the system configuration window.
- **Toggle View Mode:** Switches between Grid and List views.
- **Toggle Aspect Ratio:** Cycles through different button aspect ratios in Grid View.
- **Zoom In/Out:** Changes the size of game thumbnails in Grid View.

### View Modes (Grid vs. List)
- **Grid View:** A visual layout showing game covers as clickable buttons.
- **List View:** A detailed table showing filename, MAME description, file size, play count, and total playtime.

### Context Menu
Right-click on any game in either Grid or List view to access a rich context menu with many options:
- **Launch Game:** Starts the game with the selected emulator.
- **Add/Remove from Favorites:** Manages your favorites list.
- **Open Video/Info Link:** Searches YouTube or IGDB for the game.
- **Open ROM History:** Shows historical information about the game from `history.xml`.
- **View Media:** Opens viewers for associated media like covers, snapshots, manuals, etc.
- **Take Screenshot:** Launches the game and prompts you to select the game window to capture a new cover image.
- **Delete Game:** Permanently deletes the game file from your hard drive.

## 6. Key Features

### Game Launching & File Mounting
Simple Launcher's most powerful feature is its ability to handle different file types seamlessly.
- **Direct Launch:** `.exe`, `.bat`, and `.lnk` files are launched directly.
- **Standard Launch:** Uncompressed ROMs and disc images are passed to the configured emulator.
- **Extract Before Launch:** If enabled for a system, `.zip`, `.7z`, or `.rar` files are automatically extracted to a temporary folder before launching.
- **On-the-Fly Mounting (Requires Dokan):**
  - **RPCS3 (PS3):** Mounts `.iso` or `.zip` files to a virtual drive and automatically finds and launches the `EBOOT.BIN`.
  - **Cxbx-Reloaded (Xbox):** Mounts `.xiso` files and launches `default.xbe`.
  - **ScummVM:** Mounts `.zip` files and launches the game via ScummVM's auto-detect feature.
  - **XBLA (Xbox 360):** Mounts `.zip` files and finds the game executable within the required nested folder structure.

### Favorites Manager
- Access via **Favorites -> Favorites** or the heart icon in the navigation panel.
- Add games to your favorites from the context menu.
- View, launch, and manage all your favorite games from a single window.
- Favorites are stored in `favorites.dat`.

### Play History
- Access via **Play History -> Play History** or the clock icon in the navigation panel.
- Automatically tracks every game you play, recording the number of plays, total playtime, and last played date/time.
- Data is stored in `playhistory.dat`.

### Global Search
- Access via **Global Search -> Global Search** or the magnifying glass icon in the navigation panel.
- Search for a game across **all** configured systems.
- Supports logical operators `AND` and `OR` for advanced queries.

### Global Statistics
- Access via **Tools -> Global Stats**.
- Provides a detailed report of your entire collection, including:
  - Total number of systems, emulators, and games.
  - Total number of matched cover images.
  - Total disk space used by your games.
- You can save the report as a `.txt` file.

### Image Pack Downloader
- Access via **Edit System -> Download Image Pack**.
- Provides an easy way to download complete cover art sets for systems defined in `easymode.xml`.

### Controller Support
- Full navigation of the UI is possible with an Xbox or PlayStation-compatible controller.
- Enable/disable via **Options -> Gamepad Support -> Enable**.
- Adjust the analog stick deadzone via **Options -> Gamepad Support -> Set Gamepad Dead Zone**.

### Automatic Updates
- Simple Launcher automatically checks for new versions on startup.
- If an update is found, it will prompt you to download and install it.
- The update process uses a separate `Updater.exe` application to ensure a smooth transition.

## 7. Bundled Tools
Simple Launcher includes a suite of command-line tools accessible from the **Tools** menu to help you manage your game library.

| Tool | Description |
| :--- | :--- |
| **Batch Convert Iso To Xiso** | Converts standard Xbox ISO files to the `.xiso` format required by some emulators. |
| **Batch Convert To CHD** | Converts disc images (`.cue`, `.iso`, `.gdi`, etc.) to the compressed CHD format. |
| **Batch Convert To Compressed File** | Compresses files or folders into `.zip` or `.7z` archives. |
| **Batch Convert To RVZ** | Converts GameCube and Wii disc images to the highly compressed RVZ format used by Dolphin. |
| **Create Batch Files...** | A series of tools to generate `.bat` files for launching games that require special setups, such as for PS3, ScummVM, Sega Model 3, Windows, and Xbox 360 XBLA. |
| **FindRomCover** | A utility to help you find and organize cover art for your games. |

## 8. Troubleshooting
- **Application doesn't start:**
  - Ensure you have .NET 9 installed.
  - Make sure you are not running another instance of Simple Launcher.
  - Check that the application is in a writable folder.
- **Games won't launch:**
  - **Check paths:** Use **Expert Mode** to verify that the `SystemFolder` and `EmulatorLocation` are correct. Use absolute paths if you are unsure.
  - **Check parameters:** Ensure the `EmulatorParameters` are correct for the emulator you are using. Refer to the `helpuser.xml` guide in the Expert Mode window or the emulator's official documentation.
  - **Check for BIOS files:** Many emulators require BIOS files to be placed in a specific folder.
- **Mounting features don't work:**
  - **Install Dokan:** This is a mandatory dependency for mounting `.zip` and `.xiso` files.
- **Errors or crashes:**
  - Check the `error_user.log` file in the Simple Launcher directory for detailed error messages.
  - You can also enable the debug window by launching `SimpleLauncher.exe -debug` from the command line for real-time logs.

## 9. Dependencies & Credits
Simple Launcher is built using several excellent open-source libraries and tools.

### External Dependencies
- **Dokan:** For virtual drive mounting. [https://github.com/dokan-dev/dokany](https://github.com/dokan-dev/dokany)

### Bundled Executables
- **SimpleZipDrive.exe / xbox-iso-vfs.exe:** Custom tools based on Dokan for mounting archives.
- **extract-xiso.exe:** Used by the Batch Convert to XISO tool.
- **chdman.exe:** The official MAME tool for creating and managing CHD files.
- **7z.dll:** Used for 7-Zip archive operations.

### Core Libraries (NuGet Packages)
- **MahApps.Metro:** For the modern UI theme and controls.
- **Markdig.Wpf:** For rendering Markdown in the Update History window.
- **SharpDX:** For XInput and DirectInput controller support.
- **InputSimulatorCore:** For simulating mouse movements with a controller.
- **Squid-Box.SevenZipSharp & ICSharpCode.SharpZipLib:** For handling `.7z`, `.zip`, and `.rar` archives.
- **MessagePack:** For fast serialization of data files like `favorites.dat` and `playhistory.dat`.
- **Microsoft.Extensions.*:** For modern dependency injection and configuration.

---
Thank you for using Simple Launcher! For further help, please open an issue on the [GitHub repository](https://github.com/drpetersonfernandes/SimpleLauncher/issues).