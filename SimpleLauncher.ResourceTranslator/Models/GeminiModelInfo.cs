namespace SimpleLauncher.ResourceTranslator.Models;

public class GeminiModelInfo
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public int ContextLength { get; set; }
    public string ApiVersion { get; set; } = "v1beta";
}
