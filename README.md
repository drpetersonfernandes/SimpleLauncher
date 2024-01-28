# Simple Launcher

Simple Launcher is a free program for Windows that lets you play games on emulators with ease.<br><br>

![Screenshot](screenshot.png)

## How it Works:

- **Configuration:** The program looks for a file called "system.xml" in its folder, which holds all the system and emulator settings. You should edit this file to comply wich your needs.

- **Game Selection:** When you choose a system, the application opens the system directory and the list of emulators configured for that specific system. It then creates a grid of games located inside the system folder. Each cell of the grid is clickable, and the app will launch the selected emulator with the chosen game.

- **Game Info:** Each grid cell shows a game cover, its name, a link to a YouTube video about the game, and an info link.

- **Game Covers:** The cover images should have the same filename as the game. They are loaded from a folder inside the images folder, which should have the same name as the system. The images must be in PNG, JPG or JPEG format. If a cover is missing, it uses a default image.<br><br>

![Screenshot](screenshot2.png)

## Where to Find Game Covers:

You can find cover images on websites like https://github.com/libretro-thumbnails/libretro-thumbnails or https://emumovies.com/, but these sites are not affiliated with Simple Launcher.

## Configuration File ("system.xml"):

This file holds information about different systems and their settings. You can add as many systems and emulators as you want. You should manually configure this file to meet your needs.

- **SystemName**: Name of the system.

- **SystemFolder**: Folder where the ROMs or games are located.

- **SystemIsMAME**: Notify the program whether the system is based on MAME or not. If true, the application will load the ROM descriptions alongside the ROM filenames.

- **FormatToSearch**: List of file extensions that will be loaded from the SystemFolder. You can use as many as you want.

- **ExtractFileBeforeLaunch**: Should be true or false. If true, the launcher will extract the ZIP or 7Z file into a temp folder, then it will load the extracted file.

- **FormatToLaunch**: In case you extract the file to a temp folder. You should specify here which extensions will be launched from the extracted folder.

- **EmulatorName**: Name of the emulator. You can add as many emulators as you want for each system.

- **EmulatorLocation**: Location of the emulator.

- **EmulatorParameters**: Parameters that are used for each emulator. Not all emulators need parameters.<br><br>

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

## User Preferences ("settings.xml"):

This file contains your preferences for the program, such as thumbnail size, hiding games without covers, and enabling GamePad navigation.<br>
You should not manually configure this file. The file will be managed directly by the aplication. To change these settings you use the application menu.

- **ThumbnailSize**: Height of the thumbnail.

- **HideGamesWithNoCover**: Whether to hide games without a cover.

- **EnableGamePadNavigation**: Whether to enable GamePad navigation.<br><br>

```xml
<Settings>
	<ThumbnailSize>350</ThumbnailSize>
	<HideGamesWithNoCover>false</HideGamesWithNoCover>
	<EnableGamePadNavigation>true</EnableGamePadNavigation>
</Settings>
```
<br>

![Screenshot](screenshot3.png)

## Additional Features:

- **Update Notifications:** You'll be notified if a new version is available.

- **Error Logging:** The aplication also has an error logging mechanism that notify the developers of any errors that may occur. This way we can fix bugs and improve the program over time.

## Special Settings:

**RPCS3**

This emulator loads games extracted from ISO files into a folder.<br>
To launch a specific game, it searches for a file named EBOOT.BIN located inside the PS3_GAME\USRDIR\ or USRDIR\ directories. This makes it tricky to configure these games in an emulator frontend. The quickest and easiest solution we found was to create BAT files that can launch the game with ease. These BAT files can then be configured in Simple Launcher to be treated as games.<br>
When configuring this system in "system.xml" you should set `<FormatToSearch>bat</FormatToSearch>` as shown below.<br>
We have created a utility that can generate these BAT files for you. Please see the section "Related Utilities" for more information.<br><br>

```xml
<SystemConfig>
	<SystemName>Sony Playstation 3</SystemName>
	<SystemFolder>J:\Sony PS3 Roms</SystemFolder>
	<SystemIsMAME>false</SystemIsMAME>
	<FileFormatsToSearch>
		<FormatToSearch>bat</FormatToSearch>
	</FileFormatsToSearch>
	<ExtractFileBeforeLaunch>false</ExtractFileBeforeLaunch>
	<FileFormatsToLaunch>
		<FormatToLaunch></FormatToLaunch>
	</FileFormatsToLaunch>
	<Emulators>
		<Emulator>
			<EmulatorName>RPCS3</EmulatorName>
			<EmulatorLocation></EmulatorLocation>
			<EmulatorParameters></EmulatorParameters>
		</Emulator>
	</Emulators>
</SystemConfig>
```

<br>

**MAME**

When setting up the MAME emulator you should set SystemIsMAME to true `<SystemIsMAME>true</SystemIsMAME>` this way the application will load the game description into UI alongside the game filename.<br>
When setting the EmulatorParameters you just need to put the folder where your games are located `<EmulatorParameters>-rompath "G:\OK\MAME\MAME Roms"</EmulatorParameters>`.<br>
You can also launch MAME Roms using the Retroarch emulator just like the example below.<br>
Another way to launch MAME Roms is using RocketLauncher just like the example below. For this to work [RocketLauncher](https://www.rlauncher.com/) need to be configured to launch MAME Roms.<br><br>

```xml
<SystemConfig>
	<SystemName>MAME</SystemName>
	<SystemFolder>G:\OK\MAME\MAME Roms</SystemFolder>
	<SystemIsMAME>true</SystemIsMAME>
	<FileFormatsToSearch>
		<FormatToSearch>zip</FormatToSearch>
	</FileFormatsToSearch>
	<ExtractFileBeforeLaunch>false</ExtractFileBeforeLaunch>
	<FileFormatsToLaunch>
		<FormatToLaunch></FormatToLaunch>
	</FileFormatsToLaunch>
	<Emulators>
		<Emulator>
			<EmulatorName>MAME</EmulatorName>
			<EmulatorLocation>G:\Emulators\MAME\mame.exe</EmulatorLocation>
			<EmulatorParameters>-rompath "G:\OK\MAME\MAME Roms"</EmulatorParameters>
		</Emulator>
		<Emulator>
			<EmulatorName>Retroarch</EmulatorName>
			<EmulatorLocation>G:\Emulators\RetroArch\retroarch.exe</EmulatorLocation>
			<EmulatorParameters>-L "G:\Emulators\Retroarch\cores\mame_libretro.dll" -c "G:\Emulators\Retroarch\Config.cfg" -f</EmulatorParameters>
		</Emulator>
		<Emulator>
			<EmulatorName>MAME through RocketLauncher</EmulatorName>
			<EmulatorLocation>G:\Emulators\RocketLauncher\RocketLauncher.exe</EmulatorLocation>
			<EmulatorParameters>"MAME"</EmulatorParameters>
		</Emulator>
	</Emulators>
</SystemConfig>
```

<br>

If you want to use MAME to launch Software List Roms (or MESS Roms) you should put the name of the system in the EmulatorParameters `<EmulatorParameters>gx4000 -cart</EmulatorParameters>` just like the example below.<br>
For this to work you need to follow the patterns found in folder .\MAME\hash\\. You can found a list of XML files inside this folder that represent all the systems that MAME can emulate. Use the XML filename of the chosen system in the EmulatorParameters.<br><br>

```xml
<SystemConfig>
	<SystemName>Amstrad CPC GX4000</SystemName>
	<SystemFolder>G:\OK\Amstrad CPC GX4000</SystemFolder>
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
			<EmulatorName>MAME gx4000</EmulatorName>
			<EmulatorLocation>G:\Emulators\MAME\mame.exe</EmulatorLocation>
			<EmulatorParameters>gx4000 -cart</EmulatorParameters>
		</Emulator>
	</Emulators>
</SystemConfig>
```

## Related Utilities:

- **[PS3BatchLauncherCreator](https://github.com/drpetersonfernandes/ps3batchlaunchercreator):** Program that automatic create BAT files to easily launch PS3 games on the RPCS3 emulator. Written by a Simple Launcher developer.

## Technical Details:

Simple Launcher is written in C# using Microsoft Visual Studio Community 2022 (64-bit) and the Windows Presentation Foundation (WPF) Framework with Microsoft .NET 8.0.<br>
This program is Windows-only. Compatibility with Windows 7 and later versions is expected. It has been tested on Windows 11.

## Contributors

- **Peterson Fernandes** - [Github Profile](https://github.com/drpetersonfernandes)
- **RFSVIEIRA** - [Github Profile](https://github.com/RFSVIEIRA)
