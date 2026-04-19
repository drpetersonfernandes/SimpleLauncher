using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
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
    /// If the API also fails, it attempts to download from a fallback XML URL.
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
        manager = await LoadFromApiAsync();
        if (manager != null && manager.Systems.Count != 0)
        {
            DebugLogger.Log("Successfully loaded EasyMode configuration from API.");
            return manager;
        }

        // If both local XML and API fail, try loading from fallback URL
        DebugLogger.Log("API load failed. Attempting to load from fallback XML URL.");
        manager = await LoadFromFallbackAsync();
        if (manager != null && manager.Systems.Count != 0)
        {
            DebugLogger.Log("Successfully loaded EasyMode configuration from fallback URL.");
            return manager;
        }

        DebugLogger.Log("Failed to load EasyMode configuration from all sources (local XML, API, and fallback URL).");
        return null; // Return null if all methods fail
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
        await CacheLock.WaitAsync();
        try
        {
            // Get cache duration from configuration (default to 60 minutes)
            var configuration = App.ServiceProvider.GetRequiredService<IConfiguration>();
            var cacheDurationMinutes = configuration.GetValue("EasyModeCacheDurationMinutes", DefaultCacheDurationMinutes);

            // Check if we have valid cached data
            if (_apiCache.Manager != null &&
                DateTime.UtcNow - _apiCache.Timestamp < TimeSpan.FromMinutes(cacheDurationMinutes))
            {
                DebugLogger.Log($"Returning EasyMode configuration from session cache (valid for {cacheDurationMinutes} minutes).");
                return _apiCache.Manager;
            }

            // Cache miss or expired, fetch from API
            DebugLogger.Log("EasyMode session cache miss or expired. Fetching from API...");
            var manager = await FetchFromApiAsync();

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

            // Use a CancellationToken with a timeout (30 seconds to accommodate users with slower connections or VPN)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var response = await client.GetAsync($"api/Systems/{architecture}", cts.Token);

            response.EnsureSuccessStatusCode();

            var stream = await response.Content.ReadAsStreamAsync(cts.Token);
            var systems = await JsonSerializer.DeserializeAsync<List<EasyModeSystemConfig>>(stream, JsonOptions, cts.Token);

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

    private static async Task<EasyModeManager> LoadFromFallbackAsync()
    {
        try
        {
            // Determine the appropriate XML file based on system architecture
            var xmlFile = Environment.OSVersion.Platform == PlatformID.Win32NT
                ? RuntimeInformation.OSArchitecture switch
                {
                    Architecture.Arm64 => "easymode_arm64.xml",
                    _ => "easymode.xml" // Default fallback for x64 and others
                }
                : "easymode.xml"; // Default fallback

            // Get the fallback URL from configuration
            var configuration = App.ServiceProvider.GetRequiredService<IConfiguration>();
            var fallbackUrl = xmlFile == "easymode_arm64.xml"
                ? configuration.GetValue<string>("Urls:EasyModeFallbackXmlArm64")
                : configuration.GetValue<string>("Urls:EasyModeFallbackXmlX64");

            if (string.IsNullOrEmpty(fallbackUrl))
            {
                DebugLogger.Log("No fallback URL configured for EasyMode XML.");
                return null;
            }

            DebugLogger.Log($"Attempting to download EasyMode XML from fallback URL: {fallbackUrl}");

            // Download the XML file from fallback URL
            var httpClientFactory = App.ServiceProvider.GetRequiredService<IHttpClientFactory>();
            var client = httpClientFactory.CreateClient("EasyModeClient");

            // Use a CancellationToken with a timeout (30 seconds)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var response = await client.GetAsync(fallbackUrl, cts.Token);

            response.EnsureSuccessStatusCode();

            // Read the XML content
            var xmlContent = await response.Content.ReadAsStringAsync(cts.Token);

            if (string.IsNullOrWhiteSpace(xmlContent))
            {
                DebugLogger.Log("Fallback URL returned empty XML content.");
                return null;
            }

            // Save the downloaded XML to the application directory for future use
            var xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, xmlFile);
            await File.WriteAllTextAsync(xmlFilePath, xmlContent, cts.Token);
            DebugLogger.Log($"Downloaded EasyMode XML saved to: {xmlFilePath}");

            // Load the saved XML file
            return LoadFromXml();
        }
        catch (OperationCanceledException)
        {
            DebugLogger.Log("Fallback XML download timed out (20 seconds).");
            return null;
        }
        catch (Exception ex)
        {
            DebugLogger.Log($"Failed to load EasyMode configuration from fallback URL: {ex.Message}");
            _ = App.ServiceProvider.GetRequiredService<ILogErrors>().LogErrorAsync(ex, "An error occurred while loading EasyMode configuration from the fallback URL.");
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
