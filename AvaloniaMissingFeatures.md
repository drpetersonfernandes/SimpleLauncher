## Table of Contents

## Improve Avalonia Localization

## Gamepad Support

**Remaining gap:** SharpDX/XInput/DirectInput are Windows-only. On the `net10.0` Linux TFM the controller compiles but cannot function — Linux gamepad support would require a different library (e.g., `SDL2-CS` or `OpenTK`) behind a platform abstraction.

## 11. Other Gaps

| Gap | WPF | Avalonia | Impact |
|---|---|---|---|
| Emergency return button on loading | Loading overlay has cancel button after timeout | Not implemented | Low |
| `ApplicationStats` static class | WPF-specific stats helper | Avalonia uses Core `Stats` class directly | Minor |

