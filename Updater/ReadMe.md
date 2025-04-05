# SimpleLauncher Updater

## Overview
This is an automatic updater application for the SimpleLauncher project. It's designed to:
1. Check for new releases on GitHub
2. Download and install updates automatically
3. Provide a fallback manual update option if automatic updates fail
4. Restart the main application after successful updates

## Features
- **Automatic Updates**: Checks GitHub releases for the latest version
- **In-Memory Processing**: Downloads and extracts updates without temporary files
- **Error Handling**: Graceful fallback to manual updates when needed
- **Logging**: Detailed progress logging in the UI
- **Version Normalization**: Handles version string parsing and normalization
- **Self-Preservation**: Skips updating its own files to prevent corruption

## Technical Details
- **Target Framework**: .NET (Windows Forms application)
- **Dependencies**:
  - System.IO.Compression
  - System.Text.Json
  - System.Net.Http
- **GitHub API Integration**: Uses GitHub's Releases API to fetch latest version

## Usage
1. The updater is automatically launched by the main application when an update is needed
2. It waits for the main application to exit (3 second delay)
3. Performs the update process:
   - Fetches latest release info from GitHub
   - Downloads the update package
   - Extracts files (excluding updater-related files)
   - Restarts the main application

## Fallback Behavior
If automatic updates fail, the user is presented with options to:
1. Try manual update (opens GitHub releases page in browser)
2. Close the updater

## Configuration
The updater is configured with these constants in `UpdateForm.cs`:
```csharp
private const string RepoOwner = "drpetersonfernandes";
private const string RepoName = "SimpleLauncher";
```

## Building
1. Open the solution in Visual Studio
2. Build the project (should produce `Updater.exe`)
3. The updater should be included in your main application's release package

## Notes
- The updater excludes its own files (`Updater.exe`, `Updater.dll`, etc.) from being updated
- After successful update, the main application is launched with `whatsnew` argument
- The UI shows a simple log window with update progress

## Requirements
- .NET runtime (version should match your target framework)
- Internet connection to fetch updates
- GitHub repository must follow standard release format with ZIP assets