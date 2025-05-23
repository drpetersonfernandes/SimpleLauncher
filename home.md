# Welcome to the SimpleLauncher Wiki!

## Introduction
Simple Launcher is an open-source emulator frontend.

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

## Easy Mode
This mode downloads and installs the most commonly used emulator for a specific System.<br>
It also creates default folders for ROMs and Cover Images inside the 'Simple Launcher' folder.

## Expert Mode (Advanced Mode)
In this mode, you will be able to customize the variables of each System:

* System Name
* System Folder
* Image Folder
* File Extensions to search inside the System Folder
* Need for Extraction, in case the file needs to be extracted before launch
* File Extensions to launch after extraction
* Emulator name
* Emulator path
* Emulator launch parameters
* Option to receive notifications for emulator errors
* Users can also set multiple emulators for each system

The provided values will be checked for valid paths before being added to the frontend database.

## Where to Find ROMs or ISOs
We do NOT provide ROMs or ISOs.

## Where to Find Game Cover Images
We provide Image Packs for some systems, accessible via the **Edit System -> Download Image Pack** menu.<br>
If an Image Pack is not available for your desired system, you can download cover images from websites like [Libretro Thumbnails](https://github.com/libretro-thumbnails/libretro-thumbnails) or [EmuMovies](https://emumovies.com).
Simple Launcher also features **Fuzzy Image Matching**, which can help find cover images even if their filenames don't perfectly match the ROM names. This feature can be enabled/disabled and its sensitivity adjusted in the **Options -> Fuzzy Image Matching** menu.

## List of Parameters for Emulators
We have compiled a list of parameters for each emulator for your convenience.<br>
Click [here](https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters) to see the list.

## Special Settings & Bundled Tools
Simple Launcher includes a "Tools" menu with utilities to help manage your game collection:

*   **Batch Convert Iso To Xiso:** Converts standard ISO files to XISO format for Xbox.
*   **Batch Convert To CHD:** Converts CUE/BIN, ISO, IMG, ZIP, 7z, and RAR files to CHD format (MAME). Features parallel processing.
*   **Batch Convert To Compressed File:** Compresses files into ZIP or 7Z archives.
*   **Batch Verify CHD Files:** Verifies the integrity of CHD files.
*   **Batch Verify Compressed Files:** Verifies the integrity of ZIP, 7Z, or RAR files.
*   **Create Batch Files For PS3 Games:** Generates launcher scripts for PS3 games.
*   **Create Batch Files For ScummVM Games:** Generates launcher scripts for ScummVM games.
*   **Create BatchFiles For Sega Model 3 Games:** Generates launcher scripts for Sega Model 3 games.
*   **Create Batch Files For Windows Games:** Generates launcher scripts for PC games.
*   **Create Batch Files For Xbox 360 XBLA Games:** Generates launcher scripts for Xbox 360 XBLA titles.
*   **FindRomCover (Organize System Images):** Helps match and organize your cover images with your ROMs.

**Specific System Notes:**
*   **LaserDisk (Daphne):** Create BAT files that directly call the games.
*   **MAME CHD Games:** CHD folders should be in the same folder as the ROM zipped files.
*   **Microsoft Windows Games, Microsoft XBOX 360 XBLA, ScummVM Games, Sega Model 3, Sony PlayStation 3:** Use the respective tools in the "Tools" menu to generate launcher BAT files.

## Right Click Context Menu
When 'Simple Launcher' generates the buttons for each game (in Grid View) or lists them (in List View), it adds a Context Menu with various options:

- **Launch Game:** Launches the selected game.
- **Add To Favorites:** Adds the game to your favorites list.
- **Remove From Favorites:** Removes the game from your favorites list.
- **Open Video Link:** Searches for a video of the game on your configured video site (default: YouTube).
- **Open Info Link:** Searches for information about the game on your configured info site (default: IGDB).
- **Open ROM History:** Displays historical data and information about the game, especially useful for MAME ROMs.
- **Cover:** Opens the game's cover image. Images are typically stored in your system's designated image folder (e.g., `.\images\[SystemName]\`) or a custom path defined in Expert Mode.
- **Title Snapshot:** Opens the title screen snapshot. Files should be in `.\title_snapshots\[SystemName]\`.
- **Gameplay Snapshot:** Opens an in-game snapshot. Files should be in `.\gameplay_snapshots\[SystemName]\`.
- **Cart:** Opens an image of the game's cartridge (if applicable). Files should be in `.\carts\[SystemName]\`.
- **Video:** Plays a local video preview of the game. Files should be in `.\videos\[SystemName]\`.
- **Manual:** Opens the game's manual (PDF). Files should be in `.\manuals\[SystemName]\`.
- **Walkthrough:** Opens a game walkthrough (PDF). Files should be in `.\walkthrough\[SystemName]\`.
- **Cabinet:** Opens an image of the game's arcade cabinet (if applicable). Files should be in `.\cabinets\[SystemName]\`.
- **Flyer:** Opens an image of the game's promotional flyer. Files should be in `.\flyers\[SystemName]\`.
- **PCB:** Opens an image of the game's Printed Circuit Board (if applicable). Files should be in `.\pcbs\[SystemName]\`.
- **Take Screenshot:** Allows you to take a screenshot of the game running in an emulator and save it as the cover image for the selected system.
- **Delete Game:** Deletes the selected game file from your hard drive (use with caution!).

Links will be opened by the user's default browser.<br>
Image files should be in JPG, JPEG, or PNG format and will be opened by 'Simple Launcher'.<br>
Video files should be in MP4, AVI, or MKV format and will be opened by the user's default video player. We recommend using [VLC Player](https://www.videolan.org/vlc/download-windows.html).<br>
PDF files will be opened by the user's default PDF viewer.

## Global Search
This search engine will look for filenames (and MAME descriptions if applicable) in the System Folder of every system configured in 'Simple Launcher'.<br>
From within the search results, users can launch the game and access context menu options.

## Favorites
'Simple Launcher' allows users to save favorite games. These can be accessed via the "Favorites" menu item or the dedicated navigation panel button, opening a window to manage and launch your favorite titles.

## Play History
Track system playtime and per-game playtime, including the number of times played and the last played date/time. Access this information via the "Play History" menu item or the navigation panel button.

## Global Stats
This window will generate a summary report of every system configured in the frontend. It will report:

* Total Number of Systems
* Total Number of Emulators
* Total Number of Games
* Total Number of Matched Images
* Application Folder
* Disk Size of all Games
* Per-system statistics on game and image counts.

## Additional Features:
- **New System Selection Screen:** A visual and intuitive screen to select your gaming platform when the application starts or no system is currently chosen.
- **Revamped Navigation Panel:** Quick access buttons on the left side of the Main Window for core actions:
    - Restart UI: Resets the main view to the system selection screen.
    - Global Search: Opens the global game search window.
    - Favorites: Opens the favorites window.
    - Play History: Opens the play history window.
    - Edit System (Expert Mode): Opens the system configuration window.
    - Toggle View Mode: Switches between Grid View and List View.
    - Toggle Aspect Ratio: Cycles through different aspect ratios for game buttons in Grid View.
    - Zoom In / Zoom Out: Adjusts the size of game thumbnails in Grid View.
- **Switchable View Modes:** Choose how you want to browse your games:
    - **Grid View:** Displays game cover images as interactive buttons.
    - **List View:** Presents games in a sortable table with details like filename, MAME description, file size, times played, and total playtime.
- **Enhanced Zooming (Grid View):** Easily resize game cover thumbnails using the navigation panel buttons or by holding `Ctrl` and using the mouse wheel.
- **File Size Display:** The List View, Favorites Window, Global Search Window, and Play History Window now show game file sizes.
- **Edit Systems Menu:** Easily edit, add, or delete a system using Easy Mode or Expert Mode.
- **Automatic Installation of Most Emulators:** Easy Mode can automatically download and set up emulators and their cores for many systems.
- **Search Engine:** Users can easily search for games within the frontend (per-system search in Main Window, and a Global Search).
- **Fuzzy Image Matching:** Helps find cover images even if their filenames don't perfectly match the ROM names. This feature can be enabled/disabled and its sensitivity adjusted in the Options menu.
- **Single Instance Enforcement:** Ensures only one instance of Simple Launcher runs at a time, preventing potential conflicts.
- **Edit Links Menu:** Customize the video and info search engines used within the UI.
- **Control Button Size and Aspect Ratio:** Conveniently adjust the size and aspect ratio of the generated game buttons in the Grid View via the Options menu or navigation panel.
- **Automatic Update:** The application features an automatic update mechanism.
- **GamePad Support:** Simple Launcher supports Xbox and PlayStation gamepads for UI navigation. Deadzone can be configured.
- **Multilingual Support:** The application is translated into 17 languages.

## Related Utilities:
- **[MAME Utility](https://github.com/drpetersonfernandes/MAMEUtility):** A utility for managing the MAME full driver information in XML format available on the [MAME](https://www.mamedev.org/release.html) website. It can generate multiple simplified (and smaller) XML subsets and also copy ROMs and image files based on the created XML.
- **[FindRomCover](https://github.com/drpetersonfernandes/FindRomCover):** An application that supports the organization of your cover image collection. It attempts to match the filename of image files with the ROM filenames. Users can choose the similarity algorithm to compare filenames.

## Technical Details:
Simple Launcher is developed in C# using Windows Presentation Foundation (WPF) and Microsoft .NET 9 Framework.<br>
This program is Windows-only and has been tested on Windows 11.

## Support the Project:
Did you enjoy using the Simple Launcher frontend?  
Consider [donating](https://www.purelogiccode.com/donate) to support the project or simply to express your gratitude!

## Contributors:
- **Peterson Fernandes** - [Github Profile](https://github.com/drpetersonfernandes)
- **RFSVIEIRA** - [Github Profile](https://github.com/RFSVIEIRA)

