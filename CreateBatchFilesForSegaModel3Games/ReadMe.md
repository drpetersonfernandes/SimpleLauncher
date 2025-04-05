```markdown
# Create Batch Files for Sega Model 3 Games

## Overview

This application simplifies the process of creating batch files for launching Sega Model 3 games using the Supermodel emulator. It allows users to select the Supermodel executable and a folder containing ROM files, and then automatically generates batch files for each ROM.  The application also includes silent bug reporting to assist with development and maintenance.

## Features

*   **User-Friendly Interface:** Simple and intuitive graphical interface for easy operation.
*   **Supermodel Executable Selection:** Allows users to select the path to the `Supermodel.exe` emulator.
*   **ROM Folder Selection:** Enables users to specify the folder containing their Sega Model 3 ROM zip files.
*   **Automatic Batch File Generation:** Creates batch files for each ROM in the selected folder, configured to launch the game in fullscreen mode with FPS display.
*   **Error Logging:** Provides a log window to display status messages, errors, and warnings.
*   **Silent Bug Reporting:** Automatically reports unhandled exceptions and errors to a remote API for debugging and improvement purposes.

## Usage

1.  **Select Supermodel Executable:** Click the "Browse" button next to "Supermodel Path" and select the `Supermodel.exe` file.
2.  **Select ROM Folder:** Click the "Browse" button next to "ROM Folder" and select the folder containing your Sega Model 3 ROM zip files.
3.  **Create Batch Files:** Click the "Create Batch Files" button to generate the batch files. The created batch files will be located in the same folder as your ROM zip files.
4.  **Check Log:** Monitor the log text box for any messages, warnings, or errors during the process.

## Technical Details

### Technologies Used

*   **C#:** The primary programming language.
*   **WPF (Windows Presentation Foundation):** Used for building the graphical user interface.
*   **.NET Framework:** The application targets the .NET Framework.
*   **System.IO:** Used for file and directory operations.
*   **System.Net.Http:** Used for sending bug reports to the API.

### File Structure

*   **App.xaml/App.xaml.cs:** Defines the application entry point and handles global exception handling.
*   **MainWindow.xaml/MainWindow.xaml.cs:** Contains the main application window, including UI elements and event handlers for browsing directories, selecting files, and creating batch files.
*   **BugReportService.cs:** Implements a service for silently sending bug reports to a remote API.

### Bug Reporting

The application includes a `BugReportService` that silently sends bug reports to a remote API.  This helps in identifying and fixing issues without requiring user intervention.

**Configuration:**

*   `BugReportApiUrl`: The URL of the bug report API endpoint.
*   `BugReportApiKey`: The API key for authenticating with the bug report API.
*   `ApplicationName`: The name of the application.

**Exception Handling:**

The `App.xaml.cs` file sets up global exception handling to catch unhandled exceptions from various sources (AppDomain, Dispatcher, TaskScheduler) and report them. The `MainWindow.xaml.cs` file also reports specific errors encountered during the batch file creation process.

## Configuration

The following configuration values are used in `App.xaml.cs` and `MainWindow.xaml.cs`:

*   `BugReportApiUrl`: `"https://www.purelogiccode.com/bugreport/api/send-bug-report"`
*   `BugReportApiKey`: `"hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e"`
*   `ApplicationName`: `"CreateBatchFilesForSegaModel3Games"`

**Note:** The `BugReportApiKey` is a placeholder.  A valid API key is required for the bug reporting feature to function correctly.

## Dependencies

*   .NET Framework

## Build Instructions

1.  Open the project in Visual Studio.
2.  Build the solution.  The executable will be located in the `bin\Debug` or `bin\Release` folder.

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues to suggest improvements or report bugs.

## License

This project is licensed under the [MIT License](LICENSE).
```