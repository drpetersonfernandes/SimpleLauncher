```markdown
# Batch File Creator for Xbox 360 XBLA Games

## Overview

This application simplifies the process of creating batch files to launch Xbox 360 XBLA games using the Xenia emulator. It automates the creation of `.bat` files for each game, allowing users to quickly launch their games without manually typing commands.

## Features

*   **User-Friendly Interface:** A simple graphical interface for easy navigation.
*   **Xenia Executable Selection:** Allows users to select the path to their `xenia.exe` file.
*   **Game Folder Selection:** Enables users to specify the root folder containing their Xbox 360 XBLA game folders.
*   **Automated Batch File Creation:** Automatically generates batch files for each game found in the specified game folder.
*   **Error Logging:** Provides detailed logging of the batch file creation process, including errors and warnings.
*   **Bug Reporting:** Automatically reports unhandled exceptions and potential issues to a remote API for improved application stability.

## Usage

1.  **Select Xenia Executable:**
    *   Click the "Browse" button next to the "Xenia Path" field.
    *   Navigate to and select your `xenia.exe` file.
2.  **Select Game Folder:**
    *   Click the "Browse" button next to the "Games Folder" field.
    *   Navigate to and select the root folder containing your Xbox 360 XBLA game folders.
3.  **Create Batch Files:**
    *   Click the "Create Batch Files" button.
    *   The application will then scan the game folder, create batch files for each game, and log the progress in the "Log" text box.

## Dependencies

*   .NET Framework (WPF application)
*   Xenia Emulator (for running the generated batch files)

## Code Structure

*   **`App.xaml` and `App.xaml.cs`:**
    *   The application entry point.
    *   Initializes global exception handling and a bug reporting service.
    *   Handles unhandled exceptions at the application, dispatcher, and task scheduler levels.
    *   Reports exceptions to a remote API.
*   **`MainWindow.xaml`:**
    *   Defines the main window layout using WPF.
    *   Includes controls for selecting the Xenia executable, the game folder, creating batch files, and displaying logs.
*   **`MainWindow.xaml.cs`:**
    *   Handles user interactions and logic for the main window.
    *   Includes methods for browsing files and folders, creating batch files, and displaying messages to the user.
    *   Implements the core logic for finding game files and generating batch files.
    *   Uses a `BugReportService` to report potential issues and errors.
*   **`BugReportService.cs`:**
    *   A service responsible for sending bug reports to a remote API.
    *   Uses `HttpClient` to send asynchronous POST requests containing error messages and application information.

## Error Handling and Bug Reporting

The application implements robust error handling and bug reporting:

*   **Global Exception Handling:** Catches unhandled exceptions at the application level to prevent crashes.
*   **Detailed Logging:** Logs important events and errors to the log text box in the main window.
*   **Silent Bug Reporting:** Uses a `BugReportService` to silently send bug reports to a remote API, including exception details, system information, and application logs. This helps in identifying and fixing issues without disrupting the user experience.

## Important Notes

*   The application assumes a specific directory structure for the Xbox 360 XBLA games (i.e., presence of "000D0000" subdirectories).
*   The Bug Report API URL and API Key are hardcoded in the `App.xaml.cs` and `MainWindow.xaml.cs` files.  **Do not commit actual API keys to public repositories.** These should be managed securely.
*   The application performs basic validation of the selected Xenia executable and game folder.
*   The application attempts to locate the game file within the game directory structure. If a game file is not found, it reports the directory structure to assist in debugging.

## Future Enhancements

*   Add support for different game directory structures.
*   Implement a settings panel for configuring the Xenia executable path and Bug Report API settings.
*   Improve error handling and provide more informative error messages to the user.
*   Add a progress bar to indicate the progress of batch file creation.
*   Implement a more sophisticated method for identifying the game file within the game directory.

