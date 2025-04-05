```markdown
# Batch File Creator for Windows Games

## Overview

This application simplifies the process of creating batch files for launching Windows games. It provides a user-friendly interface to select the game executable and choose the output location for the batch file.  The application also includes automated bug reporting to help improve its reliability.

## Features

*   **Easy Batch File Creation:**  Generates batch files with just a few clicks.
*   **Game Executable Selection:**  Allows users to browse and select the game's `.exe` file.
*   **Batch File Output Location:**  Lets users choose where to save the generated `.bat` file.
*   **Logging:** Provides a log textbox to display messages and potential errors.
*   **Error Handling:** Includes error handling and warnings for common issues.
*   **Automated Bug Reporting:** Silently reports any unhandled exceptions or application errors to a remote API to help with debugging and improvements.

## How to Use

1.  **Select Game Executable:** Click the "Browse" button next to "Game Executable" and select the `.exe` file of the game you want to launch.
2.  **Choose Batch File Output:** Click the "Save As..." button next to "Batch File Output" and choose a location to save the generated `.bat` file.
3.  **Create Batch File:**  Click the "Create Batch File" button.
4.  **Confirmation:** A message box will confirm the successful creation of the batch file.
5.  **Create Another:** After creating a batch file, a "Create Another Batch File" button will appear, allowing you to easily create another batch file for a different game. Clicking this button resets the UI.

## Technical Details

*   **Language:** C#
*   **Framework:** .NET Framework (WPF)
*   **UI:** XAML
*   **Dependencies:**
    *   System.Net.Http
    *   System.Net.Http.Json
    *   Microsoft.Win32

## File Structure

*   **App.xaml/App.xaml.cs:**  Application entry point and global exception handling.  Includes logic for reporting unhandled exceptions.
*   **MainWindow.xaml/MainWindow.xaml.cs:** Main window of the application, handles UI interactions and batch file creation logic.
*   **BugReportService.cs:** A service that handles sending bug reports to a remote API.

## Code Highlights

*   **Exception Handling:** The `App.xaml.cs` file implements global exception handlers to catch and report unhandled exceptions from various sources (AppDomain, Dispatcher, TaskScheduler). These exceptions are silently reported using the `BugReportService`.
*   **Asynchronous Operations:**  The application uses asynchronous operations (`async`/`await`) for file operations and bug reporting to prevent blocking the UI thread.
*   **UI Updates:**  The `LogMessage` method uses `Application.Current.Dispatcher.Invoke` to safely update the UI from background threads.
*   **Error Reporting:** The `ReportBugAsync` method in `MainWindow.xaml.cs` collects detailed information about errors, including system information, exception details, and application logs, and sends them to the bug reporting service.
*   **File Dialogs:**  The `SelectGameExecutable` and `SaveBatchFile` methods use `OpenFileDialog` and `SaveFileDialog` respectively to allow the user to browse for files and choose a save location.

## Bug Reporting

The application includes a `BugReportService` which silently sends bug reports to a specified API endpoint.  This helps in identifying and fixing issues without requiring user intervention.  The API endpoint and key are defined as constants in `App.xaml.cs` and `MainWindow.xaml.cs`.

**Important:** The `BugReportApiUrl` and `BugReportApiKey` are placeholders and should be replaced with your actual API endpoint and key for the bug reporting service. The current values are:

*   `BugReportApiUrl = "https://www.purelogiccode.com/bugreport/api/send-bug-report"`
*   `BugReportApiKey = "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e"`

These values are used for demonstration purposes only.

## Notes

*   The application performs basic validation to ensure the selected game executable exists and appears to be a valid `.exe` file.
*   The application checks if the selected batch file directory is writable.
*   The `Window_Closing` event handler ensures the application shuts down correctly.
```