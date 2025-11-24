using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SimpleLauncher.AdminAPI.Models;

public class SystemConfiguration
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string SystemName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string Architecture { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? SystemFolder { get; set; }

    [MaxLength(500)]
    public string? SystemImageFolder { get; set; }

    public bool SystemIsMame { get; set; }
    public bool ExtractFileBeforeLaunch { get; set; }

    [MaxLength(500)]
    public string? FileFormatsToSearchDb { get; set; }

    [MaxLength(500)]
    public string? FileFormatsToLaunchDb { get; set; }

    [NotMapped]
    public List<string> FileFormatsToSearch
    {
        get => FileFormatsToSearchDb?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
        set => FileFormatsToSearchDb = string.Join(",", value);
    }

    [NotMapped]
    public List<string> FileFormatsToLaunch
    {
        get => FileFormatsToLaunchDb?.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
        set => FileFormatsToLaunchDb = string.Join(",", value);
    }

    // Navigation property for the one-to-one relationship
    public EmulatorConfiguration Emulator { get; set; } = null!;
}