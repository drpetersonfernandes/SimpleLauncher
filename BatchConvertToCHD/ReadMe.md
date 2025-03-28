# Batch Convert to CHD

A Windows desktop utility for batch converting various disk image formats to CHD (Compressed Hunks of Data) format.

![Batch Convert to CHD](images/logo.png)

## Overview

Batch Convert to CHD is a Windows application that provides a simple user interface for converting multiple disk image files to the CHD format. It uses CHDMAN from the MAME project to perform the actual conversions while providing a user-friendly interface for batch processing.

## Features

- **Batch Processing**: Convert multiple files in one operation
- **Multiple Format Support**: Handles various disk image formats
- **ZIP Support**: Automatically extracts supported files from ZIP archives
- **Progress Tracking**: Visual progress indication and detailed logging
- **Delete Original Option**: Option to remove source files after successful conversion
- **User-Friendly Interface**: Simple, intuitive Windows interface

## Supported File Formats

- **CUE+BIN files** (CD images)
- **ISO files** (CD images)
- **GDI files** (GD-ROM images)
- **TOC files** (CD images)
- **IMG files** (Hard disk images)
- **RAW files** (Raw data)
- **ZIP files** (containing any of the above formats)

## Requirements

- Windows 7 or later
- [.NET 9.0 Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)
- CHDMAN.exe (included with the application)

## Installation

1. Download the latest release
2. Extract the ZIP file to a folder of your choice
3. Ensure that CHDMAN.exe is in the same directory as the application
4. Run BatchConvertToCHD.exe

## Usage

1. **Select Input Folder**: Click "Browse" next to "Input Folder" to select the directory containing files to convert
2. **Select Output Folder**: Click "Browse" next to "Output Folder" to choose where the CHD files will be saved
3. **Delete Option**: Check "Delete original files after conversion" if you want to remove source files after successful conversion
4. **Start Conversion**: Click "Start Conversion" to begin the batch process
5. **Monitor Progress**: The application will display progress and log messages during conversion
6. **Cancel (if needed)**: Click "Cancel" to stop the conversion process

## About CHD Format

CHD (Compressed Hunks of Data) is a compressed disk image format developed for the MAME project. It offers several advantages:

- **Efficient Compression**: Significantly reduces file sizes compared to raw disk images
- **Metadata Storage**: Preserves important disk metadata
- **Checksumming**: Includes data verification to ensure integrity
- **Multiple Compression Methods**: Uses optimal compression methods for different data types

## Why Use CHD?

- **Save Disk Space**: CHD files are often much smaller than the original formats
- **Preserve All Data**: Unlike some compression methods, CHD retains all necessary data and metadata
- **MAME Compatibility**: Directly usable with MAME and other emulators that support CHD
- **Data Integrity**: Built-in checksumming helps ensure your disk images remain valid

## Troubleshooting

- Ensure CHDMAN.exe is in the same directory as the application
- Make sure you have appropriate permissions to read from the input directory and write to the output directory
- Check the application log for detailed information about any errors

## Acknowledgements

- Uses CHDMAN from the [MAME project](https://www.mamedev.org/) for CHD conversion
- Developed by [Pure Logic Code](https://www.purelogiccode.com)
