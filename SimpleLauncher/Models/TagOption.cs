namespace SimpleLauncher.Models;

/// <summary>
/// Represents a tag-display pair for use in ComboBox options throughout the application.
/// </summary>
/// <param name="Tag">The internal value/tag.</param>
/// <param name="Display">The user-facing display text.</param>
public record TagOption(string Tag, string Display);
