using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SimpleLauncher.Services.DebugAndBugReport;
using SimpleLauncher.SharedModels;

namespace SimpleLauncher.Services.EasyMode;

[XmlRoot("EasyMode")]
public class EasyModeManager : IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    // Session cache for API data (in-memory, per application instance)
    private static (EasyModeManager Manager, DateTime Timestamp) _apiCache;
    private static readonly SemaphoreSlim CacheLock = new(1, 1);
    private const int DefaultCacheDurationMinutes = 60;

    [XmlElement("EasyModeSystemConfig")]
    public List<EasyModeSystemConfig> Systems { get; set; }

    /// <summary>
    /// Asynchronously loads the EasyMode configuration. It first tries to load from a local XML file.
    /// If the file is not found or is empty, it falls back to loading from the web API.
    /// </summary>
    /// <returns>An EasyModeManager instance if successful, otherwise null.</returns>
    public static async Task<EasyModeManager> LoadAsync()
    {
        // Try loading from XML first
        var manager = LoadFromXml();
        if (manager != null && manager.Systems.Count != 0)
        {
            DebugLogger.Log("Loaded EasyMode configuration from local XML file.");
            return manager;
        }

        // If XML fails or is empty, try loading from the API
        DebugLogger.Log("Local EasyMode XML not found or is empty. Attempting to load from API.");
        manager = await LoadFromApiAsync().ConfigureAwait(false);
        if (manager != null && manager.Systems.Count != 0)
        {
            DebugLogger.Log("Successfully loaded EasyMode configuration from API.");
            return manager;
        }

        DebugLogger.Log("Failed to load EasyMode configuration from both local XML and API.");
        return null; // Return null if both methods fail
    }

    [Obsolete("Use LoadAsync() instead to support API fallback.", true)]
    public static EasyModeManager Load()
    {
        throw new NotSupportedException("Use the asynchronous LoadAsync() method instead.");
    }

    private static EasyModeManager LoadFromXml()
    {
        // Determine the XML file based on system architecture
        var xmlFile = Environment.OSVersion.Platform == PlatformID.Win32NT
            ? RuntimeInformation.OSArchitecture switch
            {
                Architecture.X64 => "easymode.xml",
                Architecture.Arm64 => "easymode_arm64.xml",
                _ => "easymode.xml" // Default fallback
            }
            : "easymode.xml"; // Default fallback

        var xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, xmlFile);

        // Check if xmlFile exists before proceeding.
        if (!File.Exists(xmlFilePath))
        {
            return null; // File not found, which is an expected scenario for API fallback.
        }

        try
        {
            var serializer = new XmlSerializer(typeof(EasyModeManager));

            // Open the file
            using var fileStream = new FileStream(xmlFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            // Create XmlReaderSettings to disable DTD processing and set XmlResolver to null
            var settings = new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            };

            // Create XmlReader with the settings
            using var xmlReader = XmlReader.Create(fileStream, settings);

            var config = (EasyModeManager)serializer.Deserialize(xmlReader);

            // Validate configuration if not null.
            if (config != null)
            {
                config.Validate(); // Exclude invalid systems
                return config;
            }

            return null;
        }
        catch (Exception ex)
        {
            // If the file exists but is corrupt, we log it but still return null to allow API fallback.
            var contextMessage = $"The file '{xmlFile}' could not be loaded. It might be corrupted.";
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, contextMessage);
            return null;
        }
    }

    private static async Task<EasyModeManager> LoadFromApiAsync()
    {
        await CacheLock.WaitAsync().ConfigureAwait(false);
        try
        {
            // Get cache duration from configuration (default to 60 minutes)
            var cacheDurationMinutes = App.Configuration.GetValue("EasyModeCacheDurationMinutes", DefaultCacheDurationMinutes);

            // Check if we have valid cached data
            if (_apiCache.Manager != null &&
                DateTime.UtcNow - _apiCache.Timestamp < TimeSpan.FromMinutes(cacheDurationMinutes))
            {
                DebugLogger.Log($"Returning EasyMode configuration from session cache (valid for {cacheDurationMinutes} minutes).");
                return _apiCache.Manager;
            }

            // Cache miss or expired, fetch from API
            DebugLogger.Log("EasyMode session cache miss or expired. Fetching from API...");
            var manager = await FetchFromApiAsync().ConfigureAwait(false);

            if (manager is { Systems.Count: > 0 })
            {
                _apiCache = (manager, DateTime.UtcNow);
                DebugLogger.Log("EasyMode configuration fetched from API and cached for session.");
            }

            return manager;
        }
        finally
        {
            CacheLock.Release();
        }
    }

    private static async Task<EasyModeManager> FetchFromApiAsync()
    {
        try
        {
            DebugLogger.Log("Fetching EasyMode configuration from API...");
            var httpClientFactory = App.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient("EasyModeClient");

            var architecture = RuntimeInformation.OSArchitecture switch
            {
                Architecture.Arm64 => "arm64",
                _ => "x64"
            };

            // Use a CancellationToken with a timeout
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
            var response = await client.GetAsync($"api/Systems/{architecture}", cts.Token).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync(cts.Token).ConfigureAwait(false);
            var systems = await JsonSerializer.DeserializeAsync<List<EasyModeSystemConfig>>(stream, JsonOptions, cts.Token).ConfigureAwait(false);

            if (systems == null || systems.Count == 0)
            {
                _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(null, "EasyMode API returned no systems.");
                return null;
            }

            var manager = new EasyModeManager { Systems = systems };
            manager.Validate();
            return manager;
        }
        catch (Exception ex)
        {
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "An error occurred while loading EasyMode configuration from the API.");
            return null;
        }
    }

    public void Validate()
    {
        Systems = Systems?.Where(static system => system.IsValid()).ToList() ?? [];
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    public static class DownloadType
    {
        public const string Emulator = "Emulator";
        public const string Core = "Core";
        public const string ImagePack1 = "ImagePack1";
        public const string ImagePack2 = "ImagePack2";
        public const string ImagePack3 = "ImagePack3";
        public const string ImagePack4 = "ImagePack4";
        public const string ImagePack5 = "ImagePack5";
    }
}