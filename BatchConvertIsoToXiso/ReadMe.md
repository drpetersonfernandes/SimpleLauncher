# Batch ISO to XISO Converter

![Application Screenshot](screenshot.png) *(Optional: Add actual screenshot later)*

A Windows application for batch converting ISO files to Xbox XISO format using `extract-xiso.exe`.

## Features

- Convert multiple ISO files to XISO format in one operation
- Select input and output folders with easy browsing
- Option to delete original files after successful conversion
- Detailed logging of the conversion process
- Progress tracking during batch operations
- Automatic error reporting (with user consent)

## Requirements

- Windows 10 or later
- .NET 7.0 or later
- `extract-xiso.exe` placed in the same folder as the application

## Installation

1. Download the latest release from [Releases Page] *(add your actual link here)*
2. Extract the ZIP file to your preferred location
3. Download `extract-xiso.exe` and place it in the same folder as the application
4. Run `BatchConvertIsoToXiso.exe`

## Usage

1. **Select Input Folder**: Click "Browse" to select the folder containing your ISO files
2. **Select Output Folder**: Choose where you want the converted XISO files to be saved
3. **Options**:
   - Check "Delete original files after conversion" if you want to remove source files
4. **Start Conversion**: Click "Start Conversion" to begin the batch process
5. **Monitor Progress**: View real-time logs and progress bar
6. **Completion**: You'll receive a summary when all files are processed

## Troubleshooting

### Common Issues

1. **extract-xiso.exe not found**
   - Ensure `extract-xiso.exe` is in the same folder as the application
   - Download the latest version from the official source

2. **Conversion failures**
   - Check the log for specific error messages
   - Verify your ISO files are not corrupted
   - Ensure you have sufficient disk space

3. **Permission errors**
   - Run the application as Administrator
   - Ensure you have write permissions to the output folder

## Privacy Notice

This application includes optional error reporting that sends:
- Application logs
- System information
- Error details

No personal files or data are collected.

## Building from Source

1. Clone the repository
2. Open the solution in Visual Studio 2022 or later
3. Restore NuGet packages
4. Build the solution

## License

GPL-3.0

## Credits

- Uses `extract-xiso` for conversion (include proper attribution)
- Application icon by [Icon Author] *(if applicable)*

---

For support or feature requests, please [open an issue](your-repo-link/issues).

```