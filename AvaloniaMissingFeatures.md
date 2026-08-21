## Table of Contents

2. [Localization](#2-localization)
3. [Gamepad Support](#3-gamepad-support)
4. [Markdown Rendering](#4-markdown-rendering)
5. [Game Launcher — Post-Exit Error Analysis](#5-game-launcher--post-exit-error-analysis)
6. [Game Launcher — Play History & Stats Integration](#6-game-launcher--play-history--stats-integration)
7. [Game Launcher — Validation & Safety Checks](#7-game-launcher--validation--safety-checks)
8. [Context Menu Localization](#8-context-menu-localization)
9. [NuGet Package Comparison](#9-nuget-package-comparison)
10. [Missing Services (extracted from MainWindow)](#10-missing-services-extracted-from-mainwindow)
11. [Other Gaps](#11-other-gaps)
12. [Summary — Priority Matrix](#12-summary--priority-matrix)

---

## 2. Localization

**WPF:** 18 languages, ~2,370 string keys in XAML resource dictionaries (`resources\strings.*.xaml`).

**Avalonia:** 18 languages present (`Resources\strings.*.json`), but only **68 string keys** (~2.9% coverage).

| Missing Coverage | Examples |
|---|---|
| Menu items | Options, Edit System, Select Window, Tools, Donate, About menus |
| Dialog labels | All EditSystemWindow fields, EasyModeWindow steps, config injection labels |
| Error messages | Launch failures, file-not-found, emulator errors, validation messages |
| Emulator config | All 21 inject config window labels/descriptions |
| Settings labels | PreferencesWindow, SoundConfiguration, FuzzyMatching, GamepadDeadZone |
| Window titles | All window title strings |
| Status messages | Startup, scanning, loading, error status texts |
| Tooltip text | Button tooltips, menu item tooltips |
| Confirmation dialogs | Delete confirmations, exit confirmations, overwrite warnings |

The `LocalizationService` infrastructure is solid — keys just need to be added to all 18 JSON files.

---

## 3. Gamepad Support

**WPF:** Full SharpDX XInput/DirectInput gamepad with dead zone config, button mapping, analog stick scrolling.

**Avalonia:** **Functional on Windows.** `GamePadController` lives in Core and is shared by both UIs:

| What Was Missing | Status |
|---|---|
| `GamePadController` DI registration | Done — `App.axaml.cs` registers it as a singleton |
| SharpDX packages in .csproj | Done — flows transitively from `SimpleLauncher.Core` (SharpDX, SharpDX.DirectInput, SharpDX.XInput) |
| Start/stop lifecycle | Done — started/stopped with MainWindow, initialized by `AvaloniaStartupInitializationService` (dead zones applied from settings), toggle in PreferencesWindow |
| Pause/resume on game launch | Done — `MinimalLauncherService` stops the controller before launch and resumes after exit |
| Dead zone configuration | Done — `SetGamepadDeadZoneWindow` + `SetGamepadDeadZoneViewModel`, persisted via `SettingsManagerService` |
| Gamepad state in context menu | Done — `RightClickContext` accepts `GamePadController` |

**Remaining gap:** SharpDX/XInput/DirectInput are Windows-only. On the `net10.0` Linux TFM the controller compiles but cannot function — Linux gamepad support would require a different library (e.g., `SDL2-CS` or `OpenTK`) behind a platform abstraction.

---

## 4. Markdown Rendering

**WPF:** MdXaml renders `WhatsNew.md` with headings, bold, lists, links.

**Avalonia:** Implemented via `Markdown.Avalonia` v11.0.3 (MIT-licensed, Markdig-based). `UpdateHistoryWindow.axaml` uses `MarkdownScrollViewer` with `Markdown` binding. SupportWindow is a contact form — no markdown needed.

~~| What's Missing | WPF Source | Notes |~~
~~|---|---|---|~~
~~| Markdown rendering library | `MdXaml` NuGet | Could use `Markdig` + custom Avalonia renderer |~~
~~| Rendered UpdateHistoryWindow | `UpdateHistoryWindow.xaml` | Currently shows raw markdown text |~~
~~| Rendered SupportWindow content | `SupportWindow.xaml` | May have markdown content too |~~

---

## 5. Game Launcher — Post-Exit Error Analysis

**WPF `GameLauncherService.cs`** has 9 post-exit diagnostic sub-handlers. **Avalonia `MinimalLauncherService.cs`** now has equivalent handlers via `AnalyzeProcessExitAsync()`.

Implemented in `MinimalLauncherService.cs`:

| Handler | Exit Code / Condition | Status |
|---|---|---|
| Memory access violation | `-1073741819` | Done — logs only |
| DEP violation | `-1073740791` | Done — logs only |
| RetroArch mkdir permission | stderr contains `mkdir(` + `Permission denied` | Done — shows dialog + AI fix offer |
| RetroArch parameter issues | any non-zero RetroArch exit | Done — shows dialog + AI fix offer |
| MAME "Not Found" | stdout/stderr contains `Not Found`, `WRONG LENGTH`, `Required files are missing` | Done — shows dialog + PleasureDome link |
| MAME "Unknown system" | stdout/stderr contains `Unknown system`, `approximately matches the following` | Done — shows dialog + PleasureDome link |
| MAME "Unable to load image" | stdout/stderr contains `Unable to load image`, `No such file or directory` | Done — shows dialog + PleasureDome link |
| MAME corrupted INI auto-restore | stderr contains `Warning: unknown option in INI` | Done — calls `MameConfigurationService.RestoreMameIniFromSample` |
| Generic error handler | any other non-zero exit code | Done — shows dialog + AI fix offer |

Also implemented: emulator skip list (`IsInEmulatorsToSkipList`), stdout capture alongside stderr, and the 7 previously-stubbed `IMessageBoxLibraryService` methods in `MessageBoxLibraryService.cs`.

---

## 6. Game Launcher — Play History & Stats Integration

**WPF** updates play history, statistics, and fires events after game exit. **Avalonia** now does the same.

Implemented in `MinimalLauncherService.cs` via `UpdateStatsAndPlayCountAsync()`:

| Feature | Status |
|---|---|
| `PlayHistoryManager.RecordPlayAsync()` | Done — records play with timestamp + play time seconds |
| `Stats.CallApiAsync(emulatorName)` | Done — fire-and-forget after play |
| `GamePlayed` event | Done — `event EventHandler<GamePlayedEventArgs>? GamePlayed` |
| 5-second minimum play threshold | Done — `LastPlayTime.TotalSeconds > 5` gate |
| Per-system play time in settings | Done — `_settings.UpdateSystemPlayTime()` + `SaveAsync()` |
| `ReceiveANotificationOnEmulatorError` gate | Done — error dialogs only shown when flag is true |

---

## 7. Game Launcher — Validation & Safety Checks

**WPF** has extensive pre-launch and post-launch validation. **Avalonia `MinimalLauncherService.cs`** now implements all of them.

| Check | WPF Source | Avalonia Status |
|---|---|---|
| Batch file path validation | `ValidateBatchFile.FindInvalidQuotedPathsSimple()` | Done — `RunBatchFileAsync` detects broken quoted paths, offers continue/abort via `BatchFilePathsMissingMessageBoxAsync` |
| Protocol handler registry check | `LaunchShortcutFileAsync` | Done — `.URL` targets verified against HKEY_CLASSES_ROOT (`IsProtocolRegistered`, Windows-only guard); missing handler shows `ProtocolHandlerNotRegisteredMessageBoxAsync`. Invalid .url files are rejected instead of silently launched |
| Unicode normalization | `ValidateContextAsync` → `TryFindFileWithNormalizedPath()` | Done — game path retried across NFC/NFD/KC/KD forms; found path replaces `ResolvedFilePath` |
| Long path support | `PathHelper.GetLongPath()` | Done — existence checks run in both standard and `\\?\` long-path formats |
| OneDrive guidance | `ValidateContextAsync` | Done — distinguishes unsynced file vs. inaccessible parent folder; also applied to emulator executable resolution |
| Ootake input validation | Pre-launch check | Done — blocks .chd/.bin/.cue/.iso on the post-extraction launch path (`OotakeDoesNotSupportImageFilesMessageBoxAsync`) |
| Geolith input validation | Pre-launch check | Done — blocks .zip/.7z/.rar when parameters reference `geolith_libretro` (`GeolithDoesNotSupportCompressedFilesMessageBoxAsync`) |
| Emulator path normalization | Pre-launch check | Done — emulator executable resolution retries via long path + Unicode normalization before failing, with OneDrive guidance |

Also ported: the WPF path-format mismatch diagnostic (standard vs. long path existence disagreement is logged for developer investigation without blocking the launch).

---

## 8. Context Menu Localization

**WPF:** Context menu items use `{DynamicResource ...}` bindings to localized strings.

**Avalonia:** Context menu items are hardcoded English strings (with emoji prefixes) built programmatically in `MainWindow.axaml.cs` → `ShowGameContextMenu()`.

| Hardcoded String | Should Be Localized |
|---|---|
| `"▶ Play"` | `ContextPlay` |
| `"♡ Add to Favorites"` / `"♥ Remove from Favorites"` | `ContextAddFavorite` / `ContextRemoveFavorite` |
| `"ℹ Show Details"` | `ContextShowDetails` |
| `"🏆 Achievements"` | `ContextAchievements` |
| `"📋 Copy Path"` | `ContextCopyPath` |
| `"📝 Copy Name"` | `ContextCopyName` |
| `"📂 Show in Folder"` | `ContextShowInFolder` |
| `"✏ Edit System"` | `ContextEditSystem` |

---

## 9. NuGet Package Comparison

Most WPF packages are no longer missing from Avalonia — they flow **transitively via the `SimpleLauncher.Core` project reference**, which now references them directly.

### Provided transitively by SimpleLauncher.Core (no action needed)

| WPF Package | Purpose | Avalonia Access |
|---|---|---|
| `MessagePack` | Favorites/history serialization | Via Core |
| `Microsoft.Data.Sqlite` + `SourceGear.sqlite3` | SQLite | Via Core |
| `Microsoft.Extensions.Caching.Memory` | In-memory caching | Via Core |
| `Microsoft.Extensions.Http.Resilience` | HTTP retry policies | Via Core — now wired into `DownloadClient` (see below) |
| `NAudio.*` (Core/Wasapi/WinMM/Alsa/SoundFile) | Audio playback | Via Core (cross-platform: WASAPI/WinMM on Windows, ALSA/libsndfile on Linux) |
| `SharpCompress` | Archive extraction (RAR, 7z) | Via Core |
| `Tomlyn` | TOML parsing (Xenia config) | Via Core |
| `YamlDotNet` | YAML parsing (RPCS3 config) | Via Core |
| `InputSimulatorCore` | Keyboard/mouse simulation (gamepad) | Via Core |
| `SharpDX` / `SharpDX.DirectInput` / `SharpDX.XInput` | Gamepad input | Via Core (Windows-only at runtime) |

### Avalonia-specific packages (replacing WPF-only equivalents)

| Avalonia Package | Replaces / Purpose |
|---|---|
| `Avalonia` / `Avalonia.Desktop` / `Avalonia.Themes.Fluent` 12.1.1 | UI framework (replaces WPF + `MahApps.Metro`) |
| `Avalonia.Controls.DataGrid` 12.1.2 | DataGrid control |
| `Markdown.Avalonia` 11.0.3 | Replaces `MdXaml` |
| `System.Drawing.Common` 10.0.11 (net10.0-windows TFM only) | F8 screenshot capture (Windows-only) |

### Intentionally absent (platform equivalents)

| WPF Package | Reason |
|---|---|
| `MahApps.Metro` | N/A — Avalonia uses the Fluent theme |
| `Hardcodet.NotifyIcon.Wpf` | N/A — Avalonia has built-in `TrayIcon` (`AvaloniaTrayIconManager`) |

### HTTP resilience (wired)

All named HttpClients in Avalonia's `App.axaml.cs` now mirror the WPF wiring:

- Every client gets a `SocketsHttpHandler` primary handler with explicit TLS 1.2/1.3, 5-minute pooled connection lifetime, and a 20-second connect timeout (`CreateHttpHandler`).
- `DownloadClient` additionally gets `.SetHandlerLifetime(5 min)` + `.AddStandardResilienceHandler()` (5 retries, 2 s delay, exponential backoff, jitter) — same options as WPF. Retries are limited to this client because download GETs are idempotent; API POSTs (bug reports, stats) must not be replayed.

---

## 10. Missing Services (extracted from MainWindow)

The WPF project extracts business logic into standalone services (all under `SimpleLauncher\Services\`). The Avalonia project inlines this logic into `MainWindow.axaml.cs` and `MainViewModel.cs`. While the functionality may exist, it's not in a testable, reusable service.

| WPF Service | Purpose | Avalonia Status |
|---|---|---|
| `DisplaySystemInformation` | OS/hardware info display | **Missing entirely** |
| `MenuActionHandlerService` | Menu action delegation | Inlined in MainWindow |
| `MenuOrchestratorService` | Central menu orchestrator | Inlined in MainWindow |
| `SystemImageResolverService` | Fuzzy-matching system image resolver | **Missing entirely** |
| `SystemSelectionOrchestratorService` | System selection coordination | **Missing entirely** |
| `UiOrchestratorService` | UI state coordination | Distributed across MainWindow/MainViewModel |
| `UIResetService` | Reset UI to initial state | **Missing entirely** |
| `ContextMenuService` | Context menu construction | Inlined in `ShowGameContextMenu()` |
| `GameFilterService` | Filter by cover/status | Logic in `MainViewModel` |
| `GameListUIService` | Grid/list view management | Logic in MainWindow |
| `LoadingOverlayService` | Centralized loading overlay | Per-window inline implementation |
| `SearchOrchestratorService` | Search coordination | `GlobalSearchSectionViewModel` (ViewModel, not service) |
| `UpdateStatusBarService` | Status bar management | Part of `AvaloniaStartupInitializationService` |

---

## 11. Other Gaps

| Gap | WPF | Avalonia | Impact |
|---|---|---|---|
| Emergency return button on loading | Loading overlay has cancel button after timeout | Not implemented | Low |
| `ApplicationStats` static class | WPF-specific stats helper | Avalonia uses Core `Stats` class directly | Minor |
| WPF-specific converters | `ImageUrlConverter`, `BooleanToFavoriteStatusConverter` | Avalonia has equivalents (`PathToImageConverter`, `BooleanToFavoriteStatusConverter`) | OK |
| `FilterMenu` letter/number panel | `UiHelpers/FilterMenu.cs` | Built into `MainViewModel.ApplyLetterFilter()` | OK |
| Updater | Standalone `Updater.exe` shipped next to the app | `SimpleLauncher.Avalonia.Updater` project referenced and copied to output by build target | OK |
| Window count (WPF: 24 + 21 inject = 45) | All WPF windows | Avalonia has 26 + 21 inject = 47 (adds `GameDetailWindow` + `PreferencesWindow` + `MessageDialogWindow`; WPF's `ToastNotificationWindow` replaced by in-window toast stack) | OK |

---

## 12. Summary — Priority Matrix

### Critical (functional gaps that affect users)

| # | Feature | Effort |
|---|---|---|
| 1 | **Theme system** — Light/Adaptive/HighContrast/Midnight + 27 accent colors | Large |
| 2 | **Localization** — expand from 68 to ~2,370 keys across 18 languages | Large |

### Important (parity gaps)

| # | Feature | Effort |
|---|---|---|
| 3 | **Context menu localization** — replace hardcoded English strings | Small |

### Nice-to-have (architectural improvements)

| # | Feature | Effort |
|---|---|---|
| 4 | **Extract services from MainWindow** — MenuOrchestrator, UIReset, etc. | Large |
| 5 | **DisplaySystemInformation** — OS/hardware info window | Small |
| 6 | **SystemImageResolverService** — fuzzy-matching system image resolver | Medium |
| 7 | **Linux gamepad backend** — SDL2-CS/OpenTK alternative to SharpDX for the net10.0 TFM | Medium |

### Already at parity (no action needed)

- Sound effects (NAudio via Core, cross-platform)
- System tray (Avalonia TrayIcon)
- Toast notifications (in-window stack)
- Loading overlays (per-window)
- Image viewer
- Flash overlay
- F8 screenshot (Windows-only, `System.Drawing.Common` on the net10.0-windows TFM)
- Favorites / Play History / Global Search (embedded sections)
- Game launch strategies (8 total, all present)
- Emulator config handlers (21 total, all present)
- Disc image converters (via Core `IDiscConverter`)
- Game scanners (11 platforms, all present)
- Markdown rendering (`Markdown.Avalonia` v11.0.3 — `MarkdownScrollViewer` in `UpdateHistoryWindow`)
- Post-exit error analysis (`AnalyzeProcessExitAsync` in `MinimalLauncherService` — all 9 handlers + emulator skip list + MAME INI auto-restore)
- Play history & stats integration (`UpdateStatsAndPlayCountAsync` — `RecordPlayAsync`, `Stats.CallApiAsync`, `GamePlayed` event, 5s threshold, `ReceiveANotificationOnEmulatorError` gate)
- Pre-launch validation & safety checks (batch file paths, protocol handler registry, Unicode normalization, long paths, OneDrive guidance, Ootake/Geolith input gates, emulator path normalization)
- HTTP resilience (SocketsHttpHandler primary handler on all named clients; standard resilience pipeline with retry/backoff/jitter on `DownloadClient` — mirrors WPF)
- Gamepad support on Windows (DI registration, lifecycle, pause-on-launch, dead zone config — SharpDX via Core)
- Bug report sink (shared from Core)
- File watcher (Core + Avalonia wrapper)
- Game cache (`AvaloniaGameCacheService`)
- Update checker (`AvaloniaCheckForUpdatesService`) + self-updater (`SimpleLauncher.Avalonia.Updater` shipped next to the app)
- 18 language files (just need more keys)
