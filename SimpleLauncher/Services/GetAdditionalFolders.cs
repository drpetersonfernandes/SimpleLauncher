using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace SimpleLauncher.Services;

public static class GetAdditionalFolders
{
    public static string[] GetFolders()
    {
        try
        {
            // Adjust the path if needed
            var jsonText = File.ReadAllText("appsettings.json");
            var jObject = JObject.Parse(jsonText);
            var foldersArray = jObject["AdditionalFolders"] as JArray;
            return foldersArray?.Select(static f => f.ToString()).ToArray() ?? Array.Empty<string>();
        }
        catch (Exception ex)
        {
            // Notify developer
            _ = LogErrors.LogErrorAsync(ex, "Failed to get additional folders.");

            return Array.Empty<string>();
        }
    }
}