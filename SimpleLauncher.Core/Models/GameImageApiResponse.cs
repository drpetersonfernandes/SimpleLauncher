using System.Text.Json.Serialization;

namespace SimpleLauncher.Core.Models;

/// <summary>
/// Represents the response from the game image API containing a cover image URL.
/// </summary>
public class GameImageApiResponse
{
    /// <summary>
    /// Gets or sets whether the image API request succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets the URL of the cover image returned by the API.
    /// </summary>
    [JsonPropertyName("imageUrl")]
    public string ImageUrl { get; set; } = null!;
}
