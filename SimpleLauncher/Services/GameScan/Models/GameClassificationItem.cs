namespace SimpleLauncher.Services.GameScan.Models;

public class GameClassificationItem
{
    public string Name { get; set; } = "";
    public string AppId { get; set; } = null!;
    public string InstallLocation { get; set; } = null!;
    public string PackageFamilyName { get; set; } = null!;
    public string LogoRelativePath { get; set; } = null!;
}
