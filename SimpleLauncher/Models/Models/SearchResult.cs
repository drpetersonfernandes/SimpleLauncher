namespace SimpleLauncher.Models;

public class SearchResult
{
    public string FileName { get; init; }
    public string FileNameWithExtension { get; init; }
    public string MachineName { get; init; }
    public string FolderName { get; init; }
    public string FilePath { get; init; }
    public double Size { get; set; }
    public string SystemName { get; init; }
    public SystemManager.Emulator EmulatorConfig { get; init; }
    public int Score { get; set; }
    public string CoverImage { get; init; }
    public string DefaultEmulator => EmulatorConfig?.EmulatorName ?? "No Default Emulator";
}