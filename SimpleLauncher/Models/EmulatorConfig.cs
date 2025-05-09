namespace SimpleLauncher.Models;

public class EmulatorConfig
{
    public string EmulatorName { get; set; }
    public string EmulatorLocation { get; set; }
    public string EmulatorParameters { get; set; }
    public string EmulatorDownloadPage { get; set; }
    public string EmulatorLatestVersion { get; set; }
    public string EmulatorDownloadLink { get; set; }
    public string EmulatorDownloadExtractPath { get; set; }
    public string CoreLocation { get; set; }
    public string CoreLatestVersion { get; set; }
    public string CoreDownloadLink { get; set; }
    public string CoreDownloadExtractPath { get; set; }
    public string ExtrasLocation { get; set; }
    public string ExtrasLatestVersion { get; set; }
    public string ExtrasDownloadLink { get; set; }
    public string ExtrasDownloadExtractPath { get; set; }
}