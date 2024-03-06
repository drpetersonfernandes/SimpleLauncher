# Welcome to the SimpleLauncher wiki!

## Introduction

Simple Launcher is an Emulator Frontend that lets you play retro games with ease.

## Installation

You just need to download the application from the [release page](https://github.com/drpetersonfernandes/SimpleLauncher/releases), then extract the zip file to your chosen folder.

## Usage

* Click on the Edit System menu item.
* Configure each System you wish. You can add, edit or delete Systems.
* Create a folder inside images folder with the same name of the System you just created.
* Put all your cover images for that specific System inside that folder.
* Now you select the System in the dropdown menu on top of the app.
* Select the Emulator you want to use.
* Select the Letter Menu on top of the application.
* Click on the game you want to launch.

## Where to Find Game Covers:

We do NOT provide ROMs, ISOs or Game Covers.
You can find cover images on websites such as [Libretro Thumbnails](https://github.com/libretro-thumbnails/libretro-thumbnails) or [EmuMovies](https://emumovies.com). However, please note that these sites are not affiliated with Simple Launcher.

## List of Parameters for Emulators

You can check a list of parameters gathered by us for each emulator.
Click [here](https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters) to see the list.

## Special Settings:

**Microsoft Windows Games or Applications**

If you want to add your Windows Games or Applications to the Simple Launcher frontend, you should create a folder with Shortcut (lnk), BAT (bat) or Executable (exe) files that directly call the Game or Application.
If you prefer to use BAT (bat) files to call your games use the model below as an example of bat file.

```bat
@echo off
cd /d "J:\Microsoft Windows\Mega Man 11"
start game.exe
```

**RPCS3 Emulator**

This emulator loads games extracted from ISO files into a Game Folder.
To launch a specific game, it searches for a file named EBOOT.BIN located inside the `PS3_GAME\USRDIR\` or `USRDIR\` directories.
Configuring these games in an Emulator Frontend can be challenging. The quickest and easiest solution we found was to create BAT (bat) files that can directly launch the games. These BAT (bat) files can then be configured in Simple Launcher to be treated as games.
When configuring the System Sony Playstation 3 you should set Format To Search In System Folder as BAT (bat).
We have created a utility that can automatic generate these BAT files for you. Please see the section "Related Utilities" below for more information.<br><br>

## Explaining how the Frontend Works:

- **Configuration:** The program searches for a file named "system.xml" in its folder, which contains all the settings for the system and emulators. You should edit this file to match your needs.
- **Game Selection:** When you select a system, the application opens the system directory and a list of emulators configured for that specific system. It then displays a grid of games located in the system folder. Each cell in the grid is clickable, and the app will launch the selected emulator with the chosen game.
- **Game Info:** Each grid cell displays a game cover, its name, a link to a YouTube video about the game, and an info link.
- **Game Covers:** The cover images should have the same filename as the game. They are loaded from a folder inside the "images" folder, which should have the same name as the system. The images must be in PNG, JPG, or JPEG format. If a cover is missing, a default image is used.

## Explaining "system.xml":

This file contains information about different systems and their settings. You can add as many systems and emulators as you want.

- **SystemName**: The name of the system.
- **SystemFolder**: The folder where the ROMs or games are located.
- **SystemIsMAME**: Indicates to the program whether the system is based on MAME. If true, the application will load the ROM descriptions alongside the ROM filenames.
- **FormatToSearch**: A list of file extensions that will be loaded from the SystemFolder. You can use as many as you want.
- **ExtractFileBeforeLaunch**: Should be true or false. If true, the launcher will extract the ZIP or 7Z file into a temporary folder before loading the extracted file.
- **FormatToLaunch**: If you extract the file to a temporary folder, you should specify here which extensions will be launched from the extracted folder.
- **EmulatorName**: The name of the emulator. You can add as many emulators as you want for each system.
- **EmulatorLocation**: The location of the emulator.
- **EmulatorParameters**: The parameters that are used for each emulator. Not all emulators require parameters.

```xml
<SystemConfig>
	<SystemName>Atari 2600</SystemName>
	<SystemFolder>G:\OK\Atari 2600</SystemFolder>
	<SystemIsMAME>false</SystemIsMAME>
	<FileFormatsToSearch>
		<FormatToSearch>zip</FormatToSearch>
	</FileFormatsToSearch>
	<ExtractFileBeforeLaunch>false</ExtractFileBeforeLaunch>
	<FileFormatsToLaunch>
		<FormatToLaunch></FormatToLaunch>
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

- **Update Notifications:** You will be notified if a new version is available.
- **Error Logging:** The application also has an error logging mechanism that notifies the developers of any errors that occur. This way, we can fix bugs and improve the program over time.

## Related Utilities:

- **[PS3BatchLauncherCreator](https://github.com/drpetersonfernandes/ps3batchlaunchercreator):** A program that automatically creates BAT files to easily launch PS3 games on the RPCS3 emulator. Written by a Simple Launcher developer.
- **[MAME Utility](https://github.com/drpetersonfernandes/MAMEUtility):** A utility to manage the MAME full driver information in XML format that is available on the [MAME](https://www.mamedev.org/release.html)  website. It can generate multiple simplified (and smaller) XML subsets and also copy ROMs and image files based on the created XML. Written by a Simple Launcher developer.
- **[FindRomCover](https://github.com/drpetersonfernandes/FindRomCover):** A program that helps you organize your cover image collection. It attempts to match the filename of image files with the filename of the ROMs. User can choose the similarity Algorithm to compare filenames of files. Written by a Simple Launcher developer.