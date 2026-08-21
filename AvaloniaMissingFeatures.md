## Table of Contents

2. [Localization](#2-localization)
3. [Gamepad Support](#3-gamepad-support)
4. [Markdown Rendering](#4-markdown-rendering)
5. [Game Launcher — Post-Exit Error Analysis](#5-game-launcher--post-exit-error-analysis)
6. [Game Launcher — Play History & Stats Integration](#6-game-launcher--play-history--stats-integration)
7. [Game Launcher — Validation & Safety Checks](#7-game-launcher--validation--safety-checks)
8. [Context Menu Localization](#8-context-menu-localization)
9. [Missing NuGet Packages](#9-missing-nuget-packages)
10. [Missing Services (extracted from MainWindow)](#10-missing-services-extracted-from-mainwindow)
11. [Other Gaps](#11-other-gaps)
12. [Summary — Priority Matrix](#12-summary--priority-matrix)

---

---

## 2. Localization

**WPF:** 18 languages, ~2,370 string keys in XAML resource dictionaries.

**Avalonia:** 18 languages present, but only **68 string keys** (~2.9% coverage).

| Missing Coverage | Examples |
|---|---|
| Menu items | Options, Edit System, Select Window, Tools, Donate, About menus |
| Dialog labels | All EditSystemWindow fields, EasyModeWindow steps, config injection labels |
| Error messages | Launch failures, file-not-found, emulator errors, validation messages |
| Emulator config | All 21 inject config window labels/descriptions |
| Settings labels | PreferencesWindow, SoundConfiguration, FuzzyMatching, GamepadDeadZone |
| Window titles | All 27+ window title strings |
| Status messages | Startup, scanning, loading, error status texts |
| Tooltip text | Button tooltips, menu item tooltips |
| Confirmation dialogs | Delete confirmations, exit confirmations, overwrite warnings |

The `LocalizationService` infrastructure is solid — keys just need to be added to all 18 JSON files.

---

## 3. Gamepad Support

**WPF:** Full SharpDX XInput/DirectInput gamepad with dead zone config, button mapping, analog stick scrolling.

**Avalonia:** UI shell exists (dead zone window, toggle checkbox) but **runtime not wired**.

| What's Missing | WPF Source | Notes |
|---|---|---|
| `GamePadController` DI registration | `App.xaml.cs` | Not registered in Avalonia's DI container |
| SharpDX packages in .csproj | `SimpleLauncher.csproj` | `SharpDX`, `SharpDX.DirectInput`, `SharpDX.XInput` not referenced |
| Gamepad navigation logic | `MainWindow.xaml.cs` | D-pad/scroll stick scrolling of game grid, button-to-action mapping |
| Pause/resume on game launch | `GameLauncherService.cs` | Pause controller input while emulator is running |
| Gamepad state in context menu | `RightClickContext` | Constructor accepts `GamePadController` but none is passed |

**Note:** SharpDX is Windows-only. Linux gamepad would require a different library (e.g., `SDL2-CS` or `OpenTK`).

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

**WPF** has extensive pre-launch and post-launch validation. **Avalonia** has basic checks only.

| What's Missing | WPF Source | Notes |
|---|---|---|
| Batch file path validation | `ValidateBatchFile.FindInvalidQuotedPathsSimple()` | Detects broken quoted paths in .bat files |
| Protocol handler registry check | `LaunchShortcutFileAsync` | Verifies .URL protocol handler exists |
| Unicode normalization | `ValidateContextAsync` → `TryFindFileWithNormalizedPath()` | Handles Unicode-normalized file paths |
| Long path support | `PathHelper.GetLongPath()` | Windows long path (>260 char) handling |
| OneDrive guidance | `ValidateContextAsync` | Detects OneDrive-managed folders, warns user |
| Ootake input validation | Pre-launch check | Prevents passing image files to Ootake |
| Geolith input validation | Pre-launch check | Prevents passing compressed files to Geolith |
| Emulator path normalization | Pre-launch check | Normalizes Unicode in emulator executable path |

---

## 8. Context Menu Localization

**WPF:** Context menu items use `{DynamicResource ...}` bindings to localized strings.

**Avalonia:** Context menu items are hardcoded English strings in `MainWindow.axaml.cs`.

| Hardcoded String | Should Be Localized |
|---|---|
| `"Play"` | `ContextPlay` |
| `"Add to Favorites"` / `"Remove from Favorites"` | `ContextAddFavorite` / `ContextRemoveFavorite` |
| `"Show Details"` | `ContextShowDetails` |
| `"Achievements"` | `ContextAchievements` |
| `"Copy Path"` | `ContextCopyPath` |
| `"Copy Name"` | `ContextCopyName` |
| `"Show in Folder"` | `ContextShowInFolder` |
| `"Edit System"` | `ContextEditSystem` |

---

## 9. Missing NuGet Packages

| WPF Package | Purpose | Avalonia Status |
|---|---|---|
| `InputSimulatorCore` | Keyboard/mouse simulation | Not referenced — needed for gamepad-to-keyboard mapping |
| `MahApps.Metro` | MetroWindow, themes, controls | N/A (Avalonia uses Fluent theme) |
| `MdXaml` | Markdown rendering | Not referenced — need alternative (e.g., Markdig) |
| `Microsoft.Extensions.Caching.Memory` | In-memory caching | Not referenced — `RemoteImageLoader` uses custom cache |
| `Microsoft.Extensions.Http.Resilience` | HTTP retry policies | Not referenced — no resilience on HTTP clients |
| `SharpDX` / `SharpDX.DirectInput` / `SharpDX.XInput` | Gamepad input | Not referenced — gamepad non-functional |
| `SharpCompress` | Archive extraction (RAR, 7z) | Not referenced — may be in Core |
| `Tomlyn` | TOML parsing (Xenia config) | Not referenced — may be in Core |
| `YamlDotNet` | YAML parsing | Not referenced — may be in Core |
| `SourceGear.sqlite3` | Native SQLite binaries | Not referenced — may be in Core |
| `NAudio.*` | Audio playback | Referenced in Core, not directly in Avalonia .csproj |

---

## 10. Missing Services (extracted from MainWindow)

The WPF project extracts business logic into standalone services. The Avalonia project inlines this logic into `MainWindow.axaml.cs` and `MainViewModel.cs`. While the functionality may exist, it's not in a testable, reusable service.

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
| `GameDetailWindow` missing | Does not exist in WPF | **Avalonia has it** — this is an Avalonia addition | N/A |
| `PreferencesWindow` missing | Settings scattered across menus | **Avalonia has it** — consolidated settings dialog | N/A |
| Emergency return button on loading | Loading overlay has cancel button after timeout | Not implemented | Low |
| `MessagePack` NuGet | Used for favorites/history serialization | Referenced in Avalonia .csproj | OK |
| `AppSettings` static class | WPF-specific settings accessor | Avalonia uses `SettingsManagerService` from Core | OK |
| `ApplicationStats` static class | WPF-specific stats helper | Avalonia uses Core `Stats` class directly | Minor |
| WPF-specific converters | `ImageUrlConverter`, `BooleanToFavoriteStatusConverter` | Avalonia has equivalents (`PathToImageConverter`, `BooleanToFavoriteStatusConverter`) | OK |
| `FilterMenu` letter/number panel | `UiHelpers/FilterMenu.cs` | Built into `MainViewModel.ApplyLetterFilter()` | OK |
| Window count (WPF: 27 + 21 inject = 48) | All WPF windows | Avalonia has 28 + 21 inject = 49 (includes `GameDetailWindow` + `PreferencesWindow` + `MessageDialogWindow`) | OK |

---

## 12. Summary — Priority Matrix

### Critical (functional gaps that affect users)

| # | Feature | Effort |
|---|---|---|
| 1 | **Theme system** — Light/Adaptive/HighContrast/Midnight + 27 accent colors | Large |
| 2 | **Localization** — expand from 68 to ~2,370 keys across 18 languages | Large |
| 3 | **Gamepad support** — wire up `GamePadController` in DI, add SharpDX or alternative | Medium |

### Important (parity gaps)

| # | Feature | Effort |
|---|---|---|
| 4 | **Pre-launch validation** — batch file paths, Unicode normalization, long paths | Medium |
| 5 | **Context menu localization** — replace hardcoded English strings | Small |

### Nice-to-have (architectural improvements)

| # | Feature | Effort |
|---|---|---|
| 6 | **Extract services from MainWindow** — MenuOrchestrator, UIReset, etc. | Large |
| 7 | **HTTP resilience** — add `Microsoft.Extensions.Http.Resilience` | Small |
| 8 | **DisplaySystemInformation** — OS/hardware info window | Small |
| 9 | **SystemImageResolverService** — fuzzy-matching system image resolver | Medium |

### Already at parity (no action needed)

- Sound effects (NAudio via Core, cross-platform)
- System tray (Avalonia TrayIcon)
- Toast notifications (in-window stack)
- Loading overlays (per-window)
- Image viewer
- Flash overlay
- F8 screenshot (Windows-only)
- Favorites / Play History / Global Search (embedded sections)
- Game launch strategies (8 total, all present)
- Emulator config handlers (21 total, all present)
- Disc image converters (via Core `IDiscConverter`)
- Game scanners (11 platforms, all present)
- Markdown rendering (`Markdown.Avalonia` v11.0.3 — `MarkdownScrollViewer` in `UpdateHistoryWindow`)
- Post-exit error analysis (`AnalyzeProcessExitAsync` in `MinimalLauncherService` — all 9 handlers + emulator skip list + MAME INI auto-restore)
- Play history & stats integration (`UpdateStatsAndPlayCountAsync` — `RecordPlayAsync`, `Stats.CallApiAsync`, `GamePlayed` event, 5s threshold, `ReceiveANotificationOnEmulatorError` gate)
- Bug report sink (shared from Core)
- File watcher (Core + Avalonia wrapper)
- Game cache (`AvaloniaGameCacheService`)
- Update checker (`AvaloniaCheckForUpdatesService`)
- 18 language files (just need more keys)
