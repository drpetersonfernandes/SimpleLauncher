## Table of Contents

## Improve Avalonia Localization

## Gamepad Support

**Remaining gap:** SharpDX/XInput/DirectInput are Windows-only. On the `net10.0` Linux TFM the controller compiles but cannot function — Linux gamepad support would require a different library (e.g., `SDL2-CS` or `OpenTK`) behind a platform abstraction.

## 10. Missing Services (extracted from MainWindow)

The WPF project extracts business logic into standalone services (all under `SimpleLauncher\Services\`). The Avalonia project inlines this logic into `MainWindow.axaml.cs` and `MainViewModel.cs`. While the functionality may exist, it's not in a testable, reusable service.

| WPF Service | Purpose | Avalonia Status |
|---|---|---|
| `SystemImageResolverService` | Fuzzy-matching system image resolver | **Missing entirely** |
| `UIResetService` | Reset UI to initial state | **Missing entirely** |
| `SystemSelectionOrchestratorService` | System selection coordination | **Missing entirely** |



| `DisplaySystemInformation` | OS/hardware info display | **Missing entirely** |
| `MenuActionHandlerService` | Menu action delegation | Inlined in MainWindow |
| `MenuOrchestratorService` | Central menu orchestrator | Inlined in MainWindow |
| `UiOrchestratorService` | UI state coordination | Distributed across MainWindow/MainViewModel |
| `ContextMenuService` | Context menu construction | Inlined in `ShowGameContextMenu()` |
| `GameFilterService` | Filter by cover/status | Logic in `MainViewModel` |
| `GameListUIService` | Grid/list view management | Logic in MainWindow |
| `LoadingOverlayService` | Centralized loading overlay | Per-window inline implementation |
| `SearchOrchestratorService` | Search coordination | `GlobalSearchSectionViewModel` (ViewModel, not service) |
| `UpdateStatusBarService` | Status bar management | Part of `AvaloniaStartupInitializationService` |

## 11. Other Gaps

| Gap | WPF | Avalonia | Impact |
|---|---|---|---|
| Emergency return button on loading | Loading overlay has cancel button after timeout | Not implemented | Low |
| `ApplicationStats` static class | WPF-specific stats helper | Avalonia uses Core `Stats` class directly | Minor |

