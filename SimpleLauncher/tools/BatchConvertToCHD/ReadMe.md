[![Platform: Windows](https://img.shields.io/badge/Platform-Windows%20x64%20%7C%20ARM64-0078d7.svg)](https://www.microsoft.com/windows)
[![.NET 10.0](https://img.shields.io/badge/.NET-10.0-512bd4.svg)](https://dotnet.microsoft.com/download/dotnet/10.0)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE.txt)
[![GitHub release](https://img.shields.io/github/v/release/purelogiccode/BatchConvertToCHD)](https://github.com/purelogiccode/BatchConvertToCHD/releases)

# Batch Convert to CHD

**Batch Convert to CHD** is a high-performance Windows desktop utility designed to streamline the conversion of various disk image formats into the **Compressed Hunks of Data (CHD)** format.

![Batch Convert to CHD Screenshot](screenshot.png)
![Batch Convert to CHD Screenshot](screenshot2.png)
![Batch Convert to CHD Screenshot](screenshot3.png)

## 🚀 Key Features

### 💻 Modern Side-by-Side Dashboard
*   **Dual-Pane Interface**: View your settings and file list on the left, while monitoring real-time process logs on the right.
*   **Interactive File Selection**: Automatically scans folders and allows you to manually pick exactly which files to process via a detailed file list.
*   **Optimized File Loader**: Utilizes a chunked loading strategy to maintain UI responsiveness even when scanning directories with thousands of files.
*   **Resizable Layout**: Includes a built-in grid splitter to adjust the balance between the file explorer and the terminal view.

### 💻 Multi-Architecture Support
*   **Native ARM64 & x64**: Automatically detects your system architecture and utilizes the appropriate `CHDSharp`/`chdman` binaries for conversion for maximum efficiency.
*   **OS-Native Tool Selection**: On ARM64 machines the native `CHDSharp_arm64.exe` and `chdman_arm64.exe` are preferred even when the app itself runs emulated as x64, and a missing preferred binary falls back to the other architecture's build instead of failing.
*   **Optimized Performance**: Leverages native instructions on ARM64 hardware to reduce overhead during heavy compression tasks.

### 🛠️ Intelligent Conversion & Extraction
*   **Automated Batch Processing**: Convert entire directories of disk images with real-time progress monitoring and immediate cancellation response.
*   **chdman Primary Encoding**: Conversions run on the bundled `chdman`, the reference MAME CHD encoder. If chdman fails, the conversion automatically falls back to [CHDSharp](https://www.nuget.org/packages/CHDSharp) (v1.4.3), the project's own managed CHD encoder whose output is byte-identical to `chdman` (verified across a 56-disc battle corpus), and a batch only refuses to start when neither encoder is present.
*   **Recursive Structure Preservation**: Maintains your original directory hierarchy in the output folder when processing subfolders.
*   **Robust Extraction**: Supports extracting CHD files back to **.cue (CD)**, **.iso (DVD)**, **.gdi (Dreamcast/Naomi)**, and **.img (HDD)** with intelligent metadata auto-detection using the [CHDSharp](https://www.nuget.org/packages/CHDSharp) library. If the built-in reader cannot decode a CHD (corrupt file or A/V laserdisc CHD), extraction automatically falls back to `chdman` — including `extractld` (AVI) / `extractraw` for laserdisc CHDs.
*   **Archive Integration**: Transparently handles `.zip`, `.7z`, and `.rar` archives, extracting and processing contents automatically while respecting cancellation tokens. Includes a 7za.exe fallback for `.7z` files that SharpCompress cannot extract.
*   **CloneCD Support**: Convert CloneCD `.ccd` disc images to CHD format via the [CCDSharp](https://) library. Automatically generates CUE/BIN from `.ccd`/`.img` sets.
*   **CSO Decompression**: Built-in support for `.cso` and `.ciso` (Compressed ISO) files via the [CSOSharp](https://github.com/PureLogicCode/CSOSharp) library (supports deflate/zlib and LZ4).
*   **PBP Extraction**: Convert PlayStation Portable `.pbp` files to CHD format via the [PBPSharp](https://github.com/PureLogicCode/PBPSharp) library. Files without a PlayStation disc image (PSP homebrew applications, unsupported or corrupt variants) are detected and skipped with a clear message instead of a generic failure.
*   **Smart CUE Normalization**: Detects the actual encoding of `.cue`/`.toc` files (UTF-8, Shift-JIS, Korean CP949, Cyrillic CP1251, Latin-1 and more), strips UTF-8 BOMs (which chdman's parser cannot handle — they produced the "couldn't find bin file []" error), resolves referenced files case-insensitively and zero-padding-tolerantly (`(Track 2)` vs `(Track 02)`), and hands chdman a self-contained, canonicalized cue set — eliminating the common "couldn't find bin file" failures on non-ASCII and BOM-prefixed cues. Bins are referenced in place via relative paths when possible, so no multi-hundred-MB copies are needed for BOM-only cues.
*   **Raw Audio Track Handling**: Cue files referencing `.raw` audio tracks (common in multi-track CD audio rips) are automatically detected via `GameFileParser` and the `-us 2352` unit size flag is passed to chdman, preventing the "Unit size must be specified if no output parent CHD is supplied" error.
*   **Archive Dependency Validation**: Cues, GDI and TOC files extracted from archives are validated before conversion — if the referenced data files are missing from the archive (incomplete download, separate bin archive), the entry is skipped with a clear warning instead of failing inside chdman.
*   **MP3 Audio Track Support**: Cue sheets with MP3 audio tracks — `cue/bin/mp3` and `cue/iso/mp3` sets (common in Neo Geo CD and older PS1 rips) — are automatically decoded to WAV before conversion, because chdman cannot read MP3 tracks. The decoded WAVs are normalized to chdman's exact requirements (44.1 kHz, stereo, 16-bit PCM), with a built-in decoder fallback for systems without Media Foundation.
*   **bin-only Archives**: Archives that contain only `.bin` files (no `.cue`/`.iso` descriptor) now get an auto-generated cue and convert automatically (MODE2/2352 with automatic MODE1/2352 fallback).

### 🔎 Content-Based Format Detection
A file's extension is the least reliable thing about it. Every input is identified by its leading bytes before the extension is trusted, which turns several "corrupt file" failures into successful conversions.

*   **Raw CD Dumps With the Wrong Name**: A 2352-bytes-per-sector CD dump saved as `.iso`, `.img`, `.bin` or even `.isz` is recognised from its sector sync mark and mode byte, and converted as the CD it is with a generated cue. Previously these went to `createdvd` or `createhd` and failed on `Data size ... is not divisible by sector size`.
*   **Disc Images Wearing an Archive Extension**: A `.rar` or `.zip` that is really a plain disc image, or a byte-split set that was never an archive, is detected and converted instead of being reported as corrupt.
*   **Bare `.bin` Files**: A raw `.bin` with no descriptor is accepted as an input and given a generated cue. When a sibling `.cue`/`.ccd`/`.mds` already covers it, the `.bin` is dropped from the batch so the disc converts once, through its descriptor.
*   **Honest Reporting**: A truncated download, a file with the wrong extension and a genuinely unsupported format read differently in the log, each naming what was found and what to do about it.

### 💿 Awkward Format Support
*   **Alcohol 120%**: `.mds`/`.mdf` sets convert directly. The descriptor's track table is parsed to build a matching cue, images storing 2448 or 2368 bytes per sector have their subchannel tail stripped first (chdman cannot read those), and a `.mdf` that is really an ISO is converted as a DVD image.
*   **ISZ Decompression**: UltraISO `.isz` images are decompressed in-process (zlib, bzip2, stored and zero chunks), including images split across `.i01`, `.i02` and further segments. Segments are matched by volume serial number, a missing one is named, and an encrypted image says so rather than failing obscurely. Written against the EZB Systems ISZ File Format Specification 1.00.
*   **Split Volume Sets**: Images split into `.001`/`.002` or `.i00`/`.i01` pieces are rejoined before conversion, and a set with a missing part is reported as such instead of being handed to chdman half-complete. Only the first volume appears in the file list, so a set is offered once rather than once per piece.
*   **ECM Decoding**: `.ecm` files are decoded in-process, with no external tool to install. ECM works by discarding each sector's EDC checksum and Reed-Solomon parity, so decoding means regenerating them; the implementation is verified byte for byte against Neill Corlett's original encoder and decoder, and the checksum ECM stores for the whole image is validated at the end, so a damaged file is reported rather than turned into a plausible-looking one.
*   **Split-Track Discs**: A `(Track 1)`, `(Track 2)`, ... bin set gets a multi-track cue, so discs with CDDA audio keep their audio tracks instead of converting as a single data track.
*   **Broken Cue Descriptors**: A cue whose `FILE` line names something that is not there is resolved against what is actually on disk, by name beside the cue, by swapping the extension, and for a single-file cue by elimination. Audio tracks and multi-file cues are left alone, because guessing there could silently drop a track.

### ✅ Integrity, Safety & Verification
*   **A Good CHD Is Never Destroyed**: Conversions are written to a staging file and moved into place only after success. Because chdman truncates its output file before it can fail, a second input that maps to the same output name can no longer wipe out the working CHD produced by the first.
*   **Output Collision Warnings**: The output `.chd` name comes from the input's base name, so `Game.cue`, `Game.zip` and `Game.ccd` in one folder all target `Game.chd`. Inputs that would collide are reported at the start of the batch, before any time is spent on them.
*   **Convert and Extract In Place**: The output folder can be the same as the source folder, or inside it. Conversion is safe there by construction: the output is always `<name>.chd`, which is never itself an input, and it is written to a staging file so an existing CHD is only replaced after success. Extraction takes the CHD's base name, so when its output would land on files that already exist, the whole disc is written to a subfolder named after it instead — existing files are never overwritten, nothing has to be confirmed, and discs with nothing in their way still land directly in the output folder.
*   **Disk Space Preflight**: Free space on the output drive is checked immediately before chdman starts. Clearly insufficient space skips the file with both figures named, rather than discovering the problem an hour into a large conversion.
*   **Output Folder Preflight**: The destination is probed for write access before a batch starts, so an unwritable folder (e.g. inside `Program Files` without elevation) produces one clear message instead of a run of per-file "Permission denied" failures.
*   **chdman-Safe Path Handling**: Non-ASCII characters anywhere along a path (`C:\Users\Kauê Chacon\...`, `D:\Emulátory\...`) and paths at or beyond the 260-character MAX_PATH limit are routed through short ASCII staging directories — older chdman builds mangle or cannot open such paths and fail with a misleading "No such file or directory". Cue work directories avoid a non-ASCII system temp folder the same way.
*   **Crash-Aware Error Reporting**: When Windows kills chdman outright (e.g. exit code `-1073741795` = `0xC000001D`, illegal instruction — typically an older CPU missing instructions the bundled build requires), the code is decoded into plain language with actionable guidance. A startup check refuses to begin a batch that would fail on every file, and startup logs record the process/OS architectures plus which tool binary was selected.
*   **Safe Deletion**: Source files (and their dependencies like `.bin`, `.sub`, etc.) are only deleted if the conversion/extraction is confirmed successful.
*   **Batch Verification**: Validate the checksums and structural integrity of existing CHD files using the [CHDSharp](https://www.nuget.org/packages/CHDSharp) library.
*   **Automated Organization**: Optionally move verified or failed files into dedicated subfolders (`Success`/`Failed`) while ignoring these special folders during subsequent scans.
*   **Cleanup**: Automatically removes empty subdirectories left behind after files are moved or deleted.
*   **Dependency Protection**: Performs a critical dependency check on startup to notify you if required components (like `chdman.exe`, needed for conversion) are missing.
*   **File System Monitoring**: Automatically monitors the input folder for file changes (deletions, renames, creations) during batch processing and provides diagnostic context when a file goes missing mid-operation.
*   **Corrupt Image Detection**: Warns early when a disc image's size does not match any standard sector layout, so you can spot truncated or corrupt files before the conversion runs.
*   **Resilient File Deletion**: Source-file deletion retries with backoff for up to ~45 seconds (handles transient antivirus/file-explorer locks) and automatically clears the read-only attribute when needed.
*   **Resilient File Copy**: CloneCD `.img` → `.bin` copies retry with backoff (4 attempts), preventing transient locking failures during conversion.
*   **Clear Error Messages**: Precise, actionable messages for data-side failures — missing volumes in multi-part RAR archives, disconnected network drives, and locked files — instead of generic errors.

### 📊 Performance & UI
*   **Real-time Telemetry**: Monitor disk write/read speeds and elapsed time during operations.
*   **Optimized Logging**: High-performance logging system with automatic truncation to keep the application responsive during long-running tasks.
*   **AppData Storage**: Logs and F8 screenshots are stored under `%LocalAppData%\BatchConvertToCHD` (`logs` / `screenshots`); the title-bar **AppData** button opens the folder.
*   **WPF-UI Theming**: Modern dark-themed UI powered by [WPF-UI](https://github.com/lepoco/wpfui) with a static dark background, rounded corners, and native Windows 11 aesthetics.

### 🔄 Updates & Stability
*   **Automatic Update Checks**: Notifies you immediately if a newer version is available on GitHub at startup.
*   **Automated Bug Reporting**: Built-in error reporting system helps improve the application by automatically sending crash reports (no personal data collected). Known OS-level issues (e.g. WPF tooltip accessibility-bridge failures) and user-data conditions (corrupt files, chdman's own failures, stats API rate limits) are filtered out automatically, while genuine application defects — including CHDSharp/PBPSharp extraction failures (with debug details) — still reach the developer. A safety timer prevents the report throttle from hanging indefinitely on network issues.

---

## 📂 Supported Formats

| Category             | Formats                                                                          |
|:---------------------|:---------------------------------------------------------------------------------|
| **Standard Images**  | `.iso`, `.cue` (+`.bin`), `.img`, `.ccd` (+`.img`), `.raw`, `.toc`, bare `.bin`   |
| **Console Specific** | `.gdi` (Dreamcast), `.pbp` (PlayStation)                                          |
| **Compressed**       | `.cso` (Compressed ISO), `.isz` (UltraISO), `.ecm` (Error Code Modeler)           |
| **Alcohol 120%**     | `.mds` (+`.mdf`), including 2448-byte subchannel sectors                           |
| **Split Sets**       | `.001`/`.002`..., `.i00`/`.i01`... (add the first volume; the rest are found)       |
| **Archives**         | `.zip`, `.7z`, `.rar`                                                             |
| **Output**           | `.chd` (Compressed Hunks of Data)                                                 |

Only the descriptor or first volume of a multi-file set is listed for conversion. The `.mdf` behind a `.mds`, the `.bin` behind a `.cue`, and the later parts of a split set are found automatically, so each disc is converted once.

---

## 🛠️ Technical Logic

The application implements priority-based logic to ensure compatibility. Content is inspected first, and the extension only decides the outcome for files whose content did not settle it:

1.  **Content Inspection**: The leading bytes are read. A raw CD image, an Alcohol descriptor, an ISZ, an ECM, an archive or an existing CHD is routed on what it is, whatever it is called. A raw CD image gets a generated cue and goes to `createcd`.
2.  **Split Volume Sets**: Numbered pieces are rejoined into a single image, which is then classified as above.
3.  **Compressed Containers**: `.isz`, `.ecm` and `.cso` are all decompressed in-process, and the restored image is classified as above.
4.  **Descriptors**: `.cue`, `.gdi` and `.toc` go to `createcd` after cue normalization. `.ccd` becomes a cue via CCDSharp, `.mds` via the Alcohol parser, and `.pbp` is extracted to CUE/BIN via PBPSharp.
5.  **DVD Images (`.iso`)**: Defaults to `createdvd`, once content inspection has ruled out a mislabelled raw CD dump.
6.  **Hard Disk Images (`.img`)**: Defaults to `createhd` unless an accompanying `.cue` file is detected, in which case `createcd` is used.
7.  **Raw Data (`.raw`)**: Defaults to `createraw`. Cue descriptors referencing `.raw` audio tracks also receive `-us 2352` automatically.

Generated cue sheets reference the disc image where it already lies rather than copying it, because chdman resolves a cue's `FILE` entry against the cue's own directory. That also means such a cue has to be written on the same volume as the image, since chdman cannot follow an absolute `FILE` path.

*Note: Users can manually override these settings via the UI to force specific modes (except for PBP which always extracts first).*

---

## 💻 Requirements

*   **Operating System**: Windows 10 / 11 (x64 or ARM64)
*   **Runtime**: [.NET 10.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)
*   **Bundled Dependencies**:
    *   `chdman.exe` / `chdman_arm64.exe` (MAME Project — conversion only)
    *   `7za.exe` / `7za_arm64.exe` (7-Zip fallback extraction)
*   **No Other Dependencies**: every format above is handled inside the application. There is nothing else to download, and both x64 and ARM64 get the same feature set.
*   **Library Dependencies**:
    * [WPF-UI](https://github.com/lepoco/wpfui) (v4.3.0) — Modern Fluent Design theming and controls
    * [CHDSharp](https://www.nuget.org/packages/CHDSharp) (v1.4.3) — Pure C# CHD reading, verification, extraction, and creation (chdman byte-identical output)
    * [CSOSharp](https://) (v1.0.0) — Pure C# CSO/CISO decompression (deflate + LZ4)
    * [PBPSharp](https://) (v1.0.0) — Pure C# PBP extraction and SFO parsing
    * [CCDSharp](https://) (v1.0.0) — Pure C# CloneCD (.ccd/.img/.sub) parsing and conversion
    * [Alcohol120Sharp](https://) (v1.0.0) — Pure C# Alcohol 120% (.mds/.mdf) parsing and cue preparation
    * [UltraIsoSharp](https://) (v1.0.0) — Pure C# UltraISO ISZ decompression
    * [SharpCompress](https://github.com/adamhathcock/sharpcompress) (v0.50.4) — Archive extraction, and bzip2 decompression for ISZ images
    * [NAudio](https://github.com/naudio/NAudio) (v3.1.0) — MP3 audio track decoding (Media Foundation)
    * [Serilog](https://serilog.net/) (v4.4.0) — Structured diagnostic logging

---

## 📥 Installation

1.  Download the latest binary from the [Releases](https://github.com/purelogiccode/BatchConvertToCHD/releases) page.
2.  Extract the contents to a permanent folder.
3.  **Important**: Ensure all `.exe` files (including ARM64 variants) remain in the same directory as `BatchConvertToCHD.exe`.
4.  Launch the application.

---

## 📖 Usage

The application also accepts a folder path as a command-line argument to quickly populate the source directory:
```sh
BatchConvertToCHD.exe "C:\ROMs\MyGames"
```

### Conversion Workflow
1.  Navigate to the **Convert to CHD** tab.
2.  Select your **Source Folder** (containing images or archives).
3.  Select your **Output Folder**.
4.  *(Optional)* Check "Process smaller files first" to sort by file size.
5.  *(Optional)* Check "Force CD" or "Force DVD" to override automatic command detection.
6.  *(Optional)* Set a time limit per file to abort conversions that exceed the specified duration.
7.  *(Optional)* Enable "Delete original files" to clean up source data after a successful conversion.
8.  Click **Start Conversion**.

### Extraction Workflow
1.  Navigate to the **Extract CHD Files** tab.
2.  Select your **Source Folder** (containing `.chd` files).
3.  Select your **Output Folder**.
4.  Choose the desired output format (Auto-detect, CD `.cue`, DVD `.iso`, Dreamcast `.gdi`, HDD `.img`).
5.  *(Optional)* Enable "Include subfolders" to process nested directories.
6.  *(Optional)* Enable "Delete original CHD files" to clean up after successful extraction.
7.  Click **Start Extraction**.

### Verification Workflow
1.  Navigate to the **Verify CHD Files** tab.
2.  Select the folder containing your `.chd` files.
3.  Configure folder organization options (Success/Failed folders).
4.  Click **Start Verification**.

---

## 📖 Documentation

The full project documentation (user guide, architecture, developer references, and troubleshooting) lives in the **[`docs/`](docs/index.md) wiki**:

* [Project Overview](docs/01-overview.md)
* [Getting Started](docs/02-getting-started.md)
* [Architecture](docs/03-architecture.md)
* [User Guide](docs/04-user-guide.md)
* [Conversion Pipeline](docs/05-conversion-pipeline.md)
* [Extraction & Verification](docs/06-extraction-and-verification.md)
* [Services Reference](docs/07-services-reference.md)
* [Utilities Reference](docs/08-utilities-reference.md)
* [Bug Reporting System](docs/09-bug-reporting.md)
* [Embedded Libraries](docs/10-libraries.md)
* [Testing](docs/11-testing.md)
* [Application Data](docs/12-application-data.md)
* [Troubleshooting](docs/13-troubleshooting.md)

The changelog for each release lives in [WhatsNew.md](WhatsNew.md).

---

## 🤝 Contributing & Support

If you encounter issues or have feature requests, please use the [GitHub Issues](https://github.com/purelogiccode/BatchConvertToCHD/issues) tracker.

**Support the Project:**
If this tool saves you time, consider supporting further development:
*   ⭐ **Star this repository** on GitHub.
*   ☕ **Donate**: [www.purelogiccode.com/donate](https://www.purelogiccode.com/donate)

---

## 📜 License

This project is licensed under the **GNU General Public License v3.0**. See the [LICENSE.txt](LICENSE.txt) file for details.

**Acknowledgements:**
*   [MAME Team](https://www.mamedev.org/) for `chdman`.
*   [CHDSharp](https://www.nuget.org/packages/CHDSharp) by Peterson Fernandes — Pure C# CHD library supporting V1-V5, all 10 codecs, parent/child chaining, parallel verification, and CHD creation that is byte-identical to `chdman` (verified across a 56-disc battle corpus).
*   [WPF-UI](https://github.com/lepoco/wpfui) by lepoco — Modern Windows 11 Fluent Design theming and controls.
*   [CSOSharp](https://) by Peterson Fernandes — Pure C# CSO/CISO decompression library.
*   [PBPSharp](https://) by Peterson Fernandes — Pure C# PlayStation PBP extraction library.
*   [CCDSharp](https://) by Peterson Fernandes — Pure C# CloneCD disc image parsing and conversion library.
*   [SharpCompress](https://github.com/adamhathcock/sharpcompress) for archive handling and bzip2 decompression.
*   [EZB Systems](https://www.ezbsystems.com/) for publishing the [ISZ File Format Specification](https://www.ezbsystems.com/isz/iszspec.txt), which the ISZ decompressor is written against.
*   Neill Corlett for ECM. His GPL-2.0-or-later reference implementation defines the format the in-process `.ecm` decoder implements, and was used to verify it byte for byte.
*   [NAudio](https://github.com/naudio/NAudio) by Mark Heath — MP3 decoding via Windows Media Foundation.
*   [Serilog](https://serilog.net/) for structured logging.
*   [Igor Pavlov](https://www.7-zip.org/) for `7za.exe` (7-Zip command-line tool).

---
Developed by [Pure Logic Code](https://www.purelogiccode.com)
