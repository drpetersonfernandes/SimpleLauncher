namespace SimpleLauncher.AdminAPI.Models.DTOs;

public class SystemConfigurationDto
{
    public required string SystemName { get; set; }
    public string? SystemFolder { get; set; }
    public string? SystemImageFolder { get; set; }
    public bool SystemIsMame { get; set; }
    public List<string>? FileFormatsToSearch { get; set; }
    public bool ExtractFileBeforeLaunch { get; set; }
    public List<string>? FileFormatsToLaunch { get; set; }
    public EmulatorsConfigDto? Emulators { get; set; }
}

public class EmulatorsConfigDto
{
    public EmulatorConfigDto? Emulator { get; set; }
}

public class EmulatorConfigDto
{
    public string? EmulatorName { get; set; }
    public string? EmulatorLocation { get; set; }
    public string? EmulatorParameters { get; set; }
    public string? EmulatorDownloadPage { get; set; }
    public string? EmulatorLatestVersion { get; set; }
    public string? EmulatorDownloadLink { get; set; }
    public string? EmulatorDownloadExtractPath { get; set; }
    public string? CoreLocation { get; set; }
    public string? CoreLatestVersion { get; set; }
    public string? CoreDownloadLink { get; set; }
    public string? CoreDownloadExtractPath { get; set; }
    public string? ImagePackDownloadLink { get; set; }
    public string? ImagePackDownloadLink2 { get; set; }
    public string? ImagePackDownloadLink3 { get; set; }
    public string? ImagePackDownloadLink4 { get; set; }
    public string? ImagePackDownloadLink5 { get; set; }
    public string? ImagePackDownloadExtractPath { get; set; }
}