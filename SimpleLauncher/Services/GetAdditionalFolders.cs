using System;
using System.IO;
using System.Text.Json;
using System.Linq;

namespace SimpleLauncher.Services;

public static class GetAdditionalFolders
{
    public static string[] GetFolders()
    {
        try
        {
            // Adjust the path if needed
            var jsonText = File.ReadAllText("appsettings.json");
            var jsonDocument = JsonDocument.Parse(jsonText);

            if (jsonDocument.RootElement.TryGetProperty("AdditionalFolders", out var foldersElement) &&
                foldersElement.ValueKind == JsonValueKind.Array)
            {
                return foldersElement.EnumerateArray()
                    .Select(element => element.GetString())
                    .Where(folder => folder != null)
                    .ToArray();
            }

            return Array.Empty<string>();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Failed to get additional folders.");

            return Array.Empty<string>();
        }
    }
}