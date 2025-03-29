# Batch Verify Compressed Files

A Windows utility for verifying the integrity of compressed archives (ZIP, 7Z, RAR) in batch. This application allows you to efficiently verify multiple compressed files at once, ensuring that your archives are intact and error-free.

![Batch Verify Compressed Files](images/logo.png)

## Features

- **Multiple Archive Formats**: Verify ZIP, 7Z, and RAR files
- **Batch Processing**: Verify multiple files in a single operation
- **Subdirectory Support**: Option to include subdirectories in the verification process
- **Detailed Logging**: Comprehensive logging of the verification process
- **File Information Display**: View detailed information about each compressed file
- **Progress Tracking**: Real-time progress bar and status updates
- **Statistics**: Track verification success and failure rates
- **User-Friendly Interface**: Clean Windows application interface

## Requirements

- Windows operating system
- .NET 9.0 or later
- 7z.exe and 7z_x86.exe (included with the application)
   - The application automatically selects the appropriate executable based on your system architecture

## Installation

1. Download the latest release
2. Extract the ZIP file to a location of your choice
3. Run `BatchVerifyCompressedFiles.exe`

*No installation required - the application is portable.*

## Usage

1. **Launch the Application**: Run `BatchVerifyCompressedFiles.exe`
2. **Select Input Folder**: Click the "Browse" button to select the folder containing your compressed files
3. **Configure Options**:
   - Select which file types to verify (ZIP, 7Z, RAR)
   - Check "Include subfolders" if you want to process files in all subdirectories
4. **Start Verification**: Click the "Start Verification" button to begin the process
5. **View Results**:
   - The verification log displays the progress and status of each file
   - The file information panel shows details about the currently processing compressed file
   - Statistics at the bottom track overall verification results

## Verification Process

The application uses 7-Zip to perform the following checks on each compressed file:

- Archive header structure verification
- File contents integrity testing
- CRC/checksum validation
- Extraction simulation (without writing files)

A compressed file passes verification only if all checks are successful.

## Troubleshooting

### Common Issues

- **"7z.exe not found" or "7z_x86.exe not found"**: Ensure that both 7z.exe (for 64-bit systems) and 7z_x86.exe (for 32-bit systems) are in the same directory as the application
- **Verification failures**: Corrupted archives will fail verification - they should be replaced or repaired
- **Application crashes**: The application includes automatic error reporting - please allow any error reports to be sent to help improve the software

### Logs

The verification log window displays all operations and can help diagnose issues. If you need assistance, you can copy the log content for support purposes.

## About Compressed File Formats

- **ZIP**: A widely used archive format that provides lossless data compression. Most operating systems have built-in support for ZIP files.
- **7Z**: A compressed archive format that supports high compression ratios and strong AES-256 encryption.
- **RAR**: A proprietary archive file format that supports data compression, error recovery, and file spanning.

## Credits

- This application uses 7-Zip for archive verification
- Developed by Pure Logic Code
- © 2025 Pure Logic Code. All rights reserved.

For more information, visit [Pure Logic Code](https://www.purelogiccode.com)