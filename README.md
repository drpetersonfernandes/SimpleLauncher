# Simple Launcher

A simple emulator launcher for Windows.

![Screenshot](screenshot.png)

This program reads a file called "system.xml" located inside the program folder. All the system and emulator settings are stored in this file.

Based on the selected system, the application opens the system directory and the list of emulators configured for that specific system. After that, it creates a grid of games located inside the system folder. Each cell of the grid is clickable, and the app will launch the selected emulator with the selected game. Each cell of the grid has a game cover, its name, a YouTube link, and an info link.

The cover images should have the same filename as the game to be launched. These images will be loaded from a folder inside the images folder, which needs to have the same name as the specific system. The images must be in PNG format and need to have the same filename as the game. The optimal image size should be 200 pixels in height. If you use a higher resolution, the app will still load the image but will consume more memory. If the launcher doesn't find an image with the matching filename inside the folder, it will load "default.png".

The format of the "system.xml" file is as follows:

```xml
<SystemConfigs>
    <SystemConfig>
        <SystemName>Amstrad CPC GX4000</SystemName>
        <SystemFolder>G:\OK\Amstrad CPC GX4000</SystemFolder>
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

The XML file contains the following fields:

- **SystemName**: Name of the system.
- **SystemFolder**: Folder where the ROMs or games are located.
- **FileFormatsToSearch**: List of file extensions that will be loaded from the SystemFolder. You can use as many as you want.
- **ExtractFileBeforeLaunch**: Should be true or false. If true, the launcher will extract the zip or 7z file into a temp folder, then it will load the extracted file.
- **FormatToLaunch**: In case you extract the file to a temp folder. You should specify here which extensions will be loaded from the extracted folder.
- **EmulatorName**: Name of the emulator. You can add as many emulators as you want for each system.
- **EmulatorLocation**: Location of the emulator.
- **EmulatorParameters**: Parameters that are used for each emulator. Not all emulators need parameters.

This program is Windows-only and has been tested on Windows 11.

### Code Language
*Written in C# using<br>
Microsoft Visual Studio Community 2022 Version 17.8.0 Preview 5.0<br>
Windows Presentation Foundation (WPF) Framework<br>
Microsoft .NET Framework Version 4.8.09032*

### Contributors
- Peterson Fernandes - [Github Profile](https://github.com/drpetersonfernandes)
- RFSVIEIRA - [Github Profile](https://github.com/RFSVIEIRA)
