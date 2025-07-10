# Welcome to the SimpleLauncher Wiki!

## Introduction
Simple Launcher is a powerful and flexible open-source emulator frontend for Windows, designed to be powerful, customizable, and easy to use. While its name implies simplicity, it packs powerful features for both beginners and advanced users.

## Table of Contents
1. [Getting Started](#getting-started)
  - [System Requirements](#system-requirements)
  - [Installation](#installation)
  - [Basic Usage](#basic-usage)
2. [Core Concepts](#core-concepts)
  - [Systems](#systems)
  - [Game Files (ROMs/ISOs)](#game-files-romsisos)
  - [Emulators](#emulators)
  - [Media and Folders](#media-and-folders)
3. [Configuration](#configuration)
  - [Easy Mode](#easy-mode)
  - [Expert Mode](#expert-mode)
  - [Path Placeholders](#path-placeholders)
4. [User Interface](#user-interface)
  - [System Selection Screen](#system-selection-screen)
  - [Navigation Panel](#navigation-panel)
  - [View Modes](#view-modes)
  - [Context Menu](#context-menu)
5. [Key Features](#key-features)
6. [Bundled Tools](#bundled-tools)
7. [Where to Find Content](#where-to-find-content)
8. [Troubleshooting](#troubleshooting)
9. [Technical Details](#technical-details)
10. [Support the Project](#support-the-project)

---

## Getting Started

### System Requirements
- **Operating System:** Windows 7 or newer (tested on Windows 11)
- **.NET Framework:** .NET 9

### Installation
1. Download the application from the [release page](https://github.com/drpetersonfernandes/SimpleLauncher/releases).
2. Extract the ZIP file into a **writable folder** (e.g., `C:\SimpleLauncher`, or a folder within your Documents). We do not recommend using a network folder or installing it inside `C:\Program Files`.
3. For certain features like on-the-fly file mounting, you must install the [Dokan Library](https://github.com/dokan-dev/dokany/releases).
4. If necessary, you may need to grant "Simple Launcher" administrative access, as the application requires write access to its own folder for saving settings, logs, and other data.
5. Run `SimpleLauncher.exe`.

### Basic Usage
1. Click the **Edit System** menu item, then select **Easy Mode**.
2. Follow the on-screen steps to download and install an emulator and its necessary files for a system.
3. Add your ROM/ISO files for that system into the designated folder (e.g., `.\roms\[SystemName]\`).
4. Add your cover images for that system into the designated image folder (e.g., `.\images\[SystemName]\`).
5. Return to the Main Window. If no system is selected, a visual **System Selection Screen** will help you choose your gaming platform. Otherwise, select the system from the dropdown menu.
6. Click the **All** button (or a letter/number filter) to display games for that system.
7. Click the game you wish to launch.

## Core Concepts

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

## Configuration

### Easy Mode
This is the recommended method for new users. Simplifies adding new systems by automatically downloading and configuring common emulators and cores.

1. Go to **Edit System -> Easy Mode**.
2. **Choose a System:** Select a pre-configured system from the dropdown list.
3. **Choose a ROMs Folder (Optional):** Specify a folder for your games or use the default.
4. **Install Components:** Download emulator, cores, and image packs as needed.
5. **Add System:** Save the configuration once components are downloaded.

### Expert Mode
Allows manual and detailed configuration of systems, emulators, paths, and launch parameters. In this mode, you can manually customize every aspect of a system configuration:

- **System Name:** The display name for the system.
- **System Folder:** The path to your ROMs/ISOs.
- **Image Folder:** The path to your cover images.
- **File Extensions:** Define which file types to search for.
- **Extraction:** Configure whether files need to be extracted before launch and which file to launch post-extraction.
- **Emulators:** Set up multiple emulators per system, each with a unique name, path, and launch parameters.
- **Error Notifications:** Toggle error popups on a per-emulator basis.

### Path Placeholders
Use placeholders like `%BASEFOLDER%`, `%SYSTEMFOLDER%`, and `%EMULATORFOLDER%` in your paths and parameters for a portable configuration.

- **`%BASEFOLDER%`**: Represents the root directory where `SimpleLauncher.exe` is located.
- **`%SYSTEMFOLDER%`**: Represents the path defined in the `<SystemFolder>` tag for the current system.
- **`%EMULATORFOLDER%`**: Represents the directory where the emulator's executable is located.

## User Interface

### System Selection Screen
A visual way to choose your gaming platform. When you first start Simple Launcher, you'll see a grid of all configured systems.

### Navigation Panel
Quick access to common actions like Global Search, Favorites, Play History, and UI adjustments:
- **Restart:** Reloads the UI to the system selection screen.
- **Global Search:** Opens the Global Search window.
- **Favorites:** Opens the Favorites window.
- **Play History:** Opens the Play History window.
- **Expert Mode:** Opens the system configuration window.
- **Toggle View Mode:** Switches between Grid and List views.
- **Toggle Aspect Ratio:** Cycles through different button aspect ratios in Grid View.
- **Zoom In/Out:** Changes the size of game thumbnails in Grid View.

### View Modes
**Dual View Modes:**
- **Grid View:** Displays game covers as interactive buttons.
- **List View:** Shows game details in a sortable table, including file size, play count, and playtime.

### Context Menu
Rich right-click menus for games to launch, manage favorites, open external links (video/info), view local media (covers, snapshots, manuals), take screenshots, and delete game files:

- **Launch Game:** Launches the selected game.
- **Add To/Remove From Favorites:** Manage your favorites list.
- **Open Video/Info Link:** Searches for the game on YouTube and IGDB (or your custom configured sites).
- **Open ROM History:** Displays historical data about the game (especially for MAME ROMs).
- **View Local Media:** Opens viewers for covers, snapshots, videos, manuals, and other media.
- **Take Screenshot:** Captures the game window and saves it as the cover image.
- **Delete Game:** Permanently deletes the game file from your hard drive (use with caution!).

## Key Features

- **Intuitive User Interface:** Modern WPF interface with light and dark themes, and customizable accent colors.
- **Global Search:** Quickly find games across all your configured systems.
- **Favorites Management:** Mark games as favorites for easy access.
- **Play History Tracking:** See which games you've played, how many times, and for how long.
- **Global Stats:** Get an overview of your game library, including total systems, games, and image counts.
- **Fuzzy Image Matching:** Helps find cover images even if filenames don't perfectly match.
- **Image Pack Downloader:** Download pre-made image packs for various systems.
- **Gamepad Support:** Navigate the UI using Xbox and PlayStation controllers (configurable deadzone).
- **Customizable UI:** Adjust thumbnail sizes, button aspect ratios, and games per page.
- **Automatic Updates:** Keeps Simple Launcher up-to-date with the latest features and fixes.
- **Multilingual:** Translated into 17 languages.
- **Single Instance Enforcement:** Prevents multiple copies of the application from running simultaneously.
- **Sound Configuration:** Customize or disable UI sound effects via the Options menu.
- **On-the-Fly File Mounting:**
  - **PS3 (ZIP/ISO), Xbox (XISO), XBLA (ZIP):** Games for these systems can be mounted as virtual drives, allowing you to launch them without manual extraction. This feature requires the [Dokan Library](https://github.com/dokan-dev/dokany).
  - **Other Compressed Files (.7z, .rar, .zip):** For other systems, if "Extract File Before Launch" is enabled, the application will automatically extract the contents to a temporary folder before launching the game.

## Bundled Tools

Simple Launcher includes a "Tools" menu with utilities to help manage your game collection:

- **Batch Convert Iso To Xiso:** Converts standard ISO files to XISO format for Xbox. Verifies the integrity of XISO files.
- **Batch Convert To CHD:** Converts CUE/BIN, ISO, IMG, ZIP, 7z, and RAR files to CHD format (MAME). Features parallel processing. Verifies the integrity of CHD files.
- **Batch Convert To Compressed File:** Compresses files into ZIP or 7Z archives. Verifies the integrity of ZIP, 7Z, or RAR files.
- **Batch Convert To RVZ:** Converts GameCube and Wii ISO files to the compressed RVZ format.
- **Create Batch Files:** Generates launcher scripts for PS3, ScummVM, Sega Model 3, Windows Games, and Xbox 360 XBLA titles.
- **FindRomCover (Organize System Images):** Helps match and organize your cover images with your ROMs.

## Where to Find Content

### ROMs and ISOs
We do **NOT** provide ROMs or ISOs.

### Game Cover Images
We provide Image Packs for some systems, accessible via the **Edit System -> Download Image Pack** menu.
If an Image Pack is not available for your desired system, you can download cover images from websites like [Libretro Thumbnails](https://github.com/libretro-thumbnails/libretro-thumbnails) or [EmuMovies](https://emumovies.com).

Simple Launcher also features **Fuzzy Image Matching**, which can help find cover images even if their filenames don't perfectly match the ROM names. This feature can be enabled/disabled and its sensitivity adjusted in the **Options -> Fuzzy Image Matching** menu.

### Emulator Parameters
We have compiled a list of parameters for each emulator for your convenience.
Click [here](https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters) to see the list.

## Troubleshooting

- **Application doesn't start:**
  - Ensure you have .NET 9 installed.
  - Make sure you are not running another instance of Simple Launcher.
  - Check that the application is in a writable folder.
- **Games won't launch:**
  - **Check paths:** Use **Expert Mode** to verify that the `SystemFolder` and `EmulatorLocation` are correct.
  - **Check parameters:** Ensure the `EmulatorParameters` are correct for the emulator you are using.
  - **Check for BIOS files:** Many emulators require BIOS files to be placed in a specific folder.
- **Mounting features don't work:**
  - **Install Dokan:** This is a mandatory dependency for mounting `.zip` and `.xiso` files.
- **Errors or crashes:**
  - Check the `error_user.log` file in the Simple Launcher directory for detailed error messages.
  - You can also enable the debug window by launching `SimpleLauncher.exe -debug` from the command line for real-time logs.

## Technical Details

- **Framework:** C# using Windows Presentation Foundation (WPF) and Microsoft .NET 9.
- **Platform:** Windows-only (tested on Windows 11).
- **MAME Data:** Game information is loaded from `mame.dat`.
- **User Data:** Favorites are stored in `favorites.dat`, and play history in `playhistory.dat`.

### Dependencies & Credits
Simple Launcher is built using several excellent open-source libraries and tools:

**External Dependencies:**
- **Dokan:** For virtual drive mounting. [https://github.com/dokan-dev/dokany](https://github.com/dokan-dev/dokany)

**Bundled Executables:**
- **xbox-iso-vfs.exe / SimpleZipDrive.exe:** Custom tools based on Dokan for mounting archives.
- **7z_x64.dll / 7z_x86.dll:** Used for 7-Zip archive operations.

**Core Libraries (NuGet Packages):**
- **MahApps.Metro:** For the modern UI theme and controls.
- **Markdig.Wpf:** For rendering Markdown in the Update History window.
- **SharpDX:** For XInput and DirectInput controller support.
- **InputSimulatorCore:** For simulating mouse movements with a controller.
- **Squid-Box.SevenZipSharp & ICSharpCode.SharpZipLib:** For handling `.7z`, `.zip`, and `.rar` archives.
- **MessagePack:** For fast serialization of data files like `favorites.dat` and `playhistory.dat`.
- **Microsoft.Extensions.*:** For modern dependency injection and configuration.

## Support the Project

Did you enjoy using the Simple Launcher frontend?
Consider [donating](https://www.purelogiccode.com/donate) to support the project or simply to express your gratitude!

### Contributors
- **Peterson Fernandes** - [Github Profile](https://github.com/drpetersonfernandes)
- **RFSVIEIRA** - [Github Profile](https://github.com/RFSVIEIRA)

---

Thank you for using Simple Launcher! For further help, please open an issue on the [GitHub repository](https://github.com/drpetersonfernandes/SimpleLauncher/issues).