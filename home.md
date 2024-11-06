# Welcome to the SimpleLauncher Wiki!

## Introduction
Simple Launcher is an emulator frontend that makes playing retro games a breeze.

## Installation
You need to download the application from the [release page](https://github.com/drpetersonfernandes/SimpleLauncher/releases),
then extract the zip file into your chosen folder.

## Basic Usage
* Click on the 'Edit System' menu item.
* Click on the 'Easy Mode' menu item.
* Follow the steps to install a System.
* Add ROM files for that system in the indicated folder.
* Add cover images for that system in the indicated folder.
* Go back to the Main Window.
* Select the added system from the dropdown menu.
* Click on the All Button to show all games for that system.
* Click on the game you wish to launch.

## Easy Mode
This Edit Mode was made for people starting in the world of emulation.<br>
This mode download and install the most used emulator for a specific System.<br>
Also create default folders for roms and cover images, located inside 'Simple Launcher' folder.<br>
The roms for a specific system need to be placed inside .\rom\SystemName.<br>
The cover images for a specific system need to be placed inside .\images\SystemName.<br>
The emulator will be installed inside the 'emulators' folder.<br>

## Expert Mode (Advanced Mode)
This mode was made for people familiar with the world of emulation.<br>
You will be able to custom set the variable of each System:

* System Name.
* System Folder.
* Image Folder.
* File Extensions to search inside the System Folder.
* Need of Extraction, in case the file needs to be extracted before launch.
* File Extensions to launch after extraction.
* Emulator name.
* Emulator path.
* Emulator launch parameters.
* Users also can set multiple emulators for each system.

The provided values will be path checked before being added to the frontend database.

## Where to Find ROMs or ISOs:
We do NOT provide ROMs or ISOs.

## Where to Find Game Covers Images:
We provide Image Pack for some systems.<br>
If the Image Pack is not provided, you can download cover images on websites like [Libretro Thumbnails](https://github.com/libretro-thumbnails/libretro-thumbnails) or [EmuMovies](https://emumovies.com).

## List of Parameters for Emulators
We have compiled a list of parameters for each emulator for your convenience. Click [here](https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters) to see the list.

## Special Settings:

**LaserDisk**

If you want to add LaserDisk games to the Simple Launcher frontend,
you should create a folder with BAT files that directly call the game.<br>

**MAME CHD Games**

The CHD folders should be in the same folder as the ROM zipped files.

**Microsoft Windows Games**

Use the Simple Launcher Tools to generate BAT files for you.

**Microsoft XBOX 360 XBLA**

Compress each game folder into a zipped file.<br>
Configure Simple Launcher to extract the file before launch.<br>
Configure the format to launch after extraction as `bin`.<br>
When a user launches a game, Simple Launcher will look into every folder inside the game folder
to find the folder `000D0000`, then launch the first found file.<br>
For this logic to work, the system name needs to have `XBLA` in its name.

**ScummVM Games**

Use the Simple Launcher Tools to generate BAT files for you.

**Sega Model 3**

Use the Simple Launcher Tools to generate BAT files for you.

**Sony Playstation 3**

Use the Simple Launcher Tools to generate BAT files for you.

## Right Click Context Menu

When 'Simple Launcher' generates the buttons for each game, it adds a Context Menu for each button.

- **Launch Game:** It launches the game.
- **Open Video Link:** Open video link related to that game.
- **Open Info Link:** Open info page related to that game.
- **Cover:** Open cover image related to that game. File should be inside .\title_snapshots\SystemName or inside custom folder.
- **Title Snapshot:** Open title snapshot related to that game. File should be inside .\title_snapshots\SystemName
- **Gameplay Snapshot:** Open title snapshot related to that game. File should be inside .\gameplay_snapshots\SystemName
- **Video:** Open local video related to that game. File should be inside .\videos\SystemName
- **Manual:** Open PDF manual related to that game. File should be inside .\manuals\SystemName
- **Walkthrough:** Open PDF walkthrough related to that game. File should be inside .\walkthrough\SystemName
- **Cabinet:** Open cabinet related to that game. File should be inside .\cabinets\SystemName
- **Flyer:** Open cabinet related to that game. File should be inside .\flyers\SystemName
- **PCB:** Open PCB related to that game. File should be inside .\pcbs\SystemName

The image files should be in format JPG, JPEG or PNG and will be opened by Simple Launcher Image Viewer Window.<br>
Video files should be in format MP4, AVI or MKV and will be opened by user's default video player.
I recommend to use [VLC Player](https://www.videolan.org/vlc/download-windows.html).<br>
PDF files will be open by user's default PDF viewer.

## Global Search

This search engine will look for filenames in System Folder of every system configured in 'Simple launcher'.<br>
From within the search results user can launch the game.

## Global Stats

This window will generate a summary report of every system configured in the frontend. It will report:

* Total Number of Systems
* Total Number of Emulators
* Total Number of Games
* Total Number of Cover Images
* Application Folder
* Disk Size of all Games

## How the Frontend Works:

- **Configuration:** The program first looks for a file named "system.xml" in its folder, which contains all the settings for the system and emulators.
- **Game Selection:** When you select a system, the application opens the system directory and lists the emulators configured for that specific system. It then displays a grid of games located in the system folder. Each cell in the grid is clickable, and the application will launch the selected emulator with the chosen game.
- **Game Info:** Each grid cell displays a game cover, its name, a link to a Video about the game, and a link to an Info Page about the game.
- **Game Covers:** The cover images should have the same filename as the game. They are loaded from a folder inside the 'images' folder, which should have the same name as the system. The images must be in PNG, JPG, or JPEG format. If a cover is missing, a default image is used.

## Explaining "system.xml":

This file contains information about various systems and their settings. You can add as many systems and emulators as you desire.
- **SystemName**: The name of the system.
- **SystemFolder**: The folder where the ROMs or games are housed.
- **SystemImageFolder**: The folder containing the cover images correlated with the System. This is optional. If you leave it empty or null, the application will load the images from a folder within the "images" folder, which should share the same name as the system.
- **SystemIsMAME**: This indicates to the program whether the system is based on MAME. If true, the application will load the ROM descriptions in conjunction with the ROM filenames.
- **FormatToSearch**: A list of file extensions to be loaded from the SystemFolder. You can include as many as you want.
- **ExtractFileBeforeLaunch**: This should be true or false. If true, the launcher will extract the ZIP or 7Z file into a temporary folder before loading the extracted file.
- **FormatToLaunch**: If you extract the file to a temporary folder, you should specify here which extensions will be launched from the extracted folder.
- **EmulatorName**: The name of the emulator. You can accommodate as many emulators as you want for each system.
- **EmulatorLocation**: The location of the emulator.
- **EmulatorParameters**: The parameters used for each emulator. Not all emulators require parameters.

```xml
<SystemConfig>
    <SystemName>Atari 2600</SystemName>
    <SystemFolder>G:\Atari 2600</SystemFolder>
    <SystemImageFolder>G:\Images\Atari 2600</SystemImageFolder>
    <SystemIsMAME>false</SystemIsMAME>
    <FileFormatsToSearch>
        <FormatToSearch>zip</FormatToSearch>
        <FormatToSearch>7z</FormatToSearch>
    </FileFormatsToSearch>
    <ExtractFileBeforeLaunch>false</ExtractFileBeforeLaunch>
    <FileFormatsToLaunch>
        <FormatToLaunch/>
    </FileFormatsToLaunch>
    <Emulators>
        <Emulator>
            <EmulatorName>Retroarch</EmulatorName>
            <EmulatorLocation>G:\Emulators\RetroArch\retroarch.exe</EmulatorLocation>
            <EmulatorParameters>-L "G:\Emulators\Retroarch\cores\stella_libretro.dll" -c "G:\Emulators\Retroarch\Config.cfg" -f</EmulatorParameters>
        </Emulator>
    </Emulators>
</SystemConfig>
```

## Additional Features:

- **Edit Systems menu:** Easily edit, add, or delete a system.
- **Automatic installation of most emulators:** We offer automatic installation of emulators that don't require BIOS or copyrighted files to work.
- **Search Engine:** User can search for games from within the frontend.
- **Right Click Context Menu:** User can load Cover image, Title snapshot, Gameplay snapshot, Manual, Walkthrough, Cabinet, Flyer or PCB of the selected game.
- **Edit Links menu:** Customize the Video and Info search engine used within the UI.
- **Control Thumbnail Size:** Conveniently adjust the size of the cover images in the UI.
- **Automatic Update:** The application has an automatic update mechanism.

## Related Utilities:

- **[PS3BatchLauncherCreator](https://github.com/drpetersonfernandes/ps3batchlaunchercreator):** An application written by a Simple Launcher developer, that automatically creates BAT files for easy launch of PS3 games on the RPCS3 emulator.
- **[MAME Utility](https://github.com/drpetersonfernandes/MAMEUtility):** A utility for managing the MAME full driver information in XML format available on the [MAME](https://www.mamedev.org/release.html) website. It can generate multiple simplified (and smaller) XML subsets and also copy ROMs and image files based on the created XML.
- **[FindRomCover](https://github.com/drpetersonfernandes/FindRomCover):** An application that supports the organization of your cover image collection. It attempts to match the filename of image files with the ROM filenames. Users can choose the similarity algorithm to compare filenames.

## Technical Details:
The Simple Launcher is developed in C# using Windows Presentation Foundation (WPF) and Microsoft .NET 8 Framework.<br>
This program is Windows-only.
It has been tested on Windows 11.

## Support the Project:
Did you enjoy using the Simple Launcher frontend?
Consider [donating](https://www.buymeacoffee.com/purelogiccode) to support the project
or simply to express your gratitude!

## Contributors:
- **Peterson Fernandes** - [Github Profile](https://github.com/drpetersonfernandes)
- **RFSVIEIRA** - [Github Profile](https://github.com/RFSVIEIRA)