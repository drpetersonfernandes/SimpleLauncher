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
- Select the added system from the dropdown menu.
- Click the **All** button to display all games for that system.
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
* Users can also set multiple emulators for each system

The provided values will be checked for valid paths before being added to the frontend database.

## Where to Find ROMs or ISOs
We do NOT provide ROMs or ISOs.

## Where to Find Game Cover Images
We provide Image Pack for some systems.<br>
If the Image Pack is not provided, you can download cover images from websites like [Libretro Thumbnails](https://github.com/libretro-thumbnails/libretro-thumbnails) or [EmuMovies](https://emumovies.com).

## List of Parameters for Emulators
We have compiled a list of parameters for each emulator for your convenience.<br>
Click [here](https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters) to see the list.

## Special Settings:
**LaserDisk**

If you want to add LaserDisk games to the Simple Launcher frontend,
you should create a folder with BAT files that directly call the games.<br>

**MAME CHD Games**

The CHD folders should be in the same folder as the ROM zipped files.

**Microsoft Windows Games**

Use 'Simple Launcher' tools to generate BAT files for you.

**Microsoft XBOX 360 XBLA**

Use 'Simple Launcher' tools to generate BAT files for you.

**ScummVM Games**

Use 'Simple Launcher' tools to generate BAT files for you.

**Sega Model 3**

Use Simple Launcher Tools to generate BAT files for you.

**Sony PlayStation 3**

Use Simple Launcher Tools to generate BAT files for you.

## Right Click Context Menu
When 'Simple Launcher' generates the buttons for each game, it adds a Context Menu for each button.

- **Launch Game:** Launches the game.
- **Open Video Link:** Opens a video link related to that game.
- **Open Info Link:** Opens an info page related to that game.
- **Open ROM History:** Opens the ROM History window that displays historical data about your ROM. This functionality is more useful for MAME-based ROMs.
- **Cover:** Opens the cover image related to that game. The file should be inside .\title_snapshots\SystemName or inside a custom folder.
- **Title Snapshot:** Opens a title snapshot related to that game. The file should be inside .\title_snapshots\SystemName
- **Gameplay Snapshot:** Opens a gameplay snapshot related to that game. The file should be inside .\gameplay_snapshots\SystemName
- **Video:** Opens a local video related to that game. The file should be inside .\videos\SystemName
- **Manual:** Opens a PDF manual related to that game. The file should be inside .\manuals\SystemName
- **Walkthrough:** Opens a PDF walkthrough related to that game. The file should be inside .\walkthrough\SystemName
- **Cabinet:** Opens a cabinet image related to that game. The file should be inside .\cabinets\SystemName
- **Flyer:** Opens a flyer image related to that game. The file should be inside .\flyers\SystemName
- **PCB:** Opens a PCB image related to that game. The file should be inside .\pcbs\SystemName

Links will be opened by the user's default browser.<br>
Image files should be in JPG, JPEG, or PNG format and will be opened by 'Simple Launcher'.<br>
Video files should be in MP4, AVI, or MKV format and will be opened by the user's default video player. We recommend using [VLC Player](https://www.videolan.org/vlc/download-windows.html).<br>
PDF files will be opened by the user's default PDF viewer.

## Global Search
This search engine will look for filenames in the System Folder of every system configured in 'Simple Launcher'.<br>
From within the search results, users can launch the game.

## Favorites
'Simple Launcher' allows users to save favorite games.

## Play History
Track system playtime and per game playtime.

## Global Stats
This window will generate a summary report of every system configured in the frontend. It will report:

* Total Number of Systems
* Total Number of Emulators
* Total Number of Games
* Total Number of Cover Images
* Application Folder
* Disk Size of all Games

## Additional Features:
- **Edit Systems Menu:** Easily edit, add, or delete a system.
- **Automatic Installation of Most Emulators:** We offer the automatic installation of emulators that do not require BIOS or copyrighted files to function.
- **Search Engine:** Users can easily search for games within the frontend.
- **Track Emulator Usage:** Track the playtime for each system.
- **Track Game Playtime:** Monitor which game was played and for how long.
- **Favorites:** Users can mark games as favorites.
- **Right-Click Context Menu:** Users can load the cover image, title snapshot, gameplay snapshot, manual, walkthrough, cabinet, flyer, or PCB of the selected game.
- **Edit Links Menu:** Customize the video and info search engines used within the UI.
- **Control Button Size and Aspect Ratio:** Conveniently adjust the size and aspect ratio of the generated buttons in the UI.
- **Automatic Update:** The application features an automatic update mechanism.
- **GamePad Support:** Simple Launcher supports Xbox and PlayStation gamepads.

## Related Utilities:
- **[MAME Utility](https://github.com/drpetersonfernandes/MAMEUtility):** A utility for managing the MAME full driver information in XML format available on the [MAME](https://www.mamedev.org/release.html) website. It can generate multiple simplified (and smaller) XML subsets and also copy ROMs and image files based on the created XML.
- **[FindRomCover](https://github.com/drpetersonfernandes/FindRomCover):** An application that supports the organization of your cover image collection. It attempts to match the filename of image files with the ROM filenames. Users can choose the similarity algorithm to compare filenames.

## Technical Details:
Simple Launcher is developed in C# using Windows Presentation Foundation (WPF) and Microsoft .NET Core 9 Framework.<br>
This program is Windows-only and has been tested on Windows 11.

## Support the Project:
Did you enjoy using the Simple Launcher frontend?  
Consider [donating](https://www.purelogiccode.com/donate) to support the project or simply to express your gratitude!

## Contributors:
- **Peterson Fernandes** - [Github Profile](https://github.com/drpetersonfernandes)
- **RFSVIEIRA** - [Github Profile](https://github.com/RFSVIEIRA)