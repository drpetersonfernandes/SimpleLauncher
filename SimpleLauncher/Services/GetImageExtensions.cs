using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SimpleLauncher.Services;

public static class GetImageExtensions
{
    public static string[] GetExtensions()
    {
        try
        {
            // Adjust the path if needed
            var jsonText = File.ReadAllText("appsettings.json");
            var jObject = JObject.Parse(jsonText);
            var foldersArray = jObject["ImageExtensions"] as JArray;
            return foldersArray?.Select(static f => f.ToString()).ToArray() ?? Array.Empty<string>();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Failed to get image extensions.");

            return Array.Empty<string>();
        }
    }
}