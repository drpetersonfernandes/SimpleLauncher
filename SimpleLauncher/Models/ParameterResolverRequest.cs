namespace SimpleLauncher.Models;

public class ParameterResolverRequest
{
    public string SystemName { get; set; } = "";
    public string SystemFolder { get; set; } = "";
    public List<string> FileFormatsToSearch { get; set; } = [];
    public bool ExtractFileBeforeLaunch { get; set; }
    public List<string> FileFormatsToLaunch { get; set; } = [];
    public bool GroupByFolder { get; set; }
    public bool DisableRecursiveSearch { get; set; }
    public string EmulatorName { get; set; } = "";
    public string EmulatorPath { get; set; } = "";
    public string CurrentParameters { get; set; } = "";
}