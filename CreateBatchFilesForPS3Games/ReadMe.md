```markdown
# Create Batch Files for PS3 Games

## Overview

This application simplifies the process of creating batch files for launching PlayStation 3 (PS3) games using the RPCS3 emulator. It automates the creation of `.bat` files that directly launch games from your game library with RPCS3, eliminating the need to manually navigate through the emulator's interface.

## Features

-   **RPCS3 Path Selection:** Allows you to select the path to your `rpcs3.exe` file.
-   **Game Folder Selection:** Enables you to specify the root folder containing your PS3 game folders.
-   **Automated Batch File Creation:** Generates batch files for each game in the selected folder, using the game's title or ID for the batch file name.
-   **Error Logging:** Provides a log textbox to display messages, errors, and progress updates.
-   **SFO Parsing:** Reads `PARAM.SFO` files to extract game titles and IDs for batch file naming.
-   **Filename Sanitization:** Cleans up filenames to ensure compatibility with the operating system.
-   **Silent Bug Reporting:** Implements a background bug reporting service to automatically send error reports to the developer.

## Usage

1.  **Select RPCS3 Executable:** Click the "Browse" button next to "RPCS3 Path" and select your `rpcs3.exe` file.
2.  **Select Game Folder:** Click the "Browse" button next to "Games Folder" and select the root folder where your PS3 game folders are located.
3.  **Create Batch Files:** Click the "Create Batch Files" button to generate the batch files.
4.  **Locate Batch Files:** The batch files will be created in the same root folder as your PS3 game folders.

## Dependencies

-   .NET Framework (The project is a WPF application, so it requires the .NET Framework to run.)
-   RPCS3 emulator

## Installation

1.  Clone or download the project repository.
2.  Open the solution file (`.sln`) in Visual Studio.
3.  Build the project.
4.  The executable file will be located in the `bin/Debug` or `bin/Release` folder.

## Code Structure

-   **App.xaml.cs:** Handles application-level events, including unhandled exceptions. It initializes the bug reporting service and sets up global exception handling.
-   **BugReportService.cs:** Implements a service to silently send bug reports to a remote API.
-   **MainWindow.xaml:** Defines the user interface of the main window, including buttons, text boxes, and labels.
-   **MainWindow.xaml.cs:** Contains the logic for the main window, including handling button clicks, creating batch files, reading SFO files, and logging messages.

## Key Classes and Methods

-   **MainWindow:**
    -   `BrowseRPCS3Button_Click`: Opens a file dialog to select the RPCS3 executable.
    -   `BrowseFolderButton_Click`: Opens a folder dialog to select the game folder.
    -   `CreateBatchFilesButton_Click`: Creates batch files for each game in the selected folder.
    -   `CreateBatchFilesForFolders`: Iterates through subdirectories and creates batch files based on the EBOOT.BIN file.
    -   `ReadSfo`: Reads and parses the `PARAM.SFO` file to extract game title and ID.
    -   `SanitizeFileName`: Sanitizes the filename for creating batch files.
    -   `ReportBugAsync`: Sends bug reports to the remote API.
-   **BugReportService:**
    -   `SendBugReportAsync`: Sends a bug report to the specified API endpoint.

## Error Handling and Bug Reporting

The application includes robust error handling and a silent bug reporting mechanism. Unhandled exceptions are caught at the application level and reported to the developer.  The `BugReportService` sends detailed error information, including system information, error messages, stack traces, and application logs, to help diagnose and fix issues.

## Contributing

Contributions are welcome! If you find a bug or have a feature request, please open an issue or submit a pull request.

## License

[MIT License](LICENSE) (Replace with the actual license file if you have one)

## Author

[Your Name]
```