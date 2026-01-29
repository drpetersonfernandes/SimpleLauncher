using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;

namespace SimpleLauncher.Services.LoadAppSettings;

public static class GetImageExtensions
{
    public static string[] GetExtensions()
    {
        try
        {
            // Adjust the path if needed
            var jsonText = File.ReadAllText("appsettings.json");
            var jsonDocument = JsonDocument.Parse(jsonText);

            if (jsonDocument.RootElement.TryGetProperty("ImageExtensions", out var imageExtensionsElement) &&
                imageExtensionsElement.ValueKind == JsonValueKind.Array)
            {
                return imageExtensionsElement.EnumerateArray()
                    .Select(static e => e.GetString())
                    .Where(static s => s != null)
                    .ToArray();
            }

            return Array.Empty<string>();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "Failed to get image extensions.");
            return Array.Empty<string>();
        }
    }
}