# Batch Verify CHD Files

A Windows utility for verifying the integrity of CHD (Compressed Hunks of Data) files in batch. This application allows you to efficiently verify multiple CHD files at once, ensuring that your ROM collection is intact and error-free.

![Batch Verify CHD Files](images/logo.png)

## Features

- **Batch Processing**: Verify multiple CHD files in a single operation
- **Subdirectory Support**: Option to include subdirectories in the verification process
- **Detailed Logging**: Comprehensive logging of the verification process
- **File Information Display**: View detailed information about each CHD file
- **Progress Tracking**: Real-time progress bar and status updates
- **Statistics**: Track verification success and failure rates
- **User-Friendly Interface**: Clean Windows application interface

## Requirements

- Windows operating system
- .NET 9.0 or later
- CHDMAN.exe (included with the application)

## Installation

1. Download the latest release
2. Extract the ZIP file to a location of your choice
3. Run `BatchVerifyCHDFiles.exe`

*No installation required - the application is portable.*

## Usage

1. **Launch the Application**: Run `BatchVerifyCHDFiles.exe`
2. **Select Input Folder**: Click the "Browse" button to select the folder containing your CHD files
3. **Configure Options**: 
   - Check "Include subfolders" if you want to process CHD files in all subdirectories
4. **Start Verification**: Click the "Start Verification" button to begin the process
5. **View Results**: 
   - The verification log displays the progress and status of each file
   - The file information panel shows details about the currently processing CHD file
   - Statistics at the bottom track overall verification results

## Verification Process

The application uses CHDMAN (from the MAME project) to perform the following checks on each CHD file:

- CHD header structure verification
- Internal metadata validation
- Data integrity checking against checksums
- Decompression testing of data hunks

A CHD file passes verification only if all checks are successful.

## Troubleshooting

### Common Issues

- **"chdman.exe not found"**: Ensure that chdman.exe is in the same directory as the application
- **Verification failures**: Corrupted CHD files will fail verification - they should be replaced or repaired
- **Application crashes**: The application includes automatic error reporting - please allow any error reports to be sent to help improve the software

### Logs

The verification log window displays all operations and can help diagnose issues. If you need assistance, you can copy the log content for support purposes.

## About CHD Files

CHD (Compressed Hunks of Data) is a compressed file format commonly used for storing disk, CD, and DVD images, primarily in emulation contexts. The format was developed by the MAME team to efficiently store large ROM images.

## Credits

- This application uses CHDMAN from the MAME project for CHD verification
- Developed by Pure Logic Code
- © 2025 Pure Logic Code. All rights reserved.

For more information, visit [Pure Logic Code](https://www.purelogiccode.com).