namespace SimpleLauncher.ResourceTranslator.Models;

/// <summary>
///     Represents information about an available DeepSeek AI model.
/// </summary>
public class DeepSeekModelInfo
{
    /// <summary>
    ///     Gets or sets the model identifier.
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
