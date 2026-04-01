using System.Collections.Generic;

namespace SimpleLauncher.Services.GameScan.Models;

public class GameClassificationResponse
{
    public List<GameClassificationItem> Games { get; set; } = [];
}