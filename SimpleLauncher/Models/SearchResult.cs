using SimpleLauncher.Managers;
using SimpleLauncher.Services;

namespace SimpleLauncher.Models;

public class SearchResult
{
    public string FileName { get; init; }
    public string FileNameWithExtension { get; init; }
    public long FileSizeBytes { get; set; }
    public string MachineName { get; init; }
    public string FolderName { get; init; }
    public string FilePath { get; init; }
    public string SystemName { get; init; }
    public SystemManager.Emulator EmulatorConfig { get; init; }
    public int Score { get; set; }
    public string CoverImage { get; init; }
    public string DefaultEmulator => EmulatorConfig?.EmulatorName ?? "No Default Emulator";
    public string FormattedFileSize => FormatFileSize.Format(FileSizeBytes);
}