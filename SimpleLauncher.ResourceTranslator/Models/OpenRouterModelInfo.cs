namespace SimpleLauncher.ResourceTranslator.Models;

/// <summary>
///     Represents information about an available OpenRouter AI model.
/// </summary>
public class OpenRouterModelInfo
{
    /// <summary>
    ///     Gets or sets the model identifier (e.g. "deepseek/deepseek-v4-flash").
    /// </summary>
    public string Id { get; set; } = "";

    /// <summary>
    ///     Gets or sets the display name of the model.
    /// </summary>
    public string Name { get; set; } = "";

    /// <summary>
    ///     Gets or sets the description of the model.
    /// </summary>
    public string Description { get; set; } = "";

    /// <summary>
    ///     Gets or sets the maximum context length in tokens.
    /// </summary>
    public int ContextLength { get; set; }
}