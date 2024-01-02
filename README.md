# Simple Launcher

A simple emulator launcher for Windows.

![Screenshot](screenshot.png)

This program reads a file called "system.xml" located inside the program folder. All system and emulator settings are stored in this file.

Based on the selected system, the application opens the system directory and the list of emulators configured for that specific system. It then creates a grid of games located inside the system folder. Each cell of the grid is clickable, and the app will launch the selected emulator with the chosen game. Each cell of the grid displays a game cover, its name, a YouTube link, and an info link.

The cover images should have the same filename as the game to be launched. These images will be loaded from a folder inside the images folder, which needs to have the same name as the specific system. The images must be in PNG format and have the same filename as the game. If the launcher doesn't find an image with the matching filename inside the folder, it will load "default.png". You can find cover images on the website https://github.com/libretro-thumbnails/libretro-thumbnails, with which I'm not affiliated.

The format of the "system.xml" file is as follows:

```xml
<SystemConfigs>
    <SystemConfig>
        <SystemName>Amstrad CPC GX4000</SystemName>
        <SystemFolder>G:\OK\Amstrad CPC GX4000</SystemFolder>
        <SystemIsMAME>false</SystemIsMAME>
        <FileFormatsToSearch>
            <FormatToSearch>zip</FormatToSearch>
            <FormatToSearch>7zip</FormatToSearch>
        </FileFormatsToSearch>
        <ExtractFileBeforeLaunch>false</ExtractFileBeforeLaunch>
        <FileFormatsToLaunch>
            <FormatToLaunch></FormatToLaunch>
            <FormatToLaunch></FormatToLaunch>
        </FileFormatsToLaunch>
        <Emulators>
            <Emulator>
                <EmulatorName>Retroarch</EmulatorName>
                <EmulatorLocation>G:\Emulators\RetroArch\retroarch.exe</EmulatorLocation>
                <EmulatorParameters>-L "G:\Emulators\Retroarch\cores\cap32_libretro.dll" -c "G:\Emulators\Retroarch\Config.cfg" -f</EmulatorParameters>
            </Emulator>
        </Emulators>
    </SystemConfig>
</SystemConfigs>
```

You can add as many systems and emulators as you want.

The "system.xml" contains the following fields:

- **SystemName**: Name of the system.
- **SystemFolder**: Folder where the ROMs or games are located.
- **SystemIsMAME**: Notify the program whether the system is based on MAME or not. If true, the application will load the ROM descriptions alongside the ROM filenames.
- **FormatToSearch**: List of file extensions that will be loaded from the SystemFolder. You can use as many as you want.
- **ExtractFileBeforeLaunch**: Should be true or false. If true, the launcher will extract the zip or 7z file into a temp folder, then it will load the extracted file.
- **FormatToLaunch**: In case you extract the file to a temp folder. You should specify here which extensions will be launched from the extracted folder.
- **EmulatorName**: Name of the emulator. You can add as many emulators as you want for each system.
- **EmulatorLocation**: Location of the emulator.
- **EmulatorParameters**: Parameters that are used for each emulator. Not all emulators need parameters.

![Screenshot](screenshot2.png)

The program also reads the file "settings.xml", which contains the user's preferences. The format of this file is as follows:

```xml
<Settings>
	<ThumbnailSize>350</ThumbnailSize>
	<HideGamesWithNoCover>false</HideGamesWithNoCover>
	<EnableGamePadNavigation>true</EnableGamePadNavigation>
</Settings>
```

The "settings.xml" contains the following fields:

- **ThumbnailSize**: Height of the thumbnail.
- **HideGamesWithNoCover**: Whether to hide games without a cover.
- **EnableGamePadNavigation**: Whether to enable GamePad navigation.

![Screenshot](screenshot3.png)

This program is Windows-only and has been tested on Windows 11.

## Code Language
*Written in C# using<br>
Microsoft Visual Studio Community 2022 (64-bit) Version 17.9.0 Preview 2.0<br>
Windows Presentation Foundation (WPF) Framework<br>
Microsoft .NET 8.0*

## Contributors
- Peterson Fernandes - [Github Profile](https://github.com/drpetersonfernandes)
- RFSVIEIRA - [Github Profile](https://github.com/RFSVIEIRA)
