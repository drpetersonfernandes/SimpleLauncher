using System.ComponentModel.DataAnnotations;

namespace SimpleLauncher.AdminAPI.Models;

public class EmulatorConfiguration
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string EmulatorName { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? EmulatorLocation { get; set; }

    [MaxLength(1000)]
    public string? EmulatorParameters { get; set; }

    [MaxLength(500)]
    public string? EmulatorDownloadPage { get; set; }

    [MaxLength(50)]
    public string? EmulatorLatestVersion { get; set; }

    [MaxLength(500)]
    public string? EmulatorDownloadLink { get; set; }

    [MaxLength(500)]
    public string? EmulatorDownloadExtractPath { get; set; }

    [MaxLength(500)]
    public string? CoreLocation { get; set; }

    [MaxLength(50)]
    public string? CoreLatestVersion { get; set; }

    [MaxLength(500)]
    public string? CoreDownloadLink { get; set; }

    [MaxLength(500)]
    public string? CoreDownloadExtractPath { get; set; }

    [MaxLength(500)]
    public string? ImagePackDownloadLink { get; set; }

    [MaxLength(500)]
    public string? ImagePackDownloadLink2 { get; set; }

    [MaxLength(500)]
    public string? ImagePackDownloadLink3 { get; set; }

    [MaxLength(500)]
    public string? ImagePackDownloadLink4 { get; set; }

    [MaxLength(500)]
    public string? ImagePackDownloadLink5 { get; set; }

    [MaxLength(500)]
    public string? ImagePackDownloadExtractPath { get; set; }

    // Foreign Key to SystemConfiguration
    public int SystemConfigurationId { get; set; }
    public SystemConfiguration SystemConfiguration { get; set; } = null!;
}