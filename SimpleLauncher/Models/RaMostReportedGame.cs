using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents a game with open achievement tickets from the API_GetTicketData.php endpoint.
/// </summary>
public class RaMostReportedGame
{
    [JsonPropertyName("GameID")]
    public int GameId { get; set; }

    [JsonPropertyName("GameTitle")]
    public string GameTitle { get; set; } = string.Empty;

    [JsonPropertyName("GameIcon")]
    public string GameIcon { get; set; } = string.Empty; // This will be the full URL after processing in the service

    [JsonPropertyName("Console")]
    public string Console { get; set; } = string.Empty;

    [JsonPropertyName("OpenTickets")]
    public int OpenTickets { get; set; }
}