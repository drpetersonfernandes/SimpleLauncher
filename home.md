# Welcome to the SimpleLauncher Wiki!

## Introduction
Simple Launcher is an open-source emulator frontend for Windows, designed to be powerful, customizable, and easy to use.

## Installation
1.  Download the application from the [release page](https://github.com/drpetersonfernandes/SimpleLauncher/releases).
2.  Extract the ZIP file into a writable folder (e.g., `C:\SimpleLauncher`, or a folder within your Documents). We do not recommend using a network folder or installing it inside `C:\Program Files`.
3.  For certain features like on-the-fly file mounting, you must install the [Dokan Library](https://github.com/dokan-dev/dokany).
4.  If necessary, you may need to grant "Simple Launcher" administrative access, as the application requires write access to its own folder for saving settings, logs, and other data.

## Basic Usage
1.  Click the **Edit System** menu item, then select **Easy Mode**.
2.  Follow the on-screen steps to download and install an emulator and its necessary files for a system.
3.  Add your ROM/ISO files for that system into the designated folder (e.g., `.\roms\[SystemName]\`).
4.  Add your cover images for that system into the designated image folder (e.g., `.\images\[SystemName]\`).
5.  Return to the Main Window. If no system is selected, a visual **System Selection Screen** will help you choose your gaming platform. Otherwise, select the system from the dropdown menu.
6.  Click the **All** button (or a letter/number filter) to display games for that system.
7.  Click the game you wish to launch.

## Key Features

- **Intuitive User Interface:** Modern WPF interface with light and dark themes, and customizable accent colors.
- **System Selection Screen:** A visual way to choose your gaming platform.
- **Navigation Panel:** Quick access to common actions like Global Search, Favorites, Play History, and UI adjustments.
- **Dual View Modes:**
  - **Grid View:** Displays game covers as interactive buttons.
  - **List View:** Shows game details in a sortable table, including file size, play count, and playtime.
- **Easy Mode:** Simplifies adding new systems by automatically downloading and configuring common emulators and cores.
- **Expert Mode:** Allows manual and detailed configuration of systems, emulators, paths, and launch parameters.
- **Global Search:** Quickly find games across all your configured systems.
- **Favorites Management:** Mark games as favorites for easy access.
- **Play History Tracking:** See which games you've played, how many times, and for how long.
- **Global Stats:** Get an overview of your game library, including total systems, games, and image counts.
- **Fuzzy Image Matching:** Helps find cover images even if filenames don't perfectly match.
- **Image Pack Downloader:** Download pre-made image packs for various systems.
- **Context Menus:** Rich right-click menus for games to launch, manage favorites, open external links (video/info), view local media (covers, snapshots, manuals), take screenshots, and delete game files.
- **Gamepad Support:** Navigate the UI using Xbox and PlayStation controllers (configurable deadzone).
- **Customizable UI:** Adjust thumbnail sizes, button aspect ratios, and games per page.
- **Automatic Updates:** Keeps Simple Launcher up-to-date with the latest features and fixes.
- **Multilingual:** Translated into 17 languages.
- **Single Instance Enforcement:** Prevents multiple copies of the application from running simultaneously.
- **Sound Configuration:** Customize or disable UI sound effects via the Options menu.
- **On-the-Fly File Mounting:**
  - **PS3 (ZIP/ISO), Xbox (XISO), XBLA (ZIP):** Games for these systems can be mounted as virtual drives, allowing you to launch them without manual extraction. This feature requires the [Dokan Library](https://github.com/dokan-dev/dokany).
  - **Other Compressed Files (.7z, .rar, .zip):** For other systems, if "Extract File Before Launch" is enabled, the application will automatically extract the contents to a temporary folder before launching the game.

## Expert Mode (Advanced Mode)
In this mode, you can manually customize every aspect of a system configuration:

*   **System Name:** The display name for the system.
*   **System Folder:** The path to your ROMs/ISOs.
*   **Image Folder:** The path to your cover images.
*   **File Extensions:** Define which file types to search for.
*   **Extraction:** Configure whether files need to be extracted before launch and which file to launch post-extraction.
*   **Emulators:** Set up multiple emulators per system, each with a unique name, path, and launch parameters.
*   **Dynamic Paths:** Use placeholders like `%BASEFOLDER%`, `%SYSTEMFOLDER%`, and `%EMULATORFOLDER%` in your paths and parameters for a portable configuration.
*   **Error Notifications:** Toggle error popups on a per-emulator basis.

## Where to Find ROMs or ISOs
We do **NOT** provide ROMs or ISOs.

## Where to Find Game Cover Images
We provide Image Packs for some systems, accessible via the **Edit System -> Download Image Pack** menu.
If an Image Pack is not available for your desired system, you can download cover images from websites like [Libretro Thumbnails](https://github.com/libretro-thumbnails/libretro-thumbnails) or [EmuMovies](https://emumovies.com).
Simple Launcher also features **Fuzzy Image Matching**, which can help find cover images even if their filenames don't perfectly match the ROM names. This feature can be enabled/disabled and its sensitivity adjusted in the **Options -> Fuzzy Image Matching** menu.

## List of Parameters for Emulators
We have compiled a list of parameters for each emulator for your convenience.
Click [here](https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters) to see the list.

## Bundled Tools
Simple Launcher includes a "Tools" menu with utilities to help manage your game collection:

*   **Batch Convert Iso To Xiso:** Converts standard ISO files to XISO format for Xbox.
*   **Batch Convert To CHD:** Converts CUE/BIN, ISO, IMG, ZIP, 7z, and RAR files to CHD format (MAME). Features parallel processing.
*   **Batch Convert To Compressed File:** Compresses files into ZIP or 7Z archives.
*   **Batch Convert To RVZ:** Converts GameCube and Wii ISO files to the compressed RVZ format.
*   **Batch Verify CHD Files:** Verifies the integrity of CHD files.
*   **Batch Verify Compressed Files:** Verifies the integrity of ZIP, 7Z, or RAR files.
*   **Create Batch Files:** Generates launcher scripts for PS3, ScummVM, Sega Model 3, Windows Games, and Xbox 360 XBLA titles.
*   **FindRomCover (Organize System Images):** Helps match and organize your cover images with your ROMs.

## Right Click Context Menu
When Simple Launcher displays your games, you can right-click on any entry to access a powerful context menu:

- **Launch Game:** Launches the selected game.
- **Add To/Remove From Favorites:** Manage your favorites list.
- **Open Video/Info Link:** Searches for the game on YouTube and IGDB (or your custom configured sites).
- **Open ROM History:** Displays historical data about the game (especially for MAME ROMs).
- **View Local Media:**
  - **Cover:** Opens the game's cover image.
  - **Title/Gameplay Snapshot:** Opens screen captures.
  - **Cart/Cabinet/Flyer/PCB:** Opens hardware-related images.
  - **Video:** Plays a local video preview.
  - **Manual/Walkthrough:** Opens PDF documents.
- **Take Screenshot:** Captures the game window and saves it as the cover image.
- **Delete Game:** Permanently deletes the game file from your hard drive (use with caution!).

## Technical Details
- **Framework:** C# using Windows Presentation Foundation (WPF) and Microsoft .NET 9.
- **Platform:** Windows-only (tested on Windows 11).
- **MAME Data:** Game information is loaded from `mame.dat`.
- **User Data:** Favorites are stored in `favorites.dat`, and play history in `playhistory.dat`.

## Support the Project
Did you enjoy using the Simple Launcher frontend?
Consider [donating](https://www.purelogiccode.com/donate) to support the project or simply to express your gratitude!

## Contributors
- **Peterson Fernandes** - [Github Profile](https://github.com/drpetersonfernandes)
- **RFSVIEIRA** - [Github Profile](https://github.com/RFSVIEIRA)
