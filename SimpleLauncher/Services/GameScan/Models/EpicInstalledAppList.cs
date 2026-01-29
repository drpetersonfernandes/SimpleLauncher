using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Services.GameScan.Models;

// DTOs for Epic's LauncherInstalled.dat
public class EpicInstalledAppList
{
    [JsonPropertyName("InstallationList")]
    public List<EpicInstalledApp> InstallationList { get; set; }
}