```markdown
# Batch File Creator for ScummVM Games

## Overview

This application helps you create batch files for launching your ScummVM games. It simplifies the process of setting up individual batch files for each game, allowing you to quickly launch your favorite ScummVM titles.

## Features

-   **User-Friendly Interface:** A simple and intuitive graphical user interface (GUI) for easy interaction.
-   **ScummVM Executable Selection:** Allows you to browse and select the path to your `scummvm.exe` file.
-   **Game Folder Selection:** Lets you choose the root folder containing your ScummVM game folders.
-   **Automated Batch File Creation:** Automatically generates batch files for each game folder within the selected root folder.
-   **Logging:** Provides a log window to display the progress and any errors that occur during the batch file creation process.
-   **Error Handling:** Includes error handling and reporting to ensure smooth operation and provide helpful feedback.
-   **Bug Reporting:** Automatically sends error reports to a backend API for tracking and fixing issues.

## How to Use

1.  **Select ScummVM Executable:** Click the "Browse" button next to the "ScummVM Path" field and select your `scummvm.exe` file.
2.  **Select Game Folder:** Click the "Browse" button next to the "Games Folder" field and select the root folder containing your ScummVM game folders.
3.  **Create Batch Files:** Click the "Create Batch Files" button to generate the batch files.
4.  **Locate Batch Files:** The generated batch files will be created in the root folder you selected. Each batch file will be named after the corresponding game folder (e.g., `game_folder_name.bat`).

## System Requirements

-   Windows Operating System
-   .NET Framework (likely 4.7.2 or higher, based on WPF usage)
-   ScummVM installed

## Installation

1.  Download the compiled application.
2.  Extract the files to a folder of your choice.
3.  Run the `CreateBatchFilesForScummVMGames.exe` executable.

## Code Structure

-   **App.xaml & App.xaml.cs:** Defines the application and handles global exception handling. It also initializes the `BugReportService`.
-   **MainWindow.xaml:** Defines the main window's user interface, including text boxes, buttons, and layout.
-   **MainWindow.xaml.cs:** Contains the logic for the main window, including:
    -   Handling button clicks for browsing and creating batch files.
    -   Selecting folders and files using `OpenFolderDialog` and `OpenFileDialog`.
    -   Creating batch files for each game folder.
    -   Logging messages to the UI.
    -   Error handling and displaying messages to the user.
    -   Reporting bugs using the `BugReportService`.
-   **BugReportService.cs:** Implements a service for sending bug reports to a remote API.

## Error Reporting

The application includes a `BugReportService` that silently sends error reports to a specified API endpoint. This helps the developer track and fix issues. The API URL and key are defined as constants in `App.xaml.cs` and `MainWindow.xaml.cs`.

**Important:** The `BugReportApiUrl` and `BugReportApiKey` are placeholders.  You will need to replace them with your own values if you want to use the bug reporting functionality.

## Batch File Format

Each generated batch file contains the following command:

```batch
"path\to\scummvm.exe" -p "path\to\game\folder" --auto-detect --fullscreen
```

-   `"path\to\scummvm.exe"`:  The path to your ScummVM executable.
-   `"path\to\game\folder"`: The path to the specific game folder.
-   `--auto-detect`: Tells ScummVM to automatically detect the game.
-   `--fullscreen`: Launches the game in fullscreen mode.

## Dependencies

-   .NET Framework (WPF)
-   `System.Net.Http`
-   `System.Net.Http.Json`
-   `Microsoft.Win32` (for `OpenFileDialog` and `OpenFolderDialog`)

## Notes

-   The application assumes that each subdirectory within the selected "Games Folder" represents a separate ScummVM game.
-   The application creates batch files in the root folder, alongside the game folders.
-   Error reporting is silent; the user will not be notified if a bug report fails to send.

```