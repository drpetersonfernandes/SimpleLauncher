```markdown
# MAME Utility

## Overview

MAME Utility is a tool designed to help manage and organize MAME (Multiple Arcade Machine Emulator) data. It provides functionalities to create lists based on various criteria, merge lists, and copy ROMs and images. It is built using .NET Framework and utilizes XML for data handling.

## Features

-   **Create MAME Lists**: Generate XML lists based on:
    -   Full driver information
    -   Manufacturer
    -   Year
    -   Source file
    -   Software list
-   **Merge Lists**: Combine multiple XML lists into a single XML and DAT file.
-   **Copy ROMs**: Copy ROM files from a source directory to a destination directory based on an XML list.
-   **Copy Images**: Copy image files (PNG, JPG, JPEG) from a source directory to a destination directory based on an XML list.
-   **Bug Reporting**: Automatically reports unhandled exceptions to a configured API endpoint.
-   **Logging**: Provides a log window to display the progress and any errors encountered during operations.

## Usage

1.  **Create Lists**:
    -   Select the desired list type from the main window (e.g., "Create MAME Manufacturer List").
    -   Choose the MAME full driver information XML file as input.
    -   Specify the output folder or file path for the generated list.
2.  **Merge Lists**:
    -   Click the "Merge Lists" button.
    -   Select multiple XML files to merge.
    -   Choose the output file path for the merged XML and DAT files.
3.  **Copy ROMs/Images**:
    -   Click the "Copy Roms" or "Copy Images" button.
    -   Select the source directory containing the ROMs or images.
    -   Select the destination directory to copy the files to.
    -   Choose the XML file(s) containing ROM or image information.

## Configuration

The application's configuration is stored in `appsettings.json`. The following settings can be configured:

-   `BugReportApiUrl`: The URL of the bug report API endpoint.
-   `BugReportApiKey`: The API key for accessing the bug report API.

Example `appsettings.json`:

```json
{
  "BugReportApiUrl": "https://www.purelogiccode.com/bugreport/api/send-bug-report",
  "BugReportApiKey": "hjh7yu6t56tyr540o9u8767676r5674534453235264c75b6t7ggghgg76trf564e"
}
```

## Dependencies

-   .NET Framework
-   MessagePack (for DAT file creation)

## Architecture

The application is structured as follows:

-   **XAML Files**: Define the user interface for the main window, about window, and log window.
-   **C# Files**: Implement the application logic and functionality.
    -   `App.xaml.cs`: Handles application startup and global exception handling.
    -   `AppConfig.cs`: Loads and manages application configuration settings.
    -   `BugReportService.cs`: Sends exception reports to a configured API.
    -   `CopyImages.cs`: Implements the logic for copying image files based on XML data.
    -   `CopyRoms.cs`: Implements the logic for copying ROM files based on XML data.
    -   `MAMEFull.cs`, `MAMEManufacturer.cs`, `MameYear.cs`, `MAMESourcefile.cs`, `MameSoftwareList.cs`: Implement the logic for creating various MAME lists.
    -   `MergeList.cs`: Implements the logic for merging XML lists and saving them as XML and DAT files.
    -   `LogWindow.xaml.cs`: Implements the log window for displaying messages.
    -   `MainWindow.xaml.cs`: Implements the main window logic, including button click handlers and progress reporting.

## Error Handling

The application includes global exception handling to catch unhandled exceptions and report them to a configured API. This helps in identifying and fixing bugs. The `BugReportService` class is responsible for sending the exception reports asynchronously.

## Contributing

Contributions are welcome! Please feel free to submit pull requests or open issues for any bugs or feature requests.

## License

[Insert License Here]

## Support

For support, please visit [https://purelogiccode.com](https://purelogiccode.com) or open an issue on the project's GitHub repository.
```