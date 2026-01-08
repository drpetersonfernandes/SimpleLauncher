namespace SimpleLauncher.Models.GameScanLogic;

public class SelectableGameItem
{
    public string Name { get; set; }
    public string AppId { get; set; }
    public string InstallLocation { get; set; }
    public string PackageFamilyName { get; set; }
    public string LogoRelativePath { get; set; }
    public bool IsSelected { get; set; }
}