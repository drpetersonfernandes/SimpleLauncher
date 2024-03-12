# Welcome to the SimpleLauncher Wiki!

## Introduction
Simple Launcher is an emulator frontend that allows you to play retro games with ease.

## Installation
Simply download the application from the [release](https://github.com/drpetersonfernandes/SimpleLauncher/releases) page, and then extract the zip file to your chosen location.

## Usage
* Click on the 'Edit System' menu item.
* Configure each system of your choice. You have the option to add, edit or delete systems.
* Create a folder with the same name as the system you created in the 'images' folder.
* Place all your cover images for that specific system inside this folder.
* Now, select the system from the dropdown menu at the top of the application.
* Select the emulator you wish to use.
* Select a button at the top of the application.
* Click on the game you want to launch.

Note: You also have the option to use a custom System Image Folder, which can be set using the 'Edit System' menu.

## Where to Find Game Covers:
Please note that we do NOT provide ROMs, ISOs or Game Covers. You can find cover images on websites like [Libretro Thumbnails](https://github.com/libretro-thumbnails/libretro-thumbnails) or [EmuMovies](https://emumovies.com). However, these sites are not affiliated with Simple Launcher.

## List of Parameters for Emulators
We have compiled a list of parameters for each emulator for your convenience. Click [here](https://github.com/drpetersonfernandes/SimpleLauncher/wiki/parameters) to see the list.

## Special Settings:

**Microsoft Windows Games or Applications**

If you want to add your Windows games or applications to the Simple Launcher frontend, you should create a folder with Shortcut (lnk), BAT (bat) or Executable (exe) files that directly call the game or application.
If you prefer to use BAT (bat) files, you can use the model below as an example of a bat file.

```bat
@echo off
cd /d "J:\Microsoft Windows\Mega Man 11"
start game.exe
```

**RPCS3 Emulator**

This emulator loads games extracted from ISO files into a Game Folder.<br>
To launch a specific game, it searches for a file named EBOOT.BIN located inside the `PS3_GAME\USRDIR\` or `USRDIR\` directories.  
Configuring these games in an Emulator Frontend can be a challenge. The quickest and easiest solution we found is to create batch (.bat) files that can directly launch the games. These batch (.bat) files can be configured in Simple Launcher to be treated as games.  
For the Sony Playstation 3 system configuration, you should set the Format To Search In System Folder as batch (.bat).
We have created a utility that can automatically generate these batch files for you. Please see the "Related Utilities" section below for more information.

## How the Frontend Works:

- **Configuration:** The program first looks for a file named "system.xml" in its folder, which contains all the settings for the system and emulators. You need to edit this file to match your requirements.
- **Game Selection:** When you select a system, the application opens the system directory and lists the emulators configured for that specific system. It then displays a grid of games located in the system folder. Each cell in the grid is clickable, and the application will launch the selected emulator with the chosen game.
- **Game Info:** Each grid cell displays a game cover, its name, a link to a Video about the game, and a link to an Info Page about the game.
- **Game Covers:** The cover images should have the same filename as the game. They are loaded from a folder inside the "images" folder, which should have the same name as the system. The images must be in PNG, JPG, or JPEG format. If a cover is missing, a default image is used. You can also use a custom System Image Folder, which should be set in the "system.xml".

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
- **EmulatorParameters**: The parameters utilized for each emulator. Not all emulators require parameters.

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

- **Edit Systems Menu:** This function allows you to easily edit, add, or delete a System.
- **Edit Links Menu:** Customize the Video and Info links within the UI.
- **Control Thumbnail size:** Adjust the size of Cover images in the UI to your preference.
- **Update Notifications:** Notifications will inform you when a new version is available.
- **Error Logging:** The application features an error-logging mechanism that sends developers notifications about any occurring errors. This enables us to fix bugs and make improvements over time.

## Related Utilities:

- **[PS3BatchLauncherCreator](https://github.com/drpetersonfernandes/ps3batchlaunchercreator):** This program automatically creates BAT files to facilitate launching PS3 games on the RPCS3 emulator.
- **[MAME Utility](https://github.com/drpetersonfernandes/MAMEUtility):** This utility manages the MAME full driver information in XML format available on the [MAME](https://www.mamedev.org/release.html) website. It can generate multiple simplified (and smaller) XML subsets and also copy ROMs and image files based on the created XML.
- **[FindRomCover](https://github.com/drpetersonfernandes/FindRomCover):** This program helps organize your cover image collection. It attempts to match each image file's filename with the corresponding ROM's filename. Users can select the similarity algorithm for comparing filenames.

## Technical Details:

Simple Launcher is written in C# using Windows Presentation Foundation (WPF) and the Microsoft .NET 8 Framework.<br>
This program is Windows-only. It is expected to be compatible with Windows 7 and subsequent versions. Tests have been carried out on Windows 11.

## Support the Project:

Have you enjoyed using the Simple Launcher frontend? Consider [donating](https://www.buymeacoffee.com/purelogiccode) to support the project or just to express your appreciation!

## Contributors:

- **Peterson Fernandes** - [Github Profile](https://github.com/drpetersonfernandes)
- **RFSVIEIRA** - [Github Profile](https://github.com/RFSVIEIRA)