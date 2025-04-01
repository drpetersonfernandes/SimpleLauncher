# Batch Convert to Compressed File

A simple Windows desktop application built with WPF to batch compress individual files from a selected folder into separate `.7z` or `.zip` archives in an output folder.

## Features

*   **Batch Compression:** Compresses each file found in the input folder individually.
*   **Folder Selection:** Easy-to-use browse buttons to select input and output directories.
*   **Format Choice:** Supports compressing files into either `.7z` or `.zip` format.
*   **Optional Deletion:** Option to automatically delete the original file after successful compression.
*   **Progress Monitoring:** Displays a progress bar showing the overall status of the batch operation.
*   **Cancellation:** Allows the user to cancel the ongoing batch compression process.
*   **Detailed Logging:** Shows real-time logs of the operations being performed, including successes and failures.
*   **Architecture Aware:** Automatically uses the appropriate 64-bit (`7z.exe`) or 32-bit (`7z_x86.exe`) version of the 7-Zip command-line tool based on your system.
*   **Error Reporting:** Includes anonymous, silent error reporting to help improve the application (sends error details, system info, and logs if a crash occurs).

## Requirements

1.  **Operating System:** Windows (Developed using WPF).
2.  **.NET Desktop Runtime:** You will need a compatible version of the .NET Desktop Runtime installed (e.g., .NET 6, 7, or 8, depending on how the application was compiled. Check the release notes if available).
3.  **7-Zip Executables:**
    *   This application **requires** the 7-Zip command-line executables (`7z.exe` for 64-bit systems and `7z_x86.exe` for 32-bit systems).
    *   You must download these from the official 7-Zip website: [https://www.7-zip.org/](https://www.7-zip.org/) (You typically need the "Command Line Version" or extract them from the full installer).
    *   **Crucially:** Place `7z.exe` and `7z_x86.exe` in the **same directory** as the `BatchConvertToCompressedFile.exe` application file. The application will not function without them.

## Installation

1.  Download the latest release package (e.g., a `.zip` file) from the releases section (if applicable) or obtain the application folder.
2.  Extract the contents to a folder on your computer (e.g., `C:\Program Files\BatchCompressor`).
3.  **Important:** Download the 7-Zip command-line executables (`7z.exe` and `7z_x86.exe`) as mentioned in the **Requirements** section.
4.  Copy `7z.exe` and `7z_x86.exe` into the same folder where you extracted `BatchConvertToCompressedFile.exe`.

## Usage

1.  Run `BatchConvertToCompressedFile.exe`.
2.  Click the **Browse** button next to "Input Folder" and select the directory containing the files you want to compress.
3.  Click the **Browse** button next to "Output Folder" and select the directory where the compressed files should be saved.
4.  Choose the desired **Compression Format** by selecting either the "7z" or "zip" radio button.
5.  (Optional) Check the **"Delete original files after compression"** box if you want the source files to be removed *after* they are successfully compressed. **Warning:** Use this option with caution!
6.  Click the **"Start Compression"** button.
7.  The application will begin processing the files. You can monitor the progress via the progress bar and the log viewer at the bottom.
8.  If you need to stop the process, click the **"Cancel"** button. Note that cancellation will prevent new files from starting but might wait for the currently processing file to finish.
9.  Once completed, a summary message will appear, and the logs will show the details of the operation.

## Important Notes

*   **Individual Archives:** This tool creates a *separate* compressed archive for *each* file in the input folder. It does not create one large archive containing all files.
*   **Overwrite Behavior:** If a file with the same name as the target archive (e.g., `document.zip`) already exists in the output folder, **it will be deleted and overwritten** without prompting before the new compression attempt begins.
*   **Subfolders:** The current version only processes files directly within the selected input folder (`SearchOption.TopDirectoryOnly`). It does not recurse into subfolders.
*   **7-Zip Dependency:** The application relies entirely on the external `7z.exe`/`7z_x86.exe` tools. Ensure they are present and accessible in the application's directory.

## Building from Source (Optional)

If you need to build the application yourself:

1.  Clone the repository.
2.  Open the solution file (`.sln`) in Visual Studio (e.g., VS 2022).
3.  Ensure you have the required .NET SDK installed.
4.  Build the solution (Ctrl+Shift+B).
5.  Remember to place the `7z.exe` and `7z_x86.exe` files in the output directory (e.g., `bin\Debug\netX.X-windows`) after building.

## License

(Specify your license here, e.g., MIT License, Apache 2.0, or state if it's proprietary). If no license is specified, standard copyright laws apply.

```