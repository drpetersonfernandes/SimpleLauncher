namespace SimpleLauncher.Models;

public class BNetAppDef
{
    public string InternalId { get; set; } = null!;
    public string Name { get; set; } = null!;
    public bool IsClassic { get; set; }
    public string Exe { get; set; } = null!;
    public string ProductId { get; set; } = null!;
}
