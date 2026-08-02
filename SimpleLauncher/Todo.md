### 8. Inverted method name: `MarkInvalid(control, true)` marks valid
- **File:** `EditSystemWindow.ValidateFields.cs:12-22`
- Calling `MarkInvalid(myControl, true)` reads as "mark invalid=true" but sets the control to valid (white foreground). Semantics are inverted.

