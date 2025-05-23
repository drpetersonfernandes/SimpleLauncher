# Simple Launcher
Simple Launcher is an open-source emulator frontend.

**Main Window in Grid View**  
![Screenshot](screenshot.jpg)

**Main Window in List View**  
![Screenshot](screenshot2.png)

**Global Search Window**  
![Screenshot](screenshot3.png)

**Favorites Window**  
![Screenshot](screenshot4.png)

**Play History Window**  
![Screenshot](screenshot5.png)

If you like the software, please give us a star and consider making a donation.

## Installation

Download the application from the [release page](https://github.com/drpetersonfernandes/SimpleLauncher/releases).  
Extract the ZIP file into a writable folder. We do not recommend using a network folder or installing it inside `C:\Program Files`.  
If necessary, you may need to grant "Simple Launcher" administrative access. The application requires write access to its own folder.

## Basic Usage

- Click the **Edit System** menu item.
- Click the **Easy Mode** menu item.
- Follow the steps to install a system.
- Add ROM files for that system in the designated folder.
- Add cover images for that system in the designated folder.
- Return to the Main Window.
- If no system is selected, a visual **System Selection Screen** will help you choose your gaming platform. Otherwise, select the added system from the dropdown menu.
- Click the **All** button (or a letter/number filter) to display games for that system.
- Click the game you wish to launch.

## Key Features

-   **Intuitive User Interface:** Modern WPF interface with light and dark themes, and customizable accent colors.
-   **System Selection Screen:** A visual way to choose your gaming platform.
-   **Navigation Panel:** Quick access to common actions like Global Search, Favorites, Play History, and UI adjustments.
-   **Dual View Modes:**
    -   **Grid View:** Displays game covers as interactive buttons.
    -   **List View:** Shows game details in a sortable table, including file size, play count, and playtime.
-   **Easy Mode:** Simplifies adding new systems by automatically downloading and configuring common emulators and cores.
-   **Expert Mode:** Allows manual and detailed configuration of systems, emulators, paths, and launch parameters.
-   **Global Search:** Quickly find games across all your configured systems.
-   **Favorites Management:** Mark games as favorites for easy access.
-   **Play History Tracking:** See which games you've played, how many times, and for how long.
-   **Global Stats:** Get an overview of your game library, including total systems, games, and image counts.
-   **Fuzzy Image Matching:** Helps find cover images even if filenames don't perfectly match.
-   **Image Pack Downloader:** Download pre-made image packs for various systems.
-   **Context Menus:** Rich right-click menus for games to launch, manage favorites, open external links (video/info), view local media (covers, snapshots, manuals), take screenshots, and delete game files.
-   **Gamepad Support:** Navigate the UI using Xbox and PlayStation controllers (configurable deadzone).
-   **Customizable UI:** Adjust thumbnail sizes, button aspect ratios, and games per page.
-   **Automatic Updates:** Keeps Simple Launcher up-to-date with the latest features and fixes.
-   **Multilingual:** Translated into 17 languages.
-   **Single Instance Enforcement:** Prevents multiple copies of the application from running simultaneously.

## Bundled Tools

Simple Launcher comes with a suite of command-line utilities accessible via the "Tools" menu to help manage your game library:
-   Batch Convert ISO to XISO
-   Batch Convert to CHD (supports CUE/BIN, ISO, IMG, ZIP, 7z, RAR)
-   Batch Convert to Compressed File (ZIP or 7Z)
-   Batch Verify CHD Files
-   Batch Verify Compressed Files
-   Create Batch Files for PS3, ScummVM, Sega Model 3, Windows Games, and Xbox 360 XBLA.
-   FindRomCover (Organize System Images)

## Where to Find ROMs or ISOs

We do **NOT** provide ROMs or ISOs.

## Where to Find Game Cover Images

Image packs for some systems can be downloaded via **Edit System -> Download Image Pack**.
If an image pack is not available, or for more comprehensive collections, you can download cover images from websites like [Libretro Thumbnails](https://github.com/libretro-thumbnails/libretro-thumbnails) or [EmuMovies](https://emumovies.com).
Simple Launcher's **Fuzzy Image Matching** feature can also assist in finding relevant covers.

## Advanced Usage & Documentation

For more detailed information on Expert Mode, emulator parameters, and advanced configurations, please refer to our [Wiki](https://github.com/drpetersonfernandes/SimpleLauncher/wiki).
A list of common emulator parameters can be found [here](https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters).

## Related Utilities

- **[MAME Utility](https://github.com/drpetersonfernandes/MAMEUtility):** A tool for managing full MAME driver information in XML format available on the [MAME](https://www.mamedev.org/release.html) website. It can generate multiple simplified (and smaller) XML subsets and copy ROMs and image files based on the generated XML.
- **[FindRomCover](https://github.com/drpetersonfernandes/FindRomCover):** An application that helps organize your cover image collection. It attempts to match image filenames with ROM filenames, and users can choose from different similarity algorithms.

## Technical Details

Simple Launcher is developed in C# using Windows Presentation Foundation (WPF) and Microsoft .NET 9 Framework.  
This program is Windows-only and has been tested on Windows 11.

## Support the Project

Did you enjoy using the Simple Launcher frontend?  
Consider [donating](https://www.purelogiccode.com/donate) to support the project or simply to express your gratitude!

## Contributors

- **Peterson Fernandes** - [GitHub Profile](https://github.com/drpetersonfernandes)
- **RFSVIEIRA** - [GitHub Profile](https://github.com/RFSVIEIRA)
