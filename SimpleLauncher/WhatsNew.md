# Release 4.4
*09/17/25*
---

### 🖥️ Platform Support
- Introduced support for Windows-arm64

### 🎨 User Interface
- Improved UI responsiveness in `PlayHistoryWindow` and `FavoritesWindow`
- Replaced `PleaseWaitWindow` with a consistent, embedded `LoadingOverlay`
- Added "Delete Cover Image" option in the right-click context menu

### 🔧 Technical Improvements
- Improved error handling and user notifications
- Updated existing emulator versions
- Enabled MAME description support in `FindRomCover` tool
- Upgraded NuGet packages `MahApps.Metro` and `Microsoft.Extensions`
- Transitioned `SettingsManager` and `FavoritesManager` to Dependency Injection (DI)
- Improved localization resource management

# Release 4.3
*07/26/25*
---

### Major Feature: Multi-Folder Support
- **Multiple System Folders:** The core logic has been refactored to allow each system to scan for games across multiple folders instead of just one.

### UI & UX Enhancements
- **Improved Button Styles:** Game and system buttons now feature refined 3D styles with hover and press animations for a more dynamic user experience.

### Stability Improvements
- **Improved Mounting Logic:** The on-the-fly mounting for ZIP and XISO files no longer uses fixed delays. It now uses a more robust polling mechanism that actively checks for successful mounts, improving reliability and reducing unnecessary waiting.

### Tooling
- **New Tool Integration:** A new `RomValidator` tool has been added. This tool can compare user game files with No-Intro dat files.

### Other Enhancements
- **Emulator Updates:** Documentation and help files have been updated with information for new emulators (Gopher64, Hades, VisualBoy Advance M, Retroarch SwanStation).
- **Emulator Updates:** Emulators from Easy Mode have been updated to the latest versions.
- **MAME Database Update:** The mame.dat file has been updated to MAME 0.278.
- **History Database Update:** The history.xml file has been updated to the latest version.

# Release 4.2.0
*07/10/25*
---
### Core Feature Enhancements: On-the-Fly File Mounting
The most significant change is the introduction of on-the-fly file mounting, which allows users to launch games directly from compressed or disk image files without needing to manually extract them first.
**Note:** You need to install the Dokan from [GitHub](https://github.com/dokan-dev/dokany) for ZIP and XISO file mounting.

*   **ISO & ZIP Mounting for RPCS3:** The launcher can now mount `.iso` and `.zip` files for the PlayStation 3 emulator (RPCS3). It uses PowerShell for native ISO mounting and a new `SimpleZipDrive.exe` tool for ZIP files. After mounting, it automatically finds and launches the required `EBOOT.BIN` file.
*   **XISO Mounting for Cxbx-Reloaded:** Support has been added to mount Xbox ISO (`.xiso`) files for the Cxbx-Reloaded emulator. This is handled by a new `MountXisoFiles` service that uses the `xbox-iso-vfs.exe` tool to create a virtual drive and launch the `default.xbe` file.
*   **XBLA ZIP Mounting:** The system can now mount `.zip` files for Xbox Live Arcade (XBLA) games, searching for a specific nested file structure required to launch them.
*   **ScummVM ZIP Mounting:** The system can now mount `.zip` files for ScummVM games and automatically launch the game.

### Major Refactoring & Dependency Changes
The project's core dependencies and internal logic for handling files have been substantially overhauled to improve robustness and unify functionality.

*   **Unified Archive Handling:** The application has migrated away from using multiple libraries and external executables for file extraction. It now primarily uses `Squid-Box.SevenZipSharp` and `ICSharpCode.SharpZipLib`, providing a more integrated and reliable way to handle `.zip`, `.7z`, and `.rar` files. The `ExtractCompressedFile` service was refactored to use these new libraries.
*   **Removal of Caching System:** The `CacheManager` has been completely removed. The previous system of caching game lists for each system has been replaced with a more direct, on-demand file scanning approach. This simplifies the application's logic, eliminates the `cache` directory, and ensures the game list is always up to date.

### Update Tools
*   **BatchConvertToCHD:** Improved UI. Added the ability to check the integrity of CHD files and convert CSO files to CHD.
*   **BatchConvertToCompressedFile:** Improved UI. Added the ability to verify the integrity of compressed files.
*   **BatchConvertIsoToXiso:** Improved UI. Added the ability to verify the integrity of XISO files.
*   **BatchConvertToRvz:** Improved UI. Added the ability to test the integrity of the RVZ files.

### Bug Fixes and Other Enhancements
*   **Fixed Threading Issue:** The `PlayTimeWindow` and `FavoritesWindow` have been refactored to handle file operations and UI updates more safely across different threads, preventing crashes and ensuring a smoother user experience.
*   **UI Consistency:** The system selection screen now limits the maximum size of system thumbnails to 100 px to prevent oversized images from distorting the layout and to ensure a consistent look.

# Release 4.1.0
*06/08/25*
---
*   Added a Sound Configuration feature allowing customizable notification sounds.
*   Implement a Debug Logger with a dedicated UI window for enhanced logging.
*   Major overhaul and standardization of path resolution and handling, including support for placeholders `%BASEFOLDER%`, `%SYSTEMFOLDER%` and `%EMULATORFOLDER%`.
*   Refactored Parameter Validation logic for clarity and robustness, particularly concerning paths.
*   Improved Error Handling, Logging, and messaging across various components (`GameLauncher`, `Services`, `Managers`), replacing `Debug.WriteLine` with the new `DebugLogger`.
*   Added Status Bar updates to show current actions (e.g., game launched).
*   Introduced 3D button styles for navigation buttons.
*   Updated Tray Icon messaging and functionality (including Debug Window access).
*   Refactored Edit System Window logic and validation for better user feedback on paths.

# Release 4.0.1
*05/26/25*
---
### Bug Fix Version
-   **Parameter Handling Overhaul (`ParameterValidator.cs`, `GameLauncher.cs`):** Paths within parameters are converted to absolute paths.
-   **File Path Standardization (`GameLauncher.cs`, `GameButtonFactory.cs`, `GameListFactory.cs`):** File paths used for launching games and creating UI elements are consistently resolved to absolute paths relative to the application directory, improving reliability.
-   **UI Enhancements (`MainWindow.xaml`):** A new custom-styled `GridSplitter` has been added to the list view in the main window, allowing for better resizing of the game list and preview areas.
-   **Improved Window Closing (`DownloadImagePackWindow.xaml.cs`, `EasyModeWindow.xaml.cs`):** Added logic to the `CloseWindowRoutine` in `DownloadImagePackWindow` and `EasyModeWindow` to stop any ongoing operations (like downloads) before the window closes, preventing potential errors.
-   **Configuration Change (`SystemManager.cs`):** The default value for `ReceiveANotificationOnEmulatorError` in system configurations has been changed from `false` to `true`, meaning users will now receive error notifications by default.

# Release 4.0
*05/24/2025*
---
This release brings significant user-interface improvements, new features for game browsing and management, enhanced stability, and various under-the-hood technical refinements.

### Improvements
-   **New System Selection Screen:** Displayed a more visual screen when no system is chosen to simplify platform selection.
-   **Revamped Navigation Panel:** Added a panel on the left side of the main window with quick-access buttons for Restart, Global Search, Favorites, Play History, Edit System (Expert Mode), Toggle View Mode, Toggle Aspect Ratio, Zoom In, and Zoom Out.
-   **Switchable View Modes:** Enabled toggling between Grid View (with game-cover images) and List View (showing game details in a table).
-   **Enhanced Zooming:** Allowed zooming in and out of game thumbnails in Grid View via the new buttons or the `Ctrl` + `mouse-wheel` shortcut.
-   **Displayed Game File Sizes:** Showed file sizes in List View.
-   **Overhauled UI Layout:** Redesigned the main window and multiple dialogs (Easy Mode, Download Image Pack, Global Search, Favorites, Play History, Edit System).
-   **Refined Error Handling:** Improved error messages and user prompts throughout the application.
-   **Updated Documentation Links:** Improved Help User section and parameters' guide links.
-   **Enhanced Gamepad Logic:** Made controller navigation more reliable.
-   **Standardized Sound Effects:** Switched to a consistent notification sound across the app.
-   **Updated Batch Convert To CHD:** Added support for `.7z` and `.rar` files, enabled parallel processing, and improved multithreading.
-   **Added Batch Convert To RVZ:** Introduced a tool to convert GameCube and Wii ISO files to RVZ format.
-   **Updated Emulators:** Upgraded to the latest emulator versions.
-   **Updated MAME Data File:** Bumped to version `0.277`.

### Bug Fixes & Internal Changes
-   Fixed various Gamepad Controller issues.
-   Corrected typos and added null checks.
-   Addressed default-logic errors in emulator error notifications.
-   Prevented potential XML vulnerabilities during file loading/saving.
-   Fixed context-menu parameter-handling bugs.
-   Refactored and reorganized code for better maintainability and performance.
-   Replaced `Newtonsoft.Json` with `System.Text.Json` for JSON processing.
-   Improved file-path handling: more robust relative-path resolution and folder-name sanitization.
-   Enhanced asynchronous operations for smoother UI responsiveness.
-   Implemented stricter code-quality checks.
-   Consolidated "Please Wait" dialog logic.
-   Removed redundant or unused code.
-   Improved the structure and reusability of context-menu creation logic.
-   Integrated `IHttpClientFactory` for HTTP client management.

# Release 3.13
*04/27/2025*
---
-   Updated the **Batch Convert To Compressed File** tool to skip files that are already compressed.
-   Improved the UI in the **Easy Mode** and **Download Image Pack** windows.
-   Added code to enforce a single instance of the application.
-   Implemented fuzzy image matching to enhance the user experience. This logic finds a cover image that partially matches the ROM filename. Users can disable this feature if desired.
-   Updated emulators to the latest version.
-   Updated `history.xml` (available at arcade-history.com) to the latest version.

# Release 3.12
*03/31/2025*
---
-   Added an option to turn off emulator error notifications from 'Simple Launcher'. Users can configure this per emulator in the **Edit System - Expert Mode**.
-   Updated the **Batch Convert to CHD** tool application to accept new input formats, including ZIP files.
-   Added the **Batch Convert to Compressed File** tool.
-   Added the **Batch Verify CHD Files** tool.
-   Added the **Batch Verify Compressed Files** tool.
-   Added a **Support Request** window. Users can send questions to the developers.
-   Updated emulators to the latest version.
-   Updated the MAME database to the latest version (v0.276).
-   Updated `history.xml`, available at arcade-history.com, to the latest version.

# Release 3.11.1
*03/22/2025*
---
-   Added logic to check file and folder paths in the parameter field. Users will be notified if the check returns invalid.
-   Updated the `LogError` class to use a new, more flexible API.
-   Added a bug report feature for all the tools bundled with 'Simple Launcher'.
-   Improved the **BatchConvertToCHD** tool to better handle the deletion of original files after conversion to CHD.
-   Fixed some bugs.

# Release 3.11
*03/12/2025*
---
-   Added **Play History Window** and all the logic to implement it. Users can now see which games have been played and for how long.
-   Implemented the **Feeling Lucky Button** in the Main Window UI. This button will pick a random game for the selected system.
-   Updated the MAME database to the latest version (v0.275).
-   Changed the logic to load the MAME database into Simple Launcher. Instead of using a large XML file, it will now use a binary DAT file. This improves the database loading time and reduces the database size.
-   Updated emulators to the latest version.
-   Fixed some bugs.
-   Improved the `LogError` logic to prevent UI freezing.
-   Updated the logic behind the tray icon to a more modern approach.

# Release 3.10.2
*03/03/2025*
---
-   Fixed an issue in List View mode where preview images failed to load.

# Release 3.10.1
*03/02/2025*
---
-   Fixed some bugs.
-   Enhanced methods and functions to improve efficiency and reduce errors.

# Release 3.10
*02/23/2025*
---
-   Exposed the GamePad DeadZone variable to the user.
-   Improved the Game Button generation for the Grid View option. Now users can customize the aspect ratio of the buttons.
-   Updated the emulators to their latest releases.
-   Updated `history.xml`, available at arcade-history.com, to the latest version.
-   Fixed some bugs.
-   Enhanced methods and functions to improve efficiency and reduce errors.

# Release 3.9.1
*02/08/2025*
---
-   Significantly improved the speed of the search engine in the Main Window and in the Global Search Window.
-   Fixed some bugs.
-   Enhanced methods and functions to improve efficiency and reduce errors.

# Release 3.9
*02/07/2025*
---
-   Enhanced support for Xbox controllers and added support for PlayStation controllers. I tested it with the following controllers: Xbox 360, Xbox One, Xbox Series X/S, PlayStation 4 DualShock, and PlayStation 5 DualSense.
-   Improved the text formatting of developer suggestions in the Edit Window.
-   Enhanced the translation resources: even error messages are now translated into 17 languages.
-   Updated the emulators to their latest releases.
-   Updated `history.xml` to the latest version.
-   Updated `mame.xml` to the latest version (0.274).
-   Added a tool to batch convert regular ISO files to the XISO (Xbox) format.
-   Added a tool to batch convert CUE, BIN, ISO, and IMG files to the CHD (MAME) format.
-   Added a tool to batch compress files into ZIP or 7Z archives.
-   Fixed some bugs.
-   Improved several methods and functions to enhance efficiency and reduce errors.

# Release 3.8.1
*01/16/2025*
---
-   Bug fixes.
-   Improved some methods and functions to enhance efficiency and reduce errors.

# Release 3.8
*01/15/2025*
---
### Multilingual Version – A Major Update!
-   **Experience the Application in Your Language!** We've added translations for 17 languages, making the application accessible to users worldwide. Supported languages now include: Arabic, Bengali, German, Spanish, French, Hindi, Indonesian (Malay), Italian, Japanese, Korean, Dutch, Portuguese, Russian, Turkish, Urdu, Vietnamese, Simplified Chinese, and Traditional Chinese.
-   Updated to the latest technologies: .NET Core 9 and C# 13, ensuring better performance and stability.
-   Fixed several entries in `helpuser.xml` for improved user assistance.
-   Updated all emulators to their latest releases.
-   Updated `mame.xml` to version 0.273.
-   Enhanced support for 'Kega Fusion' and 'Mastergear'.
-   Added more emulators to `system_model.xml`.

# Release 3.7.1
*12/12/2024*
---
-   Fixed bugs and improved exception handling in certain methods.
-   Added image packs for specific systems.

# Release 3.7
*12/08/2024*
---
-   Added a tool to generate BAT files for Xbox 360 XBLA games.
-   Added a Helper class to help users during system edits.
-   Improved the UI in the Edit window.
-   Added support for launching Mattel Aquarius games on MAME using the frontend.
-   Improved the code that gets the application version from GitHub.
-   Updated the emulator version of the Easy Mode emulators to the latest version.
-   Updated `mame.xml` to the latest version (MAME 0.272). I have also added Software List games to this list.
-   Updated `parameters.md` and `system_model.xml`.
-   Updated the binaries of the tool `FindRomCover` to the latest version.
-   Added image packs for Philips CD-i and Atari ST.
-   Added a new menu item to download image packs.
-   Added Mattel Aquarius and Sharp x68000 to Easy Mode.
-   Updated the rom history database to the latest version (ver. 2.72) from [arcade-history.com](https://www.arcade-history.com/).
-   Fixed bugs and improved exception handling in some methods.

# Release 3.6.3
*11/26/2024*
---
-   Changed the temp folder from within the application folder to the Windows temp folder to prevent access issues.
-   Implemented methods to handle the cleanup of temp files and folders.

# Release 3.6.2
*11/25/2024*
---
-   Improved error handling in some methods.

# Release 3.6.1
*11/24/2024*
---
-   Fixed a bug in several methods that attempt to load a preview image or open a cover image when the `UserImagePath` is a relative path. These methods now convert all relative paths to absolute paths before execution.
-   Moved the `DownloadAndExtractInMemory` method to a separate class.
-   Improved error handling in some methods.

# Release 3.6.0
*11/23/2024*
---
-   Performed a major refactoring of the `EditSystem` class to implement better validation in the input fields of the `EditSystem` window.
-   Improved the system validation mechanism of the `system.xml` file when 'Simple Launcher' loads.
-   Implemented in-memory download and extraction of files as a backup method in case the regular extraction method fails.
-   Added different versions of the `7z` executable for x86 and x64.
-   Added code to automatically convert URL-formatted text into real links in the `RomHistory` window.
-   Improved the log class to send more debug information to the developer.
-   Added functionality to delete a game file from within the UI, available in the right-click context menu.
-   Added functionality to auto-generate image previews for the loaded games.
-   Added an image pack for Atari 8-Bit.
-   Updated the emulator links to the latest version.
-   Bug fixes.

# Release 3.5.0
*11/09/2024*
---
-   Added a tool to generate BAT files for Sega Model 3.
-   Added a local ROM history database using `history.xml` from [arcade-history.com](https://www.arcade-history.com/).
-   Added a new way to view the games in the main UI. Users can now choose between Grid View and List View.
-   Added a Star button to the Main Window top menu, which selects all the favorites for the selected system.
-   Improved error and exception notifications in several methods for both users and developers.
-   Added 'Sony PlayStation 3' image pack.
-   Increased the font size for the game filenames in the `GameButtonFactory`.
-   Added Sega Model 3 to Easy Mode with an ImagePack available for this system.
-   Updated emulator download links to the latest version.

# Release 3.4.1
*11/03/2024*
---
-   Automatically set the `SystemImageFolder` if the user does not specify it. It will be set to `.\images\SystemName`.
-   Implemented a way to retry extraction of the downloaded file in case it is locked by antivirus software.
-   Updated the download link for the emulators to the latest version in EasyMode.
-   Added 'Sony PlayStation 3' to the EasyMode installation window.
-   Updated `mame.xml` to the latest version.

# Release 3.4
*11/03/2024*
---
-   Added a new menu item called **Tools**. In it, you will find tools related to emulation.
-   Added a tool to Create Batch Files for PS3 Games.
-   Added a tool to Create Batch Files for ScummVM Games.
-   Added a tool to Create Batch Files for Windows Games.
-   Added a tool to Organize System Images.
-   Implemented code to automatically redirect the user to the Simple Launcher Wiki page if the provided parameters for emulator launch fail.
-   Implemented the automatic launch of the UpdateHistory window after an update so the user can see what is new in the release.
-   Implemented logic to automatically reinstall Simple Launcher if the required files are missing.
-   Improved the Updater application. It will always automatically install the latest version of Simple Launcher even if the parameters provided are invalid.
-   Fixed the UpdateHistory window to wrap the text. It is better for the user.

# Release 3.3.2
*10/27/2024*
---
-   Improved the field validation in the "Edit System" window to trim input values and prevent the input of empty spaces.
-   Enhanced support for Xbox 360 XBLA games in the Launcher.
-   Fixed the download of the 'ares' emulator.
-   Fixed the download of the 'Redream' emulator.
-   Fixed a bug in the 'Easy Mode' window to prevent the extraction of partially downloaded files.
-   Added Xbox 360 XBLA into `easymode.xml` and `system_model.xml`.

# Release 3.3.1
*10/24/2024*
---
-   Fixed a minor issue in the `GameLauncher` class that was triggered when MAME output warning messages about the ROM. It will now only consider error messages if the emulator outputs an error code.

# Release 3.3.0
*10/24/2024*
---
-   Added Image Packs for Amstrad CPC, Amstrad CPC GX4000, Arcade (MAME), Commodore 64, Microsoft MSX1, Microsoft MSX2, Sony PlayStation 1, Sony PlayStation 2, and Sony PSP.
-   Added a new **UpdateHistory** window that can be launched from the **About** window.
-   Added a button in the **GlobalStats** window that allows the user to save the generated report.
-   Fixed an error in the `GameLauncher` class that was generating an `InvalidOperationException`.
-   Improved the error logging in the `EditSystemEasyModeAddSystem` class.
-   Added logic to delete the favorite if the favorite file is not found.
-   Added new functionality to `Updater.exe`.

# Release 3.2.0
*10/22/2024*
---
-   Enabled download of ImagePacks for some systems.
-   Implemented a Tray Icon for the application.
-   Added the function to present the System Play Time for each System.
-   Made the filenames and file descriptions on each button in the Main window a little bigger.
-   Updated all emulators to the latest versions.
-   Updated the `mame.xml` file to the latest release.
-   Improved error handling in the **Easy Add System** window.
-   Enhanced error handling for missing the `default.png` file.
-   Enhanced the Global Stats class and XAML.
-   Improved error handling throughout the application.
-   Improved code in the `GameLauncher` and `ExtractCompressedFile` classes.

# Release 3.1.0
*07/18/2024*
---
-   Implemented a file check before launching a favorite game.
-   Fixed the freezing issue of the **Please Wait** window during a search in the **Main** window.
-   Capped the number of games per page at 500.
-   Included a second log window in the update process.
-   Enhanced the global search functionality to also search within the machine description of MAME files.
-   Updated the emulator download links to the latest version.
-   Fixed some random code issues.

# Release 3.0.0
*07/17/2024*
---
-   Implemented a theme functionality that enables users to personalize the application's appearance.
-   The user interface of the main window has been enhanced.
-   Added icons to some menu items.
-   Some images in the Edit window have been modified.
-   Created a context menu in the main user interface for the 'Remove From Favorites' option.
-   An image preview functionality has been implemented in the Favorite and Global Search windows.
-   The management of `mame.xml` and `system.xml` has been improved.

# Release 2.15.1
*07/14/2024*
---
-   Added a star icon to each game in the favorites' list.
-   Implemented a preview window that appears when users hover over a list item.
-   Enhanced logic for generating `favorites.xml` and `settings.xml` when missing.
-   Removed Video and Info links from the generated buttons (still accessible via right-click).

# Release 2.15
*07/04/2024*
---
-   Updated emulator versions in EasyMode.
-   Added Amstrad CPC to EasyMode.
-   Updated `mame.xml` to the latest release.
-   Updated `system_model.xml` to include Amstrad CPC.
-   Enhanced the Update Class.
-   Introduced a Cart Context Menu.
-   Improved the Global Search Window to include the System Name field.
-   Enhanced the Right-Click Context Menu for Global Search results.
-   Improved the Global Stats Window with a pie chart.
-   Fixed the default image viewer for various media types.
-   Implemented a Favorite function.
-   Added icons to the Right-Click Context Menu items.

# Release 2.14.4
*06/06/2024*
---
-   Improved the Global Search algorithm to accept logical operators `AND` and `OR`.
-   Fixed a hidden bug in the Main Window search engine.
-   Added an option for the user to select a ROM Folder in the **Easy Mode - Add System** Window.
-   Improved the algorithm for detection of emulator parameters.
-   Removed the parameter validation requirement to save a System in the **Expert Mode** Window.
-   Updated the emulator version and download links in `easymode.xml`.
-   Developed a mechanism to detect the version of installed emulators in the **Easy Mode - Add System** Class.

# Release 2.14.3
*06/04/2024*
---
-   Improved the automatic update mechanism and fixed a pagination bug.

# Release 2.14.2
*06/02/2024*
---
-   Fixed the automatic update process.

# Release 2.14.1
*06/02/2024*
---
-   Fixed a small bug in the About window.

# Release 2.14.0
*06/02/2024*
---
### Big Update Today!
-   Added **Easy Mode** to Add a System.
-   Improved error detection of parameter values.
-   Added a **Global Search** function.
-   Added a **Global Stats** Window.
-   Added a right-click Context Menu to every generated button.
-   Implemented an auto-update feature for the application.
-   Updated `mame.xml` to the latest version.
-   Improved extraction method.
-   Fixed all bugs found.

# Release 2.13.0
*05/17/2024*
---
-   Implemented an experimental algorithm to check the emulator parameters.
-   Improved the extraction method that extracts files to a temporary folder.
-   Made minor bug fixes.

# Release 2.12.1
*03/31/2024*
---
-   Improved the error notification system.
-   Fixed a bug with the emulator location check.
-   Implemented code to use the MAME game description instead of the filename in the Video and Info links.
-   Implemented code to handle missing `mame.xml` and `system_model.xml` files.

# Release 2.12.0
*03/25/2024*
---
-   Implemented basic checks for the System Folder, System Image Folder, and Emulator Location.
-   Made the exception notifications more user-friendly.
-   Devised a smart method for generating the `system.xml` file.

# Release 2.11.2.23
*03/20/2024*
---
-   Fixed a bug in the 'Show Games' option.

# Release 2.11.1.15
*03/19/2024*
---
-   Improved mechanism to handle games without cover.
-   Enhanced error logging mechanism.
-   Implemented handling for the loading of corrupted images into the UI.
-   Fixed some bugs and corrected variable names.

# Release 2.11.0.5
*03/17/2024*
---
-   Added a Search Engine to the application.
-   Updated `mame.xml` to the latest version.
-   Improved the launch error message.

# Release 2.10.0.10
*03/12/2024*
---
-   Fixed the vertical scroll.
-   The app now retains the window size and state between instances.
-   Users can now use a `default.png` file from the System Image Folder.
-   Users can now see all the parameters associated with the system in the main UI.
-   Added a numeric value for pagination.
-   Fixed some bugs.

# Release 2.9.0.90
*03/10/2024*
---
-   Added the option to display all games for the selected system.
-   Implemented pagination in the main UI.
-   Added an option to edit the Video Link and Info Link.
-   Added an option to back up `system.xml` from within the Edit System Window.
-   Added the option to use a custom System Image Folder.

# Release 2.8.2.10
*03/06/2024*
---
-   Bug fix.
-   Limit Edit System Window size.
-   Update values in `system.xml`.

# Release 2.8.1.6
*03/03/2024*
---
-   Bug fix.
-   Removed the `RefreshGameButtons` method.

# Release 2.8.0.5
*03/03/2024*
---
-   Added a new menu item called 'Edit System.'
-   Updated the method for counting files within the System Folder.
-   Optimized the current code.

# Release 2.7.0.1
*02/07/2024*
---
-   Added intelligent toggling of the GamePadController state.
-   Fixed the implementation of GamePadController in the MainWindow.
-   Added support for launching `.LNK` and `.EXE` files.
-   Added Name and Email fields to the Bug Report Window.
-   Other bug fixes.

# Release 2.6.3.4
*01/28/2024*
---
-   Bug fix.

# Release 2.6.2.3
*01/28/2024*
---
-   Adds support for loading BAT files or treating them as games in the Emulator Frontend.

# Release 2.6.0.1
*01/24/2024*
---
-   Fixed some code.
-   Improved error checking for `mame.xml`, `settings.xml`, and `system.xml`.
-   Added a BugReport window.

# Release 2.5.0.0
*01/21/2024*
---
-   Fixed code and updated libraries.
-   Added support for JPG and JPEG image formats.
-   Added custom update notification.
-   Improved logging mechanism.
-   Updated some systems in `system.xml`.

# Release 2.4
*12/30/2023*
---
-   Updated `system.xml` to include the field `SystemIsMAME`.
-   Added a `mame.xml` database.
-   Updated the UI to display the System Directory and the number of games.
-   Updated the parameters within the `system.xml`.

# Release 2.3
*12/26/2023*
---
-   Added menu items for Thumbnail Size, game visibility, and GamePad support.
-   Added a `settings.xml` file to save user preferences.
-   Refined code for 'extract before launch' functionality.
-   Updated libraries and fixed some code.

# Release 2.2
*11/25/2023*
---
-   Updated to .NET 8.0.
-   Fixed some code and updated libraries.

# Release 2.1
*11/02/2023*
---
-   Fixed some code and `system.xml`.

# Release 2.0
*10/29/2023*
---
-   Major UI improvements.
-   Revised the method for loading game files.
-   Updated the process for loading cover image files.
-   Reworked the system for selecting gaming systems and emulators.
-   Implemented a `system.xml` for system and emulator settings.
-   Introduced 'extract before launch' functionality.
-   Implemented a click sound feature.
-   Incorporated Video and Info buttons into the UI.
-   Implemented asynchronous image loading.
-   Separated functions into distinct classes.

# Release 1.3
*10/19/2023*
---
-   Added Xbox controller support.
-   Added support for CSO files.
-   Improved and fixed code.

# Release 1.2
*09/24/2023*
---
-   Fixed a lot of code.
-   Added Utilities and Show menus.
-   Improved UI.
-   Added support for CHD files.

# Release 1.1
*08/29/2023*
---
-   Initial release.
