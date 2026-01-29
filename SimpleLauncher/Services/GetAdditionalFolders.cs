using System;
using System.IO;
using System.Text.Json;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

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