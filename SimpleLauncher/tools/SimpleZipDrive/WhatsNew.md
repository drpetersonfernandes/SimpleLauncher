# What's New

## 2.9.0

### Fixed
- **Halved peak memory during memory-cache decompression.** Decompression now streams directly into the final exact-size buffer instead of building a growing `MemoryStream` and copying it with `ToArray()`. A 424 MB entry now peaks at roughly one copy of the data (first-open working set ~460 MB, down from ~880 MB) — this addresses the memory-inflation follow-up reported in issue #10: the previous transient double copy made Task Manager show about twice the true footprint, and any second transient allocation could look like memory "doubling" between runs.
- **Bug-report filtering no longer masks potential real bugs.** Only canceled HTTP requests are treated as expected user errors; other network failures and `CryptographicException`s that do not originate from archive decryption (SharpCompress) are reported to the bug API again. The update check remains quiet on offline machines because it handles its own network errors.
- **Corrupt RAR archives are no longer misclassified as wrong-password prompts.** A "Unknown Rar Header" failure during an encrypted mount now routes to the corruption path instead of re-prompting for a password up to three times with a misleading message.
- **Duplicate log suppression restored to case-sensitive comparison**, so two distinct messages that differ only in case are both kept.
- **The archive open retry backoff no longer blocks the UI thread.** Waiting for a transiently locked archive file uses an awaited delay instead of `Thread.Sleep`.
- **WinFsp variant: fixed mount failure "The path is empty" in the packaged single-file build.** The winfsp-msil interop's static initializer calls `FileVersionInfo.GetVersionInfo(Assembly.Location)`, which is an empty string inside a single-file bundle. `winfsp-msil.dll` is now shipped as a real file beside the executable so the interop version check succeeds.
- **WinFsp variant: fixed cross-integrity folder mounts on fresh directories.** `IsDriveLetterMountPoint` misclassified every absolute folder path (`C:\...`) as a drive letter, so the mount-point directory was never created and mounting failed with `0xC0000034`. A bare `M` or `M:` is a drive letter; any longer path is a folder and is created on demand.

### Changed
- **Consistent packaging for both variants.** The Dokan and WinFsp executables ship with the native `7z.dll` / `7z_arm64.dll` fallback libraries (and `winfsp-msil.dll` for the WinFsp variant) as real files beside the executable, so the SevenZip extraction fallback is available in both.

### Changed
- **Update check now uses the canonical repository endpoint** (`https://github.com/purelogiccode/SimpleZipDrive`). The fallback endpoint pointing at the previous repository owner was removed now that the GitHub transfer is complete.
- **Framework-dependent single-file executables.** Releases are now built without the .NET runtime embedded — only the .NET Desktop Runtime and the filesystem driver (Dokan or WinFsp) remain prerequisites. Each package contains the single `exe`, the native `7z.dll` / `7z_arm64.dll` fallback libraries (and `winfsp-msil.dll` for the WinFsp variant) beside it, plus this README, the license, and these release notes.

### Internal
- ReDoS-safe match timeouts added to all regular expressions.
- Lock objects migrated to `System.Threading.Lock`.
- Solution reorganized with dedicated `Models/` and `Interfaces/` folders; `ErrorLogger` and `StoredEntryStream` split into per-type files; all analyzer warnings resolved.
- SharpCompress updated to 0.50.4 and SharpSevenZip to 2.0.115.

## 2.8.0

### Fixed
- **Shared, refcounted memory cache for decompressed entries with LRU eviction** (issue #10). Each entry is decompressed once and the buffer is shared by every open handle and on-demand read; buffers stay warm after the last handle closes and are evicted least-recently-used under memory pressure, falling back to the disk cache. This eliminated the one-full-decompression-per-open behavior (memory scaling as concurrent opens × file size) and the re-decompression-per-read path used by paging I/O.
- Graceful handling of entry-semaphore disposal during shutdown.
- WinFsp variant aligned with stable WinFsp 2.1 (the 2.2.x driver packages were beta releases).
