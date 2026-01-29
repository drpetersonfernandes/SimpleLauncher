using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.LoadAppSettings;

public static class GetAdditionalFolders
{
    public static IEnumerable<string> GetFolders()
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
                    .Select(static element => element.GetString())
                    .Where(static folder => folder != null)
                    .ToArray();
            }

            return Array.Empty<string>();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to get additional folders.");

            return Array.Empty<string>();
        }
    }
}