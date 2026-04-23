# SimpleLauncher.Tests

xUnit test project for [SimpleLauncher](../SimpleLauncher). These tests validate resource localization, URL health, update logic, and various utility functions.

## Running Tests

```bash
dotnet test SimpleLauncher.Tests
```

To run a specific test class:

```bash
dotnet test SimpleLauncher.Tests --filter "FullyQualifiedName~DetectMissingResourceStringsTests"
```

## Test Categories

### Resource Localization Tests

| Test | Purpose |
|------|---------|
| `DetectMissingResourceStringsTests` | Finds resource keys used in C#/XAML but missing from `strings.en.xaml`. **Auto-writes** missing keys to the English file and fails so the developer is notified. |
| `DetectMismatchedResourceStringsTests` | Detects when the same `TryFindResource("Key")` is used with **different** fallback strings (`?? "Value"`) across the codebase. |
| `DetectDuplicateResourceKeysTests` | Scans every `strings.*.xaml` for duplicate `x:Key` entries. **Fails** if duplicates are found so the admin can remove them. |
| `DetectMissingKeysInOtherLanguagesTests` | Compares all non-English language files against `strings.en.xaml`. **Fails** if any file is missing keys or has extra keys. |
| `ResourceFileLoadingTests` | Loads every `strings.*.xaml` via WPF `XamlReader` at runtime to verify they parse without errors. |

### URL Validation Tests

| Test | Purpose |
|------|---------|
| `ParametersMdAllUrlsAreReachable` | Checks every URL inside `parameters.md` is reachable. |
| `EasyModeApiEndpointIsReachable` | Validates the EasyMode API endpoints return valid JSON arrays. |
| `EasyModeFallbackXmlIsReachableAndContainsValidUrls` | Downloads fallback XML files and recursively validates every URL inside them. |

### Update Simulation Tests

| Test | Purpose |
|------|---------|
| `ExtractAllFromZip_ExtractsFilesCorrectly` | Creates an in-memory ZIP, extracts it via the real `UpdateChecker.ExtractAllFromZip` logic, verifies contents, and cleans up. |
| `ExtractAllFromZip_RejectsPathTraversal` | Ensures the extractor blocks path-traversal attacks (e.g. `../../../etc/passwd`). |
| `ExtractAllFromZip_RejectsDangerousExtensions` | Ensures dangerous file types (`.exe`, `.bat`, etc.) are blocked. |
| `IsNewVersionAvailable_SemanticComparison` | Tests semantic version comparison logic via reflection. |
| `NormalizeVersion_StripsPrefixesAndSuffixes` | Tests version string normalization. |
| `ParseVersionAndAssetUrlsFromResponse_ValidJson` | Tests GitHub release JSON parsing. |

### Utility / Unit Tests

Various tests for helper classes: `FormatFileSize`, `PathHelper`, `CheckPath`, `CheckIfDirectoryIsWritable`, `CheckForFileLock`, `BugReportFormatter`, `MameManager`, `InverseBooleanConverter`, `GetMicrosoftWindowsVersion`, `EncryptDuckStationToken`, `SanitizeInputSystemName`, `RetroAchievementsSystemMatcher`, `DeleteFiles`.

## Known Expected Failures

Some tests are designed to **fail** to alert the admin of real data issues that must be fixed manually:

- **Duplicate keys** in non-English `strings.*.xaml` files (`DetectDuplicateResourceKeysTests`)
- **Missing translation keys** in non-English files (`DetectMissingKeysInOtherLanguagesTests`)
- **Broken or unreachable URLs** in `parameters.md` or EasyMode XML (`UrlValidationTests`)

## Project Structure

```
SimpleLauncher.Tests/
├── SimpleLauncher.Tests.csproj   # Project file (references xUnit + SimpleLauncher)
├── DetectMissingResourceStringsTests.cs
├── DetectMismatchedResourceStringsTests.cs
├── DetectDuplicateResourceKeysTests.cs
├── DetectMissingKeysInOtherLanguagesTests.cs
├── ResourceFileLoadingTests.cs
├── UrlValidationTests.cs
├── UpdateSimulationTests.cs
└── ... (utility tests)
```
