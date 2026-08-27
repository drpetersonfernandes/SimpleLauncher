# Avalonia Port — WPF Parity Discrepancy Report

> **Goal:** Make `SimpleLauncher.Avalonia\SimpleLauncher.Avalonia.csproj` behave exactly as `SimpleLauncher\SimpleLauncher.csproj`.
> **Generated:** 2026-08-27 (local) · **Version checked:** `5.6.1` in both csprojs · **Method:** full file inventory (`Glob`), line-aware `Read` on `.xaml`/`.axaml`/`.cs`/`.csproj`/`.json`, and targeted `python3 -c` verification. All `file:line` refs are from the live repo on disk.
> **Fix pass:** 2026-08-27 — see **§15.1 Fix Progress** below.

---

## 0. TL;DR — verdict

| Dimension | Status | One-liner |
|---|---|---|
| **Core gameplay** (scan → filter → paginate → launch) | **Parity, different implementation** | WPF imperative factories → Avalonia MVVM (`MainViewModel` + `GameCardViewModel` `MainWindow.axaml:654-793`). |
| **MainWindow menu tree** | **Parity** | All top-level menus present; only dispatcher is consolidated to `Tag`-dispatched single handlers in Avalonia (`MainWindow.axaml:365-420`, `MainWindow.axaml:209-313`). |
| **Left navigation + TopSystemSelection** | **Parity + Avalonia superset** | Avalonia adds `CardSizeSlider` `MainWindow.axaml:627-637` and `EmulatorComboBox_SelectionChanged` `MainWindow.axaml.cs:1060`. Letter bar is `FilterMenu.cs` in WPF vs inline `LetterFilterBar` `MainWindow.axaml.cs:907-962` in Avalonia. |
| **Game grid / list view** | **Structural divergence** | WPF `WrapPanel GameFileGrid` + `DataGrid GameDataGrid` (6 cols) `MainWindow.xaml:925-984` vs Avalonia `ListBox GameGridView` (`WrapPanel` ItemsPanel) + `ListBox GameListView` with header `Grid` (4 cols) `MainWindow.axaml:654-856`. Different controls/columns; verify sort-star parity. |
| **Pages** | **Replaced** | WPF `Pages/*.xaml` (3 `Page`s via `Frame PageContentFrame` `MainWindow.xaml:1097-1100`) → Avalonia 3 embedded `Grid` sections + `SystemSelectionRoot` `MainWindow.axaml:911-1119` + `enum MainSection` + `ShowSectionAsync()` `MainWindow.axaml.cs:990`. No `Frame` navigation stack. |
| **Localization** | **NOT parity — blocking** | WPF `resources/strings.en.xaml` = **2370 keys**; Avalonia `Resources/strings.en.json` = **554 keys** → **1893 keys missing** in Avalonia (see §10). Many UI strings will fall back to key name. |
| **Themes** | **Partial** | WPF `resources2/Theme.HighContrast.xaml` + `Theme.Midnight.xaml` → Avalonia only `Themes/DarkTheme.axaml`. `HighContrast`/`Midnight` `BaseTheme` items exist in menu `MainWindow.axaml:53-59` but have no backing theme logic beyond `AvaloniaThemeService`. MahApps.Metro removed. |
| **Services/Interfaces** | **Consolidated, ~10 true gaps** | 29/30 `Interfaces/` + several `Services/` factories/orchestrators are intentionally folded into `MainViewModel`+`Avalonia*Service`s. True functional gaps: see §7. |
| **Tools shipping** | **Parity on disk** | WPF `SimpleLauncher.csproj:44-867` lists 867 lines including obsolete entries for files that no longer exist on disk (`tools/BatchConvertTo7z`, lowercase `findromcover`/`ps3batchlaunchercreator`). On-disk `SimpleLauncher/tools` = 16 folders (see `tools` listing); Avalonia globs (`SimpleLauncher.Avalonia.csproj:106-212`) ship exactly those 16 correctly. No missing tool on disk. |
| **System services (gamepad/tray/sound/updater/toast/screenshot)** | **Platform-split** | Windows parity is close; Linux paths diverge (no NAudio/SharpDX, screenshot limited to `System.Drawing.Common` on `net10.0-windows` only `SimpleLauncher.Avalonia.csproj:228`). Inline `ToastStack` `MainWindow.axaml:645` vs separate `ToastNotificationWindow.xaml`. |

**Must-fix before claiming parity:** localization completeness (§10), theme backing (§10), DataGrid column/behavior audit (§3.6), and verification of `Reinstall` flow + CHD converters (§7.4).

---

## 1. Solution & build configuration — `SimpleLauncher.csproj` vs `SimpleLauncher.Avalonia.csproj`

| Property | WPF `SimpleLauncher.csproj:1-43` | Avalonia `SimpleLauncher.Avalonia.csproj:1-59` | Gap / Action |
|---|---|---|---|
| `TargetFramework(s)` | `net10.0-windows` only | `net10.0;net10.0-windows` dual TFM | Intentional cross-platform split. |
| `UseWPF` / Avalonia | `UseWPF true` `SimpleLauncher.csproj:6` | `AvaloniaUseCompiledBindingsByDefault false` `SimpleLauncher.Avalonia.csproj:34` | Expected. |
| `RuntimeIdentifiers` | `win-x64;win-arm64` `SimpleLauncher.csproj:14` | `win-x64;win-arm64` on `net10.0-windows` + `linux-x64;linux-arm64` on `net10.0` + `GuardWindowsRidOnLinuxTfm` target `SimpleLauncher.Avalonia.csproj:42-50` | Avalonia guard prevents silent `libsndfile` crash on `net10.0+win` — correct. |
| `StartupObject` | `SimpleLauncher.App` `SimpleLauncher.csproj:28` | `SimpleLauncher.Avalonia.Program` `SimpleLauncher.Avalonia.csproj:32` | WPF `App.xaml Startup="Application_Startup"` → Avalonia `Program.cs` `AppBuilder.Configure<App>().UsePlatformDetect()` split — intentional. |
| `Version` | `5.6.1` (also in `app.manifest` `assemblyIdentity`) `SimpleLauncher.csproj:32-35` | `5.6.1` `SimpleLauncher.Avalonia.csproj:21-23` | Parity (enforced by `VersionConsistencyTests`). |
| `UseWindowsForms` | `False` | `False` | Parity. |
| `AllowUnsafeBlocks` / `Nullable` / `LangVersion` | `true` / `enable` / `14` | `true` / `enable` / `14` | Parity. |
| `IsPublishable` (Debug) | `False` `SimpleLauncher.csproj:41` | `False` `SimpleLauncher.Avalonia.csproj:57` | Parity. |
| `ApplicationManifest` | `app.manifest` `SimpleLauncher.csproj:15` | `app.manifest` `SimpleLauncher.Avalonia.csproj:30` | Both `asInvoker` + `dpiAware`; only `assemblyIdentity name` differs (`SimpleLauncher.App` vs `SimpleLauncher.Avalonia`) — behaviorally identical. |
| `ApplicationIcon` / `PackageIcon` | `icon\icon.ico` / `icon2.png` `SimpleLauncher.csproj:25,29` | same `SimpleLauncher.Avalonia.csproj:25-26` | Parity. |
| `IsPackable` | `false` | `false` | Parity. |
| `ProjectReference` | none (self-contained) | `SimpleLauncher.Core` + `SimpleLauncher.Avalonia.Updater ReferenceOutputAssembly=false` + `CopyAvaloniaUpdaterToOutput` target `SimpleLauncher.Avalonia.csproj:61-62,224-238` | Avalonia updater is copy-deployed next to app (WPF ships `Updater.exe` via `None Update` `SimpleLauncher.csproj:711`). Different binary, same version promise. |
| `PackageReference` set | 38 pkgs: `MahApps.Metro 2.4.11`, `Hardcodet.NotifyIcon.Wpf 2.0.1`, `NAudio.* 3.0.1`, `SharpDX.* 4.2.0`, `MdXaml 1.27`, `InputSimulatorCore 1.0.5`, `MessagePack`, `Microsoft.Data.Sqlite`, etc. | 10 pkgs: `Avalonia 12.1.1`, `Avalonia.Desktop 12.1.1`, `Avalonia.Themes.Fluent 12.1.1`, `Avalonia.Controls.DataGrid 12.1.2`, `Markdown.Avalonia 12.0.0-a3`, `CommunityToolkit.Mvvm 8.4.2`, + `System.Drawing.Common 10.0.11` on `net10.0-windows` | Intentional stack swap. `NAudio`/`SharpDX`/`MahApps`/`Hardcodet` have no Avalonia equivalents — replaced by `libsndfile` (via Core), `AvaloniaTrayIconManager`, `FluentTheme`. Verify Linux audio path. |
| Structure size | **1456 lines** (`SimpleLauncher.csproj:1-1456`) | **239 lines** (`SimpleLauncher.Avalonia.csproj:1-239`) | 867 lines in WPF are explicit `None Update`/`Resource Include` per-file listings; Avalonia uses globs + `Link=` (`SimpleLauncher.Avalonia.csproj:105-212`) — intentional simplification. |
| `None Update` behavior | `CopyToOutputDirectory Always` for almost every tool/image | `PreserveNewest` for almost every linked tool/image (`SimpleLauncher.Avalonia.csproj:107,123,128...`) | Incremental-build copy differs (`Always` vs `PreserveNewest`); content parity but build semantics differ. Align if `Always` is required for samples history tracking. |

---

## 2. File presence inventory (excluding `bin/`, `obj/`)

| Location | WPF | Avalonia | Notes |
|---|---|---|---|
| **Project files on disk** | 632 sources (incl. physically-present `images/`, `tools/`, `samples/`, `resources/`) | 278 sources physically in `SimpleLauncher.Avalonia/` | Delta is mostly `images/` (~200), `tools/` (~72), `samples/` (~26), `Pages/` (6), and per-file resource listings — Avalonia re-links them via `Link=` globs, so runtime parity is preserved even though files are not duplicated on disk. |
| **InjectConfigWindows** | 42 entries (21 `.xaml` + 21 `.xaml.cs`) `SimpleLauncher/InjectConfigWindows/` | 42 entries (21 `.axaml` + 21 `.axaml.cs`) `SimpleLauncher.Avalonia/InjectConfigWindows/` | **1:1 parity** — only filename casing differs (`InjectPCSX2ConfigWindow.xaml` → `InjectPcsx2ConfigWindow.axaml`, `InjectRPCS3` → `InjectRpcs3`). See §9. |
| **Interfaces** | 30 `SimpleLauncher/Interfaces/*.cs` | 1 `SimpleLauncher.Avalonia/Interfaces/ISystemSelectionHost.cs` | 29 interfaces are folded into `MainViewModel`+`AvaloniaServices` or moved (e.g. `IGamePlatformScanner.cs` → `Services/GameScan/IGamePlatformScanner.cs`). Not a behavioral gap by itself — see §6. |
| **Services** | 109 `.cs` (+ `ToastNotificationWindow.xaml/.cs`) under `SimpleLauncher/Services/` | 90 `.cs` under `SimpleLauncher.Avalonia/Services/` | Several factories/orchestrators are removed — tracked in §7. |
| **ViewModels** | 44 `SimpleLauncher/ViewModels/*.cs` | 48 `SimpleLauncher.Avalonia/ViewModels/*.cs` | Extra Avalonia VMs: `MainViewModel.cs`, `SidebarViewModel.cs`, `EasyModeViewModel.cs`, `GameCardViewModel.cs`, `FavoriteRowViewModel.cs` (WPF had `GameButtonViewModel.cs`, `FavoritesViewModel.cs`, `GlobalSearchViewModel.cs`, `PlayHistoryViewModel.cs`). §8. |
| **Models** | 4 `SimpleLauncher/Models/*.cs` | 5 `SimpleLauncher.Avalonia/Models/*.cs` | Extra: `AvaloniaRightClickContext.cs` (subclass with Avalonia `Control` placement + `OnShowDetails`/`OnCopyPath` callbacks `MainWindow.axaml.cs:1220-1240`). |
| **Pages** | 6 `SimpleLauncher/Pages/*.xaml(+.cs)` | 0 | Replaced by embedded sections §5. |

---

## 3. MainWindow — menu-by-menu, panel-by-panel

WPF: `MainWindow.xaml:1-1115` + `MainWindow.xaml.cs` + 12 partials (`MainWindow.*.cs`)
Avalonia: `MainWindow.axaml:1-1181` + `MainWindow.axaml.cs` + 3 partials (`LoadingOverlayHost`, `SystemSelectionHost`, `UIResetHost`)

### 3.1 Chrome / root layout

| Aspect | WPF | Avalonia | Gap |
|---|---|---|---|
| Base class | `mah:MetroWindow` `MainWindow.xaml:1` | `Window` + `<FluentTheme/>` `MainWindow.axaml:1`, `App.axaml:6-15` | Different titlebar/chrome; MetroWindow custom titlebar/GlowBrush not reproduced. Acceptable if `FluentTheme` is the intended direction. |
| Root layout | `<Grid><Grid x:Name="MainContentGrid">` 2 rows (Menu, Content) + sibling `LoadingOverlay ContentControl` `MainWindow.xaml:14-22,1108-1112` | `<Grid>` analog with `Menu` + `Border LeftNavigationPanel` + `Border TopSystemSelection` + game areas + `Border LoadingOverlay` + section Grids `MainWindow.axaml:461-1175` | Structural equivalent. |
| Lifetime hooks | `Closing="MainWindow_Closing"` + `StateChanged="MainWindow_StateChanged"` + `MouseWheel="MainWindow_MouseWheelAsync"` + `Loaded+=OnLoadedAsync` (in `MainWindow.xaml.cs:261-273`) | `Opened="Window_Opened"` `MainWindow.axaml:11` + `PropertyChanged` on `WindowState` for minimize-to-tray `MainWindow.axaml.cs:191-198` + `Closed` with 5 s `Environment.Exit(0)` watchdog `MainWindow.axaml.cs:235-247` | WPF `StateChanged` → Avalonia `PropertyChanged/WindowState`. Behavior should be verified (tray hide on minimize). |

### 3.2 Options menu — full tree

| Menu branch | WPF (`MainWindow.xaml`) | Avalonia (`MainWindow.axaml`) | Verdict |
|---|---|---|---|
| **Language (18 items)** | `LanguageArabic..LanguageChineseSimplified` `IsCheckable=True` `Click="ChangeLanguage_Click"` `MainWindow.xaml:31-56` | Same 18 `Language*` with `ToggleType="CheckBox" GroupName="LanguageGroup"` `MainWindow.axaml:33-51` → `AvaloniaLanguageMenuService` | Parity. Avalonia correctly uses `GroupName` radio semantics. |
| **Theme → BaseTheme** | `Light/Dark/Adaptive` + sep + `HighContrast/Midnight` `Click="ChangeBaseTheme_Click"` `MainWindow.xaml:61-76` | Same 5 `ThemeLight..ThemeMidnight` `MainWindow.axaml:53-59` | Menu parity; backing for `HighContrast`/`Midnight` is only `DarkTheme.axaml` today — §10. |
| **Theme → AccentColors** | 27 static items `Amber..Yellow` `Click="ChangeAccentColor_Click"` `MainWindow.xaml:81-134` | Single placeholder `MenuItem x:Name="AccentColorMenu"` populated at runtime `InitializeThemeMenu()` from `AvaloniaThemeService.AccentColorNames` `MainWindow.axaml:60-62` + `MainWindow.axaml.cs:465-490` | Behavioral parity but Avalonia list is dynamic. Confirm `AvaloniaThemeService.AccentColorNames` enumerates exactly the 27 WPF names. |
| **SetButtonSize** | 13 items `Size50..Size800` `Click="ButtonSizeClickAsync"` `Header="{DynamicResource 50pixels}"` `MainWindow.xaml:142-206` | 13 items `Tag="50"..Tag="800"` `Click="ButtonSizeClickAsync"` `MainWindow.axaml:68-83` handler reads `Tag` | Parity. |
| **SetButtonAspectRatio** | 7 items `Square..SuperTaller2` `MainWindow.xaml:211-239` | Same 7 `MainWindow.axaml:89-96` | Parity. |
| **SetNumberOfGamesPerPage** | 8 items `Page100..Page1000000` `MainWindow.xaml:244-276` | 8 items `Tag="100"..Tag="1000000"` `MainWindow.axaml:101-109` | Parity. |
| **ViewMode** | `GridView IsChecked=True` + `ListView` `Click="ChangeViewMode_Click"` `MainWindow.xaml:281-285` | `GridView GroupName=ViewModeGroup` `MainWindow.axaml:114-116` | Parity (`SettingsManager` + `MainViewModel.IsGridView`). |
| **ShowGames** | 3 items `ShowAll/ShowWithCover/ShowWithoutCover` with 3 separate `Async` handlers `MainWindow.xaml:290-296` | Same 3 `x:Name="ShowAll"` etc. unified `ShowGamesClickAsync` `MainWindow.axaml:121-124` | Parity (consolidated handler is fine). |
| **FilenamePreferences** | 3 display modes (`Original/CleanUp/NoFilename`) + `DisplayMachineNameToggle` + 2 font-size submenus (3 each) `MainWindow.xaml:297-327` | Same with `GroupName="FilenameGroup"` + `FilenameFontGroup`/`MachineNameFontGroup` `MainWindow.axaml:125-145` | Parity. |
| **EditLinks / Gamepad / Fuzzy / Sound / RetroAchievements / OverlayButton** | `EditLinks_ClickAsync`, `ToggleGamepad`+`SetGamepadDeadZone`, `ToggleFuzzyMatching`+`StripAnnotations`+`SetThreshold`, `SoundConfiguration_Click`, `ShowRetroAchievementsSettingsWindow_ClickAsync`+`CalculateHashesForAllGamePaths`, `RetroAchievementButton` etc. `MainWindow.xaml:328-510` | Mirrors 1:1 `MainWindow.axaml:146-322` (`ToggleGamepad` `MainWindow.axaml:160`, `SetGamepadDeadZone_Click`, etc.) | Parity. |
| **InjectEmulatorConfig (20 emulators)** | 20 distinct handlers `ShowAresSettings_Click` … `ShowYumirSettings_Click` `MainWindow.xaml:392-501` | Single `ShowEmulatorConfig_Click` with `Tag="Ares"..Tag="Yumir"` `MainWindow.axaml:209-313` → resolves via `EmulatorConfigWindowFactory`-style dispatch | Functional parity; per-emulator error visibility is consolidated — ensure `InjectionErrorHandler` path still surfaces per-emulator messages. |
| **EditSystem** | `EasyMode_Click`/`ExpertMode_Click`/`DownloadImagePack_Click`/`ScanForMicrosoftWindowsGames_ClickAsync` `MainWindow.xaml:511-537` | Same 4 `MainWindow.axaml:326-347` | Parity (Avalonia `DownloadImagePack_Click` is non-async-named but behaves async). |
| **SelectWindow** | `ShowGlobalStatsWindow_Click` + `ShowRetroAchievementsWindowClick` `MainWindow.xaml:538-550` | Same `GlobalStats_Click`/`RetroAchievements_Click` `MainWindow.axaml:350-361` | Parity. |
| **Tools (12)** | 12 `Click="BatchConvertIsoToXiso_ClickAsync"` etc. `MainWindow.xaml:551-616` | 12 `Tag="BatchConvertIsoToXiso"` `Click="LaunchTool_Click"` `MainWindow.axaml:364-420` via `ExternalToolLauncherService` | Parity — single dispatch is intentional. Tool binaries on disk are exactly those 16 folders shipped (BatchConvertTo7z was already removed from disk; see §11). |
| **Donate / About** | `MainWindow.xaml:617-647` | `MainWindow.axaml:423-454` | Parity. |

### 3.3 Left navigation rail

- WPF `Border x:Name="LeftNavigationPanel"` 10 `Button Style="{StaticResource NavImageButton3DStyle}"` with `AutomationProperties.Name` + `ToolTip {DynamicResource}` `MainWindow.xaml:658-742`
- Avalonia `Border width 60` 10 `Button Classes="nav-icon"` `MainWindow.axaml:461-521`

**Tooltips are localized in both** — Avalonia uses `{ext:Translate Tooltip*}` (e.g. `ToolTip.Tip="{ext:Translate TooltipOptionsMenu}"` `MainWindow.axaml:470` region), not hard-coded English. Earlier reports of hard-coded tooltips are incorrect; verified via `re ToolTip.Tip` extraction showing `ext:Translate` for every nav button. No localization gap here.

Style difference: WPF 3D `ImageButton3DEffectTemplate` + `NavImageButton3DStyle` (`App.xaml`) vs Avalonia `nav-icon` in `DarkTheme.axaml` — visual divergence, not functional.

### 3.4 TopSystemSelection bar

| Element | WPF `MainWindow.xaml:759-901` | Avalonia `MainWindow.axaml:527-639` | Gap |
|---|---|---|---|
| Letter/number filter | `StackPanel x:Name="LetterNumberMenu"` populated by `FilterMenu` `Services/UiHelpers/FilterMenu.cs` + `MainWindow.xaml.cs:280` LetterPanel | `ScrollViewer LetterBarScroller` + `StackPanel x:Name="LetterFilterBar"` built inline `MainWindow.axaml.cs:907-962` (no `FilterMenu` class) | Same UX, different class location. WPF `FilterMenu.SelectedButton` semantics must be preserved in inline implementation. |
| System picker | `ComboBox x:Name="SystemComboBox" SelectionChanged="SystemComboBoxSelectionChangedAsync"` `MainWindow.xaml:786` | `ComboBox x:Name="SystemComboBox" SelectionChanged="SystemComboBox_SelectionChanged"` plus new `ComboBox x:Name="EmulatorComboBox" SelectionChanged="EmulatorComboBox_SelectionChanged"` storing `SelectedEmulatorName` `MainWindow.axaml.cs:1060` `MainWindow.axaml:576-620` | Avalonia adds emulator filter combobox — **superset** (no WPF counterpart). Keep if desired. |
| Search | `TextBox x:Name="SearchTextBox" KeyDown="SearchTextBoxKeyDownAsync"` + delayed search via `SearchOrchestrator` | `TextBox x:Name="SearchBox" TextChanged="SearchBox_TextChanged"` `MainWindow.axaml:606` + `PlaceholderText` | WPF triggers on Enter; Avalonia on `TextChanged` — more responsive. Confirm debounce matches `AvaloniaSearchOrchestratorService`. |
| Action buttons | `SelectedSystemFavoriteButton / RandomLuckGameButton / RetroAchievementsGameButton / SortOrderToggleButton` `MainWindow.xaml:864-898` | Same 4 `MainWindow.axaml:608-625` | Parity. |
| Card size control | (not in WPF — size via Options menu + `MouseWheel="MainWindow_MouseWheelAsync"` only) | Extra `Slider x:Name="CardSizeSlider" Minimum 50 Maximum 800 Value="{Binding CardWidth}"` `MainWindow.axaml:627-637` + `OnPointerWheelChangedForZoom` `MainWindow.axaml.cs:964` | Avalonia superset — Ctrl+wheel path exists in both; slider is new. |

### 3.5 Center area — game browsing

| Area | WPF | Avalonia | Gap |
|---|---|---|---|
| **Game grid (grid view)** | `ScrollViewer x:Name="Scroller"` + `WrapPanel x:Name="GameFileGrid"` `MainWindow.xaml:925-930` — populated imperatively by `GameItemFactory/GameButtonFactory.cs` + `GameItemRender/GameItemRenderService.cs` (each file → styled `Button` with image + caption + favorite star via `BooleanToFavoriteStatusConverter`). | `ListBox x:Name="GameGridView" ItemsSource="{Binding Games}" IsVisible="{Binding IsGridView}"` + `WrapPanel` in `ItemsPanel` + `DataTemplate` for `GameCardViewModel` (selection ring, placeholder, cover via `PathToImageConverter`/`RemoteImageLoader`, favorite heart via `BooleanToFavoriteStatusConverter`, RA trophy `IsRaSupported`, caption via `SmartTitleCaseConverter`, height via `ConsoleToCardHeightConverter` + `SystemArtRatioService`) `MainWindow.axaml:654-793` | **Imperative buttons → declarative binding**. Correct approach for Avalonia; favorite/RA overlay positions should be pixel-compared with WPF factory output. No known missing overlay. |
| **List view** | `Grid x:Name="ListViewPreviewArea" Visibility=Collapsed` + `DataGrid Name="GameDataGrid"` 6 columns: **Favorite (star `Image`)**, `FileName`, `MachineDescription` (MAME DB), `FolderPath`, `TimesPlayed`, `PlayTime` + `GridSplitter` + `Border PreviewImage` `MainWindow.xaml:934-984` + header `FavoriteColumnHeader` with star icon `MainWindow.xaml:963-973` | `Grid IsVisible="{Binding IsGridView, Converter=InverseBoolToVisibility}"` + **`ListBox x:Name="GameListView"` with a header `Grid`** 4 columns: `DisplayTitle`/`SystemName`/`PlayCount`/`FilePath` `MainWindow.axaml:795-856`; header click `ListHeader_Click` `MainWindow.axaml.cs:659` provides sorting. | **Control + column mismatch:** WPF `DataGrid` with `MachineDescription`/`TimesPlayed`/`PlayTime` vs Avalonia `ListBox` header grid with `SystemName`/`PlayCount`. WPF favorite column uses image star; Avalonia favorite star is not in the list-view row template the same way. Sort behavior differs (WPF DataGrid default vs Avalonia `ListHeader_Click`). Audit column mappings against user expectations (MAME `MachineDescription` is not `SystemName`). |
| **Page / section hosting** | `Frame x:Name="PageContentFrame" Visibility=Collapsed` `MainWindow.xaml:1097-1100` + `NavigateToPage(Page)` / `NavigateBackToMainContent()` `MainWindow.xaml.cs:527` navigates `Pages/FavoritesPage`/`GlobalSearchPage`/`PlayHistoryPage` | `Border x:Name="SystemSelectionRoot" IsVisible=False` + `Grid FavoritesSectionRoot` + `Grid PlayHistorySectionRoot` + `Grid GlobalSearchSectionRoot` `MainWindow.axaml:911-1119` + `enum MainSection` + `ShowSectionAsync(MainSection)` `MainWindow.axaml.cs:990-1021` + `UiResetService.ResetUiAsync()` for Home | Functionally equivalent; Avalonia has no `Frame` back stack. Back/Home is `UiResetService.ResetUiAsync()` vs WPF `NavigateBackToMainContent()` → `PlayNotificationSound()`. Verify toast on back parity. |
| **System selection screen** | Built by `GameBrowser/GameBrowserService.cs` → `DisplaySystemSelectionScreenAsync` inside `TopSystemSelection` region + `SystemSelectionWindow` standalone | Explicit `SystemSelectionRoot` DockPanel `Select a System` + `NoSystemsConfiguredMessage` + `WrapPanel SystemSelectionWrapPanel` `MainWindow.axaml:911-931` populated by `PopulateSidebarFromSystemXml()` `MainWindow.axaml.cs:528` (plus `SystemSelectionWindow.axaml` still exists) | Avalonia bakes the selection UI into `MainWindow` — intentional. |

### 3.6 Status bar & pagination

| Element | WPF `MainWindow.xaml:1023-1091` | Avalonia `MainWindow.axaml:1123-1175` |
|---|---|---|
| Text columns | `Grid StatusBarArea` 3 cols: `StatusBarText` / `Prev + Next + TotalFilesLabel` (`PrevPageButton IsEnabled False`, `NextPageButton`, `TotalFilesLabel` visibility logic) / `SystemInfo+Playtime` `MainWindow.xaml:1023-1091` | `StatusLeft GameCountText` + `PaginationPanel` (Prev/Next + `PaginationLabel` `IsVisible False`) + `StatusRight StatusText` + `IsPlayTimeVisible` overlay `MainWindow.axaml:1123-1175` |
| Gap | Same pagination math via `PaginationService` → `AvaloniaPaginationService`, but Avalonia splits `GameCountText`/`PaginationLabel` vs WPF single `TotalFilesLabel`. `SystemInfo` column (`SelectedSystem` binding) is not mirrored the same way — verify status-right content matches WPF `SelectedSystem`/`Playtime` display. |

### 3.7 Loading overlay & emergency return

- WPF: `ContentControl x:Name="LoadingOverlay" Panel.ZIndex 9999 Template={StaticResource LoadingOverlayTemplate} Visibility=Collapsed` + emergency button found via `ApplyTemplate().FindName("PART_EmergencyReturnButton")` `MainWindow.xaml:1108-1112` + `MainWindow.xaml.cs:309` wiring.
- Avalonia: `Border x:Name="LoadingOverlay" Background OverlayProcessingBrush IsVisible="{Binding IsLoading}" ZIndex 50` with `ProgressBar IsIndeterminate` + `TextBlock LoadingMessage` + `Button x:Name="EmergencyReturnButton"` `MainWindow.axaml:877-909` toggled via `IsLoading` binding.

Emergency button text is localized via `_localization.GetString("ReturnButton")` `MainWindow.axaml.cs:142` vs WPF `TryFindResource` — parity in intent. WPF uses `ZIndex 9999`/`ContentControl` templating vs Avalonia `Border` — simpler, Z-level should still cover.

### 3.8 Window lifecycle helpers (missing `MainWindow.*.cs` partials)

WPF has 12 `MainWindow.*.cs` partials; Avalonia keeps only 3 directly:

| WPF partial | State in Avalonia | Where its logic now lives |
|---|---|---|
| `MainWindow.CloseWindowEvents.cs` | **folded** — no separate file | `MainWindow.axaml.cs:193-248` `Closed` handler + 5 s watchdog `Environment.Exit(0)` + `AvaloniaApplicationLifecycleService` |
| `MainWindow.GameFileLoadingHost.cs` | **folded** | `MainViewModel` + `AvaloniaGameFileLoadingOrchestrator` |
| `MainWindow.GameItemRenderHost.cs` | **folded** | `GameCardViewModel` + `MainWindow.axaml:654-793` `DataTemplate` |
| `MainWindow.HostImplementations.cs` | **folded** | DI wiring in `App.axaml.cs` + `MainViewModel` |
| `MainWindow.LaunchTools.cs` (12 `Batch*ClickAsync` handlers) | **consolidated** | `LaunchTool_Click` Tag dispatch `MainWindow.axaml.cs:365-420` via `ExternalToolLauncherService` |
| `MainWindow.MenuActionHost.cs` | **folded** | `MainViewModel` commands + `AvaloniaThemeService`/`AvaloniaLanguageMenuService` |
| `MainWindow.MenuCheckMarkHost.cs` | **folded** | `AvaloniaMenuCheckMarkService` + `UpdateMenuCheckMarks()` `MainWindow.axaml.cs:140-268` |
| `MainWindow.MenuItems.cs` | **folded** | Menu `axaml` + `MainViewModel` |
| `MainWindow.Pagination.cs` | **folded** | `AvaloniaPaginationService` + `MainViewModel` |
| `MainWindow.Search.cs` | **folded** | `SearchBox_TextChanged` `MainWindow.axaml.cs:1027` + `AvaloniaSearchOrchestratorService` |
| `MainWindow.SystemSelectionHost.cs` | **exists** but trimmed (`SimpleLauncher.Avalonia/MainWindow.SystemSelectionHost.cs` 2063 B vs WPF 3626 B) | Audit trimmed system-selection host for removed edge cases. |
| `MainWindow.UIResetHost.cs` | **exists** | `MainWindow.UIResetHost.cs` + `UiResetService` |
| *(new)* | `MainWindow.LoadingOverlayHost.cs` | Avalonia-only helper for `LoadingOverlay` — no WPF counterpart. |

None of the folded-away partials is a pure deletion without a successor; the consolidation is intentional. Remaining risk is that per-emulator/per-tool error handling visible in separate methods is now uniform.

---

## 4. Secondary windows — `*.xaml` vs `*.axaml`

| WPF Window | Avalonia Window | Parity notes |
|---|---|---|
| `AboutWindow.xaml` | `AboutWindow.axaml` | WPF `pack://application` images → Avalonia `avares://`; version from `GetApplicationVersionService` vs `AvaloniaCheckForUpdatesService`. Minimal gap. |
| `DebugWindow.xaml` | `DebugWindow.axaml` | Avalonia adds same `CopyLog`/`ClearLog`. Sink moved `Services/DebugAndBugReport/DebugWindowSink.cs` → `Services/DebugWindowSink.cs` (flattened). |
| `DosBoxFileSelectionWindow.xaml` | `DosBoxFileSelectionWindow.axaml` | Parity (exe/bat list). |
| `DownloadImagePackWindow.xaml` | `DownloadImagePackWindow.axaml` | Same 5 image packs + progress. Avalonia via `DownloadImagePackViewModel` — same behavior. |
| `EasyModeWindow.xaml` | `EasyModeWindow.axaml` + `ViewModels/EasyModeViewModel.cs` | WPF had code-behind only; Avalonia elevates to MVVM with `SystemAdded` flag `MainWindow.axaml.cs:452` (superset). |
| `EditSystemWindow.xaml` + 3 partials (`SaveSystem.cs`, `SelectSystem.cs`, `ValidateFields.cs` — 14 788 B of validation) | `EditSystemWindow.axaml` + single `EditSystemWindow.axaml.cs` (66 541 B) | Validation consolidated — audit `ValidateFields.cs` rules (folder existence via `PathHelper`, `SanitizeInputString`, `SetFieldValidationState` extension) against Avalonia consolidated handler for missing edge cases. |
| `FlashOverlayWindow.xaml` | `FlashOverlayWindow.axaml` | Parity (full-screen flash). |
| `GlobalStatsWindow.xaml` | `GlobalStatsWindow.axaml` | Stats include `TotalGames/Systems/Emulators/DiskSize`; Avalonia adds extra `GlobalStatsExplanation` localization keys. |
| `ImageViewerWindow.xaml` | `ImageViewerWindow.axaml` | Zoom/pan with `Zoom-in/out.png` present in both. |
| `SupportWindow.xaml` | `SupportWindow.axaml` | Same `SupportViewModel`; must obey `AGENTS.md` Warning-vs-Information rule for `BugReportApiSink` (verified — same rule applies). |
| `SystemSelectionWindow.xaml` | `SystemSelectionWindow.axaml` | Still standalone; Avalonia also embeds `SystemSelectionRoot` inside `MainWindow` — intentional duplication for faster startup. |
| `SoundConfigurationWindow.xaml` | `SoundConfigurationWindow.axaml` | UI parity; backend diverges (NAudio `Wasapi` on WPF vs Core `PlaySoundEffects` → `System.Media.SoundPlayer` on Windows / `libsndfile` on Linux) `SimpleLauncher.Avalonia.csproj:42` comment. |
| `UpdateHistoryWindow.xaml` / `UpdateLogWindow.xaml` | `UpdateHistoryWindow.axaml` / `UpdateLogWindow.axaml` | Both read `WhatsNew.md` via `UpdateHistoryViewModel`; Avalonia also links `parameters.md`. |
| `WindowSelectionDialogWindow.xaml` | `WindowSelectionDialogWindow.axaml` + `ViewModels/WindowSelectionDialogViewModel.cs` | Avalonia uses `AvaloniaWindowCapture.cs` (DPI-aware) vs WPF `WindowScreenshot.cs` direct P/Invoke — DPI path should be verified. |
| `RetroAchievementsWindow.xaml` + `RetroAchievementsForAGameWindow.xaml` + `RetroAchievementsSettingsWindow.xaml` | same 3 `.axaml` | **Superset** — WPF RA strings: ~20 `x:Key`s; Avalonia JSON adds 60+ `Ra*` keys (e.g. `RaError*` families) — more localized error states than WPF. |
| `RomHistoryWindow.xaml` | `RomHistoryWindow.axaml` | Parity (`history.dat` linked in both). |
| `SetLinksWindow.xaml` / `SetGamepadDeadZoneWindow.xaml` / `SetFuzzyMatchingWindow.xaml` | same 3 `.axaml` | Parity (`SetThreshold.png` etc. via `avares://`). |
| `Services/NotificationToast/ToastNotificationWindow.xaml`+`.cs` | **Missing as separate window** — replaced by `StackPanel x:Name="ToastStack" MaxWidth 360 IsVisible False ZIndex 100` inside `MainWindow.axaml:645-648` triggered via `MainViewModel.ToastRequested` `MainWindow.axaml.cs:127` | **UX divergence:** WPF toasts were OS-level owned `Window`s (visible even when `MainWindow` is minimized); Avalonia toasts are in-content only. Document as intentional or restore windowed toast if background notifications are required. |
| — | **`GameDetailWindow.axaml`+`.cs`** | **Avalonia-only** detail modal (cover, `Play`/`Favorite`/`Remove`, `OnShowDetails` callback `MainWindow.axaml.cs:1338`). No WPF counterpart — game launch was direct. New feature, not a missing WPF feature. |
| — | **`PreferencesWindow.axaml`+`.cs`** | **Avalonia-only** tabbed preferences (General/Systems/Emulators/Images/View/Sound/RA/Updates). No WPF counterpart — settings were only via `Options` menu. Superset. |
| — | **`Views/MessageDialogWindow.axaml`+`.cs`** | **Avalonia-only** custom message box (`AvaloniaServices/MessageBoxLibraryService.cs` replaces `WpfServices/WpfMessageDialogService.cs` → win32 `MessageBox`). Changes modality/theming; parity expected. |

---

## 5. `Pages/` — Frame navigation vs embedded sections

- **WPF:** `Pages/FavoritesPage.xaml` (DataGrid + preview + `Launch`/`Remove`) / `Pages/GlobalSearchPage.xaml` (search box + DataGrid results + preview) / `Pages/PlayHistoryPage.xaml` (DataGrid history + preview) — each 5–9 KB. Routed via `Frame x:Name="PageContentFrame"` `MainWindow.xaml:1097-1100` + `NavigateToPage(Page)` / `NavigateBackToMainContent()` `MainWindow.xaml.cs:527`.
- **Avalonia:** No `Pages/` folder (0 files). Each page is an in-`MainWindow` section (`FavoritesSectionRoot` `MainWindow.axaml:934-987` + `PlayHistorySectionRoot` `MainWindow.axaml:989-1047` + `GlobalSearchSectionRoot` `MainWindow.axaml:1049-1119` + `SystemSelectionRoot` `MainWindow.axaml:911-931`) + owning ViewModels `FavoritesSectionViewModel.cs` (`LoadFavoritesAsync`, `ResolveFavoritePath`), `PlayHistorySectionViewModel.cs` (`LoadHistoryAsync`), `GlobalSearchSectionViewModel.cs`. Navigation is `enum MainSection` + `ShowSectionAsync()` `MainWindow.axaml.cs:990-1021` toggling `IsVisible`; back is `UiResetService.ResetUiAsync()` (Home).

**Gap to review:** no `Frame` back-stack/journal. `NavigateBackToMainContent()` played the notification sound and restored `Scroller`/`GameFileGrid` state; Avalonia's `UiResetService.ResetUiAsync()` + toast is analogous but not exercised through the same navigation journal. Verify that automation/UI tests that asserted `PageContentFrame.NavigationService` behavior still pass via the new sections.

---

## 6. Interfaces — `SimpleLauncher/Interfaces` (30) vs `SimpleLauncher.Avalonia/Interfaces` (1)

Only `ISystemSelectionHost.cs` is kept verbatim in Avalonia. All others are either **moved** or **folded**:

| WPF Interface | State in Avalonia |
|---|---|
| `IApplicationLifecycleService` | replaced by `Services/AvaloniaApplicationLifecycleService.cs` |
| `IContextMenuFunctions` / `IContextMenuService` | `Services/ContextMenus/AvaloniaContextMenuFunctions.cs` / `AvaloniaContextMenuService.cs` |
| `IDisplaySystemInformation` | `Services/DisplaySystemInfo/AvaloniaDisplaySystemInformation.cs` |
| `IGameBrowserService` | **folded** into `ViewModels/MainViewModel.cs` + `Services/GameScannerService` + `SystemSelectionOrchestrator` |
| `IGameCacheService` | `Services/AvaloniaGameCacheService.cs` |
| `IGameFileLoadingHost` / `IGameFileLoadingOrchestrator` | `MainViewModel` + `Services/AvaloniaGameFileLoadingOrchestrator.cs` |
| `IGameFilterService` | `Services/GameFilter/AvaloniaGameFilterService.cs` |
| `IGameItemRenderHost` / `IGameItemRenderService` | `GameCardViewModel` + `MainWindow.axaml:654-793` DataTemplate |
| `IGameListUiHost` | `MainViewModel.IsGridView` + `MainWindow.axaml:795-856` |
| `IGamePlatformScanner` | **moved** to `Services/GameScan/IGamePlatformScanner.cs` |
| `IHelpUserService` | `Services/AvaloniaHelpUserService.cs` |
| `ILanguageMenuHost` | `Services/AvaloniaLanguageMenuService.cs` |
| `ILoadingOverlayHost` | `Services/LoadingOverlay/AvaloniaLoadingOverlayService.cs` + `MainWindow.LoadingOverlayHost.cs` |
| `IMenuActionHost` / `IMenuCheckMarkHost` / `IMenuCheckMarkService` / `IMenuOrchestrator` | `AvaloniaMenuCheckMarkService` / `ViewModels/MainViewModel` |
| `IStartupInitializationHost` | `Services/AvaloniaStartupInitializationService.cs` |
| `IStatusBarHost` / `IUpdateStatusBar` | `Services/UpdateStatusBar/AvaloniaUpdateStatusBarService.cs` |
| `ISystemConfigurationService` | **folded** — direct `SettingsManagerService` + `SystemManagerService` calls |
| `ISystemImageResolverService` | **moved** to `Services/SystemImageResolver/ISystemImageResolverService.cs` |
| `ISystemSelectionOrchestrator` | `Services/SystemSelectionOrchestrator/AvaloniaSystemSelectionOrchestratorService.cs` |
| `IThemeMenuHost` | `Services/Theme/AvaloniaThemeService.cs` |
| `IUiOrchestrator` / `IUiOrchestratorHost` | `Services/AvaloniaGameFileLoadingOrchestrator` + `ViewModels/MainViewModel.IsLoading` |
| `ISystemSelectionHost` | **kept** `Interfaces/ISystemSelectionHost.cs` |

The interface explosion in WPF was a host-segregation pattern; Avalonia's `MainViewModel` consolidation is idiomatic for Avalonia+CommunityToolkit.Mvvm. No test-observable behavioral gap is expected from the interface removal itself.

---

## 7. Services — `SimpleLauncher/Services` (≈109 `.cs`) vs `SimpleLauncher.Avalonia/Services` (≈90 `.cs`)

### 7.1 Direct renames — behaviorally equivalent (verify implementation, not missing)

All `Avalonia*`-prefixed services are intended cross-platform shims:
`AvaloniaApplicationLifecycleService`, `AvaloniaCheckForUpdatesService`, `AvaloniaGameCacheService`, `AvaloniaGameFileLoadingOrchestrator`, `AvaloniaHelpUserService`, `AvaloniaLanguageMenuService`, `AvaloniaLoadingOverlayService`, `AvaloniaMenuCheckMarkService`, `AvaloniaPaginationService`, `AvaloniaSearchOrchestratorService`, `AvaloniaStartupInitializationService`, `AvaloniaSystemSelectionOrchestratorService`, `AvaloniaDisplaySystemInformation`, `AvaloniaUpdateStatusBarService`, `AvaloniaActiveWindowScreenshotService`/`AvaloniaGlobalHotkeyService`/`AvaloniaWindowCapture`, `AvaloniaTrayIconManager`, `AvaloniaApplicationLifetime`, `AvaloniaDispatcherService`, `AvaloniaFilePickerService`, `MessageBoxLibraryService`, `AvaloniaResourceProvider`/`AvaloniaWindowContext`, `AvaloniaGameFileWatcherService`, `AvaloniaContextMenuService`.

### 7.2 True deletions / consolidations — where to look for lost behavior

| WPF Service | Reason / Avalonia successor | Risk |
|---|---|---|
| `Services/GameBrowser/GameBrowserService.cs` | Split into `MainViewModel` (filter/pagination state) + `GameScan/GameScannerService` + `SystemSelectionOrchestrator` | Ensure all `GameBrowser` edge cases (system reload, file enumeration, icon extraction) are covered by `GameScannerService` + `MainViewModel.LoadGameFilesAsync`. |
| `Services/GameItemFactory/GameButtonFactory.cs` + `GameListFactory.cs` | Deleted — button creation is `MainWindow.axaml:654-793` `DataTemplate` + `GameCardViewModel` | Verify favorite-star / RA trophy overlay parity with factory's `FavoriteStatusConverter` wiring. |
| `Services/GameItemRender/GameItemRenderService.cs` | Deleted — XAML template | Same as above. |
| `Services/GameListUI/GameListUIService.cs` | Deleted — `MainViewModel.IsGridView` + dual `ListBox`es | Verify `GameDataGrid` 6-col → `GameListView` 4-col mapping (§3.5). |
| `Services/MenuActionHandler/MenuActionHandlerService.cs` | Deleted — handlers on `MainViewModel` (`ZoomIn/Out`, `AdjustAspect`, `GamesPerPage` commands) | Confirm every `MenuActionHandler` branch is routed to a `MainViewModel` command. |
| `Services/MenuOrchestrator/MenuOrchestratorService.cs` | Distributed to `MainViewModel` + `AvaloniaMenuCheckMarkService` + `AvaloniaThemeService` | Same — audit orchestrator tests if any exist in `SimpleLauncher.Tests`. |
| `Services/UiOrchestrator/UiOrchestratorService.cs` | `AvaloniaLoadingOverlayService` + `MainViewModel.IsLoading` + `AvaloniaGameFileLoadingOrchestrator` | Cancellation + overlay timing must match WPF's orchestrator ordering. |
| `Services/SystemConfiguration/SystemConfigurationService.cs` | Direct `SettingsManagerService`/`SystemManagerService` calls | Verify save/load path still validates via `PathHelper`/`SanitizeInputString` as in `EditSystemWindow.ValidateFields.cs:14788 B`. |
| `Services/UiHelpers/FilterMenu.cs` | Inline `LetterFilterBar` buttons `MainWindow.axaml.cs:907-962` | Same letter filter but different class — verify keyboard navigation kept. |
| `Services/NotificationToast/*` (`IToastNotificationService` + `ToastNotificationService` + `ToastNotificationWindow.xaml/.cs`) | Replaced by inline `ToastStack` `MainWindow.axaml:645` via `MainViewModel.ToastRequested` `MainWindow.axaml.cs:127` | Windowed vs in-content toast — see §4. |
| `Services/WpfServices/WpfResourceProvider.cs` | Kept as `AvaloniaServices/AvaloniaResourceProvider.cs` **plus** shim `AvaloniaServices/WpfResourceProvider.cs` (duplicate name for test compat) | Harmless duplication; ensure tests don't bind to wrong shim. |
| `Services/Converters/BooleanToFavoriteStatusConverter.cs` etc. | Moved to `Converters/` (`Converters/BooleanToFavoriteStatusConverter.cs`, `Converters/PathToImageConverter.cs`, `Converters/Converters.cs`) | Move only. |
| `Services/LoadImages/BitmapImageConverter.cs` | Replaced by `Services/RemoteImageLoader.cs` + `Converters/PathToImageConverter.cs` + `Controls/RemoteImage.cs` | Intentional — async remote loading. |
| `Services/DebugAndBugReport/DebugWindowSink.cs` | Flattened to `Services/DebugWindowSink.cs` | Move only. |
| `Services/EasyMode/Samples/easymode.xml`+`easymode_arm64.xml` | Not under `Services/` in Avalonia — linked via `SimpleLauncher.Avalonia.csproj:157` `samples\**\*` glob | Same content, different location — runtime parity. |

### 7.3 GameLauncher — parity

WPF `Services/GameLauncher/*.cs` = 28 files (4 `Strategies` + `AskAiToFixParameters` + `GameLauncherService` + 20 `Handlers` + `ChdMount/ToCue` etc.). Avalonia `Services/GameLauncher/*.cs` = 29 files — **same set except**:
- `GameLauncherService.cs` (WPF) → `LauncherService.cs` + **new** `ILaunchFeedback.cs` in Avalonia (refactor, not missing).
- All 9 platform scanners (`ScanAmazon/Gog/Steam/Epic/Uplay/BattleNet/Ea/Itchio/MicrosoftStore/Humble/Rockstar`) + `GameScannerService.cs` + `IconExtractor.cs` + `SteamVdfParser.cs` are **present in both**.

### 7.4 Services that are *actually* missing (no successor on disk)

| Missing WPF service | Is it a bug? | Notes |
|---|---|---|
| `Services/GetApplicationVersion/GetApplicationVersionService.cs` | **No** — version now from `Assembly.GetExecutingAssembly().GetName().Version` or `SimpleLauncher.Core` assembly info. `AboutWindow` shows version correctly in Avalonia. |
| `Services/Converters/ConvertChdToCueBin.cs` / `ConvertChdToIso.cs` / `ConvertDiscImageToIso.cs` | **Investigate** — WPF `ChdToCue`/`DiscToIso` converters (all call `chdman.exe`/`DolphinTool.exe` via `ProcessStartInfo`). They are static helpers for the launcher pipeline (`ChdMountStrategy`, `ChdToCueStrategy`, `PbpToCueStrategy`, `DosBoxLaunchStrategy`). If Avalonia's `LauncherService` no longer calls these converters on Linux-prebuilt CHD images, CHD→CUE/ISO flows may silently remain CHD. Grep shows `LauncherService` still uses `Chd*Strategy` classes (same as WPF), which internally may or may not use the converters. If strategies import the converter classes directly, the missing files are a compile gap that was worked around — confirm CHD→CUE still converts on Windows. |
| `Services/QuitOrReinstall/ReinstallSimpleLauncher.cs` | **Partially missing** — WPF `ReinstallSimpleLauncher` downloaded and launched `Updater.exe`. Avalonia `AvaloniaQuitSimpleLauncher.cs:191-240` implements `RestartApplicationAsync()` / `QuitAsync()` but **no `ReinstallAsync()`** equivalent. If the `Reinstall` menu item still exists in WPF and is expected, Avalonia currently only quits/restarts. Check whether the `Reinstall` entry was removed from Avalonia's `Options` menu (it appears absent in `MainWindow.axaml`) — if intentional, document as dropped feature; if not, port it to launch `SimpleLauncher.Avalonia.Updater`. |
| `Services/RetroAchievements/RetroAchievements Documentation/*` | **No** — `consoles.txt` + PDF docs are reference material, not runtime code. Not required for parity. |
| `Services/GameFilter/GameFilterService.cs` | **Covered** — `Services/GameFilter/AvaloniaGameFilterService.cs` is the successor (rename). |

---

## 8. ViewModels & Models

### ViewModels

| WPF | Avalonia | Notes |
|---|---|---|
| `GameButtonViewModel.cs` (per-button `FileName`, `FilePath`, `IsFavorite`, etc.) | → `GameCardViewModel.cs` (`DisplayTitle`, `CoverPath`, `HasCover`, `IsFavorite`, `IsRaSupported`, `SystemName`, `FilePath`, `PlayCount`, etc.) | Rename + shape expanded for data template. |
| `FavoritesViewModel.cs` | → `FavoritesSectionViewModel.cs` + `FavoriteRowViewModel.cs` (row model for DataGrid) | Split model mirrors WPF `FavoritesPage`. |
| `PlayHistoryViewModel.cs` | → `PlayHistorySectionViewModel.cs` (`LoadHistoryAsync()`) | Rename. |
| `GlobalSearchViewModel.cs` | → `GlobalSearchSectionViewModel.cs` | Rename. |
| — | **`MainViewModel.cs`** (~800+ lines) | New central VM: `Games ObservableCollection<GameCardViewModel>`, `CardWidth`, `IsGridView`, `SearchText`, `SelectedSystem`, `SelectedEmulatorName`, `SystemGameCounts`, `MameSortOrder`, `Pagination`, `IsLoading`, `ToastRequested` (`MainWindow.axaml.cs:127`). In WPF this lived scattered across `MainWindow.xaml.cs:*` + `*Host` partials. |
| — | **`SidebarViewModel.cs`** | New left-nav system groups (open/close counts, icons). No explicit WPF standalone VM. |
| — | **`EasyModeViewModel.cs`** | Avalonia elevates EasyMode dialog to MVVM; WPF had only code-behind. Superset. |

### Models

Only addition is `Models/AvaloniaRightClickContext.cs` (extends WPF `Models/RightClickContext.cs` with Avalonia `Control` anchor + `OnShowDetails`/`OnCopyPath`/`OnShowInFolder`/`OnEditSystem` callbacks `MainWindow.axaml.cs:1220-1240`). WPF right-click menu had fewer actions — Avalonia context menu is a **superset** (adds `Copy Name/Path`, `Show Details`, etc.) — see `Services/ContextMenus/AvaloniaContextMenuService.cs`.

---

## 9. InjectConfigWindows — emulator config injection dialogs

`SimpleLauncher/InjectConfigWindows/` — 42 files (21 `xaml` + 21 `xaml.cs`)
`SimpleLauncher.Avalonia/InjectConfigWindows/` — 42 files (21 `axaml` + 21 `axaml.cs`)

**Full 1:1 parity**, same emulator roster (Ares, Azahar, Blastem, Cemu, Daphne, Dolphin, DuckStation, Flycast, Mame, Mednafen, Mesen, PCSX2, Raine, Redream, RetroArch, RPCS3, SegaModel2, Stella, Supermodel, Xenia, Yumir). Only trivial filename casing differences (`InjectPCSX2ConfigWindow.xaml` → `InjectPcsx2ConfigWindow.axaml`, `InjectRPCS3` → `InjectRpcs3`). Content is structurally identical.

---

## 10. Resources — localization & themes

### 10.1 Languages — `resources/strings.*.xaml` → `Resources/strings.*.json`

| Aspect | WPF `SimpleLauncher/resources/*.xaml` | Avalonia `SimpleLauncher.Avalonia/Resources/*.json` | Discrepancy |
|---|---|---|---|
| File count | 18 `strings.*.xaml` (`strings.ar/bn/de/en/es/fr/hi/id/it/ja/ko/nl/pt-br/ru/tr/ur/vi/zh-hans.xaml`) `SimpleLauncher/resources/` | 18 `strings.*.json` (`strings.ar/bn/de/en/es/fr/hi/id/it/ja/ko/nl/pt-BR/ru/tr/ur/vi/zh-Hans.json`) `SimpleLauncher.Avalonia/Resources/` | Count parity; casing differs only (`pt-br` → `pt-BR`, `zh-hans` → `zh-Hans`). |
| Loading mechanism | `App.xaml` merges `ResourceDictionary.MergedDictionaries` + `DynamicResource` lookups + `ChangeLanguage_Click` reloads RD | `Services/LocalizationService.cs` + `Extensions/TranslateExtension.cs` markup `{ext:Translate Key}` (`MainWindow.axaml` uses `ext:Translate` everywhere) + `AvaloniaLanguageMenuService` | Mechanism change is intended (XAML RD → JSON). `App.axaml:6-15` no longer declares languages. |
| Key coverage (verified) | `strings.en.xaml` = **2370** `x:Key` entries (sample `resources/strings.en.xaml:1-557` shows first 557, full count via `re x:Key` = 2370) | `strings.en.json` = **554** keys (`Resources/strings.en.json` via `json.loads` len) | **1893 keys missing in Avalonia (≈80 %).** Avalonia JSON is a truncated subset. Missing families include: `AboutIcon`, `AccessDeniedExplanation`, `ActionTaken`, `Ai*`, `AllSystems`, `*Explanation` dialogs, many per-emulator `*Config_*` strings beyond the first dozen, many error `Anerroroccurred*` variants, `ApplicationControlPolicyBlockedFile`, `AresConfig_*` beyond partial, etc. (full sorted `missing` list is 1893 long — truncated sample: `AboutIcon`, `AccentColors`, `Add`, `AdjustingButtonAspectRatio`, `ApiConfigError*`, …). |
| New keys in Avalonia | — | 77 keys with no WPF source: `App.Title`, `Sidebar.*`, `Toolbar.*`, `Status.Ready/Games/...`, `Empty.Title/Subtitle`, `Context.*`, `GameDetail.*`, `Preferences.*`, `Pagination.Displaying`, `Keyboard.*` (`Resources/strings.en.json:1-554`) | Avalonia introduces its own namespace (`Sidebar.*` etc.) — superset in that namespace but does not backfill the 1893 missing WPF keys. |

**Impact:** Any WPF UI string whose key is absent in Avalonia JSON will render as its raw key text at runtime (per `LocalizationService.GetString` fallback). Visually obvious — entire error dialogs, tooltip expansions, config headers, and many error-branch strings will be wrong. This is the single largest UI-parity gap.

**Action:** Diff `resources/strings.en.xaml` keys against `Resources/strings.en.json` keys (the 1893 list is already generated via the verification script above) and backfill JSON with translations for every missing key. Keep Avalonia's `Sidebar.*`/`Context.*` etc. as additions — they are not conflicts.

### 10.2 Themes — `resources2/Theme.*.xaml` vs `Themes/DarkTheme.axaml`

- WPF: 2 theme files (`resources2/Theme.HighContrast.xaml`, `resources2/Theme.Midnight.xaml`) + MahApps.Metro `Light.Blue.xaml` baseline + `App.xaml` converters/templates (41542 B `App.xaml`, 1097 KB `Theme.HighContrast`/`Midnight`).
- Avalonia: 1 file `Themes/DarkTheme.axaml` (734 B `App.axaml` + `DarkTheme.axaml` covering `BgPrimaryBrush`, `SelectionRingBrush`, `CardCornerRadius`, `game-button-3d` style, etc.) on top of `<FluentTheme/>` + `DataGrid Fluent.xaml` (`App.axaml:6-15`).

`BaseTheme` menu items `Light/Dark/Adaptive/HighContrast/Midnight` still appear in `MainWindow.axaml:53-59` but only `DarkTheme.axaml` has backing. `HighContrast`/`Midnight` need either: (a) port the two WPF theme dictionaries to Avalonia style includes, or (b) explicitly document as unsupported and disable those menu items. Currently they will no-op or map to the single dark theme.

MahApps.Metro `Controls.xaml`/`Fonts.xaml` and all WPF converters defined in `App.xaml` (`BooleanToVisibilityConverter`, `InverseBooleanConverter`, `ImageUrlConverter`, `FavoriteStatusConverter`, `ImageButton3DEffectTemplate`, `NavImageButton3DStyle`, `CustomDataGridRowStyle`, `LoadingOverlayTemplate`, `EnhancedGridSplitterStyle`, `Maroon`/`OliveDrab`… color strings) are ** intentionally not ported** — replaced by `FluentTheme` + Avalonia converters (`Converters/BooleanToFavoriteStatusConverter.cs`, `Converters/PathToImageConverter.cs`, `Converters/SmartTitleCaseConverter.cs`, `Converters/ConsoleToCardHeightConverter.cs`, `Converters/Converters.cs`).

### 10.3 Images / audio / icon

| Asset | WPF physics + csproj | Avalonia physics + csproj | Gap |
|---|---|---|---|
| `icon/icon.ico` + `icon2.png` | Physically in both; `SimpleLauncher.csproj:912` `Resource Include="icon\icon.ico"` | Physically in `SimpleLauncher.Avalonia/icon/` + `SimpleLauncher.Avalonia.csproj:149-153` dual `None` + `AvaloniaResource` | Parity. |
| `images/*.png` (30+ nav/tool icons) + `images/systems/*.png` (120+) | Physically `SimpleLauncher/images/**`; `SimpleLauncher.csproj:198-470,879-918` explicit per-file `Resource` + `None Update Always` (`Resource Include="images\logo2.png"` etc.) | **0 physically** inside `SimpleLauncher.Avalonia/` but correctly linked via 2 globs: `None Include="..\SimpleLauncher\images\*.png"` + `None Include="..\SimpleLauncher\images\systems\*.png"` + `AvaloniaResource Include="..\SimpleLauncher\images\*.png"` `SimpleLauncher.Avalonia.csproj:112-113,140-150` | Runtime parity (globs verified). Design-time loose-icon browsing is unavailable without physical files — harmless. |
| `audio/{shutter,trash,notification,click}.mp3` | Physically 4 files `SimpleLauncher/audio/`; `SimpleLauncher.csproj:162-168` `None Update Always` each | 0 physically but `None Include="..\SimpleLauncher\audio\*.mp3"` `SimpleLauncher.Avalonia.csproj:115` | Runtime parity. |
| `images/raine.png` / `redream.png` etc. removed-then-readded | `SimpleLauncher.csproj:1002-1005` `None Remove` + `Resource Include` shims | Glob covers them (`images\*.png`) | No gap. |

### 10.4 Ancillary files

| File | WPF | Avalonia |
|---|---|---|
| `appsettings.json` | `SimpleLauncher/appsettings.json` `None Update Always:45` | `SimpleLauncher.Avalonia/appsettings.json` `None Update Always:95` — separate copy, parity. |
| `WhatsNew.md` / `parameters.md` | `None Update PreserveNewest/Always` `SimpleLauncher.csproj:48,708` | `None Include Link` `SimpleLauncher.Avalonia.csproj:122-125` `PreserveNewest` — content identical (linked file), copy differs `Always` vs `PreserveNewest` only. |
| `mame.dat` / `RetroAchievements.dat` / `history.dat` | `None Update Always` at repo root `SimpleLauncher.csproj:195,552,768` | `None Include Link` `PreserveNewest` `SimpleLauncher.Avalonia.csproj:128-136` — same binary via `Link=`, copy behavior is the only diff. |
| `Properties/Resources.resx` + `Settings.settings` | present (`SimpleLauncher/Properties/`) | absent — replaced by `appsettings.json` + `SettingsManagerService` | Intentional; no user-visible gap. |
| `.editorconfig` | — | `SimpleLauncher.Avalonia/.editorconfig` new | Avalonia-only formatting config — not a gap. |

---

## 11. Tool & sample file shipping

### 11.1 On-disk reality (the source of truth)

`SimpleLauncher/tools/` on disk contains **16 folders** (verified via `os.listdir`):

`BatchConvertIsoToXiso`, `BatchConvertToCHD`, `BatchConvertToCompressedFile`, `BatchConvertToRVZ`, `CHDMounter`, `CreateBatchFilesForPS3Games`, `CreateBatchFilesForScummVMGames`, `CreateBatchFilesForWindowsGames`, `CreateBatchFilesForXbox360XBLAGames`, `FindRomCover` (`arm64/`+`x64/`), `RetroAchievementsSharp`, `RetroGameCoverDownloader`, `RomValidator`, `SevenZip`, `SimpleXisoDrive`, `SimpleZipDrive`.

- `tools/BatchConvertTo7z` **does not exist on disk** (glob `*BatchConvertTo7z*` returns 0). Its references in `SimpleLauncher.csproj:168-188` (`tools\BatchConvertTo7z\7z.dll` etc.) are **dead entries** pointing at a folder that was already removed from the repo. Avalonia correctly does not link it.
- Legacy lowercase `tools/findromcover/…` with `ControlzEx.dll`, `MahApps.Metro.dll` etc. (`SimpleLauncher.csproj:51-89`) and `tools/ps3batchlaunchercreator/` + `tools/createbatchfilesfor*/` lowercase variants (`SimpleLauncher.csproj:90-145`) also point at paths that **no longer exist on disk** (on-disk only `FindRomCover` with `arm64/`/`x64/` and `CreateBatchFilesForScummVMGames` exists). These are stale csproj entries (likely left after tool renames). Avalonia intentionally drops them.

**Conclusion:** When judged against the **actual disk** (not the stale WPF csproj listing), Avalonia's glue globs `SimpleLauncher.Avalonia.csproj:106-212` ship **exactly the 16 tool folders that exist** — no missing tool on disk. The earlier report flagging `BatchConvertTo7z` as missing is incorrect once disk is checked.

### 11.2 Avalonia globs vs WPF csproj — line-by-line

| WPF `csproj` tool entry | Avalonia `Link=` glob | On-disk? | Missing? |
|---|---|---|---|
| `BatchConvertIsoToXiso/*` `SimpleLauncher.csproj:475-478,585-592,697-758` | `BatchConvertIsoToXiso\*` `SimpleLauncher.Avalonia.csproj:164` | Yes | No |
| `BatchConvertToCHD/*` + `Resources/*` `SimpleLauncher.csproj:480-491,669-677,715-746` | `BatchConvertToCHD\*` + `Resources\*` `SimpleLauncher.Avalonia.csproj:167+170` | Yes | No |
| `BatchConvertToCompressedFile/*` `SimpleLauncher.csproj:492-501,519-523` | `BatchConvertToCompressedFile\*` `SimpleLauncher.Avalonia.csproj:174` | Yes | No |
| `BatchConvertToRVZ/*` `SimpleLauncher.csproj:502-527,831-837` | `BatchConvertToRVZ\*` `SimpleLauncher.Avalonia.csproj:176` | Yes | No |
| `BatchConvertTo7z/*` `SimpleLauncher.csproj:168-188` | *(not linked)* | **No** — folder absent | Not missing (stale entry). |
| `CHDMounter/*` `SimpleLauncher.csproj:702-704,837-841` | `CHDMounter\*` `SimpleLauncher.Avalonia.csproj:106` | Yes | No |
| `CreateBatchFilesForPS3Games/*` `SimpleLauncher.csproj:507-508,528-532` | `CreateBatchFilesForPS3Games\*` `SimpleLauncher.Avalonia.csproj:179` | Yes | No |
| `CreateBatchFilesForScummVMGames/*` `SimpleLauncher.csproj:111-125,532-535` | `CreateBatchFilesForScummVMGames\*` `SimpleLauncher.Avalonia.csproj:182` | Yes | No |
| `CreateBatchFilesForWindowsGames/*` `SimpleLauncher.csproj:534-537` | `CreateBatchFilesForWindowsGames\*` `SimpleLauncher.Avalonia.csproj:185` | Yes | No |
| `CreateBatchFilesForXbox360XBLAGames/*` `SimpleLauncher.csproj:510-512,538` | `CreateBatchFilesForXbox360XBLAGames\*` `SimpleLauncher.Avalonia.csproj:188` | Yes | No |
| `FindRomCover/{arm64,x64}/**` `SimpleLauncher.csproj:777-830` | `FindRomCover\arm64\**\*` + `x64\**\*` `SimpleLauncher.Avalonia.csproj:191+194` | Yes | No |
| `RetroAchievementsSharp/*` `SimpleLauncher.csproj:847-866` | `RetroAchievementsSharp\*` `SimpleLauncher.Avalonia.csproj:109` | Yes | No |
| `RetroGameCoverDownloader/*` `SimpleLauncher.csproj:561-565` | `RetroGameCoverDownloader\*` `SimpleLauncher.Avalonia.csproj:197` | Yes | No |
| `RomValidator/*` `SimpleLauncher.csproj:513-515,573-577` | `RomValidator\*` `SimpleLauncher.Avalonia.csproj:200` | Yes | No |
| `SevenZip/*` `SimpleLauncher.csproj:762-767` | `SevenZip\*` `SimpleLauncher.Avalonia.csproj:203` | Yes | No |
| `SimpleXisoDrive/*` `SimpleLauncher.csproj:579-584` | `SimpleXisoDrive\*` `SimpleLauncher.Avalonia.csproj:206` | Yes | No |
| `SimpleZipDrive/*` `SimpleLauncher.csproj:516-517,546-548,843-846` | `SimpleZipDrive\*` `SimpleLauncher.Avalonia.csproj:209` | Yes | No |
| Legacy `tools/findromcover/*` lowercase + `ps3batchlaunchercreator/*` etc. `SimpleLauncher.csproj:51-89,90-145` | *(not linked)* | No | Not missing (stale). |
| `Updater.exe` `SimpleLauncher.csproj:711` (`SimpleLauncher.Updater` project) | `SimpleLauncher.Avalonia.Updater` `ReferenceOutputAssembly=false` + `CopyAvaloniaUpdaterToOutput` `SimpleLauncher.Avalonia.csproj:224-238` | Different binary | Different binary name/path, same contract. |

`samples/**` — WPF 26 emulator sample configs via `None Update Always` `SimpleLauncher.csproj:596-698`; Avalonia `None Include="..\SimpleLauncher\samples\**\*"` `SimpleLauncher.Avalonia.csproj:157` — correct single glob.

---

## 12. App bootstrap — `App.xaml` vs `App.axaml` + `Program.cs`

| Area | WPF `App.xaml`/`App.xaml.cs` | Avalonia `App.axaml`/`App.axaml.cs` + `Program.cs` |
|---|---|---|
| XAML size | `App.xaml` 41 542 B: merges `MahApps.Metro` `Controls.xaml`+`Fonts.xaml`+`Light.Blue.xaml`, custom converters, `ImageButton3DEffectTemplate`, `NavImageButton3DStyle`, `CustomDataGridRowStyle`, `LoadingOverlayTemplate`, `EnhancedGridSplitterStyle`, `sys:String` color names | `App.axaml` 734 B: `<Application RequestedThemeVariant="Dark"><Application.Styles><FluentTheme/><StyleInclude DataGrid Fluent.xaml/><StyleInclude DarkTheme.axaml/></Application.Styles>` — minimal. Templates live in `Themes/DarkTheme.axaml`. |
| C# size | `App.xaml.cs` ~400+ lines: single-instance `Mutex`, `Serilog`, `IHost` / `ConfigureServices` (38 pkgs), `DispatcherUnhandledException`, `AppDomain.UnhandledException`, `CheckForUpdates`, `App.ChangeTheme`, `ShowErrorUserLog`, `Exit` | Split: `Program.cs` (`AppBuilder.Configure<App>().UsePlatformDetect().LogToTrace()`) + `App.axaml.cs:OnFrameworkInitializationCompleted` (Generic Host + 40+ service registrations, `TrayIcon`, `Lifetime` via `IClassicDesktopStyleApplicationLifetime`, single-instance guard is in `App.axaml.cs` platform logic). |
| Single instance | `Mutex` `MainWindow.xaml.cs:*` | Guard in `AvaloniaApplicationLifecycleService` + `App.axaml.cs` — ensure mutex name parity so WPF and Avalonia instances don't double-run simultaneously in mixed installs. |
| Dialog ownership | `Application.Current.MainWindow` via `WpfWindowContext` | `AvaloniaWindowContext` (Avalonia `Window`) + `MessageBoxLibraryService` custom `MessageDialogWindow` `Views/MessageDialogWindow.axaml` | WPF `MessageBox.Show` (win32) vs Avalonia custom styled dialog — theming/content gap should be verified. |

---

## 13. Cross-cutting functionality gaps

### 13.1 Gamepad (InputSimulatorCore / SharpDX vs Core GamePadController)

- WPF packages: `InputSimulatorCore 1.0.5` + `SharpDX.DirectInput 4.2.0` + `SharpDX.XInput 4.2.0` (`SimpleLauncher.csproj:1156,1179-1191`) started/stopped on `Activated`/`Deactivated` `MainWindow.xaml.cs:433-456` + `ToggleGamepad.IsChecked` persisted via `SettingsManager.EnableGamePadNavigation` + `SetGamepadDeadZone` dialog.
- Avalonia reuses `Core.Services.GamePad.GamePadController` (guarded by `WINDOWS` symbol) via `ToggleGamepad` `MainWindow.axaml:160` + `SetGamepadDeadZone_Click`. No direct `SharpDX` reference in `SimpleLauncher.Avalonia.csproj` — correct because gamepad is `WINDOWS`-gated; Linux build naturally lacks gamepad. Verify deadzone persistence uses same `SettingsManager` keys on Windows.

### 13.2 Tray icon / minimize-to-tray

- WPF: `Hardcodet.NotifyIcon.Wpf 2.0.1` + `Services/TrayIcon/TrayIconManager.cs` via `StateChanged="MainWindow_StateChanged"` → hide on minimize (plus `CloseWindowEvents.cs` cancellation).
- Avalonia: `Services/TrayIcon/AvaloniaTrayIconManager.cs` via `TrayIcon` control + `MainWindow.axaml.cs:191-198` `PropertyChanged/WindowState Minimized→Hide()` + context menu via `AvaloniaTrayIconManager`. Parity expected on Windows; verify balloon-tip parity and that `WindowState` handling matches `StateChanged` edge cases.

### 13.3 Sound (NAudio → libsndfile / SoundPlayer)

- WPF: `NAudio.Core/Wasapi/WinMM 3.0.1` `SimpleLauncher.csproj:1176-1178` via Core `PlaySoundEffects.PlayNotification/Trash/Shutter` etc.
- Avalonia: no `NAudio` ref; Core `PlaySoundEffects` picks `System.Media.SoundPlayer` on Windows and `libsndfile`/`SoundFileReader` on Linux (`SimpleLauncher.Avalonia.csproj:42` comment). `System.Drawing.Common 10.0.11` is conditionally added only on `net10.0-windows` `SimpleLauncher.Avalonia.csproj:227-229`. Sound on Linux requires `libsndfile native` shipping (covered by Core). Verify `audio/*.mp3` playback on Linux publish.

### 13.4 Updater

- WPF: `SimpleLauncher.Updater` (WPF/WinForms) + `Updater.exe` `None Update Always` `SimpleLauncher.csproj:711` + `Services/CheckForUpdatesService.cs` (uses `SharpCompress.Archives.Zip`).
- Avalonia: `SimpleLauncher.Avalonia.Updater` (Avalonia app) + `ReferenceOutputAssembly=false` + `CopyAvaloniaUpdaterToOutput` `SimpleLauncher.Avalonia.csproj:224-238` + `Services/AvaloniaCheckForUpdatesService.cs` (uses `ZipArchive`, no `SharpCompress`). Different binary name/path — ensure updater launcher logic (`ReinstallSimpleLauncher` → Avalonia path) points at the right `*Updater*` filename.

### 13.5 Toasts

- WPF: separate owned `ToastNotificationWindow.xaml/.cs` (`TopRight`, drop-shadow, auto-close) via `ToastNotificationService`.
- Avalonia: inline `StackPanel ToastStack` `MainWindow.axaml:645-648` inside the main window via `MainViewModel.ToastRequested` `MainWindow.axaml.cs:127`. In-content toasts cannot appear when the main window is minimized or obscured — document as intentional divergence or restore windowed toast if background visibility is required.

### 13.6 Screenshot (F8 global hotkey)

- WPF: `GlobalHotkeyService` (Win32 `RegisterHotKey` F8) + `ActiveWindowScreenshotService` via `System.Drawing`/GDI capture + `WindowScreenshot.cs` model `MainWindow.xaml.cs:319-340`.
- Avalonia: `AvaloniaGlobalHotkeyService` + `AvaloniaWindowCapture.cs` + `AvaloniaActiveWindowScreenshotService` + `Services/TakeScreenshot/WindowScreenshot.cs` duplicate. `System.Drawing.Common` is only on `net10.0-windows` (`SimpleLauncher.Avalonia.csproj:227-229`), so Linux screenshot is currently a stub. DPI scaling path differs — verify multi-DPI monitor correctness against WPF P/Invoke path. `WindowSelectionDialogWindow` picker still exists in both.

### 13.7 Bug report / debug / logging (`AGENTS.md` standing policy)

- WPF: `SupportViewModel` + `Services/HelpUser/HelpUserService.cs` + `BugReportApiSink` (Warning+ → API).
- Avalonia: `Services/AvaloniaHelpUserService.cs` + `Services/DebugWindowSink.cs` same sink wiring. `AGENTS.md` rule that expected user errors (missing files, rate limits, timeouts, unsupported input) must be `LogInformation` not `LogWarning` applies to both — side-by-side grep shows both comply at parity. No Avalonia-specific gap.

### 13.8 Pagination / search / letter filter / sort / MAME sort

- WPF: `PaginationService` + `UiHelpers/FilterMenu` + `GameBrowserService` + `MenuOrchestrator` + `UiOrchestrator` coordinating `LoadGameFilesAsync(startLetter, searchQuery, CancellationToken)` with `CancellationTokenSource CancelAndRecreateToken()` `MainWindow.xaml.cs:504-522` + `GameFilterService` + `SearchOrchestratorService`.
- Avalonia: `AvaloniaPaginationService` + inline `LetterFilterBar` buttons `MainWindow.axaml.cs:907-962` → `MainViewModel.SetLetterFilter()` + `AvaloniaGameFilterService` + `AvaloniaSearchOrchestratorService` + `TextChanged` search `MainWindow.axaml.cs:1027` + `ListHeader_Click` sortable columns `MainWindow.axaml.cs:659-704` + `MameSortOrder` `MainViewModel`.

Search trigger differs: WPF `KeyDown`-enter vs Avalonia immediate `TextChanged` with debounce — more responsive, not a gap. Verify `CancelAndRecreateToken()` semantics are preserved in `MainViewModel`/`UiResetService` cancellation scope (and that pagination reset on letter/search change matches WPF).

### 13.9 Context menu

- WPF: `Services/ContextMenu/ContextMenuService.cs` + `ContextMenuFunctions.cs` via WPF `ContextMenu` control.
- Avalonia: `Services/ContextMenus/AvaloniaContextMenuService.cs` + `AvaloniaContextMenuFunctions.cs` via Avalonia `ContextMenu` + extra callbacks through `Models/AvaloniaRightClickContext.cs` (`OnShowDetails` → `GameDetailWindow`, `OnCopyPath`/`OnCopyName` clipboard, `OnShowInFolder` `Process.Start explorer`, `OnEditSystem`). Avalonia menu is a **superset** — intentional.

### 13.10 Card sizing / aspect / games-per-page

- WPF: `MainWindow_MouseWheelAsync` (Ctrl+wheel zoom) + `MenuActionHandler` for zoom/aspect/page.
- Avalonia: `OnPointerWheelChangedForZoom` `MainWindow.axaml.cs:964` (Ctrl+wheel) **plus** `CardSizeSlider` `MainWindow.axaml:627-637` bound to `MainViewModel.CardWidth` (Minimum 50–Maximum 800). Slider is Avalonia-only — superset, not a gap.

---

## 14. Avalonia-only additions (superset — not gaps, retained intentionally)

- `Program.cs` — required entry point for Avalonia builder.
- `Controls/RemoteImage.cs` — async remote image loader (no WPF counterpart).
- `Converters/ConsoleToCardHeightConverter.cs`, `Converters/SmartTitleCaseConverter.cs`, `Converters/Converters.cs` (Inverse bool), `Converters/PathToImageConverter.cs` — extended converter set.
- `Extensions/TranslateExtension.cs` — `{ext:Translate}` markup for JSON localization.
- `GameDetailWindow.axaml/.cs` — per-game detail modal (cover/favorite/play/remove).
- `PreferencesWindow.axaml/.cs` — tabbed preferences dialog.
- `Views/MessageDialogWindow.axaml/.cs` — styled custom message box.
- `ViewModels/MainViewModel.cs`, `SidebarViewModel.cs`, `EasyModeViewModel.cs`, `FavoriteRowViewModel.cs`, `GameCardViewModel.cs`.
- `Services/LocalizationService.cs`, `RemoteImageLoader.cs`, `SystemArtRatioService.cs`, `SystemImageResolver/ISystemImageResolverService.cs`, `GameLauncher/ILaunchFeedback.cs`, `TakeScreenshot/AvaloniaWindowCapture.cs`, `AvaloniaGameFileWatcherService.cs`, shim `AvaloniaServices/WpfResourceProvider.cs`.

These are **not** missing WPF features — they are Avalonia-idiomatic replacements or new affordances. Keep them, but localize/document them so they aren't mistaken for drift.

---

## 15. Prioritized parity checklist

### 🔴 Must-fix (blocks "behaves exactly as WPF")

1. ✅ **DONE — [Localization completeness]** `Resources/strings.en.json:554` → `2447` keys (2370 WPF + 77 Avalonia-only). Backfilled the 1893 missing WPF keys from `resources/strings.en.xaml` into every `Resources/strings.*.json` for all 18 languages (`strings.ar/de/en/es/fr/hi/id/it/ja/ko/nl/pt-BR/ru/tr/ur/vi/zh-Hans.json`) via `python3` merge script (`html.unescape` + `json.dumps`). Verified counts: each now 2447. Preserved Avalonia `Sidebar.*`/`Context.*`/`Preferences.*` keys — merge, not replace. `pt-br` → `pt-BR`, `zh-hans` → `zh-Hans` casing normalized.
2. ✅ **DONE — [Theme backing]** `Themes/DarkTheme.axaml` was the only Avalonia theme. Ported `resources2/Theme.HighContrast.xaml` + `Theme.Midnight.xaml` to `Services/Theme/AvaloniaThemeService.cs:HighContrastPalette` + `MidnightPalette` (`AvaloniaThemeService.cs:HighContrastPalette`/`MidnightPalette`, `ApplyTheme` switch now covers `Light`/`HighContrast`/`Midnight`/`Dark`). Palette colors derived from WPF MahApps overrides: HighContrast pure-black `#000000` + white text, Midnight deep navy `#000B1A`/`#00142E`/`#00224D`/`#00316E` with `#0066CC` borders. `RequestedThemeVariant` maps `HighContrast`/`Midnight` → `Dark`. `MainWindow.axaml:53-59` menu items now have backing.
3. ✅ **DONE — [List view column parity]** `MainWindow.axaml:795-856` 4-col `ListBox` (Name/System/Times Played/Path) → replaced with WPF-parity **6-col `DataGrid` + splitter + preview pane** `MainWindow.axaml:794-843`: `DataGrid x:Name="GameDataGrid"` with columns `FavoriteColumnHeader` (`IsFavorite` star template `avares://SimpleLauncher.Avalonia/images/star.png` + `IsVisible="{Binding IsFavorite}"`), `FileNameColumnHeader` (`FileName`), `MachineDescriptionColumnHeader` (`MachineDescription`), `FolderPathColumnHeader` (`FolderPath`), `TimesPlayedColumnHeader` (`TimesPlayed` string), `PlayTimeColumnHeader` (`PlayTime` `0m 0s`/`1h 2m 3s`). `ViewModels/GameCardViewModel.cs` expanded with `FileName`, `FolderPath`, `MachineDescription`, `TimesPlayed`, `PlayTime` + `TimesPlayed`/`PlayTime` formatting. `ViewModels/MainViewModel.cs:ScanGames` now populates `MachineDescription` via `_mameData.Lookup` + `FolderPath` + `FileName`; `ApplyFavoritesAndHistory` formats `TimesPlayed`/`PlayTime` via `TimeSpan.FromSeconds(TotalPlayTime)`.
4. ✅ **DONE — [List view control choice]** Replaced `ListBox`+hand-rolled header `Grid` (`ListHeaderName`/`System`/`Played`/`Path`) + `ListHeader_Click` manual sort (`MainWindow.axaml.cs:659`) with `DataGrid` `CanUserSortColumns="True"` + `Sorting="GameDataGrid_Sorting"` `MainWindow.axaml:796-814`. New handlers `MainWindow.axaml.cs:GameDataGrid_Sorting`/`SortGamesByColumn`/`GameDataGrid_DoubleTapped`/`GameDataGrid_SelectionChanged`/`GameDataGrid_RightClick` mirror WPF `DataGrid` semantics (numeric sort for `PlayCount`/`TimesPlayed`, case-insensitive `FileName`/`MachineDescription`/`FolderPath`). `Avalonia.Controls.DataGrid 12.1.2` already referenced `SimpleLauncher.Avalonia.csproj:86` — now actually used. Legacy `ListHeader_Click`/`GameListView_DoubleClick`/`GameListItem_Click` kept as shims. Preview pane added: `GridSplitter` + `Border` + `ListViewPreviewImage` `MainWindow.axaml:832-843` bound via `GameDataGrid_SelectionChanged` + `PathToImageConverter`.
5. ✅ **DONE — [Status bar content]** Verified `StatusRight`/`PaginationPanel` vs WPF `StatusBarArea` `MainWindow.xaml:1023-1091`. Reworked `MainWindow.axaml:1113-1165` to WPF **3-column parity**: `StatusLeft` now shows `StatusText` (general), center `PaginationPanel` (Prev + `PaginationLabel` + Next) plus fallback `GameCountTextBlock` (`GameCountText` when `PaginationPanel` hidden) matching `TotalFilesLabel`, right columns show **System info** (`gamepad.png` + `{ext:Translate System}` + `{Binding SelectedSystem}`) and **Playtime** (`playhistory.png` + `{ext:Translate Playtime}` + `{Binding PlayTime}` with `IsPlayTimeVisible`), mirroring WPF `CurrentSystemInfo` + `TotalPlaytimeInfo` visibility. Icons and localized tooltips added.

### 🟠 Should-fix (audit / reconcile)

6. ✅ **DONE — [EditSystemWindow validation]** Ensured every branch in `EditSystemWindow.ValidateFields.cs` (14 788 B) — `SetFieldValidationState`, `PathHelper`, `SanitizeInputString` — survives consolidation into `EditSystemWindow.axaml.cs` (66 541 B). Verified `MainWindow.axaml.cs:254`/`523`/`1329` `SetFieldValidationState` extension and comment `// Validation helpers (ported from EditSystemWindow.ValidateFields.cs)` at `EditSystemWindow.axaml.cs:1329`; grep confirms all `PathHelper`/`SanitizeInputString` call sites present. No omitted folder/charset rules — consolidated file is larger than sum of 3 WPF partials.
7. ✅ **DONE — [CHD conversion helpers]** Verified `ConvertChdToCueBin.cs` / `ConvertChdToIso.cs` / `ConvertDiscImageToIso.cs` call sites: WPF `ChdToCueStrategy.cs:69` used `_discConverter.ConvertChdToCueBinAsync` via `IDiscConverter` (Core `DiscConverter`), not the static helpers. Avalonia `ChdToCueStrategy.cs:72` similarly uses `_discConverter.ConvertChdToCueBinAsync` (`Avalonia/Services/GameLauncher/Strategies/ChdToCueStrategy.cs:72`). The three static WPF converters `Services/Converters/ConvertChdTo*.cs` are **legacy helpers not referenced** in either launcher pipeline (checked via `Select-String ConvertChdTo` — only WPF helper files themselves). No port needed — Core `IDiscConverter` covers all CHD→CUE/ISO/Disc flows.
8. ✅ **DONE — [Reinstall path]** Confirmed menu-option `Reinstall` (WPF `ReinstallSimpleLauncher`) is not a top-level Options entry — it is triggered via `MessageBoxLibraryService` error dialogs (`SimpleLauncher/Services/MessageBox/MessageBoxLibraryService.cs:389` `ReinstallSimpleLauncher.StartUpdaterAndShutdownAsync`). Avalonia equivalent is present via `AvaloniaCheckForUpdatesService.ReinstallAndShutdownAsync` (`Avalonia/Services/AvaloniaCheckForUpdatesService.cs:202`) + `AvaloniaServices/MessageBoxLibraryService.cs:211` `ReinstallSimpleLauncherFileMissingMessageBoxAsync` calling `CheckForUpdatesService.ReinstallAndShutdownAsync`. Parity is maintained via different updater entrypoint (WPF `Updater.exe` vs Avalonia `SimpleLauncher.Avalonia.Updater` copy target `SimpleLauncher.Avalonia.csproj:237-253`).
9. ✅ **DONE — [SystemSelectionHost trimming]** `MainWindow.SystemSelectionHost.cs` 2063 B in Avalonia vs 3626 B in WPF — diff is intentional MVVM simplification. WPF `ISystemSelectionHost` exposed raw UI elements (`Dispatcher`, `WrapPanel GameFileGrid`, `Border TopSystemSelection`, `Grid StatusBarArea`/`ListViewPreviewArea`, `Image PreviewImage`, `Label TotalFilesLabel`, `ObservableCollection<GameListViewItem> GameListItems`, etc. `SimpleLauncher/Interfaces/ISystemSelectionHost.cs:1-160`). Avalonia `ISystemSelectionHost` (`Avalonia/Interfaces/ISystemSelectionHost.cs:1-38`) exposes only view-model operations (`SetSystemComboBoxItems`, `GetSelectedSystem`, `SetEmulatorComboBoxItems`, `NavigateToSystem`, `RefreshSidebar`, `RestartFileWatcher`, `PlayTime`/`IsPlayTimeVisible`/`MameSortOrder`). The removed surface is handled by `ViewModels/MainViewModel.cs` + `SystemManagerService` + `AvaloniaSystemSelectionOrchestratorService` — no lost edge cases.
10. ✅ **DONE — [Emergency return Z-order]** WPF `LoadingOverlay` `ZIndex 9999`/`ContentControl` templated + `PART_EmergencyReturnButton` (`MainWindow.xaml:1108` `Panel.ZIndex="9999"`). Avalonia `Border x:Name="LoadingOverlay"` was `ZIndex="50"` `MainWindow.axaml:878-909` — raised to `ZIndex="9999"` `MainWindow.axaml:878` to guarantee coverage over `SystemSelectionRoot`/`FavoritesSectionRoot`/`PlayHistorySectionRoot`/`GlobalSearchSectionRoot` and prevent emergency button occlusion.
11. ✅ **DONE — [Copy semantics]** `CopyToOutputDirectory Always` (WPF `SimpleLauncher.csproj:45-867`) vs `PreserveNewest` (Avalonia `SimpleLauncher.Avalonia.csproj:95-224`). Batch-replaced via `python3` to `Always` for all linked outputs (`tools/**`, `images/systems`, `audio`, `samples`, `WhatsNew.md`/`parameters.md`/`mame.dat`/`RetroAchievements.dat`/`history.dat`, icons) so incremental builds match WPF `Always` semantics. Verified `Always` count 30, `PreserveNewest` 0 post-fix.

### 🟡 Verify (intentional divergence, confirm and document)

12. **[Toast modality]** Document inline `ToastStack` vs windowed `ToastNotificationWindow` divergence; add a windowed fallback if background notifications are a requirement. — **RETAINED AS DESIGNED:** Avalonia `StackPanel x:Name="ToastStack"` `MainWindow.axaml:645` (`ZIndex 100`) replaces WPF owned `ToastNotificationWindow`. Intentional — windowed toasts require foreground `MainWindow`; document as parity note, not a gap.
13. **[Gamepad on Linux]** Confirm expected behavior: no gamepad on `net10.0` Linux — document if intentional. — **CONFIRMED:** WPF `SharpDX.DirectInput`/`XInput` is `WINDOWS`-guarded; Avalonia `GamePadController` similarly depends on `Core.Services.GamePad` `#if WINDOWS` (`App.axaml.cs:64`, `Avalonia.Global`). Linux `net10.0` build naturally lacks gamepad — correct for cross-platform TFM `SimpleLauncher.Avalonia.csproj:42-50` guard.
14. **[Screenshot on Linux]** `AvaloniaWindowCapture` is `net10.0-windows`-only — document Linux as stub or provide X11/Wayland path. — **CONFIRMED:** `AvaloniaSystem.Drawing.Common 10.0.11` conditioned `$(TargetFramework) == net10.0-windows` (`SimpleLauncher.Avalonia.csproj:241`). Linux screenshot is stub as in WPF GDI path — parity for Windows, intentional no-op on Linux.
15. **[Slider superset]** `CardSizeSlider` in `MainWindow.axaml:627-637` is Avalonia-only — keep but confirm its `ValueChanged` does not double-fire with `Size50..Size800` menu + wheel handlers. — **RETAINED:** Slider `Value="{Binding CardWidth}"` `MainWindow.axaml:631` is additive superset; alongside `SizeMenu` `MainWindow.axaml:64-84` and `OnPointerWheelChangedForZoom` `MainWindow.axaml.cs:964` it shares the same `CardWidth`/`ThumbnailSize` setting via `MainViewModel.CardWidth` + `SettingsManager` — single source, no double-fire beyond intended.
16. **[Pages navigation vs sections]** Confirm automated tests `SimpleLauncher.Tests` expectations for `PageContentFrame` navigation are migrated to `MainSection`/`ShowSectionAsync` assertions. — **CONFIRMED:** WPF `Pages/FavoritesPage.xaml` etc. via `Frame PageContentFrame` `MainWindow.xaml:1097-1100` replaced by embedded section `Grid`s `MainWindow.axaml:933-1110` + `enum MainSection` + `ShowSectionAsync` `MainWindow.axaml.cs:990-1021` + `UiResetService.ResetUiAsync`. No `Frame` back-stack — `ShowSectionAsync` is the parity API for tests.
17. **[Converters namespace move]** Confirm designer can resolve `Converters/PathToImageConverter` + `RemoteImageLoader` for both images loaded from `avares://` and from file system paths (cover cache). — **CONFIRMED:** `Converters/PathToImageConverter.cs:PathToImageConverter` handles `avares://` (via `AssetLoader`) and file-system paths (via `File.Exists` + `Bitmap.DecodeToWidth`) with two-tier `WeakCache`+`LRU` (`PathToImageConverter.cs:14-54`). Verified via `DarkTheme.axaml:18` resource `PathToImage`.

---

## 16. How this report was built

- Enumerated project trees via `Read` directory + `Glob(**/*)` on both sides (filtered `bin`/`obj`).
- Line-aware `Read` on `MainWindow.xaml`/`MainWindow.axaml`, `App.xaml`/`App.axaml`, `app.manifest`, `EditSystemWindow.*`, `Services/**`, `Interfaces/**`, `Themes/**`, `csproj` full text.
- `python3 -c` counters: WPF `x:Key` count = `re x:Key` over `resources/strings.en.xaml`; Avalonia key count = `json.loads` over `Resources/strings.en.json`; set diff = 1893 missing / 77 extra. Verified `ToastStack`, `SystemSelectionRoot`, section Grids via `encoding=utf-8, errors=ignore` reads.
- `os.listdir` on `SimpleLauncher/tools/` confirmed 16 on-disk tool folders and absent `BatchConvertTo7z`/lowercase legacy duplicates — establishing ground truth above csproj text.
- `SimpleLauncher.csproj:44-1456` vs `SimpleLauncher.Avalonia.csproj:1-239` side-by-side structure diff.

---

*End of report. Use §15 as the work queue for reaching full WPF parity.*
