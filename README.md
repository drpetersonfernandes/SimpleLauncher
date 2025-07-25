# Simple Launcher
Simple Launcher is an open-source emulator frontend.

**Main Window**
![Screenshot](screenshot.png)

**Main Window in Grid View**  
![Screenshot](screenshot2.png)

**Main Window in List View**
![Screenshot](screenshot3.png)

**Global Search Window**  
![Screenshot](screenshot4.png)

**Favorites Window**  
![Screenshot](screenshot5.png)

If you like the software, please give us a star and consider making a donation.

## Installation

Download the application from the [release page](https://github.com/drpetersonfernandes/SimpleLauncher/releases).<br>
Extract the ZIP file into a writable folder. We do not recommend using a network folder or installing it inside `C:\Program Files`.<br>
If necessary, you may need to grant "Simple Launcher" administrative access. The application requires write access to its own folder.

## Basic Usage

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
-   **On-the-Fly File Mounting:** Allows users to launch games directly from compressed or disk image files without needing to manually extract them first. You need to install Dokan from [GitHub](https://github.com/dokan-dev/dokany/releases) to use this feature.

## Bundled Tools

Simple Launcher comes with a suite of utilities accessible via the "Tools" menu to help manage your game library:
-   Batch Convert ISO to XISO
-   Batch Convert to CHD (supports CUE/BIN, ISO, IMG, ZIP, 7z, RAR, CSO)
-   Batch Convert to Compressed File (ZIP or 7Z)
-   Batch Convert to RVZ
-   Create Batch Files for PS3, ScummVM, Sega Model 3, Windows Games, and Xbox 360 XBLA.
-   FindRomCover (Organize System Images)

## Where to Find ROMs or ISOs

We do **NOT** provide ROMs or ISOs.

## Where to Find Game Cover Images

Image packs for some systems can be downloaded via **Edit System -> Download Image Pack**.<br>
If an image pack is not available, or for more comprehensive collections, you can download cover images from websites like [Libretro Thumbnails](https://github.com/libretro-thumbnails/libretro-thumbnails) or [EmuMovies](https://emumovies.com).

## Advanced Usage & Documentation

For more detailed information on Expert Mode, emulator parameters, and advanced configurations, please refer to our [Wiki](https://github.com/drpetersonfernandes/SimpleLauncher/wiki).<br>
A list of common emulator parameters can be found [here](https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters).<br>
A software map of 'Simple Launcher' can be found [here](https://github.com/drpetersonfernandes/SimpleLauncher/wiki/SoftwareMap).

## Related Utilities

- **[MAME Utility](https://github.com/drpetersonfernandes/MAMEUtility):** A tool for managing full MAME driver information in XML format available on the [MAME](https://www.mamedev.org/release.html) website. It can generate multiple simplified (and smaller) XML subsets and copy ROMs and image files based on the generated XML.
- **[FindRomCover](https://github.com/drpetersonfernandes/FindRomCover):** An application that helps organize your cover image collection. It attempts to match image filenames with ROM filenames, and users can choose from different similarity algorithms.
- **[SimpleZipDrive](https://github.com/drpetersonfernandes/SimpleZipDrive):** This application mounts ZIP archive files as virtual drives or directories on your Windows system using the DokanNet library.
- **[BatchConvertToRVZ](https://github.com/drpetersonfernandes/BatchConvertToRVZ):** A Windows desktop utility for batch converting GameCube and Wii ISO images to RVZ format with verification capabilities.
- **[BatchConvertToCompressedFile](https://github.com/drpetersonfernandes/BatchConvertToCompressedFile):** A Windows desktop utility for batch compressing files to .7z or .zip formats and for verifying the integrity of existing compressed archives.
- **[BatchConvertToCHD](https://github.com/drpetersonfernandes/BatchConvertToCHD):** A Windows desktop utility for batch converting various disk image formats to CHD (Compressed Hunks of Data) format and for verifying the integrity of existing CHD files.
- **[BatchConvertIsoToXiso](https://github.com/drpetersonfernandes/BatchConvertIsoToXiso):** A GUI application for extract-xiso that provides batch converting Xbox ISO files to the optimized XISO format and testing their integrity. Supports both Xbox 360 ISOs and original Xbox ISOs.
- **[CreateBatchFilesForPS3Games](https://github.com/drpetersonfernandes/CreateBatchFilesForPS3Games):** A batch file creator for RPCS3 emulator.

## Technical Details

Simple Launcher is developed in C# using Windows Presentation Foundation (WPF) and Microsoft .NET 9 Framework.<br>
This program is Windows-only and has been tested on Windows 11.

## Support the Project

Did you enjoy using the Simple Launcher frontend?<br>
Consider [donating](https://www.purelogiccode.com/donate) to support the project or simply to express your gratitude!

## Contributors

- **Peterson Fernandes** - [GitHub Profile](https://github.com/drpetersonfernandes)
- **RFSVIEIRA** - [GitHub Profile](https://github.com/RFSVIEIRA)