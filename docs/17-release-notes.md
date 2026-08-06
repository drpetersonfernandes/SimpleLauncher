# 17 — Release Notes

> Condensed changelog. The canonical, fully detailed file is `SimpleLauncher\WhatsNew.md` (shown in-app via **Help → Update History**).

## 5.6.0 — 2026-08-06 (current)

- **Parameter Resolver API**: "Suggest" buttons in Edit System (AI-powered parameter suggestions with confirmation dialog), `Explanation:` prefix handling, loading overlay, URL normalization with fixed 60 s timeout.
- **Game File Watcher**: `GameFileWatcherService` auto-refreshes the game list on external ROM changes (500 ms debounce, start/stop per system).
- **7za fallback extraction** when SharpCompress fails; bundled 7za binaries across conversion tools.
- **Batch file path validation** before execution; `UseShellExecute` instead of `cmd.exe /c`.
- **OneDrive error guidance**, invalid folder-character validation, CHD support for Kega Fusion + exit-code reporting + Dokan check, **CHD mounting on ARM64** (architecture-specific CHDMounter).
- **PCSX2** permission-error detection + portable config path resolution.
- **History database migration**: `history.xml` → `history.dat` (MessagePack) with fallback.
- **Global search**: results count, AND/OR operators, better loading state.
- **Downloads**: automatic retry with exponential backoff; read-only file handling.
- **Startup robustness**: fallback logger, improved init order, timeouts for silent update check & stats, temp-folder check first.
- **F8 hotkey conflict warning**; **AI parameter suggestions** polish; **UI/UX** (cancel buttons, tooltips, loading overlays, confirmation dialogs); **encrypted RA credentials** (Windows Credential Manager/DPAPI); nested emulator settings structure.
- **Debug Window** restored (constructor + theme), `-debug` flag, `parameters.md` path fix; quote trimming.
- **GameCoverScraper removed** (use Find Rom Cover / Retro Game Cover Downloader).
- **Serilog structured logging** everywhere (`ILogger`, sinks for Debug Window, rolling file 7 days, bug reports).
- Massive **DI** + **MVVM** refactoring; nullable reference types; Meziantou analyzer; Moq added.
- Dependency updates: SharpCompress 0.50.4, Tomlyn 2.6.0, MessagePack 3.1.8, Sqlite/Extensions 10.0.10, Serilog 4.4.0, Meziantou 3.0.139, Moq 4.20.72, and more.

## 5.5.0 — 2026-05-25

Commander Genius support with an intelligent launch strategy (extract to `Documents\Commander Genius\games\`, detect nested game folder, `dir=` parameter, cleanup after exit).

## 5.4.0 — 2026-05-10

`%ROMSYSTEMFOLDER%` placeholder (system folder containing the selected ROM).

## 5.3.x — 2026-04

- **5.3.3**: improved `.bat` file support.
- **5.3.2**: auto-updater refactored into clean service architecture (`UpdateService`, `GitHubService`, `DownloadService`, …).
- **5.3.1**: fixed HTTP request timeouts (15 s) for bug reports, stats, support requests.
- **5.3.0**: **Nintendo Wii** support (official RA support).

## 5.2.0 — 2026-03-28

Virtual-drive system built around Dokan with a custom wrapper tool (SimpleXisoDrive/SimpleZipDrive).

## 5.1.0 — 2026-02-14

Store Xbox/Xbox 360 ISOs in **CHD** format and convert back to ISO on the fly at launch.

## 5.0.0 — 2026-02-11

Replaced the monolithic launcher conditionals with the **strategy-based launch pattern** (`ILaunchStrategy`) for extensibility.

## 4.9.x — 2026-01

- **4.9.1**: Microsoft NuGet package updates.
- **4.9.0**: Emulator auto-configuration on setup (RetroArch etc.).
- **4.8.0**: `ScanSteamGames` (Steam library integration).

## 4.7.0 — 2025-12-07

**Group Files by Folder** for multi-file MAME games (Software List CHDs/ROMs).

## 4.6.0 — 2025-10-30

Introduced `GameCoverScraper` (later removed in 5.6.0).

## 4.5.0 — 2025-10-11

(Error handling / parameter validation / UX improvements.)

## 4.4.x — 2025-09

- **4.4.2 / 4.4.1**: error handling, parameter validation, UX improvements.
- **4.4**: **Windows-arm64** support introduced.

## 4.3 — 2025-07-26

**Multiple System Folders**: each system can scan across multiple ROM folders.

## 4.2.0 / 4.1.0 — 2025-06/07

(Stability and incremental improvements.)

## 4.0.x — 2025-05

- **4.0.1**: parameter handling overhaul — paths in parameters converted to absolute (`ParameterValidator`, `GameLauncher`).
- **4.0**: new visual **System Selection Screen**.

## 3.13 — 2025-04-27

Batch Convert To Compressed File skips already-compressed files.

## 3.12 — 2025-03-31

Per-emulator option to turn off error notifications.

## 3.11.x — 2025-03

- **3.11.1**: file/folder path checks in the parameter field.
- **3.11**: **Play History Window** (games played + duration).

## 3.10.x — 2025-02/03

- **3.10.2**: fixed List View preview images.
- **3.10**: gamepad dead-zone exposed to the user.

## 3.9.x — 2025-02

- **3.9.1**: much faster search engine (main window + global search).
- **3.9**: enhanced Xbox + **PlayStation controller** support.

## 3.8.x — 2025-01

- **3.8.1**: bug fixes.
- **3.8**: **17-language translations** introduced.

## 3.7.x — 2024-12

- **3.7.1**: bug fixes/exception handling.
- **3.7**: BAT file generator for Xbox 360 XBLA games.

## 3.6.x — 2024-11

- **3.6.3**: temp folder moved to the Windows temp folder.
- **3.6.1**: relative `UserImagePath` preview fix.
- **3.6.0**: major `EditSystem` validation refactor.

## 3.5 — 2024-11-09

BAT file generator for Sega Model 3.

## 3.4.x — 2024-11-03

- **3.4.1**: automatic `SystemImageFolder` default (`.\images\SystemName`).
- **3.4**: new **Tools** menu.

## 3.3.x — 2024-10

- **3.3.2**: Edit System input trimming/validation.
- **3.3.1**: MAME warning-only output handling fix.
- **3.3.0**: **Image Packs** for several systems.

## 3.2 — 2024-10-22

Image-pack downloads enabled.

## 3.1 — 2024-07-18

File check before launching a favorite game.

## 3.0 — 2024-07-17

**Themes** introduced.

## 2.x — 2023-2024

- **2.15.1**: favorite star icons · **2.15**: EasyMode emulator updates · **2.14.4**: Global Search `AND`/`OR` · **2.14.3-1**: update-mechanism fixes · **2.14.0**: **Easy Mode** · **2.13.0**: experimental parameter checker · **2.12.1**: error notifications · **2.12.0**: system/image/emulator path checks · **2.11.x**: search engine, cover handling · **2.10.0.10**: vertical scroll fix · **2.9.0.90**: show-all-games option · **2.8.0.5**: **Edit System** menu · **2.7.0.1**: gamepad auto-toggle · **2.6.2.3**: `.bat` files as games · **2.4**: `mame.xml` database · **2.3**: thumbnail/gamepad menu items · **2.2**: **.NET 8.0** upgrade · **2.1**: system.xml fixes · **2.0**: major UI overhaul.

## 1.x — 2023

- **1.3**: Xbox controller support · **1.2**: fixes + CHD support · **1.1** (2023-08-29): initial release.

## Related docs

- [15 — Development](15-development.md) (version bumping)
- [16 — Updater](16-updater.md)
- `SimpleLauncher\WhatsNew.md` (canonical changelog)
