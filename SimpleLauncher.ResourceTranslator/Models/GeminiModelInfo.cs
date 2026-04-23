namespace SimpleLauncher.ResourceTranslator.Models;

public class GeminiModelInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int ContextLength { get; set; }
    public string ApiVersion { get; set; } = "v1beta";
}
