# Release 5.1.0
*2026-02-12*
---

- Improved logic to generate aliases for Microsoft Windows games that will improve game detection.
- Fix MAME emulator handler to proper inject rom paths into the emulator configuration file. From now on you do not need to provide '-rompath' parameter to run MAME, since the logic will automatic inject the rompath's of the 'System Folder' into MAME configuration file. That will prevent launching failures.
- I have created a handler for emulator Ootake, a PC Engine emulator. It will automatic extract compressed files to .PCE extension, since this emulator does not support compressed files natively. That will prevent launching failures.
- I have created logic to always extract compressed files when you are using the Game Boy emulator SameBoy, since this emulator does not support compressed files natively. That will prevent launching failures.

# Release 5.0.0
*2026-02-11*
---

## Refactored GameLauncher Architecture
- Replaced the monolithic conditional structure with a flexible, strategy-based pattern that dramatically improves extensibility for new emulators and platforms.
- Created emulator-specific configuration handlers (e.g., `AresConfigHandler`, `XeniaConfigHandler`, `RaineConfigHandler`) that manage pre-launch configuration dialogs and dynamically modify emulator settings files.
- Implemented launch strategy pattern (e.g., `ChdToCueStrategy`, `XisoMountStrategy`, `ZipMountStrategy`, `DefaultLaunchStrategy`) to handle diverse launch scenarios—including ISO mounting, archive extraction, format conversion, and direct execution—with clean, maintainable code.
- Integrated format conversion services for CHD-to-CUE/BIN, CHD-to-ISO, and RVZ-to-ISO transformations, expanding compatibility across emulators. For example, it can convert a CHD file to CUE/BIN on the fly if the emulator does not support CHD.

## Configuration Injection Service
- Built a centralized service that programmatically injects settings into emulator configuration files.
- Supports 20+ emulators: Ares, Azahar, Blastem, Cemu, Daphne, Dolphin, DuckStation, Flycast, MAME, Mednafen, Mesen, PCSX2, Raine, Redream, RetroArch, RPCS3, Sega Model 2, Stella, Supermodel, Xenia, and Yumir.
- Xenia integration mirrors Xenia Manager's internal configuration injection functionality.

## Enhanced RetroAchievements Integration
- Automatically injects RetroAchievements credentials into PCSX2, DuckStation, PPSSPP, Dolphin, Flycast, BizHawk, and RetroArch.
- Added RetroAchievements filter in `MainWindow` to quickly identify and display probable supported titles.
- Refined game hashing algorithms across multiple systems for accurate achievement tracking.

## UI Enhancements
- Unified UI busy-state handling throughout the application to provide consistent blocking behavior and progress feedback across all major windows (MainWindow, Favorites, Search, etc.).

## Code Quality Improvements
- Enhanced error handling for XML corruption.
- Fixed potential deadlocks in play history updates.
- Centralized `system.xml` load, edit, and delete operations into the `SystemManager` class.

# Release 4.9.1
*2026-01-18*
---

-   **Package Updates:** Several Microsoft-related NuGet packages (Data.Sqlite, Extensions, CodeAnalysis) were updated to their latest patch versions (e.g., from `10.0.1` to `10.0.2`).
-   **Long Path Support:** Enhanced file and directory existence checks in `GameLauncher.cs` and `PathHelper.cs` by explicitly handling the long-path prefix (`\\?\`). This ensures compatibility across different drive types (local, network, removable) and paths exceeding 260 characters.
-   **Shortcut Validation:**
    -   Refined `.URL` file launching to only validate protocol handlers (like `steam://`) if the target is a true URI. This prevents drive letters (e.g., `C:\`) from being incorrectly identified as protocols.
    -   Improved error logging for shortcuts by preventing the app from attempting to read binary content from `.LNK` files during an exception.
-   **Thread Safety:** Added lock mechanisms in `DownloadManager.cs` around the `CancellationTokenSource`. This ensures that starting, canceling, and disposing of downloads is thread-safe.
-   **Enhanced Logging**
-   **Microsoft Store Filtering:** Significantly expanded the exclusion list in `ScanMicrosoftStoreGames.cs`.
-   **Tool Updates:** Updated several external tools bundled with the launcher, including `BatchConvertToCHD`, `chdman`, `GameCoverScraper` (arm64/x64), `SimpleZipDrive`, and `BatchConvertIsoToXiso`.

# Release 4.9.0
*2026-01-13*
---

## 🎮 RetroAchievements Enhancements
- **Emulator Auto-Configuration**: Streamlined setup process with automatic configuration for retroarch
- **UI Localization**: Comprehensive localization support across all RetroAchievements interfaces
- **Messaging**: Enhanced error and success feedback for better user experience

## 🏪 Microsoft Store Game Scanning
- **User Verification Window**: New interface for confirming detected games

## 🛡️ Error Handling & Robustness
- **Win32Exception Handling**: Improved support for application control policies
- **PowerShell Restrictions**: Better handling of execution policy limitations
- **File Lock Management**: User-friendly prompts for locked file scenarios
- **Download Resilience**: Enhanced retry mechanisms for download and extraction operations

## 🌍 Localization & UI Improvements
- **Full Localization**: Extended support across multiple windows:
    - GlobalStatsWindow
    - RetroAchievementsForAGameWindow
    - FavoritesWindow
    - FilterMenu
- **Dynamic Resources**: Refactored UI text to use dynamic resource binding
- **Consistent Styling**: Updated button styles in EasyModeWindow

## ⚡ Performance & Thread Safety
- **Cancellation Tokens**: Integrated across asynchronous operations for better responsiveness
- **Thread Safety**: Added explicit locks to critical sections (SettingsManager, search results)

# Release 4.8.0
*2026-01-04*
---

# Major Overhaul: Microsoft Windows Game Scanning & Enhanced Error Handling

Introduces a major overhaul of Microsoft Windows Game scanning, significantly enhancing error handling, and refining the user interface.

## Key Changes

### 1. Game Scanning Refactoring & Expansion

The monolithic `GameScannerService` was broken down into modular, platform-specific classes:

- `ScanSteamGames`
- `ScanEpicGames`
- `ScanGogGames`
- `ScanMicrosoftStoreGames`
- `ScanAmazonGames`
- `ScanBattleNetGames`
- `ScanHumbleGames`
- `ScanItchioGames`
- `ScanRockstarGames`
- `ScanUplayGames`

This adds support for new platforms and improves existing scanning logic with:
- Better filtering
- DLC detection
- Executable identification
- Icon extraction heuristics

### 2. API-based Artwork Download

A new API integration (`GameImageClient`) was added to download game artwork for Microsoft Windows Games, with robust fallback mechanisms to local icon extraction if the API fails.

### 3. Enhanced Error Handling & Robustness

- **Long path support:** Implemented `\\?\` prefix for reliable file and directory operations across services
- **Download and extraction error handling:** Including retry logic, disk space checks, user cancellation management, and a new `ShowExtractionFailedMessageBoxAsync` for manual intervention
- **Game launching error handling:** Refined in `GameLauncher`, adding `.URL` validation, protocol checks, and a new `ShowCustomMessageBox` for detailed notifications
- **UIPI exceptions:** Gracefully handled in `GamePadController` with refined error logging

### 4. UI/UX Improvements

- Introduced a new global `RetroAchievementsWindow` for user profiles and unlocks
- **Automated first-run game scanning** for Microsoft Windows games
- Updated menu structures, replacing dynamic headers with static strings for consistency
- Added a new **SupportOptionWindow** for AI-based troubleshooting and developer contact

### 5. Binary & Dependency Updates

Updated various external tools:
- RetroGameCoverDownloader
- GameCoverScraper
- BatchConvertToCHD

### 6. Code Cleanup & Localization

- Cleaned up unused code
- Updated localization strings across all supported languages

# Release 4.7.0
*2025-12-07*
---

## 🚀 New Features
- **Group Files by Folder**: Groups multi-file MAME games (e.g., Software List CHDs/ROMs from [PleasureDome](https://pleasuredome.github.io/pleasuredome/index.html)) into single UI entries. Compatible **only with MAME**—warnings shown for non-MAME setups. Handy to launch MAME Software List CHDs.
- **RetroGameCoverDownloader**: New tool to help users get cover art for their games.
- **Automatic Creation of 'Microsoft Windows' System**: Introduced an algorithm to automatically add links to Epic Games, Steam Games, and Windows Store Games to the frontend.
- **Advanced Global Search**: Filter by system, filename, MAME description, folder name, and recursive search.
- **First-Run Welcome Flow**: Guides new users to Easy Mode for quick setup.
- **Kebab Menu**: "..." button on game entries for quick context actions.
- **Configurable Status Bar**: Timeout (default 3s) with detailed feedback for loading/saving/actions.

## 🔧 Core Refactoring
- **Dependency Injection (DI)**: `GameLauncher`, `GamePadController`, `UpdateChecker`, `ExtractionService`, `PlaySoundEffects`, etc., now instance-based services.
- **ExtractionService**: Refactored from `ExtractCompressedFile` with `IExtractionService` interface; retry logic for file locks.
- **Thread Safety**: `SemaphoreSlim` for game caches; `Dispatcher.InvokeAsync` preferred.
- **.NET 10**: Target framework upgraded; C# 14 features.

## 🎨 UI/UX Improvements
- **UpdateHistoryWindow**: Native Markdown rendering (no external deps).
- **EditSystemWindow**: `GroupBox` → `Expander` (collapsible); persist states.
- **Resizable Windows**: `SetFuzzyMatchingWindow`, `SetGamepadDeadZoneWindow`.
- **Dark Mode Toggle**: Navigation menu icon/handler.
- **Loading Overlays**: Consistent across windows; disable UI during ops.

## 🛠️ Tools & Emulators
- **Updated Binaries**: tools (`bchunk.exe`, `GameCoverScraper`, `FindRomCover.exe`, `BatchConvertIsoToXiso.exe` etc.).
- **New Emulators**: Ymir (Saturn), NooDs (DS), Gearlynx/Gearboy (Lynx).
- **Docs**: Updated `parameters.md`/`helpuser.xml` with Amiga models, MAME `rompath` examples.

## 🛡️ Robustness & Perf
- **Cancellation Tokens**: `GlobalSearchWindow`, `PlayHistoryWindow`.
- **Atomic Saves**: Favorites prevent corruption via temp files.
- **Cache Invalidation**: Clear RA cache on credential changes.
- **Localization**: 100+ new strings (artwork, Easy Mode, errors).

# Release 4.6.0
*2025-10-30*
---

-   Introduced tool `GameCoverScraper` to allow users to scrape cover images online.
-   Updated tool `BatchConvertToRVZ` with bug fixes.
-   Updated tool `RomValidator` to allow users to generate No-Intro compatible dat files.
-   Updated emulator configurations for multiple systems.
-   Added options to increase the number of games per page, as requested by users.
-   Added new aspect ratios for button generation.
-   Added caching for the "Feeling Lucky" feature for improved performance.
-   Enhanced `SupportWindow` with detailed error report generation and improved support request handling.
-   Replaced `HelpUserTextBlock` with `RichTextBox` for enhanced formatting and interaction.
-   Enhanced RetroAchievements integration with improved error handling for unauthorized API responses.
-   Updated MAME emulator setup and streamlined argument handling.
-   Updated documentation for multiple systems.
-   Refactored the parameters and paths validation logic.
-   Added multi-image pack download support.
-   Bug fixes.

# Release 4.5.0
*2025-10-11*
---

🏆 **Introduced RetroAchievements Integration**
*   Full support for viewing RetroAchievements profiles, game achievements, latest masters, and user rankings within a new dedicated window.
*   Advanced hashing logic for accurate game matching.
*   System-aware matching for RetroAchievements console IDs and aliases.

✨ **UI/UX Enhancements**
*   New overlay buttons on game items for quick access to RetroAchievements, video links, and info links (user-configurable).
*   Introduced MAME sort order toggle (by filename or machine description).

🎮 **Emulator & System Support**
*   Added MAME a7800 emulator configuration and documentation.
*   Updated `easymode.xml` and `easymode_arm64.xml` with the latest emulator versions and new system configurations.

🏗️ **Infrastructure**
*   Updated `RomValidator` and `SimpleZipDrive` executables for x86 and ARM64 platforms.

🔧 **Codebase Refinements & Bug Fixes**

# Release 4.4.2
*2025-09-29*
---

-   Improvements to error handling, parameter validation, and user experience.
-   Updated `SimpleZipDrive` to the latest version.
-   Updated emulator references and download links to the latest version.
-   Improved parameter validation to reduce false positives on non-path arguments.
-   Fixed double-click handling in game list mode to prevent errors on placeholder items.

# Release 4.4.1
*2025-09-19*
---

### 🔧 Bug Fixes
-   Improvements to error handling, parameter validation, and user experience across the SimpleLauncher application.
-   Fix the `Supermodel` emulator link.

# Release 4.4
*2025-09-17*
---

### 🖥️ Platform Support
-   Introduced support for Windows-arm64

### 🎨 User Interface
-   Improved UI responsiveness in `PlayHistoryWindow` and `FavoritesWindow`.
-   Replaced `PleaseWaitWindow` with a consistent, embedded `LoadingOverlay`.
-   Added "Delete Cover Image" option in the right-click context menu.

### 🔧 Technical Improvements
-   Improved error handling and user notifications.
-   Updated existing emulator versions.
-   Enabled MAME description support in `FindRomCover` tool.
-   Upgraded NuGet packages `MahApps.Metro` and `Microsoft.Extensions`.
-   Transitioned `SettingsManager` and `FavoritesManager` to Dependency Injection (DI).
-   Improved localization resource management.

# Release 4.3
*2025-07-26*
---

### Major Feature: Multi-Folder Support
-   **Multiple System Folders:** The core logic has been refactored to allow each system to scan for games across multiple folders instead of just one.

### UI & UX Enhancements
-   **Improved Button Styles:** Game and system buttons now feature refined 3D styles with hover and press animations for a more dynamic user experience.

### Stability Improvements
-   **Improved Mounting Logic:** The on-the-fly mounting for ZIP and XISO files no longer uses fixed delays. It now uses a more robust polling mechanism that actively checks for successful mounts, improving reliability and reducing unnecessary waiting.

### Tooling
-   **New Tool Integration:** A new `RomValidator` tool has been added. This tool can compare user game files with No-Intro dat files.

### Other Enhancements
-   **Emulator Updates:** Documentation and help files have been updated with information for new emulators (`Gopher64`, `Hades`, `VisualBoy Advance M`, `Retroarch SwanStation`).
-   **Emulator Updates:** Emulators from Easy Mode have been updated to the latest versions.
-   **MAME Database Update:** The `mame.dat` file has been updated to MAME 0.278.
-   **History Database Update:** The `history.xml` file has been updated to the latest version.

# Release 4.2.0
*2025-07-10*
---
### Core Feature Enhancements: On-the-Fly File Mounting
The most significant change is the introduction of on-the-fly file mounting, which allows users to launch games directly from compressed or disk image files without needing to manually extract them first.
**Note:** You need to install the Dokan from [GitHub](https://github.com/dokan-dev/dokany) for ZIP and XISO file mounting.

*   **ISO & ZIP Mounting for RPCS3:** The launcher can now mount `.iso` and `.zip` files for the PlayStation 3 emulator (`RPCS3`). It uses PowerShell for native ISO mounting and a new `SimpleZipDrive.exe` tool for ZIP files. After mounting, it automatically finds and launches the required `EBOOT.BIN` file.
*   **XISO Mounting for Cxbx-Reloaded:** Support has been added to mount Xbox ISO (`.xiso`) files for the `Cxbx-Reloaded` emulator. This is handled by a new `MountXisoFiles` service that uses the `xbox-iso-vfs.exe` tool to create a virtual drive and launch the `default.xbe` file.
*   **XBLA ZIP Mounting:** The system can now mount `.zip` files for Xbox Live Arcade (`XBLA`) games, searching for a specific nested file structure required to launch them.
*   **ScummVM ZIP Mounting:** The system can now mount `.zip` files for `ScummVM` games and automatically launch the game.

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
*2025-06-08*
---
*   Added a Sound Configuration feature allowing customizable notification sounds.
*   Implement a Debug Logger with a dedicated UI window for enhanced logging.
*   Major overhaul and standardization of path resolution and handling, including support for placeholders `%BASEFOLDER%`, `%SYSTEMFOLDER%` and `%EMULATORFOLDER%`.
*   Refactored Parameter Validation logic for clarity and robustness, particularly concerning paths.
*   Improved Error Handling, Logging, and messaging across various components (`GameLauncher`, `Services`, `Managers`), replacing `Debug.WriteLine` with the new `DebugLogger`.
*   Added Status Bar updates to show current actions (e.g., game launched).
*   Introduced 3D button styles for navigation buttons.
*   Updated Tray Icon messaging and functionality (including Debug Window access).
*   Refactored `Edit System Window` logic and validation for better user feedback on paths.

# Release 4.0.1
*2025-05-26*
---
### Bug Fix Version
-   **Parameter Handling Overhaul (`ParameterValidator.cs`, `GameLauncher.cs`):** Paths within parameters are converted to absolute paths.
-   **File Path Standardization (`GameLauncher.cs`, `GameButtonFactory.cs`, `GameListFactory.cs`):** File paths used for launching games and creating UI elements are consistently resolved to absolute paths relative to the application directory, improving reliability.
-   **UI Enhancements (`MainWindow.xaml`):** A new custom-styled `GridSplitter` has been added to the list view in the main window, allowing for better resizing of the game list and preview areas.
-   **Improved Window Closing (`DownloadImagePackWindow.xaml.cs`, `EasyModeWindow.xaml.cs`):** Added logic to the `CloseWindowRoutine` in `DownloadImagePackWindow` and `EasyModeWindow` to stop any ongoing operations (like downloads) before the window closes, preventing potential errors.
-   **Configuration Change (`SystemManager.cs`):** The default value for `ReceiveANotificationOnEmulatorError` in system configurations has been changed from `false` to `true`, meaning users will now receive error notifications by default.

# Release 4.0
*2025-05-24*
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
*2025-04-27*
---
-   Updated the **Batch Convert To Compressed File** tool to skip files that are already compressed.
-   Improved the UI in the **Easy Mode** and **Download Image Pack** windows.
-   Added code to enforce a single instance of the application.
-   Implemented fuzzy image matching to enhance the user experience. This logic finds a cover image that partially matches the ROM filename. Users can disable this feature if desired.
-   Updated emulators to the latest version.
-   Updated `history.xml` (available at arcade-history.com) to the latest version.

# Release 3.12
*2025-03-31*
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
*2025-03-22*
---
-   Added logic to check file and folder paths in the parameter field. Users will be notified if the check returns invalid.
-   Updated the `LogError` class to use a new, more flexible API.
-   Added a bug report feature for all the tools bundled with 'Simple Launcher'.
-   Improved the `BatchConvertToCHD` tool to better handle the deletion of original files after conversion to CHD.
-   Fixed some bugs.

# Release 3.11
*2025-03-12*
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
*2025-03-03*
---
-   Fixed an issue in `List View` mode where preview images failed to load.

# Release 3.10.1
*2025-03-02*
---
-   Fixed some bugs.
-   Enhanced methods and functions to improve efficiency and reduce errors.

# Release 3.10
*2025-02-23*
---
-   Exposed the GamePad DeadZone variable to the user.
-   Improved the Game Button generation for the `Grid View` option. Now users can customize the aspect ratio of the buttons.
-   Updated the emulators to their latest releases.
-   Updated `history.xml`, available at arcade-history.com, to the latest version.
-   Fixed some bugs.
-   Enhanced methods and functions to improve efficiency and reduce errors.

# Release 3.9.1
*2025-02-08*
---
-   Significantly improved the speed of the search engine in the `Main Window` and in the `Global Search Window`.
-   Fixed some bugs.
-   Enhanced methods and functions to improve efficiency and reduce errors.

# Release 3.9
*2025-02-07*
---
-   Enhanced support for Xbox controllers and added support for PlayStation controllers. I tested it with the following controllers: `Xbox 360`, `Xbox One`, `Xbox Series X/S`, `PlayStation 4 DualShock`, and `PlayStation 5 DualSense`.
-   Improved the text formatting of developer suggestions in the `Edit Window`.
-   Enhanced the translation resources: even error messages are now translated into 17 languages.
-   Updated the emulators to their latest releases.
-   Updated `history.xml` to the latest version.
-   Updated `mame.xml` to the latest version (0.274).
-   Added a tool to batch convert regular ISO files to the `XISO` (Xbox) format.
-   Added a tool to batch convert CUE, BIN, ISO, and IMG files to the `CHD` (MAME) format.
-   Added a tool to batch compress files into ZIP or 7Z archives.
-   Fixed some bugs.
-   Improved several methods and functions to enhance efficiency and reduce errors.

# Release 3.8.1
*2025-01-16*
---
-   Bug fixes.
-   Improved some methods and functions to enhance efficiency and reduce errors.

# Release 3.8
*2025-01-15*
---
### Multilingual Version – A Major Update!
-   **Experience the Application in Your Language!** We've added translations for 17 languages, making the application accessible to users worldwide. Supported languages now include: Arabic, Bengali, German, Spanish, French, Hindi, Indonesian (Malay), Italian, Japanese, Korean, Dutch, Portuguese, Russian, Turkish, Urdu, Vietnamese, Simplified Chinese, and Traditional Chinese.
-   Updated to the latest technologies: `.NET Core 9` and `C# 13`, ensuring better performance and stability.
-   Fixed several entries in `helpuser.xml` for improved user assistance.
-   Updated all emulators to their latest releases.
-   Updated `mame.xml` to version 0.273.
-   Enhanced support for 'Kega Fusion' and 'Mastergear'.
-   Added more emulators to `system_model.xml`.

# Release 3.7.1
*2024-12-12*
---
-   Fixed bugs and improved exception handling in certain methods.
-   Added image packs for specific systems.

# Release 3.7
*2024-12-08*
---
-   Added a tool to generate `BAT files` for `Xbox 360 XBLA` games.
-   Added a `Helper class` to help users during system edits.
-   Improved the UI in the `Edit window`.
-   Added support for launching `Mattel Aquarius` games on `MAME` using the frontend.
-   Improved the code that gets the application version from GitHub.
-   Updated the emulator version of the Easy Mode emulators to the latest version.
-   Updated `mame.xml` to the latest version (MAME 0.272). I have also added Software List games to this list.
-   Updated `parameters.md` and `system_model.xml`.
-   Updated the binaries of the tool `FindRomCover` to the latest version.
-   Added image packs for `Philips CD-i` and `Atari ST`.
-   Added a new menu item to download image packs.
-   Added `Mattel Aquarius` and `Sharp x68000` to `Easy Mode`.
-   Updated the `rom history database` to the latest version (ver. 2.72) from [arcade-history.com](https://www.arcade-history.com/).
-   Fixed bugs and improved exception handling in some methods.

# Release 3.6.3
*2024-11-26*
---
-   Changed the temp folder from within the application folder to the `Windows temp folder` to prevent access issues.
-   Implemented methods to handle the cleanup of temp files and folders.

# Release 3.6.2
*2024-11-25*
---
-   Improved error handling in some methods.

# Release 3.6.1
*2024-11-24*
---
-   Fixed a bug in several methods that attempt to load a preview image or open a cover image when the `UserImagePath` is a relative path. These methods now convert all relative paths to absolute paths before execution.
-   Moved the `DownloadAndExtractInMemory` method to a separate class.
-   Improved error handling in some methods.

# Release 3.6.0
*2024-11-23*
---
-   Performed a major refactoring of the `EditSystem` class to implement better validation in the input fields of the `EditSystem` window.
-   Improved the system validation mechanism of the `system.xml` file when 'Simple Launcher' loads.
-   Implemented `in-memory download and extraction` of files as a backup method in case the regular extraction method fails.
-   Added different versions of the `7z` executable for `x86` and `x64`.
-   Added code to automatically convert URL-formatted text into real links in the `RomHistory` window.
-   Improved the `log class` to send more debug information to the developer.
-   Added functionality to `delete a game file` from within the UI, available in the right-click context menu.
-   Added functionality to `auto-generate image previews` for the loaded games.
-   Added an image pack for `Atari 8-Bit`.
-   Updated the emulator links to the latest version.
-   Bug fixes.

# Release 3.5.0
*2024-11-09*
---
-   Added a tool to generate `BAT files` for `Sega Model 3`.
-   Added a `local ROM history database` using `history.xml` from [arcade-history.com](https://www.arcade-history.com/).
-   Added a new way to view the games in the main UI. Users can now choose between `Grid View` and `List View`.
-   Added a `Star button` to the `Main Window top menu`, which selects all the favorites for the selected system.
-   Improved error and exception notifications in several methods for both users and developers.
-   Added 'Sony PlayStation 3' image pack.
-   Increased the font size for the game filenames in the `GameButtonFactory`.
-   Added `Sega Model 3` to `Easy Mode` with an `ImagePack` available for this system.
-   Updated emulator download links to the latest version.

# Release 3.4.1
*2024-11-03*
---
-   Automatically set the `SystemImageFolder` if the user does not specify it. It will be set to `.\images\SystemName`.
-   Implemented a way to retry extraction of the downloaded file in case it is locked by `antivirus software`.
-   Updated the download link for the emulators to the latest version in `EasyMode`.
-   Added 'Sony PlayStation 3' to the `EasyMode` installation window.
-   Updated `mame.xml` to the latest version.

# Release 3.4
*2024-11-03*
---
-   Added a new menu item called **Tools**. In it, you will find tools related to emulation.
-   Added a tool to `Create Batch Files for PS3 Games`.
-   Added a tool to `Create Batch Files for ScummVM Games`.
-   Added a tool to `Create Batch Files for Windows Games`.
-   Added a tool to `Organize System Images`.
-   Implemented code to automatically redirect the user to the `Simple Launcher Wiki page` if the provided parameters for emulator launch fail.
-   Implemented the automatic launch of the `UpdateHistory` window after an update so the user can see what is new in the release.
-   Implemented logic to automatically reinstall 'Simple Launcher' if the required files are missing.
-   Improved the `Updater` application. It will always automatically install the latest version of 'Simple Launcher' even if the parameters provided are invalid.
-   Fixed the `UpdateHistory` window to wrap the text. It is better for the user.

# Release 3.3.2
*2024-10-27*
---
-   Improved the field validation in the "Edit System" window to trim input values and prevent the input of empty spaces.
-   Enhanced support for `Xbox 360 XBLA` games in the `Launcher`.
-   Fixed the download of the 'ares' emulator.
-   Fixed the download of the 'Redream' emulator.
-   Fixed a bug in the `Easy Mode` window to prevent the extraction of partially downloaded files.
-   Added `Xbox 360 XBLA` into `easymode.xml` and `system_model.xml`.

# Release 3.3.1
*2024-10-24*
---
-   Fixed a minor issue in the `GameLauncher` class that was triggered when `MAME` output warning messages about the ROM. It will now only consider error messages if the emulator outputs an error code.

# Release 3.3.0
*2024-10-24*
---
-   Added `Image Packs` for `Amstrad CPC`, `Amstrad CPC GX4000`, `Arcade (MAME)`, `Commodore 64`, `Microsoft MSX1`, `Microsoft MSX2`, `Sony PlayStation 1`, `Sony PlayStation 2`, and `Sony PSP`.
-   Added a new `UpdateHistory` window that can be launched from the `About` window.
-   Added a button in the `GlobalStats` window that allows the user to save the generated report.
-   Fixed an error in the `GameLauncher` class that was generating an `InvalidOperationException`.
-   Improved the error logging in the `EditSystemEasyModeAddSystem` class.
-   Added logic to `delete the favorite` if the favorite file is not found.
-   Added new functionality to `Updater.exe`.

# Release 3.2.0
*2024-10-22*
---
-   Enabled download of `ImagePacks` for some systems.
-   Implemented a `Tray Icon` for the application.
-   Added the function to present the `System Play Time` for each System.
-   Made the filenames and file descriptions on each button in the `Main window` a little bigger.
-   Updated all emulators to the latest versions.
-   Updated the `mame.xml` file to the latest release.
-   Improved error handling in the `Easy Add System` window.
-   Enhanced error handling for missing the `default.png` file.
-   Enhanced the `Global Stats` class and `XAML`.
-   Improved error handling throughout the application.
-   Improved code in the `GameLauncher` and `ExtractCompressedFile` classes.

# Release 3.1.0
*2024-07-18*
---
-   Implemented a `file check` before launching a `favorite game`.
-   Fixed the freezing issue of the `Please Wait` window during a search in the `Main` window.
-   Capped the number of games per page at 500.
-   Included a second log window in the update process.
-   Enhanced the `global search` functionality to also search within the machine description of `MAME` files.
-   Updated the emulator download links to the latest version.
-   Fixed some random code issues.

# Release 3.0.0
*2024-07-17*
---
-   Implemented a `theme functionality` that enables users to personalize the application's appearance.
-   The user interface of the `main window` has been enhanced.
-   Added `icons` to some menu items.
-   Some images in the `Edit window` have been modified.
-   Created a `context menu` in the `main user interface` for the 'Remove From Favorites' option.
-   An `image preview functionality` has been implemented in the `Favorite` and `Global Search` windows.
-   The management of `mame.xml` and `system.xml` has been improved.

# Release 2.15.1
*2024-07-14*
---
-   Added a `star icon` to each game in the `favorites' list`.
-   Implemented a `preview window` that appears when users hover over a list item.
-   Enhanced logic for generating `favorites.xml` and `settings.xml` when missing.
-   Removed `Video` and `Info` links from the generated buttons (still accessible via right-click).

# Release 2.15
*2024-07-04*
---
-   Updated emulator versions in `EasyMode`.
-   Added `Amstrad CPC` to `EasyMode`.
-   Updated `mame.xml` to the latest release.
-   Updated `system_model.xml` to include Amstrad CPC.
-   Enhanced the `Update Class`.
-   Introduced a `Cart Context Menu`.
-   Improved the `Global Search Window` to include the `System Name` field.
-   Improved the `Global Stats Window` with a `pie chart`.
-   Fixed the `default image viewer` for various media types.
-   Implemented a `Favorite function`.
-   Added icons to the `Right-Click Context Menu` items.

# Release 2.14.4
*2024-06-06*
---
-   Improved the `Global Search algorithm` to accept logical operators `AND` and `OR`.
-   Fixed a hidden bug in the `Main Window search engine`.
-   Added an option for the user to select a `ROM Folder` in the `Easy Mode - Add System` Window.
-   Improved the algorithm for detection of `emulator parameters`.
-   Removed the `parameter validation requirement` to save a System in the `Expert Mode` Window.
-   Updated the emulator version and download links in `easymode.xml`.
-   Developed a mechanism to detect the version of `installed emulators` in the `Easy Mode - Add System` Class.

# Release 2.14.3
*2024-06-04*
---
-   Improved the `automatic update mechanism` and fixed a `pagination bug`.

# Release 2.14.2
*2024-06-02*
---
-   Fixed the `automatic update process`.

# Release 2.14.1
*2024-06-02*
---
-   Fixed a small bug in the `About` window.

# Release 2.14.0
*2024-06-02*
---
### Big Update Today!
-   Added `Easy Mode` to `Add a System`.
-   Improved error detection of `parameter values`.
-   Added a `Global Search` function.
-   Added a `Global Stats` Window.
-   Added a `right-click Context Menu` to every generated button.
-   Implemented an `auto-update feature` for the application.
-   Updated `mame.xml` to the latest version.
-   Improved `extraction method`.
-   Fixed all bugs found.

# Release 2.13.0
*2024-05-17*
---
-   Implemented an `experimental algorithm` to check the `emulator parameters`.
-   Improved the `extraction method` that extracts files to a temporary folder.
-   Made minor bug fixes.

# Release 2.12.1
*2024-03-31*
---
-   Improved the `error notification system`.
-   Fixed a bug with the `emulator location check`.
-   Implemented code to use the `MAME game description` instead of the filename in the `Video` and `Info` links.
-   Implemented code to handle missing `mame.xml` and `system_model.xml` files.

# Release 2.12.0
*2024-03-25*
---
-   Implemented basic checks for the `System Folder`, `System Image Folder`, and `Emulator Location`.
-   Made the `exception notifications` more user-friendly.
-   Devised a smart method for generating the `system.xml` file.

# Release 2.11.2.23
*2024-03-20*
---
-   Fixed a bug in the 'Show Games' option.

# Release 2.11.1.15
*2024-03-19*
---
-   Improved mechanism to handle games without `cover`.
-   Enhanced `error logging mechanism`.
-   Implemented handling for the loading of `corrupted images` into the `UI`.
-   Fixed some bugs and corrected variable names.

# Release 2.11.0.5
*2024-03-17*
---
-   Added a `Search Engine` to the application.
-   Updated `mame.xml` to the latest version.
-   Improved the launch error message.

# Release 2.10.0.10
*2024-03-12*
---
-   Fixed the `vertical scroll`.
-   The app now retains the `window size and state` between instances.
-   Users can now use a `default.png` file from the `System Image Folder`.
-   Users can now see all the `parameters` associated with the system in the `main UI`.
-   Added a `numeric value for pagination`.
-   Fixed some bugs.

# Release 2.9.0.90
*2024-03-10*
---
-   Added the option to display `all games` for the selected system.
-   Implemented `pagination` in the `main UI`.
-   Added an option to edit the `Video Link` and `Info Link`.
-   Added an option to `back up system.xml` from within the `Edit System Window`.
-   Added the option to use a `custom System Image Folder`.

# Release 2.8.2.10
*2024-03-06*
---
-   Bug fix.
-   Limit `Edit System Window` size.
-   Update values in `system.xml`.

# Release 2.8.1.6
*2024-03-03*
---
-   Bug fix.
-   Removed the `RefreshGameButtons` method.

# Release 2.8.0.5
*2024-03-03*
---
-   Added a new menu item called 'Edit System.'
-   Updated the method for counting files within the `System Folder`.
-   Optimized the current code.

# Release 2.7.0.1
*2024-02-07*
---
-   Added intelligent toggling of the `GamePadController` state.
-   Fixed the implementation of `GamePadController` in the `MainWindow`.
-   Added support for launching `.LNK` and `.EXE` files.
-   Added `Name` and `Email` fields to the `Bug Report Window`.
-   Other bug fixes.

# Release 2.6.3.4
*2024-01-28*
---
-   Bug fix.

# Release 2.6.2.3
*2024-01-28*
---
-   Adds support for loading `BAT files` or treating them as `games` in the `Emulator Frontend`.

# Release 2.6.0.1
*2024-01-24*
---
-   Fixed some code.
-   Improved error checking for `mame.xml`, `settings.xml`, and `system.xml`.
-   Added a `BugReport` window.

# Release 2.5.0.0
*2024-01-21*
---
-   Fixed code and updated libraries.
-   Added support for `JPG` and `JPEG` image formats.
-   Added `custom update notification`.
-   Improved `logging mechanism`.
-   Updated some systems in `system.xml`.

# Release 2.4
*2023-12-30*
---
-   Updated `system.xml` to include the field `SystemIsMAME`.
-   Added a `mame.xml` database.
-   Updated the `UI` to display the `System Directory` and the `number of games`.
-   Updated the `parameters` within the `system.xml`.

# Release 2.3
*2023-12-26*
---
-   Added menu items for `Thumbnail Size`, `game visibility`, and `GamePad support`.
-   Added a `settings.xml` file to save `user preferences`.
-   Refined code for 'extract before launch' functionality.
-   Updated `libraries` and fixed some code.

# Release 2.2
*2023-11-25*
---
-   Updated to `.NET 8.0`.
-   Fixed some code and updated `libraries`.

# Release 2.1
*2023-11-02*
---
-   Fixed some code and `system.xml`.

# Release 2.0
*2023-10-29*
---
-   Major `UI improvements`.
-   Revised the method for loading `game files`.
-   Updated the process for loading `cover image files`.
-   Reworked the system for selecting `gaming systems` and `emulators`.
-   Implemented a `system.xml` for `system and emulator settings`.
-   Introduced 'extract before launch' functionality.
-   Implemented a `click sound feature`.
-   Incorporated `Video` and `Info` buttons into the UI.
-   Implemented `asynchronous image loading`.
-   Separated functions into `distinct classes`.

# Release 1.3
*2023-10-19*
---
-   Added `Xbox controller support`.
-   Added support for `CSO files`.
-   Improved and fixed code.

# Release 1.2
*2023-09-24*
---
-   Fixed a lot of code.
-   Added `Utilities` and `Show` menus.
-   Improved `UI`.
-   Added support for `CHD files`.

# Release 1.1
*2023-08-29*
---
-   Initial release.
