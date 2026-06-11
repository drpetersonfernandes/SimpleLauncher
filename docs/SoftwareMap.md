# Simple Launcher — Software Architecture Map

## Solution Overview

The solution (`SimpleLauncher.sln`) contains **8 projects**:

| Project | Type | Description |
|---------|------|-------------|
| **SimpleLauncher** | WPF Application (.NET 10, C# 14) | Main launcher application |
| **Updater** | WPF Application | Standalone auto-updater |
| **SimpleLauncher.Tests** | Unit Tests | ~80 test files covering models, services, managers |
| **SimpleLauncher.ResourceTranslator** | Console Tool | AI-powered XAML resource translation via Gemini |
| **RetroAchievements.DataFetcher** | Console Tool | RetroAchievements data fetching |
| **Mame.DatCreator** | Console Tool | MAME DAT file generation |
| **XmlToBinaryConverter** | Console Tool | XML-to-binary conversion |
| **SimpleLauncher.Core** | Class Library | Placeholder (interface definitions) |

---

## Architecture Pattern

**Layered Service-Oriented MVVM** with **Microsoft Dependency Injection**.

- **44 ViewModels** — one per window/dialog, registered as Transient
- **70 Service directories** — interface/implementation pairs, mostly Singleton
- **28 Interfaces** — platform abstraction layer for future cross-platform porting
- **Partial Class Pattern** — `MainWindow` split across 13 files to manage complexity

---

## Project Structure

```
SimpleLauncher/
├── App.xaml.cs                    # Entry point, DI container, global exception handling
├── MainWindow.xaml.cs             # Main hub (13 partial class files)
├── Interfaces/                    # 28 platform-abstraction interfaces
├── Models/                        # 14 data transfer objects
├── ViewModels/                    # 44 ViewModel classes
├── Services/                      # 70 service subdirectories (core business logic)
├── WpfServices/                   # 8 WPF-specific interface implementations
├── Pages/                         # 3 WPF Pages (Favorites, GlobalSearch, PlayHistory)
├── resources/                     # 18 localized string XAML resource dictionaries
├── resources2/                    # Custom themes (Midnight, HighContrast)
├── audio/                         # Sound effect MP3 files
├── images/                        # UI images and 100+ system logo PNGs
├── samples/                       # Sample emulator config files (22 emulators)
├── tools/                         # 18 bundled external tool directories
└── Properties/                    # manifest, resources, settings
```

---

## Layer Breakdown

### 1. Entry Layer — `App.xaml.cs`

- Configures the DI container (~400 lines of service registration)
- Enforces single-instance execution
- Initializes theme and language
- Registers named `HttpClient` instances for different API endpoints
- Exposes `IServiceProvider` as `App.ServiceProvider`

### 2. UI Layer — Windows, Pages, ViewModels

**37 Windows**, each following the Window + ViewModel pattern:

| Category | Windows |
|----------|---------|
| Core | `MainWindow`, `AboutWindow`, `DebugWindow`, `SupportWindow`, `EasyModeWindow`, `EditSystemWindow` |
| Game/ROM | `DosBoxFileSelectionWindow`, `DownloadImagePackWindow`, `FlashOverlayWindow`, `GlobalStatsWindow`, `ImageViewerWindow`, `RetroAchievementsWindow`, `RetroAchievementsForAGameWindow`, `RetroAchievementsSettingsWindow`, `RomHistoryWindow`, `SystemSelectionWindow`, `WindowSelectionDialogWindow` |
| Settings | `SetFuzzyMatchingWindow`, `SetGamepadDeadZoneWindow`, `SetLinksWindow`, `SoundConfigurationWindow` |
| Update | `UpdateHistoryWindow`, `UpdateLogWindow` |
| Emulator Config Injection (21) | `InjectAresConfigWindow`, `InjectAzaharConfigWindow`, `InjectBlastemConfigWindow`, `InjectCemuConfigWindow`, `InjectDaphneConfigWindow`, `InjectDolphinConfigWindow`, `InjectDuckStationConfigWindow`, `InjectFlycastConfigWindow`, `InjectMameConfigWindow`, `InjectMednafenConfigWindow`, `InjectMesenConfigWindow`, `InjectPCSX2ConfigWindow`, `InjectRaineConfigWindow`, `InjectRedreamConfigWindow`, `InjectRetroArchConfigWindow`, `InjectRPCS3ConfigWindow`, `InjectSegaModel2ConfigWindow`, `InjectStellaConfigWindow`, `InjectSupermodelConfigWindow`, `InjectXeniaConfigWindow`, `InjectYumirConfigWindow` |

**3 Pages** embedded in `MainWindow`:
- `FavoritesPage` — Favorites list view
- `GlobalSearchPage` — Cross-system search
- `PlayHistoryPage` — Play history timeline

**MainWindow Partial Classes** (13 files):

| File | Responsibility |
|------|----------------|
| `MainWindow.xaml.cs` | Core constructor, initialization, event wiring |
| `MainWindow.CloseWindowEvents.cs` | Window close/exit logic |
| `MainWindow.GameFileLoadingHost.cs` | Game file loading orchestration |
| `MainWindow.GameItemRenderHost.cs` | Game item rendering |
| `MainWindow.HostImplementations.cs` | Host interface implementations |
| `MainWindow.LaunchTools.cs` | Game launch orchestration |
| `MainWindow.MenuActionHost.cs` | Menu action handling |
| `MainWindow.MenuCheckMarkHost.cs` | Menu check mark state |
| `MainWindow.MenuItems.cs` | Menu item definitions |
| `MainWindow.Pagination.cs` | Pagination logic |
| `MainWindow.Search.cs` | Search functionality |
| `MainWindow.SystemSelectionHost.cs` | System selection UI |
| `MainWindow.UIResetHost.cs` | UI reset logic |

### 3. Orchestration Layer

Services that coordinate complex multi-step UI operations:

- `UiOrchestrator` — Main UI state management
- `GameBrowser` — Central game browsing service
- `MenuOrchestrator` — Menu coordination
- `SearchOrchestrator` — Search with validation
- `SystemSelectionOrchestrator` — System selection screen
- `GameFileLoadingOrchestrator` — Game file loading into UI

### 4. Business Logic Layer — Services (70 directories)

#### Core Application
| Service | Purpose |
|---------|---------|
| `ApplicationLifecycle/` | Startup/shutdown, update checking, usage reporting |
| `StartupInitialization/` | First-run setup, system scanning |
| `SettingsManager/` | Persistent settings + 21 emulator-specific settings classes |
| `SystemManager/` | System configuration management |
| `SystemConfiguration/` | Read/write system configuration to XML |
| `AppDataFile/` | Application data file location management |

#### Game Discovery & Management
| Service | Purpose |
|---------|---------|
| `GameBrowser/` | Central game browsing |
| `GameScan/` | 11 store scanners (Steam, Epic, GOG, Microsoft Store, Amazon, Battle.net, EA, Humble, itch.io, Rockstar, Uplay) |
| `GameFileLoadingOrchestrator/` | Loading game files into UI |
| `GameFileWatcher/` | File system watcher for external changes |
| `GameFilter/` | Game filtering |
| `GameCache/` | Game file caching |
| `GlobalSearch/` | Cross-system search models |
| `SearchOrchestrator/` | Search orchestration with validation |
| `GetListOfFiles/` | File enumeration |
| `FindCoverImage/` | Cover art matching (with fuzzy matching) |
| `Pagination/` | Pagination for large collections |

#### Game Launching
| Service | Purpose |
|---------|---------|
| `GameLauncher/` | Central launch orchestrator |
| `GameLauncher/Handlers/` | 21 emulator-specific config handlers |
| `GameLauncher/Strategies/` | 8 launch strategies (CHD mount, ZIP mount, XISO mount, PBP-to-CUE, CHD-to-CUE, DOSBox, Commander Genius, Default) |
| `GameLauncher/MountFiles/` | 18 files for Dokan-based file mounting |
| `InjectEmulatorConfig/` | 23 emulator configuration injection services |
| `LaunchTools/` | Game launch helper tools |
| `Converters/` | Format converters (CHD, PBP, DiscImage) |

#### Emulator-Specific
| Service | Purpose |
|---------|---------|
| `EasyMode/` | One-click emulator download/configuration |
| `MameManager/` | MAME machine data management |
| `MameData/` | MAME DAT file service |
| `RetroAchievements/` | Full RA integration (service, manager, file hasher, system matcher, emulator configurator, encryption) |

#### UI Services
| Service | Purpose |
|---------|---------|
| `UiOrchestrator/` | Main UI state orchestration |
| `UiHelpers/` | Filter menu and other UI helpers |
| `GameItemFactory/` | Game button/list item creation (`GameButtonFactory`, `GameListFactory`, `GameButtonViewModel`) |
| `GameItemRender/` | Game item rendering |
| `GameListUI/` | Game list UI |
| `ContextMenu/` | Right-click context menu creation |
| `LoadingOverlay/` | Loading overlay management |
| `LoadingInterface/` | Loading state interface |
| `UIReset/` | UI reset service |
| `UpdateStatusBar/` | Status bar updates |
| `TrayIcon/` | System tray icon |
| `ThemeMenu/` | Theme selection |
| `LanguageMenu/` | Language selection |
| `MenuOrchestrator/` | Menu coordination |
| `MenuActionHandler/` | Menu action dispatch |
| `MenuCheckMark/` | Menu check mark state |
| `TakeScreenshot/` | Window screenshot capture |
| `DisplaySystemInfo/` | System information display |
| `SystemSelectionOrchestrator/` | System selection screen |
| `SystemImageResolver/` | System image/logo resolution |

#### Data Management
| Service | Purpose |
|---------|---------|
| `Favorites/` | Favorites persistence (`FavoritesManager`) |
| `PlayHistory/` | Play history tracking (`PlayHistoryManager`) |
| `PlaySound/` | Sound effects |
| `AudioInput/` | Audio/gamepad input |
| `GamePad/` | Gamepad controller (SharpDX) |
| `DownloadService/` | Download management |

#### Cross-Cutting Concerns
| Service | Purpose |
|---------|---------|
| `DebugAndBugReport/` | Error logging, bug reporting, debug logging |
| `MessageBox/` | Message box abstraction |
| `HelpUser/` | Help/user guidance |
| `CheckForUpdates/` | Update checking |
| `CheckPaths/` | Path validation |
| `CheckForFileLock/` | File lock detection |
| `CheckIfDirectoryIsWritable/` | Directory write permission |
| `CheckForRequiredFiles/` | Required file verification |
| `CheckApplicationControlPolicy/` | Windows policy checking |
| `CleanAndDeleteFiles/` | Temp file/folder cleanup |
| `QuitOrReinstall/` | Application quit/reinstall logic |
| `UsageStats/` | Usage statistics |
| `SanitizeInputString/` | Input sanitization |

### 5. Data Layer

| Format | Usage |
|--------|-------|
| XML | System configuration (`system.xml`), emulator settings |
| MessagePack | Favorites, settings (binary serialization) |
| SQLite | Play history database |
| XML Resource Dictionaries | Localization (18 languages) |
| DAT files | MAME ROM database (`mame.dat`), history (`history.dat`), RetroAchievements (`RetroAchievements.dat`) |

### 6. Platform Abstraction Layer

**28 Interfaces** in `Interfaces/` with WPF implementations in `WpfServices/`:

| Interface | WPF Implementation |
|-----------|-------------------|
| `IApplicationLifetime` | `WpfApplicationLifetime` |
| `IDispatcherService` | `WpfDispatcherService` |
| `IFilePickerService` | `WpfFilePickerService` |
| `IImageLoader` | `WpfImageLoader` |
| `IMessageDialogService` | `WpfMessageDialogService` |
| `IResourceProvider` | `WpfResourceProvider` |
| `IWindowContext` | `WpfWindowContext` |
| `ICredentialProtector` | `WindowsCredentialProtector` |
| + 20 more interfaces | (awaiting platform implementations) |

---

## Design Patterns Used

| Pattern | Implementation |
|---------|----------------|
| **Strategy** | 8 launch strategies in `GameLauncher/Strategies/` |
| **Handler** | 21 emulator config handlers via `IEmulatorConfigHandler` |
| **Observer** | File watcher service, event-based game file change detection |
| **Factory** | `GameButtonFactory`, `GameListFactory` for dynamic UI creation |
| **Partial Class** | `MainWindow` split across 13 files |
| **Dependency Injection** | ~400 lines of DI registration in `App.xaml.cs` |
| **MVVM** | 44 ViewModels with `CommunityToolkit.Mvvm` |
| **Repository** | `FavoritesManager`, `PlayHistoryManager`, `SettingsManager` |

---

## Localization

**18 languages** supported via XAML resource dictionaries in `resources/`:

`ar`, `bn`, `de`, `en`, `es`, `fr`, `hi`, `id`, `it`, `ja`, `ko`, `nl`, `pt-br`, `ru`, `tr`, `ur`, `vi`, `zh-hans`

**2 custom themes** in `resources2/`:
- `Theme.Midnight.xaml` — Deep blue dark theme
- `Theme.HighContrast.xaml` — High contrast accessibility theme

---

## Bundled External Tools (18)

`BatchConvertIsoToXiso`, `BatchConvertToCHD`, `BatchConvertToCompressedFile`, `BatchConvertToRVZ`, `CHDMounter`, `CreateBatchFilesForPS3Games`, `CreateBatchFilesForScummVMGames`, `CreateBatchFilesForWindowsGames`, `CreateBatchFilesForXbox360XBLAGames`, `FindRomCover`, `GameCoverScraper`, `PSXPackager`, `RAHasher`, `RetroGameCoverDownloader`, `RomValidator`, `SevenZip`, `SimpleXisoDrive`, `SimpleZipDrive`

---

## Key NuGet Dependencies

| Package | Purpose |
|---------|---------|
| `CommunityToolkit.Mvvm` | MVVM framework |
| `MahApps.Metro` | Modern WPF UI controls |
| `Microsoft.Extensions.DependencyInjection` | DI container |
| `Microsoft.Extensions.Http` + `Resilience` | HTTP client factory with Polly |
| `Microsoft.Extensions.Caching.Memory` | In-memory caching |
| `MessagePack` | Binary serialization |
| `SharpDX` / `SharpDX.DirectInput` | Gamepad input |
| `SharpCompress` | Archive handling |
| `DokanNet` | File system mounting |
| `NAudio` | Audio playback |
| `YamlDotNet` / `Tomlyn` | YAML/TOML config parsing |
| `SQLite` | Play history database |
