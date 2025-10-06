using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SimpleLauncher.Models;

/// <summary>
/// Represents the full response from the API_GetTicketData.php endpoint when requesting most ticketed games.
/// </summary>
public class RaMostReportedGamesResponse
{
    [JsonPropertyName("MostReportedGames")]
    public List<RaMostReportedGame> MostReportedGames { get; set; } = [];

    [JsonPropertyName("URL")]
    public string Url { get; set; } = string.Empty;
}