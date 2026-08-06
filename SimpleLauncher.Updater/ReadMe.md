# SimpleLauncher Updater

## Overview
This is an automatic updater application for the SimpleLauncher project. It is a WPF-based utility designed to:
1. Check for new releases on GitHub.
2. Download and install updates automatically.
3. Provide a fallback manual update option if automatic updates fail.
4. Restart the main application after successful updates.

## Features
- **Terminal-Style UI**: A retro "old monitor" interface with a black background and lime-green text.
- **Automatic Updates**: Checks GitHub releases for the latest version based on the system architecture (x64/ARM64).
- **In-Memory Processing**: Downloads and extracts updates without creating temporary files on disk.
- **Forced Visibility**: Automatically brings itself to the foreground and focuses the window upon launch.
- **Error Handling**: Graceful fallback to manual updates via the GitHub releases page.
- **Logging**: Real-time progress logging displayed in the terminal window.
- **Self-Preservation**: Intelligently skips updating its own active files (`Updater.exe`, `Updater.dll`, etc.) to prevent file-in-use errors or corruption.

## Technical Details
- **Target Framework**: .NET 10 (WPF Application)
- **Language Version**: C# 14
- **Dependencies**:
    - `SharpZipLib`: Used for high-performance ZIP extraction.
    - `System.Net.Http`: For GitHub API and asset downloads.
    - `System.Text.Json`: For parsing release metadata.
- **GitHub API Integration**: Uses GitHub's Releases API to fetch the latest version and matching assets.

## Usage
1. The updater is automatically launched by the main application (`SimpleLauncher`) when an update is detected.
2. It receives the Process ID (PID) of the main application as a command-line argument.
3. **Process Flow**:
    - Waits for the main application to exit (up to 10 seconds).
    - Fetches the latest release info from GitHub.
    - Matches the release asset name (e.g., `release_1.0.0_win-x64.zip`).
    - Downloads the package to memory.
    - Extracts files to the application directory (excluding updater files).
    - Restarts the main application with the `-whatsnew` flag.

## Fallback Behavior
If the automatic update fails (due to network issues, permission errors, etc.), the user is prompted with a dialog:
1. **Yes**: Opens the GitHub releases page in the default web browser for manual download.
2. **No**: Closes the updater.

## Configuration
The updater is configured via constants located in `MainWindow.xaml.cs`:
```csharp
private const string RepoOwner = "drpetersonfernandes";
private const string RepoName = "SimpleLauncher";
```

## Notes
- The updater is designed to be "architecture aware" and will look for assets containing `win-x64` or `win-arm64` in the filename.
- After a successful update, the main application is launched with the `whatsnew` argument to trigger any post-update UI in the launcher.

## Requirements
- .NET 10 Runtime (Windows).
- Active Internet connection.
- GitHub repository must follow the naming convention: `release_{version}_{rid}.zip`.