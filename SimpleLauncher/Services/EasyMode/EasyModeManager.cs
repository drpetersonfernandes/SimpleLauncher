using System.IO;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Extensions.Configuration;
using SimpleLauncher.Core.Models;
using SimpleLauncher.Core.Services.DebugAndBugReport;

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

    private readonly ILogErrors _logErrors;
    private readonly IConfiguration _configuration;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDebugLogger _debugLogger;

    [XmlElement("EasyModeSystemConfig")]
    public List<EasyModeSystemConfig> Systems { get; set; }

    public EasyModeManager(ILogErrors logErrors, IConfiguration configuration, IHttpClientFactory httpClientFactory, IDebugLogger debugLogger)
    {
        _logErrors = logErrors;
        _configuration = configuration;
        _httpClientFactory = httpClientFactory;
        _debugLogger = debugLogger;
    }

    public EasyModeManager()
    {
    }

    /// <summary>
    /// Asynchronously loads the EasyMode configuration. It first tries to load from a local XML file.
    /// If the file is not found or is empty, it falls back to loading from the web API.
    /// If the API also fails, it attempts to download from a fallback XML URL.
    /// </summary>
    /// <returns>An EasyModeManager instance if successful, otherwise null.</returns>
    public async Task<EasyModeManager> LoadAsync()
    {
        // Try loading from XML first
        var manager = LoadFromXml(_logErrors);
        if (manager != null && manager.Systems.Count != 0)
        {
            _debugLogger.Log("Loaded EasyMode configuration from local XML file.");
            return manager;
        }

        // If XML fails or is empty, try loading from the API
        _debugLogger.Log("Local EasyMode XML not found or is empty. Attempting to load from API.");
        manager = await LoadFromApiAsync();
        if (manager != null && manager.Systems.Count != 0)
        {
            _debugLogger.Log("Successfully loaded EasyMode configuration from API.");
            return manager;
        }

        // If both local XML and API fail, try loading from fallback URL
        _debugLogger.Log("API load failed. Attempting to load from fallback XML URL.");
        manager = await LoadFromFallbackAsync();
        if (manager != null && manager.Systems.Count != 0)
        {
            _debugLogger.Log("Successfully loaded EasyMode configuration from fallback URL.");
            return manager;
        }

        _debugLogger.Log("Failed to load EasyMode configuration from all sources (local XML, API, and fallback URL).");
        return null; // Return null if all methods fail
    }

    private static EasyModeManager LoadFromXml(ILogErrors logErrors)
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
            logErrors.LogAndForget(ex, contextMessage);
            return null;
        }
    }

    private async Task<EasyModeManager> LoadFromApiAsync()
    {
        await CacheLock.WaitAsync();
        try
        {
            // Get cache duration from configuration (default to 60 minutes)
            var cacheDurationMinutes = _configuration.GetValue("EasyModeCacheDurationMinutes", DefaultCacheDurationMinutes);

            // Check if we have valid cached data
            if (_apiCache.Manager != null &&
                DateTime.UtcNow - _apiCache.Timestamp < TimeSpan.FromMinutes(cacheDurationMinutes))
            {
                _debugLogger.Log($"Returning EasyMode configuration from session cache (valid for {cacheDurationMinutes} minutes).");
                return _apiCache.Manager;
            }

            // Cache miss or expired, fetch from API
            _debugLogger.Log("EasyMode session cache miss or expired. Fetching from API...");
            var manager = await FetchFromApiAsync();

            if (manager is { Systems.Count: > 0 })
            {
                _apiCache = (manager, DateTime.UtcNow);
                _debugLogger.Log("EasyMode configuration fetched from API and cached for session.");
            }

            return manager;
        }
        finally
        {
            CacheLock.Release();
        }
    }

    private async Task<EasyModeManager> FetchFromApiAsync()
    {
        try
        {
            _debugLogger.Log("Fetching EasyMode configuration from API...");
            var client = _httpClientFactory.CreateClient("EasyModeClient");

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
                _logErrors.LogAndForget(null, "EasyMode API returned no systems.");
                return null;
            }

            var manager = new EasyModeManager { Systems = systems };
            manager.Validate();
            return manager;
        }
        catch (Exception ex)
        {
            _logErrors.LogAndForget(ex, "An error occurred while loading EasyMode configuration from the API.");
            return null;
        }
    }

    private async Task<EasyModeManager> LoadFromFallbackAsync()
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
            var fallbackUrl = xmlFile == "easymode_arm64.xml"
                ? _configuration.GetValue<string>("Urls:EasyModeFallbackXmlArm64")
                : _configuration.GetValue<string>("Urls:EasyModeFallbackXmlX64");

            if (string.IsNullOrEmpty(fallbackUrl))
            {
                _debugLogger.Log("No fallback URL configured for EasyMode XML.");
                return null;
            }

            _debugLogger.Log($"Attempting to download EasyMode XML from fallback URL: {fallbackUrl}");

            // Download the XML file from fallback URL
            var client = _httpClientFactory.CreateClient("EasyModeClient");

            // Use a CancellationToken with a timeout (30 seconds)
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var response = await client.GetAsync(fallbackUrl, cts.Token);

            response.EnsureSuccessStatusCode();

            // Read the XML content
            var xmlContent = await response.Content.ReadAsStringAsync(cts.Token);

            if (string.IsNullOrWhiteSpace(xmlContent))
            {
                _debugLogger.Log("Fallback URL returned empty XML content.");
                return null;
            }

            // Save the downloaded XML to the application directory for future use
            var xmlFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, xmlFile);
            await File.WriteAllTextAsync(xmlFilePath, xmlContent, cts.Token);
            _debugLogger.Log($"Downloaded EasyMode XML saved to: {xmlFilePath}");

            // Load the saved XML file
            return LoadFromXml(_logErrors);
        }
        catch (OperationCanceledException)
        {
            _debugLogger.Log("Fallback XML download timed out (30 seconds).");
            return null;
        }
        catch (Exception ex)
        {
            _debugLogger.Log($"Failed to load EasyMode configuration from fallback URL: {ex.Message}");
            _logErrors.LogAndForget(ex, "An error occurred while loading EasyMode configuration from the fallback URL.");
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
